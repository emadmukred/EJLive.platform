using System;
using System.Collections.Concurrent;
using System.Text.Json;
using EJLive.Core;
using EJLive.Core.Models;

namespace EJLive.Core.Services;

public sealed class JournalSyncStateService
{
    private readonly ConcurrentDictionary<string, JournalState> _states = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _stateRoot;

    public JournalSyncStateService()
        : this(string.Empty)
    {
    }

    public JournalSyncStateService(string serverStateRoot)
    {
        _stateRoot = serverStateRoot ?? string.Empty;
    }

    public SyncStatus GetStatus(JournalSyncRecord record) => record.State switch
    {
        JournalSyncState.Pending => SyncStatus.Pending,
        JournalSyncState.Syncing => SyncStatus.Syncing,
        JournalSyncState.ReSyncing => SyncStatus.Resyncing,
        JournalSyncState.Completed => SyncStatus.Completed,
        JournalSyncState.Failed => SyncStatus.Failed,
        JournalSyncState.Archived => SyncStatus.Archived,
        _ => SyncStatus.Idle
    };

    public void UpdateConnectivity(string atmId, string atmType, bool connected)
    {
        var state = GetOrCreate(atmId, atmType);
        state.IsConnected = connected;
        state.UpdatedAtUtc = DateTime.UtcNow;
        PersistState(atmId, state);
    }

    public void UpdateHeartbeat(string atmId, string atmType)
    {
        var state = GetOrCreate(atmId, atmType);
        state.HeartbeatUtc = DateTime.UtcNow;
        state.UpdatedAtUtc = DateTime.UtcNow;
        PersistState(atmId, state);
    }

    public void RecordDetected(string atmId, string atmType, string filePath, long fileSize, string checksum)
    {
        var state = GetOrCreate(atmId, atmType);
        state.LastFilePath = filePath ?? string.Empty;
        state.LastChecksum = checksum ?? string.Empty;
        state.LastFileSize = fileSize;
        state.DetectedCount++;
        state.UpdatedAtUtc = DateTime.UtcNow;
        PersistState(atmId, state);
    }

    public void RecordSent(string atmId, string checksum)
    {
        if (_states.TryGetValue(NormalizeAtmId(atmId), out var state))
        {
            state.LastSentChecksum = checksum ?? string.Empty;
            state.SentCount++;
            state.UpdatedAtUtc = DateTime.UtcNow;
            PersistState(atmId, state);
        }
    }

    public void RecordFailure(string atmId, string atmType, string checksum, string reason)
    {
        var state = GetOrCreate(atmId, atmType);
        state.LastError = reason ?? string.Empty;
        state.LastChecksum = checksum ?? string.Empty;
        state.FailedCount++;
        state.UpdatedAtUtc = DateTime.UtcNow;
        PersistState(atmId, state);
    }

    private JournalState GetOrCreate(string atmId, string atmType)
    {
        var key = NormalizeAtmId(atmId);
        return _states.GetOrAdd(key, _ => new JournalState
        {
            ATM_ID = key,
            ATM_Type = AppConstants.NormalizeATMType(atmType),
            UpdatedAtUtc = DateTime.UtcNow
        });
    }

    private void PersistState(string atmId, JournalState state)
    {
        if (string.IsNullOrWhiteSpace(_stateRoot))
            return;

        try
        {
            Directory.CreateDirectory(_stateRoot);
            var safeId = NormalizeAtmId(atmId);
            var filePath = Path.Combine(_stateRoot, safeId + ".json");
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
        catch
        {
            // Best-effort compatibility persistence.
        }
    }

    private static string NormalizeAtmId(string atmId)
    {
        return string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim().ToUpperInvariant();
    }

    private sealed class JournalState
    {
        public string ATM_ID { get; set; } = "UNKNOWN";
        public string ATM_Type { get; set; } = AppConstants.ATM_TYPE_NCR;
        public bool IsConnected { get; set; }
        public DateTime HeartbeatUtc { get; set; }
        public string LastFilePath { get; set; } = string.Empty;
        public long LastFileSize { get; set; }
        public string LastChecksum { get; set; } = string.Empty;
        public string LastSentChecksum { get; set; } = string.Empty;
        public string LastError { get; set; } = string.Empty;
        public int DetectedCount { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
