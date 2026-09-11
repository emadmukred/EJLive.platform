using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    /// <summary>
        /// Correlates EJ/JOURNAL + NDC/COM + XFS/SP + device logs
        /// + vendor error codes into a unified evidence event stream,
        /// then reduces to ATMRealTimeStatusSnapshot.
        /// </summary>
        public interface IEvidenceCorrelationEngine
        {
            /// <summary>Accept a parsed journal/EJ event for correlation.</summary>
            Task IngestJournalEventAsync(string atmId, object journalEvent, CancellationToken ct);
    
            /// <summary>Accept a parsed NDC/COM event for correlation.</summary>
            Task IngestNdcEventAsync(string atmId, object ndcEvent, CancellationToken ct);
    
            /// <summary>Accept a parsed XFS/SP event for correlation.</summary>
            Task IngestXfsEventAsync(string atmId, object xfsEvent, CancellationToken ct);
    
            /// <summary>Accept a device log event for correlation.</summary>
            Task IngestDeviceLogEventAsync(string atmId, object deviceEvent, CancellationToken ct);
    
            /// <summary>Accept a vendor error code event for correlation.</summary>
            Task IngestVendorErrorEventAsync(string atmId, string errorCode, string severity, CancellationToken ct);
    
            /// <summary>
            /// Get the latest correlated snapshot for an ATM.
            /// Returns null if no evidence has been ingested yet.
            /// </summary>
            ATMRealTimeStatusSnapshot? GetLatestSnapshot(string atmId);
    
            /// <summary>
            /// Force recomputation of the snapshot for an ATM
            /// from all ingested evidence.
            /// </summary>
            Task<ATMRealTimeStatusSnapshot> RecomputeSnapshotAsync(string atmId, CancellationToken ct);
    
            /// <summary>Fired when a new snapshot is produced.</summary>
            event EventHandler<ATMRealTimeStatusSnapshot>? OnSnapshotUpdated;
    
            /// <summary>List of active ATM IDs currently being correlated.</summary>
            IReadOnlyList<string> ActiveAtmIds { get; }
    
            /// <summary>Gets the count of correlated evidence items for an ATM.</summary>
            long GetEvidenceItemCount(string atmId);
        }
    public partial interface IEvidenceCorrelationEngine
        {
            IReadOnlyList<string> ActiveAtmIds { get; }
    
    
            Task IngestJournalEventAsync(string atmId, object journalEvent, CancellationToken ct);
    
    
            Task IngestNdcEventAsync(string atmId, object ndcEvent, CancellationToken ct);
    
    
            Task IngestXfsEventAsync(string atmId, object xfsEvent, CancellationToken ct);
    
    
            Task IngestDeviceLogEventAsync(string atmId, object deviceEvent, CancellationToken ct);
    
    
            Task IngestVendorErrorEventAsync(string atmId, string errorCode, string severity, CancellationToken ct);
    
    
            ATMRealTimeStatusSnapshot? GetLatestSnapshot(string atmId);
    
    
            Task<ATMRealTimeStatusSnapshot> RecomputeSnapshotAsync(string atmId, CancellationToken ct);
    
    
            long GetEvidenceItemCount(string atmId);
    
    
            event EventHandler<ATMRealTimeStatusSnapshot>? OnSnapshotUpdated;
    
    
        }

    // Interface: IEvidenceCorrelationEngine (from 1 sources)
        public partial interface IEvidenceCorrelationEngine
        {
            // --- Properties ---
                    IReadOnlyList<string> ActiveAtmIds { get; }
    
    
            // --- Methods ---
                    Task IngestJournalEventAsync(string atmId, object journalEvent, CancellationToken ct);
    
                    Task IngestNdcEventAsync(string atmId, object ndcEvent, CancellationToken ct);
    
                    Task IngestXfsEventAsync(string atmId, object xfsEvent, CancellationToken ct);
    
                    Task IngestDeviceLogEventAsync(string atmId, object deviceEvent, CancellationToken ct);
    
                    Task IngestVendorErrorEventAsync(string atmId, string errorCode, string severity, CancellationToken ct);
    
                    ATMRealTimeStatusSnapshot? GetLatestSnapshot(string atmId);
    
                    Task<ATMRealTimeStatusSnapshot> RecomputeSnapshotAsync(string atmId, CancellationToken ct);
    
                    long GetEvidenceItemCount(string atmId);
    
    
            // --- Events ---
                    event EventHandler<ATMRealTimeStatusSnapshot>? OnSnapshotUpdated;
    
    
        }
}
