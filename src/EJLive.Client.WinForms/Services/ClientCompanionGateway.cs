using System.Globalization;
using System.Text.Json;

namespace EJLive.Client.WinForms.Services;

public sealed record ClientComponentSnapshot(
    string Name,
    string Status,
    string Detail);

public sealed record ClientRuntimeSnapshot(
    DateTime CapturedAtUtc,
    string SnapshotSource,
    string AtmId,
    string AgentState,
    bool Connected,
    bool HandshakeComplete,
    int PendingOutboxItems,
    long TotalBytesSent,
    long TotalBytesReceived,
    DateTime? LastHeartbeatUtc,
    DateTime? LastJournalSyncUtc,
    string SessionId,
    string LastError,
    double UptimeSeconds,
    IReadOnlyList<ClientComponentSnapshot> Components);

public sealed record ClientServiceQueryResult(
    bool Available,
    string Message,
    DateTime? CapturedAtUtc,
    string State);

public sealed class ClientGatewayContext
{
    public string AtmId { get; init; } = string.Empty;
    public string AgentState { get; init; } = "Unknown";
    public bool Connected { get; init; }
    public bool HandshakeComplete { get; init; }
    public int PendingOutboxItems { get; init; }
    public long TotalBytesSent { get; init; }
    public long TotalBytesReceived { get; init; }
    public DateTime? LastHeartbeatUtc { get; init; }
    public DateTime? LastJournalSyncUtc { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public string LastError { get; init; } = string.Empty;
    public double UptimeSeconds { get; init; }
    public IReadOnlyList<ClientComponentSnapshot> Components { get; init; } =
        Array.Empty<ClientComponentSnapshot>();
}

/// <summary>
/// Read-only boundary used by the interactive companion. Implementations may
/// inspect an agent health snapshot, but cannot start work, mutate paths, ping
/// a remote endpoint, or submit an operational request.
/// </summary>
public interface IClientServiceGateway
{
    Task<ClientRuntimeSnapshot> GetRuntimeSnapshotAsync(
        CancellationToken cancellationToken = default);

    Task<ClientServiceQueryResult> QueryLocalServiceAsync(
        CancellationToken cancellationToken = default);
}

public sealed class InProcessClientServiceGateway : IClientServiceGateway
{
    private readonly Func<ClientGatewayContext> _contextProvider;
    private readonly string _serviceHealthFilePath;

    public InProcessClientServiceGateway(
        Func<ClientGatewayContext> contextProvider,
        string? serviceHealthFilePath = null)
    {
        _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
        _serviceHealthFilePath = string.IsNullOrWhiteSpace(serviceHealthFilePath)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive",
                "Agent",
                "health.json")
            : serviceHealthFilePath;
    }

    public Task<ClientRuntimeSnapshot> GetRuntimeSnapshotAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = _contextProvider();

        if (TryReadServiceSnapshot(_serviceHealthFilePath, out var serviceSnapshot))
        {
            return Task.FromResult(new ClientRuntimeSnapshot(
                CapturedAtUtc: serviceSnapshot.TimestampUtc,
                SnapshotSource: "ServiceHealthFile",
                AtmId: Coalesce(serviceSnapshot.AtmId, context.AtmId),
                AgentState: serviceSnapshot.State,
                Connected: serviceSnapshot.Connected,
                HandshakeComplete: serviceSnapshot.HandshakeComplete,
                PendingOutboxItems: Math.Max(0, serviceSnapshot.PendingOutboxItems),
                TotalBytesSent: Math.Max(0, serviceSnapshot.TotalBytesSent),
                TotalBytesReceived: Math.Max(0, serviceSnapshot.TotalBytesReceived),
                LastHeartbeatUtc: serviceSnapshot.LastHeartbeatUtc,
                LastJournalSyncUtc: serviceSnapshot.LastJournalSyncUtc,
                SessionId: serviceSnapshot.SessionId,
                LastError: serviceSnapshot.LastError,
                UptimeSeconds: Math.Max(0, serviceSnapshot.UptimeSeconds),
                Components: CopyComponents(context.Components)));
        }

        return Task.FromResult(new ClientRuntimeSnapshot(
            CapturedAtUtc: DateTime.UtcNow,
            SnapshotSource: "InProcessFallback",
            AtmId: context.AtmId,
            AgentState: context.AgentState,
            Connected: context.Connected,
            HandshakeComplete: context.HandshakeComplete,
            PendingOutboxItems: Math.Max(0, context.PendingOutboxItems),
            TotalBytesSent: Math.Max(0, context.TotalBytesSent),
            TotalBytesReceived: Math.Max(0, context.TotalBytesReceived),
            LastHeartbeatUtc: context.LastHeartbeatUtc,
            LastJournalSyncUtc: context.LastJournalSyncUtc,
            SessionId: context.SessionId,
            LastError: context.LastError,
            UptimeSeconds: Math.Max(0, context.UptimeSeconds),
            Components: CopyComponents(context.Components)));
    }

    public Task<ClientServiceQueryResult> QueryLocalServiceAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!TryReadServiceSnapshot(_serviceHealthFilePath, out var snapshot))
        {
            return Task.FromResult(new ClientServiceQueryResult(
                Available: false,
                Message: "Agent health snapshot is not available.",
                CapturedAtUtc: null,
                State: "Unknown"));
        }

        return Task.FromResult(new ClientServiceQueryResult(
            Available: true,
            Message: $"Agent health is available; state={snapshot.State}, captured={snapshot.TimestampUtc:O}.",
            CapturedAtUtc: snapshot.TimestampUtc,
            State: snapshot.State));
    }

    private static IReadOnlyList<ClientComponentSnapshot> CopyComponents(
        IReadOnlyList<ClientComponentSnapshot>? components)
        => components is null || components.Count == 0
            ? Array.Empty<ClientComponentSnapshot>()
            : components.ToArray();

    private static string Coalesce(string? primary, string fallback)
        => string.IsNullOrWhiteSpace(primary) ? fallback : primary.Trim();

    private static bool TryReadServiceSnapshot(string filePath, out ServiceHealthSnapshot snapshot)
    {
        snapshot = ServiceHealthSnapshot.Empty;

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return false;

        try
        {
            using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            using var document = JsonDocument.Parse(stream);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return false;

            var root = document.RootElement;
            snapshot = new ServiceHealthSnapshot(
                AtmId: ReadJsonString(root, "atmId"),
                TimestampUtc: ReadJsonDateTime(root, "timestampUtc") ?? DateTime.UtcNow,
                State: ResolveState(root, "state"),
                Connected: ReadJsonBool(root, "connected"),
                HandshakeComplete: ReadJsonBool(root, "handshakeComplete"),
                PendingOutboxItems: ReadJsonInt(root, "pendingOutboxItems"),
                TotalBytesSent: ReadJsonLong(root, "totalBytesSent"),
                TotalBytesReceived: ReadJsonLong(root, "totalBytesReceived"),
                LastHeartbeatUtc: ReadJsonDateTime(root, "lastHeartbeatUtc"),
                LastJournalSyncUtc: ReadJsonDateTime(root, "lastJournalSyncUtc"),
                SessionId: ReadJsonString(root, "sessionId"),
                LastError: ReadJsonString(root, "lastError"),
                UptimeSeconds: ReadJsonDouble(root, "uptimeSeconds"));
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string ResolveState(JsonElement root, string key)
    {
        if (!TryGetJsonProperty(root, key, out var value))
            return "Unknown";

        if (value.ValueKind == JsonValueKind.String)
            return value.GetString() ?? "Unknown";

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var numeric))
        {
            return numeric switch
            {
                0 => "Stopped",
                1 => "Starting",
                2 => "Running",
                3 => "Paused",
                4 => "Failed",
                _ => "Unknown"
            };
        }

        return "Unknown";
    }

    private static string ReadJsonString(JsonElement root, string key)
    {
        if (!TryGetJsonProperty(root, key, out var value))
            return string.Empty;

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => value.ToString(),
            _ => string.Empty
        };
    }

    private static bool ReadJsonBool(JsonElement root, string key)
    {
        if (!TryGetJsonProperty(root, key, out var value))
            return false;

        if (value.ValueKind is JsonValueKind.True or JsonValueKind.False)
            return value.GetBoolean();

        return value.ValueKind == JsonValueKind.String &&
               bool.TryParse(value.GetString(), out var parsed) &&
               parsed;
    }

    private static int ReadJsonInt(JsonElement root, string key)
    {
        if (!TryGetJsonProperty(root, key, out var value))
            return 0;

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var direct))
            return direct;

        return value.ValueKind == JsonValueKind.String &&
               int.TryParse(
                   value.GetString(),
                   NumberStyles.Integer,
                   CultureInfo.InvariantCulture,
                   out var parsed)
            ? parsed
            : 0;
    }

    private static long ReadJsonLong(JsonElement root, string key)
    {
        if (!TryGetJsonProperty(root, key, out var value))
            return 0;

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var direct))
            return direct;

        return value.ValueKind == JsonValueKind.String &&
               long.TryParse(
                   value.GetString(),
                   NumberStyles.Integer,
                   CultureInfo.InvariantCulture,
                   out var parsed)
            ? parsed
            : 0;
    }

    private static double ReadJsonDouble(JsonElement root, string key)
    {
        if (!TryGetJsonProperty(root, key, out var value))
            return 0;

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var direct))
            return direct;

        return value.ValueKind == JsonValueKind.String &&
               double.TryParse(
                   value.GetString(),
                   NumberStyles.Float,
                   CultureInfo.InvariantCulture,
                   out var parsed)
            ? parsed
            : 0;
    }

    private static DateTime? ReadJsonDateTime(JsonElement root, string key)
    {
        if (!TryGetJsonProperty(root, key, out var value))
            return null;

        var text = value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
        if (!DateTime.TryParse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var parsed))
        {
            return null;
        }

        return parsed.Kind == DateTimeKind.Utc ? parsed : parsed.ToUniversalTime();
    }

    private static bool TryGetJsonProperty(JsonElement root, string key, out JsonElement value)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (!string.Equals(property.Name, key, StringComparison.OrdinalIgnoreCase))
                continue;

            value = property.Value;
            return true;
        }

        value = default;
        return false;
    }

    private sealed record ServiceHealthSnapshot(
        string AtmId,
        DateTime TimestampUtc,
        string State,
        bool Connected,
        bool HandshakeComplete,
        int PendingOutboxItems,
        long TotalBytesSent,
        long TotalBytesReceived,
        DateTime? LastHeartbeatUtc,
        DateTime? LastJournalSyncUtc,
        string SessionId,
        string LastError,
        double UptimeSeconds)
    {
        public static ServiceHealthSnapshot Empty { get; } = new(
            AtmId: string.Empty,
            TimestampUtc: DateTime.MinValue,
            State: "Unknown",
            Connected: false,
            HandshakeComplete: false,
            PendingOutboxItems: 0,
            TotalBytesSent: 0,
            TotalBytesReceived: 0,
            LastHeartbeatUtc: null,
            LastJournalSyncUtc: null,
            SessionId: string.Empty,
            LastError: string.Empty,
            UptimeSeconds: 0);
    }
}
