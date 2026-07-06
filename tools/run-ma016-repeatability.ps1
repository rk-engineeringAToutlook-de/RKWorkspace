param(
    [int] $Count = 2,
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma016\reports\ma016-repeatability-report.md'
$runs = [Math]::Max(1, [Math]::Min($Count, 5))
$results = New-Object System.Collections.Generic.List[string]

Write-Output 'RK Workspace MA016 Repeatability'
Write-Output '--------------------------------'

for ($index = 1; $index -le $runs; $index++) {
    Write-Output "== Repeatability Run $index/$runs =="
    $output = & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf -Policy CriticalInfrastructure 2>&1
    $exitCode = $LASTEXITCODE
    $output | ForEach-Object { Write-Output $_ }
    if ($exitCode -ne 0) {
        throw "Repeatability run $index failed with exit code $exitCode."
    }

    $text = $output -join [Environment]::NewLine
    foreach ($line in @('NoFileIngress: SUCCESS', 'Recovery: SUCCESS', 'OwnerPdfSafetyGuard: OK', 'RESULT: SUCCESS')) {
        if (-not $text.Contains($line)) {
            throw "Repeatability run $index failed because output did not contain: $line"
        }
    }

    $results.Add("Run ${index}: PASS") | Out-Null
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
$content = @"
# MA016 Repeatability Report

Generated: $timestamp

## Runs

$($results -join [Environment]::NewLine)

## Result

RepeatabilityRuns: $runs
MA016Repeatability: SUCCESS
RESULT: SUCCESS
"@

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $reportPath) | Out-Null
Set-Content -Path $reportPath -Value $content -Encoding UTF8
Write-Output "RepeatabilityRuns: $runs"
Write-Output 'MA016Repeatability: SUCCESS'
Write-Output "Report: $reportPath"
Write-Output 'RESULT: SUCCESS'
exit 0
