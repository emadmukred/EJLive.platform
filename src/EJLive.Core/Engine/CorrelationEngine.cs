using System.Globalization;
using System.Text.RegularExpressions;
using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// Correlates <see cref="NormalizedVendorEvent"/> records with <see cref="EjTransaction"/> records
/// using a tiered matching strategy: Strong, Medium, and Weak.
/// </summary>
public sealed class CorrelationEngine
{
    private readonly TimeSpan _mediumWindow;
    private readonly TimeSpan _weakWindow;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationEngine"/> class.
    /// </summary>
    /// <param name="mediumWindow">
    /// The time window for medium-strength correlation. Defaults to 2 minutes.
    /// </param>
    /// <param name="weakWindow">
    /// The time window for weak-strength correlation. Defaults to 5 minutes.
    /// </param>
    public CorrelationEngine(TimeSpan? mediumWindow = null, TimeSpan? weakWindow = null)
    {
        _mediumWindow = mediumWindow ?? TimeSpan.FromMinutes(2);
        _weakWindow = weakWindow ?? TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Correlates a collection of vendor events against a collection of EJ transactions.
    /// </summary>
    /// <param name="events">The normalized vendor events to correlate.</param>
    /// <param name="transactions">The parsed EJ transactions to match against.</param>
    /// <returns>A list of correlated events with populated correlation metadata.</returns>
    public List<NormalizedVendorEvent> Correlate(
        List<NormalizedVendorEvent> events,
        List<EjTransaction> transactions)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(transactions);

        var results = new List<NormalizedVendorEvent>();
        var txLookup = BuildTransactionLookup(transactions);

        foreach (var evt in events)
        {
            var match = FindBestMatch(evt, transactions, txLookup);
            results.Add(match);
        }

        return results;
    }

    private static Dictionary<string, List<EjTransaction>> BuildTransactionLookup(List<EjTransaction> transactions)
    {
        var lookup = new Dictionary<string, List<EjTransaction>>(StringComparer.OrdinalIgnoreCase);

        foreach (var tx in transactions)
        {
            void Add(string? key)
            {
                if (string.IsNullOrWhiteSpace(key)) return;
                if (!lookup.TryGetValue(key, out var list))
                {
                    list = new List<EjTransaction>();
                    lookup[key] = list;
                }
                list.Add(tx);
            }

            Add(tx.STAN);
            Add(tx.RRN);
            Add(tx.TransactionId);
        }

        return lookup;
    }

    private NormalizedVendorEvent FindBestMatch(
        NormalizedVendorEvent evt,
        List<EjTransaction> transactions,
        Dictionary<string, List<EjTransaction>> txLookup)
    {
        // 1. Strong match: exact identifiers
        var strong = TryStrongMatch(evt, txLookup);
        if (strong != null)
        {
            return evt with
            {
                ImpactedTransactionId = strong.TransactionId,
                CorrelationReason = $"Strong match: exact identifier correlation ({DescribeStrongKey(evt)}).",
                FalsePositiveRisk = 0.05,
                OperatorExplanation = $"High-confidence link to transaction {strong.TransactionId} via exact identifier. Review unlikely to be a false positive."
            };
        }

        // 2. Medium match: ATM_ID + timestamp window + context
        var medium = TryMediumMatch(evt, transactions);
        if (medium != null)
        {
            return evt with
            {
                ImpactedTransactionId = medium.TransactionId,
                CorrelationReason = $"Medium match: ATM_ID={evt.ATM_ID}, timestamp within {_mediumWindow.TotalMinutes}min, device context aligned.",
                FalsePositiveRisk = 0.25,
                OperatorExplanation = $"Probable link to transaction {medium.TransactionId} based on temporal and contextual proximity. Verify with STAN/RRN if available."
            };
        }

        // 3. Weak match: nearby timestamp, same session, error burst
        var weak = TryWeakMatch(evt, transactions);
        if (weak != null)
        {
            return evt with
            {
                ImpactedTransactionId = weak.TransactionId,
                CorrelationReason = $"Weak match: nearby timestamp within {_weakWindow.TotalMinutes}min, same ATM session or device class.",
                FalsePositiveRisk = 0.60,
                OperatorExplanation = $"Possible link to transaction {weak.TransactionId}. Weak evidence—manual review recommended."
            };
        }

        // No match
        return evt with
        {
            ImpactedTransactionId = string.Empty,
            CorrelationReason = "No correlation: insufficient evidence across all match tiers.",
            FalsePositiveRisk = 1.0,
            OperatorExplanation = "Unable to correlate this event to any known transaction. Consider expanding time windows or checking for missing journal data."
        };
    }

    private static EjTransaction? TryStrongMatch(
        NormalizedVendorEvent evt,
        Dictionary<string, List<EjTransaction>> txLookup)
    {
        // Extract candidate identifiers from the event raw line
        var keys = ExtractIdentifiersFromEvent(evt);

        foreach (var key in keys)
        {
            if (txLookup.TryGetValue(key, out var candidates))
            {
                foreach (var tx in candidates)
                {
                    if (string.Equals(tx.ATM_ID, evt.ATM_ID, StringComparison.OrdinalIgnoreCase))
                        return tx;
                }
            }
        }

        return null;
    }

    private static List<string> ExtractIdentifiersFromEvent(NormalizedVendorEvent evt)
    {
        var identifiers = new List<string>();
        string raw = evt.RawLine;

        // STAN
        var stanMatch = Regex.Match(raw, @"STAN\s*[:=]?\s*(\d{1,12})", RegexOptions.IgnoreCase);
        if (stanMatch.Success)
            identifiers.Add(stanMatch.Groups[1].Value.Trim());

        // RRN
        var rrnMatch = Regex.Match(raw, @"RRN\s*[:=]?\s*(\d{1,12})", RegexOptions.IgnoreCase);
        if (rrnMatch.Success)
            identifiers.Add(rrnMatch.Groups[1].Value.Trim());

        // TransferId
        var tidMatch = Regex.Match(raw, @"TRANSFER\s*ID\s*[:=]?\s*([A-Z0-9\-]+)", RegexOptions.IgnoreCase);
        if (tidMatch.Success)
            identifiers.Add(tidMatch.Groups[1].Value.Trim());

        // TransactionNumber
        var txnMatch = Regex.Match(raw, @"TRANSACTION\s*(?:NUMBER|NUM|NO)?\s*[:=]?\s*(\d+)", RegexOptions.IgnoreCase);
        if (txnMatch.Success)
            identifiers.Add(txnMatch.Groups[1].Value.Trim());

        return identifiers;
    }

    private EjTransaction? TryMediumMatch(NormalizedVendorEvent evt, List<EjTransaction> transactions)
    {
        EjTransaction? best = null;
        double bestScore = 0;

        foreach (var tx in transactions)
        {
            if (!string.Equals(tx.ATM_ID, evt.ATM_ID, StringComparison.OrdinalIgnoreCase))
                continue;

            double score = 0;

            // Timestamp proximity
            var diff = Math.Abs((evt.Timestamp - tx.Timestamp).TotalSeconds);
            if (diff >= _mediumWindow.TotalSeconds)
                continue;

            score += 0.4;

            // Device class alignment with cassette activity
            if (evt.DeviceClass.Equals("CashDispenser", StringComparison.OrdinalIgnoreCase)
                && (tx.Cassette1.HasValue || tx.Cassette2.HasValue || tx.Cassette3.HasValue || tx.Cassette4.HasValue))
            {
                score += 0.3;
            }

            // Host message direction (implied by transaction having STAN/RRN)
            if (!string.IsNullOrEmpty(tx.STAN) && !string.IsNullOrEmpty(tx.RRN))
                score += 0.2;

            // Session or file coherence
            if (!string.IsNullOrEmpty(evt.SourceFile) && evt.SourceFile.Contains(tx.ATM_ID, StringComparison.OrdinalIgnoreCase))
                score += 0.1;

            if (score > bestScore && score >= 0.6)
            {
                bestScore = score;
                best = tx;
            }
        }

        return best;
    }

    private EjTransaction? TryWeakMatch(NormalizedVendorEvent evt, List<EjTransaction> transactions)
    {
        EjTransaction? best = null;
        double bestScore = 0;

        foreach (var tx in transactions)
        {
            if (!string.Equals(tx.ATM_ID, evt.ATM_ID, StringComparison.OrdinalIgnoreCase))
                continue;

            double score = 0;

            // Nearby timestamp
            var diff = Math.Abs((evt.Timestamp - tx.Timestamp).TotalSeconds);
            if (diff <= _weakWindow.TotalSeconds)
                score += 0.4;

            // Same file session (rough heuristic)
            if (!string.IsNullOrEmpty(evt.SourceFile))
                score += 0.2;

            // Error burst: event severity is Error/Fatal and transaction is not success
            if ((evt.Severity.Equals("ERROR", StringComparison.OrdinalIgnoreCase)
                 || evt.Severity.Equals("FATAL", StringComparison.OrdinalIgnoreCase))
                && tx.Classification != TransactionClassification.Success)
            {
                score += 0.2;
            }

            // Same device class within operational window
            if (evt.DeviceClass.Equals("CashDispenser", StringComparison.OrdinalIgnoreCase)
                && tx.Amount.HasValue)
            {
                score += 0.2;
            }

            if (score > bestScore && score >= 0.5)
            {
                bestScore = score;
                best = tx;
            }
        }

        return best;
    }

    private static string DescribeStrongKey(NormalizedVendorEvent evt)
    {
        var ids = ExtractIdentifiersFromEvent(evt);
        return ids.Count > 0 ? string.Join(", ", ids.Take(3)) : "unknown";
    }
}
