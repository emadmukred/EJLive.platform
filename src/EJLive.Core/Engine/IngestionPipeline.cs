using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Core.Data;
using EJLive.Core.Data.Repositories;
using EJLive.Core.Models;
using EJLive.Core.Services;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Server-side ingestion pipeline: receive -> verify -> archive -> analyze -> snapshot.
    /// Operates outside the UI thread.
    /// </summary>
    public sealed class IngestionPipeline : IDisposable
    {
        private readonly string _stagingRoot;
        private readonly string _archiveRoot;
        private readonly DatabaseManager _db;
        private readonly JournalArchiveRepository _archive;
        private readonly ParserTransactionRepository _parserTransactions;
        private readonly CancellationTokenSource _cts = new();

        public event Action<IngestionResult>? OnIngested;
        public event Action<string>? OnLog;

        public IngestionPipeline(string stagingRoot, string archiveRoot, DatabaseManager db)
        {
            _stagingRoot = stagingRoot ?? throw new ArgumentNullException(nameof(stagingRoot));
            _archiveRoot = archiveRoot ?? throw new ArgumentNullException(nameof(archiveRoot));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _archive = new JournalArchiveRepository(_db);
            _parserTransactions = new ParserTransactionRepository(_db);
            Directory.CreateDirectory(_stagingRoot);
            Directory.CreateDirectory(_archiveRoot);
        }

        public async Task<IngestionResult> IngestAsync(string sourcePath, string atmId, string originalFileName, CancellationToken token)
        {
            var linked = CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token);
            var ct = linked.Token;
            var result = new IngestionResult { AtmId = atmId, OriginalFileName = originalFileName };

            try
            {
                // 1. Stage
                var stagedPath = Path.Combine(_stagingRoot, $"{Guid.NewGuid()}-{originalFileName}");
                File.Copy(sourcePath, stagedPath, overwrite: true);
                result.StagedPath = stagedPath;
                Log($"Staged: {stagedPath}");

                // 2. Verify SHA256 if provided
                string computedHash;
                await using (var shaStream = File.OpenRead(stagedPath))
                {
                    computedHash = Convert.ToHexString(await SHA256.HashDataAsync(shaStream, ct));
                }
                result.ComputedSha256 = computedHash;

                // 3. Archive
                var archiveDir = Path.Combine(_archiveRoot, DateTime.UtcNow.ToString("yyyy-MM"), atmId);
                Directory.CreateDirectory(archiveDir);
                var archivePath = Path.Combine(archiveDir, $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{originalFileName}");
                File.Move(stagedPath, archivePath);
                result.ArchivePath = archivePath;
                Log($"Archived: {archivePath}");

                // 4. Record in DB
                RecordArchive(atmId, originalFileName, archivePath, computedHash, result);

                // 5. Launch analysis (fire-and-forget with error handling)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await AnalyzeAsync(archivePath, atmId, result, ct);
                    }
                    catch (Exception ex)
                    {
                        Log($"Analysis failed for {archivePath}: {ex.Message}");
                    }
                }, ct);

                result.Success = true;
                OnIngested?.Invoke(result);
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                Log($"Ingestion failed: {ex.Message}");
                Cleanup(stagedPath: result.StagedPath);
                OnIngested?.Invoke(result);
                return result;
            }
        }

        private void RecordArchive(string atmId, string fileName, string archivePath, string sha256, IngestionResult result)
        {
            // SS8 ingest rule: the archive write is the commit point for JournalAck. The row is
            // idempotent on (atm_id, file_name, sha256); a resend re-points to the existing entry
            // instead of creating a second one (SS-15 pattern #2).
            var entryId = Guid.NewGuid().ToString("N");
            var size = File.Exists(archivePath) ? new FileInfo(archivePath).Length : 0L;
            var receivedAt = DateTime.UtcNow;

            var inserted = _archive.TryInsert(new JournalArchiveRecord(
                EntryId: entryId,
                AtmId: atmId,
                FileName: fileName,
                OriginalSize: size,
                CompressedSize: size,
                EncryptedSize: size,
                IsEncrypted: false,
                IsCompressed: false,
                Checksum: sha256,
                Sha256Hash: sha256,
                TransactionCount: 0,
                ArchivePath: archivePath,
                ReceivedAtUtc: receivedAt));

            result.ArchiveEntryId = entryId;
            Log(inserted
                ? $"DB archive record created: {entryId}"
                : $"DB archive record already present (duplicate resend): {entryId}");
        }

        private async Task AnalyzeAsync(string archivePath, string atmId, IngestionResult result, CancellationToken ct)
        {
            // SS8 ingest lane: journal archive -> parser -> parser_transactions. Vendor sniffing
            // uses the shared evidence rules; the registry resolves one parser per vendor (POL-1).
            var lines = await Task.Run(() => File.ReadAllLines(archivePath), ct).ConfigureAwait(false);
            var head = string.Join("\n", lines.Take(24));
            var vendor = Services.UnifiedJournalEvidenceAnalyzer.DetectVendor(null, head);
            var parser = EjParserRegistry.Default.Resolve(vendor);
            var scopeId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId;

            var transactions = parser.Parse(lines.ToList(), scopeId);

            if (!string.IsNullOrWhiteSpace(result.ArchiveEntryId))
            {
                var rows = transactions
                    .Take(5_000)
                    .Select(t => new ParserTransactionRecord(
                        TransactionNumber: TryLineNumber(t.TransactionId),
                        DateUtc: t.Timestamp == DateTime.MinValue ? null : t.Timestamp,
                        AtmId: t.ATM_ID,
                        Vendor: vendor,
                        CardMasked: string.IsNullOrEmpty(t.CardNumber) ? null : EJLive.Core.SecretRedactor.MaskCard(t.CardNumber),
                        Amount: t.Amount,
                        Currency: t.Currency,
                        Stan: t.STAN,
                        Rrn: t.RRN,
                        MCode: t.MCode,
                        RCode: t.RCode,
                        HostResponse: null,
                        Status: t.Classification.ToString(),
                        Confidence: t.Confidence.ToString("0.00", CultureInfo.InvariantCulture),
                        RawStartLine: t.StartLine,
                        RawEndLine: t.EndLine,
                        Evidence: $"lines {t.StartLine}-{t.EndLine} ({t.RawLines.Count} raw)"))
                    .ToList();
                _parserTransactions.InsertBatch(result.ArchiveEntryId, rows);
                _archive.UpdateTransactionCount(result.ArchiveEntryId, transactions.Count);
            }

            var classified = transactions.Count(t => t.Classification == TransactionClassification.Success);
            var suspicious = transactions.Count(t => t.Classification == TransactionClassification.Suspicious);
            result.AnalysisSummary =
                $"Parsed {transactions.Count} transactions via {parser.GetType().Name} (vendor {vendor}): " +
                $"{classified} success, {suspicious} suspicious.";
            Log($"Analysis complete: {result.AnalysisSummary}");
        }

        private static int? TryLineNumber(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
                return null;
            var tail = transactionId[(transactionId.LastIndexOf('-') + 1)..];
            return int.TryParse(tail, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;
        }

        private void Cleanup(string? stagedPath)
        {
            if (stagedPath != null && File.Exists(stagedPath))
            {
                try { File.Delete(stagedPath); } catch { }
            }
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[{DateTime.UtcNow:O}] [Ingestion] {message}");
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    public sealed class IngestionResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string AtmId { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string? StagedPath { get; set; }
        public string? ArchivePath { get; set; }
        public Guid? ArchiveId { get; set; }

        /// <summary><c>journal_archive.entry_id</c> for this ingest (parser_transactions FK).</summary>
        public string? ArchiveEntryId { get; set; }
        public string? ComputedSha256 { get; set; }
        public string? AnalysisSummary { get; set; }
    }
}
