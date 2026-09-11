using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;
using AppConfig = EJLive.Shared.AppConfig;
using AppConstants = EJLive.Core.AppConstants;

namespace EJLive.Client.WinForms.Services
{
    /// <summary>
    /// Unified remote command handler for legacy and modern command payloads.
    /// </summary>
    public sealed class RemoteCommandHandler
    {
        private readonly string _atmId;
        private readonly NetworkManager? _network;
        private readonly Action? _forceSyncAction;
        private readonly Dictionary<string, RemoteCommand> _history = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _sync = new();
        private bool _ghostActive;
        private static readonly HashSet<string> AllowedCommands = new(StringComparer.OrdinalIgnoreCase)
        {
            AppConstants.CMD_RESTART,
            AppConstants.CMD_SHUTDOWN,
            AppConstants.CMD_CHANGE_PASSWORD,
            AppConstants.CMD_SCREENSHOT,
            AppConstants.CMD_SYNC_TIME,
            AppConstants.CMD_GHOST_START,
            AppConstants.CMD_GHOST_STOP,
            AppConstants.CMD_SEND_FILE,
            AppConstants.CMD_GET_FILE,
            AppConstants.CMD_SEND_IMAGE,
            "CMD_RECEIVE_IMAGE",
            "CMD_APPLY_IMAGE",
            "CMD_RECEIVE_FILE",
            "CMD_APPLY_FILE",
            AppConstants.CMD_SYNC_IMAGES,
            AppConstants.CMD_FORCE_SYNC,
            AppConstants.CMD_REMOTE_CONFIG,
            AppConstants.CMD_GET_STATS,
            AppConstants.CMD_SYNC_FOLDER,
            AppConstants.CMD_WINDOWS_REMOTE_START,
            AppConstants.CMD_WINDOWS_REMOTE_STOP,
            AppConstants.CMD_WINDOWS_REMOTE_CHECK,
            AppConstants.CMD_PING
        };

        private static readonly Dictionary<string, string> RequiredRoleByCommand = new(StringComparer.OrdinalIgnoreCase)
        {
            [AppConstants.CMD_PING] = "Observer",
            [AppConstants.CMD_GET_STATS] = "Observer",
            [AppConstants.CMD_SCREENSHOT] = "Support",
            [AppConstants.CMD_SYNC_TIME] = "Support",
            [AppConstants.CMD_GHOST_START] = "Support",
            [AppConstants.CMD_GHOST_STOP] = "Support",
            [AppConstants.CMD_SEND_FILE] = "Support",
            [AppConstants.CMD_GET_FILE] = "Support",
            [AppConstants.CMD_SEND_IMAGE] = "Support",
            ["CMD_RECEIVE_IMAGE"] = "Support",
            ["CMD_APPLY_IMAGE"] = "Support",
            [AppConstants.CMD_SYNC_IMAGES] = "Support",
            [AppConstants.CMD_FORCE_SYNC] = "Support",
            [AppConstants.CMD_RESTART] = "Admin",
            [AppConstants.CMD_SHUTDOWN] = "Admin",
            [AppConstants.CMD_CHANGE_PASSWORD] = "Admin",
            [AppConstants.CMD_REMOTE_CONFIG] = "Admin",
            [AppConstants.CMD_SYNC_FOLDER] = "Admin",
            [AppConstants.CMD_WINDOWS_REMOTE_START] = "Admin",
            [AppConstants.CMD_WINDOWS_REMOTE_STOP] = "Admin",
            [AppConstants.CMD_WINDOWS_REMOTE_CHECK] = "Support",
            ["CMD_RECEIVE_FILE"] = "Admin",
            ["CMD_APPLY_FILE"] = "Admin"
        };

        public event EventHandler<RemoteCommand>? OnCommandReceived;
        public event EventHandler<RemoteCommand>? OnCommandExecuted;
        public event EventHandler<byte[]>? OnGhostFrameSent;
        public event Action<string>? OnLogMessage;

        public bool AllowProcessControl { get; set; }

        public RemoteCommandHandler(string atmId, NetworkManager? network = null, Action? forceSyncAction = null)
        {
            _atmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
            _network = network;
            _forceSyncAction = forceSyncAction;
        }

        public IReadOnlyList<RemoteCommand> GetCommandHistory(int max = 200)
        {
            lock (_sync)
            {
                return _history.Values
                    .OrderByDescending(item => item.CreatedAtUtc)
                    .Take(Math.Max(1, max))
                    .ToArray();
            }
        }

        public void HandleCommand(EJMessage message)
        {
            if (message == null)
                return;

            if (RemoteCommandEnvelope.TryParse(message.Text, out var envelope))
            {
                var command = BuildCommand(envelope.CommandType, envelope.Payload, envelope.CommandId);
                ExecuteCommand(command);
                return;
            }

            var config = AppConfig.Load();
            if (!config.AllowUnsignedLegacyCommands)
            {
                Log("Command rejected: signature verification failed and unsigned legacy fallback is disabled.");
                return;
            }

            var parts = (message.Text ?? string.Empty).Split('|', StringSplitOptions.None);
            if (parts.Length == 0)
                return;

            var commandType = parts.Length > 1 ? parts[1] : parts[0];
            var commandId = parts.Length > 2 ? parts[2] : Guid.NewGuid().ToString("N");
            var payload = parts.Length > 3 ? string.Join("|", parts.Skip(3)) : string.Empty;
            ExecuteCommand(BuildCommand(commandType, payload, commandId));
        }

        public void ProcessCommand(string command, string parameters)
        {
            var mapped = MapLegacyCommand(command);
            ExecuteCommand(BuildCommand(mapped, parameters ?? string.Empty, Guid.NewGuid().ToString("N")));
        }

        public RemoteCommand ExecuteCommand(RemoteCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            command.ATM_ID = _atmId;
            command.Status = RemoteCommandStatus.Running;
            command.SentAtUtc = DateTime.UtcNow;

            lock (_sync)
                _history[command.CommandId] = command;

            OnCommandReceived?.Invoke(this, command);
            Log($"Command received: {command.CommandType} [{command.CommandId}]");
            AuditCommand(command, true, "received");

            var auth = EvaluateAuthorization(command);
            if (!auth.Allowed)
            {
                Complete(command, false, auth.Reason);
                return command;
            }

            try
            {
                var normalized = NormalizeCommandType(command.CommandType);
                switch (normalized)
                {
                    case var value when string.Equals(value, AppConstants.CMD_RESTART, StringComparison.OrdinalIgnoreCase):
                        ExecuteRestart(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_SHUTDOWN, StringComparison.OrdinalIgnoreCase):
                        ExecuteShutdown(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_CHANGE_PASSWORD, StringComparison.OrdinalIgnoreCase):
                        ExecuteChangePassword(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_SCREENSHOT, StringComparison.OrdinalIgnoreCase):
                        ExecuteScreenshot(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_SYNC_TIME, StringComparison.OrdinalIgnoreCase):
                        ExecuteSyncTime(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_GHOST_START, StringComparison.OrdinalIgnoreCase):
                        ExecuteGhostStart(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_GHOST_STOP, StringComparison.OrdinalIgnoreCase):
                        ExecuteGhostStop(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_SEND_FILE, StringComparison.OrdinalIgnoreCase):
                        ExecuteSendFile(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_GET_FILE, StringComparison.OrdinalIgnoreCase):
                        ExecuteGetFile(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_SEND_IMAGE, StringComparison.OrdinalIgnoreCase):
                        ExecuteSendImage(command);
                        break;
                    case var value when string.Equals(value, "CMD_RECEIVE_IMAGE", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(value, "CMD_APPLY_IMAGE", StringComparison.OrdinalIgnoreCase):
                        ExecuteReceiveImage(
                            command,
                            applyDirect: string.Equals(value, "CMD_APPLY_IMAGE", StringComparison.OrdinalIgnoreCase));
                        break;
                    case var value when string.Equals(value, "CMD_RECEIVE_FILE", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(value, "CMD_APPLY_FILE", StringComparison.OrdinalIgnoreCase):
                        ExecuteReceiveFile(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_SYNC_IMAGES, StringComparison.OrdinalIgnoreCase):
                        ExecuteSyncImages(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_FORCE_SYNC, StringComparison.OrdinalIgnoreCase):
                        ExecuteForceSync(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_REMOTE_CONFIG, StringComparison.OrdinalIgnoreCase):
                        ExecuteRemoteConfig(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_GET_STATS, StringComparison.OrdinalIgnoreCase):
                        ExecuteGetStats(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_SYNC_FOLDER, StringComparison.OrdinalIgnoreCase):
                        ExecuteSyncFolder(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_WINDOWS_REMOTE_START, StringComparison.OrdinalIgnoreCase):
                        ExecuteWindowsRemoteAccess(command, true);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_WINDOWS_REMOTE_STOP, StringComparison.OrdinalIgnoreCase):
                        ExecuteWindowsRemoteAccess(command, false);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_WINDOWS_REMOTE_CHECK, StringComparison.OrdinalIgnoreCase):
                        ExecuteWindowsRemoteCheck(command);
                        break;
                    case var value when string.Equals(value, AppConstants.CMD_PING, StringComparison.OrdinalIgnoreCase):
                        ExecutePing(command);
                        break;
                    default:
                        Complete(command, false, "Unknown command: " + command.CommandType);
                        break;
                }
            }
            catch (Exception ex)
            {
                Complete(command, false, ex.Message);
            }

            return command;
        }

        private void ExecuteRestart(RemoteCommand command)
        {
            if (!AllowProcessControl)
            {
                Complete(command, true, "Restart acknowledged (process control disabled).");
                return;
            }

            Complete(command, true, "Restarting EJLive client.");
            System.Windows.Forms.Application.Restart();
        }

        private void ExecuteShutdown(RemoteCommand command)
        {
            if (!AllowProcessControl)
            {
                Complete(command, true, "Shutdown acknowledged (process control disabled).");
                return;
            }

            Complete(command, true, "Closing EJLive client.");
            System.Windows.Forms.Application.Exit();
        }

        private void ExecuteChangePassword(RemoteCommand command)
        {
            var payloadMap = ParseKeyValuePayload(command.Payload);
            var scope = ResolvePasswordScope(payloadMap);
            if (!IsSupportedPasswordScope(scope))
            {
                Complete(command, false, $"Password scope '{scope}' is not allowed. Supported scopes: APP, AGENT, LOCAL_USER.");
                return;
            }

            var password = ResolveCommandPassword(command.Payload, payloadMap);
            if (string.IsNullOrWhiteSpace(password))
            {
                Complete(command, false, "Missing password payload.");
                return;
            }

            if (IsLocalWindowsPasswordScope(scope))
            {
                var config = AppConfig.Load();
                if (!config.AllowLocalWindowsPasswordChange)
                {
                    Complete(command, false, "Local Windows password change is disabled by client policy.");
                    return;
                }

                if (config.RequireEncryptedWindowsPasswordPayload && !HasEncryptedPasswordPayload(payloadMap))
                {
                    Complete(command, false, "Encrypted password payload is required for LOCAL_USER scope. Provide PasswordEncRsa or PasswordEnc.");
                    return;
                }

                var account = ResolveTargetAccount(payloadMap);
                if (string.IsNullOrWhiteSpace(account))
                {
                    Complete(command, false, "Missing target account. Provide User or Username in payload.");
                    return;
                }

                var allowedAccounts = ParseAllowedAccounts(config.AllowedPasswordAccounts);
                if (allowedAccounts.Count == 0)
                {
                    Complete(command, false, "No allowed accounts configured for local Windows password change.");
                    return;
                }

                var normalizedAccount = NormalizeAccountName(account);
                if (!allowedAccounts.Contains(normalizedAccount))
                {
                    if (config.AutoEnableRemoteAccess)
                    {
                        allowedAccounts.Add(normalizedAccount);
                        config.AllowedPasswordAccounts = string.Join(",", allowedAccounts.OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
                        config.Save();
                        AgentConfigurationXmlService.SaveAppConfig(config);
                        Log($"Local policy extended: account '{account}' added to password-rotation allowlist.");
                    }
                    else
                    {
                        Complete(command, false, $"Account '{account}' is not in allowed password-rotation list.");
                        return;
                    }
                }

                var passwordResult = WindowsRemoteAccessService.SetLocalUserPassword(account, password);
                Complete(command, passwordResult.Success, passwordResult.Message);
                return;
            }

            var passwordHash = SecurityHelper.HashPassword(password);
            var passwordFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive",
                "Client",
                "password.hash");
            Directory.CreateDirectory(Path.GetDirectoryName(passwordFile) ?? AppDomain.CurrentDomain.BaseDirectory);
            File.WriteAllText(passwordFile, passwordHash);
            var label = string.Equals(scope, "AGENT", StringComparison.OrdinalIgnoreCase) ? "Agent" : "Application";
            Complete(command, true, $"{label} password hash updated.");
        }

        private void ExecuteScreenshot(RemoteCommand command)
        {
            byte[]? bytes = null;
            if (SessionCompanionIpcClient.IsSessionZeroLikely())
            {
                if (SessionCompanionIpcClient.TryCaptureScreenshot(out var companionFrame, out var companionDetail))
                {
                    bytes = companionFrame;
                    Log("Screenshot captured via Session Companion IPC.");
                }
                else
                {
                    Log("Session Companion capture unavailable: " + companionDetail);
                }
            }

            bytes ??= CaptureScreenJpeg();
            if (bytes == null || bytes.Length == 0)
            {
                Complete(command, false, "Screenshot capture failed.");
                return;
            }

            var path = SaveScreenshot(bytes);
            _network?.SendMessage(CommunicationProtocol.BuildGhostFrame(bytes));
            OnGhostFrameSent?.Invoke(this, bytes);
            Complete(command, true, $"Screenshot captured: {path}");
        }

        private void ExecuteSyncTime(RemoteCommand command)
        {
            if (TryReadServerUtc(command.Payload, out var serverUtc))
            {
                var drift = (DateTime.UtcNow - serverUtc).TotalSeconds;
                Complete(command, true, $"ClientUtc={DateTime.UtcNow:O};ServerUtc={serverUtc:O};DriftSec={drift:F1}");
                return;
            }

            Complete(command, true, $"ClientUtc={DateTime.UtcNow:O}");
        }

        private void ExecuteGhostStart(RemoteCommand command)
        {
            if (_ghostActive)
            {
                Complete(command, true, "Ghost streaming already active.");
                return;
            }
            _ghostActive = true;
            ExecuteScreenshot(command);
        }

        private void ExecuteGhostStop(RemoteCommand command)
        {
            if (!_ghostActive)
            {
                Complete(command, true, "Ghost streaming already stopped.");
                return;
            }
            _ghostActive = false;
            Complete(command, true, "Ghost streaming stopped.");
        }

        private void ExecuteSendFile(RemoteCommand command)
        {
            if (_network == null)
            {
                Complete(command, false, "Network manager is not configured.");
                return;
            }

            var path = ResolvePath(command.Payload, AppConfig.Load().SourcePath);
            if (!File.Exists(path))
            {
                Complete(command, false, "File not found: " + path);
                return;
            }

            var data = SecurityHelper.ReadFileSafe(path) ?? Array.Empty<byte>();
            if (data.Length == 0)
            {
                Complete(command, false, "File is empty or inaccessible.");
                return;
            }

            var sent = _network.SendJournalFile(Path.GetFileName(path), data, 0, SecurityHelper.SHA256Hash(data));
            Complete(command, sent, sent ? "File sent." : "Failed to send file.");
        }

        private void ExecuteGetFile(RemoteCommand command)
        {
            ExecuteSendFile(command);
        }

        private void ExecuteSendImage(RemoteCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Payload) ||
                string.Equals(command.Payload, "SCREENSHOT", StringComparison.OrdinalIgnoreCase))
            {
                ExecuteScreenshot(command);
                return;
            }

            ExecuteSendFile(command);
        }

        private void ExecuteReceiveImage(RemoteCommand command, bool applyDirect = false)
        {
            var config = AppConfig.Load();
            var imageRoot = applyDirect
                ? ResolveDirectImageRoot(config, command.Payload)
                : string.IsNullOrWhiteSpace(config.ImageInboxPath)
                    ? Path.Combine(AppConstants.DefaultImagesPath, _atmId)
                    : config.ImageInboxPath;
            ExecuteReceiveBinary(command, imageRoot, new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp" }, "image");
        }

        private void ExecuteReceiveFile(RemoteCommand command)
        {
            var config = AppConfig.Load();
            var fileRoot = string.IsNullOrWhiteSpace(config.BackupPath)
                ? AppConstants.DefaultClientInboxPath
                : config.BackupPath;
            ExecuteReceiveBinary(command, fileRoot, Array.Empty<string>(), "file");
        }

        private void ExecuteReceiveBinary(RemoteCommand command, string defaultRoot, IReadOnlyCollection<string> allowedExtensions, string label)
        {
            var map = ParseKeyValuePayload(command.Payload);
            var payload = map.TryGetValue("BASE64", out var base64Payload) ? base64Payload : command.Payload;
            if (!TryDecodeBase64Payload(payload, out var bytes))
            {
                Complete(command, false, $"Invalid {label} payload. Expected BASE64 content.");
                return;
            }

            var fileName = map.TryGetValue("FILE", out var file) ? file :
                           map.TryGetValue("FILENAME", out var fileNameValue) ? fileNameValue :
                           $"{label}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bin";
            fileName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = $"{label}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bin";

            if (allowedExtensions.Count > 0 &&
                !allowedExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            {
                fileName += ".bin";
            }

            var targetHint = map.TryGetValue("TARGET", out var target) ? target :
                             map.TryGetValue("PATH", out var targetPath) ? targetPath :
                             string.Empty;
            var targetFolder = ResolveAllowedTargetFolder(targetHint, defaultRoot);
            if (string.IsNullOrWhiteSpace(targetFolder))
            {
                Complete(command, false, "Rejected target path. Allowed roots: image inbox, backup, source.");
                return;
            }

            var expectedHash = ResolveExpectedHash(map);
            var actualPayloadHash = SecurityHelper.SHA256Hash(bytes);
            if (!string.IsNullOrWhiteSpace(expectedHash) &&
                !HashEquals(actualPayloadHash, expectedHash))
            {
                Complete(command, false, $"{label} checksum mismatch before staging.");
                return;
            }

            if (TryReadExpectedSize(map, out var expectedSize) && expectedSize != bytes.LongLength)
            {
                Complete(command, false, $"{label} size mismatch before staging.");
                return;
            }

            Directory.CreateDirectory(targetFolder);
            var stagingFolder = Path.Combine(targetFolder, ".staging");
            Directory.CreateDirectory(stagingFolder);
            PruneStagingFolder(stagingFolder);

            var stageFileName = BuildStagingFileName(fileName, actualPayloadHash);
            var stagingPath = Path.Combine(stagingFolder, stageFileName);
            var finalPath = Path.Combine(targetFolder, fileName);
            try
            {
                File.WriteAllBytes(stagingPath, bytes);

                var stagedBytes = SecurityHelper.ReadFileSafe(stagingPath) ?? bytes;
                var stagedHash = SecurityHelper.SHA256Hash(stagedBytes);
                if (TryReadExpectedSize(map, out expectedSize) && expectedSize != stagedBytes.LongLength)
                {
                    SafeDelete(stagingPath);
                    Complete(command, false, $"{label} size mismatch after staging.");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(expectedHash) &&
                    !HashEquals(stagedHash, expectedHash))
                {
                    SafeDelete(stagingPath);
                    Complete(command, false, $"{label} checksum mismatch after staging.");
                    return;
                }

                File.Move(stagingPath, finalPath, overwrite: true);
                PruneStagingFolder(stagingFolder);
                Complete(command, true, $"{label} received: {finalPath} ({stagedBytes.LongLength} bytes, sha256={stagedHash})");
            }
            catch (Exception ex)
            {
                SafeDelete(stagingPath);
                Complete(command, false, $"{label} staging/promote failed: {ex.Message}");
            }
        }

        private void ExecuteSyncImages(RemoteCommand command)
        {
            if (_network == null)
            {
                Complete(command, false, "Network manager is not configured.");
                return;
            }

            var config = AppConfig.Load();
            var imagesRoot = string.IsNullOrWhiteSpace(command.Payload)
                ? Path.Combine(config.BackupPath, "Images")
                : ResolvePath(command.Payload, config.BackupPath);

            if (!Directory.Exists(imagesRoot))
            {
                Complete(command, false, "Images path not found: " + imagesRoot);
                return;
            }

            var files = Directory.EnumerateFiles(imagesRoot)
                .Where(path => path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (files.Length == 0)
            {
                Complete(command, false, "No image files found.");
                return;
            }

            var sent = 0;
            foreach (var file in files)
            {
                var data = SecurityHelper.ReadFileSafe(file) ?? Array.Empty<byte>();
                if (data.Length == 0)
                    continue;
                if (_network.SendJournalFile("IMG_" + Path.GetFileName(file), data, 0, SecurityHelper.SHA256Hash(data)))
                    sent++;
            }

            Complete(command, sent == files.Length, $"Image sync sent={sent}, total={files.Length}");
        }

        private void ExecuteForceSync(RemoteCommand command)
        {
            _forceSyncAction?.Invoke();
            Complete(command, true, "Force sync started.");
        }

        private void ExecuteRemoteConfig(RemoteCommand command)
        {
            var config = AppConfig.Load();
            var updated = 0;

            foreach (var pair in ParseKeyValuePayload(command.Payload))
            {
                switch (pair.Key.ToUpperInvariant())
                {
                    case "SERVERIP":
                        config.ServerIP = pair.Value ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(config.ScopedFirewallRemoteAddresses) &&
                            !string.IsNullOrWhiteSpace(config.ServerIP))
                        {
                            config.ScopedFirewallRemoteAddresses = config.ServerIP.Trim();
                            updated++;
                        }
                        updated++;
                        break;
                    case "SERVERPORT":
                        if (int.TryParse(pair.Value, out var port))
                        {
                            config.ServerPort = port;
                            updated++;
                        }
                        break;
                    case "NETWORKTYPE":
                        config.NetworkType = pair.Value ?? string.Empty;
                        updated++;
                        break;
                    case "ATM_ID":
                    case "ATMID":
                        config.ATM_ID = pair.Value ?? string.Empty;
                        updated++;
                        break;
                    case "ATM_NAME":
                    case "ATMNAME":
                        config.ATM_Name = pair.Value ?? string.Empty;
                        updated++;
                        break;
                    case "ATM_TYPE":
                    case "ATMTYPE":
                        config.ATM_Type = AppConstants.NormalizeATMType(pair.Value);
                        updated++;
                        break;
                    case "SOURCEPATH":
                        config.SourcePath = pair.Value ?? string.Empty;
                        updated++;
                        break;
                    case "BACKUPPATH":
                        config.BackupPath = pair.Value ?? string.Empty;
                        updated++;
                        break;
                    case "AUTOCONNECT":
                        if (bool.TryParse(pair.Value, out var autoConnect))
                        {
                            config.AutoConnect = autoConnect;
                            updated++;
                        }
                        break;
                    case "ENCRYPT":
                    case "ENABLEENCRYPTION":
                        if (bool.TryParse(pair.Value, out var encryption))
                        {
                            config.EnableEncryption = encryption;
                            updated++;
                        }
                        break;
                    case "COMPRESS":
                    case "ENABLECOMPRESSION":
                        if (bool.TryParse(pair.Value, out var compression))
                        {
                            config.EnableCompression = compression;
                            updated++;
                        }
                        break;
                    case "ENABLETLSTRANSPORT":
                        if (bool.TryParse(pair.Value, out var enableTlsTransport))
                        {
                            config.EnableTlsTransport = enableTlsTransport;
                            updated++;
                        }
                        break;
                    case "REQUIRETLSTRANSPORT":
                        if (bool.TryParse(pair.Value, out var requireTlsTransport))
                        {
                            config.RequireTlsTransport = requireTlsTransport;
                            updated++;
                        }
                        break;
                    case "ALLOWUNTRUSTEDTLSCERTIFICATE":
                        if (bool.TryParse(pair.Value, out var allowUntrustedTlsCertificate))
                        {
                            config.AllowUntrustedTlsCertificate = allowUntrustedTlsCertificate;
                            updated++;
                        }
                        break;
                    case "ENABLEADAPTIVECHUNKING":
                        if (bool.TryParse(pair.Value, out var enableAdaptiveChunking))
                        {
                            config.EnableAdaptiveChunking = enableAdaptiveChunking;
                            updated++;
                        }
                        break;
                    case "WEAKNETWORKLATENCYMS":
                        if (int.TryParse(pair.Value, out var weakNetworkLatencyMs))
                        {
                            config.WeakNetworkLatencyMs = Math.Clamp(weakNetworkLatencyMs, 120, 3000);
                            updated++;
                        }
                        break;
                    case "AUTOBACKUP":
                        if (bool.TryParse(pair.Value, out var autoBackup))
                        {
                            config.AutoBackup = autoBackup;
                            updated++;
                        }
                        break;
                    case "HEARTBEATINTERVALSEC":
                        if (int.TryParse(pair.Value, out var heartbeatSec))
                        {
                            config.HeartbeatIntervalSec = Math.Clamp(heartbeatSec, 5, 300);
                            updated++;
                        }
                        break;
                    case "RECONNECTINTERVALSEC":
                        if (int.TryParse(pair.Value, out var reconnectSec))
                        {
                            config.ReconnectIntervalSec = Math.Clamp(reconnectSec, 5, 300);
                            updated++;
                        }
                        break;
                    case "AUTOENABLEREMOTEACCESS":
                        if (bool.TryParse(pair.Value, out var autoEnableRemote))
                        {
                            config.AutoEnableRemoteAccess = true;
                            if (!autoEnableRemote)
                                Log("Policy enforcement: AutoEnableRemoteAccess forced to true.");
                            updated++;
                        }
                        break;
                    case "AUTOPREPAREWINDOWSRUNTIME":
                        if (bool.TryParse(pair.Value, out var autoPrepareRuntime))
                        {
                            config.AutoPrepareWindowsRuntime = true;
                            if (!autoPrepareRuntime)
                                Log("Policy enforcement: AutoPrepareWindowsRuntime forced to true.");
                            updated++;
                        }
                        break;
                    case "ENABLEWINRMBOOTSTRAP":
                        if (bool.TryParse(pair.Value, out var enableWinRmBootstrap))
                        {
                            config.EnableWinRmBootstrap = true;
                            if (!enableWinRmBootstrap)
                                Log("Policy enforcement: EnableWinRmBootstrap forced to true.");
                            updated++;
                        }
                        break;
                    case "ENABLEREMOTEREGISTRYBOOTSTRAP":
                        if (bool.TryParse(pair.Value, out var enableRemoteRegistryBootstrap))
                        {
                            config.EnableRemoteRegistryBootstrap = true;
                            if (!enableRemoteRegistryBootstrap)
                                Log("Policy enforcement: EnableRemoteRegistryBootstrap forced to true.");
                            updated++;
                        }
                        break;
                    case "ENFORCESCOPEDFIREWALLRULE":
                        if (bool.TryParse(pair.Value, out var enforceScopedFirewallRule))
                        {
                            config.EnforceScopedFirewallRule = true;
                            if (!enforceScopedFirewallRule)
                                Log("Policy enforcement: EnforceScopedFirewallRule forced to true.");
                            updated++;
                        }
                        break;
                    case "SCOPEDFIREWALLPORT":
                        if (int.TryParse(pair.Value, out var scopedFirewallPort))
                        {
                            config.ScopedFirewallPort = Math.Clamp(scopedFirewallPort, 0, 65535);
                            updated++;
                        }
                        break;
                    case "SCOPEDFIREWALLREMOTEADDRESSES":
                        config.ScopedFirewallRemoteAddresses = pair.Value ?? string.Empty;
                        updated++;
                        break;
                    case "CONFIGUREDEFENDEREXCLUSIONS":
                        if (bool.TryParse(pair.Value, out var configureDefenderExclusions))
                        {
                            config.ConfigureDefenderExclusions = true;
                            if (!configureDefenderExclusions)
                                Log("Policy enforcement: ConfigureDefenderExclusions forced to true.");
                            updated++;
                        }
                        break;
                    case "DEFENDEREXCLUSIONPATHS":
                        config.DefenderExclusionPaths = pair.Value ?? string.Empty;
                        updated++;
                        break;
                    case "HELPDESKADGROUP":
                        config.HelpdeskAdGroup = pair.Value ?? string.Empty;
                        updated++;
                        break;
                    case "WINDOWSBASELINEREPAIRINTERVALMIN":
                        if (int.TryParse(pair.Value, out var baselineInterval))
                        {
                            config.WindowsBaselineRepairIntervalMin = Math.Clamp(baselineInterval, 5, 720);
                            updated++;
                        }
                        break;
                    case "WINDOWSPOLICYPROFILEMODE":
                        if (!string.IsNullOrWhiteSpace(pair.Value))
                        {
                            config.WindowsPolicyProfileMode = pair.Value.Trim();
                            updated++;
                        }
                        break;
                    case "ENFORCECOMMANDAUTHORIZATION":
                        if (bool.TryParse(pair.Value, out var enforceAuth))
                        {
                            config.EnforceCommandAuthorization = enforceAuth;
                            updated++;
                        }
                        break;
                    case "DEFAULTCOMMANDROLE":
                        if (!string.IsNullOrWhiteSpace(pair.Value))
                        {
                            config.DefaultCommandRole = pair.Value.Trim();
                            updated++;
                        }
                        break;
                    case "IMAGEINBOXPATH":
                        if (!string.IsNullOrWhiteSpace(pair.Value))
                        {
                            config.ImageInboxPath = pair.Value.Trim();
                            updated++;
                        }
                        break;
                    case "ALLOWLOCALWINDOWSPASSWORDCHANGE":
                        if (bool.TryParse(pair.Value, out var allowLocalPasswordChange))
                        {
                            config.AllowLocalWindowsPasswordChange = true;
                            if (!allowLocalPasswordChange)
                                Log("Policy enforcement: AllowLocalWindowsPasswordChange forced to true.");
                            updated++;
                        }
                        break;
                    case "REQUIREENCRYPTEDWINDOWSPASSWORDPAYLOAD":
                        if (bool.TryParse(pair.Value, out var requireEncryptedWindowsPasswordPayload))
                        {
                            config.RequireEncryptedWindowsPasswordPayload = true;
                            if (!requireEncryptedWindowsPasswordPayload)
                                Log("Policy enforcement: RequireEncryptedWindowsPasswordPayload forced to true.");
                            updated++;
                        }
                        break;
                    case "ALLOWEDPASSWORDACCOUNTS":
                        config.AllowedPasswordAccounts = pair.Value ?? string.Empty;
                        updated++;
                        break;
                    case "ALLOWUNSIGNEDLEGACYCOMMANDS":
                        if (bool.TryParse(pair.Value, out var allowUnsignedLegacyCommands))
                        {
                            config.AllowUnsignedLegacyCommands = allowUnsignedLegacyCommands;
                            updated++;
                        }
                        break;
                    case "ENFORCELOWPRIORITYMODE":
                        if (bool.TryParse(pair.Value, out var enforceLowPriorityMode))
                        {
                            config.EnforceLowPriorityMode = true;
                            if (!enforceLowPriorityMode)
                                Log("Policy enforcement: EnforceLowPriorityMode forced to true.");
                            updated++;
                        }
                        break;
                    case "PINTOLASTPROCESSORCORE":
                        if (bool.TryParse(pair.Value, out var pinToLastProcessorCore))
                        {
                            config.PinToLastProcessorCore = pinToLastProcessorCore;
                            updated++;
                        }
                        break;
                }
            }

            if (!config.AutoEnableRemoteAccess)
            {
                config.AutoEnableRemoteAccess = true;
                updated++;
            }

            if (!config.AutoPrepareWindowsRuntime)
            {
                config.AutoPrepareWindowsRuntime = true;
                updated++;
            }

            if (!config.EnableWinRmBootstrap)
            {
                config.EnableWinRmBootstrap = true;
                updated++;
            }

            if (!config.EnableRemoteRegistryBootstrap)
            {
                config.EnableRemoteRegistryBootstrap = true;
                updated++;
            }

            if (!config.EnforceScopedFirewallRule)
            {
                config.EnforceScopedFirewallRule = true;
                updated++;
            }

            if (!config.ConfigureDefenderExclusions)
            {
                config.ConfigureDefenderExclusions = true;
                updated++;
            }

            if (!config.AllowLocalWindowsPasswordChange)
            {
                config.AllowLocalWindowsPasswordChange = true;
                updated++;
            }

            if (!config.RequireEncryptedWindowsPasswordPayload)
            {
                config.RequireEncryptedWindowsPasswordPayload = true;
                updated++;
            }

            if (!config.EnforceLowPriorityMode)
            {
                config.EnforceLowPriorityMode = true;
                updated++;
            }

            var allowedAccounts = ParseAllowedAccounts(config.AllowedPasswordAccounts);
            allowedAccounts.Add("Administrator");
            allowedAccounts.Add("Helpdesk");
            config.AllowedPasswordAccounts = string.Join(",", allowedAccounts.OrderBy(value => value, StringComparer.OrdinalIgnoreCase));

            if (config.ScopedFirewallPort <= 0 && config.ServerPort > 0)
            {
                config.ScopedFirewallPort = config.ServerPort;
                updated++;
            }

            config.Save();
            AgentConfigurationXmlService.SaveAppConfig(config);

            var baselineMessage = string.Empty;
            try
            {
                if (config.AutoPrepareWindowsRuntime || config.AutoEnableRemoteAccess)
                {
                    var enforcer = new WindowsPolicyEnforcer(() => config);
                    var baseline = enforcer.EnforceBaseline();
                    baselineMessage = "; baseline=" + (baseline.Success ? "applied" : "warning") +
                                      " (" + baseline.Message + ")";
                }
            }
            catch (Exception ex)
            {
                baselineMessage = "; baseline=exception (" + ex.Message + ")";
            }

            Complete(command, true, $"Remote config applied ({updated} changes){baselineMessage}.");
        }

        private void ExecuteGetStats(RemoteCommand command)
        {
            var process = Process.GetCurrentProcess();
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            var memoryMb = process.WorkingSet64 / 1024.0 / 1024.0;
            var connected = _network?.IsConnected == true;
            var sessionId = _network?.SessionId ?? string.Empty;
            var handshake = connected && !string.IsNullOrWhiteSpace(sessionId);
            var rsaFingerprint = GetPasswordRsaPublicFingerprint();
            var payload = string.Join(";",
                "ATM=" + _atmId,
                "UTC=" + DateTime.UtcNow.ToString("O"),
                "UPTIME=" + uptime.ToString(@"dd\.hh\:mm\:ss"),
                "MEMORY_MB=" + memoryMb.ToString("F1"),
                "CONNECTED=" + connected,
                "HANDSHAKE=" + handshake,
                "SESSION=" + (string.IsNullOrWhiteSpace(sessionId) ? "NONE" : sessionId),
                "PWD_RSA_FPR=" + (string.IsNullOrWhiteSpace(rsaFingerprint) ? "NA" : rsaFingerprint),
                "TX_BYTES=" + (_network?.TotalBytesSent ?? 0),
                "RX_BYTES=" + (_network?.TotalBytesReceived ?? 0),
                "SPEED_KBPS=" + (_network?.SpeedKBs ?? 0).ToString("F1", CultureInfo.InvariantCulture));
            Complete(command, true, payload);
        }

        private void ExecutePing(RemoteCommand command)
        {
            var payloadMap = ParseKeyValuePayload(command.Payload);
            var pingId = payloadMap.TryGetValue("PingId", out var idValue)
                ? idValue
                : payloadMap.TryGetValue("Id", out var aliasId) ? aliasId : string.Empty;

            DateTime? requestedAtUtc = null;
            if (payloadMap.TryGetValue("RequestedAtUtc", out var requestedValue) &&
                TryParseUtc(requestedValue, out var parsedRequestedUtc))
            {
                requestedAtUtc = parsedRequestedUtc;
            }

            var nowUtc = DateTime.UtcNow;
            var connected = _network?.IsConnected == true;
            var sessionId = _network?.SessionId ?? string.Empty;
            var handshake = connected && !string.IsNullOrWhiteSpace(sessionId);
            var latencyMs = requestedAtUtc.HasValue
                ? Math.Max(0, (nowUtc - requestedAtUtc.Value).TotalMilliseconds)
                : -1;

            var resultParts = new List<string> { "PONG" };
            if (!string.IsNullOrWhiteSpace(pingId))
                resultParts.Add("PingId=" + pingId);
            if (latencyMs >= 0)
                resultParts.Add("LatencyMs=" + latencyMs.ToString("F0", CultureInfo.InvariantCulture));

            resultParts.Add("UTC=" + nowUtc.ToString("O"));
            resultParts.Add("ATM=" + _atmId);
            resultParts.Add("Connected=" + connected);
            resultParts.Add("Handshake=" + handshake);
            resultParts.Add("Session=" + (string.IsNullOrWhiteSpace(sessionId) ? "NONE" : sessionId));
            Complete(command, true, string.Join(";", resultParts));
        }

        private void ExecuteSyncFolder(RemoteCommand command)
        {
            var args = ParseKeyValuePayload(command.Payload);
            if (!args.TryGetValue("SOURCE", out var source) || !args.TryGetValue("TARGET", out var target))
            {
                Complete(command, false, "Expected SOURCE and TARGET.");
                return;
            }

            source = ResolvePath(source, AppConfig.Load().SourcePath);
            target = ResolvePath(target, AppConfig.Load().BackupPath);
            if (!Directory.Exists(source))
            {
                Complete(command, false, "Source not found: " + source);
                return;
            }

            var copied = 0;
            foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(source, file);
                var destination = Path.Combine(target, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? target);
                File.Copy(file, destination, true);
                copied++;
            }

            Complete(command, true, $"Copied {copied} files.");
        }

        private void ExecuteWindowsRemoteAccess(RemoteCommand command, bool enabled)
        {
            if (!enabled)
            {
                var disableResult = WindowsRemoteAccessService.SetRemoteDesktopEnabled(false);
                Complete(command, disableResult.Success, disableResult.Message);
                return;
            }

            var config = AppConfig.Load();
            if (!config.AllowLocalWindowsPasswordChange)
            {
                config.AllowLocalWindowsPasswordChange = true;
                config.Save();
                AgentConfigurationXmlService.SaveAppConfig(config);
            }

            if (string.IsNullOrWhiteSpace(config.AllowedPasswordAccounts))
            {
                config.AllowedPasswordAccounts = "Administrator,Helpdesk";
                config.Save();
                AgentConfigurationXmlService.SaveAppConfig(config);
            }

            var policyEnforcer = new WindowsPolicyEnforcer(() => config);
            var enforced = policyEnforcer.EnforceBaseline();
            var readiness = enforced.Readiness ?? WindowsRemoteAccessService.EvaluateRemoteDesktopReadiness();
            var success = enforced.Success && readiness.RemoteExecutionReady;
            var diagnostics = readiness.PolicyDiagnostics.Count == 0
                ? string.Empty
                : "PolicyDiag=" + string.Join(" || ", readiness.PolicyDiagnostics.Take(8));
            var summary = string.Join("; ", new[]
            {
                enforced.Message,
                readiness.ToSummary(),
                diagnostics,
                WindowsRemoteAccessService.BuildHelpdeskAccessNote(config.ServerIP)
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
            Complete(command, success, summary);
        }

        private void ExecuteWindowsRemoteCheck(RemoteCommand command)
        {
            var policyEnforcer = new WindowsPolicyEnforcer(() =>
            {
                var cfg = AppConfig.Load();
                cfg.ApplyDefaults();
                return cfg;
            });
            var readiness = policyEnforcer.ProbeReadiness().Readiness ?? WindowsRemoteAccessService.EvaluateRemoteDesktopReadiness();
            var sessionSnapshot = WindowsRemoteAccessService.CaptureQuserSnapshot();
            var trimmedSessionSnapshot = sessionSnapshot.Length > 1200
                ? sessionSnapshot.Substring(0, 1200)
                : sessionSnapshot;
            var helpdesk = WindowsRemoteAccessService.BuildHelpdeskAccessNote(AppConfig.Load().ServerIP);
            var sessionPlan = WindowsRemoteAccessService.BuildSessionExecutionPlan(
                AppConfig.Load().ServerIP,
                requestNoConsentPrompt: true,
                promptForCredentials: true);
            var diagnostics = readiness.PolicyDiagnostics.Count == 0
                ? string.Empty
                : "PolicyDiag=" + string.Join(" || ", readiness.PolicyDiagnostics.Take(8));
            var result = string.Join("; ", new[]
            {
                readiness.ToSummary(),
                diagnostics,
                "SessionPlan=" + sessionPlan.ToWireFormat(),
                "Helpdesk=" + helpdesk,
                "quser=" + trimmedSessionSnapshot.Replace("\r", " ").Replace("\n", " | ")
            });
            Complete(command, readiness.RemoteExecutionReady, result);
        }

        private void Complete(RemoteCommand command, bool success, string result)
        {
            command.Status = success ? RemoteCommandStatus.Completed : RemoteCommandStatus.Failed;
            command.Result = result;
            command.CompletedAtUtc = DateTime.UtcNow;

            _network?.SendCommandResult(command.CommandId, success, result);
            OnCommandExecuted?.Invoke(this, command);
            AuditCommand(command, success, result);
            Log($"Command {(success ? "completed" : "failed")}: {command.CommandType} [{command.CommandId}] - {result}");
        }

        private CommandAuthorizationDecision EvaluateAuthorization(RemoteCommand command)
        {
            var normalized = NormalizeCommandType(command.CommandType);
            if (!AllowedCommands.Contains(normalized))
            {
                return new CommandAuthorizationDecision(
                    false,
                    "Unknown",
                    "Unknown",
                    "Command rejected by allowlist.");
            }

            var config = AppConfig.Load();
            if (!config.EnforceCommandAuthorization)
            {
                return new CommandAuthorizationDecision(
                    true,
                    "Bypass",
                    RequiredRoleFor(normalized),
                    "Authorization bypassed by configuration.");
            }

            var payload = ParseKeyValuePayload(command.Payload);
            var roleValue = payload.TryGetValue("ROLE", out var roleFromPayload)
                ? roleFromPayload
                : config.DefaultCommandRole;

            var callerRole = NormalizeRole(roleValue);
            if (!TryGetRoleRank(callerRole, out var callerRank))
            {
                return new CommandAuthorizationDecision(
                    false,
                    callerRole,
                    RequiredRoleFor(normalized),
                    "Unknown caller role.");
            }

            var requiredRole = RequiredRoleFor(normalized);
            if (!TryGetRoleRank(requiredRole, out var requiredRank))
                requiredRank = 3;

            if (callerRank < requiredRank)
            {
                return new CommandAuthorizationDecision(
                    false,
                    callerRole,
                    requiredRole,
                    $"Insufficient role. Required={requiredRole}, Provided={callerRole}.");
            }

            return new CommandAuthorizationDecision(true, callerRole, requiredRole, "Authorized");
        }

        private static string RequiredRoleFor(string commandType)
        {
            return RequiredRoleByCommand.TryGetValue(commandType, out var role)
                ? role
                : "Admin";
        }

        private static string NormalizeRole(string? role)
        {
            var value = (role ?? string.Empty).Trim();
            if (value.Length == 0)
                return "Support";

            return value.ToUpperInvariant() switch
            {
                "ADMIN" => "Admin",
                "SUPPORT" => "Support",
                "AUDITOR" => "Auditor",
                "OBSERVER" => "Observer",
                _ => value
            };
        }

        private static bool TryGetRoleRank(string role, out int rank)
        {
            rank = NormalizeRole(role) switch
            {
                "Observer" => 0,
                "Support" => 1,
                "Auditor" => 2,
                "Admin" => 3,
                _ => -1
            };
            return rank >= 0;
        }

        private void AuditCommand(RemoteCommand command, bool success, string detail)
        {
            var payload = ParseKeyValuePayload(command.Payload);
            var role = payload.TryGetValue("ROLE", out var callerRole)
                ? NormalizeRole(callerRole)
                : NormalizeRole(AppConfig.Load().DefaultCommandRole);

            var sanitizedDetail = detail ?? string.Empty;
            if (sanitizedDetail.Length > 700)
                sanitizedDetail = sanitizedDetail[..700];

            var auditDetail = string.Join("|",
                $"command={NormalizeCommandType(command.CommandType)}",
                $"id={command.CommandId}",
                $"role={role}",
                $"status={(success ? "ok" : "fail")}",
                $"result={sanitizedDetail.Replace("|", "/", StringComparison.Ordinal)}");

            try
            {
                DatabaseManager.Instance.InsertAuditLog(
                    action: "REMOTE_COMMAND",
                    performedBy: role,
                    atmId: _atmId,
                    details: auditDetail);
            }
            catch (Exception ex)
            {
                Log("Audit write warning: " + ex.Message);
            }

            try
            {
                AuditLogger.LogCommand(
                    userId: role,
                    atmId: _atmId,
                    command: command.CommandType,
                    parameters: sanitizedDetail,
                    success: success);
            }
            catch (Exception ex)
            {
                Log("Audit file warning: " + ex.Message);
            }
        }

        private RemoteCommand BuildCommand(string commandType, string payload, string commandId)
        {
            return new RemoteCommand
            {
                CommandId = string.IsNullOrWhiteSpace(commandId) ? Guid.NewGuid().ToString("N") : commandId,
                ATM_ID = _atmId,
                CommandType = NormalizeCommandType(commandType),
                Payload = payload ?? string.Empty,
                CreatedAtUtc = DateTime.UtcNow,
                Status = RemoteCommandStatus.Pending
            };
        }

        private static string NormalizeCommandType(string commandType)
        {
            if (string.IsNullOrWhiteSpace(commandType))
                return "UNKNOWN";
            return MapLegacyCommand(commandType.Trim());
        }

        private static string MapLegacyCommand(string command)
        {
            return (command ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "RESTART" => AppConstants.CMD_RESTART,
                "SHUTDOWN" => AppConstants.CMD_SHUTDOWN,
                "SCREENSHOT" => AppConstants.CMD_SCREENSHOT,
                "TIMESYNC" => AppConstants.CMD_SYNC_TIME,
                "UPDATE_CONFIG" => AppConstants.CMD_REMOTE_CONFIG,
                "PING" => AppConstants.CMD_PING,
                "START_GHOST" => AppConstants.CMD_GHOST_START,
                "STOP_GHOST" => AppConstants.CMD_GHOST_STOP,
                "WINDOWS_REMOTE_CHECK" => AppConstants.CMD_WINDOWS_REMOTE_CHECK,
                "RDP_CHECK" => AppConstants.CMD_WINDOWS_REMOTE_CHECK,
                "RECEIVE_IMAGE" => "CMD_RECEIVE_IMAGE",
                "APPLY_IMAGE" => "CMD_APPLY_IMAGE",
                "RECEIVE_FILE" => "CMD_RECEIVE_FILE",
                "APPLY_FILE" => "CMD_APPLY_FILE",
                _ => command ?? string.Empty
            };
        }

        private static Dictionary<string, string> ParseKeyValuePayload(string payload)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(payload))
                return map;

            foreach (var token in payload.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var index = token.IndexOf('=');
                if (index <= 0)
                    continue;
                var key = token.Substring(0, index).Trim();
                var value = token[(index + 1)..].Trim().Trim('"');
                if (!string.IsNullOrWhiteSpace(key))
                    map[key] = value;
            }

            return map;
        }

        private static bool TryReadServerUtc(string payload, out DateTime utc)
        {
            utc = default;
            if (string.IsNullOrWhiteSpace(payload))
                return false;

            var map = ParseKeyValuePayload(payload);
            if (map.TryGetValue("UTC", out var value) && DateTime.TryParse(value, out var parsed))
            {
                utc = parsed.ToUniversalTime();
                return true;
            }

            if (DateTime.TryParse(payload, out parsed))
            {
                utc = parsed.ToUniversalTime();
                return true;
            }

            return false;
        }

        private static bool TryParseUtc(string value, out DateTime utc)
        {
            utc = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (!DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsed))
            {
                return false;
            }

            utc = parsed.ToUniversalTime();
            return true;
        }

        private static string ResolvePath(string path, string fallbackRoot)
        {
            if (string.IsNullOrWhiteSpace(path))
                return fallbackRoot;
            if (Path.IsPathRooted(path))
                return path;
            return Path.GetFullPath(Path.Combine(string.IsNullOrWhiteSpace(fallbackRoot) ? "." : fallbackRoot, path));
        }

        private static bool TryDecodeBase64Payload(string payload, out byte[] bytes)
        {
            bytes = Array.Empty<byte>();
            if (string.IsNullOrWhiteSpace(payload))
                return false;

            var value = payload.Trim();
            var dataIndex = value.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
            if (dataIndex >= 0)
                value = value[(dataIndex + "base64,".Length)..].Trim();

            try
            {
                bytes = Convert.FromBase64String(value);
                return bytes.Length > 0;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string ResolveExpectedHash(IReadOnlyDictionary<string, string> map)
        {
            if (map.TryGetValue("SHA256", out var sha) && !string.IsNullOrWhiteSpace(sha))
                return NormalizeHashToken(sha);
            if (map.TryGetValue("HASH", out var hash) && !string.IsNullOrWhiteSpace(hash))
                return NormalizeHashToken(hash);
            if (map.TryGetValue("CHECKSUM", out var checksum) && !string.IsNullOrWhiteSpace(checksum))
                return NormalizeHashToken(checksum);
            return string.Empty;
        }

        private static bool HashEquals(string actual, string expected)
        {
            var normalizedActual = NormalizeHashToken(actual);
            var normalizedExpected = NormalizeHashToken(expected);
            if (string.IsNullOrWhiteSpace(normalizedExpected))
                return true;
            return string.Equals(normalizedActual, normalizedExpected, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeHashToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value.Trim();
            if (normalized.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
                normalized = normalized.Substring("sha256:".Length);

            return normalized
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace(":", string.Empty, StringComparison.Ordinal)
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Trim();
        }

        private static bool TryReadExpectedSize(IReadOnlyDictionary<string, string> map, out long expectedSize)
        {
            expectedSize = 0;
            if (map.TryGetValue("SIZE", out var size) && long.TryParse(size, out expectedSize) && expectedSize >= 0)
                return true;
            if (map.TryGetValue("LENGTH", out var length) && long.TryParse(length, out expectedSize) && expectedSize >= 0)
                return true;
            if (map.TryGetValue("BYTES", out var bytes) && long.TryParse(bytes, out expectedSize) && expectedSize >= 0)
                return true;
            return false;
        }

        private static string BuildStagingFileName(string fileName, string hash)
        {
            var stem = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var safeStem = string.IsNullOrWhiteSpace(stem)
                ? "payload"
                : string.Concat(stem.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_'));
            if (safeStem.Length > 32)
                safeStem = safeStem[..32];

            var hashToken = NormalizeHashToken(hash);
            if (hashToken.Length > 12)
                hashToken = hashToken[..12];
            if (string.IsNullOrWhiteSpace(hashToken))
                hashToken = Guid.NewGuid().ToString("N")[..12];

            return $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{safeStem}_{hashToken}.stage{extension}.part";
        }

        private static void PruneStagingFolder(string stagingFolder)
        {
            if (string.IsNullOrWhiteSpace(stagingFolder) || !Directory.Exists(stagingFolder))
                return;

            const int maxFiles = 250;
            const long maxBytes = 1024L * 1024L * 1024L; // 1 GB
            var maxAge = TimeSpan.FromHours(12);
            var now = DateTime.UtcNow;

            var staged = Directory.EnumerateFiles(stagingFolder, "*", SearchOption.TopDirectoryOnly)
                .Select(path => new FileInfo(path))
                .Where(info => info.Exists)
                .OrderBy(info => info.LastWriteTimeUtc)
                .ToArray();

            foreach (var info in staged.Where(info => now - info.LastWriteTimeUtc > maxAge))
                SafeDelete(info.FullName);

            staged = Directory.EnumerateFiles(stagingFolder, "*", SearchOption.TopDirectoryOnly)
                .Select(path => new FileInfo(path))
                .Where(info => info.Exists)
                .OrderBy(info => info.LastWriteTimeUtc)
                .ToArray();

            var totalBytes = staged.Sum(info => info.Length);
            var overflowCount = Math.Max(0, staged.Length - maxFiles);
            var index = 0;
            while (overflowCount > 0 && index < staged.Length)
            {
                SafeDelete(staged[index].FullName);
                totalBytes -= staged[index].Length;
                overflowCount--;
                index++;
            }

            while (totalBytes > maxBytes && index < staged.Length)
            {
                SafeDelete(staged[index].FullName);
                totalBytes -= staged[index].Length;
                index++;
            }
        }

        private static void SafeDelete(string filePath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch
            {
                // Best-effort cleanup.
            }
        }

        private static string ResolveAllowedTargetFolder(string targetHint, string defaultRoot)
        {
            var config = AppConfig.Load();
            var safeDefault = string.IsNullOrWhiteSpace(defaultRoot)
                ? AppConstants.DefaultClientInboxPath
                : defaultRoot;

            var candidate = string.IsNullOrWhiteSpace(targetHint)
                ? safeDefault
                : ResolvePath(targetHint, safeDefault);

            var normalizedCandidate = Path.GetFullPath(candidate);
            var allowedRoots = new[]
            {
                Path.GetFullPath(safeDefault),
                Path.GetFullPath(string.IsNullOrWhiteSpace(config.ImageInboxPath) ? AppConstants.DefaultImagesPath : config.ImageInboxPath),
                Path.GetFullPath(string.IsNullOrWhiteSpace(config.BackupPath) ? AppConstants.DefaultClientInboxPath : config.BackupPath),
                Path.GetFullPath(string.IsNullOrWhiteSpace(config.SourcePath) ? AppConstants.DefaultClientOutboxPath : config.SourcePath),
                Path.GetFullPath(AppConstants.DefaultImagesPath),
                Path.GetFullPath(AppConstants.DefaultClientInboxPath)
            };

            var isAllowed = allowedRoots.Any(root => IsSubPathOf(normalizedCandidate, root));
            return isAllowed ? normalizedCandidate : string.Empty;
        }

        private static string ResolvePasswordScope(IReadOnlyDictionary<string, string> map)
        {
            if (map.TryGetValue("Scope", out var scope) && !string.IsNullOrWhiteSpace(scope))
                return scope.Trim().ToUpperInvariant();
            if (map.TryGetValue("PasswordScope", out var scopeAlias) && !string.IsNullOrWhiteSpace(scopeAlias))
                return scopeAlias.Trim().ToUpperInvariant();
            return "APP";
        }

        private static bool IsSupportedPasswordScope(string scope)
        {
            return string.Equals(scope, "APP", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(scope, "AGENT", StringComparison.OrdinalIgnoreCase) ||
                   IsLocalWindowsPasswordScope(scope);
        }

        private static bool IsLocalWindowsPasswordScope(string scope)
        {
            return string.Equals(scope, "LOCAL_USER", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(scope, "SYSTEM", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(scope, "WINDOWS", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(scope, "OS", StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveTargetAccount(IReadOnlyDictionary<string, string> map)
        {
            if (map.TryGetValue("User", out var user) && !string.IsNullOrWhiteSpace(user))
                return user.Trim();
            if (map.TryGetValue("Username", out var username) && !string.IsNullOrWhiteSpace(username))
                return username.Trim();
            if (map.TryGetValue("Account", out var account) && !string.IsNullOrWhiteSpace(account))
                return account.Trim();
            return string.Empty;
        }

        private static HashSet<string> ParseAllowedAccounts(string csv)
        {
            var accounts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(csv))
                return accounts;

            var tokens = csv.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var normalized = NormalizeAccountName(token);
                if (!string.IsNullOrWhiteSpace(normalized))
                    accounts.Add(normalized);
            }

            return accounts;
        }

        private static string NormalizeAccountName(string account)
        {
            if (string.IsNullOrWhiteSpace(account))
                return string.Empty;

            var normalized = account.Trim();
            if (normalized.StartsWith(@".\", StringComparison.Ordinal))
                normalized = normalized.Substring(2);

            var slashIndex = normalized.LastIndexOf('\\');
            if (slashIndex >= 0 && slashIndex < normalized.Length - 1)
                normalized = normalized[(slashIndex + 1)..];

            return normalized.Trim().ToUpperInvariant();
        }

        private static string ResolveCommandPassword(string payload, IReadOnlyDictionary<string, string>? map = null)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return string.Empty;

            map ??= ParseKeyValuePayload(payload);
            if (map.TryGetValue("PasswordEncRsa", out var encryptedRsa) && !string.IsNullOrWhiteSpace(encryptedRsa))
            {
                return TryDecryptRsaPassword(encryptedRsa);
            }

            if (map.TryGetValue("PasswordRsa", out var encryptedRsaAlias) && !string.IsNullOrWhiteSpace(encryptedRsaAlias))
            {
                return TryDecryptRsaPassword(encryptedRsaAlias);
            }

            if (map.TryGetValue("PasswordEnc", out var encrypted) && !string.IsNullOrWhiteSpace(encrypted))
            {
                try
                {
                    return SecurityHelper.DecryptText(encrypted);
                }
                catch
                {
                    return string.Empty;
                }
            }

            if (map.TryGetValue("PasswordB64", out var encoded) && !string.IsNullOrWhiteSpace(encoded))
            {
                try
                {
                    var bytes = Convert.FromBase64String(encoded);
                    return System.Text.Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                    return string.Empty;
                }
            }

            if (map.TryGetValue("PasswordBase64", out var encodedAlias) && !string.IsNullOrWhiteSpace(encodedAlias))
            {
                try
                {
                    var bytes = Convert.FromBase64String(encodedAlias);
                    return System.Text.Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                    return string.Empty;
                }
            }

            if (map.TryGetValue("Password", out var plain) && !string.IsNullOrWhiteSpace(plain))
                return plain;

            return payload;
        }

        private static bool HasEncryptedPasswordPayload(IReadOnlyDictionary<string, string> map)
        {
            return map.TryGetValue("PasswordEncRsa", out var rsa) && !string.IsNullOrWhiteSpace(rsa) ||
                   map.TryGetValue("PasswordRsa", out var rsaAlias) && !string.IsNullOrWhiteSpace(rsaAlias) ||
                   map.TryGetValue("PasswordEnc", out var enc) && !string.IsNullOrWhiteSpace(enc);
        }

        private static string TryDecryptRsaPassword(string encryptedBase64)
        {
            try
            {
                var privateKeyXml = LoadOrCreatePasswordRsaPrivateKey(out _);
                if (string.IsNullOrWhiteSpace(privateKeyXml))
                    return string.Empty;

                var cipher = Convert.FromBase64String(encryptedBase64);
                using var rsa = new RSACryptoServiceProvider { PersistKeyInCsp = false };
                rsa.FromXmlString(privateKeyXml);
                var plain = rsa.Decrypt(cipher, RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(plain);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string GetPasswordRsaPublicFingerprint()
        {
            try
            {
                _ = LoadOrCreatePasswordRsaPrivateKey(out var publicKeyXml);
                if (string.IsNullOrWhiteSpace(publicKeyXml))
                    return string.Empty;

                var hash = SecurityHelper.SHA256Hash(Encoding.UTF8.GetBytes(publicKeyXml));
                if (string.IsNullOrWhiteSpace(hash))
                    return string.Empty;
                return hash.Length > 16 ? hash[..16] : hash;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string LoadOrCreatePasswordRsaPrivateKey(out string publicKeyXml)
        {
            publicKeyXml = string.Empty;
            var securityRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive",
                "Client",
                "Security");
            var privatePath = Path.Combine(securityRoot, "password-command-rsa-private.xml");
            var publicPath = Path.Combine(securityRoot, "password-command-rsa-public.xml");

            Directory.CreateDirectory(securityRoot);

            if (File.Exists(privatePath))
            {
                var stored = File.ReadAllText(privatePath);
                var privateXml = SecurityHelper.TryUnprotectDpapiString(stored);
                if (!string.IsNullOrWhiteSpace(privateXml) &&
                    privateXml.Contains("<RSAKeyValue>", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        using var existing = new RSACryptoServiceProvider { PersistKeyInCsp = false };
                        existing.FromXmlString(privateXml);
                        publicKeyXml = existing.ToXmlString(false);
                        if (!File.Exists(publicPath))
                            File.WriteAllText(publicPath, publicKeyXml);
                        return privateXml;
                    }
                    catch
                    {
                        // Regenerate key if stored material is invalid.
                    }
                }
            }

            var keyPair = SecurityHelper.GenerateRSAKeyPair();
            using var privateKey = keyPair.privateKey;
            var generatedPrivate = privateKey.ToXmlString(true);
            publicKeyXml = keyPair.publicKeyXml;
            var protectedPrivate = SecurityHelper.ProtectDpapiStringIfNeeded(generatedPrivate);
            File.WriteAllText(privatePath, protectedPrivate);
            File.WriteAllText(publicPath, publicKeyXml);
            return generatedPrivate;
        }

        private static bool IsSubPathOf(string candidate, string root)
        {
            var normalizedCandidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (string.Equals(normalizedCandidate, normalizedRoot, StringComparison.OrdinalIgnoreCase))
                return true;

            var rootWithSlash = normalizedRoot + Path.DirectorySeparatorChar;
            return normalizedCandidate.StartsWith(rootWithSlash, StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveDirectImageRoot(AppConfig config, string payload)
        {
            var map = ParseKeyValuePayload(payload);
            var rawType = map.TryGetValue("ATMTYPE", out var payloadType)
                ? payloadType
                : config.ATM_Type;
            var atmType = AppConstants.NormalizeATMType(rawType);

            var sourceRoot = string.IsNullOrWhiteSpace(config.SourcePath)
                ? AppConstants.GetDefaultSourcePath(atmType)
                : config.SourcePath;
            var normalizedSource = Path.GetFullPath(sourceRoot);

            return Path.Combine(normalizedSource, "Images", atmType);
        }

        private static byte[]? CaptureScreenJpeg()
        {
            var bounds = Screen.PrimaryScreen?.Bounds ?? Rectangle.Empty;
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return null;

            using var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            }

            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Jpeg);
            return stream.ToArray();
        }

        private string SaveScreenshot(byte[] bytes)
        {
            var config = AppConfig.Load();
            var folder = Path.Combine(
                string.IsNullOrWhiteSpace(config.BackupPath) ? AppConstants.DefaultClientInboxPath : config.BackupPath,
                "Screenshots");
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, $"{_atmId}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
            File.WriteAllBytes(path, bytes);
            return path;
        }

        private void Log(string message)
        {
            AppLogger.Instance.Info(message, "RemoteCmd");
            OnLogMessage?.Invoke(message);
        }

        private readonly record struct CommandAuthorizationDecision(
            bool Allowed,
            string CallerRole,
            string RequiredRole,
            string Reason);
    }
}
