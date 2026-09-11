# Duplicate type declarations in the active compile set

## Intra-assembly duplicates (CS0101 / CS0260 -- fatal, must be zero)

none -- rule ARCH-4 holds

## Cross-assembly partial splits (CS0433 for dual consumers -- tracked debt)

17 keys. Each must appear in `docs/DEBT-LEDGER.md` or the gate fails (ARCH-5).

- `EJLive.Core.Engine.OperationalStateStore`: `EJLive.Core`, `EJLive.Server.WinForms`, `src/EJLive.Core/Models/CoreAdapters.cs`, `src/EJLive.Server.WinForms/Models/ServerModels.cs`
- `EJLive.Core.Engine.ReportExportEngine`: `EJLive.Core`, `EJLive.Server.WinForms`, `src/EJLive.Core/Engine/OperationalEngines.cs`, `src/EJLive.Server.WinForms/Models/ServerModels.cs`
- `EJLive.Core.Engine.ServerEngine`: `EJLive.Core`, `EJLive.Server.WinForms`, `src/EJLive.Core/Engine/OperationalEngines.cs`, `src/EJLive.Server.WinForms/Models/ServerModels.cs`
- `EJLive.Core.Services.AlertManager`: `EJLive.Business`, `EJLive.Core`, `src/EJLive.Business/BusinessAdapters.cs`, `src/EJLive.Core/Services/CoreServices.cs`
- `EJLive.Core.Services.DatabaseManager`: `EJLive.Business`, `EJLive.Core`, `src/EJLive.Business/BusinessAdapters.cs`, `src/EJLive.Core/Services/DatabaseManager.cs`
- `EJLive.Core.Services.JournalSyncService`: `EJLive.Business`, `EJLive.Core`, `src/EJLive.Business/BusinessAdapters.cs`, `src/EJLive.Core/Services/CoreServices.cs`
- `EJLive.Core.Services.JournalSyncTrackingService`: `EJLive.Business`, `EJLive.Core`, `src/EJLive.Business/BusinessAdapters.cs`, `src/EJLive.Core/Services/JournalSyncTrackingService.cs`
- `EJLive.Core.Services.OperationalStateStore`: `EJLive.Business`, `EJLive.Core`, `src/EJLive.Business/BusinessAdapters.cs`, `src/EJLive.Core/Services/OperationalStateStore.cs`
- `EJLive.Core.Services.RoleBasedAccess`: `EJLive.Business`, `EJLive.Core`, `src/EJLive.Business/BusinessAdapters.cs`, `src/EJLive.Core/Services/RoleBasedAccess.cs`
- `EJLive.Core.Services.TransactionAnalysisEngine`: `EJLive.Business`, `EJLive.Core`, `src/EJLive.Business/BusinessAdapters.cs`, `src/EJLive.Core/Models/CoreAdapters.cs`
- `EJLive.Core.Services.UnifiedGatewayActivationBatchResult`: `EJLive.Business`, `EJLive.Core`, `src/EJLive.Business/UnifiedServiceGateway.cs`, `src/EJLive.Core/Services/UnifiedServiceGateway.cs`
- `EJLive.Core.Services.UnifiedGatewayReferenceCoverage`: `EJLive.Business`, `EJLive.Core`, `src/EJLive.Business/UnifiedServiceGateway.cs`, `src/EJLive.Core/Services/UnifiedServiceGateway.cs`
- `EJLive.Core.Services.UnifiedServiceGateway`: `EJLive.Business`, `EJLive.Core`, `src/EJLive.Business/UnifiedServiceGateway.cs`, `src/EJLive.Core/Services/UnifiedServiceGateway.cs`
- `EJLive.Core.Services.VendorRootCapabilityService`: `EJLive.Business`, `EJLive.Core`, `src/EJLive.Business/BusinessAdapters.cs`, `src/EJLive.Core/Services/VendorRootCapabilityService.cs`
- `EJLive.Core.Services.XfsLogAnalysisService`: `EJLive.Business`, `EJLive.Core`, `src/EJLive.Business/BusinessAdapters.cs`, `src/EJLive.Core/Services/XfsLogAnalysisService.cs`
- `EJLive.Shared.AppLogger`: `EJLive.Core`, `EJLive.Shared`, `src/EJLive.Core/Models/CoreAdapters.cs`, `src/EJLive.Shared/AppLogger.cs`
- `EJLive.Shared.SecurityHelper`: `EJLive.Core`, `EJLive.Shared`, `src/EJLive.Core/Models/CoreAdapters.cs`, `src/EJLive.Shared/SecurityHelper.cs`
