using System;
using System.IO;
using System.Text;

namespace EJLive.Core.Health
{
    /// <summary>
    /// Generates and writes atomic health.json snapshots for the EJLive Client Agent.
    /// Reports service state, connection, outbox, watcher status, disk usage.
    /// </summary>
    public sealed class HealthReporter
    {
        private readonly string _healthPath;
        private readonly object _writeLock = new object();

        public HealthReporter(string healthPath)
        {
            _healthPath = healthPath ?? throw new ArgumentNullException(nameof(healthPath));
        }

        /// <summary>
        /// Writes a health snapshot atomically to health.json.
        /// Uses temp file + rename to prevent partial reads.
        /// </summary>
        public void WriteSnapshot(HealthSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            lock (_writeLock)
            {
                var dir = Path.GetDirectoryName(_healthPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = SerializeSnapshot(snapshot);
                var tmpPath = _healthPath + ".tmp";

                File.WriteAllText(tmpPath, json, Encoding.UTF8);
                if (File.Exists(_healthPath))
                    File.Delete(_healthPath);
                File.Move(tmpPath, _healthPath);
            }
        }

        /// <summary>
        /// Reads the last health snapshot. Returns null if not found.
        /// </summary>
        public HealthSnapshot ReadSnapshot()
        {
            lock (_writeLock)
            {
                if (!File.Exists(_healthPath))
                    return null;

                try
                {
                    var json = File.ReadAllText(_healthPath, Encoding.UTF8);
                    return DeserializeSnapshot(json);
                }
                catch
                {
                    return null;
                }
            }
        }

        private static string SerializeSnapshot(HealthSnapshot s)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"agentState\": \"{Escape(s.AgentState)}\",");
            sb.AppendLine($"  \"sessionId\": \"{Escape(s.SessionId)}\",");
            sb.AppendLine($"  \"networkConnected\": {s.NetworkConnected.ToString().ToLowerInvariant()},");
            sb.AppendLine($"  \"lastHeartbeatUtc\": \"{s.LastHeartbeatUtc:o}\",");
            sb.AppendLine($"  \"lastSyncUtc\": \"{s.LastSyncUtc:o}\",");
            sb.AppendLine($"  \"outboxCount\": {s.OutboxCount},");
            sb.AppendLine($"  \"failedCount\": {s.FailedCount},");
            sb.AppendLine($"  \"watcherState\": \"{Escape(s.WatcherState)}\",");
            sb.AppendLine($"  \"imageInboxState\": \"{Escape(s.ImageInboxState)}\",");
            sb.AppendLine($"  \"healthScore\": \"{Escape(s.HealthScore)}\",");
            sb.AppendLine($"  \"errorCount\": {s.ErrorCount},");
            sb.Append($"  \"lastError\": \"{Escape(s.LastError)}\"");
            sb.Append("}");
            return sb.ToString();
        }

        private static HealthSnapshot DeserializeSnapshot(string json)
        {
            var s = new HealthSnapshot();
            var lines = json.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim().TrimEnd(',');
                var colonIdx = trimmed.IndexOf(':');
                if (colonIdx < 0) continue;
                var key = trimmed.Substring(0, colonIdx).Trim().Trim('"');
                var value = trimmed.Substring(colonIdx + 1).Trim().Trim('"');

                switch (key)
                {
                    case "agentState": s.AgentState = value; break;
                    case "sessionId": s.SessionId = value; break;
                    case "networkConnected": s.NetworkConnected = value == "true"; break;
                    case "lastHeartbeatUtc": if (DateTimeOffset.TryParse(value, out var lh)) s.LastHeartbeatUtc = lh; break;
                    case "lastSyncUtc": if (DateTimeOffset.TryParse(value, out var ls)) s.LastSyncUtc = ls; break;
                    case "outboxCount": if (int.TryParse(value, out var oc)) s.OutboxCount = oc; break;
                    case "failedCount": if (int.TryParse(value, out var fc)) s.FailedCount = fc; break;
                    case "watcherState": s.WatcherState = value; break;
                    case "imageInboxState": s.ImageInboxState = value; break;
                    case "healthScore": s.HealthScore = value; break;
                    case "errorCount": if (int.TryParse(value, out var ec)) s.ErrorCount = ec; break;
                    case "lastError": s.LastError = value; break;
                }
            }
            return s;
        }

        private static string Escape(string s) => (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    /// <summary>
    /// Client agent health snapshot model.
    /// </summary>
    public sealed class HealthSnapshot
    {
        public string AgentState { get; set; } = "Unknown";
        public string SessionId { get; set; } = string.Empty;
        public bool NetworkConnected { get; set; }
        public DateTimeOffset LastHeartbeatUtc { get; set; } = DateTimeOffset.MinValue;
        public DateTimeOffset LastSyncUtc { get; set; } = DateTimeOffset.MinValue;
        public int OutboxCount { get; set; }
        public int FailedCount { get; set; }
        public string WatcherState { get; set; } = "Unknown";
        public string ImageInboxState { get; set; } = "Unknown";
        public string HealthScore { get; set; } = "Unknown";
        public int ErrorCount { get; set; }
        public string LastError { get; set; } = string.Empty;
        public DateTimeOffset SnapshotUtc { get; set; } = DateTimeOffset.UtcNow;
    }
}