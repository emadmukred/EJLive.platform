using System.Text;
using EJLive.Core.Enums;
using EJLive.Core.Models;

namespace EJLive.Core.Utils
{
    return value;

    // Class: ReportExporter (from 3 sources)
        public static partial class ReportExporter
        {
            // --- Constants & Fields ---
            foreach (var row in rows)
            sb.AppendLine(string.Join(",", row.Select(EscapeCsv)));
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\ReportExporter.cs
                    if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                    {
                        return $"\"{value.Replace("\"", "\"\"")}\"";
                    }
    /// <summary>
    /// Utility for exporting operational reports and audit logs to various formats.
    /// Supports CSV, JSON, and plain text output.
    /// </summary>
    public static class ReportExporter
    {
        /// <summary>
        /// Exports data to CSV format with header row.
        /// </summary>
        /// <param name="headers">Column headers.</param>
        /// <param name="rows">Data rows, each row is an array of string values.</param>
        /// <returns>CSV formatted string.</returns>
        public static string ToCsv(string[] headers, IEnumerable<string[]> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));
            foreach (var row in rows)
            {
                sb.AppendLine(string.Join(",", row.Select(EscapeCsv)));
            }
            return sb.ToString();
        }
    
        /// <summary>
        /// Writes report content to a timestamped file in the specified directory.
        /// </summary>
        /// <param name="directory">Target directory.</param>
        /// <param name="prefix">File name prefix.</param>
        /// <param name="extension">File extension (e.g., "csv", "json", "txt").</param>
        /// <param name="content">Report content.</param>
        /// <returns>Full path to the created file.</returns>
        public static string WriteToFile(string directory, string prefix, string extension, string content)
        {
            Directory.CreateDirectory(directory);
            var timestamp = DateTime.Now.ToString(DateTimeHelper.DateFormat);
            var fileName = $"{prefix}_{timestamp}.{extension}";
            var filePath = Path.Combine(directory, fileName);
            File.WriteAllText(filePath, content, Encoding.UTF8);
            return filePath;
        }
    
        /// <summary>
        /// Generates a plain-text summary header for operational reports.
        /// </summary>
        public static string GetReportHeader(string reportTitle, DateTime? reportDate = null)
        {
            var date = reportDate ?? DateTime.Now;
            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine($"  {reportTitle}");
            sb.AppendLine($"  Generated: {date:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("========================================");
            sb.AppendLine();
            return sb.ToString();
        }
    
        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
    public static class ReportExporter
    {
        public static async Task ExportToCSVAsync<T>(IEnumerable<T> data, string filePath,
            Func<T, string[]> rowSelector, string[] headers, CancellationToken cancellationToken = default)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers.Select(EscapeCSV)));
    
            foreach (var item in data)
            {
                var row = rowSelector(item);
                sb.AppendLine(string.Join(",", row.Select(EscapeCSV)));
            }
    
            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8, cancellationToken);
        }
    
        public static async Task ExportAlertsToCSVAsync(List<AlertPayload> alerts, string filePath,
            CancellationToken cancellationToken = default)
        {
            await ExportToCSVAsync(alerts, filePath,
                a => new[]
                {
                    a.AlertTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    a.Severity.ToString(),
                    a.Title,
                    a.Message,
                    a.Source
                },
                new[] { "Timestamp", "Severity", "Title", "Message", "Source" },
                cancellationToken);
        }
    
        public static async Task ExportATMsToCSVAsync(List<ATMDevice> devices, string filePath,
            CancellationToken cancellationToken = default)
        {
            await ExportToCSVAsync(devices, filePath,
                d => new[]
                {
                    d.DeviceId,
                    d.DeviceName,
                    d.ATMType.ToString(),
                    d.Status.ToString(),
                    d.IPAddress,
                    d.BranchName,
                    d.LastHeartbeat.ToString("yyyy-MM-dd HH:mm:ss"),
                    d.HealthScore.ToString()
                },
                new[] { "DeviceID", "Name", "Type", "Status", "IP", "Branch", "LastHeartbeat", "HealthScore" },
                cancellationToken);
        }
    
        public static async Task ExportTransactionsToCSVAsync(List<Transaction> transactions, string filePath,
            CancellationToken cancellationToken = default)
        {
            await ExportToCSVAsync(transactions, filePath,
                t => new[]
                {
                    t.TransactionTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    t.DeviceId,
                    t.Type.ToString(),
                    t.Amount?.ToString() ?? "0",
                    t.Status.ToString(),
                    t.IsSuspicious ? "YES" : "NO"
                },
                new[] { "Timestamp", "DeviceID", "Type", "Amount", "Status", "Suspicious" },
                cancellationToken);
        }
    
        private static string EscapeCSV(string field)
        {
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }
    
        public static async Task ExportToSimpleHTMLAsync<T>(IEnumerable<T> data, string title,
            string filePath, Func<T, string[]> rowSelector, string[] headers,
            CancellationToken cancellationToken = default)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
            sb.AppendLine($"<title>{title}</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body{font-family:Arial,sans-serif;margin:20px;background:#f5f5f5}");
            sb.AppendLine("h1{color:#333;text-align:center}");
            sb.AppendLine("table{width:100%;border-collapse:collapse;background:#fff;box-shadow:0 2px 4px rgba(0,0,0,0.1)}");
            sb.AppendLine("th{background:#2196F3;color:#fff;padding:12px;text-align:left;font-weight:bold}");
            sb.AppendLine("td{padding:10px;border-bottom:1px solid #ddd}");
            sb.AppendLine("tr:hover{background:#f1f1f1}");
            sb.AppendLine(".critical{color:#d32f2f;font-weight:bold}");
            sb.AppendLine(".warning{color:#f57c00;font-weight:bold}");
            sb.AppendLine(".info{color:#1976d2}");
            sb.AppendLine(".timestamp{color:#666;font-size:0.9em}");
            sb.AppendLine("</style></head><body>");
            sb.AppendLine($"<h1>{title}</h1>");
            sb.AppendLine($"<p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine("<table><thead><tr>");
            foreach (var h in headers)
                sb.AppendLine($"<th>{h}</th>");
            sb.AppendLine("</tr></thead><tbody>");
    
            foreach (var item in data)
            {
                sb.AppendLine("<tr>");
                foreach (var cell in rowSelector(item))
                    sb.AppendLine($"<td>{cell}</td>");
                sb.AppendLine("</tr>");
            }
    
            sb.AppendLine("</tbody></table></body></html>");
            await File.WriteAllTextAsync(filePath, sb.ToString(), cancellationToken);
        }
    }
    public partial class ReportExporter
        {
            foreach (var row in rows)
            sb.AppendLine(string.Join(",", row.Select(EscapeCsv)));
                if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                {
                    return $"\"{value.Replace("\"", "\"\"")}\"";
                }
    public partial public public static class ReportExporter
        {
            public static async Task ExportToCSVAsync<T>(IEnumerable<T> data, string filePath,
            Func<T, string[]> rowSelector, string[] headers, CancellationToken cancellationToken = default)
            {
            public static async Task ExportAlertsToCSVAsync(List<AlertPayload> alerts, string filePath,
            CancellationToken cancellationToken = default)
            {
            public static async Task ExportATMsToCSVAsync(List<ATMDevice> devices, string filePath,
            CancellationToken cancellationToken = default)
            {
            public static async Task ExportTransactionsToCSVAsync(List<Transaction> transactions, string filePath,
            CancellationToken cancellationToken = default)
            {
            private static string EscapeCSV(string field)
            {
            public static async Task ExportToSimpleHTMLAsync<T>(IEnumerable<T> data, string title,
            string filePath, Func<T, string[]> rowSelector, string[] headers,
            CancellationToken cancellationToken = default)
            {
            public static string ToCsv(string[] headers, IEnumerable<string[]> rows)
            {
            public static string WriteToFile(string directory, string prefix, string extension, string content)
            {
            public static string GetReportHeader(string reportTitle, DateTime? reportDate = null)
            {
            private static string EscapeCsv(string value)
            {
        }
    
    }
    public partial public static class ReportExporter
        {
            public static async Task ExportToCSVAsync<T>(IEnumerable<T> data, string filePath,
            Func<T, string[]> rowSelector, string[] headers, CancellationToken cancellationToken = default)
            {
            public static async Task ExportAlertsToCSVAsync(List<AlertPayload> alerts, string filePath,
            CancellationToken cancellationToken = default)
            {
            public static async Task ExportATMsToCSVAsync(List<ATMDevice> devices, string filePath,
            CancellationToken cancellationToken = default)
            {
            public static async Task ExportTransactionsToCSVAsync(List<Transaction> transactions, string filePath,
            CancellationToken cancellationToken = default)
            {
            private static string EscapeCSV(string field)
            {
            public static async Task ExportToSimpleHTMLAsync<T>(IEnumerable<T> data, string title,
            string filePath, Func<T, string[]> rowSelector, string[] headers,
            CancellationToken cancellationToken = default)
            {
            public static string ToCsv(string[] headers, IEnumerable<string[]> rows)
            {
            public static string WriteToFile(string directory, string prefix, string extension, string content)
            {
            public static string GetReportHeader(string reportTitle, DateTime? reportDate = null)
            {
            private static string EscapeCsv(string value)
            {
        }
    
    }
    public partial class ReportExporter
        {
            foreach (var row in rows)
            sb.AppendLine(string.Join(",", row.Select(EscapeCsv)));
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Utils\ReportExporter.cs
                if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                {
                    return $"\"{value.Replace("\"", "\"\"")}\"";
                }
}
