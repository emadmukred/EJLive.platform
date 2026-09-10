using System;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace EJLive.Client.Service;

public sealed record HealthReport(
    string AtmId,
    DateTime TimestampUtc,
    AgentControllerState State,
    bool Connected,
    bool HandshakeComplete,
    int PendingOutboxItems,
    long TotalBytesSent,
    long TotalBytesReceived,
    DateTime? LastHeartbeatUtc,
    DateTime? LastJournalSyncUtc,
    string? SessionId,
    string? LastError,
    double UptimeSeconds);

/// <summary>
/// Health reporter for the headless agent.
/// Keeps backward compatibility with older Start/Stop/Emit callers while using
/// atomic JSON writes to prevent partially-written health endpoint files.
/// </summary>
public sealed class AgentHealthReporter : IDisposable
{
    private readonly IAgentController _agent;
    private readonly string? _endpointFilePath;
    private readonly System.Threading.Timer? _reportTimer;
    private readonly DateTime _startedAt;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly object _writeLock = new();

    public AgentHealthReporter(IAgentController agent, TimeSpan interval, string? endpointFilePath = null)
    {
        _agent = agent ?? throw new ArgumentNullException(nameof(agent));
        _endpointFilePath = endpointFilePath;
        _startedAt = DateTime.UtcNow;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _reportTimer = new System.Threading.Timer(_ => Publish(), null, interval, interval);
    }

    // Backward-compatible constructor used by older service code.
    public AgentHealthReporter(IAgentController agent, string outputPath, double intervalSeconds = 30)
        : this(agent, TimeSpan.FromSeconds(intervalSeconds <= 0 ? 30 : intervalSeconds), outputPath)
    {
    }

    public void Start() => Publish();

    public void Stop()
    {
        _reportTimer?.Change(Timeout.Infinite, Timeout.Infinite);
    }

    public void Emit() => Publish();

    public void Publish()
    {
        try
        {
            var status = _agent.GetStatus();
            var report = new HealthReport(
                AtmId: _agent.AtmId,
                TimestampUtc: DateTime.UtcNow,
                State: status.State,
                Connected: status.Connected,
                HandshakeComplete: status.HandshakeComplete,
                PendingOutboxItems: status.PendingOutboxItems,
                TotalBytesSent: status.TotalBytesSent,
                TotalBytesReceived: status.TotalBytesReceived,
                LastHeartbeatUtc: status.LastHeartbeatUtc,
                LastJournalSyncUtc: status.LastJournalSyncUtc,
                SessionId: status.SessionId,
                LastError: status.LastError,
                UptimeSeconds: (DateTime.UtcNow - _startedAt).TotalSeconds);

            if (string.IsNullOrWhiteSpace(_endpointFilePath))
                return;

            var dir = Path.GetDirectoryName(_endpointFilePath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(report, _jsonOptions);

            lock (_writeLock)
            {
                var tempPath = _endpointFilePath + ".tmp";
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, _endpointFilePath, overwrite: true);
            }
        }
        catch (Exception ex)
        {
            TryWriteFallback(ex);
        }
    }

    private static void TryWriteFallback(Exception ex)
    {
        try
        {
            var fallbackDir = Path.Combine(Path.GetTempPath(), "EJLive");
            Directory.CreateDirectory(fallbackDir);
            var fallback = Path.Combine(fallbackDir, $"ejlive-health-failure-{Guid.NewGuid():N}.log");
            File.WriteAllText(fallback, ex.ToString());
        }
        catch
        {
            // Last resort: never crash the service from health publishing.
        }
    }

    public void Dispose()
    {
        Stop();
        _reportTimer?.Dispose();
    }
}
