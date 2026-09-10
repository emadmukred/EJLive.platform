using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Reduces evidence events from multiple sources
    /// (heartbeat, journal, NDC/COM, XFS/SP, device logs, vendor errors,
    /// cash counters, remote operation audit) into a unified
    /// ATMRealTimeStatusSnapshot v2.0.
    /// This is the central evidence-to-status reducer.
    /// </summary>
    public sealed class ATMRealTimeStatusReducer : IDisposable
    {
        private readonly ConcurrentDictionary<string, ATMRealTimeStatusSnapshot> _snapshots = new();

        /// <summary>Fired whenever a snapshot is updated with new evidence.</summary>
        public event EventHandler<ATMRealTimeStatusSnapshot>? OnSnapshotUpdated;

        /// <summary>Get or create a snapshot for a given ATM.</summary>
        public ATMRealTimeStatusSnapshot GetOrCreate(string atmId)
        {
            return _snapshots.GetOrAdd(atmId, _ => new ATMRealTimeStatusSnapshot { ATM_ID = atmId });
        }

        /// <summary>Get the latest snapshot for an ATM. Returns null if not found.</summary>
        public ATMRealTimeStatusSnapshot? GetLatest(string atmId)
        {
            return _snapshots.TryGetValue(atmId, out var snapshot) ? snapshot : null;
        }

        /// <summary>All currently tracked ATM IDs.</summary>
        public ICollection<string> TrackedAtmIds => _snapshots.Keys;

        /// <summary>Total number of tracked ATMs.</summary>
        public int Count => _snapshots.Count;

        // ===== Identity & Location =====

        /// <summary>Set the ATM identity and location details.</summary>
        public void SetIdentity(string atmId, string? atmName = null, string? vendor = null,
            string? model = null, string? terminalId = null, string? luno = null,
            string? branch = null, string? region = null, string? province = null,
            string? city = null, string? geography = null, string? ipAddress = null, int port = 0)
        {
            var snapshot = GetOrCreate(atmId);
            if (atmName != null) snapshot.ATM_Name = atmName;
            if (vendor != null) snapshot.Vendor = vendor;
            if (model != null) snapshot.Model = model;
            if (terminalId != null) snapshot.TerminalId = terminalId;
            if (luno != null) snapshot.LUNO = luno;
            if (branch != null) snapshot.Branch = branch;
            if (region != null) snapshot.Region = region;
            if (province != null) snapshot.Province = province;
            if (city != null) snapshot.City = city;
            if (geography != null) snapshot.Geography = geography;
            if (ipAddress != null) snapshot.IPAddress = ipAddress;
            snapshot.Port = port;
            snapshot.LastUpdatedUtc = DateTime.UtcNow;
            Publish(snapshot);
        }

        // ===== Connection & Heartbeat =====

        /// <summary>Update the connection and heartbeat evidence for an ATM.</summary>
        public void SetConnectionState(string atmId, string connectionState, bool isOnline,
            DateTime? lastHeartbeat, DateTime? lastHandshake = null, long latencyMs = 0,
            bool hasIpPortConflict = false, string? sessionId = null, int reconnectAttempts = 0)
        {
            var snapshot = GetOrCreate(atmId);
            snapshot.ConnectionState = connectionState;
            snapshot.IsOnline = isOnline;
            snapshot.LastHeartbeatUtc = lastHeartbeat;
            if (lastHandshake.HasValue) snapshot.LastHandshakeUtc = lastHandshake;
            snapshot.LatencyMs = latencyMs;
            snapshot.HasIpPortConflict = hasIpPortConflict;
            if (sessionId != null) snapshot.SessionId = sessionId;
            snapshot.ReconnectAttempts = reconnectAttempts;
            snapshot.LastUpdatedUtc = DateTime.UtcNow;
            if (!snapshot.EvidenceSources.Contains("Heartbeat"))
                snapshot.EvidenceSources.Add("Heartbeat");
            Publish(snapshot);
        }

        // ===== Service State =====

        /// <summary>Update the Windows Service and component states.</summary>
        public void SetServiceState(string atmId, string agentServiceState,
            string? watcherState = null, string? outboxState = null, string? syncState = null,
            DateTime? lastFileSyncUtc = null, DateTime? lastEJournalSyncUtc = null)
        {
            var snapshot = GetOrCreate(atmId);
            snapshot.AgentServiceState = agentServiceState;
            if (watcherState != null) snapshot.WatcherState = watcherState;
            if (outboxState != null) snapshot.OutboxState = outboxState;
            if (syncState != null) snapshot.SyncState = syncState;
            if (lastFileSyncUtc.HasValue) snapshot.LastFileSyncUtc = lastFileSyncUtc;
            if (lastEJournalSyncUtc.HasValue) snapshot.LastEJournalSyncUtc = lastEJournalSyncUtc;
            snapshot.LastUpdatedUtc = DateTime.UtcNow;
            Publish(snapshot);
        }

        // ===== Operational Mode =====

        /// <summary>Update the ATM operational mode.</summary>
        public void SetOperationalMode(string atmId, string operationalMode)
        {
            var snapshot = GetOrCreate(atmId);
            snapshot.OperationalMode = operationalMode;
            snapshot.LastUpdatedUtc = DateTime.UtcNow;
            Publish(snapshot);
        }

        // ===== Journal / EJ Evidence =====

        /// <summary>Update the journal/EJ evidence for an ATM.</summary>
        public void SetJournalEvidence(string atmId, string? lastJournalFile = null,
            long lastJournalLine = 0, string? currentTransaction = null,
            string? journalHealth = null, string? eJournalBackupState = null,
            bool journalStale = false, int journalDeltaLines = 0, DateTime? lastSyncUtc = null)
        {
            var snapshot = GetOrCreate(atmId);
            if (lastJournalFile != null) snapshot.LastJournalFile = lastJournalFile;
            snapshot.LastJournalLine = lastJournalLine;
            if (currentTransaction != null) snapshot.CurrentTransaction = currentTransaction;
            if (journalHealth != null) snapshot.JournalHealth = journalHealth;
            if (eJournalBackupState != null) snapshot.EJournalBackupState = eJournalBackupState;
            snapshot.JournalStale = journalStale;
            snapshot.JournalDeltaLines = journalDeltaLines;
            if (lastSyncUtc.HasValue) snapshot.LastJournalSyncUtc = lastSyncUtc;
            snapshot.LastUpdatedUtc = DateTime.UtcNow;
            if (!snapshot.EvidenceSources.Contains("Journal"))
                snapshot.EvidenceSources.Add("Journal");
            Publish(snapshot);
        }

        // ===== NDC / COM Switch State =====

        /// <summary>Update the NDC/COM switch evidence for an ATM.</summary>
        public void SetNdcComEvidence(string atmId, bool available,
            string? ndcState = null, string? comState = null,
            string? lastRequest = null, string? lastReply = null,
            string? lastHostResponse = null, DateTime? lastMessageUtc = null)
        {
            var snapshot = GetOrCreate(atmId);
            snapshot.NdcComAvailable = available;
            if (ndcState != null) snapshot.NdcState = ndcState;
            if (comState != null) snapshot.ComState = comState;
            if (lastRequest != null) snapshot.LastRequest = lastRequest;
            if (lastReply != null) snapshot.LastReply = lastReply;
            if (lastHostResponse != null) snapshot.LastHostResponse = lastHostResponse;
            if (lastMessageUtc.HasValue) snapshot.LastNdcMessageUtc = lastMessageUtc;
            snapshot.LastUpdatedUtc = DateTime.UtcNow;
            if (!snapshot.EvidenceSources.Contains("NDC/COM"))
                snapshot.EvidenceSources.Add("NDC/COM");
            Publish(snapshot);
        }

        // ===== XFS / SP Device State =====

        /// <summary>Update the XFS/SP device state evidence for an ATM.</summary>
        public void SetXfsEvidence(string atmId, bool available,
            IDictionary<string, string>? deviceStates = null,
            string? cashDispenser = null, string? cardReader = null,
            string? pinPad = null, string? receiptPrinter = null,
            string? journalPrinter = null, string? siu = null,
            string? camera = null, string? depository = null, string? hddHealth = null)
        {
            var snapshot = GetOrCreate(atmId);
            snapshot.XfsAvailable = available;
            if (deviceStates != null)
                snapshot.DeviceStates = new Dictionary<string, string>(deviceStates);
            if (cashDispenser != null) snapshot.CashDispenserStatus = cashDispenser;
            if (cardReader != null) snapshot.CardReaderStatus = cardReader;
            if (pinPad != null) snapshot.PinPadStatus = pinPad;
            if (receiptPrinter != null) snapshot.ReceiptPrinterStatus = receiptPrinter;
            if (journalPrinter != null) snapshot.JournalPrinterStatus = journalPrinter;
            if (siu != null) snapshot.SiuStatus = siu;
            if (camera != null) snapshot.CameraStatus = camera;
            if (depository != null) snapshot.DepositoryStatus = depository;
            if (hddHealth != null) snapshot.HddHealth = hddHealth;
            snapshot.LastUpdatedUtc = DateTime.UtcNow;
            if (!snapshot.EvidenceSources.Contains("XFS"))
                snapshot.EvidenceSources.Add("XFS");
            Publish(snapshot);
        }

        // ===== Cash / Cassette / Replenishment =====

        /// <summary>Update the cash/cassette evidence for an ATM.</summary>
        public void SetCashEvidence(string atmId, bool available,
            IDictionary<string, long>? cassetteRemaining = null,
            long rejectBinCount = 0, long retractBinCount = 0,
            long totalCash = 0, bool cashLow = false, bool cashOut = false,
            string? currency = null, IDictionary<string, int>? denominations = null)
        {
            var snapshot = GetOrCreate(atmId);
            snapshot.CashDataAvailable = available;
            if (cassetteRemaining != null)
                snapshot.CassetteRemaining = new Dictionary<string, long>(cassetteRemaining);
            snapshot.RejectBinCount = rejectBinCount;
            snapshot.RetractBinCount = retractBinCount;
            snapshot.TotalCashRemaining = totalCash;
            snapshot.CashLow = cashLow;
            snapshot.CashOut = cashOut;
            if (currency != null) snapshot.Currency = currency;
            if (denominations != null)
                snapshot.Denominations = new Dictionary<string, int>(denominations);
            snapshot.LastUpdatedUtc = DateTime.UtcNow;
            if (!snapshot.EvidenceSources.Contains("Cash"))
                snapshot.EvidenceSources.Add("Cash");
            Publish(snapshot);
        }

        // ===== Transaction Risk Indicators =====

        /// <summary>Update the transaction risk indicators for an ATM.</summary>
        public void SetRiskIndicators(string atmId,
            bool approvedNoDispense = false, bool partialDispense = false,
            bool cashJam = false, bool retractDetected = false,
            bool reversalDetected = false, bool duplicateSequence = false,
            bool missingSequence = false, bool suspectTransaction = false,
            bool declinedTransaction = false)
        {
            var snapshot = GetOrCreate(atmId);
            snapshot.ApprovedNoDispense = approvedNoDispense;
            snapshot.PartialDispense = partialDispense;
            snapshot.CashJam = cashJam;
            snapshot.RetractDetected = retractDetected;
            snapshot.ReversalDetected = reversalDetected;
            snapshot.DuplicateSequence = duplicateSequence;
            snapshot.MissingSequence = missingSequence;
            snapshot.SuspectTransaction = suspectTransaction;
            snapshot.DeclinedTransaction = declinedTransaction;
            snapshot.LastUpdatedUtc = DateTime.UtcNow;
            Publish(snapshot);
        }

        // ===== Remote Operations State =====

        /// <summary>Update the remote operations audit state.</summary>
        public void SetRemoteOperationsState(string atmId,
            DateTime? lastRemoteDesktop = null, DateTime? lastRemoteAssistance = null,
            string? lastControlledCommandId = null, bool approvalAuditState = false)
        {
            var snapshot = GetOrCreate(atmId);
            if (lastRemoteDesktop.HasValue) snapshot.LastRemoteDesktopSessionUtc = lastRemoteDesktop;
            if (lastRemoteAssistance.HasValue) snapshot.LastRemoteAssistanceSessionUtc = lastRemoteAssistance;
            if (lastControlledCommandId != null) snapshot.LastControlledCommandId = lastControlledCommandId;
            snapshot.RemoteApprovalAuditState = approvalAuditState;
            snapshot.LastUpdatedUtc = DateTime.UtcNow;
            if (!snapshot.EvidenceSources.Contains("RemoteAudit"))
                snapshot.EvidenceSources.Add("RemoteAudit");
            Publish(snapshot);
        }

        // ===== Alarms =====

        /// <summary>Set or update the active alarms list for an ATM.</summary>
        public void SetAlarms(string atmId, List<string> alarms)
        {
            var snapshot = GetOrCreate(atmId);
            snapshot.ActiveAlarms = new List<string>(alarms);
            snapshot.LastUpdatedUtc = DateTime.UtcNow;
            Publish(snapshot);
        }

        /// <summary>Add a single alarm to the ATM's active alarms.</summary>
        public void AddAlarm(string atmId, string alarm)
        {
            var snapshot = GetOrCreate(atmId);
            if (!snapshot.ActiveAlarms.Contains(alarm))
            {
                snapshot.ActiveAlarms.Add(alarm);
                snapshot.LastUpdatedUtc = DateTime.UtcNow;
                Publish(snapshot);
            }
        }

        // ===== Vendor Extensions =====

        /// <summary>Set vendor-specific extension data.</summary>
        public void SetVendorExtensions(string atmId, IDictionary<string, string> extensions)
        {
            var snapshot = GetOrCreate(atmId);
            snapshot.VendorExtensions = new Dictionary<string, string>(extensions);
            snapshot.LastUpdatedUtc = DateTime.UtcNow;
            Publish(snapshot);
        }

        // ===== Bulk Snapshot =====

        /// <summary>Apply a full pre-built snapshot (e.g., loaded from database).</summary>
        public void ApplyFullSnapshot(ATMRealTimeStatusSnapshot fullSnapshot)
        {
            if (string.IsNullOrEmpty(fullSnapshot.ATM_ID))
                throw new ArgumentException("Snapshot must have an ATM_ID.");
            _snapshots[fullSnapshot.ATM_ID] = fullSnapshot;
            fullSnapshot.LastUpdatedUtc = DateTime.UtcNow;
            Publish(fullSnapshot);
        }

        // ===== Publish & Cleanup =====

        /// <summary>Publish updated snapshot: compute confidence, generate recommendation.</summary>
        private void Publish(ATMRealTimeStatusSnapshot snapshot)
        {
            snapshot.ComputeConfidence();
            snapshot.GenerateRecommendation();
            OnSnapshotUpdated?.Invoke(this, snapshot);
        }

        /// <summary>Remove an ATM from tracking.</summary>
        public bool Remove(string atmId)
        {
            return _snapshots.TryRemove(atmId, out _);
        }

        /// <summary>Clear all tracked snapshots.</summary>
        public void Clear()
        {
            _snapshots.Clear();
        }

        /// <summary>Get a snapshot of all tracked ATM IDs and their connection states (lightweight).</summary>
        public Dictionary<string, string> GetFleetConnectionSummary()
        {
            var summary = new Dictionary<string, string>();
            foreach (var kvp in _snapshots)
            {
                summary[kvp.Key] = kvp.Value.ConnectionState;
            }
            return summary;
        }

        public void Dispose()
        {
            _snapshots.Clear();
        }
    }
}
