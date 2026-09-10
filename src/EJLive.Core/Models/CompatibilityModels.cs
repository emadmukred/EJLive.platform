using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{
    public enum TxType
    {
        Unknown,
        Withdrawal,
        BalanceInquiry,
        Deposit
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
        CardRetained,
        SupervisorMode
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed
    }

    public enum ATMOperationalState
    {
        Unknown,
        InService,
        OutOfService,
        SupervisorMode
    }

    public enum GhostSessionStatus
    {
        Connecting,
        Active,
        Disconnected
    }

    public enum ImageSyncStatus
    {
        Pending,
        Syncing,
        Completed,
        PartiallyCompleted,
        Failed
    }

    public sealed class AlertPayload
    {
        public string Severity { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime RaisedAt { get; set; } = DateTime.UtcNow;
    }

    public sealed class CashStatus
    {
        public decimal TotalDispensed { get; set; }
        public decimal TotalRemaining { get; set; }
        public bool IsLow { get; set; }
        public bool IsEmpty { get; set; }
        public Dictionary<int, int> CassetteNotes { get; set; } = new Dictionary<int, int>();
    }

    public sealed class RetainedCard
    {
        public string CardNumber { get; set; }
        public string Reason { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public sealed class ATMError
    {
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public string Severity { get; set; }
        public string RawLine { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public sealed class ATMTransaction
    {
        public string TransactionID { get; set; }
        public TransactionType Type { get; set; } = TransactionType.Unknown;
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public decimal Amount { get; set; }
        public string CardNumber { get; set; }
        public string ResponseCode { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public string RawJournalText { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IsSuccessful => Status == TransactionStatus.Completed;
    }

    public sealed class ATMDetailedStatus
    {
        public ATMOperationalState OperationalState { get; set; } = ATMOperationalState.Unknown;
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public DateTime? LastCashWithdrawal { get; set; }
        public CashStatus CashInfo { get; set; } = new CashStatus();
        public List<RetainedCard> RetainedCards { get; set; } = new List<RetainedCard>();
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
        public Dictionary<TransactionType, int> TransactionsByType { get; set; } = new Dictionary<TransactionType, int>();
        public Dictionary<int, int> TransactionsByHour { get; set; } = new Dictionary<int, int>();
    }

    public sealed class GhostSession
    {
        public string SessionID { get; set; }
        public string ATM_ID { get; set; }
        public string OperatorName { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }
        public GhostSessionStatus Status { get; set; } = GhostSessionStatus.Connecting;
        public bool IsViewOnly { get; set; }
        public bool ATMUnaffected { get; set; }
        public bool NoLogout { get; set; }
        public bool ScreenNotLocked { get; set; }
        public List<string> ActivityLog { get; set; } = new List<string>();
    }

    public sealed class ImageSyncItem
    {
        public string ImageID { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public string Checksum { get; set; }
        public string TargetATMType { get; set; }
        public List<string> TargetATMs { get; set; } = new List<string>();
        public DateTime ScheduledTime { get; set; } = DateTime.Now;
        public ImageSyncStatus Status { get; set; } = ImageSyncStatus.Pending;
        public Dictionary<string, bool> DeliveryStatus { get; set; } = new Dictionary<string, bool>();
    }
}
