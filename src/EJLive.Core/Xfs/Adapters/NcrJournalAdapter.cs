using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EJLive.Core.Xfs.Adapters
{
    public sealed class NcrJournalAdapter : IXfsVendorAdapter
    {
        private static readonly Regex TimeLineRegex = new Regex(@"^(?<time>\d{2}:\d{2}:\d{2})\s+(?<body>.+)$", RegexOptions.Compiled);
        private static readonly Regex ReceiptStatusRegex = new Regex(@"\*(?<serial>\d+)\*(?<msgType>[12])\*(?<device>[A-Z])\*(?<status>\d+),M-(?<module>\d+),R-(?<supply>[0-9A-Z]+)", RegexOptions.Compiled);
        private static readonly Regex NotesPresentedRegex = new Regex(@"NOTES PRESENTED\s+(?<c1>\d+),(?<c2>\d+),(?<c3>\d+),(?<c4>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public XfsVendor Vendor => XfsVendor.NCR;

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            return line.Contains("TRANSACTION START")
                || line.Contains("CARD INSERTED")
                || line.Contains("NOTES PRESENTED")
                || line.Contains("NOTES TAKEN")
                || line.Contains("CARD TAKEN")
                || line.Contains("GENAC")
                || line.Contains("ATR RECEIVED")
                || line.Contains("EJ LOG COPIED OK");
        }

        public IReadOnlyList<XfsNormalizedEvent> ParseLines(IEnumerable<string> lines)
        {
            var results = new List<XfsNormalizedEvent>();
            if (lines == null) return results;

            foreach (var raw in lines)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var clean = StripEscapeNoise(raw).Trim();
                if (clean.Length == 0) continue;

                XfsNormalizedEvent? evt = null;
                if (clean.Contains("TRANSACTION START", StringComparison.OrdinalIgnoreCase) || clean.Contains("TRANSACTION END", StringComparison.OrdinalIgnoreCase))
                    evt = Build(clean, XfsEventKind.TransactionLifecycle, XfsSeverity.Info, "NCR transaction lifecycle", "Transaction");
                else if (clean.Contains("CARD INSERTED", StringComparison.OrdinalIgnoreCase) || clean.Contains("CARD TAKEN", StringComparison.OrdinalIgnoreCase))
                    evt = Build(clean, XfsEventKind.CardEvent, XfsSeverity.Info, "NCR card event", "CardReader");
                else if (clean.Contains("PIN ENTERED", StringComparison.OrdinalIgnoreCase) || clean.Contains("GENAC", StringComparison.OrdinalIgnoreCase) || clean.Contains("ATR RECEIVED", StringComparison.OrdinalIgnoreCase))
                    evt = Build(clean, XfsEventKind.TransactionLifecycle, XfsSeverity.Info, "NCR EMV step", "EMV");
                else if (clean.Contains("NOTES STACKED", StringComparison.OrdinalIgnoreCase) || clean.Contains("NOTES PRESENTED", StringComparison.OrdinalIgnoreCase) || clean.Contains("NOTES TAKEN", StringComparison.OrdinalIgnoreCase))
                    evt = Build(clean, XfsEventKind.CashDispense, XfsSeverity.Info, "NCR cash dispense flow", "CashOut");
                else if (clean.Contains("EJ LOG COPIED OK", StringComparison.OrdinalIgnoreCase) || clean.Contains("EJ BACKUP MODE", StringComparison.OrdinalIgnoreCase))
                    evt = Build(clean, XfsEventKind.Maintenance, XfsSeverity.Info, "NCR journal copy/archive event", "Journal");
                else if (clean.Contains("DIAGNOSTIC DISPENSE REPORT", StringComparison.OrdinalIgnoreCase))
                    evt = Build(clean, XfsEventKind.DeviceStatus, XfsSeverity.Info, "NCR diagnostic report", "Dispenser");

                if (evt != null)
                {
                    var m = NotesPresentedRegex.Match(clean);
                    if (m.Success)
                    {
                        evt.Data["presented_cassette1"] = m.Groups["c1"].Value;
                        evt.Data["presented_cassette2"] = m.Groups["c2"].Value;
                        evt.Data["presented_cassette3"] = m.Groups["c3"].Value;
                        evt.Data["presented_cassette4"] = m.Groups["c4"].Value;
                    }
                    results.Add(evt);
                }

                var r = ReceiptStatusRegex.Match(clean);
                if (r.Success)
                {
                    var statusEvt = new XfsNormalizedEvent
                    {
                        Vendor = XfsVendor.NCR.ToString(),
                        SourceLayer = XfsSourceLayer.BusinessJournal,
                        Kind = XfsEventKind.PrinterEvent,
                        Severity = XfsSeverity.Warning,
                        DeviceCode = r.Groups["device"].Value,
                        DeviceFamily = "ReceiptOrJournalPrint",
                        RawCode = r.Value,
                        Title = "NCR receipt/journal status line",
                        Message = clean,
                        RawLine = raw
                    };
                    statusEvt.Data["transaction_serial"] = r.Groups["serial"].Value;
                    statusEvt.Data["status"] = r.Groups["status"].Value;
                    statusEvt.Data["module_status"] = r.Groups["module"].Value;
                    statusEvt.Data["supply_status"] = r.Groups["supply"].Value;
                    results.Add(statusEvt);
                }
            }

            return results;
        }

        private static XfsNormalizedEvent Build(string line, XfsEventKind kind, XfsSeverity severity, string title, string deviceFamily)
        {
            var evt = new XfsNormalizedEvent
            {
                Vendor = XfsVendor.NCR.ToString(),
                SourceLayer = XfsSourceLayer.BusinessJournal,
                Kind = kind,
                Severity = severity,
                Title = title,
                Message = line,
                DeviceFamily = deviceFamily,
                RawLine = line,
                Timestamp = ExtractTime(line)
            };
            return evt;
        }

        private static DateTime? ExtractTime(string line)
        {
            var m = TimeLineRegex.Match(line);
            if (!m.Success) return null;
            if (DateTime.TryParseExact(m.Groups["time"].Value, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ts))
                return ts;
            return null;
        }

        private static string StripEscapeNoise(string input)
        {
            var chars = new List<char>(input.Length);
            foreach (char c in input)
            {
                if (c == '\u001b' || c == '\u000f' || c == '\u0000')
                    continue;
                chars.Add(c);
            }
            return new string(chars.ToArray());
        }
    }
}
