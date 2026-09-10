using System.Collections.Concurrent;
using EJLive.Core.Models;

namespace EJLive.Core.Server;

/// <summary>Bounded, thread-safe history of ATM status snapshots for trend analysis.</summary>
public sealed class AtmSnapshotStore
{
    private readonly ConcurrentDictionary<string, Queue<ATMRealTimeStatusSnapshot>> _history = new(StringComparer.OrdinalIgnoreCase);
    private readonly int _maxHistoryPerAtm;

    public AtmSnapshotStore(int maxHistoryPerAtm = 100)
    {
        if (maxHistoryPerAtm <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxHistoryPerAtm));
        _maxHistoryPerAtm = maxHistoryPerAtm;
    }

    public void Store(ATMRealTimeStatusSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshot.ATM_ID);
        var queue = _history.GetOrAdd(snapshot.ATM_ID.Trim(), _ => new Queue<ATMRealTimeStatusSnapshot>());
        lock (queue)
        {
            queue.Enqueue(snapshot);
            while (queue.Count > _maxHistoryPerAtm)
                queue.Dequeue();
        }
    }

    public ATMRealTimeStatusSnapshot? GetLatest(string atmId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        if (!_history.TryGetValue(atmId.Trim(), out var queue))
            return null;
        lock (queue)
            return queue.Count == 0 ? null : queue.Last();
    }

    public IReadOnlyList<ATMRealTimeStatusSnapshot> GetHistory(string atmId, int count = 10)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (!_history.TryGetValue(atmId.Trim(), out var queue))
            return Array.Empty<ATMRealTimeStatusSnapshot>();
        lock (queue)
            return queue.TakeLast(count).ToArray();
    }

    public IReadOnlyList<string> TrackedAtmIds => _history.Keys.OrderBy(id => id, StringComparer.OrdinalIgnoreCase).ToArray();
    public int Count => _history.Count;
}

/// <summary>Maintains cassette state and emits alarms only when a terminal enters a new risk state.</summary>
public sealed class CashReplenishmentEngine
{
    private readonly ConcurrentDictionary<string, CashSnapshot> _cashState = new(StringComparer.OrdinalIgnoreCase);

    public event Action<string, string>? AlarmRaised;

    public CashSnapshot UpdateCashState(ATMRealTimeStatusSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshot.ATM_ID);

        var current = new CashSnapshot(
            snapshot.ATM_ID.Trim(),
            Math.Max(0, snapshot.TotalCashRemaining),
            new Dictionary<string, long>(snapshot.CassetteRemaining, StringComparer.OrdinalIgnoreCase),
            snapshot.CashLow,
            snapshot.CashOut,
            snapshot.Currency ?? string.Empty,
            new Dictionary<string, int>(snapshot.Denominations, StringComparer.OrdinalIgnoreCase),
            Math.Max(0, snapshot.RejectBinCount),
            Math.Max(0, snapshot.RetractBinCount),
            DateTime.UtcNow);

        _cashState.TryGetValue(current.AtmId, out var previous);
        _cashState[current.AtmId] = current;
        if (current.CashOut && previous?.CashOut != true)
            AlarmRaised?.Invoke(current.AtmId, "CASH_OUT_CRITICAL");
        else if (current.CashLow && !current.CashOut && previous?.CashLow != true)
            AlarmRaised?.Invoke(current.AtmId, "CASH_LOW_WARNING");
        return current;
    }

    public CashSnapshot? GetCashState(string atmId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        return _cashState.TryGetValue(atmId.Trim(), out var state) ? state : null;
    }

    public IReadOnlyList<CashSnapshot> GetRiskAtms() =>
        _cashState.Values
            .Where(state => state.CashLow || state.CashOut)
            .OrderByDescending(state => state.CashOut)
            .ThenBy(state => state.AtmId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public IReadOnlyList<ReplenishmentRecommendation> GetReplenishmentRecommendations() =>
        GetRiskAtms()
            .Select(state => new ReplenishmentRecommendation(
                state.AtmId,
                state.CashOut ? ReplenishmentPriority.Immediate : ReplenishmentPriority.Scheduled,
                state.TotalRemaining,
                state.Currency,
                state.UpdatedUtc))
            .ToArray();
}

public sealed record CashSnapshot(
    string AtmId,
    long TotalRemaining,
    IReadOnlyDictionary<string, long> CassetteRemaining,
    bool CashLow,
    bool CashOut,
    string Currency,
    IReadOnlyDictionary<string, int> Denominations,
    long RejectBin,
    long RetractBin,
    DateTime UpdatedUtc);

public enum ReplenishmentPriority { Scheduled, Immediate }

public sealed record ReplenishmentRecommendation(
    string AtmId,
    ReplenishmentPriority Priority,
    long TotalRemaining,
    string Currency,
    DateTime ObservedUtc);

/// <summary>Thread-safe terminal inventory independent from presentation state.</summary>
public sealed class TerminalAgentManagementService
{
    private readonly ConcurrentDictionary<string, TerminalAgentEntry> _terminals = new(StringComparer.OrdinalIgnoreCase);

    public event Action<TerminalAgentEntry>? TerminalRegistered;
    public event Action<TerminalAgentEntry>? TerminalUpdated;
    public event Action<string>? TerminalRemoved;

    public TerminalAgentEntry RegisterOrUpdate(
        string atmId,
        string? atmName = null,
        string? vendor = null,
        string? model = null,
        string? branch = null,
        string? region = null,
        string? ipAddress = null,
        string? clientVersion = null,
        string? agentState = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        var key = atmId.Trim();
        var now = DateTime.UtcNow;
        var added = false;
        var entry = _terminals.AddOrUpdate(
            key,
            _ =>
            {
                added = true;
                return new TerminalAgentEntry(
                    key,
                    Normalize(atmName),
                    Normalize(vendor),
                    Normalize(model),
                    Normalize(branch),
                    Normalize(region),
                    Normalize(ipAddress),
                    Normalize(clientVersion),
                    Normalize(agentState, "Unknown"),
                    now,
                    now);
            },
            (_, existing) => existing with
            {
                AtmName = Select(atmName, existing.AtmName),
                Vendor = Select(vendor, existing.Vendor),
                Model = Select(model, existing.Model),
                Branch = Select(branch, existing.Branch),
                Region = Select(region, existing.Region),
                IpAddress = Select(ipAddress, existing.IpAddress),
                ClientVersion = Select(clientVersion, existing.ClientVersion),
                AgentState = Select(agentState, existing.AgentState),
                LastSeenUtc = now
            });

        if (added) TerminalRegistered?.Invoke(entry);
        else TerminalUpdated?.Invoke(entry);
        return entry;
    }

    public bool Remove(string atmId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        var key = atmId.Trim();
        if (!_terminals.TryRemove(key, out _))
            return false;
        TerminalRemoved?.Invoke(key);
        return true;
    }

    public TerminalAgentEntry? Get(string atmId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        return _terminals.TryGetValue(atmId.Trim(), out var entry) ? entry : null;
    }

    public IReadOnlyList<TerminalAgentEntry> GetAll() =>
        _terminals.Values.OrderBy(entry => entry.AtmId, StringComparer.OrdinalIgnoreCase).ToArray();

    public IReadOnlyDictionary<string, int> GetVendorDistribution() =>
        _terminals.Values
            .GroupBy(entry => string.IsNullOrWhiteSpace(entry.Vendor) ? "Unknown" : entry.Vendor, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

    public int Count => _terminals.Count;

    private static string Select(string? candidate, string fallback) =>
        string.IsNullOrWhiteSpace(candidate) ? fallback : candidate.Trim();

    private static string Normalize(string? value, string fallback = "") =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}

public sealed record TerminalAgentEntry(
    string AtmId,
    string AtmName,
    string Vendor,
    string Model,
    string Branch,
    string Region,
    string IpAddress,
    string ClientVersion,
    string AgentState,
    DateTime RegisteredUtc,
    DateTime LastSeenUtc);

/// <summary>Region definitions and terminal assignments with snapshot-safe reads.</summary>
public sealed class RegionalSystemConfigurationService
{
    private readonly ConcurrentDictionary<string, RegionalConfiguration> _regions = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _regionAtms = new(StringComparer.OrdinalIgnoreCase);

    public RegionalConfiguration DefineRegion(
        string regionName,
        string? province = null,
        string? city = null,
        string? geography = null,
        string? timeZone = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(regionName);
        var name = regionName.Trim();
        var configuration = new RegionalConfiguration(
            name,
            province?.Trim() ?? string.Empty,
            city?.Trim() ?? string.Empty,
            geography?.Trim() ?? string.Empty,
            string.IsNullOrWhiteSpace(timeZone) ? "+03:00" : timeZone.Trim());
        _regions[name] = configuration;
        return configuration;
    }

    public void AssignAtmToRegion(string atmId, string regionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        ArgumentException.ThrowIfNullOrWhiteSpace(regionName);
        var region = regionName.Trim();
        if (!_regions.ContainsKey(region))
            throw new KeyNotFoundException($"Region '{region}' has not been defined.");
        _regionAtms.GetOrAdd(region, _ => new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase))[atmId.Trim()] = 0;
    }

    public RegionalConfiguration? GetRegion(string regionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(regionName);
        return _regions.TryGetValue(regionName.Trim(), out var configuration) ? configuration : null;
    }

    public IReadOnlyList<string> GetAtmsInRegion(string regionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(regionName);
        return _regionAtms.TryGetValue(regionName.Trim(), out var terminals)
            ? terminals.Keys.OrderBy(id => id, StringComparer.OrdinalIgnoreCase).ToArray()
            : Array.Empty<string>();
    }

    public IReadOnlyList<string> GetAllRegions() =>
        _regions.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray();
}

public sealed record RegionalConfiguration(
    string RegionName,
    string Province,
    string City,
    string Geography,
    string TimeZone);
