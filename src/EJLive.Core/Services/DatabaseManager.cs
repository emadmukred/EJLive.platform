using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Core.Services
{
    /// <summary>
    /// مدير قاعدة البيانات الكامل — SQLite WAL Mode
    /// يطبق: A-04 (Monthly Partitions), D-04 (Audit Log), L-01 (Persistent State)
    ///        L-03 (Idempotency via Unique Constraint), A-08 (Pre-Aggregated Stats)
    /// مسار قاعدة البيانات الافتراضي: %PROGRAMDATA%\EJLive\Data\ejlive.db
    /// </summary>
    public sealed class DatabaseManager
    {
        #region Singleton
        private static DatabaseManager? _instance;
        private static readonly object _lock = new object();
        public static DatabaseManager Instance
        {
            get
            {
                lock (_lock)
                    return _instance ??= new DatabaseManager();
            }
        }
        #endregion

        private string _connStr = string.Empty;
        private string _databasePath = AppConstants.DefaultDatabasePath;
        private bool   _initialized;
        private readonly object _queryLock = new object();
        public bool IsInitialized => _initialized;

        // ==========================================
        // التهيئة وبناء المخطط
        // ==========================================

        public void Initialize(string? dbPath = null)
        {
            if (_initialized) return;
            dbPath = dbPath ?? AppConstants.DefaultDatabasePath;
            _databasePath = dbPath;
            var dir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _connStr     = $"Data Source={dbPath};Version=3;Journal Mode=WAL;Cache Size=4000;Synchronous=Normal;BusyTimeout=5000;Default Timeout=5;";
            _initialized = true;
            CreateSchema();
            AppLogger.Instance.Info($"Database initialized: {dbPath}", "DB");
        }

        public SQLiteConnection CreateConnection()
        {
            EnsureInitialized();
            return new SQLiteConnection(_connStr);
        }

        public int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            EnsureInitialized();
            return ExecuteSingleWithResult(sql, parameters);
        }

        public DataTable Query(string sql, params SQLiteParameter[] parameters)
        {
            EnsureInitialized();
            return QueryTable(sql, parameters);
        }

        private void CreateSchema()
        {
            // نفّذ كل PRAGMA منفردًا ثم كل CREATE TABLE منفردًا
            var pragmas = new[]
            {
                "PRAGMA journal_mode=WAL",
                "PRAGMA synchronous=NORMAL",
                "PRAGMA cache_size=4000",
                "PRAGMA temp_store=MEMORY",
                "PRAGMA wal_autocheckpoint=1000"
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
CREATE TABLE IF NOT EXISTS telemetry_events (
    telemetry_id      TEXT PRIMARY KEY,
    atm_id            TEXT NOT NULL,
    event_type        TEXT NOT NULL,
    severity          TEXT NOT NULL DEFAULT 'info',
    detail            TEXT NOT NULL DEFAULT '',
    raw_json          TEXT,
    reported_at_utc   TEXT NOT NULL,
    received_at_utc   TEXT NOT NULL
)");

            ExecuteSchema(@"
CREATE TABLE IF NOT EXISTS client_outbox (
    item_id            TEXT PRIMARY KEY,
    atm_id             TEXT NOT NULL,
    file_name          TEXT NOT NULL,
    payload_path       TEXT NOT NULL,
    payload_size       INTEGER NOT NULL DEFAULT 0,
    file_offset        INTEGER NOT NULL DEFAULT 0,
    checksum           TEXT NOT NULL DEFAULT '',
    retry_count        INTEGER NOT NULL DEFAULT 0,
    status             TEXT NOT NULL DEFAULT 'pending',
    next_attempt_utc   TEXT NOT NULL DEFAULT '',
    last_sent_utc      TEXT NOT NULL DEFAULT '',
    ack_deadline_utc   TEXT NOT NULL DEFAULT '',
    last_ack_detail    TEXT NOT NULL DEFAULT '',
    created_at_utc     TEXT NOT NULL,
    updated_at_utc     TEXT NOT NULL
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
            ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_telemetry_atm_time ON telemetry_events (atm_id, reported_at_utc DESC)");
            ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_telemetry_type_time ON telemetry_events (event_type, reported_at_utc DESC)");
            ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_client_outbox_ready ON client_outbox (status, next_attempt_utc)");
            ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_client_outbox_atm ON client_outbox (atm_id, created_at_utc DESC)");
            ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_stats_atm_date ON daily_stats  (atm_id, stat_date DESC)");

            // Compatibility columns preserved from the previous unified runtime contract.
            ExecuteSchema("ALTER TABLE audit_log ADD COLUMN entry_id TEXT");
            ExecuteSchema("ALTER TABLE audit_log ADD COLUMN user_name TEXT NOT NULL DEFAULT ''");
            ExecuteSchema("ALTER TABLE audit_log ADD COLUMN target TEXT NOT NULL DEFAULT ''");
            ExecuteSchema("ALTER TABLE audit_log ADD COLUMN created_at_utc TEXT NOT NULL DEFAULT ''");
            ExecuteSchema("ALTER TABLE audit_log ADD COLUMN details TEXT NOT NULL DEFAULT ''");
            ExecuteSchema("ALTER TABLE sync_records ADD COLUMN updated_at_utc TEXT NOT NULL DEFAULT ''");
            ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_audit_log_created_at ON audit_log(created_at_utc)");
            ExecuteSchema("CREATE INDEX IF NOT EXISTS ix_sync_records_updated ON sync_records(updated_at_utc)");
        }

        // ==========================================
        // journal_archive
        // ==========================================

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

        public List<JournalEntry> SearchArchive(string? atmId, DateTime? from = null, DateTime? to = null,
                                                 string? keyword = null, int maxRows = 1000)
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
                    EntryId          = r["entry_id"]?.ToString() ?? string.Empty,
                    ATMId            = r["atm_id"]?.ToString() ?? string.Empty,
                    FileName         = r["file_name"]?.ToString() ?? string.Empty,
                    OriginalSize     = Convert.ToInt64(r["original_size"]),
                    CompressedSize   = Convert.ToInt64(r["compressed_size"]),
                    EncryptedSize    = Convert.ToInt64(r["encrypted_size"]),
                    Checksum         = r["checksum"]?.ToString() ?? string.Empty,
                    MD5Hash          = r["md5_hash"]?.ToString() ?? string.Empty,
                    SHA256Hash       = r["sha256_hash"]?.ToString() ?? string.Empty,
                    TransactionCount = Convert.ToInt32(r["transaction_count"]),
                    ArchivePath      = r["archive_path"]?.ToString() ?? string.Empty,
                    MonthPartition   = r["month_partition"]?.ToString() ?? string.Empty,
                    ReceivedAt       = DateTime.TryParse(r["received_at"]?.ToString(), out var dt) ? dt : DateTime.MinValue
                });
            }, parms.ToArray());
            return result;
        }

        // ==========================================
        // sync_records — Idempotency + State
        // ==========================================

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
                    SyncId          = r["sync_id"]?.ToString() ?? string.Empty,
                    ATM_ID          = r["atm_id"]?.ToString() ?? string.Empty,
                    FileName        = r["file_name"]?.ToString() ?? string.Empty,
                    FileSize        = Convert.ToInt64(r["file_size"]),
                    FileOffset      = Convert.ToInt64(r["file_offset"]),
                    Checksum        = r["checksum"]?.ToString() ?? string.Empty,
                    MD5Hash         = r["md5_hash"]?.ToString() ?? string.Empty,
                    SHA256Hash      = r["sha256_hash"]?.ToString() ?? string.Empty,
                    State           = (JournalSyncState)Convert.ToInt32(r["state"]),
                    ProgressPercent = Convert.ToInt32(r["progress_percent"]),
                    RetryCount      = Convert.ToInt32(r["retry_count"]),
                    LocalPath       = r["local_path"]?.ToString() ?? string.Empty,
                    Message         = r["message"]?.ToString() ?? string.Empty
                });
            }, P("@atm", atmId));
            return result;
        }

        // ==========================================
        // audit_log (S-03, D-04)
        // ==========================================

        public void InsertAuditLog(string action, string? performedBy, string? atmId, string? details)
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

        public void InsertTelemetryEvent(
            string? atmId,
            string? eventType,
            string? severity,
            string? detail,
            DateTime reportedAtUtc,
            string? rawJson = null)
        {
            var normalizedAtm = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
            var normalizedEvent = string.IsNullOrWhiteSpace(eventType) ? "event" : eventType.Trim();
            var normalizedSeverity = string.IsNullOrWhiteSpace(severity) ? "info" : severity.Trim().ToLowerInvariant();
            var eventUtc = reportedAtUtc == DateTime.MinValue ? DateTime.UtcNow : reportedAtUtc.ToUniversalTime();

            ExecuteSingle(@"
INSERT INTO telemetry_events
(telemetry_id, atm_id, event_type, severity, detail, raw_json, reported_at_utc, received_at_utc)
VALUES (@id,@atm,@evt,@sev,@det,@raw,@reported,@received)",
                P("@id", Guid.NewGuid().ToString("N")),
                P("@atm", normalizedAtm),
                P("@evt", normalizedEvent),
                P("@sev", normalizedSeverity),
                P("@det", detail ?? string.Empty),
                P("@raw", rawJson),
                P("@reported", eventUtc.ToString("o")),
                P("@received", DateTime.UtcNow.ToString("o")));
        }

        public void UpsertClientOutboxItem(
            string itemId,
            string atmId,
            string fileName,
            string payloadPath,
            long payloadSize,
            long fileOffset,
            string checksum,
            int retryCount,
            string status,
            DateTime nextAttemptUtc,
            DateTime createdAtUtc,
            DateTime updatedAtUtc,
            DateTime? lastSentUtc = null,
            DateTime? ackDeadlineUtc = null,
            string? lastAckDetail = null)
        {
            var normalizedStatus = string.IsNullOrWhiteSpace(status) ? "pending" : status.Trim().ToLowerInvariant();
            var normalizedAtm = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
            var normalizedFile = string.IsNullOrWhiteSpace(fileName) ? "unknown.bin" : fileName.Trim();
            var normalizedPayloadPath = string.IsNullOrWhiteSpace(payloadPath) ? string.Empty : payloadPath.Trim();
            var normalizedChecksum = checksum ?? string.Empty;

            ExecuteSingleTransactional(@"
INSERT INTO client_outbox
(item_id, atm_id, file_name, payload_path, payload_size, file_offset, checksum, retry_count, status,
  next_attempt_utc, last_sent_utc, ack_deadline_utc, last_ack_detail, created_at_utc, updated_at_utc)
VALUES
(@id,@atm,@file,@path,@size,@offset,@checksum,@retry,@status,@next,@sent,@ack_deadline,@ack_detail,@created,@updated)
ON CONFLICT(item_id) DO UPDATE SET
    atm_id=@atm,
    file_name=@file,
    payload_path=@path,
    payload_size=@size,
    file_offset=@offset,
    checksum=@checksum,
    retry_count=@retry,
    status=@status,
    next_attempt_utc=@next,
    last_sent_utc=@sent,
    ack_deadline_utc=@ack_deadline,
    last_ack_detail=@ack_detail,
    updated_at_utc=@updated",
                P("@id", itemId),
                P("@atm", normalizedAtm),
                P("@file", normalizedFile),
                P("@path", normalizedPayloadPath),
                P("@size", Math.Max(0, payloadSize)),
                P("@offset", Math.Max(0, fileOffset)),
                P("@checksum", normalizedChecksum),
                P("@retry", Math.Max(0, retryCount)),
                P("@status", normalizedStatus),
                P("@next", nextAttemptUtc.ToUniversalTime().ToString("o")),
                P("@sent", lastSentUtc.HasValue ? lastSentUtc.Value.ToUniversalTime().ToString("o") : string.Empty),
                P("@ack_deadline", ackDeadlineUtc.HasValue ? ackDeadlineUtc.Value.ToUniversalTime().ToString("o") : string.Empty),
                P("@ack_detail", lastAckDetail ?? string.Empty),
                P("@created", createdAtUtc.ToUniversalTime().ToString("o")),
                P("@updated", updatedAtUtc.ToUniversalTime().ToString("o")));
        }

        public void DeleteClientOutboxItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return;

            ExecuteSingleTransactional("DELETE FROM client_outbox WHERE item_id=@id", P("@id", itemId.Trim()));
        }

        public List<ClientOutboxRow> GetClientOutboxItems(string? atmId = null, int maxRows = 5000)
        {
            var rows = new List<ClientOutboxRow>();
            var sql = @"
SELECT item_id, atm_id, file_name, payload_path, payload_size, file_offset, checksum, retry_count, status,
       next_attempt_utc, last_sent_utc, ack_deadline_utc, last_ack_detail, created_at_utc, updated_at_utc
FROM client_outbox
WHERE 1=1";
            var parameters = new List<SQLiteParameter>();
            if (!string.IsNullOrWhiteSpace(atmId))
            {
                sql += " AND atm_id=@atm";
                parameters.Add(P("@atm", atmId.Trim()));
            }

            sql += " ORDER BY created_at_utc ASC";
            sql += $" LIMIT {Math.Max(1, maxRows)}";

            Query(sql, reader =>
            {
                rows.Add(new ClientOutboxRow(
                    ItemId: reader["item_id"]?.ToString() ?? string.Empty,
                    ATM_ID: reader["atm_id"]?.ToString() ?? string.Empty,
                    FileName: reader["file_name"]?.ToString() ?? string.Empty,
                    PayloadPath: reader["payload_path"]?.ToString() ?? string.Empty,
                    PayloadSize: Convert.ToInt64(reader["payload_size"]),
                    FileOffset: Convert.ToInt64(reader["file_offset"]),
                    Checksum: reader["checksum"]?.ToString() ?? string.Empty,
                    RetryCount: Convert.ToInt32(reader["retry_count"]),
                    Status: reader["status"]?.ToString() ?? "pending",
                    NextAttemptUtc: ParseUtcOrNow(reader["next_attempt_utc"]?.ToString()),
                    LastSentUtc: ParseUtcOrNull(reader["last_sent_utc"]?.ToString()),
                    AckDeadlineUtc: ParseUtcOrNull(reader["ack_deadline_utc"]?.ToString()),
                    LastAckDetail: reader["last_ack_detail"]?.ToString() ?? string.Empty,
                    CreatedAtUtc: ParseUtcOrNow(reader["created_at_utc"]?.ToString()),
                    UpdatedAtUtc: ParseUtcOrNow(reader["updated_at_utc"]?.ToString())));
            }, parameters.ToArray());

            return rows;
        }

        private static DateTime ParseUtcOrNow(string? value)
        {
            if (DateTime.TryParse(value, out var parsed))
                return parsed.ToUniversalTime();
            return DateTime.UtcNow;
        }

        private static DateTime? ParseUtcOrNull(string? value)
        {
            if (DateTime.TryParse(value, out var parsed))
                return parsed.ToUniversalTime();
            return null;
        }

        public DataTable GetAuditLog(string? atmId, DateTime? from = null, DateTime? to = null, int maxRows = 1000)
        {
            var sql   = "SELECT log_id, action, performed_by, atm_id, details, performed_at FROM audit_log WHERE 1=1";
            var parms = new List<SQLiteParameter>();
            if (!string.IsNullOrEmpty(atmId)) { sql += " AND atm_id=@atm";   parms.Add(P("@atm",  atmId)); }
            if (from.HasValue) { sql += " AND performed_at>=@from"; parms.Add(P("@from", from.Value.ToString("o"))); }
            if (to.HasValue)   { sql += " AND performed_at<=@to";   parms.Add(P("@to",   to.Value.AddDays(1).ToString("o"))); }
            sql += $" ORDER BY performed_at DESC LIMIT {maxRows}";
            return QueryTable(sql, parms.ToArray());
        }

        // ==========================================
        // daily_stats (A-08: Pre-Aggregated)
        // ==========================================

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

        // ==========================================
        // دوال SQLite المساعدة الداخلية
        // ==========================================

        /// <summary>تنفيذ جملة SQL واحدة بدون parameters (مناسب للـ Schema / DDL)</summary>
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
                        !ex.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase) &&
                        !ex.Message.Contains("UNIQUE") &&
                        !ex.Message.Contains("no such"))
                        AppLogger.Instance.Warning($"Schema warning: {ex.Message}", "DB");
                }
            }
        }

        /// <summary>تنفيذ جملة DML واحدة مع parameters</summary>
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

        /// <summary>
        /// Executes a single DML statement inside an explicit SQLite transaction.
        /// Used for durability-critical journal/outbox updates.
        /// </summary>
        private void ExecuteSingleTransactional(string sql, params SQLiteParameter[] parms)
        {
            if (!_initialized) return;
            lock (_queryLock)
            {
                using var conn = new SQLiteConnection(_connStr);
                conn.Open();
                using var tx = conn.BeginTransaction(IsolationLevel.Serializable);
                using var cmd = new SQLiteCommand(sql, conn, tx);
                if (parms != null) cmd.Parameters.AddRange(parms);

                try
                {
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
                catch (SQLiteException ex)
                {
                    try { tx.Rollback(); } catch { }
                    if (!ex.Message.Contains("UNIQUE"))
                    {
                        AppLogger.Instance.Error($"DB Error: {ex.Message}", "DB");
                        throw;
                    }
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
                    throw;
                }
            }
        }

        private int ExecuteSingleWithResult(string sql, params SQLiteParameter[] parms)
        {
            if (!_initialized) return 0;
            lock (_queryLock)
            {
                using var conn = new SQLiteConnection(_connStr);
                conn.Open();
                using var cmd = new SQLiteCommand(sql, conn);
                if (parms != null) cmd.Parameters.AddRange(parms);
                return cmd.ExecuteNonQuery();
            }
        }

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

        private T QueryScalar<T>(string sql, params SQLiteParameter[] parms)
        {
            if (!_initialized) return default!;
            lock (_queryLock)
            {
                using var conn = new SQLiteConnection(_connStr);
                conn.Open();
                using var cmd  = new SQLiteCommand(sql, conn);
                if (parms != null) cmd.Parameters.AddRange(parms);
                var result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value) return default!;
                return (T)Convert.ChangeType(result, typeof(T));
            }
        }

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

        private static SQLiteParameter P(string name, object? value)
            => new SQLiteParameter(name, value ?? (object)DBNull.Value);

        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            Initialize(_databasePath);
        }
    }

    public sealed record ClientOutboxRow(
        string ItemId,
        string ATM_ID,
        string FileName,
        string PayloadPath,
        long PayloadSize,
        long FileOffset,
        string Checksum,
        int RetryCount,
        string Status,
        DateTime NextAttemptUtc,
        DateTime? LastSentUtc,
        DateTime? AckDeadlineUtc,
        string LastAckDetail,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);
}
