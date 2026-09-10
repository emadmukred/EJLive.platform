using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Shared;
using AppConstants = EJLive.Shared.AppConstants;

namespace EJLive.Server.Services
{
    public partial class RemoteControlSummary
    {
        public int PendingCommands { get; set; }
        public int TotalCommandsSent { get; set; }
        public int SuccessfulCommands { get; set; }
        public int FailedCommands { get; set; }
        public int TimedOutCommands { get; set; }
        public DateTime GeneratedAtUtc { get; set; }
        public double SuccessRate => TotalCommandsSent > 0
            ? (double)SuccessfulCommands / TotalCommandsSent * 100.0
            : 0;
    }

    public partial class RemoteControlService : IDisposable
    {
        private readonly ServerEngine _serverEngine;
        private readonly Dictionary<string, RemoteCommand> _pendingCommands = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<RemoteCommand> _commandHistory = new();
        private ServerEngine _serverEngine;
        private Dictionary<string, CommandRecord> _commandHistory;
        private Dictionary<string, Queue<ScheduledCommand>> _scheduledCommands;
        private Timer _schedulerTimer;
        private int _commandCounter;
        private static readonly Regex CommandResultLine = new(
        @"^Command result from (?<atm>[^:]+):\s*(?<cmd>[^\s]+)\s+(?<state>OK|FAIL)\s*(?<message>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        private readonly Dictionary<string, Queue<ScheduledCommand>> _scheduledCommands = new(StringComparer.OrdinalIgnoreCase);
        private readonly System.Threading.Timer _schedulerTimer;
        private readonly object _lock = new();
        private int _commandCounter;
        private static readonly Regex CommandResultLine = new(
        @"^Command result from (?<atm>[^:]+):\s*(?<cmd>[^\s]+)\s+(?<state>OK|FAIL)\s*(?<message>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        private readonly Dictionary<string, Queue<ScheduledCommand>> _scheduledCommands = new(StringComparer.OrdinalIgnoreCase);
        public IReadOnlyList<RemoteCommand> GetPendingCommands()
            => _pendingCommands.Values.OrderBy(c => c.SentAtUtc).ToList();
        public IReadOnlyList<RemoteCommand> GetCommandHistory(string? atmId = null, int maxCount = 100)
        {
            var query = _commandHistory.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(atmId))
                query = query.Where(c => string.Equals(c.TargetATMId, atmId, StringComparison.OrdinalIgnoreCase));
            return query.OrderByDescending(c => c.SentAtUtc).Take(maxCount).ToList();
        }
        public RemoteControlSummary GetSummary()
        {
            return new RemoteControlSummary
            {
                PendingCommands = _pendingCommands.Count,
                TotalCommandsSent = _commandHistory.Count + _pendingCommands.Count,
                SuccessfulCommands = _commandHistory.Count(c => c.Status == "Executed"),
                FailedCommands = _commandHistory.Count(c => c.Status == "Failed"),
                TimedOutCommands = _commandHistory.Count(c => c.Status == "Timeout"),
                GeneratedAtUtc = DateTime.UtcNow
            };
        }
        public int CleanupExpiredCommands()
        {
            var expired = _pendingCommands.Values.Where(c => c.IsExpired).ToArray();
            foreach (var cmd in expired)
            {
                cmd.Status = "Timeout";
                cmd.Result = "Command timed out";
                _commandHistory.Add(cmd);
                _pendingCommands.Remove(cmd.CommandId);
            }
            if (expired.Length > 0)
                Log($"Cleaned up {expired.Length} expired commands.");
            return expired.Length;
        }
        private void Log(string message) => OnLog?.Invoke($"[RemoteControl] {message}");
        private readonly object _lock = new object();
        public string SendRestart(string atmId, int delaySeconds = 10)
        {
            string cmdId = GenerateCommandId();
            bool sent = _serverEngine.SendCommand(atmId, Protocol.CMD_RESTART, new[] { cmdId, delaySeconds.ToString() });
            RecordCommand(cmdId, atmId, "RESTART", sent);
            OnLog?.Invoke("[RemoteControl] RESTART -> " + atmId + " (delay: " + delaySeconds + "s)");
            return cmdId;
        }
        public string SendScreenshot(string atmId)
        {
            string cmdId = GenerateCommandId();
            bool sent = _serverEngine.SendCommand(atmId, Protocol.CMD_SCREENSHOT, new[] { cmdId });
            RecordCommand(cmdId, atmId, "SCREENSHOT", sent);
            OnLog?.Invoke("[RemoteControl] SCREENSHOT -> " + atmId);
            return cmdId;
        }
        public string SendTimeSync(string atmId)
        {
            string cmdId = GenerateCommandId();
            string serverTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            bool sent = _serverEngine.SendCommand(atmId, Protocol.CMD_TIMESYNC, new[] { cmdId, serverTime });
            RecordCommand(cmdId, atmId, "TIMESYNC", sent);
            OnLog?.Invoke("[RemoteControl] TIMESYNC -> " + atmId + " (" + serverTime + ")");
            return cmdId;
        }
        public string SendShutdown(string atmId, int delaySeconds = 30)
        {
            string cmdId = GenerateCommandId();
            bool sent = _serverEngine.SendCommand(atmId, Protocol.CMD_SHUTDOWN, new[] { cmdId, delaySeconds.ToString() });
            RecordCommand(cmdId, atmId, "SHUTDOWN", sent);
            OnLog?.Invoke("[RemoteControl] SHUTDOWN -> " + atmId);
            return cmdId;
        }
        public string SendChangePassword(string atmId, string newPassword)
        {
            string cmdId = GenerateCommandId();
            bool sent = _serverEngine.SendCommand(atmId, Protocol.CMD_CHANGE_PASSWORD, new[] { cmdId, newPassword });
            RecordCommand(cmdId, atmId, "CHANGE_PASSWORD", sent);
            return cmdId;
        }
        public string SendImage(string atmId, string imageName, byte[] imageData)
        {
            string cmdId = GenerateCommandId();
            string base64 = Convert.ToBase64String(imageData);
            bool sent = _serverEngine.SendCommand(atmId, Protocol.CMD_SEND_IMAGE, new[] { cmdId, imageName, base64 });
            RecordCommand(cmdId, atmId, "SEND_IMAGE", sent);
            OnLog?.Invoke("[RemoteControl] SEND_IMAGE -> " + atmId + " (" + imageData.Length + " bytes)");
            return cmdId;
        }
        public string SendUpdateConfig(string atmId, string configKey, string configValue)
        {
            string cmdId = GenerateCommandId();
            bool sent = _serverEngine.SendCommand(atmId, Protocol.CMD_UPDATE_CONFIG, new[] { cmdId, configKey, configValue });
            RecordCommand(cmdId, atmId, "UPDATE_CONFIG", sent);
            return cmdId;
        }
        public string SendGetSystemInfo(string atmId)
        {
            string cmdId = GenerateCommandId();
            bool sent = _serverEngine.SendCommand(atmId, Protocol.CMD_GET_SYSINFO, new[] { cmdId });
            RecordCommand(cmdId, atmId, "GET_SYSINFO", sent);
            return cmdId;
        }
        public int BroadcastRestart(int delaySeconds = 30)
        {
            var atms = _serverEngine.GetConnectedATMs();
            int sent = 0;
            foreach (var atm in atms) { SendRestart(atm.ATM_ID, delaySeconds); sent++; }
            OnLog?.Invoke("[Broadcast] RESTART -> " + sent + " ATMs");
            return sent;
        }
        public int BroadcastTimeSync()
        {
            var atms = _serverEngine.GetConnectedATMs();
            int sent = 0;
            foreach (var atm in atms) { SendTimeSync(atm.ATM_ID); sent++; }
            OnLog?.Invoke("[Broadcast] TIMESYNC -> " + sent + " ATMs");
            return sent;
        }
        public int BroadcastScreenshot()
        {
            var atms = _serverEngine.GetConnectedATMs();
            int sent = 0;
            foreach (var atm in atms) { SendScreenshot(atm.ATM_ID); sent++; }
            return sent;
        }
        public void ScheduleCommand(string atmId, string commandType, DateTime executeAt, string[] parameters)
        {
            lock (_lock)
            {
                if (!_scheduledCommands.ContainsKey(atmId))
                    _scheduledCommands[atmId] = new Queue<ScheduledCommand>();
                _scheduledCommands[atmId].Enqueue(new ScheduledCommand { ATM_ID = atmId, CommandType = commandType, ExecuteAt = executeAt, Parameters = parameters });
            }
            OnLog?.Invoke("[Schedule] " + commandType + " for " + atmId + " at " + executeAt.ToString("HH:mm:ss"));
        }
        private void ProcessScheduledCommands(object state)
        {
            lock (_lock)
            {
                foreach (var kvp in _scheduledCommands)
                {
                    while (kvp.Value.Count > 0 && kvp.Value.Peek().ExecuteAt <= DateTime.Now)
                    {
                        var cmd = kvp.Value.Dequeue();
                        switch (cmd.CommandType)
                        {
                            case Protocol.CMD_RESTART: SendRestart(cmd.ATM_ID); break;
                            case Protocol.CMD_SCREENSHOT: SendScreenshot(cmd.ATM_ID); break;
                            case Protocol.CMD_TIMESYNC: SendTimeSync(cmd.ATM_ID); break;
                            case Protocol.CMD_SHUTDOWN: SendShutdown(cmd.ATM_ID); break;
                            case Protocol.CMD_GET_SYSINFO: SendGetSystemInfo(cmd.ATM_ID); break;
                        }
                    }
                }
            }
        }
        private void HandleCommandResult(string atmId, string cmdId, string result)
        {
            lock (_lock)
            {
                if (_commandHistory.ContainsKey(cmdId))
                {
                    _commandHistory[cmdId].Completed = true;
                    _commandHistory[cmdId].CompletedAt = DateTime.Now;
                    _commandHistory[cmdId].Result = result;
                    _commandHistory[cmdId].Success = result.StartsWith("OK");
                }
            }
            bool success = result.StartsWith("OK");
            OnCommandResult?.Invoke(atmId, cmdId, success, result);
        }
        private void RecordCommand(string cmdId, string atmId, string cmdType, bool sent)
        {
            lock (_lock)
            {
                _commandHistory[cmdId] = new CommandRecord { CommandId = cmdId, ATM_ID = atmId, CommandType = cmdType, SentAt = DateTime.Now, Sent = sent };
            }
            if (sent) OnCommandSent?.Invoke(atmId, cmdType, cmdId);
        }
        public List<CommandRecord> GetCommandHistory(string atmId = null, int max = 100)
        {
            var result = new List<CommandRecord>();
            lock (_lock)
            {
                foreach (var r in _commandHistory.Values)
                {
                    if (atmId == null || r.ATM_ID == atmId) result.Add(r);
                    if (result.Count >= max) break;
                }
            }
            return result;
        }
        private string GenerateCommandId()
        {
            _commandCounter++;
            return "CMD_" + DateTime.Now.ToString("yyyyMMdd") + "_" + _commandCounter.ToString("D5");
        }
        _schedulerTimer = new System.Threading.Timer(ProcessScheduledCommands, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        public string SendRestart(string atmId, int delaySeconds = 10) =>
            SendCommand(atmId, AppConstants.CMD_RESTART, "RESTART", $"DelaySec={delaySeconds}");
        public string SendScreenshot(string atmId) =>
            SendCommand(atmId, AppConstants.CMD_SCREENSHOT, "SCREENSHOT", string.Empty);
        public string SendScreenshotNow(string atmId) => SendScreenshot(atmId);
        public string SendPing(string atmId, string pingId = "")
        {
            var effectivePingId = string.IsNullOrWhiteSpace(pingId)
                ? Guid.NewGuid().ToString("N")
                : pingId.Trim();
            return SendCommand(
                atmId,
                AppConstants.CMD_PING,
                "PING",
                BuildPayload(("PingId", effectivePingId), ("RequestedAtUtc", DateTime.UtcNow.ToString("O"))));
        }
        public string SendTimeSync(string atmId)
        {
            var serverTime = DateTime.UtcNow.ToString("O");
            return SendCommand(atmId, AppConstants.CMD_SYNC_TIME, "TIMESYNC", $"ServerUtc={serverTime}");
        }
        public string SendShutdown(string atmId, int delaySeconds = 30) =>
            SendCommand(atmId, AppConstants.CMD_SHUTDOWN, "SHUTDOWN", $"DelaySec={delaySeconds}");
        public string SendChangePassword(string atmId, string newPassword) =>
            SendCommand(atmId, AppConstants.CMD_CHANGE_PASSWORD, "CHANGE_PASSWORD", BuildPayload(("PasswordEnc", SecurityHelper.EncryptText(newPassword))));
        public string SendImageToInbox(string atmId, string imageName, byte[] imageData, string targetPath = "Inbox") =>
            SendImage(atmId, imageName, imageData, targetPath);
        public string SendImageDirectByAtmType(string atmId, string imageName, byte[] imageData, string atmType) =>
            SendImageDirect(atmId, imageName, imageData, atmType, string.Empty);
        public string SendUpdateConfig(string atmId, string configKey, string configValue) =>
            SendCommand(atmId, AppConstants.CMD_REMOTE_CONFIG, "UPDATE_CONFIG", $"{configKey}={configValue}");
        public string SendGetSystemInfo(string atmId) =>
            SendCommand(atmId, AppConstants.CMD_GET_STATS, "GET_SYSINFO", string.Empty);
        public string SendWindowsRemoteStart(string atmId) =>
            SendCommand(atmId, AppConstants.CMD_WINDOWS_REMOTE_START, "WINDOWS_REMOTE_START", BuildPayload());
        public string SendWindowsRemoteStop(string atmId) =>
            SendCommand(atmId, AppConstants.CMD_WINDOWS_REMOTE_STOP, "WINDOWS_REMOTE_STOP", BuildPayload());
        public string SendWindowsRemoteCheck(string atmId) =>
            SendCommand(atmId, AppConstants.CMD_WINDOWS_REMOTE_CHECK, "WINDOWS_REMOTE_CHECK", string.Empty);
            SendCommand(atmId, AppConstants.CMD_WINDOWS_REMOTE_CHECK, "WINDOWS_REMOTE_CHECK", BuildPayload());
        public static string GenerateShadowCommand(string targetIp, int sessionId = 1) =>
            GenerateShadowCommandString(
                targetIp,
                sessionId,
                control: true,
                requestNoConsentPrompt: false,
                promptForCredentials: true);
        public static string GenerateShadowCommandFromSessionPlan(string? sessionPlanWire)
        {
            if (string.IsNullOrWhiteSpace(sessionPlanWire))
                return "ShadowCommandBlocked: empty session plan.";
            var tokens = sessionPlanWire
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(token => token.Split(new[] { '=' }, 2))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim(), StringComparer.OrdinalIgnoreCase);
            if (!tokens.TryGetValue("mode", out var mode))
                return "ShadowCommandBlocked: missing mode in session plan.";
            if (!string.Equals(mode, "ShadowActiveSession", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(mode, "AdminMaintenanceSession", StringComparison.OrdinalIgnoreCase))
            {
                var reason = tokens.TryGetValue("reason", out var blockedReason) ? blockedReason : "mode is blocked";
                return "ShadowCommandBlocked: " + reason;
            }
            if (!tokens.TryGetValue("cmd", out var command) || string.IsNullOrWhiteSpace(command))
                return "ShadowCommandBlocked: missing command in session plan.";
            var trimmed = command.Trim();
            if (!trimmed.StartsWith("mstsc ", StringComparison.OrdinalIgnoreCase))
                return "ShadowCommandBlocked: session plan command is not mstsc.";
            return trimmed;
        }
        public string SendChangeWindowsPassword(string atmId, string userName, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Password is required.", nameof(newPassword));
            var normalizedUser = string.IsNullOrWhiteSpace(userName) ? string.Empty : userName.Trim();
            if (string.IsNullOrWhiteSpace(normalizedUser))
                throw new ArgumentException("Username is required.", nameof(userName));
            return SendCommand(
                atmId,
                AppConstants.CMD_CHANGE_PASSWORD,
                "CHANGE_WINDOWS_PASSWORD",
                BuildPayload(
                    ("Scope", "LOCAL_USER"),
                    ("User", normalizedUser),
                    ("PasswordEnc", SecurityHelper.EncryptText(newPassword))));
        }
        public string SendRequestJournalFile(string atmId, string filePath) =>
            SendCommand(atmId, AppConstants.CMD_SEND_FILE, "SEND_FILE", BuildPayload(("Path", filePath)));
        public string SendSyncImages(string atmId, string pathHint = "") =>
            SendCommand(atmId, AppConstants.CMD_SYNC_IMAGES, "SYNC_IMAGES", BuildPayload(("Path", pathHint)));
        public int BroadcastRestart(int delaySeconds = 30)
        {
            var sent = 0;
            foreach (var connection in _serverEngine.Connections)
            {
                SendRestart(connection.ATM_ID, delaySeconds);
                sent++;
            }
            OnLog?.Invoke("[Broadcast] RESTART -> " + sent + " ATMs");
            return sent;
        }
        public int BroadcastPing()
        {
            var sent = 0;
            foreach (var connection in _serverEngine.Connections)
            {
                SendPing(connection.ATM_ID);
                sent++;
            }
            OnLog?.Invoke("[Broadcast] PING -> " + sent + " ATMs");
            return sent;
        }
        public int BroadcastChangePassword(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return 0;
            var sent = 0;
            foreach (var connection in _serverEngine.Connections)
            {
                SendChangePassword(connection.ATM_ID, newPassword);
                sent++;
            }
            OnLog?.Invoke("[Broadcast] CHANGE_PASSWORD -> " + sent + " ATMs");
            return sent;
        }
        public int BroadcastTimeSync()
        {
            var sent = 0;
            foreach (var connection in _serverEngine.Connections)
            {
                SendTimeSync(connection.ATM_ID);
                sent++;
            }
            OnLog?.Invoke("[Broadcast] TIMESYNC -> " + sent + " ATMs");
            return sent;
        }
        public int BroadcastScreenshot()
        {
            var sent = 0;
            foreach (var connection in _serverEngine.Connections)
            {
                SendScreenshot(connection.ATM_ID);
                sent++;
            }
            OnLog?.Invoke("[Broadcast] SCREENSHOT -> " + sent + " ATMs");
            return sent;
        }
        public int BroadcastScreenshotNow() => BroadcastScreenshot();
        public int BroadcastWindowsRemoteStart()
        {
            var sent = 0;
            foreach (var connection in _serverEngine.Connections)
            {
                SendWindowsRemoteStart(connection.ATM_ID);
                sent++;
            }
            OnLog?.Invoke("[Broadcast] WINDOWS_REMOTE_START -> " + sent + " ATMs");
            return sent;
        }
        public int BroadcastWindowsRemoteStop()
        {
            var sent = 0;
            foreach (var connection in _serverEngine.Connections)
            {
                SendWindowsRemoteStop(connection.ATM_ID);
                sent++;
            }
            OnLog?.Invoke("[Broadcast] WINDOWS_REMOTE_STOP -> " + sent + " ATMs");
            return sent;
        }
        public int BroadcastWindowsRemoteCheck()
        {
            var sent = 0;
            foreach (var connection in _serverEngine.Connections)
            {
                SendWindowsRemoteCheck(connection.ATM_ID);
                sent++;
            }
            OnLog?.Invoke("[Broadcast] WINDOWS_REMOTE_CHECK -> " + sent + " ATMs");
            return sent;
        }
        public int BroadcastRequestJournalFile(string filePath)
        {
            var sent = 0;
            foreach (var connection in _serverEngine.Connections)
            {
                SendRequestJournalFile(connection.ATM_ID, filePath);
                sent++;
            }
            OnLog?.Invoke("[Broadcast] REQUEST_JOURNAL -> " + sent + " ATMs");
            return sent;
        }
        public int BroadcastSyncImages(string pathHint = "")
        {
            var sent = 0;
            foreach (var connection in _serverEngine.Connections)
            {
                SendSyncImages(connection.ATM_ID, pathHint);
                sent++;
            }
            OnLog?.Invoke("[Broadcast] SYNC_IMAGES -> " + sent + " ATMs");
            return sent;
        }
        public int BroadcastImageDirect(string imageName, byte[] imageData, Func<string, string> atmTypeResolver, string fallbackAtmType = AppConstants.ATM_TYPE_NCR)
        {
            if (atmTypeResolver == null)
                throw new ArgumentNullException(nameof(atmTypeResolver));
            var sent = 0;
            foreach (var connection in _serverEngine.Connections)
            {
                var atmType = atmTypeResolver(connection.ATM_ID);
                if (string.IsNullOrWhiteSpace(atmType))
                    atmType = fallbackAtmType;
                SendImageDirectByAtmType(connection.ATM_ID, imageName, imageData, atmType);
                sent++;
            }
            OnLog?.Invoke("[Broadcast] APPLY_IMAGE -> " + sent + " ATMs");
            return sent;
        }
        public void ScheduleCommand(string atmId, string commandType, DateTime executeAt, string[]? parameters)
        {
            var scheduled = new ScheduledCommand
            {
                ATM_ID = atmId ?? string.Empty,
                CommandType = commandType ?? string.Empty,
                ExecuteAt = executeAt,
                Parameters = parameters ?? Array.Empty<string>()
            };
            lock (_lock)
            {
                if (!_scheduledCommands.TryGetValue(scheduled.ATM_ID, out var queue))
                {
                    queue = new Queue<ScheduledCommand>();
                    _scheduledCommands[scheduled.ATM_ID] = queue;
                }
                queue.Enqueue(scheduled);
            }
            OnLog?.Invoke("[Schedule] " + scheduled.CommandType + " for " + scheduled.ATM_ID + " at " + executeAt.ToString("HH:mm:ss"));
        }
        public List<CommandRecord> GetCommandHistory(string? atmId = null, int max = 100)
        {
            lock (_lock)
            {
                return _commandHistory.Values
                    .Where(record => string.IsNullOrWhiteSpace(atmId) || string.Equals(record.ATM_ID, atmId, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(record => record.SentAt)
                    .Take(Math.Max(1, max))
                    .ToList();
            }
        }
        private string SendCommand(string atmId, string protocolCommandType, string displayType, string payload)
        {
            var commandId = GenerateCommandId();
            var finalPayload = EnsureServerMetadata(payload);
            var command = new RemoteCommandEnvelope
            {
                CommandId = commandId,
                CommandType = protocolCommandType,
                Payload = finalPayload,
                RequiresConfirmation = AppConstants.CommandsRequireConfirmation.Contains(protocolCommandType, StringComparer.OrdinalIgnoreCase)
            };
            var sent = _serverEngine.SendCommand(atmId, command);
            RecordCommand(commandId, atmId, displayType, sent);
            OnLog?.Invoke("[RemoteControl] " + displayType + " -> " + atmId + (sent ? string.Empty : " (no active connection)"));
            return commandId;
        }
        private void ProcessScheduledCommands(object? state)
        {
            lock (_lock)
            {
                foreach (var entry in _scheduledCommands.Values)
                {
                    while (entry.Count > 0 && entry.Peek().ExecuteAt <= DateTime.Now)
                    {
                        var command = entry.Dequeue();
                        switch (command.CommandType)
                        {
                            case var value when string.Equals(value, AppConstants.CMD_RESTART, StringComparison.OrdinalIgnoreCase):
                                SendRestart(command.ATM_ID);
                                break;
                            case var value when string.Equals(value, AppConstants.CMD_SCREENSHOT, StringComparison.OrdinalIgnoreCase):
                                SendScreenshot(command.ATM_ID);
                                break;
                            case var value when string.Equals(value, AppConstants.CMD_SYNC_TIME, StringComparison.OrdinalIgnoreCase):
                                SendTimeSync(command.ATM_ID);
                                break;
                            case var value when string.Equals(value, AppConstants.CMD_SHUTDOWN, StringComparison.OrdinalIgnoreCase):
                                SendShutdown(command.ATM_ID);
                                break;
                            case var value when string.Equals(value, AppConstants.CMD_GET_STATS, StringComparison.OrdinalIgnoreCase):
                                SendGetSystemInfo(command.ATM_ID);
                                break;
                            default:
                                SendCommand(command.ATM_ID, command.CommandType, command.CommandType, string.Join(";", command.Parameters));
                                break;
                        }
                    }
                }
            }
        }
        private void HandleServerLog(object? sender, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
            var match = CommandResultLine.Match(message);
            if (!match.Success)
                return;
            var atmId = match.Groups["atm"].Value.Trim();
            var commandId = match.Groups["cmd"].Value.Trim();
            var state = match.Groups["state"].Value.Trim();
            var resultMessage = match.Groups["message"].Value.Trim();
            var success = state.Equals("OK", StringComparison.OrdinalIgnoreCase);
            HandleCommandResult(atmId, commandId, success, resultMessage);
        }
        private void HandleCommandResult(string atmId, string commandId, bool success, string result)
        {
            lock (_lock)
            {
                if (_commandHistory.TryGetValue(commandId, out var record))
                {
                    record.Completed = true;
                    record.CompletedAt = DateTime.Now;
                    record.Result = result;
                    record.Success = success;
                }
            }
            OnCommandResult?.Invoke(atmId, commandId, success, result);
        }
        private void RecordCommand(string commandId, string atmId, string commandType, bool sent)
        {
            lock (_lock)
            {
                _commandHistory[commandId] = new CommandRecord
                {
                    CommandId = commandId,
                    ATM_ID = atmId ?? string.Empty,
                    CommandType = commandType ?? string.Empty,
                    SentAt = DateTime.Now,
                    Sent = sent,
                    Completed = false,
                    Success = false,
                    Result = sent ? "Queued" : "No active connection"
                };
            }
            if (sent)
                OnCommandSent?.Invoke(atmId ?? string.Empty, commandType ?? string.Empty, commandId);
        }
        private static string BuildPayload(params (string Key, string Value)[] pairs)
        {
            var tokens = new List<string>(pairs.Length + 3);
            foreach (var pair in pairs)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || string.IsNullOrWhiteSpace(pair.Value))
                    continue;
                tokens.Add(pair.Key + "=" + pair.Value);
            }
            return string.Join(";", tokens);
        }
        private static string EnsureServerMetadata(string payload)
        {
            var cleaned = string.IsNullOrWhiteSpace(payload) ? string.Empty : payload.Trim();
            var metadata = BuildPayload(
                ("Role", "Admin"),
                ("IssuedBy", "Server"),
                ("IssuedAt", DateTime.UtcNow.ToString("O")));
            if (string.IsNullOrWhiteSpace(cleaned))
                return metadata;
            return cleaned + ";" + metadata;
        }
        public void Dispose()
        {
            _serverEngine.Log -= HandleServerLog;
            _schedulerTimer.Dispose();
        }
        private void ProcessScheduledCommands(object? state)
        {
            lock (_lock)
            {
                foreach (var entry in _scheduledCommands.Values)
                {
                    while (entry.Count > 0 && entry.Peek().ExecuteAt <= DateTime.Now)
                    {
                        var command = entry.Dequeue();
                        switch (command.CommandType)
                        {
                            case var value when string.Equals(value, Protocol.CMD_RESTART, StringComparison.OrdinalIgnoreCase):
                                SendRestart(command.ATM_ID);
                                break;
                            case var value when string.Equals(value, Protocol.CMD_SCREENSHOT, StringComparison.OrdinalIgnoreCase):
                                SendScreenshot(command.ATM_ID);
                                break;
                            case var value when string.Equals(value, Protocol.CMD_TIMESYNC, StringComparison.OrdinalIgnoreCase):
                                SendTimeSync(command.ATM_ID);
                                break;
                            case var value when string.Equals(value, Protocol.CMD_SHUTDOWN, StringComparison.OrdinalIgnoreCase):
                                SendShutdown(command.ATM_ID);
                                break;
                            case var value when string.Equals(value, Protocol.CMD_GET_SYSINFO, StringComparison.OrdinalIgnoreCase):
                                SendGetSystemInfo(command.ATM_ID);
                                break;
                            default:
                                SendCommand(command.ATM_ID, command.CommandType, command.CommandType, string.Join(";", command.Parameters));
                                break;
                        }
                    }
                }
            }
        }
        public event Action<string>? OnLog;
        public event Action<string, string, bool, string>? OnCommandResult;
        public event Action<string, string, bool, string> OnCommandResult;
        public event Action<string> OnLog;
    }

    public partial class CommandRecord
    {
        public string CommandId { get; set; }
        public string ATM_ID { get; set; }
        public string CommandType { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public bool Sent { get; set; }
        public bool Completed { get; set; }
        public bool Success { get; set; }
        public string Result { get; set; }
    }

    public partial class ScheduledCommand
    {
        public string ATM_ID { get; set; }
        public string CommandType { get; set; }
        public DateTime ExecuteAt { get; set; }
        public string[] Parameters { get; set; }
    }

}
