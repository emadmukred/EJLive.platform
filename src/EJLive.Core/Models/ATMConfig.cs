using EJLive.Core.Enums;

namespace EJLive.Core.Models
{
    /// <summary>
    /// Comprehensive ATM configuration model containing all device-specific
    /// settings including paths, network parameters, and sync preferences.
    /// </summary>
    /// <remarks>
    /// Wave 1 (SS-04 D-08): rewrote the file. The previous version was an auto-merge dump with
    /// four copies of <c>ATMConfig</c> interleaved with provenance comments, brace-balanced so
    /// <c>SYN-1</c> passed it but <c>CS0101</c> at compile time. Members kept the union set the
    /// variants agreed on, and one <c>GetDefaultConfig</c> covers NCR/GRG/Wincor (WN) plus the
    /// documented fallback. SS-15 (mandated patterns): <c>sealed</c> by default omitted because
    /// this is a public DTO that consumers (Tests, Installer, Monitoring) inherit-equivalent via
    /// <c>partial</c> elsewhere.
    /// </remarks>
    public class ATMConfig
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public ATMType ATMType { get; set; } = ATMType.NCR;
        public ConnectionType ConnectionType { get; set; } = ConnectionType.LAN;
        public string JournalSourcePath { get; set; } = string.Empty;
        public string LocalBackupPath { get; set; } = string.Empty;
        public string ImageTargetPath { get; set; } = string.Empty;
        public string ServerIP { get; set; } = "127.0.0.1";
        public int ServerPort { get; set; } = 8888;
        public int HeartbeatIntervalSeconds { get; set; } = 30;
        public int ReconnectIntervalSeconds { get; set; } = 5;
        public int MaxReconnectAttempts { get; set; } = 10;
        public bool UseCompression { get; set; } = true;
        public bool UseEncryption { get; set; } = true;
        public bool AutoSync { get; set; } = true;
        public int SyncIntervalMinutes { get; set; } = 5;
        public bool EnableShadowCopy { get; set; } = true;
        public bool EnableDeltaSync { get; set; } = true;
        public int ChunkSize { get; set; } = 65536;
        public string EncryptionKey { get; set; } = string.Empty;
        public string SessionToken { get; set; } = string.Empty;

        /// <summary>
        /// Returns a default configuration preset for the specified ATM vendor type,
        /// including vendor-specific journal and image paths.
        /// </summary>
        public static ATMConfig GetDefaultConfig(ATMType type) => type switch
        {
            ATMType.NCR => new ATMConfig
            {
                ATMType = ATMType.NCR,
                JournalSourcePath = @"C:\Program Files\NCR APATRA\Advance NDC\Data",
                LocalBackupPath = @"C:\NCR_BackupLog",
                ImageTargetPath = @"C:\Program Files\NCR APATRA\Advance NDC\Media"
            },
            ATMType.GRG => new ATMConfig
            {
                ATMType = ATMType.GRG,
                JournalSourcePath = @"D:\Program Files\DTATMW\Bin\ATMAPP\Log",
                LocalBackupPath = @"D:\GRG_BackupLog",
                ImageTargetPath = @"D:\Program Files\DTATMW\Bin\AScreen\image"
            },
            ATMType.WN => new ATMConfig
            {
                ATMType = ATMType.WN,
                JournalSourcePath = @"C:\journal",
                LocalBackupPath = @"C:\WN_BackupLog",
                ImageTargetPath = @"C:\ProTopas\Images"
            },
            ATMType.Diebold => new ATMConfig
            {
                ATMType = ATMType.Diebold,
                JournalSourcePath = @"D:\Diebold\Logs",
                LocalBackupPath = @"D:\Diebold\Backup",
                ImageTargetPath = @"D:\Diebold\Images"
            },
            ATMType.Hyosung => new ATMConfig
            {
                ATMType = ATMType.Hyosung,
                JournalSourcePath = @"C:\Hyosung\Journals",
                LocalBackupPath = @"C:\Hyosung\Backup",
                ImageTargetPath = @"C:\Hyosung\Images"
            },
            _ => new ATMConfig()
        };
    }
}
