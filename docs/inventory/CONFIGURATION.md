# CONFIGURATION ledger (generated -- do not hand-edit)

Source: `tools/inventory/ejlive_inventory.py`. Regenerate with `python3 tools/inventory/ejlive_inventory.py`.

| key | value / default | map | source |
|---|---|---|---|
| BackupPath | cfg.BackupPath   = val; break; | reference | src/_reference/uncompiled/EJLive.Core/Models/TransactionModels.cs |
| BackupPath | cfg.BackupPath = val; break; | reference | src/_reference/uncompiled/EJLive.Core/Models/TransactionModels.cs |
| EJLive.Client.WinForms | [ | reference | artifacts/InventorySummary.json |
| EJLive.Client.WinForms | { | reference | src/EJLive.Client.WinForms/Properties/launchSettings.json |
| EJLive.Installer.WinForms | [ | reference | artifacts/InventorySummary.json |
| EJLive.Monitoring.WinForms | [ | reference | artifacts/InventorySummary.json |
| EJLive.NetworkType | LAN | reference | src/EJLive.Client.WinForms/app.config |
| MaxConnectedATMs | - | active | src/EJLive.Shared/UnifiedSystemConfiguration.cs |
| MaxConnectedATMs | - | reference | src/_reference/exact-duplicates/_reference/orphan-projects/EJLive.Setup/UnifiedSystemConfiguration.cs |
| ServerPort | if (int.TryParse(val | reference | src/_reference/uncompiled/EJLive.Core/Models/TransactionModels.cs |
| SourcePath | cfg.SourcePath   = val; break; | reference | src/_reference/uncompiled/EJLive.Core/Models/TransactionModels.cs |
| SourcePath | cfg.SourcePath = val; break; | reference | src/_reference/uncompiled/EJLive.Core/Models/TransactionModels.cs |
| backupJournalPath | config.BackupJournalPath = value; break; | active | src/EJLive.Core/Config/AgentConfiguration.cs |
| imageDestinationPath | config.ImageDestinationPath = value; break; | active | src/EJLive.Core/Config/AgentConfiguration.cs |
| imageInboxPath | config.ImageInboxPath = value; break; | active | src/EJLive.Core/Config/AgentConfiguration.cs |
| serverPort | if (int.TryParse(value | active | src/EJLive.Core/Config/AgentConfiguration.cs |
| sourceJournalPath | config.SourceJournalPath = value; break; | active | src/EJLive.Core/Config/AgentConfiguration.cs |

Configuration is file-backed (no env-var magic): every key above has a documented default, a validator and a redaction class. Secrets never appear here (SEC-3).
