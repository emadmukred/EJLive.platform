using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EJLive.Core.Xfs.Adapters
{
    public sealed class OoxfsRuntimeAdapter : IXfsVendorAdapter
    {
        private static readonly Regex GetInfoRegex = new Regex(@"GetInfo(?:Async)?\((?<cmd>\d+\([^\)]+\))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SessionEventRegex = new Regex(@"WFS_GETINFO_COMPLETE|WFSOpen\(\)=0|WFRegister\((?<reg>\d+)\)=0", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public XfsVendor Vendor => XfsVendor.NCR;

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            return line.Contains("<OOXFS>") || line.Contains("WFSOpen()=0") || line.Contains("WFS_GETINFO_COMPLETE") || line.Contains("GetInfoAsync(");
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
                    SourceLayer = XfsSourceLayer.HardwareDiagnostic,
                    Kind = XfsEventKind.DeviceStatus,
                    Severity = XfsSeverity.Info,
                    DeviceFamily = "OOXFSRuntime",
                    Title = "OOXFS runtime activity",
                    Message = raw.Trim(),
                    RawLine = raw,
                    ServiceImpact = "XFS session/runtime activity observed.",
                    CustomerImpact = "No direct customer impact alone; useful for runtime health and diagnostics.",
                    RecommendedAction = "Correlate with debug trace and device status events for runtime-level failures."
                };
                var g = GetInfoRegex.Match(raw);
                if (g.Success) evt.Data["xfs_command"] = g.Groups["cmd"].Value;
                var s = SessionEventRegex.Match(raw);
                if (s.Success && s.Groups["reg"].Success) evt.Data["register_code"] = s.Groups["reg"].Value;
                if (raw.IndexOf("WFS_GETINFO_COMPLETE", StringComparison.OrdinalIgnoreCase) >= 0) evt.Data["event"] = "WFS_GETINFO_COMPLETE";
                if (raw.IndexOf("WFSOpen()=0", StringComparison.OrdinalIgnoreCase) >= 0) evt.Data["event"] = "WFSOpenSuccess";
                events.Add(evt);
            }
            return events;
    }
}
}
