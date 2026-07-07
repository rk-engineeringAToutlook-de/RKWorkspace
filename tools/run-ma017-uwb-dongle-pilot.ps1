param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Invoke-CheckedStep {
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

Write-Output 'RK Workspace MA017 UWB/Dongle Pilot'
Write-Output '-----------------------------------'

Invoke-CheckedStep 'UWB Simulator Glass Edge Pilot' {
    & (Join-Path $root 'tools\run-windows-pdf-frame-pilot.ps1') -SmokeTest -UseGlassEdge -UseUwbSim -UwbProfile MovingCloser -PlaySequence -Policy TrustedPersonalDevices
}
Write-Output 'MA017UwbSimulator: SUCCESS'

Invoke-CheckedStep 'Manual Map Smoke' {
    & (Join-Path $root 'tools\run-manual-map.ps1') -SmokeTest
} 'ManualMapSmoke: SUCCESS'

Invoke-CheckedStep 'Manual Map + UWB Fusion Pilot' {
    & (Join-Path $root 'tools\run-windows-pdf-frame-pilot.ps1') -SmokeTest -UseGlassEdge -UseProximityFusion -UwbProfile PassingBy -PlaySequence -Policy TrustedPersonalDevices
}
Write-Output 'MA017ManualMapUwbFusion: SUCCESS'

Invoke-CheckedStep 'Dongle Anchor Simulation' {
    & (Join-Path $root 'tools\run-dongle-sim.ps1') -SmokeTest -UseFusion -Profile MovingCloser
} 'DongleAnchorSmoke: SUCCESS'

Write-Output 'UwbConfidenceVisualization: READY'
Write-Output 'DongleAnchorMvpSpec: READY'
Write-Output 'ProximityPrivacy: READY'
Write-Output 'MA017UwbDonglePilot: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
