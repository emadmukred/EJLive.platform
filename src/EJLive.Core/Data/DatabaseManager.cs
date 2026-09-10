using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Core.Services
{
    public partial class DatabaseManager : IDisposable
        {
            private static DatabaseManager _instance;
    
    
            public static DatabaseManager Instance
            {
                get
                {
                    if (_instance == null)
                        lock (_lock)
                            if (_instance == null)
                                _instance = new DatabaseManager();
                    return _instance;
                }
            }
    
    
            private string _connStr;
    
    
            private bool   _initialized;
    
    
            private readonly string _connectionString;
    
    
            private readonly string _databasePath;
    
    
            private readonly ConcurrentDictionary<string, object> _cache = new();
    
    
            private readonly SemaphoreSlim _writeLock = new(1, 1);
    
    
            private readonly object _migrationLock = new();
    
    
            private int _schemaVersion;
    
    
            private bool _disposed;
    
    
            public int SchemaVersion => _schemaVersion;
    
    
            public string DatabasePath => _databasePath;
    
    
            public DatabaseManager(string? databasePath = null)
            {
                _databasePath = databasePath
                    ?? Environment.GetEnvironmentVariable("EJLIVE_DATABASE_PATH")
                    ?? Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "EJLive", "Server", "ejlive.db");
    
                var dir = Path.GetDirectoryName(_databasePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
    
                _connectionString = $"Data Source={_databasePath};Version=3;Journal Mode=WAL;Pooling=True;Max Pool Size=20;";
            }
    
    
            private static readonly object _lock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Data\DatabaseManager.cs
            private readonly object _queryLock = new object();
    
    
            public void Initialize(string dbPath = null)
            {
                if (_initialized) return;
                dbPath = dbPath ?? AppConstants.DefaultDatabasePath;
                var dir = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
    
                _connStr     = $"Data Source={dbPath};Version=3;Journal Mode=WAL;Cache Size=4000;Synchronous=Normal;";
                _initialized = true;
                CreateSchema();
                AppLogger.Instance.Info($"Database initialized: {dbPath}", "DB");
            }
    
    
                    private void CreateSchema()
                    {
                        // نفّذ كل PRAGMA منفردًا ثم كل CREATE TABLE منفردًا
                        var pragmas = new[]
                        {
                            "PRAGMA journal_mode=WAL",
                            "PRAGMA synchronous=NORMAL",
                            "PRAGMA cache_size=4000",
                            "PRAGMA temp_store=MEMORY"
                        };
                        foreach (var p in pragmas)
                            ExecuteSchema(p);
    
                        ExecuteSchema(@"
            CREATE TABLE IF NOT EXISTS journal_archive (
                entry_id          TEXT PRIMARY KEY,
                atm_id            TEXT NOT NULL,
                file_name         TEXT NOT NULL,
                original_size     INTEGER NOT NULL DEFAULT 0,
                compressed_size   INTEGER NOT NULL DEFAULT 0,
                encrypted_size    INTEGER NOT NULL DEFAULT 0,
                is_encrypted      INTEGER NOT NULL DEFAULT 1,
                is_compressed     INTEGER NOT NULL DEFAULT 1,
                checksum          TEXT,
                md5_hash          TEXT,
                sha256_hash       TEXT,
                transaction_count INTEGER NOT NULL DEFAULT 0,
                archive_path      TEXT,
                month_partition   TEXT,
                received_at       TEXT NOT NULL,
                verified_at       TEXT
            )");
    
                        ExecuteSchema(@"
            CREATE TABLE IF NOT EXISTS sync_records (
                sync_id          TEXT PRIMARY KEY,
                atm_id           TEXT NOT NULL,
                file_name        TEXT NOT NULL,
                file_size        INTEGER NOT NULL DEFAULT 0,
                file_offset      INTEGER NOT NULL DEFAULT 0,
                checksum         TEXT,
                md5_hash         TEXT,
                sha256_hash      TEXT,
                state            INTEGER NOT NULL DEFAULT 0,
                progress_percent INTEGER NOT NULL DEFAULT 0,
                retry_count      INTEGER NOT NULL DEFAULT 0,
                local_path       TEXT,
                server_path      TEXT,
                message          TEXT,
                created_at       TEXT NOT NULL,
                updated_at       TEXT NOT NULL,
                completed_at     TEXT
            )");
    
                        ExecuteSchema("CREATE UNIQUE INDEX IF NOT EXISTS ux_sync_idempotency ON sync_records (atm_id, file_name, checksum)");
    
                        ExecuteSchema(@"
            CREATE TABLE IF NOT EXISTS audit_log (
                log_id        TEXT PRIMARY KEY,
                action        TEXT NOT NULL,
                performed_by  TEXT,
                atm_id        TEXT,
                details       TEXT,
                ip_address    TEXT,
                is_successful INTEGER NOT NULL DEFAULT 1,
                performed_at  TEXT NOT NULL
            )");
    
                        ExecuteSchema(@"
            CREATE TABLE IF NOT EXISTS daily_stats (
                stat_id              TEXT PRIMARY KEY,
                atm_id               TEXT NOT NULL,
                stat_date            TEXT NOT NULL,
                approved_tx          INTEGER NOT NULL DEFAULT 0,
                failed_tx            INTEGER NOT NULL DEFAULT 0,
                cards_captured       INTEGER NOT NULL DEFAULT 0,
                cash_dispensed       REAL    NOT NULL DEFAULT 0,
                journal_bytes        INTEGER NOT NULL DEFAULT 0,
                uptime_percent       REAL    NOT NULL DEFAULT 100.0,
                sync_success_percent REAL    NOT NULL DEFAULT 100.0,
                updated_at           TEXT    NOT NULL
            )");
    
                        ExecuteSchema("CREATE UNIQUE INDEX IF NOT EXISTS ux_daily_stats_atm_date ON daily_stats (atm_id, stat_date)");
                        ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_archive_atm   ON journal_archive (atm_id, received_at DESC)");
                        ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_archive_month ON journal_archive (month_partition, atm_id)");
                        ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_sync_atm_state ON sync_records (atm_id, state)");
                        ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_audit_action   ON audit_log    (action, performed_at DESC)");
                        ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_stats_atm_date ON daily_stats  (atm_id, stat_date DESC)");
                    }
    
    
                    public void InsertArchiveEntry(JournalEntry entry)
                    {
                        ExecuteSingle(@"
            INSERT OR IGNORE INTO journal_archive
            (entry_id, atm_id, file_name, original_size, compressed_size, encrypted_size,
            is_encrypted, is_compressed, checksum, md5_hash, sha256_hash, transaction_count,
            archive_path, month_partition, received_at, verified_at)
            VALUES
            (@id,@atm,@fn,@os,@cs,@es,@enc,@comp,@ck,@md5,@sha,@tc,@ap,@mp,@ra,@va)",
                            P("@id",   entry.EntryId),
                            P("@atm",  entry.ATMId),
                            P("@fn",   entry.FileName),
                            P("@os",   entry.OriginalSize),
                            P("@cs",   entry.CompressedSize),
                            P("@es",   entry.EncryptedSize),
                            P("@enc",  entry.IsEncrypted ? 1 : 0),
                            P("@comp", entry.IsCompressed ? 1 : 0),
                            P("@ck",   entry.Checksum),
                            P("@md5",  entry.MD5Hash),
                            P("@sha",  entry.SHA256Hash),
                            P("@tc",   entry.TransactionCount),
                            P("@ap",   entry.ArchivePath),
                            P("@mp",   entry.MonthPartition),
                            P("@ra",   entry.ReceivedAt.ToString("o")),
                            P("@va",   entry.VerifiedAt == default ? null : (object)entry.VerifiedAt.ToString("o"))
                        );
                    }
    
    
                    public List<JournalEntry> SearchArchive(string atmId, DateTime? from = null, DateTime? to = null,
                                                            string keyword = null, int maxRows = 1000)
                    {
                        var sql = @"
            SELECT entry_id, atm_id, file_name, original_size, compressed_size, encrypted_size,
                    checksum, md5_hash, sha256_hash, transaction_count, archive_path, month_partition, received_at
            FROM journal_archive
            WHERE 1=1";
    
                        var parms = new List<SQLiteParameter>();
                        if (!string.IsNullOrEmpty(atmId)) { sql += " AND atm_id = @atm"; parms.Add(P("@atm", atmId)); }
                        if (from.HasValue)  { sql += " AND received_at >= @from"; parms.Add(P("@from", from.Value.ToString("o"))); }
                        if (to.HasValue)    { sql += " AND received_at <= @to";   parms.Add(P("@to",   to.Value.AddDays(1).ToString("o"))); }
                        if (!string.IsNullOrEmpty(keyword)) { sql += " AND (file_name LIKE @kw OR checksum LIKE @kw)"; parms.Add(P("@kw", $"%{keyword}%")); }
                        sql += $" ORDER BY received_at DESC LIMIT {maxRows}";
    
                        var result = new List<JournalEntry>();
                        Query(sql, r =>
                        {
                            result.Add(new JournalEntry
                            {
                                EntryId          = r["entry_id"]?.ToString(),
                                ATMId            = r["atm_id"]?.ToString(),
                                FileName         = r["file_name"]?.ToString(),
                                OriginalSize     = Convert.ToInt64(r["original_size"]),
                                CompressedSize   = Convert.ToInt64(r["compressed_size"]),
                                EncryptedSize    = Convert.ToInt64(r["encrypted_size"]),
                                Checksum         = r["checksum"]?.ToString(),
                                MD5Hash          = r["md5_hash"]?.ToString(),
                                SHA256Hash       = r["sha256_hash"]?.ToString(),
                                TransactionCount = Convert.ToInt32(r["transaction_count"]),
                                ArchivePath      = r["archive_path"]?.ToString(),
                                MonthPartition   = r["month_partition"]?.ToString(),
                                ReceivedAt       = DateTime.TryParse(r["received_at"]?.ToString(), out var dt) ? dt : DateTime.MinValue
                            });
                        }, parms.ToArray());
                        return result;
                    }
    
    
                    public bool IsDuplicateSync(string atmId, string fileName, string checksum)
                    {
                        if (string.IsNullOrEmpty(checksum)) return false;
                        var count = QueryScalar<int>(@"
            SELECT COUNT(1) FROM sync_records
            WHERE atm_id=@atm AND file_name=@fn AND checksum=@ck AND state=3",
                            P("@atm", atmId), P("@fn", fileName), P("@ck", checksum));
                        return count > 0;
                    }
    
    
                    public void InsertSyncRecord(JournalSyncRecord rec)
                    {
                        ExecuteSingle(@"
            INSERT OR IGNORE INTO sync_records
            (sync_id, atm_id, file_name, file_size, file_offset, checksum, md5_hash, sha256_hash,
            state, progress_percent, retry_count, local_path, message, created_at, updated_at)
            VALUES
            (@id,@atm,@fn,@fs,@fo,@ck,@md5,@sha,@st,@prog,@rc,@lp,@msg,@ca,@ua)",
                            P("@id",   rec.SyncId),    P("@atm",  rec.ATM_ID),    P("@fn",  rec.FileName),
                            P("@fs",   rec.FileSize),  P("@fo",   rec.FileOffset), P("@ck",  rec.Checksum),
                            P("@md5",  rec.MD5Hash),   P("@sha",  rec.SHA256Hash), P("@st",  (int)rec.State),
                            P("@prog", rec.ProgressPercent), P("@rc", rec.RetryCount),  P("@lp",  rec.LocalPath),
                            P("@msg",  rec.Message),   P("@ca",   rec.CreatedAtUtc.ToString("o")),
                            P("@ua",   rec.UpdatedAtUtc.ToString("o"))
                        );
                    }
    
    
                    public void UpdateSyncState(string syncId, JournalSyncState state, int percent)
                    {
                        var completedAt = state == JournalSyncState.Completed ? DateTime.UtcNow.ToString("o") : null;
                        ExecuteSingle(@"
            UPDATE sync_records
            SET state=@st, progress_percent=@prog, updated_at=@ua, completed_at=@ca
            WHERE sync_id=@id",
                            P("@st",   (int)state),
                            P("@prog", percent),
                            P("@ua",   DateTime.UtcNow.ToString("o")),
                            P("@ca",   completedAt),
                            P("@id",   syncId));
                    }
    
    
                    public List<JournalSyncRecord> GetPendingSyncRecords(string atmId)
                    {
                        var result = new List<JournalSyncRecord>();
                        Query(@"
            SELECT sync_id, atm_id, file_name, file_size, file_offset, checksum, md5_hash, sha256_hash,
                    state, progress_percent, retry_count, local_path, message, created_at, updated_at
            FROM sync_records
            WHERE atm_id=@atm AND state IN (0,1,2,4)
            ORDER BY created_at ASC", r =>
                        {
                            result.Add(new JournalSyncRecord
                            {
                                SyncId          = r["sync_id"]?.ToString(),
                                ATM_ID          = r["atm_id"]?.ToString(),
                                FileName        = r["file_name"]?.ToString(),
                                FileSize        = Convert.ToInt64(r["file_size"]),
                                FileOffset      = Convert.ToInt64(r["file_offset"]),
                                Checksum        = r["checksum"]?.ToString(),
                                MD5Hash         = r["md5_hash"]?.ToString(),
                                SHA256Hash      = r["sha256_hash"]?.ToString(),
                                State           = (JournalSyncState)Convert.ToInt32(r["state"]),
                                ProgressPercent = Convert.ToInt32(r["progress_percent"]),
                                RetryCount      = Convert.ToInt32(r["retry_count"]),
                                LocalPath       = r["local_path"]?.ToString(),
                                Message         = r["message"]?.ToString()
                            });
                        }, P("@atm", atmId));
                        return result;
                    }
    
    
                    public void InsertAuditLog(string action, string performedBy, string atmId, string details)
                    {
                        ExecuteSingle(@"
            INSERT INTO audit_log (log_id, action, performed_by, atm_id, details, performed_at)
            VALUES (@id,@act,@by,@atm,@det,@ts)",
                            P("@id",  Guid.NewGuid().ToString("N")),
                            P("@act", action),
                            P("@by",  performedBy),
                            P("@atm", atmId),
                            P("@det", details),
                            P("@ts",  DateTime.UtcNow.ToString("o")));
                    }
    
    
            public DataTable GetAuditLog(string atmId, DateTime? from = null, DateTime? to = null, int maxRows = 1000)
            {
                var sql   = "SELECT log_id, action, performed_by, atm_id, details, performed_at FROM audit_log WHERE 1=1";
                var parms = new List<SQLiteParameter>();
                if (!string.IsNullOrEmpty(atmId)) { sql += " AND atm_id=@atm";   parms.Add(P("@atm",  atmId)); }
                if (from.HasValue) { sql += " AND performed_at>=@from"; parms.Add(P("@from", from.Value.ToString("o"))); }
                if (to.HasValue)   { sql += " AND performed_at<=@to";   parms.Add(P("@to",   to.Value.AddDays(1).ToString("o"))); }
                sql += $" ORDER BY performed_at DESC LIMIT {maxRows}";
                return QueryTable(sql, parms.ToArray());
            }
    
    
                    public void UpdateDailyStats(string atmId, DateTime date,
                        int approvedDelta = 0, int failedDelta = 0, int cardsDelta = 0,
                        long cashDelta = 0, long bytesDelta = 0)
                    {
                        var dateStr = date.ToString("yyyy-MM-dd");
                        ExecuteSingle(@"
            INSERT INTO daily_stats (stat_id, atm_id, stat_date, approved_tx, failed_tx, cards_captured,
                                        cash_dispensed, journal_bytes, updated_at)
            VALUES (@id, @atm, @date, @app, @fail, @cards, @cash, @bytes, @ua)
            ON CONFLICT(atm_id, stat_date) DO UPDATE SET
                approved_tx    = approved_tx    + excluded.approved_tx,
                failed_tx      = failed_tx      + excluded.failed_tx,
                cards_captured = cards_captured + excluded.cards_captured,
                cash_dispensed = cash_dispensed + excluded.cash_dispensed,
                journal_bytes  = journal_bytes  + excluded.journal_bytes,
                updated_at     = excluded.updated_at",
                            P("@id",    Guid.NewGuid().ToString("N")),
                            P("@atm",   atmId),
                            P("@date",  dateStr),
                            P("@app",   approvedDelta),
                            P("@fail",  failedDelta),
                            P("@cards", cardsDelta),
                            P("@cash",  cashDelta),
                            P("@bytes", bytesDelta),
                            P("@ua",    DateTime.UtcNow.ToString("o")));
                    }
    
    
                    public DataTable GetDailyStatsTable(string atmId, string fromDate, string toDate)
                    {
                        return QueryTable(@"
            SELECT stat_date, approved_tx, failed_tx, cards_captured, cash_dispensed, journal_bytes,
                    uptime_percent, sync_success_percent
            FROM daily_stats
            WHERE atm_id=@atm AND stat_date>=@from AND stat_date<=@to
            ORDER BY stat_date DESC",
                            P("@atm", atmId), P("@from", fromDate), P("@to", toDate));
                    }
    
    
            private void ExecuteSchema(string sql)
            {
                if (!_initialized || string.IsNullOrWhiteSpace(sql)) return;
                lock (_queryLock)
                {
                    conn.Open();
                    try { cmd.ExecuteNonQuery(); }
                    catch (SQLiteException ex)
                    {
                        // تجاهل "already exists" و "UNIQUE constraint"
                        if (!ex.Message.Contains("already exists") &&
                            !ex.Message.Contains("UNIQUE") &&
                            !ex.Message.Contains("no such"))
                            AppLogger.Instance.Warning($"Schema warning: {ex.Message}", "DB");
                    }
                }
            }
    
    
            private void ExecuteSingle(string sql, params SQLiteParameter[] parms)
            {
                if (!_initialized) return;
                lock (_queryLock)
                {
                    conn.Open();
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    try { cmd.ExecuteNonQuery(); }
                    catch (SQLiteException ex)
                    {
                        if (!ex.Message.Contains("UNIQUE"))
                        {
                            AppLogger.Instance.Error($"DB Error: {ex.Message}", "DB");
                            throw;
                        }
                    }
                }
            }
    
    
            private void Query(string sql, Action<IDataReader> rowHandler, params SQLiteParameter[] parms)
            {
                if (!_initialized) return;
                lock (_queryLock)
                {
                    conn.Open();
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    while (r.Read()) rowHandler(r);
                }
            }
    
    
            private T QueryScalar<T>(string sql, params SQLiteParameter[] parms)
            {
                if (!_initialized) return default;
                lock (_queryLock)
                {
                    conn.Open();
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    var result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value) return default;
                    return (T)Convert.ChangeType(result, typeof(T));
                }
            }
    
    
            private DataTable QueryTable(string sql, params SQLiteParameter[] parms)
            {
                var dt = new DataTable();
                if (!_initialized) return dt;
                lock (_queryLock)
                {
                    conn.Open();
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    adapter.Fill(dt);
                }
                return dt;
            }
    
    
            private static SQLiteParameter P(string name, object value)
                => new SQLiteParameter(name, value ?? (object)DBNull.Value);
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Data\DatabaseManager.cs
            private void ExecuteSchema(string sql)
            {
                if (!_initialized || string.IsNullOrWhiteSpace(sql)) return;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd = new SQLiteCommand(sql.Trim(), conn);
                    try { cmd.ExecuteNonQuery(); }
                    catch (SQLiteException ex)
                    {
                        // تجاهل "already exists" و "UNIQUE constraint"
                        if (!ex.Message.Contains("already exists") &&
                            !ex.Message.Contains("UNIQUE") &&
                            !ex.Message.Contains("no such"))
                            AppLogger.Instance.Warning($"Schema warning: {ex.Message}", "DB");
                    }
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Data\DatabaseManager.cs
            private void ExecuteSingle(string sql, params SQLiteParameter[] parms)
            {
                if (!_initialized) return;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd  = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    try { cmd.ExecuteNonQuery(); }
                    catch (SQLiteException ex)
                    {
                        if (!ex.Message.Contains("UNIQUE"))
                        {
                            AppLogger.Instance.Error($"DB Error: {ex.Message}", "DB");
                            throw;
                        }
                    }
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Data\DatabaseManager.cs
            private void Query(string sql, Action<IDataReader> rowHandler, params SQLiteParameter[] parms)
            {
                if (!_initialized) return;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd  = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    using var r = cmd.ExecuteReader();
                    while (r.Read()) rowHandler(r);
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Data\DatabaseManager.cs
            private T QueryScalar<T>(string sql, params SQLiteParameter[] parms)
            {
                if (!_initialized) return default;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd  = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    var result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value) return default;
                    return (T)Convert.ChangeType(result, typeof(T));
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Data\DatabaseManager.cs
            private DataTable QueryTable(string sql, params SQLiteParameter[] parms)
            {
                var dt = new DataTable();
                if (!_initialized) return dt;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd     = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    using var adapter = new SQLiteDataAdapter(cmd);
                    adapter.Fill(dt);
                }
                return dt;
            }
    
    
            public async Task InitializeAsync(CancellationToken ct = default)
            {
                await EnsureSchemaVersionTableAsync(ct).ConfigureAwait(false);
                await RunMigrationsAsync(ct).ConfigureAwait(false);
                _schemaVersion = await GetCurrentSchemaVersionAsync(ct).ConfigureAwait(false);
            }
    
    
            private async Task EnsureSchemaVersionTableAsync(CancellationToken ct)
            {
                const string sql = @"
                    CREATE TABLE IF NOT EXISTS SchemaVersion (
                        VersionId INTEGER PRIMARY KEY,
                        VersionName TEXT NOT NULL,
                        Description TEXT,
                        AppliedUtc TEXT NOT NULL DEFAULT (datetime('now')),
                        Status TEXT NOT NULL DEFAULT 'Applied'
                    );";
                await ExecuteNonQueryAsync(sql, ct: ct).ConfigureAwait(false);
            }
    
    
            private async Task RunMigrationsAsync(CancellationToken ct)
            {
                lock (_migrationLock)
                {
                    // Migration 1: Core tables
                    Migrate_V1().GetAwaiter().GetResult();
    
                    // Migration 2: Add health snapshots and parser evidence
                    Migrate_V2().GetAwaiter().GetResult();
    
                    // Migration 3: Add command audit and RBAC
                    Migrate_V3().GetAwaiter().GetResult();
                }
                await Task.CompletedTask;
            }
    
    
            private async Task Migrate_V1()
            {
                var version = await GetCurrentSchemaVersionAsync(CancellationToken.None);
                if (version >= 1) return;
    
                var sql = @"
                    CREATE TABLE IF NOT EXISTS AtmDevice (
                        AtmId TEXT PRIMARY KEY, AtmName TEXT, Vendor TEXT, Model TEXT,
                        TerminalId TEXT, LUNO TEXT, Branch TEXT, Region TEXT, Province TEXT,
                        City TEXT, Geography TEXT, IpAddress TEXT, Port INTEGER DEFAULT 0,
                        OperationalMode TEXT DEFAULT 'Unknown', ConnectionState TEXT DEFAULT 'Offline',
                        JournalSourcePath TEXT, BackupPath TEXT, ImageInboxPath TEXT, ImageDestinationPath TEXT,
                        CreatedUtc TEXT, LastUpdatedUtc TEXT
                    );
                    CREATE TABLE IF NOT EXISTS ClientSession (
                        SessionId TEXT PRIMARY KEY, AtmId TEXT, RemoteAddress TEXT,
                        ClientVersion TEXT, ProtocolVersion TEXT,
                        ConnectedUtc TEXT, LastHeartbeatUtc TEXT, DisconnectedUtc TEXT,
                        PendingCommands INTEGER DEFAULT 0, TotalBytesSent INTEGER DEFAULT 0,
                        TotalBytesReceived INTEGER DEFAULT 0, IsHealthy INTEGER DEFAULT 1,
                        SessionState TEXT DEFAULT 'Active'
                    );
                    CREATE TABLE IF NOT EXISTS FileManifest (
                        ManifestId TEXT PRIMARY KEY, AtmId TEXT, FileName TEXT,
                        FileSize INTEGER DEFAULT 0, Checksum TEXT, FileType TEXT DEFAULT 'Journal',
                        SourcePath TEXT, ArchivePath TEXT, TotalChunks INTEGER DEFAULT 0,
                        ChunkSize INTEGER DEFAULT 0, Status TEXT DEFAULT 'Pending',
                        FailureReason TEXT, RetryCount INTEGER DEFAULT 0,
                        CreatedUtc TEXT, LastModifiedUtc TEXT
                    );
                    CREATE TABLE IF NOT EXISTS CommandQueue (
                        CommandId TEXT PRIMARY KEY, CorrelationId TEXT, CommandType TEXT,
                        TargetAtm TEXT, OperatorId TEXT, ApproverId TEXT,
                        RequiredRole TEXT DEFAULT 'Admin', RiskLevel TEXT DEFAULT 'Low',
                        Payload TEXT, Signature TEXT, Nonce TEXT, State TEXT DEFAULT 'Draft',
                        FailureReason TEXT, RollbackPlan TEXT, RollbackExecuted INTEGER DEFAULT 0,
                        CreatedUtc TEXT, ApprovedUtc TEXT, ExpiryUtc TEXT, CompletedUtc TEXT,
                        RetryCount INTEGER DEFAULT 0, MaxRetries INTEGER DEFAULT 3
                    );
                    INSERT OR IGNORE INTO SchemaVersion (VersionId, VersionName, Description) VALUES (1, 'v1.0', 'Core tables');
                ";
                await ExecuteNonQueryAsync(sql).ConfigureAwait(false);
            }
    
    
            private async Task Migrate_V2()
            {
                var version = await GetCurrentSchemaVersionAsync(CancellationToken.None);
                if (version >= 2) return;
    
                var sql = @"
                    CREATE TABLE IF NOT EXISTS HealthSnapshot (
                        SnapshotId TEXT PRIMARY KEY, AtmId TEXT, SnapshotType TEXT DEFAULT 'Operational',
                        HealthScore REAL, ConfidenceScore REAL, ConnectionState TEXT, OperationalMode TEXT,
                        AgentServiceState TEXT, ActiveAlarms INTEGER DEFAULT 0, Priority TEXT DEFAULT 'None',
                        SnapshotJson TEXT, SnapshotUtc TEXT
                    );
                    CREATE TABLE IF NOT EXISTS ParserRun (
                        RunId TEXT PRIMARY KEY, JournalFileId TEXT, ParserName TEXT, Vendor TEXT,
                        TotalLines INTEGER, ParsedLines INTEGER, TransactionsFound INTEGER,
                        SuccessCount INTEGER, FailedCount INTEGER, SuspiciousCount INTEGER,
                        Status TEXT DEFAULT 'Running', ErrorMessage TEXT,
                        StartedUtc TEXT, CompletedUtc TEXT
                    );
                    CREATE TABLE IF NOT EXISTS ParserEvidence (
                        EvidenceId TEXT PRIMARY KEY, RunId TEXT, TransactionId TEXT,
                        EvidenceType TEXT, EvidenceValue TEXT, Confidence REAL, CapturedUtc TEXT
                    );
                    CREATE TABLE IF NOT EXISTS JournalFile (
                        JournalFileId TEXT PRIMARY KEY, AtmId TEXT, FileName TEXT,
                        FileSize INTEGER, Checksum TEXT, ArchivePath TEXT,
                        TotalLines INTEGER, ParsedLines INTEGER, TransactionCount INTEGER,
                        ParseStatus TEXT DEFAULT 'Pending', ParseError TEXT, ParserUsed TEXT,
                        CapturedUtc TEXT, ParsedUtc TEXT
                    );
                    INSERT OR IGNORE INTO SchemaVersion (VersionId, VersionName, Description) VALUES (2, 'v2.0', 'Health snapshots and parser evidence');
                ";
                await ExecuteNonQueryAsync(sql).ConfigureAwait(false);
            }
    
    
            private async Task Migrate_V3()
            {
                var version = await GetCurrentSchemaVersionAsync(CancellationToken.None);
                if (version >= 3) return;
    
                var sql = @"
                    CREATE TABLE IF NOT EXISTS CommandAudit (
                        AuditId TEXT PRIMARY KEY, CommandId TEXT, Action TEXT, UserId TEXT,
                        UserRole TEXT, TargetAtm TEXT, Details TEXT, Result TEXT, TimestampUtc TEXT
                    );
                    CREATE TABLE IF NOT EXISTS AuditLog (
                        LogId TEXT PRIMARY KEY, Action TEXT, UserId TEXT, UserRole TEXT,
                        TargetAtm TEXT, CommandId TEXT, Details TEXT, Result TEXT,
                        RiskLevel TEXT DEFAULT 'Low', TimestampUtc TEXT
                    );
                    CREATE TABLE IF NOT EXISTS UserEntity (
                        UserId TEXT PRIMARY KEY, Username TEXT, DisplayName TEXT,
                        Role TEXT DEFAULT 'Viewer', IsActive INTEGER DEFAULT 1,
                        CreatedUtc TEXT, LastLoginUtc TEXT, PasswordHash TEXT,
                        AllowedAtmGroups TEXT, AllowedModules TEXT
                    );
                    INSERT OR IGNORE INTO SchemaVersion (VersionId, VersionName, Description) VALUES (3, 'v3.0', 'Command audit and RBAC');
                ";
                await ExecuteNonQueryAsync(sql).ConfigureAwait(false);
            }
    
    
            private async Task<int> GetCurrentSchemaVersionAsync(CancellationToken ct)
            {
                try
                {
                    var result = await ExecuteScalarAsync("SELECT COALESCE(MAX(VersionId), 0) FROM SchemaVersion", ct: ct)
                        .ConfigureAwait(false);
                    return Convert.ToInt32(result);
                }
                catch
                {
                    return 0;
                }
            }
    
    
            public async Task<int> ExecuteNonQueryAsync(string sql,
                Dictionary<string, object?>? parameters = null,
                CancellationToken ct = default)
            {
                await _writeLock.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    using var conn = new SQLiteConnection(_connectionString);
                    await conn.OpenAsync(ct).ConfigureAwait(false);
                    using var cmd = new SQLiteCommand(sql, conn);
                    ApplyParameters(cmd, parameters);
                    return await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }
                finally { _writeLock.Release(); }
            }
    
    
            public async Task<object?> ExecuteScalarAsync(string sql,
                Dictionary<string, object?>? parameters = null,
                CancellationToken ct = default)
            {
                using var conn = new SQLiteConnection(_connectionString);
                await conn.OpenAsync(ct).ConfigureAwait(false);
                using var cmd = new SQLiteCommand(sql, conn);
                ApplyParameters(cmd, parameters);
                return await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            }
    
    
            public async Task<List<Dictionary<string, object?>>> QueryAsync(string sql,
                Dictionary<string, object?>? parameters = null,
                CancellationToken ct = default)
            {
                var results = new List<Dictionary<string, object?>>();
                using var conn = new SQLiteConnection(_connectionString);
                await conn.OpenAsync(ct).ConfigureAwait(false);
                using var cmd = new SQLiteCommand(sql, conn);
                ApplyParameters(cmd, parameters);
                using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
    
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    results.Add(row);
                }
                return results;
            }
    
    
            public async Task ExecuteTransactionAsync(List<(string Sql, Dictionary<string, object?>? Parameters)> statements,
                CancellationToken ct = default)
            {
                await _writeLock.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    using var conn = new SQLiteConnection(_connectionString);
                    await conn.OpenAsync(ct).ConfigureAwait(false);
                    using var transaction = conn.BeginTransaction();
                    try
                    {
                        foreach (var (sql, parameters) in statements)
                        {
                            using var cmd = new SQLiteCommand(sql, conn);
                            ApplyParameters(cmd, parameters);
                            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
                finally { _writeLock.Release(); }
            }
    
    
            public async Task UpsertAtmAsync(AtmDeviceEntity atm, CancellationToken ct = default)
            {
                var sql = @"
                    INSERT OR REPLACE INTO AtmDevice (AtmId, AtmName, Vendor, Model, TerminalId, LUNO, Branch, Region, Province, City, Geography, IpAddress, Port, OperationalMode, ConnectionState, JournalSourcePath, BackupPath, ImageInboxPath, ImageDestinationPath, LastUpdatedUtc)
                    VALUES (@AtmId, @AtmName, @Vendor, @Model, @TerminalId, @LUNO, @Branch, @Region, @Province, @City, @Geography, @IpAddress, @Port, @OperationalMode, @ConnectionState, @JournalSourcePath, @BackupPath, @ImageInboxPath, @ImageDestinationPath, @LastUpdatedUtc);
                ";
                await ExecuteNonQueryAsync(sql, new Dictionary<string, object?>
                {
                    ["@AtmId"] = atm.AtmId, ["@AtmName"] = atm.AtmName, ["@Vendor"] = atm.Vendor,
                    ["@Model"] = atm.Model, ["@TerminalId"] = atm.TerminalId, ["@LUNO"] = atm.LUNO,
                    ["@Branch"] = atm.Branch, ["@Region"] = atm.Region, ["@Province"] = atm.Province,
                    ["@City"] = atm.City, ["@Geography"] = atm.Geography, ["@IpAddress"] = atm.IpAddress,
                    ["@Port"] = atm.Port, ["@OperationalMode"] = atm.OperationalMode,
                    ["@ConnectionState"] = atm.ConnectionState, ["@JournalSourcePath"] = atm.JournalSourcePath,
                    ["@BackupPath"] = atm.BackupPath, ["@ImageInboxPath"] = atm.ImageInboxPath,
                    ["@ImageDestinationPath"] = atm.ImageDestinationPath,
                    ["@LastUpdatedUtc"] = DateTime.UtcNow.ToString("O")
                }, ct).ConfigureAwait(false);
            }
    
    
            public async Task<List<AtmDeviceEntity>> GetAllAtmsAsync(CancellationToken ct = default)
            {
                var rows = await QueryAsync("SELECT * FROM AtmDevice ORDER BY AtmId", ct: ct).ConfigureAwait(false);
                return rows.Select(MapAtmDevice).ToList();
            }
    
    
            public async Task UpsertSessionAsync(ClientSessionEntity session, CancellationToken ct = default)
            {
                var sql = @"
                    INSERT OR REPLACE INTO ClientSession (SessionId, AtmId, RemoteAddress, ClientVersion, ProtocolVersion, ConnectedUtc, LastHeartbeatUtc, DisconnectedUtc, PendingCommands, TotalBytesSent, TotalBytesReceived, IsHealthy, SessionState)
                    VALUES (@SessionId, @AtmId, @RemoteAddress, @ClientVersion, @ProtocolVersion, @ConnectedUtc, @LastHeartbeatUtc, @DisconnectedUtc, @PendingCommands, @TotalBytesSent, @TotalBytesReceived, @IsHealthy, @SessionState);
                ";
                await ExecuteNonQueryAsync(sql, new Dictionary<string, object?>
                {
                    ["@SessionId"] = session.SessionId, ["@AtmId"] = session.AtmId,
                    ["@RemoteAddress"] = session.RemoteAddress, ["@ClientVersion"] = session.ClientVersion,
                    ["@ProtocolVersion"] = session.ProtocolVersion,
                    ["@ConnectedUtc"] = session.ConnectedUtc.ToString("O"),
                    ["@LastHeartbeatUtc"] = session.LastHeartbeatUtc.ToString("O"),
                    ["@DisconnectedUtc"] = session.DisconnectedUtc?.ToString("O"),
                    ["@PendingCommands"] = session.PendingCommands,
                    ["@TotalBytesSent"] = session.TotalBytesSent,
                    ["@TotalBytesReceived"] = session.TotalBytesReceived,
                    ["@IsHealthy"] = session.IsHealthy ? 1 : 0,
                    ["@SessionState"] = session.SessionState
                }, ct).ConfigureAwait(false);
            }
    
    
            public async Task SaveHealthSnapshotAsync(HealthSnapshotEntity snapshot, CancellationToken ct = default)
            {
                var sql = @"
                    INSERT INTO HealthSnapshot (SnapshotId, AtmId, SnapshotType, HealthScore, ConfidenceScore, ConnectionState, OperationalMode, AgentServiceState, ActiveAlarms, Priority, SnapshotJson, SnapshotUtc)
                    VALUES (@SnapshotId, @AtmId, @SnapshotType, @HealthScore, @ConfidenceScore, @ConnectionState, @OperationalMode, @AgentServiceState, @ActiveAlarms, @Priority, @SnapshotJson, @SnapshotUtc);
                ";
                await ExecuteNonQueryAsync(sql, new Dictionary<string, object?>
                {
                    ["@SnapshotId"] = snapshot.SnapshotId,
                    ["@AtmId"] = snapshot.AtmId,
                    ["@SnapshotType"] = snapshot.SnapshotType,
                    ["@HealthScore"] = snapshot.HealthScore,
                    ["@ConfidenceScore"] = snapshot.ConfidenceScore,
                    ["@ConnectionState"] = snapshot.ConnectionState,
                    ["@OperationalMode"] = snapshot.OperationalMode,
                    ["@AgentServiceState"] = snapshot.AgentServiceState,
                    ["@ActiveAlarms"] = snapshot.ActiveAlarms,
                    ["@Priority"] = snapshot.Priority,
                    ["@SnapshotJson"] = snapshot.SnapshotJson,
                    ["@SnapshotUtc"] = snapshot.SnapshotUtc.ToString("O")
                }, ct).ConfigureAwait(false);
            }
    
    
            public async Task<List<HealthSnapshotEntity>> GetHealthSnapshotsAsync(string atmId, int limit = 100, CancellationToken ct = default)
            {
                var rows = await QueryAsync(
                    "SELECT * FROM HealthSnapshot WHERE AtmId = @AtmId ORDER BY SnapshotUtc DESC LIMIT @Limit",
                    new Dictionary<string, object?> { ["@AtmId"] = atmId, ["@Limit"] = limit }, ct).ConfigureAwait(false);
                return rows.Select(MapHealthSnapshot).ToList();
            }
    
    
            public async Task VacuumAsync(CancellationToken ct = default)
            {
                await ExecuteNonQueryAsync("VACUUM;", ct: ct).ConfigureAwait(false);
            }
    
    
            public long GetDatabaseSize()
            {
                if (File.Exists(_databasePath))
                    return new FileInfo(_databasePath).Length;
                return 0;
            }
    
    
            public async Task<Dictionary<string, long>> GetTableRowCountsAsync(CancellationToken ct = default)
            {
                var tables = new[] { "AtmDevice", "ClientSession", "FileManifest", "CommandQueue", "CommandAudit", "AuditLog", "HealthSnapshot", "ParserRun", "JournalFile", "UserEntity" };
                var counts = new Dictionary<string, long>();
                foreach (var table in tables)
                {
                    try
                    {
                        var result = await ExecuteScalarAsync($"SELECT COUNT(*) FROM {table}", ct: ct).ConfigureAwait(false);
                        counts[table] = Convert.ToInt64(result);
                    }
                    catch { counts[table] = 0; }
                }
                return counts;
            }
    
    
            private static void ApplyParameters(SQLiteCommand cmd, Dictionary<string, object?>? parameters)
            {
                if (parameters == null) return;
                foreach (var (key, value) in parameters)
                {
                    cmd.Parameters.AddWithValue(key, value ?? DBNull.Value);
                }
            }
    
    
            private static AtmDeviceEntity MapAtmDevice(Dictionary<string, object?> row)
            {
                return new AtmDeviceEntity
                {
                    AtmId = row["AtmId"]?.ToString() ?? "",
                    AtmName = row["AtmName"]?.ToString() ?? "",
                    Vendor = row["Vendor"]?.ToString() ?? "",
                    Model = row["Model"]?.ToString() ?? "",
                    TerminalId = row["TerminalId"]?.ToString() ?? "",
                    LUNO = row["LUNO"]?.ToString() ?? "",
                    Branch = row["Branch"]?.ToString() ?? "",
                    Region = row["Region"]?.ToString() ?? "",
                    Province = row["Province"]?.ToString() ?? "",
                    City = row["City"]?.ToString() ?? "",
                    Geography = row["Geography"]?.ToString() ?? "",
                    IpAddress = row["IpAddress"]?.ToString() ?? "",
                    Port = Convert.ToInt32(row["Port"] ?? 0),
                    OperationalMode = row["OperationalMode"]?.ToString() ?? "Unknown",
                    ConnectionState = row["ConnectionState"]?.ToString() ?? "Offline",
                    JournalSourcePath = row["JournalSourcePath"]?.ToString() ?? "",
                    BackupPath = row["BackupPath"]?.ToString() ?? "",
                    ImageInboxPath = row["ImageInboxPath"]?.ToString() ?? "",
                    ImageDestinationPath = row["ImageDestinationPath"]?.ToString() ?? ""
                };
            }
    
    
            private static HealthSnapshotEntity MapHealthSnapshot(Dictionary<string, object?> row)
            {
                return new HealthSnapshotEntity
                {
                    SnapshotId = row["SnapshotId"]?.ToString() ?? "",
                    AtmId = row["AtmId"]?.ToString() ?? "",
                    SnapshotType = row["SnapshotType"]?.ToString() ?? "Operational",
                    HealthScore = Convert.ToDouble(row["HealthScore"] ?? 0),
                    ConfidenceScore = Convert.ToDouble(row["ConfidenceScore"] ?? 0),
                    ConnectionState = row["ConnectionState"]?.ToString() ?? "Unknown",
                    OperationalMode = row["OperationalMode"]?.ToString() ?? "Unknown",
                    AgentServiceState = row["AgentServiceState"]?.ToString() ?? "Unknown",
                    ActiveAlarms = Convert.ToInt32(row["ActiveAlarms"] ?? 0),
                    Priority = row["Priority"]?.ToString() ?? "None",
                    SnapshotJson = row["SnapshotJson"]?.ToString()
                };
            }
    
    
            public void Dispose()
            {
                if (!_disposed)
                {
                    _writeLock.Dispose();
                    _cache.Clear();
                    _disposed = true;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v20_bak
            private readonly object _queryLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v17_bak
            private readonly object _queryLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v17_bak
            private void ExecuteSchema(string sql)
            {
                if (!_initialized || string.IsNullOrWhiteSpace(sql)) return;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd = new SQLiteCommand(sql.Trim(), conn);
                    try { cmd.ExecuteNonQuery(); }
                    catch (SQLiteException ex)
                    {
                        // تجاهل "already exists" و "UNIQUE constraint"
                        if (!ex.Message.Contains("already exists") &&
                            !ex.Message.Contains("UNIQUE") &&
                            !ex.Message.Contains("no such"))
                            AppLogger.Instance.Warning($"Schema warning: {ex.Message}", "DB");
                    }
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v17_bak
            private void ExecuteSingle(string sql, params SQLiteParameter[] parms)
            {
                if (!_initialized) return;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd  = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    try { cmd.ExecuteNonQuery(); }
                    catch (SQLiteException ex)
                    {
                        if (!ex.Message.Contains("UNIQUE"))
                        {
                            AppLogger.Instance.Error($"DB Error: {ex.Message}", "DB");
                            throw;
                        }
                    }
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v17_bak
            private void Query(string sql, Action<IDataReader> rowHandler, params SQLiteParameter[] parms)
            {
                if (!_initialized) return;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd  = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    using var r = cmd.ExecuteReader();
                    while (r.Read()) rowHandler(r);
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v17_bak
            private T QueryScalar<T>(string sql, params SQLiteParameter[] parms)
            {
                if (!_initialized) return default;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd  = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    var result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value) return default;
                    return (T)Convert.ChangeType(result, typeof(T));
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v17_bak
            private DataTable QueryTable(string sql, params SQLiteParameter[] parms)
            {
                var dt = new DataTable();
                if (!_initialized) return dt;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd     = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    using var adapter = new SQLiteDataAdapter(cmd);
                    adapter.Fill(dt);
                }
                return dt;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v16_bak
            private readonly object _queryLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v16_bak
            private void ExecuteSchema(string sql)
            {
                if (!_initialized || string.IsNullOrWhiteSpace(sql)) return;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd = new SQLiteCommand(sql.Trim(), conn);
                    try { cmd.ExecuteNonQuery(); }
                    catch (SQLiteException ex)
                    {
                        // تجاهل "already exists" و "UNIQUE constraint"
                        if (!ex.Message.Contains("already exists") &&
                            !ex.Message.Contains("UNIQUE") &&
                            !ex.Message.Contains("no such"))
                            AppLogger.Instance.Warning($"Schema warning: {ex.Message}", "DB");
                    }
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v16_bak
            private void ExecuteSingle(string sql, params SQLiteParameter[] parms)
            {
                if (!_initialized) return;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd  = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    try { cmd.ExecuteNonQuery(); }
                    catch (SQLiteException ex)
                    {
                        if (!ex.Message.Contains("UNIQUE"))
                        {
                            AppLogger.Instance.Error($"DB Error: {ex.Message}", "DB");
                            throw;
                        }
                    }
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v16_bak
            private void Query(string sql, Action<IDataReader> rowHandler, params SQLiteParameter[] parms)
            {
                if (!_initialized) return;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd  = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    using var r = cmd.ExecuteReader();
                    while (r.Read()) rowHandler(r);
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v16_bak
            private T QueryScalar<T>(string sql, params SQLiteParameter[] parms)
            {
                if (!_initialized) return default;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd  = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    var result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value) return default;
                    return (T)Convert.ChangeType(result, typeof(T));
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v16_bak
            private DataTable QueryTable(string sql, params SQLiteParameter[] parms)
            {
                var dt = new DataTable();
                if (!_initialized) return dt;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd     = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    using var adapter = new SQLiteDataAdapter(cmd);
                    adapter.Fill(dt);
                }
                return dt;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v15_bak
            private readonly object _queryLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v15_bak
            private void ExecuteSchema(string sql)
            {
                if (!_initialized || string.IsNullOrWhiteSpace(sql)) return;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd = new SQLiteCommand(sql.Trim(), conn);
                    try { cmd.ExecuteNonQuery(); }
                    catch (SQLiteException ex)
                    {
                        // تجاهل "already exists" و "UNIQUE constraint"
                        if (!ex.Message.Contains("already exists") &&
                            !ex.Message.Contains("UNIQUE") &&
                            !ex.Message.Contains("no such"))
                            AppLogger.Instance.Warning($"Schema warning: {ex.Message}", "DB");
                    }
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v15_bak
            private void ExecuteSingle(string sql, params SQLiteParameter[] parms)
            {
                if (!_initialized) return;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd  = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    try { cmd.ExecuteNonQuery(); }
                    catch (SQLiteException ex)
                    {
                        if (!ex.Message.Contains("UNIQUE"))
                        {
                            AppLogger.Instance.Error($"DB Error: {ex.Message}", "DB");
                            throw;
                        }
                    }
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v15_bak
            private void Query(string sql, Action<IDataReader> rowHandler, params SQLiteParameter[] parms)
            {
                if (!_initialized) return;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd  = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    using var r = cmd.ExecuteReader();
                    while (r.Read()) rowHandler(r);
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v15_bak
            private T QueryScalar<T>(string sql, params SQLiteParameter[] parms)
            {
                if (!_initialized) return default;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd  = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    var result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value) return default;
                    return (T)Convert.ChangeType(result, typeof(T));
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v15_bak
            private DataTable QueryTable(string sql, params SQLiteParameter[] parms)
            {
                var dt = new DataTable();
                if (!_initialized) return dt;
                lock (_queryLock)
                {
                    using var conn = new SQLiteConnection(_connStr);
                    conn.Open();
                    using var cmd     = new SQLiteCommand(sql, conn);
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    using var adapter = new SQLiteDataAdapter(cmd);
                    adapter.Fill(dt);
                }
                return dt;
            }
    
    
        }
    // Class: DatabaseManager (from 5 sources)
        public sealed partial class DatabaseManager : IDisposable
        {
            // --- Constants & Fields ---
                    private static DatabaseManager _instance;
    
                    public static DatabaseManager Instance
                    {
                        get
                        {
                            if (_instance == null)
                                lock (_lock)
                                    if (_instance == null)
                                        _instance = new DatabaseManager();
                            return _instance;
                        }
                    }
    
                    private string _connStr;
    
                    private bool   _initialized;
    
                    private readonly string _connectionString;
    
                    private readonly string _databasePath;
    
                    private readonly ConcurrentDictionary<string, object> _cache = new();
    
                    private readonly SemaphoreSlim _writeLock = new(1, 1);
    
                    private readonly object _migrationLock = new();
    
                    private int _schemaVersion;
    
                    private bool _disposed;
    
    
            // --- Properties ---
                    public int SchemaVersion => _schemaVersion;
    
                    public string DatabasePath => _databasePath;
    
    
            // --- Constructors ---
                    public DatabaseManager(string? databasePath = null)
                    {
                        _databasePath = databasePath
                            ?? Environment.GetEnvironmentVariable("EJLIVE_DATABASE_PATH")
                            ?? Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                "EJLive", "Server", "ejlive.db");
    
                        var dir = Path.GetDirectoryName(_databasePath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
    
                        _connectionString = $"Data Source={_databasePath};Version=3;Journal Mode=WAL;Pooling=True;Max Pool Size=20;";
                    }
    
    
            // --- Methods ---
                    private static readonly object _lock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v20_bak
                    private readonly object _queryLock = new object();
    
                    public void Initialize(string dbPath = null)
                    {
                        if (_initialized) return;
                        dbPath = dbPath ?? AppConstants.DefaultDatabasePath;
                        var dir = Path.GetDirectoryName(dbPath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
    
                        _connStr     = $"Data Source={dbPath};Version=3;Journal Mode=WAL;Cache Size=4000;Synchronous=Normal;";
                        _initialized = true;
                        CreateSchema();
                        AppLogger.Instance.Info($"Database initialized: {dbPath}", "DB");
                    }
    
                    private void CreateSchema()
                    {
                        // نفّذ كل PRAGMA منفردًا ثم كل CREATE TABLE منفردًا
                        var pragmas = new[]
                        {
                            "PRAGMA journal_mode=WAL",
                            "PRAGMA synchronous=NORMAL",
                            "PRAGMA cache_size=4000",
                            "PRAGMA temp_store=MEMORY"
                        };
                        foreach (var p in pragmas)
                            ExecuteSchema(p);
    
                        ExecuteSchema(@"
            CREATE TABLE IF NOT EXISTS journal_archive (
                entry_id          TEXT PRIMARY KEY,
                atm_id            TEXT NOT NULL,
                file_name         TEXT NOT NULL,
                original_size     INTEGER NOT NULL DEFAULT 0,
                compressed_size   INTEGER NOT NULL DEFAULT 0,
                encrypted_size    INTEGER NOT NULL DEFAULT 0,
                is_encrypted      INTEGER NOT NULL DEFAULT 1,
                is_compressed     INTEGER NOT NULL DEFAULT 1,
                checksum          TEXT,
                md5_hash          TEXT,
                sha256_hash       TEXT,
                transaction_count INTEGER NOT NULL DEFAULT 0,
                archive_path      TEXT,
                month_partition   TEXT,
                received_at       TEXT NOT NULL,
                verified_at       TEXT
            )");
    
                        ExecuteSchema(@"
            CREATE TABLE IF NOT EXISTS sync_records (
                sync_id          TEXT PRIMARY KEY,
                atm_id           TEXT NOT NULL,
                file_name        TEXT NOT NULL,
                file_size        INTEGER NOT NULL DEFAULT 0,
                file_offset      INTEGER NOT NULL DEFAULT 0,
                checksum         TEXT,
                md5_hash         TEXT,
                sha256_hash      TEXT,
                state            INTEGER NOT NULL DEFAULT 0,
                progress_percent INTEGER NOT NULL DEFAULT 0,
                retry_count      INTEGER NOT NULL DEFAULT 0,
                local_path       TEXT,
                server_path      TEXT,
                message          TEXT,
                created_at       TEXT NOT NULL,
                updated_at       TEXT NOT NULL,
                completed_at     TEXT
            )");
    
                        ExecuteSchema("CREATE UNIQUE INDEX IF NOT EXISTS ux_sync_idempotency ON sync_records (atm_id, file_name, checksum)");
    
                        ExecuteSchema(@"
            CREATE TABLE IF NOT EXISTS audit_log (
                log_id        TEXT PRIMARY KEY,
                action        TEXT NOT NULL,
                performed_by  TEXT,
                atm_id        TEXT,
                details       TEXT,
                ip_address    TEXT,
                is_successful INTEGER NOT NULL DEFAULT 1,
                performed_at  TEXT NOT NULL
            )");
    
                        ExecuteSchema(@"
            CREATE TABLE IF NOT EXISTS daily_stats (
                stat_id              TEXT PRIMARY KEY,
                atm_id               TEXT NOT NULL,
                stat_date            TEXT NOT NULL,
                approved_tx          INTEGER NOT NULL DEFAULT 0,
                failed_tx            INTEGER NOT NULL DEFAULT 0,
                cards_captured       INTEGER NOT NULL DEFAULT 0,
                cash_dispensed       REAL    NOT NULL DEFAULT 0,
                journal_bytes        INTEGER NOT NULL DEFAULT 0,
                uptime_percent       REAL    NOT NULL DEFAULT 100.0,
                sync_success_percent REAL    NOT NULL DEFAULT 100.0,
                updated_at           TEXT    NOT NULL
            )");
    
                        ExecuteSchema("CREATE UNIQUE INDEX IF NOT EXISTS ux_daily_stats_atm_date ON daily_stats (atm_id, stat_date)");
                        ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_archive_atm   ON journal_archive (atm_id, received_at DESC)");
                        ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_archive_month ON journal_archive (month_partition, atm_id)");
                        ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_sync_atm_state ON sync_records (atm_id, state)");
                        ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_audit_action   ON audit_log    (action, performed_at DESC)");
                        ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_stats_atm_date ON daily_stats  (atm_id, stat_date DESC)");
                    }
    
                    public void InsertArchiveEntry(JournalEntry entry)
                    {
                        ExecuteSingle(@"
            INSERT OR IGNORE INTO journal_archive
            (entry_id, atm_id, file_name, original_size, compressed_size, encrypted_size,
            is_encrypted, is_compressed, checksum, md5_hash, sha256_hash, transaction_count,
            archive_path, month_partition, received_at, verified_at)
            VALUES
            (@id,@atm,@fn,@os,@cs,@es,@enc,@comp,@ck,@md5,@sha,@tc,@ap,@mp,@ra,@va)",
                            P("@id",   entry.EntryId),
                            P("@atm",  entry.ATMId),
                            P("@fn",   entry.FileName),
                            P("@os",   entry.OriginalSize),
                            P("@cs",   entry.CompressedSize),
                            P("@es",   entry.EncryptedSize),
                            P("@enc",  entry.IsEncrypted ? 1 : 0),
                            P("@comp", entry.IsCompressed ? 1 : 0),
                            P("@ck",   entry.Checksum),
                            P("@md5",  entry.MD5Hash),
                            P("@sha",  entry.SHA256Hash),
                            P("@tc",   entry.TransactionCount),
                            P("@ap",   entry.ArchivePath),
                            P("@mp",   entry.MonthPartition),
                            P("@ra",   entry.ReceivedAt.ToString("o")),
                            P("@va",   entry.VerifiedAt == default ? null : (object)entry.VerifiedAt.ToString("o"))
                        );
                    }
    
                    public List<JournalEntry> SearchArchive(string atmId, DateTime? from = null, DateTime? to = null,
                                                            string keyword = null, int maxRows = 1000)
                    {
                        var sql = @"
            SELECT entry_id, atm_id, file_name, original_size, compressed_size, encrypted_size,
                    checksum, md5_hash, sha256_hash, transaction_count, archive_path, month_partition, received_at
            FROM journal_archive
            WHERE 1=1";
    
                        var parms = new List<SQLiteParameter>();
                        if (!string.IsNullOrEmpty(atmId)) { sql += " AND atm_id = @atm"; parms.Add(P("@atm", atmId)); }
                        if (from.HasValue)  { sql += " AND received_at >= @from"; parms.Add(P("@from", from.Value.ToString("o"))); }
                        if (to.HasValue)    { sql += " AND received_at <= @to";   parms.Add(P("@to",   to.Value.AddDays(1).ToString("o"))); }
                        if (!string.IsNullOrEmpty(keyword)) { sql += " AND (file_name LIKE @kw OR checksum LIKE @kw)"; parms.Add(P("@kw", $"%{keyword}%")); }
                        sql += $" ORDER BY received_at DESC LIMIT {maxRows}";
    
                        var result = new List<JournalEntry>();
                        Query(sql, r =>
                        {
                            result.Add(new JournalEntry
                            {
                                EntryId          = r["entry_id"]?.ToString(),
                                ATMId            = r["atm_id"]?.ToString(),
                                FileName         = r["file_name"]?.ToString(),
                                OriginalSize     = Convert.ToInt64(r["original_size"]),
                                CompressedSize   = Convert.ToInt64(r["compressed_size"]),
                                EncryptedSize    = Convert.ToInt64(r["encrypted_size"]),
                                Checksum         = r["checksum"]?.ToString(),
                                MD5Hash          = r["md5_hash"]?.ToString(),
                                SHA256Hash       = r["sha256_hash"]?.ToString(),
                                TransactionCount = Convert.ToInt32(r["transaction_count"]),
                                ArchivePath      = r["archive_path"]?.ToString(),
                                MonthPartition   = r["month_partition"]?.ToString(),
                                ReceivedAt       = DateTime.TryParse(r["received_at"]?.ToString(), out var dt) ? dt : DateTime.MinValue
                            });
                        }, parms.ToArray());
                        return result;
                    }
    
                    public bool IsDuplicateSync(string atmId, string fileName, string checksum)
                    {
                        if (string.IsNullOrEmpty(checksum)) return false;
                        var count = QueryScalar<int>(@"
            SELECT COUNT(1) FROM sync_records
            WHERE atm_id=@atm AND file_name=@fn AND checksum=@ck AND state=3",
                            P("@atm", atmId), P("@fn", fileName), P("@ck", checksum));
                        return count > 0;
                    }
    
                    public void InsertSyncRecord(JournalSyncRecord rec)
                    {
                        ExecuteSingle(@"
            INSERT OR IGNORE INTO sync_records
            (sync_id, atm_id, file_name, file_size, file_offset, checksum, md5_hash, sha256_hash,
            state, progress_percent, retry_count, local_path, message, created_at, updated_at)
            VALUES
            (@id,@atm,@fn,@fs,@fo,@ck,@md5,@sha,@st,@prog,@rc,@lp,@msg,@ca,@ua)",
                            P("@id",   rec.SyncId),    P("@atm",  rec.ATM_ID),    P("@fn",  rec.FileName),
                            P("@fs",   rec.FileSize),  P("@fo",   rec.FileOffset), P("@ck",  rec.Checksum),
                            P("@md5",  rec.MD5Hash),   P("@sha",  rec.SHA256Hash), P("@st",  (int)rec.State),
                            P("@prog", rec.ProgressPercent), P("@rc", rec.RetryCount),  P("@lp",  rec.LocalPath),
                            P("@msg",  rec.Message),   P("@ca",   rec.CreatedAtUtc.ToString("o")),
                            P("@ua",   rec.UpdatedAtUtc.ToString("o"))
                        );
                    }
    
                    public void UpdateSyncState(string syncId, JournalSyncState state, int percent)
                    {
                        var completedAt = state == JournalSyncState.Completed ? DateTime.UtcNow.ToString("o") : null;
                        ExecuteSingle(@"
            UPDATE sync_records
            SET state=@st, progress_percent=@prog, updated_at=@ua, completed_at=@ca
            WHERE sync_id=@id",
                            P("@st",   (int)state),
                            P("@prog", percent),
                            P("@ua",   DateTime.UtcNow.ToString("o")),
                            P("@ca",   completedAt),
                            P("@id",   syncId));
                    }
    
                    public List<JournalSyncRecord> GetPendingSyncRecords(string atmId)
                    {
                        var result = new List<JournalSyncRecord>();
                        Query(@"
            SELECT sync_id, atm_id, file_name, file_size, file_offset, checksum, md5_hash, sha256_hash,
                    state, progress_percent, retry_count, local_path, message, created_at, updated_at
            FROM sync_records
            WHERE atm_id=@atm AND state IN (0,1,2,4)
            ORDER BY created_at ASC", r =>
                        {
                            result.Add(new JournalSyncRecord
                            {
                                SyncId          = r["sync_id"]?.ToString(),
                                ATM_ID          = r["atm_id"]?.ToString(),
                                FileName        = r["file_name"]?.ToString(),
                                FileSize        = Convert.ToInt64(r["file_size"]),
                                FileOffset      = Convert.ToInt64(r["file_offset"]),
                                Checksum        = r["checksum"]?.ToString(),
                                MD5Hash         = r["md5_hash"]?.ToString(),
                                SHA256Hash      = r["sha256_hash"]?.ToString(),
                                State           = (JournalSyncState)Convert.ToInt32(r["state"]),
                                ProgressPercent = Convert.ToInt32(r["progress_percent"]),
                                RetryCount      = Convert.ToInt32(r["retry_count"]),
                                LocalPath       = r["local_path"]?.ToString(),
                                Message         = r["message"]?.ToString()
                            });
                        }, P("@atm", atmId));
                        return result;
                    }
    
                    public void InsertAuditLog(string action, string performedBy, string atmId, string details)
                    {
                        ExecuteSingle(@"
            INSERT INTO audit_log (log_id, action, performed_by, atm_id, details, performed_at)
            VALUES (@id,@act,@by,@atm,@det,@ts)",
                            P("@id",  Guid.NewGuid().ToString("N")),
                            P("@act", action),
                            P("@by",  performedBy),
                            P("@atm", atmId),
                            P("@det", details),
                            P("@ts",  DateTime.UtcNow.ToString("o")));
                    }
    
                    public DataTable GetAuditLog(string atmId, DateTime? from = null, DateTime? to = null, int maxRows = 1000)
                    {
                        var sql   = "SELECT log_id, action, performed_by, atm_id, details, performed_at FROM audit_log WHERE 1=1";
                        var parms = new List<SQLiteParameter>();
                        if (!string.IsNullOrEmpty(atmId)) { sql += " AND atm_id=@atm";   parms.Add(P("@atm",  atmId)); }
                        if (from.HasValue) { sql += " AND performed_at>=@from"; parms.Add(P("@from", from.Value.ToString("o"))); }
                        if (to.HasValue)   { sql += " AND performed_at<=@to";   parms.Add(P("@to",   to.Value.AddDays(1).ToString("o"))); }
                        sql += $" ORDER BY performed_at DESC LIMIT {maxRows}";
                        return QueryTable(sql, parms.ToArray());
                    }
    
                    public void UpdateDailyStats(string atmId, DateTime date,
                        int approvedDelta = 0, int failedDelta = 0, int cardsDelta = 0,
                        long cashDelta = 0, long bytesDelta = 0)
                    {
                        var dateStr = date.ToString("yyyy-MM-dd");
                        ExecuteSingle(@"
            INSERT INTO daily_stats (stat_id, atm_id, stat_date, approved_tx, failed_tx, cards_captured,
                                        cash_dispensed, journal_bytes, updated_at)
            VALUES (@id, @atm, @date, @app, @fail, @cards, @cash, @bytes, @ua)
            ON CONFLICT(atm_id, stat_date) DO UPDATE SET
                approved_tx    = approved_tx    + excluded.approved_tx,
                failed_tx      = failed_tx      + excluded.failed_tx,
                cards_captured = cards_captured + excluded.cards_captured,
                cash_dispensed = cash_dispensed + excluded.cash_dispensed,
                journal_bytes  = journal_bytes  + excluded.journal_bytes,
                updated_at     = excluded.updated_at",
                            P("@id",    Guid.NewGuid().ToString("N")),
                            P("@atm",   atmId),
                            P("@date",  dateStr),
                            P("@app",   approvedDelta),
                            P("@fail",  failedDelta),
                            P("@cards", cardsDelta),
                            P("@cash",  cashDelta),
                            P("@bytes", bytesDelta),
                            P("@ua",    DateTime.UtcNow.ToString("o")));
                    }
    
                    public DataTable GetDailyStatsTable(string atmId, string fromDate, string toDate)
                    {
                        return QueryTable(@"
            SELECT stat_date, approved_tx, failed_tx, cards_captured, cash_dispensed, journal_bytes,
                    uptime_percent, sync_success_percent
            FROM daily_stats
            WHERE atm_id=@atm AND stat_date>=@from AND stat_date<=@to
            ORDER BY stat_date DESC",
                            P("@atm", atmId), P("@from", fromDate), P("@to", toDate));
                    }
    
                    private void ExecuteSchema(string sql)
                    {
                        if (!_initialized || string.IsNullOrWhiteSpace(sql)) return;
                        lock (_queryLock)
                        {
                            conn.Open();
                            try { cmd.ExecuteNonQuery(); }
                            catch (SQLiteException ex)
                            {
                                // تجاهل "already exists" و "UNIQUE constraint"
                                if (!ex.Message.Contains("already exists") &&
                                    !ex.Message.Contains("UNIQUE") &&
                                    !ex.Message.Contains("no such"))
                                    AppLogger.Instance.Warning($"Schema warning: {ex.Message}", "DB");
                            }
                        }
                    }
    
                    private void ExecuteSingle(string sql, params SQLiteParameter[] parms)
                    {
                        if (!_initialized) return;
                        lock (_queryLock)
                        {
                            conn.Open();
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            try { cmd.ExecuteNonQuery(); }
                            catch (SQLiteException ex)
                            {
                                if (!ex.Message.Contains("UNIQUE"))
                                {
                                    AppLogger.Instance.Error($"DB Error: {ex.Message}", "DB");
                                    throw;
                                }
                            }
                        }
                    }
    
                    private void Query(string sql, Action<IDataReader> rowHandler, params SQLiteParameter[] parms)
                    {
                        if (!_initialized) return;
                        lock (_queryLock)
                        {
                            conn.Open();
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            while (r.Read()) rowHandler(r);
                        }
                    }
    
                    private T QueryScalar<T>(string sql, params SQLiteParameter[] parms)
                    {
                        if (!_initialized) return default;
                        lock (_queryLock)
                        {
                            conn.Open();
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            var result = cmd.ExecuteScalar();
                            if (result == null || result == DBNull.Value) return default;
                            return (T)Convert.ChangeType(result, typeof(T));
                        }
                    }
    
                    private DataTable QueryTable(string sql, params SQLiteParameter[] parms)
                    {
                        var dt = new DataTable();
                        if (!_initialized) return dt;
                        lock (_queryLock)
                        {
                            conn.Open();
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            adapter.Fill(dt);
                        }
                        return dt;
                    }
    
                    private static SQLiteParameter P(string name, object value)
                        => new SQLiteParameter(name, value ?? (object)DBNull.Value);
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v17_bak
                    private readonly object _queryLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v17_bak
                    private void ExecuteSchema(string sql)
                    {
                        if (!_initialized || string.IsNullOrWhiteSpace(sql)) return;
                        lock (_queryLock)
                        {
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            using var cmd = new SQLiteCommand(sql.Trim(), conn);
                            try { cmd.ExecuteNonQuery(); }
                            catch (SQLiteException ex)
                            {
                                // تجاهل "already exists" و "UNIQUE constraint"
                                if (!ex.Message.Contains("already exists") &&
                                    !ex.Message.Contains("UNIQUE") &&
                                    !ex.Message.Contains("no such"))
                                    AppLogger.Instance.Warning($"Schema warning: {ex.Message}", "DB");
                            }
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v17_bak
                    private void ExecuteSingle(string sql, params SQLiteParameter[] parms)
                    {
                        if (!_initialized) return;
                        lock (_queryLock)
                        {
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            using var cmd  = new SQLiteCommand(sql, conn);
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            try { cmd.ExecuteNonQuery(); }
                            catch (SQLiteException ex)
                            {
                                if (!ex.Message.Contains("UNIQUE"))
                                {
                                    AppLogger.Instance.Error($"DB Error: {ex.Message}", "DB");
                                    throw;
                                }
                            }
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v17_bak
                    private void Query(string sql, Action<IDataReader> rowHandler, params SQLiteParameter[] parms)
                    {
                        if (!_initialized) return;
                        lock (_queryLock)
                        {
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            using var cmd  = new SQLiteCommand(sql, conn);
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            using var r = cmd.ExecuteReader();
                            while (r.Read()) rowHandler(r);
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v17_bak
                    private T QueryScalar<T>(string sql, params SQLiteParameter[] parms)
                    {
                        if (!_initialized) return default;
                        lock (_queryLock)
                        {
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            using var cmd  = new SQLiteCommand(sql, conn);
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            var result = cmd.ExecuteScalar();
                            if (result == null || result == DBNull.Value) return default;
                            return (T)Convert.ChangeType(result, typeof(T));
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v17_bak
                    private DataTable QueryTable(string sql, params SQLiteParameter[] parms)
                    {
                        var dt = new DataTable();
                        if (!_initialized) return dt;
                        lock (_queryLock)
                        {
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            using var cmd     = new SQLiteCommand(sql, conn);
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            using var adapter = new SQLiteDataAdapter(cmd);
                            adapter.Fill(dt);
                        }
                        return dt;
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v16_bak
                    private readonly object _queryLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v16_bak
                    private void ExecuteSchema(string sql)
                    {
                        if (!_initialized || string.IsNullOrWhiteSpace(sql)) return;
                        lock (_queryLock)
                        {
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            using var cmd = new SQLiteCommand(sql.Trim(), conn);
                            try { cmd.ExecuteNonQuery(); }
                            catch (SQLiteException ex)
                            {
                                // تجاهل "already exists" و "UNIQUE constraint"
                                if (!ex.Message.Contains("already exists") &&
                                    !ex.Message.Contains("UNIQUE") &&
                                    !ex.Message.Contains("no such"))
                                    AppLogger.Instance.Warning($"Schema warning: {ex.Message}", "DB");
                            }
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v16_bak
                    private void ExecuteSingle(string sql, params SQLiteParameter[] parms)
                    {
                        if (!_initialized) return;
                        lock (_queryLock)
                        {
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            using var cmd  = new SQLiteCommand(sql, conn);
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            try { cmd.ExecuteNonQuery(); }
                            catch (SQLiteException ex)
                            {
                                if (!ex.Message.Contains("UNIQUE"))
                                {
                                    AppLogger.Instance.Error($"DB Error: {ex.Message}", "DB");
                                    throw;
                                }
                            }
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v16_bak
                    private void Query(string sql, Action<IDataReader> rowHandler, params SQLiteParameter[] parms)
                    {
                        if (!_initialized) return;
                        lock (_queryLock)
                        {
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            using var cmd  = new SQLiteCommand(sql, conn);
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            using var r = cmd.ExecuteReader();
                            while (r.Read()) rowHandler(r);
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v16_bak
                    private T QueryScalar<T>(string sql, params SQLiteParameter[] parms)
                    {
                        if (!_initialized) return default;
                        lock (_queryLock)
                        {
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            using var cmd  = new SQLiteCommand(sql, conn);
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            var result = cmd.ExecuteScalar();
                            if (result == null || result == DBNull.Value) return default;
                            return (T)Convert.ChangeType(result, typeof(T));
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v16_bak
                    private DataTable QueryTable(string sql, params SQLiteParameter[] parms)
                    {
                        var dt = new DataTable();
                        if (!_initialized) return dt;
                        lock (_queryLock)
                        {
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            using var cmd     = new SQLiteCommand(sql, conn);
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            using var adapter = new SQLiteDataAdapter(cmd);
                            adapter.Fill(dt);
                        }
                        return dt;
                    }
    
                    public async Task InitializeAsync(CancellationToken ct = default)
                    {
                        await EnsureSchemaVersionTableAsync(ct).ConfigureAwait(false);
                        await RunMigrationsAsync(ct).ConfigureAwait(false);
                        _schemaVersion = await GetCurrentSchemaVersionAsync(ct).ConfigureAwait(false);
                    }
    
                    private async Task EnsureSchemaVersionTableAsync(CancellationToken ct)
                    {
                        const string sql = @"
                            CREATE TABLE IF NOT EXISTS SchemaVersion (
                                VersionId INTEGER PRIMARY KEY,
                                VersionName TEXT NOT NULL,
                                Description TEXT,
                                AppliedUtc TEXT NOT NULL DEFAULT (datetime('now')),
                                Status TEXT NOT NULL DEFAULT 'Applied'
                            );";
                        await ExecuteNonQueryAsync(sql, ct: ct).ConfigureAwait(false);
                    }
    
                    private async Task RunMigrationsAsync(CancellationToken ct)
                    {
                        lock (_migrationLock)
                        {
                            // Migration 1: Core tables
                            Migrate_V1().GetAwaiter().GetResult();
    
                            // Migration 2: Add health snapshots and parser evidence
                            Migrate_V2().GetAwaiter().GetResult();
    
                            // Migration 3: Add command audit and RBAC
                            Migrate_V3().GetAwaiter().GetResult();
                        }
                        await Task.CompletedTask;
                    }
    
                    private async Task Migrate_V1()
                    {
                        var version = await GetCurrentSchemaVersionAsync(CancellationToken.None);
                        if (version >= 1) return;
    
                        var sql = @"
                            CREATE TABLE IF NOT EXISTS AtmDevice (
                                AtmId TEXT PRIMARY KEY, AtmName TEXT, Vendor TEXT, Model TEXT,
                                TerminalId TEXT, LUNO TEXT, Branch TEXT, Region TEXT, Province TEXT,
                                City TEXT, Geography TEXT, IpAddress TEXT, Port INTEGER DEFAULT 0,
                                OperationalMode TEXT DEFAULT 'Unknown', ConnectionState TEXT DEFAULT 'Offline',
                                JournalSourcePath TEXT, BackupPath TEXT, ImageInboxPath TEXT, ImageDestinationPath TEXT,
                                CreatedUtc TEXT, LastUpdatedUtc TEXT
                            );
                            CREATE TABLE IF NOT EXISTS ClientSession (
                                SessionId TEXT PRIMARY KEY, AtmId TEXT, RemoteAddress TEXT,
                                ClientVersion TEXT, ProtocolVersion TEXT,
                                ConnectedUtc TEXT, LastHeartbeatUtc TEXT, DisconnectedUtc TEXT,
                                PendingCommands INTEGER DEFAULT 0, TotalBytesSent INTEGER DEFAULT 0,
                                TotalBytesReceived INTEGER DEFAULT 0, IsHealthy INTEGER DEFAULT 1,
                                SessionState TEXT DEFAULT 'Active'
                            );
                            CREATE TABLE IF NOT EXISTS FileManifest (
                                ManifestId TEXT PRIMARY KEY, AtmId TEXT, FileName TEXT,
                                FileSize INTEGER DEFAULT 0, Checksum TEXT, FileType TEXT DEFAULT 'Journal',
                                SourcePath TEXT, ArchivePath TEXT, TotalChunks INTEGER DEFAULT 0,
                                ChunkSize INTEGER DEFAULT 0, Status TEXT DEFAULT 'Pending',
                                FailureReason TEXT, RetryCount INTEGER DEFAULT 0,
                                CreatedUtc TEXT, LastModifiedUtc TEXT
                            );
                            CREATE TABLE IF NOT EXISTS CommandQueue (
                                CommandId TEXT PRIMARY KEY, CorrelationId TEXT, CommandType TEXT,
                                TargetAtm TEXT, OperatorId TEXT, ApproverId TEXT,
                                RequiredRole TEXT DEFAULT 'Admin', RiskLevel TEXT DEFAULT 'Low',
                                Payload TEXT, Signature TEXT, Nonce TEXT, State TEXT DEFAULT 'Draft',
                                FailureReason TEXT, RollbackPlan TEXT, RollbackExecuted INTEGER DEFAULT 0,
                                CreatedUtc TEXT, ApprovedUtc TEXT, ExpiryUtc TEXT, CompletedUtc TEXT,
                                RetryCount INTEGER DEFAULT 0, MaxRetries INTEGER DEFAULT 3
                            );
                            INSERT OR IGNORE INTO SchemaVersion (VersionId, VersionName, Description) VALUES (1, 'v1.0', 'Core tables');
                        ";
                        await ExecuteNonQueryAsync(sql).ConfigureAwait(false);
                    }
    
                    private async Task Migrate_V2()
                    {
                        var version = await GetCurrentSchemaVersionAsync(CancellationToken.None);
                        if (version >= 2) return;
    
                        var sql = @"
                            CREATE TABLE IF NOT EXISTS HealthSnapshot (
                                SnapshotId TEXT PRIMARY KEY, AtmId TEXT, SnapshotType TEXT DEFAULT 'Operational',
                                HealthScore REAL, ConfidenceScore REAL, ConnectionState TEXT, OperationalMode TEXT,
                                AgentServiceState TEXT, ActiveAlarms INTEGER DEFAULT 0, Priority TEXT DEFAULT 'None',
                                SnapshotJson TEXT, SnapshotUtc TEXT
                            );
                            CREATE TABLE IF NOT EXISTS ParserRun (
                                RunId TEXT PRIMARY KEY, JournalFileId TEXT, ParserName TEXT, Vendor TEXT,
                                TotalLines INTEGER, ParsedLines INTEGER, TransactionsFound INTEGER,
                                SuccessCount INTEGER, FailedCount INTEGER, SuspiciousCount INTEGER,
                                Status TEXT DEFAULT 'Running', ErrorMessage TEXT,
                                StartedUtc TEXT, CompletedUtc TEXT
                            );
                            CREATE TABLE IF NOT EXISTS ParserEvidence (
                                EvidenceId TEXT PRIMARY KEY, RunId TEXT, TransactionId TEXT,
                                EvidenceType TEXT, EvidenceValue TEXT, Confidence REAL, CapturedUtc TEXT
                            );
                            CREATE TABLE IF NOT EXISTS JournalFile (
                                JournalFileId TEXT PRIMARY KEY, AtmId TEXT, FileName TEXT,
                                FileSize INTEGER, Checksum TEXT, ArchivePath TEXT,
                                TotalLines INTEGER, ParsedLines INTEGER, TransactionCount INTEGER,
                                ParseStatus TEXT DEFAULT 'Pending', ParseError TEXT, ParserUsed TEXT,
                                CapturedUtc TEXT, ParsedUtc TEXT
                            );
                            INSERT OR IGNORE INTO SchemaVersion (VersionId, VersionName, Description) VALUES (2, 'v2.0', 'Health snapshots and parser evidence');
                        ";
                        await ExecuteNonQueryAsync(sql).ConfigureAwait(false);
                    }
    
                    private async Task Migrate_V3()
                    {
                        var version = await GetCurrentSchemaVersionAsync(CancellationToken.None);
                        if (version >= 3) return;
    
                        var sql = @"
                            CREATE TABLE IF NOT EXISTS CommandAudit (
                                AuditId TEXT PRIMARY KEY, CommandId TEXT, Action TEXT, UserId TEXT,
                                UserRole TEXT, TargetAtm TEXT, Details TEXT, Result TEXT, TimestampUtc TEXT
                            );
                            CREATE TABLE IF NOT EXISTS AuditLog (
                                LogId TEXT PRIMARY KEY, Action TEXT, UserId TEXT, UserRole TEXT,
                                TargetAtm TEXT, CommandId TEXT, Details TEXT, Result TEXT,
                                RiskLevel TEXT DEFAULT 'Low', TimestampUtc TEXT
                            );
                            CREATE TABLE IF NOT EXISTS UserEntity (
                                UserId TEXT PRIMARY KEY, Username TEXT, DisplayName TEXT,
                                Role TEXT DEFAULT 'Viewer', IsActive INTEGER DEFAULT 1,
                                CreatedUtc TEXT, LastLoginUtc TEXT, PasswordHash TEXT,
                                AllowedAtmGroups TEXT, AllowedModules TEXT
                            );
                            INSERT OR IGNORE INTO SchemaVersion (VersionId, VersionName, Description) VALUES (3, 'v3.0', 'Command audit and RBAC');
                        ";
                        await ExecuteNonQueryAsync(sql).ConfigureAwait(false);
                    }
    
                    private async Task<int> GetCurrentSchemaVersionAsync(CancellationToken ct)
                    {
                        try
                        {
                            var result = await ExecuteScalarAsync("SELECT COALESCE(MAX(VersionId), 0) FROM SchemaVersion", ct: ct)
                                .ConfigureAwait(false);
                            return Convert.ToInt32(result);
                        }
                        catch
                        {
                            return 0;
                        }
                    }
    
                    public async Task<int> ExecuteNonQueryAsync(string sql,
                        Dictionary<string, object?>? parameters = null,
                        CancellationToken ct = default)
                    {
                        await _writeLock.WaitAsync(ct).ConfigureAwait(false);
                        try
                        {
                            using var conn = new SQLiteConnection(_connectionString);
                            await conn.OpenAsync(ct).ConfigureAwait(false);
                            using var cmd = new SQLiteCommand(sql, conn);
                            ApplyParameters(cmd, parameters);
                            return await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                        }
                        finally { _writeLock.Release(); }
                    }
    
                    public async Task<object?> ExecuteScalarAsync(string sql,
                        Dictionary<string, object?>? parameters = null,
                        CancellationToken ct = default)
                    {
                        using var conn = new SQLiteConnection(_connectionString);
                        await conn.OpenAsync(ct).ConfigureAwait(false);
                        using var cmd = new SQLiteCommand(sql, conn);
                        ApplyParameters(cmd, parameters);
                        return await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                    }
    
                    public async Task<List<Dictionary<string, object?>>> QueryAsync(string sql,
                        Dictionary<string, object?>? parameters = null,
                        CancellationToken ct = default)
                    {
                        var results = new List<Dictionary<string, object?>>();
                        using var conn = new SQLiteConnection(_connectionString);
                        await conn.OpenAsync(ct).ConfigureAwait(false);
                        using var cmd = new SQLiteCommand(sql, conn);
                        ApplyParameters(cmd, parameters);
                        using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
    
                        while (await reader.ReadAsync(ct).ConfigureAwait(false))
                        {
                            var row = new Dictionary<string, object?>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            }
                            results.Add(row);
                        }
                        return results;
                    }
    
                    public async Task ExecuteTransactionAsync(List<(string Sql, Dictionary<string, object?>? Parameters)> statements,
                        CancellationToken ct = default)
                    {
                        await _writeLock.WaitAsync(ct).ConfigureAwait(false);
                        try
                        {
                            using var conn = new SQLiteConnection(_connectionString);
                            await conn.OpenAsync(ct).ConfigureAwait(false);
                            using var transaction = conn.BeginTransaction();
                            try
                            {
                                foreach (var (sql, parameters) in statements)
                                {
                                    using var cmd = new SQLiteCommand(sql, conn);
                                    ApplyParameters(cmd, parameters);
                                    await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                                }
                                transaction.Commit();
                            }
                            catch
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }
                        finally { _writeLock.Release(); }
                    }
    
                    public async Task UpsertAtmAsync(AtmDeviceEntity atm, CancellationToken ct = default)
                    {
                        var sql = @"
                            INSERT OR REPLACE INTO AtmDevice (AtmId, AtmName, Vendor, Model, TerminalId, LUNO, Branch, Region, Province, City, Geography, IpAddress, Port, OperationalMode, ConnectionState, JournalSourcePath, BackupPath, ImageInboxPath, ImageDestinationPath, LastUpdatedUtc)
                            VALUES (@AtmId, @AtmName, @Vendor, @Model, @TerminalId, @LUNO, @Branch, @Region, @Province, @City, @Geography, @IpAddress, @Port, @OperationalMode, @ConnectionState, @JournalSourcePath, @BackupPath, @ImageInboxPath, @ImageDestinationPath, @LastUpdatedUtc);
                        ";
                        await ExecuteNonQueryAsync(sql, new Dictionary<string, object?>
                        {
                            ["@AtmId"] = atm.AtmId, ["@AtmName"] = atm.AtmName, ["@Vendor"] = atm.Vendor,
                            ["@Model"] = atm.Model, ["@TerminalId"] = atm.TerminalId, ["@LUNO"] = atm.LUNO,
                            ["@Branch"] = atm.Branch, ["@Region"] = atm.Region, ["@Province"] = atm.Province,
                            ["@City"] = atm.City, ["@Geography"] = atm.Geography, ["@IpAddress"] = atm.IpAddress,
                            ["@Port"] = atm.Port, ["@OperationalMode"] = atm.OperationalMode,
                            ["@ConnectionState"] = atm.ConnectionState, ["@JournalSourcePath"] = atm.JournalSourcePath,
                            ["@BackupPath"] = atm.BackupPath, ["@ImageInboxPath"] = atm.ImageInboxPath,
                            ["@ImageDestinationPath"] = atm.ImageDestinationPath,
                            ["@LastUpdatedUtc"] = DateTime.UtcNow.ToString("O")
                        }, ct).ConfigureAwait(false);
                    }
    
                    public async Task<List<AtmDeviceEntity>> GetAllAtmsAsync(CancellationToken ct = default)
                    {
                        var rows = await QueryAsync("SELECT * FROM AtmDevice ORDER BY AtmId", ct: ct).ConfigureAwait(false);
                        return rows.Select(MapAtmDevice).ToList();
                    }
    
                    public async Task UpsertSessionAsync(ClientSessionEntity session, CancellationToken ct = default)
                    {
                        var sql = @"
                            INSERT OR REPLACE INTO ClientSession (SessionId, AtmId, RemoteAddress, ClientVersion, ProtocolVersion, ConnectedUtc, LastHeartbeatUtc, DisconnectedUtc, PendingCommands, TotalBytesSent, TotalBytesReceived, IsHealthy, SessionState)
                            VALUES (@SessionId, @AtmId, @RemoteAddress, @ClientVersion, @ProtocolVersion, @ConnectedUtc, @LastHeartbeatUtc, @DisconnectedUtc, @PendingCommands, @TotalBytesSent, @TotalBytesReceived, @IsHealthy, @SessionState);
                        ";
                        await ExecuteNonQueryAsync(sql, new Dictionary<string, object?>
                        {
                            ["@SessionId"] = session.SessionId, ["@AtmId"] = session.AtmId,
                            ["@RemoteAddress"] = session.RemoteAddress, ["@ClientVersion"] = session.ClientVersion,
                            ["@ProtocolVersion"] = session.ProtocolVersion,
                            ["@ConnectedUtc"] = session.ConnectedUtc.ToString("O"),
                            ["@LastHeartbeatUtc"] = session.LastHeartbeatUtc.ToString("O"),
                            ["@DisconnectedUtc"] = session.DisconnectedUtc?.ToString("O"),
                            ["@PendingCommands"] = session.PendingCommands,
                            ["@TotalBytesSent"] = session.TotalBytesSent,
                            ["@TotalBytesReceived"] = session.TotalBytesReceived,
                            ["@IsHealthy"] = session.IsHealthy ? 1 : 0,
                            ["@SessionState"] = session.SessionState
                        }, ct).ConfigureAwait(false);
                    }
    
                    public async Task SaveHealthSnapshotAsync(HealthSnapshotEntity snapshot, CancellationToken ct = default)
                    {
                        var sql = @"
                            INSERT INTO HealthSnapshot (SnapshotId, AtmId, SnapshotType, HealthScore, ConfidenceScore, ConnectionState, OperationalMode, AgentServiceState, ActiveAlarms, Priority, SnapshotJson, SnapshotUtc)
                            VALUES (@SnapshotId, @AtmId, @SnapshotType, @HealthScore, @ConfidenceScore, @ConnectionState, @OperationalMode, @AgentServiceState, @ActiveAlarms, @Priority, @SnapshotJson, @SnapshotUtc);
                        ";
                        await ExecuteNonQueryAsync(sql, new Dictionary<string, object?>
                        {
                            ["@SnapshotId"] = snapshot.SnapshotId,
                            ["@AtmId"] = snapshot.AtmId,
                            ["@SnapshotType"] = snapshot.SnapshotType,
                            ["@HealthScore"] = snapshot.HealthScore,
                            ["@ConfidenceScore"] = snapshot.ConfidenceScore,
                            ["@ConnectionState"] = snapshot.ConnectionState,
                            ["@OperationalMode"] = snapshot.OperationalMode,
                            ["@AgentServiceState"] = snapshot.AgentServiceState,
                            ["@ActiveAlarms"] = snapshot.ActiveAlarms,
                            ["@Priority"] = snapshot.Priority,
                            ["@SnapshotJson"] = snapshot.SnapshotJson,
                            ["@SnapshotUtc"] = snapshot.SnapshotUtc.ToString("O")
                        }, ct).ConfigureAwait(false);
                    }
    
                    public async Task<List<HealthSnapshotEntity>> GetHealthSnapshotsAsync(string atmId, int limit = 100, CancellationToken ct = default)
                    {
                        var rows = await QueryAsync(
                            "SELECT * FROM HealthSnapshot WHERE AtmId = @AtmId ORDER BY SnapshotUtc DESC LIMIT @Limit",
                            new Dictionary<string, object?> { ["@AtmId"] = atmId, ["@Limit"] = limit }, ct).ConfigureAwait(false);
                        return rows.Select(MapHealthSnapshot).ToList();
                    }
    
                    public async Task VacuumAsync(CancellationToken ct = default)
                    {
                        await ExecuteNonQueryAsync("VACUUM;", ct: ct).ConfigureAwait(false);
                    }
    
                    public long GetDatabaseSize()
                    {
                        if (File.Exists(_databasePath))
                            return new FileInfo(_databasePath).Length;
                        return 0;
                    }
    
                    public async Task<Dictionary<string, long>> GetTableRowCountsAsync(CancellationToken ct = default)
                    {
                        var tables = new[] { "AtmDevice", "ClientSession", "FileManifest", "CommandQueue", "CommandAudit", "AuditLog", "HealthSnapshot", "ParserRun", "JournalFile", "UserEntity" };
                        var counts = new Dictionary<string, long>();
                        foreach (var table in tables)
                        {
                            try
                            {
                                var result = await ExecuteScalarAsync($"SELECT COUNT(*) FROM {table}", ct: ct).ConfigureAwait(false);
                                counts[table] = Convert.ToInt64(result);
                            }
                            catch { counts[table] = 0; }
                        }
                        return counts;
                    }
    
                    private static void ApplyParameters(SQLiteCommand cmd, Dictionary<string, object?>? parameters)
                    {
                        if (parameters == null) return;
                        foreach (var (key, value) in parameters)
                        {
                            cmd.Parameters.AddWithValue(key, value ?? DBNull.Value);
                        }
                    }
    
                    private static AtmDeviceEntity MapAtmDevice(Dictionary<string, object?> row)
                    {
                        return new AtmDeviceEntity
                        {
                            AtmId = row["AtmId"]?.ToString() ?? "",
                            AtmName = row["AtmName"]?.ToString() ?? "",
                            Vendor = row["Vendor"]?.ToString() ?? "",
                            Model = row["Model"]?.ToString() ?? "",
                            TerminalId = row["TerminalId"]?.ToString() ?? "",
                            LUNO = row["LUNO"]?.ToString() ?? "",
                            Branch = row["Branch"]?.ToString() ?? "",
                            Region = row["Region"]?.ToString() ?? "",
                            Province = row["Province"]?.ToString() ?? "",
                            City = row["City"]?.ToString() ?? "",
                            Geography = row["Geography"]?.ToString() ?? "",
                            IpAddress = row["IpAddress"]?.ToString() ?? "",
                            Port = Convert.ToInt32(row["Port"] ?? 0),
                            OperationalMode = row["OperationalMode"]?.ToString() ?? "Unknown",
                            ConnectionState = row["ConnectionState"]?.ToString() ?? "Offline",
                            JournalSourcePath = row["JournalSourcePath"]?.ToString() ?? "",
                            BackupPath = row["BackupPath"]?.ToString() ?? "",
                            ImageInboxPath = row["ImageInboxPath"]?.ToString() ?? "",
                            ImageDestinationPath = row["ImageDestinationPath"]?.ToString() ?? ""
                        };
                    }
    
                    private static HealthSnapshotEntity MapHealthSnapshot(Dictionary<string, object?> row)
                    {
                        return new HealthSnapshotEntity
                        {
                            SnapshotId = row["SnapshotId"]?.ToString() ?? "",
                            AtmId = row["AtmId"]?.ToString() ?? "",
                            SnapshotType = row["SnapshotType"]?.ToString() ?? "Operational",
                            HealthScore = Convert.ToDouble(row["HealthScore"] ?? 0),
                            ConfidenceScore = Convert.ToDouble(row["ConfidenceScore"] ?? 0),
                            ConnectionState = row["ConnectionState"]?.ToString() ?? "Unknown",
                            OperationalMode = row["OperationalMode"]?.ToString() ?? "Unknown",
                            AgentServiceState = row["AgentServiceState"]?.ToString() ?? "Unknown",
                            ActiveAlarms = Convert.ToInt32(row["ActiveAlarms"] ?? 0),
                            Priority = row["Priority"]?.ToString() ?? "None",
                            SnapshotJson = row["SnapshotJson"]?.ToString()
                        };
                    }
    
                    public void Dispose()
                    {
                        if (!_disposed)
                        {
                            _writeLock.Dispose();
                            _cache.Clear();
                            _disposed = true;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v15_bak
                    private readonly object _queryLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v15_bak
                    private void ExecuteSchema(string sql)
                    {
                        if (!_initialized || string.IsNullOrWhiteSpace(sql)) return;
                        lock (_queryLock)
                        {
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            using var cmd = new SQLiteCommand(sql.Trim(), conn);
                            try { cmd.ExecuteNonQuery(); }
                            catch (SQLiteException ex)
                            {
                                // تجاهل "already exists" و "UNIQUE constraint"
                                if (!ex.Message.Contains("already exists") &&
                                    !ex.Message.Contains("UNIQUE") &&
                                    !ex.Message.Contains("no such"))
                                    AppLogger.Instance.Warning($"Schema warning: {ex.Message}", "DB");
                            }
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v15_bak
                    private void ExecuteSingle(string sql, params SQLiteParameter[] parms)
                    {
                        if (!_initialized) return;
                        lock (_queryLock)
                        {
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            using var cmd  = new SQLiteCommand(sql, conn);
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            try { cmd.ExecuteNonQuery(); }
                            catch (SQLiteException ex)
                            {
                                if (!ex.Message.Contains("UNIQUE"))
                                {
                                    AppLogger.Instance.Error($"DB Error: {ex.Message}", "DB");
                                    throw;
                                }
                            }
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v15_bak
                    private void Query(string sql, Action<IDataReader> rowHandler, params SQLiteParameter[] parms)
                    {
                        if (!_initialized) return;
                        lock (_queryLock)
                        {
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            using var cmd  = new SQLiteCommand(sql, conn);
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            using var r = cmd.ExecuteReader();
                            while (r.Read()) rowHandler(r);
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v15_bak
                    private T QueryScalar<T>(string sql, params SQLiteParameter[] parms)
                    {
                        if (!_initialized) return default;
                        lock (_queryLock)
                        {
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            using var cmd  = new SQLiteCommand(sql, conn);
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            var result = cmd.ExecuteScalar();
                            if (result == null || result == DBNull.Value) return default;
                            return (T)Convert.ChangeType(result, typeof(T));
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Data\DatabaseManager.cs.v15_bak
                    private DataTable QueryTable(string sql, params SQLiteParameter[] parms)
                    {
                        var dt = new DataTable();
                        if (!_initialized) return dt;
                        lock (_queryLock)
                        {
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            using var cmd     = new SQLiteCommand(sql, conn);
                            if (parms != null) cmd.Parameters.AddRange(parms);
                            using var adapter = new SQLiteDataAdapter(cmd);
                            adapter.Fill(dt);
                        }
                        return dt;
                    }
    
    
        }
}

using var transaction = conn.BeginTransaction();
using var r = cmd.ExecuteReader();
using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
using var cmd     = new SQLiteCommand(sql, conn);
using var cmd = new SQLiteCommand(sql.Trim(), conn);
using var conn = new SQLiteConnection(_connectionString);
using var conn = new SQLiteConnection(_connStr);
using var adapter = new SQLiteDataAdapter(cmd);

namespace EJLive.Core.Data
{
    /// <summary>
        /// Production-grade database manager for EJLive Unified.
        /// Manages SQLite database with schema versioning, migration support,
        /// connection pooling, and thread-safe operations.
        /// Replaces the stub DatabaseManagerStub with full CRUD operations
        /// for all 20 entities defined in DatabaseSchema.
        /// </summary>
        public sealed class DatabaseManager : IDisposable
        {
            private readonly string _connectionString;
            private readonly string _databasePath;
            private readonly ConcurrentDictionary<string, object> _cache = new();
            private readonly SemaphoreSlim _writeLock = new(1, 1);
            private readonly object _migrationLock = new();
            private int _schemaVersion;
            private bool _disposed;
    
            /// <summary>Current database schema version.</summary>
            public int SchemaVersion => _schemaVersion;
    
            /// <summary>Database file path.</summary>
            public string DatabasePath => _databasePath;
    
            public DatabaseManager(string? databasePath = null)
            {
                _databasePath = databasePath
                    ?? Environment.GetEnvironmentVariable("EJLIVE_DATABASE_PATH")
                    ?? Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "EJLive", "Server", "ejlive.db");
    
                var dir = Path.GetDirectoryName(_databasePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
    
                _connectionString = $"Data Source={_databasePath};Version=3;Journal Mode=WAL;Pooling=True;Max Pool Size=20;";
            }
    
            // ===== Initialization & Migration =====
    
            /// <summary>Initialize database, run migrations, verify schema.</summary>
            public async Task InitializeAsync(CancellationToken ct = default)
            {
                await EnsureSchemaVersionTableAsync(ct).ConfigureAwait(false);
                await RunMigrationsAsync(ct).ConfigureAwait(false);
                _schemaVersion = await GetCurrentSchemaVersionAsync(ct).ConfigureAwait(false);
            }
    
            private async Task EnsureSchemaVersionTableAsync(CancellationToken ct)
            {
                const string sql = @"
                    CREATE TABLE IF NOT EXISTS SchemaVersion (
                        VersionId INTEGER PRIMARY KEY,
                        VersionName TEXT NOT NULL,
                        Description TEXT,
                        AppliedUtc TEXT NOT NULL DEFAULT (datetime('now')),
                        Status TEXT NOT NULL DEFAULT 'Applied'
                    );";
                await ExecuteNonQueryAsync(sql, ct: ct).ConfigureAwait(false);
            }
    
            private async Task RunMigrationsAsync(CancellationToken ct)
            {
                lock (_migrationLock)
                {
                    // Migration 1: Core tables
                    Migrate_V1().GetAwaiter().GetResult();
    
                    // Migration 2: Add health snapshots and parser evidence
                    Migrate_V2().GetAwaiter().GetResult();
    
                    // Migration 3: Add command audit and RBAC
                    Migrate_V3().GetAwaiter().GetResult();
                }
                await Task.CompletedTask;
            }
    
            private async Task Migrate_V1()
            {
                var version = await GetCurrentSchemaVersionAsync(CancellationToken.None);
                if (version >= 1) return;
    
                var sql = @"
                    CREATE TABLE IF NOT EXISTS AtmDevice (
                        AtmId TEXT PRIMARY KEY, AtmName TEXT, Vendor TEXT, Model TEXT,
                        TerminalId TEXT, LUNO TEXT, Branch TEXT, Region TEXT, Province TEXT,
                        City TEXT, Geography TEXT, IpAddress TEXT, Port INTEGER DEFAULT 0,
                        OperationalMode TEXT DEFAULT 'Unknown', ConnectionState TEXT DEFAULT 'Offline',
                        JournalSourcePath TEXT, BackupPath TEXT, ImageInboxPath TEXT, ImageDestinationPath TEXT,
                        CreatedUtc TEXT, LastUpdatedUtc TEXT
                    );
                    CREATE TABLE IF NOT EXISTS ClientSession (
                        SessionId TEXT PRIMARY KEY, AtmId TEXT, RemoteAddress TEXT,
                        ClientVersion TEXT, ProtocolVersion TEXT,
                        ConnectedUtc TEXT, LastHeartbeatUtc TEXT, DisconnectedUtc TEXT,
                        PendingCommands INTEGER DEFAULT 0, TotalBytesSent INTEGER DEFAULT 0,
                        TotalBytesReceived INTEGER DEFAULT 0, IsHealthy INTEGER DEFAULT 1,
                        SessionState TEXT DEFAULT 'Active'
                    );
                    CREATE TABLE IF NOT EXISTS FileManifest (
                        ManifestId TEXT PRIMARY KEY, AtmId TEXT, FileName TEXT,
                        FileSize INTEGER DEFAULT 0, Checksum TEXT, FileType TEXT DEFAULT 'Journal',
                        SourcePath TEXT, ArchivePath TEXT, TotalChunks INTEGER DEFAULT 0,
                        ChunkSize INTEGER DEFAULT 0, Status TEXT DEFAULT 'Pending',
                        FailureReason TEXT, RetryCount INTEGER DEFAULT 0,
                        CreatedUtc TEXT, LastModifiedUtc TEXT
                    );
                    CREATE TABLE IF NOT EXISTS CommandQueue (
                        CommandId TEXT PRIMARY KEY, CorrelationId TEXT, CommandType TEXT,
                        TargetAtm TEXT, OperatorId TEXT, ApproverId TEXT,
                        RequiredRole TEXT DEFAULT 'Admin', RiskLevel TEXT DEFAULT 'Low',
                        Payload TEXT, Signature TEXT, Nonce TEXT, State TEXT DEFAULT 'Draft',
                        FailureReason TEXT, RollbackPlan TEXT, RollbackExecuted INTEGER DEFAULT 0,
                        CreatedUtc TEXT, ApprovedUtc TEXT, ExpiryUtc TEXT, CompletedUtc TEXT,
                        RetryCount INTEGER DEFAULT 0, MaxRetries INTEGER DEFAULT 3
                    );
                    INSERT OR IGNORE INTO SchemaVersion (VersionId, VersionName, Description) VALUES (1, 'v1.0', 'Core tables');
                ";
                await ExecuteNonQueryAsync(sql).ConfigureAwait(false);
            }
    
            private async Task Migrate_V2()
            {
                var version = await GetCurrentSchemaVersionAsync(CancellationToken.None);
                if (version >= 2) return;
    
                var sql = @"
                    CREATE TABLE IF NOT EXISTS HealthSnapshot (
                        SnapshotId TEXT PRIMARY KEY, AtmId TEXT, SnapshotType TEXT DEFAULT 'Operational',
                        HealthScore REAL, ConfidenceScore REAL, ConnectionState TEXT, OperationalMode TEXT,
                        AgentServiceState TEXT, ActiveAlarms INTEGER DEFAULT 0, Priority TEXT DEFAULT 'None',
                        SnapshotJson TEXT, SnapshotUtc TEXT
                    );
                    CREATE TABLE IF NOT EXISTS ParserRun (
                        RunId TEXT PRIMARY KEY, JournalFileId TEXT, ParserName TEXT, Vendor TEXT,
                        TotalLines INTEGER, ParsedLines INTEGER, TransactionsFound INTEGER,
                        SuccessCount INTEGER, FailedCount INTEGER, SuspiciousCount INTEGER,
                        Status TEXT DEFAULT 'Running', ErrorMessage TEXT,
                        StartedUtc TEXT, CompletedUtc TEXT
                    );
                    CREATE TABLE IF NOT EXISTS ParserEvidence (
                        EvidenceId TEXT PRIMARY KEY, RunId TEXT, TransactionId TEXT,
                        EvidenceType TEXT, EvidenceValue TEXT, Confidence REAL, CapturedUtc TEXT
                    );
                    CREATE TABLE IF NOT EXISTS JournalFile (
                        JournalFileId TEXT PRIMARY KEY, AtmId TEXT, FileName TEXT,
                        FileSize INTEGER, Checksum TEXT, ArchivePath TEXT,
                        TotalLines INTEGER, ParsedLines INTEGER, TransactionCount INTEGER,
                        ParseStatus TEXT DEFAULT 'Pending', ParseError TEXT, ParserUsed TEXT,
                        CapturedUtc TEXT, ParsedUtc TEXT
                    );
                    INSERT OR IGNORE INTO SchemaVersion (VersionId, VersionName, Description) VALUES (2, 'v2.0', 'Health snapshots and parser evidence');
                ";
                await ExecuteNonQueryAsync(sql).ConfigureAwait(false);
            }
    
            private async Task Migrate_V3()
            {
                var version = await GetCurrentSchemaVersionAsync(CancellationToken.None);
                if (version >= 3) return;
    
                var sql = @"
                    CREATE TABLE IF NOT EXISTS CommandAudit (
                        AuditId TEXT PRIMARY KEY, CommandId TEXT, Action TEXT, UserId TEXT,
                        UserRole TEXT, TargetAtm TEXT, Details TEXT, Result TEXT, TimestampUtc TEXT
                    );
                    CREATE TABLE IF NOT EXISTS AuditLog (
                        LogId TEXT PRIMARY KEY, Action TEXT, UserId TEXT, UserRole TEXT,
                        TargetAtm TEXT, CommandId TEXT, Details TEXT, Result TEXT,
                        RiskLevel TEXT DEFAULT 'Low', TimestampUtc TEXT
                    );
                    CREATE TABLE IF NOT EXISTS UserEntity (
                        UserId TEXT PRIMARY KEY, Username TEXT, DisplayName TEXT,
                        Role TEXT DEFAULT 'Viewer', IsActive INTEGER DEFAULT 1,
                        CreatedUtc TEXT, LastLoginUtc TEXT, PasswordHash TEXT,
                        AllowedAtmGroups TEXT, AllowedModules TEXT
                    );
                    INSERT OR IGNORE INTO SchemaVersion (VersionId, VersionName, Description) VALUES (3, 'v3.0', 'Command audit and RBAC');
                ";
                await ExecuteNonQueryAsync(sql).ConfigureAwait(false);
            }
    
            private async Task<int> GetCurrentSchemaVersionAsync(CancellationToken ct)
            {
                try
                {
                    var result = await ExecuteScalarAsync("SELECT COALESCE(MAX(VersionId), 0) FROM SchemaVersion", ct: ct)
                        .ConfigureAwait(false);
                    return Convert.ToInt32(result);
                }
                catch
                {
                    return 0;
                }
            }
    
            // ===== Core CRUD Operations =====
    
            /// <summary>Execute a parameterized non-query and return rows affected.</summary>
            public async Task<int> ExecuteNonQueryAsync(string sql,
                Dictionary<string, object?>? parameters = null,
                CancellationToken ct = default)
            {
                await _writeLock.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    using var conn = new SQLiteConnection(_connectionString);
                    await conn.OpenAsync(ct).ConfigureAwait(false);
                    using var cmd = new SQLiteCommand(sql, conn);
                    ApplyParameters(cmd, parameters);
                    return await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }
                finally { _writeLock.Release(); }
            }
    
            /// <summary>Execute a query and return the first column of the first row.</summary>
            public async Task<object?> ExecuteScalarAsync(string sql,
                Dictionary<string, object?>? parameters = null,
                CancellationToken ct = default)
            {
                using var conn = new SQLiteConnection(_connectionString);
                await conn.OpenAsync(ct).ConfigureAwait(false);
                using var cmd = new SQLiteCommand(sql, conn);
                ApplyParameters(cmd, parameters);
                return await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            }
    
            /// <summary>Execute a query and return a data reader.</summary>
            public async Task<List<Dictionary<string, object?>>> QueryAsync(string sql,
                Dictionary<string, object?>? parameters = null,
                CancellationToken ct = default)
            {
                var results = new List<Dictionary<string, object?>>();
                using var conn = new SQLiteConnection(_connectionString);
                await conn.OpenAsync(ct).ConfigureAwait(false);
                using var cmd = new SQLiteCommand(sql, conn);
                ApplyParameters(cmd, parameters);
                using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
    
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    results.Add(row);
                }
                return results;
            }
    
            // ===== Transaction Support =====
    
            /// <summary>Execute multiple SQL statements in a transaction.</summary>
            public async Task ExecuteTransactionAsync(List<(string Sql, Dictionary<string, object?>? Parameters)> statements,
                CancellationToken ct = default)
            {
                await _writeLock.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    using var conn = new SQLiteConnection(_connectionString);
                    await conn.OpenAsync(ct).ConfigureAwait(false);
                    using var transaction = conn.BeginTransaction();
                    try
                    {
                        foreach (var (sql, parameters) in statements)
                        {
                            using var cmd = new SQLiteCommand(sql, conn);
                            ApplyParameters(cmd, parameters);
                            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
                finally { _writeLock.Release(); }
            }
    
            // ===== ATM Operations =====
    
            /// <summary>Upsert an ATM device record.</summary>
            public async Task UpsertAtmAsync(AtmDeviceEntity atm, CancellationToken ct = default)
            {
                var sql = @"
                    INSERT OR REPLACE INTO AtmDevice (AtmId, AtmName, Vendor, Model, TerminalId, LUNO, Branch, Region, Province, City, Geography, IpAddress, Port, OperationalMode, ConnectionState, JournalSourcePath, BackupPath, ImageInboxPath, ImageDestinationPath, LastUpdatedUtc)
                    VALUES (@AtmId, @AtmName, @Vendor, @Model, @TerminalId, @LUNO, @Branch, @Region, @Province, @City, @Geography, @IpAddress, @Port, @OperationalMode, @ConnectionState, @JournalSourcePath, @BackupPath, @ImageInboxPath, @ImageDestinationPath, @LastUpdatedUtc);
                ";
                await ExecuteNonQueryAsync(sql, new Dictionary<string, object?>
                {
                    ["@AtmId"] = atm.AtmId, ["@AtmName"] = atm.AtmName, ["@Vendor"] = atm.Vendor,
                    ["@Model"] = atm.Model, ["@TerminalId"] = atm.TerminalId, ["@LUNO"] = atm.LUNO,
                    ["@Branch"] = atm.Branch, ["@Region"] = atm.Region, ["@Province"] = atm.Province,
                    ["@City"] = atm.City, ["@Geography"] = atm.Geography, ["@IpAddress"] = atm.IpAddress,
                    ["@Port"] = atm.Port, ["@OperationalMode"] = atm.OperationalMode,
                    ["@ConnectionState"] = atm.ConnectionState, ["@JournalSourcePath"] = atm.JournalSourcePath,
                    ["@BackupPath"] = atm.BackupPath, ["@ImageInboxPath"] = atm.ImageInboxPath,
                    ["@ImageDestinationPath"] = atm.ImageDestinationPath,
                    ["@LastUpdatedUtc"] = DateTime.UtcNow.ToString("O")
                }, ct).ConfigureAwait(false);
            }
    
            /// <summary>Get all ATM devices.</summary>
            public async Task<List<AtmDeviceEntity>> GetAllAtmsAsync(CancellationToken ct = default)
            {
                var rows = await QueryAsync("SELECT * FROM AtmDevice ORDER BY AtmId", ct: ct).ConfigureAwait(false);
                return rows.Select(MapAtmDevice).ToList();
            }
    
            // ===== Session Operations =====
    
            /// <summary>Upsert a client session.</summary>
            public async Task UpsertSessionAsync(ClientSessionEntity session, CancellationToken ct = default)
            {
                var sql = @"
                    INSERT OR REPLACE INTO ClientSession (SessionId, AtmId, RemoteAddress, ClientVersion, ProtocolVersion, ConnectedUtc, LastHeartbeatUtc, DisconnectedUtc, PendingCommands, TotalBytesSent, TotalBytesReceived, IsHealthy, SessionState)
                    VALUES (@SessionId, @AtmId, @RemoteAddress, @ClientVersion, @ProtocolVersion, @ConnectedUtc, @LastHeartbeatUtc, @DisconnectedUtc, @PendingCommands, @TotalBytesSent, @TotalBytesReceived, @IsHealthy, @SessionState);
                ";
                await ExecuteNonQueryAsync(sql, new Dictionary<string, object?>
                {
                    ["@SessionId"] = session.SessionId, ["@AtmId"] = session.AtmId,
                    ["@RemoteAddress"] = session.RemoteAddress, ["@ClientVersion"] = session.ClientVersion,
                    ["@ProtocolVersion"] = session.ProtocolVersion,
                    ["@ConnectedUtc"] = session.ConnectedUtc.ToString("O"),
                    ["@LastHeartbeatUtc"] = session.LastHeartbeatUtc.ToString("O"),
                    ["@DisconnectedUtc"] = session.DisconnectedUtc?.ToString("O"),
                    ["@PendingCommands"] = session.PendingCommands,
                    ["@TotalBytesSent"] = session.TotalBytesSent,
                    ["@TotalBytesReceived"] = session.TotalBytesReceived,
                    ["@IsHealthy"] = session.IsHealthy ? 1 : 0,
                    ["@SessionState"] = session.SessionState
                }, ct).ConfigureAwait(false);
            }
    
            // ===== Health Snapshot =====
    
            /// <summary>Save a health snapshot.</summary>
            public async Task SaveHealthSnapshotAsync(HealthSnapshotEntity snapshot, CancellationToken ct = default)
            {
                var sql = @"
                    INSERT INTO HealthSnapshot (SnapshotId, AtmId, SnapshotType, HealthScore, ConfidenceScore, ConnectionState, OperationalMode, AgentServiceState, ActiveAlarms, Priority, SnapshotJson, SnapshotUtc)
                    VALUES (@SnapshotId, @AtmId, @SnapshotType, @HealthScore, @ConfidenceScore, @ConnectionState, @OperationalMode, @AgentServiceState, @ActiveAlarms, @Priority, @SnapshotJson, @SnapshotUtc);
                ";
                await ExecuteNonQueryAsync(sql, new Dictionary<string, object?>
                {
                    ["@SnapshotId"] = snapshot.SnapshotId,
                    ["@AtmId"] = snapshot.AtmId,
                    ["@SnapshotType"] = snapshot.SnapshotType,
                    ["@HealthScore"] = snapshot.HealthScore,
                    ["@ConfidenceScore"] = snapshot.ConfidenceScore,
                    ["@ConnectionState"] = snapshot.ConnectionState,
                    ["@OperationalMode"] = snapshot.OperationalMode,
                    ["@AgentServiceState"] = snapshot.AgentServiceState,
                    ["@ActiveAlarms"] = snapshot.ActiveAlarms,
                    ["@Priority"] = snapshot.Priority,
                    ["@SnapshotJson"] = snapshot.SnapshotJson,
                    ["@SnapshotUtc"] = snapshot.SnapshotUtc.ToString("O")
                }, ct).ConfigureAwait(false);
            }
    
            /// <summary>Get recent health snapshots for an ATM (last 100).</summary>
            public async Task<List<HealthSnapshotEntity>> GetHealthSnapshotsAsync(string atmId, int limit = 100, CancellationToken ct = default)
            {
                var rows = await QueryAsync(
                    "SELECT * FROM HealthSnapshot WHERE AtmId = @AtmId ORDER BY SnapshotUtc DESC LIMIT @Limit",
                    new Dictionary<string, object?> { ["@AtmId"] = atmId, ["@Limit"] = limit }, ct).ConfigureAwait(false);
                return rows.Select(MapHealthSnapshot).ToList();
            }
    
            // ===== Maintenance =====
    
            /// <summary>Vacuum the database (reclaim space).</summary>
            public async Task VacuumAsync(CancellationToken ct = default)
            {
                await ExecuteNonQueryAsync("VACUUM;", ct: ct).ConfigureAwait(false);
            }
    
            /// <summary>Get database file size in bytes.</summary>
            public long GetDatabaseSize()
            {
                if (File.Exists(_databasePath))
                    return new FileInfo(_databasePath).Length;
                return 0;
            }
    
            /// <summary>Get row counts for all major tables (diagnostics).</summary>
            public async Task<Dictionary<string, long>> GetTableRowCountsAsync(CancellationToken ct = default)
            {
                var tables = new[] { "AtmDevice", "ClientSession", "FileManifest", "CommandQueue", "CommandAudit", "AuditLog", "HealthSnapshot", "ParserRun", "JournalFile", "UserEntity" };
                var counts = new Dictionary<string, long>();
                foreach (var table in tables)
                {
                    try
                    {
                        var result = await ExecuteScalarAsync($"SELECT COUNT(*) FROM {table}", ct: ct).ConfigureAwait(false);
                        counts[table] = Convert.ToInt64(result);
                    }
                    catch { counts[table] = 0; }
                }
                return counts;
            }
    
            // ===== Helpers =====
    
            private static void ApplyParameters(SQLiteCommand cmd, Dictionary<string, object?>? parameters)
            {
                if (parameters == null) return;
                foreach (var (key, value) in parameters)
                {
                    cmd.Parameters.AddWithValue(key, value ?? DBNull.Value);
                }
            }
    
            private static AtmDeviceEntity MapAtmDevice(Dictionary<string, object?> row)
            {
                return new AtmDeviceEntity
                {
                    AtmId = row["AtmId"]?.ToString() ?? "",
                    AtmName = row["AtmName"]?.ToString() ?? "",
                    Vendor = row["Vendor"]?.ToString() ?? "",
                    Model = row["Model"]?.ToString() ?? "",
                    TerminalId = row["TerminalId"]?.ToString() ?? "",
                    LUNO = row["LUNO"]?.ToString() ?? "",
                    Branch = row["Branch"]?.ToString() ?? "",
                    Region = row["Region"]?.ToString() ?? "",
                    Province = row["Province"]?.ToString() ?? "",
                    City = row["City"]?.ToString() ?? "",
                    Geography = row["Geography"]?.ToString() ?? "",
                    IpAddress = row["IpAddress"]?.ToString() ?? "",
                    Port = Convert.ToInt32(row["Port"] ?? 0),
                    OperationalMode = row["OperationalMode"]?.ToString() ?? "Unknown",
                    ConnectionState = row["ConnectionState"]?.ToString() ?? "Offline",
                    JournalSourcePath = row["JournalSourcePath"]?.ToString() ?? "",
                    BackupPath = row["BackupPath"]?.ToString() ?? "",
                    ImageInboxPath = row["ImageInboxPath"]?.ToString() ?? "",
                    ImageDestinationPath = row["ImageDestinationPath"]?.ToString() ?? ""
                };
            }
    
            private static HealthSnapshotEntity MapHealthSnapshot(Dictionary<string, object?> row)
            {
                return new HealthSnapshotEntity
                {
                    SnapshotId = row["SnapshotId"]?.ToString() ?? "",
                    AtmId = row["AtmId"]?.ToString() ?? "",
                    SnapshotType = row["SnapshotType"]?.ToString() ?? "Operational",
                    HealthScore = Convert.ToDouble(row["HealthScore"] ?? 0),
                    ConfidenceScore = Convert.ToDouble(row["ConfidenceScore"] ?? 0),
                    ConnectionState = row["ConnectionState"]?.ToString() ?? "Unknown",
                    OperationalMode = row["OperationalMode"]?.ToString() ?? "Unknown",
                    AgentServiceState = row["AgentServiceState"]?.ToString() ?? "Unknown",
                    ActiveAlarms = Convert.ToInt32(row["ActiveAlarms"] ?? 0),
                    Priority = row["Priority"]?.ToString() ?? "None",
                    SnapshotJson = row["SnapshotJson"]?.ToString()
                };
            }
    
            public void Dispose()
            {
                if (!_disposed)
                {
                    _writeLock.Dispose();
                    _cache.Clear();
                    _disposed = true;
                }
            }
        }
}
