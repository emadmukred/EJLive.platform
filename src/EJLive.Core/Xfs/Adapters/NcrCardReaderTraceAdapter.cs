using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace EJLive.Core.Xfs.Adapters
{
    public sealed class NcrCardReaderTraceAdapter : IXfsVendorAdapter
    {
        private static readonly Regex TimestampLine = new Regex(
            @"^(?<date>\d{2}-\d{2}-\d{2})\s+(?<time>\d{2}:\d{2}:\d{2})\s+(?<msg>.+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public XfsVendor Vendor => XfsVendor.NCR;

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            return line.IndexOf("Primary card reader", StringComparison.OrdinalIgnoreCase) >= 0
                || line.IndexOf("FIT Search Required", StringComparison.OrdinalIgnoreCase) >= 0
                || line.IndexOf("CARD READ", StringComparison.OrdinalIgnoreCase) >= 0
                || line.IndexOf("Card Found in Primary Reader", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public IReadOnlyList<XfsNormalizedEvent> ParseLines(IEnumerable<string> lines)
        {
            var events = new List<XfsNormalizedEvent>();
            if (lines == null) return events;

            foreach (var raw in lines)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var text = raw.TrimEnd('\0').Trim();
                DateTime? ts = null;
                string msg = text;
                var m = TimestampLine.Match(text);
                if (m.Success)
                {
                    msg = m.Groups["msg"].Value.Trim();
                    if (DateTime.TryParseExact(m.Groups["date"].Value + " " + m.Groups["time"].Value, "dd-MM-yy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                        ts = parsed;
                }

                if (msg.IndexOf("Primary card reader success", StringComparison.OrdinalIgnoreCase) >= 0)
                    events.Add(NewEvent(ts, XfsEventKind.CardReaderFlow, "Card reader path", msg, "CardReader", XfsSeverity.Info, raw));
                else if (msg.IndexOf("Card Entered", StringComparison.OrdinalIgnoreCase) >= 0)
                    events.Add(NewEvent(ts, XfsEventKind.CardReaderFlow, "Card entered", msg, "CardReader", XfsSeverity.Info, raw));
                else if (msg.IndexOf("Card Found in Primary Reader", StringComparison.OrdinalIgnoreCase) >= 0)
                    events.Add(NewEvent(ts, XfsEventKind.CardReaderFlow, "Primary reader selected", msg, "CardReader", XfsSeverity.Info, raw));
                else if (msg.IndexOf("Read Condition", StringComparison.OrdinalIgnoreCase) >= 0)
                    events.Add(NewEvent(ts, XfsEventKind.CardReaderFlow, "Read condition", msg, "CardReader", XfsSeverity.Info, raw));
                else if (msg.IndexOf("FIT Search Required", StringComparison.OrdinalIgnoreCase) >= 0)
                    events.Add(NewEvent(ts, XfsEventKind.CardReaderFitness, "FIT search", msg, "CardReader", XfsSeverity.Info, raw));
                else if (msg.IndexOf("Good Match in Card Read State", StringComparison.OrdinalIgnoreCase) >= 0 || msg.IndexOf("Good FIT Match", StringComparison.OrdinalIgnoreCase) >= 0)
                    events.Add(NewEvent(ts, XfsEventKind.CardReaderFitness, "FIT match", msg, "CardReader", XfsSeverity.Info, raw));
                else if (msg.IndexOf("CARD READ", StringComparison.OrdinalIgnoreCase) >= 0 || msg.IndexOf("Good Read", StringComparison.OrdinalIgnoreCase) >= 0 || msg.IndexOf("Not FIT", StringComparison.OrdinalIgnoreCase) >= 0)
                    events.Add(NewEvent(ts, XfsEventKind.CardReaderStatistics, "Card read counters", msg, "CardReader", XfsSeverity.Info, raw));
                else if (msg.IndexOf("Waiting for Card Entry", StringComparison.OrdinalIgnoreCase) >= 0)
                    events.Add(NewEvent(ts, XfsEventKind.CardReaderFlow, "Waiting for card", msg, "CardReader", XfsSeverity.Info, raw));
            }

            return events;
        }

        private XfsNormalizedEvent NewEvent(DateTime? ts, XfsEventKind kind, string title, string message, string family, XfsSeverity severity, string raw)
        {
            return new XfsNormalizedEvent
            {
                Vendor = XfsVendor.NCR.ToString(),
                SourceLayer = XfsSourceLayer.DriverError,
                Kind = kind,
                Severity = severity,
                Timestamp = ts,
                Title = title,
                Message = message,
                DeviceFamily = family,
                ServiceImpact = "Card-reader diagnostic telemetry updated.",
                CustomerImpact = "May affect card acceptance, fit detection, and reader path behavior.",
                RecommendedAction = "Correlate with card capture, FIT mismatches, and reader error counters.",
                RawLine = raw
            };
        }
    }
}
