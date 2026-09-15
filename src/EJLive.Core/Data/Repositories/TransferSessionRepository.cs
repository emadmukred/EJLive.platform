using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text;
using EJLive.Core.Services;

namespace EJLive.Core.Data.Repositories;

/// <summary>
/// SQLite implementation of <see cref="ITransferSessionRepository"/> (SS-13 "resume": an
/// interrupted 64 MB transfer must come back with &lt; 5 % retransmit — which requires the
/// received-chunk bitmap to survive an agent restart, not just live in memory).
/// The bitmap is stored as the ascending comma-separated list of received chunk indices:
/// human-inspectable with a plain SQLite shell and trivially convertible both ways.
/// </summary>
public sealed class TransferSessionRepository : SqliteRepositoryBase, ITransferSessionRepository
{
    public TransferSessionRepository(DatabaseManager? database = null) : base(database)
    {
    }

    public void Save(TransferSessionRecord session)
    {
        ArgumentNullException.ThrowIfNull(session);
        Execute(@"
INSERT OR REPLACE INTO transfer_sessions
(transfer_id, atm_id, file_name, file_length, chunk_size, total_chunks,
 received_chunks, file_sha256, next_expected_offset, created_utc, completed_utc)
VALUES ($id,$atm,$fn,$len,$chunk,$total,$recv,$sha,$next,$created,$completed)",
            P("$id", session.TransferId),
            P("$atm", session.AtmId),
            P("$fn", session.FileName),
            P("$len", session.FileLength),
            P("$chunk", session.ChunkSize),
            P("$total", session.TotalChunks),
            P("$recv", session.ReceivedChunksCsv ?? string.Empty),
            P("$sha", session.FileSha256),
            P("$next", session.NextExpectedOffset),
            P("$created", session.CreatedUtc.ToString("o")),
            P("$completed", Stamp(session.CompletedUtc)));
    }

    public TransferSessionRecord? Load(string transferId)
    {
        var table = Query(@"
SELECT transfer_id, atm_id, file_name, file_length, chunk_size, total_chunks,
       received_chunks, file_sha256, next_expected_offset, created_utc, completed_utc
FROM transfer_sessions WHERE transfer_id=$id",
            P("$id", transferId));

        if (table.Rows.Count == 0)
            return null;

        var r = table.Rows[0];
        return new TransferSessionRecord(
            r["transfer_id"]?.ToString() ?? string.Empty,
            r["atm_id"]?.ToString() ?? string.Empty,
            r["file_name"]?.ToString() ?? string.Empty,
            Convert.ToInt64(r["file_length"]),
            Convert.ToInt32(r["chunk_size"]),
            Convert.ToInt32(r["total_chunks"]),
            r["received_chunks"]?.ToString() ?? string.Empty,
            r["file_sha256"]?.ToString(),
            Convert.ToInt64(r["next_expected_offset"]),
            ParseStamp(r["created_utc"]) ?? DateTime.MinValue,
            ParseStamp(r["completed_utc"]));
    }

    public void MarkCompleted(string transferId, DateTime completedUtc)
        => Execute("UPDATE transfer_sessions SET completed_utc=$done WHERE transfer_id=$id",
            P("$done", completedUtc.ToString("o")), P("$id", transferId));

    /// <summary>Decodes the persisted bitmap into a <see cref="BitArray"/> for the resume path.</summary>
    public static BitArray DecodeBitmap(string? csv, int totalChunks)
    {
        var bits = new BitArray(Math.Max(0, totalChunks));
        if (string.IsNullOrWhiteSpace(csv))
            return bits;
        foreach (var part in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index)
                && index >= 0 && index < bits.Length)
                bits[index] = true;
        }
        return bits;
    }

    /// <summary>Encodes a bitmap (or its set indices) into the persisted form.</summary>
    public static string EncodeBitmap(BitArray received)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < received.Length; i++)
        {
            if (received.Get(i))
            {
                if (builder.Length > 0)
                    builder.Append(',');
                builder.Append(i.ToString(CultureInfo.InvariantCulture));
            }
        }
        return builder.ToString();
    }
}
