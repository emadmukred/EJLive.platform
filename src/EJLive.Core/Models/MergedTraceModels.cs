using EJLive.Core.Xfs;

namespace EJLive.Core.Models;

public sealed class NcrMergedTraceEvent
{
    public DateTime? Timestamp { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public XfsVendor Vendor { get; set; } = XfsVendor.NCR;
    public XfsSourceLayer SourceLayer { get; set; }
    public XfsEventKind Kind { get; set; }
    public XfsSeverity Severity { get; set; }
    public string DeviceFamily { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string RawLine { get; set; } = string.Empty;
    public Dictionary<string, string> Data { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class NcrMergedTraceCorrelationResult
{
    public string SourceName { get; set; } = "NCR MergedTrace";
    public XfsVendor Vendor { get; set; } = XfsVendor.NCR;
    public int TotalRawLines { get; set; }
    public int TotalEvents { get; set; }
    public List<NcrMergedTraceEvent> Timeline { get; set; } = [];
    public Dictionary<string, int> BySourceLayer { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> ByKind { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
