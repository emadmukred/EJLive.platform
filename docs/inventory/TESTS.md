# TESTS ledger (generated -- do not hand-edit)

Source: `tools/inventory/ejlive_inventory.py`. Regenerate with `python3 tools/inventory/ejlive_inventory.py`.

| project | class | cases | file |
|---|---|---|---|
| EJLive.Tests | AgentControllerTests | 22 | src/EJLive.Tests/ContractTests.cs |
| EJLive.Tests | AtmJournalEndToEndTests | 10 | src/EJLive.Tests/AtmJournalEndToEndTests.cs |
| EJLive.Tests | CashwayEjTransactionParserTests | 4 | src/EJLive.Tests/Track09/CashwayEjTransactionParserTests.cs |
| EJLive.Tests | ChunkedTransferTests | 4 | src/EJLive.Tests/Track06/ChunkedTransferTests.cs |
| EJLive.Tests | ClientCompanionGatewayTests | 5 | src/EJLive.Tests/Track08/ClientCompanionGatewayTests.cs |
| EJLive.Tests | ClientCompanionStatusTests | 2 | src/EJLive.Tests/Track08/ClientCompanionStatusTests.cs |
| EJLive.Tests | ClientV5StartupTests | 3 | src/EJLive.Tests/ClientV5StartupTests.cs |
| EJLive.Tests | CorrelationEngineTests | 9 | src/EJLive.Tests/Track10/CorrelationEngineTests.cs |
| EJLive.Tests | DashboardSnapshotTests | 11 | src/EJLive.Tests/Track18/DashboardSnapshotTests.cs |
| EJLive.Tests | DatabaseMigrationTests | 6 | src/EJLive.Tests/AdditionalTests.cs |
| EJLive.Tests | DeploymentRollbackGuardTests | 5 | src/EJLive.Tests/Track41/DeploymentRollbackGuardTests.cs |
| EJLive.Tests | FileDistributionTests | 4 | src/EJLive.Tests/Track15/FileDistributionTests.cs |
| EJLive.Tests | FileWatcherOffsetTests | 3 | src/EJLive.Tests/Track05/FileWatcherOffsetTests.cs |
| EJLive.Tests | HeadlessAgentTests | 6 | src/EJLive.Tests/Track03/HeadlessAgentTests.cs |
| EJLive.Tests | IngestionPipelineTests | 2 | src/EJLive.Tests/Track08/IngestionPipelineTests.cs |
| EJLive.Tests | InstallerEngineTests | 8 | src/EJLive.Tests/Track19/InstallerEngineTests.cs |
| EJLive.Tests | JournalOutboxTests | 2 | src/EJLive.Tests/JournalOutboxTests.cs |
| EJLive.Tests | LocalAtmHealthSnapshotTests | 4 | src/EJLive.Tests/Track36/LocalAtmHealthSnapshotTests.cs |
| EJLive.Tests | MonitoringDashboardEventBusTests | 9 | src/EJLive.Tests/Track43/MonitoringDashboardEventBusTests.cs |
| EJLive.Tests | NcrEjTransactionParserTests | 9 | src/EJLive.Tests/Track09/NcrEjTransactionParserTests.cs |
| EJLive.Tests | NocDashboardEventBusTests | 9 | src/EJLive.Tests/Track43/NocDashboardEventBusTests.cs |
| EJLive.Tests | OutboxMaintenanceTests | 2 | src/EJLive.Tests/Track07/OutboxMaintenanceTests.cs |
| EJLive.Tests | ProtocolTests | 3 | src/EJLive.Tests/ProtocolTests.cs |
| EJLive.Tests | RegressionGateTests | 5 | src/EJLive.Tests/RegressionGateTests.cs |
| EJLive.Tests | RegressionVerificationGateTests | 3 | src/EJLive.Tests/Track44/RegressionVerificationGateTests.cs |
| EJLive.Tests | ReleaseReadinessCheckerTests | 5 | src/EJLive.Tests/Track45/ReleaseReadinessCheckerTests.cs |
| EJLive.Tests | RemoteAssistanceTests | 5 | src/EJLive.Tests/Track13/RemoteAssistanceTests.cs |
| EJLive.Tests | RuntimeServiceBehaviorTests | 8 | src/EJLive.Tests/RuntimeServiceBehaviorTests.cs |
| EJLive.Tests | SafeRemoteCommandTests | 10 | src/EJLive.Tests/Track11/SafeRemoteCommandTests.cs |
| EJLive.Tests | ScreenshotTests | 4 | src/EJLive.Tests/Track14/ScreenshotTests.cs |
| EJLive.Tests | SecureHandshakeTests | 5 | src/EJLive.Tests/Track04/SecureHandshakeTests.cs |
| EJLive.Tests | SecurityHardeningTests | 27 | src/EJLive.Tests/Track16/SecurityHardeningTests.cs |
| EJLive.Tests | SecurityHelperTests | 5 | src/EJLive.Tests/SecurityHelperTests.cs |
| EJLive.Tests | StatusReducerContractTests | 7 | src/EJLive.Tests/StatusReducerContractTests.cs |
| EJLive.Tests | StatusSnapshotTests | 10 | src/EJLive.Tests/StatusSnapshotTests.cs |
| EJLive.Tests | StructuredLoggerTests | 12 | src/EJLive.Tests/Track17/StructuredLoggerTests.cs |
| EJLive.Tests | UnifiedRuntimeTests | 88 | src/EJLive.Tests/UnifiedRuntimeTests.cs |
| EJLive.Tests | V5EnhancedDifferenceTests | 15 | src/EJLive.Tests/V5EnhancedDifferenceTests.cs |
| EJLive.Tests | VendorPathRegistryTests | 2 | src/EJLive.Tests/Track08/VendorPathRegistryTests.cs |
| EJLive.Tests | WindowsPolicyEnforcerTests | 10 | src/EJLive.Tests/Track12/WindowsPolicyEnforcerTests.cs |
| EJLive.Tests | verifies | 8 | src/EJLive.Tests/PlatformRuntimeIntegrationTests.cs |

371 executable cases in 41 files. Acceptance gate: `dotnet test` green AND `EJLive.Verification` probes green (see docs/CI.md).
