namespace EJLive.Core.Xfs;

public enum XfsSeverity { Trace, Info, Warning, Error, Critical }

public sealed class XfsNormalizedEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString("N");
    public string Vendor { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string EventCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public XfsSeverity Severity { get; set; } = XfsSeverity.Info;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> Attributes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // Extended properties used by Xfs/Adapters implementations
    public XfsSourceLayer SourceLayer { get; set; } = XfsSourceLayer.BusinessJournal;
    public XfsEventKind Kind { get; set; } = XfsEventKind.Unknown;
    public string DeviceCode { get; set; } = string.Empty;
    public string DeviceFamily { get; set; } = string.Empty;
    public string RawCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ServiceImpact { get; set; } = string.Empty;
    public string CustomerImpact { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string OperationalImpact { get; set; } = string.Empty;
    public string RawLine { get; set; } = string.Empty;
    public Dictionary<string, string> Data { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<XfsCassetteSnapshot> Cassettes { get; set; } = new();
    public string TransactionSerialNumber { get; set; } = string.Empty;
    public DateTime? Timestamp { get; set; }
}

public sealed class XfsCassetteSnapshot
{
    public string CassetteId { get; set; } = string.Empty;
    public int Denomination { get; set; }
    public int Count { get; set; }
    public string Currency { get; set; } = "SAR";
    public DateTime CapturedAtUtc { get; set; } = DateTime.UtcNow;

    // Extended properties used by GRG and other adapters
    public int RemainingCount { get; set; }
    public int RejectCount { get; set; }
    public string CassetteState { get; set; } = string.Empty;
    public string CassetteType { get; set; } = string.Empty;
    public string NoteType { get; set; } = string.Empty;
}
