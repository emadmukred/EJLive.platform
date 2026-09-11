namespace EJLive.Core;


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
