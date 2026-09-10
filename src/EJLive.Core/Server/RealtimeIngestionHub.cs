using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Core.Engine;
using EJLive.Core.Models;

namespace EJLive.Core.Server
{
    public partial class RealtimeIngestionHub : IDisposable
        {
            private readonly EvidenceCorrelationEngine _correlationEngine;
            private readonly ATMRealTimeStatusReducer _reducer;
            private readonly ConcurrentQueue<IngestionEvent> _eventQueue = new();
            private readonly ConcurrentDictionary<string, DateTime> _lastEventTimes = new();
            private readonly object _lock = new();
            private CancellationTokenSource? _cts;
            private Task? _processingTask;
            private bool _disposed;
            public int BatchSize { get; set; } = 100;
            public int ProcessingIntervalMs { get; set; } = 500;
            public void IngestJournalDelta(string atmId, string journalFile, long offset, string[] newLines)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "JournalDelta",
                    AtmId = atmId,
                    JournalFile = journalFile,
                    JournalOffset = offset,
                    Lines = newLines,
                    TimestampUtc = DateTime.UtcNow
                });
            }
            public void IngestCashStatus(string atmId, Dictionary<string, long> cassetteCounts,
                long totalCash, bool cashLow, bool cashOut)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "CashStatus",
                    AtmId = atmId,
                    CassetteCounts = cassetteCounts,
                    TotalCash = totalCash,
                    CashLow = cashLow,
                    CashOut = cashOut,
                    TimestampUtc = DateTime.UtcNow
                });
            }
            public int QueueDepth => _eventQueue.Count;
            public IReadOnlyList<string> ActiveAtmIds => _reducer.TrackedAtmIds.ToList().AsReadOnly();
            public EvidenceCorrelationEngine CorrelationEngine => _correlationEngine;
            public ATMRealTimeStatusReducer Reducer => _reducer;
            public RealtimeIngestionHub(
                EvidenceCorrelationEngine? correlationEngine = null,
                ATMRealTimeStatusReducer? reducer = null)
            {
                _correlationEngine = correlationEngine ?? new EvidenceCorrelationEngine();
                _reducer = reducer ?? new ATMRealTimeStatusReducer();
                // Wire snapshot updates from reducer/correlation
                _reducer.OnSnapshotUpdated += (_, snapshot) => OnSnapshotProduced?.Invoke(this, snapshot);
                _correlationEngine.OnSnapshotUpdated += (_, snapshot) => OnSnapshotProduced?.Invoke(this, snapshot);
            }
            public void Start(CancellationToken ct = default)
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                _processingTask = Task.Run(() => ProcessLoopAsync(_cts.Token), _cts.Token);
            }
            public async Task StopAsync()
            {
                _cts?.Cancel();
                if (_processingTask != null)
                    await _processingTask.ConfigureAwait(false);
            }
            public void IngestFile(string atmId, string fileName, byte[] data, string checksum, string fileType)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "FileReceived",
                    AtmId = atmId,
                    FileName = fileName,
                    FileData = data,
                    Checksum = checksum,
                    FileType = fileType,
                    TimestampUtc = DateTime.UtcNow
                });
            }
            public void IngestHeartbeat(string atmId, bool isOnline, DateTime timestamp,
                int outboxCount = 0, int failedCount = 0, string? lastError = null)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "Heartbeat",
                    AtmId = atmId,
                    IsOnline = isOnline,
                    TimestampUtc = timestamp,
                    OutboxCount = outboxCount,
                    FailedCount = failedCount,
                    LastError = lastError
                });
            }
            public void IngestDeviceStatus(string atmId, Dictionary<string, string> deviceStates)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "DeviceStatus",
                    AtmId = atmId,
                    DeviceStates = deviceStates,
                    TimestampUtc = DateTime.UtcNow
                });
            }
            public void IngestRemoteOperationAudit(string atmId, string commandId,
                string commandType, string result, string? operatorId = null)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "RemoteOpAudit",
                    AtmId = atmId,
                    CommandId = commandId,
                    CommandType = commandType,
                    Result = result,
                    OperatorId = operatorId,
                    TimestampUtc = DateTime.UtcNow
                });
            }
            public ATMRealTimeStatusSnapshot? GetLatestSnapshot(string atmId) => _reducer.GetLatest(atmId);
            private void Enqueue(IngestionEvent evt)
            {
                _eventQueue.Enqueue(evt);
                _lastEventTimes[evt.AtmId] = DateTime.UtcNow;
            }
            private async Task ProcessLoopAsync(CancellationToken ct)
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var batch = DequeueBatch(BatchSize);
                        if (batch.Count > 0)
                            ProcessBatch(batch);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception) { /* Log and continue processing */ }
                    await Task.Delay(ProcessingIntervalMs, ct).ConfigureAwait(false);
                }
            }
            private List<IngestionEvent> DequeueBatch(int max)
            {
                var batch = new List<IngestionEvent>();
                while (batch.Count < max && _eventQueue.TryDequeue(out var evt))
                    batch.Add(evt);
                return batch;
            }
            private void ProcessBatch(List<IngestionEvent> batch)
            {
                foreach (var evt in batch)
                {
                    try
                    {
                        switch (evt.Type)
                        {
                            case "Heartbeat":
                                _reducer.SetConnectionState(evt.AtmId,
                                    evt.IsOnline ? "Online" : "Offline",
                                    evt.IsOnline, evt.TimestampUtc);
                                _reducer.SetServiceState(evt.AtmId, "Running",
                                    outboxState: evt.OutboxCount > 0 ? "Pending" : "Idle");
                                if (!string.IsNullOrEmpty(evt.LastError))
                                    _reducer.AddAlarm(evt.AtmId, $"ClientError:{evt.LastError}");
                                break;
                            case "JournalDelta":
                                _reducer.SetJournalEvidence(evt.AtmId,
                                    lastJournalFile: evt.JournalFile,
                                    lastJournalLine: evt.JournalOffset,
                                    journalDeltaLines: evt.Lines?.Length ?? 0,
                                    lastSyncUtc: DateTime.UtcNow);
                                break;
                            case "FileReceived":
                                _reducer.SetServiceState(evt.AtmId, "Running",
                                    lastFileSyncUtc: DateTime.UtcNow, syncState: "Syncing");
                                break;
                            case "DeviceStatus":
                                if (evt.DeviceStates != null)
                                    _reducer.SetXfsEvidence(evt.AtmId, true,
                                        deviceStates: evt.DeviceStates);
                                break;
                            case "CashStatus":
                                if (evt.CassetteCounts != null)
                                    _reducer.SetCashEvidence(evt.AtmId, true,
                                        cassetteRemaining: evt.CassetteCounts,
                                        totalCash: evt.TotalCash,
                                        cashLow: evt.CashLow,
                                        cashOut: evt.CashOut);
                                if (evt.CashOut)
                                    OnAlarmRaised?.Invoke(this, (evt.AtmId, "CashOut"));
                                else if (evt.CashLow)
                                    OnAlarmRaised?.Invoke(this, (evt.AtmId, "CashLow"));
                                break;
                            case "RemoteOpAudit":
                                _reducer.SetRemoteOperationsState(evt.AtmId,
                                    lastControlledCommandId: evt.CommandId);
                                if (evt.Result?.Equals("Failed", StringComparison.OrdinalIgnoreCase) == true)
                                    OnAlarmRaised?.Invoke(this, (evt.AtmId, $"RemoteOpFailed:{evt.CommandType}"));
                                break;
                        }
                    }
                    catch { /* Skip bad events, continue processing */ }
                }
            }
            public void Dispose()
            {
                if (!_disposed)
                {
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _disposed = true;
                }
            }
            public event EventHandler<ATMRealTimeStatusSnapshot>? OnSnapshotProduced;
            public event EventHandler<(string AtmId, string Alarm)>? OnAlarmRaised;
            private sealed class IngestionEvent
            {
                public string Type { get; set; } = string.Empty;
                public string AtmId { get; set; } = string.Empty;
                public string? JournalFile { get; set; }
                public long JournalOffset { get; set; }
                public string[]? Lines { get; set; }
                public string? FileName { get; set; }
                public byte[]? FileData { get; set; }
                public string? Checksum { get; set; }
                public string? FileType { get; set; }
                public bool IsOnline { get; set; }
                public DateTime TimestampUtc { get; set; }
                public int OutboxCount { get; set; }
                public int FailedCount { get; set; }
                public string? LastError { get; set; }
                public Dictionary<string, string>? DeviceStates { get; set; }
                public Dictionary<string, long>? CassetteCounts { get; set; }
                public long TotalCash { get; set; }
                public bool CashLow { get; set; }
                public bool CashOut { get; set; }
                public string? CommandId { get; set; }
                public string? CommandType { get; set; }
                public string? Result { get; set; }
                public string? OperatorId { get; set; }
            }
        }
    public partial public public sealed class RealtimeIngestionHub : IDisposable
        {
            private readonly EvidenceCorrelationEngine _correlationEngine;
            private readonly ATMRealTimeStatusReducer _reducer;
            private readonly ConcurrentQueue<IngestionEvent> _eventQueue = new();
            private readonly ConcurrentDictionary<string, DateTime> _lastEventTimes = new();
            private readonly object _lock = new();
            private bool _disposed;
            public int QueueDepth => _eventQueue.Count;
            public IReadOnlyList<string> ActiveAtmIds => _reducer.TrackedAtmIds.ToList().AsReadOnly();
            public EvidenceCorrelationEngine CorrelationEngine => _correlationEngine;
            public ATMRealTimeStatusReducer Reducer => _reducer;
            public RealtimeIngestionHub(
            EvidenceCorrelationEngine? correlationEngine = null,
            ATMRealTimeStatusReducer? reducer = null)
            {
            public int BatchSize { get; set; }
            public int ProcessingIntervalMs { get; set; }
            public string Type { get; set; }
            public string AtmId { get; set; }
            public string? JournalFile { get; set; }
            public long JournalOffset { get; set; }
            public string? FileName { get; set; }
            public string? Checksum { get; set; }
            public string? FileType { get; set; }
            public bool IsOnline { get; set; }
            public DateTime TimestampUtc { get; set; }
            public int OutboxCount { get; set; }
            public int FailedCount { get; set; }
            public string? LastError { get; set; }
            public Dictionary<string, string>? DeviceStates { get; set; }
            public Dictionary<string, long>? CassetteCounts { get; set; }
            public long TotalCash { get; set; }
            public bool CashLow { get; set; }
            public bool CashOut { get; set; }
            public string? CommandId { get; set; }
            public string? CommandType { get; set; }
            public string? Result { get; set; }
            public string? OperatorId { get; set; }
            public void Start(CancellationToken ct = default)
            {
            public async Task StopAsync()
            {
            public void IngestJournalDelta(string atmId, string journalFile, long offset, string[] newLines)
            {
            public void IngestFile(string atmId, string fileName, byte[] data, string checksum, string fileType)
            {
            public void IngestHeartbeat(string atmId, bool isOnline, DateTime timestamp,
            int outboxCount = 0, int failedCount = 0, string? lastError = null)
            {
            public void IngestDeviceStatus(string atmId, Dictionary<string, string> deviceStates)
            {
            public void IngestCashStatus(string atmId, Dictionary<string, long> cassetteCounts,
            long totalCash, bool cashLow, bool cashOut)
            {
            public void IngestRemoteOperationAudit(string atmId, string commandId,
            string commandType, string result, string? operatorId = null)
            {
            public ATMRealTimeStatusSnapshot? GetLatestSnapshot(string atmId) =>
            private void Enqueue(IngestionEvent evt)
            {
            private async Task ProcessLoopAsync(CancellationToken ct)
            {
            private List<IngestionEvent> DequeueBatch(int max)
            {
            private void ProcessBatch(List<IngestionEvent> batch)
            {
            public void Dispose()
            {
        }
    
        public partial public private sealed class IngestionEvent
        {
            public string Type { get; set; }
            public string AtmId { get; set; }
            public string? JournalFile { get; set; }
            public long JournalOffset { get; set; }
            public string? FileName { get; set; }
            public string? Checksum { get; set; }
            public string? FileType { get; set; }
            public bool IsOnline { get; set; }
            public DateTime TimestampUtc { get; set; }
            public int OutboxCount { get; set; }
            public int FailedCount { get; set; }
            public string? LastError { get; set; }
            public Dictionary<string, string>? DeviceStates { get; set; }
            public Dictionary<string, long>? CassetteCounts { get; set; }
            public long TotalCash { get; set; }
            public bool CashLow { get; set; }
            public bool CashOut { get; set; }
            public string? CommandId { get; set; }
            public string? CommandType { get; set; }
            public string? Result { get; set; }
            public string? OperatorId { get; set; }
        }
    
    }
    public partial public sealed class RealtimeIngestionHub : IDisposable
        {
            private readonly EvidenceCorrelationEngine _correlationEngine;
            private readonly ATMRealTimeStatusReducer _reducer;
            private readonly ConcurrentQueue<IngestionEvent> _eventQueue = new();
            private readonly ConcurrentDictionary<string, DateTime> _lastEventTimes = new();
            private readonly object _lock = new();
            private bool _disposed;
            public int QueueDepth => _eventQueue.Count;
            public IReadOnlyList<string> ActiveAtmIds => _reducer.TrackedAtmIds.ToList().AsReadOnly();
            public EvidenceCorrelationEngine CorrelationEngine => _correlationEngine;
            public ATMRealTimeStatusReducer Reducer => _reducer;
            public RealtimeIngestionHub(
            EvidenceCorrelationEngine? correlationEngine = null,
            ATMRealTimeStatusReducer? reducer = null)
            {
            public int BatchSize { get; set; }
            public int ProcessingIntervalMs { get; set; }
            public string Type { get; set; }
            public string AtmId { get; set; }
            public string? JournalFile { get; set; }
            public long JournalOffset { get; set; }
            public string? FileName { get; set; }
            public string? Checksum { get; set; }
            public string? FileType { get; set; }
            public bool IsOnline { get; set; }
            public DateTime TimestampUtc { get; set; }
            public int OutboxCount { get; set; }
            public int FailedCount { get; set; }
            public string? LastError { get; set; }
            public Dictionary<string, string>? DeviceStates { get; set; }
            public Dictionary<string, long>? CassetteCounts { get; set; }
            public long TotalCash { get; set; }
            public bool CashLow { get; set; }
            public bool CashOut { get; set; }
            public string? CommandId { get; set; }
            public string? CommandType { get; set; }
            public string? Result { get; set; }
            public string? OperatorId { get; set; }
            public void Start(CancellationToken ct = default)
            {
            public async Task StopAsync()
            {
            public void IngestJournalDelta(string atmId, string journalFile, long offset, string[] newLines)
            {
            public void IngestFile(string atmId, string fileName, byte[] data, string checksum, string fileType)
            {
            public void IngestHeartbeat(string atmId, bool isOnline, DateTime timestamp,
            int outboxCount = 0, int failedCount = 0, string? lastError = null)
            {
            public void IngestDeviceStatus(string atmId, Dictionary<string, string> deviceStates)
            {
            public void IngestCashStatus(string atmId, Dictionary<string, long> cassetteCounts,
            long totalCash, bool cashLow, bool cashOut)
            {
            public void IngestRemoteOperationAudit(string atmId, string commandId,
            string commandType, string result, string? operatorId = null)
            {
            public ATMRealTimeStatusSnapshot? GetLatestSnapshot(string atmId) =>
            private void Enqueue(IngestionEvent evt)
            {
            private async Task ProcessLoopAsync(CancellationToken ct)
            {
            private List<IngestionEvent> DequeueBatch(int max)
            {
            private void ProcessBatch(List<IngestionEvent> batch)
            {
            public void Dispose()
            {
        }
    
        public partial private sealed class IngestionEvent
        {
            public string Type { get; set; }
            public string AtmId { get; set; }
            public string? JournalFile { get; set; }
            public long JournalOffset { get; set; }
            public string? FileName { get; set; }
            public string? Checksum { get; set; }
            public string? FileType { get; set; }
            public bool IsOnline { get; set; }
            public DateTime TimestampUtc { get; set; }
            public int OutboxCount { get; set; }
            public int FailedCount { get; set; }
            public string? LastError { get; set; }
            public Dictionary<string, string>? DeviceStates { get; set; }
            public Dictionary<string, long>? CassetteCounts { get; set; }
            public long TotalCash { get; set; }
            public bool CashLow { get; set; }
            public bool CashOut { get; set; }
            public string? CommandId { get; set; }
            public string? CommandType { get; set; }
            public string? Result { get; set; }
            public string? OperatorId { get; set; }
        }
    
    }
    public partial class RealtimeIngestionHub : IDisposable
        {
            private readonly EvidenceCorrelationEngine _correlationEngine;
    
    
            private readonly ATMRealTimeStatusReducer _reducer;
    
    
            private readonly ConcurrentQueue<IngestionEvent> _eventQueue = new();
    
    
            private readonly ConcurrentDictionary<string, DateTime> _lastEventTimes = new();
    
    
            private readonly object _lock = new();
    
    
            private CancellationTokenSource? _cts;
    
    
            private Task? _processingTask;
    
    
            private bool _disposed;
    
    
            public int BatchSize { get; set; } = 100;
    
    
            public int ProcessingIntervalMs { get; set; } = 500;
    
    
            public void IngestJournalDelta(string atmId, string journalFile, long offset, string[] newLines)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "JournalDelta",
                    AtmId = atmId,
                    JournalFile = journalFile,
                    JournalOffset = offset,
                    Lines = newLines,
                    TimestampUtc = DateTime.UtcNow
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Server\RealtimeIngestionHub.cs
            public void IngestCashStatus(string atmId, Dictionary<string, long> cassetteCounts,
                long totalCash, bool cashLow, bool cashOut)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "CashStatus",
                    AtmId = atmId,
                    CassetteCounts = cassetteCounts,
                    TotalCash = totalCash,
                    CashLow = cashLow,
                    CashOut = cashOut,
                    TimestampUtc = DateTime.UtcNow
                });
            }
    
    
            public int QueueDepth => _eventQueue.Count;
    
    
            public IReadOnlyList<string> ActiveAtmIds => _reducer.TrackedAtmIds.ToList().AsReadOnly();
    
    
            public EvidenceCorrelationEngine CorrelationEngine => _correlationEngine;
    
    
            public ATMRealTimeStatusReducer Reducer => _reducer;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Server\RealtimeIngestionHub.cs
            public void IngestCashStatus(string atmId, Dictionary<string, long> cassetteCounts,
                long totalCash, bool cashLow, bool cashOut)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "CashStatus",
                    AtmId = atmId,
                    CassetteCounts = cassetteCounts,
                    TotalCash = totalCash,
                    CashLow = cashLow,
                    CashOut = cashOut,
                    TimestampUtc = DateTime.UtcNow
                });
            }
    
    
            public RealtimeIngestionHub(
                EvidenceCorrelationEngine? correlationEngine = null,
                ATMRealTimeStatusReducer? reducer = null)
            {
                _correlationEngine = correlationEngine ?? new EvidenceCorrelationEngine();
                _reducer = reducer ?? new ATMRealTimeStatusReducer();
    
                // Wire snapshot updates from reducer/correlation
                _reducer.OnSnapshotUpdated += (_, snapshot) => OnSnapshotProduced?.Invoke(this, snapshot);
                _correlationEngine.OnSnapshotUpdated += (_, snapshot) => OnSnapshotProduced?.Invoke(this, snapshot);
            }
    
    
            public void Start(CancellationToken ct = default)
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                _processingTask = Task.Run(() => ProcessLoopAsync(_cts.Token), _cts.Token);
            }
    
    
            public async Task StopAsync()
            {
                _cts?.Cancel();
                if (_processingTask != null)
                    await _processingTask.ConfigureAwait(false);
            }
    
    
            public void IngestFile(string atmId, string fileName, byte[] data, string checksum, string fileType)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "FileReceived",
                    AtmId = atmId,
                    FileName = fileName,
                    FileData = data,
                    Checksum = checksum,
                    FileType = fileType,
                    TimestampUtc = DateTime.UtcNow
                });
            }
    
    
            public void IngestHeartbeat(string atmId, bool isOnline, DateTime timestamp,
                int outboxCount = 0, int failedCount = 0, string? lastError = null)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "Heartbeat",
                    AtmId = atmId,
                    IsOnline = isOnline,
                    TimestampUtc = timestamp,
                    OutboxCount = outboxCount,
                    FailedCount = failedCount,
                    LastError = lastError
                });
            }
    
    
            public void IngestDeviceStatus(string atmId, Dictionary<string, string> deviceStates)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "DeviceStatus",
                    AtmId = atmId,
                    DeviceStates = deviceStates,
                    TimestampUtc = DateTime.UtcNow
                });
            }
    
    
            public void IngestRemoteOperationAudit(string atmId, string commandId,
                string commandType, string result, string? operatorId = null)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "RemoteOpAudit",
                    AtmId = atmId,
                    CommandId = commandId,
                    CommandType = commandType,
                    Result = result,
                    OperatorId = operatorId,
                    TimestampUtc = DateTime.UtcNow
                });
            }
    
    
            public ATMRealTimeStatusSnapshot? GetLatestSnapshot(string atmId) => _reducer.GetLatest(atmId);
    
    
            private void Enqueue(IngestionEvent evt)
            {
                _eventQueue.Enqueue(evt);
                _lastEventTimes[evt.AtmId] = DateTime.UtcNow;
            }
    
    
            private async Task ProcessLoopAsync(CancellationToken ct)
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var batch = DequeueBatch(BatchSize);
                        if (batch.Count > 0)
                            ProcessBatch(batch);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception) { /* Log and continue processing */ }
    
                    await Task.Delay(ProcessingIntervalMs, ct).ConfigureAwait(false);
                }
            }
    
    
            private List<IngestionEvent> DequeueBatch(int max)
            {
                var batch = new List<IngestionEvent>();
                while (batch.Count < max && _eventQueue.TryDequeue(out var evt))
                    batch.Add(evt);
                return batch;
            }
    
    
            private void ProcessBatch(List<IngestionEvent> batch)
            {
                foreach (var evt in batch)
                {
                    try
                    {
                        switch (evt.Type)
                        {
                            case "Heartbeat":
                                _reducer.SetConnectionState(evt.AtmId,
                                    evt.IsOnline ? "Online" : "Offline",
                                    evt.IsOnline, evt.TimestampUtc);
                                _reducer.SetServiceState(evt.AtmId, "Running",
                                    outboxState: evt.OutboxCount > 0 ? "Pending" : "Idle");
                                if (!string.IsNullOrEmpty(evt.LastError))
                                    _reducer.AddAlarm(evt.AtmId, $"ClientError:{evt.LastError}");
                                break;
    
                            case "JournalDelta":
                                _reducer.SetJournalEvidence(evt.AtmId,
                                    lastJournalFile: evt.JournalFile,
                                    lastJournalLine: evt.JournalOffset,
                                    journalDeltaLines: evt.Lines?.Length ?? 0,
                                    lastSyncUtc: DateTime.UtcNow);
                                break;
    
                            case "FileReceived":
                                _reducer.SetServiceState(evt.AtmId, "Running",
                                    lastFileSyncUtc: DateTime.UtcNow, syncState: "Syncing");
                                break;
    
                            case "DeviceStatus":
                                if (evt.DeviceStates != null)
                                    _reducer.SetXfsEvidence(evt.AtmId, true,
                                        deviceStates: evt.DeviceStates);
                                break;
    
                            case "CashStatus":
                                if (evt.CassetteCounts != null)
                                    _reducer.SetCashEvidence(evt.AtmId, true,
                                        cassetteRemaining: evt.CassetteCounts,
                                        totalCash: evt.TotalCash,
                                        cashLow: evt.CashLow,
                                        cashOut: evt.CashOut);
                                if (evt.CashOut)
                                    OnAlarmRaised?.Invoke(this, (evt.AtmId, "CashOut"));
                                else if (evt.CashLow)
                                    OnAlarmRaised?.Invoke(this, (evt.AtmId, "CashLow"));
                                break;
    
                            case "RemoteOpAudit":
                                _reducer.SetRemoteOperationsState(evt.AtmId,
                                    lastControlledCommandId: evt.CommandId);
                                if (evt.Result?.Equals("Failed", StringComparison.OrdinalIgnoreCase) == true)
                                    OnAlarmRaised?.Invoke(this, (evt.AtmId, $"RemoteOpFailed:{evt.CommandType}"));
                                break;
                        }
                    }
                    catch { /* Skip bad events, continue processing */ }
                }
            }
    
    
            public void Dispose()
            {
                if (!_disposed)
                {
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _disposed = true;
                }
            }
    
    
            public event EventHandler<ATMRealTimeStatusSnapshot>? OnSnapshotProduced;
    
    
            public event EventHandler<(string AtmId, string Alarm)>? OnAlarmRaised;
    
    
            private sealed class IngestionEvent
            {
                public string Type { get; set; } = string.Empty;
                public string AtmId { get; set; } = string.Empty;
                public string? JournalFile { get; set; }
                public long JournalOffset { get; set; }
                public string[]? Lines { get; set; }
                public string? FileName { get; set; }
                public byte[]? FileData { get; set; }
                public string? Checksum { get; set; }
                public string? FileType { get; set; }
                public bool IsOnline { get; set; }
                public DateTime TimestampUtc { get; set; }
                public int OutboxCount { get; set; }
                public int FailedCount { get; set; }
                public string? LastError { get; set; }
                public Dictionary<string, string>? DeviceStates { get; set; }
                public Dictionary<string, long>? CassetteCounts { get; set; }
                public long TotalCash { get; set; }
                public bool CashLow { get; set; }
                public bool CashOut { get; set; }
                public string? CommandId { get; set; }
                public string? CommandType { get; set; }
                public string? Result { get; set; }
                public string? OperatorId { get; set; }
            }
    
    
        }

    // Class: IngestionEvent (from 2 sources)
        private sealed partial class IngestionEvent
        {
        }
    // Class: RealtimeIngestionHub (from 1 sources)
        public sealed partial class RealtimeIngestionHub : IDisposable
        {
            // --- Constants & Fields ---
                    private readonly EvidenceCorrelationEngine _correlationEngine;
    
                    private readonly ATMRealTimeStatusReducer _reducer;
    
                    private readonly ConcurrentQueue<IngestionEvent> _eventQueue = new();
    
                    private readonly ConcurrentDictionary<string, DateTime> _lastEventTimes = new();
    
                    private readonly object _lock = new();
    
                    private CancellationTokenSource? _cts;
    
                    private Task? _processingTask;
    
                    private bool _disposed;
    
    
            // --- Properties ---
                    public int BatchSize { get; set; } = 100;
    
                    public int ProcessingIntervalMs { get; set; } = 500;
    
                    public void IngestJournalDelta(string atmId, string journalFile, long offset, string[] newLines)
                    {
                        Enqueue(new IngestionEvent
                        {
                            Type = "JournalDelta",
                            AtmId = atmId,
                            JournalFile = journalFile,
                            JournalOffset = offset,
                            Lines = newLines,
                            TimestampUtc = DateTime.UtcNow
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Server\RealtimeIngestionHub.cs
                    public void IngestCashStatus(string atmId, Dictionary<string, long> cassetteCounts,
                        long totalCash, bool cashLow, bool cashOut)
                    {
                        Enqueue(new IngestionEvent
                        {
                            Type = "CashStatus",
                            AtmId = atmId,
                            CassetteCounts = cassetteCounts,
                            TotalCash = totalCash,
                            CashLow = cashLow,
                            CashOut = cashOut,
                            TimestampUtc = DateTime.UtcNow
                        });
                    }
    
                    public int QueueDepth => _eventQueue.Count;
    
                    public IReadOnlyList<string> ActiveAtmIds => _reducer.TrackedAtmIds.ToList().AsReadOnly();
    
                    public EvidenceCorrelationEngine CorrelationEngine => _correlationEngine;
    
                    public ATMRealTimeStatusReducer Reducer => _reducer;
    
    
            // --- Constructors ---
                    public RealtimeIngestionHub(
                        EvidenceCorrelationEngine? correlationEngine = null,
                        ATMRealTimeStatusReducer? reducer = null)
                    {
                        _correlationEngine = correlationEngine ?? new EvidenceCorrelationEngine();
                        _reducer = reducer ?? new ATMRealTimeStatusReducer();
    
                        // Wire snapshot updates from reducer/correlation
                        _reducer.OnSnapshotUpdated += (_, snapshot) => OnSnapshotProduced?.Invoke(this, snapshot);
                        _correlationEngine.OnSnapshotUpdated += (_, snapshot) => OnSnapshotProduced?.Invoke(this, snapshot);
                    }
    
    
            // --- Methods ---
                    public void Start(CancellationToken ct = default)
                    {
                        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                        _processingTask = Task.Run(() => ProcessLoopAsync(_cts.Token), _cts.Token);
                    }
    
                    public async Task StopAsync()
                    {
                        _cts?.Cancel();
                        if (_processingTask != null)
                            await _processingTask.ConfigureAwait(false);
                    }
    
                    public void IngestFile(string atmId, string fileName, byte[] data, string checksum, string fileType)
                    {
                        Enqueue(new IngestionEvent
                        {
                            Type = "FileReceived",
                            AtmId = atmId,
                            FileName = fileName,
                            FileData = data,
                            Checksum = checksum,
                            FileType = fileType,
                            TimestampUtc = DateTime.UtcNow
                        });
                    }
    
                    public void IngestHeartbeat(string atmId, bool isOnline, DateTime timestamp,
                        int outboxCount = 0, int failedCount = 0, string? lastError = null)
                    {
                        Enqueue(new IngestionEvent
                        {
                            Type = "Heartbeat",
                            AtmId = atmId,
                            IsOnline = isOnline,
                            TimestampUtc = timestamp,
                            OutboxCount = outboxCount,
                            FailedCount = failedCount,
                            LastError = lastError
                        });
                    }
    
                    public void IngestDeviceStatus(string atmId, Dictionary<string, string> deviceStates)
                    {
                        Enqueue(new IngestionEvent
                        {
                            Type = "DeviceStatus",
                            AtmId = atmId,
                            DeviceStates = deviceStates,
                            TimestampUtc = DateTime.UtcNow
                        });
                    }
    
                    public void IngestRemoteOperationAudit(string atmId, string commandId,
                        string commandType, string result, string? operatorId = null)
                    {
                        Enqueue(new IngestionEvent
                        {
                            Type = "RemoteOpAudit",
                            AtmId = atmId,
                            CommandId = commandId,
                            CommandType = commandType,
                            Result = result,
                            OperatorId = operatorId,
                            TimestampUtc = DateTime.UtcNow
                        });
                    }
    
                    public ATMRealTimeStatusSnapshot? GetLatestSnapshot(string atmId) => _reducer.GetLatest(atmId);
    
                    private void Enqueue(IngestionEvent evt)
                    {
                        _eventQueue.Enqueue(evt);
                        _lastEventTimes[evt.AtmId] = DateTime.UtcNow;
                    }
    
                    private async Task ProcessLoopAsync(CancellationToken ct)
                    {
                        while (!ct.IsCancellationRequested)
                        {
                            try
                            {
                                var batch = DequeueBatch(BatchSize);
                                if (batch.Count > 0)
                                    ProcessBatch(batch);
                            }
                            catch (OperationCanceledException) { break; }
                            catch (Exception) { /* Log and continue processing */ }
    
                            await Task.Delay(ProcessingIntervalMs, ct).ConfigureAwait(false);
                        }
                    }
    
                    private List<IngestionEvent> DequeueBatch(int max)
                    {
                        var batch = new List<IngestionEvent>();
                        while (batch.Count < max && _eventQueue.TryDequeue(out var evt))
                            batch.Add(evt);
                        return batch;
                    }
    
                    private void ProcessBatch(List<IngestionEvent> batch)
                    {
                        foreach (var evt in batch)
                        {
                            try
                            {
                                switch (evt.Type)
                                {
                                    case "Heartbeat":
                                        _reducer.SetConnectionState(evt.AtmId,
                                            evt.IsOnline ? "Online" : "Offline",
                                            evt.IsOnline, evt.TimestampUtc);
                                        _reducer.SetServiceState(evt.AtmId, "Running",
                                            outboxState: evt.OutboxCount > 0 ? "Pending" : "Idle");
                                        if (!string.IsNullOrEmpty(evt.LastError))
                                            _reducer.AddAlarm(evt.AtmId, $"ClientError:{evt.LastError}");
                                        break;
    
                                    case "JournalDelta":
                                        _reducer.SetJournalEvidence(evt.AtmId,
                                            lastJournalFile: evt.JournalFile,
                                            lastJournalLine: evt.JournalOffset,
                                            journalDeltaLines: evt.Lines?.Length ?? 0,
                                            lastSyncUtc: DateTime.UtcNow);
                                        break;
    
                                    case "FileReceived":
                                        _reducer.SetServiceState(evt.AtmId, "Running",
                                            lastFileSyncUtc: DateTime.UtcNow, syncState: "Syncing");
                                        break;
    
                                    case "DeviceStatus":
                                        if (evt.DeviceStates != null)
                                            _reducer.SetXfsEvidence(evt.AtmId, true,
                                                deviceStates: evt.DeviceStates);
                                        break;
    
                                    case "CashStatus":
                                        if (evt.CassetteCounts != null)
                                            _reducer.SetCashEvidence(evt.AtmId, true,
                                                cassetteRemaining: evt.CassetteCounts,
                                                totalCash: evt.TotalCash,
                                                cashLow: evt.CashLow,
                                                cashOut: evt.CashOut);
                                        if (evt.CashOut)
                                            OnAlarmRaised?.Invoke(this, (evt.AtmId, "CashOut"));
                                        else if (evt.CashLow)
                                            OnAlarmRaised?.Invoke(this, (evt.AtmId, "CashLow"));
                                        break;
    
                                    case "RemoteOpAudit":
                                        _reducer.SetRemoteOperationsState(evt.AtmId,
                                            lastControlledCommandId: evt.CommandId);
                                        if (evt.Result?.Equals("Failed", StringComparison.OrdinalIgnoreCase) == true)
                                            OnAlarmRaised?.Invoke(this, (evt.AtmId, $"RemoteOpFailed:{evt.CommandType}"));
                                        break;
                                }
                            }
                            catch { /* Skip bad events, continue processing */ }
                        }
                    }
    
                    public void Dispose()
                    {
                        if (!_disposed)
                        {
                            _cts?.Cancel();
                            _cts?.Dispose();
                            _disposed = true;
                        }
                    }
    
    
            // --- Events ---
                    public event EventHandler<ATMRealTimeStatusSnapshot>? OnSnapshotProduced;
    
                    public event EventHandler<(string AtmId, string Alarm)>? OnAlarmRaised;
    
    
            // --- Nested Classes ---
                    private sealed class IngestionEvent
                    {
                        public string Type { get; set; } = string.Empty;
                        public string AtmId { get; set; } = string.Empty;
                        public string? JournalFile { get; set; }
                        public long JournalOffset { get; set; }
                        public string[]? Lines { get; set; }
                        public string? FileName { get; set; }
                        public byte[]? FileData { get; set; }
                        public string? Checksum { get; set; }
                        public string? FileType { get; set; }
                        public bool IsOnline { get; set; }
                        public DateTime TimestampUtc { get; set; }
                        public int OutboxCount { get; set; }
                        public int FailedCount { get; set; }
                        public string? LastError { get; set; }
                        public Dictionary<string, string>? DeviceStates { get; set; }
                        public Dictionary<string, long>? CassetteCounts { get; set; }
                        public long TotalCash { get; set; }
                        public bool CashLow { get; set; }
                        public bool CashOut { get; set; }
                        public string? CommandId { get; set; }
                        public string? CommandType { get; set; }
                        public string? Result { get; set; }
                        public string? OperatorId { get; set; }
                    }
    
    
        }
    /// <summary>
        /// Realtime ingestion hub (section 10.2).
        /// Receives events, files, deltas from clients and dispatches
        /// to the pipeline: Journal → Parser → Correlation → Snapshot → Dashboard.
        /// Acts as the central event bus for the server.
        /// </summary>
        public sealed class RealtimeIngestionHub : IDisposable
        {
            private readonly EvidenceCorrelationEngine _correlationEngine;
            private readonly ATMRealTimeStatusReducer _reducer;
            private readonly ConcurrentQueue<IngestionEvent> _eventQueue = new();
            private readonly ConcurrentDictionary<string, DateTime> _lastEventTimes = new();
            private readonly object _lock = new();
            private CancellationTokenSource? _cts;
            private Task? _processingTask;
            private bool _disposed;
    
            /// <summary>Maximum events to process per batch.</summary>
            public int BatchSize { get; set; } = 100;
    
            /// <summary>Interval between processing batches (ms).</summary>
            public int ProcessingIntervalMs { get; set; } = 500;
    
            /// <summary>Fired when a new snapshot is produced.</summary>
            public event EventHandler<ATMRealTimeStatusSnapshot>? OnSnapshotProduced;
    
            /// <summary>Fired when an alarm is raised.</summary>
            public event EventHandler<(string AtmId, string Alarm)>? OnAlarmRaised;
    
            public RealtimeIngestionHub(
                EvidenceCorrelationEngine? correlationEngine = null,
                ATMRealTimeStatusReducer? reducer = null)
            {
                _correlationEngine = correlationEngine ?? new EvidenceCorrelationEngine();
                _reducer = reducer ?? new ATMRealTimeStatusReducer();
    
                // Wire snapshot updates from reducer/correlation
                _reducer.OnSnapshotUpdated += (_, snapshot) => OnSnapshotProduced?.Invoke(this, snapshot);
                _correlationEngine.OnSnapshotUpdated += (_, snapshot) => OnSnapshotProduced?.Invoke(this, snapshot);
            }
    
            /// <summary>Start the ingestion processing loop.</summary>
            public void Start(CancellationToken ct = default)
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                _processingTask = Task.Run(() => ProcessLoopAsync(_cts.Token), _cts.Token);
            }
    
            /// <summary>Stop the ingestion processing loop.</summary>
            public async Task StopAsync()
            {
                _cts?.Cancel();
                if (_processingTask != null)
                    await _processingTask.ConfigureAwait(false);
            }
    
            // ===== Ingestion Methods =====
    
            /// <summary>Ingest a journal delta (new lines from live journal).</summary>
            public void IngestJournalDelta(string atmId, string journalFile, long offset, string[] newLines)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "JournalDelta",
                    AtmId = atmId,
                    JournalFile = journalFile,
                    JournalOffset = offset,
                    Lines = newLines,
                    TimestampUtc = DateTime.UtcNow
                });
            }
    
            /// <summary>Ingest a complete file for archival and parsing.</summary>
            public void IngestFile(string atmId, string fileName, byte[] data, string checksum, string fileType)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "FileReceived",
                    AtmId = atmId,
                    FileName = fileName,
                    FileData = data,
                    Checksum = checksum,
                    FileType = fileType,
                    TimestampUtc = DateTime.UtcNow
                });
            }
    
            /// <summary>Ingest a heartbeat update.</summary>
            public void IngestHeartbeat(string atmId, bool isOnline, DateTime timestamp,
                int outboxCount = 0, int failedCount = 0, string? lastError = null)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "Heartbeat",
                    AtmId = atmId,
                    IsOnline = isOnline,
                    TimestampUtc = timestamp,
                    OutboxCount = outboxCount,
                    FailedCount = failedCount,
                    LastError = lastError
                });
            }
    
            /// <summary>Ingest a device status update.</summary>
            public void IngestDeviceStatus(string atmId, Dictionary<string, string> deviceStates)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "DeviceStatus",
                    AtmId = atmId,
                    DeviceStates = deviceStates,
                    TimestampUtc = DateTime.UtcNow
                });
            }
    
            /// <summary>Ingest a cash/cassette status update.</summary>
            public void IngestCashStatus(string atmId, Dictionary<string, long> cassetteCounts,
                long totalCash, bool cashLow, bool cashOut)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "CashStatus",
                    AtmId = atmId,
                    CassetteCounts = cassetteCounts,
                    TotalCash = totalCash,
                    CashLow = cashLow,
                    CashOut = cashOut,
                    TimestampUtc = DateTime.UtcNow
                });
            }
    
            /// <summary>Ingest a remote operation audit event.</summary>
            public void IngestRemoteOperationAudit(string atmId, string commandId,
                string commandType, string result, string? operatorId = null)
            {
                Enqueue(new IngestionEvent
                {
                    Type = "RemoteOpAudit",
                    AtmId = atmId,
                    CommandId = commandId,
                    CommandType = commandType,
                    Result = result,
                    OperatorId = operatorId,
                    TimestampUtc = DateTime.UtcNow
                });
            }
    
            // ===== Stats =====
    
            /// <summary>Get the current queue depth.</summary>
            public int QueueDepth => _eventQueue.Count;
    
            /// <summary>Get active ATM IDs being tracked.</summary>
            public IReadOnlyList<string> ActiveAtmIds => _reducer.TrackedAtmIds.ToList().AsReadOnly();
    
            /// <summary>Get latest snapshot for an ATM.</summary>
            public ATMRealTimeStatusSnapshot? GetLatestSnapshot(string atmId) => _reducer.GetLatest(atmId);
    
            /// <summary>Get the correlation engine.</summary>
            public EvidenceCorrelationEngine CorrelationEngine => _correlationEngine;
    
            /// <summary>Get the status reducer.</summary>
            public ATMRealTimeStatusReducer Reducer => _reducer;
    
            // ===== Private =====
    
            private void Enqueue(IngestionEvent evt)
            {
                _eventQueue.Enqueue(evt);
                _lastEventTimes[evt.AtmId] = DateTime.UtcNow;
            }
    
            private async Task ProcessLoopAsync(CancellationToken ct)
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var batch = DequeueBatch(BatchSize);
                        if (batch.Count > 0)
                            ProcessBatch(batch);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception) { /* Log and continue processing */ }
    
                    await Task.Delay(ProcessingIntervalMs, ct).ConfigureAwait(false);
                }
            }
    
            private List<IngestionEvent> DequeueBatch(int max)
            {
                var batch = new List<IngestionEvent>();
                while (batch.Count < max && _eventQueue.TryDequeue(out var evt))
                    batch.Add(evt);
                return batch;
            }
    
            private void ProcessBatch(List<IngestionEvent> batch)
            {
                foreach (var evt in batch)
                {
                    try
                    {
                        switch (evt.Type)
                        {
                            case "Heartbeat":
                                _reducer.SetConnectionState(evt.AtmId,
                                    evt.IsOnline ? "Online" : "Offline",
                                    evt.IsOnline, evt.TimestampUtc);
                                _reducer.SetServiceState(evt.AtmId, "Running",
                                    outboxState: evt.OutboxCount > 0 ? "Pending" : "Idle");
                                if (!string.IsNullOrEmpty(evt.LastError))
                                    _reducer.AddAlarm(evt.AtmId, $"ClientError:{evt.LastError}");
                                break;
    
                            case "JournalDelta":
                                _reducer.SetJournalEvidence(evt.AtmId,
                                    lastJournalFile: evt.JournalFile,
                                    lastJournalLine: evt.JournalOffset,
                                    journalDeltaLines: evt.Lines?.Length ?? 0,
                                    lastSyncUtc: DateTime.UtcNow);
                                break;
    
                            case "FileReceived":
                                _reducer.SetServiceState(evt.AtmId, "Running",
                                    lastFileSyncUtc: DateTime.UtcNow, syncState: "Syncing");
                                break;
    
                            case "DeviceStatus":
                                if (evt.DeviceStates != null)
                                    _reducer.SetXfsEvidence(evt.AtmId, true,
                                        deviceStates: evt.DeviceStates);
                                break;
    
                            case "CashStatus":
                                if (evt.CassetteCounts != null)
                                    _reducer.SetCashEvidence(evt.AtmId, true,
                                        cassetteRemaining: evt.CassetteCounts,
                                        totalCash: evt.TotalCash,
                                        cashLow: evt.CashLow,
                                        cashOut: evt.CashOut);
                                if (evt.CashOut)
                                    OnAlarmRaised?.Invoke(this, (evt.AtmId, "CashOut"));
                                else if (evt.CashLow)
                                    OnAlarmRaised?.Invoke(this, (evt.AtmId, "CashLow"));
                                break;
    
                            case "RemoteOpAudit":
                                _reducer.SetRemoteOperationsState(evt.AtmId,
                                    lastControlledCommandId: evt.CommandId);
                                if (evt.Result?.Equals("Failed", StringComparison.OrdinalIgnoreCase) == true)
                                    OnAlarmRaised?.Invoke(this, (evt.AtmId, $"RemoteOpFailed:{evt.CommandType}"));
                                break;
                        }
                    }
                    catch { /* Skip bad events, continue processing */ }
                }
            }
    
            public void Dispose()
            {
                if (!_disposed)
                {
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _disposed = true;
                }
            }
    
            // ===== Nested Types =====
    
            private sealed class IngestionEvent
            {
                public string Type { get; set; } = string.Empty;
                public string AtmId { get; set; } = string.Empty;
                public string? JournalFile { get; set; }
                public long JournalOffset { get; set; }
                public string[]? Lines { get; set; }
                public string? FileName { get; set; }
                public byte[]? FileData { get; set; }
                public string? Checksum { get; set; }
                public string? FileType { get; set; }
                public bool IsOnline { get; set; }
                public DateTime TimestampUtc { get; set; }
                public int OutboxCount { get; set; }
                public int FailedCount { get; set; }
                public string? LastError { get; set; }
                public Dictionary<string, string>? DeviceStates { get; set; }
                public Dictionary<string, long>? CassetteCounts { get; set; }
                public long TotalCash { get; set; }
                public bool CashLow { get; set; }
                public bool CashOut { get; set; }
                public string? CommandId { get; set; }
                public string? CommandType { get; set; }
                public string? Result { get; set; }
                public string? OperatorId { get; set; }
            }
        }

    public partial class IngestionEvent
        {
        }
}
