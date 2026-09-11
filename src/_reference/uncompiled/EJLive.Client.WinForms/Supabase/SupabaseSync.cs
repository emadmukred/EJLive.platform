using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EJLive.Client.WinForms.Supabase;

/// <summary>
/// Lightweight Supabase REST client.
/// No external SDK — pure System.Net.Http.
/// Queues events offline and flushes on reconnect.
/// </summary>
public sealed class SupabaseSync : IDisposable
{
    private static readonly HttpClient SharedHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private readonly string  _url;
    private readonly string  _key;
    private readonly string  _atmId;
    private readonly string  _atmType;
    private readonly ConcurrentQueue<QueuedRow> _pending = new();
    private System.Threading.Timer? _flushTimer;
    private bool _running;

    public event Action<string>? OnLog;
    public bool IsConfigured => !string.IsNullOrEmpty(_url) && !string.IsNullOrEmpty(_key);
    public int PendingCount => _pending.Count;

    public SupabaseSync(string? url, string? key, string atmId, string atmType)
    {
        _url    = (url ?? "").Trim();
        _key    = (key ?? "").Trim();
        _atmId  = atmId;
        _atmType= atmType;
    }

    public void Start()
    {
        _running = true;
        if (!IsConfigured)
        {
            Log("Supabase not configured — buffering events locally.");
            return;
        }

        _flushTimer = new System.Threading.Timer(_ => Flush(),
            null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
        Log("Supabase sync started.");
    }

    public void Stop() { _running = false; _flushTimer?.Dispose(); }

    public void RegisterAgent(string atmType, string serverIp, int serverPort)
    {
        var row = new AtmAgentRow(_atmId, atmType, null, null, serverIp, serverPort,
            "5.0.0", true, DateTime.UtcNow.ToString("O"),
            Environment.MachineName, Environment.UserName, Environment.OSVersion.Version.ToString());
        Enqueue("atm_agents", row);
    }

    public void LogEvent(string eventType, string details, string severity = "info")
    {
        var row = new AgentEventRow(_atmId, eventType, details, severity, DateTime.UtcNow.ToString("O"));
        Enqueue("agent_events", row);
    }

    public void PushHeartbeat(bool connected, int pending)
    {
        var row = new AgentHeartbeatRow(_atmId, DateTime.UtcNow.ToString("O"),
            connected, pending, Environment.TickCount64 / 60000.0);
        Enqueue("agent_heartbeats", row);
    }

    public void LogFileTransfer(string filePath, long size)
    {
        var row = new FileTransferRow(_atmId, Path.GetFileName(filePath),
            size, "sending", DateTime.UtcNow.ToString("O"));
        Enqueue("file_transfers", row);
    }

    public void LogDeliveryConfirmation(string fileName, bool success, string detail)
    {
        var status = success ? "acknowledged" : "failed";
        var row = new FileTransferRow(_atmId, Path.GetFileName(fileName),
            0, status, DateTime.UtcNow.ToString("O"));
        Enqueue("file_transfers", row);
        LogEvent("file_delivery_confirmation", $"{fileName}|{status}|{detail}", success ? "info" : "warning");
    }

    public void LogScreenshot(string path, long bytes)
    {
        var row = new ScreenshotRow(_atmId, path, DateTime.UtcNow.ToString("O"), (int)Math.Max(1, bytes / 1024));
        Enqueue("screenshots", row);
    }

    private void Enqueue(string table, object row)
    {
        _pending.Enqueue(new QueuedRow(table, JsonSerializer.Serialize(row)));
    }

    private void Flush()
    {
        if (!IsConfigured || !_running) return;

        var count = 0;
        var started = DateTime.UtcNow;
        while (_pending.TryDequeue(out var item))
        {
            if (!PostRow(item.table, item.json))
            {
                _pending.Enqueue(item); // re-queue on failure
                break;
            }

            count++;
            if (count >= 100 || (DateTime.UtcNow - started) > TimeSpan.FromSeconds(5))
                break;
        }

        if (count > 0) Log($"Flushed {count} events to Supabase.");
    }

    private bool PostRow(string table, string json)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_url.TrimEnd('/')}/rest/v1/{table}")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("apikey", _key);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _key);
            request.Headers.Add("Prefer", "return=minimal");

            using var response = SharedHttpClient.Send(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private void Log(string m) => OnLog?.Invoke($"[SUPA] {m}");
    public void Dispose() => Stop();

    private readonly record struct QueuedRow(string table, string json);

    private sealed record ScreenshotRow(
        string agent_id,
        string? storage_url,
        string taken_at_utc,
        int file_size_kb);
}
