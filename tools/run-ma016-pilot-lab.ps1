param(
    [switch] $SmokeTest,
    [switch] $SkipPerformance
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

Write-Output 'RK Workspace MA016 Pilot Lab'
Write-Output '----------------------------'

$feedbackPath = Join-Path $env:TEMP 'rkworkspace-ma016-feedback-smoke.jsonl'
Remove-Item -LiteralPath $feedbackPath -Force -ErrorAction SilentlyContinue

Invoke-LabStep 'Config Wizard' {
    & (Join-Path $root 'tools\configure-ma016-pilot.ps1') -Preset WindowsLocal -SmokeTest -Force
} 'PilotConfigWizard: SUCCESS'

Invoke-LabStep 'Pilot Report' {
    & (Join-Path $root 'tools\export-ma016-pilot-report.ps1') -SmokeTest:$SmokeTest
} 'MA016PilotReport: SUCCESS'

Invoke-LabStep 'Feedback Capture' {
    & (Join-Path $root 'tools\record-ma016-feedback.ps1') -SmokeTest -Scenario WindowsLocal -Rating Yellow -Comment 'Smoke feedback entry.' -OutputPath $feedbackPath
} 'OwnerFeedbackCapture: SUCCESS'

Invoke-LabStep 'Feedback Report' {
    & (Join-Path $root 'tools\export-ma016-feedback-report.ps1') -SmokeTest:$SmokeTest -InputPath $feedbackPath
} 'MA016FeedbackReport: SUCCESS'

Invoke-LabStep 'Repeatability' {
    & (Join-Path $root 'tools\run-ma016-repeatability.ps1') -SmokeTest:$SmokeTest -Count 2
} 'MA016Repeatability: SUCCESS'

if (-not $SkipPerformance) {
    Invoke-LabStep 'Performance Baseline' {
        & (Join-Path $root 'tools\export-ma016-performance-baseline.ps1') -SmokeTest:$SmokeTest
    } 'MA016PerformanceBaseline: SUCCESS'
}

Invoke-LabStep 'Cleanup Dry Run' {
    & (Join-Path $root 'tools\cleanup-ma016-pilot.ps1') -SmokeTest
} 'PilotCleanup: SUCCESS'

Write-Output 'MA016PilotLab: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
