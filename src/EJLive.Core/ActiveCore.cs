using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EJLive.Core
{
    public static class AppConstants
    {
        public const string AppName = "EJLive Enterprise";
        public const string AppVersion = "4.0.0-stabilized";
        public const int DefaultPort = 5656;
        public const int HeartbeatIntervalSeconds = 30;
        public const int HeartbeatTimeoutSeconds = 95;

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

        public static readonly string[] PolicyGovernedRemoteCommands =
        {
            "Ping",
            "ForceSync",
            "CollectDiagnostics",
            "RestartService",
            "CaptureScreenshot",
            "RetrieveFile"
        };
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
        public string DataRoot { get; set; } = "C:\\ProgramData\\EJLive";
        public bool RemoteOperationsEnabled { get; set; }
    }

    public sealed class PerformanceMetric
    {
        public string Key { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public DateTime CapturedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public sealed class DatabaseManager
    {
        private readonly List<PerformanceMetric> _metrics = new List<PerformanceMetric>
        {
            new PerformanceMetric { Key = "CPU_Usage", CurrentValue = 12 },
            new PerformanceMetric { Key = "Memory_Usage", CurrentValue = 38 },
            new PerformanceMetric { Key = "Active_Connections", CurrentValue = 0 },
            new PerformanceMetric { Key = "Error_Rate", CurrentValue = 0 }
        };

        private readonly List<EJLive.Core.ATMInfo> _atms = new List<EJLive.Core.ATMInfo>
        {
            new EJLive.Core.ATMInfo
            {
                ATMId = "ATM-DEMO-001",
                Name = "Demo Terminal",
                Vendor = "NCR",
                Branch = "Main",
                RuntimeStatus = EJLive.Core.AtmRuntimeStatus.Online,
                LastHeartbeatUtc = DateTime.UtcNow
            }
        };

        public static DatabaseManager Instance { get; } = new DatabaseManager();

        public Task<List<PerformanceMetric>> GetPerformanceDataAsync(DateTime fromDate, DateTime toDate)
        {
            return Task.FromResult(_metrics
                .Where(metric => metric.CapturedAtUtc >= fromDate && metric.CapturedAtUtc <= toDate)
                .DefaultIfEmpty()
                .Where(metric => metric != null)
                .ToList());
        }

        public Task<List<EJLive.Core.ATMInfo>> GetATMsAsync()
        {
            return Task.FromResult(_atms.Select(Clone).ToList());
        }

        private static EJLive.Core.ATMInfo Clone(EJLive.Core.ATMInfo atm)
        {
            return new EJLive.Core.ATMInfo
            {
                ATMId = atm.ATMId,
                Name = atm.Name,
                Vendor = atm.Vendor,
                Model = atm.Model,
                Branch = atm.Branch,
                Region = atm.Region,
                IpAddress = atm.IpAddress,
                Port = atm.Port,
                RuntimeStatus = atm.RuntimeStatus,
                LastHeartbeatUtc = atm.LastHeartbeatUtc,
                HasAlerts = atm.HasAlerts
            };
        }
    }
}
