using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EJLive.Core.Xfs.Adapters
{
    public sealed class HostMessageOutAdapter : IXfsVendorAdapter
    {
        private static readonly Regex LineRegex = new Regex(@"^(?<time>\d{2}:\d{2}:\d{2}\.\d{3})\s+-\s+(?<date>\d{2}\/\d{2}\/\d{4})\s+#(?<seq>\d+)\s+\[(?<len>\d+)", RegexOptions.Compiled);

        public XfsVendor Vendor => XfsVendor.NCR;

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            return line.Contains("LA050100") || line.Contains("BG531-") || (line.Contains("#") && line.Contains("[") && line.Contains("000"));
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
                    Kind = XfsEventKind.TransactionRequest,
                    Severity = XfsSeverity.Info,
                    DeviceFamily = "HostTransportOut",
                    Title = "Outbound host message",
                    Message = raw.Trim(),
                    RawLine = raw,
                    ServiceImpact = "Outbound protocol activity observed.",
                    CustomerImpact = "No direct impact alone, but useful for host/session health.",
                    RecommendedAction = "Correlate with inbound traffic and middleware traces for request lifecycle."
                };
                evt.Data["time_hint"] = m.Groups["time"].Value;
                evt.Data["date_hint"] = m.Groups["date"].Value;
                evt.Data["sequence"] = m.Groups["seq"].Value;
                evt.Data["payload_len"] = m.Groups["len"].Value;
                if (raw.Contains("LA050100")) evt.Data["payload_hint"] = "LA050100";
                if (raw.Contains("BG531-")) evt.Data["payload_hint_2"] = "BG531";
                events.Add(evt);
            }
            return events;
    }
}
}
