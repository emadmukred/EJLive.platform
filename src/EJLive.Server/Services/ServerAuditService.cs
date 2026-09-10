using System.Data;
using EJLive.Core.Models;
using EJLive.Core.Services;

namespace EJLive.Server.Services;

/// <summary>
/// Owns server audit persistence, projection, and command-event filtering.
/// </summary>
public sealed class ServerAuditService
{
    private readonly DatabaseManager _database;

    public ServerAuditService(DatabaseManager? database = null)
    {
        _database = database ?? DatabaseManager.Instance;
    }

    public void WriteCommandAudit(string action, string atmId, string detail)
    {
        _database.InsertAuditLog(
            action,
            "ServerDashboard",
            string.IsNullOrWhiteSpace(atmId) ? null : atmId,
            detail);
    }

    public IReadOnlyList<AuditLogEntry> LoadAuditEntries(int lookbackHours, int maxRows = 5000)
    {
        var windowHours = Math.Max(1, lookbackHours);
        var fromUtc = DateTime.UtcNow.AddHours(-windowHours);
        var table = _database.GetAuditLog(null, fromUtc, DateTime.UtcNow, Math.Max(1, maxRows));
        var entries = new List<AuditLogEntry>(table.Rows.Count);

        foreach (DataRow row in table.Rows)
            entries.Add(ToAuditLogEntry(row));

        return entries;
    }

    public CommandAuditQueryResult QueryCommandAudit(
        string? atmId,
        int lookbackHours,
        CommandAuditScope scope,
        int maxRows = 5000)
    {
        var windowHours = Math.Max(1, lookbackHours);
        var fromUtc = DateTime.UtcNow.AddHours(-windowHours);
        var table = _database.GetAuditLog(
            string.IsNullOrWhiteSpace(atmId) ? null : atmId,
            fromUtc,
            DateTime.UtcNow,
            Math.Max(1, maxRows));

        var rows = table.Rows
            .Cast<DataRow>()
            .Select(ToCommandAuditRow)
            .Where(row => IsCommandAuditAction(row.Action))
            .Where(row => MatchesScope(row, scope))
            .ToArray();

        return new CommandAuditQueryResult(
            rows,
            rows.Count(row => row.IsFailure),
            windowHours);
    }

    public static bool IsCommandAuditAction(string? action)
    {
        var value = (action ?? string.Empty).Trim();
        return value.Length > 0 &&
               (value.Contains("Command", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("ConnectivityProbe", StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesScope(CommandAuditRow row, CommandAuditScope scope)
    {
        return scope switch
        {
            CommandAuditScope.Dispatch =>
                row.Action.Contains("Dispatch", StringComparison.OrdinalIgnoreCase) ||
                row.Action.Contains("Broadcast", StringComparison.OrdinalIgnoreCase),
            CommandAuditScope.Results =>
                row.Action.Contains("Result", StringComparison.OrdinalIgnoreCase),
            CommandAuditScope.Failures => row.IsFailure,
            _ => true
        };
    }

    private static AuditLogEntry ToAuditLogEntry(DataRow row)
    {
        var performedAtText = Convert.ToString(row["performed_at"]) ?? string.Empty;
        var createdAtUtc = DateTime.TryParse(performedAtText, out var performedAt)
            ? performedAt.ToUniversalTime()
            : DateTime.UtcNow;

        return new AuditLogEntry
        {
            EntryId = Convert.ToString(row["log_id"]) ?? Guid.NewGuid().ToString("N"),
            Action = Convert.ToString(row["action"]) ?? string.Empty,
            UserName = Convert.ToString(row["performed_by"]) ?? string.Empty,
            Target = Convert.ToString(row["atm_id"]) ?? string.Empty,
            Details = Convert.ToString(row["details"]) ?? string.Empty,
            CreatedAtUtc = createdAtUtc
        };
    }

    private static CommandAuditRow ToCommandAuditRow(DataRow row)
    {
        var action = Convert.ToString(row["action"]) ?? string.Empty;
        var details = Convert.ToString(row["details"]) ?? string.Empty;
        var isFailure = action.Contains("Failed", StringComparison.OrdinalIgnoreCase) ||
                        details.Contains("fail", StringComparison.OrdinalIgnoreCase);

        return new CommandAuditRow(
            Convert.ToString(row["performed_at"]) ?? string.Empty,
            Convert.ToString(row["atm_id"]) ?? string.Empty,
            action,
            Convert.ToString(row["performed_by"]) ?? string.Empty,
            details,
            isFailure);
    }
}

public enum CommandAuditScope
{
    All,
    Dispatch,
    Results,
    Failures
}

public sealed record CommandAuditRow(
    string PerformedAt,
    string AtmId,
    string Action,
    string PerformedBy,
    string Details,
    bool IsFailure);

public sealed record CommandAuditQueryResult(
    IReadOnlyList<CommandAuditRow> Rows,
    int FailureCount,
    int LookbackHours);
