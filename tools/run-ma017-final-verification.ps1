param(
    [switch] $SkipHeavy
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Invoke-RkwsFinalStep {
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

Write-Host 'RK Workspace MA017 Final Verification'
Write-Host '-------------------------------------'

if ($SkipHeavy) {
    Invoke-RkwsFinalStep 'MA017 Smoke SkipHeavy' { & (Join-Path $root 'tools\run-ma017-smoke.ps1') -SkipHeavy }
}
else {
    Invoke-RkwsFinalStep 'MA017 Smoke Full' { & (Join-Path $root 'tools\run-ma017-smoke.ps1') }
}

Invoke-RkwsFinalStep 'MA017 Final Cleanup Checkpoint Smoke' {
    & (Join-Path $root 'tools\run-ma017-final-cleanup-checkpoint.ps1') -SmokeTest -AllowDirty
}

Write-Host 'MA017FinalVerification: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
