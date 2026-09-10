using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EJLive.Core.Xfs.Adapters
{
    public sealed class NcrOoxfsRuntimeAdapter : IXfsVendorAdapter
    {
        private static readonly Regex TimestampLine = new Regex(
            @"\)\s+(?<date>\d{2}\/\d{2}\/\d{4})\s+(?<time>\d{2}:\d{2}:\d{2}\.\d{3})\s+(?<msg>.+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public XfsVendor Vendor => XfsVendor.NCR;

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            return line.Contains("<OOXFS>") || line.Contains("WFSOpen()") || line.Contains("WFS_GETINFO_COMPLETE") || line.Contains("GetInfoAsync(");
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

                if (msg.Contains("WFSOpen()", StringComparison.OrdinalIgnoreCase) || msg.Contains("WFSVERSION", StringComparison.OrdinalIgnoreCase) || msg.Contains("WFRegister", StringComparison.OrdinalIgnoreCase))
                    results.Add(NewEvent(ts, XfsEventKind.XfsSessionLifecycle, "XFS session lifecycle", msg, XfsSeverity.Info, raw));
                else if (msg.Contains("GetInfoAsync(", StringComparison.OrdinalIgnoreCase) || msg.Contains("WFS_GETINFO_COMPLETE", StringComparison.OrdinalIgnoreCase) || msg.Contains("WFSCancelAsyncRequest", StringComparison.OrdinalIgnoreCase))
                    results.Add(NewEvent(ts, XfsEventKind.XfsGetInfoCycle, "XFS GetInfo cycle", msg, msg.Contains("=-27") ? XfsSeverity.Warning : XfsSeverity.Info, raw));
                else if (msg.Contains("WFS_INF_VDM_CAPABILITIES", StringComparison.OrdinalIgnoreCase))
                    results.Add(NewEvent(ts, XfsEventKind.XfsCapabilitiesQuery, "XFS capability query", msg, XfsSeverity.Info, raw));
                else if (msg.Contains("WFS_INF_SIU_STATUS", StringComparison.OrdinalIgnoreCase) || msg.Contains("WFS_INF_VDM_STATUS", StringComparison.OrdinalIgnoreCase))
                    results.Add(NewEvent(ts, XfsEventKind.XfsStatusPolling, "XFS status polling", msg, XfsSeverity.Info, raw));
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
                DeviceFamily = "OOXFS",
                ServiceImpact = "Reflects NCR XFS runtime/session health.",
                CustomerImpact = "Indirect customer impact through middleware/session instability.",
                RecommendedAction = "Correlate with DEBUG traces, device availability, and status polling cadence.",
                RawLine = raw
            };
        }
    }
}
