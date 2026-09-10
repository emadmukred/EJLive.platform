using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

namespace EJLive.Tests;

[TestClass]
public sealed class RegressionGateTests
{
    [TestMethod]
    public void ClientServicePath_HasNoWindowsFormsDependency()
    {
        var serviceAssembly = typeof(EJLive.Client.Service.ClientAgentWindowsService).Assembly;
        var hasWinForms = serviceAssembly.GetReferencedAssemblies().Any(reference =>
            string.Equals(reference.Name, "System.Windows.Forms", StringComparison.OrdinalIgnoreCase));

        Assert.IsFalse(hasWinForms, "Client.Service must remain UI-free.");
    }

    [TestMethod]
    public void ClientServicePath_HasNoDirectUiTypes()
    {
        var serviceAssembly = typeof(EJLive.Client.Service.ClientAgentWindowsService).Assembly;
        var uiTypes = serviceAssembly.GetTypes()
            .Where(type => typeof(Control).IsAssignableFrom(type) || typeof(Form).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .ToArray();

        Assert.AreEqual(0, uiTypes.Length,
            $"Client.Service contains UI types: {string.Join(", ", uiTypes)}.");
    }

    [TestMethod]
    public void ProjectReferences_AreResolvableAndStayInsideSourceTree()
    {
        var root = FindSolutionRoot();
        var sourceRoot = Path.GetFullPath(Path.Combine(root, "src")) + Path.DirectorySeparatorChar;
        var projectFiles = Directory.GetFiles(sourceRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !IsBuildOutput(path))
            .ToArray();

        Assert.IsTrue(projectFiles.Length > 0);
        foreach (var projectFile in projectFiles)
        {
            var document = XDocument.Load(projectFile);
            foreach (var reference in document.Descendants().Where(node => node.Name.LocalName == "ProjectReference"))
            {
                var include = reference.Attribute("Include")?.Value;
                Assert.IsFalse(string.IsNullOrWhiteSpace(include), $"Empty ProjectReference in {projectFile}.");
                var resolved = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectFile)!, include!));
                Assert.IsTrue(resolved.StartsWith(sourceRoot, StringComparison.OrdinalIgnoreCase),
                    $"External ProjectReference: {projectFile} -> {resolved}");
                Assert.IsTrue(File.Exists(resolved), $"Missing ProjectReference: {resolved}");
            }
        }
    }

    [TestMethod]
    public void Solution_ContainsOnlyExistingProjects()
    {
        var root = FindSolutionRoot();
        var solutionPath = Path.Combine(root, "EJLive.Platform.sln");
        var sourceRoot = Path.GetFullPath(Path.Combine(root, "src")) + Path.DirectorySeparatorChar;
        var projectPaths = File.ReadLines(solutionPath)
            .Where(line => line.StartsWith("Project(", StringComparison.Ordinal))
            .Select(line => line.Split(',').ElementAtOrDefault(1)?.Trim().Trim('"'))
            .Where(path => path?.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) == true)
            .Select(path => Path.GetFullPath(Path.Combine(root, path!)))
            .ToArray();

        Assert.IsTrue(projectPaths.Length > 0);
        foreach (var projectPath in projectPaths)
        {
            Assert.IsTrue(projectPath.StartsWith(sourceRoot, StringComparison.OrdinalIgnoreCase),
                $"Solution project is outside src: {projectPath}");
            Assert.IsTrue(File.Exists(projectPath), $"Solution project is missing: {projectPath}");
        }
    }

    [TestMethod]
    public void ProductionNamespace_HasNoUnsafeControlTypes()
    {
        var unsafeTerms = new[] { "Gho" + "st", "Stealth", "Hidden", "Bypass", "DisableDefender", "KillProcess" };
        var productionAssemblies = new[]
        {
            typeof(EJLive.Application.EJLiveApplicationHost).Assembly,
            typeof(EJLive.Business.UnifiedBusinessRuntime).Assembly,
            typeof(EJLive.Core.Constants).Assembly,
            typeof(EJLive.Shared.SecurityHelper).Assembly,
            typeof(EJLive.Client.Service.ClientServiceHost).Assembly,
            typeof(EJLive.Client.WinForms.ClientMainForm).Assembly,
            typeof(EJLive.Server.WinForms.ServerMainForm).Assembly
        };

        var violations = productionAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .SelectMany(type => unsafeTerms
                .Where(term => type.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(term => $"{type.FullName} contains '{term}'"))
            .ToArray();

        Assert.AreEqual(0, violations.Length, string.Join("; ", violations));
    }

    private static string FindSolutionRoot()
    {
        foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "EJLive.Platform.sln")))
                    return directory.FullName;
                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("Could not locate EJLive.Platform.sln.");
    }

    private static bool IsBuildOutput(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
