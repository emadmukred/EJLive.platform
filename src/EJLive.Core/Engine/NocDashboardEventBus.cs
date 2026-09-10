namespace EJLive.Core.Engine;

/// <summary>
/// NOC-dashboard specialization of the shared real-time dashboard event bus.
/// </summary>
public sealed class NocDashboardEventBus : DashboardEventBus
{
    public NocDashboardEventBus(int maxEventLogSize = 10000) : base(maxEventLogSize)
    {
    }
}
