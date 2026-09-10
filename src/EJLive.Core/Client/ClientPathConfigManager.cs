using System;
using System.Collections.Generic;
using System.IO;

namespace EJLive.Core.Client
{
    /// <summary>
        /// Manages all client-side file paths for journal/log/image synchronization.
        /// Source → Client Backup → Server, Image Inbox → ATM Destination.
        /// </summary>
        public sealed class ClientPathConfig
        {
            public string JournalSourcePath { get; set; } = @"C:\Journal\";
            public string JournalBackupPath { get; set; } = @"C:\EJLive\Backup\";
            public string LogTracePath { get; set; } = @"C:\Logs\";
            public string LogBackupPath { get; set; } = @"C:\EJLive\LogBackup\";
            public string ImageInboxPath { get; set; } = @"C:\EJLive\ImageInbox\";
            public string ImageDestinationPath { get; set; } = @"C:\ATM\Media\";
            public string ScreenshotCachePath { get; set; } = @"C:\EJLive\ScreenshotCache\";
            public string ConfigPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Config");
            public string HealthFilePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Agent", "health.json");
            public string LocalDatabasePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Database", "ejlive_client.db");
        }
    public partial class ClientPathConfig
        {
            public string JournalSourcePath { get; set; } = @"C:\Journal\";
    
    
            public string JournalBackupPath { get; set; } = @"C:\EJLive\Backup\";
    
    
            public string LogTracePath { get; set; } = @"C:\Logs\";
    
    
            public string LogBackupPath { get; set; } = @"C:\EJLive\LogBackup\";
    
    
            public string ImageInboxPath { get; set; } = @"C:\EJLive\ImageInbox\";
    
    
            public string ImageDestinationPath { get; set; } = @"C:\ATM\Media\";
    
    
            public string ScreenshotCachePath { get; set; } = @"C:\EJLive\ScreenshotCache\";
    
    
            public string ConfigPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Config");
    
    
            public string HealthFilePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Agent", "health.json");
    
    
            public string LocalDatabasePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Database", "ejlive_client.db");
    
    
        }
    public sealed class ClientPathConfigManager
        {
            public ClientPathConfig Config { get; } = new();
    
            public void EnsureAllPaths()
            {
                EnsureDirectory(Config.JournalSourcePath);
                EnsureDirectory(Config.JournalBackupPath);
                EnsureDirectory(Config.LogTracePath);
                EnsureDirectory(Config.LogBackupPath);
                EnsureDirectory(Config.ImageInboxPath);
                EnsureDirectory(Config.ImageDestinationPath);
                EnsureDirectory(Config.ScreenshotCachePath);
                EnsureDirectory(Config.ConfigPath);
                EnsureDirectory(Path.GetDirectoryName(Config.HealthFilePath) ?? "");
                EnsureDirectory(Path.GetDirectoryName(Config.LocalDatabasePath) ?? "");
            }
    
            public bool ValidateAllPaths(out List<string> errors)
            {
                errors = new List<string>();
                ValidatePath("Journal Source", Config.JournalSourcePath, errors);
                ValidatePath("Journal Backup", Config.JournalBackupPath, errors);
                ValidatePath("Log Trace", Config.LogTracePath, errors);
                ValidatePath("Log Backup", Config.LogBackupPath, errors);
                ValidatePath("Image Inbox", Config.ImageInboxPath, errors);
                return errors.Count == 0;
            }
    
            public void ApplyVendorDefaults(string vendor)
            {
                switch (vendor?.ToUpperInvariant())
                {
                    case "NCR":
                        Config.JournalSourcePath = @"C:\Program Files\NCR APATRA\Advance NDC\Data\";
                        Config.JournalBackupPath = @"C:\NCR_BackupLog\";
                        Config.LogTracePath = @"C:\Program Files\NCR APATRA\Advance NDC\Data\";
                        Config.ImageDestinationPath = @"C:\Program Files\NCR APATRA\Advance NDC\Media\";
                        break;
                    case "GRG":
                        Config.JournalSourcePath = @"D:\Program Files\DTATMW\Bin\ATMAPP\Log\";
                        Config.JournalBackupPath = @"D:\GRG_BackupLog\";
                        Config.LogTracePath = @"D:\Program Files\DTATMW\Bin\ATMAPP\Log\";
                        Config.ImageDestinationPath = @"D:\Program Files\DTATMW\Bin\AScreen\image\";
                        break;
                    case "WINCOR":
                    case "WN":
                        Config.JournalSourcePath = @"C:\journal\";
                        Config.JournalBackupPath = @"C:\WN_BackupLog\";
                        Config.LogTracePath = @"C:\journal\";
                        Config.ImageDestinationPath = @"C:\ProTopas\Images\";
                        break;
                    default:
                        Config.JournalSourcePath = @"C:\Journal\";
                        Config.JournalBackupPath = @"C:\EJLive\Backup\";
                        break;
                }
                EnsureAllPaths();
            }
    
            private static void EnsureDirectory(string path)
            {
                try { if (!string.IsNullOrWhiteSpace(path)) Directory.CreateDirectory(path); } catch { }
            }
    
            private static void ValidatePath(string label, string path, List<string> errors)
            {
                if (string.IsNullOrWhiteSpace(path)) errors.Add($"{label}: path is empty.");
                else if (!Directory.Exists(path)) errors.Add($"{label}: '{path}' does not exist.");
            }
        }
    public partial class ClientPathConfigManager
        {
            public ClientPathConfig Config { get; } = new();
    
    
            public void EnsureAllPaths()
            {
                EnsureDirectory(Config.JournalSourcePath);
                EnsureDirectory(Config.JournalBackupPath);
                EnsureDirectory(Config.LogTracePath);
                EnsureDirectory(Config.LogBackupPath);
                EnsureDirectory(Config.ImageInboxPath);
                EnsureDirectory(Config.ImageDestinationPath);
                EnsureDirectory(Config.ScreenshotCachePath);
                EnsureDirectory(Config.ConfigPath);
                EnsureDirectory(Path.GetDirectoryName(Config.HealthFilePath) ?? "");
                EnsureDirectory(Path.GetDirectoryName(Config.LocalDatabasePath) ?? "");
            }
    
    
            public bool ValidateAllPaths(out List<string> errors)
            {
                errors = new List<string>();
                ValidatePath("Journal Source", Config.JournalSourcePath, errors);
                ValidatePath("Journal Backup", Config.JournalBackupPath, errors);
                ValidatePath("Log Trace", Config.LogTracePath, errors);
                ValidatePath("Log Backup", Config.LogBackupPath, errors);
                ValidatePath("Image Inbox", Config.ImageInboxPath, errors);
                return errors.Count == 0;
            }
    
    
            public void ApplyVendorDefaults(string vendor)
            {
                switch (vendor?.ToUpperInvariant())
                {
                    case "NCR":
                        Config.JournalSourcePath = @"C:\Program Files\NCR APATRA\Advance NDC\Data\";
                        Config.JournalBackupPath = @"C:\NCR_BackupLog\";
                        Config.LogTracePath = @"C:\Program Files\NCR APATRA\Advance NDC\Data\";
                        Config.ImageDestinationPath = @"C:\Program Files\NCR APATRA\Advance NDC\Media\";
                        break;
                    case "GRG":
                        Config.JournalSourcePath = @"D:\Program Files\DTATMW\Bin\ATMAPP\Log\";
                        Config.JournalBackupPath = @"D:\GRG_BackupLog\";
                        Config.LogTracePath = @"D:\Program Files\DTATMW\Bin\ATMAPP\Log\";
                        Config.ImageDestinationPath = @"D:\Program Files\DTATMW\Bin\AScreen\image\";
                        break;
                    case "WINCOR":
                    case "WN":
                        Config.JournalSourcePath = @"C:\journal\";
                        Config.JournalBackupPath = @"C:\WN_BackupLog\";
                        Config.LogTracePath = @"C:\journal\";
                        Config.ImageDestinationPath = @"C:\ProTopas\Images\";
                        break;
                    default:
                        Config.JournalSourcePath = @"C:\Journal\";
                        Config.JournalBackupPath = @"C:\EJLive\Backup\";
                        break;
                }
                EnsureAllPaths();
            }
    
    
            private static void EnsureDirectory(string path)
            {
                try { if (!string.IsNullOrWhiteSpace(path)) Directory.CreateDirectory(path); } catch { }
            }
    
    
            private static void ValidatePath(string label, string path, List<string> errors)
            {
                if (string.IsNullOrWhiteSpace(path)) errors.Add($"{label}: path is empty.");
                else if (!Directory.Exists(path)) errors.Add($"{label}: '{path}' does not exist.");
            }
    
    
        }

    // Class: ClientPathConfig (from 3 sources)
        public sealed partial class ClientPathConfig
        {
            // --- Properties ---
                    public string JournalSourcePath { get; set; } = @"C:\Journal\";
    
                    public string JournalBackupPath { get; set; } = @"C:\EJLive\Backup\";
    
                    public string LogTracePath { get; set; } = @"C:\Logs\";
    
                    public string LogBackupPath { get; set; } = @"C:\EJLive\LogBackup\";
    
                    public string ImageInboxPath { get; set; } = @"C:\EJLive\ImageInbox\";
    
                    public string ImageDestinationPath { get; set; } = @"C:\ATM\Media\";
    
                    public string ScreenshotCachePath { get; set; } = @"C:\EJLive\ScreenshotCache\";
    
                    public string ConfigPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Config");
    
                    public string HealthFilePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Agent", "health.json");
    
                    public string LocalDatabasePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Database", "ejlive_client.db");
    
    
        }
    // Class: ClientPathConfigManager (from 3 sources)
        public sealed partial class ClientPathConfigManager
        {
            // --- Properties ---
                    public ClientPathConfig Config { get; } = new();
    
    
            // --- Methods ---
                    public void EnsureAllPaths()
                    {
                        EnsureDirectory(Config.JournalSourcePath);
                        EnsureDirectory(Config.JournalBackupPath);
                        EnsureDirectory(Config.LogTracePath);
                        EnsureDirectory(Config.LogBackupPath);
                        EnsureDirectory(Config.ImageInboxPath);
                        EnsureDirectory(Config.ImageDestinationPath);
                        EnsureDirectory(Config.ScreenshotCachePath);
                        EnsureDirectory(Config.ConfigPath);
                        EnsureDirectory(Path.GetDirectoryName(Config.HealthFilePath) ?? "");
                        EnsureDirectory(Path.GetDirectoryName(Config.LocalDatabasePath) ?? "");
                    }
    
                    public bool ValidateAllPaths(out List<string> errors)
                    {
                        errors = new List<string>();
                        ValidatePath("Journal Source", Config.JournalSourcePath, errors);
                        ValidatePath("Journal Backup", Config.JournalBackupPath, errors);
                        ValidatePath("Log Trace", Config.LogTracePath, errors);
                        ValidatePath("Log Backup", Config.LogBackupPath, errors);
                        ValidatePath("Image Inbox", Config.ImageInboxPath, errors);
                        return errors.Count == 0;
                    }
    
                    public void ApplyVendorDefaults(string vendor)
                    {
                        switch (vendor?.ToUpperInvariant())
                        {
                            case "NCR":
                                Config.JournalSourcePath = @"C:\Program Files\NCR APATRA\Advance NDC\Data\";
                                Config.JournalBackupPath = @"C:\NCR_BackupLog\";
                                Config.LogTracePath = @"C:\Program Files\NCR APATRA\Advance NDC\Data\";
                                Config.ImageDestinationPath = @"C:\Program Files\NCR APATRA\Advance NDC\Media\";
                                break;
                            case "GRG":
                                Config.JournalSourcePath = @"D:\Program Files\DTATMW\Bin\ATMAPP\Log\";
                                Config.JournalBackupPath = @"D:\GRG_BackupLog\";
                                Config.LogTracePath = @"D:\Program Files\DTATMW\Bin\ATMAPP\Log\";
                                Config.ImageDestinationPath = @"D:\Program Files\DTATMW\Bin\AScreen\image\";
                                break;
                            case "WINCOR":
                            case "WN":
                                Config.JournalSourcePath = @"C:\journal\";
                                Config.JournalBackupPath = @"C:\WN_BackupLog\";
                                Config.LogTracePath = @"C:\journal\";
                                Config.ImageDestinationPath = @"C:\ProTopas\Images\";
                                break;
                            default:
                                Config.JournalSourcePath = @"C:\Journal\";
                                Config.JournalBackupPath = @"C:\EJLive\Backup\";
                                break;
                        }
                        EnsureAllPaths();
                    }
    
                    private static void EnsureDirectory(string path)
                    {
                        try { if (!string.IsNullOrWhiteSpace(path)) Directory.CreateDirectory(path); } catch { }
                    }
    
                    private static void ValidatePath(string label, string path, List<string> errors)
                    {
                        if (string.IsNullOrWhiteSpace(path)) errors.Add($"{label}: path is empty.");
                        else if (!Directory.Exists(path)) errors.Add($"{label}: '{path}' does not exist.");
                    }
    
    
        }
}
