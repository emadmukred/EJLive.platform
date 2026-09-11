using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EJLive.Core;
using EJLive.Core.Models;

namespace EJLive.Reports
{
    public enum ReportFormat
    {
        Pdf,
        Excel,
        Html,
        Csv
    }

    public class ReportBase
    {
        public DateTime ReportDateUtc { get; set; } = DateTime.UtcNow;
        public List<string> Errors { get; } = new List<string>();
    }

    public sealed class PerformanceReport : ReportBase
    {
        public DateTime PeriodFromUtc { get; set; }
        public DateTime PeriodToUtc { get; set; }
        public List<PerformanceMetric> Metrics { get; } = new List<PerformanceMetric>();
        public PerformanceStatistics Statistics { get; set; } = new PerformanceStatistics();
    }

    public sealed class ATMReport : ReportBase
    {
        public List<ATMInfo> ATMs { get; } = new List<ATMInfo>();
        public ATMStatistics Statistics { get; set; } = new ATMStatistics();
    }

    public sealed class PerformanceStatistics
    {
        public double AverageCpu { get; set; }
        public double AverageMemory { get; set; }
        public double PeakConnections { get; set; }
        public double TotalErrors { get; set; }
    }

    public sealed class ATMStatistics
    {
        public int TotalATMs { get; set; }
        public int OnlineATMs { get; set; }
        public int OfflineATMs { get; set; }
        public int MaintenanceATMs { get; set; }
        public int AlertATMs { get; set; }
    }

    public sealed class AdvancedReportingService
    {
        private readonly DatabaseManager _database;

        public AdvancedReportingService(DatabaseManager database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<PerformanceReport> GeneratePerformanceReportAsync(DateTime fromDateUtc, DateTime toDateUtc)
        {
            var report = new PerformanceReport
            {
                PeriodFromUtc = fromDateUtc,
                PeriodToUtc = toDateUtc
            };

            try
            {
                var metrics = await _database.GetPerformanceDataAsync(fromDateUtc, toDateUtc).ConfigureAwait(false);
                report.Metrics.AddRange(metrics);
                report.Statistics = CalculatePerformanceStatistics(metrics);
            }
            catch (Exception ex)
            {
                report.Errors.Add("Performance report failed: " + ex.Message);
            }

            return report;
        }

        public async Task<ATMReport> GenerateATMReportAsync()
        {
            var report = new ATMReport();

            try
            {
                var atms = await _database.GetATMsAsync().ConfigureAwait(false);
                report.ATMs.AddRange(atms);
                report.Statistics = CalculateATMStatistics(atms);
            }
            catch (Exception ex)
            {
                report.Errors.Add("ATM report failed: " + ex.Message);
            }

            return report;
        }

        public async Task<bool> ExportReportAsync(ReportBase report, ReportFormat format, string filePath)
        {
            if (report == null || string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            var content = RenderReport(report, format);
            var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(filePath, content, Encoding.UTF8).ConfigureAwait(false);
            return true;
        }

        private static PerformanceStatistics CalculatePerformanceStatistics(IReadOnlyCollection<PerformanceMetric> metrics)
        {
            return new PerformanceStatistics
            {
                AverageCpu = AverageMetric(metrics, "CPU_Usage"),
                AverageMemory = AverageMetric(metrics, "Memory_Usage"),
                PeakConnections = MaxMetric(metrics, "Active_Connections"),
                TotalErrors = SumMetric(metrics, "Error_Rate")
            };
        }

        private static ATMStatistics CalculateATMStatistics(IReadOnlyCollection<ATMInfo> atms)
        {
            return new ATMStatistics
            {
                TotalATMs = atms.Count,
                OnlineATMs = atms.Count(a => a.RuntimeStatus == AtmRuntimeStatus.Online),
                OfflineATMs = atms.Count(a => a.RuntimeStatus == AtmRuntimeStatus.Offline),
                MaintenanceATMs = atms.Count(a => a.RuntimeStatus == AtmRuntimeStatus.Maintenance),
                AlertATMs = atms.Count(a => a.HasAlerts)
            };
        }

        private static string RenderReport(ReportBase report, ReportFormat format)
        {
            switch (format)
            {
                case ReportFormat.Html:
                    return "<html><body><h1>EJLive Report</h1><p>" + report.ReportDateUtc.ToString("O") + "</p></body></html>";
                case ReportFormat.Csv:
                    return "ReportDateUtc,Errors" + Environment.NewLine +
                           Escape(report.ReportDateUtc.ToString("O")) + "," + report.Errors.Count;
                case ReportFormat.Pdf:
                case ReportFormat.Excel:
                default:
                    return "EJLive Report " + report.ReportDateUtc.ToString("O");
            }
        }

        private static double AverageMetric(IEnumerable<PerformanceMetric> metrics, string key)
        {
            var values = metrics.Where(m => string.Equals(m.Key, key, StringComparison.OrdinalIgnoreCase))
                .Select(m => m.CurrentValue)
                .ToList();
            return values.Count == 0 ? 0 : values.Average();
        }

        private static double MaxMetric(IEnumerable<PerformanceMetric> metrics, string key)
        {
            var values = metrics.Where(m => string.Equals(m.Key, key, StringComparison.OrdinalIgnoreCase))
                .Select(m => m.CurrentValue)
                .ToList();
            return values.Count == 0 ? 0 : values.Max();
        }

        private static double SumMetric(IEnumerable<PerformanceMetric> metrics, string key)
        {
            return metrics.Where(m => string.Equals(m.Key, key, StringComparison.OrdinalIgnoreCase))
                .Sum(m => m.CurrentValue);
        }

        private static string Escape(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }
    }
}
