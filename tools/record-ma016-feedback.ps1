param(
    [string] $Scenario = 'WindowsLocal',
    [ValidateSet('Green', 'Yellow', 'Red')]
    [string] $Rating = 'Yellow',
    [string] $Comment = 'Noch nicht durch den Owner bewertet.',
    [string] $Surface = 'Windows',
    [string] $HumanExperience = 'HX-000/HX-001',
    [string] $OutputPath,
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = if ($SmokeTest) {
        Join-Path $env:TEMP 'rkworkspace-ma016-feedback-smoke.jsonl'
    }
    else {
        Join-Path $root 'release\ma016\logs\owner-feedback.jsonl'
    }
}

$entry = [ordered] @{
    schema = 'rkworkspace.ma016.owner.feedback.v0.1'
    recordedAt = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
    scenario = $Scenario
    surface = $Surface
    humanExperience = $HumanExperience
    rating = $Rating
    comment = $Comment
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputPath) | Out-Null
($entry | ConvertTo-Json -Compress -Depth 5) | Add-Content -Path $OutputPath -Encoding UTF8

Write-Output 'RK Workspace MA016 Owner Feedback Capture'
Write-Output '-----------------------------------------'
Write-Output "Scenario: $Scenario"
Write-Output "Rating: $Rating"
Write-Output "FeedbackPath: $OutputPath"
Write-Output 'OwnerFeedbackCapture: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
