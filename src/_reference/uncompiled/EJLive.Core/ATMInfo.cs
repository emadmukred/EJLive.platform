using System;
using System.Drawing;
using EJLive.Core;

namespace EJLive.Core
{
    public partial class AlertPayload
        {
            public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public AlertSeverity Severity  { get; set; }
    
    
            public string        Title     { get; set; }
    
    
            public string        Message   { get; set; }
    
    
            public string        Source    { get; set; }
    
    
            public string        DedupeKey { get; set; }
    
    
            public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
    
    
            public bool          IsRead    { get; set; }
    
    
            public string Icon => Severity switch
            {
                AlertSeverity.Emergency => "🚨",
                AlertSeverity.Critical  => "❌",
                AlertSeverity.Warning   => "⚠️",
                _                       => "ℹ️"
            };
    
    
            public string SeverityIcon => Icon;
    
    
            public Color Color => Severity switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
            public string        ATM_ID    { get; set; }
    
    
            public AlertSeverity SeverityLevel { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string        Severity  { get; set; } = "Info";
    
    
            public DateTime      CreatedAtUtc { get => CreatedAt; set => CreatedAt = value; }
    
    
            public DateTime      RaisedAt  { get => CreatedAt; set => CreatedAt = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string        Severity  { get; set; } = "Info";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string        Severity  { get; set; } = "Info";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string        Severity  { get; set; } = "Info";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string        Severity  { get; set; } = "Info";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string        Severity  { get; set; } = "Info";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string        Severity  { get; set; } = "Info";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string        Severity  { get; set; } = "Info";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string        Severity  { get; set; } = "Info";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string        Severity  { get; set; } = "Info";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public string Icon => Severity switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical => "FAIL",
                AlertSeverity.Warning => "WARN",
                _ => "INFO"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string        Severity  { get; set; } = "Info";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string        Severity  { get; set; } = "Info";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string        Severity  { get; set; } = "Info";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
        }
    public partial class AlertPayload
        {
            public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
            public AlertSeverity Severity { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public string Source { get; set; }
            public string DedupeKey { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public bool IsRead { get; set; }
            public string Icon => Severity switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical => "FAIL",
                AlertSeverity.Warning => "WARN",
                _ => "INFO"
            };
            public Color Color => Severity switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59, 48),
                AlertSeverity.Critical => Color.FromArgb(255, 59, 48),
                AlertSeverity.Warning => Color.FromArgb(255, 149, 0),
                _ => Color.FromArgb(0, 122, 255)
            };
            public string        ATM_ID    { get; set; }
            public AlertSeverity SeverityLevel { get; set; }
            public string        Severity  { get; set; } = "Info";
            public DateTime      CreatedAtUtc { get => CreatedAt; set => CreatedAt = value; }
            public DateTime      RaisedAt  { get => CreatedAt; set => CreatedAt = value; }
            public string Icon => Severity switch
            {
                AlertSeverity.Emergency => "🚨",
                AlertSeverity.Critical  => "❌",
                AlertSeverity.Warning   => "⚠️",
                _                       => "ℹ️"
            };
            public string SeverityIcon => Icon;
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
        }
    public partial public class ATMInfo
        {
            public ATMInfo()
            {
            public string ATMID { get; set; }
            public string BranchName { get; set; }
            public string IPAddress { get; set; }
            public ATMType Type { get; set; }
            public ATMStatus Status { get; set; }
            public DateTime LastSync { get; set; }
            public DateTime LastHeartbeat { get; set; }
            public long TotalBytesSynced { get; set; }
            public int FilesProcessed { get; set; }
            public bool IsOnline { get; set; }
            public string LastError { get; set; }
            public string OSVersion { get; set; }
            public string SoftwareVersion { get; set; }
            public string ATMName { get; set; }
            public string Location { get; set; }
            public DateTime LastSyncTime { get; set; }
            public decimal RemainingCash { get; set; }
            public decimal DispensedCash { get; set; }
            public string LastTransaction { get; set; }
            public string NetworkType { get; set; }
            public int CapturedCardsCount { get; set; }
            public string LastFaultCode { get; set; }
            public string LastFaultDescription { get; set; }
            public long EJTodayLines { get; set; }
            public bool IsSupervisorMode { get; set; }
            public string LastJournalLine { get; set; }
            public string GetStatusText()
            {
        }
    
    }
    public partial class ATMInfo
        {
            private string _sourcePath, _backupPath;
    
    
            public ATMType ATMType
            {
                get
                {
                    switch (AppConstants.NormalizeATMType(ATM_Type))
                    {
                        case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                        case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                        case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
                        case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
                        case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
                        default:                        return ATMType.Other;
                    }
                }
                set
                {
                    ATM_Type = value switch
                    {
                        ATMType.NCR            => AppConstants.ATM_TYPE_NCR,
                        ATMType.GRG            => AppConstants.ATM_TYPE_GRG,
                        ATMType.WN             => AppConstants.ATM_TYPE_WN,
                        ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                        ATMType.Hyosung        => AppConstants.ATM_TYPE_HY,
                        _                      => "OTHER"
                    };
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            private string _sourcePath;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            private string _backupPath;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            private string _backupPath;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            private string _backupPath;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            private string _backupPath;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            public string ATMID { get; set; }
    
    
            public string BranchName { get; set; }
    
    
            public string IPAddress { get; set; }
    
    
            public ATMType Type { get; set; }
    
    
            public ATMStatus Status { get; set; }
    
    
            public DateTime LastSync { get; set; }
    
    
            public DateTime LastHeartbeat { get; set; }
    
    
            public long TotalBytesSynced { get; set; }
    
    
            public int FilesProcessed { get; set; }
    
    
            public bool IsOnline { get; set; }
    
    
            public string LastError { get; set; }
    
    
            public string OSVersion { get; set; }
    
    
            public string SoftwareVersion { get; set; }
    
    
            public string ATMName { get; set; }
    
    
            public string Location { get; set; }
    
    
            public DateTime LastSyncTime { get; set; }
    
    
            public decimal RemainingCash { get; set; }
    
    
            public decimal DispensedCash { get; set; }
    
    
            public string LastTransaction { get; set; }
    
    
            public string NetworkType { get; set; } // LAN, CDMA, GSM, ADSL
    
    
            public int CapturedCardsCount { get; set; }
    
    
            public string LastFaultCode { get; set; }
    
    
            public string LastFaultDescription { get; set; }
    
    
            public long EJTodayLines { get; set; }
    
    
            public bool IsSupervisorMode { get; set; }
    
    
            public string ATM_ID        { get; set; }
    
    
            public string ATM_Name      { get; set; }
    
    
            public string ATM_Type      { get; set; }    // NCR / GRG / WN
    
    
            public string Region        { get; set; }
    
    
            public string ServerIP      { get; set; }
    
    
            public int    ServerPort    { get; set; } = 5656;
    
    
            public int    Latency_ms    { get; set; }
    
    
            public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    
    
            public string           SessionId        { get; set; }
    
    
            public bool             IsHostConnected  { get; set; }
    
    
            public DateTime ConnectedAtUtc       { get; set; }
    
    
            public DateTime DisconnectedAtUtc    { get; set; }
    
    
            public DateTime LastHeartbeatUtc     { get; set; }
    
    
            public DateTime LastSyncUtc          { get; set; }
    
    
            public DateTime LastDataReceivedUtc  { get; set; }
    
    
            public DateTime LastCommandSentUtc   { get; set; }
    
    
            public long   TotalSyncedBytes         { get; set; }
    
    
            public long   TotalTransactions        { get; set; }
    
    
            public int    ConsecutiveSyncFailures  { get; set; }
    
    
            public double SyncSuccessRate          { get; set; } = 100.0;
    
    
            public double ReceiveSpeedKBs          { get; set; }
    
    
            public long   JournalSizeToday         { get; set; }
    
    
            public int  ApprovedTransactions  { get; set; }
    
    
            public int  FailedTransactions    { get; set; }
    
    
            public int  CardsCaptured         { get; set; }
    
    
            public long CashDispensed         { get; set; }
    
    
            public string LastJournalFile   { get; set; }
    
    
            public string LastErrorCode     { get; set; }
    
    
            public string LastErrorMessage  { get; set; }
    
    
            public string ClientVersion { get; set; }
    
    
            public string ATM_Description { get; set; }
    
    
            public bool IsConnected { get; set; }
    
    
            public bool IsSendingData { get; set; }
    
    
            public bool IsCSCConnected { get; set; }
    
    
            public DateTime LastConnectionTime { get; set; }
    
    
            public DateTime LastDataReceived { get; set; }
    
    
            public int[,] OperationStats { get; set; }
    
    
            public int ATMCache { get; set; }
    
    
            public int TotalDispensed { get; set; }
    
    
            public SyncStatus SyncState { get; set; }
    
    
            public long TotalBytesSent { get; set; }
    
    
            public int TotalFilesSynced { get; set; }
    
    
            public int TotalLinesSent { get; set; }
    
    
            public string LastSyncFile { get; set; }
    
    
            public string ATMId   { get => ATM_ID;   set => ATM_ID   = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string Location  { get => Region;   set => Region   = value; }
    
    
            public string BranchCode { get; set; }
    
    
            public int    Latency     { get => Latency_ms; set => Latency_ms = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            public double CpuUsagePercent         { get; set; }
    
    
            public double MemoryUsagePercent      { get; set; }
    
    
            public double DiskUsagePercent        { get; set; }
    
    
            public int    HealthScore             { get; set; } = 100;
    
    
            public int    PendingJournalCount     { get; set; }
    
    
            public double SuccessRate           { get => SyncSuccessRate;   set => SyncSuccessRate   = value; }
    
    
            public long   TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string Location  { get => Region;   set => Region   = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            public string LastJournalLine { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string ATMId { get; set; }
    
    
            public string ATMType { get; set; }
    
    
            public string Vendor { get; set; }
    
    
            public string Model { get; set; }
    
    
            public string Branch { get; set; }
    
    
            public string SourceJournalPath { get; set; }
    
    
            public string BackupPath { get; set; }
    
    
            public string ImageInboxPath { get; set; }
    
    
            public string ImageDestPath { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string ATMId { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string ATM_ID { get => ATMId; set => ATMId = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string ATM_Name { get => Name; set => Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string ServerIP { get => IPAddress; set => IPAddress = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeatUtc { get => LastHeartbeat; set => LastHeartbeat = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;
    
    
            public string Name { get; set; } = string.Empty;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string Location { get => Region; set => Region = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public int Latency { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public int Latency_ms { get => Latency; set => Latency = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public int TotalTransactionsSynced { get => (int)TotalTransactions; set => TotalTransactions = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string LastErrorMessage { get => LastError; set => LastError = value; }
    
    
            public bool HasCashTelemetry { get; set; }
    
    
            public long Cassette1Remaining { get; set; }
    
    
            public long Cassette2Remaining { get; set; }
    
    
            public long Cassette3Remaining { get; set; }
    
    
            public long Cassette4Remaining { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public long ATMCache { get; set; }
    
    
            public long CashLoadedTotal { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public long TotalDispensed { get; set; }
    
    
            public long CashDepositInTotal { get; set; }
    
    
            public long CashRejectCount { get; set; }
    
    
            public long CashRetractCount { get; set; }
    
    
            public DateTime CashTelemetryUpdatedAtUtc { get; set; }
    
    
            public bool HasAlerts { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string ATMId { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string ATM_ID { get => ATMId; set => ATMId = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string ATM_Name { get => Name; set => Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string ServerIP { get => IPAddress; set => IPAddress = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public DateTime LastHeartbeatUtc { get => LastHeartbeat; set => LastHeartbeat = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string Location { get => Region; set => Region = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public int Latency { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public int Latency_ms { get => Latency; set => Latency = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public int TotalTransactionsSynced { get => (int)TotalTransactions; set => TotalTransactions = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string LastErrorMessage { get => LastError; set => LastError = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public long ATMCache { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public long TotalDispensed { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string Location  { get => Region;   set => Region   = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string Location  { get => Region;   set => Region   = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string ATMId { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string ATMId { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string ATM_ID { get => ATMId; set => ATMId = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string ATM_Name { get => Name; set => Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string ServerIP { get => IPAddress; set => IPAddress = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeatUtc { get => LastHeartbeat; set => LastHeartbeat = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string Location { get => Region; set => Region = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public int Latency { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public int Latency_ms { get => Latency; set => Latency = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public int TotalTransactionsSynced { get => (int)TotalTransactions; set => TotalTransactions = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string LastErrorMessage { get => LastError; set => LastError = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public long ATMCache { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public long TotalDispensed { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string Location  { get => Region;   set => Region   = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string Location  { get => Region;   set => Region   = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-26\src\EJLive.Core\Models\ATMInfo.cs
            public string ATMId { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-28\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string ATMId { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string Location  { get => Region;   set => Region   = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public string ATMId { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public string ATM_ID { get => ATMId; set => ATMId = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public string ATM_Name { get => Name; set => Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public string ServerIP { get => IPAddress; set => IPAddress = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeatUtc { get => LastHeartbeat; set => LastHeartbeat = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public string Location { get => Region; set => Region = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public int Latency { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public int Latency_ms { get => Latency; set => Latency = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public string ATMName { get => Name; set => Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public int TotalTransactionsSynced { get => (int)TotalTransactions; set => TotalTransactions = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public string LastErrorMessage { get => LastError; set => LastError = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public long ATMCache { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public long TotalDispensed { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string Location  { get => Region;   set => Region   = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string Location  { get => Region;   set => Region   = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string Location  { get => Region;   set => Region   = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSync = DateTime.MinValue;
                LastHeartbeat = DateTime.MinValue;
                IsOnline = false;
                LastError = string.Empty;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\ATMInfo.cs
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSyncTime = DateTime.MinValue;
                RemainingCash = 0;
                DispensedCash = 0;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\ATMInfo.cs
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSyncTime = DateTime.MinValue;
                RemainingCash = 0;
                DispensedCash = 0;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status         = ATMStatus.Unknown;
                SyncState      = SyncStatus.Idle;
                ServerPort     = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status         = ATMStatus.Unknown;
                SyncState      = SyncStatus.Idle;
                ServerPort     = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\ATMInfo.cs
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSyncTime = DateTime.MinValue;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_replik\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                ATMId = string.Empty;
                IPAddress = string.Empty;
                ServerPort = AppConstants.DefaultPort;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSyncTime = DateTime.MinValue;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status         = ATMStatus.Unknown;
                SyncState      = SyncStatus.Idle;
                ServerPort     = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status         = ATMStatus.Unknown;
                SyncState      = SyncStatus.Idle;
                ServerPort     = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\ATMInfo.cs
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSyncTime = DateTime.MinValue;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_replik\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                ATMId = string.Empty;
                IPAddress = string.Empty;
                ServerPort = AppConstants.DefaultPort;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status         = ATMStatus.Unknown;
                SyncState      = SyncStatus.Idle;
                ServerPort     = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status         = ATMStatus.Unknown;
                SyncState      = SyncStatus.Idle;
                ServerPort     = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-18\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-19\EJLive_replik\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-20\EJLive.Core\ATMInfo.cs
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSyncTime = DateTime.MinValue;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-21\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status         = ATMStatus.Unknown;
                SyncState      = SyncStatus.Idle;
                ServerPort     = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-2\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                ATMId = string.Empty;
                IPAddress = string.Empty;
                ServerPort = AppConstants.DefaultPort;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v15_bak
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v16_bak
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v17_bak
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v20_bak
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v22_bak
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status         = ATMStatus.Unknown;
                SyncState      = SyncStatus.Idle;
                ServerPort     = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-3\EJLive.Core\ATMInfo.cs
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSyncTime = DateTime.MinValue;
                RemainingCash = 0;
                DispensedCash = 0;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status         = ATMStatus.Unknown;
                SyncState      = SyncStatus.Idle;
                ServerPort     = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status         = ATMStatus.Unknown;
                SyncState      = SyncStatus.Idle;
                ServerPort     = NetworkConfig.DEFAULT_PORT;
            }
    
    
            public string GetStatusText()
            {
                switch (Status)
                {
                    case ATMStatus.InService: return "In Service";
                    case ATMStatus.ConnectedOnly: return "Connected Only";
                    case ATMStatus.WaitingResponse: return "Waiting Response";
                    case ATMStatus.OutOfService: return "Out of Service";
                    case ATMStatus.CriticalFault: return "Critical Fault";
                    case ATMStatus.Offline: return "Offline";
                    default: return "Unknown";
                }
            }
    
    
            public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath) ? _sourcePath
                : ATM_Type == "NCR" ? @"C:\NCRJournal\"
                : ATM_Type == "GRG" ? @"D:\GRGData\EJ\"
                : ATM_Type == "WN"  ? @"C:\WOSA\EJ\"
                : @"C:\Journal\";
    
    
            public void SetSourcePath(string v) => _sourcePath = v;
    
    
            public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                : System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
    
    
            public void SetBackupPath(string v) => _backupPath = v;
    
    
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5)  return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing)     return ATMCardState.Syncing;
                if (IsSupervisorMode)                                 return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
    
    
            public Color GetCardColor()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),   // أخضر
                    ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),   // أصفر
                    ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),  // أزرق
                    ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),  // أزرق
                    ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),   // برتقالي
                    ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),   // أحمر
                    ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),   // أحمر
                    ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),  // رمادي
                    ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),   // رمادي داكن
                    _ => Color.Gray
                };
            }
    
    
            public string GetStatusLabel()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive      => "● متصل ونشط",
                    ATMCardState.ConnectedIdle        => "● متصل خامل",
                    ATMCardState.Syncing              => "⟳ يزامن",
                    ATMCardState.WaitingReply         => "◎ ينتظر رد",
                    ATMCardState.Supervisor           => "★ Supervisor",
                    ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                    ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                    ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                    ATMCardState.NeverConnected       => "○ لم يتصل",
                    _ => "?"
                };
            }
    
    
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s/60)}د";
                return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
            }
    
    
            public override string ToString() =>
                $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
    
            public string GetStatusColor()
            {
                if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                if (!IsConnected)
                {
                    var elapsed = DateTime.Now - LastConnectionTime;
                    if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_OFFLINE;
                    if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_WARNING;
                    return ATMStatusColors.COLOR_OFFLINE;
                }
                if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                var idleElapsed = DateTime.Now - LastDataReceived;
                if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                    return ATMStatusColors.COLOR_IDLE;
                return ATMStatusColors.COLOR_ACTIVE;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                    default: return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            public bool NeedsAlert()
            {
                if (!IsConnected) return true;
                return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            public string GetLegacySourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_SOURCE;
                    default:                        return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            public string GetLegacyBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_BACKUP;
                    default:                        return string.Empty;
                }
            }
    
    
            public string GetStatusDescription()        => GetStatusLabel();
    
    
            public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            public void RecalculateHealthScore()
            {
                var score = 100;
                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                if (Latency_ms > 500) score -= 15;
                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                if (CpuUsagePercent    > 90) score -= 10;
                if (MemoryUsagePercent > 90) score -= 10;
                if (DiskUsagePercent   > 95) score -= 10;
                HealthScore = Math.Max(0, Math.Min(100, score));
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_replik\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_replik\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            public string GetElapsed(DateTime? dt) => !dt.HasValue ? "—" : $"{(DateTime.UtcNow - dt.Value).TotalMinutes:N0}m";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string GetStatusDescription() => Status.ToString();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public bool NeedsAlert() => HasAlerts;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeat == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeat).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5) return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing) return ATMCardState.Syncing;
                if (IsSupervisorMode) return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public void RecalculateHealthScore()
            {
                var score = 100;
                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                if (Latency > 500) score -= 15;
                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                if (CpuUsagePercent > 90) score -= 10;
                if (MemoryUsagePercent > 90) score -= 10;
                if (DiskUsagePercent > 95) score -= 10;
                HealthScore = Math.Max(0, Math.Min(100, score));
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public override string ToString() =>
                $"[{ATMId}] {Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string GetStatusDescription() => Status.ToString();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public bool NeedsAlert() => HasAlerts;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeat == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeat).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5) return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing) return ATMCardState.Syncing;
                if (IsSupervisorMode) return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public void RecalculateHealthScore()
            {
                var score = 100;
                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                if (Latency > 500) score -= 15;
                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                if (CpuUsagePercent > 90) score -= 10;
                if (MemoryUsagePercent > 90) score -= 10;
                if (DiskUsagePercent > 95) score -= 10;
                HealthScore = Math.Max(0, Math.Min(100, score));
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public override string ToString() =>
                $"[{ATMId}] {Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_replik\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_replik\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string GetStatusDescription() => Status.ToString();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public bool NeedsAlert() => HasAlerts;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeat == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeat).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5) return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing) return ATMCardState.Syncing;
                if (IsSupervisorMode) return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public void RecalculateHealthScore()
            {
                var score = 100;
                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                if (Latency > 500) score -= 15;
                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                if (CpuUsagePercent > 90) score -= 10;
                if (MemoryUsagePercent > 90) score -= 10;
                if (DiskUsagePercent > 95) score -= 10;
                HealthScore = Math.Max(0, Math.Min(100, score));
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public override string ToString() =>
                $"[{ATMId}] {Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-18\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-18\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-19\EJLive_replik\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-19\EJLive_replik\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-21\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-21\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-2\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-2\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public string GetStatusDescription() => Status.ToString();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public bool NeedsAlert() => HasAlerts;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath)
                ? _sourcePath
                : AppConstants.GetDefaultSourcePath(ATM_Type);
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                : System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATMId ?? "DEFAULT");
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeat == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeat).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5) return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing) return ATMCardState.Syncing;
                if (IsSupervisorMode) return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public void RecalculateHealthScore()
            {
                var score = 100;
                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                if (Latency > 500) score -= 15;
                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                if (CpuUsagePercent > 90) score -= 10;
                if (MemoryUsagePercent > 90) score -= 10;
                if (DiskUsagePercent > 95) score -= 10;
                HealthScore = Math.Max(0, Math.Min(100, score));
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public override string ToString() =>
                $"[{ATMId}] {Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v15_bak
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v15_bak
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v16_bak
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v16_bak
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v17_bak
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v17_bak
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v20_bak
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v20_bak
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v22_bak
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v22_bak
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
                public class ATMInfo
                {
                    // هوية
                    public string ATM_ID
                    {
                        get;
                        set;
                    }
                    public string ATM_Name
                    {
                        get;
                        set;
                    }
                    public string ATM_Type
                    {
                        get;
                        set;
                    } // NCR / GRG / WN / DIEBOLD / HYOSUNG
                    public string BranchName
                    {
                        get;
                        set;
                    }
                    public string Region
                    {
                        get;
                        set;
                    }
                    public string ATMId
                    {
                        get => ATM_ID;
                        set => ATM_ID = value;
                    }
                    public string ATMName
                    {
                        get => ATM_Name;
                        set => ATM_Name = value;
                    }
                    public string IPAddress
                    {
                        get => ServerIP;
                        set => ServerIP = value;
                    }
                    public ATMType ATMType
                    {
                        get
                        {
                            switch (AppConstants.NormalizeATMType(ATM_Type))
                            {
                                case AppConstants.ATM_TYPE_NCR:
                                    return ATMType.NCR;
                                case AppConstants.ATM_TYPE_GRG:
                                    return ATMType.GRG;
                                case AppConstants.ATM_TYPE_WN:
                                    return ATMType.WN;
                                case AppConstants.ATM_TYPE_DN:
                                    return ATMType.DieboldNixdorf;
                                case AppConstants.ATM_TYPE_HY:
                                    return ATMType.Hyosung;
                                default:
                                    return ATMType.Other;
                            }
                        }
                        set
                        {
                            ATM_Type = value
                            switch
                            {
                                ATMType.NCR => AppConstants.ATM_TYPE_NCR,
                                ATMType.GRG => AppConstants.ATM_TYPE_GRG,
                                ATMType.WN => AppConstants.ATM_TYPE_WN,
                                ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                                ATMType.Hyosung => AppConstants.ATM_TYPE_HY,
                                _ => "OTHER"
                            };
                        }
                    }
                    public string Location
                    {
                        get => Region;
                        set => Region = value;
                    }
                    public string BranchCode
                    {
                        get;
                        set;
                    }
    
                    // شبكة
                    public string ServerIP
                    {
                        get;
                        set;
                    }
                    public int ServerPort
                    {
                        get;
                        set;
                    } = 5656;
                    public string NetworkType
                    {
                        get;
                        set;
                    } = "LAN";
                    public int Latency_ms
                    {
                        get;
                        set;
                    }
                    public int Latency
                    {
                        get => Latency_ms;
                        set => Latency_ms = value;
                    }
    
                    // حالة الاتصال
                    public ConnectionStatus ConnectionStatus
                    {
                        get;
                        set;
                    } = ConnectionStatus.Disconnected;
                    public ATMStatus Status
                    {
                        get;
                        set;
                    } = ATMStatus.Unknown;
                    public string SessionId
                    {
                        get;
                        set;
                    }
                    public bool IsSupervisorMode
                    {
                        get;
                        set;
                    }
                    public bool IsHostConnected
                    {
                        get;
                        set;
                    }
    
                    // طوابع زمنية UTC (T-11)
                    public DateTime ConnectedAtUtc
                    {
                        get;
                        set;
                    }
                    public DateTime DisconnectedAtUtc
                    {
                        get;
                        set;
                    }
                    public DateTime LastHeartbeatUtc
                    {
                        get;
                        set;
                    }
                    public DateTime LastSyncUtc
                    {
                        get;
                        set;
                    }
                    public DateTime LastDataReceivedUtc
                    {
                        get;
                        set;
                    }
                    public DateTime LastCommandSentUtc
                    {
                        get;
                        set;
                    }
                    public DateTime LastConnectionTime
                    {
                        get => ConnectedAtUtc;
                        set => ConnectedAtUtc = value;
                    }
                    public DateTime LastSyncTime
                    {
                        get => LastSyncUtc;
                        set => LastSyncUtc = value;
                    }
    
                    // إحصاءات المزامنة
                    public long TotalSyncedBytes
                    {
                        get;
                        set;
                    }
                    public long TotalTransactions
                    {
                        get;
                        set;
                    }
                    public int ConsecutiveSyncFailures
                    {
                        get;
                        set;
                    }
                    public double SyncSuccessRate
                    {
                        get;
                        set;
                    } = 100.0;
                    public double ReceiveSpeedKBs
                    {
                        get;
                        set;
                    }
                    public long JournalSizeToday
                    {
                        get;
                        set;
                    }
                    public double CpuUsagePercent
                    {
                        get;
                        set;
                    }
                    public double MemoryUsagePercent
                    {
                        get;
                        set;
                    }
                    public double DiskUsagePercent
                    {
                        get;
                        set;
                    }
                    public int HealthScore
                    {
                        get;
                        set;
                    } = 100;
                    public int PendingJournalCount
                    {
                        get;
                        set;
                    }
                    public double SuccessRate
                    {
                        get => SyncSuccessRate;
                        set => SyncSuccessRate = value;
                    }
                    public long TotalTransactionsSynced
                    {
                        get => TotalTransactions;
                        set => TotalTransactions = value;
                    }
    
                    // إحصاءات العمليات
                    public int ApprovedTransactions
                    {
                        get;
                        set;
                    }
                    public int FailedTransactions
                    {
                        get;
                        set;
                    }
                    public int CardsCaptured
                    {
                        get;
                        set;
                    }
                    public long CashDispensed
                    {
                        get;
                        set;
                    }
    
                    // آخر جورنال / خطأ
                    public string LastJournalFile
                    {
                        get;
                        set;
                    }
                    public string LastErrorCode
                    {
                        get;
                        set;
                    }
                    public string LastErrorMessage
                    {
                        get;
                        set;
                    }
                    public string LastTransaction
                    {
                        get;
                        set;
                    }
                    public string LastError
                    {
                        get => LastErrorMessage;
                        set => LastErrorMessage = value;
                    }
                    public int TransactionCount
                    {
                        get => (int)Math.Min(int.MaxValue, TotalTransactions);
                        set => TotalTransactions = value;
                    }
                    public bool IsSyncing
                    {
                        get => ConnectionStatus == ConnectionStatus.Syncing;
                        set
                        {
                            if (value) ConnectionStatus = ConnectionStatus.Syncing;
                        }
                    }
    
                    // مسارات
                    private string _sourcePath, _backupPath;
                    public string ClientVersion
                    {
                        get;
                        set;
                    }
                    public string OSVersion
                    {
                        get;
                        set;
                    }
    
                    public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath) ?
                    _sourcePath :
                    AppConstants.GetDefaultSourcePath(ATM_Type);
    
                    public void SetSourcePath(string v) => _sourcePath = v;
    
                    public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath :
                    System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                        "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
    
                    public void SetBackupPath(string v) => _backupPath = v;
    
                    // ==========================================
                    // حالة البطاقة ولونها
                    // ==========================================
    
                    public ATMCardState GetCardState()
                    {
                        if (ConnectionStatus == ConnectionStatus.Disconnected)
                        {
                            if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                            var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                            if (mins > 10) return ATMCardState.CriticalOffline;
                            if (mins > 5) return ATMCardState.WarningOffline;
                            return ATMCardState.RecentlyDisconnected;
                        }
                        if (ConnectionStatus == ConnectionStatus.Syncing) return ATMCardState.Syncing;
                        if (IsSupervisorMode) return ATMCardState.Supervisor;
                        if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                        var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                        return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                    }
    
                    public Color GetCardColor()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive => Color.FromArgb(52, 199, 89), // أخضر
                            ATMCardState.ConnectedIdle => Color.FromArgb(255, 214, 10), // أصفر
                            ATMCardState.Syncing => Color.FromArgb(10, 132, 255), // أزرق
                            ATMCardState.WaitingReply => Color.FromArgb(10, 132, 255), // أزرق
                            ATMCardState.Supervisor => Color.FromArgb(255, 159, 10), // برتقالي
                            ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69, 58), // أحمر
                            ATMCardState.WarningOffline => Color.FromArgb(255, 69, 58), // أحمر
                            ATMCardState.CriticalOffline => Color.FromArgb(99, 99, 102), // رمادي
                            ATMCardState.NeverConnected => Color.FromArgb(72, 72, 74), // رمادي داكن
                            _ => Color.Gray
                        };
                    }
    
                    public string GetStatusLabel()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive => "● متصل ونشط",
                            ATMCardState.ConnectedIdle => "● متصل خامل",
                            ATMCardState.Syncing => "⟳ يزامن",
                            ATMCardState.WaitingReply => "◎ ينتظر رد",
                            ATMCardState.Supervisor => "★ Supervisor",
                            ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                            ATMCardState.WarningOffline => "✕ انقطاع >5د",
                            ATMCardState.CriticalOffline => "✕ انقطاع حرج",
                            ATMCardState.NeverConnected => "○ لم يتصل",
                            _ => "?"
                        };
                    }
    
                    public string GetStatusDescription() => GetStatusLabel();
    
                    public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
                    public string GetElapsed(DateTime utcRef)
                    {
                        if (utcRef == DateTime.MinValue) return "---";
                        var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                        if (s < 60) return $"{(int)s}ث";
                        if (s < 3600) return $"{(int)(s/60)}د";
                        return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
                    }
    
                    public void RecalculateHealthScore()
                    {
                        var score = 100;
                        if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                        if (Latency_ms > 500) score -= 15;
                        if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                        if (CpuUsagePercent > 90) score -= 10;
                        if (MemoryUsagePercent > 90) score -= 10;
                        if (DiskUsagePercent > 95) score -= 10;
                        HealthScore = Math.Max(0, Math.Min(100, score));
                    }
    
                    public override string ToString() =>
                    $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
            }
    
    
            public class AlertPayload
            {
                public string AlertId
                {
                    get;
                    set;
                } = Guid.NewGuid().ToString("N");
                public AlertSeverity Severity
                {
                    get;
                    set;
                }
                public string Title
                {
                    get;
                    set;
                }
                public string Message
                {
                    get;
                    set;
                }
                public string Source
                {
                    get;
                    set;
                }
                public string DedupeKey
                {
                    get;
                    set;
                }
                public DateTime CreatedAt
                {
                    get;
                    set;
                } = DateTime.UtcNow;
                public bool IsRead
                {
                    get;
                    set;
                }
    
                public string Icon => Severity
                switch
                {
                    AlertSeverity.Emergency => "CRIT",
                    AlertSeverity.Critical => "FAIL",
                    AlertSeverity.Warning => "WARN",
                    _ => "INFO"
                };
                public string SeverityIcon => Icon;
                public Color Color => Severity
                switch
                {
                    AlertSeverity.Emergency => Color.FromArgb(255, 59, 48),
                    AlertSeverity.Critical => Color.FromArgb(255, 59, 48),
                    AlertSeverity.Warning => Color.FromArgb(255, 149, 0),
                    _ => Color.FromArgb(0, 122, 255)
                };
            }
    
    
            public class JournalSyncRecord
            {
                public string SyncId
                {
                    get;
                    set;
                } = Guid.NewGuid().ToString("N");
                public string ATM_ID
                {
                    get;
                    set;
                }
                public string FileName
                {
                    get;
                    set;
                }
                public long FileSize
                {
                    get;
                    set;
                }
                public long FileOffset
                {
                    get;
                    set;
                }
                public string Checksum
                {
                    get;
                    set;
                }
                public string MD5Hash
                {
                    get;
                    set;
                }
                public string SHA256Hash
                {
                    get;
                    set;
                }
                public JournalSyncState State
                {
                    get;
                    set;
                }
                public int ProgressPercent
                {
                    get;
                    set;
                }
                public int RetryCount
                {
                    get;
                    set;
                }
                public string LocalPath
                {
                    get;
                    set;
                }
                public string ServerPath
                {
                    get;
                    set;
                }
                public string Message
                {
                    get;
                    set;
                }
                public DateTime CreatedAtUtc
                {
                    get;
                    set;
                } = DateTime.UtcNow;
                public DateTime UpdatedAtUtc
                {
                    get;
                    set;
                } = DateTime.UtcNow;
                public DateTime? CompletedAtUtc
                {
                    get;
                    set;
                }
    
                public string StateIcon => State
                switch
                {
                    JournalSyncState.Pending => "PEND",
                    JournalSyncState.Syncing => "SYNC",
                    JournalSyncState.ReSyncing => "RSYNC",
                    JournalSyncState.Completed => "OK",
                    JournalSyncState.Failed => "FAIL",
                    JournalSyncState.Archived => "ARCH",
                    _ => "?"
                };
    
                public string StateLabel => State
                switch
                {
                    JournalSyncState.Pending => "في الطابور",
                    JournalSyncState.Syncing => "قيد المزامنة",
                    JournalSyncState.ReSyncing => "إعادة مزامنة",
                    JournalSyncState.Completed => "محمّل",
                    JournalSyncState.Failed => "فشل",
                    JournalSyncState.Archived => "مؤرشف",
                    _ => "؟"
                };
    
                public string ATM_ID
                {
                    get;
                    set;
                }
                public string ATM_Name
                {
                    get;
                    set;
                }
                public string ATM_Description
                {
                    get;
                    set;
                }
                public string ATM_Type
                {
                    get;
                    set;
                }
                public string ServerIP
                {
                    get;
                    set;
                }
                public int ServerPort
                {
                    get;
                    set;
                }
                public ATMStatus Status
                {
                    get;
                    set;
                }
                public bool IsConnected
                {
                    get;
                    set;
                }
                public bool IsSendingData
                {
                    get;
                    set;
                }
                public bool IsCSCConnected
                {
                    get;
                    set;
                }
                public DateTime LastConnectionTime
                {
                    get;
                    set;
                }
                public DateTime LastDataReceived
                {
                    get;
                    set;
                }
                public DateTime LastHeartbeat
                {
                    get;
                    set;
                }
                public int[,] OperationStats
                {
                    get;
                    set;
                }
                public int ATMCache
                {
                    get;
                    set;
                }
                public int TotalDispensed
                {
                    get;
                    set;
                }
                public SyncStatus SyncState
                {
                    get;
                    set;
                }
                public long TotalBytesSent
                {
                    get;
                    set;
                }
                public int TotalFilesSynced
                {
                    get;
                    set;
                }
                public int TotalLinesSent
                {
                    get;
                    set;
                }
                public DateTime LastSyncTime
                {
                    get;
                    set;
                }
                public string LastSyncFile
                {
                    get;
                    set;
                }
                public string OSVersion
                {
                    get;
                    set;
                }
                public string ClientVersion
                {
                    get;
                    set;
                }
    
                public ATMInfo()
                {
                    OperationStats = new int[3, 4];
                    Status = ATMStatus.Unknown;
                    SyncState = SyncStatus.Idle;
                    ServerPort = NetworkConfig.DEFAULT_PORT;
                }
    
                public string GetStatusColor()
                {
                    if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                    if (!IsConnected)
                    {
                        var elapsed = DateTime.Now - LastConnectionTime;
                        if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                            return ATMStatusColors.COLOR_OFFLINE;
                        if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                            return ATMStatusColors.COLOR_WARNING;
                        return ATMStatusColors.COLOR_OFFLINE;
                    }
                    if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                    var idleElapsed = DateTime.Now - LastDataReceived;
                    if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                        return ATMStatusColors.COLOR_IDLE;
                    return ATMStatusColors.COLOR_ACTIVE;
                }
    
                public string GetSourcePath()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR:
                            return ATMPaths.NCR_SOURCE;
                        case AppConstants.ATM_TYPE_GRG:
                            return ATMPaths.GRG_SOURCE;
                        case AppConstants.ATM_TYPE_WN:
                            return ATMPaths.WN_SOURCE;
                        default:
                            return string.Empty;
                    }
                }
    
                public string GetBackupPath()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR:
                            return ATMPaths.NCR_BACKUP;
                        case AppConstants.ATM_TYPE_GRG:
                            return ATMPaths.GRG_BACKUP;
                        case AppConstants.ATM_TYPE_WN:
                            return ATMPaths.WN_BACKUP;
                        default:
                            return string.Empty;
                    }
                }
    
                public SyncStrategy GetSyncStrategy()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR:
                            return SyncStrategy.NCR_Overwrite;
                        case AppConstants.ATM_TYPE_GRG:
                            return SyncStrategy.GRG_DailyFiles;
                        case AppConstants.ATM_TYPE_WN:
                            return SyncStrategy.WN_DailyFiles;
                        default:
                            return SyncStrategy.NCR_Overwrite;
                    }
                }
    
                public bool NeedsAlert()
                {
                    if (!IsConnected) return true;
                    return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                }
            }
    
    
            public class ClientConfig
            {
                public string ATM_ID
                {
                    get;
                    set;
                }
                public string ATM_Name
                {
                    get;
                    set;
                }
                public string ATM_Type
                {
                    get;
                    set;
                }
                public string ServerIP
                {
                    get;
                    set;
                }
                public int ServerPort
                {
                    get;
                    set;
                }
                public bool SyncTimeEnabled
                {
                    get;
                    set;
                }
                public int MessageSizeLines
                {
                    get;
                    set;
                }
                public int FilePackageKB
                {
                    get;
                    set;
                }
                public string SourcePath
                {
                    get;
                    set;
                }
                public string BackupPath
                {
                    get;
                    set;
                }
                public bool AutoStart
                {
                    get;
                    set;
                }
                public bool RunAsService
                {
                    get;
                    set;
                }
    
                public ClientConfig()
                {
                    ServerPort = NetworkConfig.DEFAULT_PORT;
                    MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                    FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                    AutoStart = true;
                    RunAsService = true;
                }
            }
    
    
            public class ServerConfig
            {
                public int ListenPort
                {
                    get;
                    set;
                }
                public string StoragePath
                {
                    get;
                    set;
                }
                public string ArchivePath
                {
                    get;
                    set;
                }
                public bool AutoArchive
                {
                    get;
                    set;
                }
                public int MaxConnections
                {
                    get;
                    set;
                }
                public bool EnableEncryption
                {
                    get;
                    set;
                }
                public bool EnableCompression
                {
                    get;
                    set;
                }
    
                public ServerConfig()
                {
                    ListenPort = NetworkConfig.DEFAULT_PORT;
                    StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                    ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                    AutoArchive = true;
                    MaxConnections = 100;
                    EnableEncryption = true;
                    EnableCompression = true;
                }
            }
    
    
            public enum ConnectionStatus
            {
                Disconnected = 0, Connecting = 1, Connected = 2, WaitingReply = 3, Syncing = 4
            }
    
    
            public enum ATMStatus
            {
                Unknown = 0,
                Online = 1,
                Idle = 2,
                Supervisor = 3,
                Warning = 4,
                Offline = 5,
                Critical = 6,
                InService = 10,
                ConnectedOnly = 11,
                WaitingResponse = 12,
                OutOfService = 13,
                CriticalFault = 14,
                Fault = 15,
                Maintenance = 16
            }
    
    
            public enum ATMType
            {
                NCR,
                GRG,
                WN,
                DieboldNixdorf,
                Hyosung,
                Other
            }
    
    
            public enum SyncStatus
            {
                Pending,
                InProgress,
                Syncing,
                Resyncing,
                Completed,
                Failed
            }
    
    
            public enum AlertSeverity
            {
                Info = 0, Warning = 1, Critical = 2, Emergency = 3
            }
    
    
            public enum JournalSyncState
            {
                Pending = 0, Syncing = 1, ReSyncing = 2, Completed = 3, Failed = 4, Archived = 5
            }
    
    
            public enum ATMCardState
            {
                NeverConnected,
                ConnectedActive,
                ConnectedIdle,
                Syncing,
                WaitingReply,
                Supervisor,
                RecentlyDisconnected,
                WarningOffline,
                CriticalOffline
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Core\Models\ATMInfo.cs
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Core\Models\ATMInfo.cs
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Core\Models\ATMInfo.cs
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
        }
    public partial class ATMInfo
        {
            public string ATMID { get; set; }
    
    
            public string BranchName { get; set; }
    
    
            public string IPAddress { get; set; }
    
    
            public ATMType Type { get; set; }
    
    
            public ATMStatus Status { get; set; }
    
    
            public DateTime LastSync { get; set; }
    
    
            public DateTime LastHeartbeat { get; set; }
    
    
            public long TotalBytesSynced { get; set; }
    
    
            public int FilesProcessed { get; set; }
    
    
            public bool IsOnline { get; set; }
    
    
            public string LastError { get; set; }
    
    
            public string OSVersion { get; set; }
    
    
            public string SoftwareVersion { get; set; }
    
    
            public string ATMName { get; set; }
    
    
            public string Location { get; set; }
    
    
            public DateTime LastSyncTime { get; set; }
    
    
            public decimal RemainingCash { get; set; }
    
    
            public decimal DispensedCash { get; set; }
    
    
            public string LastTransaction { get; set; }
    
    
            public string NetworkType { get; set; } // LAN, CDMA, GSM, ADSL
    
    
            public int CapturedCardsCount { get; set; }
    
    
            public string LastFaultCode { get; set; }
    
    
            public string LastFaultDescription { get; set; }
    
    
            public long EJTodayLines { get; set; }
    
    
            public bool IsSupervisorMode { get; set; }
    
    
            public string LastJournalLine { get; set; }
    
    
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSync = DateTime.MinValue;
                LastHeartbeat = DateTime.MinValue;
                IsOnline = false;
                LastError = string.Empty;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\ATMInfo.cs
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSyncTime = DateTime.MinValue;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\ATMInfo.cs
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSyncTime = DateTime.MinValue;
                RemainingCash = 0;
                DispensedCash = 0;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_WinForms_v3.2.1\EJLive.Core\ATMInfo.cs
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSyncTime = DateTime.MinValue;
                RemainingCash = 0;
                DispensedCash = 0;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_System\EJLive.Core\ATMInfo.cs
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSyncTime = DateTime.MinValue;
            }
    
    
            public string GetStatusText()
            {
                switch (Status)
                {
                    case ATMStatus.InService: return "In Service";
                    case ATMStatus.ConnectedOnly: return "Connected Only";
                    case ATMStatus.WaitingResponse: return "Waiting Response";
                    case ATMStatus.OutOfService: return "Out of Service";
                    case ATMStatus.CriticalFault: return "Critical Fault";
                    case ATMStatus.Offline: return "Offline";
                    default: return "Unknown";
                }
            }
    
    
        }
    [Serializable]
        public class ATMInfo
        {
            public string ATMID { get; set; }
            public string BranchName { get; set; }
            public string IPAddress { get; set; }
            public ATMType Type { get; set; }
            public ATMStatus Status { get; set; }
            public DateTime LastSync { get; set; }
            public DateTime LastHeartbeat { get; set; }
            public long TotalBytesSynced { get; set; }
            public int FilesProcessed { get; set; }
            public bool IsOnline { get; set; }
            public string LastError { get; set; }
            public string OSVersion { get; set; }
            public string SoftwareVersion { get; set; }
    
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSync = DateTime.MinValue;
                LastHeartbeat = DateTime.MinValue;
                IsOnline = false;
                LastError = string.Empty;
            }
    
            public string GetStatusText()
            {
                switch (Status)
                {
                    case ATMStatus.InService: return "In Service";
                    case ATMStatus.ConnectedOnly: return "Connected Only";
                    case ATMStatus.WaitingResponse: return "Waiting Response";
                    case ATMStatus.OutOfService: return "Out of Service";
                    case ATMStatus.CriticalFault: return "Critical Fault";
                    case ATMStatus.Offline: return "Offline";
                    default: return "Unknown";
                }
            }
        }
    public class ATMInfo
        {
            public string ATMID { get; set; }
            public string ATMName { get; set; }
            public string Location { get; set; }
            public ATMType Type { get; set; }
            public string IPAddress { get; set; }
            public ATMStatus Status { get; set; }
            public DateTime LastSyncTime { get; set; }
    
            // Cash Info
            public decimal RemainingCash { get; set; }
            public decimal DispensedCash { get; set; }
            public string LastTransaction { get; set; }
    
            // Network & System
            public string NetworkType { get; set; } // LAN, CDMA, GSM, ADSL
            public int CapturedCardsCount { get; set; }
            public string OSVersion { get; set; } // XP, Win7, Win10, Win11
    
            // Faults & EJ
            public string LastFaultCode { get; set; }
            public string LastFaultDescription { get; set; }
            public long EJTodayLines { get; set; }
            public bool IsSupervisorMode { get; set; }
    
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSyncTime = DateTime.MinValue;
                RemainingCash = 0;
                DispensedCash = 0;
            }
        }
    public class ATMInfo
        {
            public string ATMID { get; set; }
            public string ATMName { get; set; }
            public ATMType Type { get; set; }
            public string IPAddress { get; set; }
            public ATMStatus Status { get; set; }
            public DateTime LastSyncTime { get; set; }
            public decimal RemainingCash { get; set; }
            public decimal DispensedCash { get; set; }
            public string NetworkType { get; set; } // LAN, CDMA, GSM
            public int CapturedCardsCount { get; set; }
            public string LastJournalLine { get; set; }
    
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSyncTime = DateTime.MinValue;
            }
        }
    public partial class ATMInfo
        {
            private string _sourcePath;
            private string _backupPath;
            public ATMType ATMType
            {
                get
                {
                    switch (AppConstants.NormalizeATMType(ATM_Type))
                    {
                        case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                        case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                        case AppConstants.ATM_TYPE_WN: return ATMType.WN;
                        case AppConstants.ATM_TYPE_DN: return ATMType.DieboldNixdorf;
                        case AppConstants.ATM_TYPE_HY: return ATMType.Hyosung;
                        default: return ATMType.Other;
                    }
                }
                set
                {
                    ATM_Type = value switch
                    {
                        ATMType.NCR => AppConstants.ATM_TYPE_NCR,
                        ATMType.GRG => AppConstants.ATM_TYPE_GRG,
                        ATMType.WN => AppConstants.ATM_TYPE_WN,
                        ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                        ATMType.Hyosung => AppConstants.ATM_TYPE_HY,
                        _ => "OTHER"
                    };
                }
            }
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
            private string _sourcePath, _backupPath;
            public string ATMID { get; set; }
            public string BranchName { get; set; }
            public string IPAddress { get; set; }
            public ATMType Type { get; set; }
            public ATMStatus Status { get; set; }
            public DateTime LastSync { get; set; }
            public DateTime LastHeartbeat { get; set; }
            public long TotalBytesSynced { get; set; }
            public int FilesProcessed { get; set; }
            public bool IsOnline { get; set; }
            public string LastError { get; set; }
            public string OSVersion { get; set; }
            public string SoftwareVersion { get; set; }
            public string ATMName { get; set; }
            public string Location { get; set; }
            public DateTime LastSyncTime { get; set; }
            public decimal RemainingCash { get; set; }
            public decimal DispensedCash { get; set; }
            public string LastTransaction { get; set; }
            public string NetworkType { get; set; } // LAN, CDMA, GSM, ADSL
            public int CapturedCardsCount { get; set; }
            public string LastFaultCode { get; set; }
            public string LastFaultDescription { get; set; }
            public long EJTodayLines { get; set; }
            public bool IsSupervisorMode { get; set; }
            public string LastJournalLine { get; set; }
            public string ATM_ID { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Description { get; set; }
            public string ATM_Type { get; set; }
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }
            public bool IsConnected { get; set; }
            public bool IsSendingData { get; set; }
            public bool IsCSCConnected { get; set; }
            public DateTime LastConnectionTime { get; set; }
            public DateTime LastDataReceived { get; set; }
            public int[,] OperationStats { get; set; }
            public int ATMCache { get; set; }
            public int TotalDispensed { get; set; }
            public SyncStatus SyncState { get; set; }
            public long TotalBytesSent { get; set; }
            public int TotalFilesSynced { get; set; }
            public int TotalLinesSent { get; set; }
            public string LastSyncFile { get; set; }
            public string ClientVersion { get; set; }
            public string ATMId { get; set; }
            public string ATM_ID { get => ATMId; set => ATMId = value; }
            public string ATM_Name { get => Name; set => Name = value; }
            public string ServerIP { get => IPAddress; set => IPAddress = value; }
            public DateTime LastHeartbeatUtc { get => LastHeartbeat; set => LastHeartbeat = value; }
            public long TotalSyncedBytes { get; set; }
            public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;
            public string Name { get; set; } = string.Empty;
            public string Region { get; set; } = string.Empty;
            public string Location  { get => Region;   set => Region   = value; }
            public string BranchCode { get; set; } = string.Empty;
            public int Latency { get; set; }
            public int Latency_ms { get => Latency; set => Latency = value; }
            public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
            public string SessionId { get; set; }
            public bool IsHostConnected { get; set; }
            public DateTime ConnectedAtUtc { get; set; }
            public DateTime DisconnectedAtUtc { get; set; }
            public DateTime LastSyncUtc { get; set; }
            public DateTime LastDataReceivedUtc { get; set; }
            public DateTime LastCommandSentUtc { get; set; }
            public long TotalTransactions { get; set; }
            public int ConsecutiveSyncFailures { get; set; }
            public double SyncSuccessRate { get; set; } = 100.0;
            public double ReceiveSpeedKBs { get; set; }
            public long JournalSizeToday { get; set; }
            public double CpuUsagePercent { get; set; }
            public double MemoryUsagePercent { get; set; }
            public double DiskUsagePercent { get; set; }
            public int HealthScore { get; set; } = 100;
            public int PendingJournalCount { get; set; }
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
            public double SuccessRate { get => SyncSuccessRate; set => SyncSuccessRate = value; }
            public int TotalTransactionsSynced { get => (int)TotalTransactions; set => TotalTransactions = value; }
            public int ApprovedTransactions { get; set; }
            public int FailedTransactions { get; set; }
            public int CardsCaptured { get; set; }
            public long CashDispensed { get; set; }
            public string LastJournalFile { get; set; }
            public string LastErrorCode { get; set; }
            public string LastErrorMessage { get => LastError; set => LastError = value; }
            public bool HasCashTelemetry { get; set; }
            public long Cassette1Remaining { get; set; }
            public long Cassette2Remaining { get; set; }
            public long Cassette3Remaining { get; set; }
            public long Cassette4Remaining { get; set; }
            public long ATMCache { get; set; }
            public long CashLoadedTotal { get; set; }
            public long TotalDispensed { get; set; }
            public long CashDepositInTotal { get; set; }
            public long CashRejectCount { get; set; }
            public long CashRetractCount { get; set; }
            public DateTime CashTelemetryUpdatedAtUtc { get; set; }
            public bool HasAlerts { get; set; }
            public string ATMType { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string Branch { get; set; }
            public string SourceJournalPath { get; set; }
            public string BackupPath { get; set; }
            public string ImageInboxPath { get; set; }
            public string ImageDestPath { get; set; }
            public string ATMId   { get => ATM_ID;   set => ATM_ID   = value; }
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
            public int    Latency_ms    { get; set; }
            public int    Latency     { get => Latency_ms; set => Latency_ms = value; }
            public DateTime LastHeartbeatUtc     { get; set; }
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
            public long   TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
            public string LastErrorMessage  { get; set; }
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
            public ATMInfo()
            {
                Status = ATMStatus.Offline;
                LastSync = DateTime.MinValue;
                LastHeartbeat = DateTime.MinValue;
                IsOnline = false;
                LastError = string.Empty;
            }
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
            public string GetStatusText()
            {
                switch (Status)
                {
                    case ATMStatus.InService: return "In Service";
                    case ATMStatus.ConnectedOnly: return "Connected Only";
                    case ATMStatus.WaitingResponse: return "Waiting Response";
                    case ATMStatus.OutOfService: return "Out of Service";
                    case ATMStatus.CriticalFault: return "Critical Fault";
                    case ATMStatus.Offline: return "Offline";
                    default: return "Unknown";
                }
            }
            public string GetStatusColor()
            {
                if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                if (!IsConnected)
                {
                    var elapsed = DateTime.Now - LastConnectionTime;
                    if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_OFFLINE;
                    if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_WARNING;
                    return ATMStatusColors.COLOR_OFFLINE;
                }
                if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                var idleElapsed = DateTime.Now - LastDataReceived;
                if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                    return ATMStatusColors.COLOR_IDLE;
                return ATMStatusColors.COLOR_ACTIVE;
            }
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                    default: return SyncStrategy.NCR_Overwrite;
                }
            }
            public bool NeedsAlert()
            {
                if (!IsConnected) return true;
                return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
            }
            public string GetElapsed(DateTime? dt) => !dt.HasValue ? "—" : $"{(DateTime.UtcNow - dt.Value).TotalMinutes:N0}m";
            public string GetStatusDescription() => Status.ToString();
            public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
            public bool NeedsAlert() => HasAlerts;
            public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath) ? _sourcePath
                : ATM_Type == "NCR" ? @"C:\NCRJournal\"
                : ATM_Type == "GRG" ? @"D:\GRGData\EJ\"
                : ATM_Type == "WN"  ? @"C:\WOSA\EJ\"
                : @"C:\Journal\";
            public void SetSourcePath(string v) => _sourcePath = v;
            public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                : System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            public void SetBackupPath(string v) => _backupPath = v;
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeat == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeat).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5) return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing) return ATMCardState.Syncing;
                if (IsSupervisorMode) return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
            public Color GetCardColor()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive => Color.FromArgb(52, 199, 89),
                    ATMCardState.ConnectedIdle => Color.FromArgb(255, 214, 10),
                    ATMCardState.Syncing => Color.FromArgb(10, 132, 255),
                    ATMCardState.WaitingReply => Color.FromArgb(10, 132, 255),
                    ATMCardState.Supervisor => Color.FromArgb(255, 159, 10),
                    ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69, 58),
                    ATMCardState.WarningOffline => Color.FromArgb(255, 69, 58),
                    ATMCardState.CriticalOffline => Color.FromArgb(99, 99, 102),
                    ATMCardState.NeverConnected => Color.FromArgb(72, 72, 74),
                    _ => Color.Gray
                };
            }
            public string GetStatusLabel()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive => "● متصل ونشط",
                    ATMCardState.ConnectedIdle => "● متصل خامل",
                    ATMCardState.Syncing => "⟳ يزامن",
                    ATMCardState.WaitingReply => "◎ ينتظر رد",
                    ATMCardState.Supervisor => "★ Supervisor",
                    ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                    ATMCardState.WarningOffline => "✕ انقطاع >5د",
                    ATMCardState.CriticalOffline => "✕ انقطاع حرج",
                    ATMCardState.NeverConnected => "○ لم يتصل",
                    _ => "?"
                };
            }
            public void RecalculateHealthScore()
            {
                var score = 100;
                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                if (Latency > 500) score -= 15;
                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                if (CpuUsagePercent > 90) score -= 10;
                if (MemoryUsagePercent > 90) score -= 10;
                if (DiskUsagePercent > 95) score -= 10;
                HealthScore = Math.Max(0, Math.Min(100, score));
            }
            public override string ToString() =>
                $"[{ATMId}] {Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
            public string GetLegacySourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_SOURCE;
                    default:                        return string.Empty;
                }
            }
            public string GetLegacyBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_BACKUP;
                    default:                        return string.Empty;
                }
            }
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5)  return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing)     return ATMCardState.Syncing;
                if (IsSupervisorMode)                                 return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
            public string GetStatusDescription()        => GetStatusLabel();
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
            public void RecalculateHealthScore()
            {
                var score = 100;
                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                if (Latency_ms > 500) score -= 15;
                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                if (CpuUsagePercent    > 90) score -= 10;
                if (MemoryUsagePercent > 90) score -= 10;
                if (DiskUsagePercent   > 95) score -= 10;
                HealthScore = Math.Max(0, Math.Min(100, score));
            }
            public override string ToString() =>
                $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s/60)}د";
                return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
            }
            public class ATMInfo
            {
                // هوية
                public string ATM_ID        { get; set; }
                public string ATM_Name      { get; set; }
                public string ATM_Type      { get; set; }    // NCR / GRG / WN / DIEBOLD / HYOSUNG
                public string BranchName    { get; set; }
                public string Region        { get; set; }
                public string ATMId { get => ATM_ID; set => ATM_ID = value; }
                public string ATMName { get => ATM_Name; set => ATM_Name = value; }
                public string IPAddress { get => ServerIP; set => ServerIP = value; }
                public ATMType ATMType
                {
                    get
                    {
                        switch (AppConstants.NormalizeATMType(ATM_Type))
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                            case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                            case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
                            case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
                            case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
                            default:                        return ATMType.Other;
                        }
                    }
                    set
                    {
                        ATM_Type = value switch
                        {
                            ATMType.NCR            => AppConstants.ATM_TYPE_NCR,
                            ATMType.GRG            => AppConstants.ATM_TYPE_GRG,
                            ATMType.WN             => AppConstants.ATM_TYPE_WN,
                            ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                            ATMType.Hyosung        => AppConstants.ATM_TYPE_HY,
                            _                      => "OTHER"
                        };
                    }
                }
                public string Location { get => Region; set => Region = value; }
                public string BranchCode { get; set; }
                // شبكة
                public string ServerIP      { get; set; }
                public int    ServerPort    { get; set; } = 5656;
                public string NetworkType   { get; set; } = "LAN";
                public int    Latency_ms    { get; set; }
                public int Latency { get => Latency_ms; set => Latency_ms = value; }
                // حالة الاتصال
                public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
                public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
                public string           SessionId        { get; set; }
                public bool             IsSupervisorMode { get; set; }
                public bool             IsHostConnected  { get; set; }
                // طوابع زمنية UTC (T-11)
                public DateTime ConnectedAtUtc       { get; set; }
                public DateTime DisconnectedAtUtc    { get; set; }
                public DateTime LastHeartbeatUtc     { get; set; }
                public DateTime LastSyncUtc          { get; set; }
                public DateTime LastDataReceivedUtc  { get; set; }
                public DateTime LastCommandSentUtc   { get; set; }
                public DateTime LastConnectionTime { get => ConnectedAtUtc; set => ConnectedAtUtc = value; }
                public DateTime LastSyncTime { get => LastSyncUtc; set => LastSyncUtc = value; }
                // إحصاءات المزامنة
                public long   TotalSyncedBytes         { get; set; }
                public long   TotalTransactions        { get; set; }
                public int    ConsecutiveSyncFailures  { get; set; }
                public double SyncSuccessRate          { get; set; } = 100.0;
                public double ReceiveSpeedKBs          { get; set; }
                public long   JournalSizeToday         { get; set; }
                public double CpuUsagePercent          { get; set; }
                public double MemoryUsagePercent       { get; set; }
                public double DiskUsagePercent         { get; set; }
                public int    HealthScore              { get; set; } = 100;
                public int PendingJournalCount { get; set; }
                public double SuccessRate { get => SyncSuccessRate; set => SyncSuccessRate = value; }
                public long TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
                // إحصاءات العمليات
                public int  ApprovedTransactions  { get; set; }
                public int  FailedTransactions    { get; set; }
                public int  CardsCaptured         { get; set; }
                public long CashDispensed         { get; set; }
                // آخر جورنال / خطأ
                public string LastJournalFile   { get; set; }
                public string LastErrorCode     { get; set; }
                public string LastErrorMessage  { get; set; }
                public string LastTransaction   { get; set; }
                public string LastError { get => LastErrorMessage; set => LastErrorMessage = value; }
                public int TransactionCount { get => (int)Math.Min(int.MaxValue, TotalTransactions); set => TotalTransactions = value; }
                public bool IsSyncing { get => ConnectionStatus == ConnectionStatus.Syncing; set { if (value) ConnectionStatus = ConnectionStatus.Syncing; } }
                // مسارات
                private string _sourcePath, _backupPath;
                public string ClientVersion { get; set; }
                public string OSVersion     { get; set; }
                public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath)
                    ? _sourcePath
                    : AppConstants.GetDefaultSourcePath(ATM_Type);
                public void SetSourcePath(string v) => _sourcePath = v;
                public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                    : System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                        "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
                public void SetBackupPath(string v) => _backupPath = v;
                // ==========================================
                // حالة البطاقة ولونها
                // ==========================================
                public ATMCardState GetCardState()
                {
                    if (ConnectionStatus == ConnectionStatus.Disconnected)
                    {
                        if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                        var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                        if (mins > 10) return ATMCardState.CriticalOffline;
                        if (mins > 5)  return ATMCardState.WarningOffline;
                        return ATMCardState.RecentlyDisconnected;
                    }
                    if (ConnectionStatus == ConnectionStatus.Syncing)     return ATMCardState.Syncing;
                    if (IsSupervisorMode)                                 return ATMCardState.Supervisor;
                    if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                    var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                    return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                }
                public Color GetCardColor()
                {
                    return GetCardState() switch
                    {
                        ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),   // أخضر
                        ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),   // أصفر
                        ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),  // أزرق
                        ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),  // أزرق
                        ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),   // برتقالي
                        ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),   // أحمر
                        ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),   // أحمر
                        ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),  // رمادي
                        ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),   // رمادي داكن
                        _ => Color.Gray
                    };
                }
                public string GetStatusLabel()
                {
                    return GetCardState() switch
                    {
                        ATMCardState.ConnectedActive      => "● متصل ونشط",
                        ATMCardState.ConnectedIdle        => "● متصل خامل",
                        ATMCardState.Syncing              => "⟳ يزامن",
                        ATMCardState.WaitingReply         => "◎ ينتظر رد",
                        ATMCardState.Supervisor           => "★ Supervisor",
                        ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                        ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                        ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                        ATMCardState.NeverConnected       => "○ لم يتصل",
                        _ => "?"
                    };
                }
                public string GetStatusDescription() => GetStatusLabel();
                public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
                public string GetElapsed(DateTime utcRef)
                {
                    if (utcRef == DateTime.MinValue) return "---";
                    var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                    if (s < 60)   return $"{(int)s}ث";
                    if (s < 3600) return $"{(int)(s/60)}د";
                    return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
                }
                public void RecalculateHealthScore()
                {
                    var score = 100;
                    if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                    if (Latency_ms > 500) score -= 15;
                    if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                    if (CpuUsagePercent > 90) score -= 10;
                    if (MemoryUsagePercent > 90) score -= 10;
                    if (DiskUsagePercent > 95) score -= 10;
                    HealthScore = Math.Max(0, Math.Min(100, score));
                }
                public override string ToString() =>
                    $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
            }
            public class AlertPayload
            {
                public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
                public AlertSeverity Severity  { get; set; }
                public string        Title     { get; set; }
                public string        Message   { get; set; }
                public string        Source    { get; set; }
                public string        DedupeKey { get; set; }
                public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
                public bool          IsRead    { get; set; }
                public string Icon => Severity switch
                {
                    AlertSeverity.Emergency => "CRIT",
                    AlertSeverity.Critical  => "FAIL",
                    AlertSeverity.Warning   => "WARN",
                    _                       => "INFO"
                };
                public string SeverityIcon => Icon;
                public Color Color => Severity switch
                {
                    AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                    AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                    AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                    _                       => Color.FromArgb(0,   122, 255)
                };
            }
            public class JournalSyncRecord
            {
                public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
                public string           ATM_ID          { get; set; }
                public string           FileName        { get; set; }
                public long             FileSize        { get; set; }
                public long             FileOffset      { get; set; }
                public string           Checksum        { get; set; }
                public string           MD5Hash         { get; set; }
                public string           SHA256Hash      { get; set; }
                public JournalSyncState State           { get; set; }
                public int              ProgressPercent { get; set; }
                public int              RetryCount      { get; set; }
                public string           LocalPath       { get; set; }
                public string           ServerPath      { get; set; }
                public string           Message         { get; set; }
                public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
                public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
                public DateTime?        CompletedAtUtc  { get; set; }
                public string StateIcon => State switch
                {
                    JournalSyncState.Pending   => "PEND",
                    JournalSyncState.Syncing   => "SYNC",
                    JournalSyncState.ReSyncing => "RSYNC",
                    JournalSyncState.Completed => "OK",
                    JournalSyncState.Failed    => "FAIL",
                    JournalSyncState.Archived  => "ARCH",
                    _ => "?"
                };
                public string StateLabel => State switch
                {
                    JournalSyncState.Pending   => "في الطابور",
                    JournalSyncState.Syncing   => "قيد المزامنة",
                    JournalSyncState.ReSyncing => "إعادة مزامنة",
                    JournalSyncState.Completed => "محمّل",
                    JournalSyncState.Failed    => "فشل",
                    JournalSyncState.Archived  => "مؤرشف",
                    _ => "؟"
                };
                public string ATM_ID { get; set; }
                public string ATM_Name { get; set; }
                public string ATM_Description { get; set; }
                public string ATM_Type { get; set; }
                public string ServerIP { get; set; }
                public int ServerPort { get; set; }
                public ATMStatus Status { get; set; }
                public bool IsConnected { get; set; }
                public bool IsSendingData { get; set; }
                public bool IsCSCConnected { get; set; }
                public DateTime LastConnectionTime { get; set; }
                public DateTime LastDataReceived { get; set; }
                public DateTime LastHeartbeat { get; set; }
                public int[,] OperationStats { get; set; }
                public int ATMCache { get; set; }
                public int TotalDispensed { get; set; }
                public SyncStatus SyncState { get; set; }
                public long TotalBytesSent { get; set; }
                public int TotalFilesSynced { get; set; }
                public int TotalLinesSent { get; set; }
                public DateTime LastSyncTime { get; set; }
                public string LastSyncFile { get; set; }
                public string OSVersion { get; set; }
                public string ClientVersion { get; set; }
                public ATMInfo()
                {
                    OperationStats = new int[3, 4];
                    Status = ATMStatus.Unknown;
                    SyncState = SyncStatus.Idle;
                    ServerPort = NetworkConfig.DEFAULT_PORT;
                }
                public string GetStatusColor()
                {
                    if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                    if (!IsConnected)
                    {
                        var elapsed = DateTime.Now - LastConnectionTime;
                        if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                            return ATMStatusColors.COLOR_OFFLINE;
                        if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                            return ATMStatusColors.COLOR_WARNING;
                        return ATMStatusColors.COLOR_OFFLINE;
                    }
                    if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                    var idleElapsed = DateTime.Now - LastDataReceived;
                    if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                        return ATMStatusColors.COLOR_IDLE;
                    return ATMStatusColors.COLOR_ACTIVE;
                }
                public string GetSourcePath()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                        case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                        case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                        default: return string.Empty;
                    }
                }
                public string GetBackupPath()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                        case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                        case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                        default: return string.Empty;
                    }
                }
                public SyncStrategy GetSyncStrategy()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                        case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                        case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                        default: return SyncStrategy.NCR_Overwrite;
                    }
                }
                public bool NeedsAlert()
                {
                    if (!IsConnected) return true;
                    return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                }
            }
            public class ClientConfig
            {
                public string ATM_ID { get; set; }
                public string ATM_Name { get; set; }
                public string ATM_Type { get; set; }
                public string ServerIP { get; set; }
                public int ServerPort { get; set; }
                public bool SyncTimeEnabled { get; set; }
                public int MessageSizeLines { get; set; }
                public int FilePackageKB { get; set; }
                public string SourcePath { get; set; }
                public string BackupPath { get; set; }
                public bool AutoStart { get; set; }
                public bool RunAsService { get; set; }
                public ClientConfig()
                {
                    ServerPort = NetworkConfig.DEFAULT_PORT;
                    MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                    FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                    AutoStart = true;
                    RunAsService = true;
                }
            }
            public class ServerConfig
            {
                public int ListenPort { get; set; }
                public string StoragePath { get; set; }
                public string ArchivePath { get; set; }
                public bool AutoArchive { get; set; }
                public int MaxConnections { get; set; }
                public bool EnableEncryption { get; set; }
                public bool EnableCompression { get; set; }
                public ServerConfig()
                {
                    ListenPort = NetworkConfig.DEFAULT_PORT;
                    StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                    ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                    AutoArchive = true;
                    MaxConnections = 100;
                    EnableEncryption = true;
                    EnableCompression = true;
                }
            }
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
            public enum ATMStatus
            {
                Unknown = 0,
                Online = 1,
                Idle = 2,
                Supervisor = 3,
                Warning = 4,
                Offline = 5,
                Critical = 6,
                InService = 10,
                ConnectedOnly = 11,
                WaitingResponse = 12,
                OutOfService = 13,
                CriticalFault = 14,
                Fault = 15,
                Maintenance = 16
            }
            public enum ATMType { NCR, GRG, WN, DieboldNixdorf, Hyosung, Other }
            public enum SyncStatus { Pending, InProgress, Syncing, Resyncing, Completed, Failed }
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
            public enum ATMCardState      { NeverConnected, ConnectedActive, ConnectedIdle, Syncing, WaitingReply, Supervisor, RecentlyDisconnected, WarningOffline, CriticalOffline }
            public enum ConnectionStatus
            {
                Disconnected = 0, Connecting = 1, Connected = 2, WaitingReply = 3, Syncing = 4
            }
            public enum AlertSeverity
            {
                Info = 0, Warning = 1, Critical = 2, Emergency = 3
            }
            public enum JournalSyncState
            {
                Pending = 0, Syncing = 1, ReSyncing = 2, Completed = 3, Failed = 4, Archived = 5
            }
        }
    public partial class ClientConfig
        {
            public string ATM_ID { get; set; }
    
    
            public string ATM_Name { get; set; }
    
    
            public string ATM_Type { get; set; }
    
    
            public string ServerIP { get; set; }
    
    
            public int ServerPort { get; set; }
    
    
            public bool SyncTimeEnabled { get; set; }
    
    
            public int MessageSizeLines { get; set; }
    
    
            public int FilePackageKB { get; set; }
    
    
            public string SourcePath { get; set; }
    
    
            public string BackupPath { get; set; }
    
    
            public bool AutoStart { get; set; }
    
    
            public bool RunAsService { get; set; }
    
    
            public string NetworkQuality { get; set; }
    
    
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
                NetworkQuality = "LAN";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\ATMInfo.cs
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
                NetworkQuality = "LAN";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public ClientConfig()
            {
                ServerPort = AppConstants.DefaultPort;
                MessageSizeLines = 50;
                FilePackageKB = 512;
                AutoStart = true;
                RunAsService = true;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
                NetworkQuality = "LAN";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
                NetworkQuality = "LAN";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\ATMInfo.cs
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
                NetworkQuality = "LAN";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public ClientConfig()
            {
                ServerPort = AppConstants.DefaultPort;
                MessageSizeLines = 50;
                FilePackageKB = 512;
                AutoStart = true;
                RunAsService = true;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\ATMInfo.cs
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
                NetworkQuality = "LAN";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
                NetworkQuality = "LAN";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Models\ATMInfo.cs
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
                NetworkQuality = "LAN";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public ClientConfig()
            {
                ServerPort = AppConstants.DefaultPort;
                MessageSizeLines = 50;
                FilePackageKB = 512;
                AutoStart = true;
                RunAsService = true;
            }
    
    
        }
    public partial class ClientConfig
        {
            public string ATM_ID { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }
            public bool SyncTimeEnabled { get; set; }
            public int MessageSizeLines { get; set; }
            public int FilePackageKB { get; set; }
            public string SourcePath { get; set; }
            public string BackupPath { get; set; }
            public bool AutoStart { get; set; }
            public bool RunAsService { get; set; }
            public string NetworkQuality { get; set; }
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
                NetworkQuality = "LAN";
            }
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
            }
        }
    public partial class JournalSyncRecord
        {
            public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string           ATM_ID          { get; set; }
    
    
            public string           FileName        { get; set; }
    
    
            public long             FileSize        { get; set; }
    
    
            public long             FileOffset      { get; set; }
    
    
            public string           Checksum        { get; set; }
    
    
            public string           MD5Hash         { get; set; }
    
    
            public string           SHA256Hash      { get; set; }
    
    
            public JournalSyncState State           { get; set; }
    
    
            public int              ProgressPercent { get; set; }
    
    
            public int              RetryCount      { get; set; }
    
    
            public string           LocalPath       { get; set; }
    
    
            public string           ServerPath      { get; set; }
    
    
            public string           Message         { get; set; }
    
    
            public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
    
    
            public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
    
    
            public DateTime?        CompletedAtUtc  { get; set; }
    
    
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "⏳",
                JournalSyncState.Syncing   => "🔄",
                JournalSyncState.ReSyncing => "♻️",
                JournalSyncState.Completed => "✅",
                JournalSyncState.Failed    => "❌",
                JournalSyncState.Archived  => "📦",
                _ => "?"
            };
    
    
            public string StateLabel => State switch
            {
                JournalSyncState.Pending   => "في الطابور",
                JournalSyncState.Syncing   => "قيد المزامنة",
                JournalSyncState.ReSyncing => "إعادة مزامنة",
                JournalSyncState.Completed => "محمّل",
                JournalSyncState.Failed    => "فشل",
                JournalSyncState.Archived  => "مؤرشف",
                _ => "؟"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
    
    
        }
    public partial class JournalSyncRecord
        {
            public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
            public string           ATM_ID          { get; set; }
            public string           FileName        { get; set; }
            public long             FileSize        { get; set; }
            public long             FileOffset      { get; set; }
            public string           Checksum        { get; set; }
            public string           MD5Hash         { get; set; }
            public string           SHA256Hash      { get; set; }
            public JournalSyncState State           { get; set; }
            public int              ProgressPercent { get; set; }
            public int              RetryCount      { get; set; }
            public string           LocalPath       { get; set; }
            public string           ServerPath      { get; set; }
            public string           Message         { get; set; }
            public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
            public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
            public DateTime?        CompletedAtUtc  { get; set; }
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
            public string StateLabel => State switch
            {
                JournalSyncState.Pending   => "في الطابور",
                JournalSyncState.Syncing   => "قيد المزامنة",
                JournalSyncState.ReSyncing => "إعادة مزامنة",
                JournalSyncState.Completed => "محمّل",
                JournalSyncState.Failed    => "فشل",
                JournalSyncState.Archived  => "مؤرشف",
                _                          => "؟"
            };
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "⏳",
                JournalSyncState.Syncing   => "🔄",
                JournalSyncState.ReSyncing => "♻️",
                JournalSyncState.Completed => "✅",
                JournalSyncState.Failed    => "❌",
                JournalSyncState.Archived  => "📦",
                _ => "?"
            };
        }
    public partial class ServerConfig
        {
            public int ListenPort { get; set; }
    
    
            public string StoragePath { get; set; }
    
    
            public string ArchivePath { get; set; }
    
    
            public bool AutoArchive { get; set; }
    
    
            public int MaxConnections { get; set; }
    
    
            public bool EnableEncryption { get; set; }
    
    
            public bool EnableCompression { get; set; }
    
    
            public ServerConfig()
            {
                ListenPort = NetworkConfig.DEFAULT_PORT;
                StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                AutoArchive = true;
                MaxConnections = 100;
                EnableEncryption = true;
                EnableCompression = true;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            public ServerConfig()
            {
                ListenPort = AppConstants.DefaultPort;
                StoragePath = @"C:\EJLive\Storage";
                ArchivePath = @"C:\EJLive\Archive";
                AutoArchive = true;
                MaxConnections = 100;
                EnableEncryption = true;
                EnableCompression = true;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            public ServerConfig()
            {
                ListenPort = AppConstants.DefaultPort;
                StoragePath = @"C:\EJLive\Storage";
                ArchivePath = @"C:\EJLive\Archive";
                AutoArchive = true;
                MaxConnections = 100;
                EnableEncryption = true;
                EnableCompression = true;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            public ServerConfig()
            {
                ListenPort = AppConstants.DefaultPort;
                StoragePath = @"C:\EJLive\Storage";
                ArchivePath = @"C:\EJLive\Archive";
                AutoArchive = true;
                MaxConnections = 100;
                EnableEncryption = true;
                EnableCompression = true;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            public ServerConfig()
            {
                ListenPort = AppConstants.DefaultPort;
                StoragePath = @"C:\EJLive\Storage";
                ArchivePath = @"C:\EJLive\Archive";
                AutoArchive = true;
                MaxConnections = 100;
                EnableEncryption = true;
                EnableCompression = true;
            }
    
    
        }
    public partial class ServerConfig
        {
            public int ListenPort { get; set; }
            public string StoragePath { get; set; }
            public string ArchivePath { get; set; }
            public bool AutoArchive { get; set; }
            public int MaxConnections { get; set; }
            public bool EnableEncryption { get; set; }
            public bool EnableCompression { get; set; }
            public ServerConfig()
            {
                ListenPort = NetworkConfig.DEFAULT_PORT;
                StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                AutoArchive = true;
                MaxConnections = 100;
                EnableEncryption = true;
                EnableCompression = true;
            }
            public ServerConfig()
            {
                ListenPort = AppConstants.DefaultPort;
                StoragePath = @"C:\EJLive\Storage";
                ArchivePath = @"C:\EJLive\Archive";
                AutoArchive = true;
                MaxConnections = 100;
                EnableEncryption = true;
                EnableCompression = true;
            }
        }

    // Class: ATMInfo (from 4 sources)
        public partial class ATMInfo
        {
            // --- Properties ---
                            public string ATMID { get; set; }
    
                            public string BranchName { get; set; }
    
                            public string IPAddress { get; set; }
    
                            public ATMType Type { get; set; }
    
                            public ATMStatus Status { get; set; }
    
                            public DateTime LastSync { get; set; }
    
                            public DateTime LastHeartbeat { get; set; }
    
                            public long TotalBytesSynced { get; set; }
    
                            public int FilesProcessed { get; set; }
    
                            public bool IsOnline { get; set; }
    
                            public string LastError { get; set; }
    
                            public string OSVersion { get; set; }
    
                            public string SoftwareVersion { get; set; }
    
                            public string ATMName { get; set; }
    
                            public string Location { get; set; }
    
                            public DateTime LastSyncTime { get; set; }
    
                            public decimal RemainingCash { get; set; }
    
                            public decimal DispensedCash { get; set; }
    
                            public string LastTransaction { get; set; }
    
                            public string NetworkType { get; set; } // LAN, CDMA, GSM, ADSL
    
                            public int CapturedCardsCount { get; set; }
    
                            public string LastFaultCode { get; set; }
    
                            public string LastFaultDescription { get; set; }
    
                            public long EJTodayLines { get; set; }
    
                            public bool IsSupervisorMode { get; set; }
    
                    public string LastJournalLine { get; set; }
    
    
            // --- Constructors ---
                            public ATMInfo()
                            {
                                Status = ATMStatus.Offline;
                                LastSync = DateTime.MinValue;
                                LastHeartbeat = DateTime.MinValue;
                                IsOnline = false;
                                LastError = string.Empty;
                            }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\ATMInfo.cs
                            public ATMInfo()
                            {
                                Status = ATMStatus.Offline;
                                LastSyncTime = DateTime.MinValue;
                                RemainingCash = 0;
                                DispensedCash = 0;
                            }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_WinForms_v3.2.1\EJLive.Core\ATMInfo.cs
                    public ATMInfo()
                    {
                        Status = ATMStatus.Offline;
                        LastSyncTime = DateTime.MinValue;
                        RemainingCash = 0;
                        DispensedCash = 0;
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_System\EJLive.Core\ATMInfo.cs
                    public ATMInfo()
                    {
                        Status = ATMStatus.Offline;
                        LastSyncTime = DateTime.MinValue;
                    }
    
    
            // --- Methods ---
                            public string GetStatusText()
                            {
                                switch (Status)
                                {
                                    case ATMStatus.InService: return "In Service";
                                    case ATMStatus.ConnectedOnly: return "Connected Only";
                                    case ATMStatus.WaitingResponse: return "Waiting Response";
                                    case ATMStatus.OutOfService: return "Out of Service";
                                    case ATMStatus.CriticalFault: return "Critical Fault";
                                    case ATMStatus.Offline: return "Offline";
                                    default: return "Unknown";
                                }
                            }
    
    
        }
    // ═══ Class: ATMInfo (from 2 sources) ═══
        public partial class ATMInfo
        {
            // --- Properties ---
                    public string ATMID { get; set; }
    
                    public string BranchName { get; set; }
    
                    public string IPAddress { get; set; }
    
                    public ATMType Type { get; set; }
    
                    public ATMStatus Status { get; set; }
    
                    public DateTime LastSync { get; set; }
    
                    public DateTime LastHeartbeat { get; set; }
    
                    public long TotalBytesSynced { get; set; }
    
                    public int FilesProcessed { get; set; }
    
                    public bool IsOnline { get; set; }
    
                    public string LastError { get; set; }
    
                    public string OSVersion { get; set; }
    
                    public string SoftwareVersion { get; set; }
    
                    public string ATMName { get; set; }
    
                    public string Location { get; set; }
    
                    public DateTime LastSyncTime { get; set; }
    
                    public decimal RemainingCash { get; set; }
    
                    public decimal DispensedCash { get; set; }
    
                    public string LastTransaction { get; set; }
    
                    public string NetworkType { get; set; } // LAN, CDMA, GSM, ADSL
    
                    public int CapturedCardsCount { get; set; }
    
                    public string LastFaultCode { get; set; }
    
                    public string LastFaultDescription { get; set; }
    
                    public long EJTodayLines { get; set; }
    
                    public bool IsSupervisorMode { get; set; }
    
    
            // --- Constructors ---
                    public ATMInfo()
                    {
                        Status = ATMStatus.Offline;
                        LastSync = DateTime.MinValue;
                        LastHeartbeat = DateTime.MinValue;
                        IsOnline = false;
                        LastError = string.Empty;
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_WinForms_v3.2.1\EJLive.Core\ATMInfo.cs
                    public ATMInfo()
                    {
                        Status = ATMStatus.Offline;
                        LastSyncTime = DateTime.MinValue;
                        RemainingCash = 0;
                        DispensedCash = 0;
                    }
    
    
            // --- Methods ---
                    public string GetStatusText()
                    {
                        switch (Status)
                        {
                            case ATMStatus.InService: return "In Service";
                            case ATMStatus.ConnectedOnly: return "Connected Only";
                            case ATMStatus.WaitingResponse: return "Waiting Response";
                            case ATMStatus.OutOfService: return "Out of Service";
                            case ATMStatus.CriticalFault: return "Critical Fault";
                            case ATMStatus.Offline: return "Offline";
                            default: return "Unknown";
                        }
                    }
    
    
        }

    public partial enum AlertSeverity
        {
            Info      = 0,
    
    
            Warning   = 1,
    
    
            Critical  = 2,
    
    
            Emergency = 3
    
    
        }
    public partial enum ATMCardState
        {
            NeverConnected,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            CriticalOffline
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            CriticalOffline
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
        }
    public partial enum ATMCardState
        {
            NeverConnected,
            CriticalOffline
        }
    public partial enum ATMStatus
        {
            Unknown          = 0,
    
    
            Online           = 1,
    
    
            Idle             = 2,
    
    
            Supervisor       = 3,
    
    
            Warning          = 4,
    
    
            Offline          = 5,
    
    
            Critical         = 6,
    
    
            InService        = 10,
    
    
            ConnectedOnly    = 11,
    
    
            WaitingResponse  = 12,
    
    
            OutOfService     = 13,
    
    
            CriticalFault    = 14,
    
    
            Fault            = 15,
    
    
            Maintenance      = 16
    
    
        }
    public partial enum ATMType
        {
            NCR,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            Other
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            Other
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
        }
    public partial enum ATMType
        {
            NCR,
            Other
        }
    public partial enum ConnectionStatus
        {
            Disconnected  = 0,
    
    
            Connecting    = 1,
    
    
            Connected     = 2,
    
    
            WaitingReply  = 3,
    
    
            Syncing       = 4
    
    
        }
    public partial enum JournalSyncState
        {
        }
    public partial enum SyncStatus
        {
            Pending,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
            Idle = Pending
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMInfo.cs
            Failed,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            Failed,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
        }
    public partial enum SyncStatus
        {
            Pending,
            Failed,
            Idle = Pending
        }
    public partial enum SyncStrategy
        {
        }
}

namespace EJLive.Core.Models
{
    public partial class AlertPayload
        {
            public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public AlertSeverity Severity { get; set; }
    
    
            public string Title { get; set; }
    
    
            public string Message { get; set; }
    
    
            public string Source { get; set; }
    
    
            public string DedupeKey { get; set; }
    
    
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    
            public bool IsRead { get; set; }
    
    
            public string Icon => Severity switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical => "FAIL",
                AlertSeverity.Warning => "WARN",
                _ => "INFO"
            };
    
    
            public Color Color => Severity switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59, 48),
                AlertSeverity.Critical => Color.FromArgb(255, 59, 48),
                AlertSeverity.Warning => Color.FromArgb(255, 149, 0),
                _ => Color.FromArgb(0, 122, 255)
            };
    
    
            public string        ATM_ID    { get; set; }
    
    
            public AlertSeverity SeverityLevel { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public string        Severity  { get; set; } = "Info";
    
    
            public DateTime      CreatedAtUtc { get => CreatedAt; set => CreatedAt = value; }
    
    
            public DateTime      RaisedAt  { get => CreatedAt; set => CreatedAt = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            public string SeverityIcon => Icon;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string        Severity  { get; set; } = "Info";
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
        }
    public partial class AlertPayload
        {
            public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public AlertSeverity Severity  { get; set; }
    
    
            public string        Title     { get; set; }
    
    
            public string        Message   { get; set; }
    
    
            public string        Source    { get; set; }
    
    
            public string        DedupeKey { get; set; }
    
    
            public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
    
    
            public bool          IsRead    { get; set; }
    
    
            public string Icon => Severity switch
            {
                AlertSeverity.Emergency => "🚨",
                AlertSeverity.Critical  => "❌",
                AlertSeverity.Warning   => "⚠️",
                _                       => "ℹ️"
            };
    
    
            public string SeverityIcon => Icon;
    
    
            public Color Color => Severity switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
        }
    public partial class AlertPayload
        {
            public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string        ATM_ID    { get; set; }
    
    
            public AlertSeverity SeverityLevel { get; set; }
    
    
            public string        Severity  { get; set; } = "Info";
    
    
            public string        Title     { get; set; }
    
    
            public string        Message   { get; set; }
    
    
            public string        Source    { get; set; }
    
    
            public string        DedupeKey { get; set; }
    
    
            public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
    
    
            public DateTime      CreatedAtUtc { get => CreatedAt; set => CreatedAt = value; }
    
    
            public DateTime      RaisedAt  { get => CreatedAt; set => CreatedAt = value; }
    
    
            public bool          IsRead    { get; set; }
    
    
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
    
            public string SeverityIcon => Icon;
    
    
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
    
    
        }
    // ==========================================
        // التنبيهات
        // ==========================================
    
        public class AlertPayload
        {
            public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
            public AlertSeverity Severity { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public string Source { get; set; }
            public string DedupeKey { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public bool IsRead { get; set; }
    
            public string Icon => Severity switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical => "FAIL",
                AlertSeverity.Warning => "WARN",
                _ => "INFO"
            };
    
            public Color Color => Severity switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59, 48),
                AlertSeverity.Critical => Color.FromArgb(255, 59, 48),
                AlertSeverity.Warning => Color.FromArgb(255, 149, 0),
                _ => Color.FromArgb(0, 122, 255)
            };
        }
    // ==========================================
        // التنبيهات
        // ==========================================
    
        public class AlertPayload
        {
            public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
            public AlertSeverity Severity  { get; set; }
            public string        Title     { get; set; }
            public string        Message   { get; set; }
            public string        Source    { get; set; }
            public string        DedupeKey { get; set; }
            public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
            public bool          IsRead    { get; set; }
    
            public string Icon => Severity switch
            {
                AlertSeverity.Emergency => "🚨",
                AlertSeverity.Critical  => "❌",
                AlertSeverity.Warning   => "⚠️",
                _                       => "ℹ️"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
        }
    public class ATMInfo
        {
    
            // ==========================================
            // Enumerations
            // ==========================================
    
            public enum ConnectionStatus
            {
                Disconnected = 0, Connecting = 1, Connected = 2, WaitingReply = 3, Syncing = 4
            }
            public enum ATMStatus
            {
                Unknown = 0,
                Online = 1,
                Idle = 2,
                Supervisor = 3,
                Warning = 4,
                Offline = 5,
                Critical = 6,
                InService = 10,
                ConnectedOnly = 11,
                WaitingResponse = 12,
                OutOfService = 13,
                CriticalFault = 14,
                Fault = 15,
                Maintenance = 16
            }
            public enum ATMType
            {
                NCR,
                GRG,
                WN,
                DieboldNixdorf,
                Hyosung,
                Other
            }
            public enum SyncStatus
            {
                Pending,
                InProgress,
                Syncing,
                Resyncing,
                Completed,
                Failed
            }
            public enum AlertSeverity
            {
                Info = 0, Warning = 1, Critical = 2, Emergency = 3
            }
            public enum JournalSyncState
            {
                Pending = 0, Syncing = 1, ReSyncing = 2, Completed = 3, Failed = 4, Archived = 5
            }
            public enum ATMCardState
            {
                NeverConnected,
                ConnectedActive,
                ConnectedIdle,
                Syncing,
                WaitingReply,
                Supervisor,
                RecentlyDisconnected,
                WarningOffline,
                CriticalOffline
            }
    
            // ==========================================
            // ATMInfo — نموذج الصراف الشامل
            // ==========================================
    
            public class ATMInfo
            {
                // هوية
                public string ATM_ID
                {
                    get;
                    set;
                }
                public string ATM_Name
                {
                    get;
                    set;
                }
                public string ATM_Type
                {
                    get;
                    set;
                } // NCR / GRG / WN / DIEBOLD / HYOSUNG
                public string BranchName
                {
                    get;
                    set;
                }
                public string Region
                {
                    get;
                    set;
                }
                public string ATMId
                {
                    get => ATM_ID;
                    set => ATM_ID = value;
                }
                public string ATMName
                {
                    get => ATM_Name;
                    set => ATM_Name = value;
                }
                public string IPAddress
                {
                    get => ServerIP;
                    set => ServerIP = value;
                }
                public ATMType ATMType
                {
                    get
                    {
                        switch (AppConstants.NormalizeATMType(ATM_Type))
                        {
                            case AppConstants.ATM_TYPE_NCR:
                                return ATMType.NCR;
                            case AppConstants.ATM_TYPE_GRG:
                                return ATMType.GRG;
                            case AppConstants.ATM_TYPE_WN:
                                return ATMType.WN;
                            case AppConstants.ATM_TYPE_DN:
                                return ATMType.DieboldNixdorf;
                            case AppConstants.ATM_TYPE_HY:
                                return ATMType.Hyosung;
                            default:
                                return ATMType.Other;
                        }
                    }
                    set
                    {
                        ATM_Type = value
                        switch
                        {
                            ATMType.NCR => AppConstants.ATM_TYPE_NCR,
                            ATMType.GRG => AppConstants.ATM_TYPE_GRG,
                            ATMType.WN => AppConstants.ATM_TYPE_WN,
                            ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                            ATMType.Hyosung => AppConstants.ATM_TYPE_HY,
                            _ => "OTHER"
                        };
                    }
                }
                public string Location
                {
                    get => Region;
                    set => Region = value;
                }
                public string BranchCode
                {
                    get;
                    set;
                }
    
                // شبكة
                public string ServerIP
                {
                    get;
                    set;
                }
                public int ServerPort
                {
                    get;
                    set;
                } = 5656;
                public string NetworkType
                {
                    get;
                    set;
                } = "LAN";
                public int Latency_ms
                {
                    get;
                    set;
                }
                public int Latency
                {
                    get => Latency_ms;
                    set => Latency_ms = value;
                }
    
                // حالة الاتصال
                public ConnectionStatus ConnectionStatus
                {
                    get;
                    set;
                } = ConnectionStatus.Disconnected;
                public ATMStatus Status
                {
                    get;
                    set;
                } = ATMStatus.Unknown;
                public string SessionId
                {
                    get;
                    set;
                }
                public bool IsSupervisorMode
                {
                    get;
                    set;
                }
                public bool IsHostConnected
                {
                    get;
                    set;
                }
    
                // طوابع زمنية UTC (T-11)
                public DateTime ConnectedAtUtc
                {
                    get;
                    set;
                }
                public DateTime DisconnectedAtUtc
                {
                    get;
                    set;
                }
                public DateTime LastHeartbeatUtc
                {
                    get;
                    set;
                }
                public DateTime LastSyncUtc
                {
                    get;
                    set;
                }
                public DateTime LastDataReceivedUtc
                {
                    get;
                    set;
                }
                public DateTime LastCommandSentUtc
                {
                    get;
                    set;
                }
                public DateTime LastConnectionTime
                {
                    get => ConnectedAtUtc;
                    set => ConnectedAtUtc = value;
                }
                public DateTime LastSyncTime
                {
                    get => LastSyncUtc;
                    set => LastSyncUtc = value;
                }
    
                // إحصاءات المزامنة
                public long TotalSyncedBytes
                {
                    get;
                    set;
                }
                public long TotalTransactions
                {
                    get;
                    set;
                }
                public int ConsecutiveSyncFailures
                {
                    get;
                    set;
                }
                public double SyncSuccessRate
                {
                    get;
                    set;
                } = 100.0;
                public double ReceiveSpeedKBs
                {
                    get;
                    set;
                }
                public long JournalSizeToday
                {
                    get;
                    set;
                }
                public double CpuUsagePercent
                {
                    get;
                    set;
                }
                public double MemoryUsagePercent
                {
                    get;
                    set;
                }
                public double DiskUsagePercent
                {
                    get;
                    set;
                }
                public int HealthScore
                {
                    get;
                    set;
                } = 100;
                public int PendingJournalCount
                {
                    get;
                    set;
                }
                public double SuccessRate
                {
                    get => SyncSuccessRate;
                    set => SyncSuccessRate = value;
                }
                public long TotalTransactionsSynced
                {
                    get => TotalTransactions;
                    set => TotalTransactions = value;
                }
    
                // إحصاءات العمليات
                public int ApprovedTransactions
                {
                    get;
                    set;
                }
                public int FailedTransactions
                {
                    get;
                    set;
                }
                public int CardsCaptured
                {
                    get;
                    set;
                }
                public long CashDispensed
                {
                    get;
                    set;
                }
    
                // آخر جورنال / خطأ
                public string LastJournalFile
                {
                    get;
                    set;
                }
                public string LastErrorCode
                {
                    get;
                    set;
                }
                public string LastErrorMessage
                {
                    get;
                    set;
                }
                public string LastTransaction
                {
                    get;
                    set;
                }
                public string LastError
                {
                    get => LastErrorMessage;
                    set => LastErrorMessage = value;
                }
                public int TransactionCount
                {
                    get => (int)Math.Min(int.MaxValue, TotalTransactions);
                    set => TotalTransactions = value;
                }
                public bool IsSyncing
                {
                    get => ConnectionStatus == ConnectionStatus.Syncing;
                    set
                    {
                        if (value) ConnectionStatus = ConnectionStatus.Syncing;
                    }
                }
    
                // مسارات
                private string _sourcePath, _backupPath;
                public string ClientVersion
                {
                    get;
                    set;
                }
                public string OSVersion
                {
                    get;
                    set;
                }
    
                public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath) ?
                    _sourcePath :
                    AppConstants.GetDefaultSourcePath(ATM_Type);
    
                public void SetSourcePath(string v) => _sourcePath = v;
    
                public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath :
                    System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
    
                public void SetBackupPath(string v) => _backupPath = v;
    
                // ==========================================
                // حالة البطاقة ولونها
                // ==========================================
    
                public ATMCardState GetCardState()
                {
                    if (ConnectionStatus == ConnectionStatus.Disconnected)
                    {
                        if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                        var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                        if (mins > 10) return ATMCardState.CriticalOffline;
                        if (mins > 5) return ATMCardState.WarningOffline;
                        return ATMCardState.RecentlyDisconnected;
                    }
                    if (ConnectionStatus == ConnectionStatus.Syncing) return ATMCardState.Syncing;
                    if (IsSupervisorMode) return ATMCardState.Supervisor;
                    if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                    var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                    return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                }
    
                public Color GetCardColor()
                {
                    return GetCardState() switch
                    {
                        ATMCardState.ConnectedActive => Color.FromArgb(52, 199, 89), // أخضر
                        ATMCardState.ConnectedIdle => Color.FromArgb(255, 214, 10), // أصفر
                        ATMCardState.Syncing => Color.FromArgb(10, 132, 255), // أزرق
                        ATMCardState.WaitingReply => Color.FromArgb(10, 132, 255), // أزرق
                        ATMCardState.Supervisor => Color.FromArgb(255, 159, 10), // برتقالي
                        ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69, 58), // أحمر
                        ATMCardState.WarningOffline => Color.FromArgb(255, 69, 58), // أحمر
                        ATMCardState.CriticalOffline => Color.FromArgb(99, 99, 102), // رمادي
                        ATMCardState.NeverConnected => Color.FromArgb(72, 72, 74), // رمادي داكن
                        _ => Color.Gray
                    };
                }
    
                public string GetStatusLabel()
                {
                    return GetCardState() switch
                    {
                        ATMCardState.ConnectedActive => "● متصل ونشط",
                        ATMCardState.ConnectedIdle => "● متصل خامل",
                        ATMCardState.Syncing => "⟳ يزامن",
                        ATMCardState.WaitingReply => "◎ ينتظر رد",
                        ATMCardState.Supervisor => "★ Supervisor",
                        ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                        ATMCardState.WarningOffline => "✕ انقطاع >5د",
                        ATMCardState.CriticalOffline => "✕ انقطاع حرج",
                        ATMCardState.NeverConnected => "○ لم يتصل",
                        _ => "?"
                    };
                }
    
                public string GetStatusDescription() => GetStatusLabel();
    
                public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
                public string GetElapsed(DateTime utcRef)
                {
                    if (utcRef == DateTime.MinValue) return "---";
                    var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                    if (s < 60) return $"{(int)s}ث";
                    if (s < 3600) return $"{(int)(s/60)}د";
                    return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
                }
    
                public void RecalculateHealthScore()
                {
                    var score = 100;
                    if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                    if (Latency_ms > 500) score -= 15;
                    if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                    if (CpuUsagePercent > 90) score -= 10;
                    if (MemoryUsagePercent > 90) score -= 10;
                    if (DiskUsagePercent > 95) score -= 10;
                    HealthScore = Math.Max(0, Math.Min(100, score));
                }
    
                public override string ToString() =>
                $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
            }
    
            // ==========================================
            // التنبيهات
            // ==========================================
    
            public class AlertPayload
            {
                public string AlertId
                {
                    get;
                    set;
                } = Guid.NewGuid().ToString("N");
                public AlertSeverity Severity
                {
                    get;
                    set;
                }
                public string Title
                {
                    get;
                    set;
                }
                public string Message
                {
                    get;
                    set;
                }
                public string Source
                {
                    get;
                    set;
                }
                public string DedupeKey
                {
                    get;
                    set;
                }
                public DateTime CreatedAt
                {
                    get;
                    set;
                } = DateTime.UtcNow;
                public bool IsRead
                {
                    get;
                    set;
                }
    
                public string Icon => Severity
                switch
                {
                    AlertSeverity.Emergency => "CRIT",
                    AlertSeverity.Critical => "FAIL",
                    AlertSeverity.Warning => "WARN",
                    _ => "INFO"
                };
                public string SeverityIcon => Icon;
                public Color Color => Severity
                switch
                {
                    AlertSeverity.Emergency => Color.FromArgb(255, 59, 48),
                    AlertSeverity.Critical => Color.FromArgb(255, 59, 48),
                    AlertSeverity.Warning => Color.FromArgb(255, 149, 0),
                    _ => Color.FromArgb(0, 122, 255)
                };
            }
    
            // ==========================================
            // سجل المزامنة
            // ==========================================
    
            public class JournalSyncRecord
            {
                public string SyncId
                {
                    get;
                    set;
                } = Guid.NewGuid().ToString("N");
                public string ATM_ID
                {
                    get;
                    set;
                }
                public string FileName
                {
                    get;
                    set;
                }
                public long FileSize
                {
                    get;
                    set;
                }
                public long FileOffset
                {
                    get;
                    set;
                }
                public string Checksum
                {
                    get;
                    set;
                }
                public string MD5Hash
                {
                    get;
                    set;
                }
                public string SHA256Hash
                {
                    get;
                    set;
                }
                public JournalSyncState State
                {
                    get;
                    set;
                }
                public int ProgressPercent
                {
                    get;
                    set;
                }
                public int RetryCount
                {
                    get;
                    set;
                }
                public string LocalPath
                {
                    get;
                    set;
                }
                public string ServerPath
                {
                    get;
                    set;
                }
                public string Message
                {
                    get;
                    set;
                }
                public DateTime CreatedAtUtc
                {
                    get;
                    set;
                } = DateTime.UtcNow;
                public DateTime UpdatedAtUtc
                {
                    get;
                    set;
                } = DateTime.UtcNow;
                public DateTime? CompletedAtUtc
                {
                    get;
                    set;
                }
    
                public string StateIcon => State
                switch
                {
                    JournalSyncState.Pending => "PEND",
                    JournalSyncState.Syncing => "SYNC",
                    JournalSyncState.ReSyncing => "RSYNC",
                    JournalSyncState.Completed => "OK",
                    JournalSyncState.Failed => "FAIL",
                    JournalSyncState.Archived => "ARCH",
                    _ => "?"
                };
    
                public string StateLabel => State
                switch
                {
                    JournalSyncState.Pending => "في الطابور",
                    JournalSyncState.Syncing => "قيد المزامنة",
                    JournalSyncState.ReSyncing => "إعادة مزامنة",
                    JournalSyncState.Completed => "محمّل",
                    JournalSyncState.Failed => "فشل",
                    JournalSyncState.Archived => "مؤرشف",
                    _ => "؟"
                };
    
                public string ATM_ID
                {
                    get;
                    set;
                }
                public string ATM_Name
                {
                    get;
                    set;
                }
                public string ATM_Description
                {
                    get;
                    set;
                }
                public string ATM_Type
                {
                    get;
                    set;
                }
                public string ServerIP
                {
                    get;
                    set;
                }
                public int ServerPort
                {
                    get;
                    set;
                }
                public ATMStatus Status
                {
                    get;
                    set;
                }
                public bool IsConnected
                {
                    get;
                    set;
                }
                public bool IsSendingData
                {
                    get;
                    set;
                }
                public bool IsCSCConnected
                {
                    get;
                    set;
                }
                public DateTime LastConnectionTime
                {
                    get;
                    set;
                }
                public DateTime LastDataReceived
                {
                    get;
                    set;
                }
                public DateTime LastHeartbeat
                {
                    get;
                    set;
                }
                public int[,] OperationStats
                {
                    get;
                    set;
                }
                public int ATMCache
                {
                    get;
                    set;
                }
                public int TotalDispensed
                {
                    get;
                    set;
                }
                public SyncStatus SyncState
                {
                    get;
                    set;
                }
                public long TotalBytesSent
                {
                    get;
                    set;
                }
                public int TotalFilesSynced
                {
                    get;
                    set;
                }
                public int TotalLinesSent
                {
                    get;
                    set;
                }
                public DateTime LastSyncTime
                {
                    get;
                    set;
                }
                public string LastSyncFile
                {
                    get;
                    set;
                }
                public string OSVersion
                {
                    get;
                    set;
                }
                public string ClientVersion
                {
                    get;
                    set;
                }
    
                public ATMInfo()
                {
                    OperationStats = new int[3, 4];
                    Status = ATMStatus.Unknown;
                    SyncState = SyncStatus.Idle;
                    ServerPort = NetworkConfig.DEFAULT_PORT;
                }
    
                public string GetStatusColor()
                {
                    if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                    if (!IsConnected)
                    {
                        var elapsed = DateTime.Now - LastConnectionTime;
                        if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                            return ATMStatusColors.COLOR_OFFLINE;
                        if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                            return ATMStatusColors.COLOR_WARNING;
                        return ATMStatusColors.COLOR_OFFLINE;
                    }
                    if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                    var idleElapsed = DateTime.Now - LastDataReceived;
                    if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                        return ATMStatusColors.COLOR_IDLE;
                    return ATMStatusColors.COLOR_ACTIVE;
                }
    
                public string GetSourcePath()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR:
                            return ATMPaths.NCR_SOURCE;
                        case AppConstants.ATM_TYPE_GRG:
                            return ATMPaths.GRG_SOURCE;
                        case AppConstants.ATM_TYPE_WN:
                            return ATMPaths.WN_SOURCE;
                        default:
                            return string.Empty;
                    }
                }
    
                public string GetBackupPath()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR:
                            return ATMPaths.NCR_BACKUP;
                        case AppConstants.ATM_TYPE_GRG:
                            return ATMPaths.GRG_BACKUP;
                        case AppConstants.ATM_TYPE_WN:
                            return ATMPaths.WN_BACKUP;
                        default:
                            return string.Empty;
                    }
                }
    
                public SyncStrategy GetSyncStrategy()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR:
                            return SyncStrategy.NCR_Overwrite;
                        case AppConstants.ATM_TYPE_GRG:
                            return SyncStrategy.GRG_DailyFiles;
                        case AppConstants.ATM_TYPE_WN:
                            return SyncStrategy.WN_DailyFiles;
                        default:
                            return SyncStrategy.NCR_Overwrite;
                    }
                }
    
                public bool NeedsAlert()
                {
                    if (!IsConnected) return true;
                    return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                }
            }
    
            public class ClientConfig
            {
                public string ATM_ID
                {
                    get;
                    set;
                }
                public string ATM_Name
                {
                    get;
                    set;
                }
                public string ATM_Type
                {
                    get;
                    set;
                }
                public string ServerIP
                {
                    get;
                    set;
                }
                public int ServerPort
                {
                    get;
                    set;
                }
                public bool SyncTimeEnabled
                {
                    get;
                    set;
                }
                public int MessageSizeLines
                {
                    get;
                    set;
                }
                public int FilePackageKB
                {
                    get;
                    set;
                }
                public string SourcePath
                {
                    get;
                    set;
                }
                public string BackupPath
                {
                    get;
                    set;
                }
                public bool AutoStart
                {
                    get;
                    set;
                }
                public bool RunAsService
                {
                    get;
                    set;
                }
    
                public ClientConfig()
                {
                    ServerPort = NetworkConfig.DEFAULT_PORT;
                    MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                    FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                    AutoStart = true;
                    RunAsService = true;
                }
            }
    
            public class ServerConfig
            {
                public int ListenPort
                {
                    get;
                    set;
                }
                public string StoragePath
                {
                    get;
                    set;
                }
                public string ArchivePath
                {
                    get;
                    set;
                }
                public bool AutoArchive
                {
                    get;
                    set;
                }
                public int MaxConnections
                {
                    get;
                    set;
                }
                public bool EnableEncryption
                {
                    get;
                    set;
                }
                public bool EnableCompression
                {
                    get;
                    set;
                }
    
                public ServerConfig()
                {
                    ListenPort = NetworkConfig.DEFAULT_PORT;
                    StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                    ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                    AutoArchive = true;
                    MaxConnections = 100;
                    EnableEncryption = true;
                    EnableCompression = true;
                }
            }
        }
    public class ATMInfo
        {
    
        // ==========================================
        // Enumerations
        // ==========================================
    
        public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
        public enum ATMStatus
        {
            Unknown = 0,
            Online = 1,
            Idle = 2,
            Supervisor = 3,
            Warning = 4,
            Offline = 5,
            Critical = 6,
            InService = 10,
            ConnectedOnly = 11,
            WaitingResponse = 12,
            OutOfService = 13,
            CriticalFault = 14,
            Fault = 15,
            Maintenance = 16
        }
        public enum ATMType { NCR, GRG, WN, DieboldNixdorf, Hyosung, Other }
        public enum SyncStatus { Pending, InProgress, Syncing, Resyncing, Completed, Failed }
        public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
        public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
        public enum ATMCardState      { NeverConnected, ConnectedActive, ConnectedIdle, Syncing, WaitingReply, Supervisor, RecentlyDisconnected, WarningOffline, CriticalOffline }
    
        // ==========================================
        // ATMInfo — نموذج الصراف الشامل
        // ==========================================
    
        public class ATMInfo
        {
            // هوية
            public string ATM_ID        { get; set; }
            public string ATM_Name      { get; set; }
            public string ATM_Type      { get; set; }    // NCR / GRG / WN / DIEBOLD / HYOSUNG
            public string BranchName    { get; set; }
            public string Region        { get; set; }
            public string ATMId { get => ATM_ID; set => ATM_ID = value; }
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
            public ATMType ATMType
            {
                get
                {
                    switch (AppConstants.NormalizeATMType(ATM_Type))
                    {
                        case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                        case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                        case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
                        case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
                        case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
                        default:                        return ATMType.Other;
                    }
                }
                set
                {
                    ATM_Type = value switch
                    {
                        ATMType.NCR            => AppConstants.ATM_TYPE_NCR,
                        ATMType.GRG            => AppConstants.ATM_TYPE_GRG,
                        ATMType.WN             => AppConstants.ATM_TYPE_WN,
                        ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                        ATMType.Hyosung        => AppConstants.ATM_TYPE_HY,
                        _                      => "OTHER"
                    };
                }
            }
            public string Location { get => Region; set => Region = value; }
            public string BranchCode { get; set; }
    
            // شبكة
            public string ServerIP      { get; set; }
            public int    ServerPort    { get; set; } = 5656;
            public string NetworkType   { get; set; } = "LAN";
            public int    Latency_ms    { get; set; }
            public int Latency { get => Latency_ms; set => Latency_ms = value; }
    
            // حالة الاتصال
            public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
            public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
            public string           SessionId        { get; set; }
            public bool             IsSupervisorMode { get; set; }
            public bool             IsHostConnected  { get; set; }
    
            // طوابع زمنية UTC (T-11)
            public DateTime ConnectedAtUtc       { get; set; }
            public DateTime DisconnectedAtUtc    { get; set; }
            public DateTime LastHeartbeatUtc     { get; set; }
            public DateTime LastSyncUtc          { get; set; }
            public DateTime LastDataReceivedUtc  { get; set; }
            public DateTime LastCommandSentUtc   { get; set; }
            public DateTime LastConnectionTime { get => ConnectedAtUtc; set => ConnectedAtUtc = value; }
            public DateTime LastSyncTime { get => LastSyncUtc; set => LastSyncUtc = value; }
    
            // إحصاءات المزامنة
            public long   TotalSyncedBytes         { get; set; }
            public long   TotalTransactions        { get; set; }
            public int    ConsecutiveSyncFailures  { get; set; }
            public double SyncSuccessRate          { get; set; } = 100.0;
            public double ReceiveSpeedKBs          { get; set; }
            public long   JournalSizeToday         { get; set; }
            public double CpuUsagePercent          { get; set; }
            public double MemoryUsagePercent       { get; set; }
            public double DiskUsagePercent         { get; set; }
            public int    HealthScore              { get; set; } = 100;
            public int PendingJournalCount { get; set; }
            public double SuccessRate { get => SyncSuccessRate; set => SyncSuccessRate = value; }
            public long TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
            // إحصاءات العمليات
            public int  ApprovedTransactions  { get; set; }
            public int  FailedTransactions    { get; set; }
            public int  CardsCaptured         { get; set; }
            public long CashDispensed         { get; set; }
    
            // آخر جورنال / خطأ
            public string LastJournalFile   { get; set; }
            public string LastErrorCode     { get; set; }
            public string LastErrorMessage  { get; set; }
            public string LastTransaction   { get; set; }
            public string LastError { get => LastErrorMessage; set => LastErrorMessage = value; }
            public int TransactionCount { get => (int)Math.Min(int.MaxValue, TotalTransactions); set => TotalTransactions = value; }
            public bool IsSyncing { get => ConnectionStatus == ConnectionStatus.Syncing; set { if (value) ConnectionStatus = ConnectionStatus.Syncing; } }
    
            // مسارات
            private string _sourcePath, _backupPath;
            public string ClientVersion { get; set; }
            public string OSVersion     { get; set; }
    
            public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath)
                ? _sourcePath
                : AppConstants.GetDefaultSourcePath(ATM_Type);
    
            public void SetSourcePath(string v) => _sourcePath = v;
    
            public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                : System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
    
            public void SetBackupPath(string v) => _backupPath = v;
    
            // ==========================================
            // حالة البطاقة ولونها
            // ==========================================
    
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5)  return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing)     return ATMCardState.Syncing;
                if (IsSupervisorMode)                                 return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
    
            public Color GetCardColor()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),   // أخضر
                    ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),   // أصفر
                    ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),  // أزرق
                    ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),  // أزرق
                    ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),   // برتقالي
                    ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),   // أحمر
                    ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),   // أحمر
                    ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),  // رمادي
                    ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),   // رمادي داكن
                    _ => Color.Gray
                };
            }
    
            public string GetStatusLabel()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive      => "● متصل ونشط",
                    ATMCardState.ConnectedIdle        => "● متصل خامل",
                    ATMCardState.Syncing              => "⟳ يزامن",
                    ATMCardState.WaitingReply         => "◎ ينتظر رد",
                    ATMCardState.Supervisor           => "★ Supervisor",
                    ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                    ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                    ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                    ATMCardState.NeverConnected       => "○ لم يتصل",
                    _ => "?"
                };
            }
    
            public string GetStatusDescription() => GetStatusLabel();
    
            public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s/60)}د";
                return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
            }
    
            public void RecalculateHealthScore()
            {
                var score = 100;
                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                if (Latency_ms > 500) score -= 15;
                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                if (CpuUsagePercent > 90) score -= 10;
                if (MemoryUsagePercent > 90) score -= 10;
                if (DiskUsagePercent > 95) score -= 10;
                HealthScore = Math.Max(0, Math.Min(100, score));
            }
    
            public override string ToString() =>
                $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
        }
    
        // ==========================================
        // التنبيهات
        // ==========================================
    
        public class AlertPayload
        {
            public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
            public AlertSeverity Severity  { get; set; }
            public string        Title     { get; set; }
            public string        Message   { get; set; }
            public string        Source    { get; set; }
            public string        DedupeKey { get; set; }
            public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
            public bool          IsRead    { get; set; }
    
            public string Icon => Severity switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
        }
    
        // ==========================================
        // سجل المزامنة
        // ==========================================
    
        public class JournalSyncRecord
        {
            public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
            public string           ATM_ID          { get; set; }
            public string           FileName        { get; set; }
            public long             FileSize        { get; set; }
            public long             FileOffset      { get; set; }
            public string           Checksum        { get; set; }
            public string           MD5Hash         { get; set; }
            public string           SHA256Hash      { get; set; }
            public JournalSyncState State           { get; set; }
            public int              ProgressPercent { get; set; }
            public int              RetryCount      { get; set; }
            public string           LocalPath       { get; set; }
            public string           ServerPath      { get; set; }
            public string           Message         { get; set; }
            public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
            public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
            public DateTime?        CompletedAtUtc  { get; set; }
    
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _ => "?"
            };
    
            public string StateLabel => State switch
            {
                JournalSyncState.Pending   => "في الطابور",
                JournalSyncState.Syncing   => "قيد المزامنة",
                JournalSyncState.ReSyncing => "إعادة مزامنة",
                JournalSyncState.Completed => "محمّل",
                JournalSyncState.Failed    => "فشل",
                JournalSyncState.Archived  => "مؤرشف",
                _ => "؟"
            };
    
            public string ATM_ID { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Description { get; set; }
            public string ATM_Type { get; set; }
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }
            public ATMStatus Status { get; set; }
            public bool IsConnected { get; set; }
            public bool IsSendingData { get; set; }
            public bool IsCSCConnected { get; set; }
            public DateTime LastConnectionTime { get; set; }
            public DateTime LastDataReceived { get; set; }
            public DateTime LastHeartbeat { get; set; }
            public int[,] OperationStats { get; set; }
            public int ATMCache { get; set; }
            public int TotalDispensed { get; set; }
            public SyncStatus SyncState { get; set; }
            public long TotalBytesSent { get; set; }
            public int TotalFilesSynced { get; set; }
            public int TotalLinesSent { get; set; }
            public DateTime LastSyncTime { get; set; }
            public string LastSyncFile { get; set; }
            public string OSVersion { get; set; }
            public string ClientVersion { get; set; }
    
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
            public string GetStatusColor()
            {
                if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                if (!IsConnected)
                {
                    var elapsed = DateTime.Now - LastConnectionTime;
                    if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_OFFLINE;
                    if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_WARNING;
                    return ATMStatusColors.COLOR_OFFLINE;
                }
                if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                var idleElapsed = DateTime.Now - LastDataReceived;
                if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                    return ATMStatusColors.COLOR_IDLE;
                return ATMStatusColors.COLOR_ACTIVE;
            }
    
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                    default: return SyncStrategy.NCR_Overwrite;
                }
            }
    
            public bool NeedsAlert()
            {
                if (!IsConnected) return true;
                return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
            }
        }
    
        public class ClientConfig
        {
            public string ATM_ID { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }
            public bool SyncTimeEnabled { get; set; }
            public int MessageSizeLines { get; set; }
            public int FilePackageKB { get; set; }
            public string SourcePath { get; set; }
            public string BackupPath { get; set; }
            public bool AutoStart { get; set; }
            public bool RunAsService { get; set; }
    
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
            }
        }
    
        public class ServerConfig
        {
            public int ListenPort { get; set; }
            public string StoragePath { get; set; }
            public string ArchivePath { get; set; }
            public bool AutoArchive { get; set; }
            public int MaxConnections { get; set; }
            public bool EnableEncryption { get; set; }
            public bool EnableCompression { get; set; }
    
            public ServerConfig()
            {
                ListenPort = NetworkConfig.DEFAULT_PORT;
                StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                AutoArchive = true;
                MaxConnections = 100;
                EnableEncryption = true;
                EnableCompression = true;
            }
        }
    }
    public partial class ATMInfo
        {
            private string _sourcePath;
    
    
            private string _backupPath;
    
    
            public ATMType ATMType
            {
                get
                {
                    switch (AppConstants.NormalizeATMType(ATM_Type))
                    {
                        case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                        case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                        case AppConstants.ATM_TYPE_WN: return ATMType.WN;
                        case AppConstants.ATM_TYPE_DN: return ATMType.DieboldNixdorf;
                        case AppConstants.ATM_TYPE_HY: return ATMType.Hyosung;
                        default: return ATMType.Other;
                    }
                }
                set
                {
                    ATM_Type = value switch
                    {
                        ATMType.NCR => AppConstants.ATM_TYPE_NCR,
                        ATMType.GRG => AppConstants.ATM_TYPE_GRG,
                        ATMType.WN => AppConstants.ATM_TYPE_WN,
                        ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                        ATMType.Hyosung => AppConstants.ATM_TYPE_HY,
                        _ => "OTHER"
                    };
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            private string _sourcePath, _backupPath;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            private string _sourcePath, _backupPath;
    
    
            public string ATM_ID { get; set; }
    
    
            public string ATM_Name { get; set; }
    
    
            public string ATM_Description { get; set; }
    
    
            public string ATM_Type { get; set; }
    
    
            public string ServerIP { get; set; }
    
    
            public int ServerPort { get; set; }
    
    
            public ATMStatus Status { get; set; }
    
    
            public bool IsConnected { get; set; }
    
    
            public bool IsSendingData { get; set; }
    
    
            public bool IsCSCConnected { get; set; }
    
    
            public DateTime LastConnectionTime { get; set; }
    
    
            public DateTime LastDataReceived { get; set; }
    
    
            public DateTime LastHeartbeat { get; set; }
    
    
            public int[,] OperationStats { get; set; }
    
    
            public int ATMCache { get; set; }
    
    
            public int TotalDispensed { get; set; }
    
    
            public SyncStatus SyncState { get; set; }
    
    
            public long TotalBytesSent { get; set; }
    
    
            public int TotalFilesSynced { get; set; }
    
    
            public int TotalLinesSent { get; set; }
    
    
            public DateTime LastSyncTime { get; set; }
    
    
            public string LastSyncFile { get; set; }
    
    
            public string OSVersion { get; set; }
    
    
            public string ClientVersion { get; set; }
    
    
            public string ATMId { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public string ATM_ID { get => ATMId; set => ATMId = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public string ATM_Name { get => Name; set => Name = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public string ServerIP { get => IPAddress; set => IPAddress = value; }
    
    
            public DateTime LastHeartbeatUtc { get => LastHeartbeat; set => LastHeartbeat = value; }
    
    
            public long TotalSyncedBytes { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;
    
    
            public string Name { get; set; } = string.Empty;
    
    
            public string BranchName { get; set; } = string.Empty;
    
    
            public string Region { get; set; } = string.Empty;
    
    
            public string Location { get => Region; set => Region = value; }
    
    
            public string BranchCode { get; set; } = string.Empty;
    
    
            public string IPAddress { get; set; }
    
    
            public string NetworkType { get; set; } = "LAN";
    
    
            public int Latency { get; set; }
    
    
            public int Latency_ms { get => Latency; set => Latency = value; }
    
    
            public bool IsSupervisorMode { get; set; }
    
    
            public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    
    
            public string SessionId { get; set; }
    
    
            public bool IsHostConnected { get; set; }
    
    
            public DateTime ConnectedAtUtc { get; set; }
    
    
            public DateTime DisconnectedAtUtc { get; set; }
    
    
            public DateTime LastSyncUtc { get; set; }
    
    
            public DateTime LastDataReceivedUtc { get; set; }
    
    
            public DateTime LastCommandSentUtc { get; set; }
    
    
            public long TotalTransactions { get; set; }
    
    
            public int ConsecutiveSyncFailures { get; set; }
    
    
            public double SyncSuccessRate { get; set; } = 100.0;
    
    
            public double ReceiveSpeedKBs { get; set; }
    
    
            public long JournalSizeToday { get; set; }
    
    
            public double CpuUsagePercent { get; set; }
    
    
            public double MemoryUsagePercent { get; set; }
    
    
            public double DiskUsagePercent { get; set; }
    
    
            public int HealthScore { get; set; } = 100;
    
    
            public int PendingJournalCount { get; set; }
    
    
            public string ATMName { get => Name; set => Name = value; }
    
    
            public double SuccessRate { get => SyncSuccessRate; set => SyncSuccessRate = value; }
    
    
            public int TotalTransactionsSynced { get => (int)TotalTransactions; set => TotalTransactions = value; }
    
    
            public int ApprovedTransactions { get; set; }
    
    
            public int FailedTransactions { get; set; }
    
    
            public int CardsCaptured { get; set; }
    
    
            public long CashDispensed { get; set; }
    
    
            public string LastJournalFile { get; set; }
    
    
            public string LastErrorCode { get; set; }
    
    
            public string LastError { get; set; }
    
    
            public string LastErrorMessage { get => LastError; set => LastError = value; }
    
    
            public bool HasCashTelemetry { get; set; }
    
    
            public long Cassette1Remaining { get; set; }
    
    
            public long Cassette2Remaining { get; set; }
    
    
            public long Cassette3Remaining { get; set; }
    
    
            public long Cassette4Remaining { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public long ATMCache { get; set; }
    
    
            public long CashLoadedTotal { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public long TotalDispensed { get; set; }
    
    
            public long CashDepositInTotal { get; set; }
    
    
            public long CashRejectCount { get; set; }
    
    
            public long CashRetractCount { get; set; }
    
    
            public DateTime CashTelemetryUpdatedAtUtc { get; set; }
    
    
            public string LastTransaction { get; set; }
    
    
            public bool HasAlerts { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            public string ATMType { get; set; }
    
    
            public string Vendor { get; set; }
    
    
            public string Model { get; set; }
    
    
            public string Branch { get; set; }
    
    
            public string SourceJournalPath { get; set; }
    
    
            public string BackupPath { get; set; }
    
    
            public string ImageInboxPath { get; set; }
    
    
            public string ImageDestPath { get; set; }
    
    
            public DateTime LastSync { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public string ATMId   { get => ATM_ID;   set => ATM_ID   = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public int    Latency_ms  { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public int    Latency     { get => Latency_ms; set => Latency_ms = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeatUtc    { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public long   TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public string LastErrorMessage  { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
            public string ATM_ID { get => ATMId; set => ATMId = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
            public string ATM_Name { get => Name; set => Name = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
            public string ServerIP { get => IPAddress; set => IPAddress = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
            public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
            public long ATMCache { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
            public long TotalDispensed { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Core\Models\ATMInfo.cs
            public string ATMName { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string ATMId   { get => ATM_ID;   set => ATM_ID   = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public int    Latency_ms  { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public int    Latency     { get => Latency_ms; set => Latency_ms = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public DateTime LastHeartbeatUtc    { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public long   TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string LastErrorMessage  { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                ATMId = string.Empty;
                IPAddress = string.Empty;
                ServerPort = AppConstants.DefaultPort;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
            public ATMInfo()
            {
                ATMId = string.Empty;
                IPAddress = string.Empty;
                ServerPort = AppConstants.DefaultPort;
            }
    
    
            public string GetStatusColor()
            {
                if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                if (!IsConnected)
                {
                    var elapsed = DateTime.Now - LastConnectionTime;
                    if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_OFFLINE;
                    if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_WARNING;
                    return ATMStatusColors.COLOR_OFFLINE;
                }
                if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                var idleElapsed = DateTime.Now - LastDataReceived;
                if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                    return ATMStatusColors.COLOR_IDLE;
                return ATMStatusColors.COLOR_ACTIVE;
            }
    
    
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                    default: return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            public bool NeedsAlert()
            {
                if (!IsConnected) return true;
                return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
            }
    
    
            public string GetElapsed(DateTime? dt) => !dt.HasValue ? "—" : $"{(DateTime.UtcNow - dt.Value).TotalMinutes:N0}m";
    
    
            public string GetStatusDescription() => Status.ToString();
    
    
            public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public bool NeedsAlert() => HasAlerts;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            public void SetSourcePath(string v) => _sourcePath = v;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            public void SetBackupPath(string v) => _backupPath = v;
    
    
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeat == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeat).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5) return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing) return ATMCardState.Syncing;
                if (IsSupervisorMode) return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
    
    
            public Color GetCardColor()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive => Color.FromArgb(52, 199, 89),
                    ATMCardState.ConnectedIdle => Color.FromArgb(255, 214, 10),
                    ATMCardState.Syncing => Color.FromArgb(10, 132, 255),
                    ATMCardState.WaitingReply => Color.FromArgb(10, 132, 255),
                    ATMCardState.Supervisor => Color.FromArgb(255, 159, 10),
                    ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69, 58),
                    ATMCardState.WarningOffline => Color.FromArgb(255, 69, 58),
                    ATMCardState.CriticalOffline => Color.FromArgb(99, 99, 102),
                    ATMCardState.NeverConnected => Color.FromArgb(72, 72, 74),
                    _ => Color.Gray
                };
            }
    
    
            public string GetStatusLabel()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive => "● متصل ونشط",
                    ATMCardState.ConnectedIdle => "● متصل خامل",
                    ATMCardState.Syncing => "⟳ يزامن",
                    ATMCardState.WaitingReply => "◎ ينتظر رد",
                    ATMCardState.Supervisor => "★ Supervisor",
                    ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                    ATMCardState.WarningOffline => "✕ انقطاع >5د",
                    ATMCardState.CriticalOffline => "✕ انقطاع حرج",
                    ATMCardState.NeverConnected => "○ لم يتصل",
                    _ => "?"
                };
            }
    
    
            public void RecalculateHealthScore()
            {
                var score = 100;
                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                if (Latency > 500) score -= 15;
                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                if (CpuUsagePercent > 90) score -= 10;
                if (MemoryUsagePercent > 90) score -= 10;
                if (DiskUsagePercent > 95) score -= 10;
                HealthScore = Math.Max(0, Math.Min(100, score));
            }
    
    
            public override string ToString() =>
                $"[{ATMId}] {Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
    
            public string GetLegacySourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_SOURCE;
                    default:                        return string.Empty;
                }
            }
    
    
            public string GetLegacyBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_BACKUP;
                    default:                        return string.Empty;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5)  return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing)      return ATMCardState.Syncing;
                if (IsSupervisorMode)                                   return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply)  return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public string GetStatusDescription()        => GetStatusLabel();
    
    
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public void RecalculateHealthScore()
            {
                var score = 100;
                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                if (Latency_ms > 500) score -= 15;
                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                if (CpuUsagePercent    > 90) score -= 10;
                if (MemoryUsagePercent > 90) score -= 10;
                if (DiskUsagePercent   > 95) score -= 10;
                HealthScore = Math.Max(0, Math.Min(100, score));
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public override string ToString() =>
                $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
            public bool NeedsAlert() => HasAlerts;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath)
                ? _sourcePath
                : AppConstants.GetDefaultSourcePath(ATM_Type);
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                : System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATMId ?? "DEFAULT");
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5)  return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing)      return ATMCardState.Syncing;
                if (IsSupervisorMode)                                   return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply)  return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public string GetStatusDescription()        => GetStatusLabel();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public void RecalculateHealthScore()
            {
                var score = 100;
                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                if (Latency_ms > 500) score -= 15;
                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                if (CpuUsagePercent    > 90) score -= 10;
                if (MemoryUsagePercent > 90) score -= 10;
                if (DiskUsagePercent   > 95) score -= 10;
                HealthScore = Math.Max(0, Math.Min(100, score));
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            public override string ToString() =>
                $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
    
        }
    public partial class ATMInfo
        {
            public string ATMId { get; set; }
    
    
            public string ATMName { get; set; }
    
    
            public string ATMType { get; set; }
    
    
            public string Vendor { get; set; }
    
    
            public string Model { get; set; }
    
    
            public string Branch { get; set; }
    
    
            public string Region { get; set; }
    
    
            public string NetworkType { get; set; }
    
    
            public string ServerIP { get; set; }
    
    
            public int ServerPort { get; set; }
    
    
            public string SourceJournalPath { get; set; }
    
    
            public string BackupPath { get; set; }
    
    
            public string ImageInboxPath { get; set; }
    
    
            public string ImageDestPath { get; set; }
    
    
            public bool IsConnected { get; set; }
    
    
            public DateTime LastHeartbeat { get; set; }
    
    
            public DateTime LastSync { get; set; }
    
    
        }
    public partial class ATMInfo
        {
            public string ATM_ID { get; set; }
    
    
            public string ATM_Name { get; set; }
    
    
            public string ATM_Description { get; set; }
    
    
            public string ATM_Type { get; set; }
    
    
            public string ServerIP { get; set; }
    
    
            public int ServerPort { get; set; }
    
    
            public ATMStatus Status { get; set; }
    
    
            public bool IsConnected { get; set; }
    
    
            public bool IsSendingData { get; set; }
    
    
            public bool IsCSCConnected { get; set; }
    
    
            public DateTime LastConnectionTime { get; set; }
    
    
            public DateTime LastDataReceived { get; set; }
    
    
            public DateTime LastHeartbeat { get; set; }
    
    
            public int[,] OperationStats { get; set; }
    
    
            public int ATMCache { get; set; }
    
    
            public int TotalDispensed { get; set; }
    
    
            public SyncStatus SyncState { get; set; }
    
    
            public long TotalBytesSent { get; set; }
    
    
            public int TotalFilesSynced { get; set; }
    
    
            public int TotalLinesSent { get; set; }
    
    
            public DateTime LastSyncTime { get; set; }
    
    
            public string LastSyncFile { get; set; }
    
    
            public string OSVersion { get; set; }
    
    
            public string ClientVersion { get; set; }
    
    
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            public string GetStatusColor()
            {
                if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                if (!IsConnected)
                {
                    var elapsed = DateTime.Now - LastConnectionTime;
                    if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_OFFLINE;
                    if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_WARNING;
                    return ATMStatusColors.COLOR_OFFLINE;
                }
                if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                var idleElapsed = DateTime.Now - LastDataReceived;
                if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                    return ATMStatusColors.COLOR_IDLE;
                return ATMStatusColors.COLOR_ACTIVE;
            }
    
    
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                    default: return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            public bool NeedsAlert()
            {
                if (!IsConnected) return true;
                return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
            }
    
    
        }
    public partial class ATMInfo
        {
            public class ATMInfo
            {
                // هوية
                public string ATM_ID        { get; set; }
                public string ATM_Name      { get; set; }
                public string ATM_Type      { get; set; }    // NCR / GRG / WN / DIEBOLD / HYOSUNG
                public string BranchName    { get; set; }
                public string Region        { get; set; }
                public string ATMId { get => ATM_ID; set => ATM_ID = value; }
                public string ATMName { get => ATM_Name; set => ATM_Name = value; }
                public string IPAddress { get => ServerIP; set => ServerIP = value; }
                public ATMType ATMType
                {
                    get
                    {
                        switch (AppConstants.NormalizeATMType(ATM_Type))
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                            case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                            case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
                            case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
                            case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
                            default:                        return ATMType.Other;
                        }
                    }
                    set
                    {
                        ATM_Type = value switch
                        {
                            ATMType.NCR            => AppConstants.ATM_TYPE_NCR,
                            ATMType.GRG            => AppConstants.ATM_TYPE_GRG,
                            ATMType.WN             => AppConstants.ATM_TYPE_WN,
                            ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                            ATMType.Hyosung        => AppConstants.ATM_TYPE_HY,
                            _                      => "OTHER"
                        };
                    }
                }
                public string Location { get => Region; set => Region = value; }
                public string BranchCode { get; set; }
    
                // شبكة
                public string ServerIP      { get; set; }
                public int    ServerPort    { get; set; } = 5656;
                public string NetworkType   { get; set; } = "LAN";
                public int    Latency_ms    { get; set; }
                public int Latency { get => Latency_ms; set => Latency_ms = value; }
    
                // حالة الاتصال
                public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
                public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
                public string           SessionId        { get; set; }
                public bool             IsSupervisorMode { get; set; }
                public bool             IsHostConnected  { get; set; }
    
                // طوابع زمنية UTC (T-11)
                public DateTime ConnectedAtUtc       { get; set; }
                public DateTime DisconnectedAtUtc    { get; set; }
                public DateTime LastHeartbeatUtc     { get; set; }
                public DateTime LastSyncUtc          { get; set; }
                public DateTime LastDataReceivedUtc  { get; set; }
                public DateTime LastCommandSentUtc   { get; set; }
                public DateTime LastConnectionTime { get => ConnectedAtUtc; set => ConnectedAtUtc = value; }
                public DateTime LastSyncTime { get => LastSyncUtc; set => LastSyncUtc = value; }
    
                // إحصاءات المزامنة
                public long   TotalSyncedBytes         { get; set; }
                public long   TotalTransactions        { get; set; }
                public int    ConsecutiveSyncFailures  { get; set; }
                public double SyncSuccessRate          { get; set; } = 100.0;
                public double ReceiveSpeedKBs          { get; set; }
                public long   JournalSizeToday         { get; set; }
                public double CpuUsagePercent          { get; set; }
                public double MemoryUsagePercent       { get; set; }
                public double DiskUsagePercent         { get; set; }
                public int    HealthScore              { get; set; } = 100;
                public int PendingJournalCount { get; set; }
                public double SuccessRate { get => SyncSuccessRate; set => SyncSuccessRate = value; }
                public long TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
                // إحصاءات العمليات
                public int  ApprovedTransactions  { get; set; }
                public int  FailedTransactions    { get; set; }
                public int  CardsCaptured         { get; set; }
                public long CashDispensed         { get; set; }
    
                // آخر جورنال / خطأ
                public string LastJournalFile   { get; set; }
                public string LastErrorCode     { get; set; }
                public string LastErrorMessage  { get; set; }
                public string LastTransaction   { get; set; }
                public string LastError { get => LastErrorMessage; set => LastErrorMessage = value; }
                public int TransactionCount { get => (int)Math.Min(int.MaxValue, TotalTransactions); set => TotalTransactions = value; }
                public bool IsSyncing { get => ConnectionStatus == ConnectionStatus.Syncing; set { if (value) ConnectionStatus = ConnectionStatus.Syncing; } }
    
                // مسارات
                private string _sourcePath, _backupPath;
                public string ClientVersion { get; set; }
                public string OSVersion     { get; set; }
    
                public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath)
                    ? _sourcePath
                    : AppConstants.GetDefaultSourcePath(ATM_Type);
    
                public void SetSourcePath(string v) => _sourcePath = v;
    
                public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                    : System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                        "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
    
                public void SetBackupPath(string v) => _backupPath = v;
    
                // ==========================================
                // حالة البطاقة ولونها
                // ==========================================
    
                public ATMCardState GetCardState()
                {
                    if (ConnectionStatus == ConnectionStatus.Disconnected)
                    {
                        if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                        var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                        if (mins > 10) return ATMCardState.CriticalOffline;
                        if (mins > 5)  return ATMCardState.WarningOffline;
                        return ATMCardState.RecentlyDisconnected;
                    }
                    if (ConnectionStatus == ConnectionStatus.Syncing)     return ATMCardState.Syncing;
                    if (IsSupervisorMode)                                 return ATMCardState.Supervisor;
                    if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                    var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                    return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                }
    
                public Color GetCardColor()
                {
                    return GetCardState() switch
                    {
                        ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),   // أخضر
                        ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),   // أصفر
                        ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),  // أزرق
                        ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),  // أزرق
                        ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),   // برتقالي
                        ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),   // أحمر
                        ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),   // أحمر
                        ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),  // رمادي
                        ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),   // رمادي داكن
                        _ => Color.Gray
                    };
                }
    
                public string GetStatusLabel()
                {
                    return GetCardState() switch
                    {
                        ATMCardState.ConnectedActive      => "● متصل ونشط",
                        ATMCardState.ConnectedIdle        => "● متصل خامل",
                        ATMCardState.Syncing              => "⟳ يزامن",
                        ATMCardState.WaitingReply         => "◎ ينتظر رد",
                        ATMCardState.Supervisor           => "★ Supervisor",
                        ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                        ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                        ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                        ATMCardState.NeverConnected       => "○ لم يتصل",
                        _ => "?"
                    };
                }
    
                public string GetStatusDescription() => GetStatusLabel();
    
                public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
                public string GetElapsed(DateTime utcRef)
                {
                    if (utcRef == DateTime.MinValue) return "---";
                    var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                    if (s < 60)   return $"{(int)s}ث";
                    if (s < 3600) return $"{(int)(s/60)}د";
                    return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
                }
    
                public void RecalculateHealthScore()
                {
                    var score = 100;
                    if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                    if (Latency_ms > 500) score -= 15;
                    if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                    if (CpuUsagePercent > 90) score -= 10;
                    if (MemoryUsagePercent > 90) score -= 10;
                    if (DiskUsagePercent > 95) score -= 10;
                    HealthScore = Math.Max(0, Math.Min(100, score));
                }
    
                public override string ToString() =>
                    $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
            }
    
    
            public class AlertPayload
            {
                public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
                public AlertSeverity Severity  { get; set; }
                public string        Title     { get; set; }
                public string        Message   { get; set; }
                public string        Source    { get; set; }
                public string        DedupeKey { get; set; }
                public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
                public bool          IsRead    { get; set; }
    
                public string Icon => Severity switch
                {
                    AlertSeverity.Emergency => "CRIT",
                    AlertSeverity.Critical  => "FAIL",
                    AlertSeverity.Warning   => "WARN",
                    _                       => "INFO"
                };
                public string SeverityIcon => Icon;
                public Color Color => Severity switch
                {
                    AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                    AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                    AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                    _                       => Color.FromArgb(0,   122, 255)
                };
            }
    
    
            public class JournalSyncRecord
            {
                public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
                public string           ATM_ID          { get; set; }
                public string           FileName        { get; set; }
                public long             FileSize        { get; set; }
                public long             FileOffset      { get; set; }
                public string           Checksum        { get; set; }
                public string           MD5Hash         { get; set; }
                public string           SHA256Hash      { get; set; }
                public JournalSyncState State           { get; set; }
                public int              ProgressPercent { get; set; }
                public int              RetryCount      { get; set; }
                public string           LocalPath       { get; set; }
                public string           ServerPath      { get; set; }
                public string           Message         { get; set; }
                public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
                public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
                public DateTime?        CompletedAtUtc  { get; set; }
    
                public string StateIcon => State switch
                {
                    JournalSyncState.Pending   => "PEND",
                    JournalSyncState.Syncing   => "SYNC",
                    JournalSyncState.ReSyncing => "RSYNC",
                    JournalSyncState.Completed => "OK",
                    JournalSyncState.Failed    => "FAIL",
                    JournalSyncState.Archived  => "ARCH",
                    _ => "?"
                };
    
                public string StateLabel => State switch
                {
                    JournalSyncState.Pending   => "في الطابور",
                    JournalSyncState.Syncing   => "قيد المزامنة",
                    JournalSyncState.ReSyncing => "إعادة مزامنة",
                    JournalSyncState.Completed => "محمّل",
                    JournalSyncState.Failed    => "فشل",
                    JournalSyncState.Archived  => "مؤرشف",
                    _ => "؟"
                };
    
                public string ATM_ID { get; set; }
                public string ATM_Name { get; set; }
                public string ATM_Description { get; set; }
                public string ATM_Type { get; set; }
                public string ServerIP { get; set; }
                public int ServerPort { get; set; }
                public ATMStatus Status { get; set; }
                public bool IsConnected { get; set; }
                public bool IsSendingData { get; set; }
                public bool IsCSCConnected { get; set; }
                public DateTime LastConnectionTime { get; set; }
                public DateTime LastDataReceived { get; set; }
                public DateTime LastHeartbeat { get; set; }
                public int[,] OperationStats { get; set; }
                public int ATMCache { get; set; }
                public int TotalDispensed { get; set; }
                public SyncStatus SyncState { get; set; }
                public long TotalBytesSent { get; set; }
                public int TotalFilesSynced { get; set; }
                public int TotalLinesSent { get; set; }
                public DateTime LastSyncTime { get; set; }
                public string LastSyncFile { get; set; }
                public string OSVersion { get; set; }
                public string ClientVersion { get; set; }
    
                public ATMInfo()
                {
                    OperationStats = new int[3, 4];
                    Status = ATMStatus.Unknown;
                    SyncState = SyncStatus.Idle;
                    ServerPort = NetworkConfig.DEFAULT_PORT;
                }
    
                public string GetStatusColor()
                {
                    if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                    if (!IsConnected)
                    {
                        var elapsed = DateTime.Now - LastConnectionTime;
                        if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                            return ATMStatusColors.COLOR_OFFLINE;
                        if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                            return ATMStatusColors.COLOR_WARNING;
                        return ATMStatusColors.COLOR_OFFLINE;
                    }
                    if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                    var idleElapsed = DateTime.Now - LastDataReceived;
                    if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                        return ATMStatusColors.COLOR_IDLE;
                    return ATMStatusColors.COLOR_ACTIVE;
                }
    
                public string GetSourcePath()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                        case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                        case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                        default: return string.Empty;
                    }
                }
    
                public string GetBackupPath()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                        case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                        case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                        default: return string.Empty;
                    }
                }
    
                public SyncStrategy GetSyncStrategy()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                        case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                        case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                        default: return SyncStrategy.NCR_Overwrite;
                    }
                }
    
                public bool NeedsAlert()
                {
                    if (!IsConnected) return true;
                    return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                }
            }
    
    
            public class ClientConfig
            {
                public string ATM_ID { get; set; }
                public string ATM_Name { get; set; }
                public string ATM_Type { get; set; }
                public string ServerIP { get; set; }
                public int ServerPort { get; set; }
                public bool SyncTimeEnabled { get; set; }
                public int MessageSizeLines { get; set; }
                public int FilePackageKB { get; set; }
                public string SourcePath { get; set; }
                public string BackupPath { get; set; }
                public bool AutoStart { get; set; }
                public bool RunAsService { get; set; }
    
                public ClientConfig()
                {
                    ServerPort = NetworkConfig.DEFAULT_PORT;
                    MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                    FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                    AutoStart = true;
                    RunAsService = true;
                }
            }
    
    
            public class ServerConfig
            {
                public int ListenPort { get; set; }
                public string StoragePath { get; set; }
                public string ArchivePath { get; set; }
                public bool AutoArchive { get; set; }
                public int MaxConnections { get; set; }
                public bool EnableEncryption { get; set; }
                public bool EnableCompression { get; set; }
    
                public ServerConfig()
                {
                    ListenPort = NetworkConfig.DEFAULT_PORT;
                    StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                    ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                    AutoArchive = true;
                    MaxConnections = 100;
                    EnableEncryption = true;
                    EnableCompression = true;
                }
            }
    
    
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            public enum ATMStatus
            {
                Unknown = 0,
                Online = 1,
                Idle = 2,
                Supervisor = 3,
                Warning = 4,
                Offline = 5,
                Critical = 6,
                InService = 10,
                ConnectedOnly = 11,
                WaitingResponse = 12,
                OutOfService = 13,
                CriticalFault = 14,
                Fault = 15,
                Maintenance = 16
            }
    
    
            public enum ATMType { NCR, GRG, WN, DieboldNixdorf, Hyosung, Other }
    
    
            public enum SyncStatus { Pending, InProgress, Syncing, Resyncing, Completed, Failed }
    
    
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            public enum ATMCardState      { NeverConnected, ConnectedActive, ConnectedIdle, Syncing, WaitingReply, Supervisor, RecentlyDisconnected, WarningOffline, CriticalOffline }
    
    
        }
    public partial class ATMInfo
        {
            private string _sourcePath, _backupPath;
    
    
            public string ATM_ID        { get; set; }
    
    
            public string ATM_Name      { get; set; }
    
    
            public string ATM_Type      { get; set; }    // NCR / GRG / WN
    
    
            public string BranchName    { get; set; }
    
    
            public string Region        { get; set; }
    
    
            public string ServerIP      { get; set; }
    
    
            public int    ServerPort    { get; set; } = 5656;
    
    
            public string NetworkType   { get; set; } = "LAN";
    
    
            public int    Latency_ms    { get; set; }
    
    
            public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    
    
            public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
    
    
            public string           SessionId        { get; set; }
    
    
            public bool             IsSupervisorMode { get; set; }
    
    
            public bool             IsHostConnected  { get; set; }
    
    
            public DateTime ConnectedAtUtc       { get; set; }
    
    
            public DateTime DisconnectedAtUtc    { get; set; }
    
    
            public DateTime LastHeartbeatUtc     { get; set; }
    
    
            public DateTime LastSyncUtc          { get; set; }
    
    
            public DateTime LastDataReceivedUtc  { get; set; }
    
    
            public DateTime LastCommandSentUtc   { get; set; }
    
    
            public long   TotalSyncedBytes         { get; set; }
    
    
            public long   TotalTransactions        { get; set; }
    
    
            public int    ConsecutiveSyncFailures  { get; set; }
    
    
            public double SyncSuccessRate          { get; set; } = 100.0;
    
    
            public double ReceiveSpeedKBs          { get; set; }
    
    
            public long   JournalSizeToday         { get; set; }
    
    
            public int  ApprovedTransactions  { get; set; }
    
    
            public int  FailedTransactions    { get; set; }
    
    
            public int  CardsCaptured         { get; set; }
    
    
            public long CashDispensed         { get; set; }
    
    
            public string LastJournalFile   { get; set; }
    
    
            public string LastErrorCode     { get; set; }
    
    
            public string LastErrorMessage  { get; set; }
    
    
            public string LastTransaction   { get; set; }
    
    
            public string ClientVersion { get; set; }
    
    
            public string OSVersion     { get; set; }
    
    
            public string ATM_Description { get; set; }
    
    
            public bool IsConnected { get; set; }
    
    
            public bool IsSendingData { get; set; }
    
    
            public bool IsCSCConnected { get; set; }
    
    
            public DateTime LastConnectionTime { get; set; }
    
    
            public DateTime LastDataReceived { get; set; }
    
    
            public DateTime LastHeartbeat { get; set; }
    
    
            public int[,] OperationStats { get; set; }
    
    
            public int ATMCache { get; set; }
    
    
            public int TotalDispensed { get; set; }
    
    
            public SyncStatus SyncState { get; set; }
    
    
            public long TotalBytesSent { get; set; }
    
    
            public int TotalFilesSynced { get; set; }
    
    
            public int TotalLinesSent { get; set; }
    
    
            public DateTime LastSyncTime { get; set; }
    
    
            public string LastSyncFile { get; set; }
    
    
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
            public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath) ? _sourcePath
                : ATM_Type == "NCR" ? @"C:\NCRJournal\"
                : ATM_Type == "GRG" ? @"D:\GRGData\EJ\"
                : ATM_Type == "WN"  ? @"C:\WOSA\EJ\"
                : @"C:\Journal\";
    
    
            public void SetSourcePath(string v) => _sourcePath = v;
    
    
            public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                : System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
    
    
            public void SetBackupPath(string v) => _backupPath = v;
    
    
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5)  return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing)     return ATMCardState.Syncing;
                if (IsSupervisorMode)                                 return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
    
    
            public Color GetCardColor()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),   // أخضر
                    ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),   // أصفر
                    ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),  // أزرق
                    ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),  // أزرق
                    ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),   // برتقالي
                    ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),   // أحمر
                    ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),   // أحمر
                    ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),  // رمادي
                    ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),   // رمادي داكن
                    _ => Color.Gray
                };
            }
    
    
            public string GetStatusLabel()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive      => "● متصل ونشط",
                    ATMCardState.ConnectedIdle        => "● متصل خامل",
                    ATMCardState.Syncing              => "⟳ يزامن",
                    ATMCardState.WaitingReply         => "◎ ينتظر رد",
                    ATMCardState.Supervisor           => "★ Supervisor",
                    ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                    ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                    ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                    ATMCardState.NeverConnected       => "○ لم يتصل",
                    _ => "?"
                };
            }
    
    
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s/60)}د";
                return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
            }
    
    
            public override string ToString() =>
                $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
    
            public string GetStatusColor()
            {
                if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                if (!IsConnected)
                {
                    var elapsed = DateTime.Now - LastConnectionTime;
                    if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_OFFLINE;
                    if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_WARNING;
                    return ATMStatusColors.COLOR_OFFLINE;
                }
                if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                var idleElapsed = DateTime.Now - LastDataReceived;
                if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                    return ATMStatusColors.COLOR_IDLE;
                return ATMStatusColors.COLOR_ACTIVE;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                    default: return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            public bool NeedsAlert()
            {
                if (!IsConnected) return true;
                return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Models\ATMInfo.cs
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Models\ATMInfo.cs
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
    
        }
    public partial class ATMInfo
        {
            public ATMType ATMType
            {
                get
                {
                    switch (AppConstants.NormalizeATMType(ATM_Type))
                    {
                        case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                        case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                        case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
                        case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
                        case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
                        default:                        return ATMType.Other;
                    }
                }
                set
                {
                    ATM_Type = value switch
                    {
                        ATMType.NCR            => AppConstants.ATM_TYPE_NCR,
                        ATMType.GRG            => AppConstants.ATM_TYPE_GRG,
                        ATMType.WN             => AppConstants.ATM_TYPE_WN,
                        ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                        ATMType.Hyosung        => AppConstants.ATM_TYPE_HY,
                        _                      => "OTHER"
                    };
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            private string _sourcePath, _backupPath;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            public string ATM_ID        { get; set; }
    
    
            public string ATM_Name      { get; set; }
    
    
            public string ATM_Type      { get; set; }   // NCR / GRG / WN / DIEBOLD / HYOSUNG
    
    
            public string BranchName    { get; set; }
    
    
            public string Region        { get; set; }
    
    
            public string ATM_Description { get; set; } // من النسخة القديمة
    
    
            public string ATMId   { get => ATM_ID;   set => ATM_ID   = value; }
    
    
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            public string Location  { get => Region;   set => Region   = value; }
    
    
            public string BranchCode { get; set; }
    
    
            public string ServerIP    { get; set; }
    
    
            public int    ServerPort  { get; set; } = 5656;
    
    
            public string NetworkType { get; set; } = "LAN";
    
    
            public int    Latency_ms  { get; set; }
    
    
            public int    Latency     { get => Latency_ms; set => Latency_ms = value; }
    
    
            public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    
    
            public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
    
    
            public string           SessionId        { get; set; }
    
    
            public bool             IsSupervisorMode { get; set; }
    
    
            public bool             IsHostConnected  { get; set; }
    
    
            public bool     IsSendingData  { get; set; }   // من النسخة القديمة
    
    
            public bool     IsCSCConnected { get; set; }   // من النسخة القديمة
    
    
            public DateTime ConnectedAtUtc      { get; set; }
    
    
            public DateTime DisconnectedAtUtc   { get; set; }
    
    
            public DateTime LastHeartbeatUtc    { get; set; }
    
    
            public DateTime LastSyncUtc         { get; set; }
    
    
            public DateTime LastDataReceivedUtc { get; set; }
    
    
            public DateTime LastCommandSentUtc  { get; set; }
    
    
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            public DateTime LastDataReceived   { get; set; }
    
    
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            public long   TotalSyncedBytes        { get; set; }
    
    
            public long   TotalTransactions       { get; set; }
    
    
            public int    ConsecutiveSyncFailures { get; set; }
    
    
            public double SyncSuccessRate         { get; set; } = 100.0;
    
    
            public double ReceiveSpeedKBs         { get; set; }
    
    
            public long   JournalSizeToday        { get; set; }
    
    
            public double CpuUsagePercent         { get; set; }
    
    
            public double MemoryUsagePercent      { get; set; }
    
    
            public double DiskUsagePercent        { get; set; }
    
    
            public int    HealthScore             { get; set; } = 100;
    
    
            public int    PendingJournalCount     { get; set; }
    
    
            public double SuccessRate           { get => SyncSuccessRate;   set => SyncSuccessRate   = value; }
    
    
            public long   TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
    
            public int[,]  OperationStats    { get; set; }
    
    
            public int     ATMCache          { get; set; }
    
    
            public int     TotalDispensed    { get; set; }
    
    
            public SyncStatus SyncState      { get; set; }
    
    
            public long    TotalBytesSent    { get; set; }
    
    
            public int     TotalFilesSynced  { get; set; }
    
    
            public int     TotalLinesSent    { get; set; }
    
    
            public string  LastSyncFile      { get; set; }
    
    
            public int  ApprovedTransactions { get; set; }
    
    
            public int  FailedTransactions   { get; set; }
    
    
            public int  CardsCaptured        { get; set; }
    
    
            public long CashDispensed        { get; set; }
    
    
            public string LastJournalFile   { get; set; }
    
    
            public string LastErrorCode     { get; set; }
    
    
            public string LastErrorMessage  { get; set; }
    
    
            public string LastTransaction   { get; set; }
    
    
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            public string  ClientVersion { get; set; }
    
    
            public string  OSVersion     { get; set; }
    
    
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status         = ATMStatus.Unknown;
                SyncState      = SyncStatus.Idle;
                ServerPort     = NetworkConfig.DEFAULT_PORT;
            }
    
    
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            public string GetLegacySourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_SOURCE;
                    default:                        return string.Empty;
                }
            }
    
    
            public void SetSourcePath(string v) => _sourcePath = v;
    
    
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            public string GetLegacyBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_BACKUP;
                    default:                        return string.Empty;
                }
            }
    
    
            public void SetBackupPath(string v) => _backupPath = v;
    
    
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5)  return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing)      return ATMCardState.Syncing;
                if (IsSupervisorMode)                                   return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply)  return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
    
    
            public Color GetCardColor()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),
                    ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),
                    ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),
                    ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),
                    ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),
                    ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),
                    ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),
                    ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),
                    ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),
                    _                                 => Color.Gray
                };
            }
    
    
            public string GetStatusLabel()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive      => "● متصل ونشط",
                    ATMCardState.ConnectedIdle        => "● متصل خامل",
                    ATMCardState.Syncing              => "⟳ يزامن",
                    ATMCardState.WaitingReply         => "◎ ينتظر رد",
                    ATMCardState.Supervisor           => "★ Supervisor",
                    ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                    ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                    ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                    ATMCardState.NeverConnected       => "○ لم يتصل",
                    _                                 => "?"
                };
            }
    
    
            public string GetStatusDescription()        => GetStatusLabel();
    
    
            public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
    
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            public string GetStatusColor()
            {
                if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                if (!IsConnected)
                {
                    var elapsed = DateTime.Now - LastConnectionTime;
                    if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_OFFLINE;
                    if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_WARNING;
                    return ATMStatusColors.COLOR_OFFLINE;
                }
                if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                var idleElapsed = DateTime.Now - LastDataReceived;
                if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                    return ATMStatusColors.COLOR_IDLE;
                return ATMStatusColors.COLOR_ACTIVE;
            }
    
    
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            public bool NeedsAlert()
            {
                if (!IsConnected) return true;
                return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
            }
    
    
            public void RecalculateHealthScore()
            {
                var score = 100;
                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                if (Latency_ms > 500) score -= 15;
                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                if (CpuUsagePercent    > 90) score -= 10;
                if (MemoryUsagePercent > 90) score -= 10;
                if (DiskUsagePercent   > 95) score -= 10;
                HealthScore = Math.Max(0, Math.Min(100, score));
            }
    
    
            public override string ToString() =>
                $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
    
        }
    public partial class ATMInfo
        {
            public ATMType ATMType
            {
                get
                {
                    switch (AppConstants.NormalizeATMType(ATM_Type))
                    {
                        case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                        case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                        case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
                        case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
                        case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
                        default:                        return ATMType.Other;
                    }
                }
                set
                {
                    ATM_Type = value switch
                    {
                        ATMType.NCR            => AppConstants.ATM_TYPE_NCR,
                        ATMType.GRG            => AppConstants.ATM_TYPE_GRG,
                        ATMType.WN             => AppConstants.ATM_TYPE_WN,
                        ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                        ATMType.Hyosung        => AppConstants.ATM_TYPE_HY,
                        _                      => "OTHER"
                    };
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            private string _sourcePath, _backupPath;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
    
            public string ATM_ID        { get; set; }
    
    
            public string ATM_Name      { get; set; }
    
    
            public string ATM_Type      { get; set; }   // NCR / GRG / WN / DIEBOLD / HYOSUNG
    
    
            public string BranchName    { get; set; }
    
    
            public string Region        { get; set; }
    
    
            public string ATM_Description { get; set; } // من النسخة القديمة
    
    
            public string ATMId   { get => ATM_ID;   set => ATM_ID   = value; }
    
    
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
    
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
    
            public string Location  { get => Region;   set => Region   = value; }
    
    
            public string BranchCode { get; set; }
    
    
            public string ServerIP    { get; set; }
    
    
            public int    ServerPort  { get; set; } = 5656;
    
    
            public string NetworkType { get; set; } = "LAN";
    
    
            public int    Latency_ms  { get; set; }
    
    
            public int    Latency     { get => Latency_ms; set => Latency_ms = value; }
    
    
            public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    
    
            public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
    
    
            public string           SessionId        { get; set; }
    
    
            public bool             IsSupervisorMode { get; set; }
    
    
            public bool             IsHostConnected  { get; set; }
    
    
            public bool     IsSendingData  { get; set; }   // من النسخة القديمة
    
    
            public bool     IsCSCConnected { get; set; }   // من النسخة القديمة
    
    
            public DateTime ConnectedAtUtc      { get; set; }
    
    
            public DateTime DisconnectedAtUtc   { get; set; }
    
    
            public DateTime LastHeartbeatUtc    { get; set; }
    
    
            public DateTime LastSyncUtc         { get; set; }
    
    
            public DateTime LastDataReceivedUtc { get; set; }
    
    
            public DateTime LastCommandSentUtc  { get; set; }
    
    
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
    
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
    
            public DateTime LastDataReceived   { get; set; }
    
    
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
    
            public long   TotalSyncedBytes        { get; set; }
    
    
            public long   TotalTransactions       { get; set; }
    
    
            public int    ConsecutiveSyncFailures { get; set; }
    
    
            public double SyncSuccessRate         { get; set; } = 100.0;
    
    
            public double ReceiveSpeedKBs         { get; set; }
    
    
            public long   JournalSizeToday        { get; set; }
    
    
            public double CpuUsagePercent         { get; set; }
    
    
            public double MemoryUsagePercent      { get; set; }
    
    
            public double DiskUsagePercent        { get; set; }
    
    
            public int    HealthScore             { get; set; } = 100;
    
    
            public int    PendingJournalCount     { get; set; }
    
    
            public double SuccessRate           { get => SyncSuccessRate;   set => SyncSuccessRate   = value; }
    
    
            public long   TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
    
            public int[,]  OperationStats    { get; set; }
    
    
            public int     ATMCache          { get; set; }
    
    
            public int     TotalDispensed    { get; set; }
    
    
            public SyncStatus SyncState      { get; set; }
    
    
            public long    TotalBytesSent    { get; set; }
    
    
            public int     TotalFilesSynced  { get; set; }
    
    
            public int     TotalLinesSent    { get; set; }
    
    
            public string  LastSyncFile      { get; set; }
    
    
            public int  ApprovedTransactions { get; set; }
    
    
            public int  FailedTransactions   { get; set; }
    
    
            public int  CardsCaptured        { get; set; }
    
    
            public long CashDispensed        { get; set; }
    
    
            public string LastJournalFile   { get; set; }
    
    
            public string LastErrorCode     { get; set; }
    
    
            public string LastErrorMessage  { get; set; }
    
    
            public string LastTransaction   { get; set; }
    
    
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            public string  ClientVersion { get; set; }
    
    
            public string  OSVersion     { get; set; }
    
    
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status         = ATMStatus.Unknown;
                SyncState      = SyncStatus.Idle;
                ServerPort     = NetworkConfig.DEFAULT_PORT;
            }
    
    
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
    
            public string GetLegacySourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_SOURCE;
                    default:                        return string.Empty;
                }
            }
    
    
            public void SetSourcePath(string v) => _sourcePath = v;
    
    
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
    
            public string GetLegacyBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_BACKUP;
                    default:                        return string.Empty;
                }
            }
    
    
            public void SetBackupPath(string v) => _backupPath = v;
    
    
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5)  return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing)      return ATMCardState.Syncing;
                if (IsSupervisorMode)                                   return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply)  return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
    
    
            public Color GetCardColor()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),
                    ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),
                    ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),
                    ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),
                    ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),
                    ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),
                    ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),
                    ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),
                    ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),
                    _                                 => Color.Gray
                };
            }
    
    
            public string GetStatusLabel()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive      => "● متصل ونشط",
                    ATMCardState.ConnectedIdle        => "● متصل خامل",
                    ATMCardState.Syncing              => "⟳ يزامن",
                    ATMCardState.WaitingReply         => "◎ ينتظر رد",
                    ATMCardState.Supervisor           => "★ Supervisor",
                    ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                    ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                    ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                    ATMCardState.NeverConnected       => "○ لم يتصل",
                    _                                 => "?"
                };
            }
    
    
            public string GetStatusDescription()        => GetStatusLabel();
    
    
            public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
    
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
    
            public string GetStatusColor()
            {
                if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                if (!IsConnected)
                {
                    var elapsed = DateTime.Now - LastConnectionTime;
                    if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_OFFLINE;
                    if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_WARNING;
                    return ATMStatusColors.COLOR_OFFLINE;
                }
                if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                var idleElapsed = DateTime.Now - LastDataReceived;
                if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                    return ATMStatusColors.COLOR_IDLE;
                return ATMStatusColors.COLOR_ACTIVE;
            }
    
    
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
    
            public bool NeedsAlert()
            {
                if (!IsConnected) return true;
                return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
            }
    
    
            public void RecalculateHealthScore()
            {
                var score = 100;
                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                if (Latency_ms > 500) score -= 15;
                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                if (CpuUsagePercent    > 90) score -= 10;
                if (MemoryUsagePercent > 90) score -= 10;
                if (DiskUsagePercent   > 95) score -= 10;
                HealthScore = Math.Max(0, Math.Min(100, score));
            }
    
    
            public override string ToString() =>
                $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
    
            public class ATMInfo
            {
                // هوية
                public string ATM_ID        { get; set; }
                public string ATM_Name      { get; set; }
                public string ATM_Type      { get; set; }    // NCR / GRG / WN / DIEBOLD / HYOSUNG
                public string BranchName    { get; set; }
                public string Region        { get; set; }
                public string ATMId { get => ATM_ID; set => ATM_ID = value; }
                public string ATMName { get => ATM_Name; set => ATM_Name = value; }
                public string IPAddress { get => ServerIP; set => ServerIP = value; }
                public ATMType ATMType
                {
                    get
                    {
                        switch (AppConstants.NormalizeATMType(ATM_Type))
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                            case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                            case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
                            case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
                            case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
                            default:                        return ATMType.Other;
                        }
                    }
                    set
                    {
                        ATM_Type = value switch
                        {
                            ATMType.NCR            => AppConstants.ATM_TYPE_NCR,
                            ATMType.GRG            => AppConstants.ATM_TYPE_GRG,
                            ATMType.WN             => AppConstants.ATM_TYPE_WN,
                            ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                            ATMType.Hyosung        => AppConstants.ATM_TYPE_HY,
                            _                      => "OTHER"
                        };
                    }
                }
                public string Location { get => Region; set => Region = value; }
                public string BranchCode { get; set; }
    
                // شبكة
                public string ServerIP      { get; set; }
                public int    ServerPort    { get; set; } = 5656;
                public string NetworkType   { get; set; } = "LAN";
                public int    Latency_ms    { get; set; }
                public int Latency { get => Latency_ms; set => Latency_ms = value; }
    
                // حالة الاتصال
                public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
                public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
                public string           SessionId        { get; set; }
                public bool             IsSupervisorMode { get; set; }
                public bool             IsHostConnected  { get; set; }
    
                // طوابع زمنية UTC (T-11)
                public DateTime ConnectedAtUtc       { get; set; }
                public DateTime DisconnectedAtUtc    { get; set; }
                public DateTime LastHeartbeatUtc     { get; set; }
                public DateTime LastSyncUtc          { get; set; }
                public DateTime LastDataReceivedUtc  { get; set; }
                public DateTime LastCommandSentUtc   { get; set; }
                public DateTime LastConnectionTime { get => ConnectedAtUtc; set => ConnectedAtUtc = value; }
                public DateTime LastSyncTime { get => LastSyncUtc; set => LastSyncUtc = value; }
    
                // إحصاءات المزامنة
                public long   TotalSyncedBytes         { get; set; }
                public long   TotalTransactions        { get; set; }
                public int    ConsecutiveSyncFailures  { get; set; }
                public double SyncSuccessRate          { get; set; } = 100.0;
                public double ReceiveSpeedKBs          { get; set; }
                public long   JournalSizeToday         { get; set; }
                public double CpuUsagePercent          { get; set; }
                public double MemoryUsagePercent       { get; set; }
                public double DiskUsagePercent         { get; set; }
                public int    HealthScore              { get; set; } = 100;
                public int PendingJournalCount { get; set; }
                public double SuccessRate { get => SyncSuccessRate; set => SyncSuccessRate = value; }
                public long TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
                // إحصاءات العمليات
                public int  ApprovedTransactions  { get; set; }
                public int  FailedTransactions    { get; set; }
                public int  CardsCaptured         { get; set; }
                public long CashDispensed         { get; set; }
    
                // آخر جورنال / خطأ
                public string LastJournalFile   { get; set; }
                public string LastErrorCode     { get; set; }
                public string LastErrorMessage  { get; set; }
                public string LastTransaction   { get; set; }
                public string LastError { get => LastErrorMessage; set => LastErrorMessage = value; }
                public int TransactionCount { get => (int)Math.Min(int.MaxValue, TotalTransactions); set => TotalTransactions = value; }
                public bool IsSyncing { get => ConnectionStatus == ConnectionStatus.Syncing; set { if (value) ConnectionStatus = ConnectionStatus.Syncing; } }
    
                // مسارات
                private string _sourcePath, _backupPath;
                public string ClientVersion { get; set; }
                public string OSVersion     { get; set; }
    
                public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath)
                    ? _sourcePath
                    : AppConstants.GetDefaultSourcePath(ATM_Type);
    
                public void SetSourcePath(string v) => _sourcePath = v;
    
                public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                    : System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                        "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
    
                public void SetBackupPath(string v) => _backupPath = v;
    
                // ==========================================
                // حالة البطاقة ولونها
                // ==========================================
    
                public ATMCardState GetCardState()
                {
                    if (ConnectionStatus == ConnectionStatus.Disconnected)
                    {
                        if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                        var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                        if (mins > 10) return ATMCardState.CriticalOffline;
                        if (mins > 5)  return ATMCardState.WarningOffline;
                        return ATMCardState.RecentlyDisconnected;
                    }
                    if (ConnectionStatus == ConnectionStatus.Syncing)     return ATMCardState.Syncing;
                    if (IsSupervisorMode)                                 return ATMCardState.Supervisor;
                    if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                    var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                    return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                }
    
                public Color GetCardColor()
                {
                    return GetCardState() switch
                    {
                        ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),   // أخضر
                        ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),   // أصفر
                        ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),  // أزرق
                        ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),  // أزرق
                        ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),   // برتقالي
                        ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),   // أحمر
                        ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),   // أحمر
                        ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),  // رمادي
                        ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),   // رمادي داكن
                        _ => Color.Gray
                    };
                }
    
                public string GetStatusLabel()
                {
                    return GetCardState() switch
                    {
                        ATMCardState.ConnectedActive      => "● متصل ونشط",
                        ATMCardState.ConnectedIdle        => "● متصل خامل",
                        ATMCardState.Syncing              => "⟳ يزامن",
                        ATMCardState.WaitingReply         => "◎ ينتظر رد",
                        ATMCardState.Supervisor           => "★ Supervisor",
                        ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                        ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                        ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                        ATMCardState.NeverConnected       => "○ لم يتصل",
                        _ => "?"
                    };
                }
    
                public string GetStatusDescription() => GetStatusLabel();
    
                public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
                public string GetElapsed(DateTime utcRef)
                {
                    if (utcRef == DateTime.MinValue) return "---";
                    var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                    if (s < 60)   return $"{(int)s}ث";
                    if (s < 3600) return $"{(int)(s/60)}د";
                    return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
                }
    
                public void RecalculateHealthScore()
                {
                    var score = 100;
                    if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                    if (Latency_ms > 500) score -= 15;
                    if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                    if (CpuUsagePercent > 90) score -= 10;
                    if (MemoryUsagePercent > 90) score -= 10;
                    if (DiskUsagePercent > 95) score -= 10;
                    HealthScore = Math.Max(0, Math.Min(100, score));
                }
    
                public override string ToString() =>
                    $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
            }
    
    
            public class AlertPayload
            {
                public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
                public AlertSeverity Severity  { get; set; }
                public string        Title     { get; set; }
                public string        Message   { get; set; }
                public string        Source    { get; set; }
                public string        DedupeKey { get; set; }
                public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
                public bool          IsRead    { get; set; }
    
                public string Icon => Severity switch
                {
                    AlertSeverity.Emergency => "CRIT",
                    AlertSeverity.Critical  => "FAIL",
                    AlertSeverity.Warning   => "WARN",
                    _                       => "INFO"
                };
                public string SeverityIcon => Icon;
                public Color Color => Severity switch
                {
                    AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                    AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                    AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                    _                       => Color.FromArgb(0,   122, 255)
                };
            }
    
    
            public class JournalSyncRecord
            {
                public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
                public string           ATM_ID          { get; set; }
                public string           FileName        { get; set; }
                public long             FileSize        { get; set; }
                public long             FileOffset      { get; set; }
                public string           Checksum        { get; set; }
                public string           MD5Hash         { get; set; }
                public string           SHA256Hash      { get; set; }
                public JournalSyncState State           { get; set; }
                public int              ProgressPercent { get; set; }
                public int              RetryCount      { get; set; }
                public string           LocalPath       { get; set; }
                public string           ServerPath      { get; set; }
                public string           Message         { get; set; }
                public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
                public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
                public DateTime?        CompletedAtUtc  { get; set; }
    
                public string StateIcon => State switch
                {
                    JournalSyncState.Pending   => "PEND",
                    JournalSyncState.Syncing   => "SYNC",
                    JournalSyncState.ReSyncing => "RSYNC",
                    JournalSyncState.Completed => "OK",
                    JournalSyncState.Failed    => "FAIL",
                    JournalSyncState.Archived  => "ARCH",
                    _ => "?"
                };
    
                public string StateLabel => State switch
                {
                    JournalSyncState.Pending   => "في الطابور",
                    JournalSyncState.Syncing   => "قيد المزامنة",
                    JournalSyncState.ReSyncing => "إعادة مزامنة",
                    JournalSyncState.Completed => "محمّل",
                    JournalSyncState.Failed    => "فشل",
                    JournalSyncState.Archived  => "مؤرشف",
                    _ => "؟"
                };
    
                public string ATM_ID { get; set; }
                public string ATM_Name { get; set; }
                public string ATM_Description { get; set; }
                public string ATM_Type { get; set; }
                public string ServerIP { get; set; }
                public int ServerPort { get; set; }
                public ATMStatus Status { get; set; }
                public bool IsConnected { get; set; }
                public bool IsSendingData { get; set; }
                public bool IsCSCConnected { get; set; }
                public DateTime LastConnectionTime { get; set; }
                public DateTime LastDataReceived { get; set; }
                public DateTime LastHeartbeat { get; set; }
                public int[,] OperationStats { get; set; }
                public int ATMCache { get; set; }
                public int TotalDispensed { get; set; }
                public SyncStatus SyncState { get; set; }
                public long TotalBytesSent { get; set; }
                public int TotalFilesSynced { get; set; }
                public int TotalLinesSent { get; set; }
                public DateTime LastSyncTime { get; set; }
                public string LastSyncFile { get; set; }
                public string OSVersion { get; set; }
                public string ClientVersion { get; set; }
    
                public ATMInfo()
                {
                    OperationStats = new int[3, 4];
                    Status = ATMStatus.Unknown;
                    SyncState = SyncStatus.Idle;
                    ServerPort = NetworkConfig.DEFAULT_PORT;
                }
    
                public string GetStatusColor()
                {
                    if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                    if (!IsConnected)
                    {
                        var elapsed = DateTime.Now - LastConnectionTime;
                        if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                            return ATMStatusColors.COLOR_OFFLINE;
                        if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                            return ATMStatusColors.COLOR_WARNING;
                        return ATMStatusColors.COLOR_OFFLINE;
                    }
                    if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                    var idleElapsed = DateTime.Now - LastDataReceived;
                    if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                        return ATMStatusColors.COLOR_IDLE;
                    return ATMStatusColors.COLOR_ACTIVE;
                }
    
                public string GetSourcePath()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                        case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                        case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                        default: return string.Empty;
                    }
                }
    
                public string GetBackupPath()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                        case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                        case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                        default: return string.Empty;
                    }
                }
    
                public SyncStrategy GetSyncStrategy()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                        case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                        case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                        default: return SyncStrategy.NCR_Overwrite;
                    }
                }
    
                public bool NeedsAlert()
                {
                    if (!IsConnected) return true;
                    return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                }
            }
    
    
            public class ClientConfig
            {
                public string ATM_ID { get; set; }
                public string ATM_Name { get; set; }
                public string ATM_Type { get; set; }
                public string ServerIP { get; set; }
                public int ServerPort { get; set; }
                public bool SyncTimeEnabled { get; set; }
                public int MessageSizeLines { get; set; }
                public int FilePackageKB { get; set; }
                public string SourcePath { get; set; }
                public string BackupPath { get; set; }
                public bool AutoStart { get; set; }
                public bool RunAsService { get; set; }
    
                public ClientConfig()
                {
                    ServerPort = NetworkConfig.DEFAULT_PORT;
                    MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                    FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                    AutoStart = true;
                    RunAsService = true;
                }
            }
    
    
            public class ServerConfig
            {
                public int ListenPort { get; set; }
                public string StoragePath { get; set; }
                public string ArchivePath { get; set; }
                public bool AutoArchive { get; set; }
                public int MaxConnections { get; set; }
                public bool EnableEncryption { get; set; }
                public bool EnableCompression { get; set; }
    
                public ServerConfig()
                {
                    ListenPort = NetworkConfig.DEFAULT_PORT;
                    StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                    ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                    AutoArchive = true;
                    MaxConnections = 100;
                    EnableEncryption = true;
                    EnableCompression = true;
                }
            }
    
    
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
    
            public enum ATMStatus
            {
                Unknown = 0,
                Online = 1,
                Idle = 2,
                Supervisor = 3,
                Warning = 4,
                Offline = 5,
                Critical = 6,
                InService = 10,
                ConnectedOnly = 11,
                WaitingResponse = 12,
                OutOfService = 13,
                CriticalFault = 14,
                Fault = 15,
                Maintenance = 16
            }
    
    
            public enum ATMType { NCR, GRG, WN, DieboldNixdorf, Hyosung, Other }
    
    
            public enum SyncStatus { Pending, InProgress, Syncing, Resyncing, Completed, Failed }
    
    
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
    
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
    
            public enum ATMCardState      { NeverConnected, ConnectedActive, ConnectedIdle, Syncing, WaitingReply, Supervisor, RecentlyDisconnected, WarningOffline, CriticalOffline }
    
    
        }
    public partial class ATMInfo
        {
                public class ATMInfo
                {
                    // هوية
                    public string ATM_ID
                    {
                        get;
                        set;
                    }
                    public string ATM_Name
                    {
                        get;
                        set;
                    }
                    public string ATM_Type
                    {
                        get;
                        set;
                    } // NCR / GRG / WN / DIEBOLD / HYOSUNG
                    public string BranchName
                    {
                        get;
                        set;
                    }
                    public string Region
                    {
                        get;
                        set;
                    }
                    public string ATMId
                    {
                        get => ATM_ID;
                        set => ATM_ID = value;
                    }
                    public string ATMName
                    {
                        get => ATM_Name;
                        set => ATM_Name = value;
                    }
                    public string IPAddress
                    {
                        get => ServerIP;
                        set => ServerIP = value;
                    }
                    public ATMType ATMType
                    {
                        get
                        {
                            switch (AppConstants.NormalizeATMType(ATM_Type))
                            {
                                case AppConstants.ATM_TYPE_NCR:
                                    return ATMType.NCR;
                                case AppConstants.ATM_TYPE_GRG:
                                    return ATMType.GRG;
                                case AppConstants.ATM_TYPE_WN:
                                    return ATMType.WN;
                                case AppConstants.ATM_TYPE_DN:
                                    return ATMType.DieboldNixdorf;
                                case AppConstants.ATM_TYPE_HY:
                                    return ATMType.Hyosung;
                                default:
                                    return ATMType.Other;
                            }
                        }
                        set
                        {
                            ATM_Type = value
                            switch
                            {
                                ATMType.NCR => AppConstants.ATM_TYPE_NCR,
                                ATMType.GRG => AppConstants.ATM_TYPE_GRG,
                                ATMType.WN => AppConstants.ATM_TYPE_WN,
                                ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                                ATMType.Hyosung => AppConstants.ATM_TYPE_HY,
                                _ => "OTHER"
                            };
                        }
                    }
                    public string Location
                    {
                        get => Region;
                        set => Region = value;
                    }
                    public string BranchCode
                    {
                        get;
                        set;
                    }
    
                    // شبكة
                    public string ServerIP
                    {
                        get;
                        set;
                    }
                    public int ServerPort
                    {
                        get;
                        set;
                    } = 5656;
                    public string NetworkType
                    {
                        get;
                        set;
                    } = "LAN";
                    public int Latency_ms
                    {
                        get;
                        set;
                    }
                    public int Latency
                    {
                        get => Latency_ms;
                        set => Latency_ms = value;
                    }
    
                    // حالة الاتصال
                    public ConnectionStatus ConnectionStatus
                    {
                        get;
                        set;
                    } = ConnectionStatus.Disconnected;
                    public ATMStatus Status
                    {
                        get;
                        set;
                    } = ATMStatus.Unknown;
                    public string SessionId
                    {
                        get;
                        set;
                    }
                    public bool IsSupervisorMode
                    {
                        get;
                        set;
                    }
                    public bool IsHostConnected
                    {
                        get;
                        set;
                    }
    
                    // طوابع زمنية UTC (T-11)
                    public DateTime ConnectedAtUtc
                    {
                        get;
                        set;
                    }
                    public DateTime DisconnectedAtUtc
                    {
                        get;
                        set;
                    }
                    public DateTime LastHeartbeatUtc
                    {
                        get;
                        set;
                    }
                    public DateTime LastSyncUtc
                    {
                        get;
                        set;
                    }
                    public DateTime LastDataReceivedUtc
                    {
                        get;
                        set;
                    }
                    public DateTime LastCommandSentUtc
                    {
                        get;
                        set;
                    }
                    public DateTime LastConnectionTime
                    {
                        get => ConnectedAtUtc;
                        set => ConnectedAtUtc = value;
                    }
                    public DateTime LastSyncTime
                    {
                        get => LastSyncUtc;
                        set => LastSyncUtc = value;
                    }
    
                    // إحصاءات المزامنة
                    public long TotalSyncedBytes
                    {
                        get;
                        set;
                    }
                    public long TotalTransactions
                    {
                        get;
                        set;
                    }
                    public int ConsecutiveSyncFailures
                    {
                        get;
                        set;
                    }
                    public double SyncSuccessRate
                    {
                        get;
                        set;
                    } = 100.0;
                    public double ReceiveSpeedKBs
                    {
                        get;
                        set;
                    }
                    public long JournalSizeToday
                    {
                        get;
                        set;
                    }
                    public double CpuUsagePercent
                    {
                        get;
                        set;
                    }
                    public double MemoryUsagePercent
                    {
                        get;
                        set;
                    }
                    public double DiskUsagePercent
                    {
                        get;
                        set;
                    }
                    public int HealthScore
                    {
                        get;
                        set;
                    } = 100;
                    public int PendingJournalCount
                    {
                        get;
                        set;
                    }
                    public double SuccessRate
                    {
                        get => SyncSuccessRate;
                        set => SyncSuccessRate = value;
                    }
                    public long TotalTransactionsSynced
                    {
                        get => TotalTransactions;
                        set => TotalTransactions = value;
                    }
    
                    // إحصاءات العمليات
                    public int ApprovedTransactions
                    {
                        get;
                        set;
                    }
                    public int FailedTransactions
                    {
                        get;
                        set;
                    }
                    public int CardsCaptured
                    {
                        get;
                        set;
                    }
                    public long CashDispensed
                    {
                        get;
                        set;
                    }
    
                    // آخر جورنال / خطأ
                    public string LastJournalFile
                    {
                        get;
                        set;
                    }
                    public string LastErrorCode
                    {
                        get;
                        set;
                    }
                    public string LastErrorMessage
                    {
                        get;
                        set;
                    }
                    public string LastTransaction
                    {
                        get;
                        set;
                    }
                    public string LastError
                    {
                        get => LastErrorMessage;
                        set => LastErrorMessage = value;
                    }
                    public int TransactionCount
                    {
                        get => (int)Math.Min(int.MaxValue, TotalTransactions);
                        set => TotalTransactions = value;
                    }
                    public bool IsSyncing
                    {
                        get => ConnectionStatus == ConnectionStatus.Syncing;
                        set
                        {
                            if (value) ConnectionStatus = ConnectionStatus.Syncing;
                        }
                    }
    
                    // مسارات
                    private string _sourcePath, _backupPath;
                    public string ClientVersion
                    {
                        get;
                        set;
                    }
                    public string OSVersion
                    {
                        get;
                        set;
                    }
    
                    public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath) ?
                    _sourcePath :
                    AppConstants.GetDefaultSourcePath(ATM_Type);
    
                    public void SetSourcePath(string v) => _sourcePath = v;
    
                    public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath :
                    System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                        "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
    
                    public void SetBackupPath(string v) => _backupPath = v;
    
                    // ==========================================
                    // حالة البطاقة ولونها
                    // ==========================================
    
                    public ATMCardState GetCardState()
                    {
                        if (ConnectionStatus == ConnectionStatus.Disconnected)
                        {
                            if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                            var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                            if (mins > 10) return ATMCardState.CriticalOffline;
                            if (mins > 5) return ATMCardState.WarningOffline;
                            return ATMCardState.RecentlyDisconnected;
                        }
                        if (ConnectionStatus == ConnectionStatus.Syncing) return ATMCardState.Syncing;
                        if (IsSupervisorMode) return ATMCardState.Supervisor;
                        if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                        var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                        return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                    }
    
                    public Color GetCardColor()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive => Color.FromArgb(52, 199, 89), // أخضر
                            ATMCardState.ConnectedIdle => Color.FromArgb(255, 214, 10), // أصفر
                            ATMCardState.Syncing => Color.FromArgb(10, 132, 255), // أزرق
                            ATMCardState.WaitingReply => Color.FromArgb(10, 132, 255), // أزرق
                            ATMCardState.Supervisor => Color.FromArgb(255, 159, 10), // برتقالي
                            ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69, 58), // أحمر
                            ATMCardState.WarningOffline => Color.FromArgb(255, 69, 58), // أحمر
                            ATMCardState.CriticalOffline => Color.FromArgb(99, 99, 102), // رمادي
                            ATMCardState.NeverConnected => Color.FromArgb(72, 72, 74), // رمادي داكن
                            _ => Color.Gray
                        };
                    }
    
                    public string GetStatusLabel()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive => "● متصل ونشط",
                            ATMCardState.ConnectedIdle => "● متصل خامل",
                            ATMCardState.Syncing => "⟳ يزامن",
                            ATMCardState.WaitingReply => "◎ ينتظر رد",
                            ATMCardState.Supervisor => "★ Supervisor",
                            ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                            ATMCardState.WarningOffline => "✕ انقطاع >5د",
                            ATMCardState.CriticalOffline => "✕ انقطاع حرج",
                            ATMCardState.NeverConnected => "○ لم يتصل",
                            _ => "?"
                        };
                    }
    
                    public string GetStatusDescription() => GetStatusLabel();
    
                    public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
                    public string GetElapsed(DateTime utcRef)
                    {
                        if (utcRef == DateTime.MinValue) return "---";
                        var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                        if (s < 60) return $"{(int)s}ث";
                        if (s < 3600) return $"{(int)(s/60)}د";
                        return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
                    }
    
                    public void RecalculateHealthScore()
                    {
                        var score = 100;
                        if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                        if (Latency_ms > 500) score -= 15;
                        if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                        if (CpuUsagePercent > 90) score -= 10;
                        if (MemoryUsagePercent > 90) score -= 10;
                        if (DiskUsagePercent > 95) score -= 10;
                        HealthScore = Math.Max(0, Math.Min(100, score));
                    }
    
                    public override string ToString() =>
                    $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
            }
    
    
            public class AlertPayload
            {
                public string AlertId
                {
                    get;
                    set;
                } = Guid.NewGuid().ToString("N");
                public AlertSeverity Severity
                {
                    get;
                    set;
                }
                public string Title
                {
                    get;
                    set;
                }
                public string Message
                {
                    get;
                    set;
                }
                public string Source
                {
                    get;
                    set;
                }
                public string DedupeKey
                {
                    get;
                    set;
                }
                public DateTime CreatedAt
                {
                    get;
                    set;
                } = DateTime.UtcNow;
                public bool IsRead
                {
                    get;
                    set;
                }
    
                public string Icon => Severity
                switch
                {
                    AlertSeverity.Emergency => "CRIT",
                    AlertSeverity.Critical => "FAIL",
                    AlertSeverity.Warning => "WARN",
                    _ => "INFO"
                };
                public string SeverityIcon => Icon;
                public Color Color => Severity
                switch
                {
                    AlertSeverity.Emergency => Color.FromArgb(255, 59, 48),
                    AlertSeverity.Critical => Color.FromArgb(255, 59, 48),
                    AlertSeverity.Warning => Color.FromArgb(255, 149, 0),
                    _ => Color.FromArgb(0, 122, 255)
                };
            }
    
    
            public class JournalSyncRecord
            {
                public string SyncId
                {
                    get;
                    set;
                } = Guid.NewGuid().ToString("N");
                public string ATM_ID
                {
                    get;
                    set;
                }
                public string FileName
                {
                    get;
                    set;
                }
                public long FileSize
                {
                    get;
                    set;
                }
                public long FileOffset
                {
                    get;
                    set;
                }
                public string Checksum
                {
                    get;
                    set;
                }
                public string MD5Hash
                {
                    get;
                    set;
                }
                public string SHA256Hash
                {
                    get;
                    set;
                }
                public JournalSyncState State
                {
                    get;
                    set;
                }
                public int ProgressPercent
                {
                    get;
                    set;
                }
                public int RetryCount
                {
                    get;
                    set;
                }
                public string LocalPath
                {
                    get;
                    set;
                }
                public string ServerPath
                {
                    get;
                    set;
                }
                public string Message
                {
                    get;
                    set;
                }
                public DateTime CreatedAtUtc
                {
                    get;
                    set;
                } = DateTime.UtcNow;
                public DateTime UpdatedAtUtc
                {
                    get;
                    set;
                } = DateTime.UtcNow;
                public DateTime? CompletedAtUtc
                {
                    get;
                    set;
                }
    
                public string StateIcon => State
                switch
                {
                    JournalSyncState.Pending => "PEND",
                    JournalSyncState.Syncing => "SYNC",
                    JournalSyncState.ReSyncing => "RSYNC",
                    JournalSyncState.Completed => "OK",
                    JournalSyncState.Failed => "FAIL",
                    JournalSyncState.Archived => "ARCH",
                    _ => "?"
                };
    
                public string StateLabel => State
                switch
                {
                    JournalSyncState.Pending => "في الطابور",
                    JournalSyncState.Syncing => "قيد المزامنة",
                    JournalSyncState.ReSyncing => "إعادة مزامنة",
                    JournalSyncState.Completed => "محمّل",
                    JournalSyncState.Failed => "فشل",
                    JournalSyncState.Archived => "مؤرشف",
                    _ => "؟"
                };
    
                public string ATM_ID
                {
                    get;
                    set;
                }
                public string ATM_Name
                {
                    get;
                    set;
                }
                public string ATM_Description
                {
                    get;
                    set;
                }
                public string ATM_Type
                {
                    get;
                    set;
                }
                public string ServerIP
                {
                    get;
                    set;
                }
                public int ServerPort
                {
                    get;
                    set;
                }
                public ATMStatus Status
                {
                    get;
                    set;
                }
                public bool IsConnected
                {
                    get;
                    set;
                }
                public bool IsSendingData
                {
                    get;
                    set;
                }
                public bool IsCSCConnected
                {
                    get;
                    set;
                }
                public DateTime LastConnectionTime
                {
                    get;
                    set;
                }
                public DateTime LastDataReceived
                {
                    get;
                    set;
                }
                public DateTime LastHeartbeat
                {
                    get;
                    set;
                }
                public int[,] OperationStats
                {
                    get;
                    set;
                }
                public int ATMCache
                {
                    get;
                    set;
                }
                public int TotalDispensed
                {
                    get;
                    set;
                }
                public SyncStatus SyncState
                {
                    get;
                    set;
                }
                public long TotalBytesSent
                {
                    get;
                    set;
                }
                public int TotalFilesSynced
                {
                    get;
                    set;
                }
                public int TotalLinesSent
                {
                    get;
                    set;
                }
                public DateTime LastSyncTime
                {
                    get;
                    set;
                }
                public string LastSyncFile
                {
                    get;
                    set;
                }
                public string OSVersion
                {
                    get;
                    set;
                }
                public string ClientVersion
                {
                    get;
                    set;
                }
    
                public ATMInfo()
                {
                    OperationStats = new int[3, 4];
                    Status = ATMStatus.Unknown;
                    SyncState = SyncStatus.Idle;
                    ServerPort = NetworkConfig.DEFAULT_PORT;
                }
    
                public string GetStatusColor()
                {
                    if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                    if (!IsConnected)
                    {
                        var elapsed = DateTime.Now - LastConnectionTime;
                        if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                            return ATMStatusColors.COLOR_OFFLINE;
                        if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                            return ATMStatusColors.COLOR_WARNING;
                        return ATMStatusColors.COLOR_OFFLINE;
                    }
                    if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                    var idleElapsed = DateTime.Now - LastDataReceived;
                    if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                        return ATMStatusColors.COLOR_IDLE;
                    return ATMStatusColors.COLOR_ACTIVE;
                }
    
                public string GetSourcePath()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR:
                            return ATMPaths.NCR_SOURCE;
                        case AppConstants.ATM_TYPE_GRG:
                            return ATMPaths.GRG_SOURCE;
                        case AppConstants.ATM_TYPE_WN:
                            return ATMPaths.WN_SOURCE;
                        default:
                            return string.Empty;
                    }
                }
    
                public string GetBackupPath()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR:
                            return ATMPaths.NCR_BACKUP;
                        case AppConstants.ATM_TYPE_GRG:
                            return ATMPaths.GRG_BACKUP;
                        case AppConstants.ATM_TYPE_WN:
                            return ATMPaths.WN_BACKUP;
                        default:
                            return string.Empty;
                    }
                }
    
                public SyncStrategy GetSyncStrategy()
                {
                    switch (ATM_Type)
                    {
                        case AppConstants.ATM_TYPE_NCR:
                            return SyncStrategy.NCR_Overwrite;
                        case AppConstants.ATM_TYPE_GRG:
                            return SyncStrategy.GRG_DailyFiles;
                        case AppConstants.ATM_TYPE_WN:
                            return SyncStrategy.WN_DailyFiles;
                        default:
                            return SyncStrategy.NCR_Overwrite;
                    }
                }
    
                public bool NeedsAlert()
                {
                    if (!IsConnected) return true;
                    return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                }
            }
    
    
            public class ClientConfig
            {
                public string ATM_ID
                {
                    get;
                    set;
                }
                public string ATM_Name
                {
                    get;
                    set;
                }
                public string ATM_Type
                {
                    get;
                    set;
                }
                public string ServerIP
                {
                    get;
                    set;
                }
                public int ServerPort
                {
                    get;
                    set;
                }
                public bool SyncTimeEnabled
                {
                    get;
                    set;
                }
                public int MessageSizeLines
                {
                    get;
                    set;
                }
                public int FilePackageKB
                {
                    get;
                    set;
                }
                public string SourcePath
                {
                    get;
                    set;
                }
                public string BackupPath
                {
                    get;
                    set;
                }
                public bool AutoStart
                {
                    get;
                    set;
                }
                public bool RunAsService
                {
                    get;
                    set;
                }
    
                public ClientConfig()
                {
                    ServerPort = NetworkConfig.DEFAULT_PORT;
                    MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                    FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                    AutoStart = true;
                    RunAsService = true;
                }
            }
    
    
            public class ServerConfig
            {
                public int ListenPort
                {
                    get;
                    set;
                }
                public string StoragePath
                {
                    get;
                    set;
                }
                public string ArchivePath
                {
                    get;
                    set;
                }
                public bool AutoArchive
                {
                    get;
                    set;
                }
                public int MaxConnections
                {
                    get;
                    set;
                }
                public bool EnableEncryption
                {
                    get;
                    set;
                }
                public bool EnableCompression
                {
                    get;
                    set;
                }
    
                public ServerConfig()
                {
                    ListenPort = NetworkConfig.DEFAULT_PORT;
                    StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                    ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                    AutoArchive = true;
                    MaxConnections = 100;
                    EnableEncryption = true;
                    EnableCompression = true;
                }
            }
    
    
            public enum ConnectionStatus
            {
                Disconnected = 0, Connecting = 1, Connected = 2, WaitingReply = 3, Syncing = 4
            }
    
    
            public enum ATMStatus
            {
                Unknown = 0,
                Online = 1,
                Idle = 2,
                Supervisor = 3,
                Warning = 4,
                Offline = 5,
                Critical = 6,
                InService = 10,
                ConnectedOnly = 11,
                WaitingResponse = 12,
                OutOfService = 13,
                CriticalFault = 14,
                Fault = 15,
                Maintenance = 16
            }
    
    
            public enum ATMType
            {
                NCR,
                GRG,
                WN,
                DieboldNixdorf,
                Hyosung,
                Other
            }
    
    
            public enum SyncStatus
            {
                Pending,
                InProgress,
                Syncing,
                Resyncing,
                Completed,
                Failed
            }
    
    
            public enum AlertSeverity
            {
                Info = 0, Warning = 1, Critical = 2, Emergency = 3
            }
    
    
            public enum JournalSyncState
            {
                Pending = 0, Syncing = 1, ReSyncing = 2, Completed = 3, Failed = 4, Archived = 5
            }
    
    
            public enum ATMCardState
            {
                NeverConnected,
                ConnectedActive,
                ConnectedIdle,
                Syncing,
                WaitingReply,
                Supervisor,
                RecentlyDisconnected,
                WarningOffline,
                CriticalOffline
            }
    
    
        }
    // ==========================================
        // ATMInfo — نموذج الصراف الشامل
        // ==========================================
    
        public class ATMInfo
        {
            // هوية
            public string ATMId { get; set; }
    
            // Aliases for code that uses ATM_ID / ATM_Name / ServerIP / LastHeartbeatUtc
            public string ATM_ID { get => ATMId; set => ATMId = value; }
            public string ATM_Name { get => Name; set => Name = value; }
            public string ServerIP { get => IPAddress; set => IPAddress = value; }
            public DateTime LastHeartbeatUtc { get => LastHeartbeat; set => LastHeartbeat = value; }
            public long TotalSyncedBytes { get; set; }
            public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;
    
            public string Name { get; set; } = string.Empty;
            public string ATM_Type { get; set; } = AppConstants.ATM_TYPE_NCR;
            public string BranchName { get; set; } = string.Empty;
            public string Region { get; set; } = string.Empty;
            public string Location { get => Region; set => Region = value; }
            public string BranchCode { get; set; } = string.Empty;
    
            // شبكة
            public string IPAddress { get; set; }
            public int ServerPort { get; set; } = 5656;
            public string NetworkType { get; set; } = "LAN";
            public int Latency { get; set; }
            public int Latency_ms { get => Latency; set => Latency = value; }
            public bool IsSupervisorMode { get; set; }
            public string GetElapsed(DateTime? dt) => !dt.HasValue ? "—" : $"{(DateTime.UtcNow - dt.Value).TotalMinutes:N0}m";
    
            // حالة الاتصال
            public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
            public ATMStatus Status { get; set; } = ATMStatus.Unknown;
            public string SessionId { get; set; }
            public bool IsHostConnected { get; set; }
    
            // طوابع زمنية UTC
            public DateTime ConnectedAtUtc { get; set; }
            public DateTime DisconnectedAtUtc { get; set; }
            public DateTime LastHeartbeat { get; set; }
            public DateTime LastSyncUtc { get; set; }
            public DateTime LastDataReceivedUtc { get; set; }
            public DateTime LastCommandSentUtc { get; set; }
    
            // إحصاءات المزامنة
            public long TotalBytesSent { get; set; }
            public long TotalTransactions { get; set; }
            public int ConsecutiveSyncFailures { get; set; }
            public double SyncSuccessRate { get; set; } = 100.0;
            public double ReceiveSpeedKBs { get; set; }
            public long JournalSizeToday { get; set; }
            public double CpuUsagePercent { get; set; }
            public double MemoryUsagePercent { get; set; }
            public double DiskUsagePercent { get; set; }
            public int HealthScore { get; set; } = 100;
            public int PendingJournalCount { get; set; }
            public string ATMName { get => Name; set => Name = value; }
            public DateTime LastConnectionTime { get; set; }
            public DateTime LastSyncTime { get; set; }
            public double SuccessRate { get => SyncSuccessRate; set => SyncSuccessRate = value; }
            public int TotalTransactionsSynced { get => (int)TotalTransactions; set => TotalTransactions = value; }
            public string GetStatusDescription() => Status.ToString();
            public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
            // إحصاءات العمليات
            public int ApprovedTransactions { get; set; }
            public int FailedTransactions { get; set; }
            public int CardsCaptured { get; set; }
            public long CashDispensed { get; set; }
    
            // آخر جورنال / خطأ
            public string LastJournalFile { get; set; }
            public string LastErrorCode { get; set; }
            public string LastError { get; set; }
            public string LastErrorMessage { get => LastError; set => LastError = value; }
            public bool HasCashTelemetry { get; set; }
            public long Cassette1Remaining { get; set; }
            public long Cassette2Remaining { get; set; }
            public long Cassette3Remaining { get; set; }
            public long Cassette4Remaining { get; set; }
            public long ATMCache { get; set; }
            public long CashLoadedTotal { get; set; }
            public long TotalDispensed { get; set; }
            public long CashDepositInTotal { get; set; }
            public long CashRejectCount { get; set; }
            public long CashRetractCount { get; set; }
            public DateTime CashTelemetryUpdatedAtUtc { get; set; }
            public bool NeedsAlert() => HasAlerts;
            public string LastTransaction { get; set; }
    
            // تنبيهات
            public bool HasAlerts { get; set; }
    
            // مسارات
            private string _sourcePath;
            private string _backupPath;
    
            public string ClientVersion { get; set; }
            public string OSVersion { get; set; }
    
            public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath)
                ? _sourcePath
                : AppConstants.GetDefaultSourcePath(ATM_Type);
    
            public void SetSourcePath(string v) => _sourcePath = v;
    
            public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                : System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATMId ?? "DEFAULT");
    
            public void SetBackupPath(string v) => _backupPath = v;
    
            public ATMType ATMType
            {
                get
                {
                    switch (AppConstants.NormalizeATMType(ATM_Type))
                    {
                        case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                        case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                        case AppConstants.ATM_TYPE_WN: return ATMType.WN;
                        case AppConstants.ATM_TYPE_DN: return ATMType.DieboldNixdorf;
                        case AppConstants.ATM_TYPE_HY: return ATMType.Hyosung;
                        default: return ATMType.Other;
                    }
                }
                set
                {
                    ATM_Type = value switch
                    {
                        ATMType.NCR => AppConstants.ATM_TYPE_NCR,
                        ATMType.GRG => AppConstants.ATM_TYPE_GRG,
                        ATMType.WN => AppConstants.ATM_TYPE_WN,
                        ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                        ATMType.Hyosung => AppConstants.ATM_TYPE_HY,
                        _ => "OTHER"
                    };
                }
            }
    
            // ==========================================
            // حالة البطاقة ولونها
            // ==========================================
    
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeat == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeat).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5) return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing) return ATMCardState.Syncing;
                if (IsSupervisorMode) return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
    
            public Color GetCardColor()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive => Color.FromArgb(52, 199, 89),
                    ATMCardState.ConnectedIdle => Color.FromArgb(255, 214, 10),
                    ATMCardState.Syncing => Color.FromArgb(10, 132, 255),
                    ATMCardState.WaitingReply => Color.FromArgb(10, 132, 255),
                    ATMCardState.Supervisor => Color.FromArgb(255, 159, 10),
                    ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69, 58),
                    ATMCardState.WarningOffline => Color.FromArgb(255, 69, 58),
                    ATMCardState.CriticalOffline => Color.FromArgb(99, 99, 102),
                    ATMCardState.NeverConnected => Color.FromArgb(72, 72, 74),
                    _ => Color.Gray
                };
            }
    
            public string GetStatusLabel()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive => "● متصل ونشط",
                    ATMCardState.ConnectedIdle => "● متصل خامل",
                    ATMCardState.Syncing => "⟳ يزامن",
                    ATMCardState.WaitingReply => "◎ ينتظر رد",
                    ATMCardState.Supervisor => "★ Supervisor",
                    ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                    ATMCardState.WarningOffline => "✕ انقطاع >5د",
                    ATMCardState.CriticalOffline => "✕ انقطاع حرج",
                    ATMCardState.NeverConnected => "○ لم يتصل",
                    _ => "?"
                };
            }
    
            public void RecalculateHealthScore()
            {
                var score = 100;
                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                if (Latency > 500) score -= 15;
                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                if (CpuUsagePercent > 90) score -= 10;
                if (MemoryUsagePercent > 90) score -= 10;
                if (DiskUsagePercent > 95) score -= 10;
                HealthScore = Math.Max(0, Math.Min(100, score));
            }
    
            public override string ToString() =>
                $"[{ATMId}] {Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
            public ATMInfo()
            {
                ATMId = string.Empty;
                IPAddress = string.Empty;
                ServerPort = AppConstants.DefaultPort;
            }
        }
    public class ATMInfo
        {
            public string ATMId { get; set; }
            public string ATMName { get; set; }
            public string ATMType { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string Branch { get; set; }
            public string Region { get; set; }
            public string NetworkType { get; set; }
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }
            public string SourceJournalPath { get; set; }
            public string BackupPath { get; set; }
            public string ImageInboxPath { get; set; }
            public string ImageDestPath { get; set; }
            public bool IsConnected { get; set; }
            public DateTime LastHeartbeat { get; set; }
            public DateTime LastSync { get; set; }
        }
    public class ATMInfo
        {
            public string ATM_ID { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Description { get; set; }
            public string ATM_Type { get; set; }
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }
            public ATMStatus Status { get; set; }
            public bool IsConnected { get; set; }
            public bool IsSendingData { get; set; }
            public bool IsCSCConnected { get; set; }
            public DateTime LastConnectionTime { get; set; }
            public DateTime LastDataReceived { get; set; }
            public DateTime LastHeartbeat { get; set; }
            public int[,] OperationStats { get; set; }
            public int ATMCache { get; set; }
            public int TotalDispensed { get; set; }
            public SyncStatus SyncState { get; set; }
            public long TotalBytesSent { get; set; }
            public int TotalFilesSynced { get; set; }
            public int TotalLinesSent { get; set; }
            public DateTime LastSyncTime { get; set; }
            public string LastSyncFile { get; set; }
            public string OSVersion { get; set; }
            public string ClientVersion { get; set; }
    
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status = ATMStatus.Unknown;
                SyncState = SyncStatus.Idle;
                ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
            public string GetStatusColor()
            {
                if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                if (!IsConnected)
                {
                    var elapsed = DateTime.Now - LastConnectionTime;
                    if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_OFFLINE;
                    if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_WARNING;
                    return ATMStatusColors.COLOR_OFFLINE;
                }
                if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                var idleElapsed = DateTime.Now - LastDataReceived;
                if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                    return ATMStatusColors.COLOR_IDLE;
                return ATMStatusColors.COLOR_ACTIVE;
            }
    
            public string GetSourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                    default: return string.Empty;
                }
            }
    
            public string GetBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                    default: return string.Empty;
                }
            }
    
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                    default: return SyncStrategy.NCR_Overwrite;
                }
            }
    
            public bool NeedsAlert()
            {
                if (!IsConnected) return true;
                return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
            }
        }
    // ==========================================
        // ATMInfo — نموذج الصراف الشامل
        // ==========================================
    
        public class ATMInfo
        {
            // هوية
            public string ATM_ID        { get; set; }
            public string ATM_Name      { get; set; }
            public string ATM_Type      { get; set; }    // NCR / GRG / WN
            public string BranchName    { get; set; }
            public string Region        { get; set; }
    
            // شبكة
            public string ServerIP      { get; set; }
            public int    ServerPort    { get; set; } = 5656;
            public string NetworkType   { get; set; } = "LAN";
            public int    Latency_ms    { get; set; }
    
            // حالة الاتصال
            public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
            public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
            public string           SessionId        { get; set; }
            public bool             IsSupervisorMode { get; set; }
            public bool             IsHostConnected  { get; set; }
    
            // طوابع زمنية UTC (T-11)
            public DateTime ConnectedAtUtc       { get; set; }
            public DateTime DisconnectedAtUtc    { get; set; }
            public DateTime LastHeartbeatUtc     { get; set; }
            public DateTime LastSyncUtc          { get; set; }
            public DateTime LastDataReceivedUtc  { get; set; }
            public DateTime LastCommandSentUtc   { get; set; }
    
            // إحصاءات المزامنة
            public long   TotalSyncedBytes         { get; set; }
            public long   TotalTransactions        { get; set; }
            public int    ConsecutiveSyncFailures  { get; set; }
            public double SyncSuccessRate          { get; set; } = 100.0;
            public double ReceiveSpeedKBs          { get; set; }
            public long   JournalSizeToday         { get; set; }
    
            // إحصاءات العمليات
            public int  ApprovedTransactions  { get; set; }
            public int  FailedTransactions    { get; set; }
            public int  CardsCaptured         { get; set; }
            public long CashDispensed         { get; set; }
    
            // آخر جورنال / خطأ
            public string LastJournalFile   { get; set; }
            public string LastErrorCode     { get; set; }
            public string LastErrorMessage  { get; set; }
            public string LastTransaction   { get; set; }
    
            // مسارات
            private string _sourcePath, _backupPath;
            public string ClientVersion { get; set; }
            public string OSVersion     { get; set; }
    
            public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath) ? _sourcePath
                : ATM_Type == "NCR" ? @"C:\NCRJournal\"
                : ATM_Type == "GRG" ? @"D:\GRGData\EJ\"
                : ATM_Type == "WN"  ? @"C:\WOSA\EJ\"
                : @"C:\Journal\";
    
            public void SetSourcePath(string v) => _sourcePath = v;
    
            public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                : System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
    
            public void SetBackupPath(string v) => _backupPath = v;
    
            // ==========================================
            // حالة البطاقة ولونها
            // ==========================================
    
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5)  return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing)     return ATMCardState.Syncing;
                if (IsSupervisorMode)                                 return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
    
            public Color GetCardColor()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),   // أخضر
                    ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),   // أصفر
                    ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),  // أزرق
                    ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),  // أزرق
                    ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),   // برتقالي
                    ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),   // أحمر
                    ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),   // أحمر
                    ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),  // رمادي
                    ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),   // رمادي داكن
                    _ => Color.Gray
                };
            }
    
            public string GetStatusLabel()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive      => "● متصل ونشط",
                    ATMCardState.ConnectedIdle        => "● متصل خامل",
                    ATMCardState.Syncing              => "⟳ يزامن",
                    ATMCardState.WaitingReply         => "◎ ينتظر رد",
                    ATMCardState.Supervisor           => "★ Supervisor",
                    ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                    ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                    ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                    ATMCardState.NeverConnected       => "○ لم يتصل",
                    _ => "?"
                };
            }
    
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s/60)}د";
                return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
            }
    
            public override string ToString() =>
                $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
        }
    public partial class ClientConfig
        {
            public string ATM_ID { get; set; }
    
    
            public string ATM_Name { get; set; }
    
    
            public string ATM_Type { get; set; }
    
    
            public string ServerIP { get; set; }
    
    
            public int ServerPort { get; set; }
    
    
            public bool SyncTimeEnabled { get; set; }
    
    
            public int MessageSizeLines { get; set; }
    
    
            public int FilePackageKB { get; set; }
    
    
            public string SourcePath { get; set; }
    
    
            public string BackupPath { get; set; }
    
    
            public bool AutoStart { get; set; }
    
    
            public bool RunAsService { get; set; }
    
    
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public ClientConfig()
            {
                ServerPort = AppConstants.DefaultPort;
                MessageSizeLines = 50;
                FilePackageKB = 512;
                AutoStart = true;
                RunAsService = true;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
            public ClientConfig()
            {
                ServerPort = AppConstants.DefaultPort;
                MessageSizeLines = 50;
                FilePackageKB = 512;
                AutoStart = true;
                RunAsService = true;
            }
    
    
        }
    public partial class ClientConfig
        {
            public string ATM_ID { get; set; }
    
    
            public string ATM_Name { get; set; }
    
    
            public string ATM_Type { get; set; }
    
    
            public string ServerIP { get; set; }
    
    
            public int ServerPort { get; set; }
    
    
            public bool SyncTimeEnabled { get; set; }
    
    
            public int MessageSizeLines { get; set; }
    
    
            public int FilePackageKB { get; set; }
    
    
            public string SourcePath { get; set; }
    
    
            public string BackupPath { get; set; }
    
    
            public bool AutoStart { get; set; }
    
    
            public bool RunAsService { get; set; }
    
    
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
            }
    
    
        }
    public partial class ClientConfig
        {
            public string ATM_ID { get; set; }
    
    
            public string ATM_Name { get; set; }
    
    
            public string ATM_Type { get; set; }
    
    
            public string ServerIP { get; set; }
    
    
            public int ServerPort { get; set; }
    
    
            public bool SyncTimeEnabled { get; set; }
    
    
            public int MessageSizeLines { get; set; }
    
    
            public int FilePackageKB { get; set; }
    
    
            public string SourcePath { get; set; }
    
    
            public string BackupPath { get; set; }
    
    
            public bool AutoStart { get; set; }
    
    
            public bool RunAsService { get; set; }
    
    
            public string NetworkQuality { get; set; }
    
    
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
                NetworkQuality = "LAN";
            }
    
    
        }
    // ==========================================
        // تكوين العميل
        // ==========================================
    
        public class ClientConfig
        {
            public string ATM_ID { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }
            public bool SyncTimeEnabled { get; set; }
            public int MessageSizeLines { get; set; }
            public int FilePackageKB { get; set; }
            public string SourcePath { get; set; }
            public string BackupPath { get; set; }
            public bool AutoStart { get; set; }
            public bool RunAsService { get; set; }
    
            public ClientConfig()
            {
                ServerPort = AppConstants.DefaultPort;
                MessageSizeLines = 50;
                FilePackageKB = 512;
                AutoStart = true;
                RunAsService = true;
            }
        }
    // ==========================================
        // إعدادات العميل
        // ==========================================
    
        public class ClientConfig
        {
            public string ATM_ID         { get; set; }
            public string ATM_Name       { get; set; }
            public string ATM_Type       { get; set; }
            public string ServerIP       { get; set; }
            public int    ServerPort     { get; set; }
            public bool   SyncTimeEnabled { get; set; }
            public int    MessageSizeLines { get; set; }
            public int    FilePackageKB  { get; set; }
            public string SourcePath     { get; set; }
            public string BackupPath     { get; set; }
            public bool   AutoStart      { get; set; }
            public bool   RunAsService   { get; set; }
    
            public ClientConfig()
            {
                ServerPort     = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB  = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart      = true;
                RunAsService   = true;
            }
        }
    public class ClientConfig
        {
            public string ATM_ID { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }
            public bool SyncTimeEnabled { get; set; }
            public int MessageSizeLines { get; set; }
            public int FilePackageKB { get; set; }
            public string SourcePath { get; set; }
            public string BackupPath { get; set; }
            public bool AutoStart { get; set; }
            public bool RunAsService { get; set; }
    
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
            }
        }
    public class ClientConfig
        {
            public string ATM_ID { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }
            public bool SyncTimeEnabled { get; set; }
            public int MessageSizeLines { get; set; }
            public int FilePackageKB { get; set; }
            public string SourcePath { get; set; }
            public string BackupPath { get; set; }
            public bool AutoStart { get; set; }
            public bool RunAsService { get; set; }
            public string NetworkQuality { get; set; }
    
            public ClientConfig()
            {
                ServerPort = NetworkConfig.DEFAULT_PORT;
                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                AutoStart = true;
                RunAsService = true;
                NetworkQuality = "LAN";
            }
        }
    public enum ConnectionStatus
        {
    
        public enum ATMStatus
        {
    
        public enum ATMType
        {
    
        public enum SyncStatus
        {
    
        public enum AlertSeverity
        {
    
        public enum JournalSyncState
        {
    
        public enum ATMCardState
        {
    
        public partial public public class ATMInfo
        {
            public string Icon => Severity
            switch
            {
            AlertSeverity.Emergency => "CRIT",
            AlertSeverity.Critical => "FAIL",
            AlertSeverity.Warning => "WARN",
            _ => "INFO"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity
            switch
            {
            AlertSeverity.Emergency => Color.FromArgb(255, 59, 48),
            AlertSeverity.Critical => Color.FromArgb(255, 59, 48),
            AlertSeverity.Warning => Color.FromArgb(255, 149, 0),
            _ => Color.FromArgb(0, 122, 255)
            };
            public string StateIcon => State
            switch
            {
            JournalSyncState.Pending => "PEND",
            JournalSyncState.Syncing => "SYNC",
            JournalSyncState.ReSyncing => "RSYNC",
            JournalSyncState.Completed => "OK",
            JournalSyncState.Failed => "FAIL",
            JournalSyncState.Archived => "ARCH",
            _ => "?"
            };
            public string StateLabel => State
            switch
            {
            JournalSyncState.Pending => "في الطابور",
            JournalSyncState.Syncing => "قيد المزامنة",
            JournalSyncState.ReSyncing => "إعادة مزامنة",
            JournalSyncState.Completed => "محمّل",
            JournalSyncState.Failed => "فشل",
            JournalSyncState.Archived => "مؤرشف",
            _ => "؟"
            };
            public ATMInfo()
            {
            public class ATMInfo
            {
            // هوية
            public string ATM_ID
            {
            get;
            set;
            }
            public string ATM_Name
            {
            get;
            set;
            }
            public string ATM_Type
            {
            get;
            set;
            }
            public string BranchName
            {
            get;
            set;
            }
            public string Region
            {
            get;
            set;
            }
            public string ATMId
            {
            get => ATM_ID;
            set => ATM_ID = value;
            }
            public string ATMName
            {
            get => ATM_Name;
            set => ATM_Name = value;
            }
            public string IPAddress
            {
            get => ServerIP;
            set => ServerIP = value;
            }
            public ATMType ATMType
            {
            get
            {
            switch (AppConstants.NormalizeATMType(ATM_Type))
            {
            case AppConstants.ATM_TYPE_NCR:
            return ATMType.NCR;
            case AppConstants.ATM_TYPE_GRG:
            return ATMType.GRG;
            case AppConstants.ATM_TYPE_WN:
            return ATMType.WN;
            case AppConstants.ATM_TYPE_DN:
            return ATMType.DieboldNixdorf;
            case AppConstants.ATM_TYPE_HY:
            return ATMType.Hyosung;
            default:
            return ATMType.Other;
            }
            public string Location
            {
            get => Region;
            set => Region = value;
            }
            public string BranchCode
            {
            get;
            set;
            }
            public string ServerIP
            {
            get;
            set;
            }
            public int ServerPort
            {
            get;
            set;
            }
            public string NetworkType
            {
            get;
            set;
            }
            public int Latency_ms
            {
            get;
            set;
            }
            public int Latency
            {
            get => Latency_ms;
            set => Latency_ms = value;
            }
            public ConnectionStatus ConnectionStatus
            {
            get;
            set;
            }
            public ATMStatus Status
            {
            get;
            set;
            }
            public string SessionId
            {
            get;
            set;
            }
            public bool IsSupervisorMode
            {
            get;
            set;
            }
            public bool IsHostConnected
            {
            get;
            set;
            }
            public DateTime ConnectedAtUtc
            {
            get;
            set;
            }
            public DateTime DisconnectedAtUtc
            {
            get;
            set;
            }
            public DateTime LastHeartbeatUtc
            {
            get;
            set;
            }
            public DateTime LastSyncUtc
            {
            get;
            set;
            }
            public DateTime LastDataReceivedUtc
            {
            get;
            set;
            }
            public DateTime LastCommandSentUtc
            {
            get;
            set;
            }
            public DateTime LastConnectionTime
            {
            get => ConnectedAtUtc;
            set => ConnectedAtUtc = value;
            }
            public DateTime LastSyncTime
            {
            get => LastSyncUtc;
            set => LastSyncUtc = value;
            }
            public long TotalSyncedBytes
            {
            get;
            set;
            }
            public long TotalTransactions
            {
            get;
            set;
            }
            public int ConsecutiveSyncFailures
            {
            get;
            set;
            }
            public double SyncSuccessRate
            {
            get;
            set;
            }
            public double ReceiveSpeedKBs
            {
            get;
            set;
            }
            public long JournalSizeToday
            {
            get;
            set;
            }
            public double CpuUsagePercent
            {
            get;
            set;
            }
            public double MemoryUsagePercent
            {
            get;
            set;
            }
            public double DiskUsagePercent
            {
            get;
            set;
            }
            public int HealthScore
            {
            get;
            set;
            }
            public int PendingJournalCount
            {
            get;
            set;
            }
            public double SuccessRate
            {
            get => SyncSuccessRate;
            set => SyncSuccessRate = value;
            }
            public long TotalTransactionsSynced
            {
            get => TotalTransactions;
            set => TotalTransactions = value;
            }
            public int ApprovedTransactions
            {
            get;
            set;
            }
            public int FailedTransactions
            {
            get;
            set;
            }
            public int CardsCaptured
            {
            get;
            set;
            }
            public long CashDispensed
            {
            get;
            set;
            }
            public string LastJournalFile
            {
            get;
            set;
            }
            public string LastErrorCode
            {
            get;
            set;
            }
            public string LastErrorMessage
            {
            get;
            set;
            }
            public string LastTransaction
            {
            get;
            set;
            }
            public string LastError
            {
            get => LastErrorMessage;
            set => LastErrorMessage = value;
            }
            public int TransactionCount
            {
            get => (int)Math.Min(int.MaxValue, TotalTransactions);
            set => TotalTransactions = value;
            }
            public bool IsSyncing
            {
            get => ConnectionStatus == ConnectionStatus.Syncing;
            set
            {
            if (value) ConnectionStatus = ConnectionStatus.Syncing;
            }
            public string ClientVersion
            {
            get;
            set;
            }
            public string OSVersion
            {
            get;
            set;
            }
            public class AlertPayload
            {
            public string AlertId
            {
            get;
            set;
            }
            public AlertSeverity Severity
            {
            get;
            set;
            }
            public string Title
            {
            get;
            set;
            }
            public string Message
            {
            get;
            set;
            }
            public string Source
            {
            get;
            set;
            }
            public string DedupeKey
            {
            get;
            set;
            }
            public DateTime CreatedAt
            {
            get;
            set;
            }
            public bool IsRead
            {
            get;
            set;
            }
            public class JournalSyncRecord
            {
            public string SyncId
            {
            get;
            set;
            }
            public string ATM_ID
            {
            get;
            set;
            }
            public string FileName
            {
            get;
            set;
            }
            public long FileSize
            {
            get;
            set;
            }
            public long FileOffset
            {
            get;
            set;
            }
            public string Checksum
            {
            get;
            set;
            }
            public string MD5Hash
            {
            get;
            set;
            }
            public string SHA256Hash
            {
            get;
            set;
            }
            public JournalSyncState State
            {
            get;
            set;
            }
            public int ProgressPercent
            {
            get;
            set;
            }
            public int RetryCount
            {
            get;
            set;
            }
            public string LocalPath
            {
            get;
            set;
            }
            public string ServerPath
            {
            get;
            set;
            }
            public DateTime CreatedAtUtc
            {
            get;
            set;
            }
            public DateTime UpdatedAtUtc
            {
            get;
            set;
            }
            public DateTime? CompletedAtUtc
            {
            get;
            set;
            }
            public string ATM_Description
            {
            get;
            set;
            }
            public bool IsConnected
            {
            get;
            set;
            }
            public bool IsSendingData
            {
            get;
            set;
            }
            public bool IsCSCConnected
            {
            get;
            set;
            }
            public DateTime LastDataReceived
            {
            get;
            set;
            }
            public DateTime LastHeartbeat
            {
            get;
            set;
            }
            public int ATMCache
            {
            get;
            set;
            }
            public int TotalDispensed
            {
            get;
            set;
            }
            public SyncStatus SyncState
            {
            get;
            set;
            }
            public long TotalBytesSent
            {
            get;
            set;
            }
            public int TotalFilesSynced
            {
            get;
            set;
            }
            public int TotalLinesSent
            {
            get;
            set;
            }
            public string LastSyncFile
            {
            get;
            set;
            }
            public class ClientConfig
            {
            public string ATM_ID
            {
            get;
            set;
            }
            public bool SyncTimeEnabled
            {
            get;
            set;
            }
            public int MessageSizeLines
            {
            get;
            set;
            }
            public int FilePackageKB
            {
            get;
            set;
            }
            public string SourcePath
            {
            get;
            set;
            }
            public string BackupPath
            {
            get;
            set;
            }
            public bool AutoStart
            {
            get;
            set;
            }
            public bool RunAsService
            {
            get;
            set;
            }
            public class ServerConfig
            {
            public int ListenPort
            {
            get;
            set;
            }
            public string StoragePath
            {
            get;
            set;
            }
            public string ArchivePath
            {
            get;
            set;
            }
            public bool AutoArchive
            {
            get;
            set;
            }
            public int MaxConnections
            {
            get;
            set;
            }
            public bool EnableEncryption
            {
            get;
            set;
            }
            public bool EnableCompression
            {
            get;
            set;
            }
            public enum ConnectionStatus
            {
            Disconnected = 0, Connecting = 1, Connected = 2, WaitingReply = 3, Syncing = 4
            }
            public enum ATMStatus
            {
            Unknown = 0,
            Online = 1,
            Idle = 2,
            Supervisor = 3,
            Warning = 4,
            Offline = 5,
            Critical = 6,
            InService = 10,
            ConnectedOnly = 11,
            WaitingResponse = 12,
            OutOfService = 13,
            CriticalFault = 14,
            Fault = 15,
            Maintenance = 16
            }
            public enum ATMType
            {
            NCR,
            GRG,
            WN,
            DieboldNixdorf,
            Hyosung,
            Other
            }
            public enum SyncStatus
            {
            Pending,
            InProgress,
            Syncing,
            Resyncing,
            Completed,
            Failed
            }
            public enum AlertSeverity
            {
            Info = 0, Warning = 1, Critical = 2, Emergency = 3
            }
            public enum JournalSyncState
            {
            Pending = 0, Syncing = 1, ReSyncing = 2, Completed = 3, Failed = 4, Archived = 5
            }
            public enum ATMCardState
            {
            NeverConnected,
            ConnectedActive,
            ConnectedIdle,
            Syncing,
            WaitingReply,
            Supervisor,
            RecentlyDisconnected,
            WarningOffline,
            CriticalOffline
            }
            public string AlertId
            {
            get;
            set;
            }
            public string SyncId
            {
            get;
            set;
            }
            public int ListenPort
            {
            get;
            set;
            }
            public string GetSourcePath() =>
            public void SetSourcePath(string v) =>
            public string GetBackupPath() =>
            public void SetBackupPath(string v) =>
            public ATMCardState GetCardState()
            {
            public Color GetCardColor()
            {
            public string GetStatusLabel()
            {
            public string GetStatusDescription() =>
            public string GetConnectionStatusDescription() =>
            public string GetElapsed(DateTime utcRef)
            {
            public void RecalculateHealthScore()
            {
            public override string ToString() =>
            public string GetStatusColor()
            {
            public SyncStrategy GetSyncStrategy()
            {
            public bool NeedsAlert()
            {
        }
    
        public partial public class AlertPayload
        {
            public string Icon => Severity
            switch
            {
            AlertSeverity.Emergency => "CRIT",
            AlertSeverity.Critical => "FAIL",
            AlertSeverity.Warning => "WARN",
            _ => "INFO"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity
            switch
            {
            AlertSeverity.Emergency => Color.FromArgb(255, 59, 48),
            AlertSeverity.Critical => Color.FromArgb(255, 59, 48),
            AlertSeverity.Warning => Color.FromArgb(255, 149, 0),
            _ => Color.FromArgb(0, 122, 255)
            };
            public string StateIcon => State
            switch
            {
            JournalSyncState.Pending => "PEND",
            JournalSyncState.Syncing => "SYNC",
            JournalSyncState.ReSyncing => "RSYNC",
            JournalSyncState.Completed => "OK",
            JournalSyncState.Failed => "FAIL",
            JournalSyncState.Archived => "ARCH",
            _ => "?"
            };
            public string StateLabel => State
            switch
            {
            JournalSyncState.Pending => "في الطابور",
            JournalSyncState.Syncing => "قيد المزامنة",
            JournalSyncState.ReSyncing => "إعادة مزامنة",
            JournalSyncState.Completed => "محمّل",
            JournalSyncState.Failed => "فشل",
            JournalSyncState.Archived => "مؤرشف",
            _ => "؟"
            };
            public string AlertId
            {
            get;
            set;
            }
            public AlertSeverity Severity
            {
            get;
            set;
            }
            public string Title
            {
            get;
            set;
            }
            public string Message
            {
            get;
            set;
            }
            public string Source
            {
            get;
            set;
            }
            public string DedupeKey
            {
            get;
            set;
            }
            public DateTime CreatedAt
            {
            get;
            set;
            }
            public bool IsRead
            {
            get;
            set;
            }
            public class JournalSyncRecord
            {
            public string SyncId
            {
            get;
            set;
            }
            public string ATM_ID
            {
            get;
            set;
            }
            public string FileName
            {
            get;
            set;
            }
            public long FileSize
            {
            get;
            set;
            }
            public long FileOffset
            {
            get;
            set;
            }
            public string Checksum
            {
            get;
            set;
            }
            public string MD5Hash
            {
            get;
            set;
            }
            public string SHA256Hash
            {
            get;
            set;
            }
            public JournalSyncState State
            {
            get;
            set;
            }
            public int ProgressPercent
            {
            get;
            set;
            }
            public int RetryCount
            {
            get;
            set;
            }
            public string LocalPath
            {
            get;
            set;
            }
            public string ServerPath
            {
            get;
            set;
            }
            public DateTime CreatedAtUtc
            {
            get;
            set;
            }
            public DateTime UpdatedAtUtc
            {
            get;
            set;
            }
            public DateTime? CompletedAtUtc
            {
            get;
            set;
            }
            public string ATM_Description
            {
            get;
            set;
            }
            public bool IsConnected
            {
            get;
            set;
            }
            public bool IsSendingData
            {
            get;
            set;
            }
            public bool IsCSCConnected
            {
            get;
            set;
            }
            public DateTime LastDataReceived
            {
            get;
            set;
            }
            public DateTime LastHeartbeat
            {
            get;
            set;
            }
            public int ATMCache
            {
            get;
            set;
            }
            public int TotalDispensed
            {
            get;
            set;
            }
            public SyncStatus SyncState
            {
            get;
            set;
            }
            public long TotalBytesSent
            {
            get;
            set;
            }
            public int TotalFilesSynced
            {
            get;
            set;
            }
            public int TotalLinesSent
            {
            get;
            set;
            }
            public string LastSyncFile
            {
            get;
            set;
            }
            public class ClientConfig
            {
            public string ATM_ID
            {
            get;
            set;
            }
            public bool SyncTimeEnabled
            {
            get;
            set;
            }
            public int MessageSizeLines
            {
            get;
            set;
            }
            public int FilePackageKB
            {
            get;
            set;
            }
            public string SourcePath
            {
            get;
            set;
            }
            public string BackupPath
            {
            get;
            set;
            }
            public bool AutoStart
            {
            get;
            set;
            }
            public bool RunAsService
            {
            get;
            set;
            }
            public class ServerConfig
            {
            public int ListenPort
            {
            get;
            set;
            }
            public string StoragePath
            {
            get;
            set;
            }
            public string ArchivePath
            {
            get;
            set;
            }
            public bool AutoArchive
            {
            get;
            set;
            }
            public int MaxConnections
            {
            get;
            set;
            }
            public bool EnableEncryption
            {
            get;
            set;
            }
            public bool EnableCompression
            {
            get;
            set;
            }
            public enum ConnectionStatus
            {
            Disconnected = 0, Connecting = 1, Connected = 2, WaitingReply = 3, Syncing = 4
            }
            public enum ATMStatus
            {
            Unknown = 0,
            Online = 1,
            Idle = 2,
            Supervisor = 3,
            Warning = 4,
            Offline = 5,
            Critical = 6,
            InService = 10,
            ConnectedOnly = 11,
            WaitingResponse = 12,
            OutOfService = 13,
            CriticalFault = 14,
            Fault = 15,
            Maintenance = 16
            }
            public enum ATMType
            {
            NCR,
            GRG,
            WN,
            DieboldNixdorf,
            Hyosung,
            Other
            }
            public enum SyncStatus
            {
            Pending,
            InProgress,
            Syncing,
            Resyncing,
            Completed,
            Failed
            }
            public enum AlertSeverity
            {
            Info = 0, Warning = 1, Critical = 2, Emergency = 3
            }
            public enum JournalSyncState
            {
            Pending = 0, Syncing = 1, ReSyncing = 2, Completed = 3, Failed = 4, Archived = 5
            }
            public enum ATMCardState
            {
            NeverConnected,
            ConnectedActive,
            ConnectedIdle,
            Syncing,
            WaitingReply,
            Supervisor,
            RecentlyDisconnected,
            WarningOffline,
            CriticalOffline
            }
            public string SyncId
            {
            get;
            set;
            }
            public string ATM_Name
            {
            get;
            set;
            }
            public string ATM_Type
            {
            get;
            set;
            }
            public string ServerIP
            {
            get;
            set;
            }
            public int ServerPort
            {
            get;
            set;
            }
            public ATMStatus Status
            {
            get;
            set;
            }
            public DateTime LastConnectionTime
            {
            get;
            set;
            }
            public DateTime LastSyncTime
            {
            get;
            set;
            }
            public string OSVersion
            {
            get;
            set;
            }
            public string ClientVersion
            {
            get;
            set;
            }
            public int ListenPort
            {
            get;
            set;
            }
            public string GetSourcePath() =>
            public void SetSourcePath(string v) =>
            public string GetBackupPath() =>
            public void SetBackupPath(string v) =>
            public ATMCardState GetCardState()
            {
            public Color GetCardColor()
            {
            public string GetStatusLabel()
            {
            public string GetStatusDescription() =>
            public string GetConnectionStatusDescription() =>
            public string GetElapsed(DateTime utcRef)
            {
            public void RecalculateHealthScore()
            {
            public override string ToString() =>
            public string GetStatusColor()
            {
            public SyncStrategy GetSyncStrategy()
            {
            public bool NeedsAlert()
            {
        }
    
        public partial public class JournalSyncRecord
        {
            public string Icon => Severity
            switch
            {
            AlertSeverity.Emergency => "CRIT",
            AlertSeverity.Critical => "FAIL",
            AlertSeverity.Warning => "WARN",
            _ => "INFO"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity
            switch
            {
            AlertSeverity.Emergency => Color.FromArgb(255, 59, 48),
            AlertSeverity.Critical => Color.FromArgb(255, 59, 48),
            AlertSeverity.Warning => Color.FromArgb(255, 149, 0),
            _ => Color.FromArgb(0, 122, 255)
            };
            public string StateIcon => State
            switch
            {
            JournalSyncState.Pending => "PEND",
            JournalSyncState.Syncing => "SYNC",
            JournalSyncState.ReSyncing => "RSYNC",
            JournalSyncState.Completed => "OK",
            JournalSyncState.Failed => "FAIL",
            JournalSyncState.Archived => "ARCH",
            _ => "?"
            };
            public string StateLabel => State
            switch
            {
            JournalSyncState.Pending => "في الطابور",
            JournalSyncState.Syncing => "قيد المزامنة",
            JournalSyncState.ReSyncing => "إعادة مزامنة",
            JournalSyncState.Completed => "محمّل",
            JournalSyncState.Failed => "فشل",
            JournalSyncState.Archived => "مؤرشف",
            _ => "؟"
            };
            public string SyncId
            {
            get;
            set;
            }
            public string ATM_ID
            {
            get;
            set;
            }
            public string FileName
            {
            get;
            set;
            }
            public long FileSize
            {
            get;
            set;
            }
            public long FileOffset
            {
            get;
            set;
            }
            public string Checksum
            {
            get;
            set;
            }
            public string MD5Hash
            {
            get;
            set;
            }
            public string SHA256Hash
            {
            get;
            set;
            }
            public JournalSyncState State
            {
            get;
            set;
            }
            public int ProgressPercent
            {
            get;
            set;
            }
            public int RetryCount
            {
            get;
            set;
            }
            public string LocalPath
            {
            get;
            set;
            }
            public string ServerPath
            {
            get;
            set;
            }
            public DateTime CreatedAtUtc
            {
            get;
            set;
            }
            public DateTime UpdatedAtUtc
            {
            get;
            set;
            }
            public DateTime? CompletedAtUtc
            {
            get;
            set;
            }
            public string ATM_Description
            {
            get;
            set;
            }
            public bool IsConnected
            {
            get;
            set;
            }
            public bool IsSendingData
            {
            get;
            set;
            }
            public bool IsCSCConnected
            {
            get;
            set;
            }
            public DateTime LastDataReceived
            {
            get;
            set;
            }
            public DateTime LastHeartbeat
            {
            get;
            set;
            }
            public int ATMCache
            {
            get;
            set;
            }
            public int TotalDispensed
            {
            get;
            set;
            }
            public SyncStatus SyncState
            {
            get;
            set;
            }
            public long TotalBytesSent
            {
            get;
            set;
            }
            public int TotalFilesSynced
            {
            get;
            set;
            }
            public int TotalLinesSent
            {
            get;
            set;
            }
            public string LastSyncFile
            {
            get;
            set;
            }
            public class ClientConfig
            {
            public string ATM_ID
            {
            get;
            set;
            }
            public bool SyncTimeEnabled
            {
            get;
            set;
            }
            public int MessageSizeLines
            {
            get;
            set;
            }
            public int FilePackageKB
            {
            get;
            set;
            }
            public string SourcePath
            {
            get;
            set;
            }
            public string BackupPath
            {
            get;
            set;
            }
            public bool AutoStart
            {
            get;
            set;
            }
            public bool RunAsService
            {
            get;
            set;
            }
            public class ServerConfig
            {
            public int ListenPort
            {
            get;
            set;
            }
            public string StoragePath
            {
            get;
            set;
            }
            public string ArchivePath
            {
            get;
            set;
            }
            public bool AutoArchive
            {
            get;
            set;
            }
            public int MaxConnections
            {
            get;
            set;
            }
            public bool EnableEncryption
            {
            get;
            set;
            }
            public bool EnableCompression
            {
            get;
            set;
            }
            public enum ConnectionStatus
            {
            Disconnected = 0, Connecting = 1, Connected = 2, WaitingReply = 3, Syncing = 4
            }
            public enum ATMStatus
            {
            Unknown = 0,
            Online = 1,
            Idle = 2,
            Supervisor = 3,
            Warning = 4,
            Offline = 5,
            Critical = 6,
            InService = 10,
            ConnectedOnly = 11,
            WaitingResponse = 12,
            OutOfService = 13,
            CriticalFault = 14,
            Fault = 15,
            Maintenance = 16
            }
            public enum ATMType
            {
            NCR,
            GRG,
            WN,
            DieboldNixdorf,
            Hyosung,
            Other
            }
            public enum SyncStatus
            {
            Pending,
            InProgress,
            Syncing,
            Resyncing,
            Completed,
            Failed
            }
            public enum AlertSeverity
            {
            Info = 0, Warning = 1, Critical = 2, Emergency = 3
            }
            public enum JournalSyncState
            {
            Pending = 0, Syncing = 1, ReSyncing = 2, Completed = 3, Failed = 4, Archived = 5
            }
            public enum ATMCardState
            {
            NeverConnected,
            ConnectedActive,
            ConnectedIdle,
            Syncing,
            WaitingReply,
            Supervisor,
            RecentlyDisconnected,
            WarningOffline,
            CriticalOffline
            }
            public string AlertId
            {
            get;
            set;
            }
            public AlertSeverity Severity
            {
            get;
            set;
            }
            public string Title
            {
            get;
            set;
            }
            public string Message
            {
            get;
            set;
            }
            public string Source
            {
            get;
            set;
            }
            public string DedupeKey
            {
            get;
            set;
            }
            public DateTime CreatedAt
            {
            get;
            set;
            }
            public bool IsRead
            {
            get;
            set;
            }
            public string ATM_Name
            {
            get;
            set;
            }
            public string ATM_Type
            {
            get;
            set;
            }
            public string ServerIP
            {
            get;
            set;
            }
            public int ServerPort
            {
            get;
            set;
            }
            public ATMStatus Status
            {
            get;
            set;
            }
            public DateTime LastConnectionTime
            {
            get;
            set;
            }
            public DateTime LastSyncTime
            {
            get;
            set;
            }
            public string OSVersion
            {
            get;
            set;
            }
            public string ClientVersion
            {
            get;
            set;
            }
            public int ListenPort
            {
            get;
            set;
            }
            public string GetSourcePath() =>
            public void SetSourcePath(string v) =>
            public string GetBackupPath() =>
            public void SetBackupPath(string v) =>
            public ATMCardState GetCardState()
            {
            public Color GetCardColor()
            {
            public string GetStatusLabel()
            {
            public string GetStatusDescription() =>
            public string GetConnectionStatusDescription() =>
            public string GetElapsed(DateTime utcRef)
            {
            public void RecalculateHealthScore()
            {
            public override string ToString() =>
            public string GetStatusColor()
            {
            public SyncStrategy GetSyncStrategy()
            {
            public bool NeedsAlert()
            {
        }
    
        public partial public class ClientConfig
        {
            public string Icon => Severity
            switch
            {
            AlertSeverity.Emergency => "CRIT",
            AlertSeverity.Critical => "FAIL",
            AlertSeverity.Warning => "WARN",
            _ => "INFO"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity
            switch
            {
            AlertSeverity.Emergency => Color.FromArgb(255, 59, 48),
            AlertSeverity.Critical => Color.FromArgb(255, 59, 48),
            AlertSeverity.Warning => Color.FromArgb(255, 149, 0),
            _ => Color.FromArgb(0, 122, 255)
            };
            public string StateIcon => State
            switch
            {
            JournalSyncState.Pending => "PEND",
            JournalSyncState.Syncing => "SYNC",
            JournalSyncState.ReSyncing => "RSYNC",
            JournalSyncState.Completed => "OK",
            JournalSyncState.Failed => "FAIL",
            JournalSyncState.Archived => "ARCH",
            _ => "?"
            };
            public string StateLabel => State
            switch
            {
            JournalSyncState.Pending => "في الطابور",
            JournalSyncState.Syncing => "قيد المزامنة",
            JournalSyncState.ReSyncing => "إعادة مزامنة",
            JournalSyncState.Completed => "محمّل",
            JournalSyncState.Failed => "فشل",
            JournalSyncState.Archived => "مؤرشف",
            _ => "؟"
            };
            public ClientConfig()
            {
            public string ATM_ID
            {
            get;
            set;
            }
            public bool SyncTimeEnabled
            {
            get;
            set;
            }
            public int MessageSizeLines
            {
            get;
            set;
            }
            public int FilePackageKB
            {
            get;
            set;
            }
            public string SourcePath
            {
            get;
            set;
            }
            public string BackupPath
            {
            get;
            set;
            }
            public bool AutoStart
            {
            get;
            set;
            }
            public bool RunAsService
            {
            get;
            set;
            }
            public class ServerConfig
            {
            public int ListenPort
            {
            get;
            set;
            }
            public string StoragePath
            {
            get;
            set;
            }
            public string ArchivePath
            {
            get;
            set;
            }
            public bool AutoArchive
            {
            get;
            set;
            }
            public int MaxConnections
            {
            get;
            set;
            }
            public bool EnableEncryption
            {
            get;
            set;
            }
            public bool EnableCompression
            {
            get;
            set;
            }
            public enum ConnectionStatus
            {
            Disconnected = 0, Connecting = 1, Connected = 2, WaitingReply = 3, Syncing = 4
            }
            public enum ATMStatus
            {
            Unknown = 0,
            Online = 1,
            Idle = 2,
            Supervisor = 3,
            Warning = 4,
            Offline = 5,
            Critical = 6,
            InService = 10,
            ConnectedOnly = 11,
            WaitingResponse = 12,
            OutOfService = 13,
            CriticalFault = 14,
            Fault = 15,
            Maintenance = 16
            }
            public enum ATMType
            {
            NCR,
            GRG,
            WN,
            DieboldNixdorf,
            Hyosung,
            Other
            }
            public enum SyncStatus
            {
            Pending,
            InProgress,
            Syncing,
            Resyncing,
            Completed,
            Failed
            }
            public enum AlertSeverity
            {
            Info = 0, Warning = 1, Critical = 2, Emergency = 3
            }
            public enum JournalSyncState
            {
            Pending = 0, Syncing = 1, ReSyncing = 2, Completed = 3, Failed = 4, Archived = 5
            }
            public enum ATMCardState
            {
            NeverConnected,
            ConnectedActive,
            ConnectedIdle,
            Syncing,
            WaitingReply,
            Supervisor,
            RecentlyDisconnected,
            WarningOffline,
            CriticalOffline
            }
            public string AlertId
            {
            get;
            set;
            }
            public AlertSeverity Severity
            {
            get;
            set;
            }
            public string Title
            {
            get;
            set;
            }
            public string Message
            {
            get;
            set;
            }
            public string Source
            {
            get;
            set;
            }
            public string DedupeKey
            {
            get;
            set;
            }
            public DateTime CreatedAt
            {
            get;
            set;
            }
            public bool IsRead
            {
            get;
            set;
            }
            public string SyncId
            {
            get;
            set;
            }
            public string FileName
            {
            get;
            set;
            }
            public long FileSize
            {
            get;
            set;
            }
            public long FileOffset
            {
            get;
            set;
            }
            public string Checksum
            {
            get;
            set;
            }
            public string MD5Hash
            {
            get;
            set;
            }
            public string SHA256Hash
            {
            get;
            set;
            }
            public JournalSyncState State
            {
            get;
            set;
            }
            public int ProgressPercent
            {
            get;
            set;
            }
            public int RetryCount
            {
            get;
            set;
            }
            public string LocalPath
            {
            get;
            set;
            }
            public string ServerPath
            {
            get;
            set;
            }
            public DateTime CreatedAtUtc
            {
            get;
            set;
            }
            public DateTime UpdatedAtUtc
            {
            get;
            set;
            }
            public DateTime? CompletedAtUtc
            {
            get;
            set;
            }
            public string ATM_Name
            {
            get;
            set;
            }
            public string ATM_Description
            {
            get;
            set;
            }
            public string ATM_Type
            {
            get;
            set;
            }
            public string ServerIP
            {
            get;
            set;
            }
            public int ServerPort
            {
            get;
            set;
            }
            public ATMStatus Status
            {
            get;
            set;
            }
            public bool IsConnected
            {
            get;
            set;
            }
            public bool IsSendingData
            {
            get;
            set;
            }
            public bool IsCSCConnected
            {
            get;
            set;
            }
            public DateTime LastConnectionTime
            {
            get;
            set;
            }
            public DateTime LastDataReceived
            {
            get;
            set;
            }
            public DateTime LastHeartbeat
            {
            get;
            set;
            }
            public int ATMCache
            {
            get;
            set;
            }
            public int TotalDispensed
            {
            get;
            set;
            }
            public SyncStatus SyncState
            {
            get;
            set;
            }
            public long TotalBytesSent
            {
            get;
            set;
            }
            public int TotalFilesSynced
            {
            get;
            set;
            }
            public int TotalLinesSent
            {
            get;
            set;
            }
            public DateTime LastSyncTime
            {
            get;
            set;
            }
            public string LastSyncFile
            {
            get;
            set;
            }
            public string OSVersion
            {
            get;
            set;
            }
            public string ClientVersion
            {
            get;
            set;
            }
            public int ListenPort
            {
            get;
            set;
            }
            public string GetSourcePath() =>
            public void SetSourcePath(string v) =>
            public string GetBackupPath() =>
            public void SetBackupPath(string v) =>
            public ATMCardState GetCardState()
            {
            public Color GetCardColor()
            {
            public string GetStatusLabel()
            {
            public string GetStatusDescription() =>
            public string GetConnectionStatusDescription() =>
            public string GetElapsed(DateTime utcRef)
            {
            public void RecalculateHealthScore()
            {
            public override string ToString() =>
            public string GetStatusColor()
            {
            public SyncStrategy GetSyncStrategy()
            {
            public bool NeedsAlert()
            {
        }
    
        public partial public class ServerConfig
        {
            public string Icon => Severity
            switch
            {
            AlertSeverity.Emergency => "CRIT",
            AlertSeverity.Critical => "FAIL",
            AlertSeverity.Warning => "WARN",
            _ => "INFO"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity
            switch
            {
            AlertSeverity.Emergency => Color.FromArgb(255, 59, 48),
            AlertSeverity.Critical => Color.FromArgb(255, 59, 48),
            AlertSeverity.Warning => Color.FromArgb(255, 149, 0),
            _ => Color.FromArgb(0, 122, 255)
            };
            public string StateIcon => State
            switch
            {
            JournalSyncState.Pending => "PEND",
            JournalSyncState.Syncing => "SYNC",
            JournalSyncState.ReSyncing => "RSYNC",
            JournalSyncState.Completed => "OK",
            JournalSyncState.Failed => "FAIL",
            JournalSyncState.Archived => "ARCH",
            _ => "?"
            };
            public string StateLabel => State
            switch
            {
            JournalSyncState.Pending => "في الطابور",
            JournalSyncState.Syncing => "قيد المزامنة",
            JournalSyncState.ReSyncing => "إعادة مزامنة",
            JournalSyncState.Completed => "محمّل",
            JournalSyncState.Failed => "فشل",
            JournalSyncState.Archived => "مؤرشف",
            _ => "؟"
            };
            public ServerConfig()
            {
            public int ListenPort
            {
            get;
            set;
            }
            public string StoragePath
            {
            get;
            set;
            }
            public string ArchivePath
            {
            get;
            set;
            }
            public bool AutoArchive
            {
            get;
            set;
            }
            public int MaxConnections
            {
            get;
            set;
            }
            public bool EnableEncryption
            {
            get;
            set;
            }
            public bool EnableCompression
            {
            get;
            set;
            }
            public enum ConnectionStatus
            {
            Disconnected = 0, Connecting = 1, Connected = 2, WaitingReply = 3, Syncing = 4
            }
            public enum ATMStatus
            {
            Unknown = 0,
            Online = 1,
            Idle = 2,
            Supervisor = 3,
            Warning = 4,
            Offline = 5,
            Critical = 6,
            InService = 10,
            ConnectedOnly = 11,
            WaitingResponse = 12,
            OutOfService = 13,
            CriticalFault = 14,
            Fault = 15,
            Maintenance = 16
            }
            public enum ATMType
            {
            NCR,
            GRG,
            WN,
            DieboldNixdorf,
            Hyosung,
            Other
            }
            public enum SyncStatus
            {
            Pending,
            InProgress,
            Syncing,
            Resyncing,
            Completed,
            Failed
            }
            public enum AlertSeverity
            {
            Info = 0, Warning = 1, Critical = 2, Emergency = 3
            }
            public enum JournalSyncState
            {
            Pending = 0, Syncing = 1, ReSyncing = 2, Completed = 3, Failed = 4, Archived = 5
            }
            public enum ATMCardState
            {
            NeverConnected,
            ConnectedActive,
            ConnectedIdle,
            Syncing,
            WaitingReply,
            Supervisor,
            RecentlyDisconnected,
            WarningOffline,
            CriticalOffline
            }
            public string AlertId
            {
            get;
            set;
            }
            public AlertSeverity Severity
            {
            get;
            set;
            }
            public string Title
            {
            get;
            set;
            }
            public string Message
            {
            get;
            set;
            }
            public string Source
            {
            get;
            set;
            }
            public string DedupeKey
            {
            get;
            set;
            }
            public DateTime CreatedAt
            {
            get;
            set;
            }
            public bool IsRead
            {
            get;
            set;
            }
            public string SyncId
            {
            get;
            set;
            }
            public string ATM_ID
            {
            get;
            set;
            }
            public string FileName
            {
            get;
            set;
            }
            public long FileSize
            {
            get;
            set;
            }
            public long FileOffset
            {
            get;
            set;
            }
            public string Checksum
            {
            get;
            set;
            }
            public string MD5Hash
            {
            get;
            set;
            }
            public string SHA256Hash
            {
            get;
            set;
            }
            public JournalSyncState State
            {
            get;
            set;
            }
            public int ProgressPercent
            {
            get;
            set;
            }
            public int RetryCount
            {
            get;
            set;
            }
            public string LocalPath
            {
            get;
            set;
            }
            public string ServerPath
            {
            get;
            set;
            }
            public DateTime CreatedAtUtc
            {
            get;
            set;
            }
            public DateTime UpdatedAtUtc
            {
            get;
            set;
            }
            public DateTime? CompletedAtUtc
            {
            get;
            set;
            }
            public string ATM_Name
            {
            get;
            set;
            }
            public string ATM_Description
            {
            get;
            set;
            }
            public string ATM_Type
            {
            get;
            set;
            }
            public string ServerIP
            {
            get;
            set;
            }
            public int ServerPort
            {
            get;
            set;
            }
            public ATMStatus Status
            {
            get;
            set;
            }
            public bool IsConnected
            {
            get;
            set;
            }
            public bool IsSendingData
            {
            get;
            set;
            }
            public bool IsCSCConnected
            {
            get;
            set;
            }
            public DateTime LastConnectionTime
            {
            get;
            set;
            }
            public DateTime LastDataReceived
            {
            get;
            set;
            }
            public DateTime LastHeartbeat
            {
            get;
            set;
            }
            public int ATMCache
            {
            get;
            set;
            }
            public int TotalDispensed
            {
            get;
            set;
            }
            public SyncStatus SyncState
            {
            get;
            set;
            }
            public long TotalBytesSent
            {
            get;
            set;
            }
            public int TotalFilesSynced
            {
            get;
            set;
            }
            public int TotalLinesSent
            {
            get;
            set;
            }
            public DateTime LastSyncTime
            {
            get;
            set;
            }
            public string LastSyncFile
            {
            get;
            set;
            }
            public string OSVersion
            {
            get;
            set;
            }
            public string ClientVersion
            {
            get;
            set;
            }
            public bool SyncTimeEnabled
            {
            get;
            set;
            }
            public int MessageSizeLines
            {
            get;
            set;
            }
            public int FilePackageKB
            {
            get;
            set;
            }
            public string SourcePath
            {
            get;
            set;
            }
            public string BackupPath
            {
            get;
            set;
            }
            public bool AutoStart
            {
            get;
            set;
            }
            public bool RunAsService
            {
            get;
            set;
            }
            public string GetSourcePath() =>
            public void SetSourcePath(string v) =>
            public string GetBackupPath() =>
            public void SetBackupPath(string v) =>
            public ATMCardState GetCardState()
            {
            public Color GetCardColor()
            {
            public string GetStatusLabel()
            {
            public string GetStatusDescription() =>
            public string GetConnectionStatusDescription() =>
            public string GetElapsed(DateTime utcRef)
            {
            public void RecalculateHealthScore()
            {
            public override string ToString() =>
            public string GetStatusColor()
            {
            public SyncStrategy GetSyncStrategy()
            {
            public bool NeedsAlert()
            {
        }
    
    }
    public enum ConnectionStatus  {
    
        public enum ATMStatus
        {
    
        public enum ATMType {
    
        public enum SyncStatus {
    
        public enum AlertSeverity     {
    
        public enum JournalSyncState  {
    
        public enum ATMCardState      {
    
        public enum SyncStrategy {
    
        public partial public class ATMInfo
        {
            public string Icon => Severity switch
            {
            AlertSeverity.Emergency => "CRIT",
            AlertSeverity.Critical  => "FAIL",
            AlertSeverity.Warning   => "WARN",
            _                       => "INFO"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity switch
            {
            AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
            AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
            AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
            _                       => Color.FromArgb(0,   122, 255)
            };
            public string StateIcon => State switch
            {
            JournalSyncState.Pending   => "PEND",
            JournalSyncState.Syncing   => "SYNC",
            JournalSyncState.ReSyncing => "RSYNC",
            JournalSyncState.Completed => "OK",
            JournalSyncState.Failed    => "FAIL",
            JournalSyncState.Archived  => "ARCH",
            _ => "?"
            };
            public string StateLabel => State switch
            {
            JournalSyncState.Pending   => "في الطابور",
            JournalSyncState.Syncing   => "قيد المزامنة",
            JournalSyncState.ReSyncing => "إعادة مزامنة",
            JournalSyncState.Completed => "محمّل",
            JournalSyncState.Failed    => "فشل",
            JournalSyncState.Archived  => "مؤرشف",
            _ => "؟"
            };
            public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;
            private string _sourcePath;
            private string _backupPath;
            public ATMInfo()
            {
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
            public enum ATMStatus
            {
            Unknown = 0,
            Online = 1,
            Idle = 2,
            Supervisor = 3,
            Warning = 4,
            Offline = 5,
            Critical = 6,
            InService = 10,
            ConnectedOnly = 11,
            WaitingResponse = 12,
            OutOfService = 13,
            CriticalFault = 14,
            Fault = 15,
            Maintenance = 16
            }
            public enum ATMType { NCR, GRG, WN, DieboldNixdorf, Hyosung, Other }
            public enum SyncStatus { Pending, InProgress, Syncing, Resyncing, Completed, Failed }
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
            public enum ATMCardState      { NeverConnected, ConnectedActive, ConnectedIdle, Syncing, WaitingReply, Supervisor, RecentlyDisconnected, WarningOffline, CriticalOffline }
            public class ATMInfo
            {
            // هوية
            public string ATM_ID        { get; set; }
            public string ATM_Name      { get; set; }
            public string ATM_Type      { get; set; }
            public string BranchName    { get; set; }
            public string Region        { get; set; }
            public string ATMId { get => ATM_ID; set => ATM_ID = value; }
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
            public ATMType ATMType
            {
            get
            {
            switch (AppConstants.NormalizeATMType(ATM_Type))
            {
            case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
            case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
            case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
            case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
            case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
            default:                        return ATMType.Other;
            }
            public string Location { get => Region; set => Region = value; }
            public string BranchCode { get; set; }
            public string ServerIP      { get; set; }
            public int    ServerPort    { get; set; }
            public string NetworkType   { get; set; }
            public int    Latency_ms    { get; set; }
            public int Latency { get => Latency_ms; set => Latency_ms = value; }
            public ConnectionStatus ConnectionStatus { get; set; }
            public ATMStatus        Status           { get; set; }
            public string           SessionId        { get; set; }
            public bool             IsSupervisorMode { get; set; }
            public bool             IsHostConnected  { get; set; }
            public DateTime ConnectedAtUtc       { get; set; }
            public DateTime DisconnectedAtUtc    { get; set; }
            public DateTime LastHeartbeatUtc     { get; set; }
            public DateTime LastSyncUtc          { get; set; }
            public DateTime LastDataReceivedUtc  { get; set; }
            public DateTime LastCommandSentUtc   { get; set; }
            public DateTime LastConnectionTime { get => ConnectedAtUtc; set => ConnectedAtUtc = value; }
            public DateTime LastSyncTime { get => LastSyncUtc; set => LastSyncUtc = value; }
            public long   TotalSyncedBytes         { get; set; }
            public long   TotalTransactions        { get; set; }
            public int    ConsecutiveSyncFailures  { get; set; }
            public double SyncSuccessRate          { get; set; }
            public double ReceiveSpeedKBs          { get; set; }
            public long   JournalSizeToday         { get; set; }
            public double CpuUsagePercent          { get; set; }
            public double MemoryUsagePercent       { get; set; }
            public double DiskUsagePercent         { get; set; }
            public int    HealthScore              { get; set; }
            public int PendingJournalCount { get; set; }
            public double SuccessRate { get => SyncSuccessRate; set => SyncSuccessRate = value; }
            public long TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
            public int  ApprovedTransactions  { get; set; }
            public int  FailedTransactions    { get; set; }
            public int  CardsCaptured         { get; set; }
            public long CashDispensed         { get; set; }
            public string LastJournalFile   { get; set; }
            public string LastErrorCode     { get; set; }
            public string LastErrorMessage  { get; set; }
            public string LastTransaction   { get; set; }
            public string LastError { get => LastErrorMessage; set => LastErrorMessage = value; }
            public int TransactionCount { get => (int)Math.Min(int.MaxValue, TotalTransactions); set => TotalTransactions = value; }
            public bool IsSyncing { get => ConnectionStatus == ConnectionStatus.Syncing; set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            public string ClientVersion { get; set; }
            public string OSVersion     { get; set; }
            public class AlertPayload
            {
            public string        AlertId   { get; set; }
            public AlertSeverity Severity  { get; set; }
            public string        Title     { get; set; }
            public string        Message   { get; set; }
            public string        Source    { get; set; }
            public string        DedupeKey { get; set; }
            public DateTime      CreatedAt { get; set; }
            public bool          IsRead    { get; set; }
            public class JournalSyncRecord
            {
            public string           SyncId          { get; set; }
            public string           ATM_ID          { get; set; }
            public string           FileName        { get; set; }
            public long             FileSize        { get; set; }
            public long             FileOffset      { get; set; }
            public string           Checksum        { get; set; }
            public string           MD5Hash         { get; set; }
            public string           SHA256Hash      { get; set; }
            public JournalSyncState State           { get; set; }
            public int              ProgressPercent { get; set; }
            public int              RetryCount      { get; set; }
            public string           LocalPath       { get; set; }
            public string           ServerPath      { get; set; }
            public DateTime         CreatedAtUtc    { get; set; }
            public DateTime         UpdatedAtUtc    { get; set; }
            public DateTime?        CompletedAtUtc  { get; set; }
            public string ATM_Description { get; set; }
            public bool IsConnected { get; set; }
            public bool IsSendingData { get; set; }
            public bool IsCSCConnected { get; set; }
            public DateTime LastDataReceived { get; set; }
            public DateTime LastHeartbeat { get; set; }
            public int ATMCache { get; set; }
            public int TotalDispensed { get; set; }
            public SyncStatus SyncState { get; set; }
            public long TotalBytesSent { get; set; }
            public int TotalFilesSynced { get; set; }
            public int TotalLinesSent { get; set; }
            public string LastSyncFile { get; set; }
            public class ClientConfig
            {
            public string ATM_ID { get; set; }
            public bool SyncTimeEnabled { get; set; }
            public int MessageSizeLines { get; set; }
            public int FilePackageKB { get; set; }
            public string SourcePath { get; set; }
            public string BackupPath { get; set; }
            public bool AutoStart { get; set; }
            public bool RunAsService { get; set; }
            public class ServerConfig
            {
            public int ListenPort { get; set; }
            public string StoragePath { get; set; }
            public string ArchivePath { get; set; }
            public bool AutoArchive { get; set; }
            public int MaxConnections { get; set; }
            public bool EnableEncryption { get; set; }
            public bool EnableCompression { get; set; }
            public string ATMType { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string Branch { get; set; }
            public string SourceJournalPath { get; set; }
            public string ImageInboxPath { get; set; }
            public string ImageDestPath { get; set; }
            public DateTime LastSync { get; set; }
            public string Name { get; set; }
            public int TotalTransactionsSynced { get => (int)TotalTransactions; set => TotalTransactions = value; }
            public bool HasCashTelemetry { get; set; }
            public long Cassette1Remaining { get; set; }
            public long Cassette2Remaining { get; set; }
            public long Cassette3Remaining { get; set; }
            public long Cassette4Remaining { get; set; }
            public long ATMCache { get; set; }
            public long CashLoadedTotal { get; set; }
            public long TotalDispensed { get; set; }
            public long CashDepositInTotal { get; set; }
            public long CashRejectCount { get; set; }
            public long CashRetractCount { get; set; }
            public DateTime CashTelemetryUpdatedAtUtc { get; set; }
            public bool HasAlerts { get; set; }
            public string        AlertId   { get; set; }
            public AlertSeverity SeverityLevel { get; set; }
            public string        Severity  { get; set; }
            public DateTime      RaisedAt  { get => CreatedAt; set => CreatedAt = value; }
            public string           SyncId          { get; set; }
            public string NetworkQuality { get; set; }
            public int ListenPort { get; set; }
            public string GetSourcePath() =>
            public void SetSourcePath(string v) =>
            public string GetBackupPath() =>
            public void SetBackupPath(string v) =>
            public ATMCardState GetCardState()
            {
            public Color GetCardColor()
            {
            public string GetStatusLabel()
            {
            public string GetStatusDescription() =>
            public string GetConnectionStatusDescription() =>
            public string GetElapsed(DateTime utcRef)
            {
            public void RecalculateHealthScore()
            {
            public override string ToString() =>
            public string GetStatusColor()
            {
            public SyncStrategy GetSyncStrategy()
            {
            public bool NeedsAlert()
            {
            public string GetLegacySourcePath()
            {
            public string GetLegacyBackupPath()
            {
            public string GetElapsed(DateTime? dt) =>
        }
    
        public partial public class AlertPayload
        {
            public string Icon => Severity switch
            {
            AlertSeverity.Emergency => "CRIT",
            AlertSeverity.Critical  => "FAIL",
            AlertSeverity.Warning   => "WARN",
            _                       => "INFO"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity switch
            {
            AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
            AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
            AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
            _                       => Color.FromArgb(0,   122, 255)
            };
            public string StateIcon => State switch
            {
            JournalSyncState.Pending   => "PEND",
            JournalSyncState.Syncing   => "SYNC",
            JournalSyncState.ReSyncing => "RSYNC",
            JournalSyncState.Completed => "OK",
            JournalSyncState.Failed    => "FAIL",
            JournalSyncState.Archived  => "ARCH",
            _ => "?"
            };
            public string StateLabel => State switch
            {
            JournalSyncState.Pending   => "في الطابور",
            JournalSyncState.Syncing   => "قيد المزامنة",
            JournalSyncState.ReSyncing => "إعادة مزامنة",
            JournalSyncState.Completed => "محمّل",
            JournalSyncState.Failed    => "فشل",
            JournalSyncState.Archived  => "مؤرشف",
            _ => "؟"
            };
            public string        AlertId   { get; set; }
            public AlertSeverity Severity  { get; set; }
            public string        Title     { get; set; }
            public string        Message   { get; set; }
            public string        Source    { get; set; }
            public string        DedupeKey { get; set; }
            public DateTime      CreatedAt { get; set; }
            public bool          IsRead    { get; set; }
            public class JournalSyncRecord
            {
            public string           SyncId          { get; set; }
            public string           ATM_ID          { get; set; }
            public string           FileName        { get; set; }
            public long             FileSize        { get; set; }
            public long             FileOffset      { get; set; }
            public string           Checksum        { get; set; }
            public string           MD5Hash         { get; set; }
            public string           SHA256Hash      { get; set; }
            public JournalSyncState State           { get; set; }
            public int              ProgressPercent { get; set; }
            public int              RetryCount      { get; set; }
            public string           LocalPath       { get; set; }
            public string           ServerPath      { get; set; }
            public DateTime         CreatedAtUtc    { get; set; }
            public DateTime         UpdatedAtUtc    { get; set; }
            public DateTime?        CompletedAtUtc  { get; set; }
            public string ATM_Description { get; set; }
            public bool IsConnected { get; set; }
            public bool IsSendingData { get; set; }
            public bool IsCSCConnected { get; set; }
            public DateTime LastDataReceived { get; set; }
            public DateTime LastHeartbeat { get; set; }
            public int ATMCache { get; set; }
            public int TotalDispensed { get; set; }
            public SyncStatus SyncState { get; set; }
            public long TotalBytesSent { get; set; }
            public int TotalFilesSynced { get; set; }
            public int TotalLinesSent { get; set; }
            public string LastSyncFile { get; set; }
            public class ClientConfig
            {
            public string ATM_ID { get; set; }
            public bool SyncTimeEnabled { get; set; }
            public int MessageSizeLines { get; set; }
            public int FilePackageKB { get; set; }
            public string SourcePath { get; set; }
            public string BackupPath { get; set; }
            public bool AutoStart { get; set; }
            public bool RunAsService { get; set; }
            public class ServerConfig
            {
            public int ListenPort { get; set; }
            public string StoragePath { get; set; }
            public string ArchivePath { get; set; }
            public bool AutoArchive { get; set; }
            public int MaxConnections { get; set; }
            public bool EnableEncryption { get; set; }
            public bool EnableCompression { get; set; }
            public string ATMType { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string Branch { get; set; }
            public string SourceJournalPath { get; set; }
            public string ImageInboxPath { get; set; }
            public string ImageDestPath { get; set; }
            public DateTime LastSync { get; set; }
            public string Name { get; set; }
            public int TotalTransactionsSynced { get => (int)TotalTransactions; set => TotalTransactions = value; }
            public bool HasCashTelemetry { get; set; }
            public long Cassette1Remaining { get; set; }
            public long Cassette2Remaining { get; set; }
            public long Cassette3Remaining { get; set; }
            public long Cassette4Remaining { get; set; }
            public long ATMCache { get; set; }
            public long CashLoadedTotal { get; set; }
            public long TotalDispensed { get; set; }
            public long CashDepositInTotal { get; set; }
            public long CashRejectCount { get; set; }
            public long CashRetractCount { get; set; }
            public DateTime CashTelemetryUpdatedAtUtc { get; set; }
            public bool HasAlerts { get; set; }
            public AlertSeverity SeverityLevel { get; set; }
            public string        Severity  { get; set; }
            public DateTime      RaisedAt  { get => CreatedAt; set => CreatedAt = value; }
            public string           SyncId          { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }
            public ATMStatus Status { get; set; }
            public DateTime LastConnectionTime { get; set; }
            public DateTime LastSyncTime { get; set; }
            public string OSVersion { get; set; }
            public string ClientVersion { get; set; }
            public string NetworkQuality { get; set; }
            public int ListenPort { get; set; }
            public string GetSourcePath() =>
            public void SetSourcePath(string v) =>
            public string GetBackupPath() =>
            public void SetBackupPath(string v) =>
            public ATMCardState GetCardState()
            {
            public Color GetCardColor()
            {
            public string GetStatusLabel()
            {
            public string GetStatusDescription() =>
            public string GetConnectionStatusDescription() =>
            public string GetElapsed(DateTime utcRef)
            {
            public void RecalculateHealthScore()
            {
            public override string ToString() =>
            public string GetStatusColor()
            {
            public SyncStrategy GetSyncStrategy()
            {
            public bool NeedsAlert()
            {
            public string GetLegacySourcePath()
            {
            public string GetLegacyBackupPath()
            {
            public string GetElapsed(DateTime? dt) =>
        }
    
        public partial public class JournalSyncRecord
        {
            public string StateIcon => State switch
            {
            JournalSyncState.Pending   => "PEND",
            JournalSyncState.Syncing   => "SYNC",
            JournalSyncState.ReSyncing => "RSYNC",
            JournalSyncState.Completed => "OK",
            JournalSyncState.Failed    => "FAIL",
            JournalSyncState.Archived  => "ARCH",
            _ => "?"
            };
            public string StateLabel => State switch
            {
            JournalSyncState.Pending   => "في الطابور",
            JournalSyncState.Syncing   => "قيد المزامنة",
            JournalSyncState.ReSyncing => "إعادة مزامنة",
            JournalSyncState.Completed => "محمّل",
            JournalSyncState.Failed    => "فشل",
            JournalSyncState.Archived  => "مؤرشف",
            _ => "؟"
            };
            public string Icon => Severity switch
            {
            AlertSeverity.Emergency => "CRIT",
            AlertSeverity.Critical  => "FAIL",
            AlertSeverity.Warning   => "WARN",
            _                       => "INFO"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity switch
            {
            AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
            AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
            AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
            _                       => Color.FromArgb(0,   122, 255)
            };
            public string           SyncId          { get; set; }
            public string           ATM_ID          { get; set; }
            public string           FileName        { get; set; }
            public long             FileSize        { get; set; }
            public long             FileOffset      { get; set; }
            public string           Checksum        { get; set; }
            public string           MD5Hash         { get; set; }
            public string           SHA256Hash      { get; set; }
            public JournalSyncState State           { get; set; }
            public int              ProgressPercent { get; set; }
            public int              RetryCount      { get; set; }
            public string           LocalPath       { get; set; }
            public string           ServerPath      { get; set; }
            public string           Message         { get; set; }
            public DateTime         CreatedAtUtc    { get; set; }
            public DateTime         UpdatedAtUtc    { get; set; }
            public DateTime?        CompletedAtUtc  { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Description { get; set; }
            public string ATM_Type { get; set; }
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }
            public ATMStatus Status { get; set; }
            public bool IsConnected { get; set; }
            public bool IsSendingData { get; set; }
            public bool IsCSCConnected { get; set; }
            public DateTime LastConnectionTime { get; set; }
            public DateTime LastDataReceived { get; set; }
            public DateTime LastHeartbeat { get; set; }
            public int ATMCache { get; set; }
            public int TotalDispensed { get; set; }
            public SyncStatus SyncState { get; set; }
            public long TotalBytesSent { get; set; }
            public int TotalFilesSynced { get; set; }
            public int TotalLinesSent { get; set; }
            public DateTime LastSyncTime { get; set; }
            public string LastSyncFile { get; set; }
            public string OSVersion { get; set; }
            public string ClientVersion { get; set; }
            public class ClientConfig
            {
            public string ATM_ID { get; set; }
            public bool SyncTimeEnabled { get; set; }
            public int MessageSizeLines { get; set; }
            public int FilePackageKB { get; set; }
            public string SourcePath { get; set; }
            public string BackupPath { get; set; }
            public bool AutoStart { get; set; }
            public bool RunAsService { get; set; }
            public class ServerConfig
            {
            public int ListenPort { get; set; }
            public string StoragePath { get; set; }
            public string ArchivePath { get; set; }
            public bool AutoArchive { get; set; }
            public int MaxConnections { get; set; }
            public bool EnableEncryption { get; set; }
            public bool EnableCompression { get; set; }
            public string ATMType { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string Branch { get; set; }
            public string SourceJournalPath { get; set; }
            public string ImageInboxPath { get; set; }
            public string ImageDestPath { get; set; }
            public DateTime LastSync { get; set; }
            public string Name { get; set; }
            public int TotalTransactionsSynced { get => (int)TotalTransactions; set => TotalTransactions = value; }
            public bool HasCashTelemetry { get; set; }
            public long Cassette1Remaining { get; set; }
            public long Cassette2Remaining { get; set; }
            public long Cassette3Remaining { get; set; }
            public long Cassette4Remaining { get; set; }
            public long ATMCache { get; set; }
            public long CashLoadedTotal { get; set; }
            public long TotalDispensed { get; set; }
            public long CashDepositInTotal { get; set; }
            public long CashRejectCount { get; set; }
            public long CashRetractCount { get; set; }
            public DateTime CashTelemetryUpdatedAtUtc { get; set; }
            public bool HasAlerts { get; set; }
            public string        AlertId   { get; set; }
            public AlertSeverity Severity  { get; set; }
            public string        Title     { get; set; }
            public string        Source    { get; set; }
            public string        DedupeKey { get; set; }
            public DateTime      CreatedAt { get; set; }
            public bool          IsRead    { get; set; }
            public AlertSeverity SeverityLevel { get; set; }
            public string        Severity  { get; set; }
            public DateTime      RaisedAt  { get => CreatedAt; set => CreatedAt = value; }
            public string NetworkQuality { get; set; }
            public int ListenPort { get; set; }
            public string GetStatusColor()
            {
            public string GetSourcePath()
            {
            public string GetBackupPath()
            {
            public SyncStrategy GetSyncStrategy()
            {
            public bool NeedsAlert()
            {
            public void SetSourcePath(string v) =>
            public void SetBackupPath(string v) =>
            public ATMCardState GetCardState()
            {
            public Color GetCardColor()
            {
            public string GetStatusLabel()
            {
            public string GetStatusDescription() =>
            public string GetConnectionStatusDescription() =>
            public string GetElapsed(DateTime utcRef)
            {
            public void RecalculateHealthScore()
            {
            public override string ToString() =>
            public string GetLegacySourcePath()
            {
            public string GetLegacyBackupPath()
            {
            public string GetElapsed(DateTime? dt) =>
        }
    
        public partial public class ClientConfig
        {
            public string Icon => Severity switch
            {
            AlertSeverity.Emergency => "CRIT",
            AlertSeverity.Critical  => "FAIL",
            AlertSeverity.Warning   => "WARN",
            _                       => "INFO"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity switch
            {
            AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
            AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
            AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
            _                       => Color.FromArgb(0,   122, 255)
            };
            public string StateIcon => State switch
            {
            JournalSyncState.Pending   => "PEND",
            JournalSyncState.Syncing   => "SYNC",
            JournalSyncState.ReSyncing => "RSYNC",
            JournalSyncState.Completed => "OK",
            JournalSyncState.Failed    => "FAIL",
            JournalSyncState.Archived  => "ARCH",
            _ => "?"
            };
            public string StateLabel => State switch
            {
            JournalSyncState.Pending   => "في الطابور",
            JournalSyncState.Syncing   => "قيد المزامنة",
            JournalSyncState.ReSyncing => "إعادة مزامنة",
            JournalSyncState.Completed => "محمّل",
            JournalSyncState.Failed    => "فشل",
            JournalSyncState.Archived  => "مؤرشف",
            _ => "؟"
            };
            public ClientConfig()
            {
            public string ATM_ID { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }
            public bool SyncTimeEnabled { get; set; }
            public int MessageSizeLines { get; set; }
            public int FilePackageKB { get; set; }
            public string SourcePath { get; set; }
            public string BackupPath { get; set; }
            public bool AutoStart { get; set; }
            public bool RunAsService { get; set; }
            public class ServerConfig
            {
            public int ListenPort { get; set; }
            public string StoragePath { get; set; }
            public string ArchivePath { get; set; }
            public bool AutoArchive { get; set; }
            public int MaxConnections { get; set; }
            public bool EnableEncryption { get; set; }
            public bool EnableCompression { get; set; }
            public string ATMType { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string Branch { get; set; }
            public string SourceJournalPath { get; set; }
            public string ImageInboxPath { get; set; }
            public string ImageDestPath { get; set; }
            public DateTime LastSync { get; set; }
            public string Name { get; set; }
            public int TotalTransactionsSynced { get => (int)TotalTransactions; set => TotalTransactions = value; }
            public bool HasCashTelemetry { get; set; }
            public long Cassette1Remaining { get; set; }
            public long Cassette2Remaining { get; set; }
            public long Cassette3Remaining { get; set; }
            public long Cassette4Remaining { get; set; }
            public long ATMCache { get; set; }
            public long CashLoadedTotal { get; set; }
            public long TotalDispensed { get; set; }
            public long CashDepositInTotal { get; set; }
            public long CashRejectCount { get; set; }
            public long CashRetractCount { get; set; }
            public DateTime CashTelemetryUpdatedAtUtc { get; set; }
            public bool HasAlerts { get; set; }
            public string        AlertId   { get; set; }
            public AlertSeverity Severity  { get; set; }
            public string        Title     { get; set; }
            public string        Message   { get; set; }
            public string        Source    { get; set; }
            public string        DedupeKey { get; set; }
            public DateTime      CreatedAt { get; set; }
            public bool          IsRead    { get; set; }
            public AlertSeverity SeverityLevel { get; set; }
            public string        Severity  { get; set; }
            public DateTime      CreatedAtUtc { get => CreatedAt; set => CreatedAt = value; }
            public DateTime      RaisedAt  { get => CreatedAt; set => CreatedAt = value; }
            public string           SyncId          { get; set; }
            public string           FileName        { get; set; }
            public long             FileSize        { get; set; }
            public long             FileOffset      { get; set; }
            public string           Checksum        { get; set; }
            public string           MD5Hash         { get; set; }
            public string           SHA256Hash      { get; set; }
            public JournalSyncState State           { get; set; }
            public int              ProgressPercent { get; set; }
            public int              RetryCount      { get; set; }
            public string           LocalPath       { get; set; }
            public string           ServerPath      { get; set; }
            public DateTime         UpdatedAtUtc    { get; set; }
            public DateTime?        CompletedAtUtc  { get; set; }
            public string ATM_Description { get; set; }
            public ATMStatus Status { get; set; }
            public bool IsConnected { get; set; }
            public bool IsSendingData { get; set; }
            public bool IsCSCConnected { get; set; }
            public DateTime LastConnectionTime { get; set; }
            public DateTime LastDataReceived { get; set; }
            public DateTime LastHeartbeat { get; set; }
            public int ATMCache { get; set; }
            public int TotalDispensed { get; set; }
            public SyncStatus SyncState { get; set; }
            public long TotalBytesSent { get; set; }
            public int TotalFilesSynced { get; set; }
            public int TotalLinesSent { get; set; }
            public DateTime LastSyncTime { get; set; }
            public string LastSyncFile { get; set; }
            public string OSVersion { get; set; }
            public string ClientVersion { get; set; }
            public string NetworkQuality { get; set; }
            public int ListenPort { get; set; }
            public string GetSourcePath() =>
            public void SetSourcePath(string v) =>
            public string GetBackupPath() =>
            public void SetBackupPath(string v) =>
            public ATMCardState GetCardState()
            {
            public Color GetCardColor()
            {
            public string GetStatusLabel()
            {
            public string GetStatusDescription() =>
            public string GetConnectionStatusDescription() =>
            public string GetElapsed(DateTime utcRef)
            {
            public void RecalculateHealthScore()
            {
            public override string ToString() =>
            public string GetStatusColor()
            {
            public SyncStrategy GetSyncStrategy()
            {
            public bool NeedsAlert()
            {
            public string GetLegacySourcePath()
            {
            public string GetLegacyBackupPath()
            {
            public string GetElapsed(DateTime? dt) =>
        }
    
        public partial public class ServerConfig
        {
            public string Icon => Severity switch
            {
            AlertSeverity.Emergency => "CRIT",
            AlertSeverity.Critical  => "FAIL",
            AlertSeverity.Warning   => "WARN",
            _                       => "INFO"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity switch
            {
            AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
            AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
            AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
            _                       => Color.FromArgb(0,   122, 255)
            };
            public string StateIcon => State switch
            {
            JournalSyncState.Pending   => "PEND",
            JournalSyncState.Syncing   => "SYNC",
            JournalSyncState.ReSyncing => "RSYNC",
            JournalSyncState.Completed => "OK",
            JournalSyncState.Failed    => "FAIL",
            JournalSyncState.Archived  => "ARCH",
            _ => "?"
            };
            public string StateLabel => State switch
            {
            JournalSyncState.Pending   => "في الطابور",
            JournalSyncState.Syncing   => "قيد المزامنة",
            JournalSyncState.ReSyncing => "إعادة مزامنة",
            JournalSyncState.Completed => "محمّل",
            JournalSyncState.Failed    => "فشل",
            JournalSyncState.Archived  => "مؤرشف",
            _ => "؟"
            };
            public ServerConfig()
            {
            public int ListenPort { get; set; }
            public string StoragePath { get; set; }
            public string ArchivePath { get; set; }
            public bool AutoArchive { get; set; }
            public int MaxConnections { get; set; }
            public bool EnableEncryption { get; set; }
            public bool EnableCompression { get; set; }
            public string ATMType { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string Branch { get; set; }
            public string SourceJournalPath { get; set; }
            public string ImageInboxPath { get; set; }
            public string ImageDestPath { get; set; }
            public DateTime LastSync { get; set; }
            public string Name { get; set; }
            public int TotalTransactionsSynced { get => (int)TotalTransactions; set => TotalTransactions = value; }
            public bool HasCashTelemetry { get; set; }
            public long Cassette1Remaining { get; set; }
            public long Cassette2Remaining { get; set; }
            public long Cassette3Remaining { get; set; }
            public long Cassette4Remaining { get; set; }
            public long ATMCache { get; set; }
            public long CashLoadedTotal { get; set; }
            public long TotalDispensed { get; set; }
            public long CashDepositInTotal { get; set; }
            public long CashRejectCount { get; set; }
            public long CashRetractCount { get; set; }
            public DateTime CashTelemetryUpdatedAtUtc { get; set; }
            public bool HasAlerts { get; set; }
            public string        AlertId   { get; set; }
            public AlertSeverity Severity  { get; set; }
            public string        Title     { get; set; }
            public string        Message   { get; set; }
            public string        Source    { get; set; }
            public string        DedupeKey { get; set; }
            public DateTime      CreatedAt { get; set; }
            public bool          IsRead    { get; set; }
            public string        ATM_ID    { get; set; }
            public AlertSeverity SeverityLevel { get; set; }
            public string        Severity  { get; set; }
            public DateTime      CreatedAtUtc { get => CreatedAt; set => CreatedAt = value; }
            public DateTime      RaisedAt  { get => CreatedAt; set => CreatedAt = value; }
            public string           SyncId          { get; set; }
            public string           FileName        { get; set; }
            public long             FileSize        { get; set; }
            public long             FileOffset      { get; set; }
            public string           Checksum        { get; set; }
            public string           MD5Hash         { get; set; }
            public string           SHA256Hash      { get; set; }
            public JournalSyncState State           { get; set; }
            public int              ProgressPercent { get; set; }
            public int              RetryCount      { get; set; }
            public string           LocalPath       { get; set; }
            public string           ServerPath      { get; set; }
            public DateTime         UpdatedAtUtc    { get; set; }
            public DateTime?        CompletedAtUtc  { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Description { get; set; }
            public string ATM_Type { get; set; }
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }
            public ATMStatus Status { get; set; }
            public bool IsConnected { get; set; }
            public bool IsSendingData { get; set; }
            public bool IsCSCConnected { get; set; }
            public DateTime LastConnectionTime { get; set; }
            public DateTime LastDataReceived { get; set; }
            public DateTime LastHeartbeat { get; set; }
            public int ATMCache { get; set; }
            public int TotalDispensed { get; set; }
            public SyncStatus SyncState { get; set; }
            public long TotalBytesSent { get; set; }
            public int TotalFilesSynced { get; set; }
            public int TotalLinesSent { get; set; }
            public DateTime LastSyncTime { get; set; }
            public string LastSyncFile { get; set; }
            public string OSVersion { get; set; }
            public string ClientVersion { get; set; }
            public bool SyncTimeEnabled { get; set; }
            public int MessageSizeLines { get; set; }
            public int FilePackageKB { get; set; }
            public string SourcePath { get; set; }
            public string BackupPath { get; set; }
            public bool AutoStart { get; set; }
            public bool RunAsService { get; set; }
            public string NetworkQuality { get; set; }
            public string GetSourcePath() =>
            public void SetSourcePath(string v) =>
            public string GetBackupPath() =>
            public void SetBackupPath(string v) =>
            public ATMCardState GetCardState()
            {
            public Color GetCardColor()
            {
            public string GetStatusLabel()
            {
            public string GetStatusDescription() =>
            public string GetConnectionStatusDescription() =>
            public string GetElapsed(DateTime utcRef)
            {
            public void RecalculateHealthScore()
            {
            public override string ToString() =>
            public string GetStatusColor()
            {
            public SyncStrategy GetSyncStrategy()
            {
            public bool NeedsAlert()
            {
            public string GetLegacySourcePath()
            {
            public string GetLegacyBackupPath()
            {
            public string GetElapsed(DateTime? dt) =>
        }
    
    }
    public enum ConnectionStatus
        {
    
        public enum ATMStatus
        {
    
        public enum ATMType
        {
    
        public enum SyncStatus
        {
    
        public enum AlertSeverity
        {
    
        public enum JournalSyncState
        {
    
        public enum ATMCardState
        {
    
        public partial public class ATMInfo
        {
            public string Icon => Severity
            switch
            {
            AlertSeverity.Emergency => "CRIT",
            AlertSeverity.Critical => "FAIL",
            AlertSeverity.Warning => "WARN",
            _ => "INFO"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity
            switch
            {
            AlertSeverity.Emergency => Color.FromArgb(255, 59, 48),
            AlertSeverity.Critical => Color.FromArgb(255, 59, 48),
            AlertSeverity.Warning => Color.FromArgb(255, 149, 0),
            _ => Color.FromArgb(0, 122, 255)
            };
            public string StateIcon => State
            switch
            {
            JournalSyncState.Pending => "PEND",
            JournalSyncState.Syncing => "SYNC",
            JournalSyncState.ReSyncing => "RSYNC",
            JournalSyncState.Completed => "OK",
            JournalSyncState.Failed => "FAIL",
            JournalSyncState.Archived => "ARCH",
            _ => "?"
            };
            public string StateLabel => State
            switch
            {
            JournalSyncState.Pending => "في الطابور",
            JournalSyncState.Syncing => "قيد المزامنة",
            JournalSyncState.ReSyncing => "إعادة مزامنة",
            JournalSyncState.Completed => "محمّل",
            JournalSyncState.Failed => "فشل",
            JournalSyncState.Archived => "مؤرشف",
            _ => "؟"
            };
            public ATMInfo()
            {
            public class ATMInfo
            {
            // هوية
            public string ATM_ID
            {
            get;
            set;
            }
            public string ATM_Name
            {
            get;
            set;
            }
            public string ATM_Type
            {
            get;
            set;
            }
            public string BranchName
            {
            get;
            set;
            }
            public string Region
            {
            get;
            set;
            }
            public string ATMId
            {
            get => ATM_ID;
            set => ATM_ID = value;
            }
            public string ATMName
            {
            get => ATM_Name;
            set => ATM_Name = value;
            }
            public string IPAddress
            {
            get => ServerIP;
            set => ServerIP = value;
            }
            public ATMType ATMType
            {
            get
            {
            switch (AppConstants.NormalizeATMType(ATM_Type))
            {
            case AppConstants.ATM_TYPE_NCR:
            return ATMType.NCR;
            case AppConstants.ATM_TYPE_GRG:
            return ATMType.GRG;
            case AppConstants.ATM_TYPE_WN:
            return ATMType.WN;
            case AppConstants.ATM_TYPE_DN:
            return ATMType.DieboldNixdorf;
            case AppConstants.ATM_TYPE_HY:
            return ATMType.Hyosung;
            default:
            return ATMType.Other;
            }
            public string Location
            {
            get => Region;
            set => Region = value;
            }
            public string BranchCode
            {
            get;
            set;
            }
            public string ServerIP
            {
            get;
            set;
            }
            public int ServerPort
            {
            get;
            set;
            }
            public string NetworkType
            {
            get;
            set;
            }
            public int Latency_ms
            {
            get;
            set;
            }
            public int Latency
            {
            get => Latency_ms;
            set => Latency_ms = value;
            }
            public ConnectionStatus ConnectionStatus
            {
            get;
            set;
            }
            public ATMStatus Status
            {
            get;
            set;
            }
            public string SessionId
            {
            get;
            set;
            }
            public bool IsSupervisorMode
            {
            get;
            set;
            }
            public bool IsHostConnected
            {
            get;
            set;
            }
            public DateTime ConnectedAtUtc
            {
            get;
            set;
            }
            public DateTime DisconnectedAtUtc
            {
            get;
            set;
            }
            public DateTime LastHeartbeatUtc
            {
            get;
            set;
            }
            public DateTime LastSyncUtc
            {
            get;
            set;
            }
            public DateTime LastDataReceivedUtc
            {
            get;
            set;
            }
            public DateTime LastCommandSentUtc
            {
            get;
            set;
            }
            public DateTime LastConnectionTime
            {
            get => ConnectedAtUtc;
            set => ConnectedAtUtc = value;
            }
            public DateTime LastSyncTime
            {
            get => LastSyncUtc;
            set => LastSyncUtc = value;
            }
            public long TotalSyncedBytes
            {
            get;
            set;
            }
            public long TotalTransactions
            {
            get;
            set;
            }
            public int ConsecutiveSyncFailures
            {
            get;
            set;
            }
            public double SyncSuccessRate
            {
            get;
            set;
            }
            public double ReceiveSpeedKBs
            {
            get;
            set;
            }
            public long JournalSizeToday
            {
            get;
            set;
            }
            public double CpuUsagePercent
            {
            get;
            set;
            }
            public double MemoryUsagePercent
            {
            get;
            set;
            }
            public double DiskUsagePercent
            {
            get;
            set;
            }
            public int HealthScore
            {
            get;
            set;
            }
            public int PendingJournalCount
            {
            get;
            set;
            }
            public double SuccessRate
            {
            get => SyncSuccessRate;
            set => SyncSuccessRate = value;
            }
            public long TotalTransactionsSynced
            {
            get => TotalTransactions;
            set => TotalTransactions = value;
            }
            public int ApprovedTransactions
            {
            get;
            set;
            }
            public int FailedTransactions
            {
            get;
            set;
            }
            public int CardsCaptured
            {
            get;
            set;
            }
            public long CashDispensed
            {
            get;
            set;
            }
            public string LastJournalFile
            {
            get;
            set;
            }
            public string LastErrorCode
            {
            get;
            set;
            }
            public string LastErrorMessage
            {
            get;
            set;
            }
            public string LastTransaction
            {
            get;
            set;
            }
            public string LastError
            {
            get => LastErrorMessage;
            set => LastErrorMessage = value;
            }
            public int TransactionCount
            {
            get => (int)Math.Min(int.MaxValue, TotalTransactions);
            set => TotalTransactions = value;
            }
            public bool IsSyncing
            {
            get => ConnectionStatus == ConnectionStatus.Syncing;
            set
            {
            if (value) ConnectionStatus = ConnectionStatus.Syncing;
            }
            public string ClientVersion
            {
            get;
            set;
            }
            public string OSVersion
            {
            get;
            set;
            }
            public class AlertPayload
            {
            public string AlertId
            {
            get;
            set;
            }
            public AlertSeverity Severity
            {
            get;
            set;
            }
            public string Title
            {
            get;
            set;
            }
            public string Message
            {
            get;
            set;
            }
            public string Source
            {
            get;
            set;
            }
            public string DedupeKey
            {
            get;
            set;
            }
            public DateTime CreatedAt
            {
            get;
            set;
            }
            public bool IsRead
            {
            get;
            set;
            }
            public class JournalSyncRecord
            {
            public string SyncId
            {
            get;
            set;
            }
            public string ATM_ID
            {
            get;
            set;
            }
            public string FileName
            {
            get;
            set;
            }
            public long FileSize
            {
            get;
            set;
            }
            public long FileOffset
            {
            get;
            set;
            }
            public string Checksum
            {
            get;
            set;
            }
            public string MD5Hash
            {
            get;
            set;
            }
            public string SHA256Hash
            {
            get;
            set;
            }
            public JournalSyncState State
            {
            get;
            set;
            }
            public int ProgressPercent
            {
            get;
            set;
            }
            public int RetryCount
            {
            get;
            set;
            }
            public string LocalPath
            {
            get;
            set;
            }
            public string ServerPath
            {
            get;
            set;
            }
            public DateTime CreatedAtUtc
            {
            get;
            set;
            }
            public DateTime UpdatedAtUtc
            {
            get;
            set;
            }
            public DateTime? CompletedAtUtc
            {
            get;
            set;
            }
            public string ATM_Description
            {
            get;
            set;
            }
            public bool IsConnected
            {
            get;
            set;
            }
            public bool IsSendingData
            {
            get;
            set;
            }
            public bool IsCSCConnected
            {
            get;
            set;
            }
            public DateTime LastDataReceived
            {
            get;
            set;
            }
            public DateTime LastHeartbeat
            {
            get;
            set;
            }
            public int ATMCache
            {
            get;
            set;
            }
            public int TotalDispensed
            {
            get;
            set;
            }
            public SyncStatus SyncState
            {
            get;
            set;
            }
            public long TotalBytesSent
            {
            get;
            set;
            }
            public int TotalFilesSynced
            {
            get;
            set;
            }
            public int TotalLinesSent
            {
            get;
            set;
            }
            public string LastSyncFile
            {
            get;
            set;
            }
            public class ClientConfig
            {
            public string ATM_ID
            {
            get;
            set;
            }
            public bool SyncTimeEnabled
            {
            get;
            set;
            }
            public int MessageSizeLines
            {
            get;
            set;
            }
            public int FilePackageKB
            {
            get;
            set;
            }
            public string SourcePath
            {
            get;
            set;
            }
            public string BackupPath
            {
            get;
            set;
            }
            public bool AutoStart
            {
            get;
            set;
            }
            public bool RunAsService
            {
            get;
            set;
            }
            public class ServerConfig
            {
            public int ListenPort
            {
            get;
            set;
            }
            public string StoragePath
            {
            get;
            set;
            }
            public string ArchivePath
            {
            get;
            set;
            }
            public bool AutoArchive
            {
            get;
            set;
            }
            public int MaxConnections
            {
            get;
            set;
            }
            public bool EnableEncryption
            {
            get;
            set;
            }
            public bool EnableCompression
            {
            get;
            set;
            }
            public enum ConnectionStatus
            {
            Disconnected = 0, Connecting = 1, Connected = 2, WaitingReply = 3, Syncing = 4
            }
            public enum ATMStatus
            {
            Unknown = 0,
            Online = 1,
            Idle = 2,
            Supervisor = 3,
            Warning = 4,
            Offline = 5,
            Critical = 6,
            InService = 10,
            ConnectedOnly = 11,
            WaitingResponse = 12,
            OutOfService = 13,
            CriticalFault = 14,
            Fault = 15,
            Maintenance = 16
            }
            public enum ATMType
            {
            NCR,
            GRG,
            WN,
            DieboldNixdorf,
            Hyosung,
            Other
            }
            public enum SyncStatus
            {
            Pending,
            InProgress,
            Syncing,
            Resyncing,
            Completed,
            Failed
            }
            public enum AlertSeverity
            {
            Info = 0, Warning = 1, Critical = 2, Emergency = 3
            }
            public enum JournalSyncState
            {
            Pending = 0, Syncing = 1, ReSyncing = 2, Completed = 3, Failed = 4, Archived = 5
            }
            public enum ATMCardState
            {
            NeverConnected,
            ConnectedActive,
            ConnectedIdle,
            Syncing,
            WaitingReply,
            Supervisor,
            RecentlyDisconnected,
            WarningOffline,
            CriticalOffline
            }
            public string GetSourcePath() =>
            public void SetSourcePath(string v) =>
            public string GetBackupPath() =>
            public void SetBackupPath(string v) =>
            public ATMCardState GetCardState()
            {
            public Color GetCardColor()
            {
            public string GetStatusLabel()
            {
            public string GetStatusDescription() =>
            public string GetConnectionStatusDescription() =>
            public string GetElapsed(DateTime utcRef)
            {
            public void RecalculateHealthScore()
            {
            public override string ToString() =>
            public string GetStatusColor()
            {
            public SyncStrategy GetSyncStrategy()
            {
            public bool NeedsAlert()
            {
        }
    
        public partial public class AlertPayload
        {
            public string Icon => Severity
            switch
            {
            AlertSeverity.Emergency => "CRIT",
            AlertSeverity.Critical => "FAIL",
            AlertSeverity.Warning => "WARN",
            _ => "INFO"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity
            switch
            {
            AlertSeverity.Emergency => Color.FromArgb(255, 59, 48),
            AlertSeverity.Critical => Color.FromArgb(255, 59, 48),
            AlertSeverity.Warning => Color.FromArgb(255, 149, 0),
            _ => Color.FromArgb(0, 122, 255)
            };
            public string AlertId
            {
            get;
            set;
            }
            public AlertSeverity Severity
            {
            get;
            set;
            }
            public string Title
            {
            get;
            set;
            }
            public string Message
            {
            get;
            set;
            }
            public string Source
            {
            get;
            set;
            }
            public string DedupeKey
            {
            get;
            set;
            }
            public DateTime CreatedAt
            {
            get;
            set;
            }
            public bool IsRead
            {
            get;
            set;
            }
        }
    
        public partial public class JournalSyncRecord
        {
            public string StateIcon => State
            switch
            {
            JournalSyncState.Pending => "PEND",
            JournalSyncState.Syncing => "SYNC",
            JournalSyncState.ReSyncing => "RSYNC",
            JournalSyncState.Completed => "OK",
            JournalSyncState.Failed => "FAIL",
            JournalSyncState.Archived => "ARCH",
            _ => "?"
            };
            public string StateLabel => State
            switch
            {
            JournalSyncState.Pending => "في الطابور",
            JournalSyncState.Syncing => "قيد المزامنة",
            JournalSyncState.ReSyncing => "إعادة مزامنة",
            JournalSyncState.Completed => "محمّل",
            JournalSyncState.Failed => "فشل",
            JournalSyncState.Archived => "مؤرشف",
            _ => "؟"
            };
            public string SyncId
            {
            get;
            set;
            }
            public string ATM_ID
            {
            get;
            set;
            }
            public string FileName
            {
            get;
            set;
            }
            public long FileSize
            {
            get;
            set;
            }
            public long FileOffset
            {
            get;
            set;
            }
            public string Checksum
            {
            get;
            set;
            }
            public string MD5Hash
            {
            get;
            set;
            }
            public string SHA256Hash
            {
            get;
            set;
            }
            public JournalSyncState State
            {
            get;
            set;
            }
            public int ProgressPercent
            {
            get;
            set;
            }
            public int RetryCount
            {
            get;
            set;
            }
            public string LocalPath
            {
            get;
            set;
            }
            public string ServerPath
            {
            get;
            set;
            }
            public string Message
            {
            get;
            set;
            }
            public DateTime CreatedAtUtc
            {
            get;
            set;
            }
            public DateTime UpdatedAtUtc
            {
            get;
            set;
            }
            public DateTime? CompletedAtUtc
            {
            get;
            set;
            }
            public string ATM_Name
            {
            get;
            set;
            }
            public string ATM_Description
            {
            get;
            set;
            }
            public string ATM_Type
            {
            get;
            set;
            }
            public string ServerIP
            {
            get;
            set;
            }
            public int ServerPort
            {
            get;
            set;
            }
            public ATMStatus Status
            {
            get;
            set;
            }
            public bool IsConnected
            {
            get;
            set;
            }
            public bool IsSendingData
            {
            get;
            set;
            }
            public bool IsCSCConnected
            {
            get;
            set;
            }
            public DateTime LastConnectionTime
            {
            get;
            set;
            }
            public DateTime LastDataReceived
            {
            get;
            set;
            }
            public DateTime LastHeartbeat
            {
            get;
            set;
            }
            public int ATMCache
            {
            get;
            set;
            }
            public int TotalDispensed
            {
            get;
            set;
            }
            public SyncStatus SyncState
            {
            get;
            set;
            }
            public long TotalBytesSent
            {
            get;
            set;
            }
            public int TotalFilesSynced
            {
            get;
            set;
            }
            public int TotalLinesSent
            {
            get;
            set;
            }
            public DateTime LastSyncTime
            {
            get;
            set;
            }
            public string LastSyncFile
            {
            get;
            set;
            }
            public string OSVersion
            {
            get;
            set;
            }
            public string ClientVersion
            {
            get;
            set;
            }
            public string GetStatusColor()
            {
            public string GetSourcePath()
            {
            public string GetBackupPath()
            {
            public SyncStrategy GetSyncStrategy()
            {
            public bool NeedsAlert()
            {
        }
    
        public partial public class ClientConfig
        {
            public ClientConfig()
            {
            public string ATM_ID
            {
            get;
            set;
            }
            public string ATM_Name
            {
            get;
            set;
            }
            public string ATM_Type
            {
            get;
            set;
            }
            public string ServerIP
            {
            get;
            set;
            }
            public int ServerPort
            {
            get;
            set;
            }
            public bool SyncTimeEnabled
            {
            get;
            set;
            }
            public int MessageSizeLines
            {
            get;
            set;
            }
            public int FilePackageKB
            {
            get;
            set;
            }
            public string SourcePath
            {
            get;
            set;
            }
            public string BackupPath
            {
            get;
            set;
            }
            public bool AutoStart
            {
            get;
            set;
            }
            public bool RunAsService
            {
            get;
            set;
            }
        }
    
        public partial public class ServerConfig
        {
            public ServerConfig()
            {
            public int ListenPort
            {
            get;
            set;
            }
            public string StoragePath
            {
            get;
            set;
            }
            public string ArchivePath
            {
            get;
            set;
            }
            public bool AutoArchive
            {
            get;
            set;
            }
            public int MaxConnections
            {
            get;
            set;
            }
            public bool EnableEncryption
            {
            get;
            set;
            }
            public bool EnableCompression
            {
            get;
            set;
            }
        }
    
    }
    public enum ConnectionStatus  {
    
        public enum ATMStatus
        {
    
        public enum ATMType {
    
        public enum SyncStatus {
    
        public enum AlertSeverity     {
    
        public enum JournalSyncState  {
    
        public enum ATMCardState      {
    
        public enum SyncStrategy {
    
        public partial public class ATMInfo
        {
            public string Icon => Severity switch
            {
            AlertSeverity.Emergency => "CRIT",
            AlertSeverity.Critical  => "FAIL",
            AlertSeverity.Warning   => "WARN",
            _                       => "INFO"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity switch
            {
            AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
            AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
            AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
            _                       => Color.FromArgb(0,   122, 255)
            };
            public string StateIcon => State switch
            {
            JournalSyncState.Pending   => "PEND",
            JournalSyncState.Syncing   => "SYNC",
            JournalSyncState.ReSyncing => "RSYNC",
            JournalSyncState.Completed => "OK",
            JournalSyncState.Failed    => "FAIL",
            JournalSyncState.Archived  => "ARCH",
            _ => "?"
            };
            public string StateLabel => State switch
            {
            JournalSyncState.Pending   => "في الطابور",
            JournalSyncState.Syncing   => "قيد المزامنة",
            JournalSyncState.ReSyncing => "إعادة مزامنة",
            JournalSyncState.Completed => "محمّل",
            JournalSyncState.Failed    => "فشل",
            JournalSyncState.Archived  => "مؤرشف",
            _ => "؟"
            };
            public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;
            private string _sourcePath;
            private string _backupPath;
            public ATMInfo()
            {
            public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
            public enum ATMStatus
            {
            Unknown = 0,
            Online = 1,
            Idle = 2,
            Supervisor = 3,
            Warning = 4,
            Offline = 5,
            Critical = 6,
            InService = 10,
            ConnectedOnly = 11,
            WaitingResponse = 12,
            OutOfService = 13,
            CriticalFault = 14,
            Fault = 15,
            Maintenance = 16
            }
            public enum ATMType { NCR, GRG, WN, DieboldNixdorf, Hyosung, Other }
            public enum SyncStatus { Pending, InProgress, Syncing, Resyncing, Completed, Failed }
            public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
            public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
            public enum ATMCardState      { NeverConnected, ConnectedActive, ConnectedIdle, Syncing, WaitingReply, Supervisor, RecentlyDisconnected, WarningOffline, CriticalOffline }
            public class ATMInfo
            {
            // هوية
            public string ATM_ID        { get; set; }
            public string ATM_Name      { get; set; }
            public string ATM_Type      { get; set; }
            public string BranchName    { get; set; }
            public string Region        { get; set; }
            public string ATMId { get => ATM_ID; set => ATM_ID = value; }
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
            public ATMType ATMType
            {
            get
            {
            switch (AppConstants.NormalizeATMType(ATM_Type))
            {
            case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
            case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
            case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
            case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
            case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
            default:                        return ATMType.Other;
            }
            public string Location { get => Region; set => Region = value; }
            public string BranchCode { get; set; }
            public string ServerIP      { get; set; }
            public int    ServerPort    { get; set; }
            public string NetworkType   { get; set; }
            public int    Latency_ms    { get; set; }
            public int Latency { get => Latency_ms; set => Latency_ms = value; }
            public ConnectionStatus ConnectionStatus { get; set; }
            public ATMStatus        Status           { get; set; }
            public string           SessionId        { get; set; }
            public bool             IsSupervisorMode { get; set; }
            public bool             IsHostConnected  { get; set; }
            public DateTime ConnectedAtUtc       { get; set; }
            public DateTime DisconnectedAtUtc    { get; set; }
            public DateTime LastHeartbeatUtc     { get; set; }
            public DateTime LastSyncUtc          { get; set; }
            public DateTime LastDataReceivedUtc  { get; set; }
            public DateTime LastCommandSentUtc   { get; set; }
            public DateTime LastConnectionTime { get => ConnectedAtUtc; set => ConnectedAtUtc = value; }
            public DateTime LastSyncTime { get => LastSyncUtc; set => LastSyncUtc = value; }
            public long   TotalSyncedBytes         { get; set; }
            public long   TotalTransactions        { get; set; }
            public int    ConsecutiveSyncFailures  { get; set; }
            public double SyncSuccessRate          { get; set; }
            public double ReceiveSpeedKBs          { get; set; }
            public long   JournalSizeToday         { get; set; }
            public double CpuUsagePercent          { get; set; }
            public double MemoryUsagePercent       { get; set; }
            public double DiskUsagePercent         { get; set; }
            public int    HealthScore              { get; set; }
            public int PendingJournalCount { get; set; }
            public double SuccessRate { get => SyncSuccessRate; set => SyncSuccessRate = value; }
            public long TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
            public int  ApprovedTransactions  { get; set; }
            public int  FailedTransactions    { get; set; }
            public int  CardsCaptured         { get; set; }
            public long CashDispensed         { get; set; }
            public string LastJournalFile   { get; set; }
            public string LastErrorCode     { get; set; }
            public string LastErrorMessage  { get; set; }
            public string LastTransaction   { get; set; }
            public string LastError { get => LastErrorMessage; set => LastErrorMessage = value; }
            public int TransactionCount { get => (int)Math.Min(int.MaxValue, TotalTransactions); set => TotalTransactions = value; }
            public bool IsSyncing { get => ConnectionStatus == ConnectionStatus.Syncing; set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            public string ClientVersion { get; set; }
            public string OSVersion     { get; set; }
            public class AlertPayload
            {
            public string        AlertId   { get; set; }
            public AlertSeverity Severity  { get; set; }
            public string        Title     { get; set; }
            public string        Message   { get; set; }
            public string        Source    { get; set; }
            public string        DedupeKey { get; set; }
            public DateTime      CreatedAt { get; set; }
            public bool          IsRead    { get; set; }
            public class JournalSyncRecord
            {
            public string           SyncId          { get; set; }
            public string           ATM_ID          { get; set; }
            public string           FileName        { get; set; }
            public long             FileSize        { get; set; }
            public long             FileOffset      { get; set; }
            public string           Checksum        { get; set; }
            public string           MD5Hash         { get; set; }
            public string           SHA256Hash      { get; set; }
            public JournalSyncState State           { get; set; }
            public int              ProgressPercent { get; set; }
            public int              RetryCount      { get; set; }
            public string           LocalPath       { get; set; }
            public string           ServerPath      { get; set; }
            public DateTime         CreatedAtUtc    { get; set; }
            public DateTime         UpdatedAtUtc    { get; set; }
            public DateTime?        CompletedAtUtc  { get; set; }
            public string ATM_Description { get; set; }
            public bool IsConnected { get; set; }
            public bool IsSendingData { get; set; }
            public bool IsCSCConnected { get; set; }
            public DateTime LastDataReceived { get; set; }
            public DateTime LastHeartbeat { get; set; }
            public int ATMCache { get; set; }
            public int TotalDispensed { get; set; }
            public SyncStatus SyncState { get; set; }
            public long TotalBytesSent { get; set; }
            public int TotalFilesSynced { get; set; }
            public int TotalLinesSent { get; set; }
            public string LastSyncFile { get; set; }
            public class ClientConfig
            {
            public string ATM_ID { get; set; }
            public bool SyncTimeEnabled { get; set; }
            public int MessageSizeLines { get; set; }
            public int FilePackageKB { get; set; }
            public string SourcePath { get; set; }
            public string BackupPath { get; set; }
            public bool AutoStart { get; set; }
            public bool RunAsService { get; set; }
            public class ServerConfig
            {
            public int ListenPort { get; set; }
            public string StoragePath { get; set; }
            public string ArchivePath { get; set; }
            public bool AutoArchive { get; set; }
            public int MaxConnections { get; set; }
            public bool EnableEncryption { get; set; }
            public bool EnableCompression { get; set; }
            public string ATMType { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string Branch { get; set; }
            public string SourceJournalPath { get; set; }
            public string ImageInboxPath { get; set; }
            public string ImageDestPath { get; set; }
            public DateTime LastSync { get; set; }
            public string Name { get; set; }
            public int TotalTransactionsSynced { get => (int)TotalTransactions; set => TotalTransactions = value; }
            public bool HasCashTelemetry { get; set; }
            public long Cassette1Remaining { get; set; }
            public long Cassette2Remaining { get; set; }
            public long Cassette3Remaining { get; set; }
            public long Cassette4Remaining { get; set; }
            public long ATMCache { get; set; }
            public long CashLoadedTotal { get; set; }
            public long TotalDispensed { get; set; }
            public long CashDepositInTotal { get; set; }
            public long CashRejectCount { get; set; }
            public long CashRetractCount { get; set; }
            public DateTime CashTelemetryUpdatedAtUtc { get; set; }
            public bool HasAlerts { get; set; }
            public string GetSourcePath() =>
            public void SetSourcePath(string v) =>
            public string GetBackupPath() =>
            public void SetBackupPath(string v) =>
            public ATMCardState GetCardState()
            {
            public Color GetCardColor()
            {
            public string GetStatusLabel()
            {
            public string GetStatusDescription() =>
            public string GetConnectionStatusDescription() =>
            public string GetElapsed(DateTime utcRef)
            {
            public void RecalculateHealthScore()
            {
            public override string ToString() =>
            public string GetStatusColor()
            {
            public SyncStrategy GetSyncStrategy()
            {
            public bool NeedsAlert()
            {
            public string GetLegacySourcePath()
            {
            public string GetLegacyBackupPath()
            {
            public string GetElapsed(DateTime? dt) =>
        }
    
        public partial public class AlertPayload
        {
            public string Icon => Severity switch
            {
            AlertSeverity.Emergency => "CRIT",
            AlertSeverity.Critical  => "FAIL",
            AlertSeverity.Warning   => "WARN",
            _                       => "INFO"
            };
            public string SeverityIcon => Icon;
            public Color Color => Severity switch
            {
            AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
            AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
            AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
            _                       => Color.FromArgb(0,   122, 255)
            };
            public string        AlertId   { get; set; }
            public AlertSeverity Severity  { get; set; }
            public string        Title     { get; set; }
            public string        Message   { get; set; }
            public string        Source    { get; set; }
            public string        DedupeKey { get; set; }
            public DateTime      CreatedAt { get; set; }
            public bool          IsRead    { get; set; }
            public string        ATM_ID    { get; set; }
            public AlertSeverity SeverityLevel { get; set; }
            public string        Severity  { get; set; }
            public DateTime      CreatedAtUtc { get => CreatedAt; set => CreatedAt = value; }
            public DateTime      RaisedAt  { get => CreatedAt; set => CreatedAt = value; }
        }
    
        public partial public class JournalSyncRecord
        {
            public string StateIcon => State switch
            {
            JournalSyncState.Pending   => "PEND",
            JournalSyncState.Syncing   => "SYNC",
            JournalSyncState.ReSyncing => "RSYNC",
            JournalSyncState.Completed => "OK",
            JournalSyncState.Failed    => "FAIL",
            JournalSyncState.Archived  => "ARCH",
            _ => "?"
            };
            public string StateLabel => State switch
            {
            JournalSyncState.Pending   => "في الطابور",
            JournalSyncState.Syncing   => "قيد المزامنة",
            JournalSyncState.ReSyncing => "إعادة مزامنة",
            JournalSyncState.Completed => "محمّل",
            JournalSyncState.Failed    => "فشل",
            JournalSyncState.Archived  => "مؤرشف",
            _ => "؟"
            };
            public string           SyncId          { get; set; }
            public string           ATM_ID          { get; set; }
            public string           FileName        { get; set; }
            public long             FileSize        { get; set; }
            public long             FileOffset      { get; set; }
            public string           Checksum        { get; set; }
            public string           MD5Hash         { get; set; }
            public string           SHA256Hash      { get; set; }
            public JournalSyncState State           { get; set; }
            public int              ProgressPercent { get; set; }
            public int              RetryCount      { get; set; }
            public string           LocalPath       { get; set; }
            public string           ServerPath      { get; set; }
            public string           Message         { get; set; }
            public DateTime         CreatedAtUtc    { get; set; }
            public DateTime         UpdatedAtUtc    { get; set; }
            public DateTime?        CompletedAtUtc  { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Description { get; set; }
            public string ATM_Type { get; set; }
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }
            public ATMStatus Status { get; set; }
            public bool IsConnected { get; set; }
            public bool IsSendingData { get; set; }
            public bool IsCSCConnected { get; set; }
            public DateTime LastConnectionTime { get; set; }
            public DateTime LastDataReceived { get; set; }
            public DateTime LastHeartbeat { get; set; }
            public int ATMCache { get; set; }
            public int TotalDispensed { get; set; }
            public SyncStatus SyncState { get; set; }
            public long TotalBytesSent { get; set; }
            public int TotalFilesSynced { get; set; }
            public int TotalLinesSent { get; set; }
            public DateTime LastSyncTime { get; set; }
            public string LastSyncFile { get; set; }
            public string OSVersion { get; set; }
            public string ClientVersion { get; set; }
            public string GetStatusColor()
            {
            public string GetSourcePath()
            {
            public string GetBackupPath()
            {
            public SyncStrategy GetSyncStrategy()
            {
            public bool NeedsAlert()
            {
        }
    
        public partial public class ClientConfig
        {
            public ClientConfig()
            {
            public string ATM_ID { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }
            public bool SyncTimeEnabled { get; set; }
            public int MessageSizeLines { get; set; }
            public int FilePackageKB { get; set; }
            public string SourcePath { get; set; }
            public string BackupPath { get; set; }
            public bool AutoStart { get; set; }
            public bool RunAsService { get; set; }
            public string NetworkQuality { get; set; }
        }
    
        public partial public class ServerConfig
        {
            public ServerConfig()
            {
            public int ListenPort { get; set; }
            public string StoragePath { get; set; }
            public string ArchivePath { get; set; }
            public bool AutoArchive { get; set; }
            public int MaxConnections { get; set; }
            public bool EnableEncryption { get; set; }
            public bool EnableCompression { get; set; }
        }
    
    }
    public partial class JournalSyncRecord
        {
            public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string           ATM_ID          { get; set; }
    
    
            public string           FileName        { get; set; }
    
    
            public long             FileSize        { get; set; }
    
    
            public long             FileOffset      { get; set; }
    
    
            public string           Checksum        { get; set; }
    
    
            public string           MD5Hash         { get; set; }
    
    
            public string           SHA256Hash      { get; set; }
    
    
            public JournalSyncState State           { get; set; }
    
    
            public int              ProgressPercent { get; set; }
    
    
            public int              RetryCount      { get; set; }
    
    
            public string           LocalPath       { get; set; }
    
    
            public string           ServerPath      { get; set; }
    
    
            public string           Message         { get; set; }
    
    
            public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
    
    
            public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
    
    
            public DateTime?        CompletedAtUtc  { get; set; }
    
    
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
    
    
            public string StateLabel => State switch
            {
                JournalSyncState.Pending   => "في الطابور",
                JournalSyncState.Syncing   => "قيد المزامنة",
                JournalSyncState.ReSyncing => "إعادة مزامنة",
                JournalSyncState.Completed => "محمّل",
                JournalSyncState.Failed    => "فشل",
                JournalSyncState.Archived  => "مؤرشف",
                _                          => "؟"
            };
    
    
        }
    public partial class JournalSyncRecord
        {
            public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string           ATM_ID          { get; set; }
    
    
            public string           FileName        { get; set; }
    
    
            public long             FileSize        { get; set; }
    
    
            public long             FileOffset      { get; set; }
    
    
            public string           Checksum        { get; set; }
    
    
            public string           MD5Hash         { get; set; }
    
    
            public string           SHA256Hash      { get; set; }
    
    
            public JournalSyncState State           { get; set; }
    
    
            public int              ProgressPercent { get; set; }
    
    
            public int              RetryCount      { get; set; }
    
    
            public string           LocalPath       { get; set; }
    
    
            public string           ServerPath      { get; set; }
    
    
            public string           Message         { get; set; }
    
    
            public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
    
    
            public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
    
    
            public DateTime?        CompletedAtUtc  { get; set; }
    
    
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "⏳",
                JournalSyncState.Syncing   => "🔄",
                JournalSyncState.ReSyncing => "♻️",
                JournalSyncState.Completed => "✅",
                JournalSyncState.Failed    => "❌",
                JournalSyncState.Archived  => "📦",
                _ => "?"
            };
    
    
            public string StateLabel => State switch
            {
                JournalSyncState.Pending   => "في الطابور",
                JournalSyncState.Syncing   => "قيد المزامنة",
                JournalSyncState.ReSyncing => "إعادة مزامنة",
                JournalSyncState.Completed => "محمّل",
                JournalSyncState.Failed    => "فشل",
                JournalSyncState.Archived  => "مؤرشف",
                _ => "؟"
            };
    
    
        }
    // ==========================================
        // سجل مزامنة الجورنال
        // ==========================================
    
        public class JournalSyncRecord
        {
            public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
            public string           ATM_ID          { get; set; }
            public string           FileName        { get; set; }
            public long             FileSize        { get; set; }
            public long             FileOffset      { get; set; }
            public string           Checksum        { get; set; }
            public string           MD5Hash         { get; set; }
            public string           SHA256Hash      { get; set; }
            public JournalSyncState State           { get; set; }
            public int              ProgressPercent { get; set; }
            public int              RetryCount      { get; set; }
            public string           LocalPath       { get; set; }
            public string           ServerPath      { get; set; }
            public string           Message         { get; set; }
            public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
            public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
            public DateTime?        CompletedAtUtc  { get; set; }
    
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "PEND",
                JournalSyncState.Syncing   => "SYNC",
                JournalSyncState.ReSyncing => "RSYNC",
                JournalSyncState.Completed => "OK",
                JournalSyncState.Failed    => "FAIL",
                JournalSyncState.Archived  => "ARCH",
                _                          => "?"
            };
    
            public string StateLabel => State switch
            {
                JournalSyncState.Pending   => "في الطابور",
                JournalSyncState.Syncing   => "قيد المزامنة",
                JournalSyncState.ReSyncing => "إعادة مزامنة",
                JournalSyncState.Completed => "محمّل",
                JournalSyncState.Failed    => "فشل",
                JournalSyncState.Archived  => "مؤرشف",
                _                          => "؟"
            };
        }
    // ==========================================
        // سجل المزامنة
        // ==========================================
    
        public class JournalSyncRecord
        {
            public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
            public string           ATM_ID          { get; set; }
            public string           FileName        { get; set; }
            public long             FileSize        { get; set; }
            public long             FileOffset      { get; set; }
            public string           Checksum        { get; set; }
            public string           MD5Hash         { get; set; }
            public string           SHA256Hash      { get; set; }
            public JournalSyncState State           { get; set; }
            public int              ProgressPercent { get; set; }
            public int              RetryCount      { get; set; }
            public string           LocalPath       { get; set; }
            public string           ServerPath      { get; set; }
            public string           Message         { get; set; }
            public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
            public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
            public DateTime?        CompletedAtUtc  { get; set; }
    
            public string StateIcon => State switch
            {
                JournalSyncState.Pending   => "⏳",
                JournalSyncState.Syncing   => "🔄",
                JournalSyncState.ReSyncing => "♻️",
                JournalSyncState.Completed => "✅",
                JournalSyncState.Failed    => "❌",
                JournalSyncState.Archived  => "📦",
                _ => "?"
            };
    
            public string StateLabel => State switch
            {
                JournalSyncState.Pending   => "في الطابور",
                JournalSyncState.Syncing   => "قيد المزامنة",
                JournalSyncState.ReSyncing => "إعادة مزامنة",
                JournalSyncState.Completed => "محمّل",
                JournalSyncState.Failed    => "فشل",
                JournalSyncState.Archived  => "مؤرشف",
                _ => "؟"
            };
        }
    public partial class ServerConfig
        {
            public int ListenPort { get; set; }
    
    
            public string StoragePath { get; set; }
    
    
            public string ArchivePath { get; set; }
    
    
            public bool AutoArchive { get; set; }
    
    
            public int MaxConnections { get; set; }
    
    
            public bool EnableEncryption { get; set; }
    
    
            public bool EnableCompression { get; set; }
    
    
            public ServerConfig()
            {
                ListenPort = NetworkConfig.DEFAULT_PORT;
                StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                AutoArchive = true;
                MaxConnections = 100;
                EnableEncryption = true;
                EnableCompression = true;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            public ServerConfig()
            {
                ListenPort = AppConstants.DefaultPort;
                StoragePath = @"C:\EJLive\Storage";
                ArchivePath = @"C:\EJLive\Archive";
                AutoArchive = true;
                MaxConnections = 100;
                EnableEncryption = true;
                EnableCompression = true;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
            public ServerConfig()
            {
                ListenPort = AppConstants.DefaultPort;
                StoragePath = @"C:\EJLive\Storage";
                ArchivePath = @"C:\EJLive\Archive";
                AutoArchive = true;
                MaxConnections = 100;
                EnableEncryption = true;
                EnableCompression = true;
            }
    
    
        }
    public partial class ServerConfig
        {
            public int ListenPort { get; set; }
    
    
            public string StoragePath { get; set; }
    
    
            public string ArchivePath { get; set; }
    
    
            public bool AutoArchive { get; set; }
    
    
            public int MaxConnections { get; set; }
    
    
            public bool EnableEncryption { get; set; }
    
    
            public bool EnableCompression { get; set; }
    
    
            public ServerConfig()
            {
                ListenPort = NetworkConfig.DEFAULT_PORT;
                StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                AutoArchive = true;
                MaxConnections = 100;
                EnableEncryption = true;
                EnableCompression = true;
            }
    
    
        }
    // ==========================================
        // تكوين الخادم
        // ==========================================
    
        public class ServerConfig
        {
            public int ListenPort { get; set; }
            public string StoragePath { get; set; }
            public string ArchivePath { get; set; }
            public bool AutoArchive { get; set; }
            public int MaxConnections { get; set; }
            public bool EnableEncryption { get; set; }
            public bool EnableCompression { get; set; }
    
            public ServerConfig()
            {
                ListenPort = AppConstants.DefaultPort;
                StoragePath = @"C:\EJLive\Storage";
                ArchivePath = @"C:\EJLive\Archive";
                AutoArchive = true;
                MaxConnections = 100;
                EnableEncryption = true;
                EnableCompression = true;
            }
        }
    // ==========================================
        // إعدادات الخادم
        // ==========================================
    
        public class ServerConfig
        {
            public int    ListenPort         { get; set; }
            public string StoragePath        { get; set; }
            public string ArchivePath        { get; set; }
            public bool   AutoArchive        { get; set; }
            public int    MaxConnections     { get; set; }
            public bool   EnableEncryption   { get; set; }
            public bool   EnableCompression  { get; set; }
    
            public ServerConfig()
            {
                ListenPort        = NetworkConfig.DEFAULT_PORT;
                StoragePath       = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                ArchivePath       = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                AutoArchive       = true;
                MaxConnections    = 100;
                EnableEncryption  = true;
                EnableCompression = true;
            }
        }
    public class ServerConfig
        {
            public int ListenPort { get; set; }
            public string StoragePath { get; set; }
            public string ArchivePath { get; set; }
            public bool AutoArchive { get; set; }
            public int MaxConnections { get; set; }
            public bool EnableEncryption { get; set; }
            public bool EnableCompression { get; set; }
    
            public ServerConfig()
            {
                ListenPort = NetworkConfig.DEFAULT_PORT;
                StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                AutoArchive = true;
                MaxConnections = 100;
                EnableEncryption = true;
                EnableCompression = true;
            }
        }

    // Class: AlertPayload (from 4 sources)
        public partial class AlertPayload
        {
            // --- Properties ---
                    public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
    
                    public AlertSeverity Severity { get; set; }
    
                    public string Title { get; set; }
    
                    public string Message { get; set; }
    
                    public string Source { get; set; }
    
                    public string DedupeKey { get; set; }
    
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
                    public bool IsRead { get; set; }
    
                    public string Icon => Severity switch
                    {
                        AlertSeverity.Emergency => "CRIT",
                        AlertSeverity.Critical => "FAIL",
                        AlertSeverity.Warning => "WARN",
                        _ => "INFO"
                    };
    
                    public Color Color => Severity switch
                    {
                        AlertSeverity.Emergency => Color.FromArgb(255, 59, 48),
                        AlertSeverity.Critical => Color.FromArgb(255, 59, 48),
                        AlertSeverity.Warning => Color.FromArgb(255, 149, 0),
                        _ => Color.FromArgb(0, 122, 255)
                    };
    
                    public string        ATM_ID    { get; set; }
    
                    public AlertSeverity SeverityLevel { get; set; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public string        Severity  { get; set; } = "Info";
    
                    public DateTime      CreatedAtUtc { get => CreatedAt; set => CreatedAt = value; }
    
                    public DateTime      RaisedAt  { get => CreatedAt; set => CreatedAt = value; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public string Icon => SeverityLevel switch
                    {
                        AlertSeverity.Emergency => "CRIT",
                        AlertSeverity.Critical  => "FAIL",
                        AlertSeverity.Warning   => "WARN",
                        _                       => "INFO"
                    };
    
                    public string SeverityIcon => Icon;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public Color Color => SeverityLevel switch
                    {
                        AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                        AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                        AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                        _                       => Color.FromArgb(0,   122, 255)
                    };
    
    
        }
    // Class: AlertPayload (from 2 sources)
        public partial class AlertPayload
        {
            // --- Properties ---
                            public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
    
                            public AlertSeverity Severity  { get; set; }
    
                            public string        Title     { get; set; }
    
                            public string        Message   { get; set; }
    
                            public string        Source    { get; set; }
    
                            public string        DedupeKey { get; set; }
    
                            public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
    
                            public bool          IsRead    { get; set; }
    
                            public string Icon => Severity switch
                            {
                                AlertSeverity.Emergency => "🚨",
                                AlertSeverity.Critical  => "❌",
                                AlertSeverity.Warning   => "⚠️",
                                _                       => "ℹ️"
                            };
    
                            public string SeverityIcon => Icon;
    
                            public Color Color => Severity switch
                            {
                                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                                _                       => Color.FromArgb(0,   122, 255)
                            };
    
    
        }
    // Class: AlertPayload (from 2 sources)
        public partial class AlertPayload
        {
            // --- Properties ---
                    public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string        ATM_ID    { get; set; }
    
                    public AlertSeverity SeverityLevel { get; set; }
    
                    public string        Severity  { get; set; } = "Info";
    
                    public string        Title     { get; set; }
    
                    public string        Message   { get; set; }
    
                    public string        Source    { get; set; }
    
                    public string        DedupeKey { get; set; }
    
                    public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
    
                    public DateTime      CreatedAtUtc { get => CreatedAt; set => CreatedAt = value; }
    
                    public DateTime      RaisedAt  { get => CreatedAt; set => CreatedAt = value; }
    
                    public bool          IsRead    { get; set; }
    
                    public string Icon => SeverityLevel switch
                    {
                        AlertSeverity.Emergency => "CRIT",
                        AlertSeverity.Critical  => "FAIL",
                        AlertSeverity.Warning   => "WARN",
                        _                       => "INFO"
                    };
    
                    public string SeverityIcon => Icon;
    
                    public Color Color => SeverityLevel switch
                    {
                        AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                        AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                        AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                        _                       => Color.FromArgb(0,   122, 255)
                    };
    
    
        }
    // Class: AlertPayload (from 1 sources)
        public partial class AlertPayload
        {
            // --- Properties ---
                    public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string        ATM_ID    { get; set; }
    
                    public AlertSeverity SeverityLevel { get; set; }
    
                    public string        Severity  { get; set; } = "Info";
    
                    public string        Title     { get; set; }
    
                    public string        Message   { get; set; }
    
                    public string        Source    { get; set; }
    
                    public string        DedupeKey { get; set; }
    
                    public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
    
                    public DateTime      CreatedAtUtc { get => CreatedAt; set => CreatedAt = value; }
    
                    public DateTime      RaisedAt  { get => CreatedAt; set => CreatedAt = value; }
    
                    public bool          IsRead    { get; set; }
    
                    public string Icon => SeverityLevel switch
                    {
                        AlertSeverity.Emergency => "CRIT",
                        AlertSeverity.Critical  => "FAIL",
                        AlertSeverity.Warning   => "WARN",
                        _                       => "INFO"
                    };
    
                    public string SeverityIcon => Icon;
    
                    public Color Color => SeverityLevel switch
                    {
                        AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                        AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                        AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                        _                       => Color.FromArgb(0,   122, 255)
                    };
    
    
        }
    // ═══ Class: AlertPayload (from 1 sources) ═══
        public partial class AlertPayload
        {
            // --- Properties ---
                    public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
    
                    public AlertSeverity Severity  { get; set; }
    
                    public string        Title     { get; set; }
    
                    public string        Message   { get; set; }
    
                    public string        Source    { get; set; }
    
                    public string        DedupeKey { get; set; }
    
                    public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
    
                    public bool          IsRead    { get; set; }
    
                    public string Icon => Severity switch
                    {
                        AlertSeverity.Emergency => "🚨",
                        AlertSeverity.Critical  => "❌",
                        AlertSeverity.Warning   => "⚠️",
                        _                       => "ℹ️"
                    };
    
                    public string SeverityIcon => Icon;
    
                    public Color Color => Severity switch
                    {
                        AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                        AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                        AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                        _                       => Color.FromArgb(0,   122, 255)
                    };
    
    
        }
    // ==========================================
        // التنبيه الموسّع — دمج ثلاثة مصادر:
        //   1. ATMInfo.cs القديم (AlertSeverity enum, Icon, Color)
        //   2. AnalysisRuntimeModels (AlertId, ATM_ID, Severity string, Message, CreatedAtUtc)
        //   3. CompatibilityModels (Title, RaisedAt)
        // ==========================================
    
        public class AlertPayload
        {
            public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
            public string        ATM_ID    { get; set; }
            public AlertSeverity SeverityLevel { get; set; }
            public string        Severity  { get; set; } = "Info";
            public string        Title     { get; set; }
            public string        Message   { get; set; }
            public string        Source    { get; set; }
            public string        DedupeKey { get; set; }
            public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime      CreatedAtUtc { get => CreatedAt; set => CreatedAt = value; }
            /// <summary>alias للتوافق مع CompatibilityModels</summary>
            public DateTime      RaisedAt  { get => CreatedAt; set => CreatedAt = value; }
            public bool          IsRead    { get; set; }
    
            public string Icon => SeverityLevel switch
            {
                AlertSeverity.Emergency => "CRIT",
                AlertSeverity.Critical  => "FAIL",
                AlertSeverity.Warning   => "WARN",
                _                       => "INFO"
            };
    
            public string SeverityIcon => Icon;
    
            public Color Color => SeverityLevel switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                _                       => Color.FromArgb(0,   122, 255)
            };
        }
    // Enum: AlertSeverity (from 6 sources)
        public partial enum AlertSeverity
        {
            // --- Constants & Fields ---
                    Info      = 0,
    
                    Warning   = 1,
    
                    Critical  = 2,
    
                    Emergency = 3
    
    
        }
    // Enum: AlertSeverity (from 2 sources)
        public partial enum AlertSeverity
        {
        }
    // Enum: AlertSeverity (from 2 sources)
        public partial enum AlertSeverity
        {
            // --- Constants & Fields ---
                    Info      = 0,
    
                    Warning   = 1,
    
                    Critical  = 2,
    
                    Emergency = 3
    
    
        }
    // Enum: AlertSeverity (from 1 sources)
        public partial enum AlertSeverity
        {
            // --- Constants & Fields ---
                    Info      = 0,
    
                    Warning   = 1,
    
                    Critical  = 2,
    
                    Emergency = 3
    
    
        }
    // ═══ Enum: AlertSeverity (from 1 sources) ═══
        public partial enum AlertSeverity
        {
        }
    // Enum: ATMCardState (from 6 sources)
        public partial enum ATMCardState
        {
            // --- Constants & Fields ---
                    NeverConnected,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
                    CriticalOffline
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    CriticalOffline
    
    
        }
    // Enum: ATMCardState (from 2 sources)
        public partial enum ATMCardState
        {
        }
    // Enum: ATMCardState (from 2 sources)
        public partial enum ATMCardState
        {
            // --- Constants & Fields ---
                    NeverConnected,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
                    CriticalOffline
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
                    CriticalOffline
    
    
        }
    // Enum: ATMCardState (from 2 sources)
        public partial enum ATMCardState
        {
            // --- Constants & Fields ---
                            NeverConnected,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Models\ATMInfo.cs
                            CriticalOffline
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Models\ATMInfo.cs
                    CriticalOffline
    
    
        }
    // Enum: ATMCardState (from 1 sources)
        public partial enum ATMCardState
        {
            // --- Constants & Fields ---
                    NeverConnected,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Models\ATMInfo.cs
                    CriticalOffline
    
    
        }
    // ═══ Enum: ATMCardState (from 1 sources) ═══
        public partial enum ATMCardState
        {
        }
    // Class: ATMInfo (from 8 sources)
        public partial class ATMInfo
        {
            // --- Constants & Fields ---
                    private string _sourcePath;
    
                    private string _backupPath;
    
                    public ATMType ATMType
                    {
                        get
                        {
                            switch (AppConstants.NormalizeATMType(ATM_Type))
                            {
                                case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                                case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                                case AppConstants.ATM_TYPE_WN: return ATMType.WN;
                                case AppConstants.ATM_TYPE_DN: return ATMType.DieboldNixdorf;
                                case AppConstants.ATM_TYPE_HY: return ATMType.Hyosung;
                                default: return ATMType.Other;
                            }
                        }
                        set
                        {
                            ATM_Type = value switch
                            {
                                ATMType.NCR => AppConstants.ATM_TYPE_NCR,
                                ATMType.GRG => AppConstants.ATM_TYPE_GRG,
                                ATMType.WN => AppConstants.ATM_TYPE_WN,
                                ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                                ATMType.Hyosung => AppConstants.ATM_TYPE_HY,
                                _ => "OTHER"
                            };
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public bool IsSyncing
                    {
                        get => ConnectionStatus == ConnectionStatus.Syncing;
                        set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    private string _sourcePath, _backupPath;
    
    
            // --- Properties ---
                    public string ATM_ID { get; set; }
    
                    public string ATM_Name { get; set; }
    
                    public string ATM_Description { get; set; }
    
                    public string ATM_Type { get; set; }
    
                    public string ServerIP { get; set; }
    
                    public int ServerPort { get; set; }
    
                    public ATMStatus Status { get; set; }
    
                    public bool IsConnected { get; set; }
    
                    public bool IsSendingData { get; set; }
    
                    public bool IsCSCConnected { get; set; }
    
                    public DateTime LastConnectionTime { get; set; }
    
                    public DateTime LastDataReceived { get; set; }
    
                    public DateTime LastHeartbeat { get; set; }
    
                    public int[,] OperationStats { get; set; }
    
                    public int ATMCache { get; set; }
    
                    public int TotalDispensed { get; set; }
    
                    public SyncStatus SyncState { get; set; }
    
                    public long TotalBytesSent { get; set; }
    
                    public int TotalFilesSynced { get; set; }
    
                    public int TotalLinesSent { get; set; }
    
                    public DateTime LastSyncTime { get; set; }
    
                    public string LastSyncFile { get; set; }
    
                    public string OSVersion { get; set; }
    
                    public string ClientVersion { get; set; }
    
                    public string ATMId { get; set; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
                    public string ATM_ID { get => ATMId; set => ATMId = value; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
                    public string ATM_Name { get => Name; set => Name = value; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
                    public string ServerIP { get => IPAddress; set => IPAddress = value; }
    
                    public DateTime LastHeartbeatUtc { get => LastHeartbeat; set => LastHeartbeat = value; }
    
                    public long TotalSyncedBytes { get; set; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
                    public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;
    
                    public string Name { get; set; } = string.Empty;
    
                    public string BranchName { get; set; } = string.Empty;
    
                    public string Region { get; set; } = string.Empty;
    
                    public string Location { get => Region; set => Region = value; }
    
                    public string BranchCode { get; set; } = string.Empty;
    
                    public string IPAddress { get; set; }
    
                    public string NetworkType { get; set; } = "LAN";
    
                    public int Latency { get; set; }
    
                    public int Latency_ms { get => Latency; set => Latency = value; }
    
                    public bool IsSupervisorMode { get; set; }
    
                    public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    
                    public string SessionId { get; set; }
    
                    public bool IsHostConnected { get; set; }
    
                    public DateTime ConnectedAtUtc { get; set; }
    
                    public DateTime DisconnectedAtUtc { get; set; }
    
                    public DateTime LastSyncUtc { get; set; }
    
                    public DateTime LastDataReceivedUtc { get; set; }
    
                    public DateTime LastCommandSentUtc { get; set; }
    
                    public long TotalTransactions { get; set; }
    
                    public int ConsecutiveSyncFailures { get; set; }
    
                    public double SyncSuccessRate { get; set; } = 100.0;
    
                    public double ReceiveSpeedKBs { get; set; }
    
                    public long JournalSizeToday { get; set; }
    
                    public double CpuUsagePercent { get; set; }
    
                    public double MemoryUsagePercent { get; set; }
    
                    public double DiskUsagePercent { get; set; }
    
                    public int HealthScore { get; set; } = 100;
    
                    public int PendingJournalCount { get; set; }
    
                    public string ATMName { get => Name; set => Name = value; }
    
                    public double SuccessRate { get => SyncSuccessRate; set => SyncSuccessRate = value; }
    
                    public int TotalTransactionsSynced { get => (int)TotalTransactions; set => TotalTransactions = value; }
    
                    public int ApprovedTransactions { get; set; }
    
                    public int FailedTransactions { get; set; }
    
                    public int CardsCaptured { get; set; }
    
                    public long CashDispensed { get; set; }
    
                    public string LastJournalFile { get; set; }
    
                    public string LastErrorCode { get; set; }
    
                    public string LastError { get; set; }
    
                    public string LastErrorMessage { get => LastError; set => LastError = value; }
    
                    public bool HasCashTelemetry { get; set; }
    
                    public long Cassette1Remaining { get; set; }
    
                    public long Cassette2Remaining { get; set; }
    
                    public long Cassette3Remaining { get; set; }
    
                    public long Cassette4Remaining { get; set; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
                    public long ATMCache { get; set; }
    
                    public long CashLoadedTotal { get; set; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
                    public long TotalDispensed { get; set; }
    
                    public long CashDepositInTotal { get; set; }
    
                    public long CashRejectCount { get; set; }
    
                    public long CashRetractCount { get; set; }
    
                    public DateTime CashTelemetryUpdatedAtUtc { get; set; }
    
                    public string LastTransaction { get; set; }
    
                    public bool HasAlerts { get; set; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Core\Models\ATMInfo.cs
                    public string ATMName { get; set; }
    
                    public string ATMType { get; set; }
    
                    public string Vendor { get; set; }
    
                    public string Model { get; set; }
    
                    public string Branch { get; set; }
    
                    public string SourceJournalPath { get; set; }
    
                    public string BackupPath { get; set; }
    
                    public string ImageInboxPath { get; set; }
    
                    public string ImageDestPath { get; set; }
    
                    public DateTime LastSync { get; set; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public string ATMId   { get => ATM_ID;   set => ATM_ID   = value; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public int    Latency_ms  { get; set; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public int    Latency     { get => Latency_ms; set => Latency_ms = value; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public DateTime LastHeartbeatUtc    { get; set; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public long   TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public string LastErrorMessage  { get; set; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
    
            // --- Constructors ---
                    public ATMInfo()
                    {
                        OperationStats = new int[3, 4];
                        Status = ATMStatus.Unknown;
                        SyncState = SyncStatus.Idle;
                        ServerPort = NetworkConfig.DEFAULT_PORT;
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
                    public ATMInfo()
                    {
                        ATMId = string.Empty;
                        IPAddress = string.Empty;
                        ServerPort = AppConstants.DefaultPort;
                    }
    
    
            // --- Methods ---
                    public string GetStatusColor()
                    {
                        if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                        if (!IsConnected)
                        {
                            var elapsed = DateTime.Now - LastConnectionTime;
                            if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                                return ATMStatusColors.COLOR_OFFLINE;
                            if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                                return ATMStatusColors.COLOR_WARNING;
                            return ATMStatusColors.COLOR_OFFLINE;
                        }
                        if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                        var idleElapsed = DateTime.Now - LastDataReceived;
                        if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                            return ATMStatusColors.COLOR_IDLE;
                        return ATMStatusColors.COLOR_ACTIVE;
                    }
    
                    public string GetSourcePath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                            default: return string.Empty;
                        }
                    }
    
                    public string GetBackupPath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                            default: return string.Empty;
                        }
                    }
    
                    public SyncStrategy GetSyncStrategy()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                            case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                            case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                            default: return SyncStrategy.NCR_Overwrite;
                        }
                    }
    
                    public bool NeedsAlert()
                    {
                        if (!IsConnected) return true;
                        return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                    }
    
                    public string GetElapsed(DateTime? dt) => !dt.HasValue ? "—" : $"{(DateTime.UtcNow - dt.Value).TotalMinutes:N0}m";
    
                    public string GetStatusDescription() => Status.ToString();
    
                    public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
                    public bool NeedsAlert() => HasAlerts;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
                    public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath)
                        ? _sourcePath
                        : AppConstants.GetDefaultSourcePath(ATM_Type);
    
                    public void SetSourcePath(string v) => _sourcePath = v;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
                    public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                        : System.IO.Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                            "EJLive", "LocalBackup", ATMId ?? "DEFAULT");
    
                    public void SetBackupPath(string v) => _backupPath = v;
    
                    public ATMCardState GetCardState()
                    {
                        if (ConnectionStatus == ConnectionStatus.Disconnected)
                        {
                            if (LastHeartbeat == DateTime.MinValue) return ATMCardState.NeverConnected;
                            var mins = (DateTime.UtcNow - LastHeartbeat).TotalMinutes;
                            if (mins > 10) return ATMCardState.CriticalOffline;
                            if (mins > 5) return ATMCardState.WarningOffline;
                            return ATMCardState.RecentlyDisconnected;
                        }
                        if (ConnectionStatus == ConnectionStatus.Syncing) return ATMCardState.Syncing;
                        if (IsSupervisorMode) return ATMCardState.Supervisor;
                        if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                        var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                        return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                    }
    
                    public Color GetCardColor()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive => Color.FromArgb(52, 199, 89),
                            ATMCardState.ConnectedIdle => Color.FromArgb(255, 214, 10),
                            ATMCardState.Syncing => Color.FromArgb(10, 132, 255),
                            ATMCardState.WaitingReply => Color.FromArgb(10, 132, 255),
                            ATMCardState.Supervisor => Color.FromArgb(255, 159, 10),
                            ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69, 58),
                            ATMCardState.WarningOffline => Color.FromArgb(255, 69, 58),
                            ATMCardState.CriticalOffline => Color.FromArgb(99, 99, 102),
                            ATMCardState.NeverConnected => Color.FromArgb(72, 72, 74),
                            _ => Color.Gray
                        };
                    }
    
                    public string GetStatusLabel()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive => "● متصل ونشط",
                            ATMCardState.ConnectedIdle => "● متصل خامل",
                            ATMCardState.Syncing => "⟳ يزامن",
                            ATMCardState.WaitingReply => "◎ ينتظر رد",
                            ATMCardState.Supervisor => "★ Supervisor",
                            ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                            ATMCardState.WarningOffline => "✕ انقطاع >5د",
                            ATMCardState.CriticalOffline => "✕ انقطاع حرج",
                            ATMCardState.NeverConnected => "○ لم يتصل",
                            _ => "?"
                        };
                    }
    
                    public void RecalculateHealthScore()
                    {
                        var score = 100;
                        if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                        if (Latency > 500) score -= 15;
                        if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                        if (CpuUsagePercent > 90) score -= 10;
                        if (MemoryUsagePercent > 90) score -= 10;
                        if (DiskUsagePercent > 95) score -= 10;
                        HealthScore = Math.Max(0, Math.Min(100, score));
                    }
    
                    public override string ToString() =>
                        $"[{ATMId}] {Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public string GetSourcePath()
                    {
                        if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                        return AppConstants.GetDefaultSourcePath(ATM_Type);
                    }
    
                    public string GetLegacySourcePath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                            case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_SOURCE;
                            default:                        return string.Empty;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public string GetBackupPath()
                    {
                        if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                        return System.IO.Path.Combine(
                            System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                            "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
                    }
    
                    public string GetLegacyBackupPath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                            case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_BACKUP;
                            default:                        return string.Empty;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public ATMCardState GetCardState()
                    {
                        if (ConnectionStatus == ConnectionStatus.Disconnected)
                        {
                            if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                            var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                            if (mins > 10) return ATMCardState.CriticalOffline;
                            if (mins > 5)  return ATMCardState.WarningOffline;
                            return ATMCardState.RecentlyDisconnected;
                        }
                        if (ConnectionStatus == ConnectionStatus.Syncing)      return ATMCardState.Syncing;
                        if (IsSupervisorMode)                                   return ATMCardState.Supervisor;
                        if (ConnectionStatus == ConnectionStatus.WaitingReply)  return ATMCardState.WaitingReply;
                        var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                        return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public string GetStatusDescription()        => GetStatusLabel();
    
                    public string GetElapsed(DateTime utcRef)
                    {
                        if (utcRef == DateTime.MinValue) return "---";
                        var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                        if (s < 60)   return $"{(int)s}ث";
                        if (s < 3600) return $"{(int)(s / 60)}د";
                        return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public SyncStrategy GetSyncStrategy()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                            case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                            case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                            case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                            case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                            default:                        return SyncStrategy.NCR_Overwrite;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public void RecalculateHealthScore()
                    {
                        var score = 100;
                        if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                        if (Latency_ms > 500) score -= 15;
                        if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                        if (CpuUsagePercent    > 90) score -= 10;
                        if (MemoryUsagePercent > 90) score -= 10;
                        if (DiskUsagePercent   > 95) score -= 10;
                        HealthScore = Math.Max(0, Math.Min(100, score));
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    public override string ToString() =>
                        $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
    
        }
    // Class: ATMInfo (from 2 sources)
        public partial class ATMInfo
        {
            // --- Properties ---
                    public string ATM_ID { get; set; }
    
                    public string ATM_Name { get; set; }
    
                    public string ATM_Description { get; set; }
    
                    public string ATM_Type { get; set; }
    
                    public string ServerIP { get; set; }
    
                    public int ServerPort { get; set; }
    
                    public ATMStatus Status { get; set; }
    
                    public bool IsConnected { get; set; }
    
                    public bool IsSendingData { get; set; }
    
                    public bool IsCSCConnected { get; set; }
    
                    public DateTime LastConnectionTime { get; set; }
    
                    public DateTime LastDataReceived { get; set; }
    
                    public DateTime LastHeartbeat { get; set; }
    
                    public int[,] OperationStats { get; set; }
    
                    public int ATMCache { get; set; }
    
                    public int TotalDispensed { get; set; }
    
                    public SyncStatus SyncState { get; set; }
    
                    public long TotalBytesSent { get; set; }
    
                    public int TotalFilesSynced { get; set; }
    
                    public int TotalLinesSent { get; set; }
    
                    public DateTime LastSyncTime { get; set; }
    
                    public string LastSyncFile { get; set; }
    
                    public string OSVersion { get; set; }
    
                    public string ClientVersion { get; set; }
    
    
            // --- Constructors ---
                    public ATMInfo()
                    {
                        OperationStats = new int[3, 4];
                        Status = ATMStatus.Unknown;
                        SyncState = SyncStatus.Idle;
                        ServerPort = NetworkConfig.DEFAULT_PORT;
                    }
    
    
            // --- Methods ---
                    public string GetStatusColor()
                    {
                        if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                        if (!IsConnected)
                        {
                            var elapsed = DateTime.Now - LastConnectionTime;
                            if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                                return ATMStatusColors.COLOR_OFFLINE;
                            if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                                return ATMStatusColors.COLOR_WARNING;
                            return ATMStatusColors.COLOR_OFFLINE;
                        }
                        if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                        var idleElapsed = DateTime.Now - LastDataReceived;
                        if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                            return ATMStatusColors.COLOR_IDLE;
                        return ATMStatusColors.COLOR_ACTIVE;
                    }
    
                    public string GetSourcePath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                            default: return string.Empty;
                        }
                    }
    
                    public string GetBackupPath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                            default: return string.Empty;
                        }
                    }
    
                    public SyncStrategy GetSyncStrategy()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                            case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                            case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                            default: return SyncStrategy.NCR_Overwrite;
                        }
                    }
    
                    public bool NeedsAlert()
                    {
                        if (!IsConnected) return true;
                        return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                    }
    
    
        }
    // Class: ATMInfo (from 3 sources)
        public partial class ATMInfo
        {
            // --- Nested Classes ---
                public class ATMInfo
                {
                    // هوية
                    public string ATM_ID        { get; set; }
                    public string ATM_Name      { get; set; }
                    public string ATM_Type      { get; set; }    // NCR / GRG / WN / DIEBOLD / HYOSUNG
                    public string BranchName    { get; set; }
                    public string Region        { get; set; }
                    public string ATMId { get => ATM_ID; set => ATM_ID = value; }
                    public string ATMName { get => ATM_Name; set => ATM_Name = value; }
                    public string IPAddress { get => ServerIP; set => ServerIP = value; }
                    public ATMType ATMType
                    {
                        get
                        {
                            switch (AppConstants.NormalizeATMType(ATM_Type))
                            {
                                case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                                case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                                case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
                                case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
                                case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
                                default:                        return ATMType.Other;
                            }
                        }
                        set
                        {
                            ATM_Type = value switch
                            {
                                ATMType.NCR            => AppConstants.ATM_TYPE_NCR,
                                ATMType.GRG            => AppConstants.ATM_TYPE_GRG,
                                ATMType.WN             => AppConstants.ATM_TYPE_WN,
                                ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                                ATMType.Hyosung        => AppConstants.ATM_TYPE_HY,
                                _                      => "OTHER"
                            };
                        }
                    }
                    public string Location { get => Region; set => Region = value; }
                    public string BranchCode { get; set; }
    
                    // شبكة
                    public string ServerIP      { get; set; }
                    public int    ServerPort    { get; set; } = 5656;
                    public string NetworkType   { get; set; } = "LAN";
                    public int    Latency_ms    { get; set; }
                    public int Latency { get => Latency_ms; set => Latency_ms = value; }
    
                    // حالة الاتصال
                    public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
                    public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
                    public string           SessionId        { get; set; }
                    public bool             IsSupervisorMode { get; set; }
                    public bool             IsHostConnected  { get; set; }
    
                    // طوابع زمنية UTC (T-11)
                    public DateTime ConnectedAtUtc       { get; set; }
                    public DateTime DisconnectedAtUtc    { get; set; }
                    public DateTime LastHeartbeatUtc     { get; set; }
                    public DateTime LastSyncUtc          { get; set; }
                    public DateTime LastDataReceivedUtc  { get; set; }
                    public DateTime LastCommandSentUtc   { get; set; }
                    public DateTime LastConnectionTime { get => ConnectedAtUtc; set => ConnectedAtUtc = value; }
                    public DateTime LastSyncTime { get => LastSyncUtc; set => LastSyncUtc = value; }
    
                    // إحصاءات المزامنة
                    public long   TotalSyncedBytes         { get; set; }
                    public long   TotalTransactions        { get; set; }
                    public int    ConsecutiveSyncFailures  { get; set; }
                    public double SyncSuccessRate          { get; set; } = 100.0;
                    public double ReceiveSpeedKBs          { get; set; }
                    public long   JournalSizeToday         { get; set; }
                    public double CpuUsagePercent          { get; set; }
                    public double MemoryUsagePercent       { get; set; }
                    public double DiskUsagePercent         { get; set; }
                    public int    HealthScore              { get; set; } = 100;
                    public int PendingJournalCount { get; set; }
                    public double SuccessRate { get => SyncSuccessRate; set => SyncSuccessRate = value; }
                    public long TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
                    // إحصاءات العمليات
                    public int  ApprovedTransactions  { get; set; }
                    public int  FailedTransactions    { get; set; }
                    public int  CardsCaptured         { get; set; }
                    public long CashDispensed         { get; set; }
    
                    // آخر جورنال / خطأ
                    public string LastJournalFile   { get; set; }
                    public string LastErrorCode     { get; set; }
                    public string LastErrorMessage  { get; set; }
                    public string LastTransaction   { get; set; }
                    public string LastError { get => LastErrorMessage; set => LastErrorMessage = value; }
                    public int TransactionCount { get => (int)Math.Min(int.MaxValue, TotalTransactions); set => TotalTransactions = value; }
                    public bool IsSyncing { get => ConnectionStatus == ConnectionStatus.Syncing; set { if (value) ConnectionStatus = ConnectionStatus.Syncing; } }
    
                    // مسارات
                    private string _sourcePath, _backupPath;
                    public string ClientVersion { get; set; }
                    public string OSVersion     { get; set; }
    
                    public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath)
                        ? _sourcePath
                        : AppConstants.GetDefaultSourcePath(ATM_Type);
    
                    public void SetSourcePath(string v) => _sourcePath = v;
    
                    public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                        : System.IO.Path.Combine(
                            System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                            "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
    
                    public void SetBackupPath(string v) => _backupPath = v;
    
                    // ==========================================
                    // حالة البطاقة ولونها
                    // ==========================================
    
                    public ATMCardState GetCardState()
                    {
                        if (ConnectionStatus == ConnectionStatus.Disconnected)
                        {
                            if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                            var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                            if (mins > 10) return ATMCardState.CriticalOffline;
                            if (mins > 5)  return ATMCardState.WarningOffline;
                            return ATMCardState.RecentlyDisconnected;
                        }
                        if (ConnectionStatus == ConnectionStatus.Syncing)     return ATMCardState.Syncing;
                        if (IsSupervisorMode)                                 return ATMCardState.Supervisor;
                        if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                        var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                        return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                    }
    
                    public Color GetCardColor()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),   // أخضر
                            ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),   // أصفر
                            ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),  // أزرق
                            ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),  // أزرق
                            ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),   // برتقالي
                            ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),   // أحمر
                            ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),   // أحمر
                            ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),  // رمادي
                            ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),   // رمادي داكن
                            _ => Color.Gray
                        };
                    }
    
                    public string GetStatusLabel()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive      => "● متصل ونشط",
                            ATMCardState.ConnectedIdle        => "● متصل خامل",
                            ATMCardState.Syncing              => "⟳ يزامن",
                            ATMCardState.WaitingReply         => "◎ ينتظر رد",
                            ATMCardState.Supervisor           => "★ Supervisor",
                            ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                            ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                            ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                            ATMCardState.NeverConnected       => "○ لم يتصل",
                            _ => "?"
                        };
                    }
    
                    public string GetStatusDescription() => GetStatusLabel();
    
                    public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
                    public string GetElapsed(DateTime utcRef)
                    {
                        if (utcRef == DateTime.MinValue) return "---";
                        var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                        if (s < 60)   return $"{(int)s}ث";
                        if (s < 3600) return $"{(int)(s/60)}د";
                        return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
                    }
    
                    public void RecalculateHealthScore()
                    {
                        var score = 100;
                        if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                        if (Latency_ms > 500) score -= 15;
                        if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                        if (CpuUsagePercent > 90) score -= 10;
                        if (MemoryUsagePercent > 90) score -= 10;
                        if (DiskUsagePercent > 95) score -= 10;
                        HealthScore = Math.Max(0, Math.Min(100, score));
                    }
    
                    public override string ToString() =>
                        $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
                }
    
                public class AlertPayload
                {
                    public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
                    public AlertSeverity Severity  { get; set; }
                    public string        Title     { get; set; }
                    public string        Message   { get; set; }
                    public string        Source    { get; set; }
                    public string        DedupeKey { get; set; }
                    public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
                    public bool          IsRead    { get; set; }
    
                    public string Icon => Severity switch
                    {
                        AlertSeverity.Emergency => "CRIT",
                        AlertSeverity.Critical  => "FAIL",
                        AlertSeverity.Warning   => "WARN",
                        _                       => "INFO"
                    };
                    public string SeverityIcon => Icon;
                    public Color Color => Severity switch
                    {
                        AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                        AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                        AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                        _                       => Color.FromArgb(0,   122, 255)
                    };
                }
    
                public class JournalSyncRecord
                {
                    public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
                    public string           ATM_ID          { get; set; }
                    public string           FileName        { get; set; }
                    public long             FileSize        { get; set; }
                    public long             FileOffset      { get; set; }
                    public string           Checksum        { get; set; }
                    public string           MD5Hash         { get; set; }
                    public string           SHA256Hash      { get; set; }
                    public JournalSyncState State           { get; set; }
                    public int              ProgressPercent { get; set; }
                    public int              RetryCount      { get; set; }
                    public string           LocalPath       { get; set; }
                    public string           ServerPath      { get; set; }
                    public string           Message         { get; set; }
                    public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
                    public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
                    public DateTime?        CompletedAtUtc  { get; set; }
    
                    public string StateIcon => State switch
                    {
                        JournalSyncState.Pending   => "PEND",
                        JournalSyncState.Syncing   => "SYNC",
                        JournalSyncState.ReSyncing => "RSYNC",
                        JournalSyncState.Completed => "OK",
                        JournalSyncState.Failed    => "FAIL",
                        JournalSyncState.Archived  => "ARCH",
                        _ => "?"
                    };
    
                    public string StateLabel => State switch
                    {
                        JournalSyncState.Pending   => "في الطابور",
                        JournalSyncState.Syncing   => "قيد المزامنة",
                        JournalSyncState.ReSyncing => "إعادة مزامنة",
                        JournalSyncState.Completed => "محمّل",
                        JournalSyncState.Failed    => "فشل",
                        JournalSyncState.Archived  => "مؤرشف",
                        _ => "؟"
                    };
    
                    public string ATM_ID { get; set; }
                    public string ATM_Name { get; set; }
                    public string ATM_Description { get; set; }
                    public string ATM_Type { get; set; }
                    public string ServerIP { get; set; }
                    public int ServerPort { get; set; }
                    public ATMStatus Status { get; set; }
                    public bool IsConnected { get; set; }
                    public bool IsSendingData { get; set; }
                    public bool IsCSCConnected { get; set; }
                    public DateTime LastConnectionTime { get; set; }
                    public DateTime LastDataReceived { get; set; }
                    public DateTime LastHeartbeat { get; set; }
                    public int[,] OperationStats { get; set; }
                    public int ATMCache { get; set; }
                    public int TotalDispensed { get; set; }
                    public SyncStatus SyncState { get; set; }
                    public long TotalBytesSent { get; set; }
                    public int TotalFilesSynced { get; set; }
                    public int TotalLinesSent { get; set; }
                    public DateTime LastSyncTime { get; set; }
                    public string LastSyncFile { get; set; }
                    public string OSVersion { get; set; }
                    public string ClientVersion { get; set; }
    
                    public ATMInfo()
                    {
                        OperationStats = new int[3, 4];
                        Status = ATMStatus.Unknown;
                        SyncState = SyncStatus.Idle;
                        ServerPort = NetworkConfig.DEFAULT_PORT;
                    }
    
                    public string GetStatusColor()
                    {
                        if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                        if (!IsConnected)
                        {
                            var elapsed = DateTime.Now - LastConnectionTime;
                            if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                                return ATMStatusColors.COLOR_OFFLINE;
                            if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                                return ATMStatusColors.COLOR_WARNING;
                            return ATMStatusColors.COLOR_OFFLINE;
                        }
                        if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                        var idleElapsed = DateTime.Now - LastDataReceived;
                        if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                            return ATMStatusColors.COLOR_IDLE;
                        return ATMStatusColors.COLOR_ACTIVE;
                    }
    
                    public string GetSourcePath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                            default: return string.Empty;
                        }
                    }
    
                    public string GetBackupPath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                            default: return string.Empty;
                        }
                    }
    
                    public SyncStrategy GetSyncStrategy()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                            case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                            case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                            default: return SyncStrategy.NCR_Overwrite;
                        }
                    }
    
                    public bool NeedsAlert()
                    {
                        if (!IsConnected) return true;
                        return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                    }
                }
    
                public class ClientConfig
                {
                    public string ATM_ID { get; set; }
                    public string ATM_Name { get; set; }
                    public string ATM_Type { get; set; }
                    public string ServerIP { get; set; }
                    public int ServerPort { get; set; }
                    public bool SyncTimeEnabled { get; set; }
                    public int MessageSizeLines { get; set; }
                    public int FilePackageKB { get; set; }
                    public string SourcePath { get; set; }
                    public string BackupPath { get; set; }
                    public bool AutoStart { get; set; }
                    public bool RunAsService { get; set; }
    
                    public ClientConfig()
                    {
                        ServerPort = NetworkConfig.DEFAULT_PORT;
                        MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                        FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                        AutoStart = true;
                        RunAsService = true;
                    }
                }
    
                public class ServerConfig
                {
                    public int ListenPort { get; set; }
                    public string StoragePath { get; set; }
                    public string ArchivePath { get; set; }
                    public bool AutoArchive { get; set; }
                    public int MaxConnections { get; set; }
                    public bool EnableEncryption { get; set; }
                    public bool EnableCompression { get; set; }
    
                    public ServerConfig()
                    {
                        ListenPort = NetworkConfig.DEFAULT_PORT;
                        StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                        ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                        AutoArchive = true;
                        MaxConnections = 100;
                        EnableEncryption = true;
                        EnableCompression = true;
                    }
                }
    
    
            // --- Nested Enums ---
                public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
                public enum ATMStatus
                {
                    Unknown = 0,
                    Online = 1,
                    Idle = 2,
                    Supervisor = 3,
                    Warning = 4,
                    Offline = 5,
                    Critical = 6,
                    InService = 10,
                    ConnectedOnly = 11,
                    WaitingResponse = 12,
                    OutOfService = 13,
                    CriticalFault = 14,
                    Fault = 15,
                    Maintenance = 16
                }
    
                public enum ATMType { NCR, GRG, WN, DieboldNixdorf, Hyosung, Other }
    
                public enum SyncStatus { Pending, InProgress, Syncing, Resyncing, Completed, Failed }
    
                public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
                public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
                public enum ATMCardState      { NeverConnected, ConnectedActive, ConnectedIdle, Syncing, WaitingReply, Supervisor, RecentlyDisconnected, WarningOffline, CriticalOffline }
    
    
        }
    // Class: ATMInfo (from 6 sources)
        public partial class ATMInfo
        {
            // --- Constants & Fields ---
                            private string _sourcePath, _backupPath;
    
    
            // --- Properties ---
                            public string ATM_ID        { get; set; }
    
                            public string ATM_Name      { get; set; }
    
                            public string ATM_Type      { get; set; }    // NCR / GRG / WN
    
                            public string BranchName    { get; set; }
    
                            public string Region        { get; set; }
    
                            public string ServerIP      { get; set; }
    
                            public int    ServerPort    { get; set; } = 5656;
    
                            public string NetworkType   { get; set; } = "LAN";
    
                            public int    Latency_ms    { get; set; }
    
                            public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    
                            public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
    
                            public string           SessionId        { get; set; }
    
                            public bool             IsSupervisorMode { get; set; }
    
                            public bool             IsHostConnected  { get; set; }
    
                            public DateTime ConnectedAtUtc       { get; set; }
    
                            public DateTime DisconnectedAtUtc    { get; set; }
    
                            public DateTime LastHeartbeatUtc     { get; set; }
    
                            public DateTime LastSyncUtc          { get; set; }
    
                            public DateTime LastDataReceivedUtc  { get; set; }
    
                            public DateTime LastCommandSentUtc   { get; set; }
    
                            public long   TotalSyncedBytes         { get; set; }
    
                            public long   TotalTransactions        { get; set; }
    
                            public int    ConsecutiveSyncFailures  { get; set; }
    
                            public double SyncSuccessRate          { get; set; } = 100.0;
    
                            public double ReceiveSpeedKBs          { get; set; }
    
                            public long   JournalSizeToday         { get; set; }
    
                            public int  ApprovedTransactions  { get; set; }
    
                            public int  FailedTransactions    { get; set; }
    
                            public int  CardsCaptured         { get; set; }
    
                            public long CashDispensed         { get; set; }
    
                            public string LastJournalFile   { get; set; }
    
                            public string LastErrorCode     { get; set; }
    
                            public string LastErrorMessage  { get; set; }
    
                            public string LastTransaction   { get; set; }
    
                            public string ClientVersion { get; set; }
    
                            public string OSVersion     { get; set; }
    
                            public string ATM_Description { get; set; }
    
                            public bool IsConnected { get; set; }
    
                            public bool IsSendingData { get; set; }
    
                            public bool IsCSCConnected { get; set; }
    
                            public DateTime LastConnectionTime { get; set; }
    
                            public DateTime LastDataReceived { get; set; }
    
                            public DateTime LastHeartbeat { get; set; }
    
                            public int[,] OperationStats { get; set; }
    
                            public int ATMCache { get; set; }
    
                            public int TotalDispensed { get; set; }
    
                            public SyncStatus SyncState { get; set; }
    
                            public long TotalBytesSent { get; set; }
    
                            public int TotalFilesSynced { get; set; }
    
                            public int TotalLinesSent { get; set; }
    
                            public DateTime LastSyncTime { get; set; }
    
                            public string LastSyncFile { get; set; }
    
    
            // --- Constructors ---
                            public ATMInfo()
                            {
                                OperationStats = new int[3, 4];
                                Status = ATMStatus.Unknown;
                                SyncState = SyncStatus.Idle;
                                ServerPort = NetworkConfig.DEFAULT_PORT;
                            }
    
    
            // --- Methods ---
                            public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath) ? _sourcePath
                                : ATM_Type == "NCR" ? @"C:\NCRJournal\"
                                : ATM_Type == "GRG" ? @"D:\GRGData\EJ\"
                                : ATM_Type == "WN"  ? @"C:\WOSA\EJ\"
                                : @"C:\Journal\";
    
                            public void SetSourcePath(string v) => _sourcePath = v;
    
                            public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                                : System.IO.Path.Combine(
                                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
    
                            public void SetBackupPath(string v) => _backupPath = v;
    
                            public ATMCardState GetCardState()
                            {
                                if (ConnectionStatus == ConnectionStatus.Disconnected)
                                {
                                    if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                                    var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                                    if (mins > 10) return ATMCardState.CriticalOffline;
                                    if (mins > 5)  return ATMCardState.WarningOffline;
                                    return ATMCardState.RecentlyDisconnected;
                                }
                                if (ConnectionStatus == ConnectionStatus.Syncing)     return ATMCardState.Syncing;
                                if (IsSupervisorMode)                                 return ATMCardState.Supervisor;
                                if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                            }
    
                            public Color GetCardColor()
                            {
                                return GetCardState() switch
                                {
                                    ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),   // أخضر
                                    ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),   // أصفر
                                    ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),  // أزرق
                                    ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),  // أزرق
                                    ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),   // برتقالي
                                    ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),   // أحمر
                                    ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),   // أحمر
                                    ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),  // رمادي
                                    ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),   // رمادي داكن
                                    _ => Color.Gray
                                };
                            }
    
                            public string GetStatusLabel()
                            {
                                return GetCardState() switch
                                {
                                    ATMCardState.ConnectedActive      => "● متصل ونشط",
                                    ATMCardState.ConnectedIdle        => "● متصل خامل",
                                    ATMCardState.Syncing              => "⟳ يزامن",
                                    ATMCardState.WaitingReply         => "◎ ينتظر رد",
                                    ATMCardState.Supervisor           => "★ Supervisor",
                                    ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                                    ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                                    ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                                    ATMCardState.NeverConnected       => "○ لم يتصل",
                                    _ => "?"
                                };
                            }
    
                            public string GetElapsed(DateTime utcRef)
                            {
                                if (utcRef == DateTime.MinValue) return "---";
                                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                                if (s < 60)   return $"{(int)s}ث";
                                if (s < 3600) return $"{(int)(s/60)}د";
                                return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
                            }
    
                            public override string ToString() =>
                                $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
                            public string GetStatusColor()
                            {
                                if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                                if (!IsConnected)
                                {
                                    var elapsed = DateTime.Now - LastConnectionTime;
                                    if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                                        return ATMStatusColors.COLOR_OFFLINE;
                                    if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                                        return ATMStatusColors.COLOR_WARNING;
                                    return ATMStatusColors.COLOR_OFFLINE;
                                }
                                if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                                var idleElapsed = DateTime.Now - LastDataReceived;
                                if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                                    return ATMStatusColors.COLOR_IDLE;
                                return ATMStatusColors.COLOR_ACTIVE;
                            }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Models\ATMInfo.cs
                            public string GetSourcePath()
                            {
                                switch (ATM_Type)
                                {
                                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                                    default: return string.Empty;
                                }
                            }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Models\ATMInfo.cs
                            public string GetBackupPath()
                            {
                                switch (ATM_Type)
                                {
                                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                                    default: return string.Empty;
                                }
                            }
    
                            public SyncStrategy GetSyncStrategy()
                            {
                                switch (ATM_Type)
                                {
                                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                                    case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                                    default: return SyncStrategy.NCR_Overwrite;
                                }
                            }
    
                            public bool NeedsAlert()
                            {
                                if (!IsConnected) return true;
                                return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                            }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Models\ATMInfo.cs
                    public string GetSourcePath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                            default: return string.Empty;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Models\ATMInfo.cs
                    public string GetBackupPath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                            default: return string.Empty;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Models\ATMInfo.cs
                    public string GetSourcePath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                            default: return string.Empty;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Models\ATMInfo.cs
                    public string GetBackupPath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                            default: return string.Empty;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Models\ATMInfo.cs
                    public string GetSourcePath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                            default: return string.Empty;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Models\ATMInfo.cs
                    public string GetBackupPath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                            default: return string.Empty;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Models\ATMInfo.cs
                    public string GetSourcePath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                            default: return string.Empty;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Models\ATMInfo.cs
                    public string GetBackupPath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                            default: return string.Empty;
                        }
                    }
    
    
        }
    // Class: ATMInfo (from 2 sources)
        public partial class ATMInfo
        {
            // --- Constants & Fields ---
                    public ATMType ATMType
                    {
                        get
                        {
                            switch (AppConstants.NormalizeATMType(ATM_Type))
                            {
                                case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                                case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                                case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
                                case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
                                case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
                                default:                        return ATMType.Other;
                            }
                        }
                        set
                        {
                            ATM_Type = value switch
                            {
                                ATMType.NCR            => AppConstants.ATM_TYPE_NCR,
                                ATMType.GRG            => AppConstants.ATM_TYPE_GRG,
                                ATMType.WN             => AppConstants.ATM_TYPE_WN,
                                ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                                ATMType.Hyosung        => AppConstants.ATM_TYPE_HY,
                                _                      => "OTHER"
                            };
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
                    public bool IsSyncing
                    {
                        get => ConnectionStatus == ConnectionStatus.Syncing;
                        set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
                    }
    
                    private string _sourcePath, _backupPath;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
                    public bool IsSyncing
                    {
                        get => ConnectionStatus == ConnectionStatus.Syncing;
                        set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
                    }
    
    
            // --- Properties ---
                    public string ATM_ID        { get; set; }
    
                    public string ATM_Name      { get; set; }
    
                    public string ATM_Type      { get; set; }   // NCR / GRG / WN / DIEBOLD / HYOSUNG
    
                    public string BranchName    { get; set; }
    
                    public string Region        { get; set; }
    
                    public string ATM_Description { get; set; } // من النسخة القديمة
    
                    public string ATMId   { get => ATM_ID;   set => ATM_ID   = value; }
    
                    public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
                    public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
                    public string Location  { get => Region;   set => Region   = value; }
    
                    public string BranchCode { get; set; }
    
                    public string ServerIP    { get; set; }
    
                    public int    ServerPort  { get; set; } = 5656;
    
                    public string NetworkType { get; set; } = "LAN";
    
                    public int    Latency_ms  { get; set; }
    
                    public int    Latency     { get => Latency_ms; set => Latency_ms = value; }
    
                    public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    
                    public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
    
                    public string           SessionId        { get; set; }
    
                    public bool             IsSupervisorMode { get; set; }
    
                    public bool             IsHostConnected  { get; set; }
    
                    public bool     IsSendingData  { get; set; }   // من النسخة القديمة
    
                    public bool     IsCSCConnected { get; set; }   // من النسخة القديمة
    
                    public DateTime ConnectedAtUtc      { get; set; }
    
                    public DateTime DisconnectedAtUtc   { get; set; }
    
                    public DateTime LastHeartbeatUtc    { get; set; }
    
                    public DateTime LastSyncUtc         { get; set; }
    
                    public DateTime LastDataReceivedUtc { get; set; }
    
                    public DateTime LastCommandSentUtc  { get; set; }
    
                    public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
                    public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
                    public DateTime LastDataReceived   { get; set; }
    
                    public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
                    public long   TotalSyncedBytes        { get; set; }
    
                    public long   TotalTransactions       { get; set; }
    
                    public int    ConsecutiveSyncFailures { get; set; }
    
                    public double SyncSuccessRate         { get; set; } = 100.0;
    
                    public double ReceiveSpeedKBs         { get; set; }
    
                    public long   JournalSizeToday        { get; set; }
    
                    public double CpuUsagePercent         { get; set; }
    
                    public double MemoryUsagePercent      { get; set; }
    
                    public double DiskUsagePercent        { get; set; }
    
                    public int    HealthScore             { get; set; } = 100;
    
                    public int    PendingJournalCount     { get; set; }
    
                    public double SuccessRate           { get => SyncSuccessRate;   set => SyncSuccessRate   = value; }
    
                    public long   TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
                    public int[,]  OperationStats    { get; set; }
    
                    public int     ATMCache          { get; set; }
    
                    public int     TotalDispensed    { get; set; }
    
                    public SyncStatus SyncState      { get; set; }
    
                    public long    TotalBytesSent    { get; set; }
    
                    public int     TotalFilesSynced  { get; set; }
    
                    public int     TotalLinesSent    { get; set; }
    
                    public string  LastSyncFile      { get; set; }
    
                    public int  ApprovedTransactions { get; set; }
    
                    public int  FailedTransactions   { get; set; }
    
                    public int  CardsCaptured        { get; set; }
    
                    public long CashDispensed        { get; set; }
    
                    public string LastJournalFile   { get; set; }
    
                    public string LastErrorCode     { get; set; }
    
                    public string LastErrorMessage  { get; set; }
    
                    public string LastTransaction   { get; set; }
    
                    public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
                    public string  ClientVersion { get; set; }
    
                    public string  OSVersion     { get; set; }
    
    
            // --- Constructors ---
                    public ATMInfo()
                    {
                        OperationStats = new int[3, 4];
                        Status         = ATMStatus.Unknown;
                        SyncState      = SyncStatus.Idle;
                        ServerPort     = NetworkConfig.DEFAULT_PORT;
                    }
    
    
            // --- Methods ---
                    public string GetSourcePath()
                    {
                        if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                        return AppConstants.GetDefaultSourcePath(ATM_Type);
                    }
    
                    public string GetLegacySourcePath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                            case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_SOURCE;
                            default:                        return string.Empty;
                        }
                    }
    
                    public void SetSourcePath(string v) => _sourcePath = v;
    
                    public string GetBackupPath()
                    {
                        if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                        return System.IO.Path.Combine(
                            System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                            "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
                    }
    
                    public string GetLegacyBackupPath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                            case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_BACKUP;
                            default:                        return string.Empty;
                        }
                    }
    
                    public void SetBackupPath(string v) => _backupPath = v;
    
                    public ATMCardState GetCardState()
                    {
                        if (ConnectionStatus == ConnectionStatus.Disconnected)
                        {
                            if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                            var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                            if (mins > 10) return ATMCardState.CriticalOffline;
                            if (mins > 5)  return ATMCardState.WarningOffline;
                            return ATMCardState.RecentlyDisconnected;
                        }
                        if (ConnectionStatus == ConnectionStatus.Syncing)      return ATMCardState.Syncing;
                        if (IsSupervisorMode)                                   return ATMCardState.Supervisor;
                        if (ConnectionStatus == ConnectionStatus.WaitingReply)  return ATMCardState.WaitingReply;
                        var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                        return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                    }
    
                    public Color GetCardColor()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),
                            ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),
                            ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),
                            ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),
                            ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),
                            ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),
                            ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),
                            ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),
                            ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),
                            _                                 => Color.Gray
                        };
                    }
    
                    public string GetStatusLabel()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive      => "● متصل ونشط",
                            ATMCardState.ConnectedIdle        => "● متصل خامل",
                            ATMCardState.Syncing              => "⟳ يزامن",
                            ATMCardState.WaitingReply         => "◎ ينتظر رد",
                            ATMCardState.Supervisor           => "★ Supervisor",
                            ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                            ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                            ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                            ATMCardState.NeverConnected       => "○ لم يتصل",
                            _                                 => "?"
                        };
                    }
    
                    public string GetStatusDescription()        => GetStatusLabel();
    
                    public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
                    public string GetElapsed(DateTime utcRef)
                    {
                        if (utcRef == DateTime.MinValue) return "---";
                        var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                        if (s < 60)   return $"{(int)s}ث";
                        if (s < 3600) return $"{(int)(s / 60)}د";
                        return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
                    }
    
                    public string GetStatusColor()
                    {
                        if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                        if (!IsConnected)
                        {
                            var elapsed = DateTime.Now - LastConnectionTime;
                            if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                                return ATMStatusColors.COLOR_OFFLINE;
                            if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                                return ATMStatusColors.COLOR_WARNING;
                            return ATMStatusColors.COLOR_OFFLINE;
                        }
                        if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                        var idleElapsed = DateTime.Now - LastDataReceived;
                        if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                            return ATMStatusColors.COLOR_IDLE;
                        return ATMStatusColors.COLOR_ACTIVE;
                    }
    
                    public SyncStrategy GetSyncStrategy()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                            case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                            case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                            case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                            case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                            default:                        return SyncStrategy.NCR_Overwrite;
                        }
                    }
    
                    public bool NeedsAlert()
                    {
                        if (!IsConnected) return true;
                        return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                    }
    
                    public void RecalculateHealthScore()
                    {
                        var score = 100;
                        if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                        if (Latency_ms > 500) score -= 15;
                        if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                        if (CpuUsagePercent    > 90) score -= 10;
                        if (MemoryUsagePercent > 90) score -= 10;
                        if (DiskUsagePercent   > 95) score -= 10;
                        HealthScore = Math.Max(0, Math.Min(100, score));
                    }
    
                    public override string ToString() =>
                        $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
    
        }
    // Class: ATMInfo (from 9 sources)
        public partial class ATMInfo
        {
            // --- Constants & Fields ---
                            public ATMType ATMType
                            {
                                get
                                {
                                    switch (AppConstants.NormalizeATMType(ATM_Type))
                                    {
                                        case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                                        case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                                        case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
                                        case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
                                        case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
                                        default:                        return ATMType.Other;
                                    }
                                }
                                set
                                {
                                    ATM_Type = value switch
                                    {
                                        ATMType.NCR            => AppConstants.ATM_TYPE_NCR,
                                        ATMType.GRG            => AppConstants.ATM_TYPE_GRG,
                                        ATMType.WN             => AppConstants.ATM_TYPE_WN,
                                        ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                                        ATMType.Hyosung        => AppConstants.ATM_TYPE_HY,
                                        _                      => "OTHER"
                                    };
                                }
                            }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Models\ATMInfo.cs
                            public bool IsSyncing
                            {
                                get => ConnectionStatus == ConnectionStatus.Syncing;
                                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
                            }
    
                            private string _sourcePath, _backupPath;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Models\ATMInfo.cs
                    public bool IsSyncing
                    {
                        get => ConnectionStatus == ConnectionStatus.Syncing;
                        set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
                    }
    
    
            // --- Properties ---
                            public string ATM_ID        { get; set; }
    
                            public string ATM_Name      { get; set; }
    
                            public string ATM_Type      { get; set; }   // NCR / GRG / WN / DIEBOLD / HYOSUNG
    
                            public string BranchName    { get; set; }
    
                            public string Region        { get; set; }
    
                            public string ATM_Description { get; set; } // من النسخة القديمة
    
                            public string ATMId   { get => ATM_ID;   set => ATM_ID   = value; }
    
                            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
                            public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
                            public string Location  { get => Region;   set => Region   = value; }
    
                            public string BranchCode { get; set; }
    
                            public string ServerIP    { get; set; }
    
                            public int    ServerPort  { get; set; } = 5656;
    
                            public string NetworkType { get; set; } = "LAN";
    
                            public int    Latency_ms  { get; set; }
    
                            public int    Latency     { get => Latency_ms; set => Latency_ms = value; }
    
                            public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    
                            public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
    
                            public string           SessionId        { get; set; }
    
                            public bool             IsSupervisorMode { get; set; }
    
                            public bool             IsHostConnected  { get; set; }
    
                            public bool     IsSendingData  { get; set; }   // من النسخة القديمة
    
                            public bool     IsCSCConnected { get; set; }   // من النسخة القديمة
    
                            public DateTime ConnectedAtUtc      { get; set; }
    
                            public DateTime DisconnectedAtUtc   { get; set; }
    
                            public DateTime LastHeartbeatUtc    { get; set; }
    
                            public DateTime LastSyncUtc         { get; set; }
    
                            public DateTime LastDataReceivedUtc { get; set; }
    
                            public DateTime LastCommandSentUtc  { get; set; }
    
                            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
                            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
                            public DateTime LastDataReceived   { get; set; }
    
                            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
                            public long   TotalSyncedBytes        { get; set; }
    
                            public long   TotalTransactions       { get; set; }
    
                            public int    ConsecutiveSyncFailures { get; set; }
    
                            public double SyncSuccessRate         { get; set; } = 100.0;
    
                            public double ReceiveSpeedKBs         { get; set; }
    
                            public long   JournalSizeToday        { get; set; }
    
                            public double CpuUsagePercent         { get; set; }
    
                            public double MemoryUsagePercent      { get; set; }
    
                            public double DiskUsagePercent        { get; set; }
    
                            public int    HealthScore             { get; set; } = 100;
    
                            public int    PendingJournalCount     { get; set; }
    
                            public double SuccessRate           { get => SyncSuccessRate;   set => SyncSuccessRate   = value; }
    
                            public long   TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
                            public int[,]  OperationStats    { get; set; }
    
                            public int     ATMCache          { get; set; }
    
                            public int     TotalDispensed    { get; set; }
    
                            public SyncStatus SyncState      { get; set; }
    
                            public long    TotalBytesSent    { get; set; }
    
                            public int     TotalFilesSynced  { get; set; }
    
                            public int     TotalLinesSent    { get; set; }
    
                            public string  LastSyncFile      { get; set; }
    
                            public int  ApprovedTransactions { get; set; }
    
                            public int  FailedTransactions   { get; set; }
    
                            public int  CardsCaptured        { get; set; }
    
                            public long CashDispensed        { get; set; }
    
                            public string LastJournalFile   { get; set; }
    
                            public string LastErrorCode     { get; set; }
    
                            public string LastErrorMessage  { get; set; }
    
                            public string LastTransaction   { get; set; }
    
                            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
                            public string  ClientVersion { get; set; }
    
                            public string  OSVersion     { get; set; }
    
    
            // --- Constructors ---
                            public ATMInfo()
                            {
                                OperationStats = new int[3, 4];
                                Status         = ATMStatus.Unknown;
                                SyncState      = SyncStatus.Idle;
                                ServerPort     = NetworkConfig.DEFAULT_PORT;
                            }
    
    
            // --- Methods ---
                            public string GetSourcePath()
                            {
                                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                                return AppConstants.GetDefaultSourcePath(ATM_Type);
                            }
    
                            public string GetLegacySourcePath()
                            {
                                switch (ATM_Type)
                                {
                                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                                    case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_SOURCE;
                                    default:                        return string.Empty;
                                }
                            }
    
                            public void SetSourcePath(string v) => _sourcePath = v;
    
                            public string GetBackupPath()
                            {
                                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                                return System.IO.Path.Combine(
                                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
                            }
    
                            public string GetLegacyBackupPath()
                            {
                                switch (ATM_Type)
                                {
                                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                                    case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_BACKUP;
                                    default:                        return string.Empty;
                                }
                            }
    
                            public void SetBackupPath(string v) => _backupPath = v;
    
                            public ATMCardState GetCardState()
                            {
                                if (ConnectionStatus == ConnectionStatus.Disconnected)
                                {
                                    if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                                    var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                                    if (mins > 10) return ATMCardState.CriticalOffline;
                                    if (mins > 5)  return ATMCardState.WarningOffline;
                                    return ATMCardState.RecentlyDisconnected;
                                }
                                if (ConnectionStatus == ConnectionStatus.Syncing)      return ATMCardState.Syncing;
                                if (IsSupervisorMode)                                   return ATMCardState.Supervisor;
                                if (ConnectionStatus == ConnectionStatus.WaitingReply)  return ATMCardState.WaitingReply;
                                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                            }
    
                            public Color GetCardColor()
                            {
                                return GetCardState() switch
                                {
                                    ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),
                                    ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),
                                    ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),
                                    ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),
                                    ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),
                                    ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),
                                    ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),
                                    ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),
                                    ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),
                                    _                                 => Color.Gray
                                };
                            }
    
                            public string GetStatusLabel()
                            {
                                return GetCardState() switch
                                {
                                    ATMCardState.ConnectedActive      => "● متصل ونشط",
                                    ATMCardState.ConnectedIdle        => "● متصل خامل",
                                    ATMCardState.Syncing              => "⟳ يزامن",
                                    ATMCardState.WaitingReply         => "◎ ينتظر رد",
                                    ATMCardState.Supervisor           => "★ Supervisor",
                                    ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                                    ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                                    ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                                    ATMCardState.NeverConnected       => "○ لم يتصل",
                                    _                                 => "?"
                                };
                            }
    
                            public string GetStatusDescription()        => GetStatusLabel();
    
                            public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
                            public string GetElapsed(DateTime utcRef)
                            {
                                if (utcRef == DateTime.MinValue) return "---";
                                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                                if (s < 60)   return $"{(int)s}ث";
                                if (s < 3600) return $"{(int)(s / 60)}د";
                                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
                            }
    
                            public string GetStatusColor()
                            {
                                if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                                if (!IsConnected)
                                {
                                    var elapsed = DateTime.Now - LastConnectionTime;
                                    if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                                        return ATMStatusColors.COLOR_OFFLINE;
                                    if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                                        return ATMStatusColors.COLOR_WARNING;
                                    return ATMStatusColors.COLOR_OFFLINE;
                                }
                                if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                                var idleElapsed = DateTime.Now - LastDataReceived;
                                if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                                    return ATMStatusColors.COLOR_IDLE;
                                return ATMStatusColors.COLOR_ACTIVE;
                            }
    
                            public SyncStrategy GetSyncStrategy()
                            {
                                switch (ATM_Type)
                                {
                                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                                    default:                        return SyncStrategy.NCR_Overwrite;
                                }
                            }
    
                            public bool NeedsAlert()
                            {
                                if (!IsConnected) return true;
                                return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                            }
    
                            public void RecalculateHealthScore()
                            {
                                var score = 100;
                                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                                if (Latency_ms > 500) score -= 15;
                                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                                if (CpuUsagePercent    > 90) score -= 10;
                                if (MemoryUsagePercent > 90) score -= 10;
                                if (DiskUsagePercent   > 95) score -= 10;
                                HealthScore = Math.Max(0, Math.Min(100, score));
                            }
    
                            public override string ToString() =>
                                $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
    
            // --- Nested Classes ---
                        public class ATMInfo
                        {
                            // هوية
                            public string ATM_ID        { get; set; }
                            public string ATM_Name      { get; set; }
                            public string ATM_Type      { get; set; }    // NCR / GRG / WN / DIEBOLD / HYOSUNG
                            public string BranchName    { get; set; }
                            public string Region        { get; set; }
                            public string ATMId { get => ATM_ID; set => ATM_ID = value; }
                            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
                            public string IPAddress { get => ServerIP; set => ServerIP = value; }
                            public ATMType ATMType
                            {
                                get
                                {
                                    switch (AppConstants.NormalizeATMType(ATM_Type))
                                    {
                                        case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                                        case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                                        case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
                                        case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
                                        case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
                                        default:                        return ATMType.Other;
                                    }
                                }
                                set
                                {
                                    ATM_Type = value switch
                                    {
                                        ATMType.NCR            => AppConstants.ATM_TYPE_NCR,
                                        ATMType.GRG            => AppConstants.ATM_TYPE_GRG,
                                        ATMType.WN             => AppConstants.ATM_TYPE_WN,
                                        ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                                        ATMType.Hyosung        => AppConstants.ATM_TYPE_HY,
                                        _                      => "OTHER"
                                    };
                                }
                            }
                            public string Location { get => Region; set => Region = value; }
                            public string BranchCode { get; set; }
    
                            // شبكة
                            public string ServerIP      { get; set; }
                            public int    ServerPort    { get; set; } = 5656;
                            public string NetworkType   { get; set; } = "LAN";
                            public int    Latency_ms    { get; set; }
                            public int Latency { get => Latency_ms; set => Latency_ms = value; }
    
                            // حالة الاتصال
                            public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
                            public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
                            public string           SessionId        { get; set; }
                            public bool             IsSupervisorMode { get; set; }
                            public bool             IsHostConnected  { get; set; }
    
                            // طوابع زمنية UTC (T-11)
                            public DateTime ConnectedAtUtc       { get; set; }
                            public DateTime DisconnectedAtUtc    { get; set; }
                            public DateTime LastHeartbeatUtc     { get; set; }
                            public DateTime LastSyncUtc          { get; set; }
                            public DateTime LastDataReceivedUtc  { get; set; }
                            public DateTime LastCommandSentUtc   { get; set; }
                            public DateTime LastConnectionTime { get => ConnectedAtUtc; set => ConnectedAtUtc = value; }
                            public DateTime LastSyncTime { get => LastSyncUtc; set => LastSyncUtc = value; }
    
                            // إحصاءات المزامنة
                            public long   TotalSyncedBytes         { get; set; }
                            public long   TotalTransactions        { get; set; }
                            public int    ConsecutiveSyncFailures  { get; set; }
                            public double SyncSuccessRate          { get; set; } = 100.0;
                            public double ReceiveSpeedKBs          { get; set; }
                            public long   JournalSizeToday         { get; set; }
                            public double CpuUsagePercent          { get; set; }
                            public double MemoryUsagePercent       { get; set; }
                            public double DiskUsagePercent         { get; set; }
                            public int    HealthScore              { get; set; } = 100;
                            public int PendingJournalCount { get; set; }
                            public double SuccessRate { get => SyncSuccessRate; set => SyncSuccessRate = value; }
                            public long TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
                            // إحصاءات العمليات
                            public int  ApprovedTransactions  { get; set; }
                            public int  FailedTransactions    { get; set; }
                            public int  CardsCaptured         { get; set; }
                            public long CashDispensed         { get; set; }
    
                            // آخر جورنال / خطأ
                            public string LastJournalFile   { get; set; }
                            public string LastErrorCode     { get; set; }
                            public string LastErrorMessage  { get; set; }
                            public string LastTransaction   { get; set; }
                            public string LastError { get => LastErrorMessage; set => LastErrorMessage = value; }
                            public int TransactionCount { get => (int)Math.Min(int.MaxValue, TotalTransactions); set => TotalTransactions = value; }
                            public bool IsSyncing { get => ConnectionStatus == ConnectionStatus.Syncing; set { if (value) ConnectionStatus = ConnectionStatus.Syncing; } }
    
                            // مسارات
                            private string _sourcePath, _backupPath;
                            public string ClientVersion { get; set; }
                            public string OSVersion     { get; set; }
    
                            public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath)
                                ? _sourcePath
                                : AppConstants.GetDefaultSourcePath(ATM_Type);
    
                            public void SetSourcePath(string v) => _sourcePath = v;
    
                            public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                                : System.IO.Path.Combine(
                                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
    
                            public void SetBackupPath(string v) => _backupPath = v;
    
                            // ==========================================
                            // حالة البطاقة ولونها
                            // ==========================================
    
                            public ATMCardState GetCardState()
                            {
                                if (ConnectionStatus == ConnectionStatus.Disconnected)
                                {
                                    if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                                    var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                                    if (mins > 10) return ATMCardState.CriticalOffline;
                                    if (mins > 5)  return ATMCardState.WarningOffline;
                                    return ATMCardState.RecentlyDisconnected;
                                }
                                if (ConnectionStatus == ConnectionStatus.Syncing)     return ATMCardState.Syncing;
                                if (IsSupervisorMode)                                 return ATMCardState.Supervisor;
                                if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                            }
    
                            public Color GetCardColor()
                            {
                                return GetCardState() switch
                                {
                                    ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),   // أخضر
                                    ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),   // أصفر
                                    ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),  // أزرق
                                    ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),  // أزرق
                                    ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),   // برتقالي
                                    ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),   // أحمر
                                    ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),   // أحمر
                                    ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),  // رمادي
                                    ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),   // رمادي داكن
                                    _ => Color.Gray
                                };
                            }
    
                            public string GetStatusLabel()
                            {
                                return GetCardState() switch
                                {
                                    ATMCardState.ConnectedActive      => "● متصل ونشط",
                                    ATMCardState.ConnectedIdle        => "● متصل خامل",
                                    ATMCardState.Syncing              => "⟳ يزامن",
                                    ATMCardState.WaitingReply         => "◎ ينتظر رد",
                                    ATMCardState.Supervisor           => "★ Supervisor",
                                    ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                                    ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                                    ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                                    ATMCardState.NeverConnected       => "○ لم يتصل",
                                    _ => "?"
                                };
                            }
    
                            public string GetStatusDescription() => GetStatusLabel();
    
                            public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
                            public string GetElapsed(DateTime utcRef)
                            {
                                if (utcRef == DateTime.MinValue) return "---";
                                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                                if (s < 60)   return $"{(int)s}ث";
                                if (s < 3600) return $"{(int)(s/60)}د";
                                return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
                            }
    
                            public void RecalculateHealthScore()
                            {
                                var score = 100;
                                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                                if (Latency_ms > 500) score -= 15;
                                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                                if (CpuUsagePercent > 90) score -= 10;
                                if (MemoryUsagePercent > 90) score -= 10;
                                if (DiskUsagePercent > 95) score -= 10;
                                HealthScore = Math.Max(0, Math.Min(100, score));
                            }
    
                            public override string ToString() =>
                                $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
                        }
    
                        public class AlertPayload
                        {
                            public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
                            public AlertSeverity Severity  { get; set; }
                            public string        Title     { get; set; }
                            public string        Message   { get; set; }
                            public string        Source    { get; set; }
                            public string        DedupeKey { get; set; }
                            public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
                            public bool          IsRead    { get; set; }
    
                            public string Icon => Severity switch
                            {
                                AlertSeverity.Emergency => "CRIT",
                                AlertSeverity.Critical  => "FAIL",
                                AlertSeverity.Warning   => "WARN",
                                _                       => "INFO"
                            };
                            public string SeverityIcon => Icon;
                            public Color Color => Severity switch
                            {
                                AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                                AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                                AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                                _                       => Color.FromArgb(0,   122, 255)
                            };
                        }
    
                        public class JournalSyncRecord
                        {
                            public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
                            public string           ATM_ID          { get; set; }
                            public string           FileName        { get; set; }
                            public long             FileSize        { get; set; }
                            public long             FileOffset      { get; set; }
                            public string           Checksum        { get; set; }
                            public string           MD5Hash         { get; set; }
                            public string           SHA256Hash      { get; set; }
                            public JournalSyncState State           { get; set; }
                            public int              ProgressPercent { get; set; }
                            public int              RetryCount      { get; set; }
                            public string           LocalPath       { get; set; }
                            public string           ServerPath      { get; set; }
                            public string           Message         { get; set; }
                            public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
                            public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
                            public DateTime?        CompletedAtUtc  { get; set; }
    
                            public string StateIcon => State switch
                            {
                                JournalSyncState.Pending   => "PEND",
                                JournalSyncState.Syncing   => "SYNC",
                                JournalSyncState.ReSyncing => "RSYNC",
                                JournalSyncState.Completed => "OK",
                                JournalSyncState.Failed    => "FAIL",
                                JournalSyncState.Archived  => "ARCH",
                                _ => "?"
                            };
    
                            public string StateLabel => State switch
                            {
                                JournalSyncState.Pending   => "في الطابور",
                                JournalSyncState.Syncing   => "قيد المزامنة",
                                JournalSyncState.ReSyncing => "إعادة مزامنة",
                                JournalSyncState.Completed => "محمّل",
                                JournalSyncState.Failed    => "فشل",
                                JournalSyncState.Archived  => "مؤرشف",
                                _ => "؟"
                            };
    
                            public string ATM_ID { get; set; }
                            public string ATM_Name { get; set; }
                            public string ATM_Description { get; set; }
                            public string ATM_Type { get; set; }
                            public string ServerIP { get; set; }
                            public int ServerPort { get; set; }
                            public ATMStatus Status { get; set; }
                            public bool IsConnected { get; set; }
                            public bool IsSendingData { get; set; }
                            public bool IsCSCConnected { get; set; }
                            public DateTime LastConnectionTime { get; set; }
                            public DateTime LastDataReceived { get; set; }
                            public DateTime LastHeartbeat { get; set; }
                            public int[,] OperationStats { get; set; }
                            public int ATMCache { get; set; }
                            public int TotalDispensed { get; set; }
                            public SyncStatus SyncState { get; set; }
                            public long TotalBytesSent { get; set; }
                            public int TotalFilesSynced { get; set; }
                            public int TotalLinesSent { get; set; }
                            public DateTime LastSyncTime { get; set; }
                            public string LastSyncFile { get; set; }
                            public string OSVersion { get; set; }
                            public string ClientVersion { get; set; }
    
                            public ATMInfo()
                            {
                                OperationStats = new int[3, 4];
                                Status = ATMStatus.Unknown;
                                SyncState = SyncStatus.Idle;
                                ServerPort = NetworkConfig.DEFAULT_PORT;
                            }
    
                            public string GetStatusColor()
                            {
                                if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                                if (!IsConnected)
                                {
                                    var elapsed = DateTime.Now - LastConnectionTime;
                                    if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                                        return ATMStatusColors.COLOR_OFFLINE;
                                    if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                                        return ATMStatusColors.COLOR_WARNING;
                                    return ATMStatusColors.COLOR_OFFLINE;
                                }
                                if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                                var idleElapsed = DateTime.Now - LastDataReceived;
                                if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                                    return ATMStatusColors.COLOR_IDLE;
                                return ATMStatusColors.COLOR_ACTIVE;
                            }
    
                            public string GetSourcePath()
                            {
                                switch (ATM_Type)
                                {
                                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                                    default: return string.Empty;
                                }
                            }
    
                            public string GetBackupPath()
                            {
                                switch (ATM_Type)
                                {
                                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                                    case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                                    default: return string.Empty;
                                }
                            }
    
                            public SyncStrategy GetSyncStrategy()
                            {
                                switch (ATM_Type)
                                {
                                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                                    case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                                    default: return SyncStrategy.NCR_Overwrite;
                                }
                            }
    
                            public bool NeedsAlert()
                            {
                                if (!IsConnected) return true;
                                return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                            }
                        }
    
                        public class ClientConfig
                        {
                            public string ATM_ID { get; set; }
                            public string ATM_Name { get; set; }
                            public string ATM_Type { get; set; }
                            public string ServerIP { get; set; }
                            public int ServerPort { get; set; }
                            public bool SyncTimeEnabled { get; set; }
                            public int MessageSizeLines { get; set; }
                            public int FilePackageKB { get; set; }
                            public string SourcePath { get; set; }
                            public string BackupPath { get; set; }
                            public bool AutoStart { get; set; }
                            public bool RunAsService { get; set; }
    
                            public ClientConfig()
                            {
                                ServerPort = NetworkConfig.DEFAULT_PORT;
                                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                                AutoStart = true;
                                RunAsService = true;
                            }
                        }
    
                        public class ServerConfig
                        {
                            public int ListenPort { get; set; }
                            public string StoragePath { get; set; }
                            public string ArchivePath { get; set; }
                            public bool AutoArchive { get; set; }
                            public int MaxConnections { get; set; }
                            public bool EnableEncryption { get; set; }
                            public bool EnableCompression { get; set; }
    
                            public ServerConfig()
                            {
                                ListenPort = NetworkConfig.DEFAULT_PORT;
                                StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                                ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                                AutoArchive = true;
                                MaxConnections = 100;
                                EnableEncryption = true;
                                EnableCompression = true;
                            }
                        }
    
    
            // --- Nested Enums ---
                        public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
                        public enum ATMStatus
                        {
                            Unknown = 0,
                            Online = 1,
                            Idle = 2,
                            Supervisor = 3,
                            Warning = 4,
                            Offline = 5,
                            Critical = 6,
                            InService = 10,
                            ConnectedOnly = 11,
                            WaitingResponse = 12,
                            OutOfService = 13,
                            CriticalFault = 14,
                            Fault = 15,
                            Maintenance = 16
                        }
    
                        public enum ATMType { NCR, GRG, WN, DieboldNixdorf, Hyosung, Other }
    
                        public enum SyncStatus { Pending, InProgress, Syncing, Resyncing, Completed, Failed }
    
                        public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
                        public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
                        public enum ATMCardState      { NeverConnected, ConnectedActive, ConnectedIdle, Syncing, WaitingReply, Supervisor, RecentlyDisconnected, WarningOffline, CriticalOffline }
    
    
        }
    // Class: ATMInfo (from 5 sources)
        public partial class ATMInfo
        {
            // --- Constants & Fields ---
                    public ATMType ATMType
                    {
                        get
                        {
                            switch (AppConstants.NormalizeATMType(ATM_Type))
                            {
                                case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                                case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                                case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
                                case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
                                case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
                                default:                        return ATMType.Other;
                            }
                        }
                        set
                        {
                            ATM_Type = value switch
                            {
                                ATMType.NCR            => AppConstants.ATM_TYPE_NCR,
                                ATMType.GRG            => AppConstants.ATM_TYPE_GRG,
                                ATMType.WN             => AppConstants.ATM_TYPE_WN,
                                ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                                ATMType.Hyosung        => AppConstants.ATM_TYPE_HY,
                                _                      => "OTHER"
                            };
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Models\ATMInfo.cs
                    public bool IsSyncing
                    {
                        get => ConnectionStatus == ConnectionStatus.Syncing;
                        set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
                    }
    
                    private string _sourcePath, _backupPath;
    
    
            // --- Properties ---
                    public string ATM_ID        { get; set; }
    
                    public string ATM_Name      { get; set; }
    
                    public string ATM_Type      { get; set; }   // NCR / GRG / WN / DIEBOLD / HYOSUNG
    
                    public string BranchName    { get; set; }
    
                    public string Region        { get; set; }
    
                    public string ATM_Description { get; set; } // من النسخة القديمة
    
                    public string ATMId   { get => ATM_ID;   set => ATM_ID   = value; }
    
                    public string ATMName { get => ATM_Name; set => ATM_Name = value; }
    
                    public string IPAddress { get => ServerIP; set => ServerIP = value; }
    
                    public string Location  { get => Region;   set => Region   = value; }
    
                    public string BranchCode { get; set; }
    
                    public string ServerIP    { get; set; }
    
                    public int    ServerPort  { get; set; } = 5656;
    
                    public string NetworkType { get; set; } = "LAN";
    
                    public int    Latency_ms  { get; set; }
    
                    public int    Latency     { get => Latency_ms; set => Latency_ms = value; }
    
                    public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    
                    public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
    
                    public string           SessionId        { get; set; }
    
                    public bool             IsSupervisorMode { get; set; }
    
                    public bool             IsHostConnected  { get; set; }
    
                    public bool     IsSendingData  { get; set; }   // من النسخة القديمة
    
                    public bool     IsCSCConnected { get; set; }   // من النسخة القديمة
    
                    public DateTime ConnectedAtUtc      { get; set; }
    
                    public DateTime DisconnectedAtUtc   { get; set; }
    
                    public DateTime LastHeartbeatUtc    { get; set; }
    
                    public DateTime LastSyncUtc         { get; set; }
    
                    public DateTime LastDataReceivedUtc { get; set; }
    
                    public DateTime LastCommandSentUtc  { get; set; }
    
                    public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
    
                    public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
                    public DateTime LastDataReceived   { get; set; }
    
                    public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
                    public long   TotalSyncedBytes        { get; set; }
    
                    public long   TotalTransactions       { get; set; }
    
                    public int    ConsecutiveSyncFailures { get; set; }
    
                    public double SyncSuccessRate         { get; set; } = 100.0;
    
                    public double ReceiveSpeedKBs         { get; set; }
    
                    public long   JournalSizeToday        { get; set; }
    
                    public double CpuUsagePercent         { get; set; }
    
                    public double MemoryUsagePercent      { get; set; }
    
                    public double DiskUsagePercent        { get; set; }
    
                    public int    HealthScore             { get; set; } = 100;
    
                    public int    PendingJournalCount     { get; set; }
    
                    public double SuccessRate           { get => SyncSuccessRate;   set => SyncSuccessRate   = value; }
    
                    public long   TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
                    public int[,]  OperationStats    { get; set; }
    
                    public int     ATMCache          { get; set; }
    
                    public int     TotalDispensed    { get; set; }
    
                    public SyncStatus SyncState      { get; set; }
    
                    public long    TotalBytesSent    { get; set; }
    
                    public int     TotalFilesSynced  { get; set; }
    
                    public int     TotalLinesSent    { get; set; }
    
                    public string  LastSyncFile      { get; set; }
    
                    public int  ApprovedTransactions { get; set; }
    
                    public int  FailedTransactions   { get; set; }
    
                    public int  CardsCaptured        { get; set; }
    
                    public long CashDispensed        { get; set; }
    
                    public string LastJournalFile   { get; set; }
    
                    public string LastErrorCode     { get; set; }
    
                    public string LastErrorMessage  { get; set; }
    
                    public string LastTransaction   { get; set; }
    
                    public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
                    public string  ClientVersion { get; set; }
    
                    public string  OSVersion     { get; set; }
    
    
            // --- Constructors ---
                    public ATMInfo()
                    {
                        OperationStats = new int[3, 4];
                        Status         = ATMStatus.Unknown;
                        SyncState      = SyncStatus.Idle;
                        ServerPort     = NetworkConfig.DEFAULT_PORT;
                    }
    
    
            // --- Methods ---
                    public string GetSourcePath()
                    {
                        if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                        return AppConstants.GetDefaultSourcePath(ATM_Type);
                    }
    
                    public string GetLegacySourcePath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                            case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_SOURCE;
                            default:                        return string.Empty;
                        }
                    }
    
                    public void SetSourcePath(string v) => _sourcePath = v;
    
                    public string GetBackupPath()
                    {
                        if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                        return System.IO.Path.Combine(
                            System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                            "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
                    }
    
                    public string GetLegacyBackupPath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                            case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_BACKUP;
                            default:                        return string.Empty;
                        }
                    }
    
                    public void SetBackupPath(string v) => _backupPath = v;
    
                    public ATMCardState GetCardState()
                    {
                        if (ConnectionStatus == ConnectionStatus.Disconnected)
                        {
                            if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                            var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                            if (mins > 10) return ATMCardState.CriticalOffline;
                            if (mins > 5)  return ATMCardState.WarningOffline;
                            return ATMCardState.RecentlyDisconnected;
                        }
                        if (ConnectionStatus == ConnectionStatus.Syncing)      return ATMCardState.Syncing;
                        if (IsSupervisorMode)                                   return ATMCardState.Supervisor;
                        if (ConnectionStatus == ConnectionStatus.WaitingReply)  return ATMCardState.WaitingReply;
                        var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                        return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                    }
    
                    public Color GetCardColor()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),
                            ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),
                            ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),
                            ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),
                            ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),
                            ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),
                            ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),
                            ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),
                            ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),
                            _                                 => Color.Gray
                        };
                    }
    
                    public string GetStatusLabel()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive      => "● متصل ونشط",
                            ATMCardState.ConnectedIdle        => "● متصل خامل",
                            ATMCardState.Syncing              => "⟳ يزامن",
                            ATMCardState.WaitingReply         => "◎ ينتظر رد",
                            ATMCardState.Supervisor           => "★ Supervisor",
                            ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                            ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                            ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                            ATMCardState.NeverConnected       => "○ لم يتصل",
                            _                                 => "?"
                        };
                    }
    
                    public string GetStatusDescription()        => GetStatusLabel();
    
                    public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
                    public string GetElapsed(DateTime utcRef)
                    {
                        if (utcRef == DateTime.MinValue) return "---";
                        var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                        if (s < 60)   return $"{(int)s}ث";
                        if (s < 3600) return $"{(int)(s / 60)}د";
                        return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
                    }
    
                    public string GetStatusColor()
                    {
                        if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                        if (!IsConnected)
                        {
                            var elapsed = DateTime.Now - LastConnectionTime;
                            if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                                return ATMStatusColors.COLOR_OFFLINE;
                            if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                                return ATMStatusColors.COLOR_WARNING;
                            return ATMStatusColors.COLOR_OFFLINE;
                        }
                        if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                        var idleElapsed = DateTime.Now - LastDataReceived;
                        if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                            return ATMStatusColors.COLOR_IDLE;
                        return ATMStatusColors.COLOR_ACTIVE;
                    }
    
                    public SyncStrategy GetSyncStrategy()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                            case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                            case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                            case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                            case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                            default:                        return SyncStrategy.NCR_Overwrite;
                        }
                    }
    
                    public bool NeedsAlert()
                    {
                        if (!IsConnected) return true;
                        return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                    }
    
                    public void RecalculateHealthScore()
                    {
                        var score = 100;
                        if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                        if (Latency_ms > 500) score -= 15;
                        if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                        if (CpuUsagePercent    > 90) score -= 10;
                        if (MemoryUsagePercent > 90) score -= 10;
                        if (DiskUsagePercent   > 95) score -= 10;
                        HealthScore = Math.Max(0, Math.Min(100, score));
                    }
    
                    public override string ToString() =>
                        $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
    
            // --- Nested Classes ---
                public class ATMInfo
                {
                    // هوية
                    public string ATM_ID        { get; set; }
                    public string ATM_Name      { get; set; }
                    public string ATM_Type      { get; set; }    // NCR / GRG / WN / DIEBOLD / HYOSUNG
                    public string BranchName    { get; set; }
                    public string Region        { get; set; }
                    public string ATMId { get => ATM_ID; set => ATM_ID = value; }
                    public string ATMName { get => ATM_Name; set => ATM_Name = value; }
                    public string IPAddress { get => ServerIP; set => ServerIP = value; }
                    public ATMType ATMType
                    {
                        get
                        {
                            switch (AppConstants.NormalizeATMType(ATM_Type))
                            {
                                case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                                case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                                case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
                                case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
                                case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
                                default:                        return ATMType.Other;
                            }
                        }
                        set
                        {
                            ATM_Type = value switch
                            {
                                ATMType.NCR            => AppConstants.ATM_TYPE_NCR,
                                ATMType.GRG            => AppConstants.ATM_TYPE_GRG,
                                ATMType.WN             => AppConstants.ATM_TYPE_WN,
                                ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                                ATMType.Hyosung        => AppConstants.ATM_TYPE_HY,
                                _                      => "OTHER"
                            };
                        }
                    }
                    public string Location { get => Region; set => Region = value; }
                    public string BranchCode { get; set; }
    
                    // شبكة
                    public string ServerIP      { get; set; }
                    public int    ServerPort    { get; set; } = 5656;
                    public string NetworkType   { get; set; } = "LAN";
                    public int    Latency_ms    { get; set; }
                    public int Latency { get => Latency_ms; set => Latency_ms = value; }
    
                    // حالة الاتصال
                    public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
                    public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
                    public string           SessionId        { get; set; }
                    public bool             IsSupervisorMode { get; set; }
                    public bool             IsHostConnected  { get; set; }
    
                    // طوابع زمنية UTC (T-11)
                    public DateTime ConnectedAtUtc       { get; set; }
                    public DateTime DisconnectedAtUtc    { get; set; }
                    public DateTime LastHeartbeatUtc     { get; set; }
                    public DateTime LastSyncUtc          { get; set; }
                    public DateTime LastDataReceivedUtc  { get; set; }
                    public DateTime LastCommandSentUtc   { get; set; }
                    public DateTime LastConnectionTime { get => ConnectedAtUtc; set => ConnectedAtUtc = value; }
                    public DateTime LastSyncTime { get => LastSyncUtc; set => LastSyncUtc = value; }
    
                    // إحصاءات المزامنة
                    public long   TotalSyncedBytes         { get; set; }
                    public long   TotalTransactions        { get; set; }
                    public int    ConsecutiveSyncFailures  { get; set; }
                    public double SyncSuccessRate          { get; set; } = 100.0;
                    public double ReceiveSpeedKBs          { get; set; }
                    public long   JournalSizeToday         { get; set; }
                    public double CpuUsagePercent          { get; set; }
                    public double MemoryUsagePercent       { get; set; }
                    public double DiskUsagePercent         { get; set; }
                    public int    HealthScore              { get; set; } = 100;
                    public int PendingJournalCount { get; set; }
                    public double SuccessRate { get => SyncSuccessRate; set => SyncSuccessRate = value; }
                    public long TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
                    // إحصاءات العمليات
                    public int  ApprovedTransactions  { get; set; }
                    public int  FailedTransactions    { get; set; }
                    public int  CardsCaptured         { get; set; }
                    public long CashDispensed         { get; set; }
    
                    // آخر جورنال / خطأ
                    public string LastJournalFile   { get; set; }
                    public string LastErrorCode     { get; set; }
                    public string LastErrorMessage  { get; set; }
                    public string LastTransaction   { get; set; }
                    public string LastError { get => LastErrorMessage; set => LastErrorMessage = value; }
                    public int TransactionCount { get => (int)Math.Min(int.MaxValue, TotalTransactions); set => TotalTransactions = value; }
                    public bool IsSyncing { get => ConnectionStatus == ConnectionStatus.Syncing; set { if (value) ConnectionStatus = ConnectionStatus.Syncing; } }
    
                    // مسارات
                    private string _sourcePath, _backupPath;
                    public string ClientVersion { get; set; }
                    public string OSVersion     { get; set; }
    
                    public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath)
                        ? _sourcePath
                        : AppConstants.GetDefaultSourcePath(ATM_Type);
    
                    public void SetSourcePath(string v) => _sourcePath = v;
    
                    public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                        : System.IO.Path.Combine(
                            System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                            "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
    
                    public void SetBackupPath(string v) => _backupPath = v;
    
                    // ==========================================
                    // حالة البطاقة ولونها
                    // ==========================================
    
                    public ATMCardState GetCardState()
                    {
                        if (ConnectionStatus == ConnectionStatus.Disconnected)
                        {
                            if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                            var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                            if (mins > 10) return ATMCardState.CriticalOffline;
                            if (mins > 5)  return ATMCardState.WarningOffline;
                            return ATMCardState.RecentlyDisconnected;
                        }
                        if (ConnectionStatus == ConnectionStatus.Syncing)     return ATMCardState.Syncing;
                        if (IsSupervisorMode)                                 return ATMCardState.Supervisor;
                        if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                        var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                        return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                    }
    
                    public Color GetCardColor()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),   // أخضر
                            ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),   // أصفر
                            ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),  // أزرق
                            ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),  // أزرق
                            ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),   // برتقالي
                            ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),   // أحمر
                            ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),   // أحمر
                            ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),  // رمادي
                            ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),   // رمادي داكن
                            _ => Color.Gray
                        };
                    }
    
                    public string GetStatusLabel()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive      => "● متصل ونشط",
                            ATMCardState.ConnectedIdle        => "● متصل خامل",
                            ATMCardState.Syncing              => "⟳ يزامن",
                            ATMCardState.WaitingReply         => "◎ ينتظر رد",
                            ATMCardState.Supervisor           => "★ Supervisor",
                            ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                            ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                            ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                            ATMCardState.NeverConnected       => "○ لم يتصل",
                            _ => "?"
                        };
                    }
    
                    public string GetStatusDescription() => GetStatusLabel();
    
                    public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
                    public string GetElapsed(DateTime utcRef)
                    {
                        if (utcRef == DateTime.MinValue) return "---";
                        var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                        if (s < 60)   return $"{(int)s}ث";
                        if (s < 3600) return $"{(int)(s/60)}د";
                        return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
                    }
    
                    public void RecalculateHealthScore()
                    {
                        var score = 100;
                        if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                        if (Latency_ms > 500) score -= 15;
                        if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                        if (CpuUsagePercent > 90) score -= 10;
                        if (MemoryUsagePercent > 90) score -= 10;
                        if (DiskUsagePercent > 95) score -= 10;
                        HealthScore = Math.Max(0, Math.Min(100, score));
                    }
    
                    public override string ToString() =>
                        $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
                }
    
                public class AlertPayload
                {
                    public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
                    public AlertSeverity Severity  { get; set; }
                    public string        Title     { get; set; }
                    public string        Message   { get; set; }
                    public string        Source    { get; set; }
                    public string        DedupeKey { get; set; }
                    public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
                    public bool          IsRead    { get; set; }
    
                    public string Icon => Severity switch
                    {
                        AlertSeverity.Emergency => "CRIT",
                        AlertSeverity.Critical  => "FAIL",
                        AlertSeverity.Warning   => "WARN",
                        _                       => "INFO"
                    };
                    public string SeverityIcon => Icon;
                    public Color Color => Severity switch
                    {
                        AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
                        AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
                        AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
                        _                       => Color.FromArgb(0,   122, 255)
                    };
                }
    
                public class JournalSyncRecord
                {
                    public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
                    public string           ATM_ID          { get; set; }
                    public string           FileName        { get; set; }
                    public long             FileSize        { get; set; }
                    public long             FileOffset      { get; set; }
                    public string           Checksum        { get; set; }
                    public string           MD5Hash         { get; set; }
                    public string           SHA256Hash      { get; set; }
                    public JournalSyncState State           { get; set; }
                    public int              ProgressPercent { get; set; }
                    public int              RetryCount      { get; set; }
                    public string           LocalPath       { get; set; }
                    public string           ServerPath      { get; set; }
                    public string           Message         { get; set; }
                    public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
                    public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
                    public DateTime?        CompletedAtUtc  { get; set; }
    
                    public string StateIcon => State switch
                    {
                        JournalSyncState.Pending   => "PEND",
                        JournalSyncState.Syncing   => "SYNC",
                        JournalSyncState.ReSyncing => "RSYNC",
                        JournalSyncState.Completed => "OK",
                        JournalSyncState.Failed    => "FAIL",
                        JournalSyncState.Archived  => "ARCH",
                        _ => "?"
                    };
    
                    public string StateLabel => State switch
                    {
                        JournalSyncState.Pending   => "في الطابور",
                        JournalSyncState.Syncing   => "قيد المزامنة",
                        JournalSyncState.ReSyncing => "إعادة مزامنة",
                        JournalSyncState.Completed => "محمّل",
                        JournalSyncState.Failed    => "فشل",
                        JournalSyncState.Archived  => "مؤرشف",
                        _ => "؟"
                    };
    
                    public string ATM_ID { get; set; }
                    public string ATM_Name { get; set; }
                    public string ATM_Description { get; set; }
                    public string ATM_Type { get; set; }
                    public string ServerIP { get; set; }
                    public int ServerPort { get; set; }
                    public ATMStatus Status { get; set; }
                    public bool IsConnected { get; set; }
                    public bool IsSendingData { get; set; }
                    public bool IsCSCConnected { get; set; }
                    public DateTime LastConnectionTime { get; set; }
                    public DateTime LastDataReceived { get; set; }
                    public DateTime LastHeartbeat { get; set; }
                    public int[,] OperationStats { get; set; }
                    public int ATMCache { get; set; }
                    public int TotalDispensed { get; set; }
                    public SyncStatus SyncState { get; set; }
                    public long TotalBytesSent { get; set; }
                    public int TotalFilesSynced { get; set; }
                    public int TotalLinesSent { get; set; }
                    public DateTime LastSyncTime { get; set; }
                    public string LastSyncFile { get; set; }
                    public string OSVersion { get; set; }
                    public string ClientVersion { get; set; }
    
                    public ATMInfo()
                    {
                        OperationStats = new int[3, 4];
                        Status = ATMStatus.Unknown;
                        SyncState = SyncStatus.Idle;
                        ServerPort = NetworkConfig.DEFAULT_PORT;
                    }
    
                    public string GetStatusColor()
                    {
                        if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                        if (!IsConnected)
                        {
                            var elapsed = DateTime.Now - LastConnectionTime;
                            if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                                return ATMStatusColors.COLOR_OFFLINE;
                            if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                                return ATMStatusColors.COLOR_WARNING;
                            return ATMStatusColors.COLOR_OFFLINE;
                        }
                        if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                        var idleElapsed = DateTime.Now - LastDataReceived;
                        if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                            return ATMStatusColors.COLOR_IDLE;
                        return ATMStatusColors.COLOR_ACTIVE;
                    }
    
                    public string GetSourcePath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                            default: return string.Empty;
                        }
                    }
    
                    public string GetBackupPath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                            default: return string.Empty;
                        }
                    }
    
                    public SyncStrategy GetSyncStrategy()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                            case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                            case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                            default: return SyncStrategy.NCR_Overwrite;
                        }
                    }
    
                    public bool NeedsAlert()
                    {
                        if (!IsConnected) return true;
                        return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                    }
                }
    
                public class ClientConfig
                {
                    public string ATM_ID { get; set; }
                    public string ATM_Name { get; set; }
                    public string ATM_Type { get; set; }
                    public string ServerIP { get; set; }
                    public int ServerPort { get; set; }
                    public bool SyncTimeEnabled { get; set; }
                    public int MessageSizeLines { get; set; }
                    public int FilePackageKB { get; set; }
                    public string SourcePath { get; set; }
                    public string BackupPath { get; set; }
                    public bool AutoStart { get; set; }
                    public bool RunAsService { get; set; }
    
                    public ClientConfig()
                    {
                        ServerPort = NetworkConfig.DEFAULT_PORT;
                        MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                        FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                        AutoStart = true;
                        RunAsService = true;
                    }
                }
    
                public class ServerConfig
                {
                    public int ListenPort { get; set; }
                    public string StoragePath { get; set; }
                    public string ArchivePath { get; set; }
                    public bool AutoArchive { get; set; }
                    public int MaxConnections { get; set; }
                    public bool EnableEncryption { get; set; }
                    public bool EnableCompression { get; set; }
    
                    public ServerConfig()
                    {
                        ListenPort = NetworkConfig.DEFAULT_PORT;
                        StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                        ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                        AutoArchive = true;
                        MaxConnections = 100;
                        EnableEncryption = true;
                        EnableCompression = true;
                    }
                }
    
    
            // --- Nested Enums ---
                public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    
                public enum ATMStatus
                {
                    Unknown = 0,
                    Online = 1,
                    Idle = 2,
                    Supervisor = 3,
                    Warning = 4,
                    Offline = 5,
                    Critical = 6,
                    InService = 10,
                    ConnectedOnly = 11,
                    WaitingResponse = 12,
                    OutOfService = 13,
                    CriticalFault = 14,
                    Fault = 15,
                    Maintenance = 16
                }
    
                public enum ATMType { NCR, GRG, WN, DieboldNixdorf, Hyosung, Other }
    
                public enum SyncStatus { Pending, InProgress, Syncing, Resyncing, Completed, Failed }
    
                public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    
                public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    
                public enum ATMCardState      { NeverConnected, ConnectedActive, ConnectedIdle, Syncing, WaitingReply, Supervisor, RecentlyDisconnected, WarningOffline, CriticalOffline }
    
    
        }
    // ═══ Class: ATMInfo (from 5 sources) ═══
        public partial class ATMInfo
        {
            // --- Constants & Fields ---
                    private string _sourcePath, _backupPath;
    
    
            // --- Properties ---
                    public string ATM_ID        { get; set; }
    
                    public string ATM_Name      { get; set; }
    
                    public string ATM_Type      { get; set; }    // NCR / GRG / WN
    
                    public string BranchName    { get; set; }
    
                    public string Region        { get; set; }
    
                    public string ServerIP      { get; set; }
    
                    public int    ServerPort    { get; set; } = 5656;
    
                    public string NetworkType   { get; set; } = "LAN";
    
                    public int    Latency_ms    { get; set; }
    
                    public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    
                    public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
    
                    public string           SessionId        { get; set; }
    
                    public bool             IsSupervisorMode { get; set; }
    
                    public bool             IsHostConnected  { get; set; }
    
                    public DateTime ConnectedAtUtc       { get; set; }
    
                    public DateTime DisconnectedAtUtc    { get; set; }
    
                    public DateTime LastHeartbeatUtc     { get; set; }
    
                    public DateTime LastSyncUtc          { get; set; }
    
                    public DateTime LastDataReceivedUtc  { get; set; }
    
                    public DateTime LastCommandSentUtc   { get; set; }
    
                    public long   TotalSyncedBytes         { get; set; }
    
                    public long   TotalTransactions        { get; set; }
    
                    public int    ConsecutiveSyncFailures  { get; set; }
    
                    public double SyncSuccessRate          { get; set; } = 100.0;
    
                    public double ReceiveSpeedKBs          { get; set; }
    
                    public long   JournalSizeToday         { get; set; }
    
                    public int  ApprovedTransactions  { get; set; }
    
                    public int  FailedTransactions    { get; set; }
    
                    public int  CardsCaptured         { get; set; }
    
                    public long CashDispensed         { get; set; }
    
                    public string LastJournalFile   { get; set; }
    
                    public string LastErrorCode     { get; set; }
    
                    public string LastErrorMessage  { get; set; }
    
                    public string LastTransaction   { get; set; }
    
                    public string ClientVersion { get; set; }
    
                    public string OSVersion     { get; set; }
    
                    public string ATM_Description { get; set; }
    
                    public bool IsConnected { get; set; }
    
                    public bool IsSendingData { get; set; }
    
                    public bool IsCSCConnected { get; set; }
    
                    public DateTime LastConnectionTime { get; set; }
    
                    public DateTime LastDataReceived { get; set; }
    
                    public DateTime LastHeartbeat { get; set; }
    
                    public int[,] OperationStats { get; set; }
    
                    public int ATMCache { get; set; }
    
                    public int TotalDispensed { get; set; }
    
                    public SyncStatus SyncState { get; set; }
    
                    public long TotalBytesSent { get; set; }
    
                    public int TotalFilesSynced { get; set; }
    
                    public int TotalLinesSent { get; set; }
    
                    public DateTime LastSyncTime { get; set; }
    
                    public string LastSyncFile { get; set; }
    
    
            // --- Constructors ---
                    public ATMInfo()
                    {
                        OperationStats = new int[3, 4];
                        Status = ATMStatus.Unknown;
                        SyncState = SyncStatus.Idle;
                        ServerPort = NetworkConfig.DEFAULT_PORT;
                    }
    
    
            // --- Methods ---
                    public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath) ? _sourcePath
                        : ATM_Type == "NCR" ? @"C:\NCRJournal\"
                        : ATM_Type == "GRG" ? @"D:\GRGData\EJ\"
                        : ATM_Type == "WN"  ? @"C:\WOSA\EJ\"
                        : @"C:\Journal\";
    
                    public void SetSourcePath(string v) => _sourcePath = v;
    
                    public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
                        : System.IO.Path.Combine(
                            System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                            "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
    
                    public void SetBackupPath(string v) => _backupPath = v;
    
                    public ATMCardState GetCardState()
                    {
                        if (ConnectionStatus == ConnectionStatus.Disconnected)
                        {
                            if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                            var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                            if (mins > 10) return ATMCardState.CriticalOffline;
                            if (mins > 5)  return ATMCardState.WarningOffline;
                            return ATMCardState.RecentlyDisconnected;
                        }
                        if (ConnectionStatus == ConnectionStatus.Syncing)     return ATMCardState.Syncing;
                        if (IsSupervisorMode)                                 return ATMCardState.Supervisor;
                        if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
                        var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                        return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
                    }
    
                    public Color GetCardColor()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),   // أخضر
                            ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),   // أصفر
                            ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),  // أزرق
                            ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),  // أزرق
                            ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),   // برتقالي
                            ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),   // أحمر
                            ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),   // أحمر
                            ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),  // رمادي
                            ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),   // رمادي داكن
                            _ => Color.Gray
                        };
                    }
    
                    public string GetStatusLabel()
                    {
                        return GetCardState() switch
                        {
                            ATMCardState.ConnectedActive      => "● متصل ونشط",
                            ATMCardState.ConnectedIdle        => "● متصل خامل",
                            ATMCardState.Syncing              => "⟳ يزامن",
                            ATMCardState.WaitingReply         => "◎ ينتظر رد",
                            ATMCardState.Supervisor           => "★ Supervisor",
                            ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                            ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                            ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                            ATMCardState.NeverConnected       => "○ لم يتصل",
                            _ => "?"
                        };
                    }
    
                    public string GetElapsed(DateTime utcRef)
                    {
                        if (utcRef == DateTime.MinValue) return "---";
                        var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                        if (s < 60)   return $"{(int)s}ث";
                        if (s < 3600) return $"{(int)(s/60)}د";
                        return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
                    }
    
                    public override string ToString() =>
                        $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    
                    public string GetStatusColor()
                    {
                        if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                        if (!IsConnected)
                        {
                            var elapsed = DateTime.Now - LastConnectionTime;
                            if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                                return ATMStatusColors.COLOR_OFFLINE;
                            if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                                return ATMStatusColors.COLOR_WARNING;
                            return ATMStatusColors.COLOR_OFFLINE;
                        }
                        if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                        var idleElapsed = DateTime.Now - LastDataReceived;
                        if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                            return ATMStatusColors.COLOR_IDLE;
                        return ATMStatusColors.COLOR_ACTIVE;
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Models\ATMInfo.cs
                    public string GetSourcePath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                            default: return string.Empty;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Models\ATMInfo.cs
                    public string GetBackupPath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                            default: return string.Empty;
                        }
                    }
    
                    public SyncStrategy GetSyncStrategy()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                            case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                            case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                            default: return SyncStrategy.NCR_Overwrite;
                        }
                    }
    
                    public bool NeedsAlert()
                    {
                        if (!IsConnected) return true;
                        return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Models\ATMInfo.cs
                    public string GetSourcePath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                            default: return string.Empty;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Models\ATMInfo.cs
                    public string GetBackupPath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                            default: return string.Empty;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Models\ATMInfo.cs
                    public string GetSourcePath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                            default: return string.Empty;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Models\ATMInfo.cs
                    public string GetBackupPath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                            default: return string.Empty;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Models\ATMInfo.cs
                    public string GetSourcePath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                            default: return string.Empty;
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Models\ATMInfo.cs
                    public string GetBackupPath()
                    {
                        switch (ATM_Type)
                        {
                            case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                            case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                            case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                            default: return string.Empty;
                        }
                    }
    
    
        }
    // ==========================================
        // ATMInfo — نموذج الصراف الشامل الموحّد
        // دمج النسختين: الجديدة (ConnectionStatus, GetCardState, GetCardColor, ...)
        //               والقديمة (IsConnected, GetStatusColor, GetSyncStrategy, NeedsAlert, ...)
        // ==========================================
    
        public class ATMInfo
        {
            // ==========================================
            // هوية الصراف
            // ==========================================
    
            public string ATM_ID        { get; set; }
            public string ATM_Name      { get; set; }
            public string ATM_Type      { get; set; }   // NCR / GRG / WN / DIEBOLD / HYOSUNG
            public string BranchName    { get; set; }
            public string Region        { get; set; }
            public string ATM_Description { get; set; } // من النسخة القديمة
    
            // aliases للتوافق
            public string ATMId   { get => ATM_ID;   set => ATM_ID   = value; }
            public string ATMName { get => ATM_Name; set => ATM_Name = value; }
            public string IPAddress { get => ServerIP; set => ServerIP = value; }
            public string Location  { get => Region;   set => Region   = value; }
            public string BranchCode { get; set; }
    
            public ATMType ATMType
            {
                get
                {
                    switch (AppConstants.NormalizeATMType(ATM_Type))
                    {
                        case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                        case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                        case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
                        case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
                        case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
                        default:                        return ATMType.Other;
                    }
                }
                set
                {
                    ATM_Type = value switch
                    {
                        ATMType.NCR            => AppConstants.ATM_TYPE_NCR,
                        ATMType.GRG            => AppConstants.ATM_TYPE_GRG,
                        ATMType.WN             => AppConstants.ATM_TYPE_WN,
                        ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                        ATMType.Hyosung        => AppConstants.ATM_TYPE_HY,
                        _                      => "OTHER"
                    };
                }
            }
    
            // ==========================================
            // الشبكة
            // ==========================================
    
            public string ServerIP    { get; set; }
            public int    ServerPort  { get; set; } = 5656;
            public string NetworkType { get; set; } = "LAN";
            public int    Latency_ms  { get; set; }
            public int    Latency     { get => Latency_ms; set => Latency_ms = value; }
    
            // ==========================================
            // حالة الاتصال — النسخة الجديدة
            // ==========================================
    
            public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
            public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
            public string           SessionId        { get; set; }
            public bool             IsSupervisorMode { get; set; }
            public bool             IsHostConnected  { get; set; }
    
            // ==========================================
            // حالة الاتصال — من النسخة القديمة (فريدة)
            // ==========================================
    
            /// <summary>alias لـ ConnectionStatus == Connected — للتوافق مع الكود القديم</summary>
            public bool IsConnected
            {
                get => ConnectionStatus == ConnectionStatus.Connected ||
                        ConnectionStatus == ConnectionStatus.WaitingReply ||
                        ConnectionStatus == ConnectionStatus.Syncing;
                set
                {
                    if (value && ConnectionStatus == ConnectionStatus.Disconnected)
                        ConnectionStatus = ConnectionStatus.Connected;
                    else if (!value)
                        ConnectionStatus = ConnectionStatus.Disconnected;
                }
            }
    
            public bool     IsSendingData  { get; set; }   // من النسخة القديمة
            public bool     IsCSCConnected { get; set; }   // من النسخة القديمة
    
            // ==========================================
            // طوابع زمنية UTC
            // ==========================================
    
            public DateTime ConnectedAtUtc      { get; set; }
            public DateTime DisconnectedAtUtc   { get; set; }
            public DateTime LastHeartbeatUtc    { get; set; }
            public DateTime LastSyncUtc         { get; set; }
            public DateTime LastDataReceivedUtc { get; set; }
            public DateTime LastCommandSentUtc  { get; set; }
    
            // aliases
            public DateTime LastConnectionTime { get => ConnectedAtUtc;      set => ConnectedAtUtc      = value; }
            public DateTime LastSyncTime       { get => LastSyncUtc;         set => LastSyncUtc         = value; }
    
            // من النسخة القديمة — وقت محلي (local time)
            public DateTime LastDataReceived   { get; set; }
            public DateTime LastHeartbeat      { get => LastHeartbeatUtc.ToLocalTime(); set => LastHeartbeatUtc = value.ToUniversalTime(); }
    
            // ==========================================
            // إحصاءات المزامنة
            // ==========================================
    
            public long   TotalSyncedBytes        { get; set; }
            public long   TotalTransactions       { get; set; }
            public int    ConsecutiveSyncFailures { get; set; }
            public double SyncSuccessRate         { get; set; } = 100.0;
            public double ReceiveSpeedKBs         { get; set; }
            public long   JournalSizeToday        { get; set; }
            public double CpuUsagePercent         { get; set; }
            public double MemoryUsagePercent      { get; set; }
            public double DiskUsagePercent        { get; set; }
            public int    HealthScore             { get; set; } = 100;
            public int    PendingJournalCount     { get; set; }
    
            // aliases
            public double SuccessRate           { get => SyncSuccessRate;   set => SyncSuccessRate   = value; }
            public long   TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    
            // من النسخة القديمة — فريدة
            public int[,]  OperationStats    { get; set; }
            public int     ATMCache          { get; set; }
            public int     TotalDispensed    { get; set; }
            public SyncStatus SyncState      { get; set; }
            public long    TotalBytesSent    { get; set; }
            public int     TotalFilesSynced  { get; set; }
            public int     TotalLinesSent    { get; set; }
            public string  LastSyncFile      { get; set; }
    
            // ==========================================
            // إحصاءات العمليات
            // ==========================================
    
            public int  ApprovedTransactions { get; set; }
            public int  FailedTransactions   { get; set; }
            public int  CardsCaptured        { get; set; }
            public long CashDispensed        { get; set; }
    
            // aliases
            public int TransactionCount
            {
                get => (int)Math.Min(int.MaxValue, TotalTransactions);
                set => TotalTransactions = value;
            }
    
            public bool IsSyncing
            {
                get => ConnectionStatus == ConnectionStatus.Syncing;
                set { if (value) ConnectionStatus = ConnectionStatus.Syncing; }
            }
    
            // ==========================================
            // آخر جورنال / خطأ
            // ==========================================
    
            public string LastJournalFile   { get; set; }
            public string LastErrorCode     { get; set; }
            public string LastErrorMessage  { get; set; }
            public string LastTransaction   { get; set; }
            public string LastError         { get => LastErrorMessage; set => LastErrorMessage = value; }
    
            // ==========================================
            // مسارات ومعلومات النظام
            // ==========================================
    
            private string _sourcePath, _backupPath;
            public string  ClientVersion { get; set; }
            public string  OSVersion     { get; set; }
    
            // ==========================================
            // منشئ الكائن — من النسخة القديمة
            // ==========================================
    
            public ATMInfo()
            {
                OperationStats = new int[3, 4];
                Status         = ATMStatus.Unknown;
                SyncState      = SyncStatus.Idle;
                ServerPort     = NetworkConfig.DEFAULT_PORT;
            }
    
            // ==========================================
            // مسارات الجورنال والنسخ الاحتياطي
            // ==========================================
    
            /// <summary>
            /// المسار الفعلي لجورنال الصراف (نسخة محسّنة تستخدم AppConstants).
            /// </summary>
            public string GetSourcePath()
            {
                if (!string.IsNullOrEmpty(_sourcePath)) return _sourcePath;
                return AppConstants.GetDefaultSourcePath(ATM_Type);
            }
    
            /// <summary>
            /// المسار القديم للجورنال — يستخدم ATMPaths مباشرةً للتوافق مع الكود القديم.
            /// </summary>
            public string GetLegacySourcePath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                    case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_SOURCE;
                    default:                        return string.Empty;
                }
            }
    
            public void SetSourcePath(string v) => _sourcePath = v;
    
            /// <summary>
            /// مسار النسخة الاحتياطية (نسخة محسّنة).
            /// </summary>
            public string GetBackupPath()
            {
                if (!string.IsNullOrEmpty(_backupPath)) return _backupPath;
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                    "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");
            }
    
            /// <summary>
            /// مسار النسخة الاحتياطية القديم — يستخدم ATMPaths مباشرةً.
            /// </summary>
            public string GetLegacyBackupPath()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                    case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                    case AppConstants.ATM_TYPE_WN:  return ATMPaths.WN_BACKUP;
                    default:                        return string.Empty;
                }
            }
    
            public void SetBackupPath(string v) => _backupPath = v;
    
            // ==========================================
            // لون وحالة بطاقة الصراف — النسخة الجديدة
            // ==========================================
    
            public ATMCardState GetCardState()
            {
                if (ConnectionStatus == ConnectionStatus.Disconnected)
                {
                    if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                    var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                    if (mins > 10) return ATMCardState.CriticalOffline;
                    if (mins > 5)  return ATMCardState.WarningOffline;
                    return ATMCardState.RecentlyDisconnected;
                }
                if (ConnectionStatus == ConnectionStatus.Syncing)      return ATMCardState.Syncing;
                if (IsSupervisorMode)                                   return ATMCardState.Supervisor;
                if (ConnectionStatus == ConnectionStatus.WaitingReply)  return ATMCardState.WaitingReply;
                var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
                return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
            }
    
            public Color GetCardColor()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),
                    ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),
                    ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),
                    ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),
                    ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),
                    ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),
                    ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),
                    ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),
                    ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),
                    _                                 => Color.Gray
                };
            }
    
            public string GetStatusLabel()
            {
                return GetCardState() switch
                {
                    ATMCardState.ConnectedActive      => "● متصل ونشط",
                    ATMCardState.ConnectedIdle        => "● متصل خامل",
                    ATMCardState.Syncing              => "⟳ يزامن",
                    ATMCardState.WaitingReply         => "◎ ينتظر رد",
                    ATMCardState.Supervisor           => "★ Supervisor",
                    ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                    ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                    ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                    ATMCardState.NeverConnected       => "○ لم يتصل",
                    _                                 => "?"
                };
            }
    
            public string GetStatusDescription()        => GetStatusLabel();
            public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    
            public string GetElapsed(DateTime utcRef)
            {
                if (utcRef == DateTime.MinValue) return "---";
                var s = (DateTime.UtcNow - utcRef).TotalSeconds;
                if (s < 60)   return $"{(int)s}ث";
                if (s < 3600) return $"{(int)(s / 60)}د";
                return $"{(int)(s / 3600)}س {(int)((s % 3600) / 60)}د";
            }
    
            // ==========================================
            // لون حالة الصراف — من النسخة القديمة (فريد، نصّي)
            // ==========================================
    
            /// <summary>
            /// يعيد كود لون HTML بناءً على حالة الاتصال — من النسخة القديمة للتوافق.
            /// للنسخة الجديدة استخدم GetCardColor().
            /// </summary>
            public string GetStatusColor()
            {
                if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
                if (!IsConnected)
                {
                    var elapsed = DateTime.Now - LastConnectionTime;
                    if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_OFFLINE;
                    if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                        return ATMStatusColors.COLOR_WARNING;
                    return ATMStatusColors.COLOR_OFFLINE;
                }
                if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
                var idleElapsed = DateTime.Now - LastDataReceived;
                if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                    return ATMStatusColors.COLOR_IDLE;
                return ATMStatusColors.COLOR_ACTIVE;
            }
    
            // ==========================================
            // استراتيجية المزامنة — من النسخة القديمة (فريد)
            // ==========================================
    
            public SyncStrategy GetSyncStrategy()
            {
                switch (ATM_Type)
                {
                    case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                    case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                    case AppConstants.ATM_TYPE_WN:  return SyncStrategy.WN_DailyFiles;
                    case AppConstants.ATM_TYPE_DN:  return SyncStrategy.DN_DailyFiles;
                    case AppConstants.ATM_TYPE_HY:  return SyncStrategy.HY_DailyFiles;
                    default:                        return SyncStrategy.NCR_Overwrite;
                }
            }
    
            // ==========================================
            // هل يحتاج تنبيهاً؟ — من النسخة القديمة (فريد)
            // ==========================================
    
            public bool NeedsAlert()
            {
                if (!IsConnected) return true;
                return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
            }
    
            // ==========================================
            // إعادة حساب نقاط الصحة
            // ==========================================
    
            public void RecalculateHealthScore()
            {
                var score = 100;
                if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
                if (Latency_ms > 500) score -= 15;
                if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
                if (CpuUsagePercent    > 90) score -= 10;
                if (MemoryUsagePercent > 90) score -= 10;
                if (DiskUsagePercent   > 95) score -= 10;
                HealthScore = Math.Max(0, Math.Min(100, score));
            }
    
            public override string ToString() =>
                $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
        }
    // Enum: ATMStatus (from 2 sources)
        public partial enum ATMStatus
        {
            // --- Constants & Fields ---
                    Unknown = 0,
    
                    Online = 1,
    
                    Idle = 2,
    
                    Supervisor = 3,
    
                    Warning = 4,
    
                    Offline = 5,
    
                    Critical = 6,
    
                    InService = 10,
    
                    ConnectedOnly = 11,
    
                    WaitingResponse = 12,
    
                    OutOfService = 13,
    
                    CriticalFault = 14,
    
                    Fault = 15,
    
                    Maintenance = 16
    
    
        }
    // Enum: ATMStatus (from 2 sources)
        public partial enum ATMStatus
        {
        }
    // Enum: ATMStatus (from 1 sources)
        public partial enum ATMStatus
        {
            // --- Constants & Fields ---
                    Unknown          = 0,
    
                    Online           = 1,
    
                    Idle             = 2,
    
                    Supervisor       = 3,
    
                    Warning          = 4,
    
                    Offline          = 5,
    
                    Critical         = 6,
    
                    InService        = 10,
    
                    ConnectedOnly    = 11,
    
                    WaitingResponse  = 12,
    
                    OutOfService     = 13,
    
                    CriticalFault    = 14,
    
                    Fault            = 15,
    
                    Maintenance      = 16
    
    
        }
    // ═══ Enum: ATMStatus (from 1 sources) ═══
        public partial enum ATMStatus
        {
        }
    // Enum: ATMType (from 4 sources)
        public partial enum ATMType
        {
            // --- Constants & Fields ---
                    NCR,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    Other
    
    
        }
    // Enum: ATMType (from 2 sources)
        public partial enum ATMType
        {
            // --- Constants & Fields ---
                    NCR,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
                    Other
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
                    Other
    
    
        }
    // Enum: ATMType (from 2 sources)
        public partial enum ATMType
        {
            // --- Constants & Fields ---
                            NCR,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Models\ATMInfo.cs
                            Other
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Models\ATMInfo.cs
                    Other
    
    
        }
    // Enum: ATMType (from 1 sources)
        public partial enum ATMType
        {
            // --- Constants & Fields ---
                    NCR,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Models\ATMInfo.cs
                    Other
    
    
        }
    // Class: ClientConfig (from 9 sources)
        public partial class ClientConfig
        {
            // --- Properties ---
                    public string ATM_ID { get; set; }
    
                    public string ATM_Name { get; set; }
    
                    public string ATM_Type { get; set; }
    
                    public string ServerIP { get; set; }
    
                    public int ServerPort { get; set; }
    
                    public bool SyncTimeEnabled { get; set; }
    
                    public int MessageSizeLines { get; set; }
    
                    public int FilePackageKB { get; set; }
    
                    public string SourcePath { get; set; }
    
                    public string BackupPath { get; set; }
    
                    public bool AutoStart { get; set; }
    
                    public bool RunAsService { get; set; }
    
    
            // --- Constructors ---
                    public ClientConfig()
                    {
                        ServerPort = NetworkConfig.DEFAULT_PORT;
                        MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                        FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                        AutoStart = true;
                        RunAsService = true;
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
                    public ClientConfig()
                    {
                        ServerPort = AppConstants.DefaultPort;
                        MessageSizeLines = 50;
                        FilePackageKB = 512;
                        AutoStart = true;
                        RunAsService = true;
                    }
    
    
        }
    // Class: ClientConfig (from 2 sources)
        public partial class ClientConfig
        {
            // --- Properties ---
                    public string ATM_ID { get; set; }
    
                    public string ATM_Name { get; set; }
    
                    public string ATM_Type { get; set; }
    
                    public string ServerIP { get; set; }
    
                    public int ServerPort { get; set; }
    
                    public bool SyncTimeEnabled { get; set; }
    
                    public int MessageSizeLines { get; set; }
    
                    public int FilePackageKB { get; set; }
    
                    public string SourcePath { get; set; }
    
                    public string BackupPath { get; set; }
    
                    public bool AutoStart { get; set; }
    
                    public bool RunAsService { get; set; }
    
                    public string NetworkQuality { get; set; }
    
    
            // --- Constructors ---
                    public ClientConfig()
                    {
                        ServerPort = NetworkConfig.DEFAULT_PORT;
                        MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                        FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                        AutoStart = true;
                        RunAsService = true;
                        NetworkQuality = "LAN";
                    }
    
    
        }
    // Class: ClientConfig (from 5 sources)
        public partial class ClientConfig
        {
            // --- Properties ---
                            public string ATM_ID { get; set; }
    
                            public string ATM_Name { get; set; }
    
                            public string ATM_Type { get; set; }
    
                            public string ServerIP { get; set; }
    
                            public int ServerPort { get; set; }
    
                            public bool SyncTimeEnabled { get; set; }
    
                            public int MessageSizeLines { get; set; }
    
                            public int FilePackageKB { get; set; }
    
                            public string SourcePath { get; set; }
    
                            public string BackupPath { get; set; }
    
                            public bool AutoStart { get; set; }
    
                            public bool RunAsService { get; set; }
    
    
            // --- Constructors ---
                            public ClientConfig()
                            {
                                ServerPort = NetworkConfig.DEFAULT_PORT;
                                MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                                FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                                AutoStart = true;
                                RunAsService = true;
                            }
    
    
        }
    // Class: ClientConfig (from 2 sources)
        public partial class ClientConfig
        {
            // --- Properties ---
                    public string ATM_ID         { get; set; }
    
                    public string ATM_Name       { get; set; }
    
                    public string ATM_Type       { get; set; }
    
                    public string ServerIP       { get; set; }
    
                    public int    ServerPort     { get; set; }
    
                    public bool   SyncTimeEnabled { get; set; }
    
                    public int    MessageSizeLines { get; set; }
    
                    public int    FilePackageKB  { get; set; }
    
                    public string SourcePath     { get; set; }
    
                    public string BackupPath     { get; set; }
    
                    public bool   AutoStart      { get; set; }
    
                    public bool   RunAsService   { get; set; }
    
    
            // --- Constructors ---
                    public ClientConfig()
                    {
                        ServerPort     = NetworkConfig.DEFAULT_PORT;
                        MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                        FilePackageKB  = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                        AutoStart      = true;
                        RunAsService   = true;
                    }
    
    
        }
    // Class: ClientConfig (from 1 sources)
        public partial class ClientConfig
        {
            // --- Properties ---
                    public string ATM_ID         { get; set; }
    
                    public string ATM_Name       { get; set; }
    
                    public string ATM_Type       { get; set; }
    
                    public string ServerIP       { get; set; }
    
                    public int    ServerPort     { get; set; }
    
                    public bool   SyncTimeEnabled { get; set; }
    
                    public int    MessageSizeLines { get; set; }
    
                    public int    FilePackageKB  { get; set; }
    
                    public string SourcePath     { get; set; }
    
                    public string BackupPath     { get; set; }
    
                    public bool   AutoStart      { get; set; }
    
                    public bool   RunAsService   { get; set; }
    
    
            // --- Constructors ---
                    public ClientConfig()
                    {
                        ServerPort     = NetworkConfig.DEFAULT_PORT;
                        MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                        FilePackageKB  = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                        AutoStart      = true;
                        RunAsService   = true;
                    }
    
    
        }
    // ═══ Class: ClientConfig (from 4 sources) ═══
        public partial class ClientConfig
        {
            // --- Properties ---
                    public string ATM_ID { get; set; }
    
                    public string ATM_Name { get; set; }
    
                    public string ATM_Type { get; set; }
    
                    public string ServerIP { get; set; }
    
                    public int ServerPort { get; set; }
    
                    public bool SyncTimeEnabled { get; set; }
    
                    public int MessageSizeLines { get; set; }
    
                    public int FilePackageKB { get; set; }
    
                    public string SourcePath { get; set; }
    
                    public string BackupPath { get; set; }
    
                    public bool AutoStart { get; set; }
    
                    public bool RunAsService { get; set; }
    
    
            // --- Constructors ---
                    public ClientConfig()
                    {
                        ServerPort = NetworkConfig.DEFAULT_PORT;
                        MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
                        FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
                        AutoStart = true;
                        RunAsService = true;
                    }
    
    
        }
    // Enum: ConnectionStatus (from 6 sources)
        public partial enum ConnectionStatus
        {
            // --- Constants & Fields ---
                    Disconnected  = 0,
    
                    Connecting    = 1,
    
                    Connected     = 2,
    
                    WaitingReply  = 3,
    
                    Syncing       = 4
    
    
        }
    // Enum: ConnectionStatus (from 2 sources)
        public partial enum ConnectionStatus
        {
        }
    // Enum: ConnectionStatus (from 2 sources)
        public partial enum ConnectionStatus
        {
            // --- Constants & Fields ---
                    Disconnected  = 0,
    
                    Connecting    = 1,
    
                    Connected     = 2,
    
                    WaitingReply  = 3,
    
                    Syncing       = 4
    
    
        }
    // Enum: ConnectionStatus (from 1 sources)
        public partial enum ConnectionStatus
        {
            // --- Constants & Fields ---
                    Disconnected  = 0,
    
                    Connecting    = 1,
    
                    Connected     = 2,
    
                    WaitingReply  = 3,
    
                    Syncing       = 4
    
    
        }
    // ═══ Enum: ConnectionStatus (from 1 sources) ═══
        public partial enum ConnectionStatus
        {
        }
    // ==========================================
        // Enumerations — مستوى namespace (تم نقلها من داخل الكلاس الخاطئ)
        // ==========================================
    
        public enum ConnectionStatus
        {
            Disconnected  = 0,
            Connecting    = 1,
            Connected     = 2,
            WaitingReply  = 3,
            Syncing       = 4
        }
    // Class: JournalSyncRecord (from 1 sources)
        public partial class JournalSyncRecord
        {
            // --- Properties ---
                    public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string           ATM_ID          { get; set; }
    
                    public string           FileName        { get; set; }
    
                    public long             FileSize        { get; set; }
    
                    public long             FileOffset      { get; set; }
    
                    public string           Checksum        { get; set; }
    
                    public string           MD5Hash         { get; set; }
    
                    public string           SHA256Hash      { get; set; }
    
                    public JournalSyncState State           { get; set; }
    
                    public int              ProgressPercent { get; set; }
    
                    public int              RetryCount      { get; set; }
    
                    public string           LocalPath       { get; set; }
    
                    public string           ServerPath      { get; set; }
    
                    public string           Message         { get; set; }
    
                    public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
    
                    public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
    
                    public DateTime?        CompletedAtUtc  { get; set; }
    
                    public string StateIcon => State switch
                    {
                        JournalSyncState.Pending   => "PEND",
                        JournalSyncState.Syncing   => "SYNC",
                        JournalSyncState.ReSyncing => "RSYNC",
                        JournalSyncState.Completed => "OK",
                        JournalSyncState.Failed    => "FAIL",
                        JournalSyncState.Archived  => "ARCH",
                        _                          => "?"
                    };
    
                    public string StateLabel => State switch
                    {
                        JournalSyncState.Pending   => "في الطابور",
                        JournalSyncState.Syncing   => "قيد المزامنة",
                        JournalSyncState.ReSyncing => "إعادة مزامنة",
                        JournalSyncState.Completed => "محمّل",
                        JournalSyncState.Failed    => "فشل",
                        JournalSyncState.Archived  => "مؤرشف",
                        _                          => "؟"
                    };
    
    
        }
    // Class: JournalSyncRecord (from 2 sources)
        public partial class JournalSyncRecord
        {
            // --- Properties ---
                            public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
    
                            public string           ATM_ID          { get; set; }
    
                            public string           FileName        { get; set; }
    
                            public long             FileSize        { get; set; }
    
                            public long             FileOffset      { get; set; }
    
                            public string           Checksum        { get; set; }
    
                            public string           MD5Hash         { get; set; }
    
                            public string           SHA256Hash      { get; set; }
    
                            public JournalSyncState State           { get; set; }
    
                            public int              ProgressPercent { get; set; }
    
                            public int              RetryCount      { get; set; }
    
                            public string           LocalPath       { get; set; }
    
                            public string           ServerPath      { get; set; }
    
                            public string           Message         { get; set; }
    
                            public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
    
                            public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
    
                            public DateTime?        CompletedAtUtc  { get; set; }
    
                            public string StateIcon => State switch
                            {
                                JournalSyncState.Pending   => "⏳",
                                JournalSyncState.Syncing   => "🔄",
                                JournalSyncState.ReSyncing => "♻️",
                                JournalSyncState.Completed => "✅",
                                JournalSyncState.Failed    => "❌",
                                JournalSyncState.Archived  => "📦",
                                _ => "?"
                            };
    
                            public string StateLabel => State switch
                            {
                                JournalSyncState.Pending   => "في الطابور",
                                JournalSyncState.Syncing   => "قيد المزامنة",
                                JournalSyncState.ReSyncing => "إعادة مزامنة",
                                JournalSyncState.Completed => "محمّل",
                                JournalSyncState.Failed    => "فشل",
                                JournalSyncState.Archived  => "مؤرشف",
                                _ => "؟"
                            };
    
    
        }
    // Class: JournalSyncRecord (from 2 sources)
        public partial class JournalSyncRecord
        {
            // --- Properties ---
                    public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string           ATM_ID          { get; set; }
    
                    public string           FileName        { get; set; }
    
                    public long             FileSize        { get; set; }
    
                    public long             FileOffset      { get; set; }
    
                    public string           Checksum        { get; set; }
    
                    public string           MD5Hash         { get; set; }
    
                    public string           SHA256Hash      { get; set; }
    
                    public JournalSyncState State           { get; set; }
    
                    public int              ProgressPercent { get; set; }
    
                    public int              RetryCount      { get; set; }
    
                    public string           LocalPath       { get; set; }
    
                    public string           ServerPath      { get; set; }
    
                    public string           Message         { get; set; }
    
                    public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
    
                    public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
    
                    public DateTime?        CompletedAtUtc  { get; set; }
    
                    public string StateIcon => State switch
                    {
                        JournalSyncState.Pending   => "PEND",
                        JournalSyncState.Syncing   => "SYNC",
                        JournalSyncState.ReSyncing => "RSYNC",
                        JournalSyncState.Completed => "OK",
                        JournalSyncState.Failed    => "FAIL",
                        JournalSyncState.Archived  => "ARCH",
                        _                          => "?"
                    };
    
                    public string StateLabel => State switch
                    {
                        JournalSyncState.Pending   => "في الطابور",
                        JournalSyncState.Syncing   => "قيد المزامنة",
                        JournalSyncState.ReSyncing => "إعادة مزامنة",
                        JournalSyncState.Completed => "محمّل",
                        JournalSyncState.Failed    => "فشل",
                        JournalSyncState.Archived  => "مؤرشف",
                        _                          => "؟"
                    };
    
    
        }
    // ═══ Class: JournalSyncRecord (from 1 sources) ═══
        public partial class JournalSyncRecord
        {
            // --- Properties ---
                    public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string           ATM_ID          { get; set; }
    
                    public string           FileName        { get; set; }
    
                    public long             FileSize        { get; set; }
    
                    public long             FileOffset      { get; set; }
    
                    public string           Checksum        { get; set; }
    
                    public string           MD5Hash         { get; set; }
    
                    public string           SHA256Hash      { get; set; }
    
                    public JournalSyncState State           { get; set; }
    
                    public int              ProgressPercent { get; set; }
    
                    public int              RetryCount      { get; set; }
    
                    public string           LocalPath       { get; set; }
    
                    public string           ServerPath      { get; set; }
    
                    public string           Message         { get; set; }
    
                    public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
    
                    public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
    
                    public DateTime?        CompletedAtUtc  { get; set; }
    
                    public string StateIcon => State switch
                    {
                        JournalSyncState.Pending   => "⏳",
                        JournalSyncState.Syncing   => "🔄",
                        JournalSyncState.ReSyncing => "♻️",
                        JournalSyncState.Completed => "✅",
                        JournalSyncState.Failed    => "❌",
                        JournalSyncState.Archived  => "📦",
                        _ => "?"
                    };
    
                    public string StateLabel => State switch
                    {
                        JournalSyncState.Pending   => "في الطابور",
                        JournalSyncState.Syncing   => "قيد المزامنة",
                        JournalSyncState.ReSyncing => "إعادة مزامنة",
                        JournalSyncState.Completed => "محمّل",
                        JournalSyncState.Failed    => "فشل",
                        JournalSyncState.Archived  => "مؤرشف",
                        _ => "؟"
                    };
    
    
        }
    // Enum: JournalSyncState (from 2 sources)
        public partial enum JournalSyncState
        {
        }
    // ═══ Enum: JournalSyncState (from 1 sources) ═══
        public partial enum JournalSyncState
        {
        }
    // Class: ServerConfig (from 9 sources)
        public partial class ServerConfig
        {
            // --- Properties ---
                    public int ListenPort { get; set; }
    
                    public string StoragePath { get; set; }
    
                    public string ArchivePath { get; set; }
    
                    public bool AutoArchive { get; set; }
    
                    public int MaxConnections { get; set; }
    
                    public bool EnableEncryption { get; set; }
    
                    public bool EnableCompression { get; set; }
    
    
            // --- Constructors ---
                    public ServerConfig()
                    {
                        ListenPort = NetworkConfig.DEFAULT_PORT;
                        StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                        ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                        AutoArchive = true;
                        MaxConnections = 100;
                        EnableEncryption = true;
                        EnableCompression = true;
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
                    public ServerConfig()
                    {
                        ListenPort = AppConstants.DefaultPort;
                        StoragePath = @"C:\EJLive\Storage";
                        ArchivePath = @"C:\EJLive\Archive";
                        AutoArchive = true;
                        MaxConnections = 100;
                        EnableEncryption = true;
                        EnableCompression = true;
                    }
    
    
        }
    // Class: ServerConfig (from 2 sources)
        public partial class ServerConfig
        {
            // --- Properties ---
                    public int ListenPort { get; set; }
    
                    public string StoragePath { get; set; }
    
                    public string ArchivePath { get; set; }
    
                    public bool AutoArchive { get; set; }
    
                    public int MaxConnections { get; set; }
    
                    public bool EnableEncryption { get; set; }
    
                    public bool EnableCompression { get; set; }
    
    
            // --- Constructors ---
                    public ServerConfig()
                    {
                        ListenPort = NetworkConfig.DEFAULT_PORT;
                        StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                        ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                        AutoArchive = true;
                        MaxConnections = 100;
                        EnableEncryption = true;
                        EnableCompression = true;
                    }
    
    
        }
    // Class: ServerConfig (from 5 sources)
        public partial class ServerConfig
        {
            // --- Properties ---
                            public int ListenPort { get; set; }
    
                            public string StoragePath { get; set; }
    
                            public string ArchivePath { get; set; }
    
                            public bool AutoArchive { get; set; }
    
                            public int MaxConnections { get; set; }
    
                            public bool EnableEncryption { get; set; }
    
                            public bool EnableCompression { get; set; }
    
    
            // --- Constructors ---
                            public ServerConfig()
                            {
                                ListenPort = NetworkConfig.DEFAULT_PORT;
                                StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                                ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                                AutoArchive = true;
                                MaxConnections = 100;
                                EnableEncryption = true;
                                EnableCompression = true;
                            }
    
    
        }
    // Class: ServerConfig (from 1 sources)
        public partial class ServerConfig
        {
            // --- Properties ---
                    public int    ListenPort         { get; set; }
    
                    public string StoragePath        { get; set; }
    
                    public string ArchivePath        { get; set; }
    
                    public bool   AutoArchive        { get; set; }
    
                    public int    MaxConnections     { get; set; }
    
                    public bool   EnableEncryption   { get; set; }
    
                    public bool   EnableCompression  { get; set; }
    
    
            // --- Constructors ---
                    public ServerConfig()
                    {
                        ListenPort        = NetworkConfig.DEFAULT_PORT;
                        StoragePath       = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                        ArchivePath       = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                        AutoArchive       = true;
                        MaxConnections    = 100;
                        EnableEncryption  = true;
                        EnableCompression = true;
                    }
    
    
        }
    // ═══ Class: ServerConfig (from 4 sources) ═══
        public partial class ServerConfig
        {
            // --- Properties ---
                    public int ListenPort { get; set; }
    
                    public string StoragePath { get; set; }
    
                    public string ArchivePath { get; set; }
    
                    public bool AutoArchive { get; set; }
    
                    public int MaxConnections { get; set; }
    
                    public bool EnableEncryption { get; set; }
    
                    public bool EnableCompression { get; set; }
    
    
            // --- Constructors ---
                    public ServerConfig()
                    {
                        ListenPort = NetworkConfig.DEFAULT_PORT;
                        StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
                        ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
                        AutoArchive = true;
                        MaxConnections = 100;
                        EnableEncryption = true;
                        EnableCompression = true;
                    }
    
    
        }
    // Enum: SyncStatus (from 6 sources)
        public partial enum SyncStatus
        {
            // --- Constants & Fields ---
                    Pending,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
                    Failed,
    
                    Idle = Pending
    
    
        }
    // Enum: SyncStatus (from 2 sources)
        public partial enum SyncStatus
        {
            // --- Constants & Fields ---
                    Pending,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
                    Failed,
    
                    Idle = Pending
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
                    Failed,
    
    
        }
    // Enum: SyncStatus (from 2 sources)
        public partial enum SyncStatus
        {
            // --- Constants & Fields ---
                            Pending,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Models\ATMInfo.cs
                            Failed,
    
                            Idle = Pending
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Models\ATMInfo.cs
                    Failed,
    
    
        }
    // Enum: SyncStatus (from 1 sources)
        public partial enum SyncStatus
        {
            // --- Constants & Fields ---
                    Pending,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Models\ATMInfo.cs
                    Failed,
    
                    Idle = Pending
    
    
        }
    // Enum: SyncStrategy (from 3 sources)
        public partial enum SyncStrategy
        {
        }

    public partial enum AlertSeverity
        {
            Info      = 0,
    
    
            Warning   = 1,
    
    
            Critical  = 2,
    
    
            Emergency = 3
    
    
        }
    public partial enum AlertSeverity
        {
        }
    public enum AlertSeverity { Info = 0, Warning = 1, Critical = 2, Emergency = 3 }
    public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    public partial enum ATMCardState
        {
            NeverConnected,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            CriticalOffline
    
    
        }
    public partial enum ATMCardState
        {
        }
    public partial enum ATMCardState
        {
            NeverConnected,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
        }
    public partial enum ATMCardState
        {
            NeverConnected,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            CriticalOffline
    
    
        }
    public enum ATMCardState
        {
            NeverConnected,
            ConnectedActive,
            ConnectedIdle,
            Syncing,
            WaitingReply,
            Supervisor,
            RecentlyDisconnected,
            WarningOffline,
            CriticalOffline
        }
    public partial enum ATMStatus
        {
            Unknown = 0,
    
    
            Online = 1,
    
    
            Idle = 2,
    
    
            Supervisor = 3,
    
    
            Warning = 4,
    
    
            Offline = 5,
    
    
            Critical = 6,
    
    
            InService = 10,
    
    
            ConnectedOnly = 11,
    
    
            WaitingResponse = 12,
    
    
            OutOfService = 13,
    
    
            CriticalFault = 14,
    
    
            Fault = 15,
    
    
            Maintenance = 16
    
    
        }
    public partial enum ATMStatus
        {
        }
    public enum ATMStatus
        {
            Unknown = 0,
            Online = 1,
            Idle = 2,
            Supervisor = 3,
            Warning = 4,
            Offline = 5,
            Critical = 6,
            InService = 10,
            ConnectedOnly = 11,
            WaitingResponse = 12,
            OutOfService = 13,
            CriticalFault = 14,
            Fault = 15,
            Maintenance = 16
        }
    public enum ATMStatus         { Unknown=0, Online=1, Idle=2, Supervisor=3, Warning=4, Offline=5, Critical=6 }
    public partial enum ATMType
        {
            NCR,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            Other
    
    
        }
    public partial enum ATMType
        {
            NCR,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
        }
    public partial enum ATMType
        {
            NCR,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Other
    
    
        }
    public enum ATMType { NCR, GRG, WN, DieboldNixdorf, Hyosung, Other }
    public partial enum ConnectionStatus
        {
            Disconnected  = 0,
    
    
            Connecting    = 1,
    
    
            Connected     = 2,
    
    
            WaitingReply  = 3,
    
    
            Syncing       = 4
    
    
        }
    public partial enum ConnectionStatus
        {
        }
    // ==========================================
        // Enumerations
        // ==========================================
    
        public enum ConnectionStatus { Disconnected = 0, Connecting = 1, Connected = 2, WaitingReply = 3, Syncing = 4 }
    // ==========================================
        // Enumerations
        // ==========================================
    
        public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    public partial enum JournalSyncState
        {
        }
    public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    public partial enum SyncStatus
        {
            Pending,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
            Idle = Pending
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMInfo.cs.v23_bak
            Failed,
    
    
        }
    public partial enum SyncStatus
        {
            Pending,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
            Idle = Pending
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
        }
    public partial enum SyncStatus
        {
            Pending,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
            Idle = Pending
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Models\ATMInfo.cs
            Failed,
    
    
        }
    public enum SyncStatus { Pending, InProgress, Syncing, Resyncing, Completed, Failed, Idle }
    public enum SyncStatus
        {
            Pending,
            InProgress,
            Syncing,
            Resyncing,
            Completed,
            Failed,
            /// <summary>alias للتوافق مع الكود القديم</summary>
            Idle = Pending
        }
    public partial enum SyncStrategy
        {
        }
    public enum SyncStrategy { NCR_Overwrite, GRG_DailyFiles, WN_DailyFiles }
}
