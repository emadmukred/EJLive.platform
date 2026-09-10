using System;
using System.Collections.Generic;
using System.Linq;

namespace EJLive.Monitoring.WinForms.Models
{
    internal enum TerminalHealth
    {
        Online,
        Warning,
        Critical,
        Offline
    }

    internal sealed class CashStatusSummary
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

        public int TotalCashUnits
        {
            get { return Cass1 + Cass2 + Cass3 + Cass4 + Remaining; }
        }
    }

    internal sealed class AlertEntry
    {
        public DateTime RaisedAt { get; set; }
        public string TerminalId { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
    }

    internal sealed class TerminalSnapshot
    {
        public string TerminalId { get; set; }
        public string BranchName { get; set; }
        public string Region { get; set; }
        public string Vendor { get; set; }
        public string Network { get; set; }
        public TerminalHealth Health { get; set; }
        public DateTime LastHeartbeat { get; set; }
        public DateTime LastEjSync { get; set; }
        public int ActiveAlerts { get; set; }
        public bool SupervisorMode { get; set; }
        public CashStatusSummary Cash { get; set; }
        public string LastTransaction { get; set; }
    }

    internal sealed class DashboardSnapshot
    {
        public IList<TerminalSnapshot> Terminals { get; private set; }
        public IList<AlertEntry> Alerts { get; private set; }

        public DashboardSnapshot(IList<TerminalSnapshot> terminals, IList<AlertEntry> alerts)
        {
            Terminals = terminals ?? new List<TerminalSnapshot>();
            Alerts = alerts ?? new List<AlertEntry>();
        }

        public int TotalTerminals { get { return Terminals.Count; } }
        public int OnlineTerminals { get { return Terminals.Count(x => x.Health == TerminalHealth.Online); } }
        public int WarningTerminals { get { return Terminals.Count(x => x.Health == TerminalHealth.Warning); } }
        public int CriticalTerminals { get { return Terminals.Count(x => x.Health == TerminalHealth.Critical || x.Health == TerminalHealth.Offline); } }
        public int EjSyncedTerminals { get { return Terminals.Count(x => x.LastEjSync != DateTime.MinValue && (DateTime.Now - x.LastEjSync).TotalMinutes <= 30); } }
        public int CitRequired { get { return Terminals.Count(x => x.Cash != null && x.Cash.Remaining > 0 && x.Cash.Remaining < 3000); } }
        public int SupervisorModeCount { get { return Terminals.Count(x => x.SupervisorMode); } }
        public int TotalAlerts { get { return Alerts.Count; } }
        public int TotalRemainingCash { get { return Terminals.Where(x => x.Cash != null).Sum(x => x.Cash.Remaining); } }
    }
}
