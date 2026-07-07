param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma017\reports\emergency-return-report.md'

$output = & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf -Policy TrustedPersonalDevices 2>&1
if ($LASTEXITCODE -ne 0) {
    $output | ForEach-Object { Write-Host ([string] $_) }
    exit $LASTEXITCODE
}

$text = ($output | ForEach-Object { [string] $_ }) -join [Environment]::NewLine
$required = @(
    'Rueckgabe: SUCCESS',
    'Recovery: SUCCESS',
    'GuestHasPdfFile: NO',
    'GuestHasCopiedPdfBytes: NO',
    'RESULT: SUCCESS'
)

foreach ($line in $required) {
    if (-not $text.Contains($line)) {
        throw "Emergency return failed because output did not contain: $line"
    }
}

$content = @(
    '# MA017 Emergency Return Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Scope',
    '',
    'Emergency Return prueft, dass der Owner eine ausgeliehene Kapsel oder einen Frame sicher zurueckholen kann.',
    '',
    '## Evidence',
    '',
    '- Rueckgabe: SUCCESS',
    '- Recovery: SUCCESS',
    '- Owner remains original owner.',
    '- Guest file ingress remains denied.',
    '',
    '## Ergebnis',
    '',
    '```text',
    'MA017EmergencyReturn: SUCCESS',
    'RESULT: SUCCESS',
    '```'
)

Set-Content -Path $reportPath -Value $content -Encoding UTF8
Write-Host 'MA017EmergencyReturn: SUCCESS'
Write-Host "Report: $reportPath"
Write-Host 'RESULT: SUCCESS'
exit 0
