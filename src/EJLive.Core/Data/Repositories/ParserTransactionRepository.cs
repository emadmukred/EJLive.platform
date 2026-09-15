using System;
using System.Collections.Generic;
using System.Data;
using EJLive.Core.Services;

namespace EJLive.Core.Data.Repositories;

/// <summary>
/// SQLite implementation of <see cref="IParserTransactionRepository"/>. One batch per
/// archived file, keyed by the journal archive's <c>entry_id</c> (the <c>ingestion_id</c>
/// foreign key), so the Studio can replay exactly which transactions produced a terminal's
/// archive totals (SS-10.5 acceptance: "total per kind equals archive totals").
/// </summary>
public sealed class ParserTransactionRepository : SqliteRepositoryBase, IParserTransactionRepository
{
    public ParserTransactionRepository(DatabaseManager? database = null) : base(database)
    {
    }

    public void InsertBatch(string ingestionId, IReadOnlyList<ParserTransactionRecord> rows)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ingestionId);
        if (rows is null || rows.Count == 0)
            return;

        foreach (var row in rows)
        {
            Execute(@"
INSERT INTO parser_transactions
(ingestion_id, transaction_number, date_utc, atm_id, vendor, card_masked, amount, currency,
 stan, rrn, m_code, r_code, host_response, status, confidence, raw_start_line, raw_end_line, evidence)
VALUES
($ing,$num,$date,$atm,$vendor,$card,$amount,$cur,$stan,$rrn,$mc,$rc,$host,$status,$conf,$start,$end,$ev)",
                P("$ing", ingestionId),
                P("$num", row.TransactionNumber),
                P("$date", Stamp(row.DateUtc)),
                P("$atm", row.AtmId),
                P("$vendor", row.Vendor),
                P("$card", row.CardMasked),
                P("$amount", row.Amount),
                P("$cur", row.Currency),
                P("$stan", row.Stan),
                P("$rrn", row.Rrn),
                P("$mc", row.MCode),
                P("$rc", row.RCode),
                P("$host", row.HostResponse),
                P("$status", row.Status),
                P("$conf", row.Confidence),
                P("$start", row.RawStartLine),
                P("$end", row.RawEndLine),
                P("$ev", row.Evidence));
        }
    }

    public IReadOnlyList<ParserTransactionRecord> QueryByIngestion(string ingestionId, int maxRows = 5_000)
    {
        var table = Query(@"
SELECT transaction_number, date_utc, atm_id, vendor, card_masked, amount, currency,
       stan, rrn, m_code, r_code, host_response, status, confidence,
       raw_start_line, raw_end_line, evidence
FROM parser_transactions WHERE ingestion_id=$ing
ORDER BY transaction_number ASC LIMIT $max",
            P("$ing", ingestionId), P("$max", Math.Clamp(maxRows, 1, 50_000)));

        var rows = new List<ParserTransactionRecord>(table.Rows.Count);
        foreach (DataRow r in table.Rows)
        {
            rows.Add(new ParserTransactionRecord(
                r["transaction_number"] is DBNull ? null : Convert.ToInt32(r["transaction_number"]),
                ParseStamp(r["date_utc"]),
                r["atm_id"]?.ToString() ?? string.Empty,
                r["vendor"]?.ToString() ?? string.Empty,
                r["card_masked"]?.ToString(),
                r["amount"] is DBNull ? null : Convert.ToDecimal(r["amount"]),
                r["currency"]?.ToString(),
                r["stan"]?.ToString(),
                r["rrn"]?.ToString(),
                r["m_code"]?.ToString(),
                r["r_code"]?.ToString(),
                r["host_response"]?.ToString(),
                r["status"]?.ToString() ?? string.Empty,
                r["confidence"]?.ToString() ?? string.Empty,
                r["raw_start_line"] is DBNull ? null : Convert.ToInt32(r["raw_start_line"]),
                r["raw_end_line"] is DBNull ? null : Convert.ToInt32(r["raw_end_line"]),
                r["evidence"]?.ToString()));
        }
        return rows;
    }

    public long CountByIngestion(string ingestionId)
        => ScalarLong("SELECT COUNT(1) FROM parser_transactions WHERE ingestion_id=$ing", P("$ing", ingestionId));
}
