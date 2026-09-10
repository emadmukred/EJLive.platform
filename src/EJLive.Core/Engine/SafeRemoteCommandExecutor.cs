using System;
using System.Collections.Generic;
using System.Linq;
using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Executes remote commands safely with allowlisting, auditing, and queue tracking.
    /// </summary>
    public class SafeRemoteCommandExecutor
    {
        private readonly List<CommandQueueItem> _commandQueue = new();
        private readonly HashSet<string> _allowedPresets;
        private readonly bool _passwordCommandsEnabled;
        private readonly Func<RemoteCommandEnvelope, string> _auditSink;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeRemoteCommandExecutor"/> class.
        /// </summary>
        /// <param name="allowedPresets">Set of allowed script preset names. Only these may be executed via ExecuteScript.</param>
        /// <param name="passwordCommandsEnabled">Whether password-related commands are enabled via feature flag.</param>
        /// <param name="auditSink">Callback invoked with audit log entries before send and after result.</param>
        public SafeRemoteCommandExecutor(
            HashSet<string> allowedPresets,
            bool passwordCommandsEnabled,
            Func<RemoteCommandEnvelope, string> auditSink)
        {
            _allowedPresets = allowedPresets ?? throw new ArgumentNullException(nameof(allowedPresets));
            _passwordCommandsEnabled = passwordCommandsEnabled;
            _auditSink = auditSink ?? throw new ArgumentNullException(nameof(auditSink));
        }

        /// <summary>
        /// Gets a read-only view of the command queue.
        /// </summary>
        public IReadOnlyList<CommandQueueItem> CommandQueue => _commandQueue.AsReadOnly();

        /// <summary>
        /// Attempts to execute a remote command after policy and allowlist checks.
        /// </summary>
        /// <param name="envelope">The signed command envelope.</param>
        /// <param name="currentUtc">Current UTC time for expiry and timestamp checks.</param>
        /// <param name="inMaintenanceWindow">Whether the system is in a maintenance window.</param>
        /// <param name="operatorConfirmed">Whether an operator confirmed a critical command.</param>
        /// <param name="result">When successful, contains the execution result JSON.</param>
        /// <returns>True if execution succeeded; otherwise false.</returns>
        public bool TryExecute(
            RemoteCommandEnvelope envelope,
            DateTime currentUtc,
            bool inMaintenanceWindow,
            bool operatorConfirmed,
            out string result)
        {
            if (envelope == null)
                throw new ArgumentNullException(nameof(envelope));

            if (!RemoteCommandPolicy.EnforceRiskPolicy(envelope, currentUtc, inMaintenanceWindow, operatorConfirmed, out var failureReason))
            {
                QueueCommand(envelope, CommandQueueStatus.Failed, null, failureReason);
                _auditSink(envelope);
                result = failureReason;
                return false;
            }

            if (!ValidateCommandAllowlist(envelope, out var allowlistError))
            {
                QueueCommand(envelope, CommandQueueStatus.Failed, null, allowlistError);
                _auditSink(envelope);
                result = allowlistError;
                return false;
            }

            _auditSink(envelope);

            var executionResult = ExecuteCore(envelope);

            QueueCommand(
                envelope,
                executionResult.Success ? CommandQueueStatus.Completed : CommandQueueStatus.Failed,
                executionResult.ResultJson,
                executionResult.Error);

            _auditSink(envelope);

            result = executionResult.ResultJson ?? executionResult.Error ?? string.Empty;
            return executionResult.Success;
        }

        /// <summary>
        /// Validates the command against the allowlist and feature flags.
        /// </summary>
        private bool ValidateCommandAllowlist(RemoteCommandEnvelope envelope, out string error)
        {
            var commandType = envelope.CommandType;

            if (string.Equals(commandType, "ArbitraryShell", StringComparison.OrdinalIgnoreCase))
            {
                error = "Arbitrary shell commands are not permitted.";
                return false;
            }

            if (string.Equals(commandType, "ExecuteScript", StringComparison.OrdinalIgnoreCase))
            {
                // Expect payload to contain the preset name or a JSON wrapper with PresetName
                var presetName = ExtractPresetName(envelope.Payload);
                if (string.IsNullOrWhiteSpace(presetName) || !_allowedPresets.Contains(presetName))
                {
                    error = $"Script preset '{presetName}' is not in the allowlist.";
                    return false;
                }
            }

            if (string.Equals(commandType, "Password", StringComparison.OrdinalIgnoreCase) && !_passwordCommandsEnabled)
            {
                error = "Password commands are blocked because the feature flag is disabled.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        /// <summary>
        /// Extracts the preset name from a JSON payload string. Supports plain strings and {"PresetName":"..."}.
        /// </summary>
        private static string ExtractPresetName(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return string.Empty;

            var trimmed = payload.Trim();
            if (trimmed.StartsWith("{", StringComparison.Ordinal))
            {
                // Naive JSON extraction for PresetName value; production code should use System.Text.Json
                const string key = "\"PresetName\"";
                var idx = trimmed.IndexOf(key, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    var colon = trimmed.IndexOf(':', idx + key.Length);
                    if (colon >= 0)
                    {
                        var start = colon + 1;
                        while (start < trimmed.Length && char.IsWhiteSpace(trimmed[start])) start++;
                        if (start < trimmed.Length && trimmed[start] == '"')
                        {
                            start++;
                            var end = trimmed.IndexOf('"', start);
                            if (end > start)
                                return trimmed.Substring(start, end - start);
                        }
                    }
                }
            }

            return trimmed.Trim('"');
        }

        /// <summary>
        /// Core execution logic for permitted commands.
        /// </summary>
        private static (bool Success, string? ResultJson, string? Error) ExecuteCore(RemoteCommandEnvelope envelope)
        {
            // Placeholder: production integration would dispatch to the actual agent or WMI/SSH channel.
            return (true, $"{{ \"commandId\": \"{envelope.CommandId}\", \"status\": \"ok\" }}", null);
        }

        /// <summary>
        /// Records the command in the in-memory queue.
        /// </summary>
        private void QueueCommand(
            RemoteCommandEnvelope envelope,
            CommandQueueStatus status,
            string? resultJson,
            string? error)
        {
            var item = new CommandQueueItem
            {
                Id = Guid.NewGuid().ToString("N"),
                CommandId = envelope.CommandId,
                Status = status,
                CreatedUtc = envelope.TimestampUtc,
                SentUtc = status >= CommandQueueStatus.Sent ? DateTime.UtcNow : null,
                CompletedUtc = status is CommandQueueStatus.Completed or CommandQueueStatus.Failed or CommandQueueStatus.Expired
                    ? DateTime.UtcNow
                    : null,
                ResultJson = resultJson,
                Error = error
            };

            _commandQueue.Add(item);
        }
    }
}
