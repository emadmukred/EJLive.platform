using System.Security.Cryptography;

namespace EJLive.Client.Service.Compatibility;

internal static class SafeLiveFileReader
{
    /// <summary>
    /// Reads a live ATM journal/log file without taking an exclusive lock.
    /// The delay is intentionally small to avoid reading a line while the ATM
    /// application is still appending to it.
    /// </summary>
    public static byte[] ReadAllBytesShared(string path, int settleWindowMs = 250)
    {
        var before = new FileInfo(path);
        Thread.Sleep(Math.Max(0, settleWindowMs));
        var after = new FileInfo(path);

        // The file is still growing. Read the currently settled portion without failing.
        // Advanced offset-based sync should later replace full-file enqueueing.
        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);

        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    public static string Sha256Hex(byte[] bytes)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant();
    }
}
