param(
    [ValidateSet('WindowsLocal', 'WindowsToMac', 'WindowsToiPad', 'UwbSimulation', 'DonglePrep')]
    [string] $Preset = 'WindowsLocal',
    [string] $OutputPath,
    [string] $Policy,
    [string] $Target,
    [switch] $UseUwbSim,
    [switch] $UseProximityFusion,
    [switch] $Force,
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = if ($SmokeTest) {
        Join-Path $env:TEMP 'rkworkspace-ma017-pilot-smoke.json'
    }
    else {
        Join-Path $root 'release\ma017\logs\active-pilot.json'
    }
}

if ((Test-Path $OutputPath) -and -not $Force -and -not $SmokeTest) {
    throw "Pilot config already exists. Use -Force to overwrite: $OutputPath"
}

$defaults = @{
    WindowsLocal = @{
        Target = 'WindowsGuest'
        Policy = 'TrustedPersonalDevices'
        ProximityMode = 'Simulated'
        UwbProfile = 'Static'
        Command = '.\tools\run-windows-pdf-lifecycle-pilot.ps1 -SmokeTest -ClosedPdf -Policy TrustedPersonalDevices'
    }
    WindowsToMac = @{
        Target = 'macOSGuest'
        Policy = 'TrustedPersonalDevices'
        ProximityMode = 'ProximityFusion'
        UwbProfile = 'Static'
        Command = '.\tools\run-ma017-windows-to-mac-closed-pdf.ps1 -SmokeTest -Policy TrustedPersonalDevices'
    }
    WindowsToiPad = @{
        Target = 'iPadGuest'
        Policy = 'TrustedPersonalDevices'
        ProximityMode = 'UwbSimulation'
        UwbProfile = 'MovingCloser'
        Command = '.\tools\run-ma017-windows-to-ipad-closed-pdf.ps1 -SmokeTest -Policy TrustedPersonalDevices'
    }
    UwbSimulation = @{
        Target = 'NearestAblage'
        Policy = 'TrustedPersonalDevices'
        ProximityMode = 'UwbSimulation'
        UwbProfile = 'MovingCloser'
        Command = '.\tools\run-ma017-uwb-dongle-pilot.ps1 -SmokeTest'
    }
    DonglePrep = @{
        Target = 'AblageAnchorDongle'
        Policy = 'TrustedPersonalDevices'
        ProximityMode = 'DongleSimulation'
        UwbProfile = 'MovingCloser'
        Command = '.\tools\run-dongle-sim.ps1 -SmokeTest -UseFusion -Profile MovingCloser'
    }
}

$selected = $defaults[$Preset]
if (-not [string]::IsNullOrWhiteSpace($Policy)) {
    $selected.Policy = $Policy
}

if (-not [string]::IsNullOrWhiteSpace($Target)) {
    $selected.Target = $Target
}

if ($UseUwbSim) {
    $selected.ProximityMode = 'UwbSimulation'
}
elseif ($UseProximityFusion) {
    $selected.ProximityMode = 'ProximityFusion'
}

$generatedAt = if ($SmokeTest) { 'SMOKE-DETERMINISTIC' } else { (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ') }
$config = [ordered] @{
    schema = 'rkworkspace.ma017.pilot.config.v0.1'
    generatedAt = $generatedAt
    preset = $Preset
    owner = 'WindowsOwner'
    target = $selected.Target
    policy = $selected.Policy
    pdfMode = 'ClosedPdfCapsule'
    proximity = [ordered] @{
        mode = $selected.ProximityMode
        uwbProfile = $selected.UwbProfile
        confidenceThreshold = 0.70
        distanceHysteresis = 0.24
    }
    commands = @($selected.Command)
    expected = @(
        'NoFileIngress: SUCCESS',
        'CrossDeviceAuditEventsPresent: SUCCESS',
        'RESULT: SUCCESS'
    )
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputPath) | Out-Null
$config | ConvertTo-Json -Depth 8 | Set-Content -Path $OutputPath -Encoding UTF8

Write-Output 'RK Workspace MA017 Pilot Config Wizard'
Write-Output '--------------------------------------'
Write-Output "Preset: $Preset"
Write-Output "Target: $($selected.Target)"
Write-Output "Policy: $($selected.Policy)"
Write-Output "ProximityMode: $($selected.ProximityMode)"
Write-Output "ConfigPath: $OutputPath"
Write-Output 'MA017PilotConfigWizard: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
