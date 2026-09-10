using System;

namespace EJLive.Core.Models
{
    public partial class FleetSummary
        {
            public int Total { get; set; }
    
    
            public int Connected { get; set; }
    
    
            public int Syncing { get; set; }
    
    
            public int Offline { get; set; }
    
    
            public int AverageHealth { get; set; }
    
    
            public int WarningCount { get; set; }
    
    
            public int CriticalCount { get; set; }
    
    
            public DateTime SummaryAtUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    /// <summary>
        /// Summary of the entire ATM fleet operational state.
        /// Used by OperationalStateStore and UnifiedBusinessRuntime.
        /// </summary>
        public sealed class FleetSummary
        {
            public int Total { get; set; }
            public int Connected { get; set; }
            public int Syncing { get; set; }
            public int Offline { get; set; }
            public int AverageHealth { get; set; }
            public int WarningCount { get; set; }
            public int CriticalCount { get; set; }
            public DateTime SummaryAtUtc { get; set; } = DateTime.UtcNow;
        }
    public partial class PerformanceMetric
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
    /// <summary>
        /// Represents a single performance metric tracked by the system.
        /// </summary>
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
    public partial class SyncSummary
        {
            public int TotalRecords { get; set; }
    
    
            public int Pending { get; set; }
    
    
            public int InProgress { get; set; }
    
    
            public int Completed { get; set; }
    
    
            public int Failed { get; set; }
    
    
            public long TotalBytes { get; set; }
    
    
            public long SyncedBytes { get; set; }
    
    
            public double AverageSpeedKBs { get; set; }
    
    
            public DateTime SummaryAtUtc { get; set; } = DateTime.UtcNow;
    
    
            public double ProgressPercent => TotalRecords > 0
                ? (double)Completed / TotalRecords * 100.0
                : 100.0;
    
    
        }
    /// <summary>
        /// Summary of journal synchronization across the fleet.
        /// </summary>
        public sealed class SyncSummary
        {
            public int TotalRecords { get; set; }
            public int Pending { get; set; }
            public int InProgress { get; set; }
            public int Completed { get; set; }
            public int Failed { get; set; }
            public long TotalBytes { get; set; }
            public long SyncedBytes { get; set; }
            public double AverageSpeedKBs { get; set; }
            public DateTime SummaryAtUtc { get; set; } = DateTime.UtcNow;
    
            public double ProgressPercent => TotalRecords > 0
                ? (double)Completed / TotalRecords * 100.0
                : 100.0;
        }

    // Class: FleetSummary (from 3 sources)
        public sealed partial class FleetSummary
        {
            // --- Properties ---
                    public int Total { get; set; }
    
                    public int Connected { get; set; }
    
                    public int Syncing { get; set; }
    
                    public int Offline { get; set; }
    
                    public int AverageHealth { get; set; }
    
                    public int WarningCount { get; set; }
    
                    public int CriticalCount { get; set; }
    
                    public DateTime SummaryAtUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Enum: MetricType (from 1 sources)
        public partial enum MetricType
        {
            // --- Constants & Fields ---
                    Number,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\FleetSummary.cs
                    Count
    
    
        }
    // Class: PerformanceMetric (from 3 sources)
        public sealed partial class PerformanceMetric
        {
            // --- Properties ---
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
    // Class: SyncSummary (from 3 sources)
        public sealed partial class SyncSummary
        {
            // --- Properties ---
                    public int TotalRecords { get; set; }
    
                    public int Pending { get; set; }
    
                    public int InProgress { get; set; }
    
                    public int Completed { get; set; }
    
                    public int Failed { get; set; }
    
                    public long TotalBytes { get; set; }
    
                    public long SyncedBytes { get; set; }
    
                    public double AverageSpeedKBs { get; set; }
    
                    public DateTime SummaryAtUtc { get; set; } = DateTime.UtcNow;
    
                    public double ProgressPercent => TotalRecords > 0
                        ? (double)Completed / TotalRecords * 100.0
                        : 100.0;
    
    
        }

    public partial enum MetricType
        {
            Number,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\FleetSummary.cs
            Count
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\FleetSummary.cs
            Count
    
    
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
    public partial enum MetricType
        {
            Number,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\FleetSummary.cs
            Count
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\FleetSummary.cs
            Count
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\FleetSummary.cs
            Count
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\FleetSummary.cs
            Count
    
    
        }
}
