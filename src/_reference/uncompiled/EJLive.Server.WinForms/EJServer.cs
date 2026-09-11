using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;
using EJLive.Shared.Monitoring;

namespace EJLive.Server.WinForms.Services
{
    public class ClientConnection
        {
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private string _storagePath;
            private bool _isConnected;
            private readonly StringBuilder _lineBuffer = new StringBuilder();
    
            public string ATMID { get; private set; }
            public string ATMType { get; private set; }
            public string RemoteEndPoint { get; private set; }
            public DateTime LastHeartbeatUtc { get; private set; }
            public DateTime LastSyncUtc { get; private set; }
            public bool IsConnected { get { return _isConnected; } }
    
            public event Action<string> OnLogMessage;
            public event Action<IncomingJournalPacket> OnJournalPacketReceived;
            public event Action<ClientConnection> OnDisconnected;
    
            public ClientConnection(TcpClient client, string storagePath)
            {
                _tcpClient = client;
                _storagePath = storagePath;
                _stream = client.GetStream();
                _isConnected = true;
                ATMID = "Unknown";
                ATMType = "Unknown";
                RemoteEndPoint = client.Client.RemoteEndPoint == null ? "" : client.Client.RemoteEndPoint.ToString();
                LastHeartbeatUtc = DateTime.UtcNow;
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
                while (_isConnected)
                {
                    try
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;
    
                        _lineBuffer.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                        ProcessBufferedLines();
                    }
                    catch
                    {
                        break;
                    }
                }
                _isConnected = false;
                OnDisconnected?.Invoke(this);
            }
    
            private void ProcessBufferedLines()
            {
                string full = _lineBuffer.ToString();
                int idx;
                while ((idx = full.IndexOf('\n')) >= 0)
                {
                    string line = full.Substring(0, idx).Trim();
                    full = full.Substring(idx + 1);
                    if (!string.IsNullOrEmpty(line)) ProcessLine(line);
                }
                _lineBuffer.Clear();
                if (full.Length > 0) _lineBuffer.Append(full);
            }
    
            private void ProcessLine(string line)
            {
                string[] parts = Protocol.ParseMessage(line);
                if (parts.Length == 0) return;
    
                switch (parts[0])
                {
                    case Protocol.HANDSHAKE:
                        if (parts.Length >= 2) ATMID = parts[1];
                        if (parts.Length >= 3) ATMType = parts[2];
                        SendLine(Protocol.HANDSHAKE_ACK);
                        OnLogMessage?.Invoke("Client identified: " + ATMID + " (" + ATMType + ")");
                        break;
                    case Protocol.HEARTBEAT:
                        LastHeartbeatUtc = DateTime.UtcNow;
                        SendLine(Protocol.HEARTBEAT_ACK);
                        break;
                    case Protocol.DATA_JOURNAL:
                    case Protocol.DATA_FILE:
                        ProcessJournalOrFile(parts);
                        break;
                    case Protocol.CMD_RESULT:
                        OnLogMessage?.Invoke("Command result from " + ATMID + ": " + line);
                        break;
                    default:
                        OnLogMessage?.Invoke("Message from " + ATMID + ": " + parts[0]);
                        break;
                }
            }
    
            private void ProcessJournalOrFile(string[] parts)
            {
                if (parts.Length < 7)
                {
                    SendLine(Protocol.DATA_NACK + "|INVALID_FORMAT");
                    return;
                }
    
                string atmId = parts[1];
                string fileName = parts[2];
                string declaredSizeText = parts[3];
                string checksum = parts[4];
                string payload = parts[6];
                string syncId = parts.Length > 7 ? parts[7] : Guid.NewGuid().ToString("N");
    
                byte[] data;
                try
                {
                    data = Convert.FromBase64String(payload);
                }
                catch
                {
                    SendLine(Protocol.DATA_NACK + "|" + syncId + "|INVALID_BASE64");
                    return;
                }
    
                long declaredSize;
                if (long.TryParse(declaredSizeText, out declaredSize) && declaredSize != data.Length)
                {
                    SendLine(Protocol.DATA_NACK + "|" + syncId + "|SIZE_MISMATCH");
                    return;
                }
    
                ATMID = string.IsNullOrEmpty(atmId) ? ATMID : atmId;
                LastSyncUtc = DateTime.UtcNow;
                OnJournalPacketReceived?.Invoke(new IncomingJournalPacket
                {
                    SyncId = syncId,
                    ATMId = ATMID,
                    FileName = fileName,
                    Checksum = checksum,
                    Payload = data,
                    SourceConnection = this
                });
            }
    
            public void SendData(byte[] data)
            {
                if (_isConnected && _stream != null)
                    _stream.Write(data, 0, data.Length);
            }
    
            public void SendLine(string text)
            {
                if (!_isConnected || _stream == null) return;
                byte[] data = Encoding.UTF8.GetBytes((text ?? string.Empty) + "\n");
                _stream.Write(data, 0, data.Length);
                _stream.Flush();
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
    public class ClientConnection
        {
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private string _storagePath;
            private bool _isConnected;
    
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; } }
    
            public event Action<string> OnLogMessage;
            public event Action<string, byte[]> OnDataReceived;
    
            public ClientConnection(TcpClient client, string storagePath)
            {
                _tcpClient = client;
                _storagePath = storagePath;
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
                while (_isConnected)
                {
                    try
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;
    
                        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
    
                        if (data.StartsWith("EJLIVE_HANDSHAKE"))
                        {
                            string[] parts = data.Split('|');
                            if (parts.Length >= 2) ATMID = parts[1];
                            OnLogMessage?.Invoke("Client identified: " + ATMID);
                        }
                        else if (data.StartsWith("HEARTBEAT"))
                        {
                            // Heartbeat received, update last seen
                        }
                        else if (data.StartsWith("EJDATA"))
                        {
                            // Parse header and extract data
                            int headerEnd = data.IndexOf('\n');
                            if (headerEnd > 0)
                            {
                                string header = data.Substring(0, headerEnd);
                                string[] parts = header.Split('|');
                                if (parts.Length >= 3)
                                {
                                    string atmId = parts[1];
                                    int dataLen = int.Parse(parts[2]);
                                    byte[] ejData = new byte[bytesRead - headerEnd - 1];
                                    Array.Copy(buffer, headerEnd + 1, ejData, 0, ejData.Length);
                                    OnDataReceived?.Invoke(atmId, ejData);
                                }
                            }
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
                _isConnected = false;
            }
    
            public void SendData(byte[] data)
            {
                if (_isConnected && _stream != null)
                {
                    _stream.Write(data, 0, data.Length);
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
    public partial class ClientConnection
        {
            private TcpClient _tcpClient;
    
    
            private NetworkStream _stream;
    
    
            private Thread _readThread;
    
    
            private string _storagePath;
    
    
            private bool _isConnected;
    
    
            public string ATMID { get; private set; }
    
    
            public string ATMType { get; private set; }
    
    
            public string RemoteEndPoint { get; private set; }
    
    
            public DateTime LastHeartbeatUtc { get; private set; }
    
    
            public DateTime LastSyncUtc { get; private set; }
    
    
            public bool IsConnected { get { return _isConnected; } }
    
    
            public ClientConnection(TcpClient client, string storagePath)
            {
                _tcpClient = client;
                _storagePath = storagePath;
                _stream = client.GetStream();
                _isConnected = true;
                ATMID = "Unknown";
                ATMType = "Unknown";
                RemoteEndPoint = client.Client.RemoteEndPoint == null ? "" : client.Client.RemoteEndPoint.ToString();
                LastHeartbeatUtc = DateTime.UtcNow;
            }
    
    
            private readonly StringBuilder _lineBuffer = new StringBuilder();
    
    
            public void Start()
            {
                _readThread = new Thread(ReadLoop);
                _readThread.IsBackground = true;
                _readThread.Start();
            }
    
    
            private void ReadLoop()
            {
                byte[] buffer = new byte[65536];
                while (_isConnected)
                {
                    try
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;
    
                        _lineBuffer.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                        ProcessBufferedLines();
                    }
                    catch
                    {
                        break;
                    }
                }
                _isConnected = false;
                OnDisconnected?.Invoke(this);
            }
    
    
            private void ProcessBufferedLines()
            {
                string full = _lineBuffer.ToString();
                int idx;
                while ((idx = full.IndexOf('\n')) >= 0)
                {
                    string line = full.Substring(0, idx).Trim();
                    full = full.Substring(idx + 1);
                    if (!string.IsNullOrEmpty(line)) ProcessLine(line);
                }
                _lineBuffer.Clear();
                if (full.Length > 0) _lineBuffer.Append(full);
            }
    
    
            private void ProcessLine(string line)
            {
                string[] parts = Protocol.ParseMessage(line);
                if (parts.Length == 0) return;
    
                switch (parts[0])
                {
                    case Protocol.HANDSHAKE:
                        if (parts.Length >= 2) ATMID = parts[1];
                        if (parts.Length >= 3) ATMType = parts[2];
                        SendLine(Protocol.HANDSHAKE_ACK);
                        OnLogMessage?.Invoke("Client identified: " + ATMID + " (" + ATMType + ")");
                        break;
                    case Protocol.HEARTBEAT:
                        LastHeartbeatUtc = DateTime.UtcNow;
                        SendLine(Protocol.HEARTBEAT_ACK);
                        break;
                    case Protocol.DATA_JOURNAL:
                    case Protocol.DATA_FILE:
                        ProcessJournalOrFile(parts);
                        break;
                    case Protocol.CMD_RESULT:
                        OnLogMessage?.Invoke("Command result from " + ATMID + ": " + line);
                        break;
                    default:
                        OnLogMessage?.Invoke("Message from " + ATMID + ": " + parts[0]);
                        break;
                }
            }
    
    
            private void ProcessJournalOrFile(string[] parts)
            {
                if (parts.Length < 7)
                {
                    SendLine(Protocol.DATA_NACK + "|INVALID_FORMAT");
                    return;
                }
    
                string atmId = parts[1];
                string fileName = parts[2];
                string declaredSizeText = parts[3];
                string checksum = parts[4];
                string payload = parts[6];
                string syncId = parts.Length > 7 ? parts[7] : Guid.NewGuid().ToString("N");
    
                byte[] data;
                try
                {
                    data = Convert.FromBase64String(payload);
                }
                catch
                {
                    SendLine(Protocol.DATA_NACK + "|" + syncId + "|INVALID_BASE64");
                    return;
                }
    
                long declaredSize;
                if (long.TryParse(declaredSizeText, out declaredSize) && declaredSize != data.Length)
                {
                    SendLine(Protocol.DATA_NACK + "|" + syncId + "|SIZE_MISMATCH");
                    return;
                }
    
                ATMID = string.IsNullOrEmpty(atmId) ? ATMID : atmId;
                LastSyncUtc = DateTime.UtcNow;
                OnJournalPacketReceived?.Invoke(new IncomingJournalPacket
                {
                    SyncId = syncId,
                    ATMId = ATMID,
                    FileName = fileName,
                    Checksum = checksum,
                    Payload = data,
                    SourceConnection = this
                });
            }
    
    
            public void SendData(byte[] data)
            {
                if (_isConnected && _stream != null)
                    _stream.Write(data, 0, data.Length);
            }
    
    
            public void SendLine(string text)
            {
                if (!_isConnected || _stream == null) return;
                byte[] data = Encoding.UTF8.GetBytes((text ?? string.Empty) + "\n");
                _stream.Write(data, 0, data.Length);
                _stream.Flush();
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
    
    
            public event Action<string> OnLogMessage;
    
    
            public event Action<IncomingJournalPacket> OnJournalPacketReceived;
    
    
            public event Action<ClientConnection> OnDisconnected;
    
    
        }
    public partial class ClientConnection
        {
            private TcpClient _tcpClient;
    
    
            private NetworkStream _stream;
    
    
            private Thread _readThread;
    
    
            private string _storagePath;
    
    
            private bool _isConnected;
    
    
            public string ATMID { get; private set; }
    
    
            public bool IsConnected { get { return _isConnected; } }
    
    
            public ClientConnection(TcpClient client, string storagePath)
            {
                _tcpClient = client;
                _storagePath = storagePath;
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
                while (_isConnected)
                {
                    try
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;
    
                        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
    
                        if (data.StartsWith("EJLIVE_HANDSHAKE"))
                        {
                            string[] parts = data.Split('|');
                            if (parts.Length >= 2) ATMID = parts[1];
                            OnLogMessage?.Invoke("Client identified: " + ATMID);
                        }
                        else if (data.StartsWith("HEARTBEAT"))
                        {
                            // Heartbeat received, update last seen
                        }
                        else if (data.StartsWith("EJDATA"))
                        {
                            // Parse header and extract data
                            int headerEnd = data.IndexOf('\n');
                            if (headerEnd > 0)
                            {
                                string header = data.Substring(0, headerEnd);
                                string[] parts = header.Split('|');
                                if (parts.Length >= 3)
                                {
                                    string atmId = parts[1];
                                    int dataLen = int.Parse(parts[2]);
                                    byte[] ejData = new byte[bytesRead - headerEnd - 1];
                                    Array.Copy(buffer, headerEnd + 1, ejData, 0, ejData.Length);
                                    OnDataReceived?.Invoke(atmId, ejData);
                                }
                            }
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
                _isConnected = false;
            }
    
    
            public void SendData(byte[] data)
            {
                if (_isConnected && _stream != null)
                {
                    _stream.Write(data, 0, data.Length);
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
    
    
            public event Action<string> OnLogMessage;
    
    
            public event Action<string, byte[]> OnDataReceived;
    
    
        }
    public partial class ClientConnection
        {
            private TcpClient _tcpClient;
    
    
            private NetworkStream _stream;
    
    
            private Thread _readThread;
    
    
            private bool _isConnected;
    
    
            public string RemoteEndpoint
            {
                get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            }
    
    
            public string ATMID { get; private set; }
    
    
            public bool IsConnected { get { return _isConnected; } }
    
    
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
                            else if (line.StartsWith("CMD_RESULT", StringComparison.OrdinalIgnoreCase))
                            {
                                string[] parts = line.Split('|');
                                if (parts.Length >= 5)
                                {
                                    if (parts.Length >= 2) ATMID = parts[1];
                                    OnCommandResultReceived?.Invoke(
                                        ATMID,
                                        parts[2],
                                        parts[3],
                                        parts[4]);
                                }
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
    
    
            public event Action<string> OnLogMessage;
    
    
            public event Action<string, byte[]> OnDataReceived;
    
    
            public event Action<string, string> OnHandshakeReceived;
    
    
            public event Action<string> OnHeartbeatReceived;
    
    
            public event Action<string, Dictionary<string, string>> OnMetadataReceived;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_Enterprise_Updated_20260512_build1\EJLive.Server.WinForms\Services\EJServer.cs
            public event Action<string, Dictionary<string, string>> OnCashStatusReceived;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_Enterprise_Updated_20260512_build1\EJLive.Server.WinForms\Services\EJServer.cs
            public event Action<string, string, string, string> OnCommandResultReceived;
    
    
            public event Action<string> OnDisconnected;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Updated_20260512\EJLive_Enterprise_Updated_20260512_build1\EJLive.Server.WinForms\Services\EJServer.cs
            public event Action<string, Dictionary<string, string>> OnCashStatusReceived;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Updated_20260512\EJLive_Enterprise_Updated_20260512_build1\EJLive.Server.WinForms\Services\EJServer.cs
            public event Action<string, string, string, string> OnCommandResultReceived;
    
    
        }
    public partial class ClientConnection
        {
            private TcpClient _tcpClient;
    
    
            private NetworkStream _stream;
    
    
            private Thread _readThread;
    
    
            private bool _isConnected;
    
    
            public string RemoteEndpoint
            {
                get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            }
    
    
            public string ATMID { get; private set; }
    
    
            public bool IsConnected { get { return _isConnected; } }
    
    
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
    
    
            public event Action<string> OnLogMessage;
    
    
            public event Action<string, byte[]> OnDataReceived;
    
    
            public event Action<string, string> OnHandshakeReceived;
    
    
            public event Action<string> OnHeartbeatReceived;
    
    
            public event Action<string, Dictionary<string, string>> OnMetadataReceived;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Server.WinForms\Services\EJServer.cs
            public event Action<string, Dictionary<string, string>> OnCashStatusReceived;
    
    
            public event Action<string> OnDisconnected;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_operational\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Server.WinForms\Services\EJServer.cs
            public event Action<string, Dictionary<string, string>> OnCashStatusReceived;
    
    
        }
    public partial class ClientConnection
        {
            private TcpClient _tcpClient;
    
    
            private NetworkStream _stream;
    
    
            private Thread _readThread;
    
    
            private bool _isConnected;
    
    
            public string RemoteEndpoint
            {
                get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            }
    
    
            public string ATMID { get; private set; }
    
    
            public bool IsConnected { get { return _isConnected; } }
    
    
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
    
    
            public event Action<string> OnLogMessage;
    
    
            public event Action<string, byte[]> OnDataReceived;
    
    
            public event Action<string, string> OnHandshakeReceived;
    
    
            public event Action<string> OnHeartbeatReceived;
    
    
            public event Action<string, Dictionary<string, string>> OnMetadataReceived;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Server.WinForms\Services\EJServer.cs
            public event Action<string, Dictionary<string, string>> OnCashStatusReceived;
    
    
            public event Action<string> OnDisconnected;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Server.WinForms\Services\EJServer.cs.v21_bak
            public event Action<string, Dictionary<string, string>> OnCashStatusReceived;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Server.WinForms\Services\EJServer.cs.v17_bak
            public event Action<string, Dictionary<string, string>> OnCashStatusReceived;
    
    
        }
    public partial class ClientConnection
        {
            private TcpClient _tcpClient;
    
    
            private NetworkStream _stream;
    
    
            private Thread _readThread;
    
    
            private string _storagePath;
    
    
            private bool _isConnected;
    
    
            public string RemoteEndpoint
            {
                get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            }
    
    
            public string ATMID { get; private set; }
    
    
            public bool IsConnected { get { return _isConnected; } }
    
    
            public ClientConnection(TcpClient client, string storagePath)
            {
                _tcpClient = client;
                _storagePath = storagePath;
                _stream = client.GetStream();
                _isConnected = true;
                ATMID = "Unknown";
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Server.WinForms\Services\EJServer.cs
            public ClientConnection(TcpClient client, string storagePath)
            {
                _tcpClient = client;
                _stream = client.GetStream();
                _isConnected = true;
                ATMID = "Unknown";
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Server.WinForms\Services\EJServer.cs
            public ClientConnection(TcpClient client, string storagePath)
            {
                _tcpClient = client;
                _stream = client.GetStream();
                _isConnected = true;
                ATMID = "Unknown";
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
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
                while (_isConnected)
                {
                    try
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;
    
                        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
    
                        if (data.StartsWith("EJLIVE_HANDSHAKE"))
                        {
                            string[] parts = data.Split('|');
                            if (parts.Length >= 2) ATMID = parts[1];
                            OnLogMessage?.Invoke("Client identified: " + ATMID);
                        }
                        else if (data.StartsWith("HEARTBEAT"))
                        {
                            // Heartbeat received, update last seen
                        }
                        else if (data.StartsWith("EJDATA"))
                        {
                            // Parse header and extract data
                            int headerEnd = data.IndexOf('\n');
                            if (headerEnd > 0)
                            {
                                string header = data.Substring(0, headerEnd);
                                string[] parts = header.Split('|');
                                if (parts.Length >= 3)
                                {
                                    string atmId = parts[1];
                                    int dataLen = int.Parse(parts[2]);
                                    byte[] ejData = new byte[bytesRead - headerEnd - 1];
                                    Array.Copy(buffer, headerEnd + 1, ejData, 0, ejData.Length);
                                    OnDataReceived?.Invoke(atmId, ejData);
                                }
                            }
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
                _isConnected = false;
            }
    
    
            public void SendData(byte[] data)
            {
                if (_isConnected && _stream != null)
                {
                    _stream.Write(data, 0, data.Length);
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Server.WinForms\Services\EJServer.cs
            public void SendData(byte[] data)
            {
                if (_isConnected && _stream != null)
                {
                    _stream.Write(data, 0, data.Length);
                    _stream.Flush();
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Server.WinForms\Services\EJServer.cs
            public void SendData(byte[] data)
            {
                if (_isConnected && _stream != null)
                {
                    _stream.Write(data, 0, data.Length);
                    _stream.Flush();
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
            public void SendData(byte[] data)
            {
                if (_isConnected && _stream != null)
                {
                    _stream.Write(data, 0, data.Length);
                    _stream.Flush();
                }
            }
    
    
            public event Action<string> OnLogMessage;
    
    
            public event Action<string, byte[]> OnDataReceived;
    
    
            public event Action<string, string> OnHandshakeReceived;
    
    
            public event Action<string> OnHeartbeatReceived;
    
    
            public event Action<string, Dictionary<string, string>> OnMetadataReceived;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Server.WinForms\Services\EJServer.cs
            public event Action<string, Dictionary<string, string>> OnCashStatusReceived;
    
    
            public event Action<string> OnDisconnected;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Server.WinForms\Services\EJServer.cs
            public event Action<string, Dictionary<string, string>> OnCashStatusReceived;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
            public event Action<string, Dictionary<string, string>> OnCashStatusReceived;
    
    
        }
    public partial class ClientConnection
        {
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private string _storagePath;
            private bool _isConnected;
            public string RemoteEndpoint
            {
                get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            }
            public string ATMID { get; private set; }
            public string ATMType { get; private set; }
            public string RemoteEndPoint { get; private set; }
            public DateTime LastHeartbeatUtc { get; private set; }
            public DateTime LastSyncUtc { get; private set; }
            public bool IsConnected { get { return _isConnected; } }
            public ClientConnection(TcpClient client, string storagePath)
            {
                _tcpClient = client;
                _storagePath = storagePath;
                _stream = client.GetStream();
                _isConnected = true;
                ATMID = "Unknown";
                ATMType = "Unknown";
                RemoteEndPoint = client.Client.RemoteEndPoint == null ? "" : client.Client.RemoteEndPoint.ToString();
                LastHeartbeatUtc = DateTime.UtcNow;
            }
            public ClientConnection(TcpClient client, string storagePath)
            {
                _tcpClient = client;
                _storagePath = storagePath;
                _stream = client.GetStream();
                _isConnected = true;
                ATMID = "Unknown";
            }
            private readonly StringBuilder _lineBuffer = new StringBuilder();
            public void Start()
            {
                _readThread = new Thread(ReadLoop);
                _readThread.IsBackground = true;
                _readThread.Start();
            }
            private void ReadLoop()
            {
                byte[] buffer = new byte[65536];
                while (_isConnected)
                {
                    try
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;
                        _lineBuffer.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                        ProcessBufferedLines();
                    }
                    catch
                    {
                        break;
                    }
                }
                _isConnected = false;
                OnDisconnected?.Invoke(this);
            }
            private void ProcessBufferedLines()
            {
                string full = _lineBuffer.ToString();
                int idx;
                while ((idx = full.IndexOf('\n')) >= 0)
                {
                    string line = full.Substring(0, idx).Trim();
                    full = full.Substring(idx + 1);
                    if (!string.IsNullOrEmpty(line)) ProcessLine(line);
                }
                _lineBuffer.Clear();
                if (full.Length > 0) _lineBuffer.Append(full);
            }
            private void ProcessLine(string line)
            {
                string[] parts = Protocol.ParseMessage(line);
                if (parts.Length == 0) return;
                switch (parts[0])
                {
                    case Protocol.HANDSHAKE:
                        if (parts.Length >= 2) ATMID = parts[1];
                        if (parts.Length >= 3) ATMType = parts[2];
                        SendLine(Protocol.HANDSHAKE_ACK);
                        OnLogMessage?.Invoke("Client identified: " + ATMID + " (" + ATMType + ")");
                        break;
                    case Protocol.HEARTBEAT:
                        LastHeartbeatUtc = DateTime.UtcNow;
                        SendLine(Protocol.HEARTBEAT_ACK);
                        break;
                    case Protocol.DATA_JOURNAL:
                    case Protocol.DATA_FILE:
                        ProcessJournalOrFile(parts);
                        break;
                    case Protocol.CMD_RESULT:
                        OnLogMessage?.Invoke("Command result from " + ATMID + ": " + line);
                        break;
                    default:
                        OnLogMessage?.Invoke("Message from " + ATMID + ": " + parts[0]);
                        break;
                }
            }
            private void ProcessJournalOrFile(string[] parts)
            {
                if (parts.Length < 7)
                {
                    SendLine(Protocol.DATA_NACK + "|INVALID_FORMAT");
                    return;
                }
                string atmId = parts[1];
                string fileName = parts[2];
                string declaredSizeText = parts[3];
                string checksum = parts[4];
                string payload = parts[6];
                string syncId = parts.Length > 7 ? parts[7] : Guid.NewGuid().ToString("N");
                byte[] data;
                try
                {
                    data = Convert.FromBase64String(payload);
                }
                catch
                {
                    SendLine(Protocol.DATA_NACK + "|" + syncId + "|INVALID_BASE64");
                    return;
                }
                long declaredSize;
                if (long.TryParse(declaredSizeText, out declaredSize) && declaredSize != data.Length)
                {
                    SendLine(Protocol.DATA_NACK + "|" + syncId + "|SIZE_MISMATCH");
                    return;
                }
                ATMID = string.IsNullOrEmpty(atmId) ? ATMID : atmId;
                LastSyncUtc = DateTime.UtcNow;
                OnJournalPacketReceived?.Invoke(new IncomingJournalPacket
                {
                    SyncId = syncId,
                    ATMId = ATMID,
                    FileName = fileName,
                    Checksum = checksum,
                    Payload = data,
                    SourceConnection = this
                });
            }
            public void SendData(byte[] data)
            {
                if (_isConnected && _stream != null)
                    _stream.Write(data, 0, data.Length);
            }
            public void SendLine(string text)
            {
                if (!_isConnected || _stream == null) return;
                byte[] data = Encoding.UTF8.GetBytes((text ?? string.Empty) + "\n");
                _stream.Write(data, 0, data.Length);
                _stream.Flush();
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
            private void ReadLoop()
            {
                byte[] buffer = new byte[65536];
                while (_isConnected)
                {
                    try
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;
                        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        if (data.StartsWith("EJLIVE_HANDSHAKE"))
                        {
                            string[] parts = data.Split('|');
                            if (parts.Length >= 2) ATMID = parts[1];
                            OnLogMessage?.Invoke("Client identified: " + ATMID);
                        }
                        else if (data.StartsWith("HEARTBEAT"))
                        {
                            // Heartbeat received, update last seen
                        }
                        else if (data.StartsWith("EJDATA"))
                        {
                            // Parse header and extract data
                            int headerEnd = data.IndexOf('\n');
                            if (headerEnd > 0)
                            {
                                string header = data.Substring(0, headerEnd);
                                string[] parts = header.Split('|');
                                if (parts.Length >= 3)
                                {
                                    string atmId = parts[1];
                                    int dataLen = int.Parse(parts[2]);
                                    byte[] ejData = new byte[bytesRead - headerEnd - 1];
                                    Array.Copy(buffer, headerEnd + 1, ejData, 0, ejData.Length);
                                    OnDataReceived?.Invoke(atmId, ejData);
                                }
                            }
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
                _isConnected = false;
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
                }
            }
            public event Action<string> OnLogMessage;
            public event Action<IncomingJournalPacket> OnJournalPacketReceived;
            public event Action<ClientConnection> OnDisconnected;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string, Dictionary<string, string>> OnMetadataReceived;
            public event Action<string, Dictionary<string, string>> OnCashStatusReceived;
            public event Action<string> OnDisconnected;
            public event Action<string, string, string, string> OnCommandResultReceived;
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
            public event Action<string, string, string, string> OnCommandResultReceived;
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
                            else if (line.StartsWith("CMD_RESULT", StringComparison.OrdinalIgnoreCase))
                            {
                                string[] parts = line.Split('|');
                                if (parts.Length >= 5)
                                {
                                    if (parts.Length >= 2) ATMID = parts[1];
                                    OnCommandResultReceived?.Invoke(
                                        ATMID,
                                        parts[2],
                                        parts[3],
                                        parts[4]);
                                }
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
    public class ClientConnectionInfo
        {
            public string ATMID { get; set; }
            public string RemoteEndPoint { get; set; }
            public string ATMType { get; set; }
            public bool IsConnected { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastSyncUtc { get; set; }
        }
    public partial class ClientConnectionInfo
        {
            public string ATMID { get; set; }
    
    
            public string RemoteEndPoint { get; set; }
    
    
            public string ATMType { get; set; }
    
    
            public bool IsConnected { get; set; }
    
    
            public DateTime LastHeartbeatUtc { get; set; }
    
    
            public DateTime LastSyncUtc { get; set; }
    
    
        }
    public partial class EJServerService : IDisposable
        {
            private readonly ServerEngine    _serverEngine;
    
    
            private readonly ArchiveManager  _archiveManager;
    
    
            private readonly ImageSyncEngine _imageSync;
    
    
            private readonly JournalSyncTracker _syncTracker;
    
    
            private bool _isRunning;
    
    
            private readonly int _port;
    
    
            public int    ConnectedATMs        => _serverEngine.ConnectedCount;
    
    
            public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
    
    
            public int    TotalJournalsArchived { get; private set; }
    
    
            public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
    
    
            public bool   IsRunning            => _isRunning;
    
    
            public DateTime StartedAt          => _serverEngine.StartedAt;
    
    
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
    
    
            public IEnumerable<ATMInfo>     GetConnectedATMs()   => _serverEngine.GetConnectedATMs();
    
    
            public ATMInfo                  GetATM(string atmId) => _serverEngine.GetATMByID(atmId);
    
    
            public (long files, long bytes) GetArchiveStats(string atmId)
                => _archiveManager.GetATMArchiveStats(atmId);
    
    
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
    
    
            public event EventHandler<ATMInfo>      OnATMConnected;
    
    
            public event EventHandler<ATMInfo>      OnATMDisconnected;
    
    
            public event EventHandler<ATMInfo>      OnATMUpdated;
    
    
            public event EventHandler<AlertPayload> OnAlert;
    
    
            public event EventHandler<RemoteCommand> OnCommandChanged;
    
    
            public event EventHandler<string>       OnLog;
    
    
        }
    public partial class EJServerService : IDisposable
        {
            private readonly ServerEngine    _serverEngine;
    
    
            private readonly ArchiveManager  _archiveManager;
    
    
            private readonly ImageSyncEngine _imageSync;
    
    
            private readonly JournalSyncTracker _syncTracker;
    
    
            private bool _isRunning;
    
    
            private readonly int _port;
    
    
            public int    ConnectedATMs        => _serverEngine.ConnectedCount;
    
    
            public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
    
    
            public int    TotalJournalsArchived { get; private set; }
    
    
            public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
    
    
            public bool   IsRunning            => _isRunning;
    
    
            public DateTime StartedAt          => _serverEngine.StartedAt;
    
    
            public EJServerService(int port = AppConstants.DefaultPort, string? archivePath = null)
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
    
                _archiveManager.OnArchived += (s, p) => Log($"Archived: {p}");
                _archiveManager.OnError    += (s, e) => Log($"Archive Error: {e}");
    
                AlertManager.Instance.OnAlert    += (s, a) => OnAlert?.Invoke(this, a);
                AlertManager.Instance.OnCritical += (s, a) => OnAlert?.Invoke(this, a);
            }
    
    
            public void Start()
            {
                if (_isRunning) return;
                _serverEngine.Start();
                _isRunning = true;
                DatabaseManager.Instance.InsertAuditLog("ServerStart", "System", null, $"EJLive Server started on TCP/{_port}");
                Log($"EJLive Enterprise Server v{AppConstants.AppVersion} started on TCP/{_port}");
            }
    
    
            public void Stop()
            {
                if (!_isRunning) return;
                _serverEngine.Stop();
                _isRunning = false;
                DatabaseManager.Instance.InsertAuditLog("ServerStop", "System", null, "EJLive Server stopped");
                Log("EJLive Server stopped.");
            }
    
    
            private void OnJournalReceived(object sender, ReceivedPacket pkt)
            {
                if (pkt.IsGhostFrame || pkt.Data == null || pkt.Data.Length == 0) return;
    
                var thread = new Thread(() =>
                {
                    try
                    {
                        if (DatabaseManager.Instance.IsDuplicateSync(pkt.ATM.ATM_ID, pkt.FileName, pkt.Checksum))
                        {
                            Log($"Idempotency: duplicate skipped {pkt.FileName} from {pkt.ATM.ATM_ID}");
                            return;
                        }
    
                        var archivePath = _archiveManager.Archive(pkt.ATM.ATM_ID, pkt.FileName, pkt.Data, pkt.Checksum, pkt.SHA256);
                        if (archivePath != null) TotalJournalsArchived++;
    
                        var syncId = _syncTracker.AddOrGet(pkt.ATM.ATM_ID, pkt.FileName, pkt.Data.Length, 0, pkt.Checksum)?.SyncId;
                        if (!string.IsNullOrEmpty(syncId))
                        {
                            _syncTracker.MarkCompleted(pkt.ATM.ATM_ID, syncId, pkt.SHA256);
                        }
    
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
                return SendCommand(atmId, AppConstants.CMD_SYNC_IMAGES, imagesFolder, "System");
            }
    
    
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
    
    
            public IEnumerable<ATMInfo>     GetConnectedATMs()   => _serverEngine.GetConnectedATMs();
    
    
            public ATMInfo?                 GetATM(string atmId) => _serverEngine.GetATMByID(atmId);
    
    
            public (long files, long bytes) GetArchiveStats(string atmId)
                => _archiveManager.GetATMArchiveStats(atmId);
    
    
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
    
    
            public event EventHandler<ATMInfo>?      OnATMConnected;
    
    
            public event EventHandler<ATMInfo>?      OnATMDisconnected;
    
    
            public event EventHandler<ATMInfo>?      OnATMUpdated;
    
    
            public event EventHandler<AlertPayload>? OnAlert;
    
    
            public event EventHandler<RemoteCommand>? OnCommandChanged;
    
    
            public event EventHandler<string>?       OnLog;
    
    
        }
    public partial class EJServerService : IDisposable
        {
            private readonly ServerEngine    _serverEngine;
    
    
            private readonly ArchiveManager  _archiveManager;
    
    
            private readonly ImageSyncEngine _imageSync;
    
    
            private readonly JournalSyncTracker _syncTracker;
    
    
            private bool _isRunning;
    
    
            private readonly int _port;
    
    
            public int    ConnectedATMs        => _serverEngine.ConnectedCount;
    
    
            public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
    
    
            public int    TotalJournalsArchived { get; private set; }
    
    
            public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
    
    
            public bool   IsRunning            => _isRunning;
    
    
            public DateTime StartedAt          => _serverEngine.StartedAt;
    
    
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
    
                _archiveManager.OnArchived += (s, p) => Log($"📦 Archived: {p}");
                _archiveManager.OnError    += (s, e) => Log($"Archive Error: {e}");
    
                AlertManager.Instance.OnAlert    += (s, a) => OnAlert?.Invoke(this, a);
                AlertManager.Instance.OnCritical += (s, a) => OnAlert?.Invoke(this, a);
            }
    
    
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
    
    
            public bool SendCommand(string atmId, string cmdType, string parameters, string sentBy = "System")
                => _serverEngine.SendCommand(atmId, cmdType, parameters, sentBy);
    
    
            public void Broadcast(string message, string sentBy = "System")
                => _serverEngine.Broadcast(message, sentBy);
    
    
            public bool SendImagesToATM(string atmId, string imagesFolder)
            {
                // إرسال الصور عبر CMD_SYNC_IMAGES
                return SendCommand(atmId, AppConstants.CMD_SYNC_IMAGES, imagesFolder, "System");
            }
    
    
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
    
    
            public IEnumerable<ATMInfo>     GetConnectedATMs()   => _serverEngine.GetConnectedATMs();
    
    
            public ATMInfo                  GetATM(string atmId) => _serverEngine.GetATMByID(atmId);
    
    
            public (long files, long bytes) GetArchiveStats(string atmId)
                => _archiveManager.GetATMArchiveStats(atmId);
    
    
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
    
    
            public event EventHandler<ATMInfo>      OnATMConnected;
    
    
            public event EventHandler<ATMInfo>      OnATMDisconnected;
    
    
            public event EventHandler<ATMInfo>      OnATMUpdated;
    
    
            public event EventHandler<AlertPayload> OnAlert;
    
    
            public event EventHandler<string>       OnLog;
    
    
        }
    public partial class EJServerService : IDisposable
        {
            private readonly ServerEngine    _serverEngine;
            private readonly ArchiveManager  _archiveManager;
            private readonly ImageSyncEngine _imageSync;
            private readonly JournalSyncTracker _syncTracker;
            private bool _isRunning;
            private readonly int _port;
            public int    ConnectedATMs        => _serverEngine.ConnectedCount;
            public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
            public int    TotalJournalsArchived { get; private set; }
            public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
            public bool   IsRunning            => _isRunning;
            public DateTime StartedAt          => _serverEngine.StartedAt;
            public EJServerService(int port = AppConstants.DefaultPort, string? archivePath = null)
            {
                _port           = port;
                _serverEngine   = new ServerEngine(port);
                _archiveManager = new ArchiveManager(archivePath);
                _imageSync      = new ImageSyncEngine();
                _syncTracker    = new JournalSyncTracker();
                WireEvents();
            }
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
                _archiveManager.OnArchived += (s, p) => Log($"Archived: {p}");
                _archiveManager.OnError    += (s, e) => Log($"Archive Error: {e}");
                AlertManager.Instance.OnAlert    += (s, a) => OnAlert?.Invoke(this, a);
                AlertManager.Instance.OnCritical += (s, a) => OnAlert?.Invoke(this, a);
            }
            public void Start()
            {
                if (_isRunning) return;
                _serverEngine.Start();
                _isRunning = true;
                DatabaseManager.Instance.InsertAuditLog("ServerStart", "System", null, $"EJLive Server started on TCP/{_port}");
                Log($"EJLive Enterprise Server v{AppConstants.AppVersion} started on TCP/{_port}");
            }
            public void Stop()
            {
                if (!_isRunning) return;
                _serverEngine.Stop();
                _isRunning = false;
                DatabaseManager.Instance.InsertAuditLog("ServerStop", "System", null, "EJLive Server stopped");
                Log("EJLive Server stopped.");
            }
            private void OnJournalReceived(object sender, ReceivedPacket pkt)
            {
                if (pkt.IsGhostFrame || pkt.Data == null || pkt.Data.Length == 0) return;
                var thread = new Thread(() =>
                {
                    try
                    {
                        if (DatabaseManager.Instance.IsDuplicateSync(pkt.ATM.ATM_ID, pkt.FileName, pkt.Checksum))
                        {
                            Log($"Idempotency: duplicate skipped {pkt.FileName} from {pkt.ATM.ATM_ID}");
                            return;
                        }
                        var archivePath = _archiveManager.Archive(pkt.ATM.ATM_ID, pkt.FileName, pkt.Data, pkt.Checksum, pkt.SHA256);
                        if (archivePath != null) TotalJournalsArchived++;
                        var syncId = _syncTracker.AddOrGet(pkt.ATM.ATM_ID, pkt.FileName, pkt.Data.Length, 0, pkt.Checksum)?.SyncId;
                        if (!string.IsNullOrEmpty(syncId))
                        {
                            _syncTracker.MarkCompleted(pkt.ATM.ATM_ID, syncId, pkt.SHA256);
                        }
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
                return SendCommand(atmId, AppConstants.CMD_SYNC_IMAGES, imagesFolder, "System");
            }
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
            public IEnumerable<ATMInfo>     GetConnectedATMs()   => _serverEngine.GetConnectedATMs();
            public ATMInfo?                 GetATM(string atmId) => _serverEngine.GetATMByID(atmId);
            public (long files, long bytes) GetArchiveStats(string atmId)
                => _archiveManager.GetATMArchiveStats(atmId);
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
            public void Start()
            {
                if (_isRunning) return;
                _serverEngine.Start();
                _isRunning = true;
                DatabaseManager.Instance.InsertAuditLog("ServerStart", "System", null, $"EJLive Server started on TCP/{_port}");
                Log($"★ EJLive Enterprise Server v{AppConstants.AppVersion} started on TCP/{_port}");
            }
            public ATMInfo                  GetATM(string atmId) => _serverEngine.GetATMByID(atmId);
            public event EventHandler<ATMInfo>?      OnATMConnected;
            public event EventHandler<ATMInfo>?      OnATMDisconnected;
            public event EventHandler<ATMInfo>?      OnATMUpdated;
            public event EventHandler<AlertPayload>? OnAlert;
            public event EventHandler<RemoteCommand>? OnCommandChanged;
            public event EventHandler<string>?       OnLog;
            public event EventHandler<ATMInfo>      OnATMConnected;
            public event EventHandler<ATMInfo>      OnATMDisconnected;
            public event EventHandler<ATMInfo>      OnATMUpdated;
            public event EventHandler<AlertPayload> OnAlert;
            public event EventHandler<RemoteCommand> OnCommandChanged;
            public event EventHandler<string>       OnLog;
        }
    public partial public public class EJServerService : IDisposable
        {
            private readonly ServerEngine    _serverEngine;
            private readonly ArchiveManager  _archiveManager;
            private readonly ImageSyncEngine _imageSync;
            private readonly JournalSyncTracker _syncTracker;
            private bool _isRunning;
            private readonly int _port;
            public int    ConnectedATMs        => _serverEngine.ConnectedCount;
            public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
            public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
            public bool   IsRunning            => _isRunning;
            public DateTime StartedAt          => _serverEngine.StartedAt;
            private readonly object _clientsSync = new object();
            private TcpListener _listener;
            private string _storagePath;
            private List<ClientConnection> _clients;
            private Thread _listenThread;
            private MonitoringStateStore _monitoringStore;
            private JournalSyncService _journalSyncService;
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private bool _isConnected;
            public EJServerService(int port = AppConstants.DefaultPort, string archivePath = null)
            {
            public int    TotalJournalsArchived { get; private set; }
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; }
            public string RemoteEndpoint
            {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            private void WireEvents()
            {
            public void Start()
            {
            public void Stop()
            {
            private void OnJournalReceived(object sender, ReceivedPacket pkt)
            {
            public bool SendCommand(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public RemoteCommand SendCommandDetailed(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public void Broadcast(string message, string sentBy = "System")
            =>
            public IEnumerable<RemoteCommand> GetRecentCommands()
            =>
            public bool SendImagesToATM(string atmId, string imagesFolder)
            {
            private void CheckHealth(ATMInfo atm)
            {
            public void CheckAllATMHealth()
            {
            public IEnumerable<ATMInfo>     GetConnectedATMs()   =>
            public ATMInfo                  GetATM(string atmId) =>
            private void Log(string msg)
            {
            public void Dispose()
            {
            private void ListenLoop()
            {
            private void HandleHandshakeReceived(string atmId, string remoteEndpoint)
            {
            private void HandleHeartbeatReceived(string atmId)
            {
            private void HandleMetadataReceived(string atmId, Dictionary<string, string> metadata)
            {
            private void HandleCashStatusReceived(string atmId, Dictionary<string, string> payload)
            {
            private void HandleClientDisconnected(string atmId)
            {
            private void HandleDataReceived(string atmId, byte[] encryptedData)
            {
            private byte[] DecryptAES256(byte[] data)
            {
            private byte[] DecompressData(byte[] data)
            {
            public void SendCommand(string atmId, string command, string parameters)
            {
            private string ComputeChecksum(byte[] data)
            {
            private static int ParseInt(Dictionary<string, string> values, string key)
            {
            private static string GetValue(Dictionary<string, string> values, string key, string fallback)
            {
            private void Log(string message)
            {
            private void ReadLoop()
            {
            private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
            {
            public void SendData(byte[] data)
            {
            public void Disconnect()
            {
            public event EventHandler<ATMInfo>      OnATMConnected;
            public event EventHandler<ATMInfo>      OnATMDisconnected;
            public event EventHandler<ATMInfo>      OnATMUpdated;
            public event EventHandler<AlertPayload> OnAlert;
            public event EventHandler<RemoteCommand> OnCommandChanged;
            public event EventHandler<string>       OnLog;
            public event Action<string> OnLogMessage;
            public event Action<string, bool> OnClientStatusChanged;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string> OnDisconnected;
        }
    
        public partial public public class EJServer
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
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private bool _isConnected;
            public EJServer(int port, string storagePath)
            {
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; }
            public string RemoteEndpoint
            {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            public void Start()
            {
            public void Stop()
            {
            private void ListenLoop()
            {
            private void HandleHandshakeReceived(string atmId, string remoteEndpoint)
            {
            private void HandleHeartbeatReceived(string atmId)
            {
            private void HandleMetadataReceived(string atmId, Dictionary<string, string> metadata)
            {
            private void HandleCashStatusReceived(string atmId, Dictionary<string, string> payload)
            {
            private void HandleClientDisconnected(string atmId)
            {
            private void HandleDataReceived(string atmId, byte[] encryptedData)
            {
            private byte[] DecryptAES256(byte[] data)
            {
            private byte[] DecompressData(byte[] data)
            {
            public void SendCommand(string atmId, string command, string parameters)
            {
            private string ComputeChecksum(byte[] data)
            {
            private static int ParseInt(Dictionary<string, string> values, string key)
            {
            private static string GetValue(Dictionary<string, string> values, string key, string fallback)
            {
            private void Log(string message)
            {
            private void ReadLoop()
            {
            private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
            {
            public void SendData(byte[] data)
            {
            public void Disconnect()
            {
            public event Action<string> OnLogMessage;
            public event Action<string, bool> OnClientStatusChanged;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string> OnDisconnected;
        }
    
        public partial public public class ClientConnection
        {
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private bool _isConnected;
            public ClientConnection(TcpClient client, string storagePath)
            {
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; }
            public string RemoteEndpoint
            {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            public void Start()
            {
            private void ReadLoop()
            {
            private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
            {
            public void SendData(byte[] data)
            {
            public void Disconnect()
            {
            public event Action<string> OnLogMessage;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string> OnDisconnected;
        }
    
    }
    public partial public public sealed class EJServerService : IDisposable
        {
            private readonly ServerEngine    _serverEngine;
            private readonly ArchiveManager  _archiveManager;
            private readonly ImageSyncEngine _imageSync;
            private readonly JournalSyncTracker _syncTracker;
            private bool _isRunning;
            private readonly int _port;
            public int    ConnectedATMs        => _serverEngine.ConnectedCount;
            public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
            public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
            public bool   IsRunning            => _isRunning;
            public DateTime StartedAt          => _serverEngine.StartedAt;
            private readonly object _clientsSync = new object();
            private TcpListener _listener;
            private string _storagePath;
            private List<ClientConnection> _clients;
            private Thread _listenThread;
            private MonitoringStateStore _monitoringStore;
            private JournalSyncService _journalSyncService;
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private bool _isConnected;
            public EJServerService(int port = AppConstants.DefaultPort, string archivePath = null)
            {
            public int    TotalJournalsArchived { get; private set; }
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; }
            public string RemoteEndpoint
            {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            private void WireEvents()
            {
            public void Start()
            {
            public void Stop()
            {
            private void OnJournalReceived(object sender, ReceivedPacket pkt)
            {
            public bool SendCommand(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public RemoteCommand SendCommandDetailed(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public void Broadcast(string message, string sentBy = "System")
            =>
            public IEnumerable<RemoteCommand> GetRecentCommands()
            =>
            public bool SendImagesToATM(string atmId, string imagesFolder)
            {
            private void CheckHealth(ATMInfo atm)
            {
            public void CheckAllATMHealth()
            {
            public IEnumerable<ATMInfo>     GetConnectedATMs()   =>
            public ATMInfo                  GetATM(string atmId) =>
            private void Log(string msg)
            {
            public void Dispose()
            {
            private void ListenLoop()
            {
            private void HandleHandshakeReceived(string atmId, string remoteEndpoint)
            {
            private void HandleHeartbeatReceived(string atmId)
            {
            private void HandleMetadataReceived(string atmId, Dictionary<string, string> metadata)
            {
            private void HandleCashStatusReceived(string atmId, Dictionary<string, string> payload)
            {
            private void HandleClientDisconnected(string atmId)
            {
            private void HandleDataReceived(string atmId, byte[] encryptedData)
            {
            private byte[] DecryptAES256(byte[] data)
            {
            private byte[] DecompressData(byte[] data)
            {
            public void SendCommand(string atmId, string command, string parameters)
            {
            private string ComputeChecksum(byte[] data)
            {
            private static int ParseInt(Dictionary<string, string> values, string key)
            {
            private static string GetValue(Dictionary<string, string> values, string key, string fallback)
            {
            private void Log(string message)
            {
            private void ReadLoop()
            {
            private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
            {
            public void SendData(byte[] data)
            {
            public void Disconnect()
            {
            public event EventHandler<ATMInfo>      OnATMConnected;
            public event EventHandler<ATMInfo>      OnATMDisconnected;
            public event EventHandler<ATMInfo>      OnATMUpdated;
            public event EventHandler<AlertPayload> OnAlert;
            public event EventHandler<RemoteCommand> OnCommandChanged;
            public event EventHandler<string>       OnLog;
            public event Action<string> OnLogMessage;
            public event Action<string, bool> OnClientStatusChanged;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string> OnDisconnected;
        }
    
        public partial public public class EJServer
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
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private bool _isConnected;
            public EJServer(int port, string storagePath)
            {
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; }
            public string RemoteEndpoint
            {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            public void Start()
            {
            public void Stop()
            {
            private void ListenLoop()
            {
            private void HandleHandshakeReceived(string atmId, string remoteEndpoint)
            {
            private void HandleHeartbeatReceived(string atmId)
            {
            private void HandleMetadataReceived(string atmId, Dictionary<string, string> metadata)
            {
            private void HandleCashStatusReceived(string atmId, Dictionary<string, string> payload)
            {
            private void HandleClientDisconnected(string atmId)
            {
            private void HandleDataReceived(string atmId, byte[] encryptedData)
            {
            private byte[] DecryptAES256(byte[] data)
            {
            private byte[] DecompressData(byte[] data)
            {
            public void SendCommand(string atmId, string command, string parameters)
            {
            private string ComputeChecksum(byte[] data)
            {
            private static int ParseInt(Dictionary<string, string> values, string key)
            {
            private static string GetValue(Dictionary<string, string> values, string key, string fallback)
            {
            private void Log(string message)
            {
            private void ReadLoop()
            {
            private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
            {
            public void SendData(byte[] data)
            {
            public void Disconnect()
            {
            public event Action<string> OnLogMessage;
            public event Action<string, bool> OnClientStatusChanged;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string> OnDisconnected;
        }
    
        public partial public public class ClientConnection
        {
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private bool _isConnected;
            public ClientConnection(TcpClient client, string storagePath)
            {
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; }
            public string RemoteEndpoint
            {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            public void Start()
            {
            private void ReadLoop()
            {
            private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
            {
            public void SendData(byte[] data)
            {
            public void Disconnect()
            {
            public event Action<string> OnLogMessage;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string> OnDisconnected;
        }
    
    }
    public partial public sealed class EJServerService : IDisposable
        {
            private readonly ServerEngine    _serverEngine;
            private readonly ArchiveManager  _archiveManager;
            private readonly ImageSyncEngine _imageSync;
            private readonly JournalSyncTracker _syncTracker;
            private bool _isRunning;
            private readonly int _port;
            public int    ConnectedATMs        => _serverEngine.ConnectedCount;
            public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
            public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
            public bool   IsRunning            => _isRunning;
            public DateTime StartedAt          => _serverEngine.StartedAt;
            private TcpListener _listener;
            private string _storagePath;
            private List<ClientConnection> _clients;
            private Thread _listenThread;
            private MonitoringStateStore _monitoringStore;
            private JournalSyncService _journalSyncService;
            private Dictionary<string, ATMInfo> _activeATMs = new Dictionary<string, ATMInfo>();
            private readonly object _clientsSync = new object();
            private List<ATMInfo> _connectedATMs = new List<ATMInfo>();
            private readonly ConcurrentQueue<IncomingJournalPacket> _journalQueue;
            private readonly AutoResetEvent _journalSignal;
            private readonly List<Thread> _workerThreads;
            private readonly Dictionary<string, string> _storedSyncIndex;
            private bool _workersRunning;
            private string _journalIndexPath;
            private readonly object _clientsLock = new object();
            private readonly object _journalIndexLock = new object();
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private bool _isConnected;
            private readonly StringBuilder _lineBuffer = new StringBuilder();
            public EJServerService(int port = AppConstants.DefaultPort, string archivePath = null)
            {
            public EJServerService(int port = AppConstants.DefaultPort, string? archivePath = null)
            {
            public int    TotalJournalsArchived { get; private set; }
            public string RemoteEndpoint
            {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; }
            public string ATMType { get; private set; }
            public string RemoteEndPoint { get; private set; }
            public DateTime LastHeartbeatUtc { get; private set; }
            public DateTime LastSyncUtc { get; private set; }
            public string SyncId { get; set; }
            public string ATMId { get; set; }
            public string FileName { get; set; }
            public string Checksum { get; set; }
            public ClientConnection SourceConnection { get; set; }
            private void WireEvents()
            {
            public void Start()
            {
            public void Stop()
            {
            private void OnJournalReceived(object sender, ReceivedPacket pkt)
            {
            public bool SendCommand(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public RemoteCommand SendCommandDetailed(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public void Broadcast(string message, string sentBy = "System")
            =>
            public IEnumerable<RemoteCommand> GetRecentCommands()
            =>
            public bool SendImagesToATM(string atmId, string imagesFolder)
            {
            private void CheckHealth(ATMInfo atm)
            {
            public void CheckAllATMHealth()
            {
            public IEnumerable<ATMInfo>     GetConnectedATMs()   =>
            public ATMInfo                  GetATM(string atmId) =>
            private void Log(string msg)
            {
            public void Dispose()
            {
            public ATMInfo?                 GetATM(string atmId) =>
            private void ListenLoop()
            {
            private void HandleDataReceived(string atmId, byte[] encryptedData)
            {
            private byte[] DecryptAES256(byte[] data)
            {
            private byte[] DecompressData(byte[] data)
            {
            public void SendCommand(string atmId, string command, string parameters)
            {
            private void Log(string message)
            {
            public void Start(int port)
            {
            private void HandleClient(TcpClient client)
            {
            private void HandleHandshakeReceived(string atmId, string remoteEndpoint)
            {
            private void HandleHeartbeatReceived(string atmId)
            {
            private void HandleMetadataReceived(string atmId, Dictionary<string, string> metadata)
            {
            private void HandleCashStatusReceived(string atmId, Dictionary<string, string> payload)
            {
            private void HandleClientDisconnected(string atmId)
            {
            private string ComputeChecksum(byte[] data)
            {
            private static int ParseInt(Dictionary<string, string> values, string key)
            {
            private static string GetValue(Dictionary<string, string> values, string key, string fallback)
            {
            private void ListenForClients()
            {
            private void EnqueueJournalPacket(IncomingJournalPacket packet)
            {
            private void StoreJournalPacket(IncomingJournalPacket packet)
            {
            public List<ClientConnectionInfo> GetClients()
            {
            private void StartWorkers()
            {
            private void StopWorkers()
            {
            private void JournalWorkerLoop()
            {
            private void HandleClientDisconnected(ClientConnection connection)
            {
            private static string SanitizeFileName(string fileName)
            {
            private static string ComputeMD5(byte[] data)
            {
            private void LoadJournalIndex()
            {
            private bool TryGetStoredJournal(string syncId, out string serverPath)
            {
            private void RememberStoredJournal(string syncId, string serverPath)
            {
            private static string BuildStoredJournalFileName(string fileName, string syncId)
            {
            private static string MapCommand(string command)
            {
            private void HandleCommandResultReceived(string atmId, string commandId, string status, string details)
            {
            private void ReadLoop()
            {
            public void SendData(byte[] data)
            {
            public void Disconnect()
            {
            private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
            {
            private void ProcessBufferedLines()
            {
            private void ProcessLine(string line)
            {
            private void ProcessJournalOrFile(string[] parts)
            {
            public void SendLine(string text)
            {
            public event EventHandler<ATMInfo>      OnATMConnected;
            public event EventHandler<ATMInfo>      OnATMDisconnected;
            public event EventHandler<ATMInfo>      OnATMUpdated;
            public event EventHandler<AlertPayload> OnAlert;
            public event EventHandler<RemoteCommand> OnCommandChanged;
            public event EventHandler<string>       OnLog;
            public event Action<string> OnLogMessage;
            public event Action<string, bool> OnClientStatusChanged;
            public event Action<ATMInfo> OnATMStatusChanged;
            public event Action<string, string, string> OnDataReceived;
            public event Action<string, string> OnLogReceived;
            public event Action<JournalSyncRecord> OnJournalSyncChanged;
            public event Action<string, string, string> OnJournalStored;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string> OnDisconnected;
            public event Action<IncomingJournalPacket> OnJournalPacketReceived;
            public event Action<ClientConnection> OnDisconnected;
            public event Action<string, string, string, string> OnCommandResultReceived;
        }
    
        public partial public class EJServer
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
            private Dictionary<string, ATMInfo> _activeATMs = new Dictionary<string, ATMInfo>();
            private List<ATMInfo> _connectedATMs = new List<ATMInfo>();
            private readonly ConcurrentQueue<IncomingJournalPacket> _journalQueue;
            private readonly AutoResetEvent _journalSignal;
            private readonly List<Thread> _workerThreads;
            private readonly Dictionary<string, string> _storedSyncIndex;
            private bool _workersRunning;
            private string _journalIndexPath;
            private readonly object _clientsLock = new object();
            private readonly object _journalIndexLock = new object();
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private bool _isConnected;
            private readonly StringBuilder _lineBuffer = new StringBuilder();
            public EJServer(int port, string storagePath)
            {
            public EJServer(string storagePath)
            {
            public string RemoteEndpoint
            {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; }
            public string ATMType { get; private set; }
            public string RemoteEndPoint { get; private set; }
            public DateTime LastHeartbeatUtc { get; private set; }
            public DateTime LastSyncUtc { get; private set; }
            public string SyncId { get; set; }
            public string ATMId { get; set; }
            public string FileName { get; set; }
            public string Checksum { get; set; }
            public ClientConnection SourceConnection { get; set; }
            public void Start()
            {
            public void Stop()
            {
            private void ListenLoop()
            {
            private void HandleHandshakeReceived(string atmId, string remoteEndpoint)
            {
            private void HandleHeartbeatReceived(string atmId)
            {
            private void HandleMetadataReceived(string atmId, Dictionary<string, string> metadata)
            {
            private void HandleCashStatusReceived(string atmId, Dictionary<string, string> payload)
            {
            private void HandleClientDisconnected(string atmId)
            {
            private void HandleDataReceived(string atmId, byte[] encryptedData)
            {
            private byte[] DecryptAES256(byte[] data)
            {
            private byte[] DecompressData(byte[] data)
            {
            public void SendCommand(string atmId, string command, string parameters)
            {
            private string ComputeChecksum(byte[] data)
            {
            private static int ParseInt(Dictionary<string, string> values, string key)
            {
            private static string GetValue(Dictionary<string, string> values, string key, string fallback)
            {
            private void Log(string message)
            {
            public void Start(int port)
            {
            private void HandleClient(TcpClient client)
            {
            private void ListenForClients()
            {
            private void EnqueueJournalPacket(IncomingJournalPacket packet)
            {
            private void StoreJournalPacket(IncomingJournalPacket packet)
            {
            public List<ClientConnectionInfo> GetClients()
            {
            private void StartWorkers()
            {
            private void StopWorkers()
            {
            private void JournalWorkerLoop()
            {
            private void HandleClientDisconnected(ClientConnection connection)
            {
            private static string SanitizeFileName(string fileName)
            {
            private static string ComputeMD5(byte[] data)
            {
            private void LoadJournalIndex()
            {
            private bool TryGetStoredJournal(string syncId, out string serverPath)
            {
            private void RememberStoredJournal(string syncId, string serverPath)
            {
            private static string BuildStoredJournalFileName(string fileName, string syncId)
            {
            private static string MapCommand(string command)
            {
            private void HandleCommandResultReceived(string atmId, string commandId, string status, string details)
            {
            private void ReadLoop()
            {
            public void SendData(byte[] data)
            {
            public void Disconnect()
            {
            private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
            {
            private void ProcessBufferedLines()
            {
            private void ProcessLine(string line)
            {
            private void ProcessJournalOrFile(string[] parts)
            {
            public void SendLine(string text)
            {
            public event Action<string> OnLogMessage;
            public event Action<string, bool> OnClientStatusChanged;
            public event Action<ATMInfo> OnATMStatusChanged;
            public event Action<string, string, string> OnDataReceived;
            public event Action<string, string> OnLogReceived;
            public event Action<JournalSyncRecord> OnJournalSyncChanged;
            public event Action<string, string, string> OnJournalStored;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string> OnDisconnected;
            public event Action<IncomingJournalPacket> OnJournalPacketReceived;
            public event Action<ClientConnection> OnDisconnected;
            public event Action<string, string, string, string> OnCommandResultReceived;
        }
    
        public partial public class ClientConnection
        {
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private bool _isConnected;
            private string _storagePath;
            private readonly StringBuilder _lineBuffer = new StringBuilder();
            public ClientConnection(TcpClient client, string storagePath)
            {
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; }
            public string RemoteEndpoint
            {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            public string ATMType { get; private set; }
            public string RemoteEndPoint { get; private set; }
            public DateTime LastHeartbeatUtc { get; private set; }
            public DateTime LastSyncUtc { get; private set; }
            public string SyncId { get; set; }
            public string ATMId { get; set; }
            public string FileName { get; set; }
            public string Checksum { get; set; }
            public ClientConnection SourceConnection { get; set; }
            public void Start()
            {
            private void ReadLoop()
            {
            private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
            {
            public void SendData(byte[] data)
            {
            public void Disconnect()
            {
            private void ProcessBufferedLines()
            {
            private void ProcessLine(string line)
            {
            private void ProcessJournalOrFile(string[] parts)
            {
            public void SendLine(string text)
            {
            public event Action<string> OnLogMessage;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string> OnDisconnected;
            public event Action<IncomingJournalPacket> OnJournalPacketReceived;
            public event Action<ClientConnection> OnDisconnected;
            public event Action<string, string, string, string> OnCommandResultReceived;
        }
    
        public partial public class IncomingJournalPacket
        {
            public string SyncId { get; set; }
            public string ATMId { get; set; }
            public string FileName { get; set; }
            public string Checksum { get; set; }
            public ClientConnection SourceConnection { get; set; }
        }
    
        public partial public class ClientConnectionInfo
        {
            public string ATMID { get; set; }
            public string RemoteEndPoint { get; set; }
            public string ATMType { get; set; }
            public bool IsConnected { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastSyncUtc { get; set; }
        }
    
        public partial public class StorageStats
        {
        }
    
    }
    public partial public class EJServerService : IDisposable
        {
            private readonly ServerEngine    _serverEngine;
            private readonly ArchiveManager  _archiveManager;
            private readonly ImageSyncEngine _imageSync;
            private readonly JournalSyncTracker _syncTracker;
            private bool _isRunning;
            private readonly int _port;
            public int    ConnectedATMs        => _serverEngine.ConnectedCount;
            public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
            public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
            public bool   IsRunning            => _isRunning;
            public DateTime StartedAt          => _serverEngine.StartedAt;
            public EJServerService(int port = AppConstants.DefaultPort, string archivePath = null)
            {
            public int    TotalJournalsArchived { get; private set; }
            private void WireEvents()
            {
            public void Start()
            {
            public void Stop()
            {
            private void OnJournalReceived(object sender, ReceivedPacket pkt)
            {
            public bool SendCommand(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public RemoteCommand SendCommandDetailed(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public void Broadcast(string message, string sentBy = "System")
            =>
            public IEnumerable<RemoteCommand> GetRecentCommands()
            =>
            public bool SendImagesToATM(string atmId, string imagesFolder)
            {
            private void CheckHealth(ATMInfo atm)
            {
            public void CheckAllATMHealth()
            {
            public IEnumerable<ATMInfo>     GetConnectedATMs()   =>
            public ATMInfo                  GetATM(string atmId) =>
            private void Log(string msg)
            {
            public void Dispose()
            {
            public event EventHandler<ATMInfo>      OnATMConnected;
            public event EventHandler<ATMInfo>      OnATMDisconnected;
            public event EventHandler<ATMInfo>      OnATMUpdated;
            public event EventHandler<AlertPayload> OnAlert;
            public event EventHandler<RemoteCommand> OnCommandChanged;
            public event EventHandler<string>       OnLog;
        }
    
        public partial public class EJServer
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
            public EJServer(int port, string storagePath)
            {
            public void Start()
            {
            public void Stop()
            {
            private void ListenLoop()
            {
            private void HandleHandshakeReceived(string atmId, string remoteEndpoint)
            {
            private void HandleHeartbeatReceived(string atmId)
            {
            private void HandleMetadataReceived(string atmId, Dictionary<string, string> metadata)
            {
            private void HandleCashStatusReceived(string atmId, Dictionary<string, string> payload)
            {
            private void HandleClientDisconnected(string atmId)
            {
            private void HandleDataReceived(string atmId, byte[] encryptedData)
            {
            private byte[] DecryptAES256(byte[] data)
            {
            private byte[] DecompressData(byte[] data)
            {
            public void SendCommand(string atmId, string command, string parameters)
            {
            private string ComputeChecksum(byte[] data)
            {
            private static int ParseInt(Dictionary<string, string> values, string key)
            {
            private static string GetValue(Dictionary<string, string> values, string key, string fallback)
            {
            private void Log(string message)
            {
            public event Action<string> OnLogMessage;
            public event Action<string, bool> OnClientStatusChanged;
        }
    
        public partial public class ClientConnection
        {
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private bool _isConnected;
            public ClientConnection(TcpClient client, string storagePath)
            {
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; }
            public string RemoteEndpoint
            {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            public void Start()
            {
            private void ReadLoop()
            {
            private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
            {
            public void SendData(byte[] data)
            {
            public void Disconnect()
            {
            public event Action<string> OnLogMessage;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string> OnDisconnected;
        }
    
    }
    public partial public sealed class EJServerService : IDisposable
        {
            private readonly ServerEngine    _serverEngine;
            private readonly ArchiveManager  _archiveManager;
            private readonly ImageSyncEngine _imageSync;
            private readonly JournalSyncTracker _syncTracker;
            private bool _isRunning;
            private readonly int _port;
            public int    ConnectedATMs        => _serverEngine.ConnectedCount;
            public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
            public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
            public bool   IsRunning            => _isRunning;
            public DateTime StartedAt          => _serverEngine.StartedAt;
            public EJServerService(int port = AppConstants.DefaultPort, string archivePath = null)
            {
            public int    TotalJournalsArchived { get; private set; }
            private void WireEvents()
            {
            public void Start()
            {
            public void Stop()
            {
            private void OnJournalReceived(object sender, ReceivedPacket pkt)
            {
            public bool SendCommand(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public RemoteCommand SendCommandDetailed(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public void Broadcast(string message, string sentBy = "System")
            =>
            public IEnumerable<RemoteCommand> GetRecentCommands()
            =>
            public bool SendImagesToATM(string atmId, string imagesFolder)
            {
            private void CheckHealth(ATMInfo atm)
            {
            public void CheckAllATMHealth()
            {
            public IEnumerable<ATMInfo>     GetConnectedATMs()   =>
            public ATMInfo                  GetATM(string atmId) =>
            private void Log(string msg)
            {
            public void Dispose()
            {
            public event EventHandler<ATMInfo>      OnATMConnected;
            public event EventHandler<ATMInfo>      OnATMDisconnected;
            public event EventHandler<ATMInfo>      OnATMUpdated;
            public event EventHandler<AlertPayload> OnAlert;
            public event EventHandler<RemoteCommand> OnCommandChanged;
            public event EventHandler<string>       OnLog;
        }
    
        public partial public class EJServer
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
            public EJServer(int port, string storagePath)
            {
            public void Start()
            {
            public void Stop()
            {
            private void ListenLoop()
            {
            private void HandleHandshakeReceived(string atmId, string remoteEndpoint)
            {
            private void HandleHeartbeatReceived(string atmId)
            {
            private void HandleMetadataReceived(string atmId, Dictionary<string, string> metadata)
            {
            private void HandleCashStatusReceived(string atmId, Dictionary<string, string> payload)
            {
            private void HandleClientDisconnected(string atmId)
            {
            private void HandleDataReceived(string atmId, byte[] encryptedData)
            {
            private byte[] DecryptAES256(byte[] data)
            {
            private byte[] DecompressData(byte[] data)
            {
            public void SendCommand(string atmId, string command, string parameters)
            {
            private string ComputeChecksum(byte[] data)
            {
            private static int ParseInt(Dictionary<string, string> values, string key)
            {
            private static string GetValue(Dictionary<string, string> values, string key, string fallback)
            {
            private void Log(string message)
            {
            public event Action<string> OnLogMessage;
            public event Action<string, bool> OnClientStatusChanged;
        }
    
        public partial public class ClientConnection
        {
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private bool _isConnected;
            public ClientConnection(TcpClient client, string storagePath)
            {
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; }
            public string RemoteEndpoint
            {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            public void Start()
            {
            private void ReadLoop()
            {
            private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
            {
            public void SendData(byte[] data)
            {
            public void Disconnect()
            {
            public event Action<string> OnLogMessage;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string> OnDisconnected;
        }
    
    }
    public class IncomingJournalPacket
        {
            public string SyncId { get; set; }
            public string ATMId { get; set; }
            public string FileName { get; set; }
            public string Checksum { get; set; }
            public byte[] Payload { get; set; }
            public ClientConnection SourceConnection { get; set; }
        }
    public partial class IncomingJournalPacket
        {
            public string SyncId { get; set; }
    
    
            public string ATMId { get; set; }
    
    
            public string FileName { get; set; }
    
    
            public string Checksum { get; set; }
    
    
            public byte[] Payload { get; set; }
    
    
            public ClientConnection SourceConnection { get; set; }
    
    
        }

    // ═══ Class: ClientConnection (from 4 sources) ═══
        public partial class ClientConnection
        {
            // --- Constants & Fields ---
                    private TcpClient _tcpClient;
    
                    private NetworkStream _stream;
    
                    private Thread _readThread;
    
                    private string _storagePath;
    
                    private bool _isConnected;
    
                    public string RemoteEndpoint
                    {
                        get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
                    }
    
    
            // --- Properties ---
                    public string ATMID { get; private set; }
    
                    public bool IsConnected { get { return _isConnected; } }
    
    
            // --- Constructors ---
                    public ClientConnection(TcpClient client, string storagePath)
                    {
                        _tcpClient = client;
                        _storagePath = storagePath;
                        _stream = client.GetStream();
                        _isConnected = true;
                        ATMID = "Unknown";
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
                    public ClientConnection(TcpClient client, string storagePath)
                    {
                        _tcpClient = client;
                        _stream = client.GetStream();
                        _isConnected = true;
                        ATMID = "Unknown";
                    }
    
    
            // --- Methods ---
                    public void Start()
                    {
                        _readThread = new Thread(ReadLoop);
                        _readThread.IsBackground = true;
                        _readThread.Start();
                    }
    
                    private void ReadLoop()
                    {
                        byte[] buffer = new byte[65536];
                        while (_isConnected)
                        {
                            try
                            {
                                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                                if (bytesRead == 0) break;
    
                                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
    
                                if (data.StartsWith("EJLIVE_HANDSHAKE"))
                                {
                                    string[] parts = data.Split('|');
                                    if (parts.Length >= 2) ATMID = parts[1];
                                    OnLogMessage?.Invoke("Client identified: " + ATMID);
                                }
                                else if (data.StartsWith("HEARTBEAT"))
                                {
                                    // Heartbeat received, update last seen
                                }
                                else if (data.StartsWith("EJDATA"))
                                {
                                    // Parse header and extract data
                                    int headerEnd = data.IndexOf('\n');
                                    if (headerEnd > 0)
                                    {
                                        string header = data.Substring(0, headerEnd);
                                        string[] parts = header.Split('|');
                                        if (parts.Length >= 3)
                                        {
                                            string atmId = parts[1];
                                            int dataLen = int.Parse(parts[2]);
                                            byte[] ejData = new byte[bytesRead - headerEnd - 1];
                                            Array.Copy(buffer, headerEnd + 1, ejData, 0, ejData.Length);
                                            OnDataReceived?.Invoke(atmId, ejData);
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                break;
                            }
                        }
                        _isConnected = false;
                    }
    
                    public void SendData(byte[] data)
                    {
                        if (_isConnected && _stream != null)
                        {
                            _stream.Write(data, 0, data.Length);
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
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
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
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
                    public void SendData(byte[] data)
                    {
                        if (_isConnected && _stream != null)
                        {
                            _stream.Write(data, 0, data.Length);
                            _stream.Flush();
                        }
                    }
    
    
            // --- Events ---
                    public event Action<string> OnLogMessage;
    
                    public event Action<string, byte[]> OnDataReceived;
    
                    public event Action<string, string> OnHandshakeReceived;
    
                    public event Action<string> OnHeartbeatReceived;
    
                    public event Action<string, Dictionary<string, string>> OnMetadataReceived;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
                    public event Action<string, Dictionary<string, string>> OnCashStatusReceived;
    
                    public event Action<string> OnDisconnected;
    
    
        }
    /// <summary>
        /// Central EJLive Server - Handles connections from ATM clients
        /// Receives encrypted/compressed EJ data, decrypts, decompresses, and stores
        /// </summary>
        public class EJServer
        {
            private TcpListener _listener;
            private bool _isRunning;
            private int _port;
            private string _storagePath;
            private List<ClientConnection> _clients;
            private Thread _listenThread;
            private readonly object _clientsLock = new object();
            private readonly ConcurrentQueue<IncomingJournalPacket> _journalQueue;
            private readonly AutoResetEvent _journalSignal;
            private readonly List<Thread> _workerThreads;
            private readonly object _journalIndexLock = new object();
            private readonly Dictionary<string, string> _storedSyncIndex;
            private bool _workersRunning;
            private string _journalIndexPath;
    
            public event Action<string> OnLogMessage;
            public event Action<string, bool> OnClientStatusChanged;
            public event Action<JournalSyncRecord> OnJournalSyncChanged;
            public event Action<string, string, string> OnJournalStored;
    
            public EJServer(int port, string storagePath)
            {
                _port = port;
                _storagePath = storagePath;
                _clients = new List<ClientConnection>();
                _journalQueue = new ConcurrentQueue<IncomingJournalPacket>();
                _journalSignal = new AutoResetEvent(false);
                _workerThreads = new List<Thread>();
                _storedSyncIndex = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
    
            public void Start()
            {
                _isRunning = true;
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
    
                _listenThread = new Thread(ListenLoop);
                _listenThread.IsBackground = true;
                _listenThread.Start();
    
                Log("Server started on port " + _port);
                Log("Storage path: " + _storagePath);
    
                if (!Directory.Exists(_storagePath))
                    Directory.CreateDirectory(_storagePath);
    
                _journalIndexPath = Path.Combine(_storagePath, "_journal_sync_index.tsv");
                LoadJournalIndex();
                StartWorkers();
            }
    
            public void Stop()
            {
                _isRunning = false;
                try
                {
                    _listener?.Stop();
                    lock (_clientsLock)
                    {
                        foreach (var client in _clients)
                            client.Disconnect();
                        _clients.Clear();
                    }
                    StopWorkers();
                }
                catch { }
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
                        connection.OnJournalPacketReceived += EnqueueJournalPacket;
                        connection.OnDisconnected += HandleClientDisconnected;
                        lock (_clientsLock) _clients.Add(connection);
                        connection.Start();
                        Log("New client connected: " + tcpClient.Client.RemoteEndPoint.ToString());
                    }
                    catch (SocketException)
                    {
                        if (!_isRunning) break;
                    }
                }
            }
    
            private void EnqueueJournalPacket(IncomingJournalPacket packet)
            {
                _journalQueue.Enqueue(packet);
                _journalSignal.Set();
                OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                {
                    SyncId = packet.SyncId,
                    ATM_ID = packet.ATMId,
                    FileName = packet.FileName,
                    FileSize = packet.Payload == null ? 0 : packet.Payload.Length,
                    Checksum = packet.Checksum,
                    State = JournalSyncState.Syncing,
                    ProgressPercent = 60,
                    Message = "Journal received in server queue"
                });
            }
    
            private void StoreJournalPacket(IncomingJournalPacket packet)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(packet.SyncId))
                        packet.SyncId = Guid.NewGuid().ToString("N");
    
                    string existingPath;
                    if (TryGetStoredJournal(packet.SyncId, out existingPath))
                    {
                        string existingFileName = Path.GetFileName(existingPath);
                        Log("Duplicate journal sync acknowledged without re-writing: " + packet.SyncId);
                        packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_ACK, packet.SyncId, "STORED", existingFileName));
                        OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                        {
                            SyncId = packet.SyncId,
                            ATM_ID = packet.ATMId,
                            FileName = existingFileName,
                            FileSize = packet.Payload == null ? 0 : packet.Payload.Length,
                            Checksum = packet.Checksum,
                            State = JournalSyncState.Acknowledged,
                            ProgressPercent = 100,
                            ServerPath = existingPath,
                            Message = "Duplicate resend acknowledged from server index"
                        });
                        return;
                    }
    
                    byte[] rawData = packet.Payload ?? new byte[0];
                    if (!string.IsNullOrWhiteSpace(packet.Checksum))
                    {
                        string actualChecksum = ComputeMD5(rawData);
                        if (!string.Equals(actualChecksum, packet.Checksum, StringComparison.OrdinalIgnoreCase))
                        {
                            string mismatch = "Checksum mismatch. Expected=" + packet.Checksum + ", Actual=" + actualChecksum;
                            Log("Rejected journal from " + packet.ATMId + ": " + mismatch);
                            OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                            {
                                SyncId = packet.SyncId,
                                ATM_ID = packet.ATMId,
                                FileName = packet.FileName,
                                FileSize = rawData.Length,
                                Checksum = packet.Checksum,
                                State = JournalSyncState.Failed,
                                ProgressPercent = 0,
                                Message = mismatch
                            });
                            packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_NACK, packet.SyncId, mismatch));
                            return;
                        }
                    }
    
                    string atmDir = Path.Combine(_storagePath, packet.ATMId, DateTime.UtcNow.ToString("yyyy-MM"));
                    if (!Directory.Exists(atmDir))
                        Directory.CreateDirectory(atmDir);
    
                    string fileName = SanitizeFileName(string.IsNullOrWhiteSpace(packet.FileName)
                        ? "EJ_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".dat"
                        : packet.FileName);
                    string storedFileName = BuildStoredJournalFileName(fileName, packet.SyncId);
                    string filePath = Path.Combine(atmDir, storedFileName);
                    File.WriteAllBytes(filePath, rawData);
                    RememberStoredJournal(packet.SyncId, filePath);
    
                    Log("Stored " + rawData.Length + " bytes from " + packet.ATMId + " -> " + storedFileName);
                    OnClientStatusChanged?.Invoke(packet.ATMId, true);
                    OnJournalStored?.Invoke(packet.ATMId, storedFileName, filePath);
                    OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                    {
                        SyncId = packet.SyncId,
                        ATM_ID = packet.ATMId,
                        FileName = storedFileName,
                        FileSize = rawData.Length,
                        Checksum = packet.Checksum,
                        State = JournalSyncState.StoredOnServer,
                        ProgressPercent = 100,
                        ServerPath = filePath,
                        Message = "Journal stored on server"
                    });
                    packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_ACK, packet.SyncId, "STORED", storedFileName));
                }
                catch (Exception ex)
                {
                    Log("Error processing data from " + packet.ATMId + ": " + ex.Message);
                    OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                    {
                        SyncId = packet.SyncId,
                        ATM_ID = packet.ATMId,
                        FileName = packet.FileName,
                        FileSize = packet.Payload == null ? 0 : packet.Payload.Length,
                        Checksum = packet.Checksum,
                        State = JournalSyncState.Failed,
                        ProgressPercent = 0,
                        Message = ex.Message
                    });
                    packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_NACK, packet.SyncId, ex.Message));
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
                lock (_clientsLock) client = _clients.Find(c => c.ATMID == atmId);
                if (client != null && client.IsConnected)
                {
                    string protocolCommand = MapCommand(command);
                    client.SendLine(protocolCommand);
                    Log("Command sent to " + atmId + ": " + protocolCommand);
                }
                else
                {
                    Log("Cannot send command: " + atmId + " not connected.");
                }
            }
    
            private void Log(string message)
            {
                OnLogMessage?.Invoke("[Server] " + message);
            }
    
            public List<ClientConnectionInfo> GetClients()
            {
                lock (_clientsLock)
                {
                    var result = new List<ClientConnectionInfo>();
                    foreach (var client in _clients)
                    {
                        result.Add(new ClientConnectionInfo
                        {
                            ATMID = client.ATMID,
                            RemoteEndPoint = client.RemoteEndPoint,
                            ATMType = client.ATMType,
                            IsConnected = client.IsConnected,
                            LastHeartbeatUtc = client.LastHeartbeatUtc,
                            LastSyncUtc = client.LastSyncUtc
                        });
                    }
                    return result;
                }
            }
    
            private void StartWorkers()
            {
                _workersRunning = true;
                int workerCount = Math.Max(2, Environment.ProcessorCount / 2);
                for (int i = 0; i < workerCount; i++)
                {
                    var worker = new Thread(JournalWorkerLoop) { IsBackground = true, Name = "EJServerJournalWorker" + i };
                    _workerThreads.Add(worker);
                    worker.Start();
                }
                Log("Journal workers started: " + workerCount);
            }
    
            private void StopWorkers()
            {
                _workersRunning = false;
                _journalSignal.Set();
            }
    
            private void JournalWorkerLoop()
            {
                while (_workersRunning)
                {
                    try
                    {
                        IncomingJournalPacket packet;
                        if (_journalQueue.TryDequeue(out packet))
                        {
                            StoreJournalPacket(packet);
                        }
                        else
                        {
                            _journalSignal.WaitOne(500);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Worker error: " + ex.Message);
                    }
                }
            }
    
            private void HandleClientDisconnected(ClientConnection connection)
            {
                lock (_clientsLock) _clients.Remove(connection);
                if (!string.IsNullOrEmpty(connection.ATMID))
                    OnClientStatusChanged?.Invoke(connection.ATMID, false);
            }
    
            private static string SanitizeFileName(string fileName)
            {
                foreach (var c in Path.GetInvalidFileNameChars())
                    fileName = fileName.Replace(c, '_');
                return fileName;
            }
    
            private static string ComputeMD5(byte[] data)
            {
                using (var md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(data ?? new byte[0]);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
    
            private void LoadJournalIndex()
            {
                lock (_journalIndexLock)
                {
                    _storedSyncIndex.Clear();
                    if (string.IsNullOrWhiteSpace(_journalIndexPath) || !File.Exists(_journalIndexPath))
                        return;
    
                    foreach (string line in File.ReadAllLines(_journalIndexPath, Encoding.UTF8))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] parts = line.Split('\t');
                        if (parts.Length < 2) continue;
                        _storedSyncIndex[parts[0]] = parts[1];
                    }
                }
            }
    
            private bool TryGetStoredJournal(string syncId, out string serverPath)
            {
                serverPath = null;
                if (string.IsNullOrWhiteSpace(syncId)) return false;
                lock (_journalIndexLock)
                {
                    if (_storedSyncIndex.TryGetValue(syncId, out serverPath))
                        return !string.IsNullOrWhiteSpace(serverPath) && File.Exists(serverPath);
                }
                return false;
            }
    
            private void RememberStoredJournal(string syncId, string serverPath)
            {
                if (string.IsNullOrWhiteSpace(syncId) || string.IsNullOrWhiteSpace(serverPath)) return;
                lock (_journalIndexLock)
                {
                    _storedSyncIndex[syncId] = serverPath;
                    string line = syncId + "\t" + serverPath + "\t" + DateTime.UtcNow.ToString("O");
                    File.AppendAllText(_journalIndexPath, line + Environment.NewLine, Encoding.UTF8);
                }
            }
    
            private static string BuildStoredJournalFileName(string fileName, string syncId)
            {
                fileName = SanitizeFileName(fileName);
                string name = Path.GetFileNameWithoutExtension(fileName);
                string ext = Path.GetExtension(fileName);
                string shortSyncId = string.IsNullOrWhiteSpace(syncId) ? Guid.NewGuid().ToString("N").Substring(0, 8) : syncId.Substring(0, Math.Min(8, syncId.Length));
                return name + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff") + "_" + shortSyncId + ext;
            }
    
            private static string MapCommand(string command)
            {
                switch ((command ?? string.Empty).ToUpperInvariant())
                {
                    case "RESTART":
                    case "CMD_RESTART":
                        return Protocol.CMD_RESTART;
                    case "SCREENSHOT":
                    case "CMD_SCREENSHOT":
                        return Protocol.CMD_SCREENSHOT;
                    case "TIMESYNC":
                    case "CMD_TIMESYNC":
                        return Protocol.CMD_TIMESYNC;
                    case "SHUTDOWN":
                    case "CMD_SHUTDOWN":
                        return Protocol.CMD_SHUTDOWN;
                    case "IMAGE_SYNC":
                    case "CMD_IMAGE_SYNC":
                        return Protocol.CMD_IMAGE_SYNC;
                    case "FORCE_SYNC":
                    case "CMD_FORCE_SYNC":
                        return Protocol.CMD_FORCE_SYNC;
                    case "GHOST_START":
                    case "CMD_GHOST_START":
                        return Protocol.CMD_GHOST_START;
                    case "GHOST_STOP":
                    case "CMD_GHOST_STOP":
                        return Protocol.CMD_GHOST_STOP;
                    case "GET_SYSINFO":
                    case "SYSINFO":
                    case "CMD_SYSINFO":
                        return Protocol.CMD_GET_SYSINFO;
                    default:
                        return command;
                }
            }
        }
    /// <summary>
        /// Central EJLive Server - Handles connections from ATM clients
        /// Receives encrypted/compressed EJ data, decrypts, decompresses, and stores
        /// </summary>
        public class EJServer
        {
            private TcpListener _listener;
            private bool _isRunning;
            private int _port;
            private string _storagePath;
            private List<ClientConnection> _clients;
            private Thread _listenThread;
    
            public event Action<string> OnLogMessage;
            public event Action<string, bool> OnClientStatusChanged;
    
            public EJServer(int port, string storagePath)
            {
                _port = port;
                _storagePath = storagePath;
                _clients = new List<ClientConnection>();
            }
    
            public void Start()
            {
                _isRunning = true;
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
    
                _listenThread = new Thread(ListenLoop);
                _listenThread.IsBackground = true;
                _listenThread.Start();
    
                Log("Server started on port " + _port);
                Log("Storage path: " + _storagePath);
    
                if (!Directory.Exists(_storagePath))
                    Directory.CreateDirectory(_storagePath);
            }
    
            public void Stop()
            {
                _isRunning = false;
                try
                {
                    _listener?.Stop();
                    foreach (var client in _clients)
                        client.Disconnect();
                    _clients.Clear();
                }
                catch { }
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
                        _clients.Add(connection);
                        connection.Start();
                        Log("New client connected: " + tcpClient.Client.RemoteEndPoint.ToString());
                    }
                    catch (SocketException)
                    {
                        if (!_isRunning) break;
                    }
                }
            }
    
            private void HandleDataReceived(string atmId, byte[] encryptedData)
            {
                try
                {
                    // Step 1: Decrypt
                    byte[] compressed = DecryptAES256(encryptedData);
    
                    // Step 2: Decompress
                    byte[] rawData = DecompressData(compressed);
    
                    // Step 3: Store
                    string atmDir = Path.Combine(_storagePath, atmId, DateTime.Now.ToString("yyyy-MM"));
                    if (!Directory.Exists(atmDir))
                        Directory.CreateDirectory(atmDir);
    
                    string fileName = "EJ_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".dat";
                    string filePath = Path.Combine(atmDir, fileName);
                    File.WriteAllBytes(filePath, rawData);
    
                    Log("Stored " + rawData.Length + " bytes from " + atmId + " -> " + fileName);
                    OnClientStatusChanged?.Invoke(atmId, true);
                }
                catch (Exception ex)
                {
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
                var client = _clients.Find(c => c.ATMID == atmId);
                if (client != null && client.IsConnected)
                {
                    string cmdPacket = "CMD|" + command + "|" + parameters;
                    client.SendData(Encoding.UTF8.GetBytes(cmdPacket));
                    Log("Command sent to " + atmId + ": " + command);
                }
                else
                {
                    Log("Cannot send command: " + atmId + " not connected.");
                }
            }
    
            private void Log(string message)
            {
                OnLogMessage?.Invoke("[Server] " + message);
            }
        }
    public class EJServer
        {
            private TcpListener _listener;
            private bool _isRunning;
            private string _storagePath;
            private Dictionary<string, ATMInfo> _activeATMs = new Dictionary<string, ATMInfo>();
    
            public event Action<ATMInfo> OnATMStatusChanged;
            public event Action<string, string, string> OnDataReceived;
    
            public EJServer(string storagePath)
            {
                _storagePath = storagePath;
                if (!Directory.Exists(_storagePath)) Directory.CreateDirectory(_storagePath);
            }
    
            public void Start(int port)
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                _isRunning = true;
                new Thread(ListenLoop).Start();
            }
    
            private void ListenLoop()
            {
                while (_isRunning)
                {
                    try
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        new Thread(() => HandleClient(client)).Start();
                    }
                    catch { }
                }
            }
    
            private void HandleClient(TcpClient client)
            {
                string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[8192];
                    while (client.Connected && _isRunning)
                    {
                        try
                        {
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0) break;
    
                            byte[] data = new byte[bytesRead];
                            Array.Copy(buffer, data, bytesRead);
    
                            byte[] decrypted = SecurityHelper.Decrypt(data);
                            byte[] decompressed = SecurityHelper.Decompress(decrypted);
    
                            string content = Encoding.UTF8.GetString(decompressed);
                            OnDataReceived?.Invoke(clientIP, "EJ_DATA", content);
                        }
                        catch { break; }
                    }
                }
            }
    
            public void Stop() => _isRunning = false;
        }
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
    public partial class EJServer
        {
            private TcpListener _listener;
    
    
            private bool _isRunning;
    
    
            private int _port;
    
    
            private string _storagePath;
    
    
            private List<ClientConnection> _clients;
    
    
            private Thread _listenThread;
    
    
            private readonly ConcurrentQueue<IncomingJournalPacket> _journalQueue;
    
    
            private readonly AutoResetEvent _journalSignal;
    
    
            private readonly List<Thread> _workerThreads;
    
    
            private readonly Dictionary<string, string> _storedSyncIndex;
    
    
            private bool _workersRunning;
    
    
            private string _journalIndexPath;
    
    
            public EJServer(int port, string storagePath)
            {
                _port = port;
                _storagePath = storagePath;
                _clients = new List<ClientConnection>();
                _journalQueue = new ConcurrentQueue<IncomingJournalPacket>();
                _journalSignal = new AutoResetEvent(false);
                _workerThreads = new List<Thread>();
                _storedSyncIndex = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
    
    
            private readonly object _clientsLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLiveWorkCoder\EJLive.Server.WinForms\Services\EJServer.cs
            private readonly object _journalIndexLock = new object();
    
    
            public void Start()
            {
                _isRunning = true;
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
    
                _listenThread = new Thread(ListenLoop);
                _listenThread.IsBackground = true;
                _listenThread.Start();
    
                Log("Server started on port " + _port);
                Log("Storage path: " + _storagePath);
    
                if (!Directory.Exists(_storagePath))
                    Directory.CreateDirectory(_storagePath);
    
                _journalIndexPath = Path.Combine(_storagePath, "_journal_sync_index.tsv");
                LoadJournalIndex();
                StartWorkers();
            }
    
    
            public void Stop()
            {
                _isRunning = false;
                try
                {
                    _listener?.Stop();
                    lock (_clientsLock)
                    {
                        foreach (var client in _clients)
                            client.Disconnect();
                        _clients.Clear();
                    }
                    StopWorkers();
                }
                catch { }
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
                        connection.OnJournalPacketReceived += EnqueueJournalPacket;
                        connection.OnDisconnected += HandleClientDisconnected;
                        lock (_clientsLock) _clients.Add(connection);
                        connection.Start();
                        Log("New client connected: " + tcpClient.Client.RemoteEndPoint.ToString());
                    }
                    catch (SocketException)
                    {
                        if (!_isRunning) break;
                    }
                }
            }
    
    
            private void EnqueueJournalPacket(IncomingJournalPacket packet)
            {
                _journalQueue.Enqueue(packet);
                _journalSignal.Set();
                OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                {
                    SyncId = packet.SyncId,
                    ATM_ID = packet.ATMId,
                    FileName = packet.FileName,
                    FileSize = packet.Payload == null ? 0 : packet.Payload.Length,
                    Checksum = packet.Checksum,
                    State = JournalSyncState.Syncing,
                    ProgressPercent = 60,
                    Message = "Journal received in server queue"
                });
            }
    
    
            private void StoreJournalPacket(IncomingJournalPacket packet)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(packet.SyncId))
                        packet.SyncId = Guid.NewGuid().ToString("N");
    
                    string existingPath;
                    if (TryGetStoredJournal(packet.SyncId, out existingPath))
                    {
                        string existingFileName = Path.GetFileName(existingPath);
                        Log("Duplicate journal sync acknowledged without re-writing: " + packet.SyncId);
                        packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_ACK, packet.SyncId, "STORED", existingFileName));
                        OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                        {
                            SyncId = packet.SyncId,
                            ATM_ID = packet.ATMId,
                            FileName = existingFileName,
                            FileSize = packet.Payload == null ? 0 : packet.Payload.Length,
                            Checksum = packet.Checksum,
                            State = JournalSyncState.Acknowledged,
                            ProgressPercent = 100,
                            ServerPath = existingPath,
                            Message = "Duplicate resend acknowledged from server index"
                        });
                        return;
                    }
    
                    byte[] rawData = packet.Payload ?? new byte[0];
                    if (!string.IsNullOrWhiteSpace(packet.Checksum))
                    {
                        string actualChecksum = ComputeMD5(rawData);
                        if (!string.Equals(actualChecksum, packet.Checksum, StringComparison.OrdinalIgnoreCase))
                        {
                            string mismatch = "Checksum mismatch. Expected=" + packet.Checksum + ", Actual=" + actualChecksum;
                            Log("Rejected journal from " + packet.ATMId + ": " + mismatch);
                            OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                            {
                                SyncId = packet.SyncId,
                                ATM_ID = packet.ATMId,
                                FileName = packet.FileName,
                                FileSize = rawData.Length,
                                Checksum = packet.Checksum,
                                State = JournalSyncState.Failed,
                                ProgressPercent = 0,
                                Message = mismatch
                            });
                            packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_NACK, packet.SyncId, mismatch));
                            return;
                        }
                    }
    
                    string atmDir = Path.Combine(_storagePath, packet.ATMId, DateTime.UtcNow.ToString("yyyy-MM"));
                    if (!Directory.Exists(atmDir))
                        Directory.CreateDirectory(atmDir);
    
                    string fileName = SanitizeFileName(string.IsNullOrWhiteSpace(packet.FileName)
                        ? "EJ_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".dat"
                        : packet.FileName);
                    string storedFileName = BuildStoredJournalFileName(fileName, packet.SyncId);
                    string filePath = Path.Combine(atmDir, storedFileName);
                    File.WriteAllBytes(filePath, rawData);
                    RememberStoredJournal(packet.SyncId, filePath);
    
                    Log("Stored " + rawData.Length + " bytes from " + packet.ATMId + " -> " + storedFileName);
                    OnClientStatusChanged?.Invoke(packet.ATMId, true);
                    OnJournalStored?.Invoke(packet.ATMId, storedFileName, filePath);
                    OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                    {
                        SyncId = packet.SyncId,
                        ATM_ID = packet.ATMId,
                        FileName = storedFileName,
                        FileSize = rawData.Length,
                        Checksum = packet.Checksum,
                        State = JournalSyncState.StoredOnServer,
                        ProgressPercent = 100,
                        ServerPath = filePath,
                        Message = "Journal stored on server"
                    });
                    packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_ACK, packet.SyncId, "STORED", storedFileName));
                }
                catch (Exception ex)
                {
                    Log("Error processing data from " + packet.ATMId + ": " + ex.Message);
                    OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                    {
                        SyncId = packet.SyncId,
                        ATM_ID = packet.ATMId,
                        FileName = packet.FileName,
                        FileSize = packet.Payload == null ? 0 : packet.Payload.Length,
                        Checksum = packet.Checksum,
                        State = JournalSyncState.Failed,
                        ProgressPercent = 0,
                        Message = ex.Message
                    });
                    packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_NACK, packet.SyncId, ex.Message));
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
                lock (_clientsLock) client = _clients.Find(c => c.ATMID == atmId);
                if (client != null && client.IsConnected)
                {
                    string protocolCommand = MapCommand(command);
                    client.SendLine(protocolCommand);
                    Log("Command sent to " + atmId + ": " + protocolCommand);
                }
                else
                {
                    Log("Cannot send command: " + atmId + " not connected.");
                }
            }
    
    
            private void Log(string message)
            {
                OnLogMessage?.Invoke("[Server] " + message);
            }
    
    
            public List<ClientConnectionInfo> GetClients()
            {
                lock (_clientsLock)
                {
                    var result = new List<ClientConnectionInfo>();
                    foreach (var client in _clients)
                    {
                        result.Add(new ClientConnectionInfo
                        {
                            ATMID = client.ATMID,
                            RemoteEndPoint = client.RemoteEndPoint,
                            ATMType = client.ATMType,
                            IsConnected = client.IsConnected,
                            LastHeartbeatUtc = client.LastHeartbeatUtc,
                            LastSyncUtc = client.LastSyncUtc
                        });
                    }
                    return result;
                }
            }
    
    
            private void StartWorkers()
            {
                _workersRunning = true;
                int workerCount = Math.Max(2, Environment.ProcessorCount / 2);
                for (int i = 0; i < workerCount; i++)
                {
                    var worker = new Thread(JournalWorkerLoop) { IsBackground = true, Name = "EJServerJournalWorker" + i };
                    _workerThreads.Add(worker);
                    worker.Start();
                }
                Log("Journal workers started: " + workerCount);
            }
    
    
            private void StopWorkers()
            {
                _workersRunning = false;
                _journalSignal.Set();
            }
    
    
            private void JournalWorkerLoop()
            {
                while (_workersRunning)
                {
                    try
                    {
                        IncomingJournalPacket packet;
                        if (_journalQueue.TryDequeue(out packet))
                        {
                            StoreJournalPacket(packet);
                        }
                        else
                        {
                            _journalSignal.WaitOne(500);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Worker error: " + ex.Message);
                    }
                }
            }
    
    
            private void HandleClientDisconnected(ClientConnection connection)
            {
                lock (_clientsLock) _clients.Remove(connection);
                if (!string.IsNullOrEmpty(connection.ATMID))
                    OnClientStatusChanged?.Invoke(connection.ATMID, false);
            }
    
    
            private static string SanitizeFileName(string fileName)
            {
                foreach (var c in Path.GetInvalidFileNameChars())
                    fileName = fileName.Replace(c, '_');
                return fileName;
            }
    
    
            private static string ComputeMD5(byte[] data)
            {
                using (var md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(data ?? new byte[0]);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
    
    
            private void LoadJournalIndex()
            {
                lock (_journalIndexLock)
                {
                    _storedSyncIndex.Clear();
                    if (string.IsNullOrWhiteSpace(_journalIndexPath) || !File.Exists(_journalIndexPath))
                        return;
    
                    foreach (string line in File.ReadAllLines(_journalIndexPath, Encoding.UTF8))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] parts = line.Split('\t');
                        if (parts.Length < 2) continue;
                        _storedSyncIndex[parts[0]] = parts[1];
                    }
                }
            }
    
    
            private bool TryGetStoredJournal(string syncId, out string serverPath)
            {
                serverPath = null;
                if (string.IsNullOrWhiteSpace(syncId)) return false;
                lock (_journalIndexLock)
                {
                    if (_storedSyncIndex.TryGetValue(syncId, out serverPath))
                        return !string.IsNullOrWhiteSpace(serverPath) && File.Exists(serverPath);
                }
                return false;
            }
    
    
            private void RememberStoredJournal(string syncId, string serverPath)
            {
                if (string.IsNullOrWhiteSpace(syncId) || string.IsNullOrWhiteSpace(serverPath)) return;
                lock (_journalIndexLock)
                {
                    _storedSyncIndex[syncId] = serverPath;
                    string line = syncId + "\t" + serverPath + "\t" + DateTime.UtcNow.ToString("O");
                    File.AppendAllText(_journalIndexPath, line + Environment.NewLine, Encoding.UTF8);
                }
            }
    
    
            private static string BuildStoredJournalFileName(string fileName, string syncId)
            {
                fileName = SanitizeFileName(fileName);
                string name = Path.GetFileNameWithoutExtension(fileName);
                string ext = Path.GetExtension(fileName);
                string shortSyncId = string.IsNullOrWhiteSpace(syncId) ? Guid.NewGuid().ToString("N").Substring(0, 8) : syncId.Substring(0, Math.Min(8, syncId.Length));
                return name + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff") + "_" + shortSyncId + ext;
            }
    
    
            private static string MapCommand(string command)
            {
                switch ((command ?? string.Empty).ToUpperInvariant())
                {
                    case "RESTART":
                    case "CMD_RESTART":
                        return Protocol.CMD_RESTART;
                    case "SCREENSHOT":
                    case "CMD_SCREENSHOT":
                        return Protocol.CMD_SCREENSHOT;
                    case "TIMESYNC":
                    case "CMD_TIMESYNC":
                        return Protocol.CMD_TIMESYNC;
                    case "SHUTDOWN":
                    case "CMD_SHUTDOWN":
                        return Protocol.CMD_SHUTDOWN;
                    case "IMAGE_SYNC":
                    case "CMD_IMAGE_SYNC":
                        return Protocol.CMD_IMAGE_SYNC;
                    case "FORCE_SYNC":
                    case "CMD_FORCE_SYNC":
                        return Protocol.CMD_FORCE_SYNC;
                    case "GHOST_START":
                    case "CMD_GHOST_START":
                        return Protocol.CMD_GHOST_START;
                    case "GHOST_STOP":
                    case "CMD_GHOST_STOP":
                        return Protocol.CMD_GHOST_STOP;
                    case "GET_SYSINFO":
                    case "SYSINFO":
                    case "CMD_SYSINFO":
                        return Protocol.CMD_GET_SYSINFO;
                    default:
                        return command;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLiveWorkCoder\EJLiveWorkCoder\EJLive.Server.WinForms\Services\EJServer.cs
            private readonly object _journalIndexLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLiveWorkCoder112233\EJLiveWorkCoder\EJLive.Server.WinForms\Services\EJServer.cs
            private readonly object _journalIndexLock = new object();
    
    
            public event Action<string> OnLogMessage;
    
    
            public event Action<string, bool> OnClientStatusChanged;
    
    
            public event Action<JournalSyncRecord> OnJournalSyncChanged;
    
    
            public event Action<string, string, string> OnJournalStored;
    
    
        }
    public partial class EJServer
        {
            private TcpListener _listener;
    
    
            private bool _isRunning;
    
    
            private int _port;
    
    
            private string _storagePath;
    
    
            private List<ClientConnection> _clients;
    
    
            private Thread _listenThread;
    
    
            public EJServer(int port, string storagePath)
            {
                _port = port;
                _storagePath = storagePath;
                _clients = new List<ClientConnection>();
            }
    
    
            public void Start()
            {
                _isRunning = true;
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
    
                _listenThread = new Thread(ListenLoop);
                _listenThread.IsBackground = true;
                _listenThread.Start();
    
                Log("Server started on port " + _port);
                Log("Storage path: " + _storagePath);
    
                if (!Directory.Exists(_storagePath))
                    Directory.CreateDirectory(_storagePath);
            }
    
    
            public void Stop()
            {
                _isRunning = false;
                try
                {
                    _listener?.Stop();
                    foreach (var client in _clients)
                        client.Disconnect();
                    _clients.Clear();
                }
                catch { }
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
                        _clients.Add(connection);
                        connection.Start();
                        Log("New client connected: " + tcpClient.Client.RemoteEndPoint.ToString());
                    }
                    catch (SocketException)
                    {
                        if (!_isRunning) break;
                    }
                }
            }
    
    
            private void HandleDataReceived(string atmId, byte[] encryptedData)
            {
                try
                {
                    // Step 1: Decrypt
                    byte[] compressed = DecryptAES256(encryptedData);
    
                    // Step 2: Decompress
                    byte[] rawData = DecompressData(compressed);
    
                    // Step 3: Store
                    string atmDir = Path.Combine(_storagePath, atmId, DateTime.Now.ToString("yyyy-MM"));
                    if (!Directory.Exists(atmDir))
                        Directory.CreateDirectory(atmDir);
    
                    string fileName = "EJ_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".dat";
                    string filePath = Path.Combine(atmDir, fileName);
                    File.WriteAllBytes(filePath, rawData);
    
                    Log("Stored " + rawData.Length + " bytes from " + atmId + " -> " + fileName);
                    OnClientStatusChanged?.Invoke(atmId, true);
                }
                catch (Exception ex)
                {
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
                var client = _clients.Find(c => c.ATMID == atmId);
                if (client != null && client.IsConnected)
                {
                    string cmdPacket = "CMD|" + command + "|" + parameters;
                    client.SendData(Encoding.UTF8.GetBytes(cmdPacket));
                    Log("Command sent to " + atmId + ": " + command);
                }
                else
                {
                    Log("Cannot send command: " + atmId + " not connected.");
                }
            }
    
    
            private void Log(string message)
            {
                OnLogMessage?.Invoke("[Server] " + message);
            }
    
    
            public event Action<string> OnLogMessage;
    
    
            public event Action<string, bool> OnClientStatusChanged;
    
    
        }
    public partial class EJServer
        {
            private TcpListener _listener;
    
    
            private bool _isRunning;
    
    
            private int _port;
    
    
            private string _storagePath;
    
    
            private List<ClientConnection> _clients;
    
    
            private Thread _listenThread;
    
    
            private MonitoringStateStore _monitoringStore;
    
    
            private JournalSyncService _journalSyncService;
    
    
            public EJServer(int port, string storagePath)
            {
                _port = port;
                _storagePath = storagePath;
                _clients = new List<ClientConnection>();
                _monitoringStore = new MonitoringStateStore(storagePath);
                _journalSyncService = new JournalSyncService(storagePath);
            }
    
    
            private readonly object _clientsSync = new object();
    
    
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
                        connection.OnCommandResultReceived += HandleCommandResultReceived;
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
    
    
            private void HandleCommandResultReceived(string atmId, string commandId, string status, string details)
            {
                string effectiveAtmId = string.IsNullOrWhiteSpace(atmId) ? "Unknown" : atmId;
                string effectiveStatus = string.IsNullOrWhiteSpace(status) ? "UNKNOWN" : status;
                string message = "Command result [" + effectiveStatus + "] " + commandId + ": " + (details ?? string.Empty);
    
                _monitoringStore.RecordAlert(effectiveAtmId, string.Equals(effectiveStatus, "FAILED", StringComparison.OrdinalIgnoreCase) ? "Warning" : "Info", message);
                Log(message);
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
                    string commandId = Guid.NewGuid().ToString("N");
                    string cmdPacket = "CMD|" + commandId + "|" + command + "|" + parameters;
                    client.SendData(Encoding.UTF8.GetBytes(cmdPacket + "\n"));
                    _monitoringStore.RecordAlert(atmId, "Info", "Command queued: " + command + " [" + commandId + "]");
                    Log("Command sent to " + atmId + ": " + command + " [" + commandId + "]");
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
    
    
            public event Action<string> OnLogMessage;
    
    
            public event Action<string, bool> OnClientStatusChanged;
    
    
        }
    public partial class EJServer
        {
            private TcpListener _listener;
    
    
            private bool _isRunning;
    
    
            private int _port;
    
    
            private string _storagePath;
    
    
            private List<ClientConnection> _clients;
    
    
            private Thread _listenThread;
    
    
            private MonitoringStateStore _monitoringStore;
    
    
            private JournalSyncService _journalSyncService;
    
    
            public EJServer(int port, string storagePath)
            {
                _port = port;
                _storagePath = storagePath;
                _clients = new List<ClientConnection>();
                _monitoringStore = new MonitoringStateStore(storagePath);
                _journalSyncService = new JournalSyncService(storagePath);
            }
    
    
            private readonly object _clientsSync = new object();
    
    
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
    
    
            public event Action<string> OnLogMessage;
    
    
            public event Action<string, bool> OnClientStatusChanged;
    
    
        }
    public partial class EJServer
        {
            private TcpListener _listener;
    
    
            private bool _isRunning;
    
    
            private int _port;
    
    
            private string _storagePath;
    
    
            private List<ClientConnection> _clients;
    
    
            private Thread _listenThread;
    
    
            private MonitoringStateStore _monitoringStore;
    
    
            private JournalSyncService _journalSyncService;
    
    
            public EJServer(int port, string storagePath)
            {
                _port = port;
                _storagePath = storagePath;
                _clients = new List<ClientConnection>();
            }
    
    
            public EJServer(string storagePath)
            {
                _storagePath = storagePath;
                if (!Directory.Exists(_storagePath)) Directory.CreateDirectory(_storagePath);
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Server.WinForms\Services\EJServer.cs
            public EJServer(int port, string storagePath)
            {
                _port = port;
                _storagePath = storagePath;
                _clients = new List<ClientConnection>();
                _monitoringStore = new MonitoringStateStore(storagePath);
                _journalSyncService = new JournalSyncService(storagePath);
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Server.WinForms\Services\EJServer.cs
            public EJServer(int port, string storagePath)
            {
                _port = port;
                _storagePath = storagePath;
                _clients = new List<ClientConnection>();
                _monitoringStore = new MonitoringStateStore(storagePath);
                _journalSyncService = new JournalSyncService(storagePath);
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
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
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
    
                _listenThread = new Thread(ListenLoop);
                _listenThread.IsBackground = true;
                _listenThread.Start();
    
                Log("Server started on port " + _port);
                Log("Storage path: " + _storagePath);
    
                if (!Directory.Exists(_storagePath))
                    Directory.CreateDirectory(_storagePath);
            }
    
    
            public void Stop()
            {
                _isRunning = false;
                try
                {
                    _listener?.Stop();
                    foreach (var client in _clients)
                        client.Disconnect();
                    _clients.Clear();
                }
                catch { }
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
                        _clients.Add(connection);
                        connection.Start();
                        Log("New client connected: " + tcpClient.Client.RemoteEndPoint.ToString());
                    }
                    catch (SocketException)
                    {
                        if (!_isRunning) break;
                    }
                }
            }
    
    
            private void HandleDataReceived(string atmId, byte[] encryptedData)
            {
                try
                {
                    // Step 1: Decrypt
                    byte[] compressed = DecryptAES256(encryptedData);
    
                    // Step 2: Decompress
                    byte[] rawData = DecompressData(compressed);
    
                    // Step 3: Store
                    string atmDir = Path.Combine(_storagePath, atmId, DateTime.Now.ToString("yyyy-MM"));
                    if (!Directory.Exists(atmDir))
                        Directory.CreateDirectory(atmDir);
    
                    string fileName = "EJ_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".dat";
                    string filePath = Path.Combine(atmDir, fileName);
                    File.WriteAllBytes(filePath, rawData);
    
                    Log("Stored " + rawData.Length + " bytes from " + atmId + " -> " + fileName);
                    OnClientStatusChanged?.Invoke(atmId, true);
                }
                catch (Exception ex)
                {
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
                var client = _clients.Find(c => c.ATMID == atmId);
                if (client != null && client.IsConnected)
                {
                    string cmdPacket = "CMD|" + command + "|" + parameters;
                    client.SendData(Encoding.UTF8.GetBytes(cmdPacket));
                    Log("Command sent to " + atmId + ": " + command);
                }
                else
                {
                    Log("Cannot send command: " + atmId + " not connected.");
                }
            }
    
    
            private void Log(string message)
            {
                OnLogMessage?.Invoke("[Server] " + message);
            }
    
    
            private Dictionary<string, ATMInfo> _activeATMs = new Dictionary<string, ATMInfo>();
    
    
            public void Start(int port)
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                _isRunning = true;
                new Thread(ListenLoop).Start();
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            private void HandleClient(TcpClient client)
            {
                string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[8192];
                    while (client.Connected && _isRunning)
                    {
                        try
                        {
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0) break;
    
                            byte[] data = new byte[bytesRead];
                            Array.Copy(buffer, data, bytesRead);
    
                            byte[] decrypted = SecurityHelper.Decrypt(data);
                            byte[] decompressed = SecurityHelper.Decompress(decrypted);
    
                            string content = Encoding.UTF8.GetString(decompressed);
                            OnDataReceived?.Invoke(clientIP, "EJ_DATA", content);
                        }
                        catch { break; }
                    }
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            private readonly object _clientsSync = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_WinForms_v3.2.1\EJLive.Server.WinForms\Services\EJServer.cs
            private void ListenLoop()
            {
                while (_isRunning)
                {
                    try
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        new Thread(() => HandleClient(client)).Start();
                    }
                    catch { }
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_WinForms_v3.2.1\EJLive.Server.WinForms\Services\EJServer.cs
            public void Stop() => _isRunning = false;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            public event Action<string> OnLogMessage;
    
    
            public event Action<string, bool> OnClientStatusChanged;
    
    
            public event Action<ATMInfo> OnATMStatusChanged;
    
    
            public event Action<string, string, string> OnDataReceived;
    
    
        }
    public partial class EJServer
        {
            private TcpListener _listener;
    
    
            private bool _isRunning;
    
    
            private int _port;
    
    
            private string _storagePath;
    
    
            private List<ClientConnection> _clients;
    
    
            private Thread _listenThread;
    
    
            private readonly ConcurrentQueue<IncomingJournalPacket> _journalQueue;
    
    
            private readonly AutoResetEvent _journalSignal;
    
    
            private readonly List<Thread> _workerThreads;
    
    
            private readonly Dictionary<string, string> _storedSyncIndex;
    
    
            private bool _workersRunning;
    
    
            private string _journalIndexPath;
    
    
            public EJServer(int port, string storagePath)
            {
                _port = port;
                _storagePath = storagePath;
                _clients = new List<ClientConnection>();
                _journalQueue = new ConcurrentQueue<IncomingJournalPacket>();
                _journalSignal = new AutoResetEvent(false);
                _workerThreads = new List<Thread>();
                _storedSyncIndex = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
    
    
            private readonly object _clientsLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Server.WinForms\Services\EJServer.cs
            private readonly object _journalIndexLock = new object();
    
    
            public void Start()
            {
                _isRunning = true;
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
    
                _listenThread = new Thread(ListenLoop);
                _listenThread.IsBackground = true;
                _listenThread.Start();
    
                Log("Server started on port " + _port);
                Log("Storage path: " + _storagePath);
    
                if (!Directory.Exists(_storagePath))
                    Directory.CreateDirectory(_storagePath);
    
                _journalIndexPath = Path.Combine(_storagePath, "_journal_sync_index.tsv");
                LoadJournalIndex();
                StartWorkers();
            }
    
    
            public void Stop()
            {
                _isRunning = false;
                try
                {
                    _listener?.Stop();
                    lock (_clientsLock)
                    {
                        foreach (var client in _clients)
                            client.Disconnect();
                        _clients.Clear();
                    }
                    StopWorkers();
                }
                catch { }
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
                        connection.OnJournalPacketReceived += EnqueueJournalPacket;
                        connection.OnDisconnected += HandleClientDisconnected;
                        lock (_clientsLock) _clients.Add(connection);
                        connection.Start();
                        Log("New client connected: " + tcpClient.Client.RemoteEndPoint.ToString());
                    }
                    catch (SocketException)
                    {
                        if (!_isRunning) break;
                    }
                }
            }
    
    
            private void EnqueueJournalPacket(IncomingJournalPacket packet)
            {
                _journalQueue.Enqueue(packet);
                _journalSignal.Set();
                OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                {
                    SyncId = packet.SyncId,
                    ATM_ID = packet.ATMId,
                    FileName = packet.FileName,
                    FileSize = packet.Payload == null ? 0 : packet.Payload.Length,
                    Checksum = packet.Checksum,
                    State = JournalSyncState.Syncing,
                    ProgressPercent = 60,
                    Message = "Journal received in server queue"
                });
            }
    
    
            private void StoreJournalPacket(IncomingJournalPacket packet)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(packet.SyncId))
                        packet.SyncId = Guid.NewGuid().ToString("N");
    
                    string existingPath;
                    if (TryGetStoredJournal(packet.SyncId, out existingPath))
                    {
                        string existingFileName = Path.GetFileName(existingPath);
                        Log("Duplicate journal sync acknowledged without re-writing: " + packet.SyncId);
                        packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_ACK, packet.SyncId, "STORED", existingFileName));
                        OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                        {
                            SyncId = packet.SyncId,
                            ATM_ID = packet.ATMId,
                            FileName = existingFileName,
                            FileSize = packet.Payload == null ? 0 : packet.Payload.Length,
                            Checksum = packet.Checksum,
                            State = JournalSyncState.Acknowledged,
                            ProgressPercent = 100,
                            ServerPath = existingPath,
                            Message = "Duplicate resend acknowledged from server index"
                        });
                        return;
                    }
    
                    byte[] rawData = packet.Payload ?? new byte[0];
                    if (!string.IsNullOrWhiteSpace(packet.Checksum))
                    {
                        string actualChecksum = ComputeMD5(rawData);
                        if (!string.Equals(actualChecksum, packet.Checksum, StringComparison.OrdinalIgnoreCase))
                        {
                            string mismatch = "Checksum mismatch. Expected=" + packet.Checksum + ", Actual=" + actualChecksum;
                            Log("Rejected journal from " + packet.ATMId + ": " + mismatch);
                            OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                            {
                                SyncId = packet.SyncId,
                                ATM_ID = packet.ATMId,
                                FileName = packet.FileName,
                                FileSize = rawData.Length,
                                Checksum = packet.Checksum,
                                State = JournalSyncState.Failed,
                                ProgressPercent = 0,
                                Message = mismatch
                            });
                            packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_NACK, packet.SyncId, mismatch));
                            return;
                        }
                    }
    
                    string atmDir = Path.Combine(_storagePath, packet.ATMId, DateTime.UtcNow.ToString("yyyy-MM"));
                    if (!Directory.Exists(atmDir))
                        Directory.CreateDirectory(atmDir);
    
                    string fileName = SanitizeFileName(string.IsNullOrWhiteSpace(packet.FileName)
                        ? "EJ_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".dat"
                        : packet.FileName);
                    string storedFileName = BuildStoredJournalFileName(fileName, packet.SyncId);
                    string filePath = Path.Combine(atmDir, storedFileName);
                    File.WriteAllBytes(filePath, rawData);
                    RememberStoredJournal(packet.SyncId, filePath);
    
                    Log("Stored " + rawData.Length + " bytes from " + packet.ATMId + " -> " + storedFileName);
                    OnClientStatusChanged?.Invoke(packet.ATMId, true);
                    OnJournalStored?.Invoke(packet.ATMId, storedFileName, filePath);
                    OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                    {
                        SyncId = packet.SyncId,
                        ATM_ID = packet.ATMId,
                        FileName = storedFileName,
                        FileSize = rawData.Length,
                        Checksum = packet.Checksum,
                        State = JournalSyncState.StoredOnServer,
                        ProgressPercent = 100,
                        ServerPath = filePath,
                        Message = "Journal stored on server"
                    });
                    packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_ACK, packet.SyncId, "STORED", storedFileName));
                }
                catch (Exception ex)
                {
                    Log("Error processing data from " + packet.ATMId + ": " + ex.Message);
                    OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                    {
                        SyncId = packet.SyncId,
                        ATM_ID = packet.ATMId,
                        FileName = packet.FileName,
                        FileSize = packet.Payload == null ? 0 : packet.Payload.Length,
                        Checksum = packet.Checksum,
                        State = JournalSyncState.Failed,
                        ProgressPercent = 0,
                        Message = ex.Message
                    });
                    packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_NACK, packet.SyncId, ex.Message));
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
                lock (_clientsLock) client = _clients.Find(c => c.ATMID == atmId);
                if (client != null && client.IsConnected)
                {
                    string protocolCommand = MapCommand(command);
                    client.SendLine(protocolCommand);
                    Log("Command sent to " + atmId + ": " + protocolCommand);
                }
                else
                {
                    Log("Cannot send command: " + atmId + " not connected.");
                }
            }
    
    
            private void Log(string message)
            {
                OnLogMessage?.Invoke("[Server] " + message);
            }
    
    
            public List<ClientConnectionInfo> GetClients()
            {
                lock (_clientsLock)
                {
                    var result = new List<ClientConnectionInfo>();
                    foreach (var client in _clients)
                    {
                        result.Add(new ClientConnectionInfo
                        {
                            ATMID = client.ATMID,
                            RemoteEndPoint = client.RemoteEndPoint,
                            ATMType = client.ATMType,
                            IsConnected = client.IsConnected,
                            LastHeartbeatUtc = client.LastHeartbeatUtc,
                            LastSyncUtc = client.LastSyncUtc
                        });
                    }
                    return result;
                }
            }
    
    
            private void StartWorkers()
            {
                _workersRunning = true;
                int workerCount = Math.Max(2, Environment.ProcessorCount / 2);
                for (int i = 0; i < workerCount; i++)
                {
                    var worker = new Thread(JournalWorkerLoop) { IsBackground = true, Name = "EJServerJournalWorker" + i };
                    _workerThreads.Add(worker);
                    worker.Start();
                }
                Log("Journal workers started: " + workerCount);
            }
    
    
            private void StopWorkers()
            {
                _workersRunning = false;
                _journalSignal.Set();
            }
    
    
            private void JournalWorkerLoop()
            {
                while (_workersRunning)
                {
                    try
                    {
                        IncomingJournalPacket packet;
                        if (_journalQueue.TryDequeue(out packet))
                        {
                            StoreJournalPacket(packet);
                        }
                        else
                        {
                            _journalSignal.WaitOne(500);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Worker error: " + ex.Message);
                    }
                }
            }
    
    
            private void HandleClientDisconnected(ClientConnection connection)
            {
                lock (_clientsLock) _clients.Remove(connection);
                if (!string.IsNullOrEmpty(connection.ATMID))
                    OnClientStatusChanged?.Invoke(connection.ATMID, false);
            }
    
    
            private static string SanitizeFileName(string fileName)
            {
                foreach (var c in Path.GetInvalidFileNameChars())
                    fileName = fileName.Replace(c, '_');
                return fileName;
            }
    
    
            private static string ComputeMD5(byte[] data)
            {
                using (var md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(data ?? new byte[0]);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
    
    
            private void LoadJournalIndex()
            {
                lock (_journalIndexLock)
                {
                    _storedSyncIndex.Clear();
                    if (string.IsNullOrWhiteSpace(_journalIndexPath) || !File.Exists(_journalIndexPath))
                        return;
    
                    foreach (string line in File.ReadAllLines(_journalIndexPath, Encoding.UTF8))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] parts = line.Split('\t');
                        if (parts.Length < 2) continue;
                        _storedSyncIndex[parts[0]] = parts[1];
                    }
                }
            }
    
    
            private bool TryGetStoredJournal(string syncId, out string serverPath)
            {
                serverPath = null;
                if (string.IsNullOrWhiteSpace(syncId)) return false;
                lock (_journalIndexLock)
                {
                    if (_storedSyncIndex.TryGetValue(syncId, out serverPath))
                        return !string.IsNullOrWhiteSpace(serverPath) && File.Exists(serverPath);
                }
                return false;
            }
    
    
            private void RememberStoredJournal(string syncId, string serverPath)
            {
                if (string.IsNullOrWhiteSpace(syncId) || string.IsNullOrWhiteSpace(serverPath)) return;
                lock (_journalIndexLock)
                {
                    _storedSyncIndex[syncId] = serverPath;
                    string line = syncId + "\t" + serverPath + "\t" + DateTime.UtcNow.ToString("O");
                    File.AppendAllText(_journalIndexPath, line + Environment.NewLine, Encoding.UTF8);
                }
            }
    
    
            private static string BuildStoredJournalFileName(string fileName, string syncId)
            {
                fileName = SanitizeFileName(fileName);
                string name = Path.GetFileNameWithoutExtension(fileName);
                string ext = Path.GetExtension(fileName);
                string shortSyncId = string.IsNullOrWhiteSpace(syncId) ? Guid.NewGuid().ToString("N").Substring(0, 8) : syncId.Substring(0, Math.Min(8, syncId.Length));
                return name + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff") + "_" + shortSyncId + ext;
            }
    
    
            private static string MapCommand(string command)
            {
                switch ((command ?? string.Empty).ToUpperInvariant())
                {
                    case "RESTART":
                    case "CMD_RESTART":
                        return Protocol.CMD_RESTART;
                    case "SCREENSHOT":
                    case "CMD_SCREENSHOT":
                        return Protocol.CMD_SCREENSHOT;
                    case "TIMESYNC":
                    case "CMD_TIMESYNC":
                        return Protocol.CMD_TIMESYNC;
                    case "SHUTDOWN":
                    case "CMD_SHUTDOWN":
                        return Protocol.CMD_SHUTDOWN;
                    case "IMAGE_SYNC":
                    case "CMD_IMAGE_SYNC":
                        return Protocol.CMD_IMAGE_SYNC;
                    case "FORCE_SYNC":
                    case "CMD_FORCE_SYNC":
                        return Protocol.CMD_FORCE_SYNC;
                    case "GHOST_START":
                    case "CMD_GHOST_START":
                        return Protocol.CMD_GHOST_START;
                    case "GHOST_STOP":
                    case "CMD_GHOST_STOP":
                        return Protocol.CMD_GHOST_STOP;
                    case "GET_SYSINFO":
                    case "SYSINFO":
                    case "CMD_SYSINFO":
                        return Protocol.CMD_GET_SYSINFO;
                    default:
                        return command;
                }
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Menus_ai\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Server.WinForms\Services\EJServer.cs
            private readonly object _journalIndexLock = new object();
    
    
            public event Action<string> OnLogMessage;
    
    
            public event Action<string, bool> OnClientStatusChanged;
    
    
            public event Action<JournalSyncRecord> OnJournalSyncChanged;
    
    
            public event Action<string, string, string> OnJournalStored;
    
    
        }
    public partial class EJServer
        {
            private TcpListener _listener;
            private bool _isRunning;
            private int _port;
            private string _storagePath;
            private List<ClientConnection> _clients;
            private Thread _listenThread;
            private readonly ConcurrentQueue<IncomingJournalPacket> _journalQueue;
            private readonly AutoResetEvent _journalSignal;
            private readonly List<Thread> _workerThreads;
            private readonly Dictionary<string, string> _storedSyncIndex;
            private bool _workersRunning;
            private string _journalIndexPath;
            private MonitoringStateStore _monitoringStore;
            private JournalSyncService _journalSyncService;
            public EJServer(int port, string storagePath)
            {
                _port = port;
                _storagePath = storagePath;
                _clients = new List<ClientConnection>();
                _journalQueue = new ConcurrentQueue<IncomingJournalPacket>();
                _journalSignal = new AutoResetEvent(false);
                _workerThreads = new List<Thread>();
                _storedSyncIndex = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            public EJServer(int port, string storagePath)
            {
                _port = port;
                _storagePath = storagePath;
                _clients = new List<ClientConnection>();
            }
            public EJServer(string storagePath)
            {
                _storagePath = storagePath;
                if (!Directory.Exists(_storagePath)) Directory.CreateDirectory(_storagePath);
            }
            private readonly object _clientsLock = new object();
            private readonly object _clientsSync = new object();
            public void Start()
            {
                _isRunning = true;
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
                _listenThread = new Thread(ListenLoop);
                _listenThread.IsBackground = true;
                _listenThread.Start();
                Log("Server started on port " + _port);
                Log("Storage path: " + _storagePath);
                if (!Directory.Exists(_storagePath))
                    Directory.CreateDirectory(_storagePath);
                _journalIndexPath = Path.Combine(_storagePath, "_journal_sync_index.tsv");
                LoadJournalIndex();
                StartWorkers();
            }
            public void Stop()
            {
                _isRunning = false;
                try
                {
                    _listener?.Stop();
                    lock (_clientsLock)
                    {
                        foreach (var client in _clients)
                            client.Disconnect();
                        _clients.Clear();
                    }
                    StopWorkers();
                }
                catch { }
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
                        connection.OnJournalPacketReceived += EnqueueJournalPacket;
                        connection.OnDisconnected += HandleClientDisconnected;
                        lock (_clientsLock) _clients.Add(connection);
                        connection.Start();
                        Log("New client connected: " + tcpClient.Client.RemoteEndPoint.ToString());
                    }
                    catch (SocketException)
                    {
                        if (!_isRunning) break;
                    }
                }
            }
            private void EnqueueJournalPacket(IncomingJournalPacket packet)
            {
                _journalQueue.Enqueue(packet);
                _journalSignal.Set();
                OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                {
                    SyncId = packet.SyncId,
                    ATM_ID = packet.ATMId,
                    FileName = packet.FileName,
                    FileSize = packet.Payload == null ? 0 : packet.Payload.Length,
                    Checksum = packet.Checksum,
                    State = JournalSyncState.Syncing,
                    ProgressPercent = 60,
                    Message = "Journal received in server queue"
                });
            }
            private void StoreJournalPacket(IncomingJournalPacket packet)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(packet.SyncId))
                        packet.SyncId = Guid.NewGuid().ToString("N");
                    string existingPath;
                    if (TryGetStoredJournal(packet.SyncId, out existingPath))
                    {
                        string existingFileName = Path.GetFileName(existingPath);
                        Log("Duplicate journal sync acknowledged without re-writing: " + packet.SyncId);
                        packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_ACK, packet.SyncId, "STORED", existingFileName));
                        OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                        {
                            SyncId = packet.SyncId,
                            ATM_ID = packet.ATMId,
                            FileName = existingFileName,
                            FileSize = packet.Payload == null ? 0 : packet.Payload.Length,
                            Checksum = packet.Checksum,
                            State = JournalSyncState.Acknowledged,
                            ProgressPercent = 100,
                            ServerPath = existingPath,
                            Message = "Duplicate resend acknowledged from server index"
                        });
                        return;
                    }
                    byte[] rawData = packet.Payload ?? new byte[0];
                    if (!string.IsNullOrWhiteSpace(packet.Checksum))
                    {
                        string actualChecksum = ComputeMD5(rawData);
                        if (!string.Equals(actualChecksum, packet.Checksum, StringComparison.OrdinalIgnoreCase))
                        {
                            string mismatch = "Checksum mismatch. Expected=" + packet.Checksum + ", Actual=" + actualChecksum;
                            Log("Rejected journal from " + packet.ATMId + ": " + mismatch);
                            OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                            {
                                SyncId = packet.SyncId,
                                ATM_ID = packet.ATMId,
                                FileName = packet.FileName,
                                FileSize = rawData.Length,
                                Checksum = packet.Checksum,
                                State = JournalSyncState.Failed,
                                ProgressPercent = 0,
                                Message = mismatch
                            });
                            packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_NACK, packet.SyncId, mismatch));
                            return;
                        }
                    }
                    string atmDir = Path.Combine(_storagePath, packet.ATMId, DateTime.UtcNow.ToString("yyyy-MM"));
                    if (!Directory.Exists(atmDir))
                        Directory.CreateDirectory(atmDir);
                    string fileName = SanitizeFileName(string.IsNullOrWhiteSpace(packet.FileName)
                        ? "EJ_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".dat"
                        : packet.FileName);
                    string storedFileName = BuildStoredJournalFileName(fileName, packet.SyncId);
                    string filePath = Path.Combine(atmDir, storedFileName);
                    File.WriteAllBytes(filePath, rawData);
                    RememberStoredJournal(packet.SyncId, filePath);
                    Log("Stored " + rawData.Length + " bytes from " + packet.ATMId + " -> " + storedFileName);
                    OnClientStatusChanged?.Invoke(packet.ATMId, true);
                    OnJournalStored?.Invoke(packet.ATMId, storedFileName, filePath);
                    OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                    {
                        SyncId = packet.SyncId,
                        ATM_ID = packet.ATMId,
                        FileName = storedFileName,
                        FileSize = rawData.Length,
                        Checksum = packet.Checksum,
                        State = JournalSyncState.StoredOnServer,
                        ProgressPercent = 100,
                        ServerPath = filePath,
                        Message = "Journal stored on server"
                    });
                    packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_ACK, packet.SyncId, "STORED", storedFileName));
                }
                catch (Exception ex)
                {
                    Log("Error processing data from " + packet.ATMId + ": " + ex.Message);
                    OnJournalSyncChanged?.Invoke(new JournalSyncRecord
                    {
                        SyncId = packet.SyncId,
                        ATM_ID = packet.ATMId,
                        FileName = packet.FileName,
                        FileSize = packet.Payload == null ? 0 : packet.Payload.Length,
                        Checksum = packet.Checksum,
                        State = JournalSyncState.Failed,
                        ProgressPercent = 0,
                        Message = ex.Message
                    });
                    packet.SourceConnection?.SendLine(Protocol.BuildMessage(Protocol.DATA_NACK, packet.SyncId, ex.Message));
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
                lock (_clientsLock) client = _clients.Find(c => c.ATMID == atmId);
                if (client != null && client.IsConnected)
                {
                    string protocolCommand = MapCommand(command);
                    client.SendLine(protocolCommand);
                    Log("Command sent to " + atmId + ": " + protocolCommand);
                }
                else
                {
                    Log("Cannot send command: " + atmId + " not connected.");
                }
            }
            private void Log(string message)
            {
                OnLogMessage?.Invoke("[Server] " + message);
            }
            public List<ClientConnectionInfo> GetClients()
            {
                lock (_clientsLock)
                {
                    var result = new List<ClientConnectionInfo>();
                    foreach (var client in _clients)
                    {
                        result.Add(new ClientConnectionInfo
                        {
                            ATMID = client.ATMID,
                            RemoteEndPoint = client.RemoteEndPoint,
                            ATMType = client.ATMType,
                            IsConnected = client.IsConnected,
                            LastHeartbeatUtc = client.LastHeartbeatUtc,
                            LastSyncUtc = client.LastSyncUtc
                        });
                    }
                    return result;
                }
            }
            private void StartWorkers()
            {
                _workersRunning = true;
                int workerCount = Math.Max(2, Environment.ProcessorCount / 2);
                for (int i = 0; i < workerCount; i++)
                {
                    var worker = new Thread(JournalWorkerLoop) { IsBackground = true, Name = "EJServerJournalWorker" + i };
                    _workerThreads.Add(worker);
                    worker.Start();
                }
                Log("Journal workers started: " + workerCount);
            }
            private void StopWorkers()
            {
                _workersRunning = false;
                _journalSignal.Set();
            }
            private void JournalWorkerLoop()
            {
                while (_workersRunning)
                {
                    try
                    {
                        IncomingJournalPacket packet;
                        if (_journalQueue.TryDequeue(out packet))
                        {
                            StoreJournalPacket(packet);
                        }
                        else
                        {
                            _journalSignal.WaitOne(500);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Worker error: " + ex.Message);
                    }
                }
            }
            private void HandleClientDisconnected(ClientConnection connection)
            {
                lock (_clientsLock) _clients.Remove(connection);
                if (!string.IsNullOrEmpty(connection.ATMID))
                    OnClientStatusChanged?.Invoke(connection.ATMID, false);
            }
            private static string SanitizeFileName(string fileName)
            {
                foreach (var c in Path.GetInvalidFileNameChars())
                    fileName = fileName.Replace(c, '_');
                return fileName;
            }
            private static string ComputeMD5(byte[] data)
            {
                using (var md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(data ?? new byte[0]);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
            private void LoadJournalIndex()
            {
                lock (_journalIndexLock)
                {
                    _storedSyncIndex.Clear();
                    if (string.IsNullOrWhiteSpace(_journalIndexPath) || !File.Exists(_journalIndexPath))
                        return;
                    foreach (string line in File.ReadAllLines(_journalIndexPath, Encoding.UTF8))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] parts = line.Split('\t');
                        if (parts.Length < 2) continue;
                        _storedSyncIndex[parts[0]] = parts[1];
                    }
                }
            }
            private bool TryGetStoredJournal(string syncId, out string serverPath)
            {
                serverPath = null;
                if (string.IsNullOrWhiteSpace(syncId)) return false;
                lock (_journalIndexLock)
                {
                    if (_storedSyncIndex.TryGetValue(syncId, out serverPath))
                        return !string.IsNullOrWhiteSpace(serverPath) && File.Exists(serverPath);
                }
                return false;
            }
            private void RememberStoredJournal(string syncId, string serverPath)
            {
                if (string.IsNullOrWhiteSpace(syncId) || string.IsNullOrWhiteSpace(serverPath)) return;
                lock (_journalIndexLock)
                {
                    _storedSyncIndex[syncId] = serverPath;
                    string line = syncId + "\t" + serverPath + "\t" + DateTime.UtcNow.ToString("O");
                    File.AppendAllText(_journalIndexPath, line + Environment.NewLine, Encoding.UTF8);
                }
            }
            private static string BuildStoredJournalFileName(string fileName, string syncId)
            {
                fileName = SanitizeFileName(fileName);
                string name = Path.GetFileNameWithoutExtension(fileName);
                string ext = Path.GetExtension(fileName);
                string shortSyncId = string.IsNullOrWhiteSpace(syncId) ? Guid.NewGuid().ToString("N").Substring(0, 8) : syncId.Substring(0, Math.Min(8, syncId.Length));
                return name + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff") + "_" + shortSyncId + ext;
            }
            private static string MapCommand(string command)
            {
                switch ((command ?? string.Empty).ToUpperInvariant())
                {
                    case "RESTART":
                    case "CMD_RESTART":
                        return Protocol.CMD_RESTART;
                    case "SCREENSHOT":
                    case "CMD_SCREENSHOT":
                        return Protocol.CMD_SCREENSHOT;
                    case "TIMESYNC":
                    case "CMD_TIMESYNC":
                        return Protocol.CMD_TIMESYNC;
                    case "SHUTDOWN":
                    case "CMD_SHUTDOWN":
                        return Protocol.CMD_SHUTDOWN;
                    case "IMAGE_SYNC":
                    case "CMD_IMAGE_SYNC":
                        return Protocol.CMD_IMAGE_SYNC;
                    case "FORCE_SYNC":
                    case "CMD_FORCE_SYNC":
                        return Protocol.CMD_FORCE_SYNC;
                    case "GHOST_START":
                    case "CMD_GHOST_START":
                        return Protocol.CMD_GHOST_START;
                    case "GHOST_STOP":
                    case "CMD_GHOST_STOP":
                        return Protocol.CMD_GHOST_STOP;
                    case "GET_SYSINFO":
                    case "SYSINFO":
                    case "CMD_SYSINFO":
                        return Protocol.CMD_GET_SYSINFO;
                    default:
                        return command;
                }
            }
            public void Start()
            {
                _isRunning = true;
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
                _listenThread = new Thread(ListenLoop);
                _listenThread.IsBackground = true;
                _listenThread.Start();
                Log("Server started on port " + _port);
                Log("Storage path: " + _storagePath);
                if (!Directory.Exists(_storagePath))
                    Directory.CreateDirectory(_storagePath);
            }
            public void Stop()
            {
                _isRunning = false;
                _listener.Stop();
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
                        _clients.Add(connection);
                        connection.Start();
                        Log("New client connected: " + tcpClient.Client.RemoteEndPoint.ToString());
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
            public void SendCommand(string atmId, string command, string parameters)
            {
                var client = _clients.Find(c => c.ATMID == atmId);
                if (client != null && client.IsConnected)
                {
                    string cmdPacket = "CMD|" + command + "|" + parameters;
                    client.SendData(Encoding.UTF8.GetBytes(cmdPacket));
                    Log("Command sent to " + atmId + ": " + command);
                }
                else
                {
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
            private Dictionary<string, ATMInfo> _activeATMs = new Dictionary<string, ATMInfo>();
            public void Start(int port)
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                _isRunning = true;
                new Thread(ListenLoop).Start();
            }
            private void HandleClient(TcpClient client)
            {
                string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[8192];
                    while (client.Connected && _isRunning)
                    {
                        try
                        {
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0) break;
                            byte[] data = new byte[bytesRead];
                            Array.Copy(buffer, data, bytesRead);
                            byte[] decrypted = SecurityHelper.Decrypt(data);
                            byte[] decompressed = SecurityHelper.Decompress(decrypted);
                            string content = Encoding.UTF8.GetString(decompressed);
                            OnDataReceived?.Invoke(clientIP, "EJ_DATA", content);
                        }
                        catch { break; }
                    }
                }
            }
            private void HandleDataReceived(string atmId, byte[] encryptedData)
            {
                try
                {
                    // Step 1: Decrypt
                    byte[] compressed = DecryptAES256(encryptedData);
                    // Step 2: Decompress
                    byte[] rawData = DecompressData(compressed);
                    // Step 3: Store
                    string atmDir = Path.Combine(_storagePath, atmId, DateTime.Now.ToString("yyyy-MM"));
                    if (!Directory.Exists(atmDir))
                        Directory.CreateDirectory(atmDir);
                    string fileName = "EJ_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".dat";
                    string filePath = Path.Combine(atmDir, fileName);
                    File.WriteAllBytes(filePath, rawData);
                    Log("Stored " + rawData.Length + " bytes from " + atmId + " -> " + fileName);
                    OnClientStatusChanged?.Invoke(atmId, true);
                }
                catch (Exception ex)
                {
                    Log("Error processing data from " + atmId + ": " + ex.Message);
                }
            }
            private void HandleCommandResultReceived(string atmId, string commandId, string status, string details)
            {
                string effectiveAtmId = string.IsNullOrWhiteSpace(atmId) ? "Unknown" : atmId;
                string effectiveStatus = string.IsNullOrWhiteSpace(status) ? "UNKNOWN" : status;
                string message = "Command result [" + effectiveStatus + "] " + commandId + ": " + (details ?? string.Empty);
                _monitoringStore.RecordAlert(effectiveAtmId, string.Equals(effectiveStatus, "FAILED", StringComparison.OrdinalIgnoreCase) ? "Warning" : "Info", message);
                Log(message);
            }
            private List<ATMInfo> _connectedATMs = new List<ATMInfo>();
            public void Start(int port)
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                _isRunning = true;
                new Thread(ListenForClients).Start();
                OnLogReceived?.Invoke("SYSTEM", $"Server started on port {port}");
            }
            private void ListenForClients()
            {
                while (_isRunning)
                {
                    try
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        new Thread(() => HandleClient(client)).Start();
                    }
                    catch { }
                }
            }
            private void HandleClient(TcpClient client)
            {
                string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                OnLogReceived?.Invoke("NET", $"New connection from {clientIP}");
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[4096];
                    while (client.Connected)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;
                        // Logic to decrypt, decompress and process data
                        // Update database and notify monitoring UI
                    }
                }
                client.Close();
            }
            public event Action<string> OnLogMessage;
            public event Action<string, bool> OnClientStatusChanged;
            public event Action<JournalSyncRecord> OnJournalSyncChanged;
            public event Action<string, string, string> OnJournalStored;
            public event Action<ATMInfo> OnATMStatusChanged;
            public event Action<string, string> OnLogReceived;
        }
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
                Log("Client handshake received from " + atmId + " @" + remoteEndpoint);
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
    // ═══ Class: EJServer (from 5 sources) ═══
        public partial class EJServer
        {
            // --- Constants & Fields ---
                    private TcpListener _listener;
    
                    private bool _isRunning;
    
                    private int _port;
    
                    private string _storagePath;
    
                    private List<ClientConnection> _clients;
    
                    private Thread _listenThread;
    
                    private MonitoringStateStore _monitoringStore;
    
                    private JournalSyncService _journalSyncService;
    
    
            // --- Constructors ---
                    public EJServer(int port, string storagePath)
                    {
                        _port = port;
                        _storagePath = storagePath;
                        _clients = new List<ClientConnection>();
                    }
    
                    public EJServer(string storagePath)
                    {
                        _storagePath = storagePath;
                        if (!Directory.Exists(_storagePath)) Directory.CreateDirectory(_storagePath);
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
                    public EJServer(int port, string storagePath)
                    {
                        _port = port;
                        _storagePath = storagePath;
                        _clients = new List<ClientConnection>();
                        _monitoringStore = new MonitoringStateStore(storagePath);
                        _journalSyncService = new JournalSyncService(storagePath);
                    }
    
    
            // --- Methods ---
                    public void Start()
                    {
                        _isRunning = true;
                        _listener = new TcpListener(IPAddress.Any, _port);
                        _listener.Start();
    
                        _listenThread = new Thread(ListenLoop);
                        _listenThread.IsBackground = true;
                        _listenThread.Start();
    
                        Log("Server started on port " + _port);
                        Log("Storage path: " + _storagePath);
    
                        if (!Directory.Exists(_storagePath))
                            Directory.CreateDirectory(_storagePath);
                    }
    
                    public void Stop()
                    {
                        _isRunning = false;
                        try
                        {
                            _listener?.Stop();
                            foreach (var client in _clients)
                                client.Disconnect();
                            _clients.Clear();
                        }
                        catch { }
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
                                _clients.Add(connection);
                                connection.Start();
                                Log("New client connected: " + tcpClient.Client.RemoteEndPoint.ToString());
                            }
                            catch (SocketException)
                            {
                                if (!_isRunning) break;
                            }
                        }
                    }
    
                    private void HandleDataReceived(string atmId, byte[] encryptedData)
                    {
                        try
                        {
                            // Step 1: Decrypt
                            byte[] compressed = DecryptAES256(encryptedData);
    
                            // Step 2: Decompress
                            byte[] rawData = DecompressData(compressed);
    
                            // Step 3: Store
                            string atmDir = Path.Combine(_storagePath, atmId, DateTime.Now.ToString("yyyy-MM"));
                            if (!Directory.Exists(atmDir))
                                Directory.CreateDirectory(atmDir);
    
                            string fileName = "EJ_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".dat";
                            string filePath = Path.Combine(atmDir, fileName);
                            File.WriteAllBytes(filePath, rawData);
    
                            Log("Stored " + rawData.Length + " bytes from " + atmId + " -> " + fileName);
                            OnClientStatusChanged?.Invoke(atmId, true);
                        }
                        catch (Exception ex)
                        {
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
                        var client = _clients.Find(c => c.ATMID == atmId);
                        if (client != null && client.IsConnected)
                        {
                            string cmdPacket = "CMD|" + command + "|" + parameters;
                            client.SendData(Encoding.UTF8.GetBytes(cmdPacket));
                            Log("Command sent to " + atmId + ": " + command);
                        }
                        else
                        {
                            Log("Cannot send command: " + atmId + " not connected.");
                        }
                    }
    
                    private void Log(string message)
                    {
                        OnLogMessage?.Invoke("[Server] " + message);
                    }
    
                    private Dictionary<string, ATMInfo> _activeATMs = new Dictionary<string, ATMInfo>();
    
                    public void Start(int port)
                    {
                        _listener = new TcpListener(IPAddress.Any, port);
                        _listener.Start();
                        _isRunning = true;
                        new Thread(ListenLoop).Start();
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_WinForms_v3.2.1\EJLive.Server.WinForms\Services\EJServer.cs
                    private void ListenLoop()
                    {
                        while (_isRunning)
                        {
                            try
                            {
                                TcpClient client = _listener.AcceptTcpClient();
                                new Thread(() => HandleClient(client)).Start();
                            }
                            catch { }
                        }
                    }
    
                    private void HandleClient(TcpClient client)
                    {
                        string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                        using (NetworkStream stream = client.GetStream())
                        {
                            byte[] buffer = new byte[8192];
                            while (client.Connected && _isRunning)
                            {
                                try
                                {
                                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                                    if (bytesRead == 0) break;
    
                                    byte[] data = new byte[bytesRead];
                                    Array.Copy(buffer, data, bytesRead);
    
                                    byte[] decrypted = SecurityHelper.Decrypt(data);
                                    byte[] decompressed = SecurityHelper.Decompress(decrypted);
    
                                    string content = Encoding.UTF8.GetString(decompressed);
                                    OnDataReceived?.Invoke(clientIP, "EJ_DATA", content);
                                }
                                catch { break; }
                            }
                        }
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_WinForms_v3.2.1\EJLive.Server.WinForms\Services\EJServer.cs
                    public void Stop() => _isRunning = false;
    
                    private readonly object _clientsSync = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
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
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
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
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
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
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
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
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Server.WinForms\Services\EJServer.cs
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
    
    
            // --- Events ---
                    public event Action<string> OnLogMessage;
    
                    public event Action<string, bool> OnClientStatusChanged;
    
                    public event Action<ATMInfo> OnATMStatusChanged;
    
                    public event Action<string, string, string> OnDataReceived;
    
    
        }
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
                        connection.OnCommandResultReceived += HandleCommandResultReceived;
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
    
            private void HandleCommandResultReceived(string atmId, string commandId, string status, string details)
            {
                string effectiveAtmId = string.IsNullOrWhiteSpace(atmId) ? "Unknown" : atmId;
                string effectiveStatus = string.IsNullOrWhiteSpace(status) ? "UNKNOWN" : status;
                string message = "Command result [" + effectiveStatus + "] " + commandId + ": " + (details ?? string.Empty);
    
                _monitoringStore.RecordAlert(effectiveAtmId, string.Equals(effectiveStatus, "FAILED", StringComparison.OrdinalIgnoreCase) ? "Warning" : "Info", message);
                Log(message);
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
                    string commandId = Guid.NewGuid().ToString("N");
                    string cmdPacket = "CMD|" + commandId + "|" + command + "|" + parameters;
                    client.SendData(Encoding.UTF8.GetBytes(cmdPacket + "\n"));
                    _monitoringStore.RecordAlert(atmId, "Info", "Command queued: " + command + " [" + commandId + "]");
                    Log("Command sent to " + atmId + ": " + command + " [" + commandId + "]");
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
    // Class: EJServerService (from 3 sources)
        public sealed partial class EJServerService : IDisposable
        {
            // --- Constants & Fields ---
                    private readonly ServerEngine    _serverEngine;
    
                    private readonly ArchiveManager  _archiveManager;
    
                    private readonly ImageSyncEngine _imageSync;
    
                    private readonly JournalSyncTracker _syncTracker;
    
                    private bool _isRunning;
    
                    private readonly int _port;
    
    
            // --- Properties ---
                    public int    ConnectedATMs        => _serverEngine.ConnectedCount;
    
                    public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
    
                    public int    TotalJournalsArchived { get; private set; }
    
                    public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
    
                    public bool   IsRunning            => _isRunning;
    
                    public DateTime StartedAt          => _serverEngine.StartedAt;
    
    
            // --- Constructors ---
                    public EJServerService(int port = AppConstants.DefaultPort, string archivePath = null)
                    {
                        _port           = port;
                        _serverEngine   = new ServerEngine(port);
                        _archiveManager = new ArchiveManager(archivePath);
                        _imageSync      = new ImageSyncEngine();
                        _syncTracker    = new JournalSyncTracker();
    
                        WireEvents();
                    }
    
    
            // --- Methods ---
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
    
                    public IEnumerable<ATMInfo>     GetConnectedATMs()   => _serverEngine.GetConnectedATMs();
    
                    public ATMInfo                  GetATM(string atmId) => _serverEngine.GetATMByID(atmId);
    
                    public (long files, long bytes) GetArchiveStats(string atmId)
                        => _archiveManager.GetATMArchiveStats(atmId);
    
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
    
    
            // --- Events ---
                    public event EventHandler<ATMInfo>      OnATMConnected;
    
                    public event EventHandler<ATMInfo>      OnATMDisconnected;
    
                    public event EventHandler<ATMInfo>      OnATMUpdated;
    
                    public event EventHandler<AlertPayload> OnAlert;
    
                    public event EventHandler<RemoteCommand> OnCommandChanged;
    
                    public event EventHandler<string>       OnLog;
    
    
        }
    // Class: EJServerService (from 5 sources)
        public sealed partial class EJServerService : IDisposable
        {
            // --- Constants & Fields ---
                    private readonly ServerEngine    _serverEngine;
    
                    private readonly ArchiveManager  _archiveManager;
    
                    private readonly ImageSyncEngine _imageSync;
    
                    private readonly JournalSyncTracker _syncTracker;
    
                    private bool _isRunning;
    
                    private readonly int _port;
    
    
            // --- Properties ---
                    public int    ConnectedATMs        => _serverEngine.ConnectedCount;
    
                    public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
    
                    public int    TotalJournalsArchived { get; private set; }
    
                    public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
    
                    public bool   IsRunning            => _isRunning;
    
                    public DateTime StartedAt          => _serverEngine.StartedAt;
    
    
            // --- Constructors ---
                    public EJServerService(int port = AppConstants.DefaultPort, string archivePath = null)
                    {
                        _port           = port;
                        _serverEngine   = new ServerEngine(port);
                        _archiveManager = new ArchiveManager(archivePath);
                        _imageSync      = new ImageSyncEngine();
                        _syncTracker    = new JournalSyncTracker();
    
                        WireEvents();
                    }
    
    
            // --- Methods ---
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
    
                    public IEnumerable<ATMInfo>     GetConnectedATMs()   => _serverEngine.GetConnectedATMs();
    
                    public ATMInfo                  GetATM(string atmId) => _serverEngine.GetATMByID(atmId);
    
                    public (long files, long bytes) GetArchiveStats(string atmId)
                        => _archiveManager.GetATMArchiveStats(atmId);
    
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
    
    
            // --- Events ---
                    public event EventHandler<ATMInfo>      OnATMConnected;
    
                    public event EventHandler<ATMInfo>      OnATMDisconnected;
    
                    public event EventHandler<ATMInfo>      OnATMUpdated;
    
                    public event EventHandler<AlertPayload> OnAlert;
    
                    public event EventHandler<RemoteCommand> OnCommandChanged;
    
                    public event EventHandler<string>       OnLog;
    
    
        }
    // ═══ Class: EJServerService (from 1 sources) ═══
        public sealed partial class EJServerService : IDisposable
        {
            // --- Constants & Fields ---
                    private readonly ServerEngine    _serverEngine;
    
                    private readonly ArchiveManager  _archiveManager;
    
                    private readonly ImageSyncEngine _imageSync;
    
                    private readonly JournalSyncTracker _syncTracker;
    
                    private bool _isRunning;
    
                    private readonly int _port;
    
    
            // --- Properties ---
                    public int    ConnectedATMs        => _serverEngine.ConnectedCount;
    
                    public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
    
                    public int    TotalJournalsArchived { get; private set; }
    
                    public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
    
                    public bool   IsRunning            => _isRunning;
    
                    public DateTime StartedAt          => _serverEngine.StartedAt;
    
    
            // --- Constructors ---
                    public EJServerService(int port = AppConstants.DefaultPort, string archivePath = null)
                    {
                        _port           = port;
                        _serverEngine   = new ServerEngine(port);
                        _archiveManager = new ArchiveManager(archivePath);
                        _imageSync      = new ImageSyncEngine();
                        _syncTracker    = new JournalSyncTracker();
    
                        WireEvents();
                    }
    
    
            // --- Methods ---
                    private void WireEvents()
                    {
                        _serverEngine.OnATMConnected    += (s, a) => OnATMConnected?.Invoke(this, a);
                        _serverEngine.OnATMDisconnected += (s, a) => { OnATMDisconnected?.Invoke(this, a); CheckHealth(a); };
                        _serverEngine.OnATMUpdated      += (s, a) => OnATMUpdated?.Invoke(this, a);
                        _serverEngine.OnJournalReceived += OnJournalReceived;
                        _serverEngine.OnServerLog       += (s, m) => Log(m);
    
                        _archiveManager.OnArchived += (s, p) => Log($"📦 Archived: {p}");
                        _archiveManager.OnError    += (s, e) => Log($"Archive Error: {e}");
    
                        AlertManager.Instance.OnAlert    += (s, a) => OnAlert?.Invoke(this, a);
                        AlertManager.Instance.OnCritical += (s, a) => OnAlert?.Invoke(this, a);
                    }
    
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
    
                    public bool SendCommand(string atmId, string cmdType, string parameters, string sentBy = "System")
                        => _serverEngine.SendCommand(atmId, cmdType, parameters, sentBy);
    
                    public void Broadcast(string message, string sentBy = "System")
                        => _serverEngine.Broadcast(message, sentBy);
    
                    public bool SendImagesToATM(string atmId, string imagesFolder)
                    {
                        // إرسال الصور عبر CMD_SYNC_IMAGES
                        return SendCommand(atmId, AppConstants.CMD_SYNC_IMAGES, imagesFolder, "System");
                    }
    
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
    
                    public IEnumerable<ATMInfo>     GetConnectedATMs()   => _serverEngine.GetConnectedATMs();
    
                    public ATMInfo                  GetATM(string atmId) => _serverEngine.GetATMByID(atmId);
    
                    public (long files, long bytes) GetArchiveStats(string atmId)
                        => _archiveManager.GetATMArchiveStats(atmId);
    
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
    
    
            // --- Events ---
                    public event EventHandler<ATMInfo>      OnATMConnected;
    
                    public event EventHandler<ATMInfo>      OnATMDisconnected;
    
                    public event EventHandler<ATMInfo>      OnATMUpdated;
    
                    public event EventHandler<AlertPayload> OnAlert;
    
                    public event EventHandler<string>       OnLog;
    
    
        }
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
    
            public int    ConnectedATMs        => _serverEngine.ConnectedCount;
            public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
            public int    TotalJournalsArchived { get; private set; }
            public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
            public bool   IsRunning            => _isRunning;
            public DateTime StartedAt          => _serverEngine.StartedAt;
    
            public event EventHandler<ATMInfo>?      OnATMConnected;
            public event EventHandler<ATMInfo>?      OnATMDisconnected;
            public event EventHandler<ATMInfo>?      OnATMUpdated;
            public event EventHandler<AlertPayload>? OnAlert;
            public event EventHandler<RemoteCommand>? OnCommandChanged;
            public event EventHandler<string>?       OnLog;
    
            public EJServerService(int port = AppConstants.DefaultPort, string? archivePath = null)
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
    
                _archiveManager.OnArchived += (s, p) => Log($"Archived: {p}");
                _archiveManager.OnError    += (s, e) => Log($"Archive Error: {e}");
    
                AlertManager.Instance.OnAlert    += (s, a) => OnAlert?.Invoke(this, a);
                AlertManager.Instance.OnCritical += (s, a) => OnAlert?.Invoke(this, a);
            }
    
            public void Start()
            {
                if (_isRunning) return;
                _serverEngine.Start();
                _isRunning = true;
                DatabaseManager.Instance.InsertAuditLog("ServerStart", "System", null, $"EJLive Server started on TCP/{_port}");
                Log($"EJLive Enterprise Server v{AppConstants.AppVersion} started on TCP/{_port}");
            }
    
            public void Stop()
            {
                if (!_isRunning) return;
                _serverEngine.Stop();
                _isRunning = false;
                DatabaseManager.Instance.InsertAuditLog("ServerStop", "System", null, "EJLive Server stopped");
                Log("EJLive Server stopped.");
            }
    
            private void OnJournalReceived(object sender, ReceivedPacket pkt)
            {
                if (pkt.IsGhostFrame || pkt.Data == null || pkt.Data.Length == 0) return;
    
                var thread = new Thread(() =>
                {
                    try
                    {
                        if (DatabaseManager.Instance.IsDuplicateSync(pkt.ATM.ATM_ID, pkt.FileName, pkt.Checksum))
                        {
                            Log($"Idempotency: duplicate skipped {pkt.FileName} from {pkt.ATM.ATM_ID}");
                            return;
                        }
    
                        var archivePath = _archiveManager.Archive(pkt.ATM.ATM_ID, pkt.FileName, pkt.Data, pkt.Checksum, pkt.SHA256);
                        if (archivePath != null) TotalJournalsArchived++;
    
                        var syncId = _syncTracker.AddOrGet(pkt.ATM.ATM_ID, pkt.FileName, pkt.Data.Length, 0, pkt.Checksum)?.SyncId;
                        if (!string.IsNullOrEmpty(syncId))
                        {
                            _syncTracker.MarkCompleted(pkt.ATM.ATM_ID, syncId, pkt.SHA256);
                        }
    
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
                return SendCommand(atmId, AppConstants.CMD_SYNC_IMAGES, imagesFolder, "System");
            }
    
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
    
            public IEnumerable<ATMInfo>     GetConnectedATMs()   => _serverEngine.GetConnectedATMs();
            public ATMInfo?                 GetATM(string atmId) => _serverEngine.GetATMByID(atmId);
    
            public (long files, long bytes) GetArchiveStats(string atmId)
                => _archiveManager.GetATMArchiveStats(atmId);
    
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
    
            public void Broadcast(string message, string sentBy = "System")
                => _serverEngine.Broadcast(message, sentBy);
    
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

    public partial class ClientConnectionInfo
        {
        }
    public partial class IncomingJournalPacket
        {
        }
    public partial class StorageStats
        {
        }
}

namespace EJLive.Server
{
    public partial public public class EJServer
        {
            private TcpListener _listener;
            private bool _isRunning;
            private List<ATMInfo> _connectedATMs = new List<ATMInfo>();
            private readonly object _clientsSync = new object();
            private int _port;
            private string _storagePath;
            private List<ClientConnection> _clients;
            private Thread _listenThread;
            private MonitoringStateStore _monitoringStore;
            private JournalSyncService _journalSyncService;
            private readonly ServerEngine    _serverEngine;
            private readonly ArchiveManager  _archiveManager;
            private readonly ImageSyncEngine _imageSync;
            private readonly JournalSyncTracker _syncTracker;
            public int    ConnectedATMs        => _serverEngine.ConnectedCount;
            public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
            public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
            public bool   IsRunning            => _isRunning;
            public DateTime StartedAt          => _serverEngine.StartedAt;
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private bool _isConnected;
            public EJServer(int port, string storagePath)
            {
            public int    TotalJournalsArchived { get; private set; }
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; }
            public string RemoteEndpoint
            {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            public void Start(int port)
            {
            private void ListenForClients()
            {
            private void HandleClient(TcpClient client)
            {
            public void Stop()
            {
            public void Start()
            {
            private void ListenLoop()
            {
            private void HandleHandshakeReceived(string atmId, string remoteEndpoint)
            {
            private void HandleHeartbeatReceived(string atmId)
            {
            private void HandleMetadataReceived(string atmId, Dictionary<string, string> metadata)
            {
            private void HandleCashStatusReceived(string atmId, Dictionary<string, string> payload)
            {
            private void HandleClientDisconnected(string atmId)
            {
            private void HandleDataReceived(string atmId, byte[] encryptedData)
            {
            private byte[] DecryptAES256(byte[] data)
            {
            private byte[] DecompressData(byte[] data)
            {
            public void SendCommand(string atmId, string command, string parameters)
            {
            private string ComputeChecksum(byte[] data)
            {
            private static int ParseInt(Dictionary<string, string> values, string key)
            {
            private static string GetValue(Dictionary<string, string> values, string key, string fallback)
            {
            private void Log(string message)
            {
            private void WireEvents()
            {
            private void OnJournalReceived(object sender, ReceivedPacket pkt)
            {
            public bool SendCommand(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public RemoteCommand SendCommandDetailed(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public void Broadcast(string message, string sentBy = "System")
            =>
            public IEnumerable<RemoteCommand> GetRecentCommands()
            =>
            public bool SendImagesToATM(string atmId, string imagesFolder)
            {
            private void CheckHealth(ATMInfo atm)
            {
            public void CheckAllATMHealth()
            {
            public IEnumerable<ATMInfo>     GetConnectedATMs()   =>
            public ATMInfo                  GetATM(string atmId) =>
            private void Log(string msg)
            {
            public void Dispose()
            {
            private void ReadLoop()
            {
            private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
            {
            public void SendData(byte[] data)
            {
            public void Disconnect()
            {
            public event Action<string, string> OnLogReceived;
            public event Action<string> OnLogMessage;
            public event Action<string, bool> OnClientStatusChanged;
            public event EventHandler<ATMInfo>      OnATMConnected;
            public event EventHandler<ATMInfo>      OnATMDisconnected;
            public event EventHandler<ATMInfo>      OnATMUpdated;
            public event EventHandler<AlertPayload> OnAlert;
            public event EventHandler<RemoteCommand> OnCommandChanged;
            public event EventHandler<string>       OnLog;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string> OnDisconnected;
        }
    
        public partial public public sealed class EJServerService : IDisposable
        {
            private readonly ServerEngine    _serverEngine;
            private readonly ArchiveManager  _archiveManager;
            private readonly ImageSyncEngine _imageSync;
            private readonly JournalSyncTracker _syncTracker;
            private bool _isRunning;
            private readonly int _port;
            public int    ConnectedATMs        => _serverEngine.ConnectedCount;
            public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
            public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
            public bool   IsRunning            => _isRunning;
            public DateTime StartedAt          => _serverEngine.StartedAt;
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private bool _isConnected;
            public EJServerService(int port = AppConstants.DefaultPort, string archivePath = null)
            {
            public int    TotalJournalsArchived { get; private set; }
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; }
            public string RemoteEndpoint
            {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            private void WireEvents()
            {
            public void Start()
            {
            public void Stop()
            {
            private void OnJournalReceived(object sender, ReceivedPacket pkt)
            {
            public bool SendCommand(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public RemoteCommand SendCommandDetailed(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public void Broadcast(string message, string sentBy = "System")
            =>
            public IEnumerable<RemoteCommand> GetRecentCommands()
            =>
            public bool SendImagesToATM(string atmId, string imagesFolder)
            {
            private void CheckHealth(ATMInfo atm)
            {
            public void CheckAllATMHealth()
            {
            public IEnumerable<ATMInfo>     GetConnectedATMs()   =>
            public ATMInfo                  GetATM(string atmId) =>
            private void Log(string msg)
            {
            public void Dispose()
            {
            private void ReadLoop()
            {
            private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
            {
            public void SendData(byte[] data)
            {
            public void Disconnect()
            {
            public event EventHandler<ATMInfo>      OnATMConnected;
            public event EventHandler<ATMInfo>      OnATMDisconnected;
            public event EventHandler<ATMInfo>      OnATMUpdated;
            public event EventHandler<AlertPayload> OnAlert;
            public event EventHandler<RemoteCommand> OnCommandChanged;
            public event EventHandler<string>       OnLog;
            public event Action<string> OnLogMessage;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string> OnDisconnected;
        }
    
        public partial public public class ClientConnection
        {
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private bool _isConnected;
            public ClientConnection(TcpClient client, string storagePath)
            {
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; }
            public string RemoteEndpoint
            {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            public void Start()
            {
            private void ReadLoop()
            {
            private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
            {
            public void SendData(byte[] data)
            {
            public void Disconnect()
            {
            public event Action<string> OnLogMessage;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string> OnDisconnected;
        }
    
    }
    public partial public class EJServer
        {
            private TcpListener _listener;
            private bool _isRunning;
            private List<ATMInfo> _connectedATMs = new List<ATMInfo>();
            private readonly object _clientsSync = new object();
            private int _port;
            private string _storagePath;
            private List<ClientConnection> _clients;
            private Thread _listenThread;
            private MonitoringStateStore _monitoringStore;
            private JournalSyncService _journalSyncService;
            public EJServer(int port, string storagePath)
            {
            public void Start(int port)
            {
            private void ListenForClients()
            {
            private void HandleClient(TcpClient client)
            {
            public void Stop()
            {
            public void Start()
            {
            private void ListenLoop()
            {
            private void HandleHandshakeReceived(string atmId, string remoteEndpoint)
            {
            private void HandleHeartbeatReceived(string atmId)
            {
            private void HandleMetadataReceived(string atmId, Dictionary<string, string> metadata)
            {
            private void HandleCashStatusReceived(string atmId, Dictionary<string, string> payload)
            {
            private void HandleClientDisconnected(string atmId)
            {
            private void HandleDataReceived(string atmId, byte[] encryptedData)
            {
            private byte[] DecryptAES256(byte[] data)
            {
            private byte[] DecompressData(byte[] data)
            {
            public void SendCommand(string atmId, string command, string parameters)
            {
            private string ComputeChecksum(byte[] data)
            {
            private static int ParseInt(Dictionary<string, string> values, string key)
            {
            private static string GetValue(Dictionary<string, string> values, string key, string fallback)
            {
            private void Log(string message)
            {
            public event Action<string, string> OnLogReceived;
            public event Action<string> OnLogMessage;
            public event Action<string, bool> OnClientStatusChanged;
        }
    
        public partial public sealed class EJServerService : IDisposable
        {
            private readonly ServerEngine    _serverEngine;
            private readonly ArchiveManager  _archiveManager;
            private readonly ImageSyncEngine _imageSync;
            private readonly JournalSyncTracker _syncTracker;
            private bool _isRunning;
            private readonly int _port;
            public int    ConnectedATMs        => _serverEngine.ConnectedCount;
            public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
            public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
            public bool   IsRunning            => _isRunning;
            public DateTime StartedAt          => _serverEngine.StartedAt;
            public EJServerService(int port = AppConstants.DefaultPort, string archivePath = null)
            {
            public int    TotalJournalsArchived { get; private set; }
            private void WireEvents()
            {
            public void Start()
            {
            public void Stop()
            {
            private void OnJournalReceived(object sender, ReceivedPacket pkt)
            {
            public bool SendCommand(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public RemoteCommand SendCommandDetailed(string atmId, string cmdType, string parameters, string sentBy = "System")
            =>
            public void Broadcast(string message, string sentBy = "System")
            =>
            public IEnumerable<RemoteCommand> GetRecentCommands()
            =>
            public bool SendImagesToATM(string atmId, string imagesFolder)
            {
            private void CheckHealth(ATMInfo atm)
            {
            public void CheckAllATMHealth()
            {
            public IEnumerable<ATMInfo>     GetConnectedATMs()   =>
            public ATMInfo                  GetATM(string atmId) =>
            private void Log(string msg)
            {
            public void Dispose()
            {
            public event EventHandler<ATMInfo>      OnATMConnected;
            public event EventHandler<ATMInfo>      OnATMDisconnected;
            public event EventHandler<ATMInfo>      OnATMUpdated;
            public event EventHandler<AlertPayload> OnAlert;
            public event EventHandler<RemoteCommand> OnCommandChanged;
            public event EventHandler<string>       OnLog;
        }
    
        public partial public class ClientConnection
        {
            private TcpClient _tcpClient;
            private NetworkStream _stream;
            private Thread _readThread;
            private bool _isConnected;
            public ClientConnection(TcpClient client, string storagePath)
            {
            public string ATMID { get; private set; }
            public bool IsConnected { get { return _isConnected; }
            public string RemoteEndpoint
            {
            get { return _tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.RemoteEndPoint != null ? _tcpClient.Client.RemoteEndPoint.ToString() : string.Empty; }
            public void Start()
            {
            private void ReadLoop()
            {
            private static Dictionary<string, string> ParseKeyValueParts(string[] parts, int startIndex)
            {
            public void SendData(byte[] data)
            {
            public void Disconnect()
            {
            public event Action<string> OnLogMessage;
            public event Action<string, byte[]> OnDataReceived;
            public event Action<string, string> OnHandshakeReceived;
            public event Action<string> OnHeartbeatReceived;
            public event Action<string> OnDisconnected;
        }
    
    }
    public partial class EJServerService : IDisposable
        {
            private readonly ServerEngine    _serverEngine;
    
    
            private readonly ArchiveManager  _archiveManager;
    
    
            private readonly ImageSyncEngine _imageSync;
    
    
            private readonly JournalSyncTracker _syncTracker;
    
    
            private bool _isRunning;
    
    
            private readonly int _port;
    
    
            public int    ConnectedATMs        => _serverEngine.ConnectedCount;
    
    
            public long   TotalBytesReceived   => _serverEngine.TotalBytesReceived;
    
    
            public int    TotalJournalsArchived { get; private set; }
    
    
            public double FreeSpaceGB          => _archiveManager.GetFreeSpaceGB();
    
    
            public bool   IsRunning            => _isRunning;
    
    
            public DateTime StartedAt          => _serverEngine.StartedAt;
    
    
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
    
    
            public IEnumerable<ATMInfo>     GetConnectedATMs()   => _serverEngine.GetConnectedATMs();
    
    
            public ATMInfo                  GetATM(string atmId) => _serverEngine.GetATMByID(atmId);
    
    
            public (long files, long bytes) GetArchiveStats(string atmId)
                => _archiveManager.GetATMArchiveStats(atmId);
    
    
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
    
    
            public event EventHandler<ATMInfo>      OnATMConnected;
    
    
            public event EventHandler<ATMInfo>      OnATMDisconnected;
    
    
            public event EventHandler<ATMInfo>      OnATMUpdated;
    
    
            public event EventHandler<AlertPayload> OnAlert;
    
    
            public event EventHandler<RemoteCommand> OnCommandChanged;
    
    
            public event EventHandler<string>       OnLog;
    
    
        }

    public class EJServer
        {
            private TcpListener _listener;
            private bool _isRunning;
            private List<ATMInfo> _connectedATMs = new List<ATMInfo>();
    
            public event Action<string, string> OnLogReceived;
    
            public void Start(int port)
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                _isRunning = true;
    
                new Thread(ListenForClients).Start();
                OnLogReceived?.Invoke("SYSTEM", $"Server started on port {port}");
            }
    
            private void ListenForClients()
            {
                while (_isRunning)
                {
                    try
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        new Thread(() => HandleClient(client)).Start();
                    }
                    catch { }
                }
            }
    
            private void HandleClient(TcpClient client)
            {
                string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                OnLogReceived?.Invoke("NET", $"New connection from {clientIP}");
    
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[4096];
                    while (client.Connected)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;
    
                        // Logic to decrypt, decompress and process data
                        // Update database and notify monitoring UI
                    }
                }
                client.Close();
            }
    
            public void Stop()
            {
                _isRunning = false;
                _listener.Stop();
            }
        }
    public partial class EJServer
        {
            private TcpListener _listener;
    
    
            private bool _isRunning;
    
    
            private List<ATMInfo> _connectedATMs = new List<ATMInfo>();
    
    
            public void Start(int port)
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                _isRunning = true;
    
                new Thread(ListenForClients).Start();
                OnLogReceived?.Invoke("SYSTEM", $"Server started on port {port}");
            }
    
    
            private void ListenForClients()
            {
                while (_isRunning)
                {
                    try
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        new Thread(() => HandleClient(client)).Start();
                    }
                    catch { }
                }
            }
    
    
            private void HandleClient(TcpClient client)
            {
                string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                OnLogReceived?.Invoke("NET", $"New connection from {clientIP}");
    
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[4096];
                    while (client.Connected)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;
    
                        // Logic to decrypt, decompress and process data
                        // Update database and notify monitoring UI
                    }
                }
                client.Close();
            }
    
    
            public void Stop()
            {
                _isRunning = false;
                _listener.Stop();
            }
    
    
            public event Action<string, string> OnLogReceived;
    
    
        }
}

using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
