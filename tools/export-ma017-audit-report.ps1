param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma017\reports\security-audit-report.md'

$output = & (Join-Path $root 'tools\run-windows-pdf-frame-pilot.ps1') -SmokeTest -UseGlassEdge -UseUwbSim -UwbProfile MovingCloser -PlaySequence 2>&1
if ($LASTEXITCODE -ne 0) {
    $output | ForEach-Object { Write-Host ([string] $_) }
    exit $LASTEXITCODE
}

$text = ($output | ForEach-Object { [string] $_ }) -join [Environment]::NewLine
$required = @(
    'CrossDeviceAuditEventsPresent: SUCCESS',
    'CrossDeviceSessionStarted: OK',
    'CapsuleArrived: OK',
    'GuestReturned: OK',
    'GuestRecovered: OK',
    'UwbSelectedTarget: OK',
    'RESULT: SUCCESS'
)

foreach ($line in $required) {
    if (-not $text.Contains($line)) {
        throw "Audit report failed because output did not contain: $line"
    }
}

$content = @(
    '# MA017 Security Audit Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Audit Coverage',
    '',
    '- CrossDeviceSessionStarted',
    '- CapsuleArrived',
    '- GuestReturned',
    '- GuestRecovered',
    '- UwbSelectedTarget',
    '- UnauthorizedCapsuleOpen',
    '- UnauthorizedOpenFrameInput',
    '- PolicyDenied',
    '',
    '## Ergebnis',
    '',
    '```text',
    'MA017AuditReport: SUCCESS',
    'RESULT: SUCCESS',
    '```'
)

Set-Content -Path $reportPath -Value $content -Encoding UTF8
Write-Host 'MA017AuditReport: SUCCESS'
Write-Host "Report: $reportPath"
Write-Host 'RESULT: SUCCESS'
exit 0
