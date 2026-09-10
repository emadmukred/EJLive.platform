using System.Text.Json;
using EJLive.Core.Enums;

namespace EJLive.Core.Models
{
    [Serializable]
    public class AlertPayload
    {
        public AlertSeverity Severity { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime AlertTime { get; set; } = DateTime.UtcNow;
        public string? StackTrace { get; set; }
    }
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
    public partial class FileTransferPayload
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
    [Serializable]
    public class HeartbeatPayload
    {
        public ATMStatus Status { get; set; }
        public double LatencyMs { get; set; }
        public string CurrentVersion { get; set; } = string.Empty;
        public DateTime SystemTime { get; set; }
        public Dictionary<string, int> ComponentStatus { get; set; } = new();
        public decimal RemainingCash { get; set; }
        public int RetainedCardsCount { get; set; }
        public string LastJournalFile { get; set; } = string.Empty;
        public long LastJournalSize { get; set; }
    }
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
    public partial class ImageSyncPayload
        {
            public string ImageName { get; set; } = string.Empty;
    
    
            public string TargetPath { get; set; } = string.Empty;
    
    
            public ATMType TargetATMType { get; set; }
    
    
            public DateTime ScheduleTime { get; set; }
    
    
            public int DisplayDuration { get; set; } = 0;
    
    
            public bool IsAd { get; set; } = false;
    
    
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
    public partial class NetworkMessage
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    
    
            WriteIndented = false
    
    
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    
            WriteIndented = false
    
    
            // --- Properties ---
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\NetworkMessage.cs.v22_bak
            public Guid MessageId { get; set; } = Guid.NewGuid();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\NetworkMessage.cs.before_unify
            public Guid MessageId { get; set; } = Guid.NewGuid();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\NetworkMessage.cs
            public Guid MessageId { get; set; } = Guid.NewGuid();
    
    
            public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\NetworkMessage.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\NetworkMessage.cs.v22_bak
            public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\NetworkMessage.cs.before_unify
            public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\NetworkMessage.cs
            public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
    
    
        }
    /// <summary>
    /// Universal network message for all ATM communication protocols.
    /// Supports chunked file transfers, binary payloads, and JSON serialization.
    /// </summary>
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
    [Serializable]
    public class ScreenshotPayload
    {
        public string Format { get; set; } = "PNG";
        public int Quality { get; set; } = 85;
        public int Width { get; set; } = 1024;
        public int Height { get; set; } = 768;
    }
    public partial class ScreenshotPayload
        {
            public string Format { get; set; } = "PNG";
    
    
            public int Quality { get; set; } = 85;
    
    
            public int Width { get; set; } = 1024;
    
    
            public int Height { get; set; } = 768;
    
    
        }
    // HeartbeatPayload and AlertPayload are defined in HandshakeModels.cs and AlertSnapshot.cs
    /// <summary>Payload for screenshot capture commands.</summary>
    [Serializable]
    public class ScreenshotPayload
    {
        public string Format { get; set; } = "PNG";
        public int Quality { get; set; } = 85;
        public int Width { get; set; } = 1024;
        public int Height { get; set; } = 768;
    }
    [Serializable]
    public class TimeSyncPayload
    {
        public DateTime ServerTime { get; set; } = DateTime.UtcNow;
        public string Timezone { get; set; } = "UTC";
    }
    public partial class TimeSyncPayload
        {
            public DateTime ServerTime { get; set; } = DateTime.UtcNow;
    
    
            public string Timezone { get; set; } = "UTC";
    
    
        }
    /// <summary>Payload for time synchronization commands.</summary>
    [Serializable]
    public class TimeSyncPayload
    {
        public DateTime ServerTime { get; set; } = DateTime.UtcNow;
        public string Timezone { get; set; } = "UTC";
    }

    // Class: AlertPayload (from 2 sources)
        public partial class AlertPayload
        {
        }
    // Class: FileTransferPayload (from 3 sources)
        public partial class FileTransferPayload
        {
            // --- Properties ---
                public string FileName { get; set; } = string.Empty;
    
                public string FilePath { get; set; } = string.Empty;
    
                public long FileSize { get; set; } = 0;
    
                public string FileHash { get; set; } = string.Empty;
    
                public JournalFileType FileType { get; set; }
    
                public int ChunkSize { get; set; } = 65536;
    
                public bool IsResume { get; set; } = false;
    
                public long ResumeOffset { get; set; } = 0;
    
    
        }
    // Class: HeartbeatPayload (from 2 sources)
        public partial class HeartbeatPayload
        {
        }
    // Class: ImageSyncPayload (from 3 sources)
        public partial class ImageSyncPayload
        {
            // --- Properties ---
                public string ImageName { get; set; } = string.Empty;
    
                public string TargetPath { get; set; } = string.Empty;
    
                public ATMType TargetATMType { get; set; }
    
                public DateTime ScheduleTime { get; set; }
    
                public int DisplayDuration { get; set; } = 0;
    
                public bool IsAd { get; set; } = false;
    
    
        }
    // Class: NetworkMessage (from 3 sources)
        public partial class NetworkMessage
        {
            // --- Constants & Fields ---
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    
            WriteIndented = false
    
    
            // --- Properties ---
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
    
    
            // --- Methods ---
            public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\NetworkMessage.cs
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
    // Class: ScreenshotPayload (from 3 sources)
        public partial class ScreenshotPayload
        {
            // --- Properties ---
                public string Format { get; set; } = "PNG";
    
                public int Quality { get; set; } = 85;
    
                public int Width { get; set; } = 1024;
    
                public int Height { get; set; } = 768;
    
    
        }
    // Class: TimeSyncPayload (from 3 sources)
        public partial class TimeSyncPayload
        {
            // --- Properties ---
                public DateTime ServerTime { get; set; } = DateTime.UtcNow;
    
                public string Timezone { get; set; } = "UTC";
    
    
        }

    public partial class AlertPayload
        {
        }
    public partial class HeartbeatPayload
        {
        }
}
