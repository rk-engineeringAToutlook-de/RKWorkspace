param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Host 'RK Workspace Pilot Proximity'
Write-Host '----------------------------'

& (Join-Path $root 'tools\run-ma017-uwb-dongle-pilot.ps1') -SmokeTest
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host 'PilotProximity: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
