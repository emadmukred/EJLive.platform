using System;
using System.Collections.Generic;
using System.Linq;
using EJLive.Core.Models;
using EJLive.Core.Services;

namespace EJLive.Business
{
    /// <summary>
    /// Evaluates the operational readiness of the entire ATM fleet.
    /// Produces health scores, identifies at-risk terminals, and generates readiness reports.
    /// </summary>
    public sealed class UnifiedFleetReadinessService
    {
        private readonly OperationalStateStore _stateStore;

        public UnifiedFleetReadinessService(OperationalStateStore stateStore)
        {
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        }

        public FleetReadinessReport Evaluate()
        {
            var snapshot = _stateStore.Snapshot;
            var report = new FleetReadinessReport
            {
                GeneratedAtUtc = DateTime.UtcNow,
                TotalAtms = snapshot.Count,
                OnlineAtms = snapshot.Count(a => a.ConnectionStatus == ConnectionStatus.Connected),
                SyncingAtms = snapshot.Count(a => a.ConnectionStatus == ConnectionStatus.Syncing),
                OfflineAtms = snapshot.Count(a => a.ConnectionStatus == ConnectionStatus.Disconnected),
                WarningAtms = snapshot.Count(a => a.Status == ATMStatus.Warning),
                CriticalAtms = snapshot.Count(a => a.Status is ATMStatus.Critical or ATMStatus.CriticalFault),
                AverageHealthScore = snapshot.Count == 0 ? 0 : (int)Math.Round(snapshot.Average(a => a.HealthScore))
            };

            report.AtRiskTerminals = snapshot
                .Where(a => a.HealthScore < 50 || a.ConsecutiveSyncFailures >= 3)
                .Select(a => new AtRiskTerminal
                {
                    AtmId = a.ATMId ?? a.ATM_ID ?? string.Empty,
                    Name = a.ATMName ?? a.ATM_ID ?? string.Empty,
                    HealthScore = a.HealthScore,
                    ConsecutiveSyncFailures = a.ConsecutiveSyncFailures,
                    Status = a.ConnectionStatus.ToString()
                }).ToList();

            report.IsFleetReady = report.OnlineAtms > 0 && report.CriticalAtms == 0 && report.AverageHealthScore >= 60;
            report.Summary = $"Fleet: {report.OnlineAtms}/{report.TotalAtms} online | Health: {report.AverageHealthScore}% | Ready: {report.IsFleetReady}";

            return report;
        }
    }

    public sealed class FleetReadinessReport
    {
        public DateTime GeneratedAtUtc { get; set; }
        public int TotalAtms { get; set; }
        public int OnlineAtms { get; set; }
        public int SyncingAtms { get; set; }
        public int OfflineAtms { get; set; }
        public int WarningAtms { get; set; }
        public int CriticalAtms { get; set; }
        public int AverageHealthScore { get; set; }
        public bool IsFleetReady { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<AtRiskTerminal> AtRiskTerminals { get; set; } = new();
    }

    public sealed class AtRiskTerminal
    {
        public string AtmId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int HealthScore { get; set; }
        public int ConsecutiveSyncFailures { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
