// ImageSyncEngine.cs
// Processed by Code Intelligence Scanner — Smart Merge & Restructure

using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using System.Threading;
using System;
using EJLive.Core.Enums;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Core.Utils;
using EJLive.Shared;

namespace EJLive.Core.Services
{
    public partial public public class ImageSyncEngine : IDisposable
    {
        private readonly string _sharedFolder;
        private readonly Dictionary<string, DateTime> _lastSyncTimes = new();
        private readonly object _lock = new();
        public ImageSyncEngine(string sharedFolder)
        {
            public bool IsRunning { get; private set; }
            public ImageSyncPayload Payload { get; set; }
            public string SourcePath { get; set; }
            public ATMType ATMType { get; set; }
            public void Start()
            {
                public void Stop()
                {
                    private void OnImageAdded(object sender, FileSystemEventArgs e)
                    {
                        private void ProcessNewImage(string imagePath)
                        {
                            private string GetTargetPath(ATMType type) =>
                            public async Task DistributeImageAsync(ImageSyncPayload payload, List<string> targetDeviceIds,
                            Func<ImageSyncPayload, string, Task> sendFunc, CancellationToken cancellationToken = default)
                            {
                                public async Task<byte[]> PackageImageAsync(string imagePath, CancellationToken cancellationToken = default)
                                {
                                    public async Task SaveReceivedImageAsync(byte[] compressedData, string targetPath,
                                    CancellationToken cancellationToken = default)
                                    {
                                        private bool IsImageFile(string extension) =>
                                        public void Dispose()
                                        {
                                        }

                                    public partial public public class ImageSyncEventArgs : EventArgs
                                    {
                                        public ImageSyncPayload Payload { get; set; }
                                        public string SourcePath { get; set; }
                                        public ATMType ATMType { get; set; }
                                    }

                            }
                        public partial public class ImageSyncEngine : IDisposable
                        {
                            private readonly string _sharedFolder;
                            private readonly Dictionary<string, DateTime> _lastSyncTimes = new();
                            private readonly object _lock = new();
                            public ImageSyncEngine(string sharedFolder)
                            {
                                public bool IsRunning { get; private set; }
                                public void Start()
                                {
                                    public void Stop()
                                    {
                                        private void OnImageAdded(object sender, FileSystemEventArgs e)
                                        {
                                            private void ProcessNewImage(string imagePath)
                                            {
                                                private string GetTargetPath(ATMType type) =>
                                                public async Task DistributeImageAsync(ImageSyncPayload payload, List<string> targetDeviceIds,
                                                Func<ImageSyncPayload, string, Task> sendFunc, CancellationToken cancellationToken = default)
                                                {
                                                    public async Task<byte[]> PackageImageAsync(string imagePath, CancellationToken cancellationToken = default)
                                                    {
                                                        public async Task SaveReceivedImageAsync(byte[] compressedData, string targetPath,
                                                        CancellationToken cancellationToken = default)
                                                        {
                                                            private bool IsImageFile(string extension) =>
                                                            public void Dispose()
                                                            {
                                                            }

                                                        public partial public class ImageSyncEventArgs : EventArgs
                                                        {
                                                            public ImageSyncPayload Payload { get; set; }
                                                            public string SourcePath { get; set; }
                                                            public ATMType ATMType { get; set; }
                                                        }

                                                }
                                            public partial class ImageSyncEngine : IDisposable
                                            {
                                                private readonly string _sharedFolder;


                                                private FileSystemWatcher? _watcher;


                                                private CancellationTokenSource? _cts;


                                                private Task? _syncTask;


                                                private readonly Dictionary<string, DateTime> _lastSyncTimes = new();


                                                private readonly object _lock = new();


                                                public bool IsRunning { get; private set; }


                                                public ImageSyncEngine(string sharedFolder)
                                                _sharedFolder = sharedFolder;


                                                public event EventHandler<ImageSyncEventArgs>? ImageReadyForDistribution;


                                                public event EventHandler<string>? SyncCompleted;


                                                public event EventHandler<string>? SyncError;


                                            }
                                        public class ImageSyncEngine : IDisposable
                                        {
                                            private readonly string _sharedFolder;
                                            private FileSystemWatcher? _watcher;
                                            private CancellationTokenSource? _cts;
                                            private Task? _syncTask;
                                            private readonly Dictionary<string, DateTime> _lastSyncTimes = new();
                                            private readonly object _lock = new();

                                            public bool IsRunning { get; private set; }
                                            public event EventHandler<ImageSyncEventArgs>? ImageReadyForDistribution;
                                            public event EventHandler<string>? SyncCompleted;
                                            public event EventHandler<string>? SyncError;

                                            public ImageSyncEngine(string sharedFolder)
                                            {
                                                _sharedFolder = sharedFolder;
                                            }

                                        public void Start()
                                        {
                                            if (IsRunning) return;

                                            _cts = new CancellationTokenSource();
                                            IsRunning = true;

                                            Directory.CreateDirectory(_sharedFolder);

                                            // Create subfolders for each ATM type
                                            foreach (ATMType type in Enum.GetValues<ATMType>())
                                            {
                                                var typeFolder = Path.Combine(_sharedFolder, type.ToString());
                                                Directory.CreateDirectory(typeFolder);
                                            }

                                        // Setup watcher
                                        _watcher = new FileSystemWatcher(_sharedFolder)
                                        {
                                            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                                            Filter = "*.*",
                                            IncludeSubdirectories = true,
                                            EnableRaisingEvents = true
                                        };

                                    _watcher.Created += OnImageAdded;
                                    _watcher.Changed += OnImageAdded;

                                    Logger.Info($"ImageSyncEngine started: {_sharedFolder}", "ImageSyncEngine");
                                }

                            public void Stop()
                            {
                                IsRunning = false;
                                _cts?.Cancel();
                                _watcher?.Dispose();

                                if (_syncTask != null)
                                try { _syncTask.Wait(TimeSpan.FromSeconds(10)); } catch { }

                                Logger.Info("ImageSyncEngine stopped", "ImageSyncEngine");
                            }

                        private void OnImageAdded(object sender, FileSystemEventArgs e)
                        {
                            var ext = Path.GetExtension(e.FullPath).ToLowerInvariant();
                            if (!IsImageFile(ext)) return;

                            Task.Run(() =>
                            {
                                try
                                {
                                    ProcessNewImage(e.FullPath);
                                }
                            catch (Exception ex)
                            {
                                SyncError?.Invoke(this, $"Error processing {e.Name}: {ex.Message}");
                                Logger.Error($"Image sync error: {ex.Message}", "ImageSyncEngine", ex);
                            }
                    });
            }

        private void ProcessNewImage(string imagePath)
        {
            var relativePath = Path.GetRelativePath(_sharedFolder, imagePath);
            var parts = relativePath.Split(Path.DirectorySeparatorChar);

            if (parts.Length < 2) return;

            var typeStr = parts[0];
            if (!Enum.TryParse<ATMType>(typeStr, out var atmType)) return;

            var payload = new ImageSyncPayload;
            {
                ImageName = Path.GetFileName(imagePath),
                TargetPath = GetTargetPath(atmType),
                TargetATMType = atmType,
                ScheduleTime = DateTime.Now
            };

        lock (_lock)
        {
            _lastSyncTimes[imagePath] = DateTime.UtcNow;
        }

    ImageReadyForDistribution?.Invoke(this, new ImageSyncEventArgs
    {
        Payload = payload,
        SourcePath = imagePath,
        ATMType = atmType
    });

SyncCompleted?.Invoke(this, $"Image ready: {payload.ImageName} for {atmType}");
}

private string GetTargetPath(ATMType type) => type switch
{
    ATMType.NCR => @"C:\Program Files\NCR APATRA\Advance NDC\Media",
    ATMType.GRG => @"D:\Program Files\DTATMW\Bin\AScreen\image",
    ATMType.WN => @"C:\ProTopas\Images",
    _ => @"C:\EJLive_Storage\Images"
};

public async Task DistributeImageAsync(ImageSyncPayload payload, List<string> targetDeviceIds,
Func<ImageSyncPayload, string, Task> sendFunc, CancellationToken cancellationToken = default)
{
    foreach (var deviceId in targetDeviceIds)
    {
        try
        {
            await sendFunc(payload, deviceId);
            Logger.Info($"Image distributed to {deviceId}: {payload.ImageName}", "ImageSyncEngine");
        }
    catch (Exception ex)
    {
        Logger.Error($"Failed to distribute to {deviceId}: {ex.Message}", "ImageSyncEngine", ex);
    }

await Task.Delay(100, cancellationToken);
}
}

public async Task<byte[]> PackageImageAsync(string imagePath, CancellationToken cancellationToken = default)
{
    var bytes = await File.ReadAllBytesAsync(imagePath, cancellationToken);
    return SecurityHelper.Compress(bytes);
}

public async Task SaveReceivedImageAsync(byte[] compressedData, string targetPath,
CancellationToken cancellationToken = default)
{
    var decompressed = SecurityHelper.Decompress(compressedData);
    Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
    await File.WriteAllBytesAsync(targetPath, decompressed, cancellationToken);
}

private bool IsImageFile(string extension) => extension switch
{
    ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" or ".webp" => true,
    _ => false
};

public void Dispose()
{
    Stop();
    _cts?.Dispose();
    _watcher?.Dispose();
}
}
public class ImageSyncEventArgs : EventArgs
{
    public ImageSyncPayload Payload { get; set; } = new();
    public string SourcePath { get; set; } = string.Empty;
    public ATMType ATMType { get; set; }
}

// Class: ImageSyncEngine (from 2 sources)
public partial class ImageSyncEngine : IDisposable
{
    // --- Constants & Fields ---
    private readonly string _sharedFolder;

    private FileSystemWatcher? _watcher;

    private CancellationTokenSource? _cts;

    private Task? _syncTask;

    private readonly Dictionary<string, DateTime> _lastSyncTimes = new();

    private readonly object _lock = new();


    // --- Properties ---
    public bool IsRunning { get; private set; }


    // --- Constructors ---
    public ImageSyncEngine(string sharedFolder)
    _sharedFolder = sharedFolder;


    // --- Events ---
    public event EventHandler<ImageSyncEventArgs>? ImageReadyForDistribution;

    public event EventHandler<string>? SyncCompleted;

    public event EventHandler<string>? SyncError;

}
// Class: ImageSyncEventArgs (from 2 sources)
public partial class ImageSyncEventArgs : EventArgs
{
}

public partial class ImageSyncEventArgs : EventArgs
{
}
}


{
    );

    public partial class EJMessage
    {
        public CommunicationProtocol.MsgType Type { get; set; }


        public string Text { get; set; } = string.Empty;


        public byte[] Payload { get; set; } = Array.Empty<byte>();


        public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;


    }
public partial public class ImageSyncEngine
{
    private readonly string _imagesPath;
    private readonly string _imageBasePath;
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
    public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
    {
        public ImageSyncEngine(string imageBasePath)
        {
            public int ImagesSynced
            {
                get;
                private set;
            }
        public long TotalBytesSent
        {
            get;
            private set;
        }
    public class ImageSyncEngine
    {
#region Events
        public event Action<string> OnLog;
        public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
        public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
        public event Action<Exception> OnError;
#endregion

#region Fields
        private readonly string _imageBasePath;
        private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
        private readonly List<ImageSyncItem> _syncQueue;
        private readonly object _lock = new object();
        private FileSystemWatcher _watcher;
        private bool _isRunning;
        private Thread _syncThread;
#endregion

#region Configuration
        public string NCRImagePath
        {
            get;
            set;
        }
    public string GRGImagePath
    {
        get;
        set;
    }
public string WNImagePath
{
    get;
    set;
}
public string SharedImagePath
{
    get;
    set;
}
public int SyncIntervalMs
{
    get;
    set;
}
public string NCRImagePath
{
    get;
    set;
}
public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
    {
        public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
        {
            public List<string> GetLocalImages()
            {
                private string SanitizeName(string name) =>
                private void Log(string msg) =>
                public void RegisterATM(string atmId, string atmType)
                {
                    public void UnregisterATM(string atmId)
                    {
                        public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
                        {
                            public void Start()
                            {
                                public void Stop()
                                {
                                    public List<ImageSyncItem> GetSyncQueue()
                                    {
                                        public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
                                        {
                                            private void SetupWatcher()
                                            {
                                                private void Watcher_Created(object sender, FileSystemEventArgs e)
                                                {
                                                    private void SyncLoop()
                                                    {
                                                        private void ProcessSyncItem(ImageSyncItem item)
                                                        {
                                                            private string ComputeMD5(string filePath)
                                                            {
                                                                private string ComputeMD5Bytes(byte[] data)
                                                                {
                                                                    public event EventHandler<string> OnImageSynced;
                                                                    public event EventHandler<string> OnImageError;
                                                                    public event EventHandler<string> OnLog;
                                                                    public event Action<string> OnLog;
                                                                    public event Action<ImageSyncItem, string> OnImageSent;
                                                                    public event Action<ImageSyncItem, string, string> OnImageFailed;
                                                                    public event Action<Exception> OnError;
                                                                }

                                                        }
                                                    public partial public class ImageSyncEngine
                                                    {
                                                        private readonly string _imagesPath;
                                                        private readonly string _imageBasePath;
                                                        private readonly List<ImageSyncItem> _syncQueue;
                                                        private readonly object _lock = new object();
                                                        private FileSystemWatcher _watcher;
                                                        private bool _isRunning;
                                                        private Thread _syncThread;
                                                        private readonly List<string> _pendingImages = new();
                                                        public bool IsRunning => _isRunning;
                                                        public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
                                                        {
                                                            public ImageSyncEngine(string imageBasePath)
                                                            {
                                                                public ImageSyncEngine()
                                                                {
                                                                    public int    ImagesSynced    { get; private set; }
                                                                    public long   TotalBytesSent  { get; private set; }
                                                                    public class ImageSyncEngine
                                                                    {
#region Events
                                                                        public event Action<string> OnLog;
                                                                        public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
                                                                        public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
                                                                        public event Action<Exception> OnError;
#endregion

#region Fields
                                                                        private readonly string _imageBasePath;
                                                                        private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
                                                                        private readonly List<ImageSyncItem> _syncQueue;
                                                                        private readonly object _lock = new object();
                                                                        private FileSystemWatcher _watcher;
                                                                        private bool _isRunning;
                                                                        private Thread _syncThread;
#endregion

#region Configuration
                                                                        public string NCRImagePath { get; set; }
                                                                        public string GRGImagePath { get; set; }
                                                                        public string WNImagePath { get; set; }
                                                                        public string SharedImagePath { get; set; }
                                                                        public int SyncIntervalMs { get; set; }
                                                                        public string NCRImagePath { get; set; }
                                                                        public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
                                                                        {
                                                                            public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
                                                                            {
                                                                                public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
                                                                                {
                                                                                    public List<string> GetLocalImages()
                                                                                    {
                                                                                        private string SanitizeName(string name) =>
                                                                                        private void Log(string msg) =>
                                                                                        public void RegisterATM(string atmId, string atmType)
                                                                                        {
                                                                                            public void UnregisterATM(string atmId)
                                                                                            {
                                                                                                public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
                                                                                                {
                                                                                                    public void Start()
                                                                                                    {
                                                                                                        public void Stop()
                                                                                                        {
                                                                                                            public List<ImageSyncItem> GetSyncQueue()
                                                                                                            {
                                                                                                                public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
                                                                                                                {
                                                                                                                    private void SetupWatcher()
                                                                                                                    {
                                                                                                                        private void Watcher_Created(object sender, FileSystemEventArgs e)
                                                                                                                        {
                                                                                                                            private void SyncLoop()
                                                                                                                            {
                                                                                                                                private void ProcessSyncItem(ImageSyncItem item)
                                                                                                                                {
                                                                                                                                    private string ComputeMD5(string filePath)
                                                                                                                                    {
                                                                                                                                        private string ComputeMD5Bytes(byte[] data)
                                                                                                                                        {
                                                                                                                                            public void QueueImage(string filePath)
                                                                                                                                            {
                                                                                                                                                public int SyncAll(string targetAtmType)
                                                                                                                                                {
                                                                                                                                                    private void EnsureDirectory() {
                                                                                                                                                        public void Dispose() {
                                                                                                                                                            public event EventHandler<string> OnImageSynced;
                                                                                                                                                            public event EventHandler<string> OnImageError;
                                                                                                                                                            public event EventHandler<string> OnLog;
                                                                                                                                                            public event Action<string> OnLog;
                                                                                                                                                            public event Action<ImageSyncItem, string> OnImageSent;
                                                                                                                                                            public event Action<ImageSyncItem, string, string> OnImageFailed;
                                                                                                                                                            public event Action<Exception> OnError;
                                                                                                                                                        }

                                                                                                                                                }
                                                                                                                                            public partial class ImageSyncEngine : IDisposable
                                                                                                                                            {
                                                                                                                                                private readonly string _imagesPath;


                                                                                                                                                private readonly byte[] _sessionKey;


                                                                                                                                                private readonly string _imageBasePath;


                                                                                                                                                private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs


                                                                                                                                                private readonly List<ImageSyncItem> _syncQueue;


                                                                                                                                                private FileSystemWatcher _watcher;


                                                                                                                                                private bool _isRunning;


                                                                                                                                                private Thread _syncThread;


                                                                                                                                                public int ImagesSynced
                                                                                                                                                {
                                                                                                                                                    get;
                                                                                                                                                    private set;
                                                                                                                                                }


                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                                                                                                            public long TotalBytesSent
                                                                                                                                            {
                                                                                                                                                get;
                                                                                                                                                private set;
                                                                                                                                            }


                                                                                                                                        private readonly List<string> _pendingImages = new();


                                                                                                                                        private readonly object _lock = new();


                                                                                                                                        private readonly string _sharedFolder;


                                                                                                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\ImageSyncEngine.cs
                                                                                                                                        private FileSystemWatcher? _watcher;


                                                                                                                                        private CancellationTokenSource? _cts;


                                                                                                                                        private Task? _syncTask;


                                                                                                                                        private readonly Dictionary<string, DateTime> _lastSyncTimes = new();


                                                                                                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                                                                                                        private FileSystemWatcher? _watcher;


                                                                                                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                                                                                                        public long TotalBytesSent
                                                                                                                                        {
                                                                                                                                            get;
                                                                                                                                            private set;
                                                                                                                                        }


                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                                                                                                    public long TotalBytesSent
                                                                                                                                    {
                                                                                                                                        get;
                                                                                                                                        private set;
                                                                                                                                    }


                                                                                                                                // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\ImageSyncEngine.cs
                                                                                                                                private FileSystemWatcher? _watcher;


                                                                                                                                // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                                                                                                public long TotalBytesSent
                                                                                                                                {
                                                                                                                                    get;
                                                                                                                                    private set;
                                                                                                                                }


                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\ImageSyncEngine.cs.before_unify
                                                                                                                            private FileSystemWatcher? _watcher;


                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\ImageSyncEngine.cs.v17_bak
                                                                                                                            private FileSystemWatcher? _watcher;


                                                                                                                            public int    ImagesSynced    { get; private set; }


                                                                                                                            public long   TotalBytesSent  { get; private set; }


                                                                                                                            public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";


                                                                                                                            public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";


                                                                                                                            public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";


                                                                                                                            public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";


                                                                                                                            public int SyncIntervalMs { get; set; } = 10000;


                                                                                                                            public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
                                                                                                                            {
                                                                                                                                if (!File.Exists(filePath))
                                                                                                                                {
                                                                                                                                    OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
                                                                                                                                    return;
                                                                                                                                }

                                                                                                                            var item = new ImageSyncItem;
                                                                                                                            {
                                                                                                                                ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
                                                                                                                                FileName = Path.GetFileName(filePath),
                                                                                                                                FilePath = filePath,
                                                                                                                                FileSize = new FileInfo(filePath).Length,
                                                                                                                                Checksum = ComputeMD5(filePath),
                                                                                                                                TargetATMType = targetType,
                                                                                                                                TargetATMs = specificATMs ?? new List<string>(),
                                                                                                                                ScheduledTime = DateTime.Now,
                                                                                                                                Status = ImageSyncStatus.Pending
                                                                                                                            };

                                                                                                                        lock (_lock) { _syncQueue.Add(item); }
                                                                                                                        OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
                                                                                                                    }


                                                                                                                public bool IsRunning => _isRunning;


                                                                                                                // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                                                                                public int SyncAll(string targetAtmType)
                                                                                                                {
                                                                                                                    var count = 0;
                                                                                                                    foreach (var img in _pendingImages.ToArray())
                                                                                                                    {
                                                                                                                        try { count++; OnImageSynced?.Invoke(this, img); }
                                                                                                                        catch (Exception ex) { OnImageError?.Invoke(this, ex.Message); }
                                                                                                                    }
                                                                                                                _pendingImages.Clear();
                                                                                                                return count;
                                                                                                            }


                                                                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\ImageSyncEngine.cs
                                                                                                        public bool IsRunning { get; private set; }


                                                                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                                                                        public bool IsRunning { get; private set; }


                                                                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                                                                        public int SyncAll(string targetAtmType)
                                                                                                        {
                                                                                                            var count = 0;
                                                                                                            foreach (var img in _pendingImages.ToArray())
                                                                                                            {
                                                                                                                try { count++; OnImageSynced?.Invoke(this, img); }
                                                                                                                catch (Exception ex) { OnImageError?.Invoke(this, ex.Message); }
                                                                                                            }
                                                                                                        _pendingImages.Clear();
                                                                                                        return count;
                                                                                                    }


                                                                                                // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                                                                public int SyncAll(string targetAtmType)
                                                                                                {
                                                                                                    var count = 0;
                                                                                                    foreach (var img in _pendingImages.ToArray())
                                                                                                    {
                                                                                                        try { count++; OnImageSynced?.Invoke(this, img); }
                                                                                                        catch (Exception ex) { OnImageError?.Invoke(this, ex.Message); }
                                                                                                    }
                                                                                                _pendingImages.Clear();
                                                                                                return count;
                                                                                            }


                                                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\ImageSyncEngine.cs
                                                                                        public bool IsRunning { get; private set; }


                                                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                                                        public int SyncAll(string targetAtmType)
                                                                                        {
                                                                                            var count = 0;
                                                                                            foreach (var img in _pendingImages.ToArray())
                                                                                            {
                                                                                                try { count++; OnImageSynced?.Invoke(this, img); }
                                                                                                catch (Exception ex) { OnImageError?.Invoke(this, ex.Message); }
                                                                                            }
                                                                                        _pendingImages.Clear();
                                                                                        return count;
                                                                                    }


                                                                                // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\ImageSyncEngine.cs.before_unify
                                                                                public bool IsRunning { get; private set; }


                                                                                // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\ImageSyncEngine.cs.v17_bak
                                                                                public bool IsRunning { get; private set; }


                                                                                public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
                                                                                {
                                                                                    _imagesPath = imagesPath ?? Path.Combine(
                                                                                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                                                                    "EJLive", "Images");
                                                                                    _sessionKey = sessionKey;
                                                                                    if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
                                                                                }


                                                                            public ImageSyncEngine(string imageBasePath)
                                                                            {
                                                                                _imageBasePath = imageBasePath;
                                                                                _atmsByType = new Dictionary<string, List<string>>
                                                                                {
                                                                                    { "NCR", new List<string>() },
                                                                                { "GRG", new List<string>() },
                                                                            { "WN", new List<string>() }
                                                                    };
                                                                _syncQueue = new List<ImageSyncItem>();
                                                            }


                                                        public ImageSyncEngine()
                                                        {
                                                            _imagesPath = AppConstants.DefaultImagesPath;
                                                            EnsureDirectory();
                                                        }


                                                    public ImageSyncEngine(string imagesPath)
                                                    {
                                                        _imagesPath = imagesPath ?? AppConstants.DefaultImagesPath;
                                                        EnsureDirectory();
                                                    }


                                                public ImageSyncEngine(string sharedFolder)
                                                _sharedFolder = sharedFolder;


                                                public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
                                                {
                                                    if (!File.Exists(imagePath))
                                                    {
                                                        Log($"Image not found: {imagePath}");
                                                        return false;
                                                    }

                                                try
                                                {
                                                    var data     = SecurityHelper.ReadFileSafe(imagePath);
                                                    var checksum = SecurityHelper.MD5Hash(data);
                                                    var fileName = Path.GetFileName(imagePath);

                                                    // START_FILE
                                                    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
                                                    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

                                                    // إرسال على أجزاء
                                                    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
                                                    for (int i = 0; i < totalChunks; i++)
                                                    {
                                                        var off   = i * (int)AppConstants.ChunkSizeBytes;
                                                        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
                                                        var chunk = new byte[len];
                                                        Buffer.BlockCopy(data, off, chunk, 0, len);
                                                        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
                                                        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
                                                        TotalBytesSent += len;
                                                    }

                                                // COMPLETE
                                                var sha256  = SecurityHelper.SHA256Hash(data);
                                                var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
                                                lock (sendLock) stream.Write(complete, 0, complete.Length);

                                                ImagesSynced++;
                                                OnImageSynced?.Invoke(this, imagePath);
                                                Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
                                                return true;
                                            }
                                        catch (Exception ex)
                                        {
                                            OnImageError?.Invoke(this, ex.Message);
                                            Log($"✗ Image send failed: {ex.Message}");
                                            return false;
                                        }
                                }


                            public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
                            {
                                try
                                {
                                    var buffer  = new MemoryStream();
                                    var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

                                    for (int i = 0; i < chunkCount; i++)
                                    {
                                        var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
                                        if (!CommunicationProtocol.IsChunk(msg)) return false;
                                        var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
                                        buffer.Write(data, 0, data.Length);

                                        // CHUNK_ACK
                                        var ack = CommunicationProtocol.BuildChunkAck(seqNum);
                                        stream.Write(ack, 0, ack.Length);
                                    }

                                // قراءة COMPLETE
                                var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
                                var parts    = complete.Text.Split('|');
                                var checksum = parts.Length > 2 ? parts[2] : "";

                                var imageData   = buffer.ToArray();
                                var actualMd5   = SecurityHelper.MD5Hash(imageData);
                                var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

                                if (verified)
                                {
                                    var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
                                    File.WriteAllBytes(savePath, imageData);
                                    ImagesSynced++;
                                    OnImageSynced?.Invoke(this, savePath);
                                    Log($"✓ Image received & verified: {fileName}");
                                }
                            else
                            {
                                Log($"✗ Checksum mismatch for image: {fileName}");
                            }

                        // إرسال ACK
                        var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
                        stream.Write(imgAck, 0, imgAck.Length);
                        return verified;
                    }
                catch (Exception ex)
                {
                    OnImageError?.Invoke(this, ex.Message);
                    return false;
                }
        }


    public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
    {
        if (!Directory.Exists(serverImagesFolder)) return 0;
        int count = 0;
        foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
    foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
    if (SendImageToClient(stream, file, atmId, sendLock)) count++;
    Log($"✓ Image sync complete: {count} images sent to {atmId}");
    return count;
}


public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}


private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");


private void Log(string msg) => OnLog?.Invoke(this, msg);


private readonly object _lock = new object();


public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}


public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}


public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}


public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}


public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}


public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}


private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}


private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}


private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}


private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}


private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
public void Start() { _isRunning = true; Log("ImageSync started."); }


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
public void Stop() { _isRunning = false; Log("ImageSync stopped."); }


public void QueueImage(string filePath)
{
    if (File.Exists(filePath))
    {
        lock (_lock) { _pendingImages.Add(filePath); }
        Log($"Queued image: {Path.GetFileName(filePath)}");
    }
}


private void EnsureDirectory() { if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath); }


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
private void Log(string msg) => OnLog?.Invoke(this, $"[ImageSync] {msg}");


public void Dispose() { Stop(); }


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
public void Start() { _isRunning = true; Log("ImageSync started."); }


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
public void Stop() { _isRunning = false; Log("ImageSync stopped."); }


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
private void Log(string msg) => OnLog?.Invoke(this, $"[ImageSync] {msg}");


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
public void Start() { _isRunning = true; Log("ImageSync started."); }


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
public void Stop() { _isRunning = false; Log("ImageSync stopped."); }


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
private void Log(string msg) => OnLog?.Invoke(this, $"[ImageSync] {msg}");


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs
public void Start() { _isRunning = true; Log("ImageSync started."); }


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs
public void Stop() { _isRunning = false; Log("ImageSync stopped."); }


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs
private void Log(string msg) => OnLog?.Invoke(this, $"[ImageSync] {msg}");


public event EventHandler<string> OnImageSynced;


public event EventHandler<string> OnImageError;


public event EventHandler<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


public event Action<ImageSyncItem, string> OnImageSent; // item, atmId


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_replik\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_replik\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event EventHandler<string>? OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event EventHandler<string>? OnImageSynced;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event EventHandler<string>? OnImageError;


public event EventHandler<ImageSyncEventArgs>? ImageReadyForDistribution;


public event EventHandler<string>? SyncCompleted;


public event EventHandler<string>? SyncError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
public event EventHandler<string>? OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
public event EventHandler<string>? OnImageSynced;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
public event EventHandler<string>? OnImageError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_replik\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_replik\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event EventHandler<string>? OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event EventHandler<string>? OnImageSynced;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event EventHandler<string>? OnImageError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-18\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-18\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-19\EJLive_replik\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-19\EJLive_replik\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-21\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-21\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-2\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-2\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event EventHandler<string>? OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event EventHandler<string>? OnImageSynced;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event EventHandler<string>? OnImageError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v15_bak
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v15_bak
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v16_bak
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v16_bak
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v17_bak
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v17_bak
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v20_bak
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v20_bak
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v22_bak
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v22_bak
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath
    {
        get;
        set;
    } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath
    {
        get;
        set;
    } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath
    {
        get;
        set;
    } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath
    {
        get;
        set;
    } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs
    {
        get;
        set;
    } = 10000;
#endregion

#region Constructor
public ImageSyncEngine(string imageBasePath)
{
    _imageBasePath = imageBasePath;
    _atmsByType = new Dictionary<string, List<string>> {
        {
            "NCR",
            new List < string > ()
        },
    {
        "GRG",
        new List < string > ()
    },
{
    "WN",
    new List < string > ()
}
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock)
{
    _syncQueue.Add(item);
}
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop)
    {
        IsBackground = true,
        Name = "ImageSyncThread"
    };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null)
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _watcher = null;
    }
OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock)
    {
        return _syncQueue.ToList();
    }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock)
            {
                pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList();
            }

        foreach (var item in pending)
        {
            if (!_isRunning) break;
            ProcessSyncItem(item);
        }
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
    public Func<string, byte[], string, bool>? DeliveryBridge;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

byte[] payload;
try
{
    payload = File.ReadAllBytes(item.FilePath);
    if (payload.Length == 0)
    {
        item.Status = ImageSyncStatus.Failed;
        OnLog?.Invoke($"[ImageSync] Empty payload for: {item.FileName}");
        return;
    }
}
catch (Exception ex)
{
    item.Status = ImageSyncStatus.Failed;
    OnError?.Invoke(ex);
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    try
    {
        var delivered = DeliveryBridge?.Invoke(atmId, payload, item.FileName) ?? true;
        item.DeliveryStatus[atmId] = delivered;

        if (delivered)
        {
            success++;
            OnImageSent?.Invoke(item, atmId);
        }
    else
    {
        OnImageFailed?.Invoke(item, atmId, "Delivery bridge returned false.");
    }
}
catch (Exception ex)
{
    item.DeliveryStatus[atmId] = false;
    OnImageFailed?.Invoke(item, atmId, ex.Message);
}
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
    public Func<string, byte[], string, bool>? DeliveryBridge;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

byte[] payload;
try
{
    payload = File.ReadAllBytes(item.FilePath);
    if (payload.Length == 0)
    {
        item.Status = ImageSyncStatus.Failed;
        OnLog?.Invoke($"[ImageSync] Empty payload for: {item.FileName}");
        return;
    }
}
catch (Exception ex)
{
    item.Status = ImageSyncStatus.Failed;
    OnError?.Invoke(ex);
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    try
    {
        var delivered = DeliveryBridge?.Invoke(atmId, payload, item.FileName) ?? true;
        item.DeliveryStatus[atmId] = delivered;

        if (delivered)
        {
            success++;
            OnImageSent?.Invoke(item, atmId);
        }
    else
    {
        OnImageFailed?.Invoke(item, atmId, "Delivery bridge returned false.");
    }
}
catch (Exception ex)
{
    item.DeliveryStatus[atmId] = false;
    OnImageFailed?.Invoke(item, atmId, ex.Message);
}
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
    public Func<string, byte[], string, bool>? DeliveryBridge;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

byte[] payload;
try
{
    payload = File.ReadAllBytes(item.FilePath);
    if (payload.Length == 0)
    {
        item.Status = ImageSyncStatus.Failed;
        OnLog?.Invoke($"[ImageSync] Empty payload for: {item.FileName}");
        return;
    }
}
catch (Exception ex)
{
    item.Status = ImageSyncStatus.Failed;
    OnError?.Invoke(ex);
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    try
    {
        var delivered = DeliveryBridge?.Invoke(atmId, payload, item.FileName) ?? true;
        item.DeliveryStatus[atmId] = delivered;

        if (delivered)
        {
            success++;
            OnImageSent?.Invoke(item, atmId);
        }
    else
    {
        OnImageFailed?.Invoke(item, atmId, "Delivery bridge returned false.");
    }
}
catch (Exception ex)
{
    item.DeliveryStatus[atmId] = false;
    OnImageFailed?.Invoke(item, atmId, ex.Message);
}
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v23_bak
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
    public Func<string, byte[], string, bool>? DeliveryBridge;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

byte[] payload;
try
{
    payload = File.ReadAllBytes(item.FilePath);
    if (payload.Length == 0)
    {
        item.Status = ImageSyncStatus.Failed;
        OnLog?.Invoke($"[ImageSync] Empty payload for: {item.FileName}");
        return;
    }
}
catch (Exception ex)
{
    item.Status = ImageSyncStatus.Failed;
    OnError?.Invoke(ex);
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    try
    {
        var delivered = DeliveryBridge?.Invoke(atmId, payload, item.FileName) ?? true;
        item.DeliveryStatus[atmId] = delivered;

        if (delivered)
        {
            success++;
            OnImageSent?.Invoke(item, atmId);
        }
    else
    {
        OnImageFailed?.Invoke(item, atmId, "Delivery bridge returned false.");
    }
}
catch (Exception ex)
{
    item.DeliveryStatus[atmId] = false;
    OnImageFailed?.Invoke(item, atmId, ex.Message);
}
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
    public Func<string, byte[], string, bool>? DeliveryBridge;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

byte[] payload;
try
{
    payload = File.ReadAllBytes(item.FilePath);
    if (payload.Length == 0)
    {
        item.Status = ImageSyncStatus.Failed;
        OnLog?.Invoke($"[ImageSync] Empty payload for: {item.FileName}");
        return;
    }
}
catch (Exception ex)
{
    item.Status = ImageSyncStatus.Failed;
    OnError?.Invoke(ex);
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    try
    {
        var delivered = DeliveryBridge?.Invoke(atmId, payload, item.FileName) ?? true;
        item.DeliveryStatus[atmId] = delivered;

        if (delivered)
        {
            success++;
            OnImageSent?.Invoke(item, atmId);
        }
    else
    {
        OnImageFailed?.Invoke(item, atmId, "Delivery bridge returned false.");
    }
}
catch (Exception ex)
{
    item.DeliveryStatus[atmId] = false;
    OnImageFailed?.Invoke(item, atmId, ex.Message);
}
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
    public Func<string, byte[], string, bool>? DeliveryBridge;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

byte[] payload;
try
{
    payload = File.ReadAllBytes(item.FilePath);
    if (payload.Length == 0)
    {
        item.Status = ImageSyncStatus.Failed;
        OnLog?.Invoke($"[ImageSync] Empty payload for: {item.FileName}");
        return;
    }
}
catch (Exception ex)
{
    item.Status = ImageSyncStatus.Failed;
    OnError?.Invoke(ex);
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    try
    {
        var delivered = DeliveryBridge?.Invoke(atmId, payload, item.FileName) ?? true;
        item.DeliveryStatus[atmId] = delivered;

        if (delivered)
        {
            success++;
            OnImageSent?.Invoke(item, atmId);
        }
    else
    {
        OnImageFailed?.Invoke(item, atmId, "Delivery bridge returned false.");
    }
}
catch (Exception ex)
{
    item.DeliveryStatus[atmId] = false;
    OnImageFailed?.Invoke(item, atmId, ex.Message);
}
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}

}
public partial class ImageSyncEngine : IDisposable
{
    private readonly string _imageBasePath;


    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs


    private readonly List<ImageSyncItem> _syncQueue;


    private FileSystemWatcher _watcher;


    private bool _isRunning;


    private Thread _syncThread;


    private readonly string _imagesPath;


    private readonly List<string> _pendingImages = new();


    private readonly object _lock = new();


    private readonly byte[] _sessionKey;


    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";


    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";


    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";


    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";


    public int SyncIntervalMs { get; set; } = 10000;


    public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
    {
        if (!File.Exists(filePath))
        {
            OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
            return;
        }

    var item = new ImageSyncItem;
    {
        ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
        FileName = Path.GetFileName(filePath),
        FilePath = filePath,
        FileSize = new FileInfo(filePath).Length,
        Checksum = ComputeMD5(filePath),
        TargetATMType = targetType,
        TargetATMs = specificATMs ?? new List<string>(),
        ScheduledTime = DateTime.Now,
        Status = ImageSyncStatus.Pending
    };

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}


public bool IsRunning => _isRunning;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
public int SyncAll(string targetAtmType)
{
    var count = 0;
    foreach (var img in _pendingImages.ToArray())
    {
        try { count++; OnImageSynced?.Invoke(this, img); }
        catch (Exception ex) { OnImageError?.Invoke(this, ex.Message); }
    }
_pendingImages.Clear();
return count;
}


public int    ImagesSynced    { get; private set; }


public long   TotalBytesSent  { get; private set; }


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs
public int SyncAll(string targetAtmType)
{
    var count = 0;
    foreach (var img in _pendingImages.ToArray())
    {
        try { count++; OnImageSynced?.Invoke(this, img); }
        catch (Exception ex) { OnImageError?.Invoke(this, ex.Message); }
    }
_pendingImages.Clear();
return count;
}


public ImageSyncEngine(string imageBasePath)
{
    _imageBasePath = imageBasePath;
    _atmsByType = new Dictionary<string, List<string>>
    {
        { "NCR", new List<string>() },
    { "GRG", new List<string>() },
{ "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}


public ImageSyncEngine()
{
    _imagesPath = AppConstants.DefaultImagesPath;
    EnsureDirectory();
}


public ImageSyncEngine(string imagesPath)
{
    _imagesPath = imagesPath ?? AppConstants.DefaultImagesPath;
    EnsureDirectory();
}


public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
{
    _imagesPath = imagesPath ?? Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "EJLive", "Images");
    _sessionKey = sessionKey;
    if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
}


private readonly object _lock = new object();


public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}


public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}


public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}


public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}


public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}


public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}


private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}


private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}


private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}


private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}


private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
public void Start() { _isRunning = true; Log("ImageSync started."); }


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
public void Stop() { _isRunning = false; Log("ImageSync stopped."); }


public void QueueImage(string filePath)
{
    if (File.Exists(filePath))
    {
        lock (_lock) { _pendingImages.Add(filePath); }
        Log($"Queued image: {Path.GetFileName(filePath)}");
    }
}


private void EnsureDirectory() { if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath); }


private void Log(string msg) => OnLog?.Invoke(this, $"[ImageSync] {msg}");


public void Dispose() { Stop(); }


public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data     = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off   = i * (int)AppConstants.ChunkSizeBytes;
        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256  = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}


public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer  = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts    = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData   = buffer.ToArray();
    var actualMd5   = SecurityHelper.MD5Hash(imageData);
    var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}


public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}


public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}


private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
private void Log(string msg) => OnLog?.Invoke(this, msg);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs
public void Start() { _isRunning = true; Log("ImageSync started."); }


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs
public void Stop() { _isRunning = false; Log("ImageSync stopped."); }


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v23_bak
private void Log(string msg) => OnLog?.Invoke(this, msg);


public event Action<string> OnLog;


public event Action<ImageSyncItem, string> OnImageSent; // item, atmId


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


public event Action<Exception> OnError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event EventHandler<string> OnLog;


public event EventHandler<string>? OnImageSynced;


public event EventHandler<string>? OnImageError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event EventHandler<string> OnImageSynced;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event EventHandler<string> OnImageError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v20_bak
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v17_bak
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v16_bak
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v15_bak
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event EventHandler<string>? OnLog;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v23_bak
public event EventHandler<string> OnImageSynced;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v23_bak
public event EventHandler<string> OnImageError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v23_bak
public event EventHandler<string> OnLog;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v22_bak
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}

}
public partial class ImageSyncEngine
{
    private readonly string _imageBasePath;


    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs


    private readonly List<ImageSyncItem> _syncQueue;


    private FileSystemWatcher _watcher;


    private bool _isRunning;


    private Thread _syncThread;


    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";


    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";


    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";


    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";


    public int SyncIntervalMs { get; set; } = 10000;


    public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
    {
        if (!File.Exists(filePath))
        {
            OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
            return;
        }

    var item = new ImageSyncItem;
    {
        ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
        FileName = Path.GetFileName(filePath),
        FilePath = filePath,
        FileSize = new FileInfo(filePath).Length,
        Checksum = ComputeMD5(filePath),
        TargetATMType = targetType,
        TargetATMs = specificATMs ?? new List<string>(),
        ScheduledTime = DateTime.Now,
        Status = ImageSyncStatus.Pending
    };

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}


public ImageSyncEngine(string imageBasePath)
{
    _imageBasePath = imageBasePath;
    _atmsByType = new Dictionary<string, List<string>>
    {
        { "NCR", new List<string>() },
    { "GRG", new List<string>() },
{ "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}


private readonly object _lock = new object();


public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}


public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}


public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}


public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}


public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}


public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}


private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}


private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}


private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}


private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}


private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


public event Action<string> OnLog;


public event Action<ImageSyncItem, string> OnImageSent; // item, atmId


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_replik\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


public event Action<Exception> OnError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_replik\EJLive_replik\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

}
public partial class ImageSyncEngine
{
    private readonly string _imageBasePath;


    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs


    private readonly List<ImageSyncItem> _syncQueue;


    private FileSystemWatcher _watcher;


    private bool _isRunning;


    private Thread _syncThread;


    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";


    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";


    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";


    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";


    public int SyncIntervalMs { get; set; } = 10000;


    public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
    {
        if (!File.Exists(filePath))
        {
            OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
            return;
        }

    var item = new ImageSyncItem;
    {
        ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
        FileName = Path.GetFileName(filePath),
        FilePath = filePath,
        FileSize = new FileInfo(filePath).Length,
        Checksum = ComputeMD5(filePath),
        TargetATMType = targetType,
        TargetATMs = specificATMs ?? new List<string>(),
        ScheduledTime = DateTime.Now,
        Status = ImageSyncStatus.Pending
    };

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}


public ImageSyncEngine(string imageBasePath)
{
    _imageBasePath = imageBasePath;
    _atmsByType = new Dictionary<string, List<string>>
    {
        { "NCR", new List<string>() },
    { "GRG", new List<string>() },
{ "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}


private readonly object _lock = new object();


public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}


public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}


public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}


public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}


public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}


public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}


private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}


private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}


private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}


private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}


private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


public event Action<string> OnLog;


public event Action<ImageSyncItem, string> OnImageSent; // item, atmId


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


public event Action<Exception> OnError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Menus_ai\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

}
public partial class ImageSyncEngine
{
    private readonly string _imageBasePath;


    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs


    private readonly List<ImageSyncItem> _syncQueue;


    private FileSystemWatcher _watcher;


    private bool _isRunning;


    private Thread _syncThread;


    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";


    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";


    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";


    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";


    public int SyncIntervalMs { get; set; } = 10000;


    public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
    {
        if (!File.Exists(filePath))
        {
            OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
            return;
        }

    var item = new ImageSyncItem;
    {
        ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
        FileName = Path.GetFileName(filePath),
        FilePath = filePath,
        FileSize = new FileInfo(filePath).Length,
        Checksum = ComputeMD5(filePath),
        TargetATMType = targetType,
        TargetATMs = specificATMs ?? new List<string>(),
        ScheduledTime = DateTime.Now,
        Status = ImageSyncStatus.Pending
    };

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}


public ImageSyncEngine(string imageBasePath)
{
    _imageBasePath = imageBasePath;
    _atmsByType = new Dictionary<string, List<string>>
    {
        { "NCR", new List<string>() },
    { "GRG", new List<string>() },
{ "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}


private readonly object _lock = new object();


public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}


public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}


public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}


public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}


public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}


public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}


private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}


private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}


private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}


private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}


private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


public event Action<string> OnLog;


public event Action<ImageSyncItem, string> OnImageSent; // item, atmId


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


public event Action<Exception> OnError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_operational\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

}
public partial class ImageSyncEngine
{
    private readonly string _imageBasePath;


    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs


    private readonly List<ImageSyncItem> _syncQueue;


    private FileSystemWatcher _watcher;


    private bool _isRunning;


    private Thread _syncThread;


    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";


    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";


    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";


    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";


    public int SyncIntervalMs { get; set; } = 10000;


    public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
    {
        if (!File.Exists(filePath))
        {
            OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
            return;
        }

    var item = new ImageSyncItem;
    {
        ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
        FileName = Path.GetFileName(filePath),
        FilePath = filePath,
        FileSize = new FileInfo(filePath).Length,
        Checksum = ComputeMD5(filePath),
        TargetATMType = targetType,
        TargetATMs = specificATMs ?? new List<string>(),
        ScheduledTime = DateTime.Now,
        Status = ImageSyncStatus.Pending
    };

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}


public ImageSyncEngine(string imageBasePath)
{
    _imageBasePath = imageBasePath;
    _atmsByType = new Dictionary<string, List<string>>
    {
        { "NCR", new List<string>() },
    { "GRG", new List<string>() },
{ "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}


private readonly object _lock = new object();


public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}


public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}


public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}


public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}


public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}


public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}


private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}


private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}


private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}


private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}


private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


public event Action<string> OnLog;


public event Action<ImageSyncItem, string> OnImageSent; // item, atmId


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


public event Action<Exception> OnError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Updated_20260512\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

}
public partial class ImageSyncEngine
{
    private readonly string _imageBasePath;


    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs


    private readonly List<ImageSyncItem> _syncQueue;


    private FileSystemWatcher _watcher;


    private bool _isRunning;


    private Thread _syncThread;


    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";


    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";


    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";


    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";


    public int SyncIntervalMs { get; set; } = 10000;


    public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
    {
        if (!File.Exists(filePath))
        {
            OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
            return;
        }

    var item = new ImageSyncItem;
    {
        ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
        FileName = Path.GetFileName(filePath),
        FilePath = filePath,
        FileSize = new FileInfo(filePath).Length,
        Checksum = ComputeMD5(filePath),
        TargetATMType = targetType,
        TargetATMs = specificATMs ?? new List<string>(),
        ScheduledTime = DateTime.Now,
        Status = ImageSyncStatus.Pending
    };

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}


public ImageSyncEngine(string imageBasePath)
{
    _imageBasePath = imageBasePath;
    _atmsByType = new Dictionary<string, List<string>>
    {
        { "NCR", new List<string>() },
    { "GRG", new List<string>() },
{ "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}


private readonly object _lock = new object();


public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}


public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}


public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}


public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}


public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}


public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}


private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}


private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}


private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}


private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}


private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


public event Action<string> OnLog;


public event Action<ImageSyncItem, string> OnImageSent; // item, atmId


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLiveWorkCoder\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


public event Action<Exception> OnError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLiveWorkCoder\EJLiveWorkCoder\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLiveWorkCoder112233\EJLiveWorkCoder\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

}
public partial class ImageSyncEngine
{
    private readonly string _imagesPath;


    private readonly byte[] _sessionKey;


    public int    ImagesSynced    { get; private set; }


    public long   TotalBytesSent  { get; private set; }


    public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
    {
        _imagesPath = imagesPath ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "EJLive", "Images");
        _sessionKey = sessionKey;
        if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
    }


public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data     = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off   = i * (int)AppConstants.ChunkSizeBytes;
        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256  = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}


public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer  = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts    = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData   = buffer.ToArray();
    var actualMd5   = SecurityHelper.MD5Hash(imageData);
    var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}


public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}


public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}


private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");


private void Log(string msg) => OnLog?.Invoke(this, msg);


public event EventHandler<string> OnImageSynced;


public event EventHandler<string> OnImageError;


public event EventHandler<string> OnLog;


public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}

}
public partial class ImageSyncEngine
{
    private readonly string _imagesPath;


    private readonly byte[] _sessionKey;


    public int    ImagesSynced    { get; private set; }


    public long   TotalBytesSent  { get; private set; }


    public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
    {
        _imagesPath = imagesPath ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "EJLive", "Images");
        _sessionKey = sessionKey;
        if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
    }


public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data     = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off   = i * (int)AppConstants.ChunkSizeBytes;
        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256  = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}


public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer  = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts    = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData   = buffer.ToArray();
    var actualMd5   = SecurityHelper.MD5Hash(imageData);
    var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}


public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}


public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}


private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");


private void Log(string msg) => OnLog?.Invoke(this, msg);


public event EventHandler<string> OnImageSynced;


public event EventHandler<string> OnImageError;


public event EventHandler<string> OnLog;


public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
    public Func<string, byte[], string, bool>? DeliveryBridge;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

byte[] payload;
try
{
    payload = File.ReadAllBytes(item.FilePath);
    if (payload.Length == 0)
    {
        item.Status = ImageSyncStatus.Failed;
        OnLog?.Invoke($"[ImageSync] Empty payload for: {item.FileName}");
        return;
    }
}
catch (Exception ex)
{
    item.Status = ImageSyncStatus.Failed;
    OnError?.Invoke(ex);
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    try
    {
        var delivered = DeliveryBridge?.Invoke(atmId, payload, item.FileName) ?? true;
        item.DeliveryStatus[atmId] = delivered;

        if (delivered)
        {
            success++;
            OnImageSent?.Invoke(item, atmId);
        }
    else
    {
        OnImageFailed?.Invoke(item, atmId, "Delivery bridge returned false.");
    }
}
catch (Exception ex)
{
    item.DeliveryStatus[atmId] = false;
    OnImageFailed?.Invoke(item, atmId, ex.Message);
}
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}

}
public partial class ImageSyncEngine
{
    private readonly string _imagesPath;


    private readonly byte[] _sessionKey;


    private readonly string _imageBasePath;


    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs


    private readonly List<ImageSyncItem> _syncQueue;


    private FileSystemWatcher _watcher;


    private bool _isRunning;


    private Thread _syncThread;


    public int    ImagesSynced    { get; private set; }


    public long   TotalBytesSent  { get; private set; }


    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";


    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";


    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";


    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";


    public int SyncIntervalMs { get; set; } = 10000;


    public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
    {
        if (!File.Exists(filePath))
        {
            OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
            return;
        }

    var item = new ImageSyncItem;
    {
        ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
        FileName = Path.GetFileName(filePath),
        FilePath = filePath,
        FileSize = new FileInfo(filePath).Length,
        Checksum = ComputeMD5(filePath),
        TargetATMType = targetType,
        TargetATMs = specificATMs ?? new List<string>(),
        ScheduledTime = DateTime.Now,
        Status = ImageSyncStatus.Pending
    };

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}


public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
{
    _imagesPath = imagesPath ?? Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "EJLive", "Images");
    _sessionKey = sessionKey;
    if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
}


public ImageSyncEngine(string imageBasePath)
{
    _imageBasePath = imageBasePath;
    _atmsByType = new Dictionary<string, List<string>>
    {
        { "NCR", new List<string>() },
    { "GRG", new List<string>() },
{ "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}


public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data     = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off   = i * (int)AppConstants.ChunkSizeBytes;
        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256  = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}


public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer  = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts    = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData   = buffer.ToArray();
    var actualMd5   = SecurityHelper.MD5Hash(imageData);
    var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}


public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}


public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}


private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");


private void Log(string msg) => OnLog?.Invoke(this, msg);


private readonly object _lock = new object();


public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}


public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}


public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}


public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}


public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}


public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}


private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}


private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}


private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}


private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}


private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


public event EventHandler<string> OnImageSynced;


public event EventHandler<string> OnImageError;


public event EventHandler<string> OnLog;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


public event Action<ImageSyncItem, string> OnImageSent; // item, atmId


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


public event Action<Exception> OnError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

}
public partial class ImageSyncEngine
{
    private readonly string _imagesPath;


    private readonly byte[] _sessionKey;


    public int ImagesSynced
    {
        get;
        private set;
    }


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
public long TotalBytesSent
{
    get;
    private set;
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
public long TotalBytesSent
{
    get;
    private set;
}


public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
{
    _imagesPath = imagesPath ?? Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "EJLive", "Images");
    _sessionKey = sessionKey;
    if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
}


public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off = i * (int)AppConstants.ChunkSizeBytes;
        var len = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256 = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}


public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData = buffer.ToArray();
    var actualMd5 = SecurityHelper.MD5Hash(imageData);
    var verified = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}


public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] {
        "*.jpg",
        "*.jpeg",
        "*.png",
        "*.bmp",
        "*.gif"
    })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}


public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] {
        "*.jpg",
        "*.jpeg",
        "*.png",
        "*.bmp"
    })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}


private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");


private void Log(string msg) => OnLog?.Invoke(this, msg);


public event EventHandler<string> OnImageSynced;


public event EventHandler<string> OnImageError;


public event EventHandler<string> OnLog;


public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath
    {
        get;
        set;
    } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath
    {
        get;
        set;
    } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath
    {
        get;
        set;
    } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath
    {
        get;
        set;
    } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs
    {
        get;
        set;
    } = 10000;
#endregion

#region Constructor
public ImageSyncEngine(string imageBasePath)
{
    _imageBasePath = imageBasePath;
    _atmsByType = new Dictionary<string, List<string>> {
        {
            "NCR",
            new List < string > ()
        },
    {
        "GRG",
        new List < string > ()
    },
{
    "WN",
    new List < string > ()
}
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock)
{
    _syncQueue.Add(item);
}
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop)
    {
        IsBackground = true,
        Name = "ImageSyncThread"
    };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null)
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _watcher = null;
    }
OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock)
    {
        return _syncQueue.ToList();
    }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock)
            {
                pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList();
            }

        foreach (var item in pending)
        {
            if (!_isRunning) break;
            ProcessSyncItem(item);
        }
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}

}
/// <summary>
/// محرك مزامنة الصور - نقل الصور من السرفر إلى الصرافات
/// مشاركة الصور في مجلد السرفر حسب نوع الصرافات
/// مزامنة الصور إلى كل الصرافات حسب مسار ملف الصور ونوعية الصراف
/// </summary>
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}
public partial class ImageSyncEngine : IDisposable
{
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
    private readonly string _imagesPath;
    private readonly List<string> _pendingImages = new();
    private readonly object _lock = new();
    private readonly byte[] _sessionKey;
    public int ImagesSynced
    {
        get;
        private set;
    }
public long TotalBytesSent
{
    get;
    private set;
}
private readonly string _sharedFolder;
private FileSystemWatcher? _watcher;
private CancellationTokenSource? _cts;
private Task? _syncTask;
private readonly Dictionary<string, DateTime> _lastSyncTimes = new();
public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
public int SyncIntervalMs { get; set; } = 10000;
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }
var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};
lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}
public bool IsRunning => _isRunning;
public int SyncAll(string targetAtmType)
{
    var count = 0;
    foreach (var img in _pendingImages.ToArray())
    {
        try { count++; OnImageSynced?.Invoke(this, img); }
        catch (Exception ex) { OnImageError?.Invoke(this, ex.Message); }
    }
_pendingImages.Clear();
return count;
}
public int    ImagesSynced    { get; private set; }
public long   TotalBytesSent  { get; private set; }
public bool IsRunning { get; private set; }
public ImageSyncEngine(string imageBasePath)
{
    _imageBasePath = imageBasePath;
    _atmsByType = new Dictionary<string, List<string>>
    {
        { "NCR", new List<string>() },
    { "GRG", new List<string>() },
{ "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
public ImageSyncEngine()
{
    _imagesPath = AppConstants.DefaultImagesPath;
    EnsureDirectory();
}
public ImageSyncEngine(string imagesPath)
{
    _imagesPath = imagesPath ?? AppConstants.DefaultImagesPath;
    EnsureDirectory();
}
public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
{
    _imagesPath = imagesPath ?? Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "EJLive", "Images");
    _sessionKey = sessionKey;
    if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
}
public ImageSyncEngine(string sharedFolder)
_sharedFolder = sharedFolder;
private readonly object _lock = new object();
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;
    // مراقبة مجلد الصور المشتركة
    SetupWatcher();
    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();
OnLog?.Invoke("[ImageSync] Engine started");
}
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }
    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);
        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}
private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";
        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}
private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }
            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}
private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;
    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }
if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}
int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}
item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}
private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
public void Start() { _isRunning = true; Log("ImageSync started."); }
public void Stop() { _isRunning = false; Log("ImageSync stopped."); }
public void QueueImage(string filePath)
{
    if (File.Exists(filePath))
    {
        lock (_lock) { _pendingImages.Add(filePath); }
        Log($"Queued image: {Path.GetFileName(filePath)}");
    }
}
private void EnsureDirectory() { if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath); }
private void Log(string msg) => OnLog?.Invoke(this, $"[ImageSync] {msg}");
public void Dispose() { Stop(); }
public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }
try
{
    var data     = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);
    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);
    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off   = i * (int)AppConstants.ChunkSizeBytes;
        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }
// COMPLETE
var sha256  = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);
ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}
public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer  = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);
        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);
            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }
    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts    = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";
    var imageData   = buffer.ToArray();
    var actualMd5   = SecurityHelper.MD5Hash(imageData);
    var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);
    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}
// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}
public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}
public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}
private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");
private void Log(string msg) => OnLog?.Invoke(this, msg);
public event Action<string> OnLog;
public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
public event Action<Exception> OnError;
public event EventHandler<string> OnLog;
public event EventHandler<string>? OnImageSynced;
public event EventHandler<string>? OnImageError;
public event EventHandler<string> OnImageSynced;
public event EventHandler<string> OnImageError;
public event EventHandler<ImageSyncEventArgs>? ImageReadyForDistribution;
public event EventHandler<string>? SyncCompleted;
public event EventHandler<string>? SyncError;
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion
#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion
#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion
#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion
#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}
/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}
/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }
var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};
lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}
/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;
    // مراقبة مجلد الصور المشتركة
    SetupWatcher();
    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();
OnLog?.Invoke("[ImageSync] Engine started");
}
/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}
/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}
/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }
    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion
#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);
        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}
private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";
        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}
private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }
            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}
private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;
    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }
if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}
int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}
item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}
private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
    public Func<string, byte[], string, bool>? DeliveryBridge;
#endregion
#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion
#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion
#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion
#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}
/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}
/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }
var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};
lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}
/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;
    // مراقبة مجلد الصور المشتركة
    SetupWatcher();
    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();
OnLog?.Invoke("[ImageSync] Engine started");
}
/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}
/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}
/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }
    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion
#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);
        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}
private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";
        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}
private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }
            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}
private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;
    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }
if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}
byte[] payload;
try
{
    payload = File.ReadAllBytes(item.FilePath);
    if (payload.Length == 0)
    {
        item.Status = ImageSyncStatus.Failed;
        OnLog?.Invoke($"[ImageSync] Empty payload for: {item.FileName}");
        return;
    }
}
catch (Exception ex)
{
    item.Status = ImageSyncStatus.Failed;
    OnError?.Invoke(ex);
    return;
}
int success = 0;
foreach (string atmId in targetATMs)
{
    try
    {
        var delivered = DeliveryBridge?.Invoke(atmId, payload, item.FileName) ?? true;
        item.DeliveryStatus[atmId] = delivered;
        if (delivered)
        {
            success++;
            OnImageSent?.Invoke(item, atmId);
        }
    else
    {
        OnImageFailed?.Invoke(item, atmId, "Delivery bridge returned false.");
    }
}
catch (Exception ex)
{
    item.DeliveryStatus[atmId] = false;
    OnImageFailed?.Invoke(item, atmId, ex.Message);
}
}
item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}
private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}
}
public partial public public class ImageSyncEngine
{
    private readonly string _imagesPath;
    private readonly string _imageBasePath;
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
    public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
    {
        public ImageSyncEngine(string imageBasePath)
        {
            public int ImagesSynced
            {
                get;
                private set;
            }
        public long TotalBytesSent
        {
            get;
            private set;
        }
    public class ImageSyncEngine
    {
#region Events
        public event Action<string> OnLog;
        public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
        public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
        public event Action<Exception> OnError;
#endregion

#region Fields
        private readonly string _imageBasePath;
        private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
        private readonly List<ImageSyncItem> _syncQueue;
        private readonly object _lock = new object();
        private FileSystemWatcher _watcher;
        private bool _isRunning;
        private Thread _syncThread;
#endregion

#region Configuration
        public string NCRImagePath
        {
            get;
            set;
        }
    public string GRGImagePath
    {
        get;
        set;
    }
public string WNImagePath
{
    get;
    set;
}
public string SharedImagePath
{
    get;
    set;
}
public int SyncIntervalMs
{
    get;
    set;
}
public string NCRImagePath
{
    get;
    set;
}
public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
    {
        public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
        {
            public List<string> GetLocalImages()
            {
                private string SanitizeName(string name) =>
                private void Log(string msg) =>
                public void RegisterATM(string atmId, string atmType)
                {
                    public void UnregisterATM(string atmId)
                    {
                        public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
                        {
                            public void Start()
                            {
                                public void Stop()
                                {
                                    public List<ImageSyncItem> GetSyncQueue()
                                    {
                                        public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
                                        {
                                            private void SetupWatcher()
                                            {
                                                private void Watcher_Created(object sender, FileSystemEventArgs e)
                                                {
                                                    private void SyncLoop()
                                                    {
                                                        private void ProcessSyncItem(ImageSyncItem item)
                                                        {
                                                            private string ComputeMD5(string filePath)
                                                            {
                                                                private string ComputeMD5Bytes(byte[] data)
                                                                {
                                                                    public event Action<string> OnLog;
                                                                    public event Action<ImageSyncItem, string> OnImageSent;
                                                                    public event Action<ImageSyncItem, string, string> OnImageFailed;
                                                                    public event Action<Exception> OnError;
                                                                    public event EventHandler<string> OnImageSynced;
                                                                    public event EventHandler<string> OnImageError;
                                                                    public event EventHandler<string> OnLog;
                                                                }

                                                        }
                                                    public partial enum MsgType
                                                    {
                                                        payload = string.Empty;


                                                        if (string.IsNullOrWhiteSpace(value))
                                                        return false;


                                                        if (separator < 0)
                                                        atmId = value;


                                                        if (separator >= 0)
                                                        var tail = value[(separator + 1)..].Trim();


                                                        if (tail.StartsWith("{", StringComparison.Ordinal))
                                                        jsonCandidate = tail;


                                                        if (!jsonCandidate.StartsWith("{", StringComparison.Ordinal))
                                                        try
                                                        if (document.RootElement.ValueKind != JsonValueKind.Object)
                                                        var root = document.RootElement;


                                                        var serverTime = DateTime.UtcNow;


                                                        var pendingCount = 0;


                                                        var requestImmediateSync = false;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                        serverTime = parsedServerTime.ToUniversalTime();


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                        pendingCount = Math.Max(0, parsedPending);


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        bool requestImmediateSync = false,


                                                        catch (JsonException)
                                                        public static byte[] BuildStartFile(string atmId, string fileName, long length, long offset, string checksum) => BuildFrame(MsgType.StartFile, $"{atmId}|{fileName}|{length}|{offset}|{checksum}");


                                                        ```

                                                        var frame = new byte[header.Length + body.Length];


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        catch


                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        return frame;


                                                        var typeName = header[..separator];


                                                        var lengthText = header[(separator + 1)..];


                                                        if (!Enum.TryParse<MsgType>(typeName, out var type))
                                                        type = MsgType.Unknown;


                                                        return null;


                                                        value = default;


                                                        while (true)
                                                        var next = stream.ReadByte();


                                                        if (next == '\n')
                                                        break;


                                                        var offset = 0;


                                                        while (offset < length)
                                                        var read = stream.Read(buffer, offset, length - offset);


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        offset += read;


                                                        return buffer;


                                                        if (string.IsNullOrWhiteSpace(IssuedAtUtc))
                                                        IssuedAtUtc = DateTime.UtcNow.ToString("O");


                                                        if (string.IsNullOrWhiteSpace(Nonce))
                                                        Nonce = Guid.NewGuid().ToString("N");


                                                        SignatureVersion = CommandSigningEngine.SignatureVersion;


                                                        var payloadBase64 = string.Empty;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        catch (FormatException)
                                                        command.Payload = payloadBase64;


                                                        if (!CommandSigningEngine.IsFresh(command.IssuedAtUtc, out var freshnessReason))
                                                        command.SignatureFailureReason = freshnessReason;


                                                        if (!CommandSigningEngine.VerifyCanonical(canonical, command.Signature, out var verifyReason))
                                                        command.SignatureFailureReason = verifyReason;


                                                        command.SignatureVerified = true;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        command.SignatureVerified = false;


                                                        private readonly string _encryptionIV;


                                                        private readonly bool _useEncryption;


                                                        private readonly bool _useCompression;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        _useEncryption = useEncryption;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        _useCompression = useCompression;


                                                        string fullMessage = header + "\n" + data + Protocol.DATA_END;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                        if (_useEncryption && data.Length > 0)
                                                        data = Decrypt(data);


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        if (_useCompression && data.Length > 0)
                                                        data = Decompress(data);


                                                        int pendingCommandCount,


                                                        string? serverMessage = null)


                                                        ATM_ID = atmId,


                                                        RequestImmediateSync = requestImmediateSync,


                                                        ServerMessage = serverMessage


                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                        if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                        serverTime = parsedServerTime.ToUniversalTime();


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        int pendingCommandCount,


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        string? serverMessage = null)


                                                        ATM_ID = atmId,


                                                        RequestImmediateSync = requestImmediateSync,


                                                        ServerMessage = serverMessage


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                        if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                        serverTime = parsedServerTime.ToUniversalTime();


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                        if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                        pendingCount = Math.Max(0, parsedPending);


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                        bool requestImmediateSync = false,


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                        catch


                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        return frame;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                        offset += read;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                        catch (FormatException)
                                                        command.Payload = payloadBase64;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                        command.SignatureVerified = false;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                        _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                        _useEncryption = useEncryption;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                        _useCompression = useCompression;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                        string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                        if (_useCompression && data.Length > 0)
                                                        data = Decompress(data);


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                        string? serverMessage = null)


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                        return true;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ImageSyncEngine.cs
                                                        int pendingCommandCount,


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                        serverTime = parsedServerTime.ToUniversalTime();


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                        pendingCount = Math.Max(0, parsedPending);


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        bool requestImmediateSync = false,


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        return true;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        catch

                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        return frame;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        offset += read;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        catch (FormatException)
                                                        command.Payload = payloadBase64;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        command.SignatureVerified = false;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        _useEncryption = useEncryption;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        _useCompression = useCompression;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        if (_useCompression && data.Length > 0)
                                                        data = Decompress(data);


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        int pendingCommandCount,


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                        string? serverMessage = null)


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                        serverTime = parsedServerTime.ToUniversalTime();


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                        pendingCount = Math.Max(0, parsedPending);


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        if (bool.TryParse(immediateText, out var parsedImmediate))
                                                        requestImmediateSync = parsedImmediate;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        return true;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        catch


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        return frame;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        offset += read;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        catch (FormatException)
                                                        command.Payload = payloadBase64;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        command.SignatureVerified = false;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        _useEncryption = useEncryption;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        _useCompression = useCompression;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                        if (_useCompression && data.Length > 0)
                                                        data = Decompress(data);


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                        if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                        serverTime = parsedServerTime.ToUniversalTime();


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                        if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                        pendingCount = Math.Max(0, parsedPending);


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                        if (bool.TryParse(immediateText, out var parsedImmediate))
                                                        requestImmediateSync = parsedImmediate;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                        return true;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                        ```


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                        return frame;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                        offset += read;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                        catch (FormatException)
                                                        command.Payload = payloadBase64;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                        command.SignatureVerified = false;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                        _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                        _useEncryption = useEncryption;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                        _useCompression = useCompression;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                        string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                        if (_useCompression && data.Length > 0)
                                                        data = Decompress(data);


                                                        public string CommandType { get; set; } = string.Empty;


                                                        public string Payload { get; set; } = string.Empty;


                                                        public bool RequiresConfirmation { get; set; }


                                                        public string IssuedAtUtc { get; set; } = DateTime.UtcNow.ToString("O");


                                                        public string Nonce { get; set; } = Guid.NewGuid().ToString("N");


                                                        public string Signature { get; set; } = string.Empty;


                                                        public string SignatureVersion { get; set; } = CommandSigningEngine.SignatureVersion;


                                                        public bool SignatureVerified { get; private set; }


                                                        public string SignatureFailureReason { get; private set; } = string.Empty;


                                                        public static byte[] BuildHandshakeAck(string sessionId) => BuildFrame(MsgType.HandshakeAck, $"OK|{sessionId}");


                                                        public static byte[] BuildHeartbeat(string atmId, string payload = "") => BuildFrame(MsgType.Heartbeat, $"{atmId}|{payload}");


                                                        public static byte[] BuildHeartbeatAck(string atmId = "") => BuildFrame(MsgType.HeartbeatAck, atmId);


                                                        public static byte[] BuildHeartbeatAck(


                                                        var payload = JsonSerializer.Serialize(new;


                                                        ServerTimeUtc = serverTimeUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),


                                                        PendingCommandCount = Math.Max(0, pendingCommandCount),


                                                        public static byte[] BuildHeartbeatAck(
                                                        string atmId,
                                                        int pendingCommandCount,
                                                        bool requestImmediateSync = false,
                                                        string? serverMessage = null)
                                                        var payload = JsonSerializer.Serialize(new;
                                                        ATM_ID = atmId,
                                                        ServerTimeUtc = serverTimeUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                                                        PendingCommandCount = Math.Max(0, pendingCommandCount),
                                                        RequestImmediateSync = requestImmediateSync,
                                                        ServerMessage = serverMessage
                                                    }
                                                public partial enum MsgType
                                                {
                                                    payload = string.Empty;


                                                    if (string.IsNullOrWhiteSpace(value))
                                                    return false;


                                                    if (separator < 0)
                                                    atmId = value;


                                                    if (separator >= 0)
                                                    var tail = value[(separator + 1)..].Trim();


                                                    if (tail.StartsWith("{", StringComparison.Ordinal))
                                                    jsonCandidate = tail;


                                                    if (!jsonCandidate.StartsWith("{", StringComparison.Ordinal))
                                                    try
                                                    if (document.RootElement.ValueKind != JsonValueKind.Object)
                                                    var root = document.RootElement;


                                                    var serverTime = DateTime.UtcNow;


                                                    var pendingCount = 0;


                                                    var requestImmediateSync = false;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                    if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                    serverTime = parsedServerTime.ToUniversalTime();


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                    if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                    pendingCount = Math.Max(0, parsedPending);


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                    bool requestImmediateSync = false,


                                                    return true;


                                                    catch (JsonException)
                                                    public static byte[] BuildStartFile(string atmId, string fileName, long length, long offset, string checksum) => BuildFrame(MsgType.StartFile, $"{atmId}|{fileName}|{length}|{offset}|{checksum}");


                                                    ```

                                                    var frame = new byte[header.Length + body.Length];


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                    catch

                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                    return frame;


                                                    var typeName = header[..separator];


                                                    var lengthText = header[(separator + 1)..];


                                                    if (!Enum.TryParse<MsgType>(typeName, out var type))
                                                    type = MsgType.Unknown;


                                                    return null;


                                                    value = default;


                                                    while (true)
                                                    var next = stream.ReadByte();


                                                    if (next == '\n')
                                                    break;


                                                    var offset = 0;


                                                    while (offset < length)
                                                    var read = stream.Read(buffer, offset, length - offset);


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                    offset += read;


                                                    return buffer;


                                                    if (string.IsNullOrWhiteSpace(IssuedAtUtc))
                                                    IssuedAtUtc = DateTime.UtcNow.ToString("O");


                                                    if (string.IsNullOrWhiteSpace(Nonce))
                                                    Nonce = Guid.NewGuid().ToString("N");


                                                    SignatureVersion = CommandSigningEngine.SignatureVersion;


                                                    var payloadBase64 = string.Empty;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                    catch (FormatException)
                                                    command.Payload = payloadBase64;


                                                    if (!CommandSigningEngine.IsFresh(command.IssuedAtUtc, out var freshnessReason))
                                                    command.SignatureFailureReason = freshnessReason;


                                                    if (!CommandSigningEngine.VerifyCanonical(canonical, command.Signature, out var verifyReason))
                                                    command.SignatureFailureReason = verifyReason;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                    command.SignatureVerified = true;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                    command.SignatureVerified = false;


                                                    private readonly string _encryptionIV;


                                                    private readonly bool _useEncryption;


                                                    private readonly bool _useCompression;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                    _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                    _useEncryption = useEncryption;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                    _useCompression = useCompression;


                                                    string fullMessage = header + "\n" + data + Protocol.DATA_END;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                    string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                    if (_useEncryption && data.Length > 0)
                                                    data = Decrypt(data);


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                    if (_useCompression && data.Length > 0)
                                                    data = Decompress(data);


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\ImageSyncEngine.cs
                                                    int pendingCommandCount,


                                                    string? serverMessage = null)


                                                    ATM_ID = atmId,


                                                    RequestImmediateSync = requestImmediateSync,


                                                    ServerMessage = serverMessage


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                    if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                    serverTime = parsedServerTime.ToUniversalTime();


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                    if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                    pendingCount = Math.Max(0, parsedPending);


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                    if (bool.TryParse(immediateText, out var parsedImmediate))
                                                    requestImmediateSync = parsedImmediate;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                    ```


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                    return frame;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                    offset += read;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                    catch (FormatException)
                                                    command.Payload = payloadBase64;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                    command.SignatureVerified = true;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                    command.SignatureVerified = false;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                    _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                    _useEncryption = useEncryption;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                    _useCompression = useCompression;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                    string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                    if (_useCompression && data.Length > 0)
                                                    data = Decompress(data);


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                    if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                    serverTime = parsedServerTime.ToUniversalTime();


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                    if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                    pendingCount = Math.Max(0, parsedPending);


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                    if (bool.TryParse(immediateText, out var parsedImmediate))
                                                    requestImmediateSync = parsedImmediate;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                    catch


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                    return frame;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                    offset += read;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                    catch (FormatException)
                                                    command.Payload = payloadBase64;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                    command.SignatureVerified = true;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                    command.SignatureVerified = false;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                    _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                    _useEncryption = useEncryption;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                    _useCompression = useCompression;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                    string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                    if (_useCompression && data.Length > 0)
                                                    data = Decompress(data);


                                                    public string CommandType { get; set; } = string.Empty;


                                                    public string Payload { get; set; } = string.Empty;


                                                    public bool RequiresConfirmation { get; set; }


                                                    public string IssuedAtUtc { get; set; } = DateTime.UtcNow.ToString("O");


                                                    public string Nonce { get; set; } = Guid.NewGuid().ToString("N");


                                                    public string Signature { get; set; } = string.Empty;


                                                    public string SignatureVersion { get; set; } = CommandSigningEngine.SignatureVersion;


                                                    public bool SignatureVerified { get; private set; }


                                                    public string SignatureFailureReason { get; private set; } = string.Empty;


                                                    public static byte[] BuildHandshakeAck(string sessionId) => BuildFrame(MsgType.HandshakeAck, $"OK|{sessionId}");


                                                    public static byte[] BuildHeartbeat(string atmId, string payload = "") => BuildFrame(MsgType.Heartbeat, $"{atmId}|{payload}");


                                                    public static byte[] BuildHeartbeatAck(string atmId = "") => BuildFrame(MsgType.HeartbeatAck, atmId);


                                                    public static byte[] BuildHeartbeatAck(


                                                    var payload = JsonSerializer.Serialize(new;


                                                    ServerTimeUtc = serverTimeUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),


                                                    PendingCommandCount = Math.Max(0, pendingCommandCount),


                                                    public static byte[] BuildHeartbeatAck(
                                                    string atmId,
                                                    int pendingCommandCount,
                                                    bool requestImmediateSync = false,
                                                    string? serverMessage = null)
                                                    var payload = JsonSerializer.Serialize(new;
                                                    ATM_ID = atmId,
                                                    ServerTimeUtc = serverTimeUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                                                    PendingCommandCount = Math.Max(0, pendingCommandCount),
                                                    RequestImmediateSync = requestImmediateSync,
                                                    ServerMessage = serverMessage
                                                }
                                            public partial enum MsgType
                                            {
                                                payload = string.Empty;
                                                if (string.IsNullOrWhiteSpace(value))
                                                return false;
                                                if (separator < 0)
                                                atmId = value;
                                                if (separator >= 0)
                                                var tail = value[(separator + 1)..].Trim();
                                                if (tail.StartsWith("{", StringComparison.Ordinal))
                                                jsonCandidate = tail;
                                                if (!jsonCandidate.StartsWith("{", StringComparison.Ordinal))
                                                try
                                                if (document.RootElement.ValueKind != JsonValueKind.Object)
                                                var root = document.RootElement;
                                                var serverTime = DateTime.UtcNow;
                                                var pendingCount = 0;
                                                var requestImmediateSync = false;
                                                if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                serverTime = parsedServerTime.ToUniversalTime();
                                                if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                pendingCount = Math.Max(0, parsedPending);
                                                bool requestImmediateSync = false,
                                                return true;
                                                catch (JsonException)
                                                public static byte[] BuildStartFile(string atmId, string fileName, long length, long offset, string checksum) => BuildFrame(MsgType.StartFile, $"{atmId}|{fileName}|{length}|{offset}|{checksum}");
                                                ```
                                                var frame = new byte[header.Length + body.Length];
                                                catch
                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
                                                return frame;
                                                var typeName = header[..separator];
                                                var lengthText = header[(separator + 1)..];
                                                if (!Enum.TryParse<MsgType>(typeName, out var type))
                                                type = MsgType.Unknown;
                                                return null;
                                                value = default;
                                                while (true)
                                                var next = stream.ReadByte();
                                                if (next == '\n')
                                                break;
                                                var offset = 0;
                                                while (offset < length)
                                                var read = stream.Read(buffer, offset, length - offset);
                                                offset += read;
                                                return buffer;
                                                if (string.IsNullOrWhiteSpace(IssuedAtUtc))
                                                IssuedAtUtc = DateTime.UtcNow.ToString("O");
                                                if (string.IsNullOrWhiteSpace(Nonce))
                                                Nonce = Guid.NewGuid().ToString("N");
                                                SignatureVersion = CommandSigningEngine.SignatureVersion;
                                                var payloadBase64 = string.Empty;
                                                catch (FormatException)
                                                command.Payload = payloadBase64;
                                                if (!CommandSigningEngine.IsFresh(command.IssuedAtUtc, out var freshnessReason))
                                                command.SignatureFailureReason = freshnessReason;
                                                if (!CommandSigningEngine.VerifyCanonical(canonical, command.Signature, out var verifyReason))
                                                command.SignatureFailureReason = verifyReason;
                                                command.SignatureVerified = true;
                                                command.SignatureVerified = false;
                                                private readonly string _encryptionIV;
                                                private readonly bool _useEncryption;
                                                private readonly bool _useCompression;
                                                _encryptionIV = SecurityConfig.DEFAULT_IV;
                                                _useEncryption = useEncryption;
                                                _useCompression = useCompression;
                                                string fullMessage = header + "\n" + data + Protocol.DATA_END;
                                                string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;
                                                if (_useEncryption && data.Length > 0)
                                                data = Decrypt(data);
                                                if (_useCompression && data.Length > 0)
                                                data = Decompress(data);
                                                int pendingCommandCount,
                                                string? serverMessage = null)
                                                ATM_ID = atmId,
                                                RequestImmediateSync = requestImmediateSync,
                                                ServerMessage = serverMessage
                                                int pendingCommandCount,
                                                string? serverMessage = null)
                                                ATM_ID = atmId,
                                                RequestImmediateSync = requestImmediateSync,
                                                ServerMessage = serverMessage
                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
                                                if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                serverTime = parsedServerTime.ToUniversalTime();
                                                public string CommandType { get; set; } = string.Empty;
                                                public string Payload { get; set; } = string.Empty;
                                                public bool RequiresConfirmation { get; set; }
                                                public string IssuedAtUtc { get; set; } = DateTime.UtcNow.ToString("O");
                                                public string Nonce { get; set; } = Guid.NewGuid().ToString("N");
                                                public string Signature { get; set; } = string.Empty;
                                                public string SignatureVersion { get; set; } = CommandSigningEngine.SignatureVersion;
                                                public bool SignatureVerified { get; private set; }
                                                public string SignatureFailureReason { get; private set; } = string.Empty;
                                                public static byte[] BuildHandshakeAck(string sessionId) => BuildFrame(MsgType.HandshakeAck, $"OK|{sessionId}");
                                                public static byte[] BuildHeartbeat(string atmId, string payload = "") => BuildFrame(MsgType.Heartbeat, $"{atmId}|{payload}");
                                                public static byte[] BuildHeartbeatAck(string atmId = "") => BuildFrame(MsgType.HeartbeatAck, atmId);
                                                public static byte[] BuildHeartbeatAck(
                                                var payload = JsonSerializer.Serialize(new;
                                                ServerTimeUtc = serverTimeUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                                                PendingCommandCount = Math.Max(0, pendingCommandCount),
                                            }

                                        Buffer.BlockCopy(header, 0, frame, 0, header.Length);
                                        Buffer.BlockCopy(body, 0, frame, header.Length, body.Length);

                                        public static byte[] BuildChunkAck(int sequence) => BuildFrame(MsgType.ChunkAck, sequence.ToString(), BitConverter.GetBytes(sequence));
                                        public static byte[] BuildCommand(EJLive.Core.Models.RemoteCommandEnvelope command) => BuildFrame(MsgType.Command, command.ToWireText());
                                        public static byte[] BuildCommandResult(string commandId, bool ok, string message) => BuildFrame(MsgType.CommandResult, $"{commandId}|{(ok ? "OK" : "FAIL")}|{Convert.ToBase64String(Encoding.UTF8.GetBytes(message ?? string.Empty))}");
                                        public static byte[] BuildComplete(string fileName, string checksum, string sha256) => BuildFrame(MsgType.Complete, $"{fileName}|{checksum}|{sha256}");
                                        public static byte[] BuildDisconnect(string reason = "") => BuildFrame(MsgType.Disconnect, reason);
                                        public static byte[] BuildError(string message) => BuildFrame(MsgType.Error, message);
                                        return BuildFrame(MsgType.HeartbeatAck, payload);
                                        public static byte[] BuildFrame(MsgType type, string text, byte[]? payload = null, byte[]? sessionKey = null)
                                        var body = payload is { Length: > 0 }
                                        public static byte[] BuildGhostFrame(byte[] jpegFrame) => BuildFrame(MsgType.GhostFrame, "image/jpeg", jpegFrame);
                                        public static byte[] BuildGhostStart(string atmId) => BuildFrame(MsgType.GhostStart, atmId);
                                        public static byte[] BuildGhostStop(string atmId) => BuildFrame(MsgType.GhostStop, atmId);
                                        public static byte[] BuildJournalAck(string fileName, bool ok, string message = "") => BuildFrame(MsgType.JournalAck, $"{fileName}|{(ok ? "OK" : "FAIL")}|{message}");
                                        body = SecurityHelper.DecryptAES(body, sessionKey);
                                        // Class: EJMessage (from 2 sources)
                                        public sealed partial class EJMessage
                                        {
                                            // --- Properties ---
                                            public CommunicationProtocol.MsgType Type { get; set; }

                                            public string Text { get; set; } = string.Empty;

                                            public byte[] Payload { get; set; } = Array.Empty<byte>();

                                            public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;


                                        }
                                    body = SecurityHelper.EncryptAES(body, sessionKey);
                                    ? payload : WireEncoding.GetBytes(text ?? string.Empty);
                                    var header = WireEncoding.GetBytes($"{type}:{body.Length}\n");
                                    // Class: ImageSyncEngine (from 7 sources)
                                    public partial class ImageSyncEngine : IDisposable
                                    {
                                        // --- Constants & Fields ---
                                        private readonly string _imageBasePath;

                                        private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs

                                        private readonly List<ImageSyncItem> _syncQueue;

                                        private FileSystemWatcher _watcher;

                                        private bool _isRunning;

                                        private Thread _syncThread;

                                        private readonly string _imagesPath;

                                        private readonly List<string> _pendingImages = new();

                                        private readonly object _lock = new();

                                        private readonly byte[] _sessionKey;


                                        // --- Properties ---
                                        public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";

                                        public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";

                                        public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";

                                        public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";

                                        public int SyncIntervalMs { get; set; } = 10000;

                                        public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
                                        {
                                            if (!File.Exists(filePath))
                                            {
                                                OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
                                                return;
                                            }

                                        var item = new ImageSyncItem;
                                        {
                                            ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
                                            FileName = Path.GetFileName(filePath),
                                            FilePath = filePath,
                                            FileSize = new FileInfo(filePath).Length,
                                            Checksum = ComputeMD5(filePath),
                                            TargetATMType = targetType,
                                            TargetATMs = specificATMs ?? new List<string>(),
                                            ScheduledTime = DateTime.Now,
                                            Status = ImageSyncStatus.Pending
                                        };

                                    lock (_lock) { _syncQueue.Add(item); }
                                    OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
                                }

                            public bool IsRunning => _isRunning;

                            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs
                            public int SyncAll(string targetAtmType)
                            {
                                var count = 0;
                                foreach (var img in _pendingImages.ToArray())
                                {
                                    try { count++; OnImageSynced?.Invoke(this, img); }
                                    catch (Exception ex) { OnImageError?.Invoke(this, ex.Message); }
                                }
                            _pendingImages.Clear();
                            return count;
                        }

                    public int    ImagesSynced    { get; private set; }

                    public long   TotalBytesSent  { get; private set; }


                    // --- Constructors ---
                    public ImageSyncEngine(string imageBasePath)
                    {
                        _imageBasePath = imageBasePath;
                        _atmsByType = new Dictionary<string, List<string>>
                        {
                            { "NCR", new List<string>() },
                        { "GRG", new List<string>() },
                    { "WN", new List<string>() }
            };
        _syncQueue = new List<ImageSyncItem>();
    }

public ImageSyncEngine()
{
    _imagesPath = AppConstants.DefaultImagesPath;
    EnsureDirectory();
}

public ImageSyncEngine(string imagesPath)
{
    _imagesPath = imagesPath ?? AppConstants.DefaultImagesPath;
    EnsureDirectory();
}

public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
{
    _imagesPath = imagesPath ?? Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "EJLive", "Images");
    _sessionKey = sessionKey;
    if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
}


// --- Methods ---
private readonly object _lock = new object();

public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}

private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs
public void Start() { _isRunning = true; Log("ImageSync started."); }

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs
public void Stop() { _isRunning = false; Log("ImageSync stopped."); }

public void QueueImage(string filePath)
{
    if (File.Exists(filePath))
    {
        lock (_lock) { _pendingImages.Add(filePath); }
        Log($"Queued image: {Path.GetFileName(filePath)}");
    }
}

private void EnsureDirectory() { if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath); }

private void Log(string msg) => OnLog?.Invoke(this, $"[ImageSync] {msg}");

public void Dispose() { Stop(); }

public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data     = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off   = i * (int)AppConstants.ChunkSizeBytes;
        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256  = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}

public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer  = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts    = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData   = buffer.ToArray();
    var actualMd5   = SecurityHelper.MD5Hash(imageData);
    var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}

public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}

public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}

private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v23_bak
private void Log(string msg) => OnLog?.Invoke(this, msg);


// --- Events ---
public event Action<string> OnLog;

public event Action<ImageSyncItem, string> OnImageSent; // item, atmId

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v20_bak
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

public event Action<Exception> OnError;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v17_bak
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v16_bak
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v15_bak
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs
public event EventHandler<string>? OnLog;

public event EventHandler<string>? OnImageSynced;

public event EventHandler<string>? OnImageError;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v23_bak
public event EventHandler<string> OnImageSynced;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v23_bak
public event EventHandler<string> OnImageError;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v23_bak
public event EventHandler<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v22_bak
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error


// --- Nested Classes ---
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}

}
// Class: ImageSyncEngine (from 2 sources)
public partial class ImageSyncEngine
{
    // --- Constants & Fields ---
    private readonly string _imageBasePath;

    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs

    private readonly List<ImageSyncItem> _syncQueue;

    private FileSystemWatcher _watcher;

    private bool _isRunning;

    private Thread _syncThread;


    // --- Properties ---
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";

    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";

    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";

    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";

    public int SyncIntervalMs { get; set; } = 10000;

    public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
    {
        if (!File.Exists(filePath))
        {
            OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
            return;
        }

    var item = new ImageSyncItem;
    {
        ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
        FileName = Path.GetFileName(filePath),
        FilePath = filePath,
        FileSize = new FileInfo(filePath).Length,
        Checksum = ComputeMD5(filePath),
        TargetATMType = targetType,
        TargetATMs = specificATMs ?? new List<string>(),
        ScheduledTime = DateTime.Now,
        Status = ImageSyncStatus.Pending
    };

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}


// --- Constructors ---
public ImageSyncEngine(string imageBasePath)
{
    _imageBasePath = imageBasePath;
    _atmsByType = new Dictionary<string, List<string>>
    {
        { "NCR", new List<string>() },
    { "GRG", new List<string>() },
{ "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}


// --- Methods ---
private readonly object _lock = new object();

public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}

private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


// --- Events ---
public event Action<string> OnLog;

public event Action<ImageSyncItem, string> OnImageSent; // item, atmId

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLiveWorkCoder\EJLiveWorkCoder\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

public event Action<Exception> OnError;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLiveWorkCoder112233\EJLiveWorkCoder\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

}
// Class: ImageSyncEngine (from 3 sources)
public partial class ImageSyncEngine
{
    // --- Constants & Fields ---
    private readonly string _imagesPath;

    private readonly byte[] _sessionKey;


    // --- Properties ---
    public int    ImagesSynced    { get; private set; }

    public long   TotalBytesSent  { get; private set; }


    // --- Constructors ---
    public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
    {
        _imagesPath = imagesPath ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "EJLive", "Images");
        _sessionKey = sessionKey;
        if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
    }


// --- Methods ---
public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data     = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off   = i * (int)AppConstants.ChunkSizeBytes;
        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256  = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}

public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer  = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts    = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData   = buffer.ToArray();
    var actualMd5   = SecurityHelper.MD5Hash(imageData);
    var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}

public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}

public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}

private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");

private void Log(string msg) => OnLog?.Invoke(this, msg);


// --- Events ---
public event EventHandler<string> OnImageSynced;

public event EventHandler<string> OnImageError;

public event EventHandler<string> OnLog;


// --- Nested Classes ---
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
    public Func<string, byte[], string, bool>? DeliveryBridge;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

byte[] payload;
try
{
    payload = File.ReadAllBytes(item.FilePath);
    if (payload.Length == 0)
    {
        item.Status = ImageSyncStatus.Failed;
        OnLog?.Invoke($"[ImageSync] Empty payload for: {item.FileName}");
        return;
    }
}
catch (Exception ex)
{
    item.Status = ImageSyncStatus.Failed;
    OnError?.Invoke(ex);
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    try
    {
        var delivered = DeliveryBridge?.Invoke(atmId, payload, item.FileName) ?? true;
        item.DeliveryStatus[atmId] = delivered;

        if (delivered)
        {
            success++;
            OnImageSent?.Invoke(item, atmId);
        }
    else
    {
        OnImageFailed?.Invoke(item, atmId, "Delivery bridge returned false.");
    }
}
catch (Exception ex)
{
    item.DeliveryStatus[atmId] = false;
    OnImageFailed?.Invoke(item, atmId, ex.Message);
}
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}

}
// Class: ImageSyncEngine (from 6 sources)
public partial class ImageSyncEngine
{
    // --- Constants & Fields ---
    private readonly string _imagesPath;

    private readonly byte[] _sessionKey;

    private readonly string _imageBasePath;

    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs

    private readonly List<ImageSyncItem> _syncQueue;

    private FileSystemWatcher _watcher;

    private bool _isRunning;

    private Thread _syncThread;


    // --- Properties ---
    public int    ImagesSynced    { get; private set; }

    public long   TotalBytesSent  { get; private set; }

    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";

    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";

    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";

    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";

    public int SyncIntervalMs { get; set; } = 10000;

    public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
    {
        if (!File.Exists(filePath))
        {
            OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
            return;
        }

    var item = new ImageSyncItem;
    {
        ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
        FileName = Path.GetFileName(filePath),
        FilePath = filePath,
        FileSize = new FileInfo(filePath).Length,
        Checksum = ComputeMD5(filePath),
        TargetATMType = targetType,
        TargetATMs = specificATMs ?? new List<string>(),
        ScheduledTime = DateTime.Now,
        Status = ImageSyncStatus.Pending
    };

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}


// --- Constructors ---
public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
{
    _imagesPath = imagesPath ?? Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "EJLive", "Images");
    _sessionKey = sessionKey;
    if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
}

public ImageSyncEngine(string imageBasePath)
{
    _imageBasePath = imageBasePath;
    _atmsByType = new Dictionary<string, List<string>>
    {
        { "NCR", new List<string>() },
    { "GRG", new List<string>() },
{ "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}


// --- Methods ---
public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data     = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off   = i * (int)AppConstants.ChunkSizeBytes;
        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256  = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}

public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer  = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts    = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData   = buffer.ToArray();
    var actualMd5   = SecurityHelper.MD5Hash(imageData);
    var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}

public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}

public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}

private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");

private void Log(string msg) => OnLog?.Invoke(this, msg);

private readonly object _lock = new object();

public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}

private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


// --- Events ---
public event EventHandler<string> OnImageSynced;

public event EventHandler<string> OnImageError;

public event EventHandler<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;

public event Action<ImageSyncItem, string> OnImageSent; // item, atmId

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

public event Action<Exception> OnError;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

}
// Class: ImageSyncEngine (from 2 sources)
public partial class ImageSyncEngine
{
    // --- Constants & Fields ---
    private readonly string _imagesPath;

    private readonly byte[] _sessionKey;


    // --- Properties ---
    public int    ImagesSynced    { get; private set; }

    public long   TotalBytesSent  { get; private set; }


    // --- Constructors ---
    public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
    {
        _imagesPath = imagesPath ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "EJLive", "Images");
        _sessionKey = sessionKey;
        if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
    }


// --- Methods ---
public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data     = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off   = i * (int)AppConstants.ChunkSizeBytes;
        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256  = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}

public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer  = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts    = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData   = buffer.ToArray();
    var actualMd5   = SecurityHelper.MD5Hash(imageData);
    var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}

public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}

public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}

private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");

private void Log(string msg) => OnLog?.Invoke(this, msg);


// --- Events ---
public event EventHandler<string> OnImageSynced;

public event EventHandler<string> OnImageError;

public event EventHandler<string> OnLog;


// --- Nested Classes ---
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}

}
// Class: ImageSyncEngine (from 9 sources)
public partial class ImageSyncEngine
{
    // --- Constants & Fields ---
    private readonly string _imagesPath;

    private readonly byte[] _sessionKey;


    // --- Properties ---
    public int    ImagesSynced    { get; private set; }

    public long   TotalBytesSent  { get; private set; }


    // --- Constructors ---
    public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
    {
        _imagesPath = imagesPath ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "EJLive", "Images");
        _sessionKey = sessionKey;
        if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
    }


// --- Methods ---
public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data     = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off   = i * (int)AppConstants.ChunkSizeBytes;
        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256  = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}

public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer  = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts    = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData   = buffer.ToArray();
    var actualMd5   = SecurityHelper.MD5Hash(imageData);
    var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}

public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}

public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}

private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");

private void Log(string msg) => OnLog?.Invoke(this, msg);


// --- Events ---
public event EventHandler<string> OnImageSynced;

public event EventHandler<string> OnImageError;

public event EventHandler<string> OnLog;


// --- Nested Classes ---
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}

}
// Class: ImageSyncEngine (from 5 sources)
public partial class ImageSyncEngine
{
    // --- Constants & Fields ---
    private readonly string _imagesPath;

    private readonly byte[] _sessionKey;


    // --- Properties ---
    public int    ImagesSynced    { get; private set; }

    public long   TotalBytesSent  { get; private set; }


    // --- Constructors ---
    public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
    {
        _imagesPath = imagesPath ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "EJLive", "Images");
        _sessionKey = sessionKey;
        if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
    }


// --- Methods ---
public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data     = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off   = i * (int)AppConstants.ChunkSizeBytes;
        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256  = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}

public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer  = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts    = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData   = buffer.ToArray();
    var actualMd5   = SecurityHelper.MD5Hash(imageData);
    var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}

public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}

public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}

private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");

private void Log(string msg) => OnLog?.Invoke(this, msg);


// --- Events ---
public event EventHandler<string> OnImageSynced;

public event EventHandler<string> OnImageError;

public event EventHandler<string> OnLog;


// --- Nested Classes ---
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}

}
// ═══ Class: ImageSyncEngine (from 5 sources) ═══
public partial class ImageSyncEngine
{
    // --- Constants & Fields ---
    private readonly string _imagesPath;

    private readonly byte[] _sessionKey;

    private readonly string _imageBasePath;

    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs

    private readonly List<ImageSyncItem> _syncQueue;

    private FileSystemWatcher _watcher;

    private bool _isRunning;

    private Thread _syncThread;


    // --- Properties ---
    public int    ImagesSynced    { get; private set; }

    public long   TotalBytesSent  { get; private set; }

    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";

    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";

    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";

    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";

    public int SyncIntervalMs { get; set; } = 10000;

    public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
    {
        if (!File.Exists(filePath))
        {
            OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
            return;
        }

    var item = new ImageSyncItem;
    {
        ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
        FileName = Path.GetFileName(filePath),
        FilePath = filePath,
        FileSize = new FileInfo(filePath).Length,
        Checksum = ComputeMD5(filePath),
        TargetATMType = targetType,
        TargetATMs = specificATMs ?? new List<string>(),
        ScheduledTime = DateTime.Now,
        Status = ImageSyncStatus.Pending
    };

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}


// --- Constructors ---
public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
{
    _imagesPath = imagesPath ?? Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "EJLive", "Images");
    _sessionKey = sessionKey;
    if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
}

public ImageSyncEngine(string imageBasePath)
{
    _imageBasePath = imageBasePath;
    _atmsByType = new Dictionary<string, List<string>>
    {
        { "NCR", new List<string>() },
    { "GRG", new List<string>() },
{ "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}


// --- Methods ---
public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data     = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off   = i * (int)AppConstants.ChunkSizeBytes;
        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256  = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}

public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer  = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts    = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData   = buffer.ToArray();
    var actualMd5   = SecurityHelper.MD5Hash(imageData);
    var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}

public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}

public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}

private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");

private void Log(string msg) => OnLog?.Invoke(this, msg);

private readonly object _lock = new object();

public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}

private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


// --- Events ---
public event EventHandler<string> OnImageSynced;

public event EventHandler<string> OnImageError;

public event EventHandler<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;

public event Action<ImageSyncItem, string> OnImageSent; // item, atmId

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

public event Action<Exception> OnError;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\ImageSyncEngine.cs
public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error

}
/// <summary>
/// محرك مزامنة الصور الكامل — Image Sync Engine
/// يُزامن: شاشات التوقف، التوجيهات، شاشات تعريفية
/// يطبق: نفس بروتوكول الجورنال المُشفر (START_FILE + CHUNK + COMPLETE)
///        تحقق MD5 + حفظ في مجلد الصور
/// مُستخدم من الخادم → العميل وليس العكس
/// </summary>
public class ImageSyncEngine
{
    private readonly string _imagesPath;
    private readonly byte[] _sessionKey;

    public event EventHandler<string> OnImageSynced;
    public event EventHandler<string> OnImageError;
    public event EventHandler<string> OnLog;

    // إحصائيات
    public int    ImagesSynced    { get; private set; }
    public long   TotalBytesSent  { get; private set; }

    public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
    {
        _imagesPath = imagesPath ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "EJLive", "Images");
        _sessionKey = sessionKey;
        if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
    }

// ==========================================
// إرسال صورة للعميل عبر NetworkStream
// ==========================================

public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data     = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off   = i * (int)AppConstants.ChunkSizeBytes;
        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256  = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}

// ==========================================
// استلام صورة من السيرفر (Client Side)
// ==========================================

public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer  = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts    = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData   = buffer.ToArray();
    var actualMd5   = SecurityHelper.MD5Hash(imageData);
    var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}

// ==========================================
// مزامنة جماعية
// ==========================================

public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}

public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}

private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");

private void Log(string msg) => OnLog?.Invoke(this, msg);

/// <summary>
/// محرك مزامنة الصور - نقل الصور من السرفر إلى الصرافات
/// مشاركة الصور في مجلد السرفر حسب نوع الصرافات
/// مزامنة الصور إلى كل الصرافات حسب مسار ملف الصور ونوعية الصراف
/// </summary>
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
    public Func<string, byte[], string, bool>? DeliveryBridge;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

byte[] payload;
try
{
    payload = File.ReadAllBytes(item.FilePath);
    if (payload.Length == 0)
    {
        item.Status = ImageSyncStatus.Failed;
        OnLog?.Invoke($"[ImageSync] Empty payload for: {item.FileName}");
        return;
    }
}
catch (Exception ex)
{
    item.Status = ImageSyncStatus.Failed;
    OnError?.Invoke(ex);
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    try
    {
        var delivered = DeliveryBridge?.Invoke(atmId, payload, item.FileName) ?? true;
        item.DeliveryStatus[atmId] = delivered;

        if (delivered)
        {
            success++;
            OnImageSent?.Invoke(item, atmId);
        }
    else
    {
        OnImageFailed?.Invoke(item, atmId, "Delivery bridge returned false.");
    }
}
catch (Exception ex)
{
    item.DeliveryStatus[atmId] = false;
    OnImageFailed?.Invoke(item, atmId, ex.Message);
}
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}
}
public sealed class ImageSyncEngine : IDisposable
{
    private readonly string _imagesPath;
    private readonly List<string> _pendingImages = new();
    private bool _isRunning;
    private readonly object _lock = new();

    public event EventHandler<string>? OnLog;
    public event EventHandler<string>? OnImageSynced;
    public event EventHandler<string>? OnImageError;

    public bool IsRunning => _isRunning;

    public ImageSyncEngine()
    {
        _imagesPath = AppConstants.DefaultImagesPath;
        EnsureDirectory();
    }

public ImageSyncEngine(string imagesPath)
{
    _imagesPath = imagesPath ?? AppConstants.DefaultImagesPath;
    EnsureDirectory();
}

public void Start() { _isRunning = true; Log("ImageSync started."); }
public void Stop() { _isRunning = false; Log("ImageSync stopped."); }

public void QueueImage(string filePath)
{
    if (File.Exists(filePath))
    {
        lock (_lock) { _pendingImages.Add(filePath); }
        Log($"Queued image: {Path.GetFileName(filePath)}");
    }
}

public int SyncAll(string targetAtmType)
{
    var count = 0;
    foreach (var img in _pendingImages.ToArray())
    {
        try { count++; OnImageSynced?.Invoke(this, img); }
        catch (Exception ex) { OnImageError?.Invoke(this, ex.Message); }
    }
_pendingImages.Clear();
return count;
}

private void EnsureDirectory() { if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath); }
private void Log(string msg) => OnLog?.Invoke(this, $"[ImageSync] {msg}");
public void Dispose() { Stop(); }
}
/// <summary>
/// محرك مزامنة الصور الكامل — Image Sync Engine
/// يُزامن: شاشات التوقف، التوجيهات، شاشات تعريفية
/// يطبق: نفس بروتوكول الجورنال المُشفر (START_FILE + CHUNK + COMPLETE)
///        تحقق MD5 + حفظ في مجلد الصور
/// مُستخدم من الخادم → العميل وليس العكس
/// </summary>
public class ImageSyncEngine
{
    private readonly string _imagesPath;
    private readonly byte[] _sessionKey;

    public event EventHandler<string> OnImageSynced;
    public event EventHandler<string> OnImageError;
    public event EventHandler<string> OnLog;

    // إحصائيات
    public int ImagesSynced
    {
        get;
        private set;
    }
public long TotalBytesSent
{
    get;
    private set;
}

public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
{
    _imagesPath = imagesPath ?? Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "EJLive", "Images");
    _sessionKey = sessionKey;
    if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
}

// ==========================================
// إرسال صورة للعميل عبر NetworkStream
// ==========================================

public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off = i * (int)AppConstants.ChunkSizeBytes;
        var len = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256 = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}

// ==========================================
// استلام صورة من السيرفر (Client Side)
// ==========================================

public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData = buffer.ToArray();
    var actualMd5 = SecurityHelper.MD5Hash(imageData);
    var verified = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}

// ==========================================
// مزامنة جماعية
// ==========================================

public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] {
        "*.jpg",
        "*.jpeg",
        "*.png",
        "*.bmp",
        "*.gif"
    })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}

public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] {
        "*.jpg",
        "*.jpeg",
        "*.png",
        "*.bmp"
    })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}

private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");

private void Log(string msg) => OnLog?.Invoke(this, msg);

/// <summary>
/// محرك مزامنة الصور - نقل الصور من السرفر إلى الصرافات
/// مشاركة الصور في مجلد السرفر حسب نوع الصرافات
/// مزامنة الصور إلى كل الصرافات حسب مسار ملف الصور ونوعية الصراف
/// </summary>
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath
    {
        get;
        set;
    } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath
    {
        get;
        set;
    } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath
    {
        get;
        set;
    } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath
    {
        get;
        set;
    } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs
    {
        get;
        set;
    } = 10000;
#endregion

#region Constructor
public ImageSyncEngine(string imageBasePath)
{
    _imageBasePath = imageBasePath;
    _atmsByType = new Dictionary<string, List<string>> {
        {
            "NCR",
            new List < string > ()
        },
    {
        "GRG",
        new List < string > ()
    },
{
    "WN",
    new List < string > ()
}
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock)
{
    _syncQueue.Add(item);
}
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop)
    {
        IsBackground = true,
        Name = "ImageSyncThread"
    };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null)
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _watcher = null;
    }
OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock)
    {
        return _syncQueue.ToList();
    }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock)
            {
                pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList();
            }

        foreach (var item in pending)
        {
            if (!_isRunning) break;
            ProcessSyncItem(item);
        }
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}
}
/// <summary>
/// محرك مزامنة الصور الكامل — Image Sync Engine
/// يُزامن: شاشات التوقف، التوجيهات، شاشات تعريفية
/// يطبق: نفس بروتوكول الجورنال المُشفر (START_FILE + CHUNK + COMPLETE)
///        تحقق MD5 + حفظ في مجلد الصور
/// مُستخدم من الخادم → العميل وليس العكس
/// </summary>
public class ImageSyncEngine
{
    private readonly string _imagesPath;
    private readonly byte[] _sessionKey;

    public event EventHandler<string> OnImageSynced;
    public event EventHandler<string> OnImageError;
    public event EventHandler<string> OnLog;

    // إحصائيات
    public int    ImagesSynced    { get; private set; }
    public long   TotalBytesSent  { get; private set; }

    public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
    {
        _imagesPath = imagesPath ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "EJLive", "Images");
        _sessionKey = sessionKey;
        if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
    }

// ==========================================
// إرسال صورة للعميل عبر NetworkStream
// ==========================================

public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data     = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off   = i * (int)AppConstants.ChunkSizeBytes;
        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256  = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}

// ==========================================
// استلام صورة من السيرفر (Client Side)
// ==========================================

public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer  = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts    = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData   = buffer.ToArray();
    var actualMd5   = SecurityHelper.MD5Hash(imageData);
    var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}

// ==========================================
// مزامنة جماعية
// ==========================================

public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}

public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}

private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");

private void Log(string msg) => OnLog?.Invoke(this, msg);

/// <summary>
/// محرك مزامنة الصور - نقل الصور من السرفر إلى الصرافات
/// مشاركة الصور في مجلد السرفر حسب نوع الصرافات
/// مزامنة الصور إلى كل الصرافات حسب مسار ملف الصور ونوعية الصراف
/// </summary>
public class ImageSyncEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
    public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
    public event Action<Exception> OnError;
#endregion

#region Fields
    private readonly string _imageBasePath;
    private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
    private readonly List<ImageSyncItem> _syncQueue;
    private readonly object _lock = new object();
    private FileSystemWatcher _watcher;
    private bool _isRunning;
    private Thread _syncThread;
#endregion

#region Configuration
    public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
    public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
    public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
    public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
    public int SyncIntervalMs { get; set; } = 10000;
#endregion

#region Constructor
    public ImageSyncEngine(string imageBasePath)
    {
        _imageBasePath = imageBasePath;
        _atmsByType = new Dictionary<string, List<string>>
        {
            { "NCR", new List<string>() },
        { "GRG", new List<string>() },
    { "WN", new List<string>() }
};
_syncQueue = new List<ImageSyncItem>();
}
#endregion

#region Public Methods
/// <summary>
/// تسجيل صراف في قائمة المزامنة
/// </summary>
public void RegisterATM(string atmId, string atmType)
{
    lock (_lock)
    {
        if (_atmsByType.ContainsKey(atmType))
        {
            if (!_atmsByType[atmType].Contains(atmId))
            _atmsByType[atmType].Add(atmId);
        }
}
OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
}

/// <summary>
/// إلغاء تسجيل صراف
/// </summary>
public void UnregisterATM(string atmId)
{
    lock (_lock)
    {
        foreach (var list in _atmsByType.Values)
        list.Remove(atmId);
    }
}

/// <summary>
/// إضافة صورة للمزامنة
/// </summary>
public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
{
    if (!File.Exists(filePath))
    {
        OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
        return;
    }

var item = new ImageSyncItem;
{
    ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
    FileName = Path.GetFileName(filePath),
    FilePath = filePath,
    FileSize = new FileInfo(filePath).Length,
    Checksum = ComputeMD5(filePath),
    TargetATMType = targetType,
    TargetATMs = specificATMs ?? new List<string>(),
    ScheduledTime = DateTime.Now,
    Status = ImageSyncStatus.Pending
};

lock (_lock) { _syncQueue.Add(item); }
OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
}

/// <summary>
/// بدء المزامنة
/// </summary>
public void Start()
{
    if (_isRunning) return;
    _isRunning = true;

    // مراقبة مجلد الصور المشتركة
    SetupWatcher();

    // بدء thread المزامنة
    _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
_syncThread.Start();

OnLog?.Invoke("[ImageSync] Engine started");
}

/// <summary>
/// إيقاف المزامنة
/// </summary>
public void Stop()
{
    _isRunning = false;
    if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
    OnLog?.Invoke("[ImageSync] Engine stopped");
}

/// <summary>
/// الحصول على حالة قائمة المزامنة
/// </summary>
public List<ImageSyncItem> GetSyncQueue()
{
    lock (_lock) { return _syncQueue.ToList(); }
}

/// <summary>
/// إرسال صورة لصراف محدد عبر الشبكة
/// </summary>
public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
{
    try
    {
        if (networkEngine == null || !networkEngine.IsConnected)
        {
            OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
            return false;
        }

    string checksum = ComputeMD5Bytes(imageData);
    bool sent = networkEngine.SendFile(fileName, imageData, checksum);
    if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
    return sent;
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return false;
}
}
#endregion

#region Private Methods
private void SetupWatcher()
{
    try
    {
        string watchPath = _imageBasePath;
        if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
    _watcher.Created += Watcher_Created;
    _watcher.EnableRaisingEvents = true;
    OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
}

private void Watcher_Created(object sender, FileSystemEventArgs e)
{
    string ext = Path.GetExtension(e.FullPath).ToLower();
    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
    {
        // تحديد النوع المستهدف من المسار
        string targetType = "ALL";
        if (e.FullPath.Contains("NCR")) targetType = "NCR";
        else if (e.FullPath.Contains("GRG")) targetType = "GRG";
        else if (e.FullPath.Contains("WN")) targetType = "WN";

        Thread.Sleep(500); // انتظار اكتمال الكتابة
        QueueImage(e.FullPath, targetType);
    }
}

private void SyncLoop()
{
    while (_isRunning)
    {
        try
        {
            List<ImageSyncItem> pending;
            lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

            foreach (var item in pending)
            {
                if (!_isRunning) break;
                ProcessSyncItem(item);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(SyncIntervalMs);
}
}

private void ProcessSyncItem(ImageSyncItem item)
{
    item.Status = ImageSyncStatus.Syncing;
    List<string> targetATMs;

    lock (_lock)
    {
        if (item.TargetATMs.Count > 0)
        targetATMs = item.TargetATMs.ToList();
        else if (item.TargetATMType == "ALL")
        targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
        else if (_atmsByType.ContainsKey(item.TargetATMType))
        targetATMs = _atmsByType[item.TargetATMType].ToList();
        else
        targetATMs = new List<string>();
    }

if (targetATMs.Count == 0)
{
    item.Status = ImageSyncStatus.Failed;
    OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
    return;
}

int success = 0;
foreach (string atmId in targetATMs)
{
    // هنا يتم الإرسال الفعلي عبر ServerEngine
    item.DeliveryStatus[atmId] = true; // placeholder - يتم تحديثه من ServerEngine
    success++;
    OnImageSent?.Invoke(item, atmId);
}

item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
}

private string ComputeMD5(string filePath)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    using (var stream = File.OpenRead(filePath))
    {
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

private string ComputeMD5Bytes(byte[] data)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endregion
}
}
/// <summary>
/// محرك مزامنة الصور الكامل — Image Sync Engine
/// يُزامن: شاشات التوقف، التوجيهات، شاشات تعريفية
/// يطبق: نفس بروتوكول الجورنال المُشفر (START_FILE + CHUNK + COMPLETE)
///        تحقق MD5 + حفظ في مجلد الصور
/// مُستخدم من الخادم → العميل وليس العكس
/// </summary>
public class ImageSyncEngine
{
    private readonly string _imagesPath;
    private readonly byte[] _sessionKey;

    public event EventHandler<string> OnImageSynced;
    public event EventHandler<string> OnImageError;
    public event EventHandler<string> OnLog;

    // إحصائيات
    public int    ImagesSynced    { get; private set; }
    public long   TotalBytesSent  { get; private set; }

    public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
    {
        _imagesPath = imagesPath ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "EJLive", "Images");
        _sessionKey = sessionKey;
        if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
    }

// ==========================================
// إرسال صورة للعميل عبر NetworkStream
// ==========================================

public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
{
    if (!File.Exists(imagePath))
    {
        Log($"Image not found: {imagePath}");
        return false;
    }

try
{
    var data     = SecurityHelper.ReadFileSafe(imagePath);
    var checksum = SecurityHelper.MD5Hash(data);
    var fileName = Path.GetFileName(imagePath);

    // START_FILE
    var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
    lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

    // إرسال على أجزاء
    int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
    for (int i = 0; i < totalChunks; i++)
    {
        var off   = i * (int)AppConstants.ChunkSizeBytes;
        var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
        var chunk = new byte[len];
        Buffer.BlockCopy(data, off, chunk, 0, len);
        var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
        lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
        TotalBytesSent += len;
    }

// COMPLETE
var sha256  = SecurityHelper.SHA256Hash(data);
var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
lock (sendLock) stream.Write(complete, 0, complete.Length);

ImagesSynced++;
OnImageSynced?.Invoke(this, imagePath);
Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
return true;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    Log($"✗ Image send failed: {ex.Message}");
    return false;
}
}

// ==========================================
// استلام صورة من السيرفر (Client Side)
// ==========================================

public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
{
    try
    {
        var buffer  = new MemoryStream();
        var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

        for (int i = 0; i < chunkCount; i++)
        {
            var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
            if (!CommunicationProtocol.IsChunk(msg)) return false;
            var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
            buffer.Write(data, 0, data.Length);

            // CHUNK_ACK
            var ack = CommunicationProtocol.BuildChunkAck(seqNum);
            stream.Write(ack, 0, ack.Length);
        }

    // قراءة COMPLETE
    var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
    var parts    = complete.Text.Split('|');
    var checksum = parts.Length > 2 ? parts[2] : "";

    var imageData   = buffer.ToArray();
    var actualMd5   = SecurityHelper.MD5Hash(imageData);
    var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

    if (verified)
    {
        var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
        File.WriteAllBytes(savePath, imageData);
        ImagesSynced++;
        OnImageSynced?.Invoke(this, savePath);
        Log($"✓ Image received & verified: {fileName}");
    }
else
{
    Log($"✗ Checksum mismatch for image: {fileName}");
}

// إرسال ACK
var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
stream.Write(imgAck, 0, imgAck.Length);
return verified;
}
catch (Exception ex)
{
    OnImageError?.Invoke(this, ex.Message);
    return false;
}
}

// ==========================================
// مزامنة جماعية
// ==========================================

public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
{
    if (!Directory.Exists(serverImagesFolder)) return 0;
    int count = 0;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
if (SendImageToClient(stream, file, atmId, sendLock)) count++;
Log($"✓ Image sync complete: {count} images sent to {atmId}");
return count;
}

public List<string> GetLocalImages()
{
    var images = new List<string>();
    if (!Directory.Exists(_imagesPath)) return images;
    foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
images.AddRange(Directory.GetFiles(_imagesPath, ext));
return images;
}

private string SanitizeName(string name) =>
string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");

private void Log(string msg) => OnLog?.Invoke(this, msg);
}
var separator = value.IndexOf('|');
var separator = header.IndexOf(':');
throw new InvalidDataException("Invalid EJLive frame header.");
return !string.IsNullOrWhiteSpace(atmId);
// Enum: MsgType (from 2 sources)
public partial enum MsgType
{
    // --- Constants & Fields ---
    payload = string.Empty;

    if (string.IsNullOrWhiteSpace(value))
    return false;

    if (separator < 0)
    atmId = value;

    if (separator >= 0)
    var tail = value[(separator + 1)..].Trim();

    if (tail.StartsWith("{", StringComparison.Ordinal))
    jsonCandidate = tail;

    if (!jsonCandidate.StartsWith("{", StringComparison.Ordinal))
    try
    if (document.RootElement.ValueKind != JsonValueKind.Object)
    var root = document.RootElement;

    var serverTime = DateTime.UtcNow;

    var pendingCount = 0;

    var requestImmediateSync = false;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
    if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
    serverTime = parsedServerTime.ToUniversalTime();

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
    if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
    pendingCount = Math.Max(0, parsedPending);

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
    if (bool.TryParse(immediateText, out var parsedImmediate))
    requestImmediateSync = parsedImmediate;

    return true;

    catch (JsonException)
    public static byte[] BuildStartFile(string atmId, string fileName, long length, long offset, string checksum) => BuildFrame(MsgType.StartFile, $"{atmId}|{fileName}|{length}|{offset}|{checksum}");

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
    ```

    var frame = new byte[header.Length + body.Length];

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
    return frame;

    var typeName = header[..separator];

    var lengthText = header[(separator + 1)..];

    if (!Enum.TryParse<MsgType>(typeName, out var type))
    type = MsgType.Unknown;

    return null;

    value = default;

    while (true)
    var next = stream.ReadByte();

    if (next == '\n')
    break;

    var offset = 0;

    while (offset < length)
    var read = stream.Read(buffer, offset, length - offset);

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
    offset += read;

    return buffer;

    if (string.IsNullOrWhiteSpace(IssuedAtUtc))
    IssuedAtUtc = DateTime.UtcNow.ToString("O");

    if (string.IsNullOrWhiteSpace(Nonce))
    Nonce = Guid.NewGuid().ToString("N");

    SignatureVersion = CommandSigningEngine.SignatureVersion;

    var payloadBase64 = string.Empty;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
    catch (FormatException)
    command.Payload = payloadBase64;

    if (!CommandSigningEngine.IsFresh(command.IssuedAtUtc, out var freshnessReason))
    command.SignatureFailureReason = freshnessReason;

    if (!CommandSigningEngine.VerifyCanonical(canonical, command.Signature, out var verifyReason))
    command.SignatureFailureReason = verifyReason;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
    command.SignatureVerified = true;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
    command.SignatureVerified = false;

    private readonly string _encryptionIV;

    private readonly bool _useEncryption;

    private readonly bool _useCompression;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
    _encryptionIV = SecurityConfig.DEFAULT_IV;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
    _useEncryption = useEncryption;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
    _useCompression = useCompression;

    string fullMessage = header + "\n" + data + Protocol.DATA_END;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
    string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;

    if (_useEncryption && data.Length > 0)
    data = Decrypt(data);

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.v21_bak
    if (_useCompression && data.Length > 0)
    data = Decompress(data);

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
    if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
    serverTime = parsedServerTime.ToUniversalTime();

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
    if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
    pendingCount = Math.Max(0, parsedPending);

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
    if (bool.TryParse(immediateText, out var parsedImmediate))
    requestImmediateSync = parsedImmediate;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
    catch

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
    return frame;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
    offset += read;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
    catch (FormatException)
    command.Payload = payloadBase64;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
    command.SignatureVerified = true;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
    command.SignatureVerified = false;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
    _encryptionIV = SecurityConfig.DEFAULT_IV;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
    _useEncryption = useEncryption;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
    _useCompression = useCompression;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
    string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\ImageSyncEngine.cs.before_unify
    if (_useCompression && data.Length > 0)
    data = Decompress(data);


    // --- Properties ---
    public string CommandType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public bool RequiresConfirmation { get; set; }

    public string IssuedAtUtc { get; set; } = DateTime.UtcNow.ToString("O");

    public string Nonce { get; set; } = Guid.NewGuid().ToString("N");

    public string Signature { get; set; } = string.Empty;

    public string SignatureVersion { get; set; } = CommandSigningEngine.SignatureVersion;

    public bool SignatureVerified { get; private set; }

    public string SignatureFailureReason { get; private set; } = string.Empty;


    // --- Methods ---
    public static byte[] BuildHandshakeAck(string sessionId) => BuildFrame(MsgType.HandshakeAck, $"OK|{sessionId}");

    public static byte[] BuildHeartbeat(string atmId, string payload = "") => BuildFrame(MsgType.Heartbeat, $"{atmId}|{payload}");

    public static byte[] BuildHeartbeatAck(string atmId = "") => BuildFrame(MsgType.HeartbeatAck, atmId);

    public static byte[] BuildHeartbeatAck(
    string atmId,
    int pendingCommandCount,
    bool requestImmediateSync = false,
    string? serverMessage = null)
    var payload = JsonSerializer.Serialize(new;
    ATM_ID = atmId,
    ServerTimeUtc = serverTimeUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
    PendingCommandCount = Math.Max(0, pendingCommandCount),
    RequestImmediateSync = requestImmediateSync,
    ServerMessage = serverMessage
}
var body = ReadExact(stream, length);
public static EJMessage ReadMessage(Stream stream, byte[]? sessionKey = null)
var header = ReadHeader(stream);
var value = (text ?? string.Empty).Trim();
atmId = value[..separator].Trim();
payload = value[(separator + 1)..].Trim();
public static bool TryParseHeartbeatAck(string text, out HeartbeatAck ack)
ack = new HeartbeatAck(
var jsonCandidate = value;


var serverMessage = ReadJsonValue(root, "ServerMessage", "message", "detail");


var serverTimeText = ReadJsonValue(root, "ServerTimeUtc", "serverTimeUtc", "serverTime", "timestampUtc");


var pendingText = ReadJsonValue(root, "PendingCommandCount", "CommandsPendingCount", "PendingCommands", "pending");


var immediateText = ReadJsonValue(root, "RequestImmediateSync", "ImmediateSync", "requestImmediateSync");


public static byte[] BuildChunk(int sequence, byte[] chunk, byte[]? sessionKey = null) => BuildFrame(MsgType.Chunk, sequence.ToString(), chunk, sessionKey);
public static bool TryParseHeartbeatMessage(string text, out string atmId, out string payload)
atmId = string.Empty;

public class ImageSyncEngine { }
public partial class ImageSyncEventArgs : EventArgs
{
}
}

using var document = JsonDocument.Parse(jsonCandidate);