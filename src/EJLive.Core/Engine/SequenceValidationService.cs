using System;
using System.Collections.Generic;
using System.Linq;
using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Validates transaction sequences — detects missing serial numbers,
    /// duplicate serials, and abnormal gaps within specified ranges.
    /// Uses EjTransaction record properties (TransactionId, Classification, etc.)
    /// </summary>
    public sealed class SequenceValidationService
    {
        public SequenceValidationResult Validate(
            IReadOnlyCollection<EjTransaction> transactions,
            int expectedStep = 1,
            int maxReportedAnomalies = 10_000)
        {
            ArgumentNullException.ThrowIfNull(transactions);
            if (expectedStep <= 0)
                throw new ArgumentOutOfRangeException(nameof(expectedStep), "Expected sequence step must be positive.");
            if (maxReportedAnomalies <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxReportedAnomalies), "Anomaly limit must be positive.");

            var result = new SequenceValidationResult();
            if (transactions.Count == 0)
                return result;

            var serials = transactions
                .Select(t => int.TryParse(t.TransactionId, out var serial) ? serial : (int?)null)
                .Where(serial => serial.HasValue)
                .Select(serial => serial!.Value)
                .OrderBy(serial => serial)
                .ToList();

            result.TotalTransactions = transactions.Count;
            result.NumericSequenceCount = serials.Count;
            if (serials.Count == 0)
                return result;

            foreach (var duplicate in serials.GroupBy(serial => serial).Where(group => group.Count() > 1))
            {
                result.DuplicateSequences.Add(new SequenceAnomaly
                {
                    SerialNumber = duplicate.Key,
                    Type = AnomalyType.DuplicateSequence,
                    Detail = $"Sequence #{duplicate.Key} occurs {duplicate.Count()} times."
                });
            }

            var unique = serials.Distinct().ToArray();
            var previous = unique[0];

            for (var index = 1; index < unique.Length; index++)
            {
                var current = unique[index];
                var gap = (long)current - previous;
                if (gap > expectedStep)
                {
                    var missingInGap = (gap - 1) / expectedStep;
                    result.MissingSequenceCount += missingInGap;
                    for (var missing = (long)previous + expectedStep;
                         missing < current && result.MissingSequences.Count < maxReportedAnomalies;
                         missing += expectedStep)
                    {
                        result.MissingSequences.Add(new SequenceAnomaly
                        {
                            SerialNumber = checked((int)missing),
                            Type = AnomalyType.MissingSequence,
                            Detail = $"Missing #{missing} between #{previous} and #{current}."
                        });
                    }
                }
                previous = current;
            }

            result.MissingSequencesTruncated = result.MissingSequenceCount > result.MissingSequences.Count;

            result.AbbreviatedSerials = unique.Where(serial => serial > 0 && serial < 10000).ToList();
            return result;
        }

        public TransactionRiskProfile ClassifyRisk(EjTransaction tx)
        {
            ArgumentNullException.ThrowIfNull(tx);
            var profile = new TransactionRiskProfile { TransactionId = tx.TransactionId };
            var c = tx.Classification;

            if (c == TransactionClassification.Reversal) { profile.IsReversal = true; profile.RiskFlags.Add("Reversal"); }
            if (c == TransactionClassification.PartialDispense) { profile.IsPartialDispense = true; profile.RiskFlags.Add("PartialDispense"); }
            if (c == TransactionClassification.CashJam) { profile.IsCashJam = true; profile.RiskFlags.Add("CashJam"); }
            if (c == TransactionClassification.Retract) { profile.IsCashRetract = true; profile.RiskFlags.Add("CashRetract"); }
            if (c == TransactionClassification.ApprovedNoDispense) { profile.IsApprovedNoDispense = true; profile.RiskFlags.Add("ApprovedNoDispense"); }
            if (c == TransactionClassification.HostDeclined) { profile.IsHostDeclined = true; profile.RiskFlags.Add("HostDeclined"); }
            if (c == TransactionClassification.HardwareFault) { profile.IsHardwareFault = true; profile.RiskFlags.Add("HardwareFault"); }
            if (c == TransactionClassification.Suspicious) { profile.RiskFlags.Add("Suspicious"); }
            if (c == TransactionClassification.Failed) { profile.RiskFlags.Add("Failed"); }
            if (tx.Confidence < 0.4) profile.RiskFlags.Add($"LowConfidence({tx.Confidence:P0})");

            return profile;
        }
    }

    public sealed class SequenceValidationResult
    {
        public List<SequenceAnomaly> MissingSequences { get; set; } = new();
        public List<SequenceAnomaly> DuplicateSequences { get; set; } = new();
        public List<int> AbbreviatedSerials { get; set; } = new();
        public int TotalTransactions { get; set; }
        public int NumericSequenceCount { get; set; }
        public long MissingSequenceCount { get; set; }
        public bool MissingSequencesTruncated { get; set; }
        public bool HasAnomalies => MissingSequenceCount > 0 || DuplicateSequences.Count > 0;
    }

    public sealed class SequenceAnomaly
    {
        public int SerialNumber { get; set; }
        public AnomalyType Type { get; set; }
        public string Detail { get; set; } = string.Empty;
    }

    public enum AnomalyType { MissingSequence, DuplicateSequence }

    public sealed class TransactionRiskProfile
    {
        public string TransactionId { get; set; } = string.Empty;
        public bool IsReversal { get; set; }
        public bool IsPartialDispense { get; set; }
        public bool IsCashJam { get; set; }
        public bool IsCashRetract { get; set; }
        public bool IsApprovedNoDispense { get; set; }
        public bool IsHostDeclined { get; set; }
        public bool IsHardwareFault { get; set; }
        public List<string> RiskFlags { get; set; } = new();
        public bool HasRisk => RiskFlags.Count > 0;
    }
}
