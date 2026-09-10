using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{
    /// <summary>
    /// Comprehensive evidence-based ATM Real-Time Status Snapshot v2.0.
    /// Combines Client Heartbeat, Windows Service State, EJ/Journal Evidence,
    /// Real ATM Logs, NDC/COM Switch Flow, XFS/SP Device State, Device Logs,
    /// Vendor Error/Status Codes, Cash/Cassette/Replenishment Counters,
    /// Remote Operation Audit, Server-side Correlation, and Confidence Score.
    /// This is the single source of truth for ATM operational status.
    /// </summary>
    public sealed class ATMRealTimeStatusSnapshot
    {
        // ===== Identity & Location =====

        /// <summary>Unique ATM identifier.</summary>
        public string ATM_ID { get; set; } = string.Empty;

        /// <summary>ATM name / label.</summary>
        public string ATM_Name { get; set; } = string.Empty;

        /// <summary>Vendor: NCR, GRG, Wincor, Diebold, Hyosung, Cashway.</summary>
        public string Vendor { get; set; } = string.Empty;

        /// <summary>ATM model identifier from vendor.</summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>Terminal ID (LUNO - Logical Unit Number).</summary>
        public string TerminalId { get; set; } = string.Empty;

        /// <summary>Logical Unit Number (XFS/SP identifier).</summary>
        public string LUNO { get; set; } = string.Empty;

        /// <summary>Branch name where the ATM is deployed.</summary>
        public string Branch { get; set; } = string.Empty;

        /// <summary>Region name.</summary>
        public string Region { get; set; } = string.Empty;

        /// <summary>Province or state.</summary>
        public string Province { get; set; } = string.Empty;

        /// <summary>City name.</summary>
        public string City { get; set; } = string.Empty;

        /// <summary>Geographic coordinates or zone identifier.</summary>
        public string Geography { get; set; } = string.Empty;

        /// <summary>IP address of the ATM client.</summary>
        public string IPAddress { get; set; } = string.Empty;

        /// <summary>Communication port.</summary>
        public int Port { get; set; }

        // ===== Timestamps & Version =====

        /// <summary>UTC timestamp when this snapshot was generated.</summary>
        public DateTime SnapshotUtc { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp of the last snapshot update.</summary>
        public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Snapshot schema version.</summary>
        public string SnapshotVersion { get; set; } = "v2.0";

        // ===== Connection State =====

        /// <summary>
        /// Overall connection state: Online, Offline, Degraded, Reconnecting.
        /// </summary>
        public string ConnectionState { get; set; } = "Offline";

        /// <summary>Whether the ATM client is currently connected.</summary>
        public bool IsOnline { get; set; }

        /// <summary>Last successful heartbeat timestamp.</summary>
        public DateTime? LastHeartbeatUtc { get; set; }

        /// <summary>Last successful handshake timestamp.</summary>
        public DateTime? LastHandshakeUtc { get; set; }

        /// <summary>Network latency in milliseconds.</summary>
        public long LatencyMs { get; set; }

        /// <summary>Whether there is an IP/port conflict detected.</summary>
        public bool HasIpPortConflict { get; set; }

        /// <summary>Session identifier for the current connection.</summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>Number of reconnection attempts since the last reliable connection.</summary>
        public int ReconnectAttempts { get; set; }

        // ===== Windows Service State =====

        /// <summary>Agent service state: Running, Stopped, Starting, Paused, Degraded, Failed.</summary>
        public string AgentServiceState { get; set; } = "Unknown";

        /// <summary>File watcher state: Active, Idle, Error, Stopped.</summary>
        public string WatcherState { get; set; } = "Unknown";

        /// <summary>Outbox service state.</summary>
        public string OutboxState { get; set; } = "Unknown";

        /// <summary>Sync engine state.</summary>
        public string SyncState { get; set; } = "Unknown";

        /// <summary>Timestamp of the last successful file sync.</summary>
        public DateTime? LastFileSyncUtc { get; set; }

        /// <summary>Timestamp of the last successful eJournal sync.</summary>
        public DateTime? LastEJournalSyncUtc { get; set; }

        // ===== Operational State =====

        /// <summary>
        /// ATM operational mode: InService, OutOfService, Offline,
        /// Maintenance, Supervisor, Suspend, Unknown.
        /// </summary>
        public string OperationalMode { get; set; } = "Unknown";

        // ===== NDC / COM Switch State =====

        /// <summary>Whether NDC/COM switch communication is available.</summary>
        public bool NdcComAvailable { get; set; }

        /// <summary>NDC switch state: Online, Degraded, Offline, Unknown.</summary>
        public string NdcState { get; set; } = "Unknown";

        /// <summary>COM communication state.</summary>
        public string ComState { get; set; } = "Unknown";

        /// <summary>Last NDC/COM request sent.</summary>
        public string LastRequest { get; set; } = string.Empty;

        /// <summary>Last NDC/COM reply received.</summary>
        public string LastReply { get; set; } = string.Empty;

        /// <summary>Last host response code.</summary>
        public string LastHostResponse { get; set; } = string.Empty;

        /// <summary>Timestamp of last NDC/COM message.</summary>
        public DateTime? LastNdcMessageUtc { get; set; }

        // ===== Journal / EJ Evidence =====

        /// <summary>Last journal file name processed.</summary>
        public string LastJournalFile { get; set; } = string.Empty;

        /// <summary>Last journal line number read.</summary>
        public long LastJournalLine { get; set; }

        /// <summary>Current transaction number being processed.</summary>
        public string CurrentTransaction { get; set; } = string.Empty;

        /// <summary>Journal health: Healthy, Stale, Corrupted, Missing, Unknown.</summary>
        public string JournalHealth { get; set; } = "Unknown";

        /// <summary>eJournal backup state: Current, Stale, Missing, Unknown.</summary>
        public string EJournalBackupState { get; set; } = "Unknown";

        /// <summary>Whether the journal is stale (no updates for threshold period).</summary>
        public bool JournalStale { get; set; }

        /// <summary>Number of delta lines since last sync.</summary>
        public int JournalDeltaLines { get; set; }

        /// <summary>Timestamp of last journal sync.</summary>
        public DateTime? LastJournalSyncUtc { get; set; }

        // ===== XFS / SP Device State =====

        /// <summary>Whether XFS/SP data is available.</summary>
        public bool XfsAvailable { get; set; }

        /// <summary>Per-device states: key=device class, value=status.</summary>
        public Dictionary<string, string> DeviceStates { get; set; } = new();

        /// <summary>Cash dispenser status.</summary>
        public string CashDispenserStatus { get; set; } = "Unknown";

        /// <summary>Card reader status.</summary>
        public string CardReaderStatus { get; set; } = "Unknown";

        /// <summary>PIN pad / EPP status.</summary>
        public string PinPadStatus { get; set; } = "Unknown";

        /// <summary>Receipt printer status.</summary>
        public string ReceiptPrinterStatus { get; set; } = "Unknown";

        /// <summary>Journal printer status.</summary>
        public string JournalPrinterStatus { get; set; } = "Unknown";

        /// <summary>SIU (Sensors and Indicators Unit) status.</summary>
        public string SiuStatus { get; set; } = "Unknown";

        /// <summary>Camera status.</summary>
        public string CameraStatus { get; set; } = "Unknown";

        /// <summary>Depository status (for deposit-enabled ATMs).</summary>
        public string DepositoryStatus { get; set; } = "Unknown";

        /// <summary>Hard disk health status.</summary>
        public string HddHealth { get; set; } = "Unknown";

        // ===== Cash / Cassette / Replenishment =====

        /// <summary>Whether cash/cassette data is available.</summary>
        public bool CashDataAvailable { get; set; }

        /// <summary>Per-cassette remaining note counts: key=cassette ID, value=count.</summary>
        public Dictionary<string, long> CassetteRemaining { get; set; } = new();

        /// <summary>Reject bin note count.</summary>
        public long RejectBinCount { get; set; }

        /// <summary>Retract bin note count.</summary>
        public long RetractBinCount { get; set; }

        /// <summary>Total cash remaining across all cassettes.</summary>
        public long TotalCashRemaining { get; set; }

        /// <summary>Whether any cassette is in CashLow state.</summary>
        public bool CashLow { get; set; }

        /// <summary>Whether any cassette is in CashOut state.</summary>
        public bool CashOut { get; set; }

        /// <summary>Currency code (ISO 4217).</summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>Denomination values per cassette: key=cassette ID, value=denomination.</summary>
        public Dictionary<string, int> Denominations { get; set; } = new();

        // ===== Transaction Risk Indicators =====

        /// <summary>Whether an Approved-No-Dispense case is detected.</summary>
        public bool ApprovedNoDispense { get; set; }

        /// <summary>Whether a Partial Dispense case is detected.</summary>
        public bool PartialDispense { get; set; }

        /// <summary>Whether a Cash Jam event is detected.</summary>
        public bool CashJam { get; set; }

        /// <summary>Whether a Retract event is detected.</summary>
        public bool RetractDetected { get; set; }

        /// <summary>Whether a Reversal transaction is detected.</summary>
        public bool ReversalDetected { get; set; }

        /// <summary>Whether a Duplicate Sequence anomaly is detected.</summary>
        public bool DuplicateSequence { get; set; }

        /// <summary>Whether a Missing Sequence anomaly is detected.</summary>
        public bool MissingSequence { get; set; }

        /// <summary>Whether a Suspect transaction is flagged.</summary>
        public bool SuspectTransaction { get; set; }

        /// <summary>Whether a Declined transaction is detected.</summary>
        public bool DeclinedTransaction { get; set; }

        // ===== Remote Operations State =====

        /// <summary>Timestamp of last remote desktop session.</summary>
        public DateTime? LastRemoteDesktopSessionUtc { get; set; }

        /// <summary>Timestamp of last remote assistance session.</summary>
        public DateTime? LastRemoteAssistanceSessionUtc { get; set; }

        /// <summary>Last controlled command identifier executed.</summary>
        public string LastControlledCommandId { get; set; } = string.Empty;

        /// <summary>Whether remote operations have been audited and approved.</summary>
        public bool RemoteApprovalAuditState { get; set; }

        // ===== Impact Assessment =====

        /// <summary>Customer impact level: None, Low, Medium, High, Critical.</summary>
        public string CustomerImpact { get; set; } = "None";

        /// <summary>Cash/financial impact level.</summary>
        public string CashImpact { get; set; } = "None";

        /// <summary>Device/hardware impact level.</summary>
        public string DeviceImpact { get; set; } = "None";

        /// <summary>Switch/network communication impact level.</summary>
        public string SwitchImpact { get; set; } = "None";

        /// <summary>Audit/compliance impact level.</summary>
        public string AuditImpact { get; set; } = "None";

        // ===== Recommendation =====

        /// <summary>Recommended action to address current state.</summary>
        public string RecommendedAction { get; set; } = string.Empty;

        /// <summary>Priority: Critical, High, Medium, Low, None.</summary>
        public string Priority { get; set; } = "None";

        /// <summary>Key evidence supporting the recommendation.</summary>
        public string Evidence { get; set; } = string.Empty;

        // ===== Correlation & Confidence =====

        /// <summary>
        /// 0.0 = no evidence, 1.0 = full multi-source correlation.
        /// Computed from available evidence sources.
        /// </summary>
        public double ConfidenceScore { get; set; }

        /// <summary>List of evidence sources contributing to this snapshot.</summary>
        public List<string> EvidenceSources { get; set; } = new();

        /// <summary>List of active alarms derived from this snapshot.</summary>
        public List<string> ActiveAlarms { get; set; } = new();

        // ===== Vendor-specific Extensions =====

        /// <summary>Vendor-specific state data (serialized or key-value).</summary>
        public Dictionary<string, string> VendorExtensions { get; set; } = new();

        // ===== Computation =====

        /// <summary>
        /// Computes the confidence score based on available evidence sources.
        /// Weights: Heartbeat(1), Journal(2), NDC/COM(2), XFS/SP(2), Cash(2), RemoteAudit(1).
        /// Total max weight = 10.
        /// </summary>
        public void ComputeConfidence()
        {
            double totalWeight = 0;
            double availableWeight = 0;

            // Heartbeat & Connection (weight 1)
            totalWeight += 1;
            if (LastHeartbeatUtc.HasValue) availableWeight += 1;

            // Journal / EJ Evidence (weight 2)
            totalWeight += 2;
            if (!string.IsNullOrEmpty(LastJournalFile)) availableWeight += 1;
            if (LastJournalLine > 0) availableWeight += 1;

            // NDC/COM Switch (weight 2)
            totalWeight += 2;
            if (NdcComAvailable) availableWeight += 1;
            if (LastNdcMessageUtc.HasValue) availableWeight += 1;

            // XFS/SP Devices (weight 2)
            totalWeight += 2;
            if (XfsAvailable && DeviceStates.Count > 0) availableWeight += 1;
            if (!string.IsNullOrEmpty(CashDispenserStatus) && CashDispenserStatus != "Unknown") availableWeight += 1;

            // Cash / Cassette (weight 2)
            totalWeight += 2;
            if (CashDataAvailable && CassetteRemaining.Count > 0) availableWeight += 1;
            if (TotalCashRemaining >= 0) availableWeight += 1;

            // Remote Operations Audit (weight 1)
            totalWeight += 1;
            if (LastControlledCommandId.Length > 0 || LastRemoteDesktopSessionUtc.HasValue)
                availableWeight += 1;

            ConfidenceScore = totalWeight > 0 ? availableWeight / totalWeight : 0.0;

            // Update impact assessments based on risk indicators
            UpdateImpactAssessments();
        }

        /// <summary>
        /// Updates impact levels based on detected risk indicators.
        /// </summary>
        private void UpdateImpactAssessments()
        {
            // Customer Impact
            if (OperationalMode == "OutOfService" || OperationalMode == "Offline" || OperationalMode == "Suspend")
                CustomerImpact = IsOnline ? "Medium" : "High";
            else if (CashJam || PartialDispense || ApprovedNoDispense)
                CustomerImpact = "High";
            else if (CashLow)
                CustomerImpact = "Low";
            else if (OperationalMode == "InService")
                CustomerImpact = "None";
            else
                CustomerImpact = "Medium";

            // Cash Impact
            if (ApprovedNoDispense || CashOut || ReversalDetected)
                CashImpact = "Critical";
            else if (PartialDispense || CashJam || CashLow)
                CashImpact = "High";
            else if (RetractDetected || SuspectTransaction)
                CashImpact = "Medium";
            else
                CashImpact = "Low";

            // Device Impact
            if (CashDispenserStatus.Contains("Fault") || CashDispenserStatus.Contains("Error") ||
                CardReaderStatus.Contains("Fault") || HddHealth.Contains("Critical"))
                DeviceImpact = "Critical";
            else if (CashJam || DuplicateSequence || MissingSequence)
                DeviceImpact = "High";
            else if (!string.IsNullOrEmpty(HddHealth) && HddHealth != "Healthy")
                DeviceImpact = "Medium";
            else
                DeviceImpact = "Low";

            // Switch Impact
            if (!NdcComAvailable || NdcState == "Offline" || ComState == "Offline")
                SwitchImpact = "High";
            else if (NdcState == "Degraded" || ComState == "Degraded")
                SwitchImpact = "Medium";
            else
                SwitchImpact = "Low";

            // Audit Impact
            bool hasRemoteActivity = LastRemoteDesktopSessionUtc.HasValue ||
                                     LastRemoteAssistanceSessionUtc.HasValue ||
                                     !string.IsNullOrEmpty(LastControlledCommandId);
            AuditImpact = hasRemoteActivity ? "Medium" : "Low";
        }

        /// <summary>
        /// Generates a recommended action based on current state.
        /// </summary>
        public void GenerateRecommendation()
        {
            var recommendations = new List<string>();

            if (!IsOnline)
                recommendations.Add("Check network connectivity and restart client service.");
            if (JournalStale)
                recommendations.Add("Verify journal file path and watcher service.");
            if (CashOut)
                recommendations.Add("URGENT: Replenish cash cassettes immediately.");
            if (CashLow)
                recommendations.Add("Schedule cash replenishment soon.");
            if (ApprovedNoDispense)
                recommendations.Add("Investigate approved-no-dispense case; check dispenser logs.");
            if (CashJam)
                recommendations.Add("Clear cash jam and verify dispenser mechanism.");
            if (DuplicateSequence || MissingSequence)
                recommendations.Add("Audit transaction sequence; possible journal corruption.");
            if (PartialDispense)
                recommendations.Add("Investigate partial dispense; check cassette counts.");
            if (ReversalDetected)
                recommendations.Add("Review reversal transactions for host reconciliation.");
            if (CashDispenserStatus.Contains("Fault") || CashDispenserStatus.Contains("Error"))
                recommendations.Add("Dispatch technician for dispenser fault.");
            if (CardReaderStatus.Contains("Fault"))
                recommendations.Add("Dispatch technician for card reader fault.");
            if (OperationalMode == "OutOfService")
                recommendations.Add("Restore ATM to InService mode after resolving open issues.");
            if (AgentServiceState == "Failed" || AgentServiceState == "Stopped")
                recommendations.Add("Restart Agent Service on client machine.");
            if (ReconnectAttempts > 5)
                recommendations.Add("Investigate persistent reconnection failures; check firewall/network.");
            if (ConfidenceScore < 0.3)
                recommendations.Add("Insufficient evidence; run full diagnostics on client.");
            if (NdcState == "Offline" || ComState == "Offline")
                recommendations.Add("Check switch/host communication path.");

            RecommendedAction = recommendations.Count > 0
                ? string.Join(" ", recommendations)
                : "No action required. ATM operating normally.";

            // Determine priority
            if (CashOut || ApprovedNoDispense || CashDispenserStatus.Contains("Fault"))
                Priority = "Critical";
            else if (CashLow || CashJam || PartialDispense || OperationalMode == "OutOfService" || AgentServiceState == "Failed")
                Priority = "High";
            else if (!IsOnline || JournalStale || DuplicateSequence || MissingSequence || ReversalDetected || ReconnectAttempts > 5)
                Priority = "Medium";
            else if (ConfidenceScore < 0.3)
                Priority = "Low";
            else
                Priority = "None";
        }
    }
}
