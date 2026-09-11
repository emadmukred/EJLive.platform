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






    public enum ImageSyncStatus
    {
        Pending,
        Syncing,
        Completed,
        PartiallyCompleted,
        Failed
    }


    public sealed class CashStatus
    {
        public decimal TotalDispensed { get; set; }
        public decimal TotalRemaining { get; set; }
        public bool IsLow { get; set; }
        public bool IsEmpty { get; set; }
        public Dictionary<int, int> CassetteNotes { get; set; } = new Dictionary<int, int>();
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
