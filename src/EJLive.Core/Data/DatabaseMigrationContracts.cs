using System;
using System.Collections.Generic;
using System.Linq;

namespace EJLive.Core.Data
{
    /// <summary>
    /// Represents a single versioned database migration.
    /// Each migration has an id, description, up/down SQL, and verification query.
    /// </summary>
    public sealed class DbMigration
    {
        public int MigrationId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string UpSql { get; set; } = string.Empty;
        public string? DownSql { get; set; }
        public string? VerificationQuery { get; set; }

        public override string ToString() => $"#{MigrationId:D3} — {Description}";
    }

    /// <summary>
    /// Versioned database migration runner.
    /// Ensures schema evolves deterministically through numbered migrations.
    /// Tracks applied migrations in the schema_migrations table.
    /// </summary>
    public sealed class DatabaseMigrationRunner
    {
        private readonly List<DbMigration> _pending = new List<DbMigration>();
        private readonly HashSet<int> _applied = new HashSet<int>();

        /// <summary>
        /// Registers a migration to be applied (if not already applied).
        /// </summary>
        public void Register(DbMigration migration)
        {
            if (migration == null) throw new ArgumentNullException(nameof(migration));
            if (!_applied.Contains(migration.MigrationId))
                _pending.Add(migration);
        }

        /// <summary>
        /// Marks a migration as already applied (e.g., from schema_migrations table).
        /// </summary>
        public void MarkApplied(int migrationId) => _applied.Add(migrationId);

        /// <summary>
        /// Returns all pending (unapplied) migrations, ordered by MigrationId.
        /// </summary>
        public IReadOnlyList<DbMigration> GetPending() => _pending.OrderBy(m => m.MigrationId).ToList();

        /// <summary>
        /// Clears the pending queue after successful application.
        /// </summary>
        public void CommitApplied(IEnumerable<int> migrationIds)
        {
            foreach (var id in migrationIds)
            {
                _applied.Add(id);
                _pending.RemoveAll(m => m.MigrationId == id);
            }
        }

        /// <summary>
        /// Returns true if ALL migrations have been applied.
        /// </summary>
        public bool IsUpToDate => _pending.Count == 0;

        /// <summary>
        /// Total registered migrations (applied + pending).
        /// </summary>
        public int TotalMigrationCount => _applied.Count + _pending.Count;

        /// <summary>
        /// Generates the set of required tables for EJLive.
        /// </summary>
        public static List<DbMigration> GenerateRequiredMigrations()
        {
            return new List<DbMigration>
            {
                new DbMigration
                {
                    MigrationId = 1,
                    Description = "Create schema_migrations tracking table",
                    UpSql = @"
CREATE TABLE IF NOT EXISTS schema_migrations (
    migration_id INTEGER PRIMARY KEY,
    description TEXT NOT NULL,
    applied_at_utc TEXT NOT NULL DEFAULT (datetime('now')),
    checksum TEXT
);",
                    VerificationQuery = "SELECT COUNT(*) FROM schema_migrations;"
                },
                new DbMigration
                {
                    MigrationId = 2,
                    Description = "Create ATM registry",
                    UpSql = @"
CREATE TABLE IF NOT EXISTS atm_registry (
    atm_id TEXT PRIMARY KEY,
    atm_name TEXT NOT NULL DEFAULT '',
    vendor TEXT NOT NULL DEFAULT '',
    model TEXT NOT NULL DEFAULT '',
    branch TEXT NOT NULL DEFAULT '',
    region TEXT NOT NULL DEFAULT '',
    ip_address TEXT NOT NULL DEFAULT '',
    registered_at_utc TEXT NOT NULL DEFAULT (datetime('now')),
    last_heartbeat_utc TEXT
);"
                },
                new DbMigration
                {
                    MigrationId = 3,
                    Description = "Create journal synchronization records",
                    UpSql = @"
CREATE TABLE IF NOT EXISTS sync_records (
    sync_id TEXT PRIMARY KEY,
    atm_id TEXT NOT NULL,
    file_name TEXT NOT NULL,
    file_offset INTEGER NOT NULL DEFAULT 0,
    checksum TEXT NOT NULL DEFAULT '',
    state TEXT NOT NULL DEFAULT 'Pending',
    retry_count INTEGER NOT NULL DEFAULT 0,
    progress INTEGER NOT NULL DEFAULT 0,
    created_at_utc TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at_utc TEXT NOT NULL DEFAULT (datetime('now'))
);"
                },
                new DbMigration
                {
                    MigrationId = 4,
                    Description = "Create transfer_sessions",
                    UpSql = @"
CREATE TABLE IF NOT EXISTS transfer_sessions (
    transfer_id TEXT PRIMARY KEY,
    atm_id TEXT NOT NULL,
    file_name TEXT NOT NULL,
    file_length INTEGER NOT NULL,
    chunk_size INTEGER NOT NULL,
    total_chunks INTEGER NOT NULL,
    received_chunks TEXT,
    file_sha256 TEXT,
    next_expected_offset INTEGER NOT NULL DEFAULT 0,
    created_utc TEXT NOT NULL,
    completed_utc TEXT
);"
                },
                new DbMigration
                {
                    MigrationId = 5,
                    Description = "Create journal_archive",
                    UpSql = @"
CREATE TABLE IF NOT EXISTS journal_archive (
    ingestion_id TEXT PRIMARY KEY,
    atm_id TEXT NOT NULL,
    file_name TEXT NOT NULL,
    file_size INTEGER NOT NULL,
    archive_path TEXT NOT NULL,
    sha256 TEXT,
    ingested_utc TEXT NOT NULL DEFAULT (datetime('now')),
    transaction_count INTEGER,
    stage TEXT NOT NULL DEFAULT 'Received'
);"
                },
                new DbMigration
                {
                    MigrationId = 6,
                    Description = "Create parser_transactions",
                    UpSql = @"
CREATE TABLE IF NOT EXISTS parser_transactions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    ingestion_id TEXT NOT NULL,
    transaction_number INTEGER,
    date_utc TEXT,
    atm_id TEXT NOT NULL,
    vendor TEXT NOT NULL,
    card_masked TEXT,
    amount DECIMAL,
    currency TEXT,
    stan TEXT,
    rrn TEXT,
    cass1 INTEGER,
    cass2 INTEGER,
    cass3 INTEGER,
    cass4 INTEGER,
    m_code TEXT,
    r_code TEXT,
    host_response TEXT,
    status TEXT NOT NULL,
    confidence TEXT NOT NULL,
    raw_start_line INTEGER,
    raw_end_line INTEGER,
    evidence TEXT,
    FOREIGN KEY (ingestion_id) REFERENCES journal_archive(ingestion_id)
);"
                },
                new DbMigration
                {
                    MigrationId = 7,
                    Description = "Create vendor_events + correlation_events + command_queue + command_audit + telemetry_events + client_health_snapshots",
                    UpSql = @"
CREATE TABLE IF NOT EXISTS vendor_events (
    event_id TEXT PRIMARY KEY,
    atm_id TEXT NOT NULL,
    vendor TEXT NOT NULL,
    device_class TEXT,
    code TEXT,
    message TEXT,
    severity TEXT,
    timestamp_utc TEXT,
    source_file TEXT,
    raw_line TEXT
);

CREATE TABLE IF NOT EXISTS correlation_events (
    correlation_id TEXT PRIMARY KEY,
    transaction_id TEXT NOT NULL,
    vendor_event_id TEXT NOT NULL,
    confidence TEXT NOT NULL,
    impact TEXT,
    explanation TEXT,
    created_utc TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS command_queue (
    command_id TEXT PRIMARY KEY,
    correlation_id TEXT NOT NULL,
    operator_id TEXT NOT NULL,
    role TEXT NOT NULL,
    target_atm_id TEXT NOT NULL,
    command_type TEXT NOT NULL,
    payload_json TEXT,
    signature TEXT,
    state TEXT NOT NULL DEFAULT 'Draft',
    timestamp_utc TEXT NOT NULL,
    expiry_utc TEXT NOT NULL,
    result_json TEXT,
    failure_reason TEXT,
    rollback_plan_json TEXT,
    completed_utc TEXT
);

CREATE TABLE IF NOT EXISTS command_audit (
    audit_id TEXT PRIMARY KEY,
    command_id TEXT NOT NULL,
    operator_id TEXT NOT NULL,
    action TEXT NOT NULL,
    details_json TEXT,
    timestamp_utc TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (command_id) REFERENCES command_queue(command_id)
);

CREATE TABLE IF NOT EXISTS telemetry_events (
    event_id TEXT PRIMARY KEY,
    correlation_id TEXT,
    atm_id TEXT NOT NULL,
    component TEXT NOT NULL,
    severity TEXT NOT NULL,
    message TEXT NOT NULL,
    data_json TEXT,
    timestamp_utc TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS client_health_snapshots (
    snapshot_id TEXT PRIMARY KEY,
    atm_id TEXT NOT NULL,
    agent_state TEXT NOT NULL,
    network_connected INTEGER NOT NULL,
    session_id TEXT,
    last_heartbeat_utc TEXT,
    last_sync_utc TEXT,
    outbox_count INTEGER,
    error_count INTEGER,
    last_error TEXT,
    snapshot_utc TEXT NOT NULL DEFAULT (datetime('now'))
);"
                }
            };
        }
    }
}
