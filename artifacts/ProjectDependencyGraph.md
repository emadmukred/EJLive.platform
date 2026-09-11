# Project dependency graph

Build order (topological; `dotnet build -m:1` serialises on it):

1. **EJLive.Client.WinForms** -> _none_
2. **EJLive.LegacyReference** -> _none_
3. **EJLive.Shared** -> _none_
4. **EJLive.Core** -> `EJLive.Shared`
5. **EJLive.Business** -> `EJLive.Core`, `EJLive.Shared`
6. **EJLive.Application** -> `EJLive.Business`, `EJLive.Core`, `EJLive.Shared`
7. **EJLive.Client.Service** -> `EJLive.Core`, `EJLive.Shared`
8. **EJLive.Installer.WinForms** -> `EJLive.Application`, `EJLive.Client.Service`, `EJLive.Client.WinForms`
9. **EJLive.Server** -> `EJLive.Core`, `EJLive.Shared`
10. **EJLive.Monitoring.WinForms** -> `EJLive.Core`, `EJLive.Server`, `EJLive.Shared`
11. **EJLive.Server.WinForms** -> `EJLive.Core`, `EJLive.Server`, `EJLive.Shared`
12. **EJLive.Tests** -> `EJLive.Application`, `EJLive.Business`, `EJLive.Client.Service`, `EJLive.Client.WinForms`, `EJLive.Core`, `EJLive.Server`, `EJLive.Server.WinForms`, `EJLive.Shared`
13. **EJLive.UnifiedLauncher** -> `EJLive.Client.WinForms`, `EJLive.Server.WinForms`
14. **EJLive.Verification** -> `EJLive.Application`, `EJLive.Business`, `EJLive.Client.Service`, `EJLive.Client.WinForms`, `EJLive.Core`, `EJLive.Installer.WinForms`, `EJLive.Monitoring.WinForms`, `EJLive.Server`, `EJLive.Server.WinForms`, `EJLive.Shared`

Layers: L0:EJLive.Client.WinForms,EJLive.LegacyReference,EJLive.Shared -> L1:EJLive.Core -> L2:EJLive.Business,EJLive.Client.Service,EJLive.Server -> L3:EJLive.Application,EJLive.Monitoring.WinForms,EJLive.Server.WinForms -> L4:EJLive.Installer.WinForms,EJLive.Tests,EJLive.UnifiedLauncher -> L5:EJLive.Verification

Cycles detected: none (acyclic OK)

## Project identity

| project | tfm | output | forms | files | lines | identity issues |
|---|---|---|---|---|---|---|
| EJLive.Application | net8.0-windows | - | - | 5 | 1818 | ok |
| EJLive.Business | net8.0-windows | - | - | 15 | 1495 | ok |
| EJLive.Client.Service | net8.0-windows | Exe | - | 21 | 4161 | ok |
| EJLive.Client.WinForms | net8.0-windows | WinExe | true | 7 | 1040 | AssemblyName=EJLive.Client |
| EJLive.Core | net8.0-windows | - | true | 175 | 31395 | ok |
| EJLive.Installer.WinForms | net8.0-windows | WinExe | true | 6 | 1200 | AssemblyName=EJLive.Installer |
| EJLive.LegacyReference | net8.0 | - | - | 2 | 22 | ok |
| EJLive.Monitoring.WinForms | net8.0-windows | WinExe | true | 4 | 891 | AssemblyName=EJLive.Monitoring |
| EJLive.Server | net8.0-windows | - | - | 5 | 1359 | ok |
| EJLive.Server.WinForms | net8.0-windows | WinExe | true | 5 | 2545 | ok |
| EJLive.Shared | net8.0-windows | - | - | 12 | 1532 | ok |
| EJLive.Tests | net8.0-windows | - | true | 42 | 8406 | ok |
| EJLive.UnifiedLauncher | net8.0-windows | WinExe | true | 1 | 102 | ok |
| EJLive.Verification | net8.0-windows | Exe | true | 2 | 1344 | ok |

## Demoted csproj shells -- not built, kept for audit (0)


## Unowned .cs under src/ (0)

