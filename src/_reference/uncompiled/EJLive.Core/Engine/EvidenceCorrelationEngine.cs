using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    public sealed class CorrelationLink
        {
            public string TransactionId { get; set; } = string.Empty;
            public string EventId { get; set; } = string.Empty;
            public CorrelationMatchStrength MatchLevel { get; set; }
            public string MatchBasis { get; set; } = string.Empty;
            public double Confidence { get; set; }
            public string RootCause { get; set; } = string.Empty;
            public DateTime CorrelatedUtc { get; set; } = DateTime.UtcNow;
        }
    public partial class CorrelationLink
        {
            public string TransactionId { get; set; } = string.Empty;
    
    
            public string EventId { get; set; } = string.Empty;
    
    
            public CorrelationMatchStrength MatchLevel { get; set; }
    
    
            public string MatchBasis { get; set; } = string.Empty;
    
    
            public double Confidence { get; set; }
    
    
            public string RootCause { get; set; } = string.Empty;
    
    
            public DateTime CorrelatedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    /// <summary>
        /// Correlates EJ/Journal transactions with XFS/SP events, NDC/COM evidence,
        /// device logs, vendor errors, and cash counters to produce root-cause analysis
        /// and confidence scoring. Implements Phase 7 and section 12 of the master document.
        /// Full implementation of IEvidenceCorrelationEngine.
        /// </summary>
        public sealed class EvidenceCorrelationEngine : IEvidenceCorrelationEngine
        {
            private readonly ATMRealTimeStatusReducer _reducer = new();
            private readonly Dictionary<string, List<NormalizedVendorEvent>> _eventBuffer = new();
            private readonly Dictionary<string, List<EjTransaction>> _transactionBuffer = new();
            private readonly Dictionary<string, long> _evidenceCounts = new();
            private readonly object _lock = new();
    
            public int CorrelationWindowSeconds { get; set; } = 30;
    
            // ===== IEvidenceCorrelationEngine =====
    
            public event EventHandler<ATMRealTimeStatusSnapshot>? OnSnapshotUpdated
            {
                add => _reducer.OnSnapshotUpdated += value;
                remove => _reducer.OnSnapshotUpdated -= value;
            }
    
            public IReadOnlyList<string> ActiveAtmIds => _reducer.TrackedAtmIds.ToList().AsReadOnly();
            public ATMRealTimeStatusSnapshot? GetLatestSnapshot(string atmId) => _reducer.GetLatest(atmId);
            public long GetEvidenceItemCount(string atmId) { lock (_lock) { return _evidenceCounts.TryGetValue(atmId, out var c) ? c : 0; } }
    
            public Task IngestJournalEventAsync(string atmId, object journalEvent, CancellationToken ct)
            {
                if (journalEvent is EjTransaction tx)
                {
                    FeedTransaction(tx);
                    _reducer.SetJournalEvidence(atmId, lastJournalFile: tx.TransactionId, lastJournalLine: tx.EndLine,
                        journalDeltaLines: tx.EndLine - tx.StartLine, lastSyncUtc: DateTime.UtcNow);
                }
                IncrementEvidenceCount(atmId);
                return Task.CompletedTask;
            }
    
            public Task IngestNdcEventAsync(string atmId, object ndcEvent, CancellationToken ct)
            {
                _reducer.SetNdcComEvidence(atmId, true, ndcState: "Active", lastMessageUtc: DateTime.UtcNow);
                IncrementEvidenceCount(atmId);
                return Task.CompletedTask;
            }
    
            public Task IngestXfsEventAsync(string atmId, object xfsEvent, CancellationToken ct)
            {
                if (xfsEvent is NormalizedVendorEvent evt)
                {
                    FeedEvent(evt);
                    var deviceStates = new Dictionary<string, string> { { evt.DeviceClass, evt.Severity } };
                    _reducer.SetXfsEvidence(atmId, true, deviceStates: deviceStates);
                }
                IncrementEvidenceCount(atmId);
                return Task.CompletedTask;
            }
    
            public Task IngestDeviceLogEventAsync(string atmId, object deviceEvent, CancellationToken ct)
            {
                _reducer.SetServiceState(atmId, "Running", watcherState: "Active");
                IncrementEvidenceCount(atmId);
                return Task.CompletedTask;
            }
    
            public Task IngestVendorErrorEventAsync(string atmId, string errorCode, string severity, CancellationToken ct)
            {
                _reducer.AddAlarm(atmId, $"VendorError:{errorCode}:{severity}");
                IncrementEvidenceCount(atmId);
                return Task.CompletedTask;
            }
    
            public Task<ATMRealTimeStatusSnapshot> RecomputeSnapshotAsync(string atmId, CancellationToken ct)
            {
                var snapshot = _reducer.GetOrCreate(atmId);
                snapshot.ComputeConfidence();
                snapshot.GenerateRecommendation();
                return Task.FromResult(snapshot);
            }
    
            // ===== Core Methods =====
    
            public void FeedEvent(NormalizedVendorEvent vendorEvent)
            {
                lock (_lock)
                {
                    var key = vendorEvent.ATM_ID;
                    if (!_eventBuffer.ContainsKey(key))
                        _eventBuffer[key] = new List<NormalizedVendorEvent>();
                    _eventBuffer[key].Add(vendorEvent);
                    var cutoff = DateTime.UtcNow.AddSeconds(-CorrelationWindowSeconds * 2);
                    _eventBuffer[key].RemoveAll(e => e.Timestamp < cutoff);
                }
            }
    
            public void FeedTransaction(EjTransaction transaction)
            {
                lock (_lock)
                {
                    var key = transaction.ATM_ID ?? "UNKNOWN";
                    if (!_transactionBuffer.ContainsKey(key))
                        _transactionBuffer[key] = new List<EjTransaction>();
                    _transactionBuffer[key].Add(transaction);
                }
            }
    
            private void IncrementEvidenceCount(string atmId)
            {
                lock (_lock) { _evidenceCounts.TryGetValue(atmId, out var c); _evidenceCounts[atmId] = c + 1; }
            }
    
            public List<CorrelationLink> Correlate(EjTransaction transaction)
            {
                var links = new List<CorrelationLink>();
                var atmId = transaction.ATM_ID ?? "UNKNOWN";
                lock (_lock)
                {
                    if (!_eventBuffer.TryGetValue(atmId, out var events))
                        return links;
                    foreach (var evt in events)
                    {
                        var matchLevel = DetermineMatchLevel(transaction, evt);
                        if (matchLevel != CorrelationMatchStrength.None)
                        {
                            links.Add(new CorrelationLink
                            {
                                TransactionId = transaction.TransactionId ?? string.Empty,
                                EventId = evt.EventId,
                                MatchLevel = matchLevel,
                                MatchBasis = DetermineMatchBasis(transaction, evt),
                                Confidence = ComputeCorrelationConfidence(transaction, evt, matchLevel),
                                RootCause = DetermineRootCause(transaction, evt),
                                CorrelatedUtc = DateTime.UtcNow
                            });
                        }
                    }
                }
                return links;
            }
    
            public List<CorrelationLink> CorrelateAllForAtm(string atmId)
            {
                var allLinks = new List<CorrelationLink>();
                lock (_lock)
                {
                    if (!_transactionBuffer.TryGetValue(atmId, out var transactions))
                        return allLinks;
                    foreach (var tx in transactions)
                        allLinks.AddRange(Correlate(tx));
                }
                return allLinks;
            }
    
            public FinalTransactionStatus DetermineFinalStatus(EjTransaction transaction, List<CorrelationLink> correlationLinks)
            {
                var status = new FinalTransactionStatus
                {
                    TransactionId = transaction.TransactionId ?? string.Empty,
                    ATM_ID = transaction.ATM_ID ?? string.Empty,
                    JournalClassification = transaction.Classification.ToString(),
                    JournalConfidence = transaction.Confidence
                };
                status.TotalEvidenceSources = 1 + correlationLinks.Count;
                status.EvidenceSources.Add("Journal");
                foreach (var link in correlationLinks)
                    status.EvidenceSources.Add($"XFS:{link.EventId}");
    
                var strongLinks = correlationLinks.Where(l => l.MatchLevel == CorrelationMatchStrength.Strong).ToList();
                var mediumLinks = correlationLinks.Where(l => l.MatchLevel == CorrelationMatchStrength.Medium).ToList();
    
                if (strongLinks.Any(l => l.RootCause.Contains("jam", StringComparison.OrdinalIgnoreCase)))
                { status.FinalStatus = "CashJam"; status.Confidence = Math.Max(transaction.Confidence, 0.95); status.RootCause = strongLinks.First(l => l.RootCause.Contains("jam", StringComparison.OrdinalIgnoreCase)).RootCause; }
                else if (strongLinks.Any(l => l.RootCause.Contains("partial", StringComparison.OrdinalIgnoreCase)))
                { status.FinalStatus = "PartialDispense"; status.Confidence = Math.Max(transaction.Confidence, 0.92); status.RootCause = strongLinks.First(l => l.RootCause.Contains("partial", StringComparison.OrdinalIgnoreCase)).RootCause; }
                else if (strongLinks.Any(l => l.RootCause.Contains("Approved", StringComparison.OrdinalIgnoreCase) && l.RootCause.Contains("dispense", StringComparison.OrdinalIgnoreCase)))
                { status.FinalStatus = "ApprovedNoDispense"; status.Confidence = Math.Max(transaction.Confidence, 0.95); status.RootCause = strongLinks.First(l => l.RootCause.Contains("Approved", StringComparison.OrdinalIgnoreCase)).RootCause; }
                else if (strongLinks.Any())
                { var best = strongLinks.OrderByDescending(l => l.Confidence).First(); status.FinalStatus = transaction.Classification.ToString(); status.Confidence = Math.Max(transaction.Confidence, best.Confidence); status.RootCause = best.RootCause; }
                else if (mediumLinks.Any())
                { var best = mediumLinks.OrderByDescending(l => l.Confidence).First(); status.FinalStatus = transaction.Classification.ToString(); status.Confidence = Math.Max(transaction.Confidence, best.Confidence); status.RootCause = best.RootCause; }
                else
                { status.FinalStatus = transaction.Classification.ToString(); status.RootCause = "Journal-only classification (no device correlation)."; status.Confidence = transaction.Confidence; }
    
                return status;
            }
    
            public void Clear() { lock (_lock) { _eventBuffer.Clear(); _transactionBuffer.Clear(); _evidenceCounts.Clear(); } }
            public void Clear(string atmId) { lock (_lock) { _eventBuffer.Remove(atmId); _transactionBuffer.Remove(atmId); _evidenceCounts.Remove(atmId); } }
    
            // ===== Private Matching =====
    
            private CorrelationMatchStrength DetermineMatchLevel(EjTransaction tx, NormalizedVendorEvent evt)
            {
                var timeDelta = Math.Abs((tx.Timestamp - evt.Timestamp).TotalSeconds);
    
                // Strong match: explicit ImpactedTransactionId linkage within tight window
                if (!string.IsNullOrEmpty(tx.TransactionId) && !string.IsNullOrEmpty(evt.ImpactedTransactionId) &&
                    string.Equals(tx.TransactionId, evt.ImpactedTransactionId, StringComparison.OrdinalIgnoreCase) &&
                    timeDelta <= 10)
                    return CorrelationMatchStrength.Strong;
    
                // Strong match: same ATM + device match + very tight window (<5s)
                if (timeDelta <= 5 && IsDeviceRelated(tx, evt))
                    return CorrelationMatchStrength.Strong;
    
                // Medium match: same ATM + time window + device class match
                if (timeDelta <= CorrelationWindowSeconds && IsDeviceRelated(tx, evt))
                    return CorrelationMatchStrength.Medium;
    
                // Weak match: proximity in time
                if (timeDelta <= CorrelationWindowSeconds * 2)
                    return CorrelationMatchStrength.Weak;
    
                return CorrelationMatchStrength.None;
            }
    
            private static bool IsDeviceRelated(EjTransaction tx, NormalizedVendorEvent evt)
            {
                var dc = evt.DeviceClass.ToUpperInvariant();
                var cls = tx.Classification.ToString();
                if (dc == "CDM" || dc == "CASH_DISPENSER")
                    return cls.Contains("Dispense") || cls.Contains("Cash") || cls.Contains("Success") || cls.Contains("Failed");
                if (dc == "IDC" || dc == "CARD_READER")
                    return cls.Contains("Card") || !string.IsNullOrEmpty(tx.CardNumber);
                if (dc == "PIN" || dc == "EPP" || dc == "PTR" || dc == "PRINTER")
                    return true;
                if (dc == "SIU")
                    return cls.Contains("Fault") || cls.Contains("Hardware");
                return true;
            }
    
            private static string DetermineMatchBasis(EjTransaction tx, NormalizedVendorEvent evt)
            {
                if (!string.IsNullOrEmpty(tx.TransactionId) && !string.IsNullOrEmpty(evt.ImpactedTransactionId) &&
                    string.Equals(tx.TransactionId, evt.ImpactedTransactionId, StringComparison.OrdinalIgnoreCase))
                    return $"TxId={tx.TransactionId}";
                var delta = Math.Abs((tx.Timestamp - evt.Timestamp).TotalSeconds);
                return $"ATM={tx.ATM_ID}, TimeDelta={delta:N1}s, Device={evt.DeviceClass}";
            }
    
            private static double ComputeCorrelationConfidence(EjTransaction tx, NormalizedVendorEvent evt, CorrelationMatchStrength level)
            {
                double baseConf = level switch
                {
                    CorrelationMatchStrength.Strong => 0.95,
                    CorrelationMatchStrength.Medium => 0.75,
                    CorrelationMatchStrength.Weak => 0.50,
                    _ => 0.0
                };
                if (!string.IsNullOrEmpty(tx.STAN) && !string.IsNullOrEmpty(evt.Code)) baseConf += 0.03;
                if (IsDeviceRelated(tx, evt)) baseConf += 0.02;
                return Math.Min(baseConf, 1.0);
            }
    
            private static string DetermineRootCause(EjTransaction tx, NormalizedVendorEvent evt)
            {
                var dc = evt.DeviceClass.ToUpperInvariant();
                var sev = evt.Severity.ToUpperInvariant();
                var cls = tx.Classification.ToString();
                if ((dc == "CDM" || dc == "CASH_DISPENSER") && (sev == "ERROR" || sev == "CRITICAL"))
                {
                    if (cls.Contains("CashJam")) return "Cash dispenser jam detected - dispenser mechanism failure.";
                    if (cls.Contains("PartialDispense")) return "Partial dispense due to cash dispenser error.";
                    if (cls.Contains("ApprovedNoDispense")) return "Approved no dispense - dispenser failed to present notes.";
                    return $"Cash dispenser {sev.ToLowerInvariant()} - {evt.Message}";
                }
                if (dc == "IDC" || dc == "CARD_READER") return $"Card reader {sev.ToLowerInvariant()} - {evt.Message}";
                if (dc == "COMM" || dc == "NDC") return $"Switch communication {sev.ToLowerInvariant()} - Host response affected.";
                if (dc == "PTR" || dc == "PRINTER") return $"Printer {sev.ToLowerInvariant()} - Receipt/journal affected.";
                return $"{dc} {sev.ToLowerInvariant()} event correlated with {cls} transaction.";
            }
        }
    public partial class EvidenceCorrelationEngine : IEvidenceCorrelationEngine
        {
            private readonly ATMRealTimeStatusReducer _reducer = new();
    
    
            private readonly Dictionary<string, List<NormalizedVendorEvent>> _eventBuffer = new();
    
    
            private readonly Dictionary<string, List<EjTransaction>> _transactionBuffer = new();
    
    
            private readonly Dictionary<string, long> _evidenceCounts = new();
    
    
            private readonly object _lock = new();
    
    
            public int CorrelationWindowSeconds { get; set; } = 30;
    
    
            public IReadOnlyList<string> ActiveAtmIds => _reducer.TrackedAtmIds.ToList().AsReadOnly();
    
    
            public ATMRealTimeStatusSnapshot? GetLatestSnapshot(string atmId) => _reducer.GetLatest(atmId);
    
    
            public long GetEvidenceItemCount(string atmId) { lock (_lock) { return _evidenceCounts.TryGetValue(atmId, out var c) ? c : 0; } }
    
    
            public Task IngestJournalEventAsync(string atmId, object journalEvent, CancellationToken ct)
            {
                if (journalEvent is EjTransaction tx)
                {
                    FeedTransaction(tx);
                    _reducer.SetJournalEvidence(atmId, lastJournalFile: tx.TransactionId, lastJournalLine: tx.EndLine,
                        journalDeltaLines: tx.EndLine - tx.StartLine, lastSyncUtc: DateTime.UtcNow);
                }
                IncrementEvidenceCount(atmId);
                return Task.CompletedTask;
            }
    
    
            public Task IngestNdcEventAsync(string atmId, object ndcEvent, CancellationToken ct)
            {
                _reducer.SetNdcComEvidence(atmId, true, ndcState: "Active", lastMessageUtc: DateTime.UtcNow);
                IncrementEvidenceCount(atmId);
                return Task.CompletedTask;
            }
    
    
            public Task IngestXfsEventAsync(string atmId, object xfsEvent, CancellationToken ct)
            {
                if (xfsEvent is NormalizedVendorEvent evt)
                {
                    FeedEvent(evt);
                    var deviceStates = new Dictionary<string, string> { { evt.DeviceClass, evt.Severity } };
                    _reducer.SetXfsEvidence(atmId, true, deviceStates: deviceStates);
                }
                IncrementEvidenceCount(atmId);
                return Task.CompletedTask;
            }
    
    
            public Task IngestDeviceLogEventAsync(string atmId, object deviceEvent, CancellationToken ct)
            {
                _reducer.SetServiceState(atmId, "Running", watcherState: "Active");
                IncrementEvidenceCount(atmId);
                return Task.CompletedTask;
            }
    
    
            public Task IngestVendorErrorEventAsync(string atmId, string errorCode, string severity, CancellationToken ct)
            {
                _reducer.AddAlarm(atmId, $"VendorError:{errorCode}:{severity}");
                IncrementEvidenceCount(atmId);
                return Task.CompletedTask;
            }
    
    
            public Task<ATMRealTimeStatusSnapshot> RecomputeSnapshotAsync(string atmId, CancellationToken ct)
            {
                var snapshot = _reducer.GetOrCreate(atmId);
                snapshot.ComputeConfidence();
                snapshot.GenerateRecommendation();
                return Task.FromResult(snapshot);
            }
    
    
            public void FeedEvent(NormalizedVendorEvent vendorEvent)
            {
                lock (_lock)
                {
                    var key = vendorEvent.ATM_ID;
                    if (!_eventBuffer.ContainsKey(key))
                        _eventBuffer[key] = new List<NormalizedVendorEvent>();
                    _eventBuffer[key].Add(vendorEvent);
                    var cutoff = DateTime.UtcNow.AddSeconds(-CorrelationWindowSeconds * 2);
                    _eventBuffer[key].RemoveAll(e => e.Timestamp < cutoff);
                }
            }
    
    
            public void FeedTransaction(EjTransaction transaction)
            {
                lock (_lock)
                {
                    var key = transaction.ATM_ID ?? "UNKNOWN";
                    if (!_transactionBuffer.ContainsKey(key))
                        _transactionBuffer[key] = new List<EjTransaction>();
                    _transactionBuffer[key].Add(transaction);
                }
            }
    
    
            private void IncrementEvidenceCount(string atmId)
            {
                lock (_lock) { _evidenceCounts.TryGetValue(atmId, out var c); _evidenceCounts[atmId] = c + 1; }
            }
    
    
            public List<CorrelationLink> Correlate(EjTransaction transaction)
            {
                var links = new List<CorrelationLink>();
                var atmId = transaction.ATM_ID ?? "UNKNOWN";
                lock (_lock)
                {
                    if (!_eventBuffer.TryGetValue(atmId, out var events))
                        return links;
                    foreach (var evt in events)
                    {
                        var matchLevel = DetermineMatchLevel(transaction, evt);
                        if (matchLevel != CorrelationMatchStrength.None)
                        {
                            links.Add(new CorrelationLink
                            {
                                TransactionId = transaction.TransactionId ?? string.Empty,
                                EventId = evt.EventId,
                                MatchLevel = matchLevel,
                                MatchBasis = DetermineMatchBasis(transaction, evt),
                                Confidence = ComputeCorrelationConfidence(transaction, evt, matchLevel),
                                RootCause = DetermineRootCause(transaction, evt),
                                CorrelatedUtc = DateTime.UtcNow
                            });
                        }
                    }
                }
                return links;
            }
    
    
            public List<CorrelationLink> CorrelateAllForAtm(string atmId)
            {
                var allLinks = new List<CorrelationLink>();
                lock (_lock)
                {
                    if (!_transactionBuffer.TryGetValue(atmId, out var transactions))
                        return allLinks;
                    foreach (var tx in transactions)
                        allLinks.AddRange(Correlate(tx));
                }
                return allLinks;
            }
    
    
            public FinalTransactionStatus DetermineFinalStatus(EjTransaction transaction, List<CorrelationLink> correlationLinks)
            {
                var status = new FinalTransactionStatus
                {
                    TransactionId = transaction.TransactionId ?? string.Empty,
                    ATM_ID = transaction.ATM_ID ?? string.Empty,
                    JournalClassification = transaction.Classification.ToString(),
                    JournalConfidence = transaction.Confidence
                };
                status.TotalEvidenceSources = 1 + correlationLinks.Count;
                status.EvidenceSources.Add("Journal");
                foreach (var link in correlationLinks)
                    status.EvidenceSources.Add($"XFS:{link.EventId}");
    
                var strongLinks = correlationLinks.Where(l => l.MatchLevel == CorrelationMatchStrength.Strong).ToList();
                var mediumLinks = correlationLinks.Where(l => l.MatchLevel == CorrelationMatchStrength.Medium).ToList();
    
                if (strongLinks.Any(l => l.RootCause.Contains("jam", StringComparison.OrdinalIgnoreCase)))
                { status.FinalStatus = "CashJam"; status.Confidence = Math.Max(transaction.Confidence, 0.95); status.RootCause = strongLinks.First(l => l.RootCause.Contains("jam", StringComparison.OrdinalIgnoreCase)).RootCause; }
                else if (strongLinks.Any(l => l.RootCause.Contains("partial", StringComparison.OrdinalIgnoreCase)))
                { status.FinalStatus = "PartialDispense"; status.Confidence = Math.Max(transaction.Confidence, 0.92); status.RootCause = strongLinks.First(l => l.RootCause.Contains("partial", StringComparison.OrdinalIgnoreCase)).RootCause; }
                else if (strongLinks.Any(l => l.RootCause.Contains("Approved", StringComparison.OrdinalIgnoreCase) && l.RootCause.Contains("dispense", StringComparison.OrdinalIgnoreCase)))
                { status.FinalStatus = "ApprovedNoDispense"; status.Confidence = Math.Max(transaction.Confidence, 0.95); status.RootCause = strongLinks.First(l => l.RootCause.Contains("Approved", StringComparison.OrdinalIgnoreCase)).RootCause; }
                else if (strongLinks.Any())
                { var best = strongLinks.OrderByDescending(l => l.Confidence).First(); status.FinalStatus = transaction.Classification.ToString(); status.Confidence = Math.Max(transaction.Confidence, best.Confidence); status.RootCause = best.RootCause; }
                else if (mediumLinks.Any())
                { var best = mediumLinks.OrderByDescending(l => l.Confidence).First(); status.FinalStatus = transaction.Classification.ToString(); status.Confidence = Math.Max(transaction.Confidence, best.Confidence); status.RootCause = best.RootCause; }
                else
                { status.FinalStatus = transaction.Classification.ToString(); status.RootCause = "Journal-only classification (no device correlation)."; status.Confidence = transaction.Confidence; }
    
                return status;
            }
    
    
            public void Clear() { lock (_lock) { _eventBuffer.Clear(); _transactionBuffer.Clear(); _evidenceCounts.Clear(); } }
    
    
            public void Clear(string atmId) { lock (_lock) { _eventBuffer.Remove(atmId); _transactionBuffer.Remove(atmId); _evidenceCounts.Remove(atmId); } }
    
    
            private CorrelationMatchStrength DetermineMatchLevel(EjTransaction tx, NormalizedVendorEvent evt)
            {
                var timeDelta = Math.Abs((tx.Timestamp - evt.Timestamp).TotalSeconds);
    
                // Strong match: explicit ImpactedTransactionId linkage within tight window
                if (!string.IsNullOrEmpty(tx.TransactionId) && !string.IsNullOrEmpty(evt.ImpactedTransactionId) &&
                    string.Equals(tx.TransactionId, evt.ImpactedTransactionId, StringComparison.OrdinalIgnoreCase) &&
                    timeDelta <= 10)
                    return CorrelationMatchStrength.Strong;
    
                // Strong match: same ATM + device match + very tight window (<5s)
                if (timeDelta <= 5 && IsDeviceRelated(tx, evt))
                    return CorrelationMatchStrength.Strong;
    
                // Medium match: same ATM + time window + device class match
                if (timeDelta <= CorrelationWindowSeconds && IsDeviceRelated(tx, evt))
                    return CorrelationMatchStrength.Medium;
    
                // Weak match: proximity in time
                if (timeDelta <= CorrelationWindowSeconds * 2)
                    return CorrelationMatchStrength.Weak;
    
                return CorrelationMatchStrength.None;
            }
    
    
            private static bool IsDeviceRelated(EjTransaction tx, NormalizedVendorEvent evt)
            {
                var dc = evt.DeviceClass.ToUpperInvariant();
                var cls = tx.Classification.ToString();
                if (dc == "CDM" || dc == "CASH_DISPENSER")
                    return cls.Contains("Dispense") || cls.Contains("Cash") || cls.Contains("Success") || cls.Contains("Failed");
                if (dc == "IDC" || dc == "CARD_READER")
                    return cls.Contains("Card") || !string.IsNullOrEmpty(tx.CardNumber);
                if (dc == "PIN" || dc == "EPP" || dc == "PTR" || dc == "PRINTER")
                    return true;
                if (dc == "SIU")
                    return cls.Contains("Fault") || cls.Contains("Hardware");
                return true;
            }
    
    
            private static string DetermineMatchBasis(EjTransaction tx, NormalizedVendorEvent evt)
            {
                if (!string.IsNullOrEmpty(tx.TransactionId) && !string.IsNullOrEmpty(evt.ImpactedTransactionId) &&
                    string.Equals(tx.TransactionId, evt.ImpactedTransactionId, StringComparison.OrdinalIgnoreCase))
                    return $"TxId={tx.TransactionId}";
                var delta = Math.Abs((tx.Timestamp - evt.Timestamp).TotalSeconds);
                return $"ATM={tx.ATM_ID}, TimeDelta={delta:N1}s, Device={evt.DeviceClass}";
            }
    
    
            private static double ComputeCorrelationConfidence(EjTransaction tx, NormalizedVendorEvent evt, CorrelationMatchStrength level)
            {
                double baseConf = level switch
                {
                    CorrelationMatchStrength.Strong => 0.95,
                    CorrelationMatchStrength.Medium => 0.75,
                    CorrelationMatchStrength.Weak => 0.50,
                    _ => 0.0
                };
                if (!string.IsNullOrEmpty(tx.STAN) && !string.IsNullOrEmpty(evt.Code)) baseConf += 0.03;
                if (IsDeviceRelated(tx, evt)) baseConf += 0.02;
                return Math.Min(baseConf, 1.0);
            }
    
    
            private static string DetermineRootCause(EjTransaction tx, NormalizedVendorEvent evt)
            {
                var dc = evt.DeviceClass.ToUpperInvariant();
                var sev = evt.Severity.ToUpperInvariant();
                var cls = tx.Classification.ToString();
                if ((dc == "CDM" || dc == "CASH_DISPENSER") && (sev == "ERROR" || sev == "CRITICAL"))
                {
                    if (cls.Contains("CashJam")) return "Cash dispenser jam detected - dispenser mechanism failure.";
                    if (cls.Contains("PartialDispense")) return "Partial dispense due to cash dispenser error.";
                    if (cls.Contains("ApprovedNoDispense")) return "Approved no dispense - dispenser failed to present notes.";
                    return $"Cash dispenser {sev.ToLowerInvariant()} - {evt.Message}";
                }
                if (dc == "IDC" || dc == "CARD_READER") return $"Card reader {sev.ToLowerInvariant()} - {evt.Message}";
                if (dc == "COMM" || dc == "NDC") return $"Switch communication {sev.ToLowerInvariant()} - Host response affected.";
                if (dc == "PTR" || dc == "PRINTER") return $"Printer {sev.ToLowerInvariant()} - Receipt/journal affected.";
                return $"{dc} {sev.ToLowerInvariant()} event correlated with {cls} transaction.";
            }
    
    
            public event EventHandler<ATMRealTimeStatusSnapshot>? OnSnapshotUpdated
            {
                add => _reducer.OnSnapshotUpdated += value;
                remove => _reducer.OnSnapshotUpdated -= value;
            }
    
    
        }
    public sealed class FinalTransactionStatus
        {
            public string TransactionId { get; set; } = string.Empty;
            public string ATM_ID { get; set; } = string.Empty;
            public string JournalClassification { get; set; } = string.Empty;
            public double JournalConfidence { get; set; }
            public string FinalStatus { get; set; } = string.Empty;
            public string RootCause { get; set; } = string.Empty;
            public double Confidence { get; set; }
            public int TotalEvidenceSources { get; set; }
            public List<string> EvidenceSources { get; set; } = new();
        }
    public partial class FinalTransactionStatus
        {
            public string TransactionId { get; set; } = string.Empty;
    
    
            public string ATM_ID { get; set; } = string.Empty;
    
    
            public string JournalClassification { get; set; } = string.Empty;
    
    
            public double JournalConfidence { get; set; }
    
    
            public string FinalStatus { get; set; } = string.Empty;
    
    
            public string RootCause { get; set; } = string.Empty;
    
    
            public double Confidence { get; set; }
    
    
            public int TotalEvidenceSources { get; set; }
    
    
            public List<string> EvidenceSources { get; set; } = new();
    
    
        }

    // Class: CorrelationLink (from 3 sources)
        public sealed partial class CorrelationLink
        {
            // --- Properties ---
                    public string TransactionId { get; set; } = string.Empty;
    
                    public string EventId { get; set; } = string.Empty;
    
                    public CorrelationMatchStrength MatchLevel { get; set; }
    
                    public string MatchBasis { get; set; } = string.Empty;
    
                    public double Confidence { get; set; }
    
                    public string RootCause { get; set; } = string.Empty;
    
                    public DateTime CorrelatedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: EvidenceCorrelationEngine (from 1 sources)
        public sealed partial class EvidenceCorrelationEngine : IEvidenceCorrelationEngine
        {
            // --- Constants & Fields ---
                    private readonly ATMRealTimeStatusReducer _reducer = new();
    
                    private readonly Dictionary<string, List<NormalizedVendorEvent>> _eventBuffer = new();
    
                    private readonly Dictionary<string, List<EjTransaction>> _transactionBuffer = new();
    
                    private readonly Dictionary<string, long> _evidenceCounts = new();
    
                    private readonly object _lock = new();
    
    
            // --- Properties ---
                    public int CorrelationWindowSeconds { get; set; } = 30;
    
                    public IReadOnlyList<string> ActiveAtmIds => _reducer.TrackedAtmIds.ToList().AsReadOnly();
    
    
            // --- Methods ---
                    public ATMRealTimeStatusSnapshot? GetLatestSnapshot(string atmId) => _reducer.GetLatest(atmId);
    
                    public long GetEvidenceItemCount(string atmId) { lock (_lock) { return _evidenceCounts.TryGetValue(atmId, out var c) ? c : 0; } }
    
                    public Task IngestJournalEventAsync(string atmId, object journalEvent, CancellationToken ct)
                    {
                        if (journalEvent is EjTransaction tx)
                        {
                            FeedTransaction(tx);
                            _reducer.SetJournalEvidence(atmId, lastJournalFile: tx.TransactionId, lastJournalLine: tx.EndLine,
                                journalDeltaLines: tx.EndLine - tx.StartLine, lastSyncUtc: DateTime.UtcNow);
                        }
                        IncrementEvidenceCount(atmId);
                        return Task.CompletedTask;
                    }
    
                    public Task IngestNdcEventAsync(string atmId, object ndcEvent, CancellationToken ct)
                    {
                        _reducer.SetNdcComEvidence(atmId, true, ndcState: "Active", lastMessageUtc: DateTime.UtcNow);
                        IncrementEvidenceCount(atmId);
                        return Task.CompletedTask;
                    }
    
                    public Task IngestXfsEventAsync(string atmId, object xfsEvent, CancellationToken ct)
                    {
                        if (xfsEvent is NormalizedVendorEvent evt)
                        {
                            FeedEvent(evt);
                            var deviceStates = new Dictionary<string, string> { { evt.DeviceClass, evt.Severity } };
                            _reducer.SetXfsEvidence(atmId, true, deviceStates: deviceStates);
                        }
                        IncrementEvidenceCount(atmId);
                        return Task.CompletedTask;
                    }
    
                    public Task IngestDeviceLogEventAsync(string atmId, object deviceEvent, CancellationToken ct)
                    {
                        _reducer.SetServiceState(atmId, "Running", watcherState: "Active");
                        IncrementEvidenceCount(atmId);
                        return Task.CompletedTask;
                    }
    
                    public Task IngestVendorErrorEventAsync(string atmId, string errorCode, string severity, CancellationToken ct)
                    {
                        _reducer.AddAlarm(atmId, $"VendorError:{errorCode}:{severity}");
                        IncrementEvidenceCount(atmId);
                        return Task.CompletedTask;
                    }
    
                    public Task<ATMRealTimeStatusSnapshot> RecomputeSnapshotAsync(string atmId, CancellationToken ct)
                    {
                        var snapshot = _reducer.GetOrCreate(atmId);
                        snapshot.ComputeConfidence();
                        snapshot.GenerateRecommendation();
                        return Task.FromResult(snapshot);
                    }
    
                    public void FeedEvent(NormalizedVendorEvent vendorEvent)
                    {
                        lock (_lock)
                        {
                            var key = vendorEvent.ATM_ID;
                            if (!_eventBuffer.ContainsKey(key))
                                _eventBuffer[key] = new List<NormalizedVendorEvent>();
                            _eventBuffer[key].Add(vendorEvent);
                            var cutoff = DateTime.UtcNow.AddSeconds(-CorrelationWindowSeconds * 2);
                            _eventBuffer[key].RemoveAll(e => e.Timestamp < cutoff);
                        }
                    }
    
                    public void FeedTransaction(EjTransaction transaction)
                    {
                        lock (_lock)
                        {
                            var key = transaction.ATM_ID ?? "UNKNOWN";
                            if (!_transactionBuffer.ContainsKey(key))
                                _transactionBuffer[key] = new List<EjTransaction>();
                            _transactionBuffer[key].Add(transaction);
                        }
                    }
    
                    private void IncrementEvidenceCount(string atmId)
                    {
                        lock (_lock) { _evidenceCounts.TryGetValue(atmId, out var c); _evidenceCounts[atmId] = c + 1; }
                    }
    
                    public List<CorrelationLink> Correlate(EjTransaction transaction)
                    {
                        var links = new List<CorrelationLink>();
                        var atmId = transaction.ATM_ID ?? "UNKNOWN";
                        lock (_lock)
                        {
                            if (!_eventBuffer.TryGetValue(atmId, out var events))
                                return links;
                            foreach (var evt in events)
                            {
                                var matchLevel = DetermineMatchLevel(transaction, evt);
                                if (matchLevel != CorrelationMatchStrength.None)
                                {
                                    links.Add(new CorrelationLink
                                    {
                                        TransactionId = transaction.TransactionId ?? string.Empty,
                                        EventId = evt.EventId,
                                        MatchLevel = matchLevel,
                                        MatchBasis = DetermineMatchBasis(transaction, evt),
                                        Confidence = ComputeCorrelationConfidence(transaction, evt, matchLevel),
                                        RootCause = DetermineRootCause(transaction, evt),
                                        CorrelatedUtc = DateTime.UtcNow
                                    });
                                }
                            }
                        }
                        return links;
                    }
    
                    public List<CorrelationLink> CorrelateAllForAtm(string atmId)
                    {
                        var allLinks = new List<CorrelationLink>();
                        lock (_lock)
                        {
                            if (!_transactionBuffer.TryGetValue(atmId, out var transactions))
                                return allLinks;
                            foreach (var tx in transactions)
                                allLinks.AddRange(Correlate(tx));
                        }
                        return allLinks;
                    }
    
                    public FinalTransactionStatus DetermineFinalStatus(EjTransaction transaction, List<CorrelationLink> correlationLinks)
                    {
                        var status = new FinalTransactionStatus
                        {
                            TransactionId = transaction.TransactionId ?? string.Empty,
                            ATM_ID = transaction.ATM_ID ?? string.Empty,
                            JournalClassification = transaction.Classification.ToString(),
                            JournalConfidence = transaction.Confidence
                        };
                        status.TotalEvidenceSources = 1 + correlationLinks.Count;
                        status.EvidenceSources.Add("Journal");
                        foreach (var link in correlationLinks)
                            status.EvidenceSources.Add($"XFS:{link.EventId}");
    
                        var strongLinks = correlationLinks.Where(l => l.MatchLevel == CorrelationMatchStrength.Strong).ToList();
                        var mediumLinks = correlationLinks.Where(l => l.MatchLevel == CorrelationMatchStrength.Medium).ToList();
    
                        if (strongLinks.Any(l => l.RootCause.Contains("jam", StringComparison.OrdinalIgnoreCase)))
                        { status.FinalStatus = "CashJam"; status.Confidence = Math.Max(transaction.Confidence, 0.95); status.RootCause = strongLinks.First(l => l.RootCause.Contains("jam", StringComparison.OrdinalIgnoreCase)).RootCause; }
                        else if (strongLinks.Any(l => l.RootCause.Contains("partial", StringComparison.OrdinalIgnoreCase)))
                        { status.FinalStatus = "PartialDispense"; status.Confidence = Math.Max(transaction.Confidence, 0.92); status.RootCause = strongLinks.First(l => l.RootCause.Contains("partial", StringComparison.OrdinalIgnoreCase)).RootCause; }
                        else if (strongLinks.Any(l => l.RootCause.Contains("Approved", StringComparison.OrdinalIgnoreCase) && l.RootCause.Contains("dispense", StringComparison.OrdinalIgnoreCase)))
                        { status.FinalStatus = "ApprovedNoDispense"; status.Confidence = Math.Max(transaction.Confidence, 0.95); status.RootCause = strongLinks.First(l => l.RootCause.Contains("Approved", StringComparison.OrdinalIgnoreCase)).RootCause; }
                        else if (strongLinks.Any())
                        { var best = strongLinks.OrderByDescending(l => l.Confidence).First(); status.FinalStatus = transaction.Classification.ToString(); status.Confidence = Math.Max(transaction.Confidence, best.Confidence); status.RootCause = best.RootCause; }
                        else if (mediumLinks.Any())
                        { var best = mediumLinks.OrderByDescending(l => l.Confidence).First(); status.FinalStatus = transaction.Classification.ToString(); status.Confidence = Math.Max(transaction.Confidence, best.Confidence); status.RootCause = best.RootCause; }
                        else
                        { status.FinalStatus = transaction.Classification.ToString(); status.RootCause = "Journal-only classification (no device correlation)."; status.Confidence = transaction.Confidence; }
    
                        return status;
                    }
    
                    public void Clear() { lock (_lock) { _eventBuffer.Clear(); _transactionBuffer.Clear(); _evidenceCounts.Clear(); } }
    
                    public void Clear(string atmId) { lock (_lock) { _eventBuffer.Remove(atmId); _transactionBuffer.Remove(atmId); _evidenceCounts.Remove(atmId); } }
    
                    private CorrelationMatchStrength DetermineMatchLevel(EjTransaction tx, NormalizedVendorEvent evt)
                    {
                        var timeDelta = Math.Abs((tx.Timestamp - evt.Timestamp).TotalSeconds);
    
                        // Strong match: explicit ImpactedTransactionId linkage within tight window
                        if (!string.IsNullOrEmpty(tx.TransactionId) && !string.IsNullOrEmpty(evt.ImpactedTransactionId) &&
                            string.Equals(tx.TransactionId, evt.ImpactedTransactionId, StringComparison.OrdinalIgnoreCase) &&
                            timeDelta <= 10)
                            return CorrelationMatchStrength.Strong;
    
                        // Strong match: same ATM + device match + very tight window (<5s)
                        if (timeDelta <= 5 && IsDeviceRelated(tx, evt))
                            return CorrelationMatchStrength.Strong;
    
                        // Medium match: same ATM + time window + device class match
                        if (timeDelta <= CorrelationWindowSeconds && IsDeviceRelated(tx, evt))
                            return CorrelationMatchStrength.Medium;
    
                        // Weak match: proximity in time
                        if (timeDelta <= CorrelationWindowSeconds * 2)
                            return CorrelationMatchStrength.Weak;
    
                        return CorrelationMatchStrength.None;
                    }
    
                    private static bool IsDeviceRelated(EjTransaction tx, NormalizedVendorEvent evt)
                    {
                        var dc = evt.DeviceClass.ToUpperInvariant();
                        var cls = tx.Classification.ToString();
                        if (dc == "CDM" || dc == "CASH_DISPENSER")
                            return cls.Contains("Dispense") || cls.Contains("Cash") || cls.Contains("Success") || cls.Contains("Failed");
                        if (dc == "IDC" || dc == "CARD_READER")
                            return cls.Contains("Card") || !string.IsNullOrEmpty(tx.CardNumber);
                        if (dc == "PIN" || dc == "EPP" || dc == "PTR" || dc == "PRINTER")
                            return true;
                        if (dc == "SIU")
                            return cls.Contains("Fault") || cls.Contains("Hardware");
                        return true;
                    }
    
                    private static string DetermineMatchBasis(EjTransaction tx, NormalizedVendorEvent evt)
                    {
                        if (!string.IsNullOrEmpty(tx.TransactionId) && !string.IsNullOrEmpty(evt.ImpactedTransactionId) &&
                            string.Equals(tx.TransactionId, evt.ImpactedTransactionId, StringComparison.OrdinalIgnoreCase))
                            return $"TxId={tx.TransactionId}";
                        var delta = Math.Abs((tx.Timestamp - evt.Timestamp).TotalSeconds);
                        return $"ATM={tx.ATM_ID}, TimeDelta={delta:N1}s, Device={evt.DeviceClass}";
                    }
    
                    private static double ComputeCorrelationConfidence(EjTransaction tx, NormalizedVendorEvent evt, CorrelationMatchStrength level)
                    {
                        double baseConf = level switch
                        {
                            CorrelationMatchStrength.Strong => 0.95,
                            CorrelationMatchStrength.Medium => 0.75,
                            CorrelationMatchStrength.Weak => 0.50,
                            _ => 0.0
                        };
                        if (!string.IsNullOrEmpty(tx.STAN) && !string.IsNullOrEmpty(evt.Code)) baseConf += 0.03;
                        if (IsDeviceRelated(tx, evt)) baseConf += 0.02;
                        return Math.Min(baseConf, 1.0);
                    }
    
                    private static string DetermineRootCause(EjTransaction tx, NormalizedVendorEvent evt)
                    {
                        var dc = evt.DeviceClass.ToUpperInvariant();
                        var sev = evt.Severity.ToUpperInvariant();
                        var cls = tx.Classification.ToString();
                        if ((dc == "CDM" || dc == "CASH_DISPENSER") && (sev == "ERROR" || sev == "CRITICAL"))
                        {
                            if (cls.Contains("CashJam")) return "Cash dispenser jam detected - dispenser mechanism failure.";
                            if (cls.Contains("PartialDispense")) return "Partial dispense due to cash dispenser error.";
                            if (cls.Contains("ApprovedNoDispense")) return "Approved no dispense - dispenser failed to present notes.";
                            return $"Cash dispenser {sev.ToLowerInvariant()} - {evt.Message}";
                        }
                        if (dc == "IDC" || dc == "CARD_READER") return $"Card reader {sev.ToLowerInvariant()} - {evt.Message}";
                        if (dc == "COMM" || dc == "NDC") return $"Switch communication {sev.ToLowerInvariant()} - Host response affected.";
                        if (dc == "PTR" || dc == "PRINTER") return $"Printer {sev.ToLowerInvariant()} - Receipt/journal affected.";
                        return $"{dc} {sev.ToLowerInvariant()} event correlated with {cls} transaction.";
                    }
    
    
            // --- Events ---
                    public event EventHandler<ATMRealTimeStatusSnapshot>? OnSnapshotUpdated
                    {
                        add => _reducer.OnSnapshotUpdated += value;
                        remove => _reducer.OnSnapshotUpdated -= value;
                    }
    
    
        }
    // Class: FinalTransactionStatus (from 3 sources)
        public sealed partial class FinalTransactionStatus
        {
            // --- Properties ---
                    public string TransactionId { get; set; } = string.Empty;
    
                    public string ATM_ID { get; set; } = string.Empty;
    
                    public string JournalClassification { get; set; } = string.Empty;
    
                    public double JournalConfidence { get; set; }
    
                    public string FinalStatus { get; set; } = string.Empty;
    
                    public string RootCause { get; set; } = string.Empty;
    
                    public double Confidence { get; set; }
    
                    public int TotalEvidenceSources { get; set; }
    
                    public List<string> EvidenceSources { get; set; } = new();
    
    
        }
}
