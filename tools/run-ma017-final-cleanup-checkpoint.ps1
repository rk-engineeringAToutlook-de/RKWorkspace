param(
    [switch] $SmokeTest,
    [switch] $AllowDirty
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma017\reports\final-cleanup-checkpoint-report.md'

function Test-TextFile {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Label,

        [Parameter(Mandatory = $true)]
        [string] $RelativePath,

        [Parameter(Mandatory = $true)]
        [string[]] $Required
    )

    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path)) {
        throw "$Label missing: $RelativePath"
    }

    $text = Get-Content -LiteralPath $path -Raw
    foreach ($line in $Required) {
        if (-not $text.Contains($line)) {
            throw "$Label missing required text: $line"
        }
    }

    Write-Host "${Label}: READY"
}

function Invoke-Checked {
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

Write-Host 'RK Workspace MA017 Final Cleanup Checkpoint'
Write-Host '-------------------------------------------'

Test-TextFile -Label 'MA017FinalVerificationPlan' -RelativePath 'Docs\Readiness\MA017_FinalVerificationPlan.md' -Required @(
    'MA017FinalVerification: SUCCESS',
    'No File Ingress fuer Capsule und OpenFrame',
    'macOS Handoff Package'
)

Test-TextFile -Label 'MA017FinalNoFileReport' -RelativePath 'release\ma017\reports\final-no-file-report.md' -Required @(
    'NoFileIngress: SUCCESS',
    'GuestHasPdfFile: NO',
    'GuestHasCopiedPdfBytes: NO'
)

Test-TextFile -Label 'MA017FinalSecurityReport' -RelativePath 'release\ma017\reports\final-security-report.md' -Required @(
    'MA017SecurityFinal: SUCCESS',
    'Unauthorized Capsule Open: DENIED',
    'Security Regression: SUCCESS'
)

Test-TextFile -Label 'MA017FinalProximityReport' -RelativePath 'release\ma017\reports\final-proximity-report.md' -Required @(
    'MA017ProximityFinal: SUCCESS',
    'Manual Map Smoke: SUCCESS',
    'Dongle Anchor Simulation: SUCCESS'
)

Test-TextFile -Label 'MA017FinalPdfLifecycleReport' -RelativePath 'release\ma017\reports\final-pdf-lifecycle-report.md' -Required @(
    'MA017PdfLifecycleFinal: SUCCESS',
    'ClosedPdfCapsule',
    'OpenPdfFrame'
)

Test-TextFile -Label 'MA017FinalPlatformHandoffReport' -RelativePath 'release\ma017\reports\final-platform-handoff-report.md' -Required @(
    'MA017PlatformHandoffFinal: SUCCESS',
    'macOS Guest: HANDOFF_READY',
    'iOS/iPadOS Guest: HANDOFF_READY'
)

Test-TextFile -Label 'MA017FinalSmokeReport' -RelativePath 'release\ma017\reports\final-smoke-report.md' -Required @(
    'MA017Smoke: SUCCESS',
    'MA017 Documentation Checkpoint: SUCCESS',
    'Context Pack Secret Scan: SUCCESS'
)

Test-TextFile -Label 'MA017OwnerFinalGuide' -RelativePath 'release\ma017\OWNER_FINAL_GUIDE.md' -Required @(
    'MA017OwnerFinalGuide: READY',
    'Das Original bleibt beim Owner',
    'run-ma017-final-verification.ps1'
)

Invoke-Checked -Label 'MA017 Repository Hygiene' -Command {
    if ($AllowDirty -or $SmokeTest) {
        & (Join-Path $root 'tools\check-ma017-repository-hygiene.ps1') -AllowDirty
    }
    else {
        & (Join-Path $root 'tools\check-ma017-repository-hygiene.ps1')
    }
}

$content = @(
    '# MA017 Final Cleanup Checkpoint Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Ergebnis',
    '',
    'AP731-740 buendeln die finalen MA017-Verifikations- und Cleanup-Artefakte.',
    '',
    '## Nachweise',
    '',
    '- Final Verification Plan.',
    '- Final Smoke Report.',
    '- Final No File Report.',
    '- Final Security Report.',
    '- Final Proximity Report.',
    '- Final PDF Lifecycle Report.',
    '- Final Platform Handoff Report.',
    '- Final Owner Guide.',
    '- Repository Hygiene.',
    '',
    '## Result',
    '',
    '```text',
    'MA017FinalCleanupCheckpoint: SUCCESS',
    'RESULT: SUCCESS',
    '```'
)

Set-Content -Path $reportPath -Value $content -Encoding UTF8

Write-Host 'MA017FinalCleanupCheckpoint: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
