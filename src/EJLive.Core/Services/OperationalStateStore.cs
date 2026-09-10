using System.Collections.Concurrent;
using EJLive.Core.Models;

namespace EJLive.Core.Services;

public sealed class OperationalStateStore
{
    private readonly ConcurrentDictionary<string, ATMInfo> _states = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyCollection<ATMInfo> Snapshot => _states.Values.OrderBy(s => s.ATM_ID, StringComparer.OrdinalIgnoreCase).ToArray();

    public void Upsert(ATMInfo atm)
        => _states[atm.ATM_ID ?? atm.ATMId ?? Guid.NewGuid().ToString("N")] = atm;

    public bool TryGet(string atmId, out ATMInfo? atm) => _states.TryGetValue(atmId, out atm);

    public FleetSummary BuildSummary()
    {
        var snapshot = Snapshot;
        return new FleetSummary
        {
            Total = snapshot.Count,
            Connected = snapshot.Count(a => a.ConnectionStatus is ConnectionStatus.Connected or ConnectionStatus.Syncing or ConnectionStatus.WaitingReply),
            Syncing = snapshot.Count(a => a.ConnectionStatus == ConnectionStatus.Syncing || a.SyncState is SyncStatus.InProgress or SyncStatus.Syncing or SyncStatus.Resyncing),
            Offline = snapshot.Count(a => a.ConnectionStatus == ConnectionStatus.Disconnected || a.Status is ATMStatus.Offline or ATMStatus.CriticalFault),
            AverageHealth = snapshot.Count == 0 ? 0 : (int)Math.Round(snapshot.Average(a => a.HealthScore))
        };
    }
}
