namespace EJLive.Core.Models;

public sealed class JournalEntry
{
    public string EntryId { get; set; } = Guid.NewGuid().ToString("N");
    public string ATMId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long OriginalSize { get; set; }
    public long CompressedSize { get; set; }
    public long EncryptedSize { get; set; }
    public bool IsEncrypted { get; set; } = true;
    public bool IsCompressed { get; set; } = true;
    public string Checksum { get; set; } = string.Empty;
    public string MD5Hash { get; set; } = string.Empty;
    public string SHA256Hash { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public string ArchivePath { get; set; } = string.Empty;
    public string MonthPartition { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime VerifiedAt { get; set; }
}
