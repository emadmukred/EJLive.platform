using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace EJLive.Core.Engine
{
    // Class: DatabaseManager (from 1 sources)
        public sealed partial class DatabaseManager : IDisposable
        {
            // --- Constants & Fields ---
                    private static readonly Lazy<DatabaseManager> _instance = new(() => new DatabaseManager());
    
                    private readonly string _connectionString;
    
                    private SQLiteConnection? _connection;
    
    
            // --- Properties ---
                    public static DatabaseManager Instance => _instance.Value;
    
                    public void UpsertClientOutboxItem(
                        string itemId, string atmId, string fileName,
                        string payloadPath, long payloadSize, long fileOffset,
                        string checksum, int retryCount, string status,
                        DateTime nextAttemptUtc, DateTime createdAtUtc, DateTime updatedAtUtc,
                        DateTime? lastSentUtc, DateTime? ackDeadlineUtc, string lastAckDetail)
                    {
                        ExecuteNonQuery(
                            @"INSERT OR REPLACE INTO client_outbox
                                (item_id, atm_id, file_name, payload_path, payload_size, file_offset,
                                checksum, retry_count, status, next_attempt_utc, created_at_utc,
                                updated_at_utc, last_sent_utc, ack_deadline_utc, last_ack_detail)
                                VALUES (@itemId, @atmId, @fileName, @payloadPath, @payloadSize, @fileOffset,
                                @checksum, @retryCount, @status, @nextAttemptUtc, @createdAtUtc,
                                @updatedAtUtc, @lastSentUtc, @ackDeadlineUtc, @lastAckDetail)",
                            new Dictionary<string, object>
                            {
                                ["@itemId"] = itemId, ["@atmId"] = atmId, ["@fileName"] = fileName,
                                ["@payloadPath"] = payloadPath, ["@payloadSize"] = payloadSize,
                                ["@fileOffset"] = fileOffset, ["@checksum"] = checksum,
                                ["@retryCount"] = retryCount, ["@status"] = status,
                                ["@nextAttemptUtc"] = nextAttemptUtc.ToString("O"),
                                ["@createdAtUtc"] = createdAtUtc.ToString("O"),
                                ["@updatedAtUtc"] = updatedAtUtc.ToString("O"),
                                ["@lastSentUtc"] = (lastSentUtc?.ToString("O")) ?? (object)DBNull.Value,
                                ["@ackDeadlineUtc"] = (ackDeadlineUtc?.ToString("O")) ?? (object)DBNull.Value,
                                ["@lastAckDetail"] = lastAckDetail ?? string.Empty
                            });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\DatabaseManagerStub.cs
                    public void InsertAuditLog(string action, string userId, string? target, string details) { }
    
    
            // --- Constructors ---
                    public DatabaseManager(string? databasePath = null)
                    {
                        var path = databasePath
                            ?? Environment.GetEnvironmentVariable("EJLIVE_DATABASE_PATH")
                            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Data", "ejlive.db");
                        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                        _connectionString = $"Data Source={path};Version=3;";
                    }
    
    
            // --- Methods ---
                    public SQLiteConnection GetConnection()
                    {
                        if (_connection == null || _connection.State != ConnectionState.Open)
                        {
                            _connection = new SQLiteConnection(_connectionString);
                            _connection.Open();
                        }
                        return _connection;
                    }
    
                    public void DeleteClientOutboxItem(string itemId)
                    {
                        ExecuteNonQuery("DELETE FROM client_outbox WHERE item_id = @itemId",
                            new Dictionary<string, object> { ["@itemId"] = itemId });
                    }
    
                    public List<ClientOutboxRow> GetClientOutboxItems(int maxRows)
                    {
                        var rows = ExecuteQuery($"SELECT * FROM client_outbox ORDER BY created_at DESC LIMIT {maxRows}");
                        return rows.Select(r => new ClientOutboxRow
                        {
                            ItemId = SafeStr(r, "item_id"), ATM_ID = SafeStr(r, "atm_id"),
                            FileName = SafeStr(r, "file_name"), PayloadPath = SafeStr(r, "payload_path"),
                            PayloadSize = SafeLong(r, "payload_size"), FileOffset = SafeLong(r, "file_offset"),
                            Checksum = SafeStr(r, "checksum"), RetryCount = SafeInt(r, "retry_count"),
                            Status = SafeStr(r, "status"),
                            NextAttemptUtc = SafeDateTime(r, "next_attempt_utc"),
                            LastSentUtc = SafeDateTimeNullable(r, "last_sent_utc"),
                            AckDeadlineUtc = SafeDateTimeNullable(r, "ack_deadline_utc"),
                            LastAckDetail = SafeStr(r, "last_ack_detail"),
                            CreatedAtUtc = SafeDateTime(r, "created_at_utc"),
                            UpdatedAtUtc = SafeDateTime(r, "updated_at_utc")
                        }).ToList();
                    }
    
                    private static string SafeStr(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) ? v?.ToString() ?? "" : "";
    
                    private static long SafeLong(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is long l ? l : 0;
    
                    private static int SafeInt(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is int i ? i : 0;
    
                    private static DateTime SafeDateTime(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is DateTime dt ? dt : DateTime.UtcNow;
    
                    private static DateTime? SafeDateTimeNullable(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is DateTime dt ? dt : null;
    
                    public void ExecuteNonQuery(string sql, Dictionary<string, object>? parameters = null)
                    {
                        using var cmd = new SQLiteCommand(sql, GetConnection());
                        if (parameters != null)
                            foreach (var kvp in parameters)
                                cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
    
                    public object? ExecuteScalar(string sql, Dictionary<string, object>? parameters = null)
                    {
                        using var cmd = new SQLiteCommand(sql, GetConnection());
                        if (parameters != null)
                            foreach (var kvp in parameters)
                                cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                        return cmd.ExecuteScalar();
                    }
    
                    public List<Dictionary<string, object>> ExecuteQuery(string sql, Dictionary<string, object>? parameters = null)
                    {
                        var results = new List<Dictionary<string, object>>();
                        using var cmd = new SQLiteCommand(sql, GetConnection());
                        if (parameters != null)
                            foreach (var kvp in parameters)
                                cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                        using var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                            for (int i = 0; i < reader.FieldCount; i++)
                                row[reader.GetName(i)] = reader.GetValue(i);
                            results.Add(row);
                        }
                        return results;
                    }
    
                    public bool IsDuplicateSync(string atmId, string fileName, string checksum) => false;
    
                    public void Dispose()
                    {
                        _connection?.Close();
                        _connection?.Dispose();
                        _connection = null;
                    }
    
    
        }
    public partial class DatabaseManager : IDisposable
        {
            private static readonly Lazy<DatabaseManager> _instance = new(() => new DatabaseManager());
    
    
            private readonly string _connectionString;
    
    
            private SQLiteConnection? _connection;
    
    
            public static DatabaseManager Instance => _instance.Value;
    
    
            public void UpsertClientOutboxItem(
                string itemId, string atmId, string fileName,
                string payloadPath, long payloadSize, long fileOffset,
                string checksum, int retryCount, string status,
                DateTime nextAttemptUtc, DateTime createdAtUtc, DateTime updatedAtUtc,
                DateTime? lastSentUtc, DateTime? ackDeadlineUtc, string lastAckDetail)
            {
                ExecuteNonQuery(
                    @"INSERT OR REPLACE INTO client_outbox
                        (item_id, atm_id, file_name, payload_path, payload_size, file_offset,
                        checksum, retry_count, status, next_attempt_utc, created_at_utc,
                        updated_at_utc, last_sent_utc, ack_deadline_utc, last_ack_detail)
                        VALUES (@itemId, @atmId, @fileName, @payloadPath, @payloadSize, @fileOffset,
                        @checksum, @retryCount, @status, @nextAttemptUtc, @createdAtUtc,
                        @updatedAtUtc, @lastSentUtc, @ackDeadlineUtc, @lastAckDetail)",
                    new Dictionary<string, object>
                    {
                        ["@itemId"] = itemId, ["@atmId"] = atmId, ["@fileName"] = fileName,
                        ["@payloadPath"] = payloadPath, ["@payloadSize"] = payloadSize,
                        ["@fileOffset"] = fileOffset, ["@checksum"] = checksum,
                        ["@retryCount"] = retryCount, ["@status"] = status,
                        ["@nextAttemptUtc"] = nextAttemptUtc.ToString("O"),
                        ["@createdAtUtc"] = createdAtUtc.ToString("O"),
                        ["@updatedAtUtc"] = updatedAtUtc.ToString("O"),
                        ["@lastSentUtc"] = (lastSentUtc?.ToString("O")) ?? (object)DBNull.Value,
                        ["@ackDeadlineUtc"] = (ackDeadlineUtc?.ToString("O")) ?? (object)DBNull.Value,
                        ["@lastAckDetail"] = lastAckDetail ?? string.Empty
                    });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\DatabaseManagerStub.cs
            public void InsertAuditLog(string action, string userId, string? target, string details) { }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\DatabaseManagerStub.cs
            public void InsertAuditLog(string action, string userId, string? target, string details) { }
    
    
            public DatabaseManager(string? databasePath = null)
            {
                var path = databasePath
                    ?? Environment.GetEnvironmentVariable("EJLIVE_DATABASE_PATH")
                    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Data", "ejlive.db");
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                _connectionString = $"Data Source={path};Version=3;";
            }
    
    
            public SQLiteConnection GetConnection()
            {
                if (_connection == null || _connection.State != ConnectionState.Open)
                {
                    _connection = new SQLiteConnection(_connectionString);
                    _connection.Open();
                }
                return _connection;
            }
    
    
            public void DeleteClientOutboxItem(string itemId)
            {
                ExecuteNonQuery("DELETE FROM client_outbox WHERE item_id = @itemId",
                    new Dictionary<string, object> { ["@itemId"] = itemId });
            }
    
    
            public List<ClientOutboxRow> GetClientOutboxItems(int maxRows)
            {
                var rows = ExecuteQuery($"SELECT * FROM client_outbox ORDER BY created_at DESC LIMIT {maxRows}");
                return rows.Select(r => new ClientOutboxRow
                {
                    ItemId = SafeStr(r, "item_id"), ATM_ID = SafeStr(r, "atm_id"),
                    FileName = SafeStr(r, "file_name"), PayloadPath = SafeStr(r, "payload_path"),
                    PayloadSize = SafeLong(r, "payload_size"), FileOffset = SafeLong(r, "file_offset"),
                    Checksum = SafeStr(r, "checksum"), RetryCount = SafeInt(r, "retry_count"),
                    Status = SafeStr(r, "status"),
                    NextAttemptUtc = SafeDateTime(r, "next_attempt_utc"),
                    LastSentUtc = SafeDateTimeNullable(r, "last_sent_utc"),
                    AckDeadlineUtc = SafeDateTimeNullable(r, "ack_deadline_utc"),
                    LastAckDetail = SafeStr(r, "last_ack_detail"),
                    CreatedAtUtc = SafeDateTime(r, "created_at_utc"),
                    UpdatedAtUtc = SafeDateTime(r, "updated_at_utc")
                }).ToList();
            }
    
    
            private static string SafeStr(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) ? v?.ToString() ?? "" : "";
    
    
            private static long SafeLong(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is long l ? l : 0;
    
    
            private static int SafeInt(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is int i ? i : 0;
    
    
            private static DateTime SafeDateTime(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is DateTime dt ? dt : DateTime.UtcNow;
    
    
            private static DateTime? SafeDateTimeNullable(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is DateTime dt ? dt : null;
    
    
            public void ExecuteNonQuery(string sql, Dictionary<string, object>? parameters = null)
            {
                using var cmd = new SQLiteCommand(sql, GetConnection());
                if (parameters != null)
                    foreach (var kvp in parameters)
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
    
    
            public object? ExecuteScalar(string sql, Dictionary<string, object>? parameters = null)
            {
                using var cmd = new SQLiteCommand(sql, GetConnection());
                if (parameters != null)
                    foreach (var kvp in parameters)
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                return cmd.ExecuteScalar();
            }
    
    
            public List<Dictionary<string, object>> ExecuteQuery(string sql, Dictionary<string, object>? parameters = null)
            {
                var results = new List<Dictionary<string, object>>();
                using var cmd = new SQLiteCommand(sql, GetConnection());
                if (parameters != null)
                    foreach (var kvp in parameters)
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < reader.FieldCount; i++)
                        row[reader.GetName(i)] = reader.GetValue(i);
                    results.Add(row);
                }
                return results;
            }
    
    
            public bool IsDuplicateSync(string atmId, string fileName, string checksum) => false;
    
    
            public void Dispose()
            {
                _connection?.Close();
                _connection?.Dispose();
                _connection = null;
            }
    
    
        }
    /// <summary>
        /// Stub DatabaseManager for build baseline. Provides surface area for
        /// JournalOutbox and other engines. Replace with full implementation
        /// when migration infrastructure is ready.
        /// </summary>
        public sealed class DatabaseManager : IDisposable
        {
            private static readonly Lazy<DatabaseManager> _instance = new(() => new DatabaseManager());
            public static DatabaseManager Instance => _instance.Value;
    
            private readonly string _connectionString;
            private SQLiteConnection? _connection;
    
            public DatabaseManager(string? databasePath = null)
            {
                var path = databasePath
                    ?? Environment.GetEnvironmentVariable("EJLIVE_DATABASE_PATH")
                    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Data", "ejlive.db");
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                _connectionString = $"Data Source={path};Version=3;";
            }
    
            public SQLiteConnection GetConnection()
            {
                if (_connection == null || _connection.State != ConnectionState.Open)
                {
                    _connection = new SQLiteConnection(_connectionString);
                    _connection.Open();
                }
                return _connection;
            }
    
            // ---- Exact signatures consumed by JournalOutbox ----
    
            public void DeleteClientOutboxItem(string itemId)
            {
                ExecuteNonQuery("DELETE FROM client_outbox WHERE item_id = @itemId",
                    new Dictionary<string, object> { ["@itemId"] = itemId });
            }
    
            public List<ClientOutboxRow> GetClientOutboxItems(int maxRows)
            {
                var rows = ExecuteQuery($"SELECT * FROM client_outbox ORDER BY created_at DESC LIMIT {maxRows}");
                return rows.Select(r => new ClientOutboxRow
                {
                    ItemId = SafeStr(r, "item_id"), ATM_ID = SafeStr(r, "atm_id"),
                    FileName = SafeStr(r, "file_name"), PayloadPath = SafeStr(r, "payload_path"),
                    PayloadSize = SafeLong(r, "payload_size"), FileOffset = SafeLong(r, "file_offset"),
                    Checksum = SafeStr(r, "checksum"), RetryCount = SafeInt(r, "retry_count"),
                    Status = SafeStr(r, "status"),
                    NextAttemptUtc = SafeDateTime(r, "next_attempt_utc"),
                    LastSentUtc = SafeDateTimeNullable(r, "last_sent_utc"),
                    AckDeadlineUtc = SafeDateTimeNullable(r, "ack_deadline_utc"),
                    LastAckDetail = SafeStr(r, "last_ack_detail"),
                    CreatedAtUtc = SafeDateTime(r, "created_at_utc"),
                    UpdatedAtUtc = SafeDateTime(r, "updated_at_utc")
                }).ToList();
            }
            private static string SafeStr(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) ? v?.ToString() ?? "" : "";
            private static long SafeLong(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is long l ? l : 0;
            private static int SafeInt(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is int i ? i : 0;
            private static DateTime SafeDateTime(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is DateTime dt ? dt : DateTime.UtcNow;
            private static DateTime? SafeDateTimeNullable(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is DateTime dt ? dt : null;
    
            public void UpsertClientOutboxItem(
                string itemId, string atmId, string fileName,
                string payloadPath, long payloadSize, long fileOffset,
                string checksum, int retryCount, string status,
                DateTime nextAttemptUtc, DateTime createdAtUtc, DateTime updatedAtUtc,
                DateTime? lastSentUtc, DateTime? ackDeadlineUtc, string lastAckDetail)
            {
                ExecuteNonQuery(
                    @"INSERT OR REPLACE INTO client_outbox
                        (item_id, atm_id, file_name, payload_path, payload_size, file_offset,
                        checksum, retry_count, status, next_attempt_utc, created_at_utc,
                        updated_at_utc, last_sent_utc, ack_deadline_utc, last_ack_detail)
                        VALUES (@itemId, @atmId, @fileName, @payloadPath, @payloadSize, @fileOffset,
                        @checksum, @retryCount, @status, @nextAttemptUtc, @createdAtUtc,
                        @updatedAtUtc, @lastSentUtc, @ackDeadlineUtc, @lastAckDetail)",
                    new Dictionary<string, object>
                    {
                        ["@itemId"] = itemId, ["@atmId"] = atmId, ["@fileName"] = fileName,
                        ["@payloadPath"] = payloadPath, ["@payloadSize"] = payloadSize,
                        ["@fileOffset"] = fileOffset, ["@checksum"] = checksum,
                        ["@retryCount"] = retryCount, ["@status"] = status,
                        ["@nextAttemptUtc"] = nextAttemptUtc.ToString("O"),
                        ["@createdAtUtc"] = createdAtUtc.ToString("O"),
                        ["@updatedAtUtc"] = updatedAtUtc.ToString("O"),
                        ["@lastSentUtc"] = (lastSentUtc?.ToString("O")) ?? (object)DBNull.Value,
                        ["@ackDeadlineUtc"] = (ackDeadlineUtc?.ToString("O")) ?? (object)DBNull.Value,
                        ["@lastAckDetail"] = lastAckDetail ?? string.Empty
                    });
            }
    
            // ---- Generic helpers ----
    
            public void ExecuteNonQuery(string sql, Dictionary<string, object>? parameters = null)
            {
                using var cmd = new SQLiteCommand(sql, GetConnection());
                if (parameters != null)
                    foreach (var kvp in parameters)
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
    
            public object? ExecuteScalar(string sql, Dictionary<string, object>? parameters = null)
            {
                using var cmd = new SQLiteCommand(sql, GetConnection());
                if (parameters != null)
                    foreach (var kvp in parameters)
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                return cmd.ExecuteScalar();
            }
    
            public List<Dictionary<string, object>> ExecuteQuery(string sql, Dictionary<string, object>? parameters = null)
            {
                var results = new List<Dictionary<string, object>>();
                using var cmd = new SQLiteCommand(sql, GetConnection());
                if (parameters != null)
                    foreach (var kvp in parameters)
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < reader.FieldCount; i++)
                        row[reader.GetName(i)] = reader.GetValue(i);
                    results.Add(row);
                }
                return results;
            }
    
            public bool IsDuplicateSync(string atmId, string fileName, string checksum) => false;
            public void InsertAuditLog(string action, string userId, string? target, string details) { }
    
            public void Dispose()
            {
                _connection?.Close();
                _connection?.Dispose();
                _connection = null;
            }
        }
    public partial class DatabaseManager : IDisposable
        {
            private static readonly Lazy<DatabaseManager> _instance = new(() => new DatabaseManager());
    
    
            private readonly string _connectionString;
    
    
            private SQLiteConnection? _connection;
    
    
            public static DatabaseManager Instance => _instance.Value;
    
    
            public void UpsertClientOutboxItem(
                string itemId, string atmId, string fileName,
                string payloadPath, long payloadSize, long fileOffset,
                string checksum, int retryCount, string status,
                DateTime nextAttemptUtc, DateTime createdAtUtc, DateTime updatedAtUtc,
                DateTime? lastSentUtc, DateTime? ackDeadlineUtc, string lastAckDetail)
            {
                ExecuteNonQuery(
                    @"INSERT OR REPLACE INTO client_outbox
                        (item_id, atm_id, file_name, payload_path, payload_size, file_offset,
                        checksum, retry_count, status, next_attempt_utc, created_at_utc,
                        updated_at_utc, last_sent_utc, ack_deadline_utc, last_ack_detail)
                        VALUES (@itemId, @atmId, @fileName, @payloadPath, @payloadSize, @fileOffset,
                        @checksum, @retryCount, @status, @nextAttemptUtc, @createdAtUtc,
                        @updatedAtUtc, @lastSentUtc, @ackDeadlineUtc, @lastAckDetail)",
                    new Dictionary<string, object>
                    {
                        ["@itemId"] = itemId, ["@atmId"] = atmId, ["@fileName"] = fileName,
                        ["@payloadPath"] = payloadPath, ["@payloadSize"] = payloadSize,
                        ["@fileOffset"] = fileOffset, ["@checksum"] = checksum,
                        ["@retryCount"] = retryCount, ["@status"] = status,
                        ["@nextAttemptUtc"] = nextAttemptUtc.ToString("O"),
                        ["@createdAtUtc"] = createdAtUtc.ToString("O"),
                        ["@updatedAtUtc"] = updatedAtUtc.ToString("O"),
                        ["@lastSentUtc"] = (lastSentUtc?.ToString("O")) ?? (object)DBNull.Value,
                        ["@ackDeadlineUtc"] = (ackDeadlineUtc?.ToString("O")) ?? (object)DBNull.Value,
                        ["@lastAckDetail"] = lastAckDetail ?? string.Empty
                    });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\DatabaseManagerStub.cs
            public void InsertAuditLog(string action, string userId, string? target, string details) { }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\DatabaseManagerStub.cs
            public void InsertAuditLog(string action, string userId, string? target, string details) { }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\DatabaseManagerStub.cs
            public void InsertAuditLog(string action, string userId, string? target, string details) { }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\DatabaseManagerStub.cs
            public void InsertAuditLog(string action, string userId, string? target, string details) { }
    
    
            public DatabaseManager(string? databasePath = null)
            {
                var path = databasePath
                    ?? Environment.GetEnvironmentVariable("EJLIVE_DATABASE_PATH")
                    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Data", "ejlive.db");
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                _connectionString = $"Data Source={path};Version=3;";
            }
    
    
            public SQLiteConnection GetConnection()
            {
                if (_connection == null || _connection.State != ConnectionState.Open)
                {
                    _connection = new SQLiteConnection(_connectionString);
                    _connection.Open();
                }
                return _connection;
            }
    
    
            public void DeleteClientOutboxItem(string itemId)
            {
                ExecuteNonQuery("DELETE FROM client_outbox WHERE item_id = @itemId",
                    new Dictionary<string, object> { ["@itemId"] = itemId });
            }
    
    
            public List<ClientOutboxRow> GetClientOutboxItems(int maxRows)
            {
                var rows = ExecuteQuery($"SELECT * FROM client_outbox ORDER BY created_at DESC LIMIT {maxRows}");
                return rows.Select(r => new ClientOutboxRow
                {
                    ItemId = SafeStr(r, "item_id"), ATM_ID = SafeStr(r, "atm_id"),
                    FileName = SafeStr(r, "file_name"), PayloadPath = SafeStr(r, "payload_path"),
                    PayloadSize = SafeLong(r, "payload_size"), FileOffset = SafeLong(r, "file_offset"),
                    Checksum = SafeStr(r, "checksum"), RetryCount = SafeInt(r, "retry_count"),
                    Status = SafeStr(r, "status"),
                    NextAttemptUtc = SafeDateTime(r, "next_attempt_utc"),
                    LastSentUtc = SafeDateTimeNullable(r, "last_sent_utc"),
                    AckDeadlineUtc = SafeDateTimeNullable(r, "ack_deadline_utc"),
                    LastAckDetail = SafeStr(r, "last_ack_detail"),
                    CreatedAtUtc = SafeDateTime(r, "created_at_utc"),
                    UpdatedAtUtc = SafeDateTime(r, "updated_at_utc")
                }).ToList();
            }
    
    
            private static string SafeStr(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) ? v?.ToString() ?? "" : "";
    
    
            private static long SafeLong(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is long l ? l : 0;
    
    
            private static int SafeInt(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is int i ? i : 0;
    
    
            private static DateTime SafeDateTime(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is DateTime dt ? dt : DateTime.UtcNow;
    
    
            private static DateTime? SafeDateTimeNullable(Dictionary<string, object> r, string k) => r.TryGetValue(k, out var v) && v is DateTime dt ? dt : null;
    
    
            public void ExecuteNonQuery(string sql, Dictionary<string, object>? parameters = null)
            {
                using var cmd = new SQLiteCommand(sql, GetConnection());
                if (parameters != null)
                    foreach (var kvp in parameters)
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
    
    
            public object? ExecuteScalar(string sql, Dictionary<string, object>? parameters = null)
            {
                using var cmd = new SQLiteCommand(sql, GetConnection());
                if (parameters != null)
                    foreach (var kvp in parameters)
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                return cmd.ExecuteScalar();
            }
    
    
            public List<Dictionary<string, object>> ExecuteQuery(string sql, Dictionary<string, object>? parameters = null)
            {
                var results = new List<Dictionary<string, object>>();
                using var cmd = new SQLiteCommand(sql, GetConnection());
                if (parameters != null)
                    foreach (var kvp in parameters)
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < reader.FieldCount; i++)
                        row[reader.GetName(i)] = reader.GetValue(i);
                    results.Add(row);
                }
                return results;
            }
    
    
            public bool IsDuplicateSync(string atmId, string fileName, string checksum) => false;
    
    
            public void Dispose()
            {
                _connection?.Close();
                _connection?.Dispose();
                _connection = null;
            }
    
    
        }
}

using var reader = cmd.ExecuteReader();
using var cmd = new SQLiteCommand(sql, GetConnection());
