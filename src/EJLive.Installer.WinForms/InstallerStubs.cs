namespace EJLive.Core
{
    public partial class DatabaseManager
    {
        public static DatabaseManager Instance { get; } = new();
        public void Initialize(string path) { }
    }

    public partial class AgentConfigurationXmlService
    {
    }

    public partial class WindowsPolicyEnforcer
    {
    }

    public partial class BaselineResult
    {
    }

}
