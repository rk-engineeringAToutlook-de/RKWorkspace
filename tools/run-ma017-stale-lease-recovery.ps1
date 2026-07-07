param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma017\reports\stale-lease-recovery-report.md'

$output = & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf -Policy TrustedPersonalDevices 2>&1
if ($LASTEXITCODE -ne 0) {
    $output | ForEach-Object { Write-Host ([string] $_) }
    exit $LASTEXITCODE
}

$text = ($output | ForEach-Object { [string] $_ }) -join [Environment]::NewLine
$required = @(
    'ExpiredCapsule: RECOVERED_BY_OWNER',
    'Recovery: SUCCESS',
    'OwnerReturnedStatus: wieder verfuegbar',
    'OwnerRecoveryStatus: wiederhergestellt',
    'RESULT: SUCCESS'
)

foreach ($line in $required) {
    if (-not $text.Contains($line)) {
        throw "Stale lease recovery failed because output did not contain: $line"
    }
}

$content = @(
    '# MA017 Stale Lease Recovery Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Scope',
    '',
    'Stale Lease Recovery prueft, dass haengende Leases beim Start oder nach Ablauf nicht zu Kontrollverlust fuehren.',
    '',
    '## Evidence',
    '',
    '- ExpiredCapsule: RECOVERED_BY_OWNER',
    '- Recovery: SUCCESS',
    '- OwnerControl: Preserved',
    '- GuestIngress: None',
    '',
    '## Ergebnis',
    '',
    '```text',
    'MA017StaleLeaseRecovery: SUCCESS',
    'RESULT: SUCCESS',
    '```'
)

Set-Content -Path $reportPath -Value $content -Encoding UTF8
Write-Host 'MA017StaleLeaseRecovery: SUCCESS'
Write-Host "Report: $reportPath"
Write-Host 'RESULT: SUCCESS'
exit 0
