using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;

namespace EJLive.Core.Engine;

public sealed class RemoteAssistanceStream
{
    public RemoteSession? CurrentSession { get; private set; }
    public event EventHandler<RemoteSession>? SessionChanged;

    public RemoteSession Start(string atmId)
    {
        CurrentSession = new RemoteSession { ATM_ID = atmId, Status = RemoteSessionState.Active };
        SessionChanged?.Invoke(this, CurrentSession);
        return CurrentSession;
    }

    public void Stop()
    {
        if (CurrentSession is null)
            return;
        CurrentSession.Status = RemoteSessionState.Stopped;
        CurrentSession.EndedAtUtc = DateTime.UtcNow;
        SessionChanged?.Invoke(this, CurrentSession);
    }

    public byte[] CaptureScreenJpeg()
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1024, 768);
        using var bmp = new Bitmap(bounds.Width, bounds.Height);
        using (var graphics = Graphics.FromImage(bmp))
            graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
        using var stream = new MemoryStream();
        bmp.Save(stream, ImageFormat.Jpeg);
        return stream.ToArray();
    }
}

public sealed class TransactionAnalysisEngine
{
    public TransactionAnalysisReport Analyze(string journalText)
    {
        var lines = (journalText ?? string.Empty).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        var report = new TransactionAnalysisReport();
        foreach (var line in lines)
        {
            if (line.Contains("WITHDRAW", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("DISPENSE", StringComparison.OrdinalIgnoreCase))
                report.TotalTransactions++;
            if (line.Contains("APPROVED", StringComparison.OrdinalIgnoreCase) || line.Contains("SUCCESS", StringComparison.OrdinalIgnoreCase))
                report.ApprovedCount++;
            if (line.Contains("FAIL", StringComparison.OrdinalIgnoreCase) || line.Contains("DECLIN", StringComparison.OrdinalIgnoreCase))
                report.FailedCount++;
            if (line.Contains("RETAIN", StringComparison.OrdinalIgnoreCase) || line.Contains("CAPTURE", StringComparison.OrdinalIgnoreCase))
                report.RetainedCards++;
        }

        if (report.FailedCount > 0)
            report.Findings.Add("Failed transactions detected in journal stream.");
        if (report.RetainedCards > 0)
            report.Findings.Add("Card retention event detected.");
        return report;
    }
}

public sealed class ReportExportEngine
{
    public string ExportCsv<T>(IEnumerable<T> rows, string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        var properties = typeof(T).GetProperties();
        using var writer = new StreamWriter(filePath);
        writer.WriteLine(string.Join(",", properties.Select(p => Escape(p.Name))));
        foreach (var row in rows)
            writer.WriteLine(string.Join(",", properties.Select(p => Escape(Convert.ToString(p.GetValue(row)) ?? string.Empty))));
        return filePath;
    }

    private static string Escape(string value) => '"' + value.Replace("\"", "\"\"") + '"';
}

public sealed class ServerEngine : IDisposable
{
    private readonly ConcurrentDictionary<string, ClientConnection> _connections = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, TcpClient> _clients = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IncomingJournalTransfer> _incomingTransfers = new(StringComparer.OrdinalIgnoreCase);
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _acceptLoop;

    public IReadOnlyList<ClientConnection> Connections => _connections.Values.OrderBy(c => c.ATM_ID, StringComparer.OrdinalIgnoreCase).ToArray();
    public bool IsRunning { get; private set; }
    public DateTime StartTime { get; private set; }
    public int ListenPort { get; private set; }

    public event EventHandler<ClientConnection>? ClientConnected;
    public event EventHandler<ClientConnection>? ClientDisconnected;
    public event EventHandler<EJMessage>? MessageReceived;
    public event EventHandler<RemoteFramePacket>? RemoteFrameReceived;
    public event EventHandler<JournalTransferProgressPacket>? JournalTransferProgress;
    public event EventHandler<JournalFileReceivedPacket>? JournalFileReceived;
    public event EventHandler<ClientTelemetryPacket>? ClientTelemetryReceived;
    public event EventHandler<string>? Log;
    public event EventHandler<string>? Error;

    public void Start(int port)
    {
        if (IsRunning)
            return;

        ListenPort = port;
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        IsRunning = true;
        StartTime = DateTime.UtcNow;
        Log?.Invoke(this, $"Server listener started on port {port}.");
        _acceptLoop = Task.Run(() => AcceptLoopAsync(_cts.Token));
    }

    public void Stop()
    {
        if (!IsRunning)
            return;

        IsRunning = false;
        try { _cts?.Cancel(); } catch { }
        try { _listener?.Stop(); } catch { }

        foreach (var pair in _clients.ToArray())
        {
            try { pair.Value.Close(); } catch { }
            _clients.TryRemove(pair.Key, out _);
        }
        foreach (var transfer in _incomingTransfers.Values)
            transfer.Dispose();
        _incomingTransfers.Clear();

        foreach (var connection in _connections.Values.ToArray())
            ClientDisconnected?.Invoke(this, connection);
        _connections.Clear();
        Log?.Invoke(this, "Server listener stopped.");
    }

    public void Register(ClientConnection connection)
    {
        _connections[connection.ATM_ID] = connection;
        ClientConnected?.Invoke(this, connection);
    }

    public void Remove(string atmId)
    {
        if (_incomingTransfers.TryRemove(atmId, out var transfer))
            transfer.Dispose();

        if (_clients.TryRemove(atmId, out var client))
        {
            try { client.Close(); } catch { }
        }

        if (_connections.TryRemove(atmId, out var connection))
            ClientDisconnected?.Invoke(this, connection);
    }

    public void Broadcast(string message)
    {
        var frame = CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, message);
        foreach (var pair in _clients.ToArray())
            _ = SendFrameAsync(pair.Key, pair.Value, frame, CancellationToken.None);
    }

    public bool SendCommand(string atmId, RemoteCommandEnvelope command)
    {
        if (!_clients.TryGetValue(atmId, out var client))
        {
            SafeAudit(
                "CommandDispatchFailed",
                "ServerEngine",
                atmId,
                $"No active client session for command {command?.CommandType ?? "UNKNOWN"} [{command?.CommandId ?? "-"}].");
            return false;
        }

        _ = SendFrameAsync(atmId, client, CommunicationProtocol.BuildCommand(command), CancellationToken.None);
        SafeAudit(
            "CommandDispatch",
            "ServerEngine",
            atmId,
            $"{command.CommandType} [{command.CommandId}] queued for transport.");
        return true;
    }

    public int BroadcastCommand(RemoteCommandEnvelope command)
    {
        var count = 0;
        var frame = CommunicationProtocol.BuildCommand(command);
        foreach (var pair in _clients.ToArray())
        {
            _ = SendFrameAsync(pair.Key, pair.Value, frame, CancellationToken.None);
            count++;
        }
        SafeAudit(
            "CommandBroadcast",
            "ServerEngine",
            "ALL_CONNECTED",
            $"{command.CommandType} [{command.CommandId}] queued to {count} ATM(s).");
        return count;
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_listener is null)
                    break;

                var client = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                client.NoDelay = true;
                _ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, $"Accept failed: {ex.Message}");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var remoteEndPoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        var connection = new ClientConnection { ATM_ID = $"PENDING-{Guid.NewGuid():N}"[..16], RemoteEndPoint = remoteEndPoint };

        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                while (!cancellationToken.IsCancellationRequested && client.Connected)
                {
                    var message = await Task.Run(() => CommunicationProtocol.ReadMessage(stream), cancellationToken).ConfigureAwait(false);
                    MessageReceived?.Invoke(this, message);

                    if (message.Type == CommunicationProtocol.MsgType.Handshake)
                    {
                        connection = BuildConnection(message, remoteEndPoint);
                        _connections[connection.ATM_ID] = connection;
                        _clients[connection.ATM_ID] = client;
                        ClientConnected?.Invoke(this, connection);
                        await stream.WriteAsync(CommunicationProtocol.BuildHandshakeAck(connection.SessionId), cancellationToken).ConfigureAwait(false);
                        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                        Log?.Invoke(this, $"Handshake accepted from {connection.ATM_ID} at {remoteEndPoint}.");
                    }
                    else if (message.Type == CommunicationProtocol.MsgType.Heartbeat)
                    {
                        connection.LastHeartbeatUtc = DateTime.UtcNow;
                        await stream.WriteAsync(CommunicationProtocol.BuildHeartbeatAck(connection.ATM_ID), cancellationToken).ConfigureAwait(false);
                        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else if (message.Type == CommunicationProtocol.MsgType.Broadcast)
                    {
                        if (TryParseTelemetryBroadcast(message.Text, connection.ATM_ID, out var telemetry))
                        {
                            PublishTelemetry(telemetry);
                        }
                        else if (TryParsePulseJsonBroadcast(message.Text, connection.ATM_ID, out var pulseTelemetry))
                        {
                            if (string.Equals(pulseTelemetry.ATM_ID, connection.ATM_ID, StringComparison.OrdinalIgnoreCase))
                                connection.LastHeartbeatUtc = pulseTelemetry.ReportedAtUtc;
                            PublishTelemetry(pulseTelemetry);
                        }
                        else if (TryParsePulseBroadcast(message.Text, out var pulseAtmId, out var pulseUtc))
                        {
                            if (string.Equals(pulseAtmId, connection.ATM_ID, StringComparison.OrdinalIgnoreCase))
                                connection.LastHeartbeatUtc = pulseUtc;
                        }
                    }
                    else if (message.Type == CommunicationProtocol.MsgType.StartFile)
                    {
                        var start = ParseStartFileHeader(message.Text, connection.ATM_ID);
                        if (start is null)
                        {
                            Log?.Invoke(this, $"Invalid journal start header from {connection.ATM_ID}: {message.Text}");
                        }
                        else
                        {
                            if (_incomingTransfers.TryRemove(connection.ATM_ID, out var previous))
                                previous.Dispose();
                            _incomingTransfers[connection.ATM_ID] = start;
                            JournalTransferProgress?.Invoke(this, new JournalTransferProgressPacket
                            {
                                TransferId = start.TransferId,
                                ATM_ID = connection.ATM_ID,
                                FileName = start.FileName,
                                ExpectedBytes = Math.Max(0, start.Length),
                                ReceivedBytes = start.ReceivedBytes,
                                ProgressPercent = 0,
                                State = JournalSyncState.Pending,
                                Checksum = start.Checksum,
                                Message = "Journal transfer started."
                            });
                            Log?.Invoke(this, $"Receiving journal payload: {start.FileName} ({start.Length} bytes) from {connection.ATM_ID}.");
                        }
                    }
                    else if (message.Type == CommunicationProtocol.MsgType.Chunk)
                    {
                        if (_incomingTransfers.TryGetValue(connection.ATM_ID, out var transfer))
                        {
                            transfer.Append(message.Payload);
                            if (message.Payload.Length > 0)
                                await stream.WriteAsync(CommunicationProtocol.BuildChunkAck(transfer.AppendedChunks), cancellationToken).ConfigureAwait(false);
                            JournalTransferProgress?.Invoke(this, new JournalTransferProgressPacket
                            {
                                TransferId = transfer.TransferId,
                                ATM_ID = connection.ATM_ID,
                                FileName = transfer.FileName,
                                ExpectedBytes = Math.Max(0, transfer.Length),
                                ReceivedBytes = transfer.ReceivedBytes,
                                ProgressPercent = transfer.Length > 0
                                    ? (int)Math.Min(100, Math.Round(transfer.ReceivedBytes * 100.0 / transfer.Length))
                                    : 0,
                                State = JournalSyncState.Syncing,
                                Checksum = transfer.Checksum,
                                Message = $"Chunk #{transfer.AppendedChunks} received."
                            });
                        }
                        else
                        {
                            Log?.Invoke(this, $"Chunk received without active transfer from {connection.ATM_ID}.");
                        }
                    }
                    else if (message.Type == CommunicationProtocol.MsgType.Complete)
                    {
                        var complete = ParseCompleteHeader(message.Text);
                        if (!_incomingTransfers.TryRemove(connection.ATM_ID, out var transfer))
                        {
                            await stream.WriteAsync(CommunicationProtocol.BuildJournalAck(complete.FileName, false, "No active transfer"), cancellationToken).ConfigureAwait(false);
                            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                            Log?.Invoke(this, $"Journal complete without active transfer from {connection.ATM_ID}: {message.Text}");
                            continue;
                        }

                        using (transfer)
                        {
                            var bytes = transfer.ToArray();
                            var fileName = string.IsNullOrWhiteSpace(complete.FileName) ? transfer.FileName : complete.FileName;
                            var md5 = EJLive.Shared.SecurityHelper.MD5Hash(bytes);
                            var sha256 = EJLive.Shared.SecurityHelper.SHA256Hash(bytes);
                            var receivedAtUtc = DateTime.UtcNow;
                            var checksumOk = string.IsNullOrWhiteSpace(complete.Checksum) ||
                                             string.Equals(complete.Checksum, md5, StringComparison.OrdinalIgnoreCase);
                            var shaOk = string.IsNullOrWhiteSpace(complete.Sha256) ||
                                        string.Equals(complete.Sha256, sha256, StringComparison.OrdinalIgnoreCase);
                            var lengthOk = transfer.Length <= 0 || transfer.Length == bytes.LongLength;
                            var success = checksumOk && shaOk && lengthOk;
                            var progress = transfer.Length > 0
                                ? (int)Math.Min(100, Math.Round(bytes.LongLength * 100.0 / transfer.Length))
                                : 100;
                            var stagingTimeMs = Math.Max(0L, (long)Math.Round((receivedAtUtc - transfer.StartedAtUtc).TotalMilliseconds));
                            var ackDetail = BuildRichJournalAckDetail(
                                success,
                                bytes.LongLength,
                                md5,
                                sha256,
                                stagingTimeMs,
                                receivedAtUtc,
                                lengthOk,
                                checksumOk,
                                shaOk);
                            await stream.WriteAsync(CommunicationProtocol.BuildJournalAck(fileName, success, ackDetail), cancellationToken).ConfigureAwait(false);
                            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);

                            JournalTransferProgress?.Invoke(this, new JournalTransferProgressPacket
                            {
                                TransferId = transfer.TransferId,
                                ATM_ID = connection.ATM_ID,
                                FileName = fileName,
                                ExpectedBytes = Math.Max(0, transfer.Length),
                                ReceivedBytes = bytes.LongLength,
                                ProgressPercent = success ? 100 : progress,
                                State = success ? JournalSyncState.Completed : JournalSyncState.Failed,
                                Checksum = md5,
                                Sha256 = sha256,
                                Message = ackDetail
                            });

                            if (success)
                            {
                                JournalFileReceived?.Invoke(this, new JournalFileReceivedPacket
                                {
                                    TransferId = transfer.TransferId,
                                    ATM_ID = connection.ATM_ID,
                                    FileName = fileName,
                                    Checksum = md5,
                                    Sha256 = sha256,
                                    Payload = bytes,
                                    ReceivedAtUtc = receivedAtUtc,
                                    StagingTimeMs = stagingTimeMs
                                });
                            }

                            Log?.Invoke(this,
                                $"Journal transfer completed from {connection.ATM_ID}: {fileName} ({bytes.Length} bytes), ok={success}.");
                        }
                    }
                    else if (message.Type == CommunicationProtocol.MsgType.RemoteSessionStart)
                    {
                        Log?.Invoke(this, $"Remote screen session started by {connection.ATM_ID}.");
                    }
                    else if (message.Type == CommunicationProtocol.MsgType.RemoteSessionFrame)
                    {
                        RemoteFrameReceived?.Invoke(this, new RemoteFramePacket
                        {
                            ATM_ID = connection.ATM_ID,
                            Payload = message.Payload,
                            ReceivedAtUtc = DateTime.UtcNow
                        });
                        Log?.Invoke(this, $"Remote screen frame received from {connection.ATM_ID}: {message.Payload.Length / 1024.0:N1} KB.");
                    }
                    else if (message.Type == CommunicationProtocol.MsgType.RemoteSessionStop)
                    {
                        Log?.Invoke(this, $"Remote screen session stopped by {connection.ATM_ID}.");
                    }
                    else if (message.Type == CommunicationProtocol.MsgType.CommandResult)
                    {
                        var decoded = DecodeCommandResult(message.Text);
                        Log?.Invoke(this, $"Command result from {connection.ATM_ID}: {decoded}");
                        SafeAudit("CommandResult", "ServerEngine", connection.ATM_ID, decoded);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (IOException ex)
        {
            Log?.Invoke(this, $"Client {connection.ATM_ID} disconnected: {ex.Message}");
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, $"Client {connection.ATM_ID} failed: {ex.Message}");
        }
        finally
        {
            if (!connection.ATM_ID.StartsWith("PENDING-", StringComparison.OrdinalIgnoreCase))
                Remove(connection.ATM_ID);
        }
    }

    private static ClientConnection BuildConnection(EJMessage message, string remoteEndPoint)
    {
        var parts = message.Text.Split('|');
        return new ClientConnection
        {
            ATM_ID = parts.ElementAtOrDefault(0) ?? "UNKNOWN",
            ATM_Type = parts.ElementAtOrDefault(1) ?? string.Empty,
            ClientVersion = parts.ElementAtOrDefault(2) ?? string.Empty,
            RemoteEndPoint = remoteEndPoint,
            ConnectedAtUtc = DateTime.UtcNow,
            LastHeartbeatUtc = DateTime.UtcNow
        };
    }

    private async Task SendFrameAsync(string atmId, TcpClient client, byte[] frame, CancellationToken cancellationToken)
    {
        try
        {
            if (!client.Connected)
                return;
            var stream = client.GetStream();
            await stream.WriteAsync(frame, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, $"Send to {atmId} failed: {ex.Message}");
        }
    }

    private static string DecodeCommandResult(string text)
    {
        var parts = (text ?? string.Empty).Split('|');
        if (parts.Length < 3)
            return text ?? string.Empty;

        var message = parts[2];
        try
        {
            message = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(parts[2]));
        }
        catch (FormatException)
        {
        }
        return $"{parts[0]} {parts[1]} {message}";
    }

    private void PublishTelemetry(ClientTelemetryPacket telemetry)
    {
        ClientTelemetryReceived?.Invoke(this, telemetry);
        SafeAudit(
            "ClientTelemetry",
            "ServerEngine",
            telemetry.ATM_ID,
            BuildTelemetryAuditDetail(telemetry));
        SafeStoreTelemetry(telemetry);
        Log?.Invoke(this,
            $"Client telemetry from {telemetry.ATM_ID}: {telemetry.Severity}/{telemetry.EventType} - {telemetry.Detail}");
    }

    private static void SafeAudit(string action, string performedBy, string atmId, string detail)
    {
        try
        {
            DatabaseManager.Instance.InsertAuditLog(
                action,
                performedBy,
                atmId,
                detail);
        }
        catch
        {
            // Audit failures must not interrupt command transport runtime.
        }
    }

    private static void SafeStoreTelemetry(ClientTelemetryPacket packet)
    {
        try
        {
            DatabaseManager.Instance.InsertTelemetryEvent(
                packet.ATM_ID,
                packet.EventType,
                packet.Severity,
                packet.Detail,
                packet.ReportedAtUtc,
                packet.RawJson);
        }
        catch
        {
            // Telemetry persistence failures must not interrupt command transport runtime.
        }
    }

    private static string BuildRichJournalAckDetail(
        bool success,
        long sizeBytes,
        string md5,
        string sha256,
        long stagingTimeMs,
        DateTime receivedAtUtc,
        bool lengthOk,
        bool checksumOk,
        bool shaOk)
    {
        var status = success ? "stored" : "rejected";
        var detail =
            $"status={status};size={Math.Max(0, sizeBytes)};sha256={sha256};md5={md5};staging_time_ms={Math.Max(0, stagingTimeMs)};received_at_utc={receivedAtUtc:O}";
        if (success)
            return detail + ";verified=true";

        return detail + $";verified=false;len_ok={lengthOk};md5_ok={checksumOk};sha256_ok={shaOk}";
    }

    private static IncomingJournalTransfer? ParseStartFileHeader(string text, string defaultAtmId)
    {
        var parts = (text ?? string.Empty).Split('|');
        if (parts.Length < 2)
            return null;

        var atmId = string.IsNullOrWhiteSpace(parts.ElementAtOrDefault(0)) ? defaultAtmId : parts[0];
        var fileName = string.IsNullOrWhiteSpace(parts.ElementAtOrDefault(1)) ? "journal.bin" : parts[1];
        long length = 0;
        long.TryParse(parts.ElementAtOrDefault(2), out length);
        long offset = 0;
        long.TryParse(parts.ElementAtOrDefault(3), out offset);
        var checksum = parts.ElementAtOrDefault(4) ?? string.Empty;
        return new IncomingJournalTransfer(atmId, fileName, length, offset, checksum);
    }

    private static (string FileName, string Checksum, string Sha256) ParseCompleteHeader(string text)
    {
        var parts = (text ?? string.Empty).Split('|');
        return (
            parts.ElementAtOrDefault(0) ?? "journal",
            parts.ElementAtOrDefault(1) ?? string.Empty,
            parts.ElementAtOrDefault(2) ?? string.Empty);
    }

    private static bool TryParsePulseBroadcast(string text, out string atmId, out DateTime pulseUtc)
    {
        atmId = string.Empty;
        pulseUtc = DateTime.UtcNow;
        var parts = (text ?? string.Empty).Split('|');
        if (parts.Length < 3)
            return false;
        if (!string.Equals(parts[0], "PULSE", StringComparison.OrdinalIgnoreCase))
            return false;

        atmId = parts[1].Trim();
        if (string.IsNullOrWhiteSpace(atmId))
            return false;

        if (DateTime.TryParse(parts[2], out var parsed))
            pulseUtc = parsed.ToUniversalTime();
        return true;
    }

    private static bool TryParsePulseJsonBroadcast(string text, string fallbackAtmId, out ClientTelemetryPacket packet)
    {
        packet = new ClientTelemetryPacket
        {
            ATM_ID = fallbackAtmId,
            EventType = "pulse_json",
            Severity = "info",
            Detail = string.Empty,
            ReportedAtUtc = DateTime.UtcNow
        };

        var value = (text ?? string.Empty).Trim();
        if (!value.StartsWith("PULSE_JSON|", StringComparison.OrdinalIgnoreCase))
            return false;

        var payload = value["PULSE_JSON|".Length..].Trim();
        if (string.IsNullOrWhiteSpace(payload))
            return false;

        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return false;

            var root = document.RootElement;
            packet.RawJson = payload;
            packet.ATM_ID = ReadJsonValue(root, "terminalId", "atmId", "ATM_ID", "agentId")?.Trim() is { Length: > 0 } atm
                ? atm
                : fallbackAtmId;

            var utcText = ReadJsonValue(root, "timestampUtc", "timestamp", "utc");
            if (DateTime.TryParse(utcText, out var parsedUtc))
                packet.ReportedAtUtc = parsedUtc.ToUniversalTime();

            var serviceState = ReadJsonValue(root, "serviceState", "state");
            var networkType = ReadJsonValue(root, "networkType", "network");
            var pendingOutboxText = ReadJsonValue(root, "pendingOutbox", "pending", "pendingFiles");
            var handshakeText = ReadJsonValue(root, "handshake", "handshakeOk");

            var severity = "info";
            if (bool.TryParse(handshakeText, out var handshakeOk) && !handshakeOk)
                severity = "warning";

            if (!string.IsNullOrWhiteSpace(serviceState) &&
                (serviceState.Contains("disconnect", StringComparison.OrdinalIgnoreCase) ||
                 serviceState.Contains("degraded", StringComparison.OrdinalIgnoreCase)))
                severity = "warning";

            packet.Severity = severity;
            packet.Detail =
                $"state={serviceState ?? "unknown"};handshake={NormalizePulseValue(handshakeText)};pending={NormalizePulseValue(pendingOutboxText)};network={networkType ?? "unknown"}";

            return !string.IsNullOrWhiteSpace(packet.ATM_ID);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryParseTelemetryBroadcast(string text, string fallbackAtmId, out ClientTelemetryPacket packet)
    {
        packet = new ClientTelemetryPacket
        {
            ATM_ID = fallbackAtmId,
            EventType = string.Empty,
            Severity = "info",
            Detail = string.Empty,
            ReportedAtUtc = DateTime.UtcNow
        };

        var value = (text ?? string.Empty).Trim();
        if (!value.StartsWith("TELEMETRY|", StringComparison.OrdinalIgnoreCase))
            return false;

        var payload = value["TELEMETRY|".Length..];
        if (string.IsNullOrWhiteSpace(payload))
            return false;

        var map = ParseKeyValuePayload(payload);
        if (map.Count == 0)
            return false;

        packet.ATM_ID = map.TryGetValue("ATM", out var atm) && !string.IsNullOrWhiteSpace(atm)
            ? atm.Trim()
            : fallbackAtmId;
        packet.EventType = map.TryGetValue("Type", out var type) ? type.Trim() : string.Empty;
        packet.Severity = map.TryGetValue("Severity", out var severity) && !string.IsNullOrWhiteSpace(severity)
            ? severity.Trim()
            : "info";
        if (map.TryGetValue("Utc", out var utcText) && DateTime.TryParse(utcText, out var parsedUtc))
            packet.ReportedAtUtc = parsedUtc.ToUniversalTime();

        if (map.TryGetValue("DetailB64", out var detailB64) && !string.IsNullOrWhiteSpace(detailB64))
        {
            try
            {
                packet.Detail = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(detailB64));
            }
            catch (FormatException)
            {
                packet.Detail = detailB64;
            }
        }
        else if (map.TryGetValue("Detail", out var detail))
        {
            packet.Detail = detail;
        }

        return !string.IsNullOrWhiteSpace(packet.ATM_ID) && !string.IsNullOrWhiteSpace(packet.EventType);
    }

    private static Dictionary<string, string> ParseKeyValuePayload(string payload)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var token in (payload ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var index = token.IndexOf('=');
            if (index <= 0)
                continue;
            var key = token[..index].Trim();
            var val = token[(index + 1)..].Trim();
            if (!string.IsNullOrWhiteSpace(key))
                map[key] = val;
        }

        return map;
    }

    private static string? ReadJsonValue(JsonElement root, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!TryGetJsonProperty(root, key, out var value))
                continue;

            if (value.ValueKind == JsonValueKind.String)
                return value.GetString();

            if (value.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
                return value.ToString();
        }

        return null;
    }

    private static bool TryGetJsonProperty(JsonElement root, string key, out JsonElement value)
    {
        foreach (var prop in root.EnumerateObject())
        {
            if (string.Equals(prop.Name, key, StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static string NormalizePulseValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim();

    private static string BuildTelemetryAuditDetail(ClientTelemetryPacket packet)
    {
        var detail = $"{packet.Severity}|{packet.EventType}|{packet.Detail}";
        return detail.Length <= 700 ? detail : detail[..700];
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
    }
}

internal sealed class IncomingJournalTransfer : IDisposable
{
    private readonly MemoryStream _buffer;

    public IncomingJournalTransfer(string atmId, string fileName, long length, long offset, string checksum)
    {
        TransferId = Guid.NewGuid().ToString("N");
        ATM_ID = atmId;
        FileName = fileName;
        Length = length;
        Offset = offset;
        Checksum = checksum;
        StartedAtUtc = DateTime.UtcNow;
        _buffer = length > 0 && length <= int.MaxValue
            ? new MemoryStream((int)length)
            : new MemoryStream();
    }

    public string TransferId { get; }
    public string ATM_ID { get; }
    public string FileName { get; }
    public long Length { get; }
    public long Offset { get; }
    public string Checksum { get; }
    public DateTime StartedAtUtc { get; }
    public int AppendedChunks { get; private set; }
    public long ReceivedBytes => _buffer.Length;

    public void Append(byte[] payload)
    {
        if (payload.Length == 0)
            return;
        _buffer.Write(payload, 0, payload.Length);
        AppendedChunks++;
    }

    public byte[] ToArray() => _buffer.ToArray();

    public void Dispose() => _buffer.Dispose();
}

public sealed class ClientConnection
{
    public string ATM_ID { get; set; } = string.Empty;
    public string ATM_Type { get; set; } = string.Empty;
    public string ClientVersion { get; set; } = string.Empty;
    public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime ConnectedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastHeartbeatUtc { get; set; } = DateTime.UtcNow;
    public string RemoteEndPoint { get; set; } = string.Empty;
}

public sealed class RemoteFramePacket
{
    public string ATM_ID { get; set; } = string.Empty;
    public byte[] Payload { get; set; } = Array.Empty<byte>();
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class JournalFileReceivedPacket
{
    public string TransferId { get; set; } = string.Empty;
    public string ATM_ID { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Checksum { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public byte[] Payload { get; set; } = Array.Empty<byte>();
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
    public long StagingTimeMs { get; set; }
}

public sealed class JournalTransferProgressPacket
{
    public string TransferId { get; set; } = string.Empty;
    public string ATM_ID { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long ExpectedBytes { get; set; }
    public long ReceivedBytes { get; set; }
    public int ProgressPercent { get; set; }
    public JournalSyncState State { get; set; } = JournalSyncState.Pending;
    public string Checksum { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ReportedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class ClientTelemetryPacket
{
    public string ATM_ID { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = "info";
    public string Detail { get; set; } = string.Empty;
    public DateTime ReportedAtUtc { get; set; } = DateTime.UtcNow;
    public string RawJson { get; set; } = string.Empty;
}
