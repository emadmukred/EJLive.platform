using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using EJLive.Shared;

namespace EJLive.Core.Services
{
    /// <summary>
    /// Credential store abstraction. Implementations must be thread-safe.
    /// </summary>
    public interface ICredentialStore
    {
        /// <summary>Validates the supplied credentials against the store.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="password">The plain-text password.</param>
        /// <returns>True if the credentials are valid; otherwise false.</returns>
        bool ValidateCredentials(string userId, string password);

        /// <summary>Retrieves the role assigned to the user.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user's role, or null if the user does not exist.</returns>
        UserRole? GetRole(string userId);

        /// <summary>Determines whether the user exists in the store.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>True if the user exists; otherwise false.</returns>
        bool UserExists(string userId);
    }

    /// <summary>
    /// In-memory credential store backed by PBKDF2-hashed passwords.
    /// Suitable for testing and lightweight deployments.
    /// </summary>
    public sealed class InMemoryCredentialStore : ICredentialStore
    {
        private readonly Dictionary<string, (string PasswordHash, UserRole Role)> _users;
        private readonly object _lock = new object();

        public InMemoryCredentialStore()
        {
            _users = new Dictionary<string, (string, UserRole)>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Adds a user with a PBKDF2-hashed password.</summary>
        /// <param name="userId">The unique user identifier.</param>
        /// <param name="password">The plain-text password (minimum 8 characters).</param>
        /// <param name="role">The user's role.</param>
        public void AddUser(string userId, string password, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters.", nameof(password));

            lock (_lock)
            {
                _users[userId] = (SecurityHelper.HashPassword(password), role);
            }
        }

        /// <summary>Removes a user from the store.</summary>
        /// <param name="userId">The user identifier.</param>
        public void RemoveUser(string userId)
        {
            lock (_lock)
            {
                _users.Remove(userId);
            }
        }

        /// <inheritdoc />
        public bool ValidateCredentials(string userId, string password)
        {
            lock (_lock)
            {
                return _users.TryGetValue(userId, out var info) &&
                       SecurityHelper.VerifyPassword(password, info.PasswordHash);
            }
        }

        /// <inheritdoc />
        public UserRole? GetRole(string userId)
        {
            lock (_lock)
            {
                return _users.TryGetValue(userId, out var info) ? info.Role : null;
            }
        }

        /// <inheritdoc />
        public bool UserExists(string userId)
        {
            lock (_lock)
            {
                return _users.ContainsKey(userId);
            }
        }

        public IReadOnlyDictionary<string, UserRole> GetUsers()
        {
            lock (_lock)
            {
                return _users.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Role, StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    /// <summary>
    /// File-backed credential store. JSON file format with hashed passwords.
    /// File path is resolved from environment variable EJLIVE_CREDENTIALS_PATH
    /// or falls back to %PROGRAMDATA%\EJLive\credentials.json.
    /// </summary>
    public sealed class FileBasedCredentialStore : ICredentialStore
    {
        private readonly string _filePath;
        private readonly object _lock = new object();
        private Dictionary<string, CredentialEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

        public FileBasedCredentialStore(string? filePath = null)
        {
            _filePath = filePath ?? ResolveDefaultPath();
            Load();
        }

        private static string ResolveDefaultPath()
        {
            var env = Environment.GetEnvironmentVariable("EJLIVE_CREDENTIALS_PATH");
            if (!string.IsNullOrWhiteSpace(env))
                return env;

            var root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive");
            Directory.CreateDirectory(root);
            return Path.Combine(root, "credentials.json");
        }

        private void Load()
        {
            if (!File.Exists(_filePath))
            {
                _entries = new Dictionary<string, CredentialEntry>(StringComparer.OrdinalIgnoreCase);
                return;
            }

            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<CredentialFile>(json);
            _entries = data?.Users?.ToDictionary(
                u => u.UserId,
                u => new CredentialEntry(u.PasswordHash, Enum.Parse<UserRole>(u.Role, true)),
                StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, CredentialEntry>(StringComparer.OrdinalIgnoreCase);
        }

        private void Save()
        {
            var data = new CredentialFile
            {
                Users = _entries.Select(kvp => new CredentialUser
                {
                    UserId = kvp.Key,
                    PasswordHash = kvp.Value.PasswordHash,
                    Role = kvp.Value.Role.ToString()
                }).ToList()
            };
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public void AddUser(string userId, string password, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters.", nameof(password));

            lock (_lock)
            {
                _entries[userId] = new CredentialEntry(SecurityHelper.HashPassword(password), role);
                Save();
            }
        }

        public void RemoveUser(string userId)
        {
            lock (_lock)
            {
                if (_entries.Remove(userId))
                    Save();
            }
        }

        public bool ValidateCredentials(string userId, string password)
        {
            lock (_lock)
            {
                return _entries.TryGetValue(userId, out var entry) &&
                       SecurityHelper.VerifyPassword(password, entry.PasswordHash);
            }
        }

        public UserRole? GetRole(string userId)
        {
            lock (_lock)
            {
                return _entries.TryGetValue(userId, out var entry) ? entry.Role : null;
            }
        }

        public bool UserExists(string userId)
        {
            lock (_lock)
            {
                return _entries.ContainsKey(userId);
            }
        }

        private sealed record CredentialEntry(string PasswordHash, UserRole Role);

        private sealed class CredentialFile
        {
            public List<CredentialUser> Users { get; set; } = new();
        }

        private sealed class CredentialUser
        {
            public string UserId { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
    }

    /// <summary>
    /// نظام صلاحيات المستخدمين (Role-Based Access Control)
    /// يطبق: L-12 (RBAC) — 4 أدوار: Observer / Admin / Auditor / Support
    /// </summary>
    public class RoleBasedAccess
    {
        private readonly Dictionary<string, HashSet<string>> _permissions = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Admin"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "*" },
            ["Auditor"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "view", "export" },
            ["Support"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "view", "remote", "sync" },
            ["Observer"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "view" }
        };

        private static readonly Dictionary<string, UserSession> _activeSessions =
            new Dictionary<string, UserSession>(StringComparer.OrdinalIgnoreCase);

        private static readonly object _lock = new object();
        private static ICredentialStore _credentialStore = new InMemoryCredentialStore();

        /// <summary>
        /// Sets the credential store implementation. Must be called before Login.
        /// </summary>
        public static void SetCredentialStore(ICredentialStore store)
        {
            _credentialStore = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <summary>
        /// Seeds demo credentials for test and development scenarios ONLY.
        /// Never call this in production.
        /// </summary>
        public static void SeedDemoCredentials()
        {
            if (_credentialStore is not InMemoryCredentialStore mem)
                _credentialStore = new InMemoryCredentialStore();

            var store = (InMemoryCredentialStore)_credentialStore;
            store.AddUser("admin", "AdminPass#2026", UserRole.Admin);
            store.AddUser("observer", "ObserverPass#2026", UserRole.Observer);
            store.AddUser("auditor", "AuditorPass#2026", UserRole.Auditor);
            store.AddUser("support", "SupportPass#2026", UserRole.Support);
        }

        public static LoginResult Login(string userId, string password, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
                return new LoginResult { Success = false, Message = "بيانات الدخول غير مكتملة" };

            lock (_lock)
            {
                if (!_credentialStore.UserExists(userId))
                    return new LoginResult { Success = false, Message = "المستخدم غير موجود" };

                if (!_credentialStore.ValidateCredentials(userId, password))
                    return new LoginResult { Success = false, Message = "كلمة السر غير صحيحة" };

                var role = _credentialStore.GetRole(userId);
                if (role == null)
                    return new LoginResult { Success = false, Message = "الدور غير محدد" };

                var token = Guid.NewGuid().ToString("N");
                var session = new UserSession
                {
                    Token = token,
                    UserId = userId,
                    Role = role.Value,
                    LoginTime = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow,
                    IpAddress = ipAddress ?? "127.0.0.1"
                };

                _activeSessions[token] = session;
                AuditLogger.LogLogin(userId, ipAddress ?? "127.0.0.1", true);

                return new LoginResult
                {
                    Success = true,
                    Token = token,
                    Role = role.Value,
                    Message = $"مرحبًا {userId} — دورك: {role.Value}"
                };
            }
        }

        public static void Logout(string token)
        {
            lock (_lock)
            {
                if (_activeSessions.TryGetValue(token, out var session))
                {
                    AuditLogger.Log(AuditAction.Logout, session.UserId, null, "User logged out", true);
                    _activeSessions.Remove(token);
                }
            }
        }

        public static bool HasPermission(string token, Permission permission)
        {
            lock (_lock)
            {
                if (!_activeSessions.TryGetValue(token, out var session))
                    return false;

                // تحديث النشاط
                session.LastActivity = DateTime.UtcNow;

                return RolePermissions.TryGetValue(session.Role, out var perms) &&
                       perms.Contains(permission);
            }
        }

        public static UserSession? GetSession(string token)
        {
            lock (_lock)
            {
                _activeSessions.TryGetValue(token, out var s);
                return s;
            }
        }

        public static void CleanExpiredSessions(TimeSpan maxIdle)
        {
            var cutoff = DateTime.UtcNow - maxIdle;
            lock (_lock)
            {
                var expired = new List<string>();
                foreach (var kv in _activeSessions)
                    if (kv.Value.LastActivity < cutoff)
                        expired.Add(kv.Key);
                foreach (var t in expired)
                    _activeSessions.Remove(t);
            }
        }

        // ==========================================
        // جدول الصلاحيات حسب الدور
        // ==========================================

        private static readonly Dictionary<UserRole, HashSet<Permission>> RolePermissions =
            new Dictionary<UserRole, HashSet<Permission>>
        {
            [UserRole.Observer] = new HashSet<Permission>
            {
                Permission.ViewDashboard,
                Permission.ViewATMStatus,
                Permission.ViewJournal,
                Permission.ViewAlerts,
            },
            [UserRole.Support] = new HashSet<Permission>
            {
                Permission.ViewDashboard,
                Permission.ViewATMStatus,
                Permission.ViewJournal,
                Permission.ViewAlerts,
                Permission.TakeScreenshot,
                Permission.RemoteSessionView,
                Permission.ViewLogs,
            },
            [UserRole.Auditor] = new HashSet<Permission>
            {
                Permission.ViewDashboard,
                Permission.ViewATMStatus,
                Permission.ViewJournal,
                Permission.ViewAlerts,
                Permission.ViewLogs,
                Permission.ExportReports,
                Permission.ViewAuditLog,
            },
            [UserRole.Admin] = new HashSet<Permission>
            {
                Permission.ViewDashboard,
                Permission.ViewATMStatus,
                Permission.ViewJournal,
                Permission.ViewAlerts,
                Permission.ViewLogs,
                Permission.ExportReports,
                Permission.ViewAuditLog,
                Permission.TakeScreenshot,
                Permission.RemoteSessionView,
                Permission.SendCommands,
                Permission.RestartATM,
                Permission.ChangePassword,
                Permission.SyncImages,
                Permission.ManageUsers,
                Permission.ChangeConfig,
                Permission.ForceSync,
            },
        };

        public bool Can(string role, string permission)
        {
            return _permissions.TryGetValue(role, out var permissions) &&
                   (permissions.Contains("*") || permissions.Contains(permission));
        }
    }

    public class UserSession
    {
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LastActivity { get; set; }
        public string IpAddress { get; set; } = string.Empty;
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public enum UserRole
    {
        Observer,   // عرض فقط
        Support,    // تشخيص محدود
        Auditor,    // تدقيق وتقارير
        Admin       // تحكم كامل
    }

    public enum Permission
    {
        ViewDashboard,
        ViewATMStatus,
        ViewJournal,
        ViewAlerts,
        ViewLogs,
        ExportReports,
        ViewAuditLog,
        TakeScreenshot,
        RemoteSessionView,
        SendCommands,
        RestartATM,
        ChangePassword,
        SyncImages,
        ManageUsers,
        ChangeConfig,
        ForceSync
    }
}
