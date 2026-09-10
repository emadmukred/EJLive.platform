using EJLive.Core;
using EJLive.Core.Models;
using EJLive.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace EJLive.Setup
{
    public partial class SystemConfiguration
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
        public string DatabasePath { get; set; } = "Database\\ejlive.db";
        public string LogLevel { get; set; } = "Info";
        public int LogRetentionDays { get; set; } = 30;
        public int MaxLogSizeMB { get; set; } = 50;
        public bool ArchiveEnabled { get; set; } = true;
        public int ArchiveIntervalDays { get; set; } = 30;
        public bool AlertEnabled { get; set; } = true;
        public bool EmailNotificationsEnabled { get; set; } = false;
        public SMSConfiguration SMSSettings { get; set; } = new SMSConfiguration();
        public EmailConfiguration EmailSettings { get; set; } = new EmailConfiguration();
        public SecuritySettings SecuritySettings { get; set; } = new SecuritySettings();
        public List<string> ATMTypes { get; set; } = new List<string>();
        public List<string> SupportedProtocols { get; set; } = new List<string>();
        public int DefaultCommandTimeoutSeconds { get; set; } = 30;
        public int ScreenshotQuality { get; set; } = 80;
        public int GhostSessionQuality { get; set; } = 60;
        public int MaxGhostSessions { get; set; } = 10;
        public string ReportTemplatesPath { get; set; } = "Config\\ReportTemplates";
        public BackupConfiguration BackupSettings { get; set; } = new BackupConfiguration();
    }

    public partial class SecuritySettings
    {
        public bool EnableAuditLogging { get; set; } = true;
        public bool RequireAdminPrivileges { get; set; } = true;
        public int SessionTimeoutMinutes { get; set; } = 30;
        public int MaxLoginAttempts { get; set; } = 5;
        public int PasswordMinLength { get; set; } = 8;
        public bool PasswordRequireSpecialChar { get; set; } = true;
    }

    public partial class SMSConfiguration
    {
        public bool Enabled { get; set; } = false;
        public string Provider { get; set; } = "Twilio";
        public string AccountSID { get; set; } = "";
        public string AuthToken { get; set; } = "";
        public string FromNumber { get; set; } = "";
    }

    public partial class EmailConfiguration
    {
        public bool Enabled { get; set; } = false;
        public string SMTPServer { get; set; } = "";
        public int SMTPPort { get; set; } = 587;
        public bool UseSSL { get; set; } = true;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromAddress { get; set; } = "";
        public string FromName { get; set; } = "EJLive System";
    }

    public partial class BackupConfiguration
    {
        public bool Enabled { get; set; } = true;
        public string Frequency { get; set; } = "daily";
        public int RetentionDays { get; set; } = 90;
        public bool IncludeDatabase { get; set; } = true;
        public bool IncludeConfig { get; set; } = true;
        public bool IncludeJournals { get; set; } = false;
    }

    public partial class ConfigurationValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public partial class UnifiedSystemConfiguration
    {
        private readonly string _configPath;
        private SystemConfiguration _currentConfig;
        private readonly Dictionary<string, string> _settings;
        public async Task UpdateSettingsAsync(Dictionary<string, object> settings)
        {
            if (_currentConfig == null)
                await LoadConfigurationAsync();
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
                        _currentConfig.LogLevel = setting.Value.ToString();
                        break;
                    case "Language":
                        _currentConfig.Language = setting.Value.ToString();
                        break;
                }
            }
            await SaveConfigurationInternalAsync(_currentConfig);
        }
        public string BasePath { get; }
        public string DataPath { get; }
        public string LogPath { get; }
        public string BackupPath { get; }
        public string ReportsPath { get; }
        public string ArchivePath { get; }
        public UnifiedSystemConfiguration(string basePath)
        {
            _configPath = Path.Combine(basePath, "Config", "system.json");
            _currentConfig = new SystemConfiguration();
        }
        public UnifiedSystemConfiguration(string basePath)
        {
            BasePath = basePath ?? AppDomain.CurrentDomain.BaseDirectory;
            _configPath = Path.Combine(BasePath, "ejlive.config.xml");
            _settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            // تعيين المسارات الافتراضية
            DataPath = Path.Combine(BasePath, "Data");
            LogPath = Path.Combine(BasePath, "Logs");
            BackupPath = Path.Combine(BasePath, "Backup");
            ReportsPath = Path.Combine(BasePath, "Reports");
            ArchivePath = Path.Combine(BasePath, "Archive");
            EnsureDirectories();
            LoadConfiguration();
        }
        public async Task<SystemConfiguration> LoadConfigurationAsync()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    await SaveDefaultConfigurationAsync();
                    return _currentConfig;
                }
                var json = await File.ReadAllTextAsync(_configPath);
                _currentConfig = JsonSerializer.Deserialize<SystemConfiguration>(json) ?? new SystemConfiguration();
                return _currentConfig;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في تحميل الإعدادات: {ex.Message}");
                return new SystemConfiguration();
            }
        }
        public async Task SaveDefaultConfigurationAsync()
        {
            _currentConfig = new SystemConfiguration
            {
                SystemName = "EJLive Enterprise System",
                Version = "4.0.0",
                Language = "ar",
                TimeZone = "Asia/Riyadh",
                MaxConnectedATMs = 500,
                HeartbeatIntervalSeconds = 30,
                JournalSyncIntervalSeconds = 300,
                AutoReconnectEnabled = true,
                MaxRetryAttempts = 5,
                RetryIntervalSeconds = 30,
                DatabasePath = "Database\\ejlive.db",
                LogLevel = "Info",
                LogRetentionDays = 30,
                MaxLogSizeMB = 50,
                ArchiveEnabled = true,
                ArchiveIntervalDays = 30,
                AlertEnabled = true,
                EmailNotificationsEnabled = false,
                SMSSettings = new SMSConfiguration(),
                EmailSettings = new EmailConfiguration(),
                SecuritySettings = new SecuritySettings
                {
                    EnableAuditLogging = true,
                    RequireAdminPrivileges = true,
                    SessionTimeoutMinutes = 30,
                    MaxLoginAttempts = 5,
                    PasswordMinLength = 8,
                    PasswordRequireSpecialChar = true
                },
                ATMTypes = new List<string> { "NCR", "GRG", "WINCOR", "DIEBOLD", "HYOSUNG", "CASHWAY" },
                SupportedProtocols = new List<string> { "TCP", "TLS", "HTTPS" },
                DefaultCommandTimeoutSeconds = 30,
                ScreenshotQuality = 80,
                GhostSessionQuality = 60,
                MaxGhostSessions = 10,
                ReportTemplatesPath = "Config\\ReportTemplates",
                BackupSettings = new BackupConfiguration
                {
                    Enabled = true,
                    Frequency = "daily",
                    RetentionDays = 90,
                    IncludeDatabase = true,
                    IncludeConfig = true,
                    IncludeJournals = false
                }
            };
            await SaveConfigurationInternalAsync(_currentConfig);
        }
        public ConfigurationValidationResult ValidateConfiguration()
        {
            var result = new ConfigurationValidationResult();
            var errors = new List<string>();
            if (_currentConfig.HeartbeatIntervalSeconds < 5 || _currentConfig.HeartbeatIntervalSeconds > 300)
                errors.Add("فترة نبضات القلب يجب أن تكون بين 5 و 300 ثانية");
            if (_currentConfig.JournalSyncIntervalSeconds < 60 || _currentConfig.JournalSyncIntervalSeconds > 3600)
                errors.Add("فترة مزامنة المجلات يجب أن تكون بين 60 و 3600 ثانية");
            if (_currentConfig.MaxConnectedATMs < 1 || _currentConfig.MaxConnectedATMs > 1000)
                errors.Add("العدد الأقصى لأجهزة الصراف يجب أن يكون بين 1 و 1000");
            if (_currentConfig.SecuritySettings.PasswordMinLength < 6)
                errors.Add("الحد الأدنى لطول كلمة المرور يجب أن لا يقل عن 6 أحرف");
            if (string.IsNullOrWhiteSpace(_currentConfig.DatabasePath))
                errors.Add("مسار قاعدة البيانات غير محدد");
            result.IsValid = errors.Count == 0;
            result.Errors = errors;
            return result;
        }
        public async Task BackupConfigurationAsync()
        {
            try
            {
                var backupPath = Path.Combine(
                    Path.GetDirectoryName(_configPath) ?? "Config",
                    "Backups",
                    $"system_config_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                Directory.CreateDirectory(Path.GetDirectoryName(backupPath) ?? "Config\\Backups");
                var json = JsonSerializer.Serialize(_currentConfig, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                await File.WriteAllTextAsync(backupPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"فشل إنشاء نسخة احتياطية: {ex.Message}");
            }
        }
        public async Task<bool> RestoreFromBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                    return false;
                var json = await File.ReadAllTextAsync(backupPath);
                var backupConfig = JsonSerializer.Deserialize<SystemConfiguration>(json);
                if (backupConfig == null)
                    return false;
                _currentConfig = backupConfig;
                await SaveConfigurationInternalAsync(_currentConfig);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public string ExportConfigurationTemplate()
        {
            var template = new SystemConfiguration();
            template.SecuritySettings = new SecuritySettings();
            return JsonSerializer.Serialize(template, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
        private async Task SaveConfigurationInternalAsync(SystemConfiguration config)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configPath) ?? "Config");
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            await File.WriteAllTextAsync(_configPath, json);
        }
        private readonly object _lock = new object();
        private void EnsureDirectories()
        {
            foreach (var dir in new[] { DataPath, LogPath, BackupPath, ReportsPath, ArchivePath })
            {
                try { Directory.CreateDirectory(dir); } catch { }
            }
        }
        public string GetSetting(string key, string defaultValue = "")
        {
            lock (_lock)
            {
                return _settings.TryGetValue(key, out var value) ? value : defaultValue;
            }
        }
        public void SetSetting(string key, string value)
        {
            lock (_lock)
            {
                _settings[key] = value;
            }
        }
        public T GetSetting<T>(string key, T defaultValue = default)
        {
            var value = GetSetting(key);
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        public int GetIntSetting(string key, int defaultValue = 0)
            => GetSetting(key, defaultValue);
        public bool GetBoolSetting(string key, bool defaultValue = false)
            => GetSetting(key, defaultValue);
        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    using var stream = new FileStream(_configPath, FileMode.Open, FileAccess.Read);
                    _settings.Clear();
                    LoadFromStream(stream);
                }
                // تطبيق القيم الافتراضية إذا لم تكن موجودة
                ApplyDefaults();
            }
            catch
            {
                ApplyDefaults();
            }
        }
        private void ApplyDefaults()
        {
            SetIfNotExists("ServerPort", "5656");
            SetIfNotExists("HeartbeatIntervalSec", "30");
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
            lock (_lock)
            {
                if (!_settings.ContainsKey(key))
                    _settings[key] = value;
            }
        }
        private void LoadFromStream(Stream stream)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(stream);
                var nodes = doc.SelectNodes("//Configuration/Setting");
                if (nodes != null)
                {
                    foreach (XmlNode node in nodes)
                    {
                        var key = node.Attributes?["Key"]?.Value;
                        var value = node.Attributes?["Value"]?.Value;
                        if (!string.IsNullOrEmpty(key))
                            _settings[key] = value ?? string.Empty;
                    }
                }
            }
            catch { }
        }
        public void SaveConfiguration()
        {
            try
            {
                var doc = new XmlDocument();
                var root = doc.CreateElement("Configuration");
                doc.AppendChild(root);
                lock (_lock)
                {
                    foreach (var kvp in _settings)
                    {
                        var element = doc.CreateElement("Setting");
                        element.SetAttribute("Key", kvp.Key);
                        element.SetAttribute("Value", kvp.Value);
                        root.AppendChild(element);
                    }
                }
                doc.Save(_configPath);
            }
            catch { }
        }
        public string GetConnectionString()
        {
            return $"Data Source={Path.Combine(DataPath, "ejlive.db")};Version=3;Journal Mode=WAL;Busy Timeout=5000;";
        }
    }

}
