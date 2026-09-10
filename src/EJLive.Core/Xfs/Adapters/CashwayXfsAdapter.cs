using System;
using System.Collections.Generic;

namespace EJLive.Core.Xfs.Adapters
{
    /// <summary>
    /// Cashway XFS/vendor log adapter that normalizes common operational markers.
    /// </summary>
    public sealed class CashwayXfsAdapter : IXfsVendorAdapter
    {
        public XfsVendor Vendor => XfsVendor.CashWay;

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            return line.IndexOf("CASHWAY", StringComparison.OrdinalIgnoreCase) >= 0
                   || line.IndexOf("CW-XFS", StringComparison.OrdinalIgnoreCase) >= 0
                   || line.IndexOf("CWATM", StringComparison.OrdinalIgnoreCase) >= 0
                   || line.IndexOf("CW DISPENSE", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public IReadOnlyList<XfsNormalizedEvent> ParseLines(IEnumerable<string> lines)
        {
            var events = new List<XfsNormalizedEvent>();
            foreach (var raw in lines ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                if (!CanHandle(raw))
                    continue;

                var upper = raw.ToUpperInvariant();
                var evt = new XfsNormalizedEvent
                {
                    Vendor = Vendor.ToString(),
                    SourceLayer = XfsSourceLayer.BusinessJournal,
                    DeviceFamily = DetectDeviceFamily(upper),
                    Kind = DetectKind(upper),
                    Severity = DetectSeverity(upper),
                    EventCode = "CASHWAY_EVENT",
                    Title = "Cashway event",
                    Message = raw.Trim(),
                    ServiceImpact = "Cashway terminal state update detected.",
                    CustomerImpact = "Customer impact depends on transaction state and terminal mode.",
                    RecommendedAction = "Correlate with EJ transaction window and command/audit timeline.",
                    RawLine = raw
                };

                events.Add(evt);
            }

            return events;
        }

        private static string DetectDeviceFamily(string upper)
        {
            if (upper.Contains("DISPENSE", StringComparison.Ordinal) || upper.Contains("CASSETTE", StringComparison.Ordinal))
                return "CashDispenser";
            if (upper.Contains("CARD", StringComparison.Ordinal) || upper.Contains("IDC", StringComparison.Ordinal))
                return "CardReader";
            if (upper.Contains("PRINTER", StringComparison.Ordinal) || upper.Contains("PTR", StringComparison.Ordinal))
                return "Printer";
            if (upper.Contains("HOST", StringComparison.Ordinal) || upper.Contains("NETWORK", StringComparison.Ordinal) || upper.Contains("LINE ", StringComparison.Ordinal))
                return "Connectivity";
            return "Terminal";
        }

        private static XfsEventKind DetectKind(string upper)
        {
            if (upper.Contains("DISPENSE", StringComparison.Ordinal))
                return XfsEventKind.CashDispense;
            if (upper.Contains("CARD", StringComparison.Ordinal))
                return XfsEventKind.CardEvent;
            if (upper.Contains("HOST", StringComparison.Ordinal) || upper.Contains("NETWORK", StringComparison.Ordinal) || upper.Contains("LINE ", StringComparison.Ordinal))
                return XfsEventKind.NetworkState;
            if (upper.Contains("MODE", StringComparison.Ordinal) || upper.Contains("INSERVICE", StringComparison.Ordinal) || upper.Contains("OFFLINE", StringComparison.Ordinal))
                return XfsEventKind.TerminalModeTransition;
            return XfsEventKind.DeviceStatus;
        }

        private static XfsSeverity DetectSeverity(string upper)
        {
            if (upper.Contains("CRITICAL", StringComparison.Ordinal)
                || upper.Contains("FATAL", StringComparison.Ordinal)
                || upper.Contains("FAULT", StringComparison.Ordinal)
                || upper.Contains("JAM", StringComparison.Ordinal)
                || upper.Contains("DOWN", StringComparison.Ordinal))
                return XfsSeverity.Critical;

            if (upper.Contains("WARNING", StringComparison.Ordinal)
                || upper.Contains("TIMEOUT", StringComparison.Ordinal)
                || upper.Contains("OFFLINE", StringComparison.Ordinal)
                || upper.Contains("RETRACT", StringComparison.Ordinal))
                return XfsSeverity.Warning;

            return XfsSeverity.Info;
        }
    }
}
