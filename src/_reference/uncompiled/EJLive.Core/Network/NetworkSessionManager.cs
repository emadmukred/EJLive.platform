using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EJLive.Core.Network
{
    /// <summary>
    /// Manages TCP socket connection to the EJLive server.
    /// Handles handshake, heartbeat, and basic send/receive operations.
    /// This is a production network layer — no WinForms dependency.
    /// </summary>
    public sealed class NetworkSessionManager : IDisposable
    {
        private readonly string _serverHost;
        private readonly int _serverPort;
        private readonly string _atmId;
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private bool _isConnected;
        private string _sessionId = string.Empty;
        private DateTimeOffset _lastHeartbeat = DateTimeOffset.MinValue;
        private DateTimeOffset _lastHandshake = DateTimeOffset.MinValue;
        private long _bytesSent;
        private long _bytesReceived;
        private CancellationTokenSource _heartbeatCts;
        private readonly object _lock = new object();

        public bool IsConnected { get { lock (_lock) return _isConnected; } }
        public string SessionId { get { lock (_lock) return _sessionId; } }
        public DateTimeOffset LastHeartbeat { get { lock (_lock) return _lastHeartbeat; } }
        public Models.HeartbeatStatus CurrentHeartbeatStatus
        {
            get
            {
                lock (_lock)
                {
                    if (!_isConnected) return Models.HeartbeatStatus.CriticalOffline;
                    var elapsed = DateTimeOffset.UtcNow - _lastHeartbeat;
                    if (elapsed.TotalSeconds < 35) return Models.HeartbeatStatus.Online;
                    if (elapsed.TotalSeconds < 90) return Models.HeartbeatStatus.Warning;
                    if (elapsed.TotalSeconds < 300) return Models.HeartbeatStatus.Offline;
                    return Models.HeartbeatStatus.CriticalOffline;
                }
            }
        }
        private int _heartbeatMissCount;
        private DateTimeOffset _lastConnectAttempt = DateTimeOffset.MinValue;
        private int _reconnectAttempt;
        private static readonly Random _jitterRandom = new Random();

        /// <summary>Exponential backoff with jitter for reconnect attempts.</summary>
        public int NextReconnectDelayMs()
        {
            _reconnectAttempt++;
            // Cap at 5 minutes
            var baseMs = Math.Min(1000 * Math.Pow(2, _reconnectAttempt), 300000);
            // Add jitter ±25%
            var jitter = _jitterRandom.Next(-(int)(baseMs * 0.25), (int)(baseMs * 0.25));
            return Math.Max(1000, (int)baseMs + jitter);
        }

        /// <summary>Resets reconnect backoff after successful connection.</summary>
        public void ResetReconnectBackoff()
        {
            _reconnectAttempt = 0;
            _lastConnectAttempt = DateTimeOffset.UtcNow;
        }
        public DateTimeOffset LastHandshake { get { lock (_lock) return _lastHandshake; } }
        public long BytesSent { get { lock (_lock) return _bytesSent; } }
        public long BytesReceived { get { lock (_lock) return _bytesReceived; } }

        public event Action<bool> OnConnectionChanged;
        public event Action<string> OnLog;
        public event Action<byte[]> OnDataReceived;

        public NetworkSessionManager(string serverHost, int serverPort, string atmId)
        {
            _serverHost = serverHost ?? throw new ArgumentNullException(nameof(serverHost));
            _serverPort = serverPort;
            _atmId = atmId ?? throw new ArgumentNullException(nameof(atmId));
        }

        /// <summary>
        /// Establishes a TCP connection to the server and performs handshake.
        /// Returns true on success.
        /// </summary>
        public async Task<bool> ConnectAsync(int timeoutMs = 10000)
        {
            try
            {
                Log($"Connecting to {_serverHost}:{_serverPort}...");
                _tcpClient = new TcpClient();
                var connectTask = _tcpClient.ConnectAsync(_serverHost, _serverPort);
                if (await Task.WhenAny(connectTask, Task.Delay(timeoutMs)) != connectTask)
                {
                    Log("Connection timed out.");
                    return false;
                }

                _stream = _tcpClient.GetStream();
                lock (_lock) { _isConnected = true; }

                // Perform handshake
                var handshakeOk = await PerformHandshakeAsync();
                if (!handshakeOk)
                {
                    Disconnect();
                    return false;
                }

                // Start heartbeat loop
                _heartbeatCts = new CancellationTokenSource();
                _ = Task.Run(() => HeartbeatLoopAsync(_heartbeatCts.Token));

                Log("Connected successfully. Session: " + _sessionId);
                OnConnectionChanged?.Invoke(true);
                return true;
            }
            catch (Exception ex)
            {
                Log("Connection failed: " + ex.Message);
                lock (_lock) { _isConnected = false; }
                OnConnectionChanged?.Invoke(false);
                return false;
            }
        }

        /// <summary>
        /// Sends raw bytes to the server. Returns true on success.
        /// </summary>
        public async Task<bool> SendAsync(byte[] data)
        {
            if (!_isConnected || _stream == null)
                return false;

            try
            {
                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();
                lock (_lock) { _bytesSent += data.Length; }
                return true;
            }
            catch (Exception ex)
            {
                Log("Send failed: " + ex.Message);
                lock (_lock) { _isConnected = false; }
                OnConnectionChanged?.Invoke(false);
                return false;
            }
        }

        /// <summary>
        /// Sends a text message (UTF8-encoded) to the server.
        /// </summary>
        public Task<bool> SendTextAsync(string message)
        {
            var data = Encoding.UTF8.GetBytes(message + "\n");
            return SendAsync(data);
        }

        /// <summary>
        /// Disconnects from the server and stops heartbeat.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _heartbeatCts?.Cancel();
                _heartbeatCts?.Dispose();
                _heartbeatCts = null;
            }
            catch { }

            try
            {
                if (_isConnected && _stream != null)
                {
                    var bye = Encoding.UTF8.GetBytes("DISCONNECT|" + _atmId + "\n");
                    _stream.Write(bye, 0, bye.Length);
                    _stream.Flush();
                }
            }
            catch { }

            try { _stream?.Close(); } catch { }
            try { _tcpClient?.Close(); } catch { }

            lock (_lock)
            {
                _isConnected = false;
                _sessionId = string.Empty;
            }

            Log("Disconnected.");
            OnConnectionChanged?.Invoke(false);
        }

        public void Dispose()
        {
            Disconnect();
            _tcpClient?.Dispose();
        }

        private async Task<bool> PerformHandshakeAsync()
        {
            try
            {
                var handshake = $"HANDSHAKE|{_atmId}|{Environment.MachineName}|1.0|{DateTimeOffset.UtcNow:o}";
                var handshakeData = Encoding.UTF8.GetBytes(handshake + "\n");
                await _stream.WriteAsync(handshakeData, 0, handshakeData.Length);
                await _stream.FlushAsync();

                // Read response
                var buffer = new byte[4096];
                var readTask = _stream.ReadAsync(buffer, 0, buffer.Length);
                if (await Task.WhenAny(readTask, Task.Delay(5000)) != readTask)
                {
                    Log("Handshake response timed out.");
                    return false;
                }

                var bytesRead = readTask.Result;
                if (bytesRead <= 0)
                {
                    Log("No handshake response received.");
                    return false;
                }

                var response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Log("Handshake response: " + response);
                
                // Parse response (format: ACCEPTED|SessionId)
                if (response.StartsWith("ACCEPTED|", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = response.Split('|');
                    if (parts.Length >= 2)
                    {
                        lock (_lock)
                        {
                            _sessionId = parts[1];
                            _lastHandshake = DateTimeOffset.UtcNow;
                            _lastHeartbeat = DateTimeOffset.UtcNow;
                        }
                        return true;
                    }
                }

                Log("Handshake rejected.");
                return false;
            }
            catch (Exception ex)
            {
                Log("Handshake error: " + ex.Message);
                return false;
            }
        }

        private async Task HeartbeatLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _isConnected)
            {
                try
                {
                    await Task.Delay(30000, ct); // Every 30 seconds
                    if (ct.IsCancellationRequested) break;

                    var heartbeat = $"HEARTBEAT|{_atmId}|{_sessionId}|{DateTimeOffset.UtcNow:o}";
                    var data = Encoding.UTF8.GetBytes(heartbeat + "\n");
                    await _stream.WriteAsync(data, 0, data.Length);
                    await _stream.FlushAsync();
                    lock (_lock)
                    {
                        _bytesSent += data.Length;
                        _lastHeartbeat = DateTimeOffset.UtcNow;
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Log("Heartbeat failed: " + ex.Message);
                    lock (_lock) { _isConnected = false; }
                    OnConnectionChanged?.Invoke(false);
                    break;
                }
            }
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[Network] {message}");
        }
    }
}