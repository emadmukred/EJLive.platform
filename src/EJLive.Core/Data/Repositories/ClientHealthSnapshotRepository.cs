using System;
using System.Collections.Generic;
using System.Data;
using EJLive.Core.Services;

namespace EJLive.Core.Data.Repositories;

/// <summary>
/// SQLite implementation of <see cref="IClientHealthSnapshotRepository"/>. The write is
/// append-only (one row per snapshot, minute-grained) so the fleet grid can replay the last
/// minutes of any terminal; reads use the group-by-latest pattern (no window function needed
/// on SQLite ≥ 3.39 semantics — this stays version-safe).
/// </summary>
public sealed class ClientHealthSnapshotRepository : SqliteRepositoryBase, IClientHealthSnapshotRepository
{
    public ClientHealthSnapshotRepository(DatabaseManager? database = null) : base(database)
    {
    }

    public void Save(ClientHealthSnapshotRecord snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Execute(@"
INSERT OR REPLACE INTO client_health_snapshots
(snapshot_id, atm_id, agent_state, network_connected, session_id, last_heartbeat_utc,
 last_sync_utc, outbox_count, error_count, last_error, snapshot_utc)
VALUES ($id,$atm,$state,$net,$sess,$hb,$sync,$outbox,$errors,$err,$ts)",
            P("$id", snapshot.SnapshotId),
            P("$atm", snapshot.AtmId),
            P("$state", snapshot.AgentState),
            P("$net", snapshot.NetworkConnected ? 1 : 0),
            P("$sess", snapshot.SessionId),
            P("$hb", Stamp(snapshot.LastHeartbeatUtc)),
            P("$sync", Stamp(snapshot.LastSyncUtc)),
            P("$outbox", snapshot.OutboxCount),
            P("$errors", snapshot.ErrorCount),
            P("$err", snapshot.LastError),
            P("$ts", snapshot.SnapshotUtc.ToString("o")));
    }

    public IReadOnlyList<ClientHealthSnapshotRecord> LoadLatestPerTerminal(int maxResults = 500)
    {
        var table = Query(@"
SELECT s.snapshot_id, s.atm_id, s.agent_state, s.network_connected, s.session_id,
       s.last_heartbeat_utc, s.last_sync_utc, s.outbox_count, s.error_count, s.last_error, s.snapshot_utc
FROM client_health_snapshots s
JOIN (SELECT atm_id, MAX(snapshot_utc) AS newest FROM client_health_snapshots GROUP BY atm_id) t
  ON t.atm_id = s.atm_id AND t.newest = s.snapshot_utc
ORDER BY s.snapshot_utc DESC
LIMIT $max",
            P("$max", Math.Clamp(maxResults, 1, 5000)));

        var rows = new List<ClientHealthSnapshotRecord>(table.Rows.Count);
        foreach (DataRow row in table.Rows)
        {
            rows.Add(new ClientHealthSnapshotRecord(
                row["snapshot_id"]?.ToString() ?? string.Empty,
                row["atm_id"]?.ToString() ?? string.Empty,
                row["agent_state"]?.ToString() ?? string.Empty,
                Convert.ToInt64(row["network_connected"]) != 0,
                row["session_id"]?.ToString(),
                ParseStamp(row["last_heartbeat_utc"]),
                ParseStamp(row["last_sync_utc"]),
                Convert.ToInt32(row["outbox_count"]),
                Convert.ToInt32(row["error_count"]),
                row["last_error"]?.ToString(),
                ParseStamp(row["snapshot_utc"]) ?? DateTime.MinValue));
        }
        return rows;
    }
}
