// FileTransferManager.cs
// Processed by Code Intelligence Scanner — Smart Merge & Restructure

using System.IO.Compression;
using System.Security.Cryptography;
using EJLive.Core.Enums;
using EJLive.Core.Models;
using EJLive.Core.Security;
using EJLive.Core.Utils;

namespace EJLive.Core.Services
{
    public partial public public class FileTransferManager
    {
        private readonly int _chunkSize;
        private readonly Dictionary<string, TransferState> _activeTransfers = new();
        private readonly object _lock = new();
        public double ProgressPercent => FileSize > 0 ? (BytesTransferred * 100.0 / FileSize) : 0;
        public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;
        public FileTransferManager(int chunkSize = 65536)
        {
            public string TransferId { get; set; }
            public string FilePath { get; set; }
            public string FileName { get; set; }
            public string TargetDeviceId { get; set; }
            public long FileSize { get; set; }
            public long BytesTransferred { get; set; }
            public int TotalChunks { get; set; }
            public int CurrentChunk { get; set; }
            public string FileHash { get; set; }
            public bool IsCompleted { get; set; }
            public bool IsFailed { get; set; }
            public string? ErrorMessage { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public DateTime LastActivity { get; set; }
            public double Progress { get; set; }
            public async Task<TransferState> StartSendAsync(string filePath, string targetDeviceId,
            Func<NetworkMessage, CancellationToken, Task> sendFunc,
            CancellationToken cancellationToken = default)
            {
                public TransferState? GetTransferState(string transferId)
                {
                    public List<TransferState> GetActiveTransfers()
                    {
                        private async Task<string> CalculateFileHashAsync(string filePath, CancellationToken cancellationToken)
                        {
                        }

                    public partial public public class TransferState
                    {
                        public double ProgressPercent => FileSize > 0 ? (BytesTransferred * 100.0 / FileSize) : 0;
                        public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;
                        public string TransferId { get; set; }
                        public string FilePath { get; set; }
                        public string FileName { get; set; }
                        public string TargetDeviceId { get; set; }
                        public long FileSize { get; set; }
                        public long BytesTransferred { get; set; }
                        public int TotalChunks { get; set; }
                        public int CurrentChunk { get; set; }
                        public string FileHash { get; set; }
                        public bool IsCompleted { get; set; }
                        public bool IsFailed { get; set; }
                        public string? ErrorMessage { get; set; }
                        public DateTime StartTime { get; set; }
                        public DateTime? EndTime { get; set; }
                        public DateTime LastActivity { get; set; }
                    }

                public partial public public class TransferProgressEventArgs : EventArgs
                {
                    public string TransferId { get; set; }
                    public double Progress { get; set; }
                }

        }
    public partial public class FileTransferManager
    {
        private readonly int _chunkSize;
        private readonly Dictionary<string, TransferState> _activeTransfers = new();
        private readonly object _lock = new();
        public FileTransferManager(int chunkSize = 65536)
        {
            public async Task<TransferState> StartSendAsync(string filePath, string targetDeviceId,
            Func<NetworkMessage, CancellationToken, Task> sendFunc,
            CancellationToken cancellationToken = default)
            {
                public TransferState? GetTransferState(string transferId)
                {
                    public List<TransferState> GetActiveTransfers()
                    {
                        private async Task<string> CalculateFileHashAsync(string filePath, CancellationToken cancellationToken)
                        {
                        }

                    public partial public class TransferState
                    {
                        public double ProgressPercent => FileSize > 0 ? (BytesTransferred * 100.0 / FileSize) : 0;
                        public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;
                        public string TransferId { get; set; }
                        public string FilePath { get; set; }
                        public string FileName { get; set; }
                        public string TargetDeviceId { get; set; }
                        public long FileSize { get; set; }
                        public long BytesTransferred { get; set; }
                        public int TotalChunks { get; set; }
                        public int CurrentChunk { get; set; }
                        public string FileHash { get; set; }
                        public bool IsCompleted { get; set; }
                        public bool IsFailed { get; set; }
                        public string? ErrorMessage { get; set; }
                        public DateTime StartTime { get; set; }
                        public DateTime? EndTime { get; set; }
                        public DateTime LastActivity { get; set; }
                    }

                public partial public class TransferProgressEventArgs : EventArgs
                {
                    public string TransferId { get; set; }
                    public double Progress { get; set; }
                }

        }
    public class TransferProgressEventArgs : EventArgs
    {
        public string TransferId { get; set; } = string.Empty;
        public double Progress { get; set; }
    }
public class TransferState
{
    public string TransferId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string TargetDeviceId { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public long BytesTransferred { get; set; }
    public int TotalChunks { get; set; }
    public int CurrentChunk { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsFailed { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public double ProgressPercent => FileSize > 0 ? (BytesTransferred * 100.0 / FileSize) : 0;
    public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;
}

public partial class FileTransferManager
{
    private readonly int _chunkSize;


    private readonly Dictionary<string, TransferState> _activeTransfers = new();


    private readonly object _lock = new();


    public FileTransferManager(int chunkSize = 65536)
    _chunkSize = chunkSize;


    public event EventHandler<TransferProgressEventArgs>? ProgressChanged;


    public event EventHandler<TransferState>? TransferCompleted;


    public event EventHandler<string>? TransferError;

}
// Class: FileTransferManager (from 2 sources)
public partial class FileTransferManager
{
    // --- Constants & Fields ---
    private readonly int _chunkSize;

    private readonly Dictionary<string, TransferState> _activeTransfers = new();

    private readonly object _lock = new();


    // --- Constructors ---
    public FileTransferManager(int chunkSize = 65536)
    _chunkSize = chunkSize;


    // --- Events ---
    public event EventHandler<TransferProgressEventArgs>? ProgressChanged;

    public event EventHandler<TransferState>? TransferCompleted;

    public event EventHandler<string>? TransferError;

}
public class FileTransferManager
{
    private readonly int _chunkSize;
    private readonly Dictionary<string, TransferState> _activeTransfers = new();
    private readonly object _lock = new();

    public event EventHandler<TransferProgressEventArgs>? ProgressChanged;
    public event EventHandler<TransferState>? TransferCompleted;
    public event EventHandler<string>? TransferError;

    public FileTransferManager(int chunkSize = 65536)
    {
        _chunkSize = chunkSize;
    }

public async Task<TransferState> StartSendAsync(string filePath, string targetDeviceId,
Func<NetworkMessage, CancellationToken, Task> sendFunc,
CancellationToken cancellationToken = default)
{
    var state = new TransferState;
    {
        TransferId = Guid.NewGuid().ToString(),
        FilePath = filePath,
        FileName = Path.GetFileName(filePath),
        TargetDeviceId = targetDeviceId,
        FileSize = new FileInfo(filePath).Length,
        TotalChunks = (int)Math.Ceiling(new FileInfo(filePath).Length / (double)_chunkSize)
    };

lock (_lock) { _activeTransfers[state.TransferId] = state; }

try
{
    // Calculate file hash
    state.FileHash = await CalculateFileHashAsync(filePath, cancellationToken);

    // Send file start message
    var startMessage = new NetworkMessage;
    {
        Command = CommandType.FileStart,
        DeviceId = targetDeviceId,
        FileName = state.FileName,
        TotalChunks = state.TotalChunks,
        Payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            fileHash = state.FileHash,
            fileSize = state.FileSize,
            chunkSize = _chunkSize
        })
};
await sendFunc(startMessage, cancellationToken);

// Send chunks
using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read,
FileShare.Read, 4096, FileOptions.SequentialScan);

var buffer = new byte[_chunkSize];
int chunkIndex = 0;
int bytesRead;

while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, _chunkSize), cancellationToken)) > 0)
{
    var chunk = bytesRead == _chunkSize ? buffer : buffer[..bytesRead];
    var compressedChunk = SecurityHelper.Compress(chunk);

    var chunkMessage = new NetworkMessage;
    {
        Command = CommandType.FileChunk,
        DeviceId = targetDeviceId,
        FileName = state.FileName,
        CurrentChunk = chunkIndex,
        TotalChunks = state.TotalChunks,
        FileOffset = stream.Position - bytesRead,
        BinaryData = compressedChunk,
        Checksum = SecurityHelper.CalculateMD5(chunk)
    };

await sendFunc(chunkMessage, cancellationToken);

state.CurrentChunk = chunkIndex;
state.BytesTransferred += bytesRead;
state.LastActivity = DateTime.UtcNow;

ProgressChanged?.Invoke(this, new TransferProgressEventArgs
{
    TransferId = state.TransferId,
    Progress = state.ProgressPercent
});

chunkIndex++;
}

// Send file end message
var endMessage = new NetworkMessage;
{
    Command = CommandType.FileEnd,
    DeviceId = targetDeviceId,
    FileName = state.FileName,
    TotalChunks = state.TotalChunks,
    Payload = System.Text.Json.JsonSerializer.Serialize(new { fileHash = state.FileHash })
};
await sendFunc(endMessage, cancellationToken);

state.IsCompleted = true;
state.EndTime = DateTime.UtcNow;
TransferCompleted?.Invoke(this, state);

return state;
}
catch (Exception ex)
{
    state.IsFailed = true;
    state.ErrorMessage = ex.Message;
    TransferError?.Invoke(this, ex.Message);
    throw;
}
}

public TransferState? GetTransferState(string transferId)
{
    lock (_lock)
    {
        return _activeTransfers.GetValueOrDefault(transferId);
    }
}

public List<TransferState> GetActiveTransfers()
{
    lock (_lock)
    {
        return _activeTransfers.Values.Where(t => !t.IsCompleted && !t.IsFailed).ToList();
    }
}

private async Task<string> CalculateFileHashAsync(string filePath, CancellationToken cancellationToken)
{
    await using var stream = File.OpenRead(filePath);
    var hash = await SHA256.HashDataAsync(stream, cancellationToken);
    return Convert.ToHexString(hash);
}
}
// Class: TransferProgressEventArgs (from 2 sources)
public partial class TransferProgressEventArgs : EventArgs
{
}
// Class: TransferState (from 2 sources)
public partial class TransferState
{
}

public partial class TransferProgressEventArgs : EventArgs
{
}
public partial class TransferState
{
}
}