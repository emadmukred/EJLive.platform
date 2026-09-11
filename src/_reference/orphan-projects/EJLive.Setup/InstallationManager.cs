using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EJLive.Setup
{
    public partial class InstallationManager
    {
        private readonly string _installationPath;
        private readonly SetupWizardForm? _setupForm;
        private void CreateShortcut(string targetPath, string shortcutPath, string description)
        {
            try
            {
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;
                dynamic shell = Activator.CreateInstance(shellType);
                if (shell == null) return;
                var shortcut = shell.CreateShortcut(shortcutPath);
                if (shortcut == null) return;
                shortcut.TargetPath = targetPath;
                shortcut.Description = description;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                shortcut.Save();
            }
            catch
            {
                // إذا فشل إنشاء الاختصار، نستمر في التنفيذ
            }
        }
        public InstallationManager(string installationPath, SetupWizardForm? setupForm = null)
        {
            _installationPath = installationPath;
            _setupForm = setupForm;
        }
        public async Task<bool> ValidatePrerequisitesAsync()
        {
            UpdateStatus("التحقق من متطلبات النظام...");
            if (!IsDotNetInstalled())
            {
                LogError(".NET Framework 4.8 غير مثبت");
                return false;
            }
            if (!IsAdministrator())
            {
                LogError("يجب تشغيل التثبيت بصلاحيات المسؤول");
                return false;
            }
            if (!HasEnoughDiskSpace(100))
            {
                LogError("مساحة القرص غير كافية (مطلوب 100 ميغابايت على الأقل)");
                return false;
            }
            await Task.Delay(500);
            UpdateStatus("اكتمل التحقق من المتطلبات بنجاح");
            return true;
        }
        public async Task<bool> CreateDirectoryStructureAsync()
        {
            UpdateStatus("إنشاء هيكل المجلدات...");
            var directories = new[]
            {
                _installationPath,
                Path.Combine(_installationPath, "Database"),
                Path.Combine(_installationPath, "Logs"),
                Path.Combine(_installationPath, "Config"),
                Path.Combine(_installationPath, "Journal"),
                Path.Combine(_installationPath, "Archive"),
                Path.Combine(_installationPath, "Reports"),
                Path.Combine(_installationPath, "Screenshots"),
                Path.Combine(_installationPath, "Inbox"),
                Path.Combine(_installationPath, "Outbox"),
                Path.Combine(_installationPath, "Temp")
            };
            foreach (var dir in directories)
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch (Exception ex)
                {
                    LogError($"فشل إنشاء المجلد: {dir} - {ex.Message}");
                    return false;
                }
            }
            await Task.Delay(300);
            UpdateStatus("تم إنشاء هيكل المجلدات بنجاح");
            return true;
        }
        public async Task<bool> InstallDatabaseAsync()
        {
            UpdateStatus("تثبيت قاعدة البيانات...");
            try
            {
                var dbPath = Path.Combine(_installationPath, "Database", "ejlive.db");
                var schema = GetDatabaseSchema();
                File.WriteAllText(Path.Combine(_installationPath, "Database", "schema.sql"), schema);
                var result = await ExecuteDatabaseCreationAsync(dbPath, schema);
                if (!result)
                {
                    LogError("فشل إنشاء قاعدة البيانات");
                    return false;
                }
                await Task.Delay(500);
                UpdateStatus("تم تثبيت قاعدة البيانات بنجاح");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"خطأ في تثبيت قاعدة البيانات: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> InstallWindowsServiceAsync()
        {
            UpdateStatus("تثبيت خدمة ويندوز...");
            try
            {
                var servicePath = Path.Combine(_installationPath, "EJLive.Client.Service.exe");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "sc.exe",
                        Arguments = $"create \"EJLive Agent\" binPath= \"{servicePath}\" start= auto",
                        UseShellExecute = true,
                        Verb = "runas",
                        CreateNoWindow = true
                    }
                };
                process.Start();
                await Task.Run(() => process.WaitForExit());
                if (process.ExitCode != 0)
                {
                    LogError($"فشل تثبيت خدمة ويندوز (رمز الخروج: {process.ExitCode})");
                    return false;
                }
                await Task.Delay(500);
                UpdateStatus("تم تثبيت خدمة ويندوز بنجاح");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"خطأ في تثبيت خدمة ويندوز: {ex.Message}");
                return false;
            }
        }
                public async Task<bool> ConfigureServerAsync(string serverAddress, int port)
                {
                    UpdateStatus("تكوين إعدادات الخادم...");
                    try
                    {
                        var configPath = Path.Combine(_installationPath, "Config", "server.ini");
                        var configContent = $@"[Server]
        ServerAddress={serverAddress}
        ServerPort={port}
        EnableSSL=false
        MaxConnections=100
        HeartbeatInterval=30
        JournalSyncInterval=300
        AutoReconnect=true
        MaxRetries=5
        [Database]
        Path=Database\ejlive.db
        BackupEnabled=true
        BackupInterval=3600
        [Logging]
        LogLevel=Info
        LogPath=Logs\
        MaxLogSizeMB=50
        RetainDays=30
        [Security]
        EnableAudit=true
        RequireAdminPrivileges=true
        CommandSigningEnabled=true
        EncryptionEnabled=true
        ";
                        File.WriteAllText(configPath, configContent);
                        await Task.Delay(300);
                        UpdateStatus("تم تكوين إعدادات الخادم بنجاح");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LogError($"خطأ في تكوين إعدادات الخادم: {ex.Message}");
                        return false;
                    }
                }
        public async Task<bool> CreateShortcutsAsync()
        {
            UpdateStatus("إنشاء اختصارات سطح المكتب...");
            try
            {
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var programsPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                CreateShortcut(
                    Path.Combine(_installationPath, "EJLive.Server.exe"),
                    Path.Combine(desktopPath, "EJLive Server.lnk"),
                    "EJLive - خادم إدارة الصرافات");
                CreateShortcut(
                    Path.Combine(_installationPath, "EJLive.Client.exe"),
                    Path.Combine(desktopPath, "EJLive Client.lnk"),
                    "EJLive - عميل الصراف");
                var startMenuFolder = Path.Combine(programsPath, "EJLive");
                Directory.CreateDirectory(startMenuFolder);
                File.Copy(
                    Path.Combine(desktopPath, "EJLive Server.lnk"),
                    Path.Combine(startMenuFolder, "EJLive Server.lnk"),
                    true);
                File.Copy(
                    Path.Combine(desktopPath, "EJLive Client.lnk"),
                    Path.Combine(startMenuFolder, "EJLive Client.lnk"),
                    true);
                await Task.Delay(300);
                UpdateStatus("تم إنشاء اختصارات سطح المكتب بنجاح");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"خطأ في إنشاء الاختصارات: {ex.Message}");
                return false;
            }
        }
                public async Task<bool> RegisterSystemAsync()
                {
                    UpdateStatus("تسجيل النظام...");
                    try
                    {
                        var systemId = Guid.NewGuid().ToString();
                        var registerPath = Path.Combine(_installationPath, "Config", "registry.ini");
                        File.WriteAllText(registerPath, $@"[System]
        SystemId={systemId}
        InstallDate={DateTime.Now:yyyy-MM-dd HH:mm:ss}
        Version=4.0.0
        LicenseType=Enterprise
        MaxATMs=1000
        ");
                        await Task.Delay(300);
                        UpdateStatus("تم تسجيل النظام بنجاح");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LogError($"خطأ في تسجيل النظام: {ex.Message}");
                        return false;
                    }
                }
        public async Task<bool> InstallFullSystemAsync(string serverAddress, int port)
        {
            var steps = new Func<Task<bool>>[]
            {
                () => ValidatePrerequisitesAsync(),
                () => CreateDirectoryStructureAsync(),
                () => InstallDatabaseAsync(),
                () => InstallWindowsServiceAsync(),
                () => ConfigureServerAsync(serverAddress, port),
                () => CreateShortcutsAsync(),
                () => RegisterSystemAsync()
            };
            foreach (var step in steps)
            {
                if (!await step())
                {
                    UpdateStatus("فشل التثبيت - راجع السجل للتفاصيل");
                    return false;
                }
            }
            UpdateStatus("تم التثبيت بنجاح!");
            return true;
        }
        private void UpdateStatus(string message)
        {
            _setupForm?.UpdateStatus(message);
            LogInfo(message);
        }
        private void LogInfo(string message)
        {
            var logPath = Path.Combine(_installationPath, "Logs", $"install_{DateTime.Now:yyyyMMdd}.log");
            var logDir = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(logDir))
                Directory.CreateDirectory(logDir);
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}\n");
        }
        private void LogError(string message)
        {
            var logPath = Path.Combine(_installationPath, "Logs", $"install_{DateTime.Now:yyyyMMdd}.log");
            var logDir = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(logDir))
                Directory.CreateDirectory(logDir);
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}\n");
            var errorLogPath = Path.Combine(_installationPath, "Logs", "install_errors.log");
            File.AppendAllText(errorLogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
        }
        private bool IsDotNetInstalled()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "--version",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
        private bool IsAdministrator()
        {
            try
            {
                using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
        private bool HasEnoughDiskSpace(int requiredMB)
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(_installationPath) ?? "C:\\");
                var availableMB = drive.AvailableFreeSpace / (1024 * 1024);
                return availableMB >= requiredMB;
            }
            catch
            {
                return false;
            }
        }
                private string GetDatabaseSchema()
                {
                    return @"
        -- قاعدة بيانات EJLive - Unified System
        CREATE TABLE IF NOT EXISTS atms (
            atm_id TEXT PRIMARY KEY,
            atm_name TEXT NOT NULL,
            atm_type TEXT NOT NULL,
            status TEXT DEFAULT 'offline',
            ip_address TEXT,
            port INTEGER,
            location TEXT,
            branch_code TEXT,
            last_heartbeat DATETIME,
            last_sync DATETIME,
            health_score REAL DEFAULT 0,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
        );
        CREATE TABLE IF NOT EXISTS journal_sync (
            sync_id INTEGER PRIMARY KEY AUTOINCREMENT,
            atm_id TEXT NOT NULL,
            file_name TEXT NOT NULL,
            file_size INTEGER,
            state TEXT DEFAULT 'pending',
            progress REAL DEFAULT 0,
            checksum TEXT,
            started_at DATETIME,
            completed_at DATETIME,
            error_message TEXT,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            FOREIGN KEY (atm_id) REFERENCES atms(atm_id)
        );
        CREATE TABLE IF NOT EXISTS commands (
            command_id INTEGER PRIMARY KEY AUTOINCREMENT,
            atm_id TEXT NOT NULL,
            command_type TEXT NOT NULL,
            parameters TEXT,
            status TEXT DEFAULT 'pending',
            result TEXT,
            risk_level TEXT DEFAULT 'low',
            signature TEXT,
            executed_by TEXT,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            completed_at DATETIME,
            FOREIGN KEY (atm_id) REFERENCES atms(atm_id)
        );
        CREATE TABLE IF NOT EXISTS audit_log (
            audit_id INTEGER PRIMARY KEY AUTOINCREMENT,
            event_type TEXT NOT NULL,
            atm_id TEXT,
            user_id TEXT,
            action TEXT NOT NULL,
            details TEXT,
            ip_address TEXT,
            severity TEXT DEFAULT 'info',
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP
        );
        CREATE TABLE IF NOT EXISTS alerts (
            alert_id INTEGER PRIMARY KEY AUTOINCREMENT,
            atm_id TEXT,
            severity TEXT NOT NULL,
            title TEXT NOT NULL,
            message TEXT,
            category TEXT,
            source TEXT,
            recommendation TEXT,
            is_read INTEGER DEFAULT 0,
            is_resolved INTEGER DEFAULT 0,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            resolved_at DATETIME,
            FOREIGN KEY (atm_id) REFERENCES atms(atm_id)
        );
        CREATE TABLE IF NOT EXISTS transactions (
            transaction_id TEXT PRIMARY KEY,
            atm_id TEXT NOT NULL,
            transaction_type TEXT NOT NULL,
            amount REAL,
            currency TEXT DEFAULT 'SAR',
            status TEXT,
            card_present INTEGER,
            timestamp DATETIME NOT NULL,
            response_code TEXT,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            FOREIGN KEY (atm_id) REFERENCES atms(atm_id)
        );
        CREATE TABLE IF NOT EXISTS reports (
            report_id INTEGER PRIMARY KEY AUTOINCREMENT,
            report_type TEXT NOT NULL,
            title TEXT NOT NULL,
            parameters TEXT,
            file_path TEXT,
            file_size INTEGER,
            created_by TEXT,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            expires_at DATETIME
        );
        CREATE INDEX IF NOT EXISTS idx_atm_status ON atms(status);
        CREATE INDEX IF NOT EXISTS idx_journal_sync_atm ON journal_sync(atm_id);
        CREATE INDEX IF NOT EXISTS idx_journal_sync_state ON journal_sync(state);
        CREATE INDEX IF NOT EXISTS idx_commands_atm ON commands(atm_id);
        CREATE INDEX IF NOT EXISTS idx_commands_status ON commands(status);
        CREATE INDEX IF NOT EXISTS idx_audit_log_created ON audit_log(created_at);
        CREATE INDEX IF NOT EXISTS idx_alerts_severity ON alerts(severity);
        CREATE INDEX IF NOT EXISTS idx_alerts_created ON alerts(created_at);
        CREATE INDEX IF NOT EXISTS idx_transactions_atm ON transactions(atm_id);
        CREATE INDEX IF NOT EXISTS idx_transactions_timestamp ON transactions(timestamp);
        ";
                }
        private async Task<bool> ExecuteDatabaseCreationAsync(string dbPath, string schema)
        {
            try
            {
                var schemaFile = Path.Combine(_installationPath, "Database", "schema.sql");
                if (!File.Exists("sqlite3.exe"))
                {
                    ExecuteSchemaDirectly(dbPath, schema);
                }
                else
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "sqlite3.exe",
                            Arguments = $"\"{dbPath}\" < \"{schemaFile}\"",
                            UseShellExecute = true,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    await Task.Run(() => process.WaitForExit());
                }
                return File.Exists(dbPath);
            }
            catch (Exception ex)
            {
                LogError($"فشل تنفيذ السكربت: {ex.Message}");
                return false;
            }
        }
        private void ExecuteSchemaDirectly(string dbPath, string schema)
        {
            File.WriteAllText(dbPath, "SQLite format 3\0");
            var schemaFile = Path.Combine(_installationPath, "Database", "deferred_schema.sql");
            File.WriteAllText(schemaFile, schema);
        }
    }

}
