using System.Text;

namespace EJLive.Core.Services;

/// <summary>
/// Non-blocking journal file reader compatible with ATM vendor runtimes.
/// Uses read/write shared access to avoid locking primary ATM processes.
/// </summary>
public static class JournalReadService
{
    public static bool TryReadAllNonBlocking(
        string filePath,
        out string content,
        Encoding? encoding = null)
    {
        content = string.Empty;
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return false;

        encoding ??= Encoding.UTF8;
        try
        {
            using var stream = OpenSharedRead(filePath);
            using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 64 * 1024, leaveOpen: false);
            content = reader.ReadToEnd();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryReadDeltaNonBlocking(
        string filePath,
        long previousOffset,
        out byte[] delta,
        out long nextOffset,
        int maxAttempts = 3)
    {
        delta = Array.Empty<byte>();
        nextOffset = previousOffset;
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return false;

        maxAttempts = Math.Clamp(maxAttempts, 1, 8);
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var preWriteUtc = File.GetLastWriteTimeUtc(filePath);
                var preLength = new FileInfo(filePath).Length;
                if (preLength <= 0)
                    return false;

                var safeOffset = Math.Clamp(previousOffset, 0, preLength);
                var required = preLength - safeOffset;
                if (required <= 0)
                {
                    nextOffset = preLength;
                    return false;
                }

                byte[] chunk;
                using (var stream = OpenSharedRead(filePath))
                {
                    stream.Seek(safeOffset, SeekOrigin.Begin);
                    chunk = new byte[required];
                    var read = stream.Read(chunk, 0, chunk.Length);
                    if (read != chunk.Length)
                        Array.Resize(ref chunk, read);
                }

                var postWriteUtc = File.GetLastWriteTimeUtc(filePath);
                var postLength = new FileInfo(filePath).Length;
                if (preWriteUtc == postWriteUtc && preLength == postLength)
                {
                    delta = chunk;
                    nextOffset = preLength;
                    return delta.Length > 0;
                }
            }
            catch
            {
                // Best effort read under active writer contention.
            }

            Thread.Sleep(35 * attempt);
        }

        return false;
    }

    private static FileStream OpenSharedRead(string filePath)
    {
        var options = new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.ReadWrite | FileShare.Delete,
            BufferSize = 64 * 1024,
            Options = FileOptions.SequentialScan
        };
        return new FileStream(filePath, options);
    }
}

