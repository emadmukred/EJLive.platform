#Requires -Version 5.1
<#
  Canonical build entry point. Serialised (-m:1) on purpose: the solution shares
  generated files between EJLive.Core and EJLive.Shared and parallel node reuse
  races on them (see docs/CI.md "why -m:1").
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
    [string]$Solution = 'EJLive.Platform.slnx',
    [switch]$NoRestore,
    [switch]$WithGate,
    [switch]$WithTests
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Set-Location $root
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error ".NET 8 SDK required (global.json pins 8.0.404, rollForward latestFeature)."
}
if (-not $NoRestore) { dotnet restore $Solution --configfile NuGet.Config; if ($LASTEXITCODE) { exit $LASTEXITCODE } }
dotnet build $Solution -c $Configuration -m:1 /p:BuildInParallel=false
if ($LASTEXITCODE) { exit $LASTEXITCODE }
if ($WithTests) {
    dotnet test src/EJLive.Tests/EJLive.Tests.csproj -c $Configuration --no-build
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
    dotnet run --project src/EJLive.Verification/EJLive.Verification.csproj -c $Configuration --no-build
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}
if ($WithGate) {
    powershell -NoProfile -ExecutionPolicy Bypass -File tools/gates/run-gate.ps1 -SkipDotNet
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}
Write-Host "build: OK ($Configuration)"
