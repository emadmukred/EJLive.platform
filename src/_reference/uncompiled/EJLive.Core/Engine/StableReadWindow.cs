using System;
using System.IO;
using System.Text;
using System.Threading;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Reads live journal files with FileShare.ReadWrite, retry on transient IOException,
    /// and stable-read window to avoid returning incomplete trailing lines.
    /// </summary>
    public sealed class StableReadWindow
    {
        private readonly string _filePath;
        private readonly int _maxRetries;
        private readonly TimeSpan _retryDelay;
        private readonly Encoding _encoding;

        public StableReadWindow(string filePath, int maxRetries = 5, int retryDelayMs = 200)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _maxRetries = maxRetries;
            _retryDelay = TimeSpan.FromMilliseconds(retryDelayMs);
            _encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Reads new content from the file starting at the given offset.
        /// Returns only complete lines, leaving the trailing partial line for the next call.
        /// </summary>
        public ReadResult ReadDelta(long startOffset, long stableMillis = 500)
        {
            // Wait briefly to allow the writer to finish the current line
            if (stableMillis > 0)
                Thread.Sleep((int)stableMillis);

            Exception? lastEx = null;
            for (int attempt = 0; attempt < _maxRetries; attempt++)
            {
                try
                {
                    return AttemptRead(startOffset);
                }
                catch (IOException ex)
                {
                    lastEx = ex;
                    Thread.Sleep(_retryDelay);
                }
            }

            throw new IOException($"Failed to read {_filePath} after {_maxRetries} attempts.", lastEx);
        }

        private ReadResult AttemptRead(long startOffset)
        {
            using var stream = new FileStream(
                _filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);

            if (startOffset > stream.Length)
                startOffset = 0; // file was truncated/rotated

            stream.Seek(startOffset, SeekOrigin.Begin);

            var buffer = new byte[stream.Length - startOffset];
            int read = stream.Read(buffer, 0, buffer.Length);

            if (read == 0)
                return new ReadResult(Array.Empty<byte>(), startOffset, startOffset, true);

            // Trim to last newline to ensure stable lines only
            int effectiveLength = read;
            for (int i = read - 1; i >= 0; i--)
            {
                if (buffer[i] == (byte)'\n')
                    break;
                effectiveLength--;
            }

            if (effectiveLength < 0)
                effectiveLength = 0;

            var data = new byte[effectiveLength];
            Buffer.BlockCopy(buffer, 0, data, 0, effectiveLength);

            long newOffset = startOffset + effectiveLength;
            bool isComplete = effectiveLength == read;

            return new ReadResult(data, startOffset, newOffset, isComplete);
        }
    }

    public sealed record ReadResult(
        byte[] Data,
        long StartOffset,
        long EndOffset,
        bool IsComplete);
}
