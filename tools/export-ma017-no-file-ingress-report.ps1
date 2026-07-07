param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma017\reports\no-file-ingress-report.md'
$lifecyclePilot = Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1'

$closed = & $lifecyclePilot -SmokeTest -ClosedPdf 2>&1
if ($LASTEXITCODE -ne 0) {
    $closed | ForEach-Object { Write-Host ([string] $_) }
    exit $LASTEXITCODE
}

$open = & $lifecyclePilot -SmokeTest -OpenPdf -Page 1 -Zoom 1.25 -ViewerName 'Windows PDF Viewer' 2>&1
if ($LASTEXITCODE -ne 0) {
    $open | ForEach-Object { Write-Host ([string] $_) }
    exit $LASTEXITCODE
}

$closedText = ($closed | ForEach-Object { [string] $_ }) -join [Environment]::NewLine
$openText = ($open | ForEach-Object { [string] $_ }) -join [Environment]::NewLine
$required = @(
    'GuestHasPdfFile: NO',
    'GuestHasOriginalPath: NO',
    'GuestHasCopiedPdfBytes: NO',
    'CapsuleNoFileIngress: SUCCESS',
    'OpenFrameNoFileIngress: SUCCESS',
    'CapsuleCache: MemoryOnly',
    'OpenFrameCache: MemoryOnly',
    'RESULT: SUCCESS'
)

foreach ($line in $required) {
    if (-not $closedText.Contains($line)) {
        throw "Closed PDF No File Ingress failed because output did not contain: $line"
    }

    if (-not $openText.Contains($line)) {
        throw "Open PDF No File Ingress failed because output did not contain: $line"
    }
}

$content = @(
    '# MA017 No File Ingress Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Scope',
    '',
    'MA017 prueft Closed PDF Capsule und Open PDF Frame.',
    '',
    '## Evidence',
    '',
    '- GuestHasPdfFile: NO',
    '- GuestHasOriginalPath: NO',
    '- GuestHasCopiedPdfBytes: NO',
    '- CapsuleNoFileIngress: SUCCESS',
    '- OpenFrameNoFileIngress: SUCCESS',
    '- CapsuleCache: MemoryOnly',
    '- OpenFrameCache: MemoryOnly',
    '',
    '## Ergebnis',
    '',
    '```text',
    'MA017NoFileIngressReport: SUCCESS',
    'RESULT: SUCCESS',
    '```'
)

Set-Content -Path $reportPath -Value $content -Encoding UTF8
Write-Host 'MA017NoFileIngressReport: SUCCESS'
Write-Host "Report: $reportPath"
Write-Host 'RESULT: SUCCESS'
exit 0
