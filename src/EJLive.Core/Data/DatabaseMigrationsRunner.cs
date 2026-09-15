using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace EJLive.Core.Data;

/// <summary>
/// The platform's schema book. Wave 4 (SS-11) collapsed the two books this assembly used
/// to keep — <c>__migrations</c> (this runner) and <c>schema_migrations</c> (the contract
/// in <see cref="DatabaseMigrationRunner"/>) — into the single canonical
/// <c>schema_migrations(version, name, applied_utc, checksum, rolled_back)</c> table
/// promised by the engineering contract; rows from the retired <c>__migrations</c> book
/// are backfilled and the legacy table dropped, so an upgraded database shows a complete
/// history.
///
/// Properties:
///  * forward-only, numbered; versions 1–6 are the legacy set every field database was
///    built with, versions 7–13 are the seven Phase-2 migrations from
///    <see cref="DatabaseMigrationRunner.GenerateRequiredMigrations"/>, whose SQL bodies
///    are aligned to the canonical shapes <c>DatabaseManager.CreateSchema</c> owns so the
///    two DDL sources cannot fork a table;
///  * each migration applies inside one transaction with <c>PRAGMA foreign_keys=OFF</c>
///    around the swap; the applied name is logged with its SHA-256 checksum;
///  * a database whose book contains a version beyond this binary's <see cref="LatestVersion"/>
///    <b>refuses to run</b> rather than downgrade (<see cref="TryEnsureCurrent"/>);
///  * <see cref="RunAll"/> returns the number of migrations applied this call (0 when current).
///
/// Public surface and version numbering are unchanged from the pre-Wave-4 runner; only the
/// book and the guard are new.
/// </summary>
public sealed class DatabaseMigrationsRunner
{
    /// <summary>Highest version this binary knows how to apply (legacy 6 + Phase-2 7).</summary>
    public const int LatestVersion = 13;

    private const string LegacyBook = "__migrations";

    private readonly string _connectionString;

    /// <summary>Initializes the runner with the target database connection string.</summary>
    public DatabaseMigrationsRunner(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Opens the runner against a SQLite file. Kept as a factory rather than a
    /// <c>(string, bool)</c> overload: the boolean existed only to break the signature
    /// tie and carried no meaning at the call site.
    /// </summary>
    public static DatabaseMigrationsRunner FromDatabaseFile(string dbPath)
    {
        if (string.IsNullOrWhiteSpace(dbPath))
            throw new ArgumentException("A database file path is required.", nameof(dbPath));
        return new DatabaseMigrationsRunner($"Data Source={dbPath}");
    }

    /// <summary>
    /// Reads the on-disk version and reports whether this binary can manage it.
    /// Returns false when the database carries a version beyond <see cref="LatestVersion"/>
    /// (the newer-on-disk case that must refuse startup) — never throws.
    /// </summary>
    public bool TryEnsureCurrent(out int currentVersion, out int binaryHeadVersion)
    {
        binaryHeadVersion = LatestVersion;
        try
        {
            currentVersion = GetCurrentVersion();
            return currentVersion <= LatestVersion;
        }
        catch (Exception)
        {
            // Unreadable book (missing file, first run, locked): treat as "nothing applied
            // yet"; the caller's next action (RunAll or DatabaseManager) surfaces the real error.
            currentVersion = 0;
            return true;
        }
    }

    /// <summary>
    /// Runs all pending migrations up to the current version.
    /// </summary>
    /// <returns>The number of migrations applied.</returns>
    public int RunAll()
    {
        EnsureMigrationsTable();
        var applied = GetAppliedVersions();
        var all = GetAllMigrations();

        if (applied.Count > 0 && applied.Max() > LatestVersion)
            throw new InvalidOperationException(
                $"Database schema version {applied.Max()} is newer than this binary (max {LatestVersion}); refusing to run.");

        var count = 0;
        foreach (var migration in all)
        {
            if (applied.Contains(migration.Version))
                continue;

            ApplyMigration(migration);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Gets the current schema version (highest recorded migration).
    /// </summary>
    public int GetCurrentVersion()
    {
        EnsureMigrationsTable();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT MAX(version) FROM schema_migrations;";
        var result = cmd.ExecuteScalar();
        return result is DBNull || result is null ? 0 : Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Creates the canonical book, then folds the retired <c>__migrations</c> book into it
    /// (backfill, then drop) so both histories live in one table.
    /// </summary>
    private void EnsureMigrationsTable()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using (var create = conn.CreateCommand())
        {
            create.CommandText = """
                CREATE TABLE IF NOT EXISTS schema_migrations (
                    version     INTEGER PRIMARY KEY,
                    name        TEXT NOT NULL,
                    applied_utc TEXT NOT NULL DEFAULT (datetime('now')),
                    checksum    TEXT NOT NULL DEFAULT '',
                    rolled_back INTEGER NOT NULL DEFAULT 0
                );
                """;
            create.ExecuteNonQuery();
        }

        using (var legacyExists = conn.CreateCommand())
        {
            legacyExists.CommandText =
                "SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name=$name;";
            legacyExists.Parameters.AddWithValue("$name", LegacyBook);
            var hasLegacy = Convert.ToInt64(legacyExists.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;

            if (hasLegacy)
            {
                using var fold = conn.CreateCommand();
                fold.CommandText = $"""
                    INSERT OR IGNORE INTO schema_migrations (version, name, applied_utc)
                    SELECT version, name, applied_at FROM {LegacyBook};
                    DROP TABLE {LegacyBook};
                    """;
                fold.ExecuteNonQuery();
            }
        }
    }

    private HashSet<int> GetAppliedVersions()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT version FROM schema_migrations;";
        var versions = new HashSet<int>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            versions.Add(reader.GetInt32(0));
        return versions;
    }

    private void ApplyMigration(SchemaMigration migration)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        // PRAGMA foreign_keys is a no-op inside a transaction, so it is set before the swap
        // and restored after — exactly the "OFF during the swap" rule the contract states.
        ExecutePragma(conn, "PRAGMA foreign_keys=OFF;");

        using var tx = conn.BeginTransaction();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = (SqliteTransaction)tx;
            cmd.CommandText = migration.Sql;
            cmd.ExecuteNonQuery();

            cmd.CommandText = """
                INSERT OR REPLACE INTO schema_migrations (version, name, applied_utc, checksum, rolled_back)
                VALUES ($v, $n, datetime('now'), $c, 0);
                """;
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("$v", migration.Version);
            cmd.Parameters.AddWithValue("$n", migration.Name);
            cmd.Parameters.AddWithValue("$c", ChecksumOf(migration.Sql));
            cmd.ExecuteNonQuery();

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
        finally
        {
            ExecutePragma(conn, "PRAGMA foreign_keys=ON;");
        }
    }

    private static void ExecutePragma(SqliteConnection conn, string pragma)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = pragma;
        cmd.ExecuteNonQuery();
    }

    /// <summary>SHA-256 over the migration SQL normalised for whitespace (ledger integrity).</summary>
    public static string ChecksumOf(string sql)
    {
        var normalized = string.Join(' ', sql.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Returns all known migrations in version order: the six legacy steps every field
    /// database already carries, followed by the seven Phase-2 migrations declared in the
    /// contract, renumbered 7–13.
    /// </summary>
    public static IReadOnlyList<SchemaMigration> GetAllMigrations()
    {
        var list = new List<SchemaMigration>
        {
            new(1, "InitialSchema", """
                -- Canonical shapes (SS-11): identical to DatabaseManager.CreateSchema so the
                -- migration path and the live schema can never fork a table (finding E-20).
                CREATE TABLE IF NOT EXISTS audit_log (
                    log_id        TEXT PRIMARY KEY,
                    action        TEXT NOT NULL,
                    performed_by  TEXT,
                    atm_id        TEXT,
                    details       TEXT,
                    ip_address    TEXT,
                    is_successful INTEGER NOT NULL DEFAULT 1,
                    performed_at  TEXT NOT NULL,
                    prev_hash     TEXT NOT NULL DEFAULT '',
                    payload_hash  TEXT NOT NULL DEFAULT ''
                );

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
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ux_sync_idempotency ON sync_records (atm_id, file_name, checksum);
                CREATE INDEX IF NOT EXISTS ix_sync_atm_state ON sync_records (atm_id, state);

                CREATE TABLE IF NOT EXISTS atm_registry (
                    atm_id TEXT PRIMARY KEY,
                    atm_name TEXT NOT NULL DEFAULT '',
                    atm_type TEXT NOT NULL DEFAULT 'NCR',
                    ip_address TEXT NOT NULL DEFAULT '',
                    registered_at_utc TEXT NOT NULL DEFAULT (datetime('now')),
                    last_heartbeat_utc TEXT,
                    last_data_received_utc TEXT
                );
                """),

            new(2, "AddCommandQueue", """
                CREATE TABLE IF NOT EXISTS command_queue (
                    command_id TEXT PRIMARY KEY,
                    atm_id TEXT NOT NULL,
                    command_type TEXT NOT NULL,
                    payload TEXT NOT NULL DEFAULT '',
                    state TEXT NOT NULL DEFAULT 'Pending',
                    created_at_utc TEXT NOT NULL DEFAULT (datetime('now')),
                    sent_at_utc TEXT,
                    result_at_utc TEXT,
                    result_text TEXT,
                    retry_count INTEGER NOT NULL DEFAULT 0
                );
                CREATE INDEX IF NOT EXISTS ix_command_queue_atm ON command_queue(atm_id);
                CREATE INDEX IF NOT EXISTS ix_command_queue_state ON command_queue(state);
                """),

            new(3, "AddJournalArchive", """
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
                );
                CREATE INDEX IF NOT EXISTS ix_journal_archive_atm ON journal_archive(atm_id);
                """),

            new(4, "AddTelemetryEvents", """
                CREATE TABLE IF NOT EXISTS telemetry_events (
                    telemetry_id      TEXT PRIMARY KEY,
                    atm_id            TEXT NOT NULL,
                    event_type        TEXT NOT NULL,
                    severity          TEXT NOT NULL DEFAULT 'info',
                    detail            TEXT NOT NULL DEFAULT '',
                    raw_json          TEXT,
                    reported_at_utc   TEXT NOT NULL,
                    received_at_utc   TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS ix_telemetry_atm_time ON telemetry_events(atm_id, reported_at_utc DESC);
                CREATE INDEX IF NOT EXISTS ix_telemetry_type_time ON telemetry_events(event_type, reported_at_utc DESC);
                """),

            new(5, "AddUserManagement", """
                CREATE TABLE IF NOT EXISTS users (
                    user_id TEXT PRIMARY KEY,
                    username TEXT NOT NULL UNIQUE,
                    password_hash TEXT NOT NULL,
                    role TEXT NOT NULL DEFAULT 'Observer',
                    full_name TEXT NOT NULL DEFAULT '',
                    email TEXT NOT NULL DEFAULT '',
                    is_active INTEGER NOT NULL DEFAULT 1,
                    created_at_utc TEXT NOT NULL DEFAULT (datetime('now')),
                    last_login_at_utc TEXT
                );
                """),

            new(6, "AddScreenshotHistory", """
                CREATE TABLE IF NOT EXISTS screenshot_history (
                    screenshot_id TEXT PRIMARY KEY,
                    atm_id TEXT NOT NULL,
                    captured_at_utc TEXT NOT NULL DEFAULT (datetime('now')),
                    file_path TEXT NOT NULL DEFAULT '',
                    file_size INTEGER NOT NULL DEFAULT 0,
                    success INTEGER NOT NULL DEFAULT 1
                );
                CREATE INDEX IF NOT EXISTS ix_screenshot_atm ON screenshot_history(atm_id);
                """)
        };

        // Phase-2 canonical set: the contract owns the SQL (single DDL source for both the
        // ledger and any tooling that wants it); the runner applies it renumbered 7–13.
        var contract = DatabaseMigrationRunner.GenerateRequiredMigrations();
        foreach (var migration in contract)
            list.Add(new SchemaMigration(migration.MigrationId + 6, migration.Description, migration.UpSql));

        return list;
    }
}

/// <summary>
/// Represents a single schema migration step.
/// </summary>
/// <param name="Version">Migration version number (monotonically increasing).</param>
/// <param name="Name">Human-readable name of the migration.</param>
/// <param name="Sql">The SQL statements to execute.</param>
public sealed record SchemaMigration(int Version, string Name, string Sql);
