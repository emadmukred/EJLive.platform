using System.Reflection;
using System.Runtime.InteropServices;

// The reference archive carries no functional surface: it exists so that the
// demoted legacy tree stays navigable from Solution Explorer. Identity is fixed
// here because Directory.Build.props disables generated assembly info.
[assembly: AssemblyTitle("EJLive.LegacyReference")]
[assembly: AssemblyDescription("Read-only archive of demoted EJLive source; never linked by a runtime project.")]
[assembly: AssemblyCompany("EJLive Enterprise Systems")]
[assembly: AssemblyProduct("EJLive.PLATFORM")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("4.0.0.0")]
[assembly: AssemblyFileVersion("4.0.0.0")]
