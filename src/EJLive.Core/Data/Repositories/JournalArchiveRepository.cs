using System;
using System.Collections.Generic;
using System.Data;
using EJLive.Core.Services;

namespace EJLive.Core.Data.Repositories;

/// <summary>
/// SQLite implementation of <see cref="IJournalArchiveRepository"/>. Idempotent on the
/// archive key <c>(atm_id, file_name, sha256_hash)</c> — a resend is a data outcome, not an
/// error and never a second row (SS-15 pattern #2).
/// </summary>
public sealed class JournalArchiveRepository : SqliteRepositoryBase, IJournalArchiveRepository
{
    public JournalArchiveRepository(DatabaseManager? database = null) : base(database)
    {
    }

    public bool TryInsert(JournalArchiveRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var existing = ScalarText(
            "SELECT entry_id FROM journal_archive WHERE atm_id=$atm AND file_name=$fn AND COALESCE(sha256_hash,'')=$sha",
            P("$atm", record.AtmId), P("$fn", record.FileName), P("$sha", record.Sha256Hash));

        if (!string.IsNullOrEmpty(existing))
            return false; // duplicate resend — already archived (SS-15 pattern #2)

        Execute(@"
INSERT OR IGNORE INTO journal_archive
(entry_id, atm_id, file_name, original_size, compressed_size, encrypted_size,
 is_encrypted, is_compressed, checksum, sha256_hash, transaction_count,
 archive_path, month_partition, received_at)
VALUES
($id,$atm,$fn,$os,$cs,$es,$enc,$comp,$ck,$sha,$tc,$ap,$mp,$ra)",
            P("$id", record.EntryId),
            P("$atm", record.AtmId),
            P("$fn", record.FileName),
            P("$os", record.OriginalSize),
            P("$cs", record.CompressedSize),
            P("$es", record.EncryptedSize),
            P("$enc", record.IsEncrypted ? 1 : 0),
            P("$comp", record.IsCompressed ? 1 : 0),
            P("$ck", record.Checksum),
            P("$sha", record.Sha256Hash),
            P("$tc", record.TransactionCount),
            P("$ap", record.ArchivePath),
            P("$mp", record.MonthPartition),
            P("$ra", record.ReceivedAtUtc.ToString("o")));
        return true;
    }

    public long CountFiles(string atmId, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        var (sql, parms) = Window("SELECT COUNT(1) FROM journal_archive WHERE atm_id=$atm", atmId, fromUtc, toUtc);
        return ScalarLong(sql, parms);
    }

    public long SumTransactions(string atmId, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        var (sql, parms) = Window(
            "SELECT COALESCE(SUM(transaction_count), 0) FROM journal_archive WHERE atm_id=$atm",
            atmId, fromUtc, toUtc);
        return ScalarLong(sql, parms);
    }

    public void UpdateTransactionCount(string entryId, int count)
        => Execute("UPDATE journal_archive SET transaction_count=$c WHERE entry_id=$id",
            P("$c", count), P("$id", entryId));

    public int PruneBefore(DateTime olderThanUtc)
        => Execute("DELETE FROM journal_archive WHERE received_at < $cut",
            P("$cut", olderThanUtc.ToUniversalTime().ToString("o")));

    private static (string Sql, Microsoft.Data.Sqlite.SqliteParameter[] Parms) Window(
        string head, string atmId, DateTime? fromUtc, DateTime? toUtc)
    {
        var sql = head;
        var parms = new List<Microsoft.Data.Sqlite.SqliteParameter> { P("$atm", atmId) };
        if (fromUtc.HasValue)
        {
            sql += " AND received_at>=$from";
            parms.Add(P("$from", fromUtc.Value.ToUniversalTime().ToString("o")));
        }
        if (toUtc.HasValue)
        {
            sql += " AND received_at<=$to";
            parms.Add(P("$to", toUtc.Value.ToUniversalTime().ToString("o")));
        }
        return (sql, parms.ToArray());
    }
}
