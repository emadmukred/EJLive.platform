using System.Text.RegularExpressions;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Shared;
using AppConstants = EJLive.Core.AppConstants;

namespace EJLive.Server.Services;

/// <summary>
/// Policy-aware remote-control service backed by the ServerEngine command API.
/// </summary>
public sealed class RemoteControlService : IDisposable
{
    private static readonly Regex CommandResultLine = new(
        @"^Command result from (?<atm>[^:]+):\s*(?<cmd>[^\s]+)\s+(?<state>[^\s]+)\s*(?<message>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private readonly ServerEngine _serverEngine;
    private readonly Dictionary<string, CommandRecord> _commandHistory = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Queue<ScheduledCommand>> _scheduledCommands = new(StringComparer.OrdinalIgnoreCase);
    private readonly System.Threading.Timer _schedulerTimer;
    private readonly object _lock = new();
    private int _commandCounter;

    public event Action<string, string, string>? OnCommandSent;
    public event Action<string, string, bool, string>? OnCommandResult;
    public event Action<string>? OnLog;

    public RemoteControlService(ServerEngine serverEngine)
    {
        _serverEngine = serverEngine ?? throw new ArgumentNullException(nameof(serverEngine));
        _serverEngine.Log += HandleServerLog;
        _schedulerTimer = new System.Threading.Timer(ProcessScheduledCommands, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public static bool TryParseCommandResultLog(
        string? message,
        out string atmId,
        out string commandId,
        out bool success,
        out string detail)
    {
        atmId = string.Empty;
        commandId = string.Empty;
        success = false;
        detail = string.Empty;

        var match = CommandResultLine.Match((message ?? string.Empty).Trim());
        if (!match.Success)
            return false;

        atmId = match.Groups["atm"].Value.Trim();
        commandId = match.Groups["cmd"].Value.Trim();
        var state = match.Groups["state"].Value.Trim();
        detail = match.Groups["message"].Value.Trim();
        success = state.Equals("OK", StringComparison.OrdinalIgnoreCase) ||
                  state.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase);

        return atmId.Length > 0 && commandId.Length > 0;
    }

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

    public string SendImage(string atmId, string imageName, byte[] imageData, string targetPath = "Inbox")
    {
        var encoded = Convert.ToBase64String(imageData ?? Array.Empty<byte>());
        return SendCommand(
            atmId,
            "CMD_RECEIVE_IMAGE",
            "RECEIVE_IMAGE",
            BuildPayload(("File", imageName), ("Base64", encoded), ("Target", string.IsNullOrWhiteSpace(targetPath) ? "Inbox" : targetPath)));
    }

    public string SendImageDirect(string atmId, string imageName, byte[] imageData, string atmType, string targetPath = "")
    {
        var encoded = Convert.ToBase64String(imageData ?? Array.Empty<byte>());
        var payloadParts = new List<(string Key, string Value)>
        {
            ("File", imageName),
            ("Base64", encoded),
            ("Mode", "Direct"),
            ("ATMType", AppConstants.NormalizeATMType(atmType))
        };
        if (!string.IsNullOrWhiteSpace(targetPath))
            payloadParts.Add(("Target", targetPath.Trim()));

        return SendCommand(
            atmId,
            "CMD_APPLY_IMAGE",
            "APPLY_IMAGE",
            BuildPayload(payloadParts.ToArray()));
    }

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
        SendCommand(atmId, AppConstants.CMD_WINDOWS_REMOTE_CHECK, "WINDOWS_REMOTE_CHECK", BuildPayload());

    public static string GenerateShadowCommandString(
        string targetIp,
        int sessionId,
        bool control = true,
        bool requestNoConsentPrompt = false,
        bool promptForCredentials = true)
    {
        if (sessionId <= 0)
            return "ShadowCommandBlocked: invalid session id.";

        if (string.IsNullOrWhiteSpace(targetIp))
            return "ShadowCommandBlocked: target ip/host is required.";

        var target = targetIp.Trim();
        if (target.IndexOfAny(new[] { ' ', '\t', ';', '|', '&', '"' }) >= 0)
            return "ShadowCommandBlocked: invalid target ip/host.";

        var args = new List<string> { $"/v:{target}" };
        if (promptForCredentials)
            args.Add("/prompt");
        args.Add($"/shadow:{sessionId}");
        if (control)
            args.Add("/control");
        if (requestNoConsentPrompt)
            args.Add("/noconsentprompt");
        return "mstsc " + string.Join(" ", args);
    }

    // Concise entry point retained for callers that do not need session-plan options.
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

    public int BroadcastImage(string imageName, byte[] imageData, string targetPath = "Inbox")
    {
        var sent = 0;
        foreach (var connection in _serverEngine.Connections)
        {
            SendImage(connection.ATM_ID, imageName, imageData, targetPath);
            sent++;
        }
        OnLog?.Invoke("[Broadcast] DISTRIBUTE_IMAGE -> " + sent + " ATMs");
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

    private void HandleServerLog(object? sender, string message)
    {
        if (!TryParseCommandResultLog(message, out var atmId, out var commandId, out var success, out var resultMessage))
            return;

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

    private string GenerateCommandId()
    {
        _commandCounter++;
        return "CMD_" + DateTime.Now.ToString("yyyyMMdd") + "_" + _commandCounter.ToString("D5");
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

    public static string BuildAtmTypeImageTarget(string? atmType)
    {
        var normalized = AppConstants.NormalizeATMType(atmType);
        return Path.Combine("Inbox", "ByType", normalized);
    }

    public void Dispose()
    {
        _serverEngine.Log -= HandleServerLog;
        _schedulerTimer.Dispose();
    }
}

public sealed class CommandRecord
{
    public string CommandId { get; set; } = string.Empty;
    public string ATM_ID { get; set; } = string.Empty;
    public string CommandType { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public bool Sent { get; set; }
    public bool Completed { get; set; }
    public bool Success { get; set; }
    public string Result { get; set; } = string.Empty;
}

public sealed class ScheduledCommand
{
    public string ATM_ID { get; set; } = string.Empty;
    public string CommandType { get; set; } = string.Empty;
    public DateTime ExecuteAt { get; set; }
    public string[] Parameters { get; set; } = Array.Empty<string>();
}
