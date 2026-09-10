using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace EJLive.Core.Xfs.Adapters
{
    public sealed class NcrXfsEventAdapter : IXfsVendorAdapter
    {
        private static readonly Regex StatusRegex = new Regex(@"\*(?<txn>\d{1,4}|\s{4})\*(?<msgType>[12])\*(?<device>[A-Z])\*(?<status>[^,\r\n]+)(?:,\s*M-(?<module>[^,\r\n]*))?(?:,\s*R-(?<supply>[0-9A-Z]+))?", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex CommandRejectRegex = new Regex(@"\*(?<txn>\d{1,4})\*2\*(?<category>[123ABCDE])(?<detail>[0-9A-F]{2})", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        private static readonly Regex TimePrefixRegex = new Regex(@"(?<time>\d{2}:\d{2}:\d{2})", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex NotesPresentedRegex = new Regex(@"NOTES\s+PRESENTED\s+(?<c1>\d+)\,(?<c2>\d+)\,(?<c3>\d+)\,(?<c4>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex DiagnosticDispenseRegex = new Regex(@"DIAGNOSTIC\s+DISPENSE\s+REPORT", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SensorStateRegex = new Regex(@"^(?<first>[123])(?<rest>[01]{1,12})$", RegexOptions.Compiled);

        public XfsVendor Vendor => XfsVendor.NCR;

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            var trimmed = line.Trim();
            return StatusRegex.IsMatch(trimmed)
                   || CommandRejectRegex.IsMatch(trimmed)
                   || trimmed.IndexOf("NOTES PRESENTED", StringComparison.OrdinalIgnoreCase) >= 0
                   || trimmed.IndexOf("NOTES TAKEN", StringComparison.OrdinalIgnoreCase) >= 0
                   || trimmed.IndexOf("CARD TAKEN", StringComparison.OrdinalIgnoreCase) >= 0
                   || trimmed.IndexOf("TRANSACTION START", StringComparison.OrdinalIgnoreCase) >= 0
                   || trimmed.IndexOf("TRANSACTION END", StringComparison.OrdinalIgnoreCase) >= 0
                   || trimmed.IndexOf("PIN ENTERED", StringComparison.OrdinalIgnoreCase) >= 0
                   || DiagnosticDispenseRegex.IsMatch(trimmed)
                   || trimmed.IndexOf("ATR RECEIVED", StringComparison.OrdinalIgnoreCase) >= 0
                   || trimmed.IndexOf("GENAC", StringComparison.OrdinalIgnoreCase) >= 0
                   || trimmed.IndexOf("POWER-UP/RESET", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public IReadOnlyList<XfsNormalizedEvent> ParseLines(IEnumerable<string> lines)
        {
            var results = new List<XfsNormalizedEvent>();
            if (lines == null)
                return results;

            foreach (var raw in lines)
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                var trimmed = raw.Trim();

                var rejectMatch = CommandRejectRegex.Match(trimmed);
                if (rejectMatch.Success)
                {
                    results.Add(ParseCommandReject(raw, rejectMatch));
                    continue;
                }

                var statusMatch = StatusRegex.Match(trimmed);
                if (statusMatch.Success)
                {
                    results.Add(ParseStatusMessage(raw, statusMatch));
                    continue;
                }

                if (trimmed.IndexOf("TRANSACTION START", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    results.Add(BuildFlowEvent(raw, XfsEventKind.TransactionLifecycle, XfsSeverity.Info, "NCR transaction start", "Transaction lifecycle entered start state.", "Transaction"));
                    continue;
                }

                if (trimmed.IndexOf("TRANSACTION END", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    results.Add(BuildFlowEvent(raw, XfsEventKind.TransactionLifecycle, XfsSeverity.Info, "NCR transaction end", "Transaction lifecycle reached end state.", "Transaction"));
                    continue;
                }

                if (trimmed.IndexOf("PIN ENTERED", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    results.Add(BuildFlowEvent(raw, XfsEventKind.TransactionLifecycle, XfsSeverity.Info, "PIN entered", "Customer PIN entry recorded.", "CardholderAuth"));
                    continue;
                }

                if (trimmed.IndexOf("CARD TAKEN", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    results.Add(BuildFlowEvent(raw, XfsEventKind.CardEvent, XfsSeverity.Info, "Card taken", "Customer took the card from the exit path.", "CardReader"));
                    continue;
                }

                if (trimmed.IndexOf("NOTES TAKEN", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    results.Add(BuildFlowEvent(raw, XfsEventKind.CashDispense, XfsSeverity.Info, "Notes taken", "Customer took the dispensed notes.", "CurrencyDispenser"));
                    continue;
                }

                var presented = NotesPresentedRegex.Match(trimmed);
                if (presented.Success)
                {
                    var evt = BuildFlowEvent(raw, XfsEventKind.CashDispense, XfsSeverity.Info, "Notes presented", "Notes were presented to the customer.", "CurrencyDispenser");
                    evt.Data["cassette1_notes"] = presented.Groups["c1"].Value;
                    evt.Data["cassette2_notes"] = presented.Groups["c2"].Value;
                    evt.Data["cassette3_notes"] = presented.Groups["c3"].Value;
                    evt.Data["cassette4_notes"] = presented.Groups["c4"].Value;
                    results.Add(evt);
                    continue;
                }

                if (DiagnosticDispenseRegex.IsMatch(trimmed))
                {
                    results.Add(BuildFlowEvent(raw, XfsEventKind.CashDispense, XfsSeverity.Info, "Diagnostic dispense report", "Diagnostic dispense reporting section detected.", "CurrencyDispenser"));
                    continue;
                }

                if (trimmed.IndexOf("ATR RECEIVED", StringComparison.OrdinalIgnoreCase) >= 0 || trimmed.IndexOf("GENAC", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    results.Add(BuildFlowEvent(raw, XfsEventKind.CardEvent, XfsSeverity.Info, "Card EMV flow", "Card/EMV state transition recorded in NCR journal.", "CardKernel"));
                    continue;
                }

                if (trimmed.IndexOf("POWER-UP/RESET", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    results.Add(BuildFlowEvent(raw, XfsEventKind.TerminalModeTransition, XfsSeverity.Info, "Power-up / reset", "Terminal power-up or reset marker recorded.", "System"));
                }
            }

            return results;
        }

        private XfsNormalizedEvent ParseStatusMessage(string raw, Match match)
        {
            string deviceCode = match.Groups["device"].Value;
            string moduleStatus = match.Groups["module"].Success ? match.Groups["module"].Value.Trim() : string.Empty;
            string supply = match.Groups["supply"].Success ? match.Groups["supply"].Value : string.Empty;
            string transactionStatus = match.Groups["status"].Value.Trim();
            string messageType = match.Groups["msgType"].Value == "1" ? "Unsolicited" : "Solicited";
            string txn = match.Groups["txn"].Value.Trim();

            var evt = new XfsNormalizedEvent
            {
                Vendor = XfsVendor.NCR.ToString(),
                SourceLayer = XfsSourceLayer.XfsStatus,
                Kind = DetermineKind(deviceCode),
                Severity = DetermineSeverity(deviceCode, moduleStatus, transactionStatus),
                DeviceCode = deviceCode,
                DeviceFamily = MapDeviceFamily(deviceCode),
                RawCode = string.Format(CultureInfo.InvariantCulture, "{0}|M-{1}|R-{2}", transactionStatus, moduleStatus, supply).TrimEnd('|'),
                Title = BuildTitle(deviceCode, transactionStatus, moduleStatus),
                Message = BuildMessage(deviceCode, messageType, transactionStatus, moduleStatus, supply),
                ServiceImpact = DetermineServiceImpact(deviceCode, transactionStatus, moduleStatus),
                CustomerImpact = DetermineCustomerImpact(deviceCode, transactionStatus),
                RecommendedAction = RecommendAction(deviceCode, moduleStatus, supply, transactionStatus),
                RawLine = raw
            };

            if (!string.IsNullOrWhiteSpace(txn)) evt.Data["transaction_serial"] = txn;
            evt.Data["message_status_type"] = messageType;
            evt.Data["transaction_device_status"] = transactionStatus;
            if (!string.IsNullOrWhiteSpace(moduleStatus)) evt.Data["module_status"] = moduleStatus;
            if (!string.IsNullOrWhiteSpace(supply)) evt.Data["supply_status"] = supply;

            var time = TimePrefixRegex.Match(raw);
            if (time.Success) evt.Data["time_hint"] = time.Groups["time"].Value;

            string? handler = ExtractHandler(transactionStatus);
            if (!string.IsNullOrWhiteSpace(handler))
            {
                evt.Data["status_handler"] = handler;
                evt.Data["status_handler_meaning"] = MapStatusHandler(handler);
            }

            if (deviceCode == "E" && !string.IsNullOrWhiteSpace(supply))
                ApplyNcrDispenserSupplyInterpretation(evt, transactionStatus, supply);
            else if ((deviceCode == "G" || deviceCode == "H" || deviceCode == "V") && !string.IsNullOrWhiteSpace(supply))
                ApplyNcrPrinterSupplyInterpretation(evt, supply);
            else if (deviceCode == "D" && !string.IsNullOrWhiteSpace(supply))
                evt.Data["capture_bin_state"] = supply == "4" ? "Overfilled" : supply == "1" ? "NotOverfilled" : "NoNewState";
            else if (deviceCode == "P")
                ApplySensorVectorInterpretation(evt, transactionStatus);

            return evt;
        }

        private XfsNormalizedEvent ParseCommandReject(string raw, Match match)
        {
            string category = match.Groups["category"].Value.ToUpperInvariant();
            string detail = match.Groups["detail"].Value.ToUpperInvariant();
            var evt = new XfsNormalizedEvent
            {
                Vendor = XfsVendor.NCR.ToString(),
                SourceLayer = XfsSourceLayer.XfsStatus,
                Kind = XfsEventKind.CommandReject,
                Severity = XfsSeverity.Warning,
                DeviceFamily = "HostCommand",
                Title = "NCR command reject",
                Message = BuildCommandRejectMessage(category, detail),
                CustomerImpact = "The requested host/network command likely did not complete successfully.",
                ServiceImpact = "Command sequencing, message format, security, or device support issue detected.",
                RecommendedAction = RecommendCommandRejectAction(category, detail),
                RawLine = raw,
                RawCode = category + detail
            };
            evt.Data["transaction_serial"] = match.Groups["txn"].Value;
            evt.Data["reject_category"] = category;
            evt.Data["reject_detail"] = detail;
            return evt;
        }

        private static void ApplyNcrDispenserSupplyInterpretation(XfsNormalizedEvent evt, string transactionStatus, string supply)
        {
            if (supply.Length >= 5)
            {
                evt.Data["divert_bin_state"] = supply[0] == '4' ? "Overfilled" : "OK";
                evt.Data["cassette1_supply"] = MapCassetteSupplyDigit(supply[1]);
                evt.Data["cassette2_supply"] = MapCassetteSupplyDigit(supply[2]);
                evt.Data["cassette3_supply"] = MapCassetteSupplyDigit(supply[3]);
                evt.Data["cassette4_supply"] = MapCassetteSupplyDigit(supply[4]);
            }

            if (!string.IsNullOrWhiteSpace(transactionStatus) && transactionStatus.Length >= 9)
            {
                evt.Data["dispense_outcome"] = MapDispenseOutcome(transactionStatus[0]);
                evt.Data["cassette1_dispensed_notes"] = SafeSlice(transactionStatus, 1, 2);
                evt.Data["cassette2_dispensed_notes"] = SafeSlice(transactionStatus, 3, 2);
                evt.Data["cassette3_dispensed_notes"] = SafeSlice(transactionStatus, 5, 2);
                evt.Data["cassette4_dispensed_notes"] = SafeSlice(transactionStatus, 7, 2);
            }
        }

        private static void ApplyNcrPrinterSupplyInterpretation(XfsNormalizedEvent evt, string supply)
        {
            if (evt.DeviceCode == "G" && supply.Length >= 4)
            {
                evt.Data["paper_status"] = MapPaperSupplyDigit(supply[0]);
                evt.Data["ribbon_status"] = supply[1] == '2' ? "Replace" : "OK";
                evt.Data["printhead_status"] = supply[2] == '2' ? "Replace" : "OK";
                evt.Data["knife_status"] = supply[3] == '2' ? "Replace" : "OK";
            }
            else if (evt.DeviceCode == "H" && supply.Length >= 3)
            {
                evt.Data["paper_status"] = MapPaperSupplyDigit(supply[0]);
                evt.Data["ribbon_status"] = supply[1] == '2' ? "Replace" : "OK";
                evt.Data["printhead_status"] = supply[2] == '2' ? "Replace" : "OK";
            }
            else if (evt.DeviceCode == "V" && supply.Length >= 5)
            {
                evt.Data["paper_status"] = MapPaperSupplyDigit(supply[0]);
                evt.Data["ribbon_status"] = MapTriStateSupplyDigit(supply[1]);
                evt.Data["printhead_status"] = MapTriStateSupplyDigit(supply[2]);
                evt.Data["knife_status"] = MapTriStateSupplyDigit(supply[3]);
                evt.Data["capture_bin_status"] = supply[4] == '4' ? "Overfilled" : "OK";
            }
        }

        private static void ApplySensorVectorInterpretation(XfsNormalizedEvent evt, string transactionStatus)
        {
            var match = SensorStateRegex.Match(transactionStatus);
            if (!match.Success)
                return;

            evt.Data["sensor_vector_type"] = match.Groups["first"].Value == "1" ? "TamperChange" : match.Groups["first"].Value == "2" ? "ModeSwitchChange" : "AlarmStateChange";
            string rest = match.Groups["rest"].Value;
            if (match.Groups["first"].Value == "2" && rest.Length >= 1)
                evt.Data["mode_switch_state"] = rest[0] == '1' ? "Supervisor" : "Normal";
            if (rest.Length >= 8)
            {
                evt.Data["door_sensor"] = rest.Length > 2 ? rest[2].ToString() : string.Empty;
                evt.Data["silent_alarm_sensor"] = rest.Length > 4 ? rest[4].ToString() : string.Empty;
                evt.Data["card_capture_bin_state"] = rest.Length > 7 ? (rest[7] == '1' ? "In" : "Out") : string.Empty;
            }
        }

        private XfsNormalizedEvent BuildFlowEvent(string raw, XfsEventKind kind, XfsSeverity severity, string title, string message, string deviceFamily)
        {
            var evt = new XfsNormalizedEvent
            {
                Vendor = XfsVendor.NCR.ToString(),
                SourceLayer = XfsSourceLayer.BusinessJournal,
                Kind = kind,
                Severity = severity,
                DeviceFamily = deviceFamily,
                Title = title,
                Message = message,
                ServiceImpact = "NCR business journal state updated.",
                CustomerImpact = "Customer impact depends on surrounding transaction context.",
                RecommendedAction = "Correlate with adjacent NCR journal and status lines.",
                RawLine = raw
            };
            var time = TimePrefixRegex.Match(raw);
            if (time.Success) evt.Data["time_hint"] = time.Groups["time"].Value;
            return evt;
        }

        private static string? ExtractHandler(string transactionStatus)
        {
            if (string.IsNullOrWhiteSpace(transactionStatus)) return null;
            return transactionStatus.Length >= 2 ? transactionStatus.Substring(0, 2).ToUpperInvariant() : null;
        }

        private static string MapStatusHandler(string handler)
        {
            switch (handler)
            {
                case "21": return "OperationCompleted";
                case "23": return "Timeout";
                case "24": return "Cancel";
                case "2D": return "AttentionRequired";
                case "30": return "Reject";
                case "31": return "Idle";
                case "38": return "Communications";
                case "39": return "FaultBeforeCompletion";
                case "3A": return "FaultAfterStart";
                case "3B": return "FaultRetryContext";
                case "3C": return "WarningDegraded";
                case "3D": return "FaultSupplyAction";
                case "3E": return "InstitutionRepairFault";
                case "3F": return "WarningDeviceDependent";
                default: return "Unknown";
            }
        }

        private static string SafeSlice(string value, int start, int len)
        {
            if (string.IsNullOrEmpty(value) || value.Length < start + len) return string.Empty;
            return value.Substring(start, len);
        }

        private static string MapDispenseOutcome(char code)
        {
            switch (code)
            {
                case '0': return "SuccessWithException";
                case '1': return "ShortDispense";
                case '2': return "NoBillsDispensed";
                case '3': return "FaultyDispenseUnknownQuantity";
                case '4': return "NoDispenseOrCardNotEjected";
                case '5': return "RetractAfterNoTake";
                default: return "Unknown";
            }
        }

        private static string MapCassetteSupplyDigit(char code)
        {
            switch (code)
            {
                case '0': return "NoNewState";
                case '1': return "Sufficient";
                case '2': return "Low";
                case '3': return "Out";
                default: return "Unknown";
            }
        }

        private static string MapPaperSupplyDigit(char code)
        {
            switch (code)
            {
                case '1': return "Sufficient";
                case '2': return "Low";
                case '3': return "Out";
                default: return "Unknown";
            }
        }

        private static string MapTriStateSupplyDigit(char code)
        {
            switch (code)
            {
                case '1': return "OK";
                case '2': return "ReplaceSoon";
                case '3': return "ReplaceNow";
                case '4': return "Overfilled";
                default: return "Unknown";
            }
        }

        private static XfsEventKind DetermineKind(string deviceCode)
        {
            switch (deviceCode)
            {
                case "D": return XfsEventKind.CardEvent;
                case "E": return XfsEventKind.CashDispense;
                case "F": return XfsEventKind.CashDeposit;
                case "G":
                case "H":
                case "V": return XfsEventKind.PrinterEvent;
                case "P": return XfsEventKind.DeviceStatus;
                case "L": return XfsEventKind.DeviceFault;
                case "R": return XfsEventKind.Maintenance;
                case "A":
                case "B": return XfsEventKind.TerminalModeTransition;
                default: return XfsEventKind.DeviceFault;
            }
        }

        private static XfsSeverity DetermineSeverity(string deviceCode, string moduleStatus, string transactionStatus)
        {
            string? handler = ExtractHandler(transactionStatus);
            if (handler == "3E" || handler == "39" || moduleStatus == "146") return XfsSeverity.Critical;
            if (handler == "3A" || handler == "3D" || handler == "38" || handler == "23") return XfsSeverity.Warning;
            if (moduleStatus == "00") return XfsSeverity.Info;
            if (deviceCode == "E" || deviceCode == "D" || deviceCode == "L") return XfsSeverity.Warning;
            return XfsSeverity.Info;
        }

        private static string MapDeviceFamily(string deviceCode)
        {
            switch (deviceCode)
            {
                case "A": return "Clock";
                case "B": return "Power";
                case "D": return "CardReaderWriter";
                case "E": return "CurrencyDispenser";
                case "F": return "Depository";
                case "G": return "ReceiptPrinter";
                case "H": return "JournalPrinter";
                case "L": return "Encryptor";
                case "P": return "Sensors";
                case "R": return "Supervisor";
                case "V": return "StatementPrinter";
                default: return "UnknownDevice";
            }
        }

        private static string BuildTitle(string deviceCode, string status, string moduleStatus)
        {
            return string.Format(CultureInfo.InvariantCulture, "NCR {0} status {1}{2}", MapDeviceFamily(deviceCode), status, string.IsNullOrWhiteSpace(moduleStatus) ? string.Empty : " / M-" + moduleStatus);
        }

        private static string BuildMessage(string deviceCode, string messageType, string status, string moduleStatus, string supply)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} status from {1}: device-status={2}{3}{4}", messageType, MapDeviceFamily(deviceCode), status, string.IsNullOrWhiteSpace(moduleStatus) ? string.Empty : ", module-status=" + moduleStatus, string.IsNullOrWhiteSpace(supply) ? string.Empty : ", supply-status=" + supply);
        }

        private static string DetermineServiceImpact(string deviceCode, string status, string moduleStatus)
        {
            if (deviceCode == "E") return "Cash dispensing may be degraded or unavailable.";
            if (deviceCode == "D") return "Card intake/eject/capture flow may fail.";
            if (deviceCode == "G" || deviceCode == "H" || deviceCode == "V") return "Printing capability may be degraded.";
            if (deviceCode == "F") return "Deposit/envelope handling may be degraded.";
            if (deviceCode == "L") return "PIN encryption or secure key handling may fail.";
            if (deviceCode == "P") return "Sensor, tamper, or mode switch state changed.";
            if (moduleStatus == "00") return "No immediate service degradation detected.";
            return "Operational degradation requires review.";
        }

        private static string DetermineCustomerImpact(string deviceCode, string status)
        {
            if (deviceCode == "E") return "Customer may receive short dispense, no dispense, or retract behavior.";
            if (deviceCode == "D") return "Customer card may jam, fail to eject, or be captured.";
            if (deviceCode == "G") return "Customer receipt may fail to print.";
            if (deviceCode == "F") return "Deposit flow may fail or leave media inaccessible.";
            if (deviceCode == "P") return "Possible tamper, alarm, or supervisor-mode change affecting availability.";
            return "Customer impact depends on active transaction context.";
        }

        private static string RecommendAction(string deviceCode, string moduleStatus, string supply, string status)
        {
            string? handler = ExtractHandler(status);
            if (moduleStatus == "146") return "Check XFS/SP communication path or SDC link integrity first.";
            if (deviceCode == "P") return "Correlate sensor vector with plain-English journal lines and supervisor/tamper flow.";
            if (deviceCode == "E" && handler == "3D") return "Check cassette supply, divert/reject path, and replenishment state.";
            if (deviceCode == "E") return "Inspect dispenser module, cassette state, reject/divert path, and transport sensors.";
            if (deviceCode == "D") return "Inspect reader throat, shutter, card path, capture bin, and card-reader health counters.";
            if (deviceCode == "G" || deviceCode == "H" || deviceCode == "V") return "Check media levels, printer configuration, top-of-form state, knife/head status, and communications.";
            if (deviceCode == "F") return "Check deposit transport, gate, bin state, and whether media is accessible to the customer.";
            if (deviceCode == "L") return "Verify EPP/encryptor key state, communications, and security module health.";
            if (deviceCode == "R") return "Correlate with supervisor mode or operator intervention flow.";
            return "Review adjacent journal, debug, and XFS runtime lines for this event.";
        }

        private static string BuildCommandRejectMessage(string category, string detail)
        {
            return string.Format(CultureInfo.InvariantCulture, "NCR command reject {0}{1}: {2}", category, detail, DescribeCommandReject(category, detail));
        }

        private static string DescribeCommandReject(string category, string detail)
        {
            switch (category)
            {
                case "1": return "Message authentication code failure";
                case "2": return "Time variant number failure";
                case "3": return "Security terminal number mismatch";
                case "A":
                    if (detail == "01") return "Message length error";
                    if (detail == "02") return "Field separator missing or unexpected";
                    if (detail == "03") return "Too many print groups in transaction reply";
                    return "Message format error";
                case "B":
                    if (detail == "04") return "Illegal terminal command code";
                    if (detail == "05") return "Illegal terminal command modifier";
                    if (detail == "07") return "Non-numeric data in numeric field";
                    if (detail == "08") return "Numeric value out of range";
                    if (detail == "11") return "Too many bills requested in transaction reply";
                    return "Field value error";
                case "C":
                    if (detail == "01") return "Message only accepted while in-service";
                    if (detail == "02") return "Message not accepted during diagnostics";
                    if (detail == "03") return "Message not accepted while out-of-service/supply mode";
                    if (detail == "04") return "Message not accepted while in-service mode";
                    if (detail == "05") return "Message not allowed in NCR status mode";
                    if (detail == "06") return "Message not allowed in Diebold status mode";
                    return "Illegal message type for current mode";
                case "D":
                    if (detail == "01") return "Encryption failure during key change";
                    if (detail == "02") return "Time-of-day clock failure during date/time set";
                    return "Hardware failure";
                case "E":
                    if (detail == "01") return "Command not supported by current software version";
                    if (detail == "02") return "Required device not configured";
                    return "Not supported";
                default: return "Unknown command reject";
            }
        }

        private static string RecommendCommandRejectAction(string category, string detail)
        {
            switch (category)
            {
                case "A":
                case "B": return "Review host message composition, field content, and command payload formatting.";
                case "C": return "Check terminal mode/state progression before resending the command.";
                case "D": return "Inspect device or security module state involved in the rejected command.";
                case "E": return "Check software version and whether the target device/capability is configured.";
                default: return "Review host command composition and terminal state alignment.";
            }
        }
    }
}
