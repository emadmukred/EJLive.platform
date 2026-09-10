using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using EJLive.Core.Models;


namespace EJLive.Core.Engine
{
    /// <summary>
    /// محرك الخادم الكامل — Multi-Client TCP مع Producer-Consumer Pattern (T-02, T-06)
    /// يطبق: Channel Decoupling (استلام منفصل عن المعالجة)
    ///        RSA Handshake → AES Session Key لكل جلسة (S-06)
    ///        Idempotency على الاستلام (L-03)
    ///        Chunked File Transfer مع CHUNK_ACK (T-08)
    /// </summary>
    public class ServerEngine : IDisposable
    {
        private TcpListener _listener;
        private Thread      _acceptThread;
        private volatile bool _running;
        private readonly int  _port;

        // Connected Clients
        private readonly ConcurrentDictionary<string, ClientSession> _sessions =
            new ConcurrentDictionary<string, ClientSession>();

        // Producer-Consumer Channel (T-02, T-06)
        private readonly BlockingCollection<ReceivedPacket> _incomingChannel =
            new BlockingCollection<ReceivedPacket>(10000);
        private Thread _processorThread;

        // ATM State Cache
        private readonly ConcurrentDictionary<string, ATMInfo> _atmCache =
            new ConcurrentDictionary<string, ATMInfo>();

        // Remote command lifecycle ledger
        private readonly ConcurrentDictionary<string, RemoteCommand> _commandLedger =
            new ConcurrentDictionary<string, RemoteCommand>();

        // Events
        public event EventHandler<ATMInfo>          OnATMConnected;
        public event EventHandler<ATMInfo>          OnATMDisconnected;
        public event EventHandler<ATMInfo>          OnATMUpdated;
        public event EventHandler<ReceivedPacket>   OnJournalReceived;
        public event EventHandler<RemoteCommand>    OnCommandChanged;
        public event EventHandler<string>           OnServerLog;

        // إحصائيات
        public int    ConnectedCount        => _sessions.Count;
        public long   TotalBytesReceived    { get; private set; }
        public int    TotalJournalsReceived { get; private set; }
        public DateTime StartedAt           { get; private set; }

        public IEnumerable<ATMInfo> GetConnectedATMs() => _atmCache.Values;

        public ServerEngine(int port = AppConstants.DefaultPort)
        {
            _port = port;
        }

        // ==========================================
        // التشغيل
        // ==========================================

        public void Start()
        {
            if (_running) return;
            _running = true;
            StartedAt = DateTime.UtcNow;

            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start(AppConstants.MaxConcurrentClients);
            Log($"★ EJLive Server started on TCP/{_port}");

            _acceptThread = new Thread(AcceptLoop) { IsBackground = true, Name = "EJLive.AcceptLoop" };
            _acceptThread.Start();

            _processorThread = new Thread(ProcessLoop) { IsBackground = true, Name = "EJLive.Processor" };
            _processorThread.Start();
        }

        public void Stop()
        {
            _running = false;
            _listener?.Stop();
            _incomingChannel.CompleteAdding();

            foreach (var session in _sessions.Values)
                session.Dispose();
            _sessions.Clear();
            Log("EJLive Server stopped.");
        }

        // ==========================================
        // Accept Loop — استقبال الاتصالات
        // ==========================================

        private void AcceptLoop()
        {
            while (_running)
            {
                try
                {
                    var client  = _listener.AcceptTcpClient();
                    var thread  = new Thread(() => ClientHandshake(client)) { IsBackground = true };
                    thread.Start();
                }
                catch (SocketException) { if (_running) Thread.Sleep(500); }
                catch (Exception ex) { Log($"AcceptLoop error: {ex.Message}"); }
            }
        }

        // ==========================================
        // Handshake + RSA/AES Key Exchange (S-06)
        // ==========================================

        private void ClientHandshake(TcpClient client)
        {
            string clientIp = "?";
            try
            {
                clientIp = ((IPEndPoint)client.Client.RemoteEndPoint)?.Address?.ToString() ?? "?";
                client.ReceiveTimeout = AppConstants.SocketTimeoutMs;
                client.SendTimeout    = AppConstants.SocketTimeoutMs;

                var stream = client.GetStream();

                // خطوة 1: إرسال RSA Public Key للعميل
                var (pubKeyXml, privateKey) = SecurityHelper.GenerateRSAKeyPair();
                var rsaMsg = CommunicationProtocol.BuildRsaPublicKey(pubKeyXml);
                stream.Write(rsaMsg, 0, rsaMsg.Length);

                // خطوة 2: استلام AES Session Key مشفر بـ RSA
                var aesMsg      = CommunicationProtocol.ReadMessage(stream);
                var sessionKey  = SecurityHelper.DecryptWithRSAPrivateKey(aesMsg.Payload, privateKey);
                privateKey.Dispose();

                // خطوة 3: استلام HANDSHAKE
                var handshakeMsg = CommunicationProtocol.ReadMessage(stream, sessionKey);
                var parts        = handshakeMsg.Text.Split('|');
                if (parts.Length < 4 || parts[0] != AppConstants.MSG_HANDSHAKE)
                {
                    client.Close();
                    return;
                }

                var atmId      = parts[1];
                var atmType    = parts[2];
                var clientVer  = parts[3];
                var sessionId  = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant();

                // خطوة 4: إرسال ACK
                var ack = CommunicationProtocol.BuildHandshakeAck(sessionId, AppConstants.AppVersion);
                stream.Write(ack, 0, ack.Length);

                // سجل الجلسة
                var atm = GetOrCreateATM(atmId, atmType, clientIp, sessionId, clientVer);
                var session = new ClientSession(client, stream, sessionKey, atmId, sessionId);
                _sessions[sessionId] = session;
                _atmCache[atmId]      = atm;

                atm.ConnectionStatus  = ConnectionStatus.Connected;
                atm.ConnectedAtUtc    = DateTime.UtcNow;
                atm.LastHeartbeatUtc  = DateTime.UtcNow;
                atm.SessionId         = sessionId;

                DatabaseManager.Instance.InsertAuditLog("Connection", "System", atmId,
                    $"ATM connected from {clientIp} — Session {sessionId}");

                Log($"✓ ATM connected: {atmId} ({atmType}) from {clientIp}");
                OnATMConnected?.Invoke(this, atm);

                // بدء حلقة استماع هذا العميل
                ClientReadLoop(session, atm);
            }
            catch (Exception ex)
            {
                Log($"Handshake failed from {clientIp}: {ex.Message}");
                try { client?.Close(); } catch { }
            }
        }

        // ==========================================
        // Client Read Loop — استلام البيانات
        // ==========================================

        private void ClientReadLoop(ClientSession session, ATMInfo atm)
        {
            try
            {
                while (_running && session.IsAlive)
                {
                    var msg = CommunicationProtocol.ReadMessage(session.Stream, session.SessionKey);
                    HandleMessage(session, atm, msg);
                }
            }
            catch (IOException)   { /* اتصال منقطع */ }
            catch (Exception ex)  { Log($"Client error [{atm.ATM_ID}]: {ex.Message}"); }
            finally
            {
                DisconnectClient(session, atm);
            }
        }

        private void HandleMessage(ClientSession session, ATMInfo atm, EJMessage msg)
        {
            atm.LastHeartbeatUtc = DateTime.UtcNow;

            if (CommunicationProtocol.IsHeartbeat(msg))
            {
                ApplyHeartbeatStats(atm, msg.Text);
                atm.RecalculateHealthScore();
                OnATMUpdated?.Invoke(this, atm);
                var ack = CommunicationProtocol.BuildHeartbeatAck();
                lock (session.SendLock) session.Stream.Write(ack, 0, ack.Length);
                return;
            }

            if (CommunicationProtocol.IsDisconnect(msg))
            {
                session.IsAlive = false;
                return;
            }

            // كل الرسائل الأخرى تذهب إلى Channel (T-02 — لا معالجة في thread الاستلام)
            _incomingChannel.TryAdd(new ReceivedPacket
            {
                Session   = session,
                ATM       = atm,
                Message   = msg,
                ReceivedAt = DateTime.UtcNow
            });
        }

        // ==========================================
        // Process Loop — المعالجة منفصلة (T-02, T-06)
        // ==========================================

        private void ProcessLoop()
        {
            // حالة الاستلام لكل جلسة (file chunking state)
            var fileStates = new ConcurrentDictionary<string, FileReceiveState>();

            foreach (var packet in _incomingChannel.GetConsumingEnumerable())
            {
                try
                {
                    ProcessPacket(packet, fileStates);
                }
                catch (Exception ex)
                {
                    Log($"ProcessLoop error: {ex.Message}");
                }
            }
        }

        private void ProcessPacket(ReceivedPacket pkt, ConcurrentDictionary<string, FileReceiveState> fileStates)
        {
            var session = pkt.Session;
            var atm     = pkt.ATM;
            var msg     = pkt.Message;

            switch (msg.Type)
            {
                case CommunicationProtocol.MsgType.StartFile:
                {
                    var parts    = msg.Text.Split('|');
                    var fileName = parts.Length > 2 ? parts[2] : "unknown";
                    var fileSize = parts.Length > 3 && long.TryParse(parts[3], out var s) ? s : 0;
                    var offset   = parts.Length > 4 && long.TryParse(parts[4], out var o) ? o : 0;
                    var checksum = parts.Length > 5 ? parts[5] : "";

                    // Idempotency: هل هذا الملف موجود بالفعل؟ (L-03)
                    if (DatabaseManager.Instance.IsDuplicateSync(atm.ATM_ID, fileName, checksum))
                    {
                        OperationalStateStore.Instance.UpsertUpload(CreateUploadLog(atm.ATM_ID, fileName, "EJ", fileSize, 0,
                            checksum, UploadHealthState.Duplicate, "Duplicate content skipped by server", null));
                        var dupAck = CommunicationProtocol.BuildJournalAck(fileName, true);
                        lock (session.SendLock) session.Stream.Write(dupAck, 0, dupAck.Length);
                        Log($"Idempotency: skipping duplicate {fileName} from {atm.ATM_ID}");
                        return;
                    }

                    OperationalStateStore.Instance.UpsertUpload(CreateUploadLog(atm.ATM_ID, fileName, "EJ", fileSize, offset,
                        checksum, UploadHealthState.Uploading, null, null));

                    var state = new FileReceiveState
                    {
                        SessionId = session.SessionId,
                        ATMId     = atm.ATM_ID,
                        FileName  = fileName,
                        FileSize  = fileSize,
                        Offset    = offset,
                        Checksum  = checksum,
                        Buffer    = new System.IO.MemoryStream()
                    };
                    fileStates[session.SessionId + fileName] = state;
                    atm.ConnectionStatus = ConnectionStatus.Syncing;
                    OnATMUpdated?.Invoke(this, atm);
                    break;
                }

                case CommunicationProtocol.MsgType.Chunk:
                {
                    var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
                    var key = session.SessionId + msg.Text?.Split('|')?[0]; // best effort key

                    // ابحث عن الـ state بأي طريقة
                    foreach (var kvp in fileStates)
                    {
                        if (kvp.Key.StartsWith(session.SessionId))
                        {
                            kvp.Value.Buffer.Write(data, 0, data.Length);
                            kvp.Value.LastSeqNum = seqNum;
                            TotalBytesReceived += data.Length;
                            atm.TotalSyncedBytes += data.Length;
                            break;
                        }
                    }

                    // إرسال CHUNK_ACK (T-08)
                    var ack = CommunicationProtocol.BuildChunkAck(seqNum);
                    lock (session.SendLock) session.Stream.Write(ack, 0, ack.Length);
                    break;
                }

                case CommunicationProtocol.MsgType.Complete:
                {
                    var parts     = msg.Text.Split('|');
                    var fileName  = parts.Length > 1 ? parts[1] : "";
                    var checksum  = parts.Length > 2 ? parts[2] : "";
                    var sha256    = parts.Length > 3 ? parts[3] : "";
                    var stateKey  = session.SessionId + fileName;

                    if (fileStates.TryRemove(stateKey, out var state))
                    {
                        var data  = state.Buffer.ToArray();
                        var actualMd5 = SecurityHelper.MD5Hash(data);
                        var verified  = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

                        // إرسال Journal ACK
                        var jAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
                        lock (session.SendLock) session.Stream.Write(jAck, 0, jAck.Length);

                        if (verified)
                        {
                            OperationalStateStore.Instance.UpsertUpload(CreateUploadLog(atm.ATM_ID, fileName, "EJ", state.FileSize, data.Length,
                                checksum, UploadHealthState.Acked, null, $"ACK-{atm.ATM_ID}-{DateTime.UtcNow:yyyyMMddHHmmss}"));
                            TotalJournalsReceived++;
                            atm.LastSyncUtc          = DateTime.UtcNow;
                            atm.LastDataReceivedUtc  = DateTime.UtcNow;
                            atm.LastJournalFile      = fileName;
                            atm.ConsecutiveSyncFailures = 0;
                            atm.ConnectionStatus     = ConnectionStatus.Connected;

                            // أطلق حدث للأرشفة
                            OnJournalReceived?.Invoke(this, new ReceivedPacket
                            {
                                ATM        = atm,
                                FileName   = fileName,
                                Data       = data,
                                Checksum   = checksum,
                                SHA256     = sha256,
                                ReceivedAt = DateTime.UtcNow
                            });
                        }
                        else
                        {
                            OperationalStateStore.Instance.UpsertUpload(CreateUploadLog(atm.ATM_ID, fileName, "EJ", state.FileSize, data.Length,
                                checksum, UploadHealthState.IntegrityFailure, "Checksum mismatch", null));
                            atm.ConsecutiveSyncFailures++;
                            Log($"Checksum mismatch for {fileName} from {atm.ATM_ID}");
                        }
                        atm.RecalculateHealthScore();
                        OnATMUpdated?.Invoke(this, atm);
                    }
                    break;
                }

                case CommunicationProtocol.MsgType.CommandResult:
                {
                    var parts   = msg.Text.Split('|');
                    var cmdId   = parts.Length > 1 ? parts[1] : "";
                    var success = parts.Length > 2 && parts[2] == "SUCCESS";
                    var result  = parts.Length > 3 ? parts[3] : "";
                    if (!string.IsNullOrWhiteSpace(cmdId))
                    {
                        if (!_commandLedger.TryGetValue(cmdId, out var tracked))
                        {
                            tracked = new RemoteCommand
                            {
                                CommandId = cmdId,
                                CommandType = "UNKNOWN",
                                TargetATMId = atm.ATM_ID,
                                SentBy = "System",
                                SentAtUtc = pkt.ReceivedAt
                            };
                            _commandLedger[cmdId] = tracked;
                        }

                        tracked.Status = success ? "Executed" : "Failed";
                        tracked.Result = result;
                        tracked.AckedAtUtc = DateTime.UtcNow;
                        tracked.ExecutedAt = DateTime.UtcNow;
                        NotifyCommandChanged(tracked);
                    }
                    Log($"CMD Result [{cmdId}]: {(success ? "✓" : "✗")} {result}");
                    DatabaseManager.Instance.InsertAuditLog("CommandResult", "System", atm.ATM_ID, $"CMD {cmdId}: {result}");
                    break;
                }

                case CommunicationProtocol.MsgType.GhostFrame:
                {
                    // JPEG Frame من وضع الشبح — أطلق للعرض
                    OnJournalReceived?.Invoke(this, new ReceivedPacket
                    {
                        ATM  = atm, Message = msg, ReceivedAt = DateTime.UtcNow, IsGhostFrame = true
                    });
                    break;
                }
            }
        }

        private static UploadLogRecord CreateUploadLog(string atmId, string fileName, string kind, long expected, long received,
            string checksum, UploadHealthState state, string failure, string ackId)
        {
            return new UploadLogRecord
            {
                UploadId = $"{atmId}|{fileName}|{checksum}".ToUpperInvariant(),
                TerminalId = atmId,
                FileName = fileName,
                FileKind = kind,
                BytesExpected = expected,
                BytesReceived = received,
                Checksum = checksum,
                AckId = ackId,
                State = state,
                FailureReason = failure,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };
        }

        // ==========================================
        // إرسال الأوامر
        // ==========================================

        public bool SendCommand(string atmId, string cmdType, string parameters, string sentBy = "System")
            => SendCommandDetailed(atmId, cmdType, parameters, sentBy).Status == "Sent";

        public RemoteCommand SendCommandDetailed(string atmId, string cmdType, string parameters, string sentBy = "System")
        {
            var cmdId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
            var tracked = new RemoteCommand
            {
                CommandId = cmdId,
                CommandType = cmdType,
                TargetATMId = atmId,
                Parameters = new CommandParameters { Raw = parameters ?? "" },
                SentBy = sentBy,
                SentAtUtc = DateTime.UtcNow,
                RequireConfirm = Array.IndexOf(AppConstants.CommandsRequireConfirmation, cmdType) >= 0,
                Status = "Created"
            };
            _commandLedger[cmdId] = tracked;

            var atm = GetATMByID(atmId);
            if (atm == null)
            {
                tracked.Status = "Failed";
                tracked.Result = "الصراف غير معروف في ذاكرة الخادم";
                tracked.AckedAtUtc = DateTime.UtcNow;
                NotifyCommandChanged(tracked);
                return tracked;
            }

            var session = GetSessionByATMId(atmId);
            if (session == null)
            {
                tracked.Status = "Failed";
                tracked.Result = "لا توجد جلسة اتصال نشطة للصراف";
                tracked.AckedAtUtc = DateTime.UtcNow;
                NotifyCommandChanged(tracked);
                return tracked;
            }

            var frame  = CommunicationProtocol.BuildCommand(cmdType, cmdId, atmId, parameters ?? "", session.SessionKey);

            try
            {
                lock (session.SendLock)
                {
                    session.Stream.Write(frame, 0, frame.Length);
                    session.Stream.Flush();
                }
                tracked.Status = "Sent";
                DatabaseManager.Instance.InsertAuditLog("Command", sentBy, atmId, $"{cmdType} [{cmdId}]");
                Log($"→ CMD {cmdType} sent to {atmId} [{cmdId}]");
                NotifyCommandChanged(tracked);
                return tracked;
            }
            catch (Exception ex)
            {
                tracked.Status = "Failed";
                tracked.Result = ex.Message;
                tracked.AckedAtUtc = DateTime.UtcNow;
                Log($"Failed to send CMD to {atmId}: {ex.Message}");
                NotifyCommandChanged(tracked);
                return tracked;
            }
        }

        public IEnumerable<RemoteCommand> GetRecentCommands()
            => new List<RemoteCommand>(_commandLedger.Values);

        private void NotifyCommandChanged(RemoteCommand command)
            => OnCommandChanged?.Invoke(this, command);

        public void Broadcast(string message, string sentBy = "System")
        {
            foreach (var session in _sessions.Values)
            {
                try
                {
                    var frame = CommunicationProtocol.BuildBroadcast(message, session.SessionKey);
                    lock (session.SendLock)
                        session.Stream.Write(frame, 0, frame.Length);
                }
                catch { }
            }
            Log($"★ Broadcast sent to {_sessions.Count} clients: {message}");
        }

        // ==========================================
        // إدارة الجلسات
        // ==========================================

        private void DisconnectClient(ClientSession session, ATMInfo atm)
        {
            _sessions.TryRemove(session.SessionId, out _);
            session.Dispose();

            atm.ConnectionStatus    = ConnectionStatus.Disconnected;
            atm.DisconnectedAtUtc   = DateTime.UtcNow;
            atm.SessionId           = null;
            atm.RecalculateHealthScore();

            OnATMDisconnected?.Invoke(this, atm);
            Log($"✕ ATM disconnected: {atm.ATM_ID}");
        }

        private void ApplyHeartbeatStats(ATMInfo atm, string heartbeatText)
        {
            try
            {
                var parts = (heartbeatText ?? string.Empty).Split('|');
                if (parts.Length > 2 && DateTime.TryParse(parts[2], out var sentAt))
                {
                    var latency = (DateTime.UtcNow - sentAt.ToUniversalTime()).TotalMilliseconds;
                    if (latency >= 0 && latency <= 120000)
                        atm.Latency_ms = (int)latency;
                }

                if (parts.Length <= 3) return;
                var kvPairs = parts[3].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var kv in kvPairs)
                {
                    var idx = kv.IndexOf('=');
                    if (idx <= 0) continue;
                    var key = kv.Substring(0, idx).Trim().ToUpperInvariant();
                    var val = kv.Substring(idx + 1).Trim();

                    if (!double.TryParse(val, out var num)) continue;
                    switch (key)
                    {
                        case "CPU":
                            atm.CpuUsagePercent = Math.Max(0, Math.Min(100, num));
                            break;
                        case "MEM":
                            atm.MemoryUsagePercent = Math.Max(0, Math.Min(100, num));
                            break;
                        case "DISK":
                            atm.DiskUsagePercent = Math.Max(0, Math.Min(100, num));
                            break;
                        case "HEALTH":
                            atm.HealthScore = (int)Math.Max(0, Math.Min(100, num));
                            break;
                    }
                }
            }
            catch
            {
                // ignore malformed heartbeat extension payloads
            }
        }

        private ATMInfo GetOrCreateATM(string atmId, string atmType, string ip, string sessionId, string version)
        {
            if (!_atmCache.TryGetValue(atmId, out var atm))
            {
                atm = new ATMInfo
                {
                    ATM_ID = atmId,
                    ATM_Type = AppConstants.NormalizeATMType(atmType),
                    ATM_Name = atmId
                };
                _atmCache[atmId] = atm;
            }
            else
            {
                atm.ATM_Type = AppConstants.NormalizeATMType(atmType);
            }
            atm.ServerIP       = ip;
            atm.SessionId      = sessionId;
            atm.ClientVersion  = version;
            return atm;
        }

        public ATMInfo GetATMByID(string atmId) =>
            _atmCache.TryGetValue(atmId, out var a) ? a : null;

        private ClientSession GetSessionByATMId(string atmId)
        {
            foreach (var s in _sessions.Values)
                if (s.ATMId == atmId && s.IsAlive) return s;
            return null;
        }

        private void Log(string msg) => OnServerLog?.Invoke(this, msg);

        public void Dispose() => Stop();
    }

    // ==========================================
    // نماذج الجلسة والحزمة
    // ==========================================

    public class ClientSession : IDisposable
    {
        public TcpClient   Client      { get; }
        public NetworkStream Stream    { get; }
        public byte[]      SessionKey  { get; }
        public string      ATMId       { get; }
        public string      SessionId   { get; }
        public object      SendLock    { get; } = new object();
        public volatile bool IsAlive   = true;

        public ClientSession(TcpClient client, NetworkStream stream, byte[] key, string atmId, string sessionId)
        {
            Client     = client;
            Stream     = stream;
            SessionKey = key;
            ATMId      = atmId;
            SessionId  = sessionId;
        }

        public void Dispose()
        {
            IsAlive = false;
            try { Stream?.Close(); Client?.Close(); } catch { }
        }
    }

    public class FileReceiveState
    {
        public string            SessionId  { get; set; }
        public string            ATMId      { get; set; }
        public string            FileName   { get; set; }
        public long              FileSize   { get; set; }
        public long              Offset     { get; set; }
        public string            Checksum   { get; set; }
        public MemoryStream      Buffer     { get; set; }
        public int               LastSeqNum { get; set; }
    }

    public class ReceivedPacket
    {
        public ClientSession Session     { get; set; }
        public ATMInfo       ATM         { get; set; }
        public EJMessage     Message     { get; set; }
        public string        FileName    { get; set; }
        public byte[]        Data        { get; set; }
        public string        Checksum    { get; set; }
        public string        SHA256      { get; set; }
        public DateTime      ReceivedAt  { get; set; }
        public bool          IsGhostFrame { get; set; }
    
    /// <summary>
    /// محرك السيرفر - يستقبل اتصالات الكلاينتات ويدير البيانات الواردة
    /// </summary>
    public class ServerEngine
    {
        private TcpListener _listener;
        private Thread _listenThread;
        private bool _isRunning;
        private int _port;
        private string _storagePath;
        private Dictionary<string, ClientConnection> _connections;
        private readonly object _connectionsLock = new object();

        // أحداث
        public event Action<string, ATMInfo> OnClientConnected;
        public event Action<string> OnClientDisconnected;
        public event Action<string, string, byte[]> OnDataReceived;  // (atmId, fileName, data)
        public event Action<string, string, byte[]> OnFileReceived;  // (atmId, fileName, data)
        public event Action<string, string> OnStatusReceived;        // (atmId, status)
        public event Action<string, string, string> OnCommandResult; // (atmId, cmdId, result)
        public event Action<string> OnLog;
        public event Action<Exception> OnError;

        public bool IsRunning { get { return _isRunning; } }
        public int ConnectedClients { get { lock (_connectionsLock) return _connections.Count; } }

        public ServerEngine(int port, string storagePath)
        {
            _port = port;
            _storagePath = storagePath;
            _connections = new Dictionary<string, ClientConnection>();
        }

        /// <summary>
        /// بدء السيرفر
        /// </summary>
        public bool Start()
        {
            try
            {
                if (_isRunning) return true;

                if (!Directory.Exists(_storagePath))
                    Directory.CreateDirectory(_storagePath);

                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
                _isRunning = true;

                _listenThread = new Thread(ListenLoop);
                _listenThread.IsBackground = true;
                _listenThread.Start();

                OnLog?.Invoke("[Server] Started on port " + _port);
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
        }

        /// <summary>
        /// إيقاف السيرفر
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            try
            {
                _listener?.Stop();
                lock (_connectionsLock)
                {
                    foreach (var conn in _connections.Values)
                        conn.Disconnect();
                    _connections.Clear();
                }
            }
            catch { }
            OnLog?.Invoke("[Server] Stopped");
        }

        /// <summary>
        /// إرسال أمر إلى صراف محدد
        /// </summary>
        public bool SendCommand(string atmId, string command, string[] parameters)
        {
            lock (_connectionsLock)
            {
                if (!_connections.ContainsKey(atmId)) return false;
                var conn = _connections[atmId];
                string msg = Protocol.BuildMessage(command, parameters);
                return conn.Send(msg);
            }
        }

        /// <summary>
        /// إرسال أمر لجميع الصرافات المتصلة
        /// </summary>
        public int BroadcastCommand(string command, string[] parameters)
        {
            int sent = 0;
            lock (_connectionsLock)
            {
                foreach (var conn in _connections.Values)
                {
                    string msg = Protocol.BuildMessage(command, parameters);
                    if (conn.Send(msg)) sent++;
                }
            }
            return sent;
        }

        /// <summary>
        /// الحصول على قائمة الصرافات المتصلة
        /// </summary>
        public List<ATMInfo> GetConnectedATMs()
        {
            var list = new List<ATMInfo>();
            lock (_connectionsLock)
            {
                foreach (var conn in _connections.Values)
                    list.Add(conn.ATMData);
            }
            return list;
        }

        /// <summary>
        /// طلب حالة صراف محدد
        /// </summary>
        public bool RequestStatus(string atmId)
        {
            return SendCommand(atmId, Protocol.STATUS_REQUEST, new string[] { });
        }

        #region Private Methods

        private void ListenLoop()
        {
            while (_isRunning)
            {
                try
                {
                    if (_listener.Pending())
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        Thread clientThread = new Thread(() => HandleClient(client));
                        clientThread.IsBackground = true;
                        clientThread.Start();
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    if (_isRunning) OnError?.Invoke(ex);
                }
            }
        }

        private void HandleClient(TcpClient tcpClient)
        {
            string atmId = null;
            try
            {
                var stream = tcpClient.GetStream();
                stream.ReadTimeout = NetworkConfig.CONNECTION_TIMEOUT_MS;

                // انتظار Handshake
                string handshake = ReadLine(stream, 10000);
                if (string.IsNullOrEmpty(handshake))
                {
                    tcpClient.Close();
                    return;
                }

                string[] parts = Protocol.ParseMessage(handshake);
                if (parts.Length < 4 || parts[0] != Protocol.HANDSHAKE)
                {
                    // رفض الاتصال
                    byte[] reject = Encoding.UTF8.GetBytes(Protocol.HANDSHAKE_REJECT + "\n");
                    stream.Write(reject, 0, reject.Length);
                    tcpClient.Close();
                    return;
                }

                atmId = parts[1];
                string atmType = parts[2];
                string version = parts[3];

                // قبول الاتصال
                byte[] ack = Encoding.UTF8.GetBytes(Protocol.HANDSHAKE_ACK + "\n");
                stream.Write(ack, 0, ack.Length);

                // إنشاء اتصال جديد
                var atmInfo = new ATMInfo
                {
                    ATM_ID = atmId,
                    ATM_Type = atmType,
                    ClientVersion = version,
                    IsConnected = true,
                    LastConnectionTime = DateTime.Now,
                    LastHeartbeat = DateTime.Now,
                    Status = ATMStatus.Online_Active
                };

                var connection = new ClientConnection(tcpClient, stream, atmInfo);

                lock (_connectionsLock)
                {
                    if (_connections.ContainsKey(atmId))
                    {
                        _connections[atmId].Disconnect();
                        _connections.Remove(atmId);
                    }
                    _connections[atmId] = connection;
                }

                // إنشاء مجلد للصراف في التخزين
                string atmFolder = Path.Combine(_storagePath, atmId);
                if (!Directory.Exists(atmFolder))
                    Directory.CreateDirectory(atmFolder);

                OnLog?.Invoke("[Server] Client connected: " + atmId + " (" + atmType + ")");
                OnClientConnected?.Invoke(atmId, atmInfo);

                // حلقة الاستقبال
                ClientReceiveLoop(connection, atmFolder);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
            finally
            {
                if (atmId != null)
                {
                    lock (_connectionsLock)
                    {
                        if (_connections.ContainsKey(atmId))
                        {
                            _connections[atmId].ATMData.IsConnected = false;
                            _connections[atmId].ATMData.Status = ATMStatus.Offline;
                            _connections.Remove(atmId);
                        }
                    }
                    OnClientDisconnected?.Invoke(atmId);
                    OnLog?.Invoke("[Server] Client disconnected: " + atmId);
                }
                try { tcpClient.Close(); } catch { }
            }
        }

        private void ClientReceiveLoop(ClientConnection conn, string atmFolder)
        {
            byte[] buffer = new byte[NetworkConfig.SOCKET_BUFFER_SIZE];
            StringBuilder msgBuilder = new StringBuilder();

            while (_isRunning && conn.IsConnected)
            {
                try
                {
                    if (!conn.Stream.DataAvailable)
                    {
                        Thread.Sleep(50);
                        // فحص Heartbeat timeout
                        if ((DateTime.Now - conn.ATMData.LastHeartbeat).TotalSeconds > 90)
                        {
                            OnLog?.Invoke("[Server] Heartbeat timeout: " + conn.ATMData.ATM_ID);
                            break;
                        }
                        continue;
                    }

                    int bytesRead = conn.Stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    msgBuilder.Append(received);

                    string full = msgBuilder.ToString();
                    int idx;
                    while ((idx = full.IndexOf('\n')) >= 0)
                    {
                        string line = full.Substring(0, idx).Trim();
                        full = full.Substring(idx + 1);

                        if (!string.IsNullOrEmpty(line))
                        {
                            ProcessClientMessage(conn, line, atmFolder);
                        }
                    }
                    msgBuilder.Clear();
                    if (full.Length > 0) msgBuilder.Append(full);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private void ProcessClientMessage(ClientConnection conn, string message, string atmFolder)
        {
            string[] parts = Protocol.ParseMessage(message);
            if (parts.Length == 0) return;

            string msgType = parts[0];
            string atmId = conn.ATMData.ATM_ID;

            switch (msgType)
            {
                case Protocol.HEARTBEAT:
                    conn.ATMData.LastHeartbeat = DateTime.Now;
                    conn.ATMData.IsConnected = true;
                    // رد Heartbeat
                    conn.Send(Protocol.HEARTBEAT_ACK);
                    break;

                case Protocol.DATA_JOURNAL:
                    // EJDATA|ATM_ID|FILENAME|LENGTH|CHECKSUM|TIMESTAMP
                    if (parts.Length >= 5)
                    {
                        string fileName = parts[2];
                        int dataLength = int.Parse(parts[3]);
                        // البيانات ستأتي في الرسائل التالية
                        conn.ATMData.IsSendingData = true;
                        conn.ATMData.LastDataReceived = DateTime.Now;
                        conn.ATMData.Status = ATMStatus.Online_Active;
                        conn.ATMData.TotalLinesSent++;

                        // حفظ البيانات
                        OnDataReceived?.Invoke(atmId, fileName, null);
                        conn.Send(Protocol.DATA_ACK);
                    }
                    break;

                case Protocol.DATA_FILE:
                    if (parts.Length >= 5)
                    {
                        string fileName = parts[2];
                        conn.ATMData.IsSendingData = true;
                        conn.ATMData.LastDataReceived = DateTime.Now;
                        conn.ATMData.TotalFilesSynced++;

                        OnFileReceived?.Invoke(atmId, fileName, null);
                        conn.Send(Protocol.DATA_ACK);
                    }
                    break;

                case Protocol.STATUS_RESPONSE:
                    if (parts.Length >= 4)
                    {
                        string status = parts[2];
                        string cscStatus = parts.Length > 3 ? parts[3] : "";
                        conn.ATMData.IsCSCConnected = (cscStatus == "CSC_OK");
                        OnStatusReceived?.Invoke(atmId, status);
                    }
                    break;

                case Protocol.CMD_RESULT:
                    if (parts.Length >= 4)
                    {
                        string cmdId = parts[2];
                        string result = parts[3];
                        OnCommandResult?.Invoke(atmId, cmdId, result);
                    }
                    break;
            }
        }

        private string ReadLine(NetworkStream stream, int timeoutMs)
        {
            byte[] buffer = new byte[4096];
            stream.ReadTimeout = timeoutMs;
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                    return Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            }
            catch { }
            return null;
        }

        #endregion
    }

    /// <summary>
    /// يمثل اتصال كلاينت واحد
    /// </summary>
    public class ClientConnection
    {
        public TcpClient Client { get; private set; }
        public NetworkStream Stream { get; private set; }
        public ATMInfo ATMData { get; set; }
        public bool IsConnected { get; private set; }
        private readonly object _sendLock = new object();

        public ClientConnection(TcpClient client, NetworkStream stream, ATMInfo atmData)
        {
            Client = client;
            Stream = stream;
            ATMData = atmData;
            IsConnected = true;
        }

        public bool Send(string message)
        {
            try
            {
                if (!IsConnected) return false;
                lock (_sendLock)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                    Stream.Write(data, 0, data.Length);
                    Stream.Flush();
                }
                return true;
            }
            catch { IsConnected = false; return false; }
        }

        public void Disconnect()
        {
            IsConnected = false;
            try { Stream?.Close(); } catch { }
            try { Client?.Close(); } catch { }
        }
    }
}
