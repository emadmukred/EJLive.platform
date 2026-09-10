using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

namespace EJLive.Core.Data
{
    /// <summary>
    /// SQLite-backed offset store for journal file tracking.
    /// Supports NCR overwrite strategy and GRG/WN daily rollover.
    /// </summary>
    public sealed class JournalOffsetStore : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly string _dbPath;

        public JournalOffsetStore(string databasePath)
        {
            _dbPath = databasePath ?? throw new ArgumentNullException(nameof(databasePath));
            var dir = Path.GetDirectoryName(_dbPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            _connection = new SqliteConnection($"Data Source={_dbPath};");
            _connection.Open();
            InitializeSchema();
        }

        private void InitializeSchema()
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS journal_offsets (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    atm_id TEXT NOT NULL,
                    file_path TEXT NOT NULL,
                    file_identity TEXT NOT NULL,
                    vendor_strategy TEXT NOT NULL,
                    last_offset INTEGER NOT NULL DEFAULT 0,
                    last_line INTEGER NOT NULL DEFAULT 0,
                    checksum TEXT,
                    last_seen_utc TEXT NOT NULL,
                    UNIQUE(atm_id, file_path, file_identity)
                );
                CREATE INDEX IF NOT EXISTS idx_offsets_atm ON journal_offsets(atm_id);
                CREATE INDEX IF NOT EXISTS idx_offsets_file ON journal_offsets(file_path);
            ";
            cmd.ExecuteNonQuery();
        }

        public void SaveOffset(string atmId, string filePath, string fileIdentity, string vendorStrategy, long offset, long line, string? checksum)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO journal_offsets (atm_id, file_path, file_identity, vendor_strategy, last_offset, last_line, checksum, last_seen_utc)
                VALUES ($atmId, $filePath, $fileIdentity, $vendorStrategy, $offset, $line, $checksum, $seen)
                ON CONFLICT(atm_id, file_path, file_identity) DO UPDATE SET
                    last_offset = excluded.last_offset,
                    last_line = excluded.last_line,
                    checksum = excluded.checksum,
                    last_seen_utc = excluded.last_seen_utc;
            ";
            cmd.Parameters.AddWithValue("$atmId", atmId);
            cmd.Parameters.AddWithValue("$filePath", filePath);
            cmd.Parameters.AddWithValue("$fileIdentity", fileIdentity);
            cmd.Parameters.AddWithValue("$vendorStrategy", vendorStrategy);
            cmd.Parameters.AddWithValue("$offset", offset);
            cmd.Parameters.AddWithValue("$line", line);
            cmd.Parameters.AddWithValue("$checksum", checksum ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$seen", DateTime.UtcNow.ToString("O"));
            cmd.ExecuteNonQuery();
        }

        public JournalOffsetRecord? GetOffset(string atmId, string filePath, string fileIdentity)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                SELECT last_offset, last_line, checksum, last_seen_utc
                FROM journal_offsets
                WHERE atm_id = $atmId AND file_path = $filePath AND file_identity = $fileIdentity
                LIMIT 1;
            ";
            cmd.Parameters.AddWithValue("$atmId", atmId);
            cmd.Parameters.AddWithValue("$filePath", filePath);
            cmd.Parameters.AddWithValue("$fileIdentity", fileIdentity);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new JournalOffsetRecord(
                    reader.GetInt64(0),
                    reader.GetInt64(1),
                    reader.IsDBNull(2) ? null : reader.GetString(2),
                    reader.GetString(3));
            }
            return null;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }

    public sealed record JournalOffsetRecord(
        long LastOffset,
        long LastLine,
        string? Checksum,
        string LastSeenUtc);
}
