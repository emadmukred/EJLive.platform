using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{

    public enum TxResult
    {
        Unknown,
        Approved,
        Declined,
        Error,
        Warning
    }




    public enum GhostSessionStatus
    {
        Connecting,
        Active,
        Disconnected
    }


    public sealed class ATMError
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string ATM_ID { get; set; }
        public string ErrorCode { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; } = "Information";
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


    public sealed class AlertPayload
    {
        public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
        public string ATM_ID { get; set; }
        public string Severity { get; set; } = "Info";
        public string Message { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

}
