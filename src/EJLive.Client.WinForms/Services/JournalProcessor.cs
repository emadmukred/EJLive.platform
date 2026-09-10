using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;
using AppConstants = EJLive.Core.AppConstants;

namespace EJLive.Client.WinForms.Services
{
    /// <summary>
    /// Unified journal processor:
    /// - Queue mode: sends JournalOutbox items over NetworkManager.
    /// - Watch mode: monitors journal folder and emits raw payload/status updates.
    /// </summary>
    public sealed class JournalProcessor : IDisposable
    {
        private readonly NetworkManager? _network;
        private readonly JournalOutbox? _outbox;
        private readonly RetryPolicy _retryPolicy;

        private readonly string _watchPath = string.Empty;
        private readonly ATMType _watchAtmType = ATMType.Other;
        private readonly string _watchAtmId = string.Empty;
        private readonly Dictionary<string, long> _fileOffsets = new(StringComparer.OrdinalIgnoreCase);

        private Thread? _processorThread;
        private FileSystemWatcher? _watcher;
        private volatile bool _running;
        private volatile bool _paused;
        private DateTime _lastAckSweepUtc = DateTime.MinValue;
        private readonly TimeSpan _ackTimeout = TimeSpan.FromSeconds(Math.Max(30, AppConstants.HeartbeatTimeoutSec));

        public event EventHandler<LiveSyncProgress>? OnSyncProgress;
        public event EventHandler<JournalOutboxItem>? OnItemDispatched;
        public event EventHandler<JournalOutboxItem>? OnItemCompleted;
        public event EventHandler<JournalOutboxItem>? OnItemFailed;
        public event Action<string>? OnLogMessage;
        public event Action<byte[]>? OnDataReady;
        public event Action<string, ATMStatus>? OnStatusChanged;

        public int TotalSent { get; private set; }
        public int TotalFailed { get; private set; }
        public bool IsRunning => _running;
        public bool IsPaused => _paused;

        public JournalProcessor(NetworkManager network, JournalOutbox outbox)
        {
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
            _retryPolicy = RetryPolicy.ForNetwork("LAN");
        }

        public JournalProcessor(string ejPath, ATMType atmType, string atmId)
        {
            _watchPath = string.IsNullOrWhiteSpace(ejPath) ? string.Empty : ejPath;
            _watchAtmType = atmType;
            _watchAtmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
            _retryPolicy = RetryPolicy.ForNetwork("LAN");
        }

        public void Start()
        {
            if (_running)
                return;

            _running = true;
            _paused = false;

            if (_outbox != null && _network != null)
            {
                _processorThread = new Thread(ProcessQueueLoop)
                {
                    IsBackground = true,
                    Name = "EJLive.JournalProcessor.Queue"
                };
                _processorThread.Start();
                Log("JournalProcessor queue mode started.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_watchPath) || !Directory.Exists(_watchPath))
            {
                Log("JournalProcessor watch path not found: " + _watchPath);
                _running = false;
                return;
            }

            ConfigureWatcher();
            ScanExistingFiles();
            Log("JournalProcessor watch mode started.");
        }

        public void Stop()
        {
            _running = false;
            _paused = false;

            try
            {
                _watcher?.Dispose();
                _watcher = null;
            }
            catch
            {
            }

            if (_processorThread is { IsAlive: true })
            {
                try
                {
                    _processorThread.Join(TimeSpan.FromSeconds(2));
                }
                catch
                {
                }
            }

            Log("JournalProcessor stopped.");
        }

        public void Pause()
        {
            _paused = true;
            Log("JournalProcessor paused.");
        }

        public void Resume()
        {
            _paused = false;
            Log("JournalProcessor resumed.");
        }

        public void ForceSendNow()
        {
            _paused = false;
        }

        public bool VerifyIntegrity(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return false;

            try
            {
                var sourceHash = ComputeFileMd5(filePath);
                var backupRoot = GetBackupRoot();
                if (!Directory.Exists(backupRoot))
                    return false;

                var fileName = Path.GetFileName(filePath);
                var backupCandidates = Directory.EnumerateFiles(backupRoot, fileName + ".*.bin", SearchOption.AllDirectories)
                    .OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                if (backupCandidates.Length == 0)
                    return false;

                var backupHash = ComputeFileMd5(backupCandidates[0]);
                return string.Equals(sourceHash, backupHash, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private void ProcessQueueLoop()
        {
            while (_running)
            {
                try
                {
                    SweepAckTimeouts();

                    if (_paused || _network == null || _outbox == null || !_network.IsConnected)
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    if (!_outbox.TryDequeue(out var item))
                    {
                        Thread.Sleep(200);
                        continue;
                    }

                    if (!item.IsReadyToSend)
                    {
                        _outbox.Enqueue(item);
                        Thread.Sleep(120);
                        continue;
                    }

                    SendOutboxItem(item);
                }
                catch (Exception ex)
                {
                    Log("Queue loop error: " + ex.Message);
                    Thread.Sleep(500);
                }
            }
        }

        private void SendOutboxItem(JournalOutboxItem item)
        {
            if (_network == null || _outbox == null)
                return;

            try
            {
                var checksum = string.IsNullOrWhiteSpace(item.Checksum)
                    ? SecurityHelper.SHA256Hash(item.Data)
                    : item.Checksum;
                ReportProgress(item.ATM_ID, item.FileName, 0, 0, item.Data.LongLength, SyncStatus.InProgress);

                var sent = _network.SendJournalFile(item.FileName, item.Data, item.Offset, checksum);
                if (sent)
                {
                    _outbox.MarkAwaitingAcknowledgement(item.ItemId, _ackTimeout);
                    TotalSent++;
                    OnItemDispatched?.Invoke(this, item);
                    ReportProgress(item.ATM_ID, item.FileName, item.Data.LongLength, item.Data.LongLength, item.Data.LongLength, SyncStatus.Syncing);
                }
                else
                {
                    var delay = _retryPolicy.ComputeDelay(item.RetryCount + 1);
                    _outbox.RetryItem(item.ItemId, delay);
                    TotalFailed++;
                    OnItemFailed?.Invoke(this, item);
                    ReportProgress(item.ATM_ID, item.FileName, 0, 0, item.Data.LongLength, SyncStatus.Failed);
                }
            }
            catch (Exception ex)
            {
                _outbox.RetryItem(item.ItemId, _retryPolicy.ComputeDelay(item.RetryCount + 1));
                TotalFailed++;
                OnItemFailed?.Invoke(this, item);
                Log("Send item error: " + ex.Message);
            }
        }

        public bool ApplyAcknowledgement(string ackText, out JournalOutboxItem? item, out bool success, out string detail)
        {
            item = null;
            success = false;
            detail = string.Empty;
            if (_outbox == null)
                return false;

            if (!_outbox.TryApplyAcknowledgement(ackText, _retryPolicy, out var resolved, out success, out detail))
                return false;

            item = resolved;
            if (success)
            {
                resolved.Status = SyncStatus.Completed;
                resolved.LastAckDetail = detail;
                OnItemCompleted?.Invoke(this, resolved);
                ReportProgress(resolved.ATM_ID, resolved.FileName, resolved.SizeBytes, resolved.SizeBytes, resolved.SizeBytes, SyncStatus.Completed);
            }
            else
            {
                resolved.Status = SyncStatus.Failed;
                resolved.LastAckDetail = detail;
                OnItemFailed?.Invoke(this, resolved);
                ReportProgress(resolved.ATM_ID, resolved.FileName, 0, 0, resolved.SizeBytes, SyncStatus.Failed);
            }

            return true;
        }

        private void SweepAckTimeouts()
        {
            if (_outbox == null)
                return;

            var now = DateTime.UtcNow;
            if ((now - _lastAckSweepUtc) < TimeSpan.FromSeconds(5))
                return;

            _lastAckSweepUtc = now;
            var requeued = _outbox.RequeueTimedOutAcknowledgements(_retryPolicy, maxItems: 20);
            if (requeued > 0)
                Log($"Acknowledgement timeout recovery requeued {requeued} item(s).");
        }

        private void ConfigureWatcher()
        {
            _watcher?.Dispose();
            _watcher = new FileSystemWatcher(_watchPath)
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                Filter = "*.*",
                EnableRaisingEvents = true
            };

            _watcher.Created += (_, e) => ProcessWatchFileSafely(e.FullPath, fullRead: true);
            _watcher.Changed += (_, e) => ProcessWatchFileSafely(e.FullPath, fullRead: false);
            _watcher.Renamed += (_, e) => ProcessWatchFileSafely(e.FullPath, fullRead: true);
        }

        private void ScanExistingFiles()
        {
            foreach (var path in Directory.EnumerateFiles(_watchPath))
            {
                ProcessWatchFileSafely(path, fullRead: true);
            }
        }

        private void ProcessWatchFileSafely(string filePath, bool fullRead)
        {
            if (!_running || _paused)
                return;
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;
            if (!IsSupportedWatchFile(filePath))
                return;

            try
            {
                Thread.Sleep(80);
                ProcessWatchFile(filePath, fullRead);
            }
            catch (Exception ex)
            {
                Log("Watch file error: " + ex.Message);
            }
        }

        private void ProcessWatchFile(string filePath, bool fullRead)
        {
            var previousOffset = 0L;
            if (!fullRead && _fileOffsets.TryGetValue(filePath, out var existingOffset))
                previousOffset = existingOffset;

            if (!TryReadStableJournalDelta(filePath, previousOffset, out var data, out var nextOffset))
                return;

            _fileOffsets[filePath] = nextOffset;

            if (data.Length == 0)
                return;

            OnDataReady?.Invoke(data);
            BackupChunk(filePath, data);
            AnalyzeStatus(data);
            Log($"Journal payload detected: {Path.GetFileName(filePath)} ({data.Length} bytes).");
        }

        /// <summary>
        /// Optimistic read with stability validation to avoid corrupt/partial chunks while file is being written.
        /// </summary>
        private static bool TryReadStableJournalDelta(
            string filePath,
            long previousOffset,
            out byte[] data,
            out long nextOffset)
        {
            return JournalReadService.TryReadDeltaNonBlocking(filePath, previousOffset, out data, out nextOffset, maxAttempts: 3);
        }

        private bool IsSupportedWatchFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            return _watchAtmType switch
            {
                ATMType.NCR => Constants.EJPatterns.NCRFiles.Any(value => fileName.Equals(value, StringComparison.OrdinalIgnoreCase)),
                ATMType.GRG => fileName.StartsWith(Constants.EJPatterns.GRGPrefix, StringComparison.OrdinalIgnoreCase),
                ATMType.WN => fileName.StartsWith(Constants.EJPatterns.WNPrefix, StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".ej", StringComparison.OrdinalIgnoreCase),
                ATMType.DieboldNixdorf => fileName.StartsWith(Constants.EJPatterns.DNPrefix, StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".jrn", StringComparison.OrdinalIgnoreCase),
                ATMType.Hyosung => fileName.StartsWith(Constants.EJPatterns.HYPrefix, StringComparison.OrdinalIgnoreCase),
                _ => Constants.EJPatterns.ValidExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            };
        }

        private void AnalyzeStatus(byte[] payload)
        {
            var text = Encoding.UTF8.GetString(payload).ToUpperInvariant();
            if (text.Contains("SUPERVISOR") || text.Contains("SUPV"))
            {
                OnStatusChanged?.Invoke(_watchAtmId, ATMStatus.Supervisor);
                return;
            }
            if (text.Contains("FAULT") || text.Contains("ERROR"))
            {
                OnStatusChanged?.Invoke(_watchAtmId, ATMStatus.CriticalFault);
                return;
            }
            if (text.Contains("OUT OF SERVICE") || text.Contains("OOS"))
            {
                OnStatusChanged?.Invoke(_watchAtmId, ATMStatus.OutOfService);
                return;
            }
            if (text.Contains("IN SERVICE") || text.Contains("READY"))
            {
                OnStatusChanged?.Invoke(_watchAtmId, ATMStatus.InService);
            }
        }

        private void BackupChunk(string sourceFilePath, byte[] payload)
        {
            try
            {
                var backupRoot = Path.Combine(GetBackupRoot(), DateTime.UtcNow.ToString("yyyy-MM"));
                Directory.CreateDirectory(backupRoot);
                var fileName = Path.GetFileName(sourceFilePath) + "." + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + ".bin";
                File.WriteAllBytes(Path.Combine(backupRoot, fileName), payload);
            }
            catch (Exception ex)
            {
                Log("Backup write failed: " + ex.Message);
            }
        }

        private string GetBackupRoot()
        {
            var baseRoot = AppConstants.DefaultClientOutboxPath;
            var atm = string.IsNullOrWhiteSpace(_watchAtmId) ? "UNKNOWN" : _watchAtmId;
            return Path.Combine(baseRoot, "JournalBackups", atm);
        }

        private void ReportProgress(string atmId, string fileName, long bytesSent, long currentBytes, long totalBytes, SyncStatus status)
        {
            OnSyncProgress?.Invoke(this, new LiveSyncProgress
            {
                ATM_ID = string.IsNullOrWhiteSpace(atmId) ? _watchAtmId : atmId,
                FileName = fileName ?? string.Empty,
                BytesSent = Math.Max(bytesSent, currentBytes),
                TotalBytes = Math.Max(0, totalBytes),
                Status = status,
                UpdatedAtUtc = DateTime.UtcNow
            });
        }

        private static string ComputeFileMd5(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(stream);
            return Convert.ToHexString(hash);
        }

        private void Log(string message)
        {
            AppLogger.Instance.Info(message, "JournalProcessor");
            OnLogMessage?.Invoke(message);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
