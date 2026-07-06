param(
    [switch] $SmokeTest,
    [string] $OutputPath
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Split-Path -Parent $PSScriptRoot)).Path

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = if ($SmokeTest) {
        Join-Path $env:TEMP 'rkworkspace-ma016-owner-test.zip'
    }
    else {
        Join-Path $root 'release\ma016\packages\ma016-owner-test.zip'
    }
}

$files = @(
    'Docs\Readiness\MA016_ReadinessReview.md',
    'Docs\Readiness\MA016_GoNoGoDecision.md',
    'Docs\Readiness\MA016_GoNoGoCriteria.md',
    'Docs\Testing\MA016_RealTestRunbook.md',
    'Docs\Testing\MA016_SecurityOwnerChecklist.md',
    'release\MA016_READINESS_SUMMARY.md',
    'release\ma016\handoff\macOS_FINAL_HANDOFF.md',
    'release\ma016\handoff\iOS_iPadOS_FINAL_HANDOFF.md',
    'release\ma016\config\windows-local.sample.json',
    'release\ma016\config\windows-to-mac.sample.json',
    'release\ma016\config\windows-to-ipad.sample.json',
    'release\ma016\config\uwb-simulation.sample.json',
    'release\ma016\reports\ma016-pilot-report.md',
    'release\ma016\reports\ma016-feedback-report.md',
    'tools\run-ma016-pilot-lab.ps1',
    'tools\record-ma016-feedback.ps1'
)

$missing = @()
foreach ($file in $files) {
    if (-not (Test-Path (Join-Path $root $file))) {
        $missing += $file
    }
}

if ($missing.Count -gt 0 -and -not $SmokeTest) {
    throw "Owner test package is missing required files: $($missing -join ', ')"
}

$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$staging = Join-Path $env:TEMP "RKWorkspace_MA016_OwnerTest_$timestamp"
Remove-Item -LiteralPath $staging -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $staging | Out-Null

foreach ($file in $files) {
    $source = Join-Path $root $file
    if (-not (Test-Path $source)) {
        continue
    }

    $target = Join-Path $staging $file
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $target) | Out-Null
    Copy-Item -LiteralPath $source -Destination $target -Force
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputPath) | Out-Null
Compress-Archive -Path (Join-Path $staging '*') -DestinationPath $OutputPath -Force
Remove-Item -LiteralPath $staging -Recurse -Force

Write-Output 'RK Workspace MA016 Owner Test Package'
Write-Output '-------------------------------------'
Write-Output "Output: $OutputPath"
Write-Output "Files: $($files.Count - $missing.Count)"
Write-Output 'MA016OwnerTestPackage: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
