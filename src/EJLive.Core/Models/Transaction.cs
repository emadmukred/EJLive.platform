using EJLive.Core.Enums;

namespace EJLive.Core.Models
{
    public class FraudAlert
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string DeviceId { get; set; } = string.Empty;
        public FraudType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public double ConfidenceScore { get; set; } = 0;
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        public List<string> Evidence { get; set; } = new();
        public bool IsAcknowledged { get; set; } = false;
    }
    public partial class FraudAlert
        {
            public Guid Id { get; set; } = Guid.NewGuid();
    
    
            public string DeviceId { get; set; } = string.Empty;
    
    
            public FraudType Type { get; set; }
    
    
            public string Description { get; set; } = string.Empty;
    
    
            public double ConfidenceScore { get; set; } = 0;
    
    
            public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    
    
            public List<string> Evidence { get; set; } = new();
    
    
            public bool IsAcknowledged { get; set; } = false;
    
    
        }
    /// <summary>
    /// Represents a financial transaction processed at an ATM terminal.
    /// </summary>
    public class Transaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public DateTime TransactionTime { get; set; }
        public TransactionType Type { get; set; } = TransactionType.Unknown;
        public string RawData { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public string? CardNumber { get; set; }
        public string? AccountNumber { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Unknown;
        public string? ResponseCode { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsSuspicious { get; set; } = false;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime ParsedAt { get; set; } = DateTime.UtcNow;
    }
    public class Transaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public DateTime TransactionTime { get; set; }
        public TransactionType Type { get; set; } = TransactionType.Unknown;
        public string RawData { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public string? CardNumber { get; set; }
        public string? AccountNumber { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Unknown;
        public string? ResponseCode { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsSuspicious { get; set; } = false;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime ParsedAt { get; set; } = DateTime.UtcNow;
    }
    public partial class Transaction
        {
            public Guid Id { get; set; } = Guid.NewGuid();
    
    
            public string DeviceId { get; set; } = string.Empty;
    
    
            public string DeviceName { get; set; } = string.Empty;
    
    
            public DateTime TransactionTime { get; set; }
    
    
            public TransactionType Type { get; set; } = TransactionType.Unknown;
    
    
            public string RawData { get; set; } = string.Empty;
    
    
            public decimal? Amount { get; set; }
    
    
            public string? CardNumber { get; set; }
    
    
            public string? AccountNumber { get; set; }
    
    
            public TransactionStatus Status { get; set; } = TransactionStatus.Unknown;
    
    
            public string? ResponseCode { get; set; }
    
    
            public string? ErrorMessage { get; set; }
    
    
            public bool IsSuspicious { get; set; } = false;
    
    
            public double? Latitude { get; set; }
    
    
            public double? Longitude { get; set; }
    
    
            public DateTime ParsedAt { get; set; } = DateTime.UtcNow;
    
    
        }
    public class TransactionSummary
    {
        public string DeviceId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int TotalTransactions { get; set; } = 0;
        public int SuccessfulCount { get; set; } = 0;
        public int FailedCount { get; set; } = 0;
        public int SuspiciousCount { get; set; } = 0;
        public decimal TotalAmount { get; set; } = 0;
        public decimal TotalDispensed { get; set; } = 0;
        public Dictionary<string, int> ByType { get; set; } = new();
        public Dictionary<string, int> ByHour { get; set; } = new();
        public List<Transaction> TopAlerts { get; set; } = new();
    }
    public partial class TransactionSummary
        {
            public string DeviceId { get; set; } = string.Empty;
    
    
            public DateTime Date { get; set; }
    
    
            public int TotalTransactions { get; set; } = 0;
    
    
            public int SuccessfulCount { get; set; } = 0;
    
    
            public int FailedCount { get; set; } = 0;
    
    
            public int SuspiciousCount { get; set; } = 0;
    
    
            public decimal TotalAmount { get; set; } = 0;
    
    
            public decimal TotalDispensed { get; set; } = 0;
    
    
            public Dictionary<string, int> ByType { get; set; } = new();
    
    
            public Dictionary<string, int> ByHour { get; set; } = new();
    
    
            public List<Transaction> TopAlerts { get; set; } = new();
    
    
        }

    // Class: FraudAlert (from 3 sources)
        public partial class FraudAlert
        {
            // --- Properties ---
                public Guid Id { get; set; } = Guid.NewGuid();
    
                public string DeviceId { get; set; } = string.Empty;
    
                public FraudType Type { get; set; }
    
                public string Description { get; set; } = string.Empty;
    
                public double ConfidenceScore { get; set; } = 0;
    
                public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    
                public List<string> Evidence { get; set; } = new();
    
                public bool IsAcknowledged { get; set; } = false;
    
    
        }
    // Enum: FraudType (from 1 sources)
        public partial enum FraudType
        {
            // --- Constants & Fields ---
                SkimmingAttempt = 1,
    
                UnusualCashOut = 2,
    
                MultipleFailedPins = 3,
    
                BlackBoxAttack = 4,
    
                CardRetainedPattern = 5,
    
                Jackpotting = 6,
    
                CashTrapping = 7
    
    
        }
    // Class: Transaction (from 3 sources)
        public partial class Transaction
        {
            // --- Properties ---
            public Guid Id { get; set; } = Guid.NewGuid();
    
            public string DeviceId { get; set; } = string.Empty;
    
            public string DeviceName { get; set; } = string.Empty;
    
            public DateTime TransactionTime { get; set; }
    
            public TransactionType Type { get; set; } = TransactionType.Unknown;
    
            public string RawData { get; set; } = string.Empty;
    
            public decimal? Amount { get; set; }
    
            public string? CardNumber { get; set; }
    
            public string? AccountNumber { get; set; }
    
            public TransactionStatus Status { get; set; } = TransactionStatus.Unknown;
    
            public string? ResponseCode { get; set; }
    
            public string? ErrorMessage { get; set; }
    
            public bool IsSuspicious { get; set; } = false;
    
            public double? Latitude { get; set; }
    
            public double? Longitude { get; set; }
    
            public DateTime ParsedAt { get; set; } = DateTime.UtcNow;
    
    
        }
    // Enum: TransactionStatus (from 1 sources)
        public partial enum TransactionStatus
        {
            // --- Constants & Fields ---
                Success = 1,
    
                Failed = 2,
    
                Reversed = 3,
    
                Timeout = 4,
    
                Declined = 5,
    
                Unknown = 99
    
    
        }
    // Class: TransactionSummary (from 3 sources)
        public partial class TransactionSummary
        {
            // --- Properties ---
                public string DeviceId { get; set; } = string.Empty;
    
                public DateTime Date { get; set; }
    
                public int TotalTransactions { get; set; } = 0;
    
                public int SuccessfulCount { get; set; } = 0;
    
                public int FailedCount { get; set; } = 0;
    
                public int SuspiciousCount { get; set; } = 0;
    
                public decimal TotalAmount { get; set; } = 0;
    
                public decimal TotalDispensed { get; set; } = 0;
    
                public Dictionary<string, int> ByType { get; set; } = new();
    
                public Dictionary<string, int> ByHour { get; set; } = new();
    
                public List<Transaction> TopAlerts { get; set; } = new();
    
    
        }

    public enum FraudType
    {
        SkimmingAttempt = 1,
        UnusualCashOut = 2,
        MultipleFailedPins = 3,
        BlackBoxAttack = 4,
        CardRetainedPattern = 5,
        Jackpotting = 6,
        CashTrapping = 7
    }
    public partial enum FraudType
        {
            SkimmingAttempt = 1,
    
    
            UnusualCashOut = 2,
    
    
            MultipleFailedPins = 3,
    
    
            BlackBoxAttack = 4,
    
    
            CardRetainedPattern = 5,
    
    
            Jackpotting = 6,
    
    
            CashTrapping = 7
    
    
        }
    public enum TransactionStatus
    {
        Success = 1,
        Failed = 2,
        Reversed = 3,
        Timeout = 4,
        Declined = 5,
        Unknown = 99
    }
    public partial enum TransactionStatus
        {
            Success = 1,
    
    
            Failed = 2,
    
    
            Reversed = 3,
    
    
            Timeout = 4,
    
    
            Declined = 5,
    
    
            Unknown = 99
    
    
        }
}
