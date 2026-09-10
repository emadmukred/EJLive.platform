namespace EJLive.Core.Models;

/// <summary>
/// Represents a parsed Electronic Journal (EJ) transaction with forensic metadata.
/// </summary>
/// <param name="TransactionId">Unique identifier assigned to this parsed transaction.</param>
/// <param name="StartLine">Zero-based index of the first raw line belonging to this transaction.</param>
/// <param name="EndLine">Zero-based index of the last raw line belonging to this transaction (inclusive).</param>
/// <param name="ATM_ID">The ATM identifier extracted from context or headers.</param>
/// <param name="CardNumber">Masked card number (e.g., 123456******7890) extracted from the journal.</param>
/// <param name="AccountNumber">Masked account number extracted from the journal.</param>
/// <param name="Amount">The monetary amount associated with the transaction.</param>
/// <param name="Currency">The ISO currency code (e.g., USD, EUR) for the transaction amount.</param>
/// <param name="STAN">System Trace Audit Number used for host reconciliation.</param>
/// <param name="RRN">Retrieval Reference Number used for host reconciliation.</param>
/// <param name="Cassette1">Count or status for cassette 1, if available.</param>
/// <param name="Cassette2">Count or status for cassette 2, if available.</param>
/// <param name="Cassette3">Count or status for cassette 3, if available.</param>
/// <param name="Cassette4">Count or status for cassette 4, if available.</param>
/// <param name="MCode">Maintenance or message code extracted from terminal diagnostics.</param>
/// <param name="RCode">Response or reason code from host or terminal.</param>
/// <param name="RawLines">The complete set of unparsed lines forming this transaction block.</param>
/// <param name="Classification">Forensic classification derived from evidence analysis.</param>
/// <param name="Confidence">Confidence score (0.0 to 1.0) of the classification.</param>
/// <param name="Timestamp">The timestamp when the transaction was parsed.</param>
public sealed record EjTransaction(
    string TransactionId,
    int StartLine,
    int EndLine,
    string ATM_ID,
    string CardNumber,
    string AccountNumber,
    decimal? Amount,
    string Currency,
    string STAN,
    string RRN,
    int? Cassette1,
    int? Cassette2,
    int? Cassette3,
    int? Cassette4,
    string MCode,
    string RCode,
    List<string> RawLines,
    TransactionClassification Classification,
    double Confidence,
    DateTime Timestamp
);
