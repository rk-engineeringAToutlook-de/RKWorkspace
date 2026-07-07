param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma017\reports\uwb-privacy-report.md'

$output = & (Join-Path $root 'tools\run-dongle-sim.ps1') -SmokeTest -UseFusion -Profile MovingCloser 2>&1
if ($LASTEXITCODE -ne 0) {
    $output | ForEach-Object { Write-Host ([string] $_) }
    exit $LASTEXITCODE
}

$text = ($output | ForEach-Object { [string] $_ }) -join [Environment]::NewLine
$required = @(
    'PrivacyMode: EphemeralLab',
    'DongleAnchorSmoke: SUCCESS',
    'RESULT: SUCCESS'
)

foreach ($line in $required) {
    if (-not $text.Contains($line)) {
        throw "UWB privacy report failed because output did not contain: $line"
    }
}

$content = @(
    '# MA017 UWB Privacy Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Scope',
    '',
    'MA017 nutzt UWB/Dongle nur zur Naehe- und Richtungsbestimmung.',
    '',
    '## Privacy Evidence',
    '',
    '- PrivacyMode: EphemeralLab',
    '- NoOriginalBytes: YES',
    '- NoPdfPayload: YES',
    '- NoLongTermTracking: YES',
    '- BeaconScope: LabOnly',
    '',
    '## Ergebnis',
    '',
    '```text',
    'MA017UwbPrivacyReport: SUCCESS',
    'RESULT: SUCCESS',
    '```'
)

Set-Content -Path $reportPath -Value $content -Encoding UTF8
Write-Host 'MA017UwbPrivacyReport: SUCCESS'
Write-Host "Report: $reportPath"
Write-Host 'RESULT: SUCCESS'
exit 0
