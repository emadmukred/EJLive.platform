using System;
using System.Collections.Generic;
using System.Data;
using EJLive.Core.Services;

namespace EJLive.Core.Data.Repositories;

/// <summary>
/// SQLite implementation of <see cref="IATMRegistryRepository"/>. Upsert keeps the original
/// registration timestamp; heartbeats only move the two "last seen" columns forward — never
/// backwards, which matters when agents and server clocks disagree by seconds.
/// </summary>
public sealed class ATMRegistryRepository : SqliteRepositoryBase, IATMRegistryRepository
{
    public ATMRegistryRepository(DatabaseManager? database = null) : base(database)
    {
    }

    public void Upsert(AtmRegistrationRecord registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        Execute(@"
INSERT INTO atm_registry (atm_id, atm_name, atm_type, ip_address, registered_at_utc, last_heartbeat_utc, last_data_received_utc)
VALUES ($id,$name,$type,$ip,$registered,$hb,$data)
ON CONFLICT(atm_id) DO UPDATE SET
    atm_name = COALESCE(NULLIF(excluded.atm_name, ''), atm_registry.atm_name),
    atm_type = COALESCE(NULLIF(excluded.atm_type, ''), atm_registry.atm_type),
    ip_address = COALESCE(NULLIF(excluded.ip_address, ''), atm_registry.ip_address),
    last_heartbeat_utc = MAX(COALESCE(atm_registry.last_heartbeat_utc, ''), COALESCE(excluded.last_heartbeat_utc, '')),
    last_data_received_utc = MAX(COALESCE(atm_registry.last_data_received_utc, ''), COALESCE(excluded.last_data_received_utc, ''))",
            P("$id", registration.AtmId),
            P("$name", registration.AtmName),
            P("$type", registration.AtmType),
            P("$ip", registration.IpAddress),
            P("$registered", registration.RegisteredAtUtc.ToString("o")),
            P("$hb", Stamp(registration.LastHeartbeatUtc)),
            P("$data", Stamp(registration.LastDataReceivedUtc)));
    }

    public IReadOnlyList<AtmRegistrationRecord> Snapshot(int maxRows = 1_000)
    {
        var table = Query(@"
SELECT atm_id, atm_name, atm_type, ip_address, registered_at_utc, last_heartbeat_utc, last_data_received_utc
FROM atm_registry ORDER BY atm_id ASC LIMIT $max",
            P("$max", Math.Clamp(maxRows, 1, 10_000)));

        var rows = new List<AtmRegistrationRecord>(table.Rows.Count);
        foreach (DataRow r in table.Rows)
        {
            rows.Add(new AtmRegistrationRecord(
                r["atm_id"]?.ToString() ?? string.Empty,
                r["atm_name"]?.ToString() ?? string.Empty,
                r["atm_type"]?.ToString() ?? "NCR",
                r["ip_address"]?.ToString() ?? string.Empty,
                ParseStamp(r["registered_at_utc"]) ?? DateTime.MinValue,
                ParseStamp(r["last_heartbeat_utc"]),
                ParseStamp(r["last_data_received_utc"])));
        }
        return rows;
    }
}
