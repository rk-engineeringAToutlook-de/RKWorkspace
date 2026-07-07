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

Write-Host 'RK Workspace MA017 Smoke'
Write-Host '------------------------'

if ($SkipHeavy) {
    Invoke-RkwsSmoke 'MA016 Verified Baseline' { & (Join-Path $root 'tools\run-ma016-smoke.ps1') -SkipHeavy }
}
else {
    Invoke-RkwsSmoke 'MA016 Verified Baseline' { & (Join-Path $root 'tools\run-ma016-smoke.ps1') }
}

Invoke-RkwsSmoke 'MA017 Windows PDF Standard Pilot' { & (Join-Path $root 'tools\run-ma017-windows-pdf-pilot.ps1') -SmokeTest }
Invoke-RkwsSmoke 'MA017 Security/Policy Pilot' { & (Join-Path $root 'tools\run-ma017-security-policy-pilot.ps1') -SmokeTest }
Invoke-RkwsSmoke 'MA017 macOS Handoff' { & (Join-Path $root 'tools\run-ma017-macos-handoff.ps1') -SmokeTest }
Invoke-RkwsSmoke 'MA017 iOS/iPad Handoff' { & (Join-Path $root 'tools\run-ma017-ios-handoff.ps1') -SmokeTest }
Invoke-RkwsSmoke 'MA017 UWB/Dongle Pilot' { & (Join-Path $root 'tools\run-ma017-uwb-dongle-pilot.ps1') -SmokeTest }
Invoke-RkwsSmoke 'MA017 Cross-Device Session Monitor' { & (Join-Path $root 'tools\run-ma017-cross-device-session-monitor.ps1') -SmokeTest }
Invoke-RkwsSmoke 'MA017 Cross-Device Diagnostics' { & (Join-Path $root 'tools\export-ma017-cross-device-diagnostics.ps1') -SmokeTest }
Invoke-RkwsSmoke 'MA017 Pilot Lab' { & (Join-Path $root 'tools\run-ma017-pilot-lab.ps1') -SmokeTest -SkipRepeatability }
Invoke-RkwsSmoke 'MA017 Human Experience Check' { & (Join-Path $root 'tools\run-ma017-hx-check.ps1') -SmokeTest }
Invoke-RkwsSmoke 'MA017 Security Checkpoint' { & (Join-Path $root 'tools\run-ma017-security-checkpoint.ps1') -SmokeTest -SkipRegression }
Invoke-RkwsSmoke 'MA017 Packaging Checkpoint' { & (Join-Path $root 'tools\run-ma017-packaging-checkpoint.ps1') -SmokeTest -AllowDirty }
Invoke-RkwsSmoke 'MA017 Performance/Stability Checkpoint' { & (Join-Path $root 'tools\run-ma017-performance-stability.ps1') -SmokeTest -SkipMeasurements }
Invoke-RkwsSmoke 'MA017 Documentation Checkpoint' { & (Join-Path $root 'tools\run-ma017-documentation-checkpoint.ps1') -SmokeTest }
Invoke-RkwsSmoke 'MA017 Final Cleanup Checkpoint' { & (Join-Path $root 'tools\run-ma017-final-cleanup-checkpoint.ps1') -SmokeTest -AllowDirty }
Invoke-RkwsSmoke 'MA017 Context Export' { & (Join-Path $root 'tools\export-codex-context.ps1') }
Invoke-RkwsSmoke 'MA017 Context Secret Scan' { & (Join-Path $root 'tools\test-context-pack-no-secrets.ps1') -SmokeTest }
Invoke-RkwsSmoke 'MA017 Final Status Guard Smoke' { & (Join-Path $root 'tools\check-ma016-final-status.ps1') -SmokeTest }

Write-Host 'MA017Smoke: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
