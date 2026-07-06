param(
    [string] $InputPath,
    [string] $OutputPath,
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($InputPath)) {
    $InputPath = Join-Path $root 'release\ma016\logs\owner-feedback.jsonl'
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $root 'release\ma016\reports\ma016-feedback-report.md'
}

$entries = @()
if (Test-Path $InputPath) {
    $entries = @(Get-Content -Path $InputPath | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { $_ | ConvertFrom-Json })
}

$green = @($entries | Where-Object { $_.rating -eq 'Green' }).Count
$yellow = @($entries | Where-Object { $_.rating -eq 'Yellow' }).Count
$red = @($entries | Where-Object { $_.rating -eq 'Red' }).Count
$latest = if ($entries.Count -gt 0) { $entries[-1].comment } else { 'No owner feedback captured yet.' }
$timestamp = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')

$content = @"
# MA016 Owner Feedback Report

Generated: $timestamp

## Summary

- Feedback entries: $($entries.Count)
- Green: $green
- Yellow: $yellow
- Red: $red

## Latest note

$latest

## Interpretation

Owner feedback is a human-experience signal, not a technical benchmark. A larger pilot only moves forward when the Owner can repeat the flow and still trusts No File Ingress, Return and Recovery.

## Result

MA016FeedbackReport: SUCCESS
RESULT: SUCCESS
"@

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputPath) | Out-Null
Set-Content -Path $OutputPath -Value $content -Encoding UTF8
Write-Output 'MA016FeedbackReport: SUCCESS'
Write-Output "FeedbackEntries: $($entries.Count)"
Write-Output "Report: $OutputPath"
Write-Output 'RESULT: SUCCESS'
exit 0
