#Requires -Version 5.1
<#
  EJLIVE.PLATFORM gate runner for Windows developer shells and the build agent.
  Runs the python static gate, the ledger drift checks and (when a .NET SDK is
  present) the build + test + probe sequence. Exit code is the CI verdict.
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
    [switch]$SkipDotNet
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Set-Location $root

function Invoke-Gate([string]$title, [scriptblock]$body) {
    Write-Host ""
    Write-Host "== $title" -ForegroundColor Cyan
    & $body
    if ($LASTEXITCODE -ne 0) { Write-Host "FAILED: $title" -ForegroundColor Red; exit 1 }
    Write-Host "ok: $title" -ForegroundColor Green
}

$py = (Get-Command python -ErrorAction SilentlyContinue).Source
if (-not $py) { $py = (Get-Command python3 -ErrorAction SilentlyContinue).Source }
if (-not $py) { Write-Host "python 3 is required to run the ledgers and the gate" -ForegroundColor Red; exit 1 }

Invoke-Gate 'inventory ledgers' { & $py tools/inventory/ejlive_inventory.py --check }
Invoke-Gate 'service activation ledger' { & $py tools/inventory/service_activation.py --check }
Invoke-Gate 'static gate' { & $py tools/gates/ejlive_static_gate.py }

if (-not $SkipDotNet) {
    $dotnet = (Get-Command dotnet -ErrorAction SilentlyContinue).Source
    if (-not $dotnet) { Write-Host "dotnet not found: build/test skipped (-SkipDotNet implied)" -ForegroundColor Yellow }
    else {
        Invoke-Gate 'build' { & $dotnet build EJLive.Platform.slnx -c $Configuration -m:1 /p:BuildInParallel=false }
        Invoke-Gate 'tests' { & $dotnet test src/EJLive.Tests/EJLive.Tests.csproj -c $Configuration --no-build }
        Invoke-Gate 'verification probes' {
            & $dotnet run --project src/EJLive.Verification/EJLive.Verification.csproj -c $Configuration --no-build
        }
    }
}
Write-Host ""
Write-Host "gate: PASS" -ForegroundColor Green
