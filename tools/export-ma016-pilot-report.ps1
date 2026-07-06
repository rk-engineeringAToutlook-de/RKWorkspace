param(
    [switch] $SmokeTest,
    [string] $ConfigPath,
    [string] $OutputPath
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($ConfigPath)) {
    $ConfigPath = Join-Path $root 'release\ma016\config\windows-local.sample.json'
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $root 'release\ma016\reports\ma016-pilot-report.md'
}

if (-not (Test-Path $ConfigPath)) {
    throw "Pilot config not found: $ConfigPath"
}

$config = Get-Content -Path $ConfigPath -Raw | ConvertFrom-Json
$presetName = if ($null -ne $config.scenario -and -not [string]::IsNullOrWhiteSpace($config.scenario)) {
    $config.scenario
}
else {
    $config.preset
}
$monitor = & (Join-Path $root 'tools\run-cross-device-session-monitor.ps1') -SmokeTest:$SmokeTest 2>&1
if ($LASTEXITCODE -ne 0) {
    $monitor | ForEach-Object { Write-Output $_ }
    exit $LASTEXITCODE
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
$content = @"
# MA016 Pilot Lab Report

Generated: $timestamp

## Config

- Preset: $presetName
- Owner: $($config.owner)
- Target: $($config.target)
- Policy: $($config.policy)
- PDF mode: $($config.pdfMode)
- Proximity: $($config.proximity.mode)

## Monitor

~~~text
$($monitor -join [Environment]::NewLine)
~~~

## Required proof

- No File Ingress remains mandatory.
- Return and recovery remain mandatory.
- Dev/Lab security warnings must stay visible when non-production security is used.
- Native macOS/iPad execution remains a handoff until Xcode runs are attached.

## Result

MA016PilotReport: SUCCESS
RESULT: SUCCESS
"@

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputPath) | Out-Null
Set-Content -Path $OutputPath -Value $content -Encoding UTF8
Write-Output 'MA016PilotReport: SUCCESS'
Write-Output "Report: $OutputPath"
Write-Output 'RESULT: SUCCESS'
exit 0
