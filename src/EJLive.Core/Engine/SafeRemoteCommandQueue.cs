using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// Thread-safe queue for server-dispatched remote commands with policy enforcement,
/// risk level validation, audit logging, and delivery tracking.
/// Implements Track 026 (Safe Remote Command Queue) and Track 038 (Command Retry).
/// </summary>
public sealed class SafeRemoteCommandQueue
{
    private readonly object _lock = new();
    private readonly List<QueuedCommand> _queue = new();
    private readonly Dictionary<string, QueuedCommand> _history = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Raised when a command is enqueued and ready for dispatch.</summary>
    public event Action<QueuedCommand>? OnCommandQueued;

    /// <summary>Raised when a command result is received.</summary>
    public event Action<QueuedCommand>? OnCommandResult;

    /// <summary>
    /// Enqueues a remote command after validating it against the safety policy.
    /// </summary>
    /// <param name="atmId">Target ATM identifier.</param>
    /// <param name="commandType">The command type (e.g., CMD_RESTART, CMD_SCREENSHOT).</param>
    /// <param name="role">The operator role requesting the command.</param>
    /// <param name="operatorConfirmed">Whether the operator explicitly confirmed the action.</param>
    /// <param name="maintenanceWindow">Whether current time is within maintenance window.</param>
    /// <returns>The queued command with policy decision.</returns>
    public QueuedCommand Enqueue(
        string atmId,
        string commandType,
        string role = "Admin",
        bool operatorConfirmed = false,
        bool maintenanceWindow = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        ArgumentException.ThrowIfNullOrWhiteSpace(commandType);
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        var normalizedAtmId = atmId.Trim();
        var normalizedCommand = commandType.Trim().ToUpperInvariant();
        var normalizedRole = role.Trim();
        var knownCommand = TryAssessRisk(normalizedCommand, out var riskLevel);
        var allowed = knownCommand && EvaluatePolicy(normalizedRole, operatorConfirmed, maintenanceWindow, riskLevel);

        var cmd = new QueuedCommand
        {
            CommandId = Guid.NewGuid().ToString("N"),
            AtmId = normalizedAtmId,
            CommandType = normalizedCommand,
            Role = normalizedRole,
            RiskLevel = riskLevel,
            Allowed = allowed,
            RequiresConfirmation = riskLevel >= CommandRiskLevel.High,
            OperatorConfirmed = operatorConfirmed,
            State = allowed ? CommandState.Pending : CommandState.Rejected,
            CreatedAtUtc = DateTime.UtcNow
        };

        if (!allowed)
        {
            cmd.RejectionReason = knownCommand
                ? $"Policy denied: risk={riskLevel}, role={normalizedRole}, confirmed={operatorConfirmed}, maintenance={maintenanceWindow}"
                : "Command type is not allowlisted.";
        }

        lock (_lock)
        {
            _queue.Add(cmd);
            _history[cmd.CommandId] = cmd;
        }

        if (allowed)
            OnCommandQueued?.Invoke(cmd);

        return cmd;
    }

    /// <summary>
    /// Gets the next pending command for a specific ATM.
    /// </summary>
    public QueuedCommand? Dequeue(string atmId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        lock (_lock)
        {
            var cmd = _queue.FirstOrDefault(c =>
                c.AtmId.Equals(atmId, StringComparison.OrdinalIgnoreCase) &&
                c.State == CommandState.Pending &&
                c.Allowed);

            if (cmd != null)
            {
                cmd.State = CommandState.Sent;
                cmd.SentAtUtc = DateTime.UtcNow;
            }

            return cmd;
        }
    }

    /// <summary>
    /// Records the result of a command execution.
    /// </summary>
    public void RecordResult(string commandId, bool success, string? resultText = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        QueuedCommand? completed = null;
        lock (_lock)
        {
            if (!_history.TryGetValue(commandId, out var cmd))
                return;

            cmd.State = success ? CommandState.Completed : CommandState.Failed;
            cmd.ResultAtUtc = DateTime.UtcNow;
            cmd.ResultText = resultText ?? (success ? "OK" : "FAILED");
            cmd.Success = success;
            completed = cmd;
        }

        OnCommandResult?.Invoke(completed);
    }

    /// <summary>
    /// Retries a failed command if within retry limits.
    /// </summary>
    /// <param name="commandId">The command ID to retry.</param>
    /// <param name="maxRetries">Maximum retry count. Default 3.</param>
    /// <returns>True if retry was accepted.</returns>
    public bool Retry(string commandId, int maxRetries = 3)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        if (maxRetries <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRetries));

        QueuedCommand? retried = null;
        lock (_lock)
        {
            if (!_history.TryGetValue(commandId, out var cmd))
                return false;

            if (cmd.State != CommandState.Failed || cmd.RetryCount >= maxRetries)
                return false;

            cmd.RetryCount++;
            cmd.State = CommandState.Pending;
            cmd.SentAtUtc = null;
            cmd.ResultAtUtc = null;
            cmd.ResultText = null;
            cmd.Success = null;

            retried = cmd;
        }

        OnCommandQueued?.Invoke(retried);
        return true;
    }

    /// <summary>
    /// Gets the command history for an ATM.
    /// </summary>
    public List<QueuedCommand> GetHistory(string? atmId = null, int limit = 50)
    {
        if (limit <= 0)
            throw new ArgumentOutOfRangeException(nameof(limit));
        lock (_lock)
        {
            var query = _history.Values.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(atmId))
                query = query.Where(c => c.AtmId.Equals(atmId, StringComparison.OrdinalIgnoreCase));
            return query.OrderByDescending(c => c.CreatedAtUtc).Take(limit).ToList();
        }
    }

    /// <summary>
    /// Gets all pending commands across all ATMs.
    /// </summary>
    public List<QueuedCommand> GetPending()
    {
        lock (_lock)
            return _queue.Where(c => c.State == CommandState.Pending && c.Allowed).ToList();
    }

    private static bool TryAssessRisk(string commandType, out CommandRiskLevel risk)
    {
        switch (commandType)
        {
            case "CMD_PING":
            case "CMD_STATUS":
            case "CMD_GET_STATS":
            case "CMD_SYNC_TIME":
            case "CMD_WINDOWS_REMOTE_CHECK":
                risk = CommandRiskLevel.Low;
                return true;
            case "CMD_SCREENSHOT":
            case "CMD_SYNC_JOURNAL":
            case "CMD_FORCE_SYNC":
            case "CMD_GET_FILE":
            case "CMD_SEND_FILE":
            case "CMD_SEND_IMAGE":
                risk = CommandRiskLevel.Medium;
                return true;
            case "CMD_REMOTE_SESSION_START":
            case "CMD_REMOTE_SESSION_STOP":
            case "CMD_REMOTE_CONFIG":
            case "CMD_WINDOWS_REMOTE_START":
            case "CMD_WINDOWS_REMOTE_STOP":
                risk = CommandRiskLevel.High;
                return true;
            case "CMD_RESTART":
            case "CMD_SHUTDOWN":
            case "CMD_CHANGE_PASSWORD":
                risk = CommandRiskLevel.Critical;
                return true;
            default:
                risk = CommandRiskLevel.Critical;
                return false;
        }
    }

    private static bool EvaluatePolicy(
        string role, bool confirmed, bool maintenance, CommandRiskLevel risk)
    {
        if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
            !role.Equals("Support", StringComparison.OrdinalIgnoreCase) &&
            !role.Equals("Auditor", StringComparison.OrdinalIgnoreCase) &&
            !role.Equals("Observer", StringComparison.OrdinalIgnoreCase))
            return false;

        // Allowlisted commands: always allowed for any authenticated role
        if (risk == CommandRiskLevel.Low)
            return true;

        // Medium risk: requires Admin or Support role
        if (risk == CommandRiskLevel.Medium)
            return role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                   role.Equals("Support", StringComparison.OrdinalIgnoreCase);

        // High risk: requires Admin + explicit confirmation
        if (risk == CommandRiskLevel.High)
            return role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && confirmed;

        // Critical risk: requires Admin + confirmation + maintenance window
        if (risk == CommandRiskLevel.Critical)
            return role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && confirmed && maintenance;

        return false;
    }
}

/// <summary>
/// Represents a command in the safe remote command queue.
/// </summary>
public sealed class QueuedCommand
{
    public string CommandId { get; set; } = string.Empty;
    public string AtmId { get; set; } = string.Empty;
    public string CommandType { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public CommandRiskLevel RiskLevel { get; set; }
    public bool Allowed { get; set; }
    public bool RequiresConfirmation { get; set; }
    public bool OperatorConfirmed { get; set; }
    public CommandState State { get; set; } = CommandState.Pending;
    public string? RejectionReason { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SentAtUtc { get; set; }
    public DateTime? ResultAtUtc { get; set; }
    public string? ResultText { get; set; }
    public bool? Success { get; set; }
    public int RetryCount { get; set; }
}

/// <summary>
/// Command state in the queue lifecycle.
/// </summary>
public enum CommandState
{
    Pending,
    Sent,
    Completed,
    Failed,
    Rejected,
    Expired
}
