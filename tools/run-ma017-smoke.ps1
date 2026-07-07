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
Invoke-RkwsSmoke 'MA017 Context Export' { & (Join-Path $root 'tools\export-codex-context.ps1') }
Invoke-RkwsSmoke 'MA017 Context Secret Scan' { & (Join-Path $root 'tools\test-context-pack-no-secrets.ps1') -SmokeTest }
Invoke-RkwsSmoke 'MA017 Final Status Guard Smoke' { & (Join-Path $root 'tools\check-ma016-final-status.ps1') -SmokeTest }

Write-Host 'MA017Smoke: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
