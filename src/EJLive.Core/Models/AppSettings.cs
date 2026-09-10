namespace EJLive.Core.Models
{
    public class AppSettings
    {
        public int ServerPort { get; set; } = 8888;
        public int MaxConcurrentConnections { get; set; } = 1000;
        public int WorkerThreadCount { get; set; } = 8;
        public int MessageQueueSize { get; set; } = 10000;
        public string MasterEncryptionKey { get; set; } = string.Empty;
        public bool EnableHMAC { get; set; } = true;
        public bool EnableMFA { get; set; } = false;
        public int SessionTimeoutMinutes { get; set; } = 30;
        public int DefaultChunkSize { get; set; } = 65536;
        public int MaxRetryAttempts { get; set; } = 10;
        public int RetryBaseDelayMs { get; set; } = 5000;
        public bool EnableExponentialBackoff { get; set; } = true;
        public bool EnableJitter { get; set; } = true;
        public string SharedImageFolder { get; set; } = @"C:\EJLive_Storage\SharedImages";
        public int ImageSyncIntervalMinutes { get; set; } = 60;
        public string ArchivePath { get; set; } = @"C:\EJLive_Storage\Archive";
        public int ArchiveRetentionDays { get; set; } = 365;
        public bool CompressArchive { get; set; } = true;
        public bool DarkMode { get; set; } = true;
        public int DashboardRefreshIntervalMs { get; set; } = 1000;
        public int AlertDisplayDurationSeconds { get; set; } = 30;
        public string DatabasePath { get; set; } = @"C:\EJLive_Storage\Data\ejlive.db";
        public bool UsePostgreSQL { get; set; } = false;
        public string PostgreSQLConnectionString { get; set; } = string.Empty;
    }
    public partial class AppSettings
        {
            public int ServerPort { get; set; } = 8888;
    
    
            public int MaxConcurrentConnections { get; set; } = 1000;
    
    
            public int WorkerThreadCount { get; set; } = 8;
    
    
            public int MessageQueueSize { get; set; } = 10000;
    
    
            public string MasterEncryptionKey { get; set; } = string.Empty;
    
    
            public bool EnableHMAC { get; set; } = true;
    
    
            public bool EnableMFA { get; set; } = false;
    
    
            public int SessionTimeoutMinutes { get; set; } = 30;
    
    
            public int DefaultChunkSize { get; set; } = 65536;
    
    
            public int MaxRetryAttempts { get; set; } = 10;
    
    
            public int RetryBaseDelayMs { get; set; } = 5000;
    
    
            public bool EnableExponentialBackoff { get; set; } = true;
    
    
            public bool EnableJitter { get; set; } = true;
    
    
            public string SharedImageFolder { get; set; } = @"C:\EJLive_Storage\SharedImages";
    
    
            public int ImageSyncIntervalMinutes { get; set; } = 60;
    
    
            public string ArchivePath { get; set; } = @"C:\EJLive_Storage\Archive";
    
    
            public int ArchiveRetentionDays { get; set; } = 365;
    
    
            public bool CompressArchive { get; set; } = true;
    
    
            public bool DarkMode { get; set; } = true;
    
    
            public int DashboardRefreshIntervalMs { get; set; } = 1000;
    
    
            public int AlertDisplayDurationSeconds { get; set; } = 30;
    
    
            public string DatabasePath { get; set; } = @"C:\EJLive_Storage\Data\ejlive.db";
    
    
            public bool UsePostgreSQL { get; set; } = false;
    
    
            public string PostgreSQLConnectionString { get; set; } = string.Empty;
    
    
        }
    /// <summary>
    /// Centralized application settings model covering server, security,
    /// storage, and UI configuration parameters.
    /// </summary>
    public class AppSettings
    {
        public int ServerPort { get; set; } = 8888;
        public int MaxConcurrentConnections { get; set; } = 1000;
        public int WorkerThreadCount { get; set; } = 8;
        public int MessageQueueSize { get; set; } = 10000;
        public string MasterEncryptionKey { get; set; } = string.Empty;
        public bool EnableHMAC { get; set; } = true;
        public bool EnableMFA { get; set; } = false;
        public int SessionTimeoutMinutes { get; set; } = 30;
        public int DefaultChunkSize { get; set; } = 65536;
        public int MaxRetryAttempts { get; set; } = 10;
        public int RetryBaseDelayMs { get; set; } = 5000;
        public bool EnableExponentialBackoff { get; set; } = true;
        public bool EnableJitter { get; set; } = true;
        public string SharedImageFolder { get; set; } = @"C:\EJLive_Storage\SharedImages";
        public int ImageSyncIntervalMinutes { get; set; } = 60;
        public string ArchivePath { get; set; } = @"C:\EJLive_Storage\Archive";
        public int ArchiveRetentionDays { get; set; } = 365;
        public bool CompressArchive { get; set; } = true;
        public bool DarkMode { get; set; } = true;
        public int DashboardRefreshIntervalMs { get; set; } = 1000;
        public int AlertDisplayDurationSeconds { get; set; } = 30;
        public string DatabasePath { get; set; } = @"C:\EJLive_Storage\Data\ejlive.db";
        public bool UsePostgreSQL { get; set; } = false;
        public string PostgreSQLConnectionString { get; set; } = string.Empty;
    }

    // Class: AppSettings (from 4 sources)
        public partial class AppSettings
        {
            // --- Properties ---
            public int ServerPort { get; set; } = 8888;
    
            public int MaxConcurrentConnections { get; set; } = 1000;
    
            public int WorkerThreadCount { get; set; } = 8;
    
            public int MessageQueueSize { get; set; } = 10000;
    
            public string MasterEncryptionKey { get; set; } = string.Empty;
    
            public bool EnableHMAC { get; set; } = true;
    
            public bool EnableMFA { get; set; } = false;
    
            public int SessionTimeoutMinutes { get; set; } = 30;
    
            public int DefaultChunkSize { get; set; } = 65536;
    
            public int MaxRetryAttempts { get; set; } = 10;
    
            public int RetryBaseDelayMs { get; set; } = 5000;
    
            public bool EnableExponentialBackoff { get; set; } = true;
    
            public bool EnableJitter { get; set; } = true;
    
            public string SharedImageFolder { get; set; } = @"C:\EJLive_Storage\SharedImages";
    
            public int ImageSyncIntervalMinutes { get; set; } = 60;
    
            public string ArchivePath { get; set; } = @"C:\EJLive_Storage\Archive";
    
            public int ArchiveRetentionDays { get; set; } = 365;
    
            public bool CompressArchive { get; set; } = true;
    
            public bool DarkMode { get; set; } = true;
    
            public int DashboardRefreshIntervalMs { get; set; } = 1000;
    
            public int AlertDisplayDurationSeconds { get; set; } = 30;
    
            public string DatabasePath { get; set; } = @"C:\EJLive_Storage\Data\ejlive.db";
    
            public bool UsePostgreSQL { get; set; } = false;
    
            public string PostgreSQLConnectionString { get; set; } = string.Empty;
    
    
        }
}
