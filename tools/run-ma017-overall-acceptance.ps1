param(
    [switch] $SkipHeavy
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Invoke-RkwsAcceptanceStep {
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

Write-Host 'RK Workspace MA016+MA017 Overall Acceptance'
Write-Host '-------------------------------------------'

if ($SkipHeavy) {
    Invoke-RkwsAcceptanceStep 'MA016 Smoke SkipHeavy' { & (Join-Path $root 'tools\run-ma016-smoke.ps1') -SkipHeavy }
    Invoke-RkwsAcceptanceStep 'MA017 Smoke SkipHeavy' { & (Join-Path $root 'tools\run-ma017-smoke.ps1') -SkipHeavy }
}
else {
    Invoke-RkwsAcceptanceStep 'MA016 Smoke Full' { & (Join-Path $root 'tools\run-ma016-smoke.ps1') }
    Invoke-RkwsAcceptanceStep 'MA017 Smoke Full' { & (Join-Path $root 'tools\run-ma017-smoke.ps1') }
}

Invoke-RkwsAcceptanceStep 'All Tests' { & (Join-Path $root 'tools\run-tests.ps1') }
Invoke-RkwsAcceptanceStep 'RKWP Tests' { & (Join-Path $root 'tools\run-rkwp-tests.ps1') }
Invoke-RkwsAcceptanceStep 'PDF Frame Smoke' { & (Join-Path $root 'tools\run-pdf-frame-smoke.ps1') }
Invoke-RkwsAcceptanceStep 'Windows PDF Lifecycle Closed' { & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf }
Invoke-RkwsAcceptanceStep 'Windows PDF Lifecycle Open' { & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -OpenPdf }
Invoke-RkwsAcceptanceStep 'Glass Edge Closed PDF Capsule' { & (Join-Path $root 'tools\run-glass-edge-pdf-frame-e2e.ps1') -SmokeTest -ClosedPdfCapsuleTest }
Invoke-RkwsAcceptanceStep 'Glass Edge Open PDF Frame' { & (Join-Path $root 'tools\run-glass-edge-pdf-frame-e2e.ps1') -SmokeTest -OpenPdfFrameTest }
Invoke-RkwsAcceptanceStep 'Pilot Proximity' { & (Join-Path $root 'tools\run-pilot-proximity.ps1') -SmokeTest }
Invoke-RkwsAcceptanceStep 'UWB Simulation' { & (Join-Path $root 'tools\run-uwb-sim.ps1') -SmokeTest }
Invoke-RkwsAcceptanceStep 'Security Regression' { & (Join-Path $root 'tools\run-security-regression.ps1') -SmokeTest }
Invoke-RkwsAcceptanceStep 'Policy Regression' { & (Join-Path $root 'tools\run-policy-regression.ps1') -SmokeTest }
Invoke-RkwsAcceptanceStep 'RKWP Chaos' { & (Join-Path $root 'tools\run-rkwp-chaos.ps1') -SmokeTest }
Invoke-RkwsAcceptanceStep 'Context Export' { & (Join-Path $root 'tools\export-codex-context.ps1') }
Invoke-RkwsAcceptanceStep 'Context Secret Scan' { & (Join-Path $root 'tools\test-context-pack-no-secrets.ps1') -SmokeTest }
Invoke-RkwsAcceptanceStep 'Final Cleanup Checkpoint' { & (Join-Path $root 'tools\run-ma017-final-cleanup-checkpoint.ps1') -SmokeTest -AllowDirty }

Write-Host 'MA017OverallAcceptance: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
