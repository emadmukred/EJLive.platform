using System.Text;

namespace EJLive.Core.Engine;

/// <summary>
/// Reads appended journal bytes with shared access and returns complete lines
/// only, preserving a trailing partial line for the next read.
/// </summary>
public sealed class CompleteLineReadWindow
{
    private readonly string _filePath;
    private readonly int _maxRetries;
    private readonly TimeSpan _retryDelay;

    public CompleteLineReadWindow(string filePath, int maxRetries = 5, int retryDelayMs = 200)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        if (maxRetries < 1)
            throw new ArgumentOutOfRangeException(nameof(maxRetries));
        if (retryDelayMs < 0)
            throw new ArgumentOutOfRangeException(nameof(retryDelayMs));

        _filePath = filePath;
        _maxRetries = maxRetries;
        _retryDelay = TimeSpan.FromMilliseconds(retryDelayMs);
    }

    public ReadResult ReadDelta(long startOffset, long settleMilliseconds = 500)
    {
        if (startOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(startOffset));
        if (settleMilliseconds < 0 || settleMilliseconds > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(settleMilliseconds));

        if (settleMilliseconds > 0)
            Thread.Sleep((int)settleMilliseconds);

        Exception? lastError = null;
        for (var attempt = 0; attempt < _maxRetries; attempt++)
        {
            try
            {
                return ReadAvailableBytes(startOffset);
            }
            catch (IOException ex)
            {
                lastError = ex;
                if (attempt + 1 < _maxRetries && _retryDelay > TimeSpan.Zero)
                    Thread.Sleep(_retryDelay);
            }
        }

        throw new IOException($"Failed to read {_filePath} after {_maxRetries} attempts.", lastError);
    }

    private ReadResult ReadAvailableBytes(long startOffset)
    {
        using var stream = new FileStream(
            _filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);

        if (startOffset > stream.Length)
            startOffset = 0;

        stream.Seek(startOffset, SeekOrigin.Begin);
        var available = checked((int)Math.Min(int.MaxValue, stream.Length - startOffset));
        if (available == 0)
            return new ReadResult(Array.Empty<byte>(), startOffset, startOffset, true);

        var buffer = new byte[available];
        var bytesRead = 0;
        while (bytesRead < buffer.Length)
        {
            var count = stream.Read(buffer, bytesRead, buffer.Length - bytesRead);
            if (count == 0)
                break;
            bytesRead += count;
        }

        var completeLength = FindLastCompleteLineLength(buffer, bytesRead);
        var data = completeLength == 0 ? Array.Empty<byte>() : buffer[..completeLength];
        return new ReadResult(
            data,
            startOffset,
            startOffset + completeLength,
            completeLength == bytesRead);
    }

    private static int FindLastCompleteLineLength(byte[] buffer, int length)
    {
        for (var index = length - 1; index >= 0; index--)
        {
            if (buffer[index] == (byte)'\n')
                return index + 1;
        }
        return 0;
    }
}

public sealed record ReadResult(
    byte[] Data,
    long StartOffset,
    long EndOffset,
    bool IsComplete);
