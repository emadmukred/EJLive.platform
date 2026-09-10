using System.Data;
using System.Data.SQLite;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Xml.Linq;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Core.Services;

public sealed class DatabaseManager
{
    private string _databasePath = AppConstants.DefaultDatabasePath;
    private readonly object _initLock = new();
    public static DatabaseManager Instance { get; } = new();
    public bool IsInitialized { get; private set; }

    private DatabaseManager()
    {
    }

    public void Initialize(string databasePath)
    {
        lock (_initLock)
        {
            _databasePath = string.IsNullOrWhiteSpace(databasePath) ? AppConstants.DefaultDatabasePath : databasePath;
            Directory.CreateDirectory(Path.GetDirectoryName(_databasePath) ?? ".");
            using var connection = CreateConnection();
            connection.Open();
            ApplyConnectionPragmas(connection);
            using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS audit_log (
                    entry_id TEXT PRIMARY KEY,
                    user_name TEXT NOT NULL,
                    action TEXT NOT NULL,
                    target TEXT NOT NULL,
                    created_at_utc TEXT NOT NULL,
                    details TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS sync_records (
                    sync_id TEXT PRIMARY KEY,
                    atm_id TEXT NOT NULL,
                    file_name TEXT NOT NULL,
                    state TEXT NOT NULL,
                    progress INTEGER NOT NULL,
                    updated_at_utc TEXT NOT NULL
                );
                """;
            command.ExecuteNonQuery();
            EnsureColumn(connection, "audit_log", "entry_id", "TEXT");
            EnsureColumn(connection, "audit_log", "user_name", "TEXT NOT NULL DEFAULT ''");
            EnsureColumn(connection, "audit_log", "action", "TEXT NOT NULL DEFAULT ''");
            EnsureColumn(connection, "audit_log", "target", "TEXT NOT NULL DEFAULT ''");
            EnsureColumn(connection, "audit_log", "created_at_utc", "TEXT NOT NULL DEFAULT ''");
            EnsureColumn(connection, "audit_log", "details", "TEXT NOT NULL DEFAULT ''");
            EnsureColumn(connection, "sync_records", "sync_id", "TEXT");
            EnsureColumn(connection, "sync_records", "atm_id", "TEXT NOT NULL DEFAULT ''");
            EnsureColumn(connection, "sync_records", "file_name", "TEXT NOT NULL DEFAULT ''");
            EnsureColumn(connection, "sync_records", "state", "TEXT NOT NULL DEFAULT ''");
            EnsureColumn(connection, "sync_records", "progress", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumn(connection, "sync_records", "updated_at_utc", "TEXT NOT NULL DEFAULT ''");
            CreateIndexes(connection);
            IsInitialized = true;
        }
    }

    public SQLiteConnection CreateConnection()
    {
        return new SQLiteConnection($"Data Source={_databasePath};Version=3;Pooling=True;");
    }

    public int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
    {
        EnsureInitialized();
        using var connection = CreateConnection();
        connection.Open();
        ApplyConnectionPragmas(connection);
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);
        return command.ExecuteNonQuery();
    }

    public DataTable Query(string sql, params SQLiteParameter[] parameters)
    {
        EnsureInitialized();
        using var connection = CreateConnection();
        connection.Open();
        ApplyConnectionPragmas(connection);
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);
        using var adapter = new SQLiteDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    private void EnsureInitialized()
    {
        if (!IsInitialized)
            Initialize(_databasePath);
    }

    private static void ApplyConnectionPragmas(SQLiteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA journal_mode=WAL;
            PRAGMA synchronous=NORMAL;
            PRAGMA busy_timeout=5000;
        """;
        command.ExecuteNonQuery();
    }

    private static void EnsureColumn(SQLiteConnection connection, string tableName, string columnName, string definition)
    {
        using var infoCommand = connection.CreateCommand();
        infoCommand.CommandText = $"PRAGMA table_info({tableName});";
        using var reader = infoCommand.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(Convert.ToString(reader["name"]), columnName, StringComparison.OrdinalIgnoreCase))
                return;
        }

        using var alterCommand = connection.CreateCommand();
        alterCommand.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {definition};";
        alterCommand.ExecuteNonQuery();
    }

    private static void CreateIndexes(SQLiteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE INDEX IF NOT EXISTS ix_audit_log_created_at ON audit_log(created_at_utc);
            CREATE INDEX IF NOT EXISTS ix_audit_log_target ON audit_log(target);
            CREATE INDEX IF NOT EXISTS ix_sync_records_atm_state ON sync_records(atm_id, state);
            CREATE INDEX IF NOT EXISTS ix_sync_records_updated ON sync_records(updated_at_utc);
            """;
        command.ExecuteNonQuery();
    }
}

public sealed class AuditLogger
{
    public void Record(AuditLogEntry entry)
    {
        DatabaseManager.Instance.ExecuteNonQuery(
            "INSERT OR REPLACE INTO audit_log(entry_id,user_name,action,target,created_at_utc,details) VALUES(@id,@user,@action,@target,@created,@details)",
            new SQLiteParameter("@id", entry.EntryId),
            new SQLiteParameter("@user", entry.UserName),
            new SQLiteParameter("@action", entry.Action),
            new SQLiteParameter("@target", entry.Target),
            new SQLiteParameter("@created", entry.CreatedAtUtc.ToString("O")),
            new SQLiteParameter("@details", entry.Details));
    }
}

public sealed class AlertManager
{
    private readonly List<AlertPayload> _alerts = new();
    public event EventHandler<AlertPayload>? AlertRaised;
    public IReadOnlyList<AlertPayload> Alerts => _alerts;

    public AlertPayload Raise(AlertSeverity severity, string title, string message, string source, string dedupeKey = "")
    {
        var existing = !string.IsNullOrWhiteSpace(dedupeKey)
            ? _alerts.FirstOrDefault(a => a.DedupeKey == dedupeKey && !a.IsRead)
            : null;
        if (existing is not null)
            return existing;

        var alert = new AlertPayload { Severity = severity, Title = title, Message = message, Source = source, DedupeKey = dedupeKey };
        _alerts.Add(alert);
        AlertRaised?.Invoke(this, alert);
        return alert;
    }
}

public static class AgentConfigurationXmlService
{
    public static AppConfig LoadAppConfig(AppConfig fallback)
    {
        fallback.ApplyDefaults();
        return fallback;
    }

    public static AgentConfigurationRecord LoadOrCreate(AppConfig config)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Client", "AgentConf.xml");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        if (!File.Exists(path))
        {
            var doc = new XDocument(
                new XElement("AgentConfiguration",
                    new XElement("ATM_ID", config.ATM_ID),
                    new XElement("ATM_Name", config.ATM_Name),
                    new XElement("ATM_Type", config.ATM_Type),
                    new XElement("ServerIP", config.ServerIP),
                    new XElement("ServerPort", config.ServerPort)));
            doc.Save(path);
        }

        var loaded = XDocument.Load(path);
        var record = new AgentConfigurationRecord { ConfigPath = path };
        foreach (var element in loaded.Root?.Elements() ?? Enumerable.Empty<XElement>())
            record.Values[element.Name.LocalName] = element.Value;
        return record;
    }
}

public sealed class OperationalStateStore
{
    private readonly ConcurrentDictionary<string, ATMInfo> _states = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyCollection<ATMInfo> Snapshot => _states.Values.OrderBy(s => s.ATM_ID, StringComparer.OrdinalIgnoreCase).ToArray();
    public void Upsert(ATMInfo atm) => _states[atm.ATM_ID ?? atm.ATMId ?? Guid.NewGuid().ToString("N")] = atm;
    public bool TryGet(string atmId, out ATMInfo? atm) => _states.TryGetValue(atmId, out atm);

    public FleetSummary BuildSummary()
    {
        var snapshot = Snapshot;
        return new FleetSummary
        {
            Total = snapshot.Count,
            Connected = snapshot.Count(a => a.ConnectionStatus is ConnectionStatus.Connected or ConnectionStatus.Syncing or ConnectionStatus.WaitingReply),
            Syncing = snapshot.Count(a => a.ConnectionStatus == ConnectionStatus.Syncing || a.SyncState is SyncStatus.InProgress or SyncStatus.Syncing or SyncStatus.Resyncing),
            Offline = snapshot.Count(a => a.ConnectionStatus == ConnectionStatus.Disconnected || a.Status is ATMStatus.Offline or ATMStatus.CriticalFault),
            AverageHealth = snapshot.Count == 0 ? 0 : (int)Math.Round(snapshot.Average(a => a.HealthScore))
        };
    }
}

public class JournalSyncTrackingService
{
    private readonly ConcurrentDictionary<string, JournalSyncRecord> _records = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyList<JournalSyncRecord> Records => _records.Values.OrderByDescending(r => r.UpdatedAtUtc).ToArray();
    public void AddOrUpdate(JournalSyncRecord record)
    {
        record.UpdatedAtUtc = DateTime.UtcNow;
        _records.AddOrUpdate(record.SyncId, record, (_, _) => record);
    }

    public IReadOnlyList<JournalSyncRecord> GetByAtm(string atmId)
    {
        return _records.Values
            .Where(r => string.Equals(r.ATM_ID, atmId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.UpdatedAtUtc)
            .ToArray();
    }

    public SyncSummary BuildSummary()
    {
        var snapshot = Records;
        return new SyncSummary
        {
            Total = snapshot.Count,
            Pending = snapshot.Count(r => r.State == JournalSyncState.Pending),
            InProgress = snapshot.Count(r => r.State is JournalSyncState.Syncing or JournalSyncState.ReSyncing),
            Completed = snapshot.Count(r => r.State == JournalSyncState.Completed),
            Failed = snapshot.Count(r => r.State == JournalSyncState.Failed),
            AverageProgress = snapshot.Count == 0 ? 0 : (int)Math.Round(snapshot.Average(r => r.ProgressPercent))
        };
    }
}

public sealed class JournalSyncTracker : JournalSyncTrackingService
{
}

public sealed class JournalSyncTrackerService : JournalSyncTrackingService
{
}

public sealed class JournalSyncStateStore
{
    public JournalSyncTrackingService Tracking { get; } = new();
}

public sealed class JournalSyncStateService
{
    private readonly ConcurrentDictionary<string, LegacyJournalState> _legacyStates = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _stateRoot;

    public JournalSyncStateService()
        : this(string.Empty)
    {
    }

    public JournalSyncStateService(string serverStateRoot)
    {
        _stateRoot = serverStateRoot ?? string.Empty;
    }

    public SyncStatus GetStatus(JournalSyncRecord record) => record.State switch
    {
        JournalSyncState.Pending => SyncStatus.Pending,
        JournalSyncState.Syncing => SyncStatus.Syncing,
        JournalSyncState.ReSyncing => SyncStatus.Resyncing,
        JournalSyncState.Completed => SyncStatus.Completed,
        JournalSyncState.Failed => SyncStatus.Failed,
        JournalSyncState.Archived => SyncStatus.Archived,
        _ => SyncStatus.Idle
    };

    public void UpdateConnectivity(string atmId, string atmType, bool connected)
    {
        var state = GetOrCreate(atmId, atmType);
        state.IsConnected = connected;
        state.UpdatedAtUtc = DateTime.UtcNow;
        PersistState(atmId, state);
    }

    public void UpdateHeartbeat(string atmId, string atmType)
    {
        var state = GetOrCreate(atmId, atmType);
        state.HeartbeatUtc = DateTime.UtcNow;
        state.UpdatedAtUtc = DateTime.UtcNow;
        PersistState(atmId, state);
    }

    public void RecordDetected(string atmId, string atmType, string filePath, long fileSize, string checksum)
    {
        var state = GetOrCreate(atmId, atmType);
        state.LastFilePath = filePath ?? string.Empty;
        state.LastChecksum = checksum ?? string.Empty;
        state.LastFileSize = fileSize;
        state.DetectedCount++;
        state.UpdatedAtUtc = DateTime.UtcNow;
        PersistState(atmId, state);
    }

    public void RecordSent(string atmId, string checksum)
    {
        if (_legacyStates.TryGetValue(NormalizeAtmId(atmId), out var state))
        {
            state.LastSentChecksum = checksum ?? string.Empty;
            state.SentCount++;
            state.UpdatedAtUtc = DateTime.UtcNow;
            PersistState(atmId, state);
        }
    }

    public void RecordFailure(string atmId, string atmType, string checksum, string reason)
    {
        var state = GetOrCreate(atmId, atmType);
        state.LastError = reason ?? string.Empty;
        state.LastChecksum = checksum ?? string.Empty;
        state.FailedCount++;
        state.UpdatedAtUtc = DateTime.UtcNow;
        PersistState(atmId, state);
    }

    private LegacyJournalState GetOrCreate(string atmId, string atmType)
    {
        var key = NormalizeAtmId(atmId);
        return _legacyStates.GetOrAdd(key, _ => new LegacyJournalState
        {
            ATM_ID = key,
            ATM_Type = AppConstants.NormalizeATMType(atmType),
            UpdatedAtUtc = DateTime.UtcNow
        });
    }

    private void PersistState(string atmId, LegacyJournalState state)
    {
        if (string.IsNullOrWhiteSpace(_stateRoot))
            return;

        try
        {
            Directory.CreateDirectory(_stateRoot);
            var safeId = NormalizeAtmId(atmId);
            var filePath = Path.Combine(_stateRoot, safeId + ".json");
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
        catch
        {
            // Compatibility persistence is best-effort and must not break sync flow.
        }
    }

    private static string NormalizeAtmId(string atmId)
    {
        return string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim().ToUpperInvariant();
    }

    private sealed class LegacyJournalState
    {
        public string ATM_ID { get; set; } = "UNKNOWN";
        public string ATM_Type { get; set; } = AppConstants.ATM_TYPE_NCR;
        public bool IsConnected { get; set; }
        public DateTime HeartbeatUtc { get; set; }
        public string LastFilePath { get; set; } = string.Empty;
        public long LastFileSize { get; set; }
        public string LastChecksum { get; set; } = string.Empty;
        public string LastSentChecksum { get; set; } = string.Empty;
        public string LastError { get; set; } = string.Empty;
        public int DetectedCount { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}

public interface IJournalSyncService
{
    bool IsRunning { get; }
    void StartSync();
    void StopSync();
}

public sealed class JournalSyncService : IJournalSyncService
{
    public JournalOutbox Outbox { get; } = new();
    public bool IsRunning { get; private set; }
    public event EventHandler<LiveSyncProgress>? ProgressChanged;
    public void StartSync() => IsRunning = true;
    public void StopSync() => IsRunning = false;
    public void Queue(string atmId, string fileName, byte[] data, long offset, string checksum) => Outbox.Enqueue(atmId, fileName, data, offset, checksum);
    public void ReportProgress(LiveSyncProgress progress) => ProgressChanged?.Invoke(this, progress);
}

public sealed class JournalSyncServiceStub : IJournalSyncService
{
    public bool IsRunning { get; private set; }
    public void StartSync() => IsRunning = true;
    public void StopSync() => IsRunning = false;
}

public sealed class JournalSyncMonitorService
{
    public IEnumerable<LiveSyncProgress> GetLiveProgress(IEnumerable<JournalSyncRecord> records)
    {
        return records.Select(r => new LiveSyncProgress { ATM_ID = r.ATM_ID, FileName = r.FileName, BytesSent = r.FileOffset, TotalBytes = r.FileSize, Status = SyncStatus.InProgress });
    }
}

public sealed class JournalSyncDashboardService
{
    public object BuildSummary(IEnumerable<JournalSyncRecord> records)
    {
        var list = records.ToList();
        return new
        {
            Total = list.Count,
            Completed = list.Count(r => r.State == JournalSyncState.Completed),
            Failed = list.Count(r => r.State == JournalSyncState.Failed),
            Pending = list.Count(r => r.State == JournalSyncState.Pending)
        };
    }
}

public sealed class JournalSyncAlertService
{
    private readonly AlertManager _alerts;
    public JournalSyncAlertService(AlertManager alerts) => _alerts = alerts;
    public void Evaluate(JournalSyncRecord record)
    {
        if (record.State == JournalSyncState.Failed)
            _alerts.Raise(AlertSeverity.Warning, "Journal sync failed", record.Message, record.ATM_ID, record.SyncId);
    }
}

public sealed class RoleBasedAccess
{
    private readonly Dictionary<string, HashSet<string>> _permissions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Admin"] = new(StringComparer.OrdinalIgnoreCase) { "*" },
        ["Auditor"] = new(StringComparer.OrdinalIgnoreCase) { "view", "export" },
        ["Support"] = new(StringComparer.OrdinalIgnoreCase) { "view", "remote", "sync" },
        ["Observer"] = new(StringComparer.OrdinalIgnoreCase) { "view" }
    };

    public bool Can(string role, string permission)
    {
        return _permissions.TryGetValue(role, out var permissions) &&
               (permissions.Contains("*") || permissions.Contains(permission));
    }
}

public sealed class XfsLogAnalysisService
{
    public IEnumerable<string> AnalyzeLines(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            if (line.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("FAULT", StringComparison.OrdinalIgnoreCase))
                yield return line;
        }
    }
}

public class VendorRootCapabilityService
{
    public IReadOnlyList<string> GetCapabilities(string vendor) => AppConstants.NormalizeATMType(vendor) switch
    {
        AppConstants.ATM_TYPE_NCR => new[] { "EJDATA", "EJRCPY", "OOXFS", "HOST_MESSAGES" },
        AppConstants.ATM_TYPE_GRG => new[] { "DAILY_EJ", "TRACE", "JOURNAL_ADAPTER" },
        AppConstants.ATM_TYPE_DN => new[] { "MDS", "JOURNAL", "TRACE" },
        _ => new[] { "JOURNAL", "TRACE" }
    };
}

public sealed class VendorRootProfileCatalogService : VendorRootCapabilityService
{
}

public class NcrConfigCapabilityParser
{
    public Dictionary<string, string> Parse(string text)
    {
        return (text ?? string.Empty)
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim(), StringComparer.OrdinalIgnoreCase);
    }
}

public sealed class NcrConfigurationCapabilityParser : NcrConfigCapabilityParser
{
}

public sealed class NcrReferenceCapabilityFactory
{
    public IReadOnlyDictionary<string, string> CreateDefaults() => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Journal"] = AppConstants.NCR_JournalPath,
        ["EJDATA"] = AppConstants.NCR_EJData,
        ["EJRCPY"] = AppConstants.NCR_EJRcpy
    };
}

public sealed class MergedTraceCorrelationService
{
    public IReadOnlyList<string> Correlate(IEnumerable<string> hostMessages, IEnumerable<string> xfsEvents)
    {
        return hostMessages.Concat(xfsEvents).OrderBy(line => line, StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
