namespace EJLive.Core.Engine
{
    /// <summary>
    /// Defines how policy changes are applied: observed-only or actively enforced.
    /// </summary>
    public enum PolicyEnforcementMode
    {
        /// <summary>Policy changes are evaluated and logged but not written to the system.</summary>
        Audit,

        /// <summary>Policy changes are actively applied to the system.</summary>
        Enforce
    }
}
