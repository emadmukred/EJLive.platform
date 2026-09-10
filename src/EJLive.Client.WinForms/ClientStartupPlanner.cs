namespace EJLive.Client.WinForms;

public enum ClientStartupMode
{
    Interactive,
    BackgroundAgent
}

public sealed record ClientStartupPlan(
    ClientStartupMode Mode,
    bool RequiresElevation,
    string? ElevationArguments,
    string MutexName)
{
    public bool IsBackground => Mode == ClientStartupMode.BackgroundAgent;
}

public static class ClientStartupPlanner
{
    public const string BackgroundArgument = "--background";
    public const string AutoStartArgument = "--autostart";
    public const string CompanionArgument = "--companion";
    public const string AgentMutexName = "EJLive_Agent_v5";

    public static ClientStartupPlan Create(IEnumerable<string>? args, bool isAdministrator)
    {
        var isBackground = ContainsAgentArgument(args);
        var requiresElevation = isBackground && !isAdministrator;

        return new ClientStartupPlan(
            isBackground ? ClientStartupMode.BackgroundAgent : ClientStartupMode.Interactive,
            requiresElevation,
            requiresElevation ? AutoStartArgument : null,
            AgentMutexName);
    }

    public static bool ContainsAgentArgument(IEnumerable<string>? args)
    {
        if (args is null)
            return false;

        return args.Any(arg =>
            string.Equals(arg, BackgroundArgument, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(arg, AutoStartArgument, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(arg, CompanionArgument, StringComparison.OrdinalIgnoreCase));
    }
}
