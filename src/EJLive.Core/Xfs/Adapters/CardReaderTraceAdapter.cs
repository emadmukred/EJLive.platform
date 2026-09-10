using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EJLive.Core.Xfs.Adapters
{
    public sealed class CardReaderTraceAdapter : IXfsVendorAdapter
    {
        private static readonly Regex TimeRegex = new Regex(@"^(?<ts>\d{2}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2})", RegexOptions.Compiled);

        public XfsVendor Vendor => XfsVendor.NCR;

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            return line.IndexOf("Primary card reader success", StringComparison.OrdinalIgnoreCase) >= 0
                || line.IndexOf("Card Found in Primary Reader", StringComparison.OrdinalIgnoreCase) >= 0
                || line.IndexOf("FIT Search Required", StringComparison.OrdinalIgnoreCase) >= 0
                || line.IndexOf("Good FIT Match", StringComparison.OrdinalIgnoreCase) >= 0
                || line.IndexOf("CARD READ", StringComparison.OrdinalIgnoreCase) >= 0
                || line.IndexOf("Primary Acceptor Fitness", StringComparison.OrdinalIgnoreCase) >= 0
                || line.IndexOf("Secondary CardReader Synchroniser", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public IReadOnlyList<XfsNormalizedEvent> ParseLines(IEnumerable<string> lines)
        {
            var events = new List<XfsNormalizedEvent>();
            foreach (var raw in lines ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var evt = Parse(raw);
                if (evt != null) events.Add(evt);
            }
            return events;
        }

        private XfsNormalizedEvent Parse(string raw)
        {
            var evt = new XfsNormalizedEvent
            {
                Vendor = XfsVendor.NCR.ToString(),
                SourceLayer = XfsSourceLayer.DriverError,
                Kind = XfsEventKind.CardEvent,
                Severity = XfsSeverity.Info,
                DeviceFamily = "CardReader",
                RawLine = raw,
                Title = "Card reader trace",
                Message = raw.Trim(),
                ServiceImpact = "Card-reader diagnostic telemetry updated.",
                CustomerImpact = "May explain card acceptance, FIT match, or reader-path behavior.",
                RecommendedAction = "Correlate with NCR journal and status events when card-read issues are reported."
            };

            if (raw.IndexOf("FIT Search Required", StringComparison.OrdinalIgnoreCase) >= 0)
                evt.Data["trace_stage"] = "FitSearch";
            else if (raw.IndexOf("Good FIT Match", StringComparison.OrdinalIgnoreCase) >= 0)
                evt.Data["trace_stage"] = "FitMatch";
            else if (raw.IndexOf("Card Entered", StringComparison.OrdinalIgnoreCase) >= 0)
                evt.Data["trace_stage"] = "CardEntered";
            else if (raw.IndexOf("Card Found in Primary Reader", StringComparison.OrdinalIgnoreCase) >= 0)
                evt.Data["trace_stage"] = "PrimaryReaderSelected";
            else if (raw.IndexOf("Read Condition", StringComparison.OrdinalIgnoreCase) >= 0)
                evt.Data["trace_stage"] = "ReadCondition";
            else if (raw.IndexOf("Primary Acceptor Fitness", StringComparison.OrdinalIgnoreCase) >= 0)
                evt.Data["trace_stage"] = "PrimaryFitness";
            else if (raw.IndexOf("Secondary Card Acceptor Fitness", StringComparison.OrdinalIgnoreCase) >= 0)
                evt.Data["trace_stage"] = "SecondaryFitness";
            else if (raw.IndexOf("CARD READ", StringComparison.OrdinalIgnoreCase) >= 0)
                evt.Data["trace_stage"] = "Summary";

            var t = TimeRegex.Match(raw.Trim());
            if (t.Success) evt.Data["time_hint"] = t.Groups["ts"].Value;
            return evt;
    }
}
}
