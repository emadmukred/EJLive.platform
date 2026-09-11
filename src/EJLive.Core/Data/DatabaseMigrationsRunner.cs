using System.Data.SQLite;

namespace EJLive.Core.Data;

/// <summary>
/// Manages incremental database schema migrations for the EJLive SQLite database.
/// Applies migrations idempotently and tracks applied versions in a migrations table.
/// </summary>
public sealed class DatabaseMigrationsRunner
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes the migrations runner with the target database connection string.
    /// </summary>
    public DatabaseMigrationsRunner(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Opens the runner against a SQLite file. Kept as a factory rather than a
    /// `(string, bool)` overload: the boolean existed only to break the signature
    /// tie and carried no meaning at the call site.
    /// </summary>
    public static DatabaseMigrationsRunner FromDatabaseFile(string dbPath)
    {
        if (string.IsNullOrWhiteSpace(dbPath))
        {
            throw new ArgumentException("A database file path is required.", nameof(dbPath));
        }
        return new DatabaseMigrationsRunner($"Data Source={dbPath};Version=3;");
    }

    /// <summary>
    /// Runs all pending migrations up to the current version.
    /// </summary>
    /// <returns>The number of migrations applied.</returns>
    public int RunAll()
    {
        EnsureMigrationsTable();
        var applied = GetAppliedVersions();
        int count = 0;

        foreach (var migration in GetAllMigrations())
        {
            if (applied.Contains(migration.Version))
                continue;

            ApplyMigration(migration);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Gets the current schema version (highest applied migration).
    /// </summary>
    public int GetCurrentVersion()
    {
        EnsureMigrationsTable();
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT MAX(version) FROM __migrations;";
        var result = cmd.ExecuteScalar();
        return result is DBNull || result is null ? 0 : Convert.ToInt32(result);
    }

    private void EnsureMigrationsTable()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS __migrations (
                version INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                applied_at TEXT NOT NULL DEFAULT (datetime('now'))
            );
            """;
        cmd.ExecuteNonQuery();
    }

    private HashSet<int> GetAppliedVersions()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT version FROM __migrations;";
        var versions = new HashSet<int>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            versions.Add(reader.GetInt32(0));
        return versions;
    }

    private void ApplyMigration(SchemaMigration migration)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = migration.Sql;
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO __migrations (version, name) VALUES (@v, @n);";
            cmd.Parameters.AddWithValue("@v", migration.Version);
            cmd.Parameters.AddWithValue("@n", migration.Name);
            cmd.ExecuteNonQuery();

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Returns all known migrations in version order.
    /// </summary>
    private static List<SchemaMigration> GetAllMigrations() => new()
    {
        new(1, "InitialSchema", """
            CREATE TABLE IF NOT EXISTS audit_log (
                entry_id TEXT PRIMARY KEY,
                user_name TEXT NOT NULL DEFAULT '',
                action TEXT NOT NULL,
                target TEXT NOT NULL DEFAULT '',
                details TEXT NOT NULL DEFAULT '',
                created_at_utc TEXT NOT NULL DEFAULT (datetime('now'))
            );
            CREATE INDEX IF NOT EXISTS ix_audit_log_created_at ON audit_log(created_at_utc);

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
            );
            CREATE INDEX IF NOT EXISTS ix_sync_records_atm ON sync_records(atm_id);

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
                archive_id TEXT PRIMARY KEY,
                atm_id TEXT NOT NULL,
                file_name TEXT NOT NULL,
                file_size INTEGER NOT NULL DEFAULT 0,
                sha256 TEXT NOT NULL DEFAULT '',
                archived_at_utc TEXT NOT NULL DEFAULT (datetime('now')),
                storage_path TEXT NOT NULL DEFAULT ''
            );
            CREATE INDEX IF NOT EXISTS ix_journal_archive_atm ON journal_archive(atm_id);
            """),

        new(4, "AddTelemetryEvents", """
            CREATE TABLE IF NOT EXISTS telemetry_events (
                event_id TEXT PRIMARY KEY,
                atm_id TEXT NOT NULL,
                event_type TEXT NOT NULL,
                severity TEXT NOT NULL DEFAULT 'info',
                detail TEXT NOT NULL DEFAULT '',
                raw_json TEXT,
                received_at_utc TEXT NOT NULL DEFAULT (datetime('now'))
            );
            CREATE INDEX IF NOT EXISTS ix_telemetry_atm ON telemetry_events(atm_id);
            CREATE INDEX IF NOT EXISTS ix_telemetry_type ON telemetry_events(event_type);
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
            """),
    };
}

/// <summary>
/// Represents a single schema migration step.
/// </summary>
/// <param name="Version">Migration version number (monotonically increasing).</param>
/// <param name="Name">Human-readable name of the migration.</param>
/// <param name="Sql">The SQL statements to execute.</param>
public sealed record SchemaMigration(int Version, string Name, string Sql);
