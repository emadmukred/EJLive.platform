using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EJLive.Core.Xfs.Adapters
{
    public sealed class GrgXfsAdapter : IXfsVendorAdapter
    {
        private static readonly Regex TimestampedLine = new Regex(@"^(?<ts>\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}\.\d{3})\s+(?<body>.+)$", RegexOptions.Compiled);
        private static readonly Regex CassetteLine = new Regex(@"CAS\((?<id>[^\)]+)\):(?<remain>\d+)\/(?<reject>\d+)\/(?<deno>\s*\d+)\/(?<currency>[^\/]+)\/(?<state>[^\/]+)\/(?<type>[^\/]+)\/(?<noteType>[^\r\n]+)", RegexOptions.Compiled);

        public XfsVendor Vendor => XfsVendor.GRG;

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            return line.IndexOf("TRANSACTION", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   line.IndexOf("CASSETTE STATUS", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   line.IndexOf("CASH UNIT CHANGED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   line.IndexOf("ENTER INSERVICE MODE", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   line.IndexOf("ENTER OFFLINE MODE", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   CassetteLine.IsMatch(line);
        }

        public IReadOnlyList<XfsNormalizedEvent> ParseLines(IEnumerable<string> lines)
        {
            var results = new List<XfsNormalizedEvent>();
            foreach (var line in lines ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                DateTime? timestamp = null;
                string body = line;
                var timeMatch = TimestampedLine.Match(line);
                if (timeMatch.Success)
                {
                    body = timeMatch.Groups["body"].Value.Trim();
                    if (DateTime.TryParse(timeMatch.Groups["ts"].Value, out var parsed))
                        timestamp = parsed;
                }

                var cassetteMatch = CassetteLine.Match(body);
                if (cassetteMatch.Success)
                {
                    var evt = new XfsNormalizedEvent
                    {
                        Vendor = "GRG",
                        SourceLayer = XfsSourceLayer.BusinessJournal,
                        DeviceFamily = "CashDispenser",
                        EventCode = "GRG_CASSETTE_SNAPSHOT",
                        Title = "GRG cassette snapshot",
                        Message = "Cassette counters or state were recorded in the journal.",
                        RecommendedAction = "Use this snapshot to update normalized cassette state and reconcile OUT/IN/REJ/REM flows.",
                        RawLine = line,
                        Timestamp = timestamp
                    };

                    evt.Cassettes.Add(new XfsCassetteSnapshot
                    {
                        CassetteId = cassetteMatch.Groups["id"].Value,
                        RemainingCount = ParseInt(cassetteMatch.Groups["remain"].Value),
                        RejectCount = ParseInt(cassetteMatch.Groups["reject"].Value),
                        Denomination = ParseInt(cassetteMatch.Groups["deno"].Value),
                        Currency = cassetteMatch.Groups["currency"].Value.Trim(),
                        CassetteState = cassetteMatch.Groups["state"].Value.Trim(),
                        CassetteType = cassetteMatch.Groups["type"].Value.Trim(),
                        NoteType = cassetteMatch.Groups["noteType"].Value.Trim()
                    });

                    results.Add(evt);
                    continue;
                }

                results.Add(BuildEvent(body, line, timestamp));
            }
            return results;
        }

        private static XfsNormalizedEvent BuildEvent(string body, string rawLine, DateTime? timestamp)
        {
            string upper = body.ToUpperInvariant();
            var evt = new XfsNormalizedEvent
            {
                Vendor = "GRG",
                SourceLayer = XfsSourceLayer.BusinessJournal,
                DeviceFamily = DetectDeviceFamily(upper).ToString(),
                EventCode = DetectCode(upper),
                EventName = body,
                Severity = DetectSeverity(upper),
                OperationalImpact = DetectImpact(upper),
                RecommendedAction = DetectAction(upper),
                RawLine = rawLine,
                Timestamp = timestamp
            };

            if (upper.Contains("TRANSACTION SERIAL NUMBER:"))
            {
                int idx = upper.IndexOf("TRANSACTION SERIAL NUMBER:", StringComparison.Ordinal);
                evt.TransactionSerialNumber = body.Substring(idx + "TRANSACTION SERIAL NUMBER:".Length).Trim();
            }

            return evt;
        }

        private static string DetectDeviceFamily(string upper)
        {
            if (upper.Contains("CARD READER")) return "CardReader";
            if (upper.Contains("RECEIPT PRINTER")) return "ReceiptPrinter";
            if (upper.Contains("CASH DISPENSER") || upper.Contains("DISPENSE") || upper.Contains("CASSETTE")) return "CashDispenser";
            if (upper.Contains("CASH DEPOSIT") || upper.Contains("COUNT NOTES") || upper.Contains("CIM RETRACT")) return "CashAcceptor";
            if (upper.Contains("LINE DOWN") || upper.Contains("LINE UP") || upper.Contains("INSERVICE") || upper.Contains("OFFLINE") || upper.Contains("OUTOFSERVICE") || upper.Contains("MAINTENANCE")) return "Terminal";
            return "Unknown";
        }

        private static string DetectCode(string upper)
        {
            if (upper.Contains("ENTER INSERVICE MODE")) return "ENTER_INSERVICE_MODE";
            if (upper.Contains("ENTER OFFLINE MODE")) return "ENTER_OFFLINE_MODE";
            if (upper.Contains("ENTER OUTOFSERVICE MODE")) return "ENTER_OUTOFSERVICE_MODE";
            if (upper.Contains("ENTER MAINTENANCE MODE")) return "ENTER_MAINTENANCE_MODE";
            if (upper.Contains("TRANSACTION REQUEST")) return "TRANSACTION_REQUEST";
            if (upper.Contains("TRANSACTION REPLY NEXT")) return "TRANSACTION_REPLY_NEXT";
            if (upper.Contains("DISPENSE SUCCESS")) return "DISPENSE_SUCCESS";
            if (upper.Contains("DISPENSE FAIL")) return "DISPENSE_FAIL";
            if (upper.Contains("TAKE CASH TIMEOUT")) return "TAKE_CASH_TIMEOUT";
            if (upper.Contains("CARD CAPTURED: TAKE CARD TIMEOUT")) return "TAKE_CARD_TIMEOUT_CAPTURE";
            if (upper.Contains("CASH DEPOSIT RETRACT")) return "CASH_DEPOSIT_RETRACT";
            if (upper.Contains("CIM RETRACT")) return "CIM_RETRACT";
            if (upper.Contains("CDM RETRACT")) return "CDM_RETRACT";
            return "GRG_EVENT";
        }

        private static XfsSeverity DetectSeverity(string upper)
        {
            if (upper.Contains("ERROR") || upper.Contains("FAIL") || upper.Contains("TIMEOUT") || upper.Contains("LINE DOWN")) return XfsSeverity.Critical;
            if (upper.Contains("OFFLINE") || upper.Contains("OUTOFSERVICE") || upper.Contains("CHANGED")) return XfsSeverity.Warning;
            return XfsSeverity.Info;
        }

        private static string DetectImpact(string upper)
        {
            if (upper.Contains("TAKE CASH TIMEOUT")) return "Customer did not collect dispensed cash; retract flow is expected.";
            if (upper.Contains("CARD CAPTURED: TAKE CARD TIMEOUT")) return "Customer card capture occurred after timeout.";
            if (upper.Contains("DISPENSE FAIL")) return "Cash-out failed and may require reconciliation of reject and remaining cassette counts.";
            if (upper.Contains("CASH DEPOSIT RETRACT") || upper.Contains("CIM RETRACT")) return "Deposited notes were retracted due to timeout, rollback, or module failure.";
            if (upper.Contains("ENTER OFFLINE MODE") || upper.Contains("LINE DOWN")) return "Terminal lost host connectivity and entered offline service state.";
            return "GRG journal event affects transaction or terminal operational flow.";
        }

        private static string DetectAction(string upper)
        {
            if (upper.Contains("DISPENSE FAIL")) return "Correlate DISPENSE FAIL with cassette counters, reject totals, and subsequent reset/retract events.";
            if (upper.Contains("TAKE CASH TIMEOUT")) return "Record timeout, mark customer not-taken cash incident, and expect retract evidence afterward.";
            if (upper.Contains("CARD CAPTURED")) return "Log captured-card incident and flag for operator/engineer follow-up.";
            if (upper.Contains("LINE DOWN") || upper.Contains("ENTER OFFLINE MODE")) return "Track host connectivity loss and suppress normal service assumptions until INSERVICE returns.";
            if (upper.Contains("COUNT NOTES")) return "Update deposit counting state, OCR evidence, and provisional cash-in totals.";
            return "Map this GRG journal line into the normalized transaction/device timeline.";
        }

        private static int ParseInt(string value)
        {
            int parsed;
            return int.TryParse((value ?? string.Empty).Trim(), out parsed) ? parsed : 0;
        }
    }
}
