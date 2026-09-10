using System;
using System.Collections.Generic;
using EJLive.Core.Models;

namespace EJLive.Core.Services
{
    public sealed class VendorRootProfileCatalogService : VendorRootCapabilityService
    {
        private readonly Dictionary<string, VendorRootProfileSummary> _profiles;

        public VendorRootProfileCatalogService()
        {
            _profiles = new Dictionary<string, VendorRootProfileSummary>(StringComparer.OrdinalIgnoreCase)
            {
                ["NCR"] = new VendorRootProfileSummary
                {
                    VendorName = "NCR",
                    PlatformLineage = VendorPlatformLineage.Hybrid.ToString(),
                    HasFilterIni = true,
                    HasXfsMediaTemplates = false,
                    HasDispenserConfigData = true,
                    HasKeyboardMapData = true,
                    HasKbapeConfig = true,
                    FilterHeaderHint = "ProTopas / NDC-DDC filter with NCR-specific XFS data files",
                    Artifacts = new List<RootConfigArtifact>
                    {
                        new RootConfigArtifact { RelativePath = "PROAGENT/DATA/filter.ini", ArtifactType = "filter.ini", Summary = "Shared event filter catalog" },
                        new RootConfigArtifact { RelativePath = "XFS/data/cdmdata/exp.dat", ArtifactType = "cdm-exp-data", Summary = "Currency exponent configuration" },
                        new RootConfigArtifact { RelativePath = "XFS/data/ttu/dckeymap.dat", ArtifactType = "keyboard-map", Summary = "Operator panel key mapping" },
                        new RootConfigArtifact { RelativePath = "XFS/kbape.cnf", ArtifactType = "kbape-config", Summary = "Keyboard / BAPE capability profile" }
                    }
                },
                ["GRG"] = new VendorRootProfileSummary
                {
                    VendorName = "GRG",
                    PlatformLineage = VendorPlatformLineage.ProTopas_ProView_WOSA.ToString(),
                    HasFilterIni = true,
                    HasXfsMediaTemplates = true,
                    HasDispenserConfigData = false,
                    HasKeyboardMapData = false,
                    HasKbapeConfig = false,
                    FilterHeaderHint = "ProTopas / ProView filter with XFS media templates",
                    Artifacts = new List<RootConfigArtifact>
                    {
                        new RootConfigArtifact { RelativePath = "PROAGENT/DATA/filter.ini", ArtifactType = "filter.ini", Summary = "Shared event filter catalog" },
                        new RootConfigArtifact { RelativePath = "XFS/Media/RPTR/ReceiptPtrMediaDC.wfm", ArtifactType = "xfs-media-template", Summary = "Receipt printer media template" },
                        new RootConfigArtifact { RelativePath = "XFS/Media/SPTR/DocumentPtrMediaDC.wfm", ArtifactType = "xfs-media-template", Summary = "Document printer media template" }
                    }
                },
                ["WN"] = new VendorRootProfileSummary
                {
                    VendorName = "WN",
                    PlatformLineage = VendorPlatformLineage.ProTopas_ProView_WOSA.ToString(),
                    HasFilterIni = true,
                    HasXfsMediaTemplates = false,
                    HasDispenserConfigData = false,
                    HasKeyboardMapData = false,
                    HasKbapeConfig = false,
                    FilterHeaderHint = "Wincor Nixdorf NDC-DDC filter profile",
                    Artifacts = new List<RootConfigArtifact>
                    {
                        new RootConfigArtifact { RelativePath = "PROAGENT/DATA/filter.ini", ArtifactType = "filter.ini", Summary = "WN event filter catalog" }
                    }
                },
                ["DIEBOLD"] = new VendorRootProfileSummary
                {
                    VendorName = "Diebold",
                    PlatformLineage = VendorPlatformLineage.ProTopas_ProView_WOSA.ToString(),
                    HasFilterIni = true,
                    HasXfsMediaTemplates = false,
                    HasDispenserConfigData = false,
                    HasKeyboardMapData = false,
                    HasKbapeConfig = false,
                    FilterHeaderHint = "Diebold root carried through shared ProTopas / WOSA filter lineage",
                    Artifacts = new List<RootConfigArtifact>
                    {
                        new RootConfigArtifact { RelativePath = "PROAGENT/DATA/filter.ini", ArtifactType = "filter.ini", Summary = "Shared event filter catalog" }
                    }
                },
                ["NAUTILUS"] = new VendorRootProfileSummary
                {
                    VendorName = "Nautilus",
                    PlatformLineage = VendorPlatformLineage.ProTopas_ProView_WOSA.ToString(),
                    HasFilterIni = true,
                    HasXfsMediaTemplates = false,
                    HasDispenserConfigData = false,
                    HasKeyboardMapData = false,
                    HasKbapeConfig = false,
                    FilterHeaderHint = "Nautilus root carried through shared ProTopas / WOSA filter lineage",
                    Artifacts = new List<RootConfigArtifact>
                    {
                        new RootConfigArtifact { RelativePath = "PROAGENT/DATA/filter.ini", ArtifactType = "filter.ini", Summary = "Shared event filter catalog" }
                    }
                },
                ["DELARUE"] = new VendorRootProfileSummary
                {
                    VendorName = "DelaRue",
                    PlatformLineage = VendorPlatformLineage.ProTopas_ProView_WOSA.ToString(),
                    HasFilterIni = true,
                    HasXfsMediaTemplates = false,
                    HasDispenserConfigData = false,
                    HasKeyboardMapData = false,
                    HasKbapeConfig = false,
                    FilterHeaderHint = "DeLaRue-specific filter profile for NDC-DDC on Windows NT",
                    Artifacts = new List<RootConfigArtifact>
                    {
                        new RootConfigArtifact { RelativePath = "PROAGENT/DATA/filter.ini", ArtifactType = "filter.ini", Summary = "DeLaRue filter and ProView event mappings" }
                    }
                }
            };
        }

        public VendorRootProfileSummary? Resolve(string vendorOrType)
        {
            if (string.IsNullOrWhiteSpace(vendorOrType))
                return null;

            string key = vendorOrType.Trim();
            if (_profiles.TryGetValue(key, out var profile))
                return profile;

            if (key.Equals("WN", StringComparison.OrdinalIgnoreCase) || key.Contains("WINCOR", StringComparison.OrdinalIgnoreCase))
                return _profiles["WN"];
            if (key.Contains("DIEBOLD", StringComparison.OrdinalIgnoreCase))
                return _profiles["DIEBOLD"];
            if (key.Contains("NAUTILUS", StringComparison.OrdinalIgnoreCase))
                return _profiles["NAUTILUS"];
            if (key.Contains("DELARUE", StringComparison.OrdinalIgnoreCase) || key.Contains("DE LA RUE", StringComparison.OrdinalIgnoreCase))
                return _profiles["DELARUE"];

            return null;
        }
    }
}
