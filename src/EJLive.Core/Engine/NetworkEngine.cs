using System.IO;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text.Json;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Core.Engine;

public sealed class NetworkTransportOptions
{
    public bool EnableTlsTransport { get; set; }
    public bool RequireTlsTransport { get; set; }
    public bool AllowUntrustedTlsCertificate { get; set; }
    public bool EnableAdaptiveChunking { get; set; } = true;
    public int WeakNetworkLatencyMs { get; set; } = 500;

    public static NetworkTransportOptions FromEnvironment()
    {
        static bool ReadBool(string key, bool defaultValue = false)
        {
            var raw = Environment.GetEnvironmentVariable(key);
            return bool.TryParse(raw, out var value) ? value : defaultValue;
        }

        static int ReadInt(string key, int defaultValue, int min, int max)
        {
            var raw = Environment.GetEnvironmentVariable(key);
            return int.TryParse(raw, out var value) ? Math.Clamp(value, min, max) : defaultValue;
        }

        return new NetworkTransportOptions
        {
            EnableTlsTransport = ReadBool("EJLIVE_SOCKET_TLS"),
            RequireTlsTransport = ReadBool("EJLIVE_SOCKET_TLS_REQUIRED"),
            AllowUntrustedTlsCertificate = ReadBool("EJLIVE_SOCKET_TLS_ALLOW_UNTRUSTED"),
            EnableAdaptiveChunking = ReadBool("EJLIVE_ADAPTIVE_CHUNKING", true),
            WeakNetworkLatencyMs = ReadInt("EJLIVE_WEAK_NETWORK_LATENCY_MS", 500, 120, 3000)
        };
    }
}

public sealed class NetworkEngine : IDisposable
{
    private enum CircuitBreakerState
    {
        Closed,
        Open,
        HalfOpen
    }

    private readonly string _serverIp;
    private readonly int _serverPort;
    private readonly string _atmId;
    private readonly string _atmType;
    private readonly RetryPolicy _reconnectPolicy;
    private readonly NetworkTransportOptions _transportOptions;
    private readonly SemaphoreSlim _sendGate = new(1, 1);
    private readonly object _circuitLock = new();
    private readonly object _latencyLock = new();
    private TcpClient? _client;
    private Stream? _stream;
    private CancellationTokenSource? _cts;
    private volatile bool _running;
    private volatile bool _usingTlsTransport;
    private CircuitBreakerState _circuitState = CircuitBreakerState.Closed;
    private int _consecutiveConnectFailures;
    private int _circuitOpenCycles;
    private int _lastNetworkLatencyMs = -1;
    private DateTime _nextCircuitProbeUtc = DateTime.MinValue;
    private DateTime _lastLatencyProbeUtc = DateTime.MinValue;
    private static readonly TimeSpan CircuitBaseWindow = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan CircuitMaxWindow = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan LatencyProbeInterval = TimeSpan.FromSeconds(20);

    public NetworkEngine(
        string serverIp,
        int serverPort,
        string atmId,
        string atmType,
        string networkType = "LAN",
        JournalOutbox? outbox = null,
        NetworkTransportOptions? transportOptions = null)
    {
        _serverIp = serverIp;
        _serverPort = serverPort;
        _atmId = atmId;
        _atmType = atmType;
        NetworkType = networkType;
        Outbox = outbox ?? new JournalOutbox();
        _reconnectPolicy = RetryPolicy.ForNetwork(networkType);
        _transportOptions = transportOptions ?? NetworkTransportOptions.FromEnvironment();
    }

    public string NetworkType { get; }
    public JournalOutbox Outbox { get; }
    public string SessionId { get; private set; } = string.Empty;
    public bool IsConnected => _client?.Connected == true;
    public int ReconnectAttempts { get; private set; }
    public DateTime ConnectedAt { get; private set; }
    public long TotalBytesSent { get; private set; }
    public long TotalBytesReceived { get; private set; }
    public double SpeedKBs { get; private set; }

    public event EventHandler<bool>? OnConnectionChanged;
    public event EventHandler<string>? OnSessionEstablished;
    public event EventHandler<EJMessage>? OnMessageReceived;
    public event EventHandler<string>? OnJournalAcknowledged;
    public event EventHandler<HandshakeResponse>? OnHandshakeResponse;
    public event EventHandler<HeartbeatAck>? OnHeartbeatAck;
    public event EventHandler<ChunkAck>? OnChunkAck;
    public event EventHandler<string>? OnError;
    public event EventHandler<string>? OnLog;

    public bool Connect()
    {
        _running = true;
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        if (!TryEnterConnectWindow(out var waitFor))
        {
            Log($"Connection circuit open; next probe in {Math.Max(1, waitFor.TotalSeconds):F0}s.");
            return false;
        }

        for (var attempt = 1; attempt <= _reconnectPolicy.MaxAttempts; attempt++)
        {
            try
            {
                ReconnectAttempts = attempt;
                if (attempt == 1 && _consecutiveConnectFailures > 0)
                {
                    var initialJitterMs = Random.Shared.Next(250, 1500);
                    Thread.Sleep(initialJitterMs);
                }

                _client = new TcpClient { ReceiveTimeout = AppConstants.SocketTimeoutMs, SendTimeout = AppConstants.SocketTimeoutMs, NoDelay = true };
                var result = _client.BeginConnect(_serverIp, _serverPort, null, null);
                if (!result.AsyncWaitHandle.WaitOne(AppConstants.SocketTimeoutMs))
                    throw new TimeoutException("Connection timed out.");

                _client.EndConnect(result);
                var stream = _client.GetStream();
                _stream = UpgradeStreamIfRequested(stream);
                SessionId = Guid.NewGuid().ToString("N");
                ConnectedAt = DateTime.UtcNow;
                OnConnectSuccess();
                OnConnectionChanged?.Invoke(this, true);
                OnSessionEstablished?.Invoke(this, SessionId);
                Log(_usingTlsTransport
                    ? $"Connected to {_serverIp}:{_serverPort} over TLS."
                    : $"Connected to {_serverIp}:{_serverPort}");
                SendMessage(CommunicationProtocol.BuildHandshake(_atmId, _atmType, AppConstants.AppVersion));
                _ = Task.Run(() => ReceiveLoopAsync(_cts.Token), _cts.Token);
                _ = Task.Run(() => HeartbeatLoopAsync(_cts.Token), _cts.Token);
                return true;
            }
            catch (Exception ex)
            {
                ReportError("Connect", ex);
                var circuitOpened = OnConnectFailure(ex);
                Disconnect();
                if (!_running || attempt >= _reconnectPolicy.MaxAttempts || circuitOpened)
                    break;

                var retryDelayMs = ComputeRetryDelayWithJitter(attempt);
                Thread.Sleep(retryDelayMs);
            }
        }

        return false;
    }

    public void SendMessage(byte[] frame)
    {
        if (_stream is null || !IsConnected)
            return;

        _sendGate.Wait();
        try
        {
            _stream.Write(frame, 0, frame.Length);
            _stream.Flush();
            TotalBytesSent += frame.Length;
            SpeedKBs = frame.Length / 1024.0;
        }
        finally
        {
            _sendGate.Release();
        }
    }

    public async Task SendMessageAsync(byte[] frame, CancellationToken cancellationToken = default)
    {
        if (_stream is null || !IsConnected)
            return;

        await _sendGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await _stream.WriteAsync(frame.AsMemory(0, frame.Length), cancellationToken).ConfigureAwait(false);
            await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            TotalBytesSent += frame.Length;
            SpeedKBs = frame.Length / 1024.0;
        }
        finally
        {
            _sendGate.Release();
        }
    }

    public void DispatchReceived(EJMessage message)
    {
        TotalBytesReceived += message.Payload.Length;
        OnMessageReceived?.Invoke(this, message);
    }

    public void SendHandshake(HandshakeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SendMessage(CommunicationProtocol.BuildHandshake(request.AtmId, _atmType, request.ClientVersion));
    }

    public void SendHeartbeat(HeartbeatPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        if (!IsConnected)
            return;

        SendMessage(CommunicationProtocol.BuildHeartbeat(payload.AtmId, JsonSerializer.Serialize(payload)));
    }

    public void SendChunk(ChunkPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        SendMessage(CommunicationProtocol.BuildChunk(payload.Sequence, payload.Data));
    }

    public void SendTransferComplete(TransferComplete complete)
    {
        ArgumentNullException.ThrowIfNull(complete);
        SendMessage(CommunicationProtocol.BuildComplete(complete.FileName, string.Empty, complete.Sha256));
    }

    public bool SendJournalFile(string fileName, byte[] data, long offset, string checksum)
    {
        if (!IsConnected)
        {
            Outbox.Enqueue(_atmId, fileName, data, offset, checksum);
            return false;
        }

        SendMessage(CommunicationProtocol.BuildStartFile(_atmId, fileName, data.Length, offset, checksum));
        var chunkSize = GetEffectiveChunkSizeBytes();
        var totalChunks = (int)Math.Ceiling(data.Length / (double)chunkSize);
        for (var i = 0; i < totalChunks; i++)
        {
            var chunkOffset = i * chunkSize;
            var chunkLen = Math.Min(chunkSize, data.Length - chunkOffset);
            var chunk = new byte[chunkLen];
            Buffer.BlockCopy(data, chunkOffset, chunk, 0, chunkLen);
            SendMessage(CommunicationProtocol.BuildChunk(i, chunk));
        }
        SendMessage(CommunicationProtocol.BuildComplete(fileName, checksum, SecurityHelper.SHA256Hash(data)));
        return true;
    }

    public async Task<bool> SendJournalFileAsync(string fileName, byte[] data, long offset, string checksum, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            Outbox.Enqueue(_atmId, fileName, data, offset, checksum);
            return false;
        }

        await SendMessageAsync(CommunicationProtocol.BuildStartFile(_atmId, fileName, data.Length, offset, checksum), cancellationToken).ConfigureAwait(false);
        var chunkSize = GetEffectiveChunkSizeBytes();
        var totalChunks = (int)Math.Ceiling(data.Length / (double)chunkSize);
        for (var i = 0; i < totalChunks; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var chunkOffset = i * chunkSize;
            var chunkLen = Math.Min(chunkSize, data.Length - chunkOffset);
            var chunk = new byte[chunkLen];
            Buffer.BlockCopy(data, chunkOffset, chunk, 0, chunkLen);
            await SendMessageAsync(CommunicationProtocol.BuildChunk(i, chunk), cancellationToken).ConfigureAwait(false);
        }
        await SendMessageAsync(CommunicationProtocol.BuildComplete(fileName, checksum, SecurityHelper.SHA256Hash(data)), cancellationToken).ConfigureAwait(false);
        return true;
    }

    public void Disconnect()
    {
        _running = false;
        try { _cts?.Cancel(); } catch { }
        try { _stream?.Dispose(); } catch { }
        try { _client?.Close(); } catch { }
        _stream = null;
        _client = null;
        OnConnectionChanged?.Invoke(this, false);
    }

    private void Log(string message) => OnLog?.Invoke(this, message);
    private void ReportError(string context, Exception ex) => OnError?.Invoke(this, $"{context}: {ex.Message}");

    private bool OnConnectFailure(Exception ex)
    {
        string? openedMessage = null;
        var opened = false;
        lock (_circuitLock)
        {
            _consecutiveConnectFailures++;
            if (_consecutiveConnectFailures < 4)
                return false;

            _circuitState = CircuitBreakerState.Open;
            _circuitOpenCycles++;
            var exponent = Math.Min(6, _circuitOpenCycles - 1);
            var scaled = CircuitBaseWindow.TotalMilliseconds * Math.Pow(2, exponent);
            var jitterMs = Random.Shared.Next(250, 3000);
            var openMs = Math.Min(CircuitMaxWindow.TotalMilliseconds, scaled + jitterMs);
            _nextCircuitProbeUtc = DateTime.UtcNow.AddMilliseconds(openMs);
            openedMessage = $"Circuit opened after {_consecutiveConnectFailures} failures. reason={ex.GetType().Name} nextProbe={_nextCircuitProbeUtc:O}";
            opened = true;
        }

        if (!string.IsNullOrWhiteSpace(openedMessage))
            Log(openedMessage);

        return opened;
    }

    private void OnConnectSuccess()
    {
        lock (_circuitLock)
        {
            _consecutiveConnectFailures = 0;
            _circuitOpenCycles = 0;
            _nextCircuitProbeUtc = DateTime.MinValue;
            _circuitState = CircuitBreakerState.Closed;
        }
    }

    private bool TryEnterConnectWindow(out TimeSpan waitFor)
    {
        lock (_circuitLock)
        {
            var now = DateTime.UtcNow;
            if (_circuitState == CircuitBreakerState.Open)
            {
                if (_nextCircuitProbeUtc > now)
                {
                    waitFor = _nextCircuitProbeUtc - now;
                    return false;
                }

                _circuitState = CircuitBreakerState.HalfOpen;
                waitFor = TimeSpan.Zero;
                return true;
            }

            waitFor = TimeSpan.Zero;
            return true;
        }
    }

    private int ComputeRetryDelayWithJitter(int attempt)
    {
        var baseDelay = _reconnectPolicy.ComputeDelay(attempt);
        var herdJitter = Random.Shared.Next(200, 2000);
        return checked(baseDelay + herdJitter);
    }

    private Stream UpgradeStreamIfRequested(NetworkStream networkStream)
    {
        _usingTlsTransport = false;
        if (!_transportOptions.EnableTlsTransport)
            return networkStream;

        try
        {
            var ssl = new SslStream(
                networkStream,
                leaveInnerStreamOpen: false,
                (_, cert, chain, errors) =>
                {
                    if (_transportOptions.AllowUntrustedTlsCertificate)
                        return true;
                    return errors == SslPolicyErrors.None;
                });

            var sslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12;
            ssl.AuthenticateAsClient(_serverIp, null, sslProtocols, checkCertificateRevocation: true);
            _usingTlsTransport = true;
            return ssl;
        }
        catch (Exception ex)
        {
            if (_transportOptions.RequireTlsTransport)
                throw new AuthenticationException("TLS is required but handshake failed.", ex);

            Log("TLS handshake failed, fallback to TCP stream: " + ex.Message);
            return networkStream;
        }
    }

    private int GetEffectiveChunkSizeBytes()
    {
        var baseSize = AppConstants.ChunkSizeBytes;
        if (!_transportOptions.EnableAdaptiveChunking)
            return baseSize;

        var latencyMs = GetRecentNetworkLatencyMs();
        if (latencyMs < 0)
            return baseSize;

        var weakThreshold = Math.Clamp(_transportOptions.WeakNetworkLatencyMs, 120, 3000);
        if (latencyMs >= weakThreshold * 2)
            return Math.Max(8 * 1024, baseSize / 4);
        if (latencyMs >= weakThreshold)
            return Math.Max(16 * 1024, baseSize / 2);
        if (latencyMs >= weakThreshold / 2)
            return Math.Max(32 * 1024, (baseSize * 3) / 4);

        return baseSize;
    }

    private int GetRecentNetworkLatencyMs()
    {
        lock (_latencyLock)
        {
            var now = DateTime.UtcNow;
            if (_lastNetworkLatencyMs >= 0 && now - _lastLatencyProbeUtc <= LatencyProbeInterval)
                return _lastNetworkLatencyMs;
        }

        var measured = ProbeNetworkLatencyMs();
        lock (_latencyLock)
        {
            _lastNetworkLatencyMs = measured;
            _lastLatencyProbeUtc = DateTime.UtcNow;
            return _lastNetworkLatencyMs;
        }
    }

    private int ProbeNetworkLatencyMs()
    {
        try
        {
            using var ping = new Ping();
            var reply = ping.Send(_serverIp, 1200);
            if (reply?.Status == IPStatus.Success)
                return (int)Math.Clamp(reply.RoundtripTime, 1, int.MaxValue);
        }
        catch
        {
            // Best effort QoS signal.
        }

        return -1;
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _stream is not null && IsConnected)
        {
            try
            {
                var message = await Task.Run(() => CommunicationProtocol.ReadMessage(_stream), cancellationToken).ConfigureAwait(false);
                if (message.Type == CommunicationProtocol.MsgType.HandshakeAck)
                {
                    var parts = message.Text.Split('|');
                    if (parts.Length >= 2)
                    {
                        SessionId = parts[1];
                        OnSessionEstablished?.Invoke(this, SessionId);
                    }
                    OnHandshakeResponse?.Invoke(this, new HandshakeResponse(
                        Accepted: true,
                        ServerSessionId: SessionId,
                        RejectionReason: null,
                        ServerTimeUtc: DateTime.UtcNow,
                        CommandPendingCount: 0,
                        HeartbeatInterval: TimeSpan.FromSeconds(AppConstants.HeartbeatIntervalSec)));
                    Log($"Server acknowledged session {SessionId}.");
                }
                else if (message.Type == CommunicationProtocol.MsgType.HeartbeatAck)
                {
                    if (!CommunicationProtocol.TryParseHeartbeatAck(message.Text, out var heartbeatAck))
                    {
                        heartbeatAck = new HeartbeatAck(
                            ServerTimeUtc: DateTime.UtcNow,
                            CommandsPendingCount: 0,
                            RequestImmediateSync: false,
                            ServerMessage: message.Text);
                    }
                    OnHeartbeatAck?.Invoke(this, heartbeatAck);
                    Log($"Heartbeat acknowledged. pending={heartbeatAck.CommandsPendingCount}; immediateSync={heartbeatAck.RequestImmediateSync}.");
                }
                else if (message.Type == CommunicationProtocol.MsgType.ChunkAck)
                {
                    _ = int.TryParse(message.Text, out var sequence);
                    OnChunkAck?.Invoke(this, new ChunkAck(
                        TransferId: Guid.Empty,
                        Sequence: sequence,
                        Ok: true,
                        NextExpectedOffset: 0,
                        ServerHash: null));
                }
                else if (message.Type == CommunicationProtocol.MsgType.JournalAck)
                {
                    OnJournalAcknowledged?.Invoke(this, message.Text ?? string.Empty);
                    Log($"Journal acknowledged: {message.Text}");
                }
                else
                {
                    DispatchReceived(message);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (IOException ex)
            {
                ReportError("Receive", ex);
                Disconnect();
                break;
            }
            catch (Exception ex)
            {
                ReportError("Receive", ex);
                Disconnect();
                break;
            }
        }
    }

    private async Task HeartbeatLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _running)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(AppConstants.HeartbeatIntervalSec), cancellationToken).ConfigureAwait(false);
                if (IsConnected)
                    await SendMessageAsync(CommunicationProtocol.BuildHeartbeat(_atmId, DateTime.UtcNow.ToString("O")), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                ReportError("Heartbeat", ex);
            }
        }
    }

    public void Dispose()
    {
        Disconnect();
        _cts?.Dispose();
        _sendGate.Dispose();
    }
}
