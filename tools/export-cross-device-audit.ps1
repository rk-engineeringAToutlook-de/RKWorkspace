param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportDir = Join-Path $root 'release\ma016\reports'
$reportPath = Join-Path $reportDir 'cross-device-audit-events.md'

New-Item -ItemType Directory -Force -Path $reportDir | Out-Null

$pilot = & (Join-Path $root 'tools\run-windows-pdf-frame-pilot.ps1') -SmokeTest:$SmokeTest -UseGlassEdge -UseUwbSim -UwbProfile MovingCloser -PlaySequence -Policy TrustedPersonalDevices 2>&1
if ($LASTEXITCODE -ne 0) {
    $pilot | ForEach-Object { Write-Output $_ }
    exit $LASTEXITCODE
}

$text = $pilot -join [Environment]::NewLine
$required = @(
    'CrossDeviceSessionStarted: OK',
    'CapsuleArrived: OK',
    'GuestReturned: OK',
    'GuestRecovered: OK',
    'UwbSelectedTarget: OK',
    'CrossDeviceAuditEventsPresent: SUCCESS',
    'RESULT: SUCCESS'
)

foreach ($line in $required) {
    if (-not $text.Contains($line)) {
        throw "Cross-device audit export failed because output did not contain: $line"
    }
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
$content = @"
# MA016 Cross-Device Audit Events

Generated: $timestamp

## Required events

- CrossDeviceSessionStarted
- CapsuleArrived
- OpenFrameArrived
- GuestReturned
- GuestRecovered
- UwbSelectedTarget

## Current Windows pilot proof

~~~text
$($pilot -join [Environment]::NewLine)
~~~

## Result

CrossDeviceAudit: SUCCESS
RESULT: SUCCESS
"@

Set-Content -Path $reportPath -Value $content -Encoding UTF8
Write-Output 'CrossDeviceAudit: SUCCESS'
Write-Output "Report: $reportPath"
Write-Output 'RESULT: SUCCESS'
exit 0
