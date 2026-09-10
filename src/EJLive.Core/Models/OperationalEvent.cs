using System;
using EJLive.Core.Engine;

namespace EJLive.Core.Models
{
    /// <summary>
    /// Represents a single operational event emitted by the EJLive system.
    /// Carries correlation context, severity, and extensible metadata.
    /// </summary>
    /// <param name="EventId">Unique identifier for this event.</param>
    /// <param name="CorrelationId">Correlation identifier used to trace a request or session across components.</param>
    /// <param name="AtmId">The ATM terminal identifier associated with the event.</param>
    /// <param name="EventType">The type of event (e.g., Sync, Command, Heartbeat, Parser).</param>
    /// <param name="Severity">The severity level of the event.</param>
    /// <param name="Message">A human-readable description of the event.</param>
    /// <param name="TimestampUtc">The UTC timestamp when the event was created.</param>
    /// <param name="MetadataJson">Optional JSON-encoded metadata associated with the event.</param>
    public sealed record OperationalEvent(
        Guid EventId,
        Guid CorrelationId,
        string AtmId,
        string EventType,
        OperationalSeverity Severity,
        string Message,
        DateTime TimestampUtc,
        string MetadataJson);
}
