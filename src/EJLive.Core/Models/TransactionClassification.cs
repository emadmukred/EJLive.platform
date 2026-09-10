namespace EJLive.Core.Models;

/// <summary>
/// Classification of an EJ transaction based on forensic analysis of the electronic journal.
/// </summary>
public enum TransactionClassification
{
    /// <summary>Transaction completed successfully with cash dispensed and taken.</summary>
    Success,

    /// <summary>Transaction failed due to host decline, hardware fault, or other terminal-reported failure.</summary>
    Failed,

    /// <summary>Transaction exhibits anomalies warranting further investigation.</summary>
    Suspicious,

    /// <summary>A reversal was recorded for this transaction.</summary>
    Reversal,

    /// <summary>Cash was partially dispensed (not all requested notes delivered).</summary>
    PartialDispense,

    /// <summary>Transaction was approved by host but no cash was dispensed.</summary>
    ApprovedNoDispense,

    /// <summary>Cash became jammed during the dispense operation.</summary>
    CashJam,

    /// <summary>Notes were retracted by the device after presentation.</summary>
    Retract,

    /// <summary>The customer card was captured by the device.</summary>
    CardCaptured,

    /// <summary>Transaction was explicitly declined by the host/authorizer.</summary>
    HostDeclined,

    /// <summary>A hardware fault was detected during transaction processing.</summary>
    HardwareFault,

    /// <summary>Expected sequence number is missing from the journal stream.</summary>
    MissingSequence,

    /// <summary>A duplicate sequence number was detected in the journal.</summary>
    DuplicateSequence,

    /// <summary>Retrieval Reference Number (RRN) is missing or could not be extracted.</summary>
    MissingRRN,

    /// <summary>System Trace Audit Number (STAN) is missing or could not be extracted.</summary>
    MissingSTAN,

    /// <summary>Currency reported in journal does not match expected or host-reported currency.</summary>
    CurrencyMismatch,

    /// <summary>Amount reported in journal does not match expected or host-reported amount.</summary>
    AmountMismatch
}
