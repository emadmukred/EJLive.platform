using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{
    public enum TxType
    {
        Unknown,
        CashWithdrawal,
        BalanceInquiry,
        CardRetained,
        SupervisorMode,
        Error
    }

    public enum TxResult
    {
        Unknown,
        Approved,
        Declined,
        Error,
        Warning
    }

    public enum TransactionType
    {
        Unknown,
        CashWithdrawal,
        BalanceInquiry,
        MiniStatement,
        PINChange,
        FundTransfer,
        BillPayment,
        CardRetained,
        SupervisorMode,
        CashDeposit
    }

    public enum TransactionStatus
    {
        Unknown,
        Pending,
        InProgress,
        Completed,
        Failed,
        Reversed
    }

    public enum ATMOperationalState
    {
        Unknown,
        InService,
        OutOfService,
        SupervisorMode,
        Offline,
        Maintenance
    }

    public enum GhostSessionStatus
    {
        Connecting,
        Active,
        Disconnected
    }

    public sealed class ATMTransaction
    {
        public string TransactionID { get; set; }
        public string ATM_ID { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public TransactionType Type { get; set; } = TransactionType.Unknown;
        public TransactionStatus Status { get; set; } = TransactionStatus.Unknown;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public string CardNumber { get; set; }
        public string ResponseCode { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public string RawJournalText { get; set; }
        public int DurationSeconds { get; set; }
        public bool IsSuccessful => Status == TransactionStatus.Completed;
    }

    public sealed class ATMError
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string ATM_ID { get; set; }
        public string ErrorCode { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; } = "Information";
    }

    public sealed class RetainedCard
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string ATM_ID { get; set; }
        public string RetainCode { get; set; }
        public string RetainReason { get; set; }
        public string CardNumberMasked { get; set; }
    }

    public sealed class ATMCashInfo
    {
        public decimal TotalDispensed { get; set; }
        public decimal TotalRemaining { get; set; }
        public bool IsLow { get; set; }
        public bool IsEmpty { get; set; }
        public Dictionary<int, int> CassetteNotes { get; } = new Dictionary<int, int>();
    }

    public sealed class ATMDetailedStatus
    {
        public string ATM_ID { get; set; }
        public string ATMName { get; set; }
        public ATMOperationalState OperationalState { get; set; } = ATMOperationalState.Unknown;
        public ATMCashInfo CashInfo { get; } = new ATMCashInfo();
        public List<RetainedCard> RetainedCards { get; } = new List<RetainedCard>();
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public DateTime LastDataReceived { get; set; } = DateTime.Now;
        public DateTime? LastCashWithdrawal { get; set; }
    }

    public sealed class TransactionAnalysisReport
    {
        public string ReportID { get; set; }
        public string ATM_ID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public int TotalTransactions { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public decimal TotalAmountDispensed { get; set; }
        public int RetainedCardsCount { get; set; }
        public List<ATMTransaction> Transactions { get; set; } = new List<ATMTransaction>();
        public List<ATMError> Errors { get; set; } = new List<ATMError>();
        public Dictionary<TransactionType, int> TransactionsByType { get; } = new Dictionary<TransactionType, int>();
        public Dictionary<int, int> TransactionsByHour { get; } = new Dictionary<int, int>();

        public double SuccessRate => TotalTransactions > 0 ? (double)SuccessfulTransactions / TotalTransactions * 100.0 : 0.0;

        public double AvgTransactionsPerHour
        {
            get
            {
                var hours = Math.Max(1.0, (ToDate - FromDate).TotalHours);
                return TotalTransactions / hours;
            }
        }
    }

    public sealed class AlertPayload
    {
        public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
        public string ATM_ID { get; set; }
        public string Severity { get; set; } = "Info";
        public string Message { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public sealed class GhostSession
    {
        public string SessionID { get; set; }
        public string ATM_ID { get; set; }
        public string OperatorName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public GhostSessionStatus Status { get; set; } = GhostSessionStatus.Connecting;
        public bool IsViewOnly { get; set; }
        public bool ATMUnaffected { get; set; }
        public bool NoLogout { get; set; }
        public bool ScreenNotLocked { get; set; }
        public List<string> ActivityLog { get; } = new List<string>();
    }
}
