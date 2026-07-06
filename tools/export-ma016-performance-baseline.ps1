param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma016\reports\ma016-performance-baseline.md'
$measurements = New-Object System.Collections.Generic.List[object]

function Invoke-Measured {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Scenario,

        [Parameter(Mandatory = $true)]
        [scriptblock] $Command
    )

    $watch = [System.Diagnostics.Stopwatch]::StartNew()
    $output = & $Command 2>&1
    $exitCode = $LASTEXITCODE
    $watch.Stop()
    $output | ForEach-Object { Write-Output $_ }
    if ($exitCode -ne 0) {
        throw "$Scenario failed with exit code $exitCode."
    }

    $measurements.Add([pscustomobject] @{
        Scenario = $Scenario
        DurationMs = $watch.ElapsedMilliseconds
    }) | Out-Null
}

Write-Output 'RK Workspace MA016 Performance Baseline'
Write-Output '---------------------------------------'

Invoke-Measured 'ClosedPdfCapsule' {
    & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf -Policy CriticalInfrastructure
}

Invoke-Measured 'UwbGlassEdge' {
    & (Join-Path $root 'tools\run-windows-pdf-frame-pilot.ps1') -SmokeTest -UseGlassEdge -UseUwbSim -UwbProfile MovingCloser -PlaySequence -Policy TrustedPersonalDevices
}

if (-not $SmokeTest) {
    Invoke-Measured 'WindowsToMacPrep' {
        & (Join-Path $root 'tools\run-pilot-windows-to-mac-closed-pdf.ps1') -SmokeTest -Policy CriticalInfrastructure
    }
}

$rows = $measurements | ForEach-Object { "| $($_.Scenario) | $($_.DurationMs) |" }
$timestamp = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
$content = @"
# MA016 Pilot Lab Performance Baseline

Generated: $timestamp

## Measured smoke scenarios

| Scenario | DurationMs |
| --- | ---: |
$($rows -join [Environment]::NewLine)

## Planned full scenarios

- ClosedPdfCapsule
- OpenPdfFrame
- UWB nearest Ablage
- Windows local
- Windows-to-MacPrep

## Result

MA016PerformanceBaseline: SUCCESS
RESULT: SUCCESS
"@

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $reportPath) | Out-Null
Set-Content -Path $reportPath -Value $content -Encoding UTF8
Write-Output 'MA016PerformanceBaseline: SUCCESS'
Write-Output "Report: $reportPath"
Write-Output 'RESULT: SUCCESS'
exit 0
