using EJLive.Core.Enums;

namespace EJLive.Core.Models
{
    public partial class ATMConfig
        {
            ATMType = ATMType.NCR,
    
    
            JournalSourcePath = @"C:\Program Files\NCR APATRA\Advance NDC\Data",
    
    
            LocalBackupPath = @"C:\NCR_BackupLog",
    
    
            ImageTargetPath = @"C:\Program Files\NCR APATRA\Advance NDC\Media"
    
    
            ATMType = ATMType.NCR,
    
    
            JournalSourcePath = @"C:\Program Files\NCR APATRA\Advance NDC\Data",
    
    
            LocalBackupPath = @"C:\NCR_BackupLog",
    
    
            ImageTargetPath = @"C:\Program Files\NCR APATRA\Advance NDC\Media"
    
    
            ATMType = ATMType.NCR,
    
            JournalSourcePath = @"C:\Program Files\NCR APATRA\Advance NDC\Data",
    
            LocalBackupPath = @"C:\NCR_BackupLog",
    
            ImageTargetPath = @"C:\Program Files\NCR APATRA\Advance NDC\Media"
    
    
            // --- Properties ---
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
    
    
            ATMType.NCR => new ATMConfig
    
    
            // --- Methods ---
            public static ATMConfig GetDefaultConfig(ATMType type) => type switch
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMConfig.cs
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
                    _ => new ATMConfig()
                };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMConfig.cs
            ATMType.NCR => new ATMConfig
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMConfig.cs
            public string DeviceId { get; set; } = string.Empty;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMConfig.cs
            ATMType.NCR => new ATMConfig
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMConfig.cs
            public string DeviceId { get; set; } = string.Empty;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMConfig.cs
            ATMType = ATMType.NCR,
    
            JournalSourcePath = @"C:\Program Files\NCR APATRA\Advance NDC\Data",
    
            LocalBackupPath = @"C:\NCR_BackupLog",
    
            ImageTargetPath = @"C:\Program Files\NCR APATRA\Advance NDC\Media"
    
    
            // --- Properties ---
            public string DeviceId { get; set; } = string.Empty;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMConfig.cs
            public string DeviceId { get; set; } = string.Empty;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMConfig.cs.before_unify
            public string DeviceId { get; set; } = string.Empty;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMConfig.cs.before_unify
            ATMType.NCR => new ATMConfig
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMConfig.cs.v22_bak
            public string DeviceId { get; set; } = string.Empty;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMConfig.cs.v22_bak
            ATMType.NCR => new ATMConfig
    
    
            public static ATMConfig GetDefaultConfig(ATMType type) => type switch
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMConfig.cs
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
                _ => new ATMConfig()
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMConfig.cs
            public static ATMConfig GetDefaultConfig(ATMType type) => type switch
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMConfig.cs.before_unify
            public static ATMConfig GetDefaultConfig(ATMType type) => type switch
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMConfig.cs
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
                _ => new ATMConfig()
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMConfig.cs
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
                _ => new ATMConfig()
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMConfig.cs.before_unify
            public static ATMConfig GetDefaultConfig(ATMType type) => type switch
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMConfig.cs.v22_bak
            public static ATMConfig GetDefaultConfig(ATMType type) => type switch
    
    
        }
    /// <summary>
    /// Comprehensive ATM configuration model containing all device-specific
    /// settings including paths, network parameters, and sync preferences.
    /// </summary>
    public class ATMConfig
    {
        /// <summary>Unique device identifier assigned by the bank/fleet.</summary>
        public string DeviceId { get; set; } = string.Empty;
    
        /// <summary>Human-readable device name for dashboards.</summary>
        public string DeviceName { get; set; } = string.Empty;
    
        /// <summary>Branch name where the ATM is physically located.</summary>
        public string BranchName { get; set; } = string.Empty;
    
        /// <summary>Region or zone name for fleet grouping.</summary>
        public string RegionName { get; set; } = string.Empty;
    
        /// <summary>Vendor/manufacturer type of this ATM.</summary>
        public ATMType ATMType { get; set; } = ATMType.NCR;
    
        /// <summary>Network connection method used by the ATM.</summary>
        public ConnectionType ConnectionType { get; set; } = ConnectionType.LAN;
    
        /// <summary>Source path where the ATM writes its journal files.</summary>
        public string JournalSourcePath { get; set; } = string.Empty;
    
        /// <summary>Local backup path for journal files before transfer.</summary>
        public string LocalBackupPath { get; set; } = string.Empty;
    
        /// <summary>Path to ATM image/screenshot files directory.</summary>
        public string ImageTargetPath { get; set; } = string.Empty;
    
        /// <summary>Server IP address for network communication.</summary>
        public string ServerIP { get; set; } = "127.0.0.1";
    
        /// <summary>Server port for network communication.</summary>
        public int ServerPort { get; set; } = 8888;
    
        /// <summary>Interval in seconds between heartbeat signals.</summary>
        public int HeartbeatIntervalSeconds { get; set; } = 30;
    
        /// <summary>Interval in seconds before attempting reconnection.</summary>
        public int ReconnectIntervalSeconds { get; set; } = 5;
    
        /// <summary>Maximum number of reconnection attempts before escalating.</summary>
        public int MaxReconnectAttempts { get; set; } = 10;
    
        /// <summary>Whether to use data compression for file transfers.</summary>
        public bool UseCompression { get; set; } = true;
    
        /// <summary>Whether to use encryption for sensitive data.</summary>
        public bool UseEncryption { get; set; } = true;
    
        /// <summary>Whether automatic journal synchronization is enabled.</summary>
        public bool AutoSync { get; set; } = true;
    
        /// <summary>Interval in minutes between automatic sync operations.</summary>
        public int SyncIntervalMinutes { get; set; } = 5;
    
        /// <summary>Whether to use shadow copy for reading in-use files.</summary>
        public bool EnableShadowCopy { get; set; } = true;
    
        /// <summary>Whether delta (incremental) sync is enabled.</summary>
        public bool EnableDeltaSync { get; set; } = true;
    
        /// <summary>Chunk size in bytes for file transfer operations.</summary>
        public int ChunkSize { get; set; } = 65536;
    
        /// <summary>Encryption key for secure communication.</summary>
        public string EncryptionKey { get; set; } = string.Empty;
    
        /// <summary>Session token for authenticated connections.</summary>
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
            _ => new ATMConfig()
        };
    }
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
            _ => new ATMConfig()
        };
    }
    public partial class ATMConfig
        {
            ATMType = ATMType.NCR,
    
    
            JournalSourcePath = @"C:\Program Files\NCR APATRA\Advance NDC\Data",
    
    
            LocalBackupPath = @"C:\NCR_BackupLog",
    
    
            ImageTargetPath = @"C:\Program Files\NCR APATRA\Advance NDC\Media"
    
    
            ATMType = ATMType.NCR,
    
            JournalSourcePath = @"C:\Program Files\NCR APATRA\Advance NDC\Data",
    
            LocalBackupPath = @"C:\NCR_BackupLog",
    
            ImageTargetPath = @"C:\Program Files\NCR APATRA\Advance NDC\Media"
    
    
            // --- Properties ---
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
    
    
            ATMType.NCR => new ATMConfig
    
    
            // --- Methods ---
            public static ATMConfig GetDefaultConfig(ATMType type) => type switch
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMConfig.cs
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
                    _ => new ATMConfig()
                };
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMConfig.cs.v22_bak
            public string DeviceId { get; set; } = string.Empty;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMConfig.cs.v22_bak
            ATMType.NCR => new ATMConfig
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMConfig.cs.before_unify
            public string DeviceId { get; set; } = string.Empty;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMConfig.cs.before_unify
            ATMType.NCR => new ATMConfig
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMConfig.cs
            public string DeviceId { get; set; } = string.Empty;
    
    
            public static ATMConfig GetDefaultConfig(ATMType type) => type switch
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMConfig.cs
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
                _ => new ATMConfig()
            };
    
    
        }

    // Class: ATMConfig (from 3 sources)
        public partial class ATMConfig
        {
            // --- Constants & Fields ---
            ATMType = ATMType.NCR,
    
            JournalSourcePath = @"C:\Program Files\NCR APATRA\Advance NDC\Data",
    
            LocalBackupPath = @"C:\NCR_BackupLog",
    
            ImageTargetPath = @"C:\Program Files\NCR APATRA\Advance NDC\Media"
    
    
            // --- Properties ---
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
    
            ATMType.NCR => new ATMConfig
    
    
            // --- Methods ---
            public static ATMConfig GetDefaultConfig(ATMType type) => type switch
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMConfig.cs
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
                    _ => new ATMConfig()
                };
    
    
        }
}
