# VENDORS ledger (generated -- do not hand-edit)

Source: `tools/inventory/ejlive_inventory.py`. Regenerate with `python3 tools/inventory/ejlive_inventory.py`.

| vendor | artefacts | compiled parser owners | POL-1 status |
|---|---|---|---|
| CASHWAY | 3 | 2 (`CashwayEjTransactionParser.cs`, `CashwayEjTransactionParserTests.cs`) | VIOLATION (2 compiled parsers) |
| DIEBOLD | 2 | 1 (`DieboldEjTransactionParser.cs`) | OK |
| GRG | 3 | 1 (`GrgEjTransactionParser.cs`) | OK |
| HYOSUNG | 1 | 1 (`HyosungEjTransactionParser.cs`) | OK |
| NCR | 46 | 8 (`CashDistributionParser.cs`, `EjParserRegistry.cs`, `NcrEjTransactionParser.cs`, `NcrConfigCapabilityParser.cs`) | VIOLATION (6 compiled parsers) |
| WINCOR | 1 | 1 (`WincorEjTransactionParser.cs`) | OK |

### CASHWAY
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/CashwayXfsAdapter.cs`
- `parser` [active] `src/EJLive.Core/Engine/CashwayEjTransactionParser.cs`
- `parser` [active] `src/EJLive.Tests/Track09/CashwayEjTransactionParserTests.cs`

### DIEBOLD
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/DieboldMdsAdapter.cs`
- `parser` [active] `src/EJLive.Core/Engine/DieboldEjTransactionParser.cs`

### GRG
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/GrgJournalAdapter.cs`
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/GrgXfsAdapter.cs`
- `parser` [active] `src/EJLive.Core/Engine/GrgEjTransactionParser.cs`

### HYOSUNG
- `parser` [active] `src/EJLive.Core/Engine/HyosungEjTransactionParser.cs`

### NCR
- `adapter` [active] `src/EJLive.Core/Engine/NcrXfsAdapter.cs`
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/CardReaderTraceAdapter.cs`
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/DebugTraceAdapter.cs`
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/HostMessageInAdapter.cs`
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/HostMessageOutAdapter.cs`
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/NcrCardReaderTraceAdapter.cs`
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/NcrDebugTraceAdapter.cs`
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/NcrHostMessageInAdapter.cs`
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/NcrHostMessageOutAdapter.cs`
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/NcrJournalAdapter.cs`
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/NcrOoxfsRuntimeAdapter.cs`
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/NcrXfsAdapter.cs`
- `adapter` [active] `src/EJLive.Core/Xfs/Adapters/OoxfsRuntimeAdapter.cs`
- `adapter` [active] `src/EJLive.Core/Xfs/XfsAdapterRegistry.cs`
- `adapter` [reference] `src/_reference/uncompiled/EJLive.Core/Xfs/IXfsVendorAdapter.cs`
- `auxiliary` [active] `src/EJLive.Application/EJLiveApplicationHost.cs`
- `auxiliary` [active] `src/EJLive.Core/ActiveCore.cs`
- `auxiliary` [active] `src/EJLive.Core/Communication/Protocol.cs`
- `auxiliary` [active] `src/EJLive.Core/Constants.cs`
- `auxiliary` [active] `src/EJLive.Core/Engine/RegressionVerificationGate.cs`
- `auxiliary` [active] `src/EJLive.Core/Engine/ReleaseReadinessChecker.cs`
- `auxiliary` [active] `src/EJLive.Core/Journal/JournalContracts.cs`
- `auxiliary` [active] `src/EJLive.Core/Models/NcrConfigModels.cs`
- `auxiliary` [active] `src/EJLive.Core/Models/NcrConfigurationModels.cs`
- `auxiliary` [active] `src/EJLive.Core/Models/NormalizedVendorEvent.cs`
- `auxiliary` [active] `src/EJLive.Core/Services/MergedTraceCorrelationService.cs`
- `auxiliary` [active] `src/EJLive.Core/Services/NcrReferenceCapabilityFactory.cs`
- `auxiliary` [active] `src/EJLive.Core/Services/UnifiedOperationalFusion.cs`
- `auxiliary` [active] `src/EJLive.Tests/AtmJournalEndToEndTests.cs`
- `auxiliary` [active] `src/EJLive.Tests/ContractTests.cs`
- `auxiliary` [active] `src/EJLive.Tests/PlatformRuntimeIntegrationTests.cs`
- `auxiliary` [active] `src/EJLive.Tests/Track44/RegressionVerificationGateTests.cs`
- `auxiliary` [active] `src/EJLive.Tests/Track45/ReleaseReadinessCheckerTests.cs`
- `auxiliary` [reference] `src/_reference/uncompiled/EJLive.Core/Models/NcrCapabilityModels.cs`
- `auxiliary` [reference] `src/_reference/uncompiled/EJLive.Core/Models/NcrConfigCapabilityModels.cs`
- `auxiliary` [reference] `src/_reference/vs-copies/EJLive.Core/Services/MergedTraceCorrelationService (51).cs`
- `parser` [active] `src/EJLive.Core/Engine/CashDistributionParser.cs`
- `parser` [active] `src/EJLive.Core/Engine/EjParserRegistry.cs`
- `parser` [active] `src/EJLive.Core/Engine/NcrEjTransactionParser.cs`
- `parser` [active] `src/EJLive.Core/Services/NcrConfigCapabilityParser.cs`
- `parser` [active] `src/EJLive.Core/Services/NcrConfigurationCapabilityParser.cs`
- `parser` [active] `src/EJLive.Tests/Track09/NcrEjTransactionParserTests.cs`
- `parser` [reference] `src/_reference/uncompiled/EJLive.Core/Journal/VendorParsers.cs`
- `parser` [reference] `src/_reference/vs-copies/EJLive.Core/Services/NcrConfigCapabilityParser (72).cs`
- `profile` [reference] `src/_reference/uncompiled/EJLive.Core/Models/NcrConfigProfileModels.cs`
- `strategy` [reference] `src/_reference/uncompiled/EJLive.Core/Engine/AtmVendorStrategyRegistry.cs`

### WINCOR
- `parser` [active] `src/EJLive.Core/Engine/WincorEjTransactionParser.cs`

Rule POL-1: one compiled parser per vendor. Additional vendor-specific parsers may exist only as linked reference (map=`reference`) and must not enter a compile map.
