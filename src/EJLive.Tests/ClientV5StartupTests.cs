using EJLive.Client.WinForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests;

[TestClass]
public sealed class ClientV5StartupTests
{
    [TestMethod]
    public void StartupPlanner_KeepsDefaultInteractiveClientMode()
    {
        var plan = ClientStartupPlanner.Create(Array.Empty<string>(), isAdministrator: false);

        Assert.AreEqual(ClientStartupMode.Interactive, plan.Mode);
        Assert.IsFalse(plan.RequiresElevation);
        Assert.IsFalse(plan.IsBackground);
        Assert.AreEqual(ClientStartupPlanner.AgentMutexName, plan.MutexName);
    }

    [TestMethod]
    public void StartupPlanner_PromotesV5BackgroundArgumentsWithElevationGate()
    {
        var plan = ClientStartupPlanner.Create(new[] { "--background" }, isAdministrator: false);

        Assert.AreEqual(ClientStartupMode.BackgroundAgent, plan.Mode);
        Assert.IsTrue(plan.RequiresElevation);
        Assert.AreEqual(ClientStartupPlanner.AutoStartArgument, plan.ElevationArguments);
        Assert.AreEqual("EJLive_Agent_v5", plan.MutexName);
    }

    [TestMethod]
    public void StartupPlanner_AcceptsAutostartWhenAlreadyElevated()
    {
        var plan = ClientStartupPlanner.Create(new[] { "--AUTOSTART" }, isAdministrator: true);

        Assert.AreEqual(ClientStartupMode.BackgroundAgent, plan.Mode);
        Assert.IsFalse(plan.RequiresElevation);
        Assert.IsNull(plan.ElevationArguments);
        Assert.IsTrue(plan.IsBackground);
    }
}
