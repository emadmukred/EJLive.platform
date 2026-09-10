using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace EJLive.Core.Xfs.Adapters
{
    public sealed class GrgJournalAdapter : IXfsVendorAdapter
    {
        private static readonly Regex TimestampedLineRegex = new Regex(
            @"^(?<ts>\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}\.\d{3})\s+(?<body>.+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex CassetteRegex = new Regex(
            @"^CAS\((?<id>[^\)]+)\):(?<remain>\d+)\/(?<reject>\d+)\/\s*(?<deno>\d+)\/(?<currency>[A-Z]*)\/(?<state>[A-Z]+)\/(?<ctype>[A-Z]+)\/(?<ntype>TYPE\([A-Z]?\)|TYPE\(\))$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public XfsVendor Vendor => XfsVendor.GRG;

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            return line.Contains("ENTER INSERVICE MODE", StringComparison.OrdinalIgnoreCase)
                || line.Contains("CASH DEPOSIT START", StringComparison.OrdinalIgnoreCase)
                || line.Contains("DISPENSE COMMAND FROM HOST", StringComparison.OrdinalIgnoreCase)
                || line.Contains("CASSETTE STATUS:", StringComparison.OrdinalIgnoreCase)
                || CassetteRegex.IsMatch(line.Trim());
        }

        public IReadOnlyList<XfsNormalizedEvent> ParseLines(IEnumerable<string> lines)
        {
            var events = new List<XfsNormalizedEvent>();
            if (lines == null)
                return events;

            foreach (var raw in lines)
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                var evt = ParseSingleLine(raw);
                if (evt != null)
                    events.Add(evt);
            }

            return events;
        }

        private static XfsNormalizedEvent? ParseSingleLine(string raw)
        {
            string body = raw.Trim();
            DateTime timestamp;
            var tsMatch = TimestampedLineRegex.Match(body);
            if (tsMatch.Success)
            {
                DateTime.TryParseExact(tsMatch.Groups["ts"].Value, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp);
                body = tsMatch.Groups["body"].Value.Trim();
            }
            else
            {
                timestamp = DateTime.MinValue;
            }

            var cassette = CassetteRegex.Match(body);
            if (cassette.Success)
            {
                var evt = NewBaseEvent(raw, timestamp, XfsEventKind.CassetteSnapshot, XfsSeverity.Info, "Cassette snapshot", body);
                evt.Data["cassette_id"] = cassette.Groups["id"].Value;
                evt.Data["remaining_count"] = cassette.Groups["remain"].Value;
                evt.Data["reject_count"] = cassette.Groups["reject"].Value;
                evt.Data["denomination"] = cassette.Groups["deno"].Value;
                evt.Data["currency"] = cassette.Groups["currency"].Value;
                evt.Data["cassette_state"] = cassette.Groups["state"].Value;
                evt.Data["cassette_type"] = cassette.Groups["ctype"].Value;
                evt.Data["note_type"] = cassette.Groups["ntype"].Value;
                evt.DeviceFamily = "Cassette";
                evt.ServiceImpact = "Updates live cassette inventory snapshot.";
                evt.CustomerImpact = "No direct customer impact unless correlated with a transaction or low/out state.";
                evt.RecommendedAction = "Use for inventory, low-cash, reject, and cassette-state monitoring.";
                return evt;
            }

            if (body.StartsWith("ENTER ", StringComparison.OrdinalIgnoreCase) && body.EndsWith("MODE", StringComparison.OrdinalIgnoreCase))
            {
                return NewBaseEvent(raw, timestamp, XfsEventKind.TerminalModeTransition, XfsSeverity.Info, "Terminal mode transition", body, "TerminalState");
            }

            if (body.Equals("LINE UP", StringComparison.OrdinalIgnoreCase) || body.Equals("LINE DOWN", StringComparison.OrdinalIgnoreCase) || body.Contains("EXIT OFFLINE MODE", StringComparison.OrdinalIgnoreCase))
            {
                var severity = body.Equals("LINE DOWN", StringComparison.OrdinalIgnoreCase) ? XfsSeverity.Warning : XfsSeverity.Info;
                return NewBaseEvent(raw, timestamp, XfsEventKind.NetworkState, severity, "Network state", body, "Network");
            }

            if (body.Contains("TRANSACTION START", StringComparison.OrdinalIgnoreCase) || body.Contains("TRANSACTION END", StringComparison.OrdinalIgnoreCase))
            {
                return NewBaseEvent(raw, timestamp, XfsEventKind.TransactionLifecycle, XfsSeverity.Info, "Transaction lifecycle", body, "Transaction");
            }

            if (body.StartsWith("TRANSACTION REQUEST", StringComparison.OrdinalIgnoreCase))
            {
                var evt = NewBaseEvent(raw, timestamp, XfsEventKind.TransactionRequest, XfsSeverity.Info, "Transaction request", body, "Transaction");
                evt.Data["operation_code"] = body.Replace("TRANSACTION REQUEST", string.Empty).Trim();
                return evt;
            }

            if (body.StartsWith("TRANSACTION REPLY NEXT", StringComparison.OrdinalIgnoreCase))
            {
                var evt = NewBaseEvent(raw, timestamp, XfsEventKind.HostReply, XfsSeverity.Info, "Host reply next state", body, "HostFlow");
                evt.Data["next_state"] = body.Replace("TRANSACTION REPLY NEXT", string.Empty).Trim();
                return evt;
            }

            if (body.StartsWith("TRANSACTION SERIAL NUMBER:", StringComparison.OrdinalIgnoreCase))
            {
                var evt = NewBaseEvent(raw, timestamp, XfsEventKind.TransactionSerial, XfsSeverity.Info, "Transaction serial", body, "Transaction");
                evt.Data["transaction_serial"] = body.Replace("TRANSACTION SERIAL NUMBER:", string.Empty).Trim();
                return evt;
            }

            if (body.StartsWith("CASH DEPOSIT START", StringComparison.OrdinalIgnoreCase) || body.StartsWith("ENCASH SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                return NewBaseEvent(raw, timestamp, XfsEventKind.CashDeposit, XfsSeverity.Info, "Cash deposit flow", body, "CashIn");
            }

            if (body.StartsWith("START COUNT NOTES", StringComparison.OrdinalIgnoreCase) || body.StartsWith("COUNT NOTES", StringComparison.OrdinalIgnoreCase) || body.StartsWith("CASH TOTAL", StringComparison.OrdinalIgnoreCase))
            {
                return NewBaseEvent(raw, timestamp, XfsEventKind.CashDeposit, XfsSeverity.Info, "Cash counting", body, "CashIn");
            }

            if (body.StartsWith("DISPENSE COMMAND FROM HOST", StringComparison.OrdinalIgnoreCase) || body.StartsWith("DISPENSE COMMAND TO CASSETTE", StringComparison.OrdinalIgnoreCase) || body.StartsWith("DISPENSE SUCCESS", StringComparison.OrdinalIgnoreCase) || body.StartsWith("DISPENSE FAIL", StringComparison.OrdinalIgnoreCase) || body.StartsWith("PRESENT SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                var severity = body.StartsWith("DISPENSE FAIL", StringComparison.OrdinalIgnoreCase) ? XfsSeverity.Warning : XfsSeverity.Info;
                return NewBaseEvent(raw, timestamp, XfsEventKind.CashDispense, severity, "Cash dispense flow", body, "CashOut");
            }

            if (body.Contains("MONEY TAKEN", StringComparison.OrdinalIgnoreCase) || body.Contains("TAKE CASH TIMEOUT", StringComparison.OrdinalIgnoreCase) || body.Contains("TAKE DOCUMENTS TIME OUT", StringComparison.OrdinalIgnoreCase) || body.Contains("TAKE CARD TIMEOUT", StringComparison.OrdinalIgnoreCase))
            {
                var severity = body.Contains("TIMEOUT", StringComparison.OrdinalIgnoreCase) ? XfsSeverity.Warning : XfsSeverity.Info;
                return NewBaseEvent(raw, timestamp, XfsEventKind.Timeout, severity, "Customer timeout/action", body, "CustomerFlow");
            }

            if (body.Contains("RETRACT", StringComparison.OrdinalIgnoreCase))
            {
                return NewBaseEvent(raw, timestamp, XfsEventKind.Retract, XfsSeverity.Warning, "Retract flow", body, "CashRetract");
            }

            if (body.StartsWith("NOTE SERIAL NO:", StringComparison.OrdinalIgnoreCase))
            {
                return NewBaseEvent(raw, timestamp, XfsEventKind.OcrCapture, XfsSeverity.Info, "OCR / note serial capture", body, "OCR");
            }

            if (body.StartsWith("CARD INSERTED", StringComparison.OrdinalIgnoreCase) || body.StartsWith("CARD TAKEN", StringComparison.OrdinalIgnoreCase) || body.StartsWith("CARD CAPTURED", StringComparison.OrdinalIgnoreCase) || body.Contains("CARD READER MODULE STATUS ERROR", StringComparison.OrdinalIgnoreCase) || body.StartsWith("CARD READER |", StringComparison.OrdinalIgnoreCase))
            {
                var severity = body.Contains("ERROR", StringComparison.OrdinalIgnoreCase) || body.Contains("CAPTURED", StringComparison.OrdinalIgnoreCase) ? XfsSeverity.Warning : XfsSeverity.Info;
                return NewBaseEvent(raw, timestamp, XfsEventKind.CardEvent, severity, "Card event", body, "CardReader");
            }

            if (body.Contains("RECEIPT PRINTER", StringComparison.OrdinalIgnoreCase))
            {
                var severity = body.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ? XfsSeverity.Warning : XfsSeverity.Info;
                return NewBaseEvent(raw, timestamp, XfsEventKind.PrinterEvent, severity, "Receipt printer event", body, "ReceiptPrinter");
            }

            if (body.StartsWith("CASH UNIT CHANGED.", StringComparison.OrdinalIgnoreCase))
            {
                return NewBaseEvent(raw, timestamp, XfsEventKind.CassetteChange, XfsSeverity.Info, "Cassette state changed", body, "Cassette");
            }

            if (body.Contains("MODULE STATUS ERROR", StringComparison.OrdinalIgnoreCase) || body.Contains("WFS_ERR_HARDWARE_ERROR", StringComparison.OrdinalIgnoreCase) || body.StartsWith("BNA DEV |", StringComparison.OrdinalIgnoreCase) || body.StartsWith("CASH DISPENSER |", StringComparison.OrdinalIgnoreCase))
            {
                return NewBaseEvent(raw, timestamp, XfsEventKind.DeviceFault, XfsSeverity.Warning, "Device fault", body, "DeviceModule");
            }

            return null;
        }

        private static XfsNormalizedEvent NewBaseEvent(string raw, DateTime timestamp, XfsEventKind kind, XfsSeverity severity, string title, string message, string? deviceFamily = null)
        {
            return new XfsNormalizedEvent
            {
                Vendor = XfsVendor.GRG.ToString(),
                SourceLayer = XfsSourceLayer.BusinessJournal,
                Kind = kind,
                Severity = severity,
                Timestamp = timestamp == DateTime.MinValue ? (DateTime?)null : timestamp,
                Title = title,
                Message = message,
                DeviceFamily = deviceFamily ?? string.Empty,
                RawLine = raw,
                ServiceImpact = BuildServiceImpact(kind, message),
                CustomerImpact = BuildCustomerImpact(kind, message),
                RecommendedAction = BuildRecommendedAction(kind, message)
            };
        }

        private static string BuildServiceImpact(XfsEventKind kind, string message)
        {
            switch (kind)
            {
                case XfsEventKind.NetworkState: return "Connectivity with host or upstream line changed.";
                case XfsEventKind.CashDispense: return "Cash-out workflow state updated; may affect availability.";
                case XfsEventKind.CashDeposit: return "Cash-in workflow state updated; may affect deposit availability.";
                case XfsEventKind.DeviceFault: return "A device module reported an error that may require reset or maintenance.";
                case XfsEventKind.Timeout: return "Customer interaction timed out, possibly leading to retract/capture.";
                default: return "Business journal state updated.";
            }
        }

        private static string BuildCustomerImpact(XfsEventKind kind, string message)
        {
            if (kind == XfsEventKind.Timeout) return "Customer may lose access to card, notes, or deposit media until retract/capture completes.";
            if (kind == XfsEventKind.DeviceFault) return "Customer transaction may fail or be delayed.";
            if (kind == XfsEventKind.CashDispense && message.Contains("FAIL", StringComparison.OrdinalIgnoreCase)) return "Dispense may fail or short-dispense with rejects/retracts.";
            if (kind == XfsEventKind.CardEvent && message.Contains("CAPTURED", StringComparison.OrdinalIgnoreCase)) return "Card may be captured due to timeout or device condition.";
            return "Customer impact depends on surrounding transaction context.";
        }

        private static string BuildRecommendedAction(XfsEventKind kind, string message)
        {
            if (kind == XfsEventKind.CassetteSnapshot || kind == XfsEventKind.CassetteChange) return "Feed cassette state into live inventory and low-cash monitoring.";
            if (kind == XfsEventKind.NetworkState) return "Correlate with offline/out-of-service transitions and reconnect attempts.";
            if (kind == XfsEventKind.DeviceFault) return "Map vendor error tokens and associated device family into engineer actions.";
            if (kind == XfsEventKind.Timeout) return "Correlate with retract OCR lines and abnormal transaction summaries.";
            return "Correlate this event with neighboring journal lines for full transaction context.";
        }
    }
}
