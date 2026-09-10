using System;
using System.Collections.Generic;

namespace EJLive.Shared.Monitoring
{
    [Serializable]
    public class MonitoringSystemState
    {
        public DateTime LastUpdatedUtc { get; set; }
        public List<MonitoringTerminalState> Terminals { get; set; }
        public List<MonitoringAlertEntry> Alerts { get; set; }

        public MonitoringSystemState()
        {
            LastUpdatedUtc = DateTime.UtcNow;
            Terminals = new List<MonitoringTerminalState>();
            Alerts = new List<MonitoringAlertEntry>();
        }
    }

    [Serializable]
    public class MonitoringTerminalState
    {
        public string TerminalId { get; set; }
        public string BranchName { get; set; }
        public string Region { get; set; }
        public string Vendor { get; set; }
        public string Network { get; set; }
        public string Status { get; set; }
        public string Health { get; set; }
        public string RemoteEndpoint { get; set; }
        public string LastTransaction { get; set; }
        public DateTime LastHeartbeatUtc { get; set; }
        public DateTime LastEjSyncUtc { get; set; }
        public int ActiveAlerts { get; set; }
        public bool SupervisorMode { get; set; }
        public MonitoringCashState Cash { get; set; }

        public MonitoringTerminalState()
        {
            TerminalId = "Unknown";
            BranchName = "Unassigned";
            Region = "Unknown";
            Vendor = "Unknown";
            Network = "Unknown";
            Status = "Disconnected";
            Health = "Offline";
            RemoteEndpoint = string.Empty;
            LastTransaction = string.Empty;
            LastHeartbeatUtc = DateTime.MinValue;
            LastEjSyncUtc = DateTime.MinValue;
            Cash = new MonitoringCashState();
        }
    }

    [Serializable]
    public class MonitoringCashState
    {
        public int Cass1 { get; set; }
        public int Cass2 { get; set; }
        public int Cass3 { get; set; }
        public int Cass4 { get; set; }
        public int Remaining { get; set; }
        public int Loaded { get; set; }
        public int DepositIn { get; set; }
        public int DispenseOut { get; set; }
        public int Reject { get; set; }
        public int Retract { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        public MonitoringCashState()
        {
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    [Serializable]
    public class MonitoringAlertEntry
    {
        public DateTime RaisedAtUtc { get; set; }
        public string TerminalId { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }

        public MonitoringAlertEntry()
        {
            RaisedAtUtc = DateTime.UtcNow;
            TerminalId = "SYSTEM";
            Severity = "Info";
            Message = string.Empty;
        }
    }
}
