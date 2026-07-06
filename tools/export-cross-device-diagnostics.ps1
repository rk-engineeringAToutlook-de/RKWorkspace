param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportDir = Join-Path $root 'release\ma016\reports'
$reportPath = Join-Path $reportDir 'cross-device-diagnostics.md'

New-Item -ItemType Directory -Force -Path $reportDir | Out-Null

$monitor = & (Join-Path $root 'tools\run-cross-device-session-monitor.ps1') -SmokeTest:$SmokeTest 2>&1
if ($LASTEXITCODE -ne 0) {
    $monitor | ForEach-Object { Write-Output $_ }
    exit $LASTEXITCODE
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
$content = @"
# MA016 Cross-Device Diagnostics

Generated: $timestamp

## Monitor

```text
$($monitor -join [Environment]::NewLine)
```

## Result

CrossDeviceDiagnostics: SUCCESS
RESULT: SUCCESS
"@

Set-Content -Path $reportPath -Value $content -Encoding UTF8
Write-Output 'CrossDeviceDiagnostics: SUCCESS'
Write-Output "Report: $reportPath"
Write-Output 'RESULT: SUCCESS'
exit 0
