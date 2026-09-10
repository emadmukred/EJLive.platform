using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{
    public enum VendorPlatformLineage
    {
        Unknown,
        NCR_APTRA_XFS,
        ProTopas_ProView_WOSA,
        Hybrid
    }

    public sealed class VendorRootProfile
    {
        public string VendorName { get; set; } = string.Empty;
        public VendorPlatformLineage PlatformLineage { get; set; }
        public bool HasFilterIni { get; set; }
        public bool HasXfsMediaTemplates { get; set; }
        public bool HasDispenserConfigData { get; set; }
        public bool HasKeyboardMapData { get; set; }
        public bool HasKbapeConfig { get; set; }
        public string FilterHeaderHint { get; set; } = string.Empty;
        public List<RootConfigArtifact> Artifacts { get; set; }

        public VendorRootProfile()
        {
            PlatformLineage = VendorPlatformLineage.Unknown;
            Artifacts = new List<RootConfigArtifact>();
        }
    }

    public sealed class RootConfigArtifact
    {
        public string RelativePath { get; set; } = string.Empty;
        public string ArtifactType { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }

    public sealed class FilterRuleDefinition
    {
        public string ProviderId { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public string SourceFile { get; set; } = string.Empty;
    }

    public sealed class XfsMediaTemplateDefinition
    {
        public string LogicalName { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public string SourceFile { get; set; } = string.Empty;
    }

    public sealed class VendorRootProfileSummary
    {
        public string VendorName { get; set; } = string.Empty;
        public string PlatformLineage { get; set; } = string.Empty;
        public bool HasFilterIni { get; set; }
        public bool HasXfsMediaTemplates { get; set; }
        public bool HasDispenserConfigData { get; set; }
        public bool HasKeyboardMapData { get; set; }
        public bool HasKbapeConfig { get; set; }
        public string FilterHeaderHint { get; set; } = string.Empty;
        public List<RootConfigArtifact> Artifacts { get; set; } = new List<RootConfigArtifact>();
    }
}
