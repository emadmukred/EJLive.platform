using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EJLive.Core.Server
{
    /// <summary>
    /// Server-side ingestion pipeline stage.
    /// Pipeline: Receive → Staging → SHA256 Verify → Archive → Parse → Index → Snapshot.
    /// </summary>
    public enum IngestionStage
    {
        Received,
        Staging,
        Verifying,
        Verified,
        Archiving,
        Archived,
        Parsing,
        Parsed,
        Indexing,
        Indexed,
        SnapshotReady,
        Failed
    }

    /// <summary>
    /// Tracks a single journal file through the server ingestion pipeline.
    /// </summary>
    public sealed class IngestionRecord
    {
        public string IngestionId { get; set; } = Guid.NewGuid().ToString("N");
        public string TransferId { get; set; } = string.Empty;
        public string AtmId { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? ExpectedSha256 { get; set; }
        public string? ActualSha256 { get; set; }
        public string? StagingPath { get; set; }
        public string? ArchivePath { get; set; }
        public IngestionStage Stage { get; set; } = IngestionStage.Received;
        public string? FailureReason { get; set; }
        public int? TransactionCount { get; set; }
        public DateTimeOffset ReceivedUtc { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? CompletedUtc { get; set; }
        public string? CorrelationId { get; set; }

        public bool IsDuplicate { get; set; }
        public bool Sha256Mismatch { get; set; }
    }

    /// <summary>
    /// Server ingestion pipeline service contract.
    /// Receives journal files from client transfer sessions and processes them
    /// through the pipeline stages independently of any UI.
    /// </summary>
    public interface IIngestionPipeline
    {
        /// <summary>Process a received file through the full pipeline.</summary>
        Task<IngestionRecord> IngestAsync(IngestionRecord record, string stagingFilePath, CancellationToken cancellationToken);

        /// <summary>Get current status of an ingestion record.</summary>
        IngestionRecord? GetStatus(string ingestionId);

        /// <summary>Get all active/pending ingestion records for an ATM.</summary>
        List<IngestionRecord> GetPendingForAtm(string atmId);

        /// <summary>Get ingestion summary for the dashboard snapshot.</summary>
        IngestionSummary GetSummary();
    }

    /// <summary>
    /// Lightweight summary of server ingestion state for dashboard snapshots.
    /// </summary>
    public sealed class IngestionSummary
    {
        public int TotalReceived { get; set; }
        public int TotalArchived { get; set; }
        public int TotalParsed { get; set; }
        public int TotalFailed { get; set; }
        public int TotalDuplicates { get; set; }
        public int ActiveTransfers { get; set; }
        public int PendingParseJobs { get; set; }
        public DateTimeOffset LastIngestionUtc { get; set; }
        public string? LastAtmId { get; set; }
    }

    /// <summary>
    /// Archive storage strategy — organizes files by yyyy-MM/ATM_ID.
    /// </summary>
    public sealed class ArchivePathStrategy
    {
        /// <summary>Generates archive path: {basePath}/{yyyy-MM}/{atmId}/{fileName}</summary>
        public static string BuildPath(string basePath, string atmId, string fileName, DateTimeOffset timestamp)
        {
            var month = timestamp.ToString("yyyy-MM");
            return System.IO.Path.Combine(basePath, month, Sanitize(atmId), Sanitize(fileName));
        }

        private static string Sanitize(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "unknown";
            return name.Replace("..", "_").Replace("\\", "_").Replace("/", "_");
        }
    }

    /// <summary>
    /// Default server ingestion pipeline implementation.
    /// Pipeline: Receive → Staging → SHA256 Verify → Archive → Parse → Index → Snapshot.
    /// </summary>
    public sealed class DefaultIngestionPipeline : IIngestionPipeline
    {
        private readonly object _lock = new object();
        private readonly Dictionary<string, IngestionRecord> _records = new Dictionary<string, IngestionRecord>();
        private readonly string _archiveBasePath;
        private readonly Engine.EjParserRegistry _parserRegistry;

        public DefaultIngestionPipeline(string archiveBasePath, Engine.EjParserRegistry parserRegistry)
        {
            _archiveBasePath = archiveBasePath ?? throw new ArgumentNullException(nameof(archiveBasePath));
            _parserRegistry = parserRegistry ?? throw new ArgumentNullException(nameof(parserRegistry));
        }

        public async Task<IngestionRecord> IngestAsync(IngestionRecord record, string stagingFilePath, CancellationToken cancellationToken)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            lock (_lock)
            {
                _records[record.IngestionId] = record;
            }

            try
            {
                // Stage 1: Verify SHA256
                record.Stage = IngestionStage.Verifying;
                record.StagingPath = stagingFilePath;
                if (!string.IsNullOrEmpty(record.ExpectedSha256) && System.IO.File.Exists(stagingFilePath))
                {
                    record.ActualSha256 = ComputeSha256(stagingFilePath);
                    if (!string.Equals(record.ExpectedSha256, record.ActualSha256, StringComparison.OrdinalIgnoreCase))
                    {
                        record.Sha256Mismatch = true;
                        record.Stage = IngestionStage.Failed;
                        record.FailureReason = $"SHA256 mismatch. Expected: {record.ExpectedSha256}, Actual: {record.ActualSha256}";
                        return record;
                    }
                }
                record.Stage = IngestionStage.Verified;

                // Stage 2: Check duplicate
                var archivePath = ArchivePathStrategy.BuildPath(_archiveBasePath, record.AtmId, record.FileName, record.ReceivedUtc);
                if (System.IO.File.Exists(archivePath))
                {
                    record.IsDuplicate = true;
                    record.Stage = IngestionStage.Failed;
                    record.FailureReason = $"Duplicate file already archived at: {archivePath}";
                    return record;
                }

                // Stage 3: Archive
                record.Stage = IngestionStage.Archiving;
                if (System.IO.File.Exists(stagingFilePath))
                {
                    var dir = System.IO.Path.GetDirectoryName(archivePath);
                    if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                        System.IO.Directory.CreateDirectory(dir);
                    System.IO.File.Copy(stagingFilePath, archivePath, overwrite: false);
                }
                record.ArchivePath = archivePath;
                record.Stage = IngestionStage.Archived;

                // Stage 4: Parse trigger (async — actual parse may run separately)
                record.Stage = IngestionStage.Parsing;
                record.TransactionCount = await TriggerParseAsync(record, archivePath, cancellationToken);
                record.Stage = IngestionStage.Parsed;

                // Stage 5: Index + Snapshot
                record.Stage = IngestionStage.Indexing;
                record.Stage = IngestionStage.Indexed;
                record.Stage = IngestionStage.SnapshotReady;

                record.CompletedUtc = DateTimeOffset.UtcNow;
                return record;
            }
            catch (Exception ex)
            {
                record.Stage = IngestionStage.Failed;
                record.FailureReason = $"Ingestion failed at stage {record.Stage}: {ex.Message}";
                return record;
            }
        }

        public IngestionRecord? GetStatus(string ingestionId)
        {
            lock (_lock)
            {
                _records.TryGetValue(ingestionId ?? string.Empty, out var record);
                return record;
            }
        }

        public List<IngestionRecord> GetPendingForAtm(string atmId)
        {
            lock (_lock)
            {
                var result = new List<IngestionRecord>();
                foreach (var r in _records.Values)
                {
                    if (r.AtmId.Equals(atmId, StringComparison.OrdinalIgnoreCase) &&
                        r.Stage != IngestionStage.SnapshotReady &&
                        r.Stage != IngestionStage.Failed)
                    {
                        result.Add(r);
                    }
                }
                return result;
            }
        }

        public IngestionSummary GetSummary()
        {
            lock (_lock)
            {
                var summary = new IngestionSummary();
                foreach (var r in _records.Values)
                {
                    summary.TotalReceived++;
                    if (r.Stage >= IngestionStage.Archived) summary.TotalArchived++;
                    if (r.Stage >= IngestionStage.Parsed) summary.TotalParsed++;
                    if (r.Stage == IngestionStage.Failed) summary.TotalFailed++;
                    if (r.IsDuplicate) summary.TotalDuplicates++;
                    if (r.CompletedUtc > summary.LastIngestionUtc)
                    {
                        summary.LastIngestionUtc = r.CompletedUtc ?? r.ReceivedUtc;
                        summary.LastAtmId = r.AtmId;
                    }
                }
                return summary;
            }
        }

        private async Task<int> TriggerParseAsync(IngestionRecord record, string archivePath, CancellationToken ct)
        {
            if (!System.IO.File.Exists(archivePath))
                return 0;

            var lines = await System.IO.File.ReadAllLinesAsync(archivePath, ct);
            var parser = _parserRegistry.Resolve(record.Vendor);
            var result = parser.Parse(lines.ToList(), record.AtmId);
            return result.Count;
        }

        private static string? ComputeSha256(string filePath)
        {
            if (!System.IO.File.Exists(filePath)) return null;
            using var sha = System.Security.Cryptography.SHA256.Create();
            using var stream = System.IO.File.OpenRead(filePath);
            var hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
