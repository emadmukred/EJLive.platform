using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EJLive.Core.Xfs.Adapters
{
    public sealed class NcrHostMessageInAdapter : IXfsVendorAdapter
    {
        private static readonly Regex LineRegex = new Regex(
            @"^(?<time>\d{2}:\d{2}:\d{2}\.\d{3})\s+-\s+(?<date>\d{2}\/\d{2}\/\d{4})\s+#(?<seq>\d+)\s+\[(?<payload>.+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public XfsVendor Vendor => XfsVendor.NCR;

        public bool CanHandle(string line)
        {
            return !string.IsNullOrWhiteSpace(line) && line.Contains("#") && line.Contains("[") && line.Contains("/") == false && line.Contains("TRANSMIT") == false;
        }

        public IReadOnlyList<XfsNormalizedEvent> ParseLines(IEnumerable<string> lines)
        {
            var results = new List<XfsNormalizedEvent>();
            if (lines == null) return results;

            foreach (var raw in lines)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var match = LineRegex.Match(raw.TrimEnd('\0'));
                if (!match.Success) continue;

                DateTime? ts = null;
                if (DateTime.TryParseExact(match.Groups["date"].Value + " " + match.Groups["time"].Value, "dd/MM/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                    ts = parsed;

                var evt = new XfsNormalizedEvent
                {
                    Vendor = XfsVendor.NCR.ToString(),
                    SourceLayer = XfsSourceLayer.HostTransport,
                    Kind = XfsEventKind.HostMessageInbound,
                    Severity = XfsSeverity.Info,
                    Timestamp = ts,
                    Title = "Inbound host message",
                    Message = "Inbound protocol traffic received.",
                    DeviceFamily = "HostProtocol",
                    ServiceImpact = "Shows host-to-terminal traffic freshness and polling continuity.",
                    CustomerImpact = "No direct customer impact unless traffic stops or sequences break.",
                    RecommendedAction = "Correlate inbound cadence with outbound traffic and journal sync freshness.",
                    RawLine = raw
                };
                evt.Data["sequence"] = match.Groups["seq"].Value;
                evt.Data["payload_fragment"] = match.Groups["payload"].Value.Trim();
                results.Add(evt);
            }

            return results;
        }
    }
}
