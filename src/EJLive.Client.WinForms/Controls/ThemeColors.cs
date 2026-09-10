using System.Drawing;
using EJLive.Core.Models;

namespace EJLive.Client.Controls;

public static class ThemeColors
{
    // Legacy tokens retained for older controls. Defaults are light.
    public static Color BackgroundDark { get; } = Color.FromArgb(24, 28, 32);
    public static Color SurfaceDark { get; } = Color.FromArgb(31, 36, 41);
    public static Color CardDark { get; } = Color.FromArgb(38, 44, 50);
    public static Color TextPrimary { get; } = Color.FromArgb(31, 41, 55);
    public static Color TextSecondary { get; } = Color.FromArgb(71, 85, 105);
    public static Color TextMuted { get; } = Color.FromArgb(100, 116, 139);
    public static Color AccentBlue { get; } = Color.FromArgb(48, 128, 184);
    public static Color AccentGreen { get; } = Color.FromArgb(54, 177, 117);
    public static Color AccentOrange { get; } = Color.FromArgb(220, 139, 58);
    public static Color AccentRed { get; } = Color.FromArgb(218, 83, 83);
    public static Color AccentPurple { get; } = Color.FromArgb(134, 104, 190);
    public static Color BorderDark { get; } = Color.FromArgb(65, 74, 82);
    public static Color HoverDark { get; } = Color.FromArgb(48, 56, 64);

    // Status Colors
    public static Color StatusOnline { get; } = AccentGreen;
    public static Color StatusOffline { get; } = Color.FromArgb(124, 134, 143);
    public static Color StatusWarning { get; } = AccentOrange;
    public static Color StatusCritical { get; } = AccentRed;
    public static Color StatusInService { get; } = AccentBlue;

    // Light Mode
    public static Color BackgroundLight { get; } = Color.FromArgb(246, 248, 250);
    public static Color SurfaceLight { get; } = Color.FromArgb(255, 255, 255);
    public static Color TextDark { get; } = Color.FromArgb(31, 41, 55);
    public static Color BorderLight { get; } = Color.FromArgb(203, 213, 225);

    public static bool IsDarkMode { get; set; } = false;

    public static Color Background => IsDarkMode ? BackgroundDark : BackgroundLight;
    public static Color Surface => IsDarkMode ? SurfaceDark : SurfaceLight;
    public static Color Card => IsDarkMode ? CardDark : Color.White;
    public static Color Foreground => IsDarkMode ? TextPrimary : TextDark;
    public static Color ForegroundSecondary => IsDarkMode ? TextSecondary : Color.Gray;
    public static Color Border => IsDarkMode ? BorderDark : BorderLight;
    public static Color Hover => IsDarkMode ? HoverDark : Color.FromArgb(230, 230, 230);

    public static Color GetStatusColor(ATMStatus status) => status switch
    {
        ATMStatus.Online => StatusOnline,
        ATMStatus.InService => StatusInService,
        ATMStatus.Supervisor => StatusWarning,
        ATMStatus.Fault => StatusCritical,
        ATMStatus.Offline => StatusOffline,
        ATMStatus.Maintenance => AccentPurple,
        _ => StatusOffline
    };

    public static Color GetSyncColor(SyncStatus status) => status switch
    {
        SyncStatus.Completed => StatusOnline,
        SyncStatus.InProgress => StatusInService,
        SyncStatus.Syncing => StatusWarning,
        SyncStatus.Resyncing => AccentOrange,
        SyncStatus.Failed => StatusCritical,
        _ => TextMuted
    };

    public static Color GetSeverityColor(AlertSeverity severity) => severity switch
    {
        AlertSeverity.Info => AccentBlue,
        AlertSeverity.Warning => StatusWarning,
        AlertSeverity.Critical => StatusCritical,
        AlertSeverity.Emergency => AccentRed,
        _ => TextMuted
    };
}
