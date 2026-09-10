namespace EJLive.Core;

public static class AppConstants
{
    public const string AppName = "EJLive Enterprise";
    public const string AppVersion = "4.0.0";
    public const string APP_VERSION = AppVersion;
    public const string BuildDate = "2026-05";
    public const string SupportEmail = "support@ejlive.com";

    public const int DefaultPort = 5656;
    public const int HeartbeatIntervalSec = 30;
    public const int HeartbeatTimeoutSec = 95;
    public const int ChunkSizeBytes = 65536;
    public const int MaxConcurrentClients = 1000;
    public const int SocketTimeoutMs = 30000;
    public const int ReconnectBaseMs = 1000;
    public const int ReconnectMaxMs = 60000;
    public const string ProtocolVersion = "EJLv4";

    public const int AlertDisconnectWarningMin = 5;
    public const int AlertDisconnectCriticalMin = 10;
    public const int AlertNoDataWarningMin = 60;
    public const int AlertNoDataCriticalMin = 240;

    public const string NCR_JournalPath = @"C:\Program Files\NCR APATRA\Advance NDC\Data\";
    public const string NCR_EJData = "EJDATA.LOG";
    public const string NCR_EJRcpy = "EJRCPY.LOG";
    public const string NCR_EJDataLob = "EJDATA.LOb";
    public const string NCR_BackupPath = @"C:\NCR_BackupLog\";

    public const string GRG_JournalPath = @"D:\Program Files\DTATMW\Bin\ATMAPP\Log\";
    public const string GRG_FilePattern = "EJ_*.dat";
    public const string GRG_TracePattern = "TRACE*";
    public const string GRG_BackupPath = @"D:\GRG_BackupLog\";

    public const string WN_JournalPath = @"C:\journal\";
    public const string WN_EJPattern = "*.ej";
    public const string WN_LogPattern = "*.log";
    public const string WN_BackupPath = @"C:\WN_BackupLog\";

    public const string DN_JournalPath = @"C:\Diebold\EJ\";
    public const string DN_EJPattern = "*.jrn";
    public const string DN_LogPattern = "*.log";
    public const string DN_BackupPath = @"C:\Diebold_BackupLog\";

    public const string HY_JournalPath = @"C:\Hyosung\EJ\";
    public const string HY_EJPattern = "EJ_*.dat";
    public const string HY_LogPattern = "*.log";
    public const string HY_BackupPath = @"C:\Hyosung_BackupLog\";
    public const string CW_JournalPath = @"C:\Cashway\EJ\";
    public const string CW_BackupPath = @"C:\Cashway_BackupLog\";

    public const string MSG_HANDSHAKE = "EJLIVE_HANDSHAKE";
    public const string MSG_ACK = "EJLIVE_ACK";
    public const string MSG_HEARTBEAT = "HEARTBEAT";
    public const string MSG_HB_ACK = "HB_ACK";
    public const string MSG_START_FILE = "START_FILE";
    public const string MSG_CHUNK = "CHUNK";
    public const string MSG_CHUNK_ACK = "CHUNK_ACK";
    public const string MSG_COMPLETE = "COMPLETE";
    public const string MSG_JOURNAL_ACK = "JOURNAL_ACK";
    public const string MSG_CMD = "CMD";
    public const string MSG_CMD_RESULT = "CMD_RESULT";
    public const string MSG_REMOTE_SESSION_START = "REMOTE_SESSION_START_REQUEST";
    public const string MSG_REMOTE_SESSION_FRAME = "REMOTE_SESSION_FRAME";
    public const string MSG_REMOTE_SESSION_STOP = "REMOTE_SESSION_STOP";
    public const string MSG_IMAGE_SYNC = "IMAGE_SYNC";
    public const string MSG_IMAGE_ACK = "IMAGE_SYNC_ACK";
    public const string MSG_BROADCAST = "BROADCAST";
    public const string MSG_DISCONNECT = "DISCONNECT";
    public const string MSG_ERROR = "ERROR";
    public const string MSG_RSA_PUB = "RSA_PUBLIC_KEY";
    public const string MSG_AES_KEY = "AES_SESSION_KEY";

    public const string CMD_RESTART = "CMD_RESTART";
    public const string CMD_SHUTDOWN = "CMD_SHUTDOWN";
    public const string CMD_CHANGE_PASSWORD = "CMD_CHANGE_PASSWORD";
    public const string CMD_SCREENSHOT = "CMD_SCREENSHOT";
    public const string CMD_SYNC_TIME = "CMD_SYNC_TIME";
    public const string CMD_REMOTE_SESSION_START = "CMD_REMOTE_SESSION_START";
    public const string CMD_REMOTE_SESSION_STOP = "CMD_REMOTE_SESSION_STOP";
    public const string CMD_SEND_IMAGE = "CMD_SEND_IMAGE";
    public const string CMD_SYNC_IMAGES = "CMD_SYNC_IMAGES";
    public const string CMD_FORCE_SYNC = "CMD_FORCE_SYNC";
    public const string CMD_GET_FILE = "CMD_GET_FILE";
    public const string CMD_SEND_FILE = "CMD_SEND_FILE";
    public const string CMD_PING = "CMD_PING";
    public const string CMD_REMOTE_CONFIG = "CMD_REMOTE_CONFIG";
    public const string CMD_GET_STATS = "CMD_GET_STATS";
    public const string CMD_SYNC_FOLDER = "CMD_SYNC_FOLDER";
    public const string CMD_WINDOWS_REMOTE_START = "CMD_WINDOWS_REMOTE_START";
    public const string CMD_WINDOWS_REMOTE_STOP = "CMD_WINDOWS_REMOTE_STOP";
    public const string CMD_WINDOWS_REMOTE_CHECK = "CMD_WINDOWS_REMOTE_CHECK";

    public static readonly string[] CommandsRequireConfirmation =
    {
        CMD_RESTART, CMD_SHUTDOWN, CMD_CHANGE_PASSWORD, CMD_REMOTE_SESSION_START
    };

    public const string SYNC_PENDING = "Pending";
    public const string SYNC_SYNCING = "Syncing";
    public const string SYNC_RESYNCING = "ReSyncing";
    public const string SYNC_COMPLETED = "Completed";
    public const string SYNC_FAILED = "Failed";
    public const string SYNC_ARCHIVED = "Archived";

    public const int MaxRetryAttempts = 10;
    public const int OutboxMaxItems = 10000;
    public const long MaxChunkSize = 65536;
    public const long LargeFileThreshold = 5242880;
    public const int OffsetSaveInterval = 1000;

    public const string ATM_TYPE_NCR = "NCR";
    public const string ATM_TYPE_GRG = "GRG";
    public const string ATM_TYPE_WN = "WN";
    public const string ATM_TYPE_DN = "DIEBOLD";
    public const string ATM_TYPE_HY = "HYOSUNG";
    public const string ATM_TYPE_CW = "CASHWAY";

    public static string[] GetSupportedATMTypes() => new[] { ATM_TYPE_NCR, ATM_TYPE_GRG, ATM_TYPE_WN, ATM_TYPE_DN, ATM_TYPE_HY, ATM_TYPE_CW };

    public static string NormalizeATMType(string? atmType)
    {
        if (string.IsNullOrWhiteSpace(atmType))
            return ATM_TYPE_NCR;

        var value = atmType.Trim().ToUpperInvariant();
        if (value == ATM_TYPE_NCR)
            return ATM_TYPE_NCR;
        if (value == ATM_TYPE_GRG)
            return ATM_TYPE_GRG;
        if (value == ATM_TYPE_WN || value.Contains("WINCOR"))
            return ATM_TYPE_WN;
        if (value == "DN" || value.Contains("DIEBOLD") || value.Contains("NIXDORF"))
            return ATM_TYPE_DN;
        if (value == "HY" || value.Contains("HYOSUNG"))
            return ATM_TYPE_HY;
        if (value == "CW" || value.Contains("CASHWAY"))
            return ATM_TYPE_CW;
        return value;
    }

    public static string GetDefaultSourcePath(string? atmType)
    {
        return NormalizeATMType(atmType) switch
        {
            ATM_TYPE_NCR => NCR_JournalPath,
            ATM_TYPE_GRG => GRG_JournalPath,
            ATM_TYPE_WN => WN_JournalPath,
            ATM_TYPE_DN => DN_JournalPath,
            ATM_TYPE_HY => HY_JournalPath,
            ATM_TYPE_CW => CW_JournalPath,
            _ => @"C:\Journal\"
        };
    }

    public static string GetDefaultBackupPath(string? atmType)
    {
        return NormalizeATMType(atmType) switch
        {
            ATM_TYPE_NCR => NCR_BackupPath,
            ATM_TYPE_GRG => GRG_BackupPath,
            ATM_TYPE_WN => WN_BackupPath,
            ATM_TYPE_DN => DN_BackupPath,
            ATM_TYPE_HY => HY_BackupPath,
            ATM_TYPE_CW => CW_BackupPath,
            _ => @"C:\EJLive_BackupLog\"
        };
    }

    public static bool IsOverwriteJournalMode(string? atmType) => NormalizeATMType(atmType) == ATM_TYPE_NCR;

    public static string DefaultArchivePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Archive");
    public static string DefaultReportsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Reports");
    public static string DefaultDatabasePath => Environment.GetEnvironmentVariable("EJLIVE_DATABASE_PATH")
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Data", "ejlive.db");
    public static string DefaultLogPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Logs");
    public static string DefaultImagesPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Images");
    public static string DefaultServerSharePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "ServerShares");
    public static string ShareImagesAllPath => Path.Combine(DefaultServerSharePath, "01_AllATMs");
    public static string ShareImagesByTypePath => Path.Combine(DefaultServerSharePath, "02_ByATMType");
    public static string ShareImagesStagingPath => Path.Combine(DefaultServerSharePath, "03_Staging");
    public static string DefaultClientOutboxPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Client", "Outbox");
    public static string DefaultClientInboxPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Client", "Inbox");

    public static string[] GetServerImageShareFolders() => new[]
    {
        ShareImagesAllPath,
        ShareImagesByTypePath,
        ShareImagesStagingPath,
        Path.Combine(ShareImagesByTypePath, ATM_TYPE_NCR),
        Path.Combine(ShareImagesByTypePath, ATM_TYPE_GRG),
        Path.Combine(ShareImagesByTypePath, ATM_TYPE_WN),
        Path.Combine(ShareImagesByTypePath, ATM_TYPE_DN),
        Path.Combine(ShareImagesByTypePath, ATM_TYPE_HY)
    };
}

public static class Constants
{
    public const string AppVersion = AppConstants.AppVersion;
    public const int HeartbeatInterval = NetworkConfig.HEARTBEAT_INTERVAL_MS;
    public const int ConnectionTimeout = NetworkConfig.CONNECTION_TIMEOUT_MS;
    public const int MaxMessageSize = NetworkConfig.MAX_MESSAGE_SIZE;
}

public static class NetworkConfig
{
    public const int DEFAULT_PORT = AppConstants.DefaultPort;
    public const int CONNECTION_TIMEOUT_MS = AppConstants.SocketTimeoutMs;
    public const int SOCKET_BUFFER_SIZE = 64 * 1024;
    public const int PING_TIMEOUT_MS = 3000;
    public const int HEARTBEAT_INTERVAL_MS = AppConstants.HeartbeatIntervalSec * 1000;
    public const int DEFAULT_MESSAGE_SIZE_LINES = 50;
    public const int DEFAULT_FILE_PACKAGE_KB = 512;
    public const int MAX_MESSAGE_SIZE = 1024 * 1024;
}

public static class ATMPaths
{
    public const string NCR_SOURCE = AppConstants.NCR_JournalPath;
    public const string GRG_SOURCE = AppConstants.GRG_JournalPath;
    public const string WN_SOURCE = AppConstants.WN_JournalPath;
    public const string NCR_BACKUP = AppConstants.NCR_BackupPath;
    public const string GRG_BACKUP = AppConstants.GRG_BackupPath;
    public const string WN_BACKUP = AppConstants.WN_BackupPath;
    public const string SERVER_DEFAULT_DRIVE = @"C:";
    public const string SERVER_EJOURNAL_FILES = @"EJLive\Storage";
    public const string SERVER_EJOURNAL_REPORTS = @"EJLive\Archive";
}

public static class ATMStatusColors
{
    public const string COLOR_SUPERVISOR = "#7E57C2";
    public const string COLOR_OFFLINE = "#C62828";
    public const string COLOR_WARNING = "#F9A825";
    public const string COLOR_ACTIVE = "#2E7D32";
    public const string COLOR_IDLE = "#1565C0";
    public const int OFFLINE_THRESHOLD_MINUTES = 10;
    public const int WARNING_THRESHOLD_MINUTES = 5;
    public const int IDLE_THRESHOLD_SECONDS = 120;
}

public static class NCRFiles
{
    public static readonly string[] TargetFiles = { AppConstants.NCR_EJData, AppConstants.NCR_EJRcpy, AppConstants.NCR_EJDataLob };
}

public static class Protocol
{
    public const string HANDSHAKE = AppConstants.MSG_HANDSHAKE;
    public const string HANDSHAKE_ACK = AppConstants.MSG_ACK;
    public const string HANDSHAKE_REJECT = "EJLIVE_REJECT";
    public const string HEARTBEAT = AppConstants.MSG_HEARTBEAT;
    public const string HEARTBEAT_ACK = AppConstants.MSG_HB_ACK;
    public const string DATA_JOURNAL = "EJDATA";
    public const string DATA_FILE = "EJFILE";
    public const string DATA_ACK = AppConstants.MSG_JOURNAL_ACK;
    public const string STATUS_REQUEST = "STATUS_REQ";
    public const string STATUS_RESPONSE = "STATUS_RES";
    public const string CMD_RESULT = AppConstants.MSG_CMD_RESULT;
    public const string CMD_RESTART = AppConstants.CMD_RESTART;
    public const string CMD_SCREENSHOT = AppConstants.CMD_SCREENSHOT;
    public const string CMD_TIMESYNC = AppConstants.CMD_SYNC_TIME;
    public const string CMD_SHUTDOWN = AppConstants.CMD_SHUTDOWN;
    public const string CMD_CHANGE_PASSWORD = AppConstants.CMD_CHANGE_PASSWORD;
    public const string CMD_SEND_IMAGE = AppConstants.CMD_SEND_IMAGE;
    public const string CMD_UPDATE_CONFIG = AppConstants.CMD_REMOTE_CONFIG;
    public const string CMD_GET_SYSINFO = AppConstants.CMD_GET_STATS;
    public const string HEADER_END = "\n";
    public const string DATA_END = "\n<<END>>";

    public static string BuildMessage(string messageType, params string[] parts)
    {
        return string.Join("|", new[] { messageType ?? string.Empty }.Concat(parts ?? Array.Empty<string>()));
    }

    public static string[] ParseMessage(string? message)
    {
        return string.IsNullOrWhiteSpace(message)
            ? Array.Empty<string>()
            : message.Trim().Split('|', StringSplitOptions.None);
    }
}
