# PROTOCOL ledger (generated -- do not hand-edit)

Source: `tools/inventory/ejlive_inventory.py`. Regenerate with `python3 tools/inventory/ejlive_inventory.py`.

| wire literal | map |
|---|---|
| `AUDIT_ONLY` | reference |
| `CMD_20260520_00001` | active |
| `CMD_20260520_00002` | active |
| `CMD_APPLY_FILE` | reference |
| `CMD_APPLY_IMAGE` | active |
| `CMD_APPLY_IMAGE` | reference |
| `CMD_CHANGE_PASSWORD` | active |
| `CMD_CHANGE_PASSWORD` | reference |
| `CMD_FORCE_SYNC` | active |
| `CMD_FORCE_SYNC` | reference |
| `CMD_GET_FILE` | active |
| `CMD_GET_STATS` | active |
| `CMD_GET_STATS` | reference |
| `CMD_GHOST_START` | reference |
| `CMD_GHOST_STOP` | reference |
| `CMD_IMAGE_SYNC` | reference |
| `CMD_MESSAGE` | reference |
| `CMD_PING` | active |
| `CMD_PING` | reference |
| `CMD_RECEIVE_FILE` | active |
| `CMD_RECEIVE_FILE` | reference |
| `CMD_RECEIVE_IMAGE` | active |
| `CMD_RECEIVE_IMAGE` | reference |
| `CMD_REMOTE_CONFIG` | active |
| `CMD_REMOTE_CONFIG` | reference |
| `CMD_REMOTE_SESSION_START` | active |
| `CMD_REMOTE_SESSION_STOP` | active |
| `CMD_RESTART` | active |
| `CMD_RESTART` | reference |
| `CMD_RESULT` | active |
| `CMD_RESULT` | reference |
| `CMD_SCREENSHOT` | active |
| `CMD_SCREENSHOT` | reference |
| `CMD_SEND_FILE` | active |
| `CMD_SEND_IMAGE` | active |
| `CMD_SEND_IMAGE` | reference |
| `CMD_SHUTDOWN` | active |
| `CMD_SHUTDOWN` | reference |
| `CMD_STATUS` | active |
| `CMD_SYNC_JOURNAL` | active |
| `CMD_SYNC_TIME` | active |
| `CMD_SYNC_TIME` | reference |
| `CMD_SYSINFO` | reference |
| `CMD_TIMESYNC` | reference |
| `CMD_WINDOWS_REMOTE_CHECK` | active |
| `CMD_WINDOWS_REMOTE_START` | active |
| `CMD_WINDOWS_REMOTE_STOP` | active |
| `EJLIVE_ACK` | active |
| `EJLIVE_ACK` | reference |
| `EJLIVE_ADAPTIVE_CHUNKING` | active |
| `EJLIVE_ADAPTIVE_CHUNKING` | reference |
| `EJLIVE_ALLOW_UNSIGNED_COMMANDS` | active |
| `EJLIVE_ATM_ID` | active |
| `EJLIVE_ATM_ID` | reference |
| `EJLIVE_BACKUP_INTEGRITY_KEY` | active |
| `EJLIVE_COMMAND_HMAC_KEY` | active |
| `EJLIVE_COMMAND_MAX_AGE_MIN` | active |
| `EJLIVE_CREDENTIALS_PATH` | active |
| `EJLIVE_DATABASE_PATH` | active |
| `EJLIVE_DATABASE_PATH` | reference |
| `EJLIVE_HANDSHAKE` | active |
| `EJLIVE_HANDSHAKE` | reference |
| `EJLIVE_HEARTBEAT` | reference |
| `EJLIVE_KDF_SALT` | active |
| `EJLIVE_MIGRATION_AES_IV` | active |
| `EJLIVE_MIGRATION_AES_KEY` | active |
| `EJLIVE_REJECT` | active |
| `EJLIVE_REJECT` | reference |
| `EJLIVE_SERVER_IP` | active |
| `EJLIVE_SERVER_IP` | reference |
| `EJLIVE_SERVER_PORT` | active |
| `EJLIVE_SERVER_PORT` | reference |
| `EJLIVE_SOCKET_TLS` | active |
| `EJLIVE_SOCKET_TLS` | reference |
| `EJLIVE_SOCKET_TLS_ALLOW_UNTRUSTED` | active |
| `EJLIVE_SOCKET_TLS_ALLOW_UNTRUSTED` | reference |
| `EJLIVE_SOCKET_TLS_REQUIRED` | active |
| `EJLIVE_SOCKET_TLS_REQUIRED` | reference |
| `EJLIVE_SUPABASE_SERVICE_KEY` | reference |
| `EJLIVE_SUPABASE_URL` | reference |
| `EJLIVE_TRANSFER_KEY` | active |
| `EJLIVE_WEAK_NETWORK_LATENCY_MS` | active |
| `EJLIVE_WEAK_NETWORK_LATENCY_MS` | reference |
| `PING_ALL` | reference |
| `PING_LOCAL` | active |
| `PING_LOCAL` | reference |
| `SESSION0_ISOLATION` | reference |
| `STATUS_CHECK` | active |
| `STATUS_REQ` | active |
| `STATUS_REQ` | reference |
| `STATUS_RES` | active |
| `STATUS_RES` | reference |

92 message literals. Envelope grammar, chunking, signing and resume semantics are normative in `docs/EJLIVE-ENGINEERING-PROMPT.md` SS5. Ports observed: 3389, 5656, 8080.

## frame and header probes

- `:\n` <- `src/EJLive.UnifiedLauncher/Program.cs`
- `:\n` <- `src/_reference/orphan-projects/EJLive.Launcher/Program.cs`
- `:\n` <- `src/_reference/orphan-projects/EJLive.Monitoring/MainDashboardForm.cs`
- `:\n` <- `src/_reference/uncompiled/EJLive.Monitoring.WinForms/Dashboard/MainDashboardForm.cs`
- `:\n` <- `src/_reference/uncompiled/EJLive.Server.WinForms/Dashboards/Primary/MonitoringDashboardPrimaryForm.cs`
- `:\n` <- `src/_reference/uncompiled/EJLive.Server.WinForms/JournalViewerForm.cs`
- `:\n` <- `src/_reference/uncompiled/EJLive.Server/JournalViewerForm.cs`
- `:\n` <- `src/_reference/uncompiled/EJLive.Server/ServerMainForm.cs`
- `MsgType` <- `src/EJLive.Client.Service/Compatibility/ServiceStubs.cs`
- `MsgType` <- `src/EJLive.Core/Engine/CommunicationProtocol.cs`
- `MsgType` <- `src/_reference/corrupted/EJLive.Core/Services/GhostRemoteEngine.cs`
- `MsgType` <- `src/_reference/corrupted/EJLive.Core/Services/ImageSyncEngine.cs`
- `MsgType (from 2 sources)` <- `src/_reference/corrupted/EJLive.Core/Services/GhostRemoteEngine.cs`
- `MsgType (from 2 sources)` <- `src/_reference/corrupted/EJLive.Core/Services/ImageSyncEngine.cs`
- `MsgType Type { get; set; }` <- `src/EJLive.Client.Service/Compatibility/ServiceStubs.cs`
- `MsgType Type { get; set; }` <- `src/EJLive.Core/Engine/CommunicationProtocol.cs`
- `MsgType Type { get; set; }` <- `src/_reference/corrupted/EJLive.Core/Services/GhostRemoteEngine.cs`
- `MsgType Type { get; set; }` <- `src/_reference/corrupted/EJLive.Core/Services/ImageSyncEngine.cs`
- `MsgType type, string text, byte[]? payload = null, byte[]? sessionK` <- `src/EJLive.Core/Engine/CommunicationProtocol.cs`
- `MsgType type, string text, byte[]? payload = null, byte[]? sessionK` <- `src/_reference/corrupted/EJLive.Core/Services/GhostRemoteEngine.cs`
- `MsgType type, string text, byte[]? payload = null, byte[]? sessionK` <- `src/_reference/corrupted/EJLive.Core/Services/ImageSyncEngine.cs`
- `MsgType.AesSessionKey, Convert.ToBase64String(encryptedKey));` <- `src/EJLive.Core/Engine/CommunicationProtocol.cs`
- `MsgType.Broadcast &&` <- `src/EJLive.Client.Service/AgentHeadlessController.cs`
- `MsgType.Broadcast &&` <- `src/_reference/uncompiled/EJLive.Client.WinForms/Agent/AgentBootstrapper.cs`
- `MsgType.Broadcast)` <- `src/EJLive.Core/Engine/OperationalEngines.cs`
- `MsgType.Broadcast,` <- `src/EJLive.Client.Service/AgentHeadlessController.cs`
- `MsgType.Broadcast,` <- `src/EJLive.Client.Service/LogBackupScheduler.cs`
- `MsgType.Broadcast,` <- `src/EJLive.Client.Service/TimeSyncScheduler.cs`
- `MsgType.Broadcast, "PULSE_JSON|" + payload));` <- `src/EJLive.Verification/Program.cs`
- `MsgType.Broadcast, "PULSE_JSON|" + pulseJson));` <- `src/_reference/uncompiled/EJLive.Client.WinForms/Agent/AgentBootstrapper.cs`
- `MsgType.Broadcast, message);` <- `src/EJLive.Core/Engine/OperationalEngines.cs`
- `MsgType.Broadcast, payload));` <- `src/EJLive.Verification/Program.cs`
- `MsgType.Broadcast, payload));` <- `src/_reference/exact-duplicates/EJLive.Core/Transport/NetworkEngine.cs`
- `MsgType.Broadcast, payload));` <- `src/_reference/uncompiled/EJLive.Client.WinForms/Agent/AgentBootstrapper.cs`
- `MsgType.Broadcast, payload));` <- `src/_reference/uncompiled/EJLive.Client.WinForms/Services/LegacySnippetAdapters.cs`
- `MsgType.Broadcast, payload));` <- `src/_reference/uncompiled/EJLive.Client.WinForms/Services/NetworkEngine.cs`
- `MsgType.Broadcast, payload));` <- `src/_reference/uncompiled/EJLive.Client.WinForms/Services/NetworkServiceAdapter.cs`
- `MsgType.Broadcast, payload));` <- `src/_reference/uncompiled/EJLive.Core/Network/NetworkEngine.cs`
- `MsgType.Broadcast, pulse));` <- `src/_reference/uncompiled/EJLive.Client.WinForms/Agent/AgentBootstrapper.cs`
- `MsgType.Broadcast, pulse));` <- `src/_reference/uncompiled/EJLive.Client.WinForms/Services/LegacySnippetAdapters.cs`
- `MsgType.Broadcast, text));` <- `src/_reference/uncompiled/EJLive.Client.WinForms/Agent/BootNotifier.cs`
- `MsgType.Broadcast, text));` <- `src/_reference/uncompiled/EJLive.Client.WinForms/Agent/LogBackupScheduler.cs`
- `MsgType.Broadcast, text));` <- `src/_reference/uncompiled/EJLive.Client.WinForms/Agent/TimeSyncScheduler.cs`
- `MsgType.Chunk)` <- `src/EJLive.Core/Engine/OperationalEngines.cs`
- `MsgType.Chunk, sequence.ToString(), chunk, sessionKey);` <- `src/EJLive.Core/Engine/CommunicationProtocol.cs`
- `MsgType.Chunk, sequence.ToString(), chunk, sessionKey);` <- `src/_reference/corrupted/EJLive.Core/Services/GhostRemoteEngine.cs`
- `MsgType.Chunk, sequence.ToString(), chunk, sessionKey);` <- `src/_reference/corrupted/EJLive.Core/Services/ImageSyncEngine.cs`
- `MsgType.Chunk:` <- `src/_reference/exact-duplicates/EJLive.Core/Network/ServerEngine.cs`
- `MsgType.Chunk:` <- `src/_reference/uncompiled/EJLive.Core/Engine/ServerEngine.cs`
- `MsgType.Chunk:` <- `src/_reference/uncompiled/EJLive.Core/Utils/ServerEngine.cs`
- `MsgType.ChunkAck)` <- `src/EJLive.Core/Engine/NetworkEngine.cs`
- `MsgType.ChunkAck)` <- `src/_reference/exact-duplicates/EJLive.Core/Transport/NetworkEngine.cs`
- `MsgType.ChunkAck)` <- `src/_reference/uncompiled/EJLive.Core/Engine/JournalNetworkEngine.cs`
- `MsgType.ChunkAck)` <- `src/_reference/uncompiled/EJLive.Core/Network/NetworkEngine.cs`
- `MsgType.ChunkAck, sequence.ToString(), BitConverter.GetBytes(sequen` <- `src/EJLive.Core/Engine/CommunicationProtocol.cs`
- `MsgType.ChunkAck, sequence.ToString(), BitConverter.GetBytes(sequen` <- `src/_reference/corrupted/EJLive.Core/Services/GhostRemoteEngine.cs`
- `MsgType.ChunkAck, sequence.ToString(), BitConverter.GetBytes(sequen` <- `src/_reference/corrupted/EJLive.Core/Services/ImageSyncEngine.cs`
- `MsgType.Command &&` <- `src/EJLive.Client.Service/AgentHeadlessController.cs`
- `MsgType.Command &&` <- `src/EJLive.Verification/Program.cs`
- `MsgType.Command)` <- `src/_reference/exact-duplicates/EJLive.Core/Transport/NetworkEngine.cs`
