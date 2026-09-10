// JournalSyncTrackerService (91).cs
// Processed by Code Intelligence Scanner — Smart Merge & Restructure

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System;
using EJLive.Core.Models;

namespace EJLive.Core.Services
{
    public sealed class JournalSyncTrackerService
    {
        private readonly object _sync = new object();
        private readonly string _storageRoot;
        private readonly string _stateDirectory;
        private readonly string _stateFilePath;

        public JournalSyncTrackerService(string storageRoot)
        {
            _storageRoot = string.IsNullOrWhiteSpace(storageRoot)
            ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")
            : storageRoot;
            _stateDirectory = Path.Combine(_storageRoot, "sync-state");
            _stateFilePath = Path.Combine(_stateDirectory, "journal-sync-dashboard.xml");
        }

    public void EnsureInitialized()
    {
        lock (_sync)
        {
            if (!Directory.Exists(_stateDirectory))
            Directory.CreateDirectory(_stateDirectory);
            if (!File.Exists(_stateFilePath))
            SaveInternal(new JournalSyncDashboardSnapshot());
        }
}

public JournalSyncDashboardSnapshot LoadSnapshot()
{
    lock (_sync)
    {
        EnsureInitialized();
        var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
        using (var stream = File.OpenRead(_stateFilePath))
        {
            return serializer.Deserialize(stream) as JournalSyncDashboardSnapshot ?? new JournalSyncDashboardSnapshot();
        }
}
}

public void RecordClientConnected(string atmId, string endpoint, string storageRootPath)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.RemoteEndpoint = endpoint ?? string.Empty;
        atm.IsConnected = true;
        atm.SyncState = JournalSyncState.ConnectedIdle;
        atm.StorageRootPath = storageRootPath ?? string.Empty;
        ClearAlerts(snapshot, atmId, "Disconnected");
    });
}

public void RecordHeartbeat(string atmId)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.LastHeartbeatUtc = DateTime.UtcNow;
        if (atm.SyncState == JournalSyncState.Disconnected)
        atm.SyncState = JournalSyncState.ConnectedIdle;
    });
}

public void RecordSyncReceived(string atmId, string fileName, string relativeStoragePath, long fileSizeBytes, string checksum)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.IsConnected = true;
        atm.SyncState = JournalSyncState.Synced;
        atm.LastJournalReceivedUtc = DateTime.UtcNow;
        atm.LastSuccessfulSyncUtc = DateTime.UtcNow;
        atm.TotalBytesReceived += Math.Max(0, fileSizeBytes);
        atm.TotalFilesReceived += 1;
        atm.LastFileName = fileName ?? string.Empty;
        atm.LastError = string.Empty;

        snapshot.RecentEntries.Insert(0, new JournalSyncEntry
        {
            ATMId = atmId,
            FileName = fileName ?? string.Empty,
            RelativeStoragePath = relativeStoragePath ?? string.Empty,
            FileSizeBytes = fileSizeBytes,
            Checksum = checksum ?? string.Empty,
            Status = "Success",
            Message = "Journal file received and stored.",
            TimestampUtc = DateTime.UtcNow
        });
    TrimEntries(snapshot);
    ClearAlerts(snapshot, atmId, null);
});
}

public void RecordSyncFailure(string atmId, string fileName, string message)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.SyncState = JournalSyncState.Warning;
        atm.LastFailureUtc = DateTime.UtcNow;
        atm.FailedSyncCount += 1;
        atm.LastError = message ?? string.Empty;

        snapshot.RecentEntries.Insert(0, new JournalSyncEntry
        {
            ATMId = atmId,
            FileName = fileName ?? string.Empty,
            Status = "Failure",
            Message = message ?? string.Empty,
            TimestampUtc = DateTime.UtcNow,
            RetryCount = atm.FailedSyncCount
        });
    TrimEntries(snapshot);
    AddOrReplaceAlert(snapshot, new JournalSyncAlert
    {
        ATMId = atmId,
        Severity = JournalSyncAlertSeverity.Warning,
        Title = "Sync failure",
        Message = message ?? "Journal synchronization failed."
    });
});
}

public void RecordClientDisconnected(string atmId)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.IsConnected = false;
        atm.SyncState = JournalSyncState.Disconnected;
        AddOrReplaceAlert(snapshot, new JournalSyncAlert
        {
            ATMId = atmId,
            Severity = JournalSyncAlertSeverity.Warning,
            Title = "Disconnected",
            Message = "ATM disconnected from the central server."
        });
});
}

public void RefreshDerivedAlerts(TimeSpan warningThreshold, TimeSpan criticalThreshold)
{
    Update(snapshot =>
    {
        DateTime now = DateTime.UtcNow;
        foreach (var atm in snapshot.Atms)
        {
            if (!atm.LastSuccessfulSyncUtc.HasValue)
            continue;

            TimeSpan age = now - atm.LastSuccessfulSyncUtc.Value;
            if (age >= criticalThreshold)
            {
                atm.SyncState = JournalSyncState.Critical;
                AddOrReplaceAlert(snapshot, new JournalSyncAlert
                {
                    ATMId = atm.ATMId,
                    Severity = JournalSyncAlertSeverity.Critical,
                    Title = "No journal sync",
                    Message = "No journal synchronization received for " + (int)age.TotalMinutes + " minutes."
                });
        }
    else if (age >= warningThreshold)
    {
        atm.SyncState = JournalSyncState.Warning;
        AddOrReplaceAlert(snapshot, new JournalSyncAlert
        {
            ATMId = atm.ATMId,
            Severity = JournalSyncAlertSeverity.Warning,
            Title = "Sync delayed",
            Message = "Last successful journal sync is older than the warning threshold."
        });
}
}
});
}

private void Update(Action<JournalSyncDashboardSnapshot> action)
{
    lock (_sync)
    {
        var snapshot = LoadSnapshot();
        action(snapshot);
        snapshot.SnapshotAtUtc = DateTime.UtcNow;
        SaveInternal(snapshot);
    }
}

private void SaveInternal(JournalSyncDashboardSnapshot snapshot)
{
    if (!Directory.Exists(_stateDirectory))
    Directory.CreateDirectory(_stateDirectory);
    var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
    using (var stream = File.Create(_stateFilePath))
    {
        serializer.Serialize(stream, snapshot);
    }
}

private AtmJournalSyncStatus GetOrCreateAtm(JournalSyncDashboardSnapshot snapshot, string atmId)
{
    var key = string.IsNullOrWhiteSpace(atmId) ? "Unknown" : atmId.Trim();
    var atm = snapshot.Atms.FirstOrDefault(a => string.Equals(a.ATMId, key, StringComparison.OrdinalIgnoreCase));
    if (atm == null)
    {
        atm = new AtmJournalSyncStatus { ATMId = key, SyncState = JournalSyncState.Unknown };
        snapshot.Atms.Add(atm);
    }
return atm;
}

private void AddOrReplaceAlert(JournalSyncDashboardSnapshot snapshot, JournalSyncAlert alert)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, alert.ATMId, StringComparison.OrdinalIgnoreCase)
    && string.Equals(a.Title, alert.Title, StringComparison.OrdinalIgnoreCase));
    snapshot.ActiveAlerts.Insert(0, alert);
    if (snapshot.ActiveAlerts.Count > 100)
    snapshot.ActiveAlerts = snapshot.ActiveAlerts.Take(100).ToList();
}

private void ClearAlerts(JournalSyncDashboardSnapshot snapshot, string atmId, string exactTitle)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, atmId, StringComparison.OrdinalIgnoreCase)
    && (exactTitle == null || string.Equals(a.Title, exactTitle, StringComparison.OrdinalIgnoreCase) || string.Equals(a.Title, "Sync delayed", StringComparison.OrdinalIgnoreCase) || string.Equals(a.Title, "No journal sync", StringComparison.OrdinalIgnoreCase)));
}

private void TrimEntries(JournalSyncDashboardSnapshot snapshot)
{
    if (snapshot.RecentEntries.Count > 500)
    snapshot.RecentEntries = snapshot.RecentEntries.Take(500).ToList();
}
}
public partial class JournalSyncTrackerService
{
    private readonly string _storageRoot;
    private readonly string _stateDirectory;
    private readonly string _stateFilePath;
    private readonly object _sync = new();
    public JournalSyncTrackerService(string storageRoot)
    {
        _storageRoot = string.IsNullOrWhiteSpace(storageRoot)
        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")
        : storageRoot;
        _stateDirectory = Path.Combine(_storageRoot, "sync-state");
        _stateFilePath = Path.Combine(_stateDirectory, "journal-sync-dashboard.xml");
    }
private readonly object _sync = new object();
public void EnsureInitialized()
{
    lock (_sync)
    {
        if (!Directory.Exists(_stateDirectory))
        Directory.CreateDirectory(_stateDirectory);
        if (!File.Exists(_stateFilePath))
        SaveInternal(new JournalSyncDashboardSnapshot());
    }
}
public JournalSyncDashboardSnapshot LoadSnapshot()
{
    lock (_sync)
    {
        EnsureInitialized();
        var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
        using (var stream = File.OpenRead(_stateFilePath))
        {
            return serializer.Deserialize(stream) as JournalSyncDashboardSnapshot ?? new JournalSyncDashboardSnapshot();
        }
}
}
public void RecordClientConnected(string atmId, string endpoint, string storageRootPath)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.RemoteEndpoint = endpoint ?? string.Empty;
        atm.IsConnected = true;
        atm.SyncState = JournalSyncState.ConnectedIdle;
        atm.StorageRootPath = storageRootPath ?? string.Empty;
        ClearAlerts(snapshot, atmId, "Disconnected");
    });
}
public void RecordHeartbeat(string atmId)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.LastHeartbeatUtc = DateTime.UtcNow;
        if (atm.SyncState == JournalSyncState.Disconnected)
        atm.SyncState = JournalSyncState.ConnectedIdle;
    });
}
public void RecordSyncReceived(string atmId, string fileName, string relativeStoragePath, long fileSizeBytes, string checksum)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.IsConnected = true;
        atm.SyncState = JournalSyncState.Synced;
        atm.LastJournalReceivedUtc = DateTime.UtcNow;
        atm.LastSuccessfulSyncUtc = DateTime.UtcNow;
        atm.TotalBytesReceived += Math.Max(0, fileSizeBytes);
        atm.TotalFilesReceived += 1;
        atm.LastFileName = fileName ?? string.Empty;
        atm.LastError = string.Empty;
        snapshot.RecentEntries.Insert(0, new JournalSyncEntry
        {
            ATMId = atmId,
            FileName = fileName ?? string.Empty,
            RelativeStoragePath = relativeStoragePath ?? string.Empty,
            FileSizeBytes = fileSizeBytes,
            Checksum = checksum ?? string.Empty,
            Status = "Success",
            Message = "Journal file received and stored.",
            TimestampUtc = DateTime.UtcNow
        });
    TrimEntries(snapshot);
    ClearAlerts(snapshot, atmId, null);
});
}
public void RecordSyncFailure(string atmId, string fileName, string message)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.SyncState = JournalSyncState.Warning;
        atm.LastFailureUtc = DateTime.UtcNow;
        atm.FailedSyncCount += 1;
        atm.LastError = message ?? string.Empty;
        snapshot.RecentEntries.Insert(0, new JournalSyncEntry
        {
            ATMId = atmId,
            FileName = fileName ?? string.Empty,
            Status = "Failure",
            Message = message ?? string.Empty,
            TimestampUtc = DateTime.UtcNow,
            RetryCount = atm.FailedSyncCount
        });
    TrimEntries(snapshot);
    AddOrReplaceAlert(snapshot, new JournalSyncAlert
    {
        ATMId = atmId,
        Severity = JournalSyncAlertSeverity.Warning,
        Title = "Sync failure",
        Message = message ?? "Journal synchronization failed."
    });
});
}
public void RecordClientDisconnected(string atmId)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.IsConnected = false;
        atm.SyncState = JournalSyncState.Disconnected;
        AddOrReplaceAlert(snapshot, new JournalSyncAlert
        {
            ATMId = atmId,
            Severity = JournalSyncAlertSeverity.Warning,
            Title = "Disconnected",
            Message = "ATM disconnected from the central server."
        });
});
}
public void RefreshDerivedAlerts(TimeSpan warningThreshold, TimeSpan criticalThreshold)
{
    Update(snapshot =>
    {
        DateTime now = DateTime.UtcNow;
        foreach (var atm in snapshot.Atms)
        {
            if (!atm.LastSuccessfulSyncUtc.HasValue)
            continue;
            TimeSpan age = now - atm.LastSuccessfulSyncUtc.Value;
            if (age >= criticalThreshold)
            {
                atm.SyncState = JournalSyncState.Critical;
                AddOrReplaceAlert(snapshot, new JournalSyncAlert
                {
                    ATMId = atm.ATMId,
                    Severity = JournalSyncAlertSeverity.Critical,
                    Title = "No journal sync",
                    Message = "No journal synchronization received for " + (int)age.TotalMinutes + " minutes."
                });
        }
    else if (age >= warningThreshold)
    {
        atm.SyncState = JournalSyncState.Warning;
        AddOrReplaceAlert(snapshot, new JournalSyncAlert
        {
            ATMId = atm.ATMId,
            Severity = JournalSyncAlertSeverity.Warning,
            Title = "Sync delayed",
            Message = "Last successful journal sync is older than the warning threshold."
        });
}
}
});
}
private void Update(Action<JournalSyncDashboardSnapshot> action)
{
    lock (_sync)
    {
        var snapshot = LoadSnapshot();
        action(snapshot);
        snapshot.SnapshotAtUtc = DateTime.UtcNow;
        SaveInternal(snapshot);
    }
}
private void SaveInternal(JournalSyncDashboardSnapshot snapshot)
{
    if (!Directory.Exists(_stateDirectory))
    Directory.CreateDirectory(_stateDirectory);
    var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
    using (var stream = File.Create(_stateFilePath))
    {
        serializer.Serialize(stream, snapshot);
    }
}
private AtmJournalSyncStatus GetOrCreateAtm(JournalSyncDashboardSnapshot snapshot, string atmId)
{
    var key = string.IsNullOrWhiteSpace(atmId) ? "Unknown" : atmId.Trim();
    var atm = snapshot.Atms.FirstOrDefault(a => string.Equals(a.ATMId, key, StringComparison.OrdinalIgnoreCase));
    if (atm == null)
    {
        atm = new AtmJournalSyncStatus { ATMId = key, SyncState = JournalSyncState.Unknown };
        snapshot.Atms.Add(atm);
    }
return atm;
}
private void AddOrReplaceAlert(JournalSyncDashboardSnapshot snapshot, JournalSyncAlert alert)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, alert.ATMId, StringComparison.OrdinalIgnoreCase)
    && string.Equals(a.Title, alert.Title, StringComparison.OrdinalIgnoreCase));
    snapshot.ActiveAlerts.Insert(0, alert);
    if (snapshot.ActiveAlerts.Count > 100)
    snapshot.ActiveAlerts = snapshot.ActiveAlerts.Take(100).ToList();
}
private void ClearAlerts(JournalSyncDashboardSnapshot snapshot, string atmId, string exactTitle)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, atmId, StringComparison.OrdinalIgnoreCase)
    && (exactTitle == null || string.Equals(a.Title, exactTitle, StringComparison.OrdinalIgnoreCase) || string.Equals(a.Title, "Sync delayed", StringComparison.OrdinalIgnoreCase) || string.Equals(a.Title, "No journal sync", StringComparison.OrdinalIgnoreCase)));
}
private void TrimEntries(JournalSyncDashboardSnapshot snapshot)
{
    if (snapshot.RecentEntries.Count > 500)
    snapshot.RecentEntries = snapshot.RecentEntries.Take(500).ToList();
}
private void ClearAlerts(JournalSyncDashboardSnapshot snapshot, string atmId, string? exactTitle)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, atmId, StringComparison.OrdinalIgnoreCase)
    && (exactTitle == null || string.Equals(a.Title, exactTitle, StringComparison.OrdinalIgnoreCase)
    || string.Equals(a.Title, "Sync delayed", StringComparison.OrdinalIgnoreCase)
    || string.Equals(a.Title, "No journal sync", StringComparison.OrdinalIgnoreCase)));
}
}
public partial public sealed class JournalSyncTrackerService
{
    private readonly object _sync = new object();
    private readonly string _storageRoot;
    private readonly string _stateDirectory;
    private readonly string _stateFilePath;
    public JournalSyncTrackerService(string storageRoot)
    {
        public JournalSyncTrackerService()
        {
            public void EnsureInitialized()
            {
                public JournalSyncDashboardSnapshot LoadSnapshot()
                {
                    public void RecordClientConnected(string atmId, string endpoint, string storageRootPath)
                    {
                        public void RecordHeartbeat(string atmId)
                        {
                            public void RecordSyncReceived(string atmId, string fileName, string relativeStoragePath, long fileSizeBytes, string checksum)
                            {
                                public void RecordSyncFailure(string atmId, string fileName, string message)
                                {
                                    public void RecordClientDisconnected(string atmId)
                                    {
                                        public void RefreshDerivedAlerts(TimeSpan warningThreshold, TimeSpan criticalThreshold)
                                        {
                                            private void Update(Action<JournalSyncDashboardSnapshot> action)
                                            {
                                                private void SaveInternal(JournalSyncDashboardSnapshot snapshot)
                                                {
                                                    private AtmJournalSyncStatus GetOrCreateAtm(JournalSyncDashboardSnapshot snapshot, string atmId)
                                                    {
                                                        private void AddOrReplaceAlert(JournalSyncDashboardSnapshot snapshot, JournalSyncAlert alert)
                                                        {
                                                            private void ClearAlerts(JournalSyncDashboardSnapshot snapshot, string atmId, string exactTitle)
                                                            {
                                                                private void TrimEntries(JournalSyncDashboardSnapshot snapshot)
                                                                {
                                                                    private void ClearAlerts(JournalSyncDashboardSnapshot snapshot, string atmId, string? exactTitle)
                                                                    {
                                                                    }

                                                            }
                                                        public partial public public class JournalSyncTrackerService
                                                        {
                                                            private readonly string _storageRoot;
                                                            private readonly string _stateDirectory;
                                                            private readonly string _stateFilePath;
                                                            private readonly object _sync = new object();
                                                            public JournalSyncTrackerService(string storageRoot)
                                                            {
                                                                public JournalSyncTrackerService()
                                                                {
                                                                    public void EnsureInitialized()
                                                                    {
                                                                        public JournalSyncDashboardSnapshot LoadSnapshot()
                                                                        {
                                                                            public void RecordClientConnected(string atmId, string endpoint, string storageRootPath)
                                                                            {
                                                                                public void RecordHeartbeat(string atmId)
                                                                                {
                                                                                    public void RecordSyncReceived(string atmId, string fileName, string relativeStoragePath, long fileSizeBytes, string checksum)
                                                                                    {
                                                                                        public void RecordSyncFailure(string atmId, string fileName, string message)
                                                                                        {
                                                                                            public void RecordClientDisconnected(string atmId)
                                                                                            {
                                                                                                public void RefreshDerivedAlerts(TimeSpan warningThreshold, TimeSpan criticalThreshold)
                                                                                                {
                                                                                                    private void Update(Action<JournalSyncDashboardSnapshot> action)
                                                                                                    {
                                                                                                        private void SaveInternal(JournalSyncDashboardSnapshot snapshot)
                                                                                                        {
                                                                                                            private AtmJournalSyncStatus GetOrCreateAtm(JournalSyncDashboardSnapshot snapshot, string atmId)
                                                                                                            {
                                                                                                                private void AddOrReplaceAlert(JournalSyncDashboardSnapshot snapshot, JournalSyncAlert alert)
                                                                                                                {
                                                                                                                    private void ClearAlerts(JournalSyncDashboardSnapshot snapshot, string atmId, string exactTitle)
                                                                                                                    {
                                                                                                                        private void TrimEntries(JournalSyncDashboardSnapshot snapshot)
                                                                                                                        {
                                                                                                                        }

                                                                                                                }
                                                                                                            public partial public class JournalSyncTrackerService
                                                                                                            {
                                                                                                                private readonly string _storageRoot;
                                                                                                                private readonly string _stateDirectory;
                                                                                                                private readonly string _stateFilePath;
                                                                                                                private readonly object _sync = new object();
                                                                                                                public JournalSyncTrackerService(string storageRoot)
                                                                                                                {
                                                                                                                    public JournalSyncTrackerService()
                                                                                                                    {
                                                                                                                        public void EnsureInitialized()
                                                                                                                        {
                                                                                                                            public JournalSyncDashboardSnapshot LoadSnapshot()
                                                                                                                            {
                                                                                                                                public void RecordClientConnected(string atmId, string endpoint, string storageRootPath)
                                                                                                                                {
                                                                                                                                    public void RecordHeartbeat(string atmId)
                                                                                                                                    {
                                                                                                                                        public void RecordSyncReceived(string atmId, string fileName, string relativeStoragePath, long fileSizeBytes, string checksum)
                                                                                                                                        {
                                                                                                                                            public void RecordSyncFailure(string atmId, string fileName, string message)
                                                                                                                                            {
                                                                                                                                                public void RecordClientDisconnected(string atmId)
                                                                                                                                                {
                                                                                                                                                    public void RefreshDerivedAlerts(TimeSpan warningThreshold, TimeSpan criticalThreshold)
                                                                                                                                                    {
                                                                                                                                                        private void Update(Action<JournalSyncDashboardSnapshot> action)
                                                                                                                                                        {
                                                                                                                                                            private void SaveInternal(JournalSyncDashboardSnapshot snapshot)
                                                                                                                                                            {
                                                                                                                                                                private AtmJournalSyncStatus GetOrCreateAtm(JournalSyncDashboardSnapshot snapshot, string atmId)
                                                                                                                                                                {
                                                                                                                                                                    private void AddOrReplaceAlert(JournalSyncDashboardSnapshot snapshot, JournalSyncAlert alert)
                                                                                                                                                                    {
                                                                                                                                                                        private void ClearAlerts(JournalSyncDashboardSnapshot snapshot, string atmId, string exactTitle)
                                                                                                                                                                        {
                                                                                                                                                                            private void TrimEntries(JournalSyncDashboardSnapshot snapshot)
                                                                                                                                                                            {
                                                                                                                                                                            }

                                                                                                                                                                    }
                                                                                                                                                                public sealed class JournalSyncTrackerService : JournalSyncTrackingService
                                                                                                                                                                {
                                                                                                                                                                    public JournalSyncTrackerService()
                                                                                                                                                                    {
                                                                                                                                                                    }

                                                                                                                                                                public JournalSyncTrackerService(string storageRoot)
                                                                                                                                                                : base(storageRoot)
                                                                                                                                                                {
                                                                                                                                                                }

                                                                                                                                                            public void EnsureInitialized()
                                                                                                                                                            {
                                                                                                                                                            }
                                                                                                                                                    }
                                                                                                                                                public partial class JournalSyncTrackerService
                                                                                                                                                {
                                                                                                                                                    private readonly string _storageRoot;


                                                                                                                                                    private readonly string _stateDirectory;


                                                                                                                                                    private readonly string _stateFilePath;


                                                                                                                                                    public JournalSyncTrackerService(string storageRoot)
                                                                                                                                                    {
                                                                                                                                                        _storageRoot = string.IsNullOrWhiteSpace(storageRoot)
                                                                                                                                                        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")
                                                                                                                                                        : storageRoot;
                                                                                                                                                        _stateDirectory = Path.Combine(_storageRoot, "sync-state");
                                                                                                                                                        _stateFilePath = Path.Combine(_stateDirectory, "journal-sync-dashboard.xml");
                                                                                                                                                    }


                                                                                                                                                private readonly object _sync = new object();


                                                                                                                                                public void EnsureInitialized()
                                                                                                                                                {
                                                                                                                                                    lock (_sync)
                                                                                                                                                    {
                                                                                                                                                        if (!Directory.Exists(_stateDirectory))
                                                                                                                                                        Directory.CreateDirectory(_stateDirectory);
                                                                                                                                                        if (!File.Exists(_stateFilePath))
                                                                                                                                                        SaveInternal(new JournalSyncDashboardSnapshot());
                                                                                                                                                    }
                                                                                                                                            }


                                                                                                                                        public JournalSyncDashboardSnapshot LoadSnapshot()
                                                                                                                                        {
                                                                                                                                            lock (_sync)
                                                                                                                                            {
                                                                                                                                                EnsureInitialized();
                                                                                                                                                var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
                                                                                                                                                using (var stream = File.OpenRead(_stateFilePath))
                                                                                                                                                {
                                                                                                                                                    return serializer.Deserialize(stream) as JournalSyncDashboardSnapshot ?? new JournalSyncDashboardSnapshot();
                                                                                                                                                }
                                                                                                                                        }
                                                                                                                                }


                                                                                                                            public void RecordClientConnected(string atmId, string endpoint, string storageRootPath)
                                                                                                                            {
                                                                                                                                Update(snapshot =>
                                                                                                                                {
                                                                                                                                    var atm = GetOrCreateAtm(snapshot, atmId);
                                                                                                                                    atm.RemoteEndpoint = endpoint ?? string.Empty;
                                                                                                                                    atm.IsConnected = true;
                                                                                                                                    atm.SyncState = JournalSyncState.ConnectedIdle;
                                                                                                                                    atm.StorageRootPath = storageRootPath ?? string.Empty;
                                                                                                                                    ClearAlerts(snapshot, atmId, "Disconnected");
                                                                                                                                });
                                                                                                                        }


                                                                                                                    public void RecordHeartbeat(string atmId)
                                                                                                                    {
                                                                                                                        Update(snapshot =>
                                                                                                                        {
                                                                                                                            var atm = GetOrCreateAtm(snapshot, atmId);
                                                                                                                            atm.LastHeartbeatUtc = DateTime.UtcNow;
                                                                                                                            if (atm.SyncState == JournalSyncState.Disconnected)
                                                                                                                            atm.SyncState = JournalSyncState.ConnectedIdle;
                                                                                                                        });
                                                                                                                }


                                                                                                            public void RecordSyncReceived(string atmId, string fileName, string relativeStoragePath, long fileSizeBytes, string checksum)
                                                                                                            {
                                                                                                                Update(snapshot =>
                                                                                                                {
                                                                                                                    var atm = GetOrCreateAtm(snapshot, atmId);
                                                                                                                    atm.IsConnected = true;
                                                                                                                    atm.SyncState = JournalSyncState.Synced;
                                                                                                                    atm.LastJournalReceivedUtc = DateTime.UtcNow;
                                                                                                                    atm.LastSuccessfulSyncUtc = DateTime.UtcNow;
                                                                                                                    atm.TotalBytesReceived += Math.Max(0, fileSizeBytes);
                                                                                                                    atm.TotalFilesReceived += 1;
                                                                                                                    atm.LastFileName = fileName ?? string.Empty;
                                                                                                                    atm.LastError = string.Empty;

                                                                                                                    snapshot.RecentEntries.Insert(0, new JournalSyncEntry
                                                                                                                    {
                                                                                                                        ATMId = atmId,
                                                                                                                        FileName = fileName ?? string.Empty,
                                                                                                                        RelativeStoragePath = relativeStoragePath ?? string.Empty,
                                                                                                                        FileSizeBytes = fileSizeBytes,
                                                                                                                        Checksum = checksum ?? string.Empty,
                                                                                                                        Status = "Success",
                                                                                                                        Message = "Journal file received and stored.",
                                                                                                                        TimestampUtc = DateTime.UtcNow
                                                                                                                    });
                                                                                                                TrimEntries(snapshot);
                                                                                                                ClearAlerts(snapshot, atmId, null);
                                                                                                            });
                                                                                                    }


                                                                                                public void RecordSyncFailure(string atmId, string fileName, string message)
                                                                                                {
                                                                                                    Update(snapshot =>
                                                                                                    {
                                                                                                        var atm = GetOrCreateAtm(snapshot, atmId);
                                                                                                        atm.SyncState = JournalSyncState.Warning;
                                                                                                        atm.LastFailureUtc = DateTime.UtcNow;
                                                                                                        atm.FailedSyncCount += 1;
                                                                                                        atm.LastError = message ?? string.Empty;

                                                                                                        snapshot.RecentEntries.Insert(0, new JournalSyncEntry
                                                                                                        {
                                                                                                            ATMId = atmId,
                                                                                                            FileName = fileName ?? string.Empty,
                                                                                                            Status = "Failure",
                                                                                                            Message = message ?? string.Empty,
                                                                                                            TimestampUtc = DateTime.UtcNow,
                                                                                                            RetryCount = atm.FailedSyncCount
                                                                                                        });
                                                                                                    TrimEntries(snapshot);
                                                                                                    AddOrReplaceAlert(snapshot, new JournalSyncAlert
                                                                                                    {
                                                                                                        ATMId = atmId,
                                                                                                        Severity = JournalSyncAlertSeverity.Warning,
                                                                                                        Title = "Sync failure",
                                                                                                        Message = message ?? "Journal synchronization failed."
                                                                                                    });
                                                                                            });
                                                                                    }


                                                                                public void RecordClientDisconnected(string atmId)
                                                                                {
                                                                                    Update(snapshot =>
                                                                                    {
                                                                                        var atm = GetOrCreateAtm(snapshot, atmId);
                                                                                        atm.IsConnected = false;
                                                                                        atm.SyncState = JournalSyncState.Disconnected;
                                                                                        AddOrReplaceAlert(snapshot, new JournalSyncAlert
                                                                                        {
                                                                                            ATMId = atmId,
                                                                                            Severity = JournalSyncAlertSeverity.Warning,
                                                                                            Title = "Disconnected",
                                                                                            Message = "ATM disconnected from the central server."
                                                                                        });
                                                                                });
                                                                        }


                                                                    public void RefreshDerivedAlerts(TimeSpan warningThreshold, TimeSpan criticalThreshold)
                                                                    {
                                                                        Update(snapshot =>
                                                                        {
                                                                            DateTime now = DateTime.UtcNow;
                                                                            foreach (var atm in snapshot.Atms)
                                                                            {
                                                                                if (!atm.LastSuccessfulSyncUtc.HasValue)
                                                                                continue;

                                                                                TimeSpan age = now - atm.LastSuccessfulSyncUtc.Value;
                                                                                if (age >= criticalThreshold)
                                                                                {
                                                                                    atm.SyncState = JournalSyncState.Critical;
                                                                                    AddOrReplaceAlert(snapshot, new JournalSyncAlert
                                                                                    {
                                                                                        ATMId = atm.ATMId,
                                                                                        Severity = JournalSyncAlertSeverity.Critical,
                                                                                        Title = "No journal sync",
                                                                                        Message = "No journal synchronization received for " + (int)age.TotalMinutes + " minutes."
                                                                                    });
                                                                            }
                                                                        else if (age >= warningThreshold)
                                                                        {
                                                                            atm.SyncState = JournalSyncState.Warning;
                                                                            AddOrReplaceAlert(snapshot, new JournalSyncAlert
                                                                            {
                                                                                ATMId = atm.ATMId,
                                                                                Severity = JournalSyncAlertSeverity.Warning,
                                                                                Title = "Sync delayed",
                                                                                Message = "Last successful journal sync is older than the warning threshold."
                                                                            });
                                                                    }
                                                            }
                                                    });
                                            }


                                        private void Update(Action<JournalSyncDashboardSnapshot> action)
                                        {
                                            lock (_sync)
                                            {
                                                var snapshot = LoadSnapshot();
                                                action(snapshot);
                                                snapshot.SnapshotAtUtc = DateTime.UtcNow;
                                                SaveInternal(snapshot);
                                            }
                                    }


                                private void SaveInternal(JournalSyncDashboardSnapshot snapshot)
                                {
                                    if (!Directory.Exists(_stateDirectory))
                                    Directory.CreateDirectory(_stateDirectory);
                                    var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
                                    using (var stream = File.Create(_stateFilePath))
                                    {
                                        serializer.Serialize(stream, snapshot);
                                    }
                            }


                        private AtmJournalSyncStatus GetOrCreateAtm(JournalSyncDashboardSnapshot snapshot, string atmId)
                        {
                            var key = string.IsNullOrWhiteSpace(atmId) ? "Unknown" : atmId.Trim();
                            var atm = snapshot.Atms.FirstOrDefault(a => string.Equals(a.ATMId, key, StringComparison.OrdinalIgnoreCase));
                            if (atm == null)
                            {
                                atm = new AtmJournalSyncStatus { ATMId = key, SyncState = JournalSyncState.Unknown };
                                snapshot.Atms.Add(atm);
                            }
                        return atm;
                    }


                private void AddOrReplaceAlert(JournalSyncDashboardSnapshot snapshot, JournalSyncAlert alert)
                {
                    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, alert.ATMId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(a.Title, alert.Title, StringComparison.OrdinalIgnoreCase));
                    snapshot.ActiveAlerts.Insert(0, alert);
                    if (snapshot.ActiveAlerts.Count > 100)
                    snapshot.ActiveAlerts = snapshot.ActiveAlerts.Take(100).ToList();
                }


            private void ClearAlerts(JournalSyncDashboardSnapshot snapshot, string atmId, string exactTitle)
            {
                snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, atmId, StringComparison.OrdinalIgnoreCase)
                && (exactTitle == null || string.Equals(a.Title, exactTitle, StringComparison.OrdinalIgnoreCase) || string.Equals(a.Title, "Sync delayed", StringComparison.OrdinalIgnoreCase) || string.Equals(a.Title, "No journal sync", StringComparison.OrdinalIgnoreCase)));
            }


        private void TrimEntries(JournalSyncDashboardSnapshot snapshot)
        {
            if (snapshot.RecentEntries.Count > 500)
            snapshot.RecentEntries = snapshot.RecentEntries.Take(500).ToList();
        }

}
// Class: JournalSyncTrackerService (from 6 sources)
public sealed partial class JournalSyncTrackerService
{
    // --- Constants & Fields ---
    private readonly string _storageRoot;

    private readonly string _stateDirectory;

    private readonly string _stateFilePath;

    private readonly object _sync = new();


    // --- Constructors ---
    public JournalSyncTrackerService(string storageRoot)
    {
        _storageRoot = string.IsNullOrWhiteSpace(storageRoot)
        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")
        : storageRoot;
        _stateDirectory = Path.Combine(_storageRoot, "sync-state");
        _stateFilePath = Path.Combine(_stateDirectory, "journal-sync-dashboard.xml");
    }


// --- Methods ---
private readonly object _sync = new object();

public void EnsureInitialized()
{
    lock (_sync)
    {
        if (!Directory.Exists(_stateDirectory))
        Directory.CreateDirectory(_stateDirectory);
        if (!File.Exists(_stateFilePath))
        SaveInternal(new JournalSyncDashboardSnapshot());
    }
}

public JournalSyncDashboardSnapshot LoadSnapshot()
{
    lock (_sync)
    {
        EnsureInitialized();
        var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
        using (var stream = File.OpenRead(_stateFilePath))
        {
            return serializer.Deserialize(stream) as JournalSyncDashboardSnapshot ?? new JournalSyncDashboardSnapshot();
        }
}
}

public void RecordClientConnected(string atmId, string endpoint, string storageRootPath)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.RemoteEndpoint = endpoint ?? string.Empty;
        atm.IsConnected = true;
        atm.SyncState = JournalSyncState.ConnectedIdle;
        atm.StorageRootPath = storageRootPath ?? string.Empty;
        ClearAlerts(snapshot, atmId, "Disconnected");
    });
}

public void RecordHeartbeat(string atmId)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.LastHeartbeatUtc = DateTime.UtcNow;
        if (atm.SyncState == JournalSyncState.Disconnected)
        atm.SyncState = JournalSyncState.ConnectedIdle;
    });
}

public void RecordSyncReceived(string atmId, string fileName, string relativeStoragePath, long fileSizeBytes, string checksum)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.IsConnected = true;
        atm.SyncState = JournalSyncState.Synced;
        atm.LastJournalReceivedUtc = DateTime.UtcNow;
        atm.LastSuccessfulSyncUtc = DateTime.UtcNow;
        atm.TotalBytesReceived += Math.Max(0, fileSizeBytes);
        atm.TotalFilesReceived += 1;
        atm.LastFileName = fileName ?? string.Empty;
        atm.LastError = string.Empty;

        snapshot.RecentEntries.Insert(0, new JournalSyncEntry
        {
            ATMId = atmId,
            FileName = fileName ?? string.Empty,
            RelativeStoragePath = relativeStoragePath ?? string.Empty,
            FileSizeBytes = fileSizeBytes,
            Checksum = checksum ?? string.Empty,
            Status = "Success",
            Message = "Journal file received and stored.",
            TimestampUtc = DateTime.UtcNow
        });
    TrimEntries(snapshot);
    ClearAlerts(snapshot, atmId, null);
});
}

public void RecordSyncFailure(string atmId, string fileName, string message)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.SyncState = JournalSyncState.Warning;
        atm.LastFailureUtc = DateTime.UtcNow;
        atm.FailedSyncCount += 1;
        atm.LastError = message ?? string.Empty;

        snapshot.RecentEntries.Insert(0, new JournalSyncEntry
        {
            ATMId = atmId,
            FileName = fileName ?? string.Empty,
            Status = "Failure",
            Message = message ?? string.Empty,
            TimestampUtc = DateTime.UtcNow,
            RetryCount = atm.FailedSyncCount
        });
    TrimEntries(snapshot);
    AddOrReplaceAlert(snapshot, new JournalSyncAlert
    {
        ATMId = atmId,
        Severity = JournalSyncAlertSeverity.Warning,
        Title = "Sync failure",
        Message = message ?? "Journal synchronization failed."
    });
});
}

public void RecordClientDisconnected(string atmId)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.IsConnected = false;
        atm.SyncState = JournalSyncState.Disconnected;
        AddOrReplaceAlert(snapshot, new JournalSyncAlert
        {
            ATMId = atmId,
            Severity = JournalSyncAlertSeverity.Warning,
            Title = "Disconnected",
            Message = "ATM disconnected from the central server."
        });
});
}

public void RefreshDerivedAlerts(TimeSpan warningThreshold, TimeSpan criticalThreshold)
{
    Update(snapshot =>
    {
        DateTime now = DateTime.UtcNow;
        foreach (var atm in snapshot.Atms)
        {
            if (!atm.LastSuccessfulSyncUtc.HasValue)
            continue;

            TimeSpan age = now - atm.LastSuccessfulSyncUtc.Value;
            if (age >= criticalThreshold)
            {
                atm.SyncState = JournalSyncState.Critical;
                AddOrReplaceAlert(snapshot, new JournalSyncAlert
                {
                    ATMId = atm.ATMId,
                    Severity = JournalSyncAlertSeverity.Critical,
                    Title = "No journal sync",
                    Message = "No journal synchronization received for " + (int)age.TotalMinutes + " minutes."
                });
        }
    else if (age >= warningThreshold)
    {
        atm.SyncState = JournalSyncState.Warning;
        AddOrReplaceAlert(snapshot, new JournalSyncAlert
        {
            ATMId = atm.ATMId,
            Severity = JournalSyncAlertSeverity.Warning,
            Title = "Sync delayed",
            Message = "Last successful journal sync is older than the warning threshold."
        });
}
}
});
}

private void Update(Action<JournalSyncDashboardSnapshot> action)
{
    lock (_sync)
    {
        var snapshot = LoadSnapshot();
        action(snapshot);
        snapshot.SnapshotAtUtc = DateTime.UtcNow;
        SaveInternal(snapshot);
    }
}

private void SaveInternal(JournalSyncDashboardSnapshot snapshot)
{
    if (!Directory.Exists(_stateDirectory))
    Directory.CreateDirectory(_stateDirectory);
    var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
    using (var stream = File.Create(_stateFilePath))
    {
        serializer.Serialize(stream, snapshot);
    }
}

private AtmJournalSyncStatus GetOrCreateAtm(JournalSyncDashboardSnapshot snapshot, string atmId)
{
    var key = string.IsNullOrWhiteSpace(atmId) ? "Unknown" : atmId.Trim();
    var atm = snapshot.Atms.FirstOrDefault(a => string.Equals(a.ATMId, key, StringComparison.OrdinalIgnoreCase));
    if (atm == null)
    {
        atm = new AtmJournalSyncStatus { ATMId = key, SyncState = JournalSyncState.Unknown };
        snapshot.Atms.Add(atm);
    }
return atm;
}

private void AddOrReplaceAlert(JournalSyncDashboardSnapshot snapshot, JournalSyncAlert alert)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, alert.ATMId, StringComparison.OrdinalIgnoreCase)
    && string.Equals(a.Title, alert.Title, StringComparison.OrdinalIgnoreCase));
    snapshot.ActiveAlerts.Insert(0, alert);
    if (snapshot.ActiveAlerts.Count > 100)
    snapshot.ActiveAlerts = snapshot.ActiveAlerts.Take(100).ToList();
}

private void ClearAlerts(JournalSyncDashboardSnapshot snapshot, string atmId, string exactTitle)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, atmId, StringComparison.OrdinalIgnoreCase)
    && (exactTitle == null || string.Equals(a.Title, exactTitle, StringComparison.OrdinalIgnoreCase) || string.Equals(a.Title, "Sync delayed", StringComparison.OrdinalIgnoreCase) || string.Equals(a.Title, "No journal sync", StringComparison.OrdinalIgnoreCase)));
}

private void TrimEntries(JournalSyncDashboardSnapshot snapshot)
{
    if (snapshot.RecentEntries.Count > 500)
    snapshot.RecentEntries = snapshot.RecentEntries.Take(500).ToList();
}

private void ClearAlerts(JournalSyncDashboardSnapshot snapshot, string atmId, string? exactTitle)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, atmId, StringComparison.OrdinalIgnoreCase)
    && (exactTitle == null || string.Equals(a.Title, exactTitle, StringComparison.OrdinalIgnoreCase)
    || string.Equals(a.Title, "Sync delayed", StringComparison.OrdinalIgnoreCase)
    || string.Equals(a.Title, "No journal sync", StringComparison.OrdinalIgnoreCase)));
}

}
// Class: JournalSyncTrackerService (from 2 sources)
public sealed partial class JournalSyncTrackerService
{
    // --- Constants & Fields ---
    private readonly string _storageRoot;

    private readonly string _stateDirectory;

    private readonly string _stateFilePath;


    // --- Constructors ---
    public JournalSyncTrackerService(string storageRoot)
    {
        _storageRoot = string.IsNullOrWhiteSpace(storageRoot)
        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")
        : storageRoot;
        _stateDirectory = Path.Combine(_storageRoot, "sync-state");
        _stateFilePath = Path.Combine(_stateDirectory, "journal-sync-dashboard.xml");
    }


// --- Methods ---
private readonly object _sync = new object();

public void EnsureInitialized()
{
    lock (_sync)
    {
        if (!Directory.Exists(_stateDirectory))
        Directory.CreateDirectory(_stateDirectory);
        if (!File.Exists(_stateFilePath))
        SaveInternal(new JournalSyncDashboardSnapshot());
    }
}

public JournalSyncDashboardSnapshot LoadSnapshot()
{
    lock (_sync)
    {
        EnsureInitialized();
        var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
        using (var stream = File.OpenRead(_stateFilePath))
        {
            return serializer.Deserialize(stream) as JournalSyncDashboardSnapshot ?? new JournalSyncDashboardSnapshot();
        }
}
}

public void RecordClientConnected(string atmId, string endpoint, string storageRootPath)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.RemoteEndpoint = endpoint ?? string.Empty;
        atm.IsConnected = true;
        atm.SyncState = JournalSyncState.ConnectedIdle;
        atm.StorageRootPath = storageRootPath ?? string.Empty;
        ClearAlerts(snapshot, atmId, "Disconnected");
    });
}

public void RecordHeartbeat(string atmId)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.LastHeartbeatUtc = DateTime.UtcNow;
        if (atm.SyncState == JournalSyncState.Disconnected)
        atm.SyncState = JournalSyncState.ConnectedIdle;
    });
}

public void RecordSyncReceived(string atmId, string fileName, string relativeStoragePath, long fileSizeBytes, string checksum)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.IsConnected = true;
        atm.SyncState = JournalSyncState.Synced;
        atm.LastJournalReceivedUtc = DateTime.UtcNow;
        atm.LastSuccessfulSyncUtc = DateTime.UtcNow;
        atm.TotalBytesReceived += Math.Max(0, fileSizeBytes);
        atm.TotalFilesReceived += 1;
        atm.LastFileName = fileName ?? string.Empty;
        atm.LastError = string.Empty;

        snapshot.RecentEntries.Insert(0, new JournalSyncEntry
        {
            ATMId = atmId,
            FileName = fileName ?? string.Empty,
            RelativeStoragePath = relativeStoragePath ?? string.Empty,
            FileSizeBytes = fileSizeBytes,
            Checksum = checksum ?? string.Empty,
            Status = "Success",
            Message = "Journal file received and stored.",
            TimestampUtc = DateTime.UtcNow
        });
    TrimEntries(snapshot);
    ClearAlerts(snapshot, atmId, null);
});
}

public void RecordSyncFailure(string atmId, string fileName, string message)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.SyncState = JournalSyncState.Warning;
        atm.LastFailureUtc = DateTime.UtcNow;
        atm.FailedSyncCount += 1;
        atm.LastError = message ?? string.Empty;

        snapshot.RecentEntries.Insert(0, new JournalSyncEntry
        {
            ATMId = atmId,
            FileName = fileName ?? string.Empty,
            Status = "Failure",
            Message = message ?? string.Empty,
            TimestampUtc = DateTime.UtcNow,
            RetryCount = atm.FailedSyncCount
        });
    TrimEntries(snapshot);
    AddOrReplaceAlert(snapshot, new JournalSyncAlert
    {
        ATMId = atmId,
        Severity = JournalSyncAlertSeverity.Warning,
        Title = "Sync failure",
        Message = message ?? "Journal synchronization failed."
    });
});
}

public void RecordClientDisconnected(string atmId)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.IsConnected = false;
        atm.SyncState = JournalSyncState.Disconnected;
        AddOrReplaceAlert(snapshot, new JournalSyncAlert
        {
            ATMId = atmId,
            Severity = JournalSyncAlertSeverity.Warning,
            Title = "Disconnected",
            Message = "ATM disconnected from the central server."
        });
});
}

public void RefreshDerivedAlerts(TimeSpan warningThreshold, TimeSpan criticalThreshold)
{
    Update(snapshot =>
    {
        DateTime now = DateTime.UtcNow;
        foreach (var atm in snapshot.Atms)
        {
            if (!atm.LastSuccessfulSyncUtc.HasValue)
            continue;

            TimeSpan age = now - atm.LastSuccessfulSyncUtc.Value;
            if (age >= criticalThreshold)
            {
                atm.SyncState = JournalSyncState.Critical;
                AddOrReplaceAlert(snapshot, new JournalSyncAlert
                {
                    ATMId = atm.ATMId,
                    Severity = JournalSyncAlertSeverity.Critical,
                    Title = "No journal sync",
                    Message = "No journal synchronization received for " + (int)age.TotalMinutes + " minutes."
                });
        }
    else if (age >= warningThreshold)
    {
        atm.SyncState = JournalSyncState.Warning;
        AddOrReplaceAlert(snapshot, new JournalSyncAlert
        {
            ATMId = atm.ATMId,
            Severity = JournalSyncAlertSeverity.Warning,
            Title = "Sync delayed",
            Message = "Last successful journal sync is older than the warning threshold."
        });
}
}
});
}

private void Update(Action<JournalSyncDashboardSnapshot> action)
{
    lock (_sync)
    {
        var snapshot = LoadSnapshot();
        action(snapshot);
        snapshot.SnapshotAtUtc = DateTime.UtcNow;
        SaveInternal(snapshot);
    }
}

private void SaveInternal(JournalSyncDashboardSnapshot snapshot)
{
    if (!Directory.Exists(_stateDirectory))
    Directory.CreateDirectory(_stateDirectory);
    var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
    using (var stream = File.Create(_stateFilePath))
    {
        serializer.Serialize(stream, snapshot);
    }
}

private AtmJournalSyncStatus GetOrCreateAtm(JournalSyncDashboardSnapshot snapshot, string atmId)
{
    var key = string.IsNullOrWhiteSpace(atmId) ? "Unknown" : atmId.Trim();
    var atm = snapshot.Atms.FirstOrDefault(a => string.Equals(a.ATMId, key, StringComparison.OrdinalIgnoreCase));
    if (atm == null)
    {
        atm = new AtmJournalSyncStatus { ATMId = key, SyncState = JournalSyncState.Unknown };
        snapshot.Atms.Add(atm);
    }
return atm;
}

private void AddOrReplaceAlert(JournalSyncDashboardSnapshot snapshot, JournalSyncAlert alert)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, alert.ATMId, StringComparison.OrdinalIgnoreCase)
    && string.Equals(a.Title, alert.Title, StringComparison.OrdinalIgnoreCase));
    snapshot.ActiveAlerts.Insert(0, alert);
    if (snapshot.ActiveAlerts.Count > 100)
    snapshot.ActiveAlerts = snapshot.ActiveAlerts.Take(100).ToList();
}

private void ClearAlerts(JournalSyncDashboardSnapshot snapshot, string atmId, string exactTitle)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, atmId, StringComparison.OrdinalIgnoreCase)
    && (exactTitle == null || string.Equals(a.Title, exactTitle, StringComparison.OrdinalIgnoreCase) || string.Equals(a.Title, "Sync delayed", StringComparison.OrdinalIgnoreCase) || string.Equals(a.Title, "No journal sync", StringComparison.OrdinalIgnoreCase)));
}

private void TrimEntries(JournalSyncDashboardSnapshot snapshot)
{
    if (snapshot.RecentEntries.Count > 500)
    snapshot.RecentEntries = snapshot.RecentEntries.Take(500).ToList();
}

}
// Class: JournalSyncTrackerService (from 9 sources)
public sealed partial class JournalSyncTrackerService
{
    // --- Constants & Fields ---
    private readonly string _storageRoot;

    private readonly string _stateDirectory;

    private readonly string _stateFilePath;


    // --- Constructors ---
    public JournalSyncTrackerService(string storageRoot)
    {
        _storageRoot = string.IsNullOrWhiteSpace(storageRoot)
        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")
        : storageRoot;
        _stateDirectory = Path.Combine(_storageRoot, "sync-state");
        _stateFilePath = Path.Combine(_stateDirectory, "journal-sync-dashboard.xml");
    }


// --- Methods ---
private readonly object _sync = new object();

public void EnsureInitialized()
{
    lock (_sync)
    {
        if (!Directory.Exists(_stateDirectory))
        Directory.CreateDirectory(_stateDirectory);
        if (!File.Exists(_stateFilePath))
        SaveInternal(new JournalSyncDashboardSnapshot());
    }
}

public JournalSyncDashboardSnapshot LoadSnapshot()
{
    lock (_sync)
    {
        EnsureInitialized();
        var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
        using (var stream = File.OpenRead(_stateFilePath))
        {
            return serializer.Deserialize(stream) as JournalSyncDashboardSnapshot ?? new JournalSyncDashboardSnapshot();
        }
}
}

public void RecordClientConnected(string atmId, string endpoint, string storageRootPath)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.RemoteEndpoint = endpoint ?? string.Empty;
        atm.IsConnected = true;
        atm.SyncState = JournalSyncState.ConnectedIdle;
        atm.StorageRootPath = storageRootPath ?? string.Empty;
        ClearAlerts(snapshot, atmId, "Disconnected");
    });
}

public void RecordHeartbeat(string atmId)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.LastHeartbeatUtc = DateTime.UtcNow;
        if (atm.SyncState == JournalSyncState.Disconnected)
        atm.SyncState = JournalSyncState.ConnectedIdle;
    });
}

public void RecordSyncReceived(string atmId, string fileName, string relativeStoragePath, long fileSizeBytes, string checksum)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.IsConnected = true;
        atm.SyncState = JournalSyncState.Synced;
        atm.LastJournalReceivedUtc = DateTime.UtcNow;
        atm.LastSuccessfulSyncUtc = DateTime.UtcNow;
        atm.TotalBytesReceived += Math.Max(0, fileSizeBytes);
        atm.TotalFilesReceived += 1;
        atm.LastFileName = fileName ?? string.Empty;
        atm.LastError = string.Empty;

        snapshot.RecentEntries.Insert(0, new JournalSyncEntry
        {
            ATMId = atmId,
            FileName = fileName ?? string.Empty,
            RelativeStoragePath = relativeStoragePath ?? string.Empty,
            FileSizeBytes = fileSizeBytes,
            Checksum = checksum ?? string.Empty,
            Status = "Success",
            Message = "Journal file received and stored.",
            TimestampUtc = DateTime.UtcNow
        });
    TrimEntries(snapshot);
    ClearAlerts(snapshot, atmId, null);
});
}

public void RecordSyncFailure(string atmId, string fileName, string message)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.SyncState = JournalSyncState.Warning;
        atm.LastFailureUtc = DateTime.UtcNow;
        atm.FailedSyncCount += 1;
        atm.LastError = message ?? string.Empty;

        snapshot.RecentEntries.Insert(0, new JournalSyncEntry
        {
            ATMId = atmId,
            FileName = fileName ?? string.Empty,
            Status = "Failure",
            Message = message ?? string.Empty,
            TimestampUtc = DateTime.UtcNow,
            RetryCount = atm.FailedSyncCount
        });
    TrimEntries(snapshot);
    AddOrReplaceAlert(snapshot, new JournalSyncAlert
    {
        ATMId = atmId,
        Severity = JournalSyncAlertSeverity.Warning,
        Title = "Sync failure",
        Message = message ?? "Journal synchronization failed."
    });
});
}

public void RecordClientDisconnected(string atmId)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.IsConnected = false;
        atm.SyncState = JournalSyncState.Disconnected;
        AddOrReplaceAlert(snapshot, new JournalSyncAlert
        {
            ATMId = atmId,
            Severity = JournalSyncAlertSeverity.Warning,
            Title = "Disconnected",
            Message = "ATM disconnected from the central server."
        });
});
}

public void RefreshDerivedAlerts(TimeSpan warningThreshold, TimeSpan criticalThreshold)
{
    Update(snapshot =>
    {
        DateTime now = DateTime.UtcNow;
        foreach (var atm in snapshot.Atms)
        {
            if (!atm.LastSuccessfulSyncUtc.HasValue)
            continue;

            TimeSpan age = now - atm.LastSuccessfulSyncUtc.Value;
            if (age >= criticalThreshold)
            {
                atm.SyncState = JournalSyncState.Critical;
                AddOrReplaceAlert(snapshot, new JournalSyncAlert
                {
                    ATMId = atm.ATMId,
                    Severity = JournalSyncAlertSeverity.Critical,
                    Title = "No journal sync",
                    Message = "No journal synchronization received for " + (int)age.TotalMinutes + " minutes."
                });
        }
    else if (age >= warningThreshold)
    {
        atm.SyncState = JournalSyncState.Warning;
        AddOrReplaceAlert(snapshot, new JournalSyncAlert
        {
            ATMId = atm.ATMId,
            Severity = JournalSyncAlertSeverity.Warning,
            Title = "Sync delayed",
            Message = "Last successful journal sync is older than the warning threshold."
        });
}
}
});
}

private void Update(Action<JournalSyncDashboardSnapshot> action)
{
    lock (_sync)
    {
        var snapshot = LoadSnapshot();
        action(snapshot);
        snapshot.SnapshotAtUtc = DateTime.UtcNow;
        SaveInternal(snapshot);
    }
}

private void SaveInternal(JournalSyncDashboardSnapshot snapshot)
{
    if (!Directory.Exists(_stateDirectory))
    Directory.CreateDirectory(_stateDirectory);
    var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
    using (var stream = File.Create(_stateFilePath))
    {
        serializer.Serialize(stream, snapshot);
    }
}

private AtmJournalSyncStatus GetOrCreateAtm(JournalSyncDashboardSnapshot snapshot, string atmId)
{
    var key = string.IsNullOrWhiteSpace(atmId) ? "Unknown" : atmId.Trim();
    var atm = snapshot.Atms.FirstOrDefault(a => string.Equals(a.ATMId, key, StringComparison.OrdinalIgnoreCase));
    if (atm == null)
    {
        atm = new AtmJournalSyncStatus { ATMId = key, SyncState = JournalSyncState.Unknown };
        snapshot.Atms.Add(atm);
    }
return atm;
}

private void AddOrReplaceAlert(JournalSyncDashboardSnapshot snapshot, JournalSyncAlert alert)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, alert.ATMId, StringComparison.OrdinalIgnoreCase)
    && string.Equals(a.Title, alert.Title, StringComparison.OrdinalIgnoreCase));
    snapshot.ActiveAlerts.Insert(0, alert);
    if (snapshot.ActiveAlerts.Count > 100)
    snapshot.ActiveAlerts = snapshot.ActiveAlerts.Take(100).ToList();
}

private void ClearAlerts(JournalSyncDashboardSnapshot snapshot, string atmId, string exactTitle)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, atmId, StringComparison.OrdinalIgnoreCase)
    && (exactTitle == null || string.Equals(a.Title, exactTitle, StringComparison.OrdinalIgnoreCase) || string.Equals(a.Title, "Sync delayed", StringComparison.OrdinalIgnoreCase) || string.Equals(a.Title, "No journal sync", StringComparison.OrdinalIgnoreCase)));
}

private void TrimEntries(JournalSyncDashboardSnapshot snapshot)
{
    if (snapshot.RecentEntries.Count > 500)
    snapshot.RecentEntries = snapshot.RecentEntries.Take(500).ToList();
}

}
// Class: JournalSyncTrackerService (from 5 sources)
public sealed partial class JournalSyncTrackerService
{
    // --- Constants & Fields ---
    private readonly string _storageRoot;

    private readonly string _stateDirectory;

    private readonly string _stateFilePath;


    // --- Constructors ---
    public JournalSyncTrackerService(string storageRoot)
    {
        _storageRoot = string.IsNullOrWhiteSpace(storageRoot)
        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")
        : storageRoot;
        _stateDirectory = Path.Combine(_storageRoot, "sync-state");
        _stateFilePath = Path.Combine(_stateDirectory, "journal-sync-dashboard.xml");
    }


// --- Methods ---
private readonly object _sync = new object();

public void EnsureInitialized()
{
    lock (_sync)
    {
        if (!Directory.Exists(_stateDirectory))
        Directory.CreateDirectory(_stateDirectory);
        if (!File.Exists(_stateFilePath))
        SaveInternal(new JournalSyncDashboardSnapshot());
    }
}

public JournalSyncDashboardSnapshot LoadSnapshot()
{
    lock (_sync)
    {
        EnsureInitialized();
        var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
        using (var stream = File.OpenRead(_stateFilePath))
        {
            return serializer.Deserialize(stream) as JournalSyncDashboardSnapshot ?? new JournalSyncDashboardSnapshot();
        }
}
}

public void RecordClientConnected(string atmId, string endpoint, string storageRootPath)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.RemoteEndpoint = endpoint ?? string.Empty;
        atm.IsConnected = true;
        atm.SyncState = JournalSyncState.ConnectedIdle;
        atm.StorageRootPath = storageRootPath ?? string.Empty;
        ClearAlerts(snapshot, atmId, "Disconnected");
    });
}

public void RecordHeartbeat(string atmId)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.LastHeartbeatUtc = DateTime.UtcNow;
        if (atm.SyncState == JournalSyncState.Disconnected)
        atm.SyncState = JournalSyncState.ConnectedIdle;
    });
}

public void RecordSyncReceived(string atmId, string fileName, string relativeStoragePath, long fileSizeBytes, string checksum)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.IsConnected = true;
        atm.SyncState = JournalSyncState.Synced;
        atm.LastJournalReceivedUtc = DateTime.UtcNow;
        atm.LastSuccessfulSyncUtc = DateTime.UtcNow;
        atm.TotalBytesReceived += Math.Max(0, fileSizeBytes);
        atm.TotalFilesReceived += 1;
        atm.LastFileName = fileName ?? string.Empty;
        atm.LastError = string.Empty;

        snapshot.RecentEntries.Insert(0, new JournalSyncEntry
        {
            ATMId = atmId,
            FileName = fileName ?? string.Empty,
            RelativeStoragePath = relativeStoragePath ?? string.Empty,
            FileSizeBytes = fileSizeBytes,
            Checksum = checksum ?? string.Empty,
            Status = "Success",
            Message = "Journal file received and stored.",
            TimestampUtc = DateTime.UtcNow
        });
    TrimEntries(snapshot);
    ClearAlerts(snapshot, atmId, null);
});
}

public void RecordSyncFailure(string atmId, string fileName, string message)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.SyncState = JournalSyncState.Warning;
        atm.LastFailureUtc = DateTime.UtcNow;
        atm.FailedSyncCount += 1;
        atm.LastError = message ?? string.Empty;

        snapshot.RecentEntries.Insert(0, new JournalSyncEntry
        {
            ATMId = atmId,
            FileName = fileName ?? string.Empty,
            Status = "Failure",
            Message = message ?? string.Empty,
            TimestampUtc = DateTime.UtcNow,
            RetryCount = atm.FailedSyncCount
        });
    TrimEntries(snapshot);
    AddOrReplaceAlert(snapshot, new JournalSyncAlert
    {
        ATMId = atmId,
        Severity = JournalSyncAlertSeverity.Warning,
        Title = "Sync failure",
        Message = message ?? "Journal synchronization failed."
    });
});
}

public void RecordClientDisconnected(string atmId)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.IsConnected = false;
        atm.SyncState = JournalSyncState.Disconnected;
        AddOrReplaceAlert(snapshot, new JournalSyncAlert
        {
            ATMId = atmId,
            Severity = JournalSyncAlertSeverity.Warning,
            Title = "Disconnected",
            Message = "ATM disconnected from the central server."
        });
});
}

public void RefreshDerivedAlerts(TimeSpan warningThreshold, TimeSpan criticalThreshold)
{
    Update(snapshot =>
    {
        DateTime now = DateTime.UtcNow;
        foreach (var atm in snapshot.Atms)
        {
            if (!atm.LastSuccessfulSyncUtc.HasValue)
            continue;

            TimeSpan age = now - atm.LastSuccessfulSyncUtc.Value;
            if (age >= criticalThreshold)
            {
                atm.SyncState = JournalSyncState.Critical;
                AddOrReplaceAlert(snapshot, new JournalSyncAlert
                {
                    ATMId = atm.ATMId,
                    Severity = JournalSyncAlertSeverity.Critical,
                    Title = "No journal sync",
                    Message = "No journal synchronization received for " + (int)age.TotalMinutes + " minutes."
                });
        }
    else if (age >= warningThreshold)
    {
        atm.SyncState = JournalSyncState.Warning;
        AddOrReplaceAlert(snapshot, new JournalSyncAlert
        {
            ATMId = atm.ATMId,
            Severity = JournalSyncAlertSeverity.Warning,
            Title = "Sync delayed",
            Message = "Last successful journal sync is older than the warning threshold."
        });
}
}
});
}

private void Update(Action<JournalSyncDashboardSnapshot> action)
{
    lock (_sync)
    {
        var snapshot = LoadSnapshot();
        action(snapshot);
        snapshot.SnapshotAtUtc = DateTime.UtcNow;
        SaveInternal(snapshot);
    }
}

private void SaveInternal(JournalSyncDashboardSnapshot snapshot)
{
    if (!Directory.Exists(_stateDirectory))
    Directory.CreateDirectory(_stateDirectory);
    var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
    using (var stream = File.Create(_stateFilePath))
    {
        serializer.Serialize(stream, snapshot);
    }
}

private AtmJournalSyncStatus GetOrCreateAtm(JournalSyncDashboardSnapshot snapshot, string atmId)
{
    var key = string.IsNullOrWhiteSpace(atmId) ? "Unknown" : atmId.Trim();
    var atm = snapshot.Atms.FirstOrDefault(a => string.Equals(a.ATMId, key, StringComparison.OrdinalIgnoreCase));
    if (atm == null)
    {
        atm = new AtmJournalSyncStatus { ATMId = key, SyncState = JournalSyncState.Unknown };
        snapshot.Atms.Add(atm);
    }
return atm;
}

private void AddOrReplaceAlert(JournalSyncDashboardSnapshot snapshot, JournalSyncAlert alert)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, alert.ATMId, StringComparison.OrdinalIgnoreCase)
    && string.Equals(a.Title, alert.Title, StringComparison.OrdinalIgnoreCase));
    snapshot.ActiveAlerts.Insert(0, alert);
    if (snapshot.ActiveAlerts.Count > 100)
    snapshot.ActiveAlerts = snapshot.ActiveAlerts.Take(100).ToList();
}

private void ClearAlerts(JournalSyncDashboardSnapshot snapshot, string atmId, string exactTitle)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, atmId, StringComparison.OrdinalIgnoreCase)
    && (exactTitle == null || string.Equals(a.Title, exactTitle, StringComparison.OrdinalIgnoreCase) || string.Equals(a.Title, "Sync delayed", StringComparison.OrdinalIgnoreCase) || string.Equals(a.Title, "No journal sync", StringComparison.OrdinalIgnoreCase)));
}

private void TrimEntries(JournalSyncDashboardSnapshot snapshot)
{
    if (snapshot.RecentEntries.Count > 500)
    snapshot.RecentEntries = snapshot.RecentEntries.Take(500).ToList();
}

}
/// <summary>
/// متتبع متكامل لحالة مزامنة السجلات مع استمرارية XML.
/// يدير Dashboard Snapshot، التنبيهات، وسجل الإدخالات الحديثة.
/// </summary>
public sealed class JournalSyncTrackerService
{
    private readonly object _sync = new();
    private readonly string _storageRoot;
    private readonly string _stateDirectory;
    private readonly string _stateFilePath;

    public JournalSyncTrackerService(string storageRoot)
    {
        _storageRoot = string.IsNullOrWhiteSpace(storageRoot)
        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")
        : storageRoot;
        _stateDirectory = Path.Combine(_storageRoot, "sync-state");
        _stateFilePath = Path.Combine(_stateDirectory, "journal-sync-dashboard.xml");
    }

public void EnsureInitialized()
{
    lock (_sync)
    {
        if (!Directory.Exists(_stateDirectory))
        Directory.CreateDirectory(_stateDirectory);
        if (!File.Exists(_stateFilePath))
        SaveInternal(new JournalSyncDashboardSnapshot());
    }
}

public JournalSyncDashboardSnapshot LoadSnapshot()
{
    lock (_sync)
    {
        EnsureInitialized();
        var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
        using (var stream = File.OpenRead(_stateFilePath))
        {
            return serializer.Deserialize(stream) as JournalSyncDashboardSnapshot ?? new JournalSyncDashboardSnapshot();
        }
}
}

public void RecordClientConnected(string atmId, string endpoint, string storageRootPath)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.RemoteEndpoint = endpoint ?? string.Empty;
        atm.IsConnected = true;
        atm.SyncState = JournalSyncState.ConnectedIdle;
        atm.StorageRootPath = storageRootPath ?? string.Empty;
        ClearAlerts(snapshot, atmId, "Disconnected");
    });
}

public void RecordHeartbeat(string atmId)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.LastHeartbeatUtc = DateTime.UtcNow;
        if (atm.SyncState == JournalSyncState.Disconnected)
        atm.SyncState = JournalSyncState.ConnectedIdle;
    });
}

public void RecordSyncReceived(string atmId, string fileName, string relativeStoragePath, long fileSizeBytes, string checksum)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.IsConnected = true;
        atm.SyncState = JournalSyncState.Synced;
        atm.LastJournalReceivedUtc = DateTime.UtcNow;
        atm.LastSuccessfulSyncUtc = DateTime.UtcNow;
        atm.TotalBytesReceived += Math.Max(0, fileSizeBytes);
        atm.TotalFilesReceived += 1;
        atm.LastFileName = fileName ?? string.Empty;
        atm.LastError = string.Empty;

        snapshot.RecentEntries.Insert(0, new JournalSyncEntry
        {
            ATMId = atmId,
            FileName = fileName ?? string.Empty,
            RelativeStoragePath = relativeStoragePath ?? string.Empty,
            FileSizeBytes = fileSizeBytes,
            Checksum = checksum ?? string.Empty,
            Status = "Success",
            Message = "Journal file received and stored.",
            TimestampUtc = DateTime.UtcNow
        });
    TrimEntries(snapshot);
    ClearAlerts(snapshot, atmId, null);
});
}

public void RecordSyncFailure(string atmId, string fileName, string message)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.SyncState = JournalSyncState.Warning;
        atm.LastFailureUtc = DateTime.UtcNow;
        atm.FailedSyncCount += 1;
        atm.LastError = message ?? string.Empty;

        snapshot.RecentEntries.Insert(0, new JournalSyncEntry
        {
            ATMId = atmId,
            FileName = fileName ?? string.Empty,
            Status = "Failure",
            Message = message ?? string.Empty,
            TimestampUtc = DateTime.UtcNow,
            RetryCount = atm.FailedSyncCount
        });
    TrimEntries(snapshot);
    AddOrReplaceAlert(snapshot, new JournalSyncAlert
    {
        ATMId = atmId,
        Severity = JournalSyncAlertSeverity.Warning,
        Title = "Sync failure",
        Message = message ?? "Journal synchronization failed."
    });
});
}

public void RecordClientDisconnected(string atmId)
{
    Update(snapshot =>
    {
        var atm = GetOrCreateAtm(snapshot, atmId);
        atm.IsConnected = false;
        atm.SyncState = JournalSyncState.Disconnected;
        AddOrReplaceAlert(snapshot, new JournalSyncAlert
        {
            ATMId = atmId,
            Severity = JournalSyncAlertSeverity.Warning,
            Title = "Disconnected",
            Message = "ATM disconnected from the central server."
        });
});
}

public void RefreshDerivedAlerts(TimeSpan warningThreshold, TimeSpan criticalThreshold)
{
    Update(snapshot =>
    {
        DateTime now = DateTime.UtcNow;
        foreach (var atm in snapshot.Atms)
        {
            if (!atm.LastSuccessfulSyncUtc.HasValue)
            continue;

            TimeSpan age = now - atm.LastSuccessfulSyncUtc.Value;
            if (age >= criticalThreshold)
            {
                atm.SyncState = JournalSyncState.Critical;
                AddOrReplaceAlert(snapshot, new JournalSyncAlert
                {
                    ATMId = atm.ATMId,
                    Severity = JournalSyncAlertSeverity.Critical,
                    Title = "No journal sync",
                    Message = "No journal synchronization received for " + (int)age.TotalMinutes + " minutes."
                });
        }
    else if (age >= warningThreshold)
    {
        atm.SyncState = JournalSyncState.Warning;
        AddOrReplaceAlert(snapshot, new JournalSyncAlert
        {
            ATMId = atm.ATMId,
            Severity = JournalSyncAlertSeverity.Warning,
            Title = "Sync delayed",
            Message = "Last successful journal sync is older than the warning threshold."
        });
}
}
});
}

private void Update(Action<JournalSyncDashboardSnapshot> action)
{
    lock (_sync)
    {
        var snapshot = LoadSnapshot();
        action(snapshot);
        snapshot.SnapshotAtUtc = DateTime.UtcNow;
        SaveInternal(snapshot);
    }
}

private void SaveInternal(JournalSyncDashboardSnapshot snapshot)
{
    if (!Directory.Exists(_stateDirectory))
    Directory.CreateDirectory(_stateDirectory);
    var serializer = new XmlSerializer(typeof(JournalSyncDashboardSnapshot));
    using (var stream = File.Create(_stateFilePath))
    {
        serializer.Serialize(stream, snapshot);
    }
}

private AtmJournalSyncStatus GetOrCreateAtm(JournalSyncDashboardSnapshot snapshot, string atmId)
{
    var key = string.IsNullOrWhiteSpace(atmId) ? "Unknown" : atmId.Trim();
    var atm = snapshot.Atms.FirstOrDefault(a => string.Equals(a.ATMId, key, StringComparison.OrdinalIgnoreCase));
    if (atm == null)
    {
        atm = new AtmJournalSyncStatus { ATMId = key, SyncState = JournalSyncState.Unknown };
        snapshot.Atms.Add(atm);
    }
return atm;
}

private void AddOrReplaceAlert(JournalSyncDashboardSnapshot snapshot, JournalSyncAlert alert)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, alert.ATMId, StringComparison.OrdinalIgnoreCase)
    && string.Equals(a.Title, alert.Title, StringComparison.OrdinalIgnoreCase));
    snapshot.ActiveAlerts.Insert(0, alert);
    if (snapshot.ActiveAlerts.Count > 100)
    snapshot.ActiveAlerts = snapshot.ActiveAlerts.Take(100).ToList();
}

private void ClearAlerts(JournalSyncDashboardSnapshot snapshot, string atmId, string? exactTitle)
{
    snapshot.ActiveAlerts.RemoveAll(a => string.Equals(a.ATMId, atmId, StringComparison.OrdinalIgnoreCase)
    && (exactTitle == null || string.Equals(a.Title, exactTitle, StringComparison.OrdinalIgnoreCase)
    || string.Equals(a.Title, "Sync delayed", StringComparison.OrdinalIgnoreCase)
    || string.Equals(a.Title, "No journal sync", StringComparison.OrdinalIgnoreCase)));
}

private void TrimEntries(JournalSyncDashboardSnapshot snapshot)
{
    if (snapshot.RecentEntries.Count > 500)
    snapshot.RecentEntries = snapshot.RecentEntries.Take(500).ToList();
}
}
}


{
    public class JournalSyncTrackerService { }
}


public partial class JournalSyncTrackerService
{
}