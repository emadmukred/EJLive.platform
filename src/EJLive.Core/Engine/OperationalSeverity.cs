namespace EJLive.Core.Engine
{
    /// <summary>
    /// Defines the severity taxonomy for operational events in the EJLive system.
    /// </summary>
    public enum OperationalSeverity
    {
        /// <summary>
        /// Detailed diagnostic information intended for development and debugging.
        /// </summary>
        Debug,

        /// <summary>
        /// General informational messages that highlight normal application progress.
        /// </summary>
        Info,

        /// <summary>
        /// Anomalies or unexpected conditions that do not prevent normal operation.
        /// </summary>
        Warning,

        /// <summary>
        /// Failures that prevent a specific operation from completing but may allow the application to continue.
        /// </summary>
        Error,

        /// <summary>
        /// Catastrophic failures that require immediate attention and may halt the application.
        /// </summary>
        Critical
    }
}
