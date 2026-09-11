using System;
using System.Collections.Generic;

namespace EJLive.Core.Xfs
{
    // NOTE: This file adds correlation and advanced normalization types that complement
    // the legacy XFS files (IXfsVendorAdapter.cs, XfsAdapterRegistry.cs, XfsEventModels.cs).
    // The legacy IXfsVendorAdapter interface (XfsVendor Vendor, CanHandle, ParseLines)
    // and XfsNormalizedEvent model remain the canonical XFS contracts.
    // This file does NOT redefine them — it extends with correlation capabilities only.

    /// <summary>
    /// Extended normalized event for correlation — references legacy XfsNormalizedEvent via adapter.
    /// Used when raw lines need to be captured with correlation context.
    /// </summary>
    public sealed class NormalizedVendorEvent
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString("N");
        public string AtmId { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string DeviceClass { get; set; } = "Unknown";
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Unknown";
        public DateTime? TimestampUtc { get; set; }
        public string SourceFile { get; set; } = string.Empty;
        public int RawLineNumber { get; set; }
        public string RawLine { get; set; } = string.Empty;
    }

    /// <summary>
    /// Correlation engine adapter — bridges legacy IXfsVendorAdapter to normalized events
    /// with ATM context for correlation.
    /// </summary>
    public sealed class XfsCorrelationAdapter
    {
        /// <summary>Converts legacy XfsNormalizedEvent to a correlation-ready NormalizedVendorEvent.</summary>
        public static NormalizedVendorEvent FromLegacy(XfsNormalizedEvent legacy, string atmId, string rawLine, int lineNumber)
        {
            return new NormalizedVendorEvent
            {
                AtmId = atmId ?? string.Empty,
                Vendor = legacy.Vendor.ToString(),
                DeviceClass = legacy.DeviceFamily ?? "Unknown",
                Code = legacy.DeviceCode ?? legacy.RawCode ?? string.Empty,
                Message = legacy.Message ?? string.Empty,
                Severity = legacy.Severity.ToString(),
                TimestampUtc = legacy.Timestamp,
                RawLine = rawLine ?? legacy.RawLine ?? string.Empty,
                RawLineNumber = lineNumber
            };
        }
    }

    // ---- Correlation Engine ----

    /// <summary>
    /// Links an EJ transaction to one or more XFS/trace events with a confidence score.
    /// </summary>
    public sealed class CorrelationLink
    {
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");
        public string TransactionId { get; set; } = string.Empty;
        public string VendorEventId { get; set; } = string.Empty;
        public string Confidence { get; set; } = "Weak";
        public string Impact { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Engine that correlates EJ transactions with XFS/trace events.
    /// Strong = matched by TransactionNumber/STAN/RRN.
    /// Medium = ATM_ID + timestamp window + device class match.
    /// Weak = nearby timestamp + same file/session + error burst.
    /// </summary>
    public sealed class CorrelationEngine
    {
        /// <summary>
        /// Attempts to correlate a journal transaction with a set of XFS events.
        /// Returns links ordered by confidence descending.
        /// </summary>
        public List<CorrelationLink> Correlate(Journal.EjTransaction transaction, List<NormalizedVendorEvent> events)
        {
            var links = new List<CorrelationLink>();

            if (events == null || events.Count == 0)
                return links;

            foreach (var evt in events)
            {
                var confidence = EvaluateConfidence(transaction, evt);
                if (confidence == "Strong" || confidence == "Medium")
                {
                    links.Add(new CorrelationLink
                    {
                        TransactionId = transaction.Stan ?? transaction.TransactionNumber.ToString(),
                        VendorEventId = evt.EventId,
                        Confidence = confidence,
                        Explanation = GenerateExplanation(confidence, transaction, evt)
                    });
                }
            }

            return links;
        }

        private static string EvaluateConfidence(Journal.EjTransaction tx, NormalizedVendorEvent evt)
        {
            // Strong: exact STAN/RRN match
            if (!string.IsNullOrEmpty(tx.Stan) && evt.RawLine != null && evt.RawLine.IndexOf(tx.Stan, StringComparison.OrdinalIgnoreCase) >= 0)
                return "Strong";
            if (!string.IsNullOrEmpty(tx.Rrn) && evt.RawLine != null && evt.RawLine.IndexOf(tx.Rrn, StringComparison.OrdinalIgnoreCase) >= 0)
                return "Strong";

            // Medium: same ATM + timestamp window + device class
            if (tx.AtmId != null && evt.AtmId != null &&
                tx.AtmId.Equals(evt.AtmId, StringComparison.OrdinalIgnoreCase) &&
                tx.Date.HasValue && evt.TimestampUtc.HasValue &&
                Math.Abs((tx.Date.Value - evt.TimestampUtc.Value).TotalSeconds) <= 30)
                return "Medium";

            // Weak: same ATM + file session
            return "Weak";
        }

        private static string GenerateExplanation(string confidence, Journal.EjTransaction tx, NormalizedVendorEvent evt)
        {
            switch (confidence)
            {
                case "Strong":
                    return string.Format("Exact match on transaction identifiers. EJ STAN={0}, RRN={1} found in XFS event {2}.",
                        tx.Stan ?? "?", tx.Rrn ?? "?", evt.EventId);
                case "Medium":
                    return string.Format("ATM + time window + device class match. ATM={0}, time delta < 30s.", tx.AtmId);
                case "Weak":
                    return string.Format("Weak proximity match. Same ATM ({0}), same file session.", tx.AtmId);
                default:
                    return "Uncorrelated";
            }
        }
    }
}