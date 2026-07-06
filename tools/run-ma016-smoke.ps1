param(
    [switch] $SkipHeavy
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Invoke-RkwsSmoke {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Label,

        [Parameter(Mandatory = $true)]
        [scriptblock] $Command
    )

    Write-Host "== $Label =="
    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$Label failed with exit code $LASTEXITCODE."
    }
}

Write-Host 'RK Workspace MA016 Smoke'
Write-Host '------------------------'

Invoke-RkwsSmoke 'RKWP Protocol' { & (Join-Path $root 'tools\run-rkwp-tests.ps1') }
Invoke-RkwsSmoke 'PDF Frame' { & (Join-Path $root 'tools\run-pdf-frame-smoke.ps1') }
Invoke-RkwsSmoke 'Windows PDF Pilot' { & (Join-Path $root 'tools\run-windows-pdf-frame-pilot.ps1') -SmokeTest }
Invoke-RkwsSmoke 'Manual Map' { & (Join-Path $root 'tools\run-manual-map.ps1') -SmokeTest }
Invoke-RkwsSmoke 'RKWP Chaos' { & (Join-Path $root 'tools\run-rkwp-chaos.ps1') -SmokeTest }

if (-not $SkipHeavy) {
    Invoke-RkwsSmoke 'RKWP Performance' { & (Join-Path $root 'tools\run-rkwp-perf.ps1') -SmokeTest }
    Invoke-RkwsSmoke 'RKWP Load' { & (Join-Path $root 'tools\run-rkwp-load.ps1') -SmokeTest }
}

Write-Host 'MA016Smoke: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
