using System;
using System.Collections.Generic;
using System.Linq;
using EJLive.Core;

namespace EJLive.Business
{
    /// <summary>
    /// Policy engine for remote commands. Enforces RBAC, maintenance windows,
    /// operator confirmation, and audit trails before command execution.
    /// </summary>
    public sealed class UnifiedRemoteCommandPolicy
    {
        private readonly HashSet<string> _allowedRoles = new(StringComparer.OrdinalIgnoreCase) { "Admin", "Support" };
        private readonly HashSet<string> _allowedCommands = new(StringComparer.OrdinalIgnoreCase)
        {
            AppConstants.CMD_RESTART,
            AppConstants.CMD_SHUTDOWN,
            AppConstants.CMD_CHANGE_PASSWORD,
            AppConstants.CMD_SCREENSHOT,
            AppConstants.CMD_SYNC_TIME,
            AppConstants.CMD_REMOTE_SESSION_START,
            AppConstants.CMD_REMOTE_SESSION_STOP,
            AppConstants.CMD_SEND_IMAGE,
            AppConstants.CMD_SYNC_IMAGES,
            AppConstants.CMD_FORCE_SYNC,
            AppConstants.CMD_GET_FILE,
            AppConstants.CMD_SEND_FILE,
            AppConstants.CMD_PING,
            AppConstants.CMD_REMOTE_CONFIG,
            AppConstants.CMD_GET_STATS,
            AppConstants.CMD_SYNC_FOLDER,
            AppConstants.CMD_WINDOWS_REMOTE_START,
            AppConstants.CMD_WINDOWS_REMOTE_STOP,
            AppConstants.CMD_WINDOWS_REMOTE_CHECK
        };
        private readonly HashSet<string> _commandsRequiringConfirmation = new(StringComparer.OrdinalIgnoreCase)
        {
            AppConstants.CMD_RESTART,
            AppConstants.CMD_SHUTDOWN,
            AppConstants.CMD_CHANGE_PASSWORD,
            AppConstants.CMD_REMOTE_SESSION_START,
            AppConstants.CMD_REMOTE_CONFIG,
            AppConstants.CMD_WINDOWS_REMOTE_START,
            AppConstants.CMD_WINDOWS_REMOTE_STOP
        };

        public PolicyResult Validate(string commandType, string role, bool operatorConfirmed, bool maintenanceWindow)
        {
            var normalizedCommand = (commandType ?? string.Empty).Trim();
            var normalizedRole = (role ?? string.Empty).Trim();
            var requiresConfirmation = _commandsRequiringConfirmation.Contains(normalizedCommand);
            var result = new PolicyResult
            {
                Allowed = true,
                RequiresConfirmation = requiresConfirmation,
                RequiresMaintenanceWindow = requiresConfirmation
            };

            if (!_allowedCommands.Contains(normalizedCommand))
            {
                result.Allowed = false;
                result.Reason = $"Command '{normalizedCommand}' is not allowed";
                return result;
            }

            if (!_allowedRoles.Contains(normalizedRole))
            {
                result.Allowed = false;
                result.Reason = $"Role '{normalizedRole}' is not authorized for remote commands";
                return result;
            }

            if (requiresConfirmation && !string.Equals(normalizedRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                result.Allowed = false;
                result.Reason = $"Command '{normalizedCommand}' requires the Admin role";
                return result;
            }

            if (requiresConfirmation && !operatorConfirmed)
            {
                result.Allowed = false;
                result.Reason = $"Command '{normalizedCommand}' requires operator confirmation";
                return result;
            }

            if (requiresConfirmation && !maintenanceWindow)
            {
                result.Allowed = false;
                result.Reason = $"Command '{normalizedCommand}' requires an active maintenance window";
                return result;
            }

            result.Reason = string.IsNullOrEmpty(result.Reason) ? "Policy check passed" : result.Reason;
            return result;
        }

        public IReadOnlyList<string> GetAllowedRoles() => _allowedRoles.ToList();
        public IReadOnlyList<string> GetAllowedCommands() => _allowedCommands.OrderBy(command => command).ToList();
        public IReadOnlyList<string> GetCommandsRequiringConfirmation() => _commandsRequiringConfirmation.ToList();
    }

    public sealed class PolicyResult
    {
        public bool Allowed { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool RequiresConfirmation { get; set; }
        public bool RequiresMaintenanceWindow { get; set; }
    }
}
