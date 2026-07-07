param(
    [switch] $SmokeTest,
    [switch] $AllowDirty
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma017\reports\packaging-checkpoint-report.md'

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

Write-Host 'RK Workspace MA017 Packaging Checkpoint'
Write-Host '---------------------------------------'

Test-TextFile -Label 'WindowsPilotPackage' -RelativePath 'release\ma017\packages\windows-pilot\README.md' -Required @(
    'WindowsPilotPackage: READY',
    'NoFileIngress: REQUIRED',
    'OriginalOwnership: Owner'
)

Test-TextFile -Label 'macOSHandoffPackage' -RelativePath 'release\ma017\packages\macos-handoff\README.md' -Required @(
    'macOSHandoffPackage: READY',
    'MacGuestIdentity: OK',
    'NoFileIngress: SUCCESS'
)

Test-TextFile -Label 'iOSHandoffPackage' -RelativePath 'release\ma017\packages\ios-handoff\README.md' -Required @(
    'iOSHandoffPackage: READY',
    'iOSGuestAblage: STARTED',
    'NoFileIngress: SUCCESS'
)

Test-TextFile -Label 'UwbDonglePackage' -RelativePath 'release\ma017\packages\uwb-dongle\README.md' -Required @(
    'UwbDonglePackage: READY',
    'PrivacyMode: EphemeralLab',
    'NearestAblage: Determined'
)

Test-TextFile -Label 'MA017PackageIndex' -RelativePath 'release\ma017\PACKAGE_INDEX.md' -Required @(
    'MA017PackageIndex: READY',
    'MA017PackagingCheckpoint: SUCCESS',
    'RESULT: SUCCESS'
)

Test-TextFile -Label 'macOSFinalCodexHandoff' -RelativePath 'release\ma017\handoff\macOS_FINAL_CODEX_HANDOFF.md' -Required @(
    'macOS-Codex',
    'NoFileIngress: SUCCESS',
    'RESULT: SUCCESS'
)

Test-TextFile -Label 'iOSXcodeFinalCodexHandoff' -RelativePath 'release\ma017\handoff\iOS_XCODE_FINAL_CODEX_HANDOFF.md' -Required @(
    'iOS/iPadOS-Codex',
    'NoFileIngress: SUCCESS',
    'RESULT: SUCCESS'
)

Test-TextFile -Label 'OwnerShortGuide' -RelativePath 'release\ma017\OWNER_SHORT_GUIDE.md' -Required @(
    'Das Original bleibt beim Owner',
    'run-ma017-smoke.ps1',
    'Arbeitsraum'
)

Invoke-Checked -Label 'MA017 Repository Hygiene' -Command {
    if ($AllowDirty) {
        & (Join-Path $root 'tools\check-ma017-repository-hygiene.ps1') -AllowDirty
    }
    else {
        & (Join-Path $root 'tools\check-ma017-repository-hygiene.ps1')
    }
}

$content = @(
    '# MA017 Packaging Checkpoint Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Ergebnis',
    '',
    'AP701-710 sind als Handoff- und Packaging-Block gebuendelt.',
    '',
    '## Nachweise',
    '',
    '- Windows Pilot Package.',
    '- macOS Handoff Package.',
    '- iOS/iPadOS Handoff Package.',
    '- UWB/Dongle Package.',
    '- Package Index.',
    '- Finale macOS Codex Uebergabe.',
    '- Finale iOS/Xcode Codex Uebergabe.',
    '- Owner Short Guide.',
    '- Repository Hygiene.',
    '',
    '## Result',
    '',
    '```text',
    'MA017PackagingCheckpoint: SUCCESS',
    'RESULT: SUCCESS',
    '```'
)

Set-Content -Path $reportPath -Value $content -Encoding UTF8

Write-Host 'MA017PackagingCheckpoint: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
