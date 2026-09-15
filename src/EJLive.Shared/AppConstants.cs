using System;
using IO = System.IO;

namespace EJLive.Shared
{
    /// <summary>
    /// Central application constants — the single canonical home for the values every
    /// EJLive component shares (L0). <c>EJLive.Core.AppConstants</c> is a same-surface
    /// facade over this class so files inside the <c>EJLive.Core.*</c> namespaces resolve
    /// the identical values without a using directive (finding E-16: the facade had lost
    /// 46 members that 10 assemblies reference, which made the compiled set unbuildable).
    ///
    /// All runtime paths derive from <see cref="DataRootPaths"/> — one fixed, central
    /// <c>dataroot</c> per machine, overridable once via the <c>EJLIVE_DATAROOT</c>
    /// environment variable. No project may hard-code <c>C:\ProgramData\EJLive</c>.
    ///
    /// Wave 4 (SS-11/SS-20): rebuilt by cross-referencing every consumer in the compiled
    /// set (parsers, vendor path registry, remote-command policy, installer runners, the
    /// NOC console and the verification harness). Legacy values were preserved wherever a
    /// consumer depends on the exact string; the two recorded corrections are the
    /// <c>CMD_WINDOWS_REMOTE_CHECK</c> value (it equalled the member name instead of the
    /// wire verb) and vendor-code coverage (GRG/Cashway/short codes were unresolvable).
    /// </summary>
    public static class AppConstants
    {
        // ─────────────────────────────────────────────────────────────────────
        // Identity and versions
        // ─────────────────────────────────────────────────────────────────────
        public const string AppName = "EJLive Enterprise";
        public const string AppVersion = "4.0.0";
        public const string ProtocolVersion = "EJ-4.0";

        // ─────────────────────────────────────────────────────────────────────
        // Network and timing
        // ─────────────────────────────────────────────────────────────────────
        public const int DefaultPort = 5656;
        public const int HeartbeatIntervalSec = 30;
        public const int HeartbeatTimeoutSec = 90;
        public const int SocketTimeoutMs = 5000;
        public const int ChunkSizeBytes = 64 * 1024;
        public const int OutboxMaxItems = 1000;

        /// <summary>Vendor list shown across consoles and the installer self-test.</summary>
        public static readonly string[] SupportedVendors =
        {
            "NCR",
            "GRG",
            "Wincor",
            "Diebold",
            "Hyosung",
            "Cashway",
            "Generic"
        };

        /// <summary>Commands the agent executor will only run through the policy pipeline.</summary>
        public static readonly string[] PolicyGovernedRemoteCommands =
        {
            "Ping",
            "ForceSync",
            "CollectDiagnostics",
            "RestartService",
            "CaptureScreenshot",
            "RetrieveFile"
        };

        // ─────────────────────────────────────────────────────────────────────
        // Vendor codes (canonical strings; the parser/registry keys are case-insensitive)
        // ─────────────────────────────────────────────────────────────────────
        public const string ATM_TYPE_NCR = "NCR";
        public const string ATM_TYPE_GRG = "GRG";
        public const string ATM_TYPE_WN = "Wincor";
        public const string ATM_TYPE_DN = "Diebold";
        public const string ATM_TYPE_HY = "Hyosung";
        public const string ATM_TYPE_CW = "Cashway";

        // Legacy long-name aliases — same values, kept so every historical call site
        // (installer UI, client console) binds to one source of truth.
        public const string ATM_TYPE_WINCOR = ATM_TYPE_WN;
        public const string ATM_TYPE_DIEBOLD = ATM_TYPE_DN;
        public const string ATM_TYPE_HYOSUNG = ATM_TYPE_HY;

        // ─────────────────────────────────────────────────────────────────────
        // Default vendor journal/backup locations on the endpoint (ATM drive layout).
        // These are capture paths, not dataroot paths — they intentionally stay on the
        // machine-local journal folders each vendor writes to.
        // ─────────────────────────────────────────────────────────────────────
        public const string NCR_JournalPath = @"C:\EJ\Journals";
        public const string NCR_BackupPath = @"C:\EJ\Backup";
        public const string NCR_EJData = "EJDATA.LOG";
        public const string NCR_EJRcpy = "EJRCPY.LOG";
        public const string NCR_EJDataLob = "EJDATA.LOB";

        public const string GRG_JournalPath = @"C:\ATM\Journals";
        public const string GRG_BackupPath = @"C:\ATM\Backup";
        public const string WN_JournalPath = @"E:\Wincor\Journals";
        public const string WN_BackupPath = @"E:\Wincor\Backup";
        public const string DN_JournalPath = @"D:\Diebold\Logs";
        public const string DN_BackupPath = @"D:\Diebold\Backup";
        public const string HY_JournalPath = @"C:\ATM\Journals";
        public const string HY_BackupPath = @"C:\ATM\Backup";
        public const string CW_JournalPath = @"C:\ATM\Journals";
        public const string CW_BackupPath = @"C:\ATM\Backup";

        // ─────────────────────────────────────────────────────────────────────
        // Data-root derived paths (fixed central layout, see DataRootPaths)
        // ─────────────────────────────────────────────────────────────────────
        public static string DefaultDataRootPath => DataRootPaths.Root;
        public static string DefaultLogPath => DataRootPaths.LogsDirectory;
        public static string DefaultDatabasePath => DataRootPaths.DatabaseFile;
        public static string DefaultClientOutboxPath => DataRootPaths.OutboxDirectory;
        public static string DefaultClientInboxPath => DataRootPaths.InboxDirectory;
        public static string DefaultReportsPath => DataRootPaths.ReportsDirectory;
        public static string DefaultArchivePath => DataRootPaths.ArchiveDirectory;
        public static string DefaultImagesPath => DataRootPaths.ImagesDirectory;
        public static string DefaultServerSharePath => DataRootPaths.ShareDirectory;
        public static string ShareImagesAllPath => DataRootPaths.ShareImagesAllPath;
        public static string ShareImagesByTypePath => DataRootPaths.ShareImagesByTypePath;
        public static string ShareImagesStagingPath => DataRootPaths.ShareImagesStagingPath;

        /// <summary>Server-side image distribution folders (created by the server at startup).</summary>
        public static string[] GetServerImageShareFolders() => new[]
        {
            ShareImagesAllPath,
            ShareImagesByTypePath,
            ShareImagesStagingPath
        };

        // ─────────────────────────────────────────────────────────────────────
        // Fleet alert thresholds (minutes of staleness before a risk contribution)
        // ─────────────────────────────────────────────────────────────────────
        public const int AlertDisconnectWarningMin = 10;
        public const int AlertDisconnectCriticalMin = 30;
        public const int AlertNoDataWarningMin = 30;
        public const int AlertNoDataCriticalMin = 120;

        // ─────────────────────────────────────────────────────────────────────
        // Remote control commands (wire verbs; the agent compares these case-insensitively)
        // ─────────────────────────────────────────────────────────────────────
        public const string CMD_PING = "PING";
        public const string CMD_SYNC_TIME = "SYNC_TIME";
        public const string CMD_SCREENSHOT = "SCREENSHOT";
        public const string CMD_GHOST_START = "GHOST_START";
        public const string CMD_GHOST_STOP = "GHOST_STOP";
        public const string CMD_FORCE_SYNC = "FORCE_SYNC";
        public const string CMD_GET_STATS = "GET_STATS";
        public const string CMD_GET_SYSINFO = "GET_SYSINFO";
        public const string CMD_GET_FILE = "GET_FILE";
        public const string CMD_SEND_FILE = "SEND_FILE";
        public const string CMD_SEND_IMAGE = "SEND_IMAGE";
        public const string CMD_SYNC_IMAGES = "SYNC_IMAGES";
        public const string CMD_SYNC_FOLDER = "SYNC_FOLDER";
        public const string CMD_RESTART = "RESTART";
        public const string CMD_SHUTDOWN = "SHUTDOWN";
        public const string CMD_CHANGE_PASSWORD = "CHANGE_PASSWORD";
        public const string CMD_REMOTE_CONFIG = "REMOTE_CONFIG";
        public const string CMD_REMOTE_SESSION_START = "REMOTE_SESSION_START";
        public const string CMD_REMOTE_SESSION_STOP = "REMOTE_SESSION_STOP";
        public const string CMD_WINDOWS_REMOTE_START = "WINDOWS_REMOTE_START";
        public const string CMD_WINDOWS_REMOTE_STOP = "WINDOWS_REMOTE_STOP";

        // Correction (wave 4): this value used to be the literal "CMD_WINDOWS_REMOTE_CHECK"
        // (the member name echoed into the string). Every consumer compares the constant, so
        // the wire verb is now the canonical upper-snake form shared by its siblings.
        public const string CMD_WINDOWS_REMOTE_CHECK = "WINDOWS_REMOTE_CHECK";

        // ─────────────────────────────────────────────────────────────────────
        // Legacy pipe-delimited protocol verbs (Constants.Protocol facade)
        // ─────────────────────────────────────────────────────────────────────
        public const string MSG_HANDSHAKE = "HANDSHAKE";
        public const string MSG_ACK = "ACK";
        public const string MSG_HEARTBEAT = "HEARTBEAT";
        public const string MSG_HB_ACK = "HB_ACK";
        public const string MSG_JOURNAL_ACK = "JOURNAL_ACK";
        public const string MSG_CMD_RESULT = "CMD_RESULT";

        // ─────────────────────────────────────────────────────────────────────
        // Confirmation policy
        // ─────────────────────────────────────────────────────────────────────
        public static readonly string[] CommandsRequireConfirmation =
        {
            CMD_RESTART, CMD_SHUTDOWN, CMD_CHANGE_PASSWORD
        };

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────
        public static string[] GetSupportedATMTypes()
        {
            return new[] { ATM_TYPE_NCR, ATM_TYPE_DN, ATM_TYPE_WN, ATM_TYPE_HY };
        }

        /// <summary>
        /// Maps any historical spelling (long name, short code, legacy alias) onto the
        /// canonical vendor code. Unknown input maps to NCR — the shipped default — matching
        /// the pre-wave-4 behaviour so no profile lookup silently changes vendors.
        /// </summary>
        public static string NormalizeATMType(string? type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return ATM_TYPE_NCR;

            var normalized = type.Trim().ToUpperInvariant();
            return normalized switch
            {
                "NCR" => ATM_TYPE_NCR,
                "GRG" => ATM_TYPE_GRG,
                "WINCOR" or "WN" or "NIXDORF" or "WINCOR NIXDORF" => ATM_TYPE_WN,
                "DIEBOLD" or "DN" or "DIEBOLDNIXDORF" or "DIEBOLD NIXDORF" => ATM_TYPE_DN,
                "HYOSUNG" or "HY" => ATM_TYPE_HY,
                "CASHWAY" or "CW" => ATM_TYPE_CW,
                _ => ATM_TYPE_NCR
            };
        }

        public static string GetDefaultSourcePath(string atmType)
        {
            return NormalizeATMType(atmType) switch
            {
                ATM_TYPE_NCR => NCR_JournalPath,
                ATM_TYPE_GRG => GRG_JournalPath,
                ATM_TYPE_WN => WN_JournalPath,
                ATM_TYPE_DN => DN_JournalPath,
                ATM_TYPE_HY => HY_JournalPath,
                ATM_TYPE_CW => CW_JournalPath,
                _ => GRG_JournalPath
            };
        }

        public static string GetDefaultBackupPath(string atmType)
        {
            return NormalizeATMType(atmType) switch
            {
                ATM_TYPE_NCR => NCR_BackupPath,
                ATM_TYPE_GRG => GRG_BackupPath,
                ATM_TYPE_WN => WN_BackupPath,
                ATM_TYPE_DN => DN_BackupPath,
                ATM_TYPE_HY => HY_BackupPath,
                ATM_TYPE_CW => CW_BackupPath,
                _ => GRG_BackupPath
            };
        }

        public static string[] GetClientRuntimeFolders(string atmType, string sourcePath, string backupPath)
        {
            return new[]
            {
                DefaultClientOutboxPath,
                DefaultClientInboxPath,
                DefaultLogPath,
                DefaultReportsPath,
                sourcePath,
                backupPath,
                IO.Path.Combine(backupPath, "Screenshots"),
                IO.Path.Combine(backupPath, "MonthlyArchive")
            };
        }
    }

    /// <summary>
    /// Network tuning constants. Kept separate from <see cref="AppConstants"/> because
    /// the transport engines import these without the identity block.
    /// </summary>
    public static class NetworkConfig
    {
        public const int PING_TIMEOUT_MS = 2000;
        public const int CONNECT_TIMEOUT_MS = 5000;
        public const int SEND_TIMEOUT_MS = 10000;
        public const int RECEIVE_TIMEOUT_MS = 10000;
        public const int MAX_RETRY_ATTEMPTS = 3;
        public const int RETRY_BASE_DELAY_MS = 200;
        public const int RETRY_MAX_DELAY_MS = 3000;
        public const int BUFFER_SIZE = 65536;
    }
}
