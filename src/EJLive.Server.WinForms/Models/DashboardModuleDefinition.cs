using EJLive.Core.Models;
using System;
using System.Collections.Generic;

namespace EJLive.Server.WinForms.Models
{
    public partial class DashboardModuleDefinition
    {
        public string ModuleKey { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public bool EnabledByDefault { get; set; } = true;
        public int DisplayOrder { get; set; }
        public Type? ModuleType { get; set; }
        public static IReadOnlyList<DashboardModuleDefinition> GetDefaultModules()
        {
            return new[]
            {
                new DashboardModuleDefinition
                {
                    ModuleKey = "fleet_overview",
                    DisplayName = "Fleet Overview",
                    Description = "Real-time fleet status and health summary",
                    Category = "Monitoring",
                    DisplayOrder = 1
                },
                new DashboardModuleDefinition
                {
                    ModuleKey = "sync_status",
                    DisplayName = "Sync Status",
                    Description = "Journal synchronization status across all ATMs",
                    Category = "Monitoring",
                    DisplayOrder = 2
                },
                new DashboardModuleDefinition
                {
                    ModuleKey = "alert_panel",
                    DisplayName = "Alert Panel",
                    Description = "Active alerts and notifications",
                    Category = "Monitoring",
                    DisplayOrder = 3
                },
                new DashboardModuleDefinition
                {
                    ModuleKey = "command_center",
                    DisplayName = "Command Center",
                    Description = "Remote command dispatch and tracking",
                    Category = "Control",
                    DisplayOrder = 4
                },
                new DashboardModuleDefinition
                {
                    ModuleKey = "journal_analytics",
                    DisplayName = "Journal Analytics",
                    Description = "Journal transaction analysis and statistics",
                    Category = "Analytics",
                    DisplayOrder = 5
                },
                new DashboardModuleDefinition
                {
                    ModuleKey = "network_map",
                    DisplayName = "Network Map",
                    Description = "Visual network topology and ATM locations",
                    Category = "Visualization",
                    DisplayOrder = 6
                },
                new DashboardModuleDefinition
                {
                    ModuleKey = "delivery_tracker",
                    DisplayName = "Delivery Tracker",
                    Description = "File and content delivery tracking",
                    Category = "Monitoring",
                    DisplayOrder = 7
                },
                new DashboardModuleDefinition
                {
                    ModuleKey = "ops_analytics",
                    DisplayName = "Operational Analytics",
                    Description = "Operational metrics and performance trends",
                    Category = "Analytics",
                    DisplayOrder = 8
                },
                new DashboardModuleDefinition
                {
                    ModuleKey = "command_audit",
                    DisplayName = "Command Audit",
                    Description = "Audit log of all remote commands",
                    Category = "Security",
                    DisplayOrder = 9
                }
            };
        }
        public override string ToString() => $"{DisplayName} ({ModuleKey})";
    }

}
