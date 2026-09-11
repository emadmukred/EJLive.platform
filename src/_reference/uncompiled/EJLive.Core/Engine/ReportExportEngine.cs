using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EJLive.Core.Models;


namespace EJLive.Core.Engine
{
    /// <summary>
    /// محرك تصدير التقارير الكامل — CSV + HTML + JSON
    /// يطبق: تقارير حسب الفترة، الصراف، نوع العملية
    ///        جدولة التقارير التلقائية (A-06, A-07)
    ///        Pre-Aggregated Statistics (A-08)
    /// </summary>
    public class ReportExportEngine
    {
        private readonly string _reportsPath;

        public ReportExportEngine(string reportsPath = null)
        {
            _reportsPath = reportsPath ?? AppConstants.DefaultReportsPath;
            if (!Directory.Exists(_reportsPath)) Directory.CreateDirectory(_reportsPath);
        }

        // ==========================================
        // تقرير CSV للمعاملات
        // ==========================================

        public string ExportTransactionsCSV(string atmId, DateTime from, DateTime to, string title = null)
        {
            var entries = DatabaseManager.Instance.SearchArchive(atmId, from, to, null, 10000);
            var sb      = new StringBuilder();

            sb.AppendLine("sep=,");
            sb.AppendLine($"# EJLive Enterprise — {title ?? "تقرير المعاملات"}");
            sb.AppendLine($"# الصراف: {atmId ?? "جميع الصرافات"} | الفترة: {from:yyyy-MM-dd} إلى {to:yyyy-MM-dd}");
            sb.AppendLine($"# تاريخ التصدير: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine("معرف الإدخال,معرف الصراف,اسم الملف,الحجم الأصلي (KB),حجم مضغوط (KB),عدد العمليات,مسار الأرشيف,تاريخ الاستلام");

            foreach (var e in entries)
            {
                sb.AppendLine($"{e.EntryId},{e.ATMId},{e.FileName},{e.OriginalSize/1024.0:F1},{e.CompressedSize/1024.0:F1},{e.TransactionCount},{e.ArchivePath},{e.ReceivedAt:yyyy-MM-dd HH:mm:ss}");
            }

            sb.AppendLine();
            sb.AppendLine($"# إجمالي الإدخالات: {entries.Count}");

            return SaveReport("transactions", "csv", sb.ToString());
        }

        // ==========================================
        // تقرير HTML تحليلي
        // ==========================================

        public string ExportAnalyticsHTML(JournalAnalysisResult analysis)
        {
            var sb = new StringBuilder();
            sb.AppendLine(HtmlHeader($"تقرير تحليل الجورنال — {analysis.ATMId}"));
            sb.AppendLine($@"
<div class='header'>
    <h1>📊 تقرير تحليل الجورنال</h1>
    <div class='meta'>
        <span>🏧 الصراف: <strong>{analysis.ATMId}</strong></span>
        <span>📁 الملف: <strong>{Path.GetFileName(analysis.FilePath)}</strong></span>
        <span>📅 {DateTime.Now:yyyy-MM-dd HH:mm}</span>
    </div>
</div>

<div class='stats-grid'>
    <div class='stat-card green'>
        <div class='stat-value'>{analysis.ApprovedCount}</div>
        <div class='stat-label'>✅ عمليات ناجحة</div>
    </div>
    <div class='stat-card red'>
        <div class='stat-value'>{analysis.DeclinedCount}</div>
        <div class='stat-label'>❌ عمليات مرفوضة</div>
    </div>
    <div class='stat-card orange'>
        <div class='stat-value'>{analysis.CardsCaptured}</div>
        <div class='stat-label'>💳 بطاقات محتجزة</div>
    </div>
    <div class='stat-card blue'>
        <div class='stat-value'>{analysis.SuccessRate:F1}%</div>
        <div class='stat-label'>📈 معدل النجاح</div>
    </div>
    <div class='stat-card purple'>
        <div class='stat-value'>{analysis.TotalCashDispensed:N0}</div>
        <div class='stat-label'>💰 إجمالي النقد المصروف</div>
    </div>
    <div class='stat-card gray'>
        <div class='stat-value'>{analysis.ErrorCount}</div>
        <div class='stat-label'>⚠️ أخطاء الجورنال</div>
    </div>
    <div class='stat-card yellow'>
        <div class='stat-value'>{analysis.PowerResetCount}</div>
        <div class='stat-label'>⚡ إعادة تشغيل</div>
    </div>
    <div class='stat-card teal'>
        <div class='stat-value'>{analysis.LineCount:N0}</div>
        <div class='stat-label'>📝 إجمالي السطور</div>
    </div>
</div>");

            // أكواد الأخطاء
            if (analysis.ErrorCodes.Count > 0)
            {
                sb.AppendLine("<div class='section'><h2>🚨 أكواد الأخطاء المكتشفة</h2><ul class='error-list'>");
                foreach (var code in analysis.ErrorCodes)
                    sb.AppendLine($"<li class='error-item'>{code}</li>");
                sb.AppendLine("</ul></div>");
            }

            // جدول العمليات
            sb.AppendLine("<div class='section'><h2>📋 قائمة العمليات</h2><table class='data-table'>");
            sb.AppendLine("<tr><th>#</th><th>النوع</th><th>النتيجة</th><th>المبلغ</th><th>رمز الخطأ</th><th>السطر</th></tr>");
            int n = 0;
            foreach (var tx in analysis.AllTransactions)
            {
                sb.AppendLine($"<tr class='row-{(++n % 2 == 0 ? "even" : "odd")}'>");
                sb.AppendLine($"<td>{n}</td><td>{tx.TxType}</td><td class='result-{tx.Result.ToString().ToLower()}'>{tx.ResultIcon} {tx.Result}</td>");
                sb.AppendLine($"<td>{(tx.Amount > 0 ? tx.Amount.ToString("N0") : "—")}</td><td>{tx.ErrorCode ?? "—"}</td><td>{tx.LineNumber}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table></div>");
            sb.AppendLine(HtmlFooter());

            return SaveReport($"analytics_{analysis.ATMId}", "html", sb.ToString());
        }

        // ==========================================
        // تقرير NOC اليومي (A-06)
        // ==========================================

        public string ExportDailyNocReport(List<ATMInfo> atms, DateTime date)
        {
            var sb = new StringBuilder();
            sb.AppendLine(HtmlHeader($"تقرير NOC اليومي — {date:yyyy-MM-dd}"));
            sb.AppendLine($@"
<div class='header noc-header'>
    <h1>🖥️ تقرير NOC اليومي</h1>
    <p>التاريخ: <strong>{date:yyyy-MM-dd}</strong> | وقت التصدير: <strong>{DateTime.Now:HH:mm:ss}</strong></p>
</div>

<div class='stats-grid'>
    <div class='stat-card green'><div class='stat-value'>{CountByState(atms, ConnectionStatus.Connected)}</div><div class='stat-label'>✅ متصل</div></div>
    <div class='stat-card red'><div class='stat-value'>{CountByState(atms, ConnectionStatus.Disconnected)}</div><div class='stat-label'>✕ منقطع</div></div>
    <div class='stat-card blue'><div class='stat-value'>{atms.Count}</div><div class='stat-label'>🏧 إجمالي الصرافات</div></div>
</div>

<div class='section'><h2>📊 حالة الصرافات</h2>
<table class='data-table'>
<tr><th>المعرف</th><th>الاسم</th><th>النوع</th><th>الفرع</th><th>الحالة</th><th>آخر Heartbeat</th><th>آخر جورنال</th><th>الكمون</th></tr>");

            foreach (var atm in atms)
            {
                var stateColor = atm.GetCardState() == ATMCardState.ConnectedActive ? "green"
                               : atm.GetCardState() == ATMCardState.ConnectedIdle   ? "yellow"
                               : atm.GetCardState() == ATMCardState.CriticalOffline ? "gray"
                               : "red";
                sb.AppendLine($@"<tr class='row-{stateColor}'>
<td><strong>{atm.ATM_ID}</strong></td><td>{atm.ATM_Name}</td><td>{atm.ATM_Type}</td>
<td>{atm.BranchName ?? "—"}</td>
<td class='status-{stateColor}'>{atm.GetStatusLabel()}</td>
<td>{atm.GetElapsed(atm.LastHeartbeatUtc)}</td>
<td>{atm.LastJournalFile ?? "—"}</td>
<td>{atm.Latency_ms} ms</td></tr>");
            }
            sb.AppendLine("</table></div>");
            sb.AppendLine(HtmlFooter());

            return SaveReport($"noc_daily_{date:yyyyMMdd}", "html", sb.ToString());
        }

        // ==========================================
        // تقرير سجل التدقيق CSV (S-03)
        // ==========================================

        public string ExportAuditLogCSV(DateTime from, DateTime to)
        {
            var table = DatabaseManager.Instance.GetAuditLog(null, from, to, 50000);
            var sb    = new StringBuilder();
            sb.AppendLine("sep=,");
            sb.AppendLine($"# EJLive Enterprise — سجل التدقيق");
            sb.AppendLine($"# الفترة: {from:yyyy-MM-dd} إلى {to:yyyy-MM-dd}");
            sb.AppendLine();

            // رأس العمود
            var headers = new List<string>();
            foreach (System.Data.DataColumn col in table.Columns) headers.Add(col.ColumnName);
            sb.AppendLine(string.Join(",", headers));

            // البيانات
            foreach (System.Data.DataRow row in table.Rows)
            {
                var cols = new List<string>();
                foreach (var h in headers) cols.Add($"\"{row[h]?.ToString()?.Replace("\"", "\"\"")}\"");
                sb.AppendLine(string.Join(",", cols));
            }

            return SaveReport($"audit_{from:yyyyMMdd}_{to:yyyyMMdd}", "csv", sb.ToString());
        }

        // ==========================================
        // مساعدات
        // ==========================================

        private string SaveReport(string name, string ext, string content)
        {
            var fileName = Path.Combine(_reportsPath, $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.{ext}");
            File.WriteAllText(fileName, content, Encoding.UTF8);
            AppLogger.Instance.Info($"Report exported: {fileName}", "Reports");
            return fileName;
        }

        private int CountByState(List<ATMInfo> atms, ConnectionStatus status)
            => atms?.FindAll(a => a.ConnectionStatus == status)?.Count ?? 0;

        private string HtmlHeader(string title) => $@"<!DOCTYPE html>
<html lang='ar' dir='rtl'>
<head>
<meta charset='UTF-8'>
<title>{title}</title>
<style>
body{{font-family:'Segoe UI',Tahoma,sans-serif;background:#0d1117;color:#e6edf3;margin:0;padding:20px;direction:rtl}}
.header{{background:linear-gradient(135deg,#1e3a5f,#0d1117);border:1px solid #30363d;border-radius:12px;padding:24px;margin-bottom:24px}}
.header h1{{margin:0 0 8px;font-size:28px;color:#58a6ff}}
.meta span{{margin-left:24px;color:#8b949e;font-size:14px}}
.meta strong{{color:#e6edf3}}
.stats-grid{{display:grid;grid-template-columns:repeat(auto-fit,minmax(160px,1fr));gap:16px;margin-bottom:24px}}
.stat-card{{border-radius:10px;padding:20px;text-align:center;border:1px solid rgba(255,255,255,.1)}}
.stat-card.green{{background:#0a2e1b;border-color:#238636}}
.stat-card.red{{background:#2e0a0a;border-color:#f85149}}
.stat-card.blue{{background:#0a1f3a;border-color:#1f6feb}}
.stat-card.orange{{background:#2e1a0a;border-color:#db6d28}}
.stat-card.purple{{background:#1a0a2e;border-color:#8957e5}}
.stat-card.gray{{background:#161b22;border-color:#30363d}}
.stat-card.yellow{{background:#2e2a0a;border-color:#e3b341}}
.stat-card.teal{{background:#0a2e2e;border-color:#39d353}}
.stat-value{{font-size:36px;font-weight:700;margin-bottom:8px}}
.stat-label{{font-size:13px;color:#8b949e}}
.section{{background:#161b22;border:1px solid #30363d;border-radius:10px;padding:20px;margin-bottom:20px}}
.section h2{{margin:0 0 16px;font-size:18px;color:#79c0ff}}
.data-table{{width:100%;border-collapse:collapse;font-size:13px}}
.data-table th{{background:#21262d;padding:10px 12px;text-align:right;color:#8b949e;border-bottom:1px solid #30363d;font-weight:600}}
.data-table td{{padding:8px 12px;border-bottom:1px solid #21262d}}
.row-odd{{background:#0d1117}}.row-even{{background:#161b22}}
.row-green td{{color:#56d364}}.row-red td{{color:#f85149}}.row-yellow td{{color:#e3b341}}.row-gray td{{color:#6e7681}}
.result-approved{{color:#56d364}}.result-declined{{color:#f85149}}.result-error{{color:#f85149}}.result-warning{{color:#e3b341}}
.error-list{{list-style:none;padding:0;display:flex;flex-wrap:wrap;gap:8px}}
.error-item{{background:#2e1a0a;color:#ffa657;padding:4px 12px;border-radius:20px;border:1px solid #db6d28;font-size:13px}}
.footer{{text-align:center;padding:20px;color:#6e7681;font-size:12px;border-top:1px solid #21262d;margin-top:32px}}
.noc-header{{background:linear-gradient(135deg,#1a0a2e,#0d1117)}}
</style></head><body>";

        private string HtmlFooter() => $@"
<div class='footer'>
    <p>EJLive Enterprise v{AppConstants.AppVersion} — {AppConstants.Copyright}</p>
    <p>تم التصدير في: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
</div>
</body></html>";
   
    /// <summary>
    /// محرك تصدير التقارير - CSV و PDF و HTML
    /// شاشة مخصصة لتصدير سجلات أجهزة الصراف الآلي وسجل مزامنة EJ
    /// والتنبيهات والمقاييس التشغيلية
    /// </summary>
    public class ReportExportEngine
    {
        #region Events
        public event Action<string> OnLog;
        public event Action<string> OnExportCompleted; // file path
        public event Action<Exception> OnError;
        #endregion

        #region Fields
        private readonly string _outputBasePath;
        #endregion

        #region Constructor
        public ReportExportEngine(string outputBasePath)
        {
            _outputBasePath = outputBasePath;
            if (!Directory.Exists(_outputBasePath))
                Directory.CreateDirectory(_outputBasePath);
        }
        #endregion

        #region CSV Export
        /// <summary>
        /// تصدير العمليات إلى CSV
        /// </summary>
        public string ExportTransactionsToCSV(List<ATMTransaction> transactions, ExportSettings settings = null)
        {
            settings = settings ?? new ExportSettings { Format = ExportFormat.CSV };
            string fileName = $"Transactions_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string filePath = Path.Combine(settings.OutputPath ?? _outputBasePath, fileName);

            try
            {
                var sb = new StringBuilder();
                if (settings.IncludeHeaders)
                {
                    sb.AppendLine("TransactionID,ATM_ID,Timestamp,Type,Status,Amount,Currency,CardNumber,ResponseCode,ErrorCode,Duration");
                }

                foreach (var tx in transactions)
                {
                    if (!string.IsNullOrEmpty(settings.FilterATM) && tx.ATM_ID != settings.FilterATM) continue;
                    if (settings.FilterFrom.HasValue && tx.Timestamp < settings.FilterFrom.Value) continue;
                    if (settings.FilterTo.HasValue && tx.Timestamp > settings.FilterTo.Value) continue;

                    sb.AppendLine($"{tx.TransactionID},{tx.ATM_ID},{tx.Timestamp.ToString(settings.DateFormat)}," +
                                  $"{tx.Type},{tx.Status},{tx.Amount},{tx.Currency}," +
                                  $"{tx.CardNumber},{tx.ResponseCode},{tx.ErrorCode},{tx.DurationSeconds}");
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                OnLog?.Invoke($"[Export] CSV exported: {filePath} ({transactions.Count} records)");
                OnExportCompleted?.Invoke(filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return null;
            }
        }

        /// <summary>
        /// تصدير التنبيهات إلى CSV
        /// </summary>
        public string ExportAlertsToCSV(List<SystemAlert> alerts, string outputPath = null)
        {
            string fileName = $"Alerts_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string filePath = Path.Combine(outputPath ?? _outputBasePath, fileName);

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("AlertID,Timestamp,Severity,Source,Message,Acknowledged,AcknowledgedBy");
                foreach (var alert in alerts)
                {
                    sb.AppendLine($"{alert.AlertID},{alert.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                                  $"{alert.Severity},{alert.Source},\"{alert.Message.Replace("\"", "\"\"")}\"," +
                                  $"{alert.IsAcknowledged},{alert.AcknowledgedBy}");
                }
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                OnLog?.Invoke($"[Export] Alerts CSV: {filePath}");
                OnExportCompleted?.Invoke(filePath);
                return filePath;
            }
            catch (Exception ex) { OnError?.Invoke(ex); return null; }
        }

        /// <summary>
        /// تصدير سجل مزامنة EJ إلى CSV
        /// </summary>
        public string ExportSyncLogToCSV(List<SyncLogEntry> syncLog, string outputPath = null)
        {
            string fileName = $"SyncLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string filePath = Path.Combine(outputPath ?? _outputBasePath, fileName);

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("ATM_ID,Timestamp,FileName,FileSize,Status,Duration,Checksum");
                foreach (var entry in syncLog)
                {
                    sb.AppendLine($"{entry.ATM_ID},{entry.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                                  $"{entry.FileName},{entry.FileSize},{entry.Status},{entry.DurationMs},{entry.Checksum}");
                }
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                OnLog?.Invoke($"[Export] Sync Log CSV: {filePath}");
                OnExportCompleted?.Invoke(filePath);
                return filePath;
            }
            catch (Exception ex) { OnError?.Invoke(ex); return null; }
        }
        #endregion

        #region HTML Export
        /// <summary>
        /// تصدير تقرير تحليل شامل إلى HTML
        /// </summary>
        public string ExportAnalysisReportToHTML(TransactionAnalysisReport report, string outputPath = null)
        {
            string fileName = $"Report_{report.ATM_ID}_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            string filePath = Path.Combine(outputPath ?? _outputBasePath, fileName);

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("<!DOCTYPE html>");
                sb.AppendLine("<html dir='rtl' lang='ar'>");
                sb.AppendLine("<head><meta charset='UTF-8'>");
                sb.AppendLine("<title>EJLive - Transaction Analysis Report</title>");
                sb.AppendLine("<style>");
                sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; background: #f5f5f5; }");
                sb.AppendLine(".header { background: #1a237e; color: white; padding: 20px; border-radius: 8px; margin-bottom: 20px; }");
                sb.AppendLine(".card { background: white; border-radius: 8px; padding: 15px; margin-bottom: 15px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
                sb.AppendLine(".stats { display: flex; gap: 15px; flex-wrap: wrap; }");
                sb.AppendLine(".stat-box { flex: 1; min-width: 150px; padding: 15px; border-radius: 8px; text-align: center; }");
                sb.AppendLine(".stat-box.green { background: #e8f5e9; border: 2px solid #4caf50; }");
                sb.AppendLine(".stat-box.red { background: #ffebee; border: 2px solid #f44336; }");
                sb.AppendLine(".stat-box.blue { background: #e3f2fd; border: 2px solid #2196f3; }");
                sb.AppendLine(".stat-box.orange { background: #fff3e0; border: 2px solid #ff9800; }");
                sb.AppendLine(".stat-value { font-size: 28px; font-weight: bold; }");
                sb.AppendLine(".stat-label { font-size: 12px; color: #666; margin-top: 5px; }");
                sb.AppendLine("table { width: 100%; border-collapse: collapse; }");
                sb.AppendLine("th, td { padding: 8px 12px; border: 1px solid #ddd; text-align: right; }");
                sb.AppendLine("th { background: #1a237e; color: white; }");
                sb.AppendLine("tr:nth-child(even) { background: #f9f9f9; }");
                sb.AppendLine(".success { color: #4caf50; font-weight: bold; }");
                sb.AppendLine(".failed { color: #f44336; font-weight: bold; }");
                sb.AppendLine("</style></head><body>");

                // Header
                sb.AppendLine("<div class='header'>");
                sb.AppendLine($"<h1>EJLive - تقرير تحليل العمليات</h1>");
                sb.AppendLine($"<p>الصراف: {report.ATM_ID} | الفترة: {report.FromDate:yyyy-MM-dd} إلى {report.ToDate:yyyy-MM-dd} | تاريخ التقرير: {report.GeneratedAt:yyyy-MM-dd HH:mm}</p>");
                sb.AppendLine("</div>");

                // Statistics
                sb.AppendLine("<div class='card'><h2>الإحصائيات</h2><div class='stats'>");
                sb.AppendLine($"<div class='stat-box blue'><div class='stat-value'>{report.TotalTransactions}</div><div class='stat-label'>إجمالي العمليات</div></div>");
                sb.AppendLine($"<div class='stat-box green'><div class='stat-value'>{report.SuccessfulTransactions}</div><div class='stat-label'>ناجحة</div></div>");
                sb.AppendLine($"<div class='stat-box red'><div class='stat-value'>{report.FailedTransactions}</div><div class='stat-label'>فاشلة</div></div>");
                sb.AppendLine($"<div class='stat-box orange'><div class='stat-value'>{report.TotalAmountDispensed:N0}</div><div class='stat-label'>المبلغ المصروف</div></div>");
                sb.AppendLine("</div></div>");

                // Success Rate
                sb.AppendLine("<div class='card'>");
                sb.AppendLine($"<h2>معدل النجاح: {report.SuccessRate:F1}%</h2>");
                sb.AppendLine($"<p>متوسط العمليات في الساعة: {report.AvgTransactionsPerHour:F1}</p>");
                sb.AppendLine($"<p>البطاقات المحتجزة: {report.RetainedCardsCount}</p>");
                sb.AppendLine("</div>");

                // Transactions by Type
                if (report.TransactionsByType.Count > 0)
                {
                    sb.AppendLine("<div class='card'><h2>العمليات حسب النوع</h2><table>");
                    sb.AppendLine("<tr><th>النوع</th><th>العدد</th><th>النسبة</th></tr>");
                    foreach (var kvp in report.TransactionsByType.OrderByDescending(k => k.Value))
                    {
                        double pct = report.TotalTransactions > 0 ? (double)kvp.Value / report.TotalTransactions * 100 : 0;
                        sb.AppendLine($"<tr><td>{GetTransactionTypeName(kvp.Key)}</td><td>{kvp.Value}</td><td>{pct:F1}%</td></tr>");
                    }
                    sb.AppendLine("</table></div>");
                }

                // Transactions by Hour
                sb.AppendLine("<div class='card'><h2>العمليات حسب الساعة</h2><table>");
                sb.AppendLine("<tr><th>الساعة</th><th>العدد</th></tr>");
                foreach (var kvp in report.TransactionsByHour.Where(k => k.Value > 0).OrderBy(k => k.Key))
                {
                    sb.AppendLine($"<tr><td>{kvp.Key:00}:00</td><td>{kvp.Value}</td></tr>");
                }
                sb.AppendLine("</table></div>");

                // Recent Transactions
                sb.AppendLine("<div class='card'><h2>آخر العمليات</h2><table>");
                sb.AppendLine("<tr><th>الوقت</th><th>النوع</th><th>المبلغ</th><th>الحالة</th><th>البطاقة</th></tr>");
                foreach (var tx in report.Transactions.OrderByDescending(t => t.Timestamp).Take(50))
                {
                    string statusClass = tx.IsSuccessful ? "success" : "failed";
                    sb.AppendLine($"<tr><td>{tx.Timestamp:HH:mm:ss}</td><td>{GetTransactionTypeName(tx.Type)}</td>" +
                                  $"<td>{tx.Amount:N0}</td><td class='{statusClass}'>{tx.Status}</td><td>{tx.CardNumber}</td></tr>");
                }
                sb.AppendLine("</table></div>");

                // Errors
                if (report.Errors.Count > 0)
                {
                    sb.AppendLine("<div class='card'><h2>الأخطاء</h2><table>");
                    sb.AppendLine("<tr><th>الوقت</th><th>الكود</th><th>الخطورة</th><th>الوصف</th></tr>");
                    foreach (var err in report.Errors.OrderByDescending(e => e.Timestamp).Take(20))
                    {
                        sb.AppendLine($"<tr><td>{err.Timestamp:HH:mm:ss}</td><td>{err.ErrorCode}</td><td>{err.Severity}</td><td>{err.Description}</td></tr>");
                    }
                    sb.AppendLine("</table></div>");
                }

                sb.AppendLine("<div class='card' style='text-align:center; color:#666;'>");
                sb.AppendLine($"<p>EJLive Enterprise v3.3.0 - Generated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
                sb.AppendLine("</div></body></html>");

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                OnLog?.Invoke($"[Export] HTML Report: {filePath}");
                OnExportCompleted?.Invoke(filePath);
                return filePath;
            }
            catch (Exception ex) { OnError?.Invoke(ex); return null; }
        }

        /// <summary>
        /// تصدير لوحة المراقبة إلى HTML
        /// </summary>
        public string ExportDashboardToHTML(List<ATMDetailedStatus> statuses, SystemHealthMetrics metrics, string outputPath = null)
        {
            string fileName = $"Dashboard_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            string filePath = Path.Combine(outputPath ?? _outputBasePath, fileName);

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("<!DOCTYPE html><html dir='rtl' lang='ar'><head><meta charset='UTF-8'>");
                sb.AppendLine("<title>EJLive Dashboard Export</title>");
                sb.AppendLine("<style>body{font-family:'Segoe UI',Arial;margin:20px;background:#1a1a2e;color:white;}");
                sb.AppendLine(".grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(250px,1fr));gap:15px;}");
                sb.AppendLine(".card{background:#16213e;border-radius:10px;padding:15px;border-left:4px solid #0f3460;}");
                sb.AppendLine(".card.green{border-left-color:#4caf50;} .card.yellow{border-left-color:#ffc107;}");
                sb.AppendLine(".card.red{border-left-color:#f44336;} .card.gray{border-left-color:#9e9e9e;}");
                sb.AppendLine("h1{color:#e94560;} h3{margin:0 0 10px;} .info{color:#aaa;font-size:12px;}</style></head><body>");
                sb.AppendLine($"<h1>EJLive Dashboard - {DateTime.Now:yyyy-MM-dd HH:mm}</h1>");

                // Health Metrics
                sb.AppendLine($"<p>صحة النظام: <strong>{metrics.OverallHealth}</strong> | ");
                sb.AppendLine($"CPU: {metrics.CPUUsage:F0}% | RAM: {metrics.MemoryUsage:F0}% | ");
                sb.AppendLine($"اتصالات: {metrics.ActiveConnections} | مزامنة: {metrics.SyncSuccessRate:F0}%</p>");

                // ATM Cards
                sb.AppendLine("<div class='grid'>");
                foreach (var status in statuses)
                {
                    string cardClass = status.OperationalState == ATMOperationalState.InService ? "green" :
                                       status.OperationalState == ATMOperationalState.SupervisorMode ? "yellow" :
                                       status.OperationalState == ATMOperationalState.OutOfService ? "red" : "gray";
                    sb.AppendLine($"<div class='card {cardClass}'>");
                    sb.AppendLine($"<h3>{status.ATM_ID}</h3>");
                    sb.AppendLine($"<p>الحالة: {GetStateName(status.OperationalState)}</p>");
                    sb.AppendLine($"<p>آخر صرف: {status.LastCashWithdrawal:HH:mm:ss}</p>");
                    sb.AppendLine($"<p>النقود: {status.CashInfo.TotalRemaining:N0}</p>");
                    sb.AppendLine($"<p class='info'>آخر بيانات: {status.LastDataReceived:HH:mm:ss}</p>");
                    sb.AppendLine("</div>");
                }
                sb.AppendLine("</div></body></html>");

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                OnLog?.Invoke($"[Export] Dashboard HTML: {filePath}");
                OnExportCompleted?.Invoke(filePath);
                return filePath;
            }
            catch (Exception ex) { OnError?.Invoke(ex); return null; }
        }
        #endregion

        #region Helpers
        private string GetTransactionTypeName(TransactionType type)
        {
            switch (type)
            {
                case TransactionType.CashWithdrawal: return "سحب نقدي";
                case TransactionType.BalanceInquiry: return "استعلام رصيد";
                case TransactionType.MiniStatement: return "كشف مصغر";
                case TransactionType.PINChange: return "تغيير رقم سري";
                case TransactionType.FundTransfer: return "تحويل";
                case TransactionType.BillPayment: return "دفع فاتورة";
                case TransactionType.CardRetained: return "بطاقة محتجزة";
                case TransactionType.SupervisorMode: return "وضع المشرف";
                case TransactionType.CashDeposit: return "إيداع";
                default: return "غير معروف";
            }
        }

        private string GetStateName(ATMOperationalState state)
        {
            switch (state)
            {
                case ATMOperationalState.InService: return "في الخدمة";
                case ATMOperationalState.OutOfService: return "خارج الخدمة";
                case ATMOperationalState.SupervisorMode: return "وضع المشرف";
                case ATMOperationalState.Offline: return "غير متصل";
                case ATMOperationalState.Maintenance: return "صيانة";
                default: return "غير معروف";
            }
        }
        #endregion
    }

    /// <summary>
    /// سجل مزامنة
    /// </summary>
    public class SyncLogEntry
    {
        public string ATM_ID { get; set; }
        public DateTime Timestamp { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string Status { get; set; }
        public int DurationMs { get; set; }
        public string Checksum { get; set; }
    }
}
