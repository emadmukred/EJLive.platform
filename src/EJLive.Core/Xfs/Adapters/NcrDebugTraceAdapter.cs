using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EJLive.Core.Xfs.Adapters
{
    public sealed class NcrDebugTraceAdapter : IXfsVendorAdapter
    {
        private static readonly Regex TimestampLine = new Regex(
            @"\)\s+(?<date>\d{2}\/\d{2}\/\d{4})\s+(?<time>\d{2}:\d{2}:\d{2}\.\d{3})\s+(?<msg>.+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public XfsVendor Vendor => XfsVendor.NCR;

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            return line.Contains("<DEBUG>") || line.Contains("Virtual controllers processing") || line.Contains("Message Handler") || line.Contains("UPS - ");
        }

        public IReadOnlyList<XfsNormalizedEvent> ParseLines(IEnumerable<string> lines)
        {
            var results = new List<XfsNormalizedEvent>();
            if (lines == null) return results;

            foreach (var raw in lines)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                string line = raw.Trim();
                DateTime? ts = null;
                string msg = line;
                var m = TimestampLine.Match(line);
                if (m.Success)
                {
                    msg = m.Groups["msg"].Value.Trim();
                    if (DateTime.TryParseExact(m.Groups["date"].Value + " " + m.Groups["time"].Value, "dd/MM/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                        ts = parsed;
                }

                if (msg.Contains("Inbound virtual controller dll", StringComparison.OrdinalIgnoreCase))
                    results.Add(NewEvent(ts, XfsEventKind.VirtualControllerInbound, "Virtual controller inbound", msg, XfsSeverity.Info, raw));
                else if (msg.Contains("Outbound virtual controller dll", StringComparison.OrdinalIgnoreCase))
                    results.Add(NewEvent(ts, XfsEventKind.VirtualControllerOutbound, "Virtual controller outbound", msg, XfsSeverity.Info, raw));
                else if (msg.Contains("Processing Message - Terminal Command", StringComparison.OrdinalIgnoreCase) || msg.Contains("Terminal State response sent", StringComparison.OrdinalIgnoreCase))
                    results.Add(NewEvent(ts, XfsEventKind.TerminalCommandLifecycle, "Terminal command lifecycle", msg, XfsSeverity.Info, raw));
                else if (msg.Contains("Validating message", StringComparison.OrdinalIgnoreCase) || msg.Contains("Deformatting message", StringComparison.OrdinalIgnoreCase) || msg.Contains("Customisation Layer Status", StringComparison.OrdinalIgnoreCase))
                    results.Add(NewEvent(ts, XfsEventKind.MiddlewareValidationFlow, "Middleware validation flow", msg, XfsSeverity.Info, raw));
                else if (msg.Contains("heartbeat", StringComparison.OrdinalIgnoreCase))
                    results.Add(NewEvent(ts, XfsEventKind.HeartbeatTelemetry, "Heartbeat telemetry", msg, XfsSeverity.Info, raw));
                else if (msg.Contains("UPS", StringComparison.OrdinalIgnoreCase))
                    results.Add(NewEvent(ts, XfsEventKind.UpsState, "UPS state", msg, msg.Contains("NOT AVAILABLE", StringComparison.OrdinalIgnoreCase) ? XfsSeverity.Warning : XfsSeverity.Info, raw));
            }

            return results;
        }

        private XfsNormalizedEvent NewEvent(DateTime? ts, XfsEventKind kind, string title, string message, XfsSeverity severity, string raw)
        {
            return new XfsNormalizedEvent
            {
                Vendor = XfsVendor.NCR.ToString(),
                SourceLayer = XfsSourceLayer.MiddlewareRuntime,
                Kind = kind,
                Severity = severity,
                Timestamp = ts,
                Title = title,
                Message = message,
                DeviceFamily = "Middleware",
                ServiceImpact = "Reflects NCR middleware/message-handler health.",
                CustomerImpact = "Indirect customer impact through command, routing, or runtime degradation.",
                RecommendedAction = "Correlate with OOXFS runtime, host traffic, and business transaction outcomes.",
                RawLine = raw
            };
        }
    }
}
