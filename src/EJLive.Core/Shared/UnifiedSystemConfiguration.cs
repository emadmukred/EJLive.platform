using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EJLive.Setup
{
    /// <summary>
    /// Persisted system configuration document (JSON under the central data root,
    /// <c>Config/system.json</c>). Rebuilt in Wave 4 (SS-20 / finding E-16): the previous
    /// file was an un-collapsed auto-merge dump carrying two constructors with the same
    /// signature (CS0111), a <c>GetIntSetting</c> that returned a string for an int
    /// (CS0029), get-only path properties one constructor left unassigned (CS0171), and a
    /// second persistence identity (an XML <c>ejlive.config.xml</c> document no consumer
    /// read). One identity, one constructor, one format — JSON — remains.
    ///
    /// The connection string this class hands out must be valid for the single ADO.NET
    /// provider the solution pins (Microsoft.Data.Sqlite 8.0.5, debt D-06): the retired
    /// <c>Version=3</c>/<c>Journal Mode</c>/<c>Busy Timeout</c> keywords are System.Data.SQLite
    /// syntax the unified provider rejects at open time, so they are gone; WAL and the
    /// performance PRAGMAs are applied once by <c>DatabaseManager</c> instead.
    /// </summary>
    public sealed class UnifiedSystemConfiguration
    {
        private readonly string _configPath;
        private readonly object _gate = new object();
        private SystemConfiguration _currentConfig = new SystemConfiguration();
        private readonly Dictionary<string, string> _settings =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string BasePath { get; }
        public string ConfigDirectory => Path.Combine(BasePath, "Config");
        public string DataPath { get; }
        public string LogPath { get; }
        public string BackupPath { get; }
        public string ReportsPath { get; }
        public string ArchivePath { get; }
        public SystemConfiguration Current => _currentConfig;

        /// <summary>Binds to the central data root unless an explicit base path is given.</summary>
        public UnifiedSystemConfiguration(string? basePath = null)
        {
            BasePath = string.IsNullOrWhiteSpace(basePath)
                ? EJLive.Shared.DataRootPaths.Root
                : Path.GetFullPath(basePath);
            DataPath = Path.Combine(BasePath, "Data");
            LogPath = Path.Combine(BasePath, "Logs");
            BackupPath = Path.Combine(BasePath, "Backup");
            ReportsPath = Path.Combine(BasePath, "Reports");
            ArchivePath = Path.Combine(BasePath, "Archive");
            _configPath = Path.Combine(ConfigDirectory, "system.json");
            EnsureDirectories();
            LoadFromDisk();
        }

        // ── Document load/save ─────────────────────────────────────────────────
        public Task<SystemConfiguration> LoadConfigurationAsync()
        {
            try
            {
                if (!File.Exists(_configPath))
                    return Task.FromResult(SaveDefaultConfiguration());

                lock (_gate)
                {
                    var json = File.ReadAllText(_configPath);
                    _currentConfig = JsonSerializer.Deserialize<SystemConfiguration>(json) ?? new SystemConfiguration();
                    MergeSettingsFromDocument();
                }
                return Task.FromResult(_currentConfig);
            }
            catch (Exception ex)
            {
                // Fail-open to defaults: a corrupt document must not stop the host; the
                // caller receives the validated default set and the next save overwrites.
                Console.WriteLine($"[Setup] configuration load failed, defaults applied: {ex.Message}");
                lock (_gate)
                    _currentConfig = new SystemConfiguration();
                return Task.FromResult(_currentConfig);
            }
        }

        public SystemConfiguration SaveDefaultConfiguration()
        {
            lock (_gate)
            {
                _currentConfig = new SystemConfiguration
                {
                    SystemName = "EJLive Enterprise System",
                    Version = "4.0.0",
                    Language = "en",
                    TimeZone = "UTC",
                    DatabasePath = Path.Combine(DataPath, "ejlive.db"),
                    LogDirectory = LogPath,
                    ArchiveDirectory = ArchivePath,
                    ReportsDirectory = ReportsPath,
                    BackupDirectory = BackupPath,
                    ATMTypes = new List<string>(EJLive.Shared.AppConstants.SupportedVendors)
                };
                SaveToDisk();
                return _currentConfig;
            }
        }

        public Task SaveDefaultConfigurationAsync() => Task.FromResult(SaveDefaultConfiguration());

        public async Task UpdateSettingsAsync(Dictionary<string, object> settings)
        {
            if (settings == null) return;
            lock (_gate)
            {
                foreach (var setting in settings)
                {
                    switch (setting.Key)
                    {
                        case "HeartbeatIntervalSeconds":
                            _currentConfig.HeartbeatIntervalSeconds = Convert.ToInt32(setting.Value);
                            break;
                        case "JournalSyncIntervalSeconds":
                            _currentConfig.JournalSyncIntervalSeconds = Convert.ToInt32(setting.Value);
                            break;
                        case "MaxConnectedATMs":
                            _currentConfig.MaxConnectedATMs = Convert.ToInt32(setting.Value);
                            break;
                        case "LogLevel":
                            _currentConfig.LogLevel = Convert.ToString(setting.Value) ?? "Info";
                            break;
                        case "Language":
                            _currentConfig.Language = Convert.ToString(setting.Value) ?? "en";
                            break;
                        default:
                            _settings[setting.Key] = Convert.ToString(setting.Value) ?? string.Empty;
                            break;
                    }
                }
            }
            await SaveConfigurationInternalAsync();
        }

        public ConfigurationValidationResult ValidateConfiguration()
        {
            var errors = new List<string>();
            var config = _currentConfig;
            if (config.HeartbeatIntervalSeconds is < 5 or > 300)
                errors.Add("Heartbeat interval must be between 5 and 300 seconds.");
            if (config.JournalSyncIntervalSeconds is < 60 or > 3600)
                errors.Add("Journal sync interval must be between 60 and 3600 seconds.");
            if (config.MaxConnectedATMs is < 1 or > 1000)
                errors.Add("Max connected ATMs must be between 1 and 1000.");
            if (config.SecuritySettings.PasswordMinLength < 6)
                errors.Add("Minimum password length must be at least 6 characters.");
            if (string.IsNullOrWhiteSpace(config.DatabasePath))
                errors.Add("Database path is not set.");
            return new ConfigurationValidationResult { IsValid = errors.Count == 0, Errors = errors };
        }

        public async Task BackupConfigurationAsync()
        {
            string backupPath;
            string json;
            lock (_gate)
            {
                backupPath = Path.Combine(ConfigDirectory, "Backups",
                    $"system_config_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
                json = SerializeDocument();
            }
            Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
            await File.WriteAllTextAsync(backupPath, json);
        }

        public async Task<bool> RestoreFromBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                    return false;
                var json = await File.ReadAllTextAsync(backupPath);
                var restored = JsonSerializer.Deserialize<SystemConfiguration>(json);
                if (restored == null)
                    return false;
                lock (_gate)
                {
                    _currentConfig = restored;
                    SaveToDisk();
                }
                return true;
            }
            catch (Exception)
            {
                // Fail-closed on restore: a bad backup must never overwrite the live document.
                return false;
            }
        }

        public string ExportConfigurationTemplate() =>
            JsonSerializer.Serialize(new SystemConfiguration(), JsonOptions);

        // ── Free-form settings layer (flat key/value persisted inside the document) ──
        public string GetSetting(string key, string defaultValue = "")
        {
            lock (_gate)
                return _settings.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public void SetSetting(string key, string value)
        {
            lock (_gate)
                _settings[key] = value ?? string.Empty;
        }

        public T GetSetting<T>(string key, T defaultValue = default!)
        {
            var value = GetSetting(key);
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception)
            {
                // A malformed value degrades to the caller's default; the document stays valid.
                return defaultValue;
            }
        }

        public int GetIntSetting(string key, int defaultValue = 0) => GetSetting(key, defaultValue.ToString()) is { } s ? ParseInt(s, defaultValue) : defaultValue;

        public bool GetBoolSetting(string key, bool defaultValue = false) =>
            bool.TryParse(GetSetting(key, defaultValue.ToString()), out var value) ? value : defaultValue;

        private static int ParseInt(string value, int fallback) =>
            int.TryParse(value, out var parsed) ? parsed : fallback;

        /// <summary>
        /// Microsoft.Data.Sqlite compatible connection string for the system database.
        /// Shared cache + pooling; PRAGMAs are applied by <c>DatabaseManager</c> (SS-11).
        /// </summary>
        public string GetConnectionString()
        {
            var database = string.IsNullOrWhiteSpace(_currentConfig.DatabasePath)
                ? Path.Combine(DataPath, "ejlive.db")
                : _currentConfig.DatabasePath;
            return $"Data Source={database};Cache=Shared;Pooling=True;";
        }

        // ── internals ──────────────────────────────────────────────────────────
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private void LoadFromDisk()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    _currentConfig = JsonSerializer.Deserialize<SystemConfiguration>(json) ?? new SystemConfiguration();
                    MergeSettingsFromDocument();
                }
                else
                {
                    SaveDefaultConfiguration();
                }
            }
            catch (Exception)
            {
                // Startup must survive an unreadable document; defaults are applied and the
                // next save repairs the file. (Same decision the JSON Load() fallback makes.)
                _currentConfig = new SystemConfiguration();
            }
            ApplySettingDefaults();
        }

        private void MergeSettingsFromDocument()
        {
            if (_currentConfig.Settings == null)
                return;
            foreach (var pair in _currentConfig.Settings)
                _settings[pair.Key] = pair.Value ?? string.Empty;
        }

        private void ApplySettingDefaults()
        {
            SetIfNotExists("ServerPort", EJLive.Core.AppConstants.DefaultPort.ToString());
            SetIfNotExists("HeartbeatIntervalSec", EJLive.Core.AppConstants.HeartbeatIntervalSec.ToString());
            SetIfNotExists("MaxConnections", "1000");
            SetIfNotExists("LogLevel", "Info");
            SetIfNotExists("AutoSync", "true");
            SetIfNotExists("BackupEnabled", "true");
            SetIfNotExists("BackupRetentionDays", "30");
            SetIfNotExists("JournalRetentionMonths", "12");
            SetIfNotExists("CompressionEnabled", "true");
            SetIfNotExists("EncryptionEnabled", "true");
        }

        private void SetIfNotExists(string key, string value)
        {
            lock (_gate)
            {
                if (!_settings.ContainsKey(key))
                    _settings[key] = value;
            }
        }

        private string SerializeDocument()
        {
            _currentConfig.Settings = new Dictionary<string, string>(_settings, StringComparer.OrdinalIgnoreCase);
            return JsonSerializer.Serialize(_currentConfig, JsonOptions);
        }

        private void SaveToDisk()
        {
            Directory.CreateDirectory(ConfigDirectory);
            File.WriteAllText(_configPath, SerializeDocument());
        }

        private Task SaveConfigurationInternalAsync()
        {
            lock (_gate)
                SaveToDisk();
            return Task.CompletedTask;
        }

        private void EnsureDirectories()
        {
            foreach (var dir in new[] { ConfigDirectory, DataPath, LogPath, BackupPath, ReportsPath, ArchivePath })
            {
                try { Directory.CreateDirectory(dir); }
                catch (IOException) { /* read-only share: the bootstrap report surfaces it */ }
            }
        }
    }

    /// <summary>Persisted configuration document. DTO by design: it is the JSON contract.</summary>
    public sealed class SystemConfiguration
    {
        public string SystemName { get; set; } = "EJLive Enterprise System";
        public string Version { get; set; } = "4.0.0";
        public string Language { get; set; } = "en";
        public string TimeZone { get; set; } = "UTC";
        public int MaxConnectedATMs { get; set; } = 500;
        public int HeartbeatIntervalSeconds { get; set; } = 30;
        public int JournalSyncIntervalSeconds { get; set; } = 300;
        public bool AutoReconnectEnabled { get; set; } = true;
        public int MaxRetryAttempts { get; set; } = 5;
        public int RetryIntervalSeconds { get; set; } = 30;
        public string DatabasePath { get; set; } = string.Empty;
        public string LogDirectory { get; set; } = string.Empty;
        public string ArchiveDirectory { get; set; } = string.Empty;
        public string ReportsDirectory { get; set; } = string.Empty;
        public string BackupDirectory { get; set; } = string.Empty;
        public string LogLevel { get; set; } = "Info";
        public int LogRetentionDays { get; set; } = 30;
        public int MaxLogSizeMB { get; set; } = 50;
        public bool ArchiveEnabled { get; set; } = true;
        public int ArchiveIntervalDays { get; set; } = 30;
        public bool AlertEnabled { get; set; } = true;
        public bool EmailNotificationsEnabled { get; set; }
        public EmailConfiguration EmailSettings { get; set; } = new EmailConfiguration();
        public SecuritySettings SecuritySettings { get; set; } = new SecuritySettings();
        public List<string> ATMTypes { get; set; } = new List<string>();
        public List<string> SupportedProtocols { get; set; } = new List<string> { "TCP", "TLS" };
        public int DefaultCommandTimeoutSeconds { get; set; } = 30;
        public int ScreenshotQuality { get; set; } = 80;
        public int GhostSessionQuality { get; set; } = 60;
        public int MaxGhostSessions { get; set; } = 10;
        public string ReportTemplatesPath { get; set; } = "Config\\ReportTemplates";
        public BackupConfiguration BackupSettings { get; set; } = new BackupConfiguration();

        /// <summary>Flat extension settings persisted with the document (not a wire format).</summary>
        public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public sealed class SecuritySettings
    {
        public bool EnableAuditLogging { get; set; } = true;
        public bool RequireAdminPrivileges { get; set; } = true;
        public int SessionTimeoutMinutes { get; set; } = 30;
        public int MaxLoginAttempts { get; set; } = 5;
        public int PasswordMinLength { get; set; } = 8;
        public bool PasswordRequireSpecialChar { get; set; } = true;
    }

    public sealed class EmailConfiguration
    {
        public bool Enabled { get; set; }
        public string SMTPServer { get; set; } = string.Empty;
        public int SMTPPort { get; set; } = 587;
        public bool UseSSL { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = "EJLive System";
    }

    public sealed class BackupConfiguration
    {
        public bool Enabled { get; set; } = true;
        public string Frequency { get; set; } = "daily";
        public int RetentionDays { get; set; } = 90;
        public bool IncludeDatabase { get; set; } = true;
        public bool IncludeConfig { get; set; } = true;
        public bool IncludeJournals { get; set; }
    }

    public sealed class ConfigurationValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
