using System.Reflection;
using System.Windows.Forms;

namespace EJLive.Core.UI;

/// <summary>
/// Reflection-based <c>DoubleBuffered</c> setter. WinForms exposes the
/// property as <c>protected</c>; the only public way to flip it is to
/// reflect. Pre-wave-4 every form had its own private copy of this
/// helper; this module is the single source.
/// </summary>
public static class ControlRenderingExtensions
{
    private static readonly PropertyInfo? DoubleBufferedProperty =
        typeof(Control).GetProperty("DoubleBuffered",
            BindingFlags.Instance | BindingFlags.NonPublic);

    public static void EnableDoubleBuffering(this Control control)
    {
        DoubleBufferedProperty?.SetValue(control, true, null);
    }
}
