param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Host 'RK Workspace UWB Simulation'
Write-Host '---------------------------'

& (Join-Path $root 'tools\run-dongle-sim.ps1') -SmokeTest -UseFusion -Profile MovingCloser
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host 'UwbSim: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
