using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EJLive.Core.Xfs.Adapters
{
    public sealed class HostMessageInAdapter : IXfsVendorAdapter
    {
        private static readonly Regex LineRegex = new Regex(@"^(?<time>\d{2}:\d{2}:\d{2}\.\d{3})\s+-\s+(?<date>\d{2}\/\d{2}\/\d{4})\s+#(?<seq>\d+)\s+\[(?<len>\d+)", RegexOptions.Compiled);

        public XfsVendor Vendor => XfsVendor.NCR;

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            return line.Contains("#") && line.Contains("[") && line.Contains("75]") && line.Contains("/") == false;
        }

        public IReadOnlyList<XfsNormalizedEvent> ParseLines(IEnumerable<string> lines)
        {
            var events = new List<XfsNormalizedEvent>();
            foreach (var raw in lines ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var m = LineRegex.Match(raw.Trim());
                if (!m.Success) continue;
                var evt = new XfsNormalizedEvent
                {
                    Vendor = XfsVendor.NCR.ToString(),
                    SourceLayer = XfsSourceLayer.DriverError,
                    Kind = XfsEventKind.HostReply,
                    Severity = XfsSeverity.Info,
                    DeviceFamily = "HostTransportIn",
                    Title = "Inbound host message",
                    Message = raw.Trim(),
                    RawLine = raw,
                    ServiceImpact = "Inbound host traffic heartbeat/poll observed.",
                    CustomerImpact = "No direct impact alone, but useful for transport health.",
                    RecommendedAction = "Correlate cadence with outbound traffic and journal activity."
                };
                evt.Data["time_hint"] = m.Groups["time"].Value;
                evt.Data["date_hint"] = m.Groups["date"].Value;
                evt.Data["sequence"] = m.Groups["seq"].Value;
                evt.Data["payload_len"] = m.Groups["len"].Value;
                events.Add(evt);
            }
            return events;
    }
}
}
