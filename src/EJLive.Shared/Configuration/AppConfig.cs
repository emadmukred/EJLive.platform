using System;
using System.IO;
using System.Xml.Serialization;

namespace EJLive.Shared.Configuration
{
    /// <summary>
    /// Application configuration class for EJLive
    /// </summary>
    public class AppConfig
    {
        // Server settings
        public int ServerPort { get; set; } = 8080;
        public string ServerHost { get; set; } = "localhost";
        public string ArchivePath { get; set; } = @"C:\EJLive\Archive";
        
        // Security settings
        public string EncryptionKey { get; set; } = "default_key";
        public int KeySize { get; set; } = 256;
        
        // Client settings
        public string ClientId { get; set; } = "default_client";
        public string ServerAddress { get; set; } = "localhost";
        public int ClientPort { get; set; } = 8080;
        
        // Logging settings
        public string LogPath { get; set; } = @"C:\EJLive\Logs";
        public bool EnableDebugLogging { get; set; } = false;
        public int MaxLogSizeMB { get; set; } = 10;
        
        // File monitoring settings
        public string WatchPath { get; set; } = @"C:\EJLive\Watch";
        public int ScanIntervalSeconds { get; set; } = 30;
        public string[] FileExtensions { get; set; } = { ".ej", ".log" };
        
        // UI settings
        public string Theme { get; set; } = "Light";
        public bool EnableNotifications { get; set; } = true;
        public int RefreshIntervalSeconds { get; set; } = 5;
        
        /// <summary>
        /// Loads configuration from XML file
        /// </summary>
        /// <param name="path">Path to configuration file</param>
        /// <returns>AppConfig instance</returns>
        public static AppConfig Load(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var serializer = new XmlSerializer(typeof(AppConfig));
                    using (var reader = new StreamReader(path))
                    {
                        return (AppConfig)serializer.Deserialize(reader);
                    }
                }
            }
            catch
            {
                // Return default configuration if file doesn't exist or is invalid
            }
            
            return new AppConfig();
        }
        
        /// <summary>
        /// Saves configuration to XML file
        /// </summary>
        /// <param name="path">Path to configuration file</param>
        public void Save(string path)
        {
            var serializer = new XmlSerializer(typeof(AppConfig));
            using (var writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, this);
            }
        }
    }
}