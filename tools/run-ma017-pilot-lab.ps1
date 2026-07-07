param(
    [switch] $SmokeTest,
    [switch] $SkipRepeatability
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Invoke-LabStep {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Label,

        [Parameter(Mandatory = $true)]
        [scriptblock] $Command,

        [string] $Required = 'RESULT: SUCCESS'
    )

    Write-Output "== $Label =="
    $output = & $Command 2>&1
    $exitCode = $LASTEXITCODE
    $output | ForEach-Object { Write-Output $_ }
    if ($exitCode -ne 0) {
        throw "$Label failed with exit code $exitCode."
    }

    $text = $output -join [Environment]::NewLine
    if (-not $text.Contains($Required)) {
        throw "$Label failed because output did not contain: $Required"
    }
}

Write-Output 'RK Workspace MA017 Pilot Lab'
Write-Output '----------------------------'

$feedbackPath = Join-Path $env:TEMP 'rkworkspace-ma017-feedback-smoke.jsonl'
Remove-Item -LiteralPath $feedbackPath -Force -ErrorAction SilentlyContinue

Invoke-LabStep 'Config Wizard' {
    & (Join-Path $root 'tools\configure-ma017-pilot.ps1') -Preset WindowsToMac -SmokeTest -Force
} 'MA017PilotConfigWizard: SUCCESS'

Invoke-LabStep 'Pilot Report' {
    & (Join-Path $root 'tools\export-ma017-pilot-report.ps1') -SmokeTest:$SmokeTest
} 'MA017PilotReport: SUCCESS'

Invoke-LabStep 'Feedback Capture' {
    & (Join-Path $root 'tools\record-ma017-feedback.ps1') -SmokeTest -Scenario WindowsToMac -Rating Yellow -Comment 'Smoke feedback entry.' -OutputPath $feedbackPath
} 'MA017OwnerFeedbackCapture: SUCCESS'

Invoke-LabStep 'Feedback Report' {
    & (Join-Path $root 'tools\export-ma017-feedback-report.ps1') -SmokeTest:$SmokeTest -InputPath $feedbackPath
} 'MA017FeedbackReport: SUCCESS'

if (-not $SkipRepeatability) {
    Invoke-LabStep 'Repeatability' {
        & (Join-Path $root 'tools\run-ma017-repeatability.ps1') -SmokeTest:$SmokeTest -Count 2
    } 'MA017Repeatability: SUCCESS'
}

Invoke-LabStep 'Cleanup Dry Run' {
    & (Join-Path $root 'tools\cleanup-ma017-pilot.ps1') -SmokeTest
} 'MA017PilotCleanup: SUCCESS'

Write-Output 'MA017PilotLab: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
