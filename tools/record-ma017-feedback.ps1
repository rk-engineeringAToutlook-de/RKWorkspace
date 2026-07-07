param(
    [string] $Scenario = 'WindowsLocal',
    [ValidateSet('Green', 'Yellow', 'Red')]
    [string] $Rating = 'Yellow',
    [string] $Comment = 'Noch nicht durch den Owner bewertet.',
    [string] $Surface = 'Windows',
    [string] $HumanExperience = 'HX-000/HX-001/HX-001A/HX-002',
    [string] $OutputPath,
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = if ($SmokeTest) {
        Join-Path $env:TEMP 'rkworkspace-ma017-feedback-smoke.jsonl'
    }
    else {
        Join-Path $root 'release\ma017\logs\owner-feedback.jsonl'
    }
}

$recordedAt = if ($SmokeTest) { 'SMOKE-DETERMINISTIC' } else { (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ') }
$entry = [ordered] @{
    schema = 'rkworkspace.ma017.owner.feedback.v0.1'
    recordedAt = $recordedAt
    scenario = $Scenario
    surface = $Surface
    humanExperience = $HumanExperience
    rating = $Rating
    comment = $Comment
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputPath) | Out-Null
($entry | ConvertTo-Json -Compress -Depth 5) | Add-Content -Path $OutputPath -Encoding UTF8

Write-Output 'RK Workspace MA017 Owner Feedback Capture'
Write-Output '-----------------------------------------'
Write-Output "Scenario: $Scenario"
Write-Output "Rating: $Rating"
Write-Output "FeedbackPath: $OutputPath"
Write-Output 'MA017OwnerFeedbackCapture: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
