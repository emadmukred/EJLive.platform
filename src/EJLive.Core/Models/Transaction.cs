using EJLive.Core.Enums;

namespace EJLive.Core.Models
{
    /// <summary>
    /// Represents a financial transaction processed at an ATM terminal.
    /// </summary>
    /// <remarks>
    /// Wave 1 (SS-04 D-08): rewrote the file. The previous version had three to four copies
    /// of <c>Transaction</c> and <c>TransactionSummary</c> interleaved with provenance
    /// comments, plus two copies of <c>FraudType</c> (one real, one marked <c>partial</c> before <c>enum</c>
    /// which is not a C# construct). The bodies are identical, so the union collapses to
    /// the canonical declaration. The richer <c>FraudType</c> set carried by every copy
    /// is preserved as-is.
    /// </remarks>
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

    // TransactionStatus is defined in UnifiedModels.cs (the canonical owner for the
    // EJLive.Core.Models namespace). The richer six-member shape there is the
    // bonded-to-database column representation; this file carries the fraud and
    // transaction-summary carriers only.
}
