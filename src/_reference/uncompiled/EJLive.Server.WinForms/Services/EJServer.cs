using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;

namespace EJLive.Server.WinForms.Services
{
    /// <summary>
    /// خدمة الخادم الرئيسية — Facade يجمع ServerEngine + ArchiveManager + ImageSyncEngine
    /// تُستخدم من ServerMainForm كواجهة موحدة لكل عمليات الخادم
    /// يطبق: D-02 (تسجيل الأحداث), D-04 (Audit Log), D-06 (Toast Events)
    /// </summary>
    public sealed class EJServerService : IDisposable
    {
        private readonly ServerEngine    _serverEngine;
        private readonly ArchiveManager  _archiveManager;
        private readonly ImageSyncEngine _imageSync;
        private readonly JournalSyncTracker _syncTracker;

        private bool _isRunning;
        private readonly int _port;

        // ==========================================
        // إحصائيات مجمعة
        // ==========================================

        public int    ConnectedATMs        => _serverEngine.ConnectedCount;
        public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
        public int    TotalJournalsArchived { get; private set; }
        public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
        public bool   IsRunning            => _isRunning;
        public DateTime StartedAt          => _serverEngine.StartedAt;

        // ==========================================
        // الأحداث
        // ==========================================

        public event EventHandler<ATMInfo>      OnATMConnected;
        public event EventHandler<ATMInfo>      OnATMDisconnected;
        public event EventHandler<ATMInfo>      OnATMUpdated;
        public event EventHandler<AlertPayload> OnAlert;
        public event EventHandler<RemoteCommand> OnCommandChanged;
        public event EventHandler<string>       OnLog;

        public EJServerService(int port = AppConstants.DefaultPort, string archivePath = null)
        {
            _port           = port;
            _serverEngine   = new ServerEngine(port);
            _archiveManager = new ArchiveManager(archivePath);
            _imageSync      = new ImageSyncEngine();
            _syncTracker    = new JournalSyncTracker();

            WireEvents();
        }

        private void WireEvents()
        {
            _serverEngine.OnATMConnected    += (s, a) => OnATMConnected?.Invoke(this, a);
            _serverEngine.OnATMDisconnected += (s, a) => { OnATMDisconnected?.Invoke(this, a); CheckHealth(a); };
            _serverEngine.OnATMUpdated      += (s, a) => OnATMUpdated?.Invoke(this, a);
            _serverEngine.OnJournalReceived += OnJournalReceived;
            _serverEngine.OnServerLog       += (s, m) => Log(m);
            _serverEngine.OnCommandChanged  += (s, c) => OnCommandChanged?.Invoke(this, c);

            _archiveManager.OnArchived += (s, p) => Log($"📦 Archived: {p}");
            _archiveManager.OnError    += (s, e) => Log($"Archive Error: {e}");

            AlertManager.Instance.OnAlert    += (s, a) => OnAlert?.Invoke(this, a);
            AlertManager.Instance.OnCritical += (s, a) => OnAlert?.Invoke(this, a);
        }

        // ==========================================
        // تشغيل وإيقاف الخادم
        // ==========================================

        public void Start()
        {
            if (_isRunning) return;
            _serverEngine.Start();
            _isRunning = true;
            DatabaseManager.Instance.InsertAuditLog("ServerStart", "System", null, $"EJLive Server started on TCP/{_port}");
            Log($"★ EJLive Enterprise Server v{AppConstants.AppVersion} started on TCP/{_port}");
        }

        public void Stop()
        {
            if (!_isRunning) return;
            _serverEngine.Stop();
            _isRunning = false;
            DatabaseManager.Instance.InsertAuditLog("ServerStop", "System", null, "EJLive Server stopped");
            Log("EJLive Server stopped.");
        }

        // ==========================================
        // استلام الجورنال وأرشفته
        // ==========================================

        private void OnJournalReceived(object sender, ReceivedPacket pkt)
        {
            if (pkt.IsGhostFrame || pkt.Data == null || pkt.Data.Length == 0) return;

            var thread = new Thread(() =>
            {
                try
                {
                    // Idempotency: هل مزامن من قبل؟ (L-03)
                    if (DatabaseManager.Instance.IsDuplicateSync(pkt.ATM.ATM_ID, pkt.FileName, pkt.Checksum))
                    {
                        Log($"Idempotency: duplicate skipped {pkt.FileName} from {pkt.ATM.ATM_ID}");
                        return;
                    }

                    // أرشفة
                    var archivePath = _archiveManager.Archive(pkt.ATM.ATM_ID, pkt.FileName, pkt.Data, pkt.Checksum, pkt.SHA256);
                    if (archivePath != null) TotalJournalsArchived++;

                    // تحديث حالة المزامنة في DB
                    var syncId = _syncTracker.AddOrGet(pkt.ATM.ATM_ID, pkt.FileName, pkt.Data.Length, 0, pkt.Checksum)?.SyncId;
                    if (!string.IsNullOrEmpty(syncId))
                    {
                        _syncTracker.MarkCompleted(pkt.ATM.ATM_ID, syncId, pkt.SHA256);
                    }

                    // تحديث إحصائيات الصراف
                    pkt.ATM.LastSyncUtc         = DateTime.UtcNow;
                    pkt.ATM.LastJournalFile      = pkt.FileName;
                    pkt.ATM.JournalSizeToday    += pkt.Data.Length;

                    DatabaseManager.Instance.UpdateDailyStats(pkt.ATM.ATM_ID, DateTime.UtcNow);
                    DatabaseManager.Instance.InsertAuditLog("JournalReceived", "System", pkt.ATM.ATM_ID,
                        $"{pkt.FileName} [{pkt.Data.Length / 1024.0:F1} KB]");
                }
                catch (Exception ex)
                {
                    AppLogger.Instance.Error(ex, "EJServer.JournalReceived");
                }
            }) { IsBackground = true };
            thread.Start();
        }

        // ==========================================
        // إرسال الأوامر
        // ==========================================

        public bool SendCommand(string atmId, string cmdType, string parameters, string sentBy = "System")
            => _serverEngine.SendCommand(atmId, cmdType, parameters, sentBy);

        public RemoteCommand SendCommandDetailed(string atmId, string cmdType, string parameters, string sentBy = "System")
            => _serverEngine.SendCommandDetailed(atmId, cmdType, parameters, sentBy);

        public void Broadcast(string message, string sentBy = "System")
            => _serverEngine.Broadcast(message, sentBy);

        public IEnumerable<RemoteCommand> GetRecentCommands()
            => _serverEngine.GetRecentCommands();

        public bool SendImagesToATM(string atmId, string imagesFolder)
        {
            // إرسال الصور عبر CMD_SYNC_IMAGES
            return SendCommand(atmId, AppConstants.CMD_SYNC_IMAGES, imagesFolder, "System");
        }

        // ==========================================
        // الصحة والتنبيهات
        // ==========================================

        private void CheckHealth(ATMInfo atm)
        {
            AlertManager.Instance.CheckATMHealth(atm);
            AlertManager.Instance.CheckDiskSpace("Server", FreeSpaceGB, _archiveManager.GetTotalSpaceGB());
        }

        public void CheckAllATMHealth()
        {
            foreach (var atm in _serverEngine.GetConnectedATMs())
                CheckHealth(atm);
        }

        // ==========================================
        // الاستعلام
        // ==========================================

        public IEnumerable<ATMInfo>     GetConnectedATMs()   => _serverEngine.GetConnectedATMs();
        public ATMInfo                  GetATM(string atmId) => _serverEngine.GetATMByID(atmId);

        public (long files, long bytes) GetArchiveStats(string atmId)
            => _archiveManager.GetATMArchiveStats(atmId);

        // ==========================================
        // تسجيل
        // ==========================================

        private void Log(string msg)
        {
            AppLogger.Instance.Info(msg, "EJServer");
            OnLog?.Invoke(this, msg);
        }

        public void Dispose()
        {
            Stop();
            _serverEngine.Dispose();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using EJLive.Core;
using EJLive.Core.Services;
using EJLive.Shared.Monitoring;

namespace EJLive.Server.WinForms.Services
{
    public class EJServer
    {
        private readonly object _clientsSync = new object();
        private TcpListener _listener;
        private bool _isRunning;
        private int _port;
        private string _storagePath;
        private List<ClientConnection> _clients;
        private Thread _listenThread;
        private MonitoringStateStore _monitoringStore;
        private JournalSyncService _journalSyncService;

        public event Action<string> OnLogMessage;
        public event Action<string, bool> OnClientStatusChanged;

        public EJServer(int port, string storagePath)
        {
            _port = port;
            _storagePath = storagePath;
            _clients = new List<ClientConnection>();
            _monitoringStore = new MonitoringStateStore(storagePath);
            _journalSyncService = new JournalSyncService(storagePath);
        }

        public void Start()
        {
            _isRunning = true;
            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath);

            _monitoringStore.EnsureInitialized();
            _journalSyncService.EnsureInitialized();
            _monitoringStore.RecordSystemEvent("Info", "Server started on port " + _port);

            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();

            _listenThread = new Thread(ListenLoop);
            _listenThread.IsBackground = true;
            _listenThread.Start();

            Log("Server started on port " + _port);
            Log("Storage path: " + _storagePath);
        }

        public void Stop()
        {
            _isRunning = false;
            try
            {
                _listener?.Stop();
                lock (_clientsSync)
                {
                    foreach (var client in _clients)
                        client.Disconnect();
                    _clients.Clear();
                }
            }
            catch { }

            _monitoringStore.RecordSystemEvent("Info", "Server stopped");
            Log("Server stopped.");
        }

        private void ListenLoop()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient tcpClient = _listener.AcceptTcpClient();
                    var connection = new ClientConnection(tcpClient, _storagePath);
                    connection.OnLogMessage += (msg) => Log(msg);
                    connection.OnDataReceived += HandleDataReceived;
                    connection.OnHandshakeReceived += HandleHandshakeReceived;
                    connection.OnHeartbeatReceived += HandleHeartbeatReceived;
                    connection.OnMetadataReceived += HandleMetadataReceived;
                    connection.OnCashStatusReceived += HandleCashStatusReceived;
                    connection.OnDisconnected += HandleClientDisconnected;

                    lock (_clientsSync)
                    {
                        _clients.Add(connection);
                    }

                    connection.Start();
                    Log("New client connected: " + tcpClient.Client.RemoteEndPoint);
                }
                catch (SocketException)
                {
                    if (!_isRunning) break;
                }
            }
        }

        private void HandleHandshakeReceived(string atmId, string remoteEndpoint)
        {
            _monitoringStore.RecordTerminalConnected(atmId, remoteEndpoint);
            _journalSyncService.RecordHeartbeat(atmId, true);
            Log("Client handshake received from " + atmId + " @ " + remoteEndpoint);
            OnClientStatusChanged?.Invoke(atmId, true);
        }

        private void HandleHeartbeatReceived(string atmId)
        {
            _monitoringStore.RecordHeartbeat(atmId);
            _journalSyncService.RecordHeartbeat(atmId, true);
            OnClientStatusChanged?.Invoke(atmId, true);
        }

        private void HandleMetadataReceived(string atmId, Dictionary<string, string> metadata)
        {
            string branch = GetValue(metadata, "branch", atmId);
            string vendor = GetValue(metadata, "vendor", "Unknown");
            string network = GetValue(metadata, "network", "Unknown");
            string region = GetValue(metadata, "region", "Unknown");
            _monitoringStore.UpdateTerminalMetadata(atmId, branch, vendor, network, region);
            Log("Updated metadata for " + atmId + " | branch=" + branch + " | vendor=" + vendor + " | network=" + network);
        }

        private void HandleCashStatusReceived(string atmId, Dictionary<string, string> payload)
        {
            var cash = new MonitoringCashState
            {
                Cass1 = ParseInt(payload, "cass1"),
                Cass2 = ParseInt(payload, "cass2"),
                Cass3 = ParseInt(payload, "cass3"),
                Cass4 = ParseInt(payload, "cass4"),
                Remaining = ParseInt(payload, "remaining"),
                Loaded = ParseInt(payload, "loaded"),
                DepositIn = ParseInt(payload, "depositIn"),
                DispenseOut = ParseInt(payload, "dispenseOut"),
                Reject = ParseInt(payload, "reject"),
                Retract = ParseInt(payload, "retract"),
                UpdatedAtUtc = DateTime.UtcNow
            };
            _monitoringStore.UpdateCashState(atmId, cash);
            Log("Updated cassette state for " + atmId + " | remaining=" + cash.Remaining);
        }

        private void HandleClientDisconnected(string atmId)
        {
            lock (_clientsSync)
            {
                _clients.RemoveAll(c => string.Equals(c.ATMID, atmId, StringComparison.OrdinalIgnoreCase) || (!c.IsConnected && string.Equals(c.ATMID, "Unknown", StringComparison.OrdinalIgnoreCase)));
            }

            _monitoringStore.RecordTerminalDisconnected(atmId);
            _journalSyncService.RecordHeartbeat(atmId, false);
            _monitoringStore.RecordAlert(atmId, "Warning", "Terminal disconnected from central server.");
            Log("Client disconnected: " + atmId);
            OnClientStatusChanged?.Invoke(atmId, false);
        }

        private void HandleDataReceived(string atmId, byte[] encryptedData)
        {
            try
            {
                byte[] compressed = DecryptAES256(encryptedData);
                byte[] rawData = DecompressData(compressed);

                string atmDir = Path.Combine(_storagePath, atmId, DateTime.Now.ToString("yyyy-MM"));
                if (!Directory.Exists(atmDir))
                    Directory.CreateDirectory(atmDir);

                string fileName = "EJ_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".dat";
                string filePath = Path.Combine(atmDir, fileName);
                File.WriteAllBytes(filePath, rawData);

                string checksum = ComputeChecksum(rawData);
                _journalSyncService.RecordSyncSuccess(atmId, fileName, rawData.Length, checksum);
                _monitoringStore.RecordEjSync(atmId, rawData.Length);
                Log("Stored " + rawData.Length + " bytes from " + atmId + " -> " + fileName);
                OnClientStatusChanged?.Invoke(atmId, true);
            }
            catch (Exception ex)
            {
                _journalSyncService.RecordSyncFailure(atmId, ex.Message);
                _monitoringStore.RecordAlert(atmId, "Critical", "Error processing inbound EJ payload: " + ex.Message);
                Log("Error processing data from " + atmId + ": " + ex.Message);
            }
        }

        private byte[] DecryptAES256(byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Constants.Encryption.AESKey);
                aes.IV = Encoding.UTF8.GetBytes(Constants.Encryption.AESIV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    return decryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        private byte[] DecompressData(byte[] data)
        {
            using (MemoryStream input = new MemoryStream(data))
            using (DeflateStream deflate = new DeflateStream(input, CompressionMode.Decompress))
            using (MemoryStream output = new MemoryStream())
            {
                deflate.CopyTo(output);
                return output.ToArray();
            }
        }

        public void SendCommand(string atmId, string command, string parameters)
        {
            ClientConnection client;
            lock (_clientsSync)
            {
                client = _clients.Find(c => c.ATMID == atmId);
            }

            if (client != null && client.IsConnected)
            {
                string cmdPacket = "CMD|" + command + "|" + parameters;
                client.SendData(Encoding.UTF8.GetBytes(cmdPacket + "\n"));
                _monitoringStore.RecordAlert(atmId, "Info", "Command sent: " + command);
                Log("Command sent to " + atmId + ": " + command);
            }
            else
            {
                _monitoringStore.RecordAlert(atmId, "Warning", "Command rejected because terminal is not connected: " + command);
                Log("Cannot send command: " + atmId + " not connected.");
            }
        }

        private string ComputeChecksum(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private static int ParseInt(Dictionary<string, string> values, string key)
        {
            int parsed;
            return int.TryParse(GetValue(values, key, "0"), out parsed) ? parsed : 0;
        }

        private static string GetValue(Dictionary<string, string> values, string key, string fallback)
        {
            if (values == null) return fallback;
            string value;
            return values.TryGetValue(key, out value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
        }

        private void Log(string message)
        {
            OnLogMessage?.Invoke("[Server] " + message);
        }
    }

    public class ClientConnection
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private Thread _readThread;
        private bool _isConnected;

        public string ATMID { get; private set; }
        public bool IsConnected { get { return _isConnected; } }
        public string RemoteEndpoint
        {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
        }

        public event Action<string> OnLogMessage;
        public event Action<string, byte[]> OnDataReceived;
        public event Action<string, string> OnHandshakeReceived;
        public event Action<string> OnHeartbeatReceived;
        public event Action<string, Dictionary<string, string>> OnMetadataReceived;
        public event Action<string, Dictionary<string, string>> OnCashStatusReceived;
        public event Action<string> OnDisconnected;

        public ClientConnection(TcpClient client, string storagePath)
        {
            _tcpClient = client;
            _stream = client.GetStream();
            _isConnected = true;
            ATMID = "Unknown";
        }

        public void Start()
        {
            _readThread = new Thread(ReadLoop);
            _readThread.IsBackground = true;
            _readThread.Start();
        }

        private void ReadLoop()
        {
            byte[] buffer = new byte[65536];
            try
            {
                while (_isConnected)
                {
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    if (data.StartsWith("EJDATA"))
                    {
                        int headerEnd = data.IndexOf('\n');
                        if (headerEnd > 0)
                        {
                            string header = data.Substring(0, headerEnd);
                            string[] parts = header.Split('|');
                            if (parts.Length >= 3)
                            {
                                string atmId = parts[1];
                                int dataLen;
                                int.TryParse(parts[2], out dataLen);
                                int payloadLength = bytesRead - headerEnd - 1;
                                if (payloadLength < 0) payloadLength = 0;
                                if (dataLen > 0 && payloadLength > dataLen) payloadLength = dataLen;
                                byte[] ejData = new byte[payloadLength];
                                Array.Copy(buffer, headerEnd + 1, ejData, 0, payloadLength);
                                OnDataReceived?.Invoke(atmId, ejData);
                            }
                        }
                        continue;
                    }

                    foreach (string rawLine in data.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string line = rawLine.Trim();
                        if (line.Length == 0) continue;

                        if (line.StartsWith("EJLIVE_HANDSHAKE", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] parts = line.Split('|');
                            if (parts.Length >= 2) ATMID = parts[1];
                            OnLogMessage?.Invoke("Client identified: " + ATMID);
                            OnHandshakeReceived?.Invoke(ATMID, RemoteEndpoint);
                        }
                        else if (line.StartsWith("HEARTBEAT", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] parts = line.Split('|');
                            if (parts.Length >= 2) ATMID = parts[1];
                            OnHeartbeatReceived?.Invoke(ATMID);
                        }
                        else if (line.StartsWith("STATUSMETA", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] parts = line.Split('|');
                            if (parts.Length >= 2) ATMID = parts[1];
                            OnMetadataReceived?.Invoke(ATMID, ParseKeyValueParts(parts, 2));
                        }
                        else if (line.StartsWith("CASHSTATUS", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] parts = line.Split('|');
                            if (parts.Length >= 2) ATMID = parts[1];
                            OnCashStatusReceived?.Invoke(ATMID, ParseKeyValueParts(parts, 2));
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                _isConnected = false;
                OnDisconnected?.Invoke(ATMID);
            }
        }

        private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = startIndex; i < parts.Length; i++)
            {
                int idx = parts[i].IndexOf('=');
                if (idx <= 0) continue;
                string key = parts[i].Substring(0, idx).Trim();
                string value = idx < parts[i].Length - 1 ? parts[i].Substring(idx + 1).Trim() : string.Empty;
                result[key] = value;
            }
            return result;
        }

        public void SendData(byte[] data)
        {
            if (_isConnected && _stream != null)
            {
                _stream.Write(data, 0, data.Length);
                _stream.Flush();
            }
        }

        public void Disconnect()
        {
            _isConnected = false;
            try
            {
                _stream?.Close();
                _tcpClient?.Close();
            }
            catch { }
        }
    }
}
