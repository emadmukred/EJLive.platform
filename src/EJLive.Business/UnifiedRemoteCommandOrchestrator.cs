using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Business
{
    /// <summary>
    /// Orchestrates remote commands across the ATM fleet.
    /// Handles command creation, validation, dispatch, and result tracking.
    /// </summary>
    public sealed class UnifiedRemoteCommandOrchestrator
    {
        private readonly UnifiedRemoteCommandPolicy _policy;
        private readonly ConcurrentDictionary<string, RemoteCommand> _commands = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, CommandExecutionRecord> _executions = new(StringComparer.OrdinalIgnoreCase);

        public UnifiedRemoteCommandOrchestrator(UnifiedRemoteCommandPolicy policy)
        {
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        public CommandOrchestrationResult Submit(string commandType, string targetAtmId, string operatorRole,
            bool operatorConfirmed, bool maintenanceWindow, IReadOnlyDictionary<string, string>? parameters = null)
        {
            var policyResult = _policy.Validate(commandType, operatorRole, operatorConfirmed, maintenanceWindow);

            var result = new CommandOrchestrationResult
            {
                Allowed = policyResult.Allowed,
                Reason = policyResult.Reason
            };

            if (!policyResult.Allowed)
                return result;

            var command = new RemoteCommand
            {
                CommandId = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant(),
                CommandType = commandType,
                ATM_ID = targetAtmId,
                Payload = parameters == null ? string.Empty : JsonSerializer.Serialize(parameters),
                RequiresConfirmation = policyResult.RequiresConfirmation,
                CreatedAtUtc = DateTime.UtcNow,
                SentAtUtc = DateTime.UtcNow,
                Status = RemoteCommandStatus.Sent
            };

            _commands[command.CommandId] = command;
            result.CommandId = command.CommandId;
            result.Status = "Sent";

            // Record execution for audit
            _executions[command.CommandId] = new CommandExecutionRecord
            {
                CommandId = command.CommandId,
                AtmId = targetAtmId,
                CommandType = commandType,
                Status = "Queued",
                CreatedAtUtc = DateTime.UtcNow
            };

            AppLogger.Instance.Info($"Command {command.CommandId} ({commandType}) queued for ATM {targetAtmId}", "Commands");

            return result;
        }

        public bool UpdateCommandStatus(string commandId, string status, string? resultText = null)
        {
            if (!_commands.TryGetValue(commandId, out var command))
                return false;

            command.Status = ParseStatus(status);
            if (resultText != null)
                command.Result = resultText;

            if (status is "Executed" or "Failed" or "Timeout")
            {
                command.CompletedAtUtc = DateTime.UtcNow;
            }

            if (_executions.TryGetValue(commandId, out var execution))
            {
                execution.Status = status;
                execution.Result = resultText;
                execution.UpdatedAtUtc = DateTime.UtcNow;
            }

            AppLogger.Instance.Info($"Command {commandId} status updated: {status}", "Commands");
            return true;
        }

        public RemoteCommand? GetCommand(string commandId)
        {
            _commands.TryGetValue(commandId, out var command);
            return command;
        }

        public IReadOnlyList<RemoteCommand> GetPendingCommands()
            => _commands.Values.Where(c => c.Status is RemoteCommandStatus.Pending or RemoteCommandStatus.Sent).ToList();

        public IReadOnlyList<CommandExecutionRecord> GetExecutionHistory()
            => _executions.Values.OrderByDescending(e => e.CreatedAtUtc).ToList();

        private static RemoteCommandStatus ParseStatus(string status)
        {
            if (Enum.TryParse<RemoteCommandStatus>(status, true, out var parsed))
                return parsed;

            return status switch
            {
                "Executed" => RemoteCommandStatus.Completed,
                "Timeout" => RemoteCommandStatus.Failed,
                _ => RemoteCommandStatus.Pending
            };
        }
    }

    public sealed class CommandOrchestrationResult
    {
        public bool Allowed { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public string Status { get; set; } = "Rejected";
    }

    public sealed class CommandExecutionRecord
    {
        public string CommandId { get; set; } = string.Empty;
        public string AtmId { get; set; } = string.Empty;
        public string CommandType { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string? Result { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
