using System;
using System.Threading;
using System.Threading.Tasks;

namespace EJLive.Core.Engine
{
    public partial class StreamErrorEventArgs : EventArgs
        {
            public string ATM_ID { get; set; } = string.Empty;
    
    
            public string SourceFile { get; set; } = string.Empty;
    
    
            public Exception Exception { get; set; } = new();
    
    
            public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    /// <summary>Event payload for a stream error.</summary>
        public sealed class StreamErrorEventArgs : EventArgs
        {
            public string ATM_ID { get; set; } = string.Empty;
            public string SourceFile { get; set; } = string.Empty;
            public Exception Exception { get; set; } = new();
            public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        }
    public partial class StreamLineEventArgs : EventArgs
        {
            public string ATM_ID { get; set; } = string.Empty;
    
    
            public string SourceFile { get; set; } = string.Empty;
    
    
            public string RawLine { get; set; } = string.Empty;
    
    
            public long ByteOffset { get; set; }
    
    
            public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    /// <summary>Event payload for a parsed stream line/frame.</summary>
        public sealed class StreamLineEventArgs : EventArgs
        {
            public string ATM_ID { get; set; } = string.Empty;
            public string SourceFile { get; set; } = string.Empty;
            public string RawLine { get; set; } = string.Empty;
            public long ByteOffset { get; set; }
            public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        }

    // Interface: IStreamAnalysisEngine (from 1 sources)
        public partial interface IStreamAnalysisEngine
        {
            // --- Methods ---
                    Task StartAsync(string atmId, CancellationToken cancellationToken);
    
                    Task StopAsync(string atmId, CancellationToken cancellationToken);
    
                    bool IsRunning(string atmId);
    
    
            // --- Events ---
                    event EventHandler<StreamLineEventArgs>? OnLineParsed;
    
                    event EventHandler<StreamErrorEventArgs>? OnError;
    
    
        }
    public partial interface IStreamAnalysisEngine
        {
            Task StartAsync(string atmId, CancellationToken cancellationToken);
    
    
            Task StopAsync(string atmId, CancellationToken cancellationToken);
    
    
            bool IsRunning(string atmId);
    
    
            event EventHandler<StreamLineEventArgs>? OnLineParsed;
    
    
            event EventHandler<StreamErrorEventArgs>? OnError;
    
    
        }
    /// <summary>
        /// .NET Real-Time Stream Analysis Engine contract.
        /// Converts journal/log/NDC/XFS/device evidence into observable event streams.
        /// Implementations use BackgroundService, System.IO.Pipelines,
        /// System.Threading.Channels, and/or TPL Dataflow.
        /// </summary>
        public interface IStreamAnalysisEngine
        {
            /// <summary>Start processing the stream for a given ATM.</summary>
            Task StartAsync(string atmId, CancellationToken cancellationToken);
    
            /// <summary>Stop processing the stream for a given ATM.</summary>
            Task StopAsync(string atmId, CancellationToken cancellationToken);
    
            /// <summary>Check whether the engine is actively processing a stream.</summary>
            bool IsRunning(string atmId);
    
            /// <summary>Fired when a new line/frame is parsed from the stream.</summary>
            event EventHandler<StreamLineEventArgs>? OnLineParsed;
    
            /// <summary>Fired when an error occurs in the stream pipeline.</summary>
            event EventHandler<StreamErrorEventArgs>? OnError;
        }
    // Class: StreamErrorEventArgs (from 3 sources)
        public sealed partial class StreamErrorEventArgs : EventArgs
        {
            // --- Properties ---
                    public string ATM_ID { get; set; } = string.Empty;
    
                    public string SourceFile { get; set; } = string.Empty;
    
                    public Exception Exception { get; set; } = new();
    
                    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: StreamLineEventArgs (from 3 sources)
        public sealed partial class StreamLineEventArgs : EventArgs
        {
            // --- Properties ---
                    public string ATM_ID { get; set; } = string.Empty;
    
                    public string SourceFile { get; set; } = string.Empty;
    
                    public string RawLine { get; set; } = string.Empty;
    
                    public long ByteOffset { get; set; }
    
                    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    
    
        }
}
