using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EJLive.Core
{
    /// <summary>
    /// Canonical facade over <see cref="EJLive.Shared.AppConstants"/> (finding E-16): the
    /// pre-wave-4 copy had drifted down to seven members while ten assemblies kept calling
    /// the full historical surface (CS0117 everywhere the enclosing namespace preferred this
    /// class over the L0 original). This facade re-exports the complete surface — const for
    /// const — so <c>EJLive.Core.*</c> files resolve every member without a using directive
    /// and the literal values live in exactly one place. The retired Core-only members
    /// (AppName-style duplicates, <c>HeartbeatIntervalSeconds</c>/<c>HeartbeatTimeoutSeconds</c>
    /// with the *Seconds suffix) were unified onto the Shared *Sec names; the one consumer of
    /// a custom 95 s timeout (<c>ClientMainForm</c>) declares its own local const.
    /// </summary>
    public static class AppConstants
    {
        // ── Identity ──────────────────────────────────────────────────────────
        public const string AppName = EJLive.Shared.AppConstants.AppName;
        public const string AppVersion = EJLive.Shared.AppConstants.AppVersion;
        public const string ProtocolVersion = EJLive.Shared.AppConstants.ProtocolVersion;

        // ── Network and timing ────────────────────────────────────────────────
        public const int DefaultPort = EJLive.Shared.AppConstants.DefaultPort;
        public const int HeartbeatIntervalSec = EJLive.Shared.AppConstants.HeartbeatIntervalSec;
        public const int HeartbeatTimeoutSec = EJLive.Shared.AppConstants.HeartbeatTimeoutSec;
        public const int SocketTimeoutMs = EJLive.Shared.AppConstants.SocketTimeoutMs;
        public const int ChunkSizeBytes = EJLive.Shared.AppConstants.ChunkSizeBytes;
        public const int OutboxMaxItems = EJLive.Shared.AppConstants.OutboxMaxItems;

        public static readonly string[] SupportedVendors = EJLive.Shared.AppConstants.SupportedVendors;
        public static readonly string[] PolicyGovernedRemoteCommands = EJLive.Shared.AppConstants.PolicyGovernedRemoteCommands;

        // ── Vendor codes ───────────────────────────────────────────────────────
        public const string ATM_TYPE_NCR = EJLive.Shared.AppConstants.ATM_TYPE_NCR;
        public const string ATM_TYPE_GRG = EJLive.Shared.AppConstants.ATM_TYPE_GRG;
        public const string ATM_TYPE_WN = EJLive.Shared.AppConstants.ATM_TYPE_WN;
        public const string ATM_TYPE_DN = EJLive.Shared.AppConstants.ATM_TYPE_DN;
        public const string ATM_TYPE_HY = EJLive.Shared.AppConstants.ATM_TYPE_HY;
        public const string ATM_TYPE_CW = EJLive.Shared.AppConstants.ATM_TYPE_CW;
        public const string ATM_TYPE_WINCOR = EJLive.Shared.AppConstants.ATM_TYPE_WINCOR;
        public const string ATM_TYPE_DIEBOLD = EJLive.Shared.AppConstants.ATM_TYPE_DIEBOLD;
        public const string ATM_TYPE_HYOSUNG = EJLive.Shared.AppConstants.ATM_TYPE_HYOSUNG;

        // ── Vendor journal/backup locations and NCR file names ────────────────
        public const string NCR_JournalPath = EJLive.Shared.AppConstants.NCR_JournalPath;
        public const string NCR_BackupPath = EJLive.Shared.AppConstants.NCR_BackupPath;
        public const string NCR_EJData = EJLive.Shared.AppConstants.NCR_EJData;
        public const string NCR_EJRcpy = EJLive.Shared.AppConstants.NCR_EJRcpy;
        public const string NCR_EJDataLob = EJLive.Shared.AppConstants.NCR_EJDataLob;
        public const string GRG_JournalPath = EJLive.Shared.AppConstants.GRG_JournalPath;
        public const string GRG_BackupPath = EJLive.Shared.AppConstants.GRG_BackupPath;
        public const string WN_JournalPath = EJLive.Shared.AppConstants.WN_JournalPath;
        public const string WN_BackupPath = EJLive.Shared.AppConstants.WN_BackupPath;
        public const string DN_JournalPath = EJLive.Shared.AppConstants.DN_JournalPath;
        public const string DN_BackupPath = EJLive.Shared.AppConstants.DN_BackupPath;
        public const string HY_JournalPath = EJLive.Shared.AppConstants.HY_JournalPath;
        public const string HY_BackupPath = EJLive.Shared.AppConstants.HY_BackupPath;
        public const string CW_JournalPath = EJLive.Shared.AppConstants.CW_JournalPath;
        public const string CW_BackupPath = EJLive.Shared.AppConstants.CW_BackupPath;

        // ── Data-root derived paths ───────────────────────────────────────────
        public static string DefaultDataRootPath => EJLive.Shared.DataRootPaths.Root;
        public static string DefaultLogPath => EJLive.Shared.AppConstants.DefaultLogPath;
        public static string DefaultDatabasePath => EJLive.Shared.AppConstants.DefaultDatabasePath;
        public static string DefaultClientOutboxPath => EJLive.Shared.AppConstants.DefaultClientOutboxPath;
        public static string DefaultClientInboxPath => EJLive.Shared.AppConstants.DefaultClientInboxPath;
        public static string DefaultReportsPath => EJLive.Shared.AppConstants.DefaultReportsPath;
        public static string DefaultArchivePath => EJLive.Shared.AppConstants.DefaultArchivePath;
        public static string DefaultImagesPath => EJLive.Shared.AppConstants.DefaultImagesPath;
        public static string DefaultServerSharePath => EJLive.Shared.AppConstants.DefaultServerSharePath;
        public static string ShareImagesAllPath => EJLive.Shared.AppConstants.ShareImagesAllPath;
        public static string ShareImagesByTypePath => EJLive.Shared.AppConstants.ShareImagesByTypePath;
        public static string ShareImagesStagingPath => EJLive.Shared.AppConstants.ShareImagesStagingPath;
        public static string[] GetServerImageShareFolders() => EJLive.Shared.AppConstants.GetServerImageShareFolders();

        // ── Fleet alert thresholds ────────────────────────────────────────────
        public const int AlertDisconnectWarningMin = EJLive.Shared.AppConstants.AlertDisconnectWarningMin;
        public const int AlertDisconnectCriticalMin = EJLive.Shared.AppConstants.AlertDisconnectCriticalMin;
        public const int AlertNoDataWarningMin = EJLive.Shared.AppConstants.AlertNoDataWarningMin;
        public const int AlertNoDataCriticalMin = EJLive.Shared.AppConstants.AlertNoDataCriticalMin;

        // ── Remote command verbs ──────────────────────────────────────────────
        public const string CMD_PING = EJLive.Shared.AppConstants.CMD_PING;
        public const string CMD_SYNC_TIME = EJLive.Shared.AppConstants.CMD_SYNC_TIME;
        public const string CMD_SCREENSHOT = EJLive.Shared.AppConstants.CMD_SCREENSHOT;
        public const string CMD_GHOST_START = EJLive.Shared.AppConstants.CMD_GHOST_START;
        public const string CMD_GHOST_STOP = EJLive.Shared.AppConstants.CMD_GHOST_STOP;
        public const string CMD_FORCE_SYNC = EJLive.Shared.AppConstants.CMD_FORCE_SYNC;
        public const string CMD_GET_STATS = EJLive.Shared.AppConstants.CMD_GET_STATS;
        public const string CMD_GET_SYSINFO = EJLive.Shared.AppConstants.CMD_GET_SYSINFO;
        public const string CMD_GET_FILE = EJLive.Shared.AppConstants.CMD_GET_FILE;
        public const string CMD_SEND_FILE = EJLive.Shared.AppConstants.CMD_SEND_FILE;
        public const string CMD_SEND_IMAGE = EJLive.Shared.AppConstants.CMD_SEND_IMAGE;
        public const string CMD_SYNC_IMAGES = EJLive.Shared.AppConstants.CMD_SYNC_IMAGES;
        public const string CMD_SYNC_FOLDER = EJLive.Shared.AppConstants.CMD_SYNC_FOLDER;
        public const string CMD_RESTART = EJLive.Shared.AppConstants.CMD_RESTART;
        public const string CMD_SHUTDOWN = EJLive.Shared.AppConstants.CMD_SHUTDOWN;
        public const string CMD_CHANGE_PASSWORD = EJLive.Shared.AppConstants.CMD_CHANGE_PASSWORD;
        public const string CMD_REMOTE_CONFIG = EJLive.Shared.AppConstants.CMD_REMOTE_CONFIG;
        public const string CMD_REMOTE_SESSION_START = EJLive.Shared.AppConstants.CMD_REMOTE_SESSION_START;
        public const string CMD_REMOTE_SESSION_STOP = EJLive.Shared.AppConstants.CMD_REMOTE_SESSION_STOP;
        public const string CMD_WINDOWS_REMOTE_START = EJLive.Shared.AppConstants.CMD_WINDOWS_REMOTE_START;
        public const string CMD_WINDOWS_REMOTE_STOP = EJLive.Shared.AppConstants.CMD_WINDOWS_REMOTE_STOP;
        public const string CMD_WINDOWS_REMOTE_CHECK = EJLive.Shared.AppConstants.CMD_WINDOWS_REMOTE_CHECK;

        public static readonly string[] CommandsRequireConfirmation = EJLive.Shared.AppConstants.CommandsRequireConfirmation;

        // ── Legacy pipe-delimited protocol verbs ──────────────────────────────
        public const string MSG_HANDSHAKE = EJLive.Shared.AppConstants.MSG_HANDSHAKE;
        public const string MSG_ACK = EJLive.Shared.AppConstants.MSG_ACK;
        public const string MSG_HEARTBEAT = EJLive.Shared.AppConstants.MSG_HEARTBEAT;
        public const string MSG_HB_ACK = EJLive.Shared.AppConstants.MSG_HB_ACK;
        public const string MSG_JOURNAL_ACK = EJLive.Shared.AppConstants.MSG_JOURNAL_ACK;
        public const string MSG_CMD_RESULT = EJLive.Shared.AppConstants.MSG_CMD_RESULT;

        // ── Helpers ───────────────────────────────────────────────────────────
        public static string[] GetSupportedATMTypes() => EJLive.Shared.AppConstants.GetSupportedATMTypes();
        public static string NormalizeATMType(string? type) => EJLive.Shared.AppConstants.NormalizeATMType(type);
        public static string GetDefaultSourcePath(string atmType) => EJLive.Shared.AppConstants.GetDefaultSourcePath(atmType);
        public static string GetDefaultBackupPath(string atmType) => EJLive.Shared.AppConstants.GetDefaultBackupPath(atmType);
        public static string[] GetClientRuntimeFolders(string atmType, string sourcePath, string backupPath) =>
            EJLive.Shared.AppConstants.GetClientRuntimeFolders(atmType, sourcePath, backupPath);
    }

    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical,
        Emergency
    }

    public enum AtmRuntimeStatus
    {
        Unknown,
        Online,
        Warning,
        Offline,
        OutOfService,
        Supervisor,
        Maintenance
    }

    public sealed class ATMInfo
    {
        public string ATM_ID { get; set; } = string.Empty;
        public string TerminalId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Vendor { get; set; } = "Generic";
        public string Model { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = AppConstants.DefaultPort;
        public AtmRuntimeStatus RuntimeStatus { get; set; } = AtmRuntimeStatus.Unknown;
        public DateTime LastHeartbeatUtc { get; set; } = DateTime.MinValue;
        public bool HasAlerts { get; set; }

        public string ATMId
        {
            get => string.IsNullOrWhiteSpace(ATM_ID) ? TerminalId : ATM_ID;
            set
            {
                ATM_ID = value ?? string.Empty;
                TerminalId = value ?? string.Empty;
            }
        }

        public string Status
        {
            get => RuntimeStatus.ToString();
            set => RuntimeStatus = ParseStatus(value);
        }

        private static AtmRuntimeStatus ParseStatus(string value)
        {
            if (Enum.TryParse(value ?? string.Empty, true, out AtmRuntimeStatus status))
            {
                return status;
            }

            if (string.Equals(value, "online", StringComparison.OrdinalIgnoreCase))
            {
                return AtmRuntimeStatus.Online;
            }

            if (string.Equals(value, "offline", StringComparison.OrdinalIgnoreCase))
            {
                return AtmRuntimeStatus.Offline;
            }

            if (string.Equals(value, "maintenance", StringComparison.OrdinalIgnoreCase))
            {
                return AtmRuntimeStatus.Maintenance;
            }

            return AtmRuntimeStatus.Unknown;
        }
    }

    public sealed class AlertPayload
    {
        public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
        public string ATM_ID { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; } = AlertSeverity.Info;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; } = "System";
        public string DedupeKey { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
    }

    public sealed class JournalSyncRecord
    {
        public string AtmId { get; set; } = string.Empty;
        public string Vendor { get; set; } = "Generic";
        public string SourcePath { get; set; } = string.Empty;
        public string Checksum { get; set; } = string.Empty;
        public long Offset { get; set; }
        public DateTime LastSyncedUtc { get; set; } = DateTime.UtcNow;
        public string State { get; set; } = "Pending";
    }

    public sealed class RemoteCommandEnvelope
    {
        public string CommandId { get; set; } = Guid.NewGuid().ToString("N");
        public string CommandType { get; set; } = string.Empty;
        public string TargetAtmId { get; set; } = string.Empty;
        public string OperatorId { get; set; } = string.Empty;
        public string OperatorRole { get; set; } = string.Empty;
        public bool Approved { get; set; }
        public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
        public string Nonce { get; set; } = Guid.NewGuid().ToString("N");
        public string Signature { get; set; } = string.Empty;
    }

    public sealed class RemoteCommandPolicyResult
    {
        public bool Allowed { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public sealed class UnifiedRemoteCommandPolicy
    {
        private static readonly HashSet<string> AllowedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Admin",
            "Supervisor",
            "Support"
        };

        private static readonly HashSet<string> AllowedCommands = new HashSet<string>(
            AppConstants.PolicyGovernedRemoteCommands,
            StringComparer.OrdinalIgnoreCase);

        public RemoteCommandPolicyResult Validate(RemoteCommandEnvelope command)
        {
            if (command == null)
            {
                return Deny("Command envelope is required.");
            }

            if (!AllowedCommands.Contains(command.CommandType ?? string.Empty))
            {
                return Deny("Command is not allowlisted.");
            }

            if (!AllowedRoles.Contains(command.OperatorRole ?? string.Empty))
            {
                return Deny("Operator role is not authorized.");
            }

            if (!command.Approved)
            {
                return Deny("Approval is required before dispatch.");
            }

            if (DateTimeOffset.UtcNow - command.TimestampUtc > TimeSpan.FromMinutes(5))
            {
                return Deny("Command timestamp is stale.");
            }

            if (string.IsNullOrWhiteSpace(command.Nonce))
            {
                return Deny("Replay-protection nonce is required.");
            }

            return new RemoteCommandPolicyResult { Allowed = true, Reason = "Policy check passed." };
        }

        private static RemoteCommandPolicyResult Deny(string reason)
        {
            return new RemoteCommandPolicyResult { Allowed = false, Reason = reason };
        }
    }

    public static class SecretRedactor
    {
        public static string MaskCard(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var digits = new string(value.Where(char.IsDigit).ToArray());
            if (digits.Length < 10)
            {
                return "****";
            }

            return digits.Substring(0, 6) + "****" + digits.Substring(digits.Length - 4);
        }
    }
}

namespace EJLive.Core.Models
{
    public sealed class UnifiedSystemConfiguration
    {
        public string ServerHost { get; set; } = "127.0.0.1";
        public int ServerPort { get; set; } = EJLive.Core.AppConstants.DefaultPort;
        public string DataRoot { get; set; } = EJLive.Shared.DataRootPaths.Root;
        public bool RemoteOperationsEnabled { get; set; }
    }

    // EJLive.Core.Models.DatabaseManager (a demo store with hard-coded metrics) was removed in Wave 4:
    // every consumer of the data layer binds to EJLive.Core.Services.DatabaseManager; a second
    // type of the same name in this assembly only created CS0104 ambiguity for files that import both namespaces.

}
