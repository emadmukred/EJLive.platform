using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Client.WinForms.Services
{
    public sealed class AdvancedJournalEntry
    {
        public string ATMId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public int OriginalSize { get; set; }
        public int CompressedSize { get; set; }
        public int EncryptedSize { get; set; }
        public bool IsEncrypted { get; set; }
        public bool IsCompressed { get; set; }
        public string Checksum { get; set; } = string.Empty;
        public string MD5Hash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ReceivedAt { get; set; }
        public string Status { get; set; } = "Ready";
        public int TransactionCount { get; set; }
    }

    public sealed class AdvancedSyncStatusInfo
    {
        public string ATMId { get; set; } = string.Empty;
        public string Status { get; set; } = "Idle";
        public int TotalFiles { get; set; }
        public int SyncedFiles { get; set; }
        public int FailedFiles { get; set; }
        public int ProgressPercentage { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? FailureReason { get; set; }
    }

    /// <summary>
    /// Advanced journal processor rebuilt into a single operational implementation.
    /// </summary>
    public sealed class AdvancedJournalProcessor : IDisposable
    {
        private readonly string _journalPath;
        private readonly ATMType _atmType;
        private readonly string _atmId;
        private readonly HashSet<string> _processedFiles = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _sync = new();

        private Thread? _processingThread;
        private volatile bool _isRunning;

        public event Action<string>? OnLogMessage;
        public event Action<AdvancedJournalEntry>? OnJournalEntryReady;
        public event Action<AdvancedSyncStatusInfo>? OnSyncStatusChanged;

        public bool IsRunning => _isRunning;

        public AdvancedJournalProcessor(string journalPath, ATMType atmType, string atmId)
        {
            _journalPath = journalPath ?? string.Empty;
            _atmType = atmType;
            _atmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
        }

        public void Start()
        {
            if (_isRunning)
                return;
            if (!Directory.Exists(_journalPath))
            {
                LogMessage("Journal path not found: " + _journalPath);
                return;
            }

            _isRunning = true;
            _processingThread = new Thread(ProcessingLoop)
            {
                IsBackground = true,
                Name = "EJLive.AdvancedJournalProcessor"
            };
            _processingThread.Start();
            LogMessage("Advanced journal processor started.");
            PublishStatus("Started", 0, 0, 0, "Processor started");
        }

        public void Stop()
        {
            _isRunning = false;
            if (_processingThread is { IsAlive: true })
            {
                try
                {
                    _processingThread.Join(TimeSpan.FromSeconds(5));
                }
                catch
                {
                }
            }
            LogMessage("Advanced journal processor stopped.");
            PublishStatus("Stopped", 0, 0, 0, "Processor stopped");
        }

        public int GetProcessedFileCount()
        {
            lock (_sync)
                return _processedFiles.Count;
        }

        public void ClearProcessedFiles()
        {
            lock (_sync)
                _processedFiles.Clear();
        }

        private void ProcessingLoop()
        {
            while (_isRunning)
            {
                try
                {
                    var batch = FindNewJournalFiles();
                    if (batch.Count > 0)
                    {
                        var processed = 0;
                        var failed = 0;
                        PublishStatus("Syncing", batch.Count, processed, failed, "Processing new journal files");

                        foreach (var filePath in batch)
                        {
                            try
                            {
                                var entry = BuildEntry(filePath);
                                OnJournalEntryReady?.Invoke(entry);
                                lock (_sync)
                                    _processedFiles.Add(filePath);
                                processed++;
                                PublishStatus("Syncing", batch.Count, processed, failed, "Processed " + Path.GetFileName(filePath));
                            }
                            catch (Exception ex)
                            {
                                failed++;
                                LogMessage("Failed processing file " + filePath + ": " + ex.Message);
                                PublishStatus("Syncing", batch.Count, processed, failed, "Failed " + Path.GetFileName(filePath));
                            }
                        }

                        PublishStatus(
                            failed == 0 ? "Completed" : "CompletedWithErrors",
                            batch.Count,
                            processed,
                            failed,
                            failed == 0 ? "Batch completed successfully" : "Batch completed with errors");
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("Processor loop error: " + ex.Message);
                }

                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }

        private List<string> FindNewJournalFiles()
        {
            var list = new List<string>();
            foreach (var path in Directory.EnumerateFiles(_journalPath))
            {
                var fileName = Path.GetFileName(path);
                if (!IsValidJournalFile(fileName))
                    continue;
                lock (_sync)
                {
                    if (!_processedFiles.Contains(path))
                        list.Add(path);
                }
            }
            return list;
        }

        private bool IsValidJournalFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            return _atmType switch
            {
                ATMType.NCR => Constants.EJPatterns.NCRFiles.Any(value => fileName.Equals(value, StringComparison.OrdinalIgnoreCase)),
                ATMType.GRG => fileName.StartsWith(Constants.EJPatterns.GRGPrefix, StringComparison.OrdinalIgnoreCase),
                ATMType.WN => fileName.StartsWith(Constants.EJPatterns.WNPrefix, StringComparison.OrdinalIgnoreCase),
                ATMType.DieboldNixdorf => fileName.StartsWith(Constants.EJPatterns.DNPrefix, StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".jrn", StringComparison.OrdinalIgnoreCase),
                ATMType.Hyosung => fileName.StartsWith(Constants.EJPatterns.HYPrefix, StringComparison.OrdinalIgnoreCase),
                _ => Constants.EJPatterns.ValidExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            };
        }

        private AdvancedJournalEntry BuildEntry(string filePath)
        {
            var info = new FileInfo(filePath);
            var data = File.ReadAllBytes(filePath);
            var compressed = SecurityHelper.Compress(data);
            var encrypted = SecurityHelper.Encrypt(compressed);

            return new AdvancedJournalEntry
            {
                ATMId = _atmId,
                FileName = info.Name,
                OriginalSize = data.Length,
                CompressedSize = compressed.Length,
                EncryptedSize = encrypted.Length,
                IsEncrypted = true,
                IsCompressed = true,
                Checksum = SecurityHelper.SHA256Hash(data),
                MD5Hash = SecurityHelper.MD5Hash(data),
                CreatedAt = info.CreationTimeUtc,
                ReceivedAt = DateTime.UtcNow,
                Status = "Ready",
                TransactionCount = EstimateTransactions(data)
            };
        }

        private static int EstimateTransactions(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
                return 0;
            return Math.Max(1, payload.Length / 100);
        }

        private void PublishStatus(string status, int total, int synced, int failed, string message)
        {
            OnSyncStatusChanged?.Invoke(new AdvancedSyncStatusInfo
            {
                ATMId = _atmId,
                Status = status,
                TotalFiles = total,
                SyncedFiles = synced,
                FailedFiles = failed,
                ProgressPercentage = total > 0 ? (int)Math.Round((double)synced * 100 / total) : 0,
                LastUpdated = DateTime.UtcNow,
                FailureReason = failed > 0 ? message : null
            });
        }

        private void LogMessage(string message)
        {
            OnLogMessage?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
