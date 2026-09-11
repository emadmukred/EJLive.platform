using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;

namespace EJLive.Core.Engine
{
    public partial class JournalSyncRecord
        {
            public string FileName { get; set; }
    
    
            public long Size { get; set; }
    
    
            public string Status { get; set; }
    
    
            public DateTime AddedAt { get; set; }
    
    
        }
    public partial class JournalSyncStatusSnapshot
        {
            public string ATMId { get; set; }
    
    
            public List<JournalSyncRecord> Records { get; set; } = new List<JournalSyncRecord>();
    
    
            public DateTime SnapshotTime { get; set; }
    
    
        }
    public class JournalSyncTracker { public long LastOffset { get; set; } }
    public partial class JournalSyncTracker
        {
            private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
                = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
    
            private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _recordsByAtm
                = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>(StringComparer.OrdinalIgnoreCase);
    
    
            private readonly string _persistFilePath;
    
    
            private readonly object _persistLock = new();
    
    
            public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
            {
                var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                var key     = $"{fileName}|{checksum}";
    
                if (records.TryGetValue(key, out var existing))
                    return existing;
    
                // Idempotency: هل مزامن من قبل؟ (L-03)
                if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                {
                    AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                    return null;
                }
    
                var record = new JournalSyncRecord
                {
                    ATM_ID          = atmId,
                    FileName        = fileName,
                    FileSize        = fileSize,
                    FileOffset      = offset,
                    Checksum        = checksum,
                    State           = JournalSyncState.Pending,
                    ProgressPercent = 0,
                    CreatedAtUtc    = DateTime.UtcNow
                };
    
                // تسجيل في قاعدة البيانات
                DatabaseManager.Instance.InsertSyncRecord(record);
                records[key] = record;
                OnStateChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r => {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) => {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            public long LastOffset { get; set; }
    
    
            public string LastFileName { get; set; }
    
    
            public DateTime LastSyncTime { get; set; }
    
    
            public int PendingCount { get; set; }
    
    
            public int FailedCount { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
                PersistState();
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r => {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) => {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
                PersistState();
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-12\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r => {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) => {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\JournalSyncTracker.cs.v15_bak
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\JournalSyncTracker.cs.v16_bak
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\JournalSyncTracker.cs.v17_bak
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\JournalSyncTracker.cs.v20_bak
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
                PersistState();
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs.v17_bak
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs.v21_bak
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs.v23_bak
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncRecord AddOrUpdatePending(string atmId, string fileName, string localPath, long fileSize, long offset, string checksum)
            {
                if (string.IsNullOrWhiteSpace(atmId)) throw new ArgumentException("atmId is required", nameof(atmId));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));
    
                var perAtm = _recordsByAtm.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase));
                string key = BuildKey(fileName, checksum);
    
                var record = perAtm.AddOrUpdate(key,
                    _ => new JournalSyncRecord
                    {
                        ATM_ID = atmId,
                        FileName = fileName,
                        LocalPath = localPath,
                        FileSize = fileSize,
                        FileOffset = offset,
                        Checksum = checksum,
                        State = JournalSyncState.Pending,
                        ProgressPercent = 0,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.LocalPath = localPath;
                        existing.FileSize = fileSize;
                        existing.FileOffset = offset;
                        existing.Checksum = checksum;
                        if (!existing.IsFinalState)
                            existing.State = JournalSyncState.Pending;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            public JournalSyncTracker(string persistFilePath = null)
            {
                _persistFilePath = persistFilePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "journal-sync-state.json");
                LoadState();
            }
    
    
            public JournalSyncTracker(string? persistFilePath = null)
            {
                _persistFilePath = persistFilePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "journal-sync-state.json");
                LoadState();
            }
    
    
            public void MarkSyncing(string atmId, string syncId, int chunkPercent)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Syncing;
                    r.ProgressPercent = chunkPercent;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
            }
    
    
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"✓ Sync completed: {syncId}", "SyncTracker");
            }
    
    
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"✗ Sync failed: {syncId} — {reason}", "SyncTracker");
            }
    
    
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State = JournalSyncState.ReSyncing;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
            }
    
    
            public void MarkArchived(string syncId)
            {
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
            }
    
    
            public List<JournalSyncRecord> GetAllForATM(string atmId)
            {
                if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
                return new List<JournalSyncRecord>(records.Values);
            }
    
    
            public List<JournalSyncRecord> GetPendingForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
    
            public List<JournalSyncRecord> GetFailedForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
    
            public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
    
            public (int completed, int failed, int pending) GetStats(string atmId)
            {
                var all = GetAllForATM(atmId);
                return (
                    all.Count(r => r.State == JournalSyncState.Completed),
                    all.Count(r => r.State == JournalSyncState.Failed),
                    all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                );
            }
    
    
            public void LoadFromDatabase(string atmId)
            {
                var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                if (!_state.ContainsKey(atmId))
                    _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                foreach (var r in records)
                {
                    var key = $"{r.FileName}|{r.Checksum}";
                    _state[atmId][key] = r;
                }
                AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
            }
    
    
            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values)
                {
                    if (r.SyncId == syncId)
                    {
                        updater(r);
                        OnStateChanged?.Invoke(this, r);
                        return;
                    }
                }
            }
    
    
            private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values) updater(r);
            }
    
    
            private readonly ConcurrentDictionary<string, JournalSyncStatusSnapshot> _statusByAtm
                = new ConcurrentDictionary<string, JournalSyncStatusSnapshot>(StringComparer.OrdinalIgnoreCase);
    
    
            private readonly object _persistLock = new object();
    
    
            public void MarkSyncing(string atmId, string syncId, int progressPercent)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.Syncing;
                    record.ProgressPercent = progressPercent;
                    record.StartedAtUtc ??= DateTime.UtcNow;
                    record.LastAttemptAtUtc = DateTime.UtcNow;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            public void MarkCompleted(string atmId, string syncId, string archivePath = null, string sha256Hash = null)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.Completed;
                    record.ProgressPercent = 100;
                    record.ArchivePath = archivePath;
                    record.SHA256Hash = sha256Hash;
                    record.CompletedAtUtc = DateTime.UtcNow;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            public void MarkFailed(string atmId, string syncId, string message)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.Failed;
                    record.Message = message;
                    record.RetryCount++;
                    record.LastAttemptAtUtc = DateTime.UtcNow;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            public void UpdateConnectionState(string atmId, bool isConnected, DateTime? heartbeatUtc = null)
            {
                var snapshot = _statusByAtm.AddOrUpdate(atmId,
                    _ => new JournalSyncStatusSnapshot
                    {
                        ATM_ID = atmId,
                        IsConnected = isConnected,
                        LastHeartbeatUtc = heartbeatUtc,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    (_, existing) =>
                    {
                        existing.IsConnected = isConnected;
                        existing.LastHeartbeatUtc = heartbeatUtc ?? existing.LastHeartbeatUtc;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                        return existing;
                    });
    
                OnStatusChanged?.Invoke(this, snapshot);
                PersistState();
            }
    
    
            public void UpsertStatusSnapshot(JournalSyncStatusSnapshot snapshot)
            {
                if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.ATM_ID))
                    return;
    
                snapshot.UpdatedAtUtc = DateTime.UtcNow;
                _statusByAtm[snapshot.ATM_ID] = snapshot;
                PersistState();
                OnStatusChanged?.Invoke(this, snapshot);
            }
    
    
            public IReadOnlyList<JournalSyncRecord> GetRecordsForAtm(string atmId)
            {
                if (!_recordsByAtm.TryGetValue(atmId, out var records))
                    return Array.Empty<JournalSyncRecord>();
                return records.Values.OrderByDescending(r => r.UpdatedAtUtc).ToList();
            }
    
    
            public IReadOnlyList<JournalSyncStatusSnapshot> GetAllStatuses()
            {
                return _statusByAtm.Values.OrderBy(s => s.ATM_ID).ToList();
            }
    
    
            public JournalSyncStatusSnapshot GetStatus(string atmId)
            {
                _statusByAtm.TryGetValue(atmId, out var snapshot);
                return snapshot;
            }
    
    
            private void UpdateRecord(string atmId, string syncId, Action<JournalSyncRecord> update)
            {
                if (!_recordsByAtm.TryGetValue(atmId, out var records))
                    return;
    
                var record = records.Values.FirstOrDefault(r => string.Equals(r.SyncId, syncId, StringComparison.OrdinalIgnoreCase));
                if (record == null)
                    return;
    
                update(record);
                UpdateStatusSnapshot(atmId);
                PersistState();
                OnRecordChanged?.Invoke(this, record);
            }
    
    
            private void UpdateStatusSnapshot(string atmId)
            {
                var records = GetRecordsForAtm(atmId);
                var latestCompleted = records.Where(r => r.State == JournalSyncState.Completed).OrderByDescending(r => r.CompletedAtUtc).FirstOrDefault();
                var latestError = records.Where(r => r.State == JournalSyncState.Failed && !string.IsNullOrWhiteSpace(r.Message)).OrderByDescending(r => r.UpdatedAtUtc).FirstOrDefault();
    
                var snapshot = _statusByAtm.AddOrUpdate(atmId,
                    _ => new JournalSyncStatusSnapshot { ATM_ID = atmId },
                    (_, existing) => existing);
    
                snapshot.PendingFiles = records.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing);
                snapshot.SyncingFiles = records.Count(r => r.State == JournalSyncState.Syncing);
                snapshot.FailedFiles = records.Count(r => r.State == JournalSyncState.Failed);
                snapshot.CompletedFiles = records.Count(r => r.State == JournalSyncState.Completed || r.State == JournalSyncState.Archived);
                snapshot.PendingBytes = records.Where(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing || r.State == JournalSyncState.Syncing).Sum(r => r.FileSize);
                snapshot.LastJournalSyncUtc = latestCompleted?.CompletedAtUtc;
                snapshot.LastError = latestError?.Message;
                snapshot.UpdatedAtUtc = DateTime.UtcNow;
    
                OnStatusChanged?.Invoke(this, snapshot);
            }
    
    
            private void PersistState()
            {
                lock (_persistLock)
                {
                    var state = new PersistedJournalSyncState
                    {
                        Records = _recordsByAtm.ToDictionary(k => k.Key, v => v.Value.Values.ToList(), StringComparer.OrdinalIgnoreCase),
                        Statuses = _statusByAtm.Values.ToList()
                    };
    
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    File.WriteAllText(_persistFilePath, JsonSerializer.Serialize(state, options));
                }
            }
    
    
            private void LoadState()
            {
                if (!File.Exists(_persistFilePath))
                    return;
    
                try
                {
                    var content = File.ReadAllText(_persistFilePath);
                    var state = JsonSerializer.Deserialize<PersistedJournalSyncState>(content);
                    if (state == null)
                        return;
    
                    if (state.Records != null)
                    {
                        foreach (var pair in state.Records)
                        {
                            var bucket = new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase);
                            foreach (var record in pair.Value ?? new List<JournalSyncRecord>())
                                bucket[BuildKey(record.FileName, record.Checksum)] = record;
                            _recordsByAtm[pair.Key] = bucket;
                        }
                    }
    
                    if (state.Statuses != null)
                    {
                        foreach (var snapshot in state.Statuses)
                            _statusByAtm[snapshot.ATM_ID] = snapshot;
                    }
                }
                catch
                {
                }
            }
    
    
            private static string BuildKey(string fileName, string checksum)
            {
                return (fileName ?? string.Empty) + "|" + (checksum ?? string.Empty);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r => {
                    r.State = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash = sha256;
                    r.CompletedAtUtc = DateTime.UtcNow;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r => {
                    r.State = JournalSyncState.Failed;
                    r.Message = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record => {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            private readonly Dictionary<string, JournalSyncRecord> _records = new Dictionary<string, JournalSyncRecord>();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Engine\JournalSyncTracker.cs
            private readonly object _lock = new object();
    
    
            public JournalSyncRecord BeginLocalSave(string atmId, string fileName, long fileSize, string checksum, string localPath = null, string syncId = null)
            {
                var record = new JournalSyncRecord
                {
                    SyncId = string.IsNullOrWhiteSpace(syncId) ? Guid.NewGuid().ToString("N") : syncId,
                    ATM_ID = atmId,
                    FileName = fileName,
                    FileSize = fileSize,
                    Checksum = checksum,
                    LocalPath = localPath,
                    State = JournalSyncState.LocalSaving,
                    ProgressPercent = 10,
                    Message = "Local journal copy is being prepared"
                };
                Save(record);
                return record;
            }
    
    
            public JournalSyncRecord MarkPending(string syncId, string message = "Waiting for server connection")
            {
                return Update(syncId, JournalSyncState.Pending, 25, message);
            }
    
    
            public JournalSyncRecord MarkSyncing(string syncId, int progressPercent = 50, string message = "Journal is being sent to server")
            {
                return Update(syncId, JournalSyncState.Syncing, progressPercent, message);
            }
    
    
            public JournalSyncRecord MarkReSyncing(string syncId, int retryCount, string message = "Connection interrupted; journal will be re-synced")
            {
                JournalSyncRecord record = null;
                lock (_lock)
                {
                    if (_records.TryGetValue(syncId, out record))
                    {
                        record.RetryCount = retryCount;
                    }
                }
                return Update(syncId, JournalSyncState.ReSyncing, 35, message);
            }
    
    
            public JournalSyncRecord MarkStored(string syncId, string serverPath)
            {
                JournalSyncRecord record = null;
                lock (_lock)
                {
                    if (_records.TryGetValue(syncId, out record))
                    {
                        record.ServerPath = serverPath;
                    }
                }
                return Update(syncId, JournalSyncState.StoredOnServer, 90, "Journal stored on server");
            }
    
    
            public JournalSyncRecord MarkCompleted(string syncId, string message = "Journal sync completed")
            {
                return Update(syncId, JournalSyncState.Completed, 100, message);
            }
    
    
            public JournalSyncRecord MarkFailed(string syncId, string message)
            {
                return Update(syncId, JournalSyncState.Failed, 0, message);
            }
    
    
            public JournalSyncRecord TrackServerReceive(string atmId, string fileName, long fileSize, string checksum, string serverPath)
            {
                var record = BeginLocalSave(atmId, fileName, fileSize, checksum, null);
                record.ServerPath = serverPath;
                record.State = JournalSyncState.StoredOnServer;
                record.ProgressPercent = 100;
                record.Message = "Journal received and stored on server";
                Save(record);
                return record;
            }
    
    
            public List<JournalSyncRecord> GetRecords()
            {
                lock (_lock)
                {
                    return _records.Values.OrderByDescending(r => r.UpdatedAtUtc).ToList();
                }
            }
    
    
            public string ExportCsv(string outputPath)
            {
                var records = GetRecords();
                Directory.CreateDirectory(outputPath);
                string filePath = Path.Combine(outputPath, "JournalSync_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".csv");
                using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("SyncId,ATM_ID,FileName,FileSize,Checksum,State,ProgressPercent,RetryCount,CreatedAtUtc,UpdatedAtUtc,LocalPath,ServerPath,Message");
                    foreach (var r in records)
                    {
                        writer.WriteLine(string.Join(",", new[]
                        {
                            Csv(r.SyncId),
                            Csv(r.ATM_ID),
                            Csv(r.FileName),
                            r.FileSize.ToString(),
                            Csv(r.Checksum),
                            Csv(r.State.ToString()),
                            r.ProgressPercent.ToString(),
                            r.RetryCount.ToString(),
                            Csv(r.CreatedAtUtc.ToString("O")),
                            Csv(r.UpdatedAtUtc.ToString("O")),
                            Csv(r.LocalPath),
                            Csv(r.ServerPath),
                            Csv(r.Message)
                        }));
                    }
                }
                return filePath;
            }
    
    
            private JournalSyncRecord Update(string syncId, JournalSyncState state, int progressPercent, string message)
            {
                JournalSyncRecord record = null;
                lock (_lock)
                {
                    if (!_records.TryGetValue(syncId, out record))
                    {
                        return null;
                    }
    
                    record.State = state;
                    record.ProgressPercent = Math.Max(0, Math.Min(100, progressPercent));
                    record.Message = message;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                }
                OnRecordChanged?.Invoke(record);
                return record;
            }
    
    
            private void Save(JournalSyncRecord record)
            {
                lock (_lock)
                {
                    record.UpdatedAtUtc = DateTime.UtcNow;
                    _records[record.SyncId] = record;
                }
                OnRecordChanged?.Invoke(record);
            }
    
    
            private static string Csv(string value)
            {
                value = value ?? string.Empty;
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Services\JournalSyncTracker.cs
            private void PersistState()
            {
                // Persistence is intentionally deferred for the current
                // wiring-first stabilization pass so the tracker does not depend on
                // additional JSON packages during local .NET Framework builds.
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Services\JournalSyncTracker.cs
            private void LoadState()
            {
                // No-op for the current package. Runtime state still works in
                // memory and this keeps the project lighter for Visual Studio
                // verification on Windows.
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\JournalSyncTracker.cs
            private readonly object _lock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\JournalSyncTracker.cs
            private readonly object _lock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            public void MarkCompleted(string atmId, string syncId, string? sha256 = null, string? archivePath = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256 ?? string.Empty;
                    r.ArchivePath     = archivePath;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\JournalSyncTracker.cs
            public IReadOnlyList<JournalSyncStatusSnapshot> GetAllStatuses()
                => _statusByAtm.Values.OrderBy(s => s.ATM_ID).ToList();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncStatusSnapshot? GetStatus(string atmId)
            {
                _statusByAtm.TryGetValue(atmId, out var snapshot);
                return snapshot;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\JournalSyncTracker.cs
            private void PersistState()
            {
                lock (_persistLock)
                {
                    var state = new PersistedJournalSyncState
                    {
                        Records = _state.ToDictionary(k => k.Key, v => v.Value.Values.ToList(), StringComparer.OrdinalIgnoreCase),
                        Statuses = _statusByAtm.Values.ToList()
                    };
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    File.WriteAllText(_persistFilePath, JsonSerializer.Serialize(state, options));
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\JournalSyncTracker.cs
            private void LoadState()
            {
                if (!File.Exists(_persistFilePath)) return;
                try
                {
                    var content = File.ReadAllText(_persistFilePath);
                    var state = JsonSerializer.Deserialize<PersistedJournalSyncState>(content);
                    if (state == null) return;
                    if (state.Records != null)
                    {
                        foreach (var pair in state.Records)
                        {
                            var bucket = new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase);
                            foreach (var record in pair.Value ?? new List<JournalSyncRecord>())
                                bucket[BuildKey(record.FileName, record.Checksum)] = record;
                            _state[pair.Key] = bucket;
                        }
                    }
                    if (state.Statuses != null)
                    {
                        foreach (var snapshot in state.Statuses)
                            _statusByAtm[snapshot.ATM_ID] = snapshot;
                    }
                }
                catch { /* best-effort */ }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\JournalSyncTracker.cs
            private static string BuildKey(string fileName, string checksum)
                => (fileName ?? string.Empty) + "|" + (checksum ?? string.Empty);
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\JournalSyncTracker.cs
            public IReadOnlyList<JournalSyncRecord> GetRecordsForAtm(string atmId)
            {
                if (!_state.TryGetValue(atmId, out var records))
                    return Array.Empty<JournalSyncRecord>();
                return records.Values.OrderByDescending(r => r.UpdatedAtUtc).ToList();
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\JournalSyncTracker.cs
            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values)
                {
                    if (r.SyncId == syncId)
                    {
                        updater(r);
                        UpdateStatusSnapshot(atmId);
                        PersistState();
                        OnStateChanged?.Invoke(this, r);
                        return;
                    }
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            public IReadOnlyList<JournalSyncStatusSnapshot> GetAllStatuses()
                => _statusByAtm.Values.OrderBy(s => s.ATM_ID).ToList();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            public JournalSyncStatusSnapshot? GetStatus(string atmId)
            {
                _statusByAtm.TryGetValue(atmId, out var snapshot);
                return snapshot;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            private void PersistState()
            {
                // Persistence is intentionally deferred for the current
                // wiring-first stabilization pass so the tracker does not depend on
                // additional JSON packages during local .NET Framework builds.
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            private void LoadState()
            {
                // No-op for the current package. Runtime state still works in
                // memory and this keeps the project lighter for Visual Studio
                // verification on Windows.
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            private static string BuildKey(string fileName, string checksum)
                => (fileName ?? string.Empty) + "|" + (checksum ?? string.Empty);
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            public IReadOnlyList<JournalSyncRecord> GetRecordsForAtm(string atmId)
            {
                if (!_state.TryGetValue(atmId, out var records))
                    return Array.Empty<JournalSyncRecord>();
                return records.Values.OrderByDescending(r => r.UpdatedAtUtc).ToList();
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values)
                {
                    if (r.SyncId == syncId)
                    {
                        updater(r);
                        UpdateStatusSnapshot(atmId);
                        PersistState();
                        OnStateChanged?.Invoke(this, r);
                        return;
                    }
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            private readonly object _lock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r => {
                    r.State = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash = sha256;
                    r.CompletedAtUtc = DateTime.UtcNow;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r => {
                    r.State = JournalSyncState.Failed;
                    r.Message = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record => {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Engine\JournalSyncTracker.cs
            private readonly object _lock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Services\JournalSyncTracker.cs
            private void PersistState()
            {
                // Persistence is intentionally deferred for the current
                // wiring-first stabilization pass so the tracker does not depend on
                // additional JSON packages during local .NET Framework builds.
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Services\JournalSyncTracker.cs
            private void LoadState()
            {
                // No-op for the current package. Runtime state still works in
                // memory and this keeps the project lighter for Visual Studio
                // verification on Windows.
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\JournalSyncTracker.cs
            private readonly object _lock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\JournalSyncTracker.cs
            private readonly object _lock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\JournalSyncTracker.cs
            public IReadOnlyList<JournalSyncStatusSnapshot> GetAllStatuses()
                => _statusByAtm.Values.OrderBy(s => s.ATM_ID).ToList();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncStatusSnapshot? GetStatus(string atmId)
            {
                _statusByAtm.TryGetValue(atmId, out var snapshot);
                return snapshot;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\JournalSyncTracker.cs
            private void PersistState()
            {
                lock (_persistLock)
                {
                    var state = new PersistedJournalSyncState
                    {
                        Records = _state.ToDictionary(k => k.Key, v => v.Value.Values.ToList(), StringComparer.OrdinalIgnoreCase),
                        Statuses = _statusByAtm.Values.ToList()
                    };
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    File.WriteAllText(_persistFilePath, JsonSerializer.Serialize(state, options));
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\JournalSyncTracker.cs
            private void LoadState()
            {
                if (!File.Exists(_persistFilePath)) return;
                try
                {
                    var content = File.ReadAllText(_persistFilePath);
                    var state = JsonSerializer.Deserialize<PersistedJournalSyncState>(content);
                    if (state == null) return;
                    if (state.Records != null)
                    {
                        foreach (var pair in state.Records)
                        {
                            var bucket = new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase);
                            foreach (var record in pair.Value ?? new List<JournalSyncRecord>())
                                bucket[BuildKey(record.FileName, record.Checksum)] = record;
                            _state[pair.Key] = bucket;
                        }
                    }
                    if (state.Statuses != null)
                    {
                        foreach (var snapshot in state.Statuses)
                            _statusByAtm[snapshot.ATM_ID] = snapshot;
                    }
                }
                catch { /* best-effort */ }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\JournalSyncTracker.cs
            private static string BuildKey(string fileName, string checksum)
                => (fileName ?? string.Empty) + "|" + (checksum ?? string.Empty);
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\JournalSyncTracker.cs
            public IReadOnlyList<JournalSyncRecord> GetRecordsForAtm(string atmId)
            {
                if (!_state.TryGetValue(atmId, out var records))
                    return Array.Empty<JournalSyncRecord>();
                return records.Values.OrderByDescending(r => r.UpdatedAtUtc).ToList();
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\JournalSyncTracker.cs
            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values)
                {
                    if (r.SyncId == syncId)
                    {
                        updater(r);
                        UpdateStatusSnapshot(atmId);
                        PersistState();
                        OnStateChanged?.Invoke(this, r);
                        return;
                    }
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Services\JournalSyncTracker.cs
            private void PersistState()
            {
                // Persistence is intentionally deferred for the current
                // wiring-first stabilization pass so the tracker does not depend on
                // additional JSON packages during local .NET Framework builds.
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Services\JournalSyncTracker.cs
            private void LoadState()
            {
                // No-op for the current package. Runtime state still works in
                // memory and this keeps the project lighter for Visual Studio
                // verification on Windows.
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\JournalSyncTracker.cs
            private readonly object _lock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Engine\JournalSyncTracker.cs
            private readonly object _lock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Engine\JournalSyncTracker.cs
            private readonly object _lock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r => {
                    r.State = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash = sha256;
                    r.CompletedAtUtc = DateTime.UtcNow;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r => {
                    r.State = JournalSyncState.Failed;
                    r.Message = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record => {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\JournalSyncTracker.cs
            private readonly object _lock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\JournalSyncTracker.cs.v15_bak
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\JournalSyncTracker.cs.v15_bak
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\JournalSyncTracker.cs.v16_bak
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\JournalSyncTracker.cs.v16_bak
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\JournalSyncTracker.cs.v17_bak
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\JournalSyncTracker.cs.v17_bak
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\JournalSyncTracker.cs.v20_bak
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\JournalSyncTracker.cs.v20_bak
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs
            public IReadOnlyList<JournalSyncStatusSnapshot> GetAllStatuses()
                => _statusByAtm.Values.OrderBy(s => s.ATM_ID).ToList();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs
            public JournalSyncStatusSnapshot? GetStatus(string atmId)
            {
                _statusByAtm.TryGetValue(atmId, out var snapshot);
                return snapshot;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs
            private void PersistState()
            {
                lock (_persistLock)
                {
                    var state = new PersistedJournalSyncState
                    {
                        Records = _state.ToDictionary(k => k.Key, v => v.Value.Values.ToList(), StringComparer.OrdinalIgnoreCase),
                        Statuses = _statusByAtm.Values.ToList()
                    };
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    File.WriteAllText(_persistFilePath, JsonSerializer.Serialize(state, options));
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs
            private void LoadState()
            {
                if (!File.Exists(_persistFilePath)) return;
                try
                {
                    var content = File.ReadAllText(_persistFilePath);
                    var state = JsonSerializer.Deserialize<PersistedJournalSyncState>(content);
                    if (state == null) return;
                    if (state.Records != null)
                    {
                        foreach (var pair in state.Records)
                        {
                            var bucket = new ConcurrentDictionary<string, JournalSyncRecord>(StringComparer.OrdinalIgnoreCase);
                            foreach (var record in pair.Value ?? new List<JournalSyncRecord>())
                                bucket[BuildKey(record.FileName, record.Checksum)] = record;
                            _state[pair.Key] = bucket;
                        }
                    }
                    if (state.Statuses != null)
                    {
                        foreach (var snapshot in state.Statuses)
                            _statusByAtm[snapshot.ATM_ID] = snapshot;
                    }
                }
                catch { /* best-effort */ }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs
            private static string BuildKey(string fileName, string checksum)
                => (fileName ?? string.Empty) + "|" + (checksum ?? string.Empty);
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs
            public IReadOnlyList<JournalSyncRecord> GetRecordsForAtm(string atmId)
            {
                if (!_state.TryGetValue(atmId, out var records))
                    return Array.Empty<JournalSyncRecord>();
                return records.Values.OrderByDescending(r => r.UpdatedAtUtc).ToList();
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs
            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values)
                {
                    if (r.SyncId == syncId)
                    {
                        updater(r);
                        UpdateStatusSnapshot(atmId);
                        PersistState();
                        OnStateChanged?.Invoke(this, r);
                        return;
                    }
                }
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs.v17_bak
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs.v21_bak
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs.v23_bak
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Services\JournalSyncTracker.cs
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateRecord(atmId, syncId, record =>
                {
                    record.State = JournalSyncState.ReSyncing;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                });
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            public event EventHandler<JournalSyncRecord> OnStateChanged;
    
    
            public event EventHandler<JournalSyncRecord> OnRecordChanged;
    
    
            public event EventHandler<JournalSyncStatusSnapshot> OnStatusChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Engine\JournalSyncTracker.cs
            public event Action<JournalSyncRecord> OnRecordChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\JournalSyncTracker.cs
            public event Action<JournalSyncRecord> OnRecordChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public event Action<JournalSyncRecord> OnRecordChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\JournalSyncTracker.cs
            public event EventHandler<JournalSyncRecord>? OnStateChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\JournalSyncTracker.cs
            public event EventHandler<JournalSyncStatusSnapshot>? OnStatusChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            public event EventHandler<JournalSyncRecord>? OnStateChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            public event EventHandler<JournalSyncStatusSnapshot>? OnStatusChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            public event Action<JournalSyncRecord> OnRecordChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Engine\JournalSyncTracker.cs
            public event Action<JournalSyncRecord> OnRecordChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\JournalSyncTracker.cs
            public event Action<JournalSyncRecord> OnRecordChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public event Action<JournalSyncRecord> OnRecordChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\JournalSyncTracker.cs
            public event EventHandler<JournalSyncRecord>? OnStateChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\JournalSyncTracker.cs
            public event EventHandler<JournalSyncStatusSnapshot>? OnStatusChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\JournalSyncTracker.cs
            public event Action<JournalSyncRecord> OnRecordChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Engine\JournalSyncTracker.cs
            public event Action<JournalSyncRecord> OnRecordChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Engine\JournalSyncTracker.cs
            public event Action<JournalSyncRecord> OnRecordChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public event Action<JournalSyncRecord> OnRecordChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs
            public event EventHandler<JournalSyncRecord>? OnStateChanged;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs
            public event EventHandler<JournalSyncStatusSnapshot>? OnStatusChanged;
    
    
            private sealed class PersistedJournalSyncState
            {
                public Dictionary<string, List<JournalSyncRecord>> Records { get; set; } = new Dictionary<string, List<JournalSyncRecord>>(StringComparer.OrdinalIgnoreCase);
                public List<JournalSyncStatusSnapshot> Statuses { get; set; } = new List<JournalSyncStatusSnapshot>();
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\JournalSyncTracker.cs
            private sealed class PersistedJournalSyncState
            {
                public Dictionary<string, List<JournalSyncRecord>> Records { get; set; } = new(StringComparer.OrdinalIgnoreCase);
                public List<JournalSyncStatusSnapshot> Statuses { get; set; } = new();
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncTracker.cs
            private sealed class PersistedJournalSyncState
            {
                public Dictionary<string, List<JournalSyncRecord>> Records { get; set; } = new(StringComparer.OrdinalIgnoreCase);
                public List<JournalSyncStatusSnapshot> Statuses { get; set; } = new();
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\JournalSyncTracker.cs
            private sealed class PersistedJournalSyncState
            {
                public Dictionary<string, List<JournalSyncRecord>> Records { get; set; } = new(StringComparer.OrdinalIgnoreCase);
                public List<JournalSyncStatusSnapshot> Statuses { get; set; } = new();
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncTracker.cs
            private sealed class PersistedJournalSyncState
            {
                public Dictionary<string, List<JournalSyncRecord>> Records { get; set; } = new(StringComparer.OrdinalIgnoreCase);
                public List<JournalSyncStatusSnapshot> Statuses { get; set; } = new();
            }
    
    
        }

    public class JournalSyncTracker
        {
            private readonly Dictionary<string, JournalSyncRecord> _records = new Dictionary<string, JournalSyncRecord>();
            private readonly object _lock = new object();
    
            public event Action<JournalSyncRecord> OnRecordChanged;
    
            public JournalSyncRecord BeginLocalSave(string atmId, string fileName, long fileSize, string checksum, string localPath = null, string syncId = null)
            {
                var record = new JournalSyncRecord
                {
                    SyncId = string.IsNullOrWhiteSpace(syncId) ? Guid.NewGuid().ToString("N") : syncId,
                    ATM_ID = atmId,
                    FileName = fileName,
                    FileSize = fileSize,
                    Checksum = checksum,
                    LocalPath = localPath,
                    State = JournalSyncState.LocalSaving,
                    ProgressPercent = 10,
                    Message = "Local journal copy is being prepared"
                };
                Save(record);
                return record;
            }
    
            public JournalSyncRecord MarkPending(string syncId, string message = "Waiting for server connection")
            {
                return Update(syncId, JournalSyncState.Pending, 25, message);
            }
    
            public JournalSyncRecord MarkSyncing(string syncId, int progressPercent = 50, string message = "Journal is being sent to server")
            {
                return Update(syncId, JournalSyncState.Syncing, progressPercent, message);
            }
    
            public JournalSyncRecord MarkReSyncing(string syncId, int retryCount, string message = "Connection interrupted; journal will be re-synced")
            {
                JournalSyncRecord record = null;
                lock (_lock)
                {
                    if (_records.TryGetValue(syncId, out record))
                    {
                        record.RetryCount = retryCount;
                    }
                }
                return Update(syncId, JournalSyncState.ReSyncing, 35, message);
            }
    
            public JournalSyncRecord MarkStored(string syncId, string serverPath)
            {
                JournalSyncRecord record = null;
                lock (_lock)
                {
                    if (_records.TryGetValue(syncId, out record))
                    {
                        record.ServerPath = serverPath;
                    }
                }
                return Update(syncId, JournalSyncState.StoredOnServer, 90, "Journal stored on server");
            }
    
            public JournalSyncRecord MarkCompleted(string syncId, string message = "Journal sync completed")
            {
                return Update(syncId, JournalSyncState.Completed, 100, message);
            }
    
            public JournalSyncRecord MarkFailed(string syncId, string message)
            {
                return Update(syncId, JournalSyncState.Failed, 0, message);
            }
    
            public JournalSyncRecord TrackServerReceive(string atmId, string fileName, long fileSize, string checksum, string serverPath)
            {
                var record = BeginLocalSave(atmId, fileName, fileSize, checksum, null);
                record.ServerPath = serverPath;
                record.State = JournalSyncState.StoredOnServer;
                record.ProgressPercent = 100;
                record.Message = "Journal received and stored on server";
                Save(record);
                return record;
            }
    
            public List<JournalSyncRecord> GetRecords()
            {
                lock (_lock)
                {
                    return _records.Values.OrderByDescending(r => r.UpdatedAtUtc).ToList();
                }
            }
    
            public string ExportCsv(string outputPath)
            {
                var records = GetRecords();
                Directory.CreateDirectory(outputPath);
                string filePath = Path.Combine(outputPath, "JournalSync_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".csv");
                using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("SyncId,ATM_ID,FileName,FileSize,Checksum,State,ProgressPercent,RetryCount,CreatedAtUtc,UpdatedAtUtc,LocalPath,ServerPath,Message");
                    foreach (var r in records)
                    {
                        writer.WriteLine(string.Join(",", new[]
                        {
                            Csv(r.SyncId),
                            Csv(r.ATM_ID),
                            Csv(r.FileName),
                            r.FileSize.ToString(),
                            Csv(r.Checksum),
                            Csv(r.State.ToString()),
                            r.ProgressPercent.ToString(),
                            r.RetryCount.ToString(),
                            Csv(r.CreatedAtUtc.ToString("O")),
                            Csv(r.UpdatedAtUtc.ToString("O")),
                            Csv(r.LocalPath),
                            Csv(r.ServerPath),
                            Csv(r.Message)
                        }));
                    }
                }
                return filePath;
            }
    
            private JournalSyncRecord Update(string syncId, JournalSyncState state, int progressPercent, string message)
            {
                JournalSyncRecord record = null;
                lock (_lock)
                {
                    if (!_records.TryGetValue(syncId, out record))
                    {
                        return null;
                    }
    
                    record.State = state;
                    record.ProgressPercent = Math.Max(0, Math.Min(100, progressPercent));
                    record.Message = message;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                }
                OnRecordChanged?.Invoke(record);
                return record;
            }
    
            private void Save(JournalSyncRecord record)
            {
                lock (_lock)
                {
                    record.UpdatedAtUtc = DateTime.UtcNow;
                    _records[record.SyncId] = record;
                }
                OnRecordChanged?.Invoke(record);
            }
    
            private static string Csv(string value)
            {
                value = value ?? string.Empty;
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
        }
    /// <summary>
        /// متتبع حالة المزامنة الكامل — JournalSyncTracker
        /// يدير آلة الحالة: Pending → Syncing → [Completed | Failed → ReSyncing]
        /// يضمن: Idempotency (L-03), عدم فقدان السجلات (L-01), إعادة الإرسال (L-05)
        /// يوفر: GetPendingForATM, MarkSyncing, MarkCompleted, MarkFailed, ScheduleResync
        /// </summary>
        public class JournalSyncTracker
        {
            // حالة في الذاكرة لكل صراف
            private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
                = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
            public event EventHandler<JournalSyncRecord> OnStateChanged;
    
            // ==========================================
            // إضافة سجل جديد
            // ==========================================
    
            public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
            {
                var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                var key     = $"{fileName}|{checksum}";
    
                if (records.TryGetValue(key, out var existing))
                    return existing;
    
                // Idempotency: هل مزامن من قبل؟ (L-03)
                if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                {
                    AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                    return null;
                }
    
                var record = new JournalSyncRecord
                {
                    ATM_ID          = atmId,
                    FileName        = fileName,
                    FileSize        = fileSize,
                    FileOffset      = offset,
                    Checksum        = checksum,
                    State           = JournalSyncState.Pending,
                    ProgressPercent = 0,
                    CreatedAtUtc    = DateTime.UtcNow
                };
    
                // تسجيل في قاعدة البيانات
                DatabaseManager.Instance.InsertSyncRecord(record);
                records[key] = record;
                OnStateChanged?.Invoke(this, record);
                return record;
            }
    
            // ==========================================
            // تحديث الحالة
            // ==========================================
    
            public void MarkSyncing(string atmId, string syncId, int chunkPercent)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Syncing;
                    r.ProgressPercent = chunkPercent;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
            }
    
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State = JournalSyncState.ReSyncing;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
            }
    
            public void MarkArchived(string syncId)
            {
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
            }
    
            // ==========================================
            // الاستعلام
            // ==========================================
    
            public List<JournalSyncRecord> GetAllForATM(string atmId)
            {
                if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
                return new List<JournalSyncRecord>(records.Values);
            }
    
            public List<JournalSyncRecord> GetPendingForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
            public List<JournalSyncRecord> GetFailedForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
            public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
            public (int completed, int failed, int pending) GetStats(string atmId)
            {
                var all = GetAllForATM(atmId);
                return (
                    all.Count(r => r.State == JournalSyncState.Completed),
                    all.Count(r => r.State == JournalSyncState.Failed),
                    all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                );
            }
    
            // ==========================================
            // تحميل من قاعدة البيانات
            // ==========================================
    
            public void LoadFromDatabase(string atmId)
            {
                var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                if (!_state.ContainsKey(atmId))
                    _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                foreach (var r in records)
                {
                    var key = $"{r.FileName}|{r.Checksum}";
                    _state[atmId][key] = r;
                }
                AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
            }
    
            // مساعد تحديث الحالة
            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values)
                {
                    if (r.SyncId == syncId)
                    {
                        updater(r);
                        OnStateChanged?.Invoke(this, r);
                        return;
                    }
                }
            }
    
            // إعادة ضبط حالة ATM بعد إعادة الاتصال
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
            private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values) updater(r);
            }
        }
    // Class: JournalSyncTracker (from 5 sources)
        public partial class JournalSyncTracker
        {
            // --- Constants & Fields ---
                    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
                        = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
    
            // --- Properties ---
                    public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
                    {
                        var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                        var key     = $"{fileName}|{checksum}";
    
                        if (records.TryGetValue(key, out var existing))
                            return existing;
    
                        // Idempotency: هل مزامن من قبل؟ (L-03)
                        if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                        {
                            AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                            return null;
                        }
    
                        var record = new JournalSyncRecord
                        {
                            ATM_ID          = atmId,
                            FileName        = fileName,
                            FileSize        = fileSize,
                            FileOffset      = offset,
                            Checksum        = checksum,
                            State           = JournalSyncState.Pending,
                            ProgressPercent = 0,
                            CreatedAtUtc    = DateTime.UtcNow
                        };
    
                        // تسجيل في قاعدة البيانات
                        DatabaseManager.Instance.InsertSyncRecord(record);
                        records[key] = record;
                        OnStateChanged?.Invoke(this, record);
                        return record;
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
    
            // --- Methods ---
                    public void MarkSyncing(string atmId, string syncId, int chunkPercent)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State           = JournalSyncState.Syncing;
                            r.ProgressPercent = chunkPercent;
                            r.UpdatedAtUtc    = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
                    }
    
                    public void MarkCompleted(string atmId, string syncId, string sha256 = null)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State           = JournalSyncState.Completed;
                            r.ProgressPercent = 100;
                            r.SHA256Hash      = sha256;
                            r.CompletedAtUtc  = DateTime.UtcNow;
                            r.UpdatedAtUtc    = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                        AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
                    }
    
                    public void MarkFailed(string atmId, string syncId, string reason)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State       = JournalSyncState.Failed;
                            r.Message     = reason;
                            r.RetryCount++;
                            r.UpdatedAtUtc = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                        AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
                    }
    
                    public void MarkReSyncing(string atmId, string syncId)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State = JournalSyncState.ReSyncing;
                            r.UpdatedAtUtc = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
                    }
    
                    public void MarkArchived(string syncId)
                    {
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
                    }
    
                    public List<JournalSyncRecord> GetAllForATM(string atmId)
                    {
                        if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
                        return new List<JournalSyncRecord>(records.Values);
                    }
    
                    public List<JournalSyncRecord> GetPendingForATM(string atmId)
                        => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
                    public List<JournalSyncRecord> GetFailedForATM(string atmId)
                        => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
                    public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
                    public (int completed, int failed, int pending) GetStats(string atmId)
                    {
                        var all = GetAllForATM(atmId);
                        return (
                            all.Count(r => r.State == JournalSyncState.Completed),
                            all.Count(r => r.State == JournalSyncState.Failed),
                            all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                        );
                    }
    
                    public void LoadFromDatabase(string atmId)
                    {
                        var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                        if (!_state.ContainsKey(atmId))
                            _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                        foreach (var r in records)
                        {
                            var key = $"{r.FileName}|{r.Checksum}";
                            _state[atmId][key] = r;
                        }
                        AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
                    }
    
                    private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
                    {
                        if (!_state.TryGetValue(atmId, out var records)) return;
                        foreach (var r in records.Values)
                        {
                            if (r.SyncId == syncId)
                            {
                                updater(r);
                                OnStateChanged?.Invoke(this, r);
                                return;
                            }
                        }
                    }
    
                    private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
                    {
                        if (!_state.TryGetValue(atmId, out var records)) return;
                        foreach (var r in records.Values) updater(r);
                    }
    
    
            // --- Events ---
                    public event EventHandler<JournalSyncRecord> OnStateChanged;
    
    
        }
    // Class: JournalSyncTracker (from 5 sources)
        public partial class JournalSyncTracker
        {
            // --- Constants & Fields ---
                    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
                        = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
    
            // --- Properties ---
                    public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
                    {
                        var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                        var key     = $"{fileName}|{checksum}";
    
                        if (records.TryGetValue(key, out var existing))
                            return existing;
    
                        // Idempotency: هل مزامن من قبل؟ (L-03)
                        if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                        {
                            AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                            return null;
                        }
    
                        var record = new JournalSyncRecord
                        {
                            ATM_ID          = atmId,
                            FileName        = fileName,
                            FileSize        = fileSize,
                            FileOffset      = offset,
                            Checksum        = checksum,
                            State           = JournalSyncState.Pending,
                            ProgressPercent = 0,
                            CreatedAtUtc    = DateTime.UtcNow
                        };
    
                        // تسجيل في قاعدة البيانات
                        DatabaseManager.Instance.InsertSyncRecord(record);
                        records[key] = record;
                        OnStateChanged?.Invoke(this, record);
                        return record;
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\JournalSyncTracker.cs.v20_bak
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\JournalSyncTracker.cs.v17_bak
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\JournalSyncTracker.cs.v16_bak
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\JournalSyncTracker.cs.v15_bak
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
    
            // --- Methods ---
                    public void MarkSyncing(string atmId, string syncId, int chunkPercent)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State           = JournalSyncState.Syncing;
                            r.ProgressPercent = chunkPercent;
                            r.UpdatedAtUtc    = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
                    }
    
                    public void MarkCompleted(string atmId, string syncId, string sha256 = null)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State           = JournalSyncState.Completed;
                            r.ProgressPercent = 100;
                            r.SHA256Hash      = sha256;
                            r.CompletedAtUtc  = DateTime.UtcNow;
                            r.UpdatedAtUtc    = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                        AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
                    }
    
                    public void MarkFailed(string atmId, string syncId, string reason)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State       = JournalSyncState.Failed;
                            r.Message     = reason;
                            r.RetryCount++;
                            r.UpdatedAtUtc = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                        AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
                    }
    
                    public void MarkReSyncing(string atmId, string syncId)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State = JournalSyncState.ReSyncing;
                            r.UpdatedAtUtc = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
                    }
    
                    public void MarkArchived(string syncId)
                    {
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
                    }
    
                    public List<JournalSyncRecord> GetAllForATM(string atmId)
                    {
                        if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
                        return new List<JournalSyncRecord>(records.Values);
                    }
    
                    public List<JournalSyncRecord> GetPendingForATM(string atmId)
                        => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
                    public List<JournalSyncRecord> GetFailedForATM(string atmId)
                        => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
                    public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
                    public (int completed, int failed, int pending) GetStats(string atmId)
                    {
                        var all = GetAllForATM(atmId);
                        return (
                            all.Count(r => r.State == JournalSyncState.Completed),
                            all.Count(r => r.State == JournalSyncState.Failed),
                            all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                        );
                    }
    
                    public void LoadFromDatabase(string atmId)
                    {
                        var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                        if (!_state.ContainsKey(atmId))
                            _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                        foreach (var r in records)
                        {
                            var key = $"{r.FileName}|{r.Checksum}";
                            _state[atmId][key] = r;
                        }
                        AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
                    }
    
                    private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
                    {
                        if (!_state.TryGetValue(atmId, out var records)) return;
                        foreach (var r in records.Values)
                        {
                            if (r.SyncId == syncId)
                            {
                                updater(r);
                                OnStateChanged?.Invoke(this, r);
                                return;
                            }
                        }
                    }
    
                    private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
                    {
                        if (!_state.TryGetValue(atmId, out var records)) return;
                        foreach (var r in records.Values) updater(r);
                    }
    
                    private readonly Dictionary<string, JournalSyncRecord> _records = new Dictionary<string, JournalSyncRecord>();
    
                    private readonly object _lock = new object();
    
                    public JournalSyncRecord BeginLocalSave(string atmId, string fileName, long fileSize, string checksum, string localPath = null, string syncId = null)
                    {
                        var record = new JournalSyncRecord
                        {
                            SyncId = string.IsNullOrWhiteSpace(syncId) ? Guid.NewGuid().ToString("N") : syncId,
                            ATM_ID = atmId,
                            FileName = fileName,
                            FileSize = fileSize,
                            Checksum = checksum,
                            LocalPath = localPath,
                            State = JournalSyncState.LocalSaving,
                            ProgressPercent = 10,
                            Message = "Local journal copy is being prepared"
                        };
                        Save(record);
                        return record;
                    }
    
                    public JournalSyncRecord MarkPending(string syncId, string message = "Waiting for server connection")
                    {
                        return Update(syncId, JournalSyncState.Pending, 25, message);
                    }
    
                    public JournalSyncRecord MarkSyncing(string syncId, int progressPercent = 50, string message = "Journal is being sent to server")
                    {
                        return Update(syncId, JournalSyncState.Syncing, progressPercent, message);
                    }
    
                    public JournalSyncRecord MarkReSyncing(string syncId, int retryCount, string message = "Connection interrupted; journal will be re-synced")
                    {
                        JournalSyncRecord record = null;
                        lock (_lock)
                        {
                            if (_records.TryGetValue(syncId, out record))
                            {
                                record.RetryCount = retryCount;
                            }
                        }
                        return Update(syncId, JournalSyncState.ReSyncing, 35, message);
                    }
    
                    public JournalSyncRecord MarkStored(string syncId, string serverPath)
                    {
                        JournalSyncRecord record = null;
                        lock (_lock)
                        {
                            if (_records.TryGetValue(syncId, out record))
                            {
                                record.ServerPath = serverPath;
                            }
                        }
                        return Update(syncId, JournalSyncState.StoredOnServer, 90, "Journal stored on server");
                    }
    
                    public JournalSyncRecord MarkCompleted(string syncId, string message = "Journal sync completed")
                    {
                        return Update(syncId, JournalSyncState.Completed, 100, message);
                    }
    
                    public JournalSyncRecord MarkFailed(string syncId, string message)
                    {
                        return Update(syncId, JournalSyncState.Failed, 0, message);
                    }
    
                    public JournalSyncRecord TrackServerReceive(string atmId, string fileName, long fileSize, string checksum, string serverPath)
                    {
                        var record = BeginLocalSave(atmId, fileName, fileSize, checksum, null);
                        record.ServerPath = serverPath;
                        record.State = JournalSyncState.StoredOnServer;
                        record.ProgressPercent = 100;
                        record.Message = "Journal received and stored on server";
                        Save(record);
                        return record;
                    }
    
                    public List<JournalSyncRecord> GetRecords()
                    {
                        lock (_lock)
                        {
                            return _records.Values.OrderByDescending(r => r.UpdatedAtUtc).ToList();
                        }
                    }
    
                    public string ExportCsv(string outputPath)
                    {
                        var records = GetRecords();
                        Directory.CreateDirectory(outputPath);
                        string filePath = Path.Combine(outputPath, "JournalSync_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".csv");
                        using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                        {
                            writer.WriteLine("SyncId,ATM_ID,FileName,FileSize,Checksum,State,ProgressPercent,RetryCount,CreatedAtUtc,UpdatedAtUtc,LocalPath,ServerPath,Message");
                            foreach (var r in records)
                            {
                                writer.WriteLine(string.Join(",", new[]
                                {
                                    Csv(r.SyncId),
                                    Csv(r.ATM_ID),
                                    Csv(r.FileName),
                                    r.FileSize.ToString(),
                                    Csv(r.Checksum),
                                    Csv(r.State.ToString()),
                                    r.ProgressPercent.ToString(),
                                    r.RetryCount.ToString(),
                                    Csv(r.CreatedAtUtc.ToString("O")),
                                    Csv(r.UpdatedAtUtc.ToString("O")),
                                    Csv(r.LocalPath),
                                    Csv(r.ServerPath),
                                    Csv(r.Message)
                                }));
                            }
                        }
                        return filePath;
                    }
    
                    private JournalSyncRecord Update(string syncId, JournalSyncState state, int progressPercent, string message)
                    {
                        JournalSyncRecord record = null;
                        lock (_lock)
                        {
                            if (!_records.TryGetValue(syncId, out record))
                            {
                                return null;
                            }
    
                            record.State = state;
                            record.ProgressPercent = Math.Max(0, Math.Min(100, progressPercent));
                            record.Message = message;
                            record.UpdatedAtUtc = DateTime.UtcNow;
                        }
                        OnRecordChanged?.Invoke(record);
                        return record;
                    }
    
                    private void Save(JournalSyncRecord record)
                    {
                        lock (_lock)
                        {
                            record.UpdatedAtUtc = DateTime.UtcNow;
                            _records[record.SyncId] = record;
                        }
                        OnRecordChanged?.Invoke(record);
                    }
    
                    private static string Csv(string value)
                    {
                        value = value ?? string.Empty;
                        return "\"" + value.Replace("\"", "\"\"") + "\"";
                    }
    
    
            // --- Events ---
                    public event EventHandler<JournalSyncRecord> OnStateChanged;
    
                    public event Action<JournalSyncRecord> OnRecordChanged;
    
    
        }
    // Class: JournalSyncTracker (from 2 sources)
        public partial class JournalSyncTracker
        {
            // --- Methods ---
                    private readonly Dictionary<string, JournalSyncRecord> _records = new Dictionary<string, JournalSyncRecord>();
    
                    private readonly object _lock = new object();
    
                    public JournalSyncRecord BeginLocalSave(string atmId, string fileName, long fileSize, string checksum, string localPath = null, string syncId = null)
                    {
                        var record = new JournalSyncRecord
                        {
                            SyncId = string.IsNullOrWhiteSpace(syncId) ? Guid.NewGuid().ToString("N") : syncId,
                            ATM_ID = atmId,
                            FileName = fileName,
                            FileSize = fileSize,
                            Checksum = checksum,
                            LocalPath = localPath,
                            State = JournalSyncState.LocalSaving,
                            ProgressPercent = 10,
                            Message = "Local journal copy is being prepared"
                        };
                        Save(record);
                        return record;
                    }
    
                    public JournalSyncRecord MarkPending(string syncId, string message = "Waiting for server connection")
                    {
                        return Update(syncId, JournalSyncState.Pending, 25, message);
                    }
    
                    public JournalSyncRecord MarkSyncing(string syncId, int progressPercent = 50, string message = "Journal is being sent to server")
                    {
                        return Update(syncId, JournalSyncState.Syncing, progressPercent, message);
                    }
    
                    public JournalSyncRecord MarkReSyncing(string syncId, int retryCount, string message = "Connection interrupted; journal will be re-synced")
                    {
                        JournalSyncRecord record = null;
                        lock (_lock)
                        {
                            if (_records.TryGetValue(syncId, out record))
                            {
                                record.RetryCount = retryCount;
                            }
                        }
                        return Update(syncId, JournalSyncState.ReSyncing, 35, message);
                    }
    
                    public JournalSyncRecord MarkStored(string syncId, string serverPath)
                    {
                        JournalSyncRecord record = null;
                        lock (_lock)
                        {
                            if (_records.TryGetValue(syncId, out record))
                            {
                                record.ServerPath = serverPath;
                            }
                        }
                        return Update(syncId, JournalSyncState.StoredOnServer, 90, "Journal stored on server");
                    }
    
                    public JournalSyncRecord MarkCompleted(string syncId, string message = "Journal sync completed")
                    {
                        return Update(syncId, JournalSyncState.Completed, 100, message);
                    }
    
                    public JournalSyncRecord MarkFailed(string syncId, string message)
                    {
                        return Update(syncId, JournalSyncState.Failed, 0, message);
                    }
    
                    public JournalSyncRecord TrackServerReceive(string atmId, string fileName, long fileSize, string checksum, string serverPath)
                    {
                        var record = BeginLocalSave(atmId, fileName, fileSize, checksum, null);
                        record.ServerPath = serverPath;
                        record.State = JournalSyncState.StoredOnServer;
                        record.ProgressPercent = 100;
                        record.Message = "Journal received and stored on server";
                        Save(record);
                        return record;
                    }
    
                    public List<JournalSyncRecord> GetRecords()
                    {
                        lock (_lock)
                        {
                            return _records.Values.OrderByDescending(r => r.UpdatedAtUtc).ToList();
                        }
                    }
    
                    public string ExportCsv(string outputPath)
                    {
                        var records = GetRecords();
                        Directory.CreateDirectory(outputPath);
                        string filePath = Path.Combine(outputPath, "JournalSync_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".csv");
                        using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                        {
                            writer.WriteLine("SyncId,ATM_ID,FileName,FileSize,Checksum,State,ProgressPercent,RetryCount,CreatedAtUtc,UpdatedAtUtc,LocalPath,ServerPath,Message");
                            foreach (var r in records)
                            {
                                writer.WriteLine(string.Join(",", new[]
                                {
                                    Csv(r.SyncId),
                                    Csv(r.ATM_ID),
                                    Csv(r.FileName),
                                    r.FileSize.ToString(),
                                    Csv(r.Checksum),
                                    Csv(r.State.ToString()),
                                    r.ProgressPercent.ToString(),
                                    r.RetryCount.ToString(),
                                    Csv(r.CreatedAtUtc.ToString("O")),
                                    Csv(r.UpdatedAtUtc.ToString("O")),
                                    Csv(r.LocalPath),
                                    Csv(r.ServerPath),
                                    Csv(r.Message)
                                }));
                            }
                        }
                        return filePath;
                    }
    
                    private JournalSyncRecord Update(string syncId, JournalSyncState state, int progressPercent, string message)
                    {
                        JournalSyncRecord record = null;
                        lock (_lock)
                        {
                            if (!_records.TryGetValue(syncId, out record))
                            {
                                return null;
                            }
    
                            record.State = state;
                            record.ProgressPercent = Math.Max(0, Math.Min(100, progressPercent));
                            record.Message = message;
                            record.UpdatedAtUtc = DateTime.UtcNow;
                        }
                        OnRecordChanged?.Invoke(record);
                        return record;
                    }
    
                    private void Save(JournalSyncRecord record)
                    {
                        lock (_lock)
                        {
                            record.UpdatedAtUtc = DateTime.UtcNow;
                            _records[record.SyncId] = record;
                        }
                        OnRecordChanged?.Invoke(record);
                    }
    
                    private static string Csv(string value)
                    {
                        value = value ?? string.Empty;
                        return "\"" + value.Replace("\"", "\"\"") + "\"";
                    }
    
    
            // --- Events ---
                    public event Action<JournalSyncRecord> OnRecordChanged;
    
    
        }
    // Class: JournalSyncTracker (from 3 sources)
        public partial class JournalSyncTracker
        {
            // --- Constants & Fields ---
                    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
                        = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
    
            // --- Properties ---
                    public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
                    {
                        var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                        var key     = $"{fileName}|{checksum}";
    
                        if (records.TryGetValue(key, out var existing))
                            return existing;
    
                        // Idempotency: هل مزامن من قبل؟ (L-03)
                        if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                        {
                            AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                            return null;
                        }
    
                        var record = new JournalSyncRecord
                        {
                            ATM_ID          = atmId,
                            FileName        = fileName,
                            FileSize        = fileSize,
                            FileOffset      = offset,
                            Checksum        = checksum,
                            State           = JournalSyncState.Pending,
                            ProgressPercent = 0,
                            CreatedAtUtc    = DateTime.UtcNow
                        };
    
                        // تسجيل في قاعدة البيانات
                        DatabaseManager.Instance.InsertSyncRecord(record);
                        records[key] = record;
                        OnStateChanged?.Invoke(this, record);
                        return record;
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\Ejlive\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified.Enhanced\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
    
            // --- Methods ---
                    public void MarkSyncing(string atmId, string syncId, int chunkPercent)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State           = JournalSyncState.Syncing;
                            r.ProgressPercent = chunkPercent;
                            r.UpdatedAtUtc    = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
                    }
    
                    public void MarkCompleted(string atmId, string syncId, string sha256 = null)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State           = JournalSyncState.Completed;
                            r.ProgressPercent = 100;
                            r.SHA256Hash      = sha256;
                            r.CompletedAtUtc  = DateTime.UtcNow;
                            r.UpdatedAtUtc    = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                        AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
                    }
    
                    public void MarkFailed(string atmId, string syncId, string reason)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State       = JournalSyncState.Failed;
                            r.Message     = reason;
                            r.RetryCount++;
                            r.UpdatedAtUtc = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                        AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
                    }
    
                    public void MarkReSyncing(string atmId, string syncId)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State = JournalSyncState.ReSyncing;
                            r.UpdatedAtUtc = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
                    }
    
                    public void MarkArchived(string syncId)
                    {
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
                    }
    
                    public List<JournalSyncRecord> GetAllForATM(string atmId)
                    {
                        if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
                        return new List<JournalSyncRecord>(records.Values);
                    }
    
                    public List<JournalSyncRecord> GetPendingForATM(string atmId)
                        => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
                    public List<JournalSyncRecord> GetFailedForATM(string atmId)
                        => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
                    public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
                    public (int completed, int failed, int pending) GetStats(string atmId)
                    {
                        var all = GetAllForATM(atmId);
                        return (
                            all.Count(r => r.State == JournalSyncState.Completed),
                            all.Count(r => r.State == JournalSyncState.Failed),
                            all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                        );
                    }
    
                    public void LoadFromDatabase(string atmId)
                    {
                        var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                        if (!_state.ContainsKey(atmId))
                            _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                        foreach (var r in records)
                        {
                            var key = $"{r.FileName}|{r.Checksum}";
                            _state[atmId][key] = r;
                        }
                        AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
                    }
    
                    private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
                    {
                        if (!_state.TryGetValue(atmId, out var records)) return;
                        foreach (var r in records.Values)
                        {
                            if (r.SyncId == syncId)
                            {
                                updater(r);
                                OnStateChanged?.Invoke(this, r);
                                return;
                            }
                        }
                    }
    
                    private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
                    {
                        if (!_state.TryGetValue(atmId, out var records)) return;
                        foreach (var r in records.Values) updater(r);
                    }
    
    
            // --- Events ---
                    public event EventHandler<JournalSyncRecord> OnStateChanged;
    
    
        }
    // Class: JournalSyncTracker (from 2 sources)
        public partial class JournalSyncTracker
        {
            // --- Constants & Fields ---
                    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
                        = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
    
            // --- Properties ---
                    public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
                    {
                        var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                        var key     = $"{fileName}|{checksum}";
    
                        if (records.TryGetValue(key, out var existing))
                            return existing;
    
                        // Idempotency: هل مزامن من قبل؟ (L-03)
                        if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                        {
                            AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                            return null;
                        }
    
                        var record = new JournalSyncRecord
                        {
                            ATM_ID          = atmId,
                            FileName        = fileName,
                            FileSize        = fileSize,
                            FileOffset      = offset,
                            Checksum        = checksum,
                            State           = JournalSyncState.Pending,
                            ProgressPercent = 0,
                            CreatedAtUtc    = DateTime.UtcNow
                        };
    
                        // تسجيل في قاعدة البيانات
                        DatabaseManager.Instance.InsertSyncRecord(record);
                        records[key] = record;
                        OnStateChanged?.Invoke(this, record);
                        return record;
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
    
            // --- Methods ---
                    public void MarkSyncing(string atmId, string syncId, int chunkPercent)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State           = JournalSyncState.Syncing;
                            r.ProgressPercent = chunkPercent;
                            r.UpdatedAtUtc    = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
                    }
    
                    public void MarkCompleted(string atmId, string syncId, string sha256 = null)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State           = JournalSyncState.Completed;
                            r.ProgressPercent = 100;
                            r.SHA256Hash      = sha256;
                            r.CompletedAtUtc  = DateTime.UtcNow;
                            r.UpdatedAtUtc    = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                        AppLogger.Instance.Info($"✓ Sync completed: {syncId}", "SyncTracker");
                    }
    
                    public void MarkFailed(string atmId, string syncId, string reason)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State       = JournalSyncState.Failed;
                            r.Message     = reason;
                            r.RetryCount++;
                            r.UpdatedAtUtc = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                        AppLogger.Instance.Warning($"✗ Sync failed: {syncId} — {reason}", "SyncTracker");
                    }
    
                    public void MarkReSyncing(string atmId, string syncId)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State = JournalSyncState.ReSyncing;
                            r.UpdatedAtUtc = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
                    }
    
                    public void MarkArchived(string syncId)
                    {
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
                    }
    
                    public List<JournalSyncRecord> GetAllForATM(string atmId)
                    {
                        if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
                        return new List<JournalSyncRecord>(records.Values);
                    }
    
                    public List<JournalSyncRecord> GetPendingForATM(string atmId)
                        => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
                    public List<JournalSyncRecord> GetFailedForATM(string atmId)
                        => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
                    public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
                    public (int completed, int failed, int pending) GetStats(string atmId)
                    {
                        var all = GetAllForATM(atmId);
                        return (
                            all.Count(r => r.State == JournalSyncState.Completed),
                            all.Count(r => r.State == JournalSyncState.Failed),
                            all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                        );
                    }
    
                    public void LoadFromDatabase(string atmId)
                    {
                        var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                        if (!_state.ContainsKey(atmId))
                            _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                        foreach (var r in records)
                        {
                            var key = $"{r.FileName}|{r.Checksum}";
                            _state[atmId][key] = r;
                        }
                        AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
                    }
    
                    private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
                    {
                        if (!_state.TryGetValue(atmId, out var records)) return;
                        foreach (var r in records.Values)
                        {
                            if (r.SyncId == syncId)
                            {
                                updater(r);
                                OnStateChanged?.Invoke(this, r);
                                return;
                            }
                        }
                    }
    
                    private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
                    {
                        if (!_state.TryGetValue(atmId, out var records)) return;
                        foreach (var r in records.Values) updater(r);
                    }
    
    
            // --- Events ---
                    public event EventHandler<JournalSyncRecord> OnStateChanged;
    
    
        }
    // Class: JournalSyncTracker (from 2 sources)
        public partial class JournalSyncTracker
        {
            // --- Constants & Fields ---
                    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
                        = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
    
            // --- Properties ---
                    public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
                    {
                        var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                        var key     = $"{fileName}|{checksum}";
    
                        if (records.TryGetValue(key, out var existing))
                            return existing;
    
                        // Idempotency: هل مزامن من قبل؟ (L-03)
                        if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                        {
                            AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                            return null;
                        }
    
                        var record = new JournalSyncRecord
                        {
                            ATM_ID          = atmId,
                            FileName        = fileName,
                            FileSize        = fileSize,
                            FileOffset      = offset,
                            Checksum        = checksum,
                            State           = JournalSyncState.Pending,
                            ProgressPercent = 0,
                            CreatedAtUtc    = DateTime.UtcNow
                        };
    
                        // تسجيل في قاعدة البيانات
                        DatabaseManager.Instance.InsertSyncRecord(record);
                        records[key] = record;
                        OnStateChanged?.Invoke(this, record);
                        return record;
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
    
            // --- Methods ---
                    public void MarkSyncing(string atmId, string syncId, int chunkPercent)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State           = JournalSyncState.Syncing;
                            r.ProgressPercent = chunkPercent;
                            r.UpdatedAtUtc    = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
                    }
    
                    public void MarkCompleted(string atmId, string syncId, string sha256 = null)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State           = JournalSyncState.Completed;
                            r.ProgressPercent = 100;
                            r.SHA256Hash      = sha256;
                            r.CompletedAtUtc  = DateTime.UtcNow;
                            r.UpdatedAtUtc    = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                        AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
                    }
    
                    public void MarkFailed(string atmId, string syncId, string reason)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State       = JournalSyncState.Failed;
                            r.Message     = reason;
                            r.RetryCount++;
                            r.UpdatedAtUtc = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                        AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
                    }
    
                    public void MarkReSyncing(string atmId, string syncId)
                    {
                        UpdateState(atmId, syncId, r =>
                        {
                            r.State = JournalSyncState.ReSyncing;
                            r.UpdatedAtUtc = DateTime.UtcNow;
                        });
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
                    }
    
                    public void MarkArchived(string syncId)
                    {
                        DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
                    }
    
                    public List<JournalSyncRecord> GetAllForATM(string atmId)
                    {
                        if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
                        return new List<JournalSyncRecord>(records.Values);
                    }
    
                    public List<JournalSyncRecord> GetPendingForATM(string atmId)
                        => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
                    public List<JournalSyncRecord> GetFailedForATM(string atmId)
                        => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
                    public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
                    public (int completed, int failed, int pending) GetStats(string atmId)
                    {
                        var all = GetAllForATM(atmId);
                        return (
                            all.Count(r => r.State == JournalSyncState.Completed),
                            all.Count(r => r.State == JournalSyncState.Failed),
                            all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                        );
                    }
    
                    public void LoadFromDatabase(string atmId)
                    {
                        var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                        if (!_state.ContainsKey(atmId))
                            _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                        foreach (var r in records)
                        {
                            var key = $"{r.FileName}|{r.Checksum}";
                            _state[atmId][key] = r;
                        }
                        AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
                    }
    
                    private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
                    {
                        if (!_state.TryGetValue(atmId, out var records)) return;
                        foreach (var r in records.Values)
                        {
                            if (r.SyncId == syncId)
                            {
                                updater(r);
                                OnStateChanged?.Invoke(this, r);
                                return;
                            }
                        }
                    }
    
                    private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
                    {
                        if (!_state.TryGetValue(atmId, out var records)) return;
                        foreach (var r in records.Values) updater(r);
                    }
    
    
            // --- Events ---
                    public event EventHandler<JournalSyncRecord> OnStateChanged;
    
    
        }
    // Class: JournalSyncTracker (from 9 sources)
        public partial class JournalSyncTracker
        {
            // --- Constants & Fields ---
                            private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
                                = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
    
            // --- Properties ---
                            public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
                            {
                                var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                                var key     = $"{fileName}|{checksum}";
    
                                if (records.TryGetValue(key, out var existing))
                                    return existing;
    
                                // Idempotency: هل مزامن من قبل؟ (L-03)
                                if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                                {
                                    AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                                    return null;
                                }
    
                                var record = new JournalSyncRecord
                                {
                                    ATM_ID          = atmId,
                                    FileName        = fileName,
                                    FileSize        = fileSize,
                                    FileOffset      = offset,
                                    Checksum        = checksum,
                                    State           = JournalSyncState.Pending,
                                    ProgressPercent = 0,
                                    CreatedAtUtc    = DateTime.UtcNow
                                };
    
                                // تسجيل في قاعدة البيانات
                                DatabaseManager.Instance.InsertSyncRecord(record);
                                records[key] = record;
                                OnStateChanged?.Invoke(this, record);
                                return record;
                            }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
                            public void ResetInFlightToResync(string atmId)
                            {
                                UpdateAllForATM(atmId, r =>
                                {
                                    if (r.State == JournalSyncState.Syncing)
                                    {
                                        r.State = JournalSyncState.ReSyncing;
                                        r.UpdatedAtUtc = DateTime.UtcNow;
                                    }
                                });
                            }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4_Unified\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Final\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
                    public void ResetInFlightToResync(string atmId)
                    {
                        UpdateAllForATM(atmId, r =>
                        {
                            if (r.State == JournalSyncState.Syncing)
                            {
                                r.State = JournalSyncState.ReSyncing;
                                r.UpdatedAtUtc = DateTime.UtcNow;
                            }
                        });
                    }
    
    
            // --- Methods ---
                            public void MarkSyncing(string atmId, string syncId, int chunkPercent)
                            {
                                UpdateState(atmId, syncId, r =>
                                {
                                    r.State           = JournalSyncState.Syncing;
                                    r.ProgressPercent = chunkPercent;
                                    r.UpdatedAtUtc    = DateTime.UtcNow;
                                });
                                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
                            }
    
                            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
                            {
                                UpdateState(atmId, syncId, r =>
                                {
                                    r.State           = JournalSyncState.Completed;
                                    r.ProgressPercent = 100;
                                    r.SHA256Hash      = sha256;
                                    r.CompletedAtUtc  = DateTime.UtcNow;
                                    r.UpdatedAtUtc    = DateTime.UtcNow;
                                });
                                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
                            }
    
                            public void MarkFailed(string atmId, string syncId, string reason)
                            {
                                UpdateState(atmId, syncId, r =>
                                {
                                    r.State       = JournalSyncState.Failed;
                                    r.Message     = reason;
                                    r.RetryCount++;
                                    r.UpdatedAtUtc = DateTime.UtcNow;
                                });
                                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
                            }
    
                            public void MarkReSyncing(string atmId, string syncId)
                            {
                                UpdateState(atmId, syncId, r =>
                                {
                                    r.State = JournalSyncState.ReSyncing;
                                    r.UpdatedAtUtc = DateTime.UtcNow;
                                });
                                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
                            }
    
                            public void MarkArchived(string syncId)
                            {
                                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
                            }
    
                            public List<JournalSyncRecord> GetAllForATM(string atmId)
                            {
                                if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
                                return new List<JournalSyncRecord>(records.Values);
                            }
    
                            public List<JournalSyncRecord> GetPendingForATM(string atmId)
                                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
                            public List<JournalSyncRecord> GetFailedForATM(string atmId)
                                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
                            public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
                            public (int completed, int failed, int pending) GetStats(string atmId)
                            {
                                var all = GetAllForATM(atmId);
                                return (
                                    all.Count(r => r.State == JournalSyncState.Completed),
                                    all.Count(r => r.State == JournalSyncState.Failed),
                                    all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                                );
                            }
    
                            public void LoadFromDatabase(string atmId)
                            {
                                var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                                if (!_state.ContainsKey(atmId))
                                    _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                                foreach (var r in records)
                                {
                                    var key = $"{r.FileName}|{r.Checksum}";
                                    _state[atmId][key] = r;
                                }
                                AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
                            }
    
                            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
                            {
                                if (!_state.TryGetValue(atmId, out var records)) return;
                                foreach (var r in records.Values)
                                {
                                    if (r.SyncId == syncId)
                                    {
                                        updater(r);
                                        OnStateChanged?.Invoke(this, r);
                                        return;
                                    }
                                }
                            }
    
                            private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
                            {
                                if (!_state.TryGetValue(atmId, out var records)) return;
                                foreach (var r in records.Values) updater(r);
                            }
    
    
            // --- Events ---
                            public event EventHandler<JournalSyncRecord> OnStateChanged;
    
    
        }
    /// <summary>
        /// متتبع حالة المزامنة الكامل — JournalSyncTracker
        /// يدير آلة الحالة: Pending → Syncing → [Completed | Failed → ReSyncing]
        /// يضمن: Idempotency (L-03), عدم فقدان السجلات (L-01), إعادة الإرسال (L-05)
        /// يوفر: GetPendingForATM, MarkSyncing, MarkCompleted, MarkFailed, ScheduleResync
        /// </summary>
        public class JournalSyncTracker
        {
            // حالة في الذاكرة لكل صراف
            private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
            public event EventHandler<JournalSyncRecord> OnStateChanged;
    
            // ==========================================
            // إضافة سجل جديد
            // ==========================================
    
            public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
            {
                var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                var key = $"{fileName}|{checksum}";
    
                if (records.TryGetValue(key, out
                    var existing))
                    return existing;
    
                // Idempotency: هل مزامن من قبل؟ (L-03)
                if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                {
                    AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0, 8)}]", "SyncTracker");
                    return null;
                }
    
                var record = new JournalSyncRecord
                {
                    ATM_ID = atmId,
                    FileName = fileName,
                    FileSize = fileSize,
                    FileOffset = offset,
                    Checksum = checksum,
                    State = JournalSyncState.Pending,
                    ProgressPercent = 0,
                    CreatedAtUtc = DateTime.UtcNow
                };
    
                // تسجيل في قاعدة البيانات
                DatabaseManager.Instance.InsertSyncRecord(record);
                records[key] = record;
                OnStateChanged?.Invoke(this, record);
                return record;
            }
    
            // ==========================================
            // تحديث الحالة
            // ==========================================
    
            public void MarkSyncing(string atmId, string syncId, int chunkPercent)
            {
                UpdateState(atmId, syncId, r => {
                    r.State = JournalSyncState.Syncing;
                    r.ProgressPercent = chunkPercent;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
            }
    
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r => {
                    r.State = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash = sha256;
                    r.CompletedAtUtc = DateTime.UtcNow;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r => {
                    r.State = JournalSyncState.Failed;
                    r.Message = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateState(atmId, syncId, r => {
                    r.State = JournalSyncState.ReSyncing;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
            }
    
            public void MarkArchived(string syncId)
            {
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
            }
    
            // ==========================================
            // الاستعلام
            // ==========================================
    
            public List<JournalSyncRecord> GetAllForATM(string atmId)
            {
                if (!_state.TryGetValue(atmId, out
                    var records)) return new List<JournalSyncRecord>();
                return new List<JournalSyncRecord>(records.Values);
            }
    
            public List<JournalSyncRecord> GetPendingForATM(string atmId) => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
            public List<JournalSyncRecord> GetFailedForATM(string atmId) => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
            public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
            public (int completed, int failed, int pending) GetStats(string atmId)
            {
                var all = GetAllForATM(atmId);
                return (
                    all.Count(r => r.State == JournalSyncState.Completed),
                    all.Count(r => r.State == JournalSyncState.Failed),
                    all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                );
            }
    
            // ==========================================
            // تحميل من قاعدة البيانات
            // ==========================================
    
            public void LoadFromDatabase(string atmId)
            {
                var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                if (!_state.ContainsKey(atmId))
                    _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                foreach (var r in records)
                {
                    var key = $"{r.FileName}|{r.Checksum}";
                    _state[atmId][key] = r;
                }
                AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
            }
    
            // مساعد تحديث الحالة
            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out
                    var records)) return;
                foreach (var r in records.Values)
                {
                    if (r.SyncId == syncId)
                    {
                        updater(r);
                        OnStateChanged?.Invoke(this, r);
                        return;
                    }
                }
            }
    
            // إعادة ضبط حالة ATM بعد إعادة الاتصال
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r => {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
            private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out
                    var records)) return;
                foreach (var r in records.Values) updater(r);
            }
        }
    public partial class JournalSyncTracker
        {
            private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
                = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
    
            public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
            {
                var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                var key     = $"{fileName}|{checksum}";
    
                if (records.TryGetValue(key, out var existing))
                    return existing;
    
                // Idempotency: هل مزامن من قبل؟ (L-03)
                if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                {
                    AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                    return null;
                }
    
                var record = new JournalSyncRecord
                {
                    ATM_ID          = atmId,
                    FileName        = fileName,
                    FileSize        = fileSize,
                    FileOffset      = offset,
                    Checksum        = checksum,
                    State           = JournalSyncState.Pending,
                    ProgressPercent = 0,
                    CreatedAtUtc    = DateTime.UtcNow
                };
    
                // تسجيل في قاعدة البيانات
                DatabaseManager.Instance.InsertSyncRecord(record);
                records[key] = record;
                OnStateChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\JournalSyncTracker.cs.v20_bak
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\JournalSyncTracker.cs.v17_bak
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\JournalSyncTracker.cs.v16_bak
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\JournalSyncTracker.cs.v15_bak
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            public void MarkSyncing(string atmId, string syncId, int chunkPercent)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Syncing;
                    r.ProgressPercent = chunkPercent;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
            }
    
    
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State = JournalSyncState.ReSyncing;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
            }
    
    
            public void MarkArchived(string syncId)
            {
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
            }
    
    
            public List<JournalSyncRecord> GetAllForATM(string atmId)
            {
                if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
                return new List<JournalSyncRecord>(records.Values);
            }
    
    
            public List<JournalSyncRecord> GetPendingForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
    
            public List<JournalSyncRecord> GetFailedForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
    
            public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
    
            public (int completed, int failed, int pending) GetStats(string atmId)
            {
                var all = GetAllForATM(atmId);
                return (
                    all.Count(r => r.State == JournalSyncState.Completed),
                    all.Count(r => r.State == JournalSyncState.Failed),
                    all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                );
            }
    
    
            public void LoadFromDatabase(string atmId)
            {
                var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                if (!_state.ContainsKey(atmId))
                    _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                foreach (var r in records)
                {
                    var key = $"{r.FileName}|{r.Checksum}";
                    _state[atmId][key] = r;
                }
                AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
            }
    
    
            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values)
                {
                    if (r.SyncId == syncId)
                    {
                        updater(r);
                        OnStateChanged?.Invoke(this, r);
                        return;
                    }
                }
            }
    
    
            private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values) updater(r);
            }
    
    
            private readonly Dictionary<string, JournalSyncRecord> _records = new Dictionary<string, JournalSyncRecord>();
    
    
            private readonly object _lock = new object();
    
    
            public JournalSyncRecord BeginLocalSave(string atmId, string fileName, long fileSize, string checksum, string localPath = null, string syncId = null)
            {
                var record = new JournalSyncRecord
                {
                    SyncId = string.IsNullOrWhiteSpace(syncId) ? Guid.NewGuid().ToString("N") : syncId,
                    ATM_ID = atmId,
                    FileName = fileName,
                    FileSize = fileSize,
                    Checksum = checksum,
                    LocalPath = localPath,
                    State = JournalSyncState.LocalSaving,
                    ProgressPercent = 10,
                    Message = "Local journal copy is being prepared"
                };
                Save(record);
                return record;
            }
    
    
            public JournalSyncRecord MarkPending(string syncId, string message = "Waiting for server connection")
            {
                return Update(syncId, JournalSyncState.Pending, 25, message);
            }
    
    
            public JournalSyncRecord MarkSyncing(string syncId, int progressPercent = 50, string message = "Journal is being sent to server")
            {
                return Update(syncId, JournalSyncState.Syncing, progressPercent, message);
            }
    
    
            public JournalSyncRecord MarkReSyncing(string syncId, int retryCount, string message = "Connection interrupted; journal will be re-synced")
            {
                JournalSyncRecord record = null;
                lock (_lock)
                {
                    if (_records.TryGetValue(syncId, out record))
                    {
                        record.RetryCount = retryCount;
                    }
                }
                return Update(syncId, JournalSyncState.ReSyncing, 35, message);
            }
    
    
            public JournalSyncRecord MarkStored(string syncId, string serverPath)
            {
                JournalSyncRecord record = null;
                lock (_lock)
                {
                    if (_records.TryGetValue(syncId, out record))
                    {
                        record.ServerPath = serverPath;
                    }
                }
                return Update(syncId, JournalSyncState.StoredOnServer, 90, "Journal stored on server");
            }
    
    
            public JournalSyncRecord MarkCompleted(string syncId, string message = "Journal sync completed")
            {
                return Update(syncId, JournalSyncState.Completed, 100, message);
            }
    
    
            public JournalSyncRecord MarkFailed(string syncId, string message)
            {
                return Update(syncId, JournalSyncState.Failed, 0, message);
            }
    
    
            public JournalSyncRecord TrackServerReceive(string atmId, string fileName, long fileSize, string checksum, string serverPath)
            {
                var record = BeginLocalSave(atmId, fileName, fileSize, checksum, null);
                record.ServerPath = serverPath;
                record.State = JournalSyncState.StoredOnServer;
                record.ProgressPercent = 100;
                record.Message = "Journal received and stored on server";
                Save(record);
                return record;
            }
    
    
            public List<JournalSyncRecord> GetRecords()
            {
                lock (_lock)
                {
                    return _records.Values.OrderByDescending(r => r.UpdatedAtUtc).ToList();
                }
            }
    
    
            public string ExportCsv(string outputPath)
            {
                var records = GetRecords();
                Directory.CreateDirectory(outputPath);
                string filePath = Path.Combine(outputPath, "JournalSync_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".csv");
                using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("SyncId,ATM_ID,FileName,FileSize,Checksum,State,ProgressPercent,RetryCount,CreatedAtUtc,UpdatedAtUtc,LocalPath,ServerPath,Message");
                    foreach (var r in records)
                    {
                        writer.WriteLine(string.Join(",", new[]
                        {
                            Csv(r.SyncId),
                            Csv(r.ATM_ID),
                            Csv(r.FileName),
                            r.FileSize.ToString(),
                            Csv(r.Checksum),
                            Csv(r.State.ToString()),
                            r.ProgressPercent.ToString(),
                            r.RetryCount.ToString(),
                            Csv(r.CreatedAtUtc.ToString("O")),
                            Csv(r.UpdatedAtUtc.ToString("O")),
                            Csv(r.LocalPath),
                            Csv(r.ServerPath),
                            Csv(r.Message)
                        }));
                    }
                }
                return filePath;
            }
    
    
            private JournalSyncRecord Update(string syncId, JournalSyncState state, int progressPercent, string message)
            {
                JournalSyncRecord record = null;
                lock (_lock)
                {
                    if (!_records.TryGetValue(syncId, out record))
                    {
                        return null;
                    }
    
                    record.State = state;
                    record.ProgressPercent = Math.Max(0, Math.Min(100, progressPercent));
                    record.Message = message;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                }
                OnRecordChanged?.Invoke(record);
                return record;
            }
    
    
            private void Save(JournalSyncRecord record)
            {
                lock (_lock)
                {
                    record.UpdatedAtUtc = DateTime.UtcNow;
                    _records[record.SyncId] = record;
                }
                OnRecordChanged?.Invoke(record);
            }
    
    
            private static string Csv(string value)
            {
                value = value ?? string.Empty;
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
    
    
            public event EventHandler<JournalSyncRecord> OnStateChanged;
    
    
            public event Action<JournalSyncRecord> OnRecordChanged;
    
    
        }
    public partial class JournalSyncTracker
        {
            private readonly Dictionary<string, JournalSyncRecord> _records = new Dictionary<string, JournalSyncRecord>();
    
    
            private readonly object _lock = new object();
    
    
            public JournalSyncRecord BeginLocalSave(string atmId, string fileName, long fileSize, string checksum, string localPath = null, string syncId = null)
            {
                var record = new JournalSyncRecord
                {
                    SyncId = string.IsNullOrWhiteSpace(syncId) ? Guid.NewGuid().ToString("N") : syncId,
                    ATM_ID = atmId,
                    FileName = fileName,
                    FileSize = fileSize,
                    Checksum = checksum,
                    LocalPath = localPath,
                    State = JournalSyncState.LocalSaving,
                    ProgressPercent = 10,
                    Message = "Local journal copy is being prepared"
                };
                Save(record);
                return record;
            }
    
    
            public JournalSyncRecord MarkPending(string syncId, string message = "Waiting for server connection")
            {
                return Update(syncId, JournalSyncState.Pending, 25, message);
            }
    
    
            public JournalSyncRecord MarkSyncing(string syncId, int progressPercent = 50, string message = "Journal is being sent to server")
            {
                return Update(syncId, JournalSyncState.Syncing, progressPercent, message);
            }
    
    
            public JournalSyncRecord MarkReSyncing(string syncId, int retryCount, string message = "Connection interrupted; journal will be re-synced")
            {
                JournalSyncRecord record = null;
                lock (_lock)
                {
                    if (_records.TryGetValue(syncId, out record))
                    {
                        record.RetryCount = retryCount;
                    }
                }
                return Update(syncId, JournalSyncState.ReSyncing, 35, message);
            }
    
    
            public JournalSyncRecord MarkStored(string syncId, string serverPath)
            {
                JournalSyncRecord record = null;
                lock (_lock)
                {
                    if (_records.TryGetValue(syncId, out record))
                    {
                        record.ServerPath = serverPath;
                    }
                }
                return Update(syncId, JournalSyncState.StoredOnServer, 90, "Journal stored on server");
            }
    
    
            public JournalSyncRecord MarkCompleted(string syncId, string message = "Journal sync completed")
            {
                return Update(syncId, JournalSyncState.Completed, 100, message);
            }
    
    
            public JournalSyncRecord MarkFailed(string syncId, string message)
            {
                return Update(syncId, JournalSyncState.Failed, 0, message);
            }
    
    
            public JournalSyncRecord TrackServerReceive(string atmId, string fileName, long fileSize, string checksum, string serverPath)
            {
                var record = BeginLocalSave(atmId, fileName, fileSize, checksum, null);
                record.ServerPath = serverPath;
                record.State = JournalSyncState.StoredOnServer;
                record.ProgressPercent = 100;
                record.Message = "Journal received and stored on server";
                Save(record);
                return record;
            }
    
    
            public List<JournalSyncRecord> GetRecords()
            {
                lock (_lock)
                {
                    return _records.Values.OrderByDescending(r => r.UpdatedAtUtc).ToList();
                }
            }
    
    
            public string ExportCsv(string outputPath)
            {
                var records = GetRecords();
                Directory.CreateDirectory(outputPath);
                string filePath = Path.Combine(outputPath, "JournalSync_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".csv");
                using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("SyncId,ATM_ID,FileName,FileSize,Checksum,State,ProgressPercent,RetryCount,CreatedAtUtc,UpdatedAtUtc,LocalPath,ServerPath,Message");
                    foreach (var r in records)
                    {
                        writer.WriteLine(string.Join(",", new[]
                        {
                            Csv(r.SyncId),
                            Csv(r.ATM_ID),
                            Csv(r.FileName),
                            r.FileSize.ToString(),
                            Csv(r.Checksum),
                            Csv(r.State.ToString()),
                            r.ProgressPercent.ToString(),
                            r.RetryCount.ToString(),
                            Csv(r.CreatedAtUtc.ToString("O")),
                            Csv(r.UpdatedAtUtc.ToString("O")),
                            Csv(r.LocalPath),
                            Csv(r.ServerPath),
                            Csv(r.Message)
                        }));
                    }
                }
                return filePath;
            }
    
    
            private JournalSyncRecord Update(string syncId, JournalSyncState state, int progressPercent, string message)
            {
                JournalSyncRecord record = null;
                lock (_lock)
                {
                    if (!_records.TryGetValue(syncId, out record))
                    {
                        return null;
                    }
    
                    record.State = state;
                    record.ProgressPercent = Math.Max(0, Math.Min(100, progressPercent));
                    record.Message = message;
                    record.UpdatedAtUtc = DateTime.UtcNow;
                }
                OnRecordChanged?.Invoke(record);
                return record;
            }
    
    
            private void Save(JournalSyncRecord record)
            {
                lock (_lock)
                {
                    record.UpdatedAtUtc = DateTime.UtcNow;
                    _records[record.SyncId] = record;
                }
                OnRecordChanged?.Invoke(record);
            }
    
    
            private static string Csv(string value)
            {
                value = value ?? string.Empty;
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
    
    
            public event Action<JournalSyncRecord> OnRecordChanged;
    
    
        }
    public partial class JournalSyncTracker
        {
            private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
                = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
    
            public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
            {
                var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                var key     = $"{fileName}|{checksum}";
    
                if (records.TryGetValue(key, out var existing))
                    return existing;
    
                // Idempotency: هل مزامن من قبل؟ (L-03)
                if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                {
                    AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                    return null;
                }
    
                var record = new JournalSyncRecord
                {
                    ATM_ID          = atmId,
                    FileName        = fileName,
                    FileSize        = fileSize,
                    FileOffset      = offset,
                    Checksum        = checksum,
                    State           = JournalSyncState.Pending,
                    ProgressPercent = 0,
                    CreatedAtUtc    = DateTime.UtcNow
                };
    
                // تسجيل في قاعدة البيانات
                DatabaseManager.Instance.InsertSyncRecord(record);
                records[key] = record;
                OnStateChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLiveEnterprisev4Unified\EJLiveEnterprisev4Unified\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLiveEnterprisev4Unified\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            public void MarkSyncing(string atmId, string syncId, int chunkPercent)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Syncing;
                    r.ProgressPercent = chunkPercent;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
            }
    
    
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State = JournalSyncState.ReSyncing;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
            }
    
    
            public void MarkArchived(string syncId)
            {
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
            }
    
    
            public List<JournalSyncRecord> GetAllForATM(string atmId)
            {
                if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
                return new List<JournalSyncRecord>(records.Values);
            }
    
    
            public List<JournalSyncRecord> GetPendingForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
    
            public List<JournalSyncRecord> GetFailedForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
    
            public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
    
            public (int completed, int failed, int pending) GetStats(string atmId)
            {
                var all = GetAllForATM(atmId);
                return (
                    all.Count(r => r.State == JournalSyncState.Completed),
                    all.Count(r => r.State == JournalSyncState.Failed),
                    all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                );
            }
    
    
            public void LoadFromDatabase(string atmId)
            {
                var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                if (!_state.ContainsKey(atmId))
                    _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                foreach (var r in records)
                {
                    var key = $"{r.FileName}|{r.Checksum}";
                    _state[atmId][key] = r;
                }
                AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
            }
    
    
            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values)
                {
                    if (r.SyncId == syncId)
                    {
                        updater(r);
                        OnStateChanged?.Invoke(this, r);
                        return;
                    }
                }
            }
    
    
            private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values) updater(r);
            }
    
    
            public event EventHandler<JournalSyncRecord> OnStateChanged;
    
    
        }
    public partial class JournalSyncTracker
        {
            private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
                = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
    
            public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
            {
                var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                var key     = $"{fileName}|{checksum}";
    
                if (records.TryGetValue(key, out var existing))
                    return existing;
    
                // Idempotency: هل مزامن من قبل؟ (L-03)
                if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                {
                    AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                    return null;
                }
    
                var record = new JournalSyncRecord
                {
                    ATM_ID          = atmId,
                    FileName        = fileName,
                    FileSize        = fileSize,
                    FileOffset      = offset,
                    Checksum        = checksum,
                    State           = JournalSyncState.Pending,
                    ProgressPercent = 0,
                    CreatedAtUtc    = DateTime.UtcNow
                };
    
                // تسجيل في قاعدة البيانات
                DatabaseManager.Instance.InsertSyncRecord(record);
                records[key] = record;
                OnStateChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\Ejlive\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified.Enhanced\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            public void MarkSyncing(string atmId, string syncId, int chunkPercent)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Syncing;
                    r.ProgressPercent = chunkPercent;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
            }
    
    
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State = JournalSyncState.ReSyncing;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
            }
    
    
            public void MarkArchived(string syncId)
            {
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
            }
    
    
            public List<JournalSyncRecord> GetAllForATM(string atmId)
            {
                if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
                return new List<JournalSyncRecord>(records.Values);
            }
    
    
            public List<JournalSyncRecord> GetPendingForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
    
            public List<JournalSyncRecord> GetFailedForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
    
            public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
    
            public (int completed, int failed, int pending) GetStats(string atmId)
            {
                var all = GetAllForATM(atmId);
                return (
                    all.Count(r => r.State == JournalSyncState.Completed),
                    all.Count(r => r.State == JournalSyncState.Failed),
                    all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                );
            }
    
    
            public void LoadFromDatabase(string atmId)
            {
                var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                if (!_state.ContainsKey(atmId))
                    _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                foreach (var r in records)
                {
                    var key = $"{r.FileName}|{r.Checksum}";
                    _state[atmId][key] = r;
                }
                AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
            }
    
    
            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values)
                {
                    if (r.SyncId == syncId)
                    {
                        updater(r);
                        OnStateChanged?.Invoke(this, r);
                        return;
                    }
                }
            }
    
    
            private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values) updater(r);
            }
    
    
            public event EventHandler<JournalSyncRecord> OnStateChanged;
    
    
        }
    public partial class JournalSyncTracker
        {
            private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
                = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
    
            public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
            {
                var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                var key     = $"{fileName}|{checksum}";
    
                if (records.TryGetValue(key, out var existing))
                    return existing;
    
                // Idempotency: هل مزامن من قبل؟ (L-03)
                if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                {
                    AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                    return null;
                }
    
                var record = new JournalSyncRecord
                {
                    ATM_ID          = atmId,
                    FileName        = fileName,
                    FileSize        = fileSize,
                    FileOffset      = offset,
                    Checksum        = checksum,
                    State           = JournalSyncState.Pending,
                    ProgressPercent = 0,
                    CreatedAtUtc    = DateTime.UtcNow
                };
    
                // تسجيل في قاعدة البيانات
                DatabaseManager.Instance.InsertSyncRecord(record);
                records[key] = record;
                OnStateChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            public void MarkSyncing(string atmId, string syncId, int chunkPercent)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Syncing;
                    r.ProgressPercent = chunkPercent;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
            }
    
    
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"✓ Sync completed: {syncId}", "SyncTracker");
            }
    
    
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"✗ Sync failed: {syncId} — {reason}", "SyncTracker");
            }
    
    
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State = JournalSyncState.ReSyncing;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
            }
    
    
            public void MarkArchived(string syncId)
            {
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
            }
    
    
            public List<JournalSyncRecord> GetAllForATM(string atmId)
            {
                if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
                return new List<JournalSyncRecord>(records.Values);
            }
    
    
            public List<JournalSyncRecord> GetPendingForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
    
            public List<JournalSyncRecord> GetFailedForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
    
            public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
    
            public (int completed, int failed, int pending) GetStats(string atmId)
            {
                var all = GetAllForATM(atmId);
                return (
                    all.Count(r => r.State == JournalSyncState.Completed),
                    all.Count(r => r.State == JournalSyncState.Failed),
                    all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                );
            }
    
    
            public void LoadFromDatabase(string atmId)
            {
                var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                if (!_state.ContainsKey(atmId))
                    _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                foreach (var r in records)
                {
                    var key = $"{r.FileName}|{r.Checksum}";
                    _state[atmId][key] = r;
                }
                AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
            }
    
    
            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values)
                {
                    if (r.SyncId == syncId)
                    {
                        updater(r);
                        OnStateChanged?.Invoke(this, r);
                        return;
                    }
                }
            }
    
    
            private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values) updater(r);
            }
    
    
            public event EventHandler<JournalSyncRecord> OnStateChanged;
    
    
        }
    public partial class JournalSyncTracker
        {
            private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
                = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
    
            public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
            {
                var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                var key     = $"{fileName}|{checksum}";
    
                if (records.TryGetValue(key, out var existing))
                    return existing;
    
                // Idempotency: هل مزامن من قبل؟ (L-03)
                if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                {
                    AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                    return null;
                }
    
                var record = new JournalSyncRecord
                {
                    ATM_ID          = atmId,
                    FileName        = fileName,
                    FileSize        = fileSize,
                    FileOffset      = offset,
                    Checksum        = checksum,
                    State           = JournalSyncState.Pending,
                    ProgressPercent = 0,
                    CreatedAtUtc    = DateTime.UtcNow
                };
    
                // تسجيل في قاعدة البيانات
                DatabaseManager.Instance.InsertSyncRecord(record);
                records[key] = record;
                OnStateChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            public void MarkSyncing(string atmId, string syncId, int chunkPercent)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Syncing;
                    r.ProgressPercent = chunkPercent;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
            }
    
    
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State = JournalSyncState.ReSyncing;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
            }
    
    
            public void MarkArchived(string syncId)
            {
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
            }
    
    
            public List<JournalSyncRecord> GetAllForATM(string atmId)
            {
                if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
                return new List<JournalSyncRecord>(records.Values);
            }
    
    
            public List<JournalSyncRecord> GetPendingForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
    
            public List<JournalSyncRecord> GetFailedForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
    
            public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
    
            public (int completed, int failed, int pending) GetStats(string atmId)
            {
                var all = GetAllForATM(atmId);
                return (
                    all.Count(r => r.State == JournalSyncState.Completed),
                    all.Count(r => r.State == JournalSyncState.Failed),
                    all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                );
            }
    
    
            public void LoadFromDatabase(string atmId)
            {
                var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                if (!_state.ContainsKey(atmId))
                    _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                foreach (var r in records)
                {
                    var key = $"{r.FileName}|{r.Checksum}";
                    _state[atmId][key] = r;
                }
                AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
            }
    
    
            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values)
                {
                    if (r.SyncId == syncId)
                    {
                        updater(r);
                        OnStateChanged?.Invoke(this, r);
                        return;
                    }
                }
            }
    
    
            private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values) updater(r);
            }
    
    
            public event EventHandler<JournalSyncRecord> OnStateChanged;
    
    
        }
    public partial class JournalSyncTracker
        {
            private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
                = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
    
            public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
            {
                var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                var key     = $"{fileName}|{checksum}";
    
                if (records.TryGetValue(key, out var existing))
                    return existing;
    
                // Idempotency: هل مزامن من قبل؟ (L-03)
                if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                {
                    AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                    return null;
                }
    
                var record = new JournalSyncRecord
                {
                    ATM_ID          = atmId,
                    FileName        = fileName,
                    FileSize        = fileSize,
                    FileOffset      = offset,
                    Checksum        = checksum,
                    State           = JournalSyncState.Pending,
                    ProgressPercent = 0,
                    CreatedAtUtc    = DateTime.UtcNow
                };
    
                // تسجيل في قاعدة البيانات
                DatabaseManager.Instance.InsertSyncRecord(record);
                records[key] = record;
                OnStateChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4_Unified\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Final\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            public void MarkSyncing(string atmId, string syncId, int chunkPercent)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Syncing;
                    r.ProgressPercent = chunkPercent;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
            }
    
    
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State = JournalSyncState.ReSyncing;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
            }
    
    
            public void MarkArchived(string syncId)
            {
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
            }
    
    
            public List<JournalSyncRecord> GetAllForATM(string atmId)
            {
                if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
                return new List<JournalSyncRecord>(records.Values);
            }
    
    
            public List<JournalSyncRecord> GetPendingForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
    
            public List<JournalSyncRecord> GetFailedForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
    
            public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
    
            public (int completed, int failed, int pending) GetStats(string atmId)
            {
                var all = GetAllForATM(atmId);
                return (
                    all.Count(r => r.State == JournalSyncState.Completed),
                    all.Count(r => r.State == JournalSyncState.Failed),
                    all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                );
            }
    
    
            public void LoadFromDatabase(string atmId)
            {
                var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                if (!_state.ContainsKey(atmId))
                    _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                foreach (var r in records)
                {
                    var key = $"{r.FileName}|{r.Checksum}";
                    _state[atmId][key] = r;
                }
                AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
            }
    
    
            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values)
                {
                    if (r.SyncId == syncId)
                    {
                        updater(r);
                        OnStateChanged?.Invoke(this, r);
                        return;
                    }
                }
            }
    
    
            private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values) updater(r);
            }
    
    
            public event EventHandler<JournalSyncRecord> OnStateChanged;
    
    
        }
    public partial class JournalSyncTracker
        {
            private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
    
            public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
            {
                var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                var key = $"{fileName}|{checksum}";
    
                if (records.TryGetValue(key, out
                    var existing))
                    return existing;
    
                // Idempotency: هل مزامن من قبل؟ (L-03)
                if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                {
                    AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0, 8)}]", "SyncTracker");
                    return null;
                }
    
                var record = new JournalSyncRecord
                {
                    ATM_ID = atmId,
                    FileName = fileName,
                    FileSize = fileSize,
                    FileOffset = offset,
                    Checksum = checksum,
                    State = JournalSyncState.Pending,
                    ProgressPercent = 0,
                    CreatedAtUtc = DateTime.UtcNow
                };
    
                // تسجيل في قاعدة البيانات
                DatabaseManager.Instance.InsertSyncRecord(record);
                records[key] = record;
                OnStateChanged?.Invoke(this, record);
                return record;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r => {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\JournalSyncTracker.cs
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r => {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
    
            public void MarkSyncing(string atmId, string syncId, int chunkPercent)
            {
                UpdateState(atmId, syncId, r => {
                    r.State = JournalSyncState.Syncing;
                    r.ProgressPercent = chunkPercent;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
            }
    
    
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r => {
                    r.State = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash = sha256;
                    r.CompletedAtUtc = DateTime.UtcNow;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
            }
    
    
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r => {
                    r.State = JournalSyncState.Failed;
                    r.Message = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
            }
    
    
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateState(atmId, syncId, r => {
                    r.State = JournalSyncState.ReSyncing;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
            }
    
    
            public void MarkArchived(string syncId)
            {
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
            }
    
    
            public List<JournalSyncRecord> GetAllForATM(string atmId)
            {
                if (!_state.TryGetValue(atmId, out
                    var records)) return new List<JournalSyncRecord>();
                return new List<JournalSyncRecord>(records.Values);
            }
    
    
            public List<JournalSyncRecord> GetPendingForATM(string atmId) => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
    
            public List<JournalSyncRecord> GetFailedForATM(string atmId) => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
    
            public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
    
            public (int completed, int failed, int pending) GetStats(string atmId)
            {
                var all = GetAllForATM(atmId);
                return (
                    all.Count(r => r.State == JournalSyncState.Completed),
                    all.Count(r => r.State == JournalSyncState.Failed),
                    all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                );
            }
    
    
            public void LoadFromDatabase(string atmId)
            {
                var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                if (!_state.ContainsKey(atmId))
                    _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                foreach (var r in records)
                {
                    var key = $"{r.FileName}|{r.Checksum}";
                    _state[atmId][key] = r;
                }
                AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
            }
    
    
            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out
                    var records)) return;
                foreach (var r in records.Values)
                {
                    if (r.SyncId == syncId)
                    {
                        updater(r);
                        OnStateChanged?.Invoke(this, r);
                        return;
                    }
                }
            }
    
    
            private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out
                    var records)) return;
                foreach (var r in records.Values) updater(r);
            }
    
    
            public event EventHandler<JournalSyncRecord> OnStateChanged;
    
    
        }
    /// <summary>
        /// متتبع حالة المزامنة الكامل — JournalSyncTracker
        /// يدير آلة الحالة: Pending → Syncing → [Completed | Failed → ReSyncing]
        /// يضمن: Idempotency (L-03), عدم فقدان السجلات (L-01), إعادة الإرسال (L-05)
        /// يوفر: GetPendingForATM, MarkSyncing, MarkCompleted, MarkFailed, ScheduleResync
        /// </summary>
        public class JournalSyncTracker
        {
            // حالة في الذاكرة لكل صراف
            private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
                = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();
    
            public event EventHandler<JournalSyncRecord> OnStateChanged;
    
            // ==========================================
            // إضافة سجل جديد
            // ==========================================
    
            public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
            {
                var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
                var key     = $"{fileName}|{checksum}";
    
                if (records.TryGetValue(key, out var existing))
                    return existing;
    
                // Idempotency: هل مزامن من قبل؟ (L-03)
                if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
                {
                    AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                    return null;
                }
    
                var record = new JournalSyncRecord
                {
                    ATM_ID          = atmId,
                    FileName        = fileName,
                    FileSize        = fileSize,
                    FileOffset      = offset,
                    Checksum        = checksum,
                    State           = JournalSyncState.Pending,
                    ProgressPercent = 0,
                    CreatedAtUtc    = DateTime.UtcNow
                };
    
                // تسجيل في قاعدة البيانات
                DatabaseManager.Instance.InsertSyncRecord(record);
                records[key] = record;
                OnStateChanged?.Invoke(this, record);
                return record;
            }
    
            // ==========================================
            // تحديث الحالة
            // ==========================================
    
            public void MarkSyncing(string atmId, string syncId, int chunkPercent)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Syncing;
                    r.ProgressPercent = chunkPercent;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
            }
    
            public void MarkCompleted(string atmId, string syncId, string sha256 = null)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State           = JournalSyncState.Completed;
                    r.ProgressPercent = 100;
                    r.SHA256Hash      = sha256;
                    r.CompletedAtUtc  = DateTime.UtcNow;
                    r.UpdatedAtUtc    = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
                AppLogger.Instance.Info($"✓ Sync completed: {syncId}", "SyncTracker");
            }
    
            public void MarkFailed(string atmId, string syncId, string reason)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State       = JournalSyncState.Failed;
                    r.Message     = reason;
                    r.RetryCount++;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
                AppLogger.Instance.Warning($"✗ Sync failed: {syncId} — {reason}", "SyncTracker");
            }
    
            public void MarkReSyncing(string atmId, string syncId)
            {
                UpdateState(atmId, syncId, r =>
                {
                    r.State = JournalSyncState.ReSyncing;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                });
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
            }
    
            public void MarkArchived(string syncId)
            {
                DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
            }
    
            // ==========================================
            // الاستعلام
            // ==========================================
    
            public List<JournalSyncRecord> GetAllForATM(string atmId)
            {
                if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
                return new List<JournalSyncRecord>(records.Values);
            }
    
            public List<JournalSyncRecord> GetPendingForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);
    
            public List<JournalSyncRecord> GetFailedForATM(string atmId)
                => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);
    
            public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;
    
            public (int completed, int failed, int pending) GetStats(string atmId)
            {
                var all = GetAllForATM(atmId);
                return (
                    all.Count(r => r.State == JournalSyncState.Completed),
                    all.Count(r => r.State == JournalSyncState.Failed),
                    all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
                );
            }
    
            // ==========================================
            // تحميل من قاعدة البيانات
            // ==========================================
    
            public void LoadFromDatabase(string atmId)
            {
                var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
                if (!_state.ContainsKey(atmId))
                    _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();
    
                foreach (var r in records)
                {
                    var key = $"{r.FileName}|{r.Checksum}";
                    _state[atmId][key] = r;
                }
                AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
            }
    
            // مساعد تحديث الحالة
            private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values)
                {
                    if (r.SyncId == syncId)
                    {
                        updater(r);
                        OnStateChanged?.Invoke(this, r);
                        return;
                    }
                }
            }
    
            // إعادة ضبط حالة ATM بعد إعادة الاتصال
            public void ResetInFlightToResync(string atmId)
            {
                UpdateAllForATM(atmId, r =>
                {
                    if (r.State == JournalSyncState.Syncing)
                    {
                        r.State = JournalSyncState.ReSyncing;
                        r.UpdatedAtUtc = DateTime.UtcNow;
                    }
                });
            }
    
            private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
            {
                if (!_state.TryGetValue(atmId, out var records)) return;
                foreach (var r in records.Values) updater(r);
            }
        }

    public partial class PersistedJournalSyncState
        {
        }
}
