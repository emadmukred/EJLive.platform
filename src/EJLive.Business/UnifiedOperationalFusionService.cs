using System;
using System.Collections.Generic;
using System.Linq;
using EJLive.Core.Models;
using EJLive.Core.Services;

namespace EJLive.Business
{
    /// <summary>
    /// Fuses operational state, sync tracking, journal analysis, and command validation
    /// into a single coherent operational snapshot for the server dashboard.
    /// </summary>
    public sealed class UnifiedOperationalFusionService
    {
        public UnifiedOperationalFusionSnapshot Build(
            IReadOnlyCollection<ATMInfo> atmStates,
            IReadOnlyCollection<JournalSyncRecord> syncRecords,
            string journalText,
            RemoteCommand? command,
            string role,
            bool operatorConfirmed,
            bool maintenanceWindow)
        {
            var policy = new UnifiedRemoteCommandPolicy();
            var commandAllowed = true;
            var commandReason = "No command issued";

            if (command != null)
            {
                var result = policy.Validate(command.CommandType, role, operatorConfirmed, maintenanceWindow);
                commandAllowed = result.Allowed;
                commandReason = result.Reason;
            }

            var evidence = new UnifiedJournalEvidenceAnalyzer();
            var analysis = evidence.Analyze("FLT", journalText);

            return new UnifiedOperationalFusionSnapshot
            {
                FleetTotal = atmStates.Count,
                FleetOnline = atmStates.Count(a => a.ConnectionStatus == ConnectionStatus.Connected),
                SyncPending = syncRecords.Count(r => r.State == JournalSyncState.Pending),
                SyncFailed = syncRecords.Count(r => r.State == JournalSyncState.Failed),
                SyncCompleted = syncRecords.Count(r => r.State == JournalSyncState.Completed),
                JournalLinesAnalyzed = analysis.TotalLines,
                JournalConfidence = analysis.Confidence,
                CommandAllowed = commandAllowed,
                CommandReason = commandReason,
                GeneratedAtUtc = DateTime.UtcNow
            };
        }
    }

    public sealed class UnifiedOperationalFusionSnapshot
    {
        public int FleetTotal { get; set; }
        public int FleetOnline { get; set; }
        public int SyncPending { get; set; }
        public int SyncFailed { get; set; }
        public int SyncCompleted { get; set; }
        public int JournalLinesAnalyzed { get; set; }
        public string JournalConfidence { get; set; } = "Unknown";
        public bool CommandAllowed { get; set; }
        public string CommandReason { get; set; } = string.Empty;
        public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
