namespace EJLive.Core.Engine
{
    /// <summary>
    /// Risk classification for remote commands, driving authorization and approval requirements.
    /// </summary>
    public enum CommandRiskLevel
    {
        /// <summary>Low risk operations such as status queries or benign telemetry commands.</summary>
        Low,

        /// <summary>Medium risk operations such as non-destructive configuration reads or screenshots.</summary>
        Medium,

        /// <summary>High risk operations such as service restarts or user-level configuration changes.</summary>
        High,

        /// <summary>Critical risk operations such as system shutdowns, password changes, or firewall modifications.</summary>
        Critical
    }
}
