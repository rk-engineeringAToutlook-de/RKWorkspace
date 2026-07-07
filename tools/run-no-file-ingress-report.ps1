param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$reportDir = Join-Path $root 'release\ma016\reports'
$reportPath = Join-Path $reportDir 'no-file-ingress-report.md'
$lifecyclePilot = Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1'

New-Item -ItemType Directory -Force -Path $reportDir | Out-Null

$closed = & $lifecyclePilot -SmokeTest -ClosedPdf 2>&1
if ($LASTEXITCODE -ne 0) {
    $closed | ForEach-Object { Write-Output $_ }
    exit $LASTEXITCODE
}

$open = & $lifecyclePilot -SmokeTest -OpenPdf -Page 1 -Zoom 1.25 -ViewerName 'Windows PDF Viewer' 2>&1
if ($LASTEXITCODE -ne 0) {
    $open | ForEach-Object { Write-Output $_ }
    exit $LASTEXITCODE
}

$closedText = $closed -join [Environment]::NewLine
$openText = $open -join [Environment]::NewLine
$required = @(
    'GuestHasPdfFile: NO',
    'GuestHasOriginalPath: NO',
    'GuestHasCopiedPdfBytes: NO',
    'CapsuleNoFileIngress: SUCCESS',
    'OpenFrameNoFileIngress: SUCCESS',
    'CapsuleCache: MemoryOnly',
    'OpenFrameCache: MemoryOnly',
    'UnauthorizedCapsuleOpen: DENIED',
    'UnauthorizedOpenFrameInput: DENIED',
    'ExpiredCapsule: RECOVERED_BY_OWNER',
    'RESULT: SUCCESS'
)

foreach ($line in $required) {
    if (-not $closedText.Contains($line)) {
        throw "Closed PDF No File Ingress report failed because output did not contain: $line"
    }

    if (-not $openText.Contains($line)) {
        throw "Open PDF No File Ingress report failed because output did not contain: $line"
    }
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
$content = @"
# MA016 No File Ingress Report

Generated: $timestamp

## Scope

This report covers the Windows PDF lifecycle pilot for Closed PDF Capsule and Open PDF Frame.

## Evidence

- Closed PDF Capsule: PASS
- Open PDF Frame: PASS
- Guest has no PDF file: PASS
- Guest has no original path: PASS
- Guest has no copied PDF bytes: PASS
- Capsule cache is MemoryOnly: PASS
- OpenFrame cache is MemoryOnly: PASS
- Unauthorized capsule open is denied: PASS
- Unauthorized OpenFrame input is denied and audited: PASS
- Expired capsule is recovered by owner: PASS

## Commands

~~~powershell
.\tools\run-windows-pdf-lifecycle-pilot.ps1 -SmokeTest -ClosedPdf
.\tools\run-windows-pdf-lifecycle-pilot.ps1 -SmokeTest -OpenPdf -Page 1 -Zoom 1.25 -ViewerName 'Windows PDF Viewer'
~~~

## Result

NoFileIngressReport: SUCCESS
RESULT: SUCCESS
"@

Set-Content -Path $reportPath -Value $content -Encoding UTF8

Write-Output "NoFileIngressReport: SUCCESS"
Write-Output "Report: $reportPath"
Write-Output "RESULT: SUCCESS"
exit 0
