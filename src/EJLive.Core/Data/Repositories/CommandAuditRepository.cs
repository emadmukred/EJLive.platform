using System;
using System.Collections.Generic;
using System.Data;
using EJLive.Core.Services;

namespace EJLive.Core.Data.Repositories;

/// <summary>
/// SQLite implementation of <see cref="ICommandAuditRepository"/>. One row per policy
/// decision — allow and deny alike (SS-9: "every decision writes command_audit"). Rows carry
/// the operator, command id, args hash, outcome and latency so the audit viewer can answer
/// "who did what, on whose authority, how long did it take" without the queue table.
/// </summary>
public sealed class CommandAuditRepository : SqliteRepositoryBase, ICommandAuditRepository
{
    public CommandAuditRepository(DatabaseManager? database = null) : base(database)
    {
    }

    public void Append(CommandAuditRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        Execute(@"
INSERT OR REPLACE INTO command_audit
(audit_id, command_id, operator_id, action, details_json, timestamp_utc, atm_id, args_hash, outcome, latency_ms)
VALUES ($id,$cmd,$op,$act,$det,$ts,$atm,$args,$outcome,$latency)",
            P("$id", record.AuditId),
            P("$cmd", record.CommandId),
            P("$op", record.OperatorId),
            P("$act", record.Action),
            P("$det", record.DetailsJson),
            P("$ts", record.TimestampUtc.ToString("o")),
            P("$atm", record.AtmId),
            P("$args", record.ArgsHash),
            P("$outcome", string.IsNullOrWhiteSpace(record.Outcome) ? "Pending" : record.Outcome),
            P("$latency", record.LatencyMs));
    }

    public IReadOnlyList<CommandAuditRecord> QueryRecent(string? operatorId = null, int maxRows = 200)
    {
        var sql = @"
SELECT audit_id, command_id, operator_id, action, details_json, timestamp_utc,
       atm_id, args_hash, outcome, latency_ms
FROM command_audit";
        var parms = new List<Microsoft.Data.Sqlite.SqliteParameter>();
        if (!string.IsNullOrWhiteSpace(operatorId))
        {
            sql += " WHERE operator_id=$op";
            parms.Add(P("$op", operatorId));
        }
        sql += $" ORDER BY timestamp_utc DESC LIMIT {Math.Clamp(maxRows, 1, 5000)}";

        var table = Query(sql, parms.ToArray());
        var rows = new List<CommandAuditRecord>(table.Rows.Count);
        foreach (DataRow row in table.Rows)
        {
            rows.Add(new CommandAuditRecord(
                row["audit_id"]?.ToString() ?? string.Empty,
                row["command_id"]?.ToString() ?? string.Empty,
                row["operator_id"]?.ToString() ?? string.Empty,
                row["action"]?.ToString() ?? string.Empty,
                row["details_json"]?.ToString(),
                ParseStampRequired(row["timestamp_utc"], "command_audit.timestamp_utc"),
                row["atm_id"]?.ToString(),
                row["args_hash"]?.ToString(),
                row["outcome"]?.ToString() ?? "Pending",
                row["latency_ms"] is DBNull ? null : Convert.ToInt32(row["latency_ms"])));
        }
        return rows;
    }

    public int RecordOutcome(string commandId, string outcome, int latencyMs)
        => Execute(@"
UPDATE command_audit
SET outcome=$outcome, latency_ms=$latency
WHERE command_id=$cmd AND outcome IN ('Pending','Dispatched')",
            P("$cmd", commandId),
            P("$outcome", outcome),
            P("$latency", latencyMs));
}
