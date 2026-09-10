using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EJLive.Core.Xfs.Adapters
{
    public sealed class DieboldMdsAdapter : IXfsVendorAdapter
    {
        private static readonly Regex MdsRegex = new Regex(
            @"^(?<solicited>\d{3})(?<device>[A-Z\*]{2,4}\d{0,2}|\*\*\*\*)\:(?<h1>[0-9A-F]{2})\:(?<h2>[0-9A-F]{2})\:(?<h3>[0-9A-F]{2})(?<tail>(?:\:[0-9A-F]{2})*)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public XfsVendor Vendor => XfsVendor.Unknown;

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            return MdsRegex.IsMatch(line.Trim())
                   || line.Contains("CNS", StringComparison.OrdinalIgnoreCase)
                   || line.Contains("000MNT", StringComparison.OrdinalIgnoreCase)
                   || line.Contains("000TIA", StringComparison.OrdinalIgnoreCase);
        }

        public IReadOnlyList<XfsNormalizedEvent> ParseLines(IEnumerable<string> lines)
        {
            var events = new List<XfsNormalizedEvent>();
            foreach (var raw in lines ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var trimmed = raw.Trim();

                if (trimmed.StartsWith("000MNT", StringComparison.OrdinalIgnoreCase))
                {
                    events.Add(BuildSimpleEvent(raw, XfsEventKind.Maintenance, XfsSeverity.Info, "Diebold maintenance log", "Maintenance mode activity recorded.", "Maintenance"));
                    continue;
                }

                if (trimmed.StartsWith("000TIA", StringComparison.OrdinalIgnoreCase))
                {
                    events.Add(BuildSimpleEvent(raw, XfsEventKind.Maintenance, XfsSeverity.Info, "Diebold terminal initiated audit", "TIA lifecycle event recorded.", "Audit"));
                    continue;
                }

                if (trimmed.StartsWith("CNS", StringComparison.OrdinalIgnoreCase))
                {
                    events.Add(BuildSimpleEvent(raw, XfsEventKind.CommandReject, XfsSeverity.Warning, "Diebold state-escape event", "State escape event recorded in Agilis terminal flow.", "StateEngine"));
                    continue;
                }

                var m = MdsRegex.Match(trimmed);
                if (!m.Success) continue;

                string device = m.Groups["device"].Value.ToUpperInvariant();
                string handler = m.Groups["h1"].Value.ToUpperInvariant();
                string d1 = m.Groups["h2"].Value.ToUpperInvariant();
                string d2 = m.Groups["h3"].Value.ToUpperInvariant();
                string tail = m.Groups["tail"].Value;
                string rawCode = handler + ":" + d1 + ":" + d2 + tail;

                var evt = new XfsNormalizedEvent
                {
                    Vendor = XfsVendor.Unknown.ToString(),
                    SourceLayer = XfsSourceLayer.XfsStatus,
                    Kind = DetermineKind(device),
                    Severity = DetermineSeverity(handler),
                    DeviceCode = device,
                    DeviceFamily = MapDeviceFamily(device),
                    RawCode = rawCode,
                    Title = "Diebold " + device + " status",
                    Message = BuildMessage(device, handler, d1, d2, tail),
                    ServiceImpact = DetermineServiceImpact(device, handler),
                    CustomerImpact = DetermineCustomerImpact(device),
                    RecommendedAction = RecommendAction(device, handler, d1, d2),
                    RawLine = raw
                };

                evt.Data["solicited_code"] = m.Groups["solicited"].Value;
                evt.Data["handler"] = handler;
                evt.Data["detail_1"] = d1;
                evt.Data["detail_2"] = d2;
                if (!string.IsNullOrWhiteSpace(tail)) evt.Data["tail"] = tail.TrimStart(':');
                evt.Data["handler_meaning"] = MapHandlerMeaning(handler);
                events.Add(evt);
            }
            return events;
        }

        private XfsNormalizedEvent BuildSimpleEvent(string raw, XfsEventKind kind, XfsSeverity severity, string title, string message, string deviceFamily)
        {
            return new XfsNormalizedEvent
            {
                Vendor = XfsVendor.Unknown.ToString(),
                SourceLayer = XfsSourceLayer.XfsStatus,
                Kind = kind,
                Severity = severity,
                DeviceFamily = deviceFamily,
                Title = title,
                Message = message,
                ServiceImpact = "Diebold operational event recorded.",
                CustomerImpact = "Impact depends on terminal workflow context.",
                RecommendedAction = "Correlate with surrounding Diebold MDS and 912 status mappings.",
                RawLine = raw
            };
        }

        private static XfsEventKind DetermineKind(string device)
        {
            switch (device)
            {
                case "CR01": return XfsEventKind.CardEvent;
                case "D901":
                case "DI01": return XfsEventKind.CashDispense;
                case "DR01": return XfsEventKind.Retract;
                case "DP01": return XfsEventKind.CashDeposit;
                case "PR01":
                case "PR02":
                case "PR03":
                case "SP01": return XfsEventKind.PrinterEvent;
                case "EP01": return XfsEventKind.DeviceFault;
                case "AL01":
                case "LC01":
                case "SY01": return XfsEventKind.DeviceStatus;
                case "****": return XfsEventKind.TerminalModeTransition;
                default: return XfsEventKind.DeviceFault;
            }
        }

        private static XfsSeverity DetermineSeverity(string handler)
        {
            switch (handler)
            {
                case "39":
                case "3A":
                case "3E": return XfsSeverity.Critical;
                case "38":
                case "3D":
                case "23":
                case "24": return XfsSeverity.Warning;
                case "3C":
                case "3F": return XfsSeverity.Info;
                default: return XfsSeverity.Info;
            }
        }

        private static string MapDeviceFamily(string device)
        {
            switch (device)
            {
                case "AH01": return "AfterHourDepository";
                case "AL01": return "Alarms";
                case "CI01": return "CurrencyAcceptor";
                case "CN01": return "CoinDispenser";
                case "CR01": return "CardReader";
                case "D901":
                case "DI01": return "BillDispenser";
                case "DP01": return "Depository";
                case "DR01": return "Presenter";
                case "EP01": return "EncryptingPinPad";
                case "LC01": return "LcdFailureDetection";
                case "PR01": return "ReceiptPrinter";
                case "PR02": return "JournalPrinter";
                case "PR03": return "ElectronicJournal";
                case "SD01": return "EnvelopeDispenser";
                case "SP01": return "StatementPrinter";
                case "SY01": return "SystemStability";
                case "****": return "PowerUpSys";
                default: return "UnknownDevice";
            }
        }

        private static string MapHandlerMeaning(string handler)
        {
            switch (handler)
            {
                case "21": return "OperationCompleted";
                case "23": return "Timeout";
                case "24": return "Cancel";
                case "2D": return "AttentionRequired";
                case "30": return "Reject";
                case "31": return "Idle";
                case "38": return "CommunicationsFault";
                case "39": return "FaultBeforeFinish";
                case "3A": return "FaultStartedNotFinished";
                case "3B": return "RecoverableOrRetryFault";
                case "3C": return "WarningDegraded";
                case "3D": return "SupplyOrServiceAction";
                case "3E": return "InstitutionRepairFault";
                case "3F": return "WarningOrDeviceDependentOutcome";
                default: return "Unknown";
            }
        }

        private static string BuildMessage(string device, string handler, string d1, string d2, string tail)
        {
            return string.Format(CultureInfo.InvariantCulture, "Diebold {0} status {1}:{2}:{3}{4}", device, handler, d1, d2, tail ?? string.Empty);
        }

        private static string DetermineServiceImpact(string device, string handler)
        {
            if (device == "CR01") return "Card reader service may be degraded or blocked.";
            if (device == "D901" || device == "DI01") return "Bill dispense path may be unavailable or degraded.";
            if (device == "DR01") return "Presenter/retract path may fail, affecting delivered cash certainty.";
            if (device == "DP01" || device == "CI01") return "Deposit/accept path may be degraded or blocked.";
            if (device.StartsWith("PR", StringComparison.OrdinalIgnoreCase) || device == "SP01") return "Printer or journal/statement recording capability may be degraded.";
            if (device == "EP01") return "PIN encryption/EPP capability may be impaired.";
            if (device == "SY01") return "System stability warning may affect storage/operation continuity.";
            return "Operational impact requires correlation with terminal workflow.";
        }

        private static string DetermineCustomerImpact(string device)
        {
            if (device == "CR01") return "Customer card flow may fail, capture, or jam.";
            if (device == "D901" || device == "DI01") return "Customer may receive no cash, short cash, or delayed dispense handling.";
            if (device == "DR01") return "Presented cash may be forgotten, retracted, or not fully delivered.";
            if (device == "DP01" || device == "CI01") return "Customer deposit/check/media flow may fail or be captured unexpectedly.";
            if (device == "PR01" || device == "SP01") return "Customer receipt/statement may not be delivered.";
            return "Customer impact depends on transaction context.";
        }

        private static string RecommendAction(string device, string handler, string d1, string d2)
        {
            if (device == "CR01") return "Correlate with card-reader state and decide whether retain, clear jam, or remove terminal from service is required.";
            if (device == "D901" || device == "DI01") return "Check cassette mapping, supply state, jam path, and retry semantics before re-enabling dispense.";
            if (device == "DR01") return "Check presenter, exit shutter, retract/dump path, and note certainty before resuming service.";
            if (device == "DP01" || device == "CI01") return "Inspect gate, transport, bins/cassettes, and media access before keeping deposit features in service.";
            if (device == "PR01" || device == "PR02" || device == "PR03" || device == "SP01") return "Check media, top-of-form, jams, communications, and configuration/forms definitions.";
            if (device == "EP01") return "Inspect EPP state, keys, and communications; may require service intervention.";
            if (device == "SY01") return "Check system storage or stability threshold conditions immediately.";
            return "Review device-specific MDS reference and correlate with 912 mappings and workflow context.";
        }
    }
}
