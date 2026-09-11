using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{
    /// <summary>
        /// تقرير التنبيهات المجمّع لفترة زمنية.
        /// </summary>
        public class AlertReport
        {
            public DateTime From { get; set; }
            public DateTime To { get; set; }
            public int TotalAlerts { get; set; }
            public int CriticalAlerts { get; set; }
            public int WarningAlerts { get; set; }
            public int InfoAlerts { get; set; }
            public List<AlertPayload> Alerts { get; set; } = new List<AlertPayload>();
            public string Summary => $"\U0001f6a8{CriticalAlerts} \u26a0\ufe0f{WarningAlerts} \u2139\ufe0f{InfoAlerts} \u2014 \u0625\u062c\u0645\u0627\u0644\u064a: {TotalAlerts}";
        }
    public partial class AlertReport
        {
            public DateTime From { get; set; }
    
    
            public DateTime To { get; set; }
    
    
            public int TotalAlerts { get; set; }
    
    
            public int CriticalAlerts { get; set; }
    
    
            public int WarningAlerts { get; set; }
    
    
            public int InfoAlerts { get; set; }
    
    
            public List<AlertPayload> Alerts { get; set; } = new List<AlertPayload>();
    
    
            public string Summary => $"\U0001f6a8{CriticalAlerts} \u26a0\ufe0f{WarningAlerts} \u2139\ufe0f{InfoAlerts} \u2014 \u0625\u062c\u0645\u0627\u0644\u064a: {TotalAlerts}";
    
    
        }
    /// <summary>
        /// إحصائيات الصراف الشاملة للتقارير والتصدير.
        /// </summary>
        public class ATMStatsSummary
        {
            public string ATMId { get; set; }
            public string ATMName { get; set; }
            public string ATMType { get; set; }
            public DateTime PeriodFrom { get; set; }
            public DateTime PeriodTo { get; set; }
            public int TotalTransactions { get; set; }
            public int ApprovedTransactions { get; set; }
            public int DeclinedTransactions { get; set; }
            public int CardsCaptured { get; set; }
            public int BalanceInquiries { get; set; }
            public long TotalCashDispensed { get; set; }
            public double SuccessRate => TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100 : 0;
            public int PowerResets { get; set; }
            public int SupervisorEntries { get; set; }
            public int CashErrors { get; set; }
            public int MediaErrors { get; set; }
            public List<string> ErrorCodesFound { get; set; } = new List<string>();
            public int PeakHour { get; set; }
            public int PeakDayOfWeek { get; set; }
            public long TotalJournalBytesReceived { get; set; }
            public int TotalJournalFiles { get; set; }
            public double SyncSuccessRate { get; set; } = 100.0;
            public double UptimePercent { get; set; } = 100.0;
            public string CashDisplay =>
                TotalCashDispensed >= 1_000_000 ? $"{TotalCashDispensed / 1_000_000.0:F2} M" :
                TotalCashDispensed >= 1_000 ? $"{TotalCashDispensed / 1_000.0:F1} K" :
                TotalCashDispensed.ToString("N0");
        }
    public partial class ATMStatsSummary
        {
            public string ATMId { get; set; }
    
    
            public string ATMName { get; set; }
    
    
            public string ATMType { get; set; }
    
    
            public DateTime PeriodFrom { get; set; }
    
    
            public DateTime PeriodTo { get; set; }
    
    
            public int TotalTransactions { get; set; }
    
    
            public int ApprovedTransactions { get; set; }
    
    
            public int DeclinedTransactions { get; set; }
    
    
            public int CardsCaptured { get; set; }
    
    
            public int BalanceInquiries { get; set; }
    
    
            public long TotalCashDispensed { get; set; }
    
    
            public double SuccessRate => TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100 : 0;
    
    
            public int PowerResets { get; set; }
    
    
            public int SupervisorEntries { get; set; }
    
    
            public int CashErrors { get; set; }
    
    
            public int MediaErrors { get; set; }
    
    
            public List<string> ErrorCodesFound { get; set; } = new List<string>();
    
    
            public int PeakHour { get; set; }
    
    
            public int PeakDayOfWeek { get; set; }
    
    
            public long TotalJournalBytesReceived { get; set; }
    
    
            public int TotalJournalFiles { get; set; }
    
    
            public double SyncSuccessRate { get; set; } = 100.0;
    
    
            public double UptimePercent { get; set; } = 100.0;
    
    
            public string CashDisplay =>
                TotalCashDispensed >= 1_000_000 ? $"{TotalCashDispensed / 1_000_000.0:F2} M" :
                TotalCashDispensed >= 1_000 ? $"{TotalCashDispensed / 1_000.0:F1} K" :
                TotalCashDispensed.ToString("N0");
    
    
        }
    /// <summary>
        /// تقرير المزامنة اليومي.
        /// </summary>
        public class DailySyncReport
        {
            public DateTime ReportDate { get; set; }
            public string ATMId { get; set; }
            public int FilesReceived { get; set; }
            public int FilesFailed { get; set; }
            public long BytesReceived { get; set; }
            public double AverageSpeedKBs { get; set; }
            public TimeSpan TotalSyncTime { get; set; }
            public int ReconnectCount { get; set; }
            public double UptimePercent { get; set; }
            public string Notes { get; set; }
        }
    public partial class DailySyncReport
        {
            public DateTime ReportDate { get; set; }
    
    
            public string ATMId { get; set; }
    
    
            public int FilesReceived { get; set; }
    
    
            public int FilesFailed { get; set; }
    
    
            public long BytesReceived { get; set; }
    
    
            public double AverageSpeedKBs { get; set; }
    
    
            public TimeSpan TotalSyncTime { get; set; }
    
    
            public int ReconnectCount { get; set; }
    
    
            public double UptimePercent { get; set; }
    
    
            public string Notes { get; set; }
    
    
        }
    /// <summary>
        /// حدث عملية حية يُستخدم في لوحة المراقبة المباشرة.
        /// </summary>
        public class LiveTransactionEvent
        {
            public string ATMId { get; set; }
            public TxType TxType { get; set; }
            public TxResult Result { get; set; }
            public long Amount { get; set; }
            public string ErrorCode { get; set; }
            public bool CardCaptured { get; set; }
            public string RawLine { get; set; }
            public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
            public string Icon => Result switch
            {
                TxResult.Approved => "\u2705",
                TxResult.Declined => "\u274c",
                TxResult.Error => "\U0001f6a8",
                TxResult.Warning => "\u26a0\ufe0f",
                _ => "\u2139\ufe0f"
            };
            public string DisplayLabel => $"{Icon} {TxType} {(Amount > 0 ? Amount.ToString("N0") : "")} {ErrorCode ?? ""}".Trim();
        }
    public partial class LiveTransactionEvent
        {
            public string ATMId { get; set; }
    
    
            public TxType TxType { get; set; }
    
    
            public TxResult Result { get; set; }
    
    
            public long Amount { get; set; }
    
    
            public string ErrorCode { get; set; }
    
    
            public bool CardCaptured { get; set; }
    
    
            public string RawLine { get; set; }
    
    
            public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    
    
            public string Icon => Result switch
            {
                TxResult.Approved => "\u2705",
                TxResult.Declined => "\u274c",
                TxResult.Error => "\U0001f6a8",
                TxResult.Warning => "\u26a0\ufe0f",
                _ => "\u2139\ufe0f"
            };
    
    
            public string DisplayLabel => $"{Icon} {TxType} {(Amount > 0 ? Amount.ToString("N0") : "")} {ErrorCode ?? ""}".Trim();
    
    
        }
    /// <summary>
        /// مستخدم النظام مع صلاحياته.
        /// </summary>
        public class SystemUser
        {
            public string UserId { get; set; } = Guid.NewGuid().ToString("N");
            public string Username { get; set; }
            public string PasswordHash { get; set; }
            public string Role { get; set; } = AppConstants.ROLE_OBSERVER;
            public string FullName { get; set; }
            public string Email { get; set; }
            public bool IsActive { get; set; } = true;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? LastLoginAt { get; set; }
            public bool CanManageUsers => Role == AppConstants.ROLE_ADMIN;
            public bool CanSendCommands => Role == AppConstants.ROLE_ADMIN || Role == AppConstants.ROLE_SUPPORT;
            public bool CanExportReports => Role != AppConstants.ROLE_OBSERVER;
            public bool CanViewArchive => true;
            public bool CanViewGhostView => Role == AppConstants.ROLE_ADMIN || Role == AppConstants.ROLE_SUPPORT;
        }
    public partial class SystemUser
        {
            public string UserId { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string Username { get; set; }
    
    
            public string PasswordHash { get; set; }
    
    
            public string Role { get; set; } = AppConstants.ROLE_OBSERVER;
    
    
            public string FullName { get; set; }
    
    
            public string Email { get; set; }
    
    
            public bool IsActive { get; set; } = true;
    
    
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    
            public DateTime? LastLoginAt { get; set; }
    
    
            public bool CanManageUsers => Role == AppConstants.ROLE_ADMIN;
    
    
            public bool CanSendCommands => Role == AppConstants.ROLE_ADMIN || Role == AppConstants.ROLE_SUPPORT;
    
    
            public bool CanExportReports => Role != AppConstants.ROLE_OBSERVER;
    
    
            public bool CanViewArchive => true;
    
    
            public bool CanViewGhostView => Role == AppConstants.ROLE_ADMIN || Role == AppConstants.ROLE_SUPPORT;
    
    
        }

    // Class: AlertReport (from 3 sources)
        public partial class AlertReport
        {
            // --- Properties ---
                    public DateTime From { get; set; }
    
                    public DateTime To { get; set; }
    
                    public int TotalAlerts { get; set; }
    
                    public int CriticalAlerts { get; set; }
    
                    public int WarningAlerts { get; set; }
    
                    public int InfoAlerts { get; set; }
    
                    public List<AlertPayload> Alerts { get; set; } = new List<AlertPayload>();
    
                    public string Summary => $"\U0001f6a8{CriticalAlerts} \u26a0\ufe0f{WarningAlerts} \u2139\ufe0f{InfoAlerts} \u2014 \u0625\u062c\u0645\u0627\u0644\u064a: {TotalAlerts}";
    
    
        }
    // Class: ATMStatsSummary (from 3 sources)
        public partial class ATMStatsSummary
        {
            // --- Properties ---
                    public string ATMId { get; set; }
    
                    public string ATMName { get; set; }
    
                    public string ATMType { get; set; }
    
                    public DateTime PeriodFrom { get; set; }
    
                    public DateTime PeriodTo { get; set; }
    
                    public int TotalTransactions { get; set; }
    
                    public int ApprovedTransactions { get; set; }
    
                    public int DeclinedTransactions { get; set; }
    
                    public int CardsCaptured { get; set; }
    
                    public int BalanceInquiries { get; set; }
    
                    public long TotalCashDispensed { get; set; }
    
                    public double SuccessRate => TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100 : 0;
    
                    public int PowerResets { get; set; }
    
                    public int SupervisorEntries { get; set; }
    
                    public int CashErrors { get; set; }
    
                    public int MediaErrors { get; set; }
    
                    public List<string> ErrorCodesFound { get; set; } = new List<string>();
    
                    public int PeakHour { get; set; }
    
                    public int PeakDayOfWeek { get; set; }
    
                    public long TotalJournalBytesReceived { get; set; }
    
                    public int TotalJournalFiles { get; set; }
    
                    public double SyncSuccessRate { get; set; } = 100.0;
    
                    public double UptimePercent { get; set; } = 100.0;
    
                    public string CashDisplay =>
                        TotalCashDispensed >= 1_000_000 ? $"{TotalCashDispensed / 1_000_000.0:F2} M" :
                        TotalCashDispensed >= 1_000 ? $"{TotalCashDispensed / 1_000.0:F1} K" :
                        TotalCashDispensed.ToString("N0");
    
    
        }
    // Class: DailySyncReport (from 3 sources)
        public partial class DailySyncReport
        {
            // --- Properties ---
                    public DateTime ReportDate { get; set; }
    
                    public string ATMId { get; set; }
    
                    public int FilesReceived { get; set; }
    
                    public int FilesFailed { get; set; }
    
                    public long BytesReceived { get; set; }
    
                    public double AverageSpeedKBs { get; set; }
    
                    public TimeSpan TotalSyncTime { get; set; }
    
                    public int ReconnectCount { get; set; }
    
                    public double UptimePercent { get; set; }
    
                    public string Notes { get; set; }
    
    
        }
    // Class: LiveTransactionEvent (from 3 sources)
        public partial class LiveTransactionEvent
        {
            // --- Properties ---
                    public string ATMId { get; set; }
    
                    public TxType TxType { get; set; }
    
                    public TxResult Result { get; set; }
    
                    public long Amount { get; set; }
    
                    public string ErrorCode { get; set; }
    
                    public bool CardCaptured { get; set; }
    
                    public string RawLine { get; set; }
    
                    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    
                    public string Icon => Result switch
                    {
                        TxResult.Approved => "\u2705",
                        TxResult.Declined => "\u274c",
                        TxResult.Error => "\U0001f6a8",
                        TxResult.Warning => "\u26a0\ufe0f",
                        _ => "\u2139\ufe0f"
                    };
    
                    public string DisplayLabel => $"{Icon} {TxType} {(Amount > 0 ? Amount.ToString("N0") : "")} {ErrorCode ?? ""}".Trim();
    
    
        }
    // Class: SystemUser (from 3 sources)
        public partial class SystemUser
        {
            // --- Properties ---
                    public string UserId { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string Username { get; set; }
    
                    public string PasswordHash { get; set; }
    
                    public string Role { get; set; } = AppConstants.ROLE_OBSERVER;
    
                    public string FullName { get; set; }
    
                    public string Email { get; set; }
    
                    public bool IsActive { get; set; } = true;
    
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
                    public DateTime? LastLoginAt { get; set; }
    
                    public bool CanManageUsers => Role == AppConstants.ROLE_ADMIN;
    
                    public bool CanSendCommands => Role == AppConstants.ROLE_ADMIN || Role == AppConstants.ROLE_SUPPORT;
    
                    public bool CanExportReports => Role != AppConstants.ROLE_OBSERVER;
    
                    public bool CanViewArchive => true;
    
                    public bool CanViewGhostView => Role == AppConstants.ROLE_ADMIN || Role == AppConstants.ROLE_SUPPORT;
    
    
        }
}
