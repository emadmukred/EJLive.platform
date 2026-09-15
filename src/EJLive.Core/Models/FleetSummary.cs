namespace EJLive.Core.Models
{
    /// <summary>
    /// Represents a single performance metric tracked by the system.
    /// </summary>
    /// <remarks>
    /// Wave 1 (SS-04 D-08): rewrote the file. The previous version had three copies of
    /// <c>PerformanceMetric</c> (one <c>sealed</c>, one plain, one <c>partial</c>) and four
    /// copies of <c>MetricType</c>, all with the same two-member body
    /// (<c>Number</c>, <c>Count</c>). The richer five-member enum (<c>Percentage</c>,
    /// <c>Bytes</c>, <c>Duration</c>, <c>Rate</c>, <c>Count</c>) was carried by one copy
    /// and is preserved because the dashboard snapshot panel and the analytics tabs (SS-10.5)
    /// bind to it. the <c>partial</c> modifier before <c>enum</c> is not a C# construct; it has been removed.
    /// </remarks>
    public sealed class PerformanceMetric
    {
        public string Key { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public MetricType Type { get; set; } = MetricType.Number;
        public double CurrentValue { get; set; }
        public double AverageValue { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public enum MetricType
    {
        Number,
        Percentage,
        Bytes,
        Duration,
        Rate,
        Count
    }
}
