using System;
using System.Collections.Generic;

namespace EJLive.Core.Xfs.Adapters
{
    public sealed class DebugTraceAdapter : IXfsVendorAdapter
    {
        public XfsVendor Vendor => XfsVendor.NCR;

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            return line.Contains("<DEBUG>") || line.Contains("Virtual controllers processing") || line.Contains("Message Handler") || line.Contains("UPS - ") || line.Contains("Customisation Layer Status");
        }

        public IReadOnlyList<XfsNormalizedEvent> ParseLines(IEnumerable<string> lines)
        {
            var events = new List<XfsNormalizedEvent>();
            foreach (var raw in lines ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(raw) || !CanHandle(raw)) continue;
                var evt = new XfsNormalizedEvent
                {
                    Vendor = XfsVendor.NCR.ToString(),
                    SourceLayer = XfsSourceLayer.DriverError,
                    Kind = XfsEventKind.DeviceStatus,
                    Severity = raw.IndexOf("ERROR", StringComparison.OrdinalIgnoreCase) >= 0 ? XfsSeverity.Warning : XfsSeverity.Info,
                    DeviceFamily = "MiddlewareDebug",
                    Title = "Middleware/DEBUG trace",
                    Message = raw.Trim(),
                    RawLine = raw,
                    ServiceImpact = "Middleware or virtual controller orchestration activity observed.",
                    CustomerImpact = "Indirect impact through command handling or terminal state progression.",
                    RecommendedAction = "Correlate with OOXFS runtime, message traffic, and journal events to diagnose orchestration issues."
                };

                if (raw.IndexOf("Inbound virtual controller", StringComparison.OrdinalIgnoreCase) >= 0) evt.Data["trace_stage"] = "InboundVirtualController";
                else if (raw.IndexOf("Outbound virtual controller", StringComparison.OrdinalIgnoreCase) >= 0) evt.Data["trace_stage"] = "OutboundVirtualController";
                else if (raw.IndexOf("Processing Message - Terminal Command", StringComparison.OrdinalIgnoreCase) >= 0) evt.Data["trace_stage"] = "TerminalCommandProcessing";
                else if (raw.IndexOf("Validating message", StringComparison.OrdinalIgnoreCase) >= 0) evt.Data["trace_stage"] = "MessageValidation";
                else if (raw.IndexOf("Deformatting message", StringComparison.OrdinalIgnoreCase) >= 0) evt.Data["trace_stage"] = "MessageDeformatting";
                else if (raw.IndexOf("Terminal State response sent", StringComparison.OrdinalIgnoreCase) >= 0) evt.Data["trace_stage"] = "TerminalStateResponse";
                else if (raw.IndexOf("UPS -", StringComparison.OrdinalIgnoreCase) >= 0) evt.Data["trace_stage"] = "UpsState";
                else if (raw.IndexOf("heartbeat", StringComparison.OrdinalIgnoreCase) >= 0) evt.Data["trace_stage"] = "Heartbeat";

                events.Add(evt);
            }
            return events;
    }
}
}
