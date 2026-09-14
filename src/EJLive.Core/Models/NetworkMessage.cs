using System.Text.Json;
using EJLive.Core.Enums;

namespace EJLive.Core.Models
{
    /// <summary>
    /// Universal network message for all ATM communication protocols.
    /// Supports chunked file transfers, binary payloads, and JSON serialization.
    /// </summary>
    /// <remarks>
    /// Wave 1 (SS-04 D-08): rewrote the file. The previous version was a four-source
    /// merge dump: three to four copies of <c>NetworkMessage</c>, <c>FileTransferPayload</c>,
    /// <c>ImageSyncPayload</c>, <c>ScreenshotPayload</c> and <c>TimeSyncPayload</c>
    /// interleaved with provenance comments and orphan <c>JsonSerializerOptions</c>
    /// fragments (the trailing <c>PropertyNamingPolicy = ...</c> lines that the merge
    /// tool separated from the enclosing object initialiser). The bodies are identical
    /// across copies, so the union is the original single declaration. The
    /// <c>[Serializable]</c> attribute is kept for compatibility with the older
    /// formatter-based transport adapters still linked in the reference archive.
    /// </remarks>
    [Serializable]
    public class NetworkMessage
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public CommandType Command { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public ATMType ATMType { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Payload { get; set; } = string.Empty;
        public byte[]? BinaryData { get; set; }
        public string Checksum { get; set; } = string.Empty;
        public string SessionToken { get; set; } = string.Empty;
        public int SequenceNumber { get; set; } = 0;
        public int TotalChunks { get; set; } = 1;
        public int CurrentChunk { get; set; } = 0;
        public long FileOffset { get; set; } = 0;

        public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        public static NetworkMessage? FromJson(string json) =>
            JsonSerializer.Deserialize<NetworkMessage>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        public byte[] ToBytes() => System.Text.Encoding.UTF8.GetBytes(ToJson());
    }

    /// <summary>Payload for file transfer initiation and progress.</summary>
    [Serializable]
    public class FileTransferPayload
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; } = 0;
        public string FileHash { get; set; } = string.Empty;
        public JournalFileType FileType { get; set; }
        public int ChunkSize { get; set; } = 65536;
        public bool IsResume { get; set; } = false;
        public long ResumeOffset { get; set; } = 0;
    }

    /// <summary>Payload for image synchronization commands.</summary>
    [Serializable]
    public class ImageSyncPayload
    {
        public string ImageName { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
        public ATMType TargetATMType { get; set; }
        public DateTime ScheduleTime { get; set; }
        public int DisplayDuration { get; set; } = 0;
        public bool IsAd { get; set; } = false;
    }

    /// <summary>Payload for screenshot capture commands.</summary>
    [Serializable]
    public class ScreenshotPayload
    {
        public string Format { get; set; } = "PNG";
        public int Quality { get; set; } = 85;
        public int Width { get; set; } = 1024;
        public int Height { get; set; } = 768;
    }

    /// <summary>Payload for time synchronization commands.</summary>
    [Serializable]
    public class TimeSyncPayload
    {
        public DateTime ServerTime { get; set; } = DateTime.UtcNow;
        public string Timezone { get; set; } = "UTC";
    }

    // HeartbeatPayload and AlertPayload are defined in HandshakeModels.cs and AlertSnapshot.cs.
}
