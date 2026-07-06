param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Label,

        [Parameter(Mandatory = $true)]
        [scriptblock] $Command,

        [string[]] $Required = @()
    )

    Write-Output "== $Label =="
    $output = & $Command 2>&1
    $exitCode = $LASTEXITCODE
    $output | ForEach-Object { Write-Output $_ }
    if ($exitCode -ne 0) {
        throw "$Label failed with exit code $exitCode."
    }

    $text = $output -join [Environment]::NewLine
    foreach ($line in $Required) {
        if (-not $text.Contains($line)) {
            throw "$Label failed because output did not contain: $line"
        }
    }
}

Write-Output 'RK Workspace Cross-Device Policy Regression'
Write-Output '-------------------------------------------'

Invoke-Checked 'Policy Profile Smoke' {
    & (Join-Path $root 'tools\run-policy-profile.ps1') -SmokeTest
} @(
    'PolicyProfileValidate: SUCCESS',
    'RESULT: SUCCESS'
)

Invoke-Checked 'Critical Infrastructure Pilot Mode' {
    & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf -KeepCapsule -Policy CriticalInfrastructure
} @(
    'PolicyProfile: CriticalInfrastructure',
    'KeepCapsulePolicy: DENIED',
    'SecurityModeWarning: NONE',
    'OwnerPdfSafetyGuard: OK',
    'RESULT: SUCCESS'
)

Invoke-Checked 'Trusted Personal Devices Pilot Mode' {
    & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf -KeepCapsule -Policy TrustedPersonalDevices
} @(
    'PolicyProfile: TrustedPersonalDevices',
    'KeepCapsulePolicy: ALLOWED',
    'SecurityModeWarning: NON_PRODUCTION_SECURITY',
    'OwnerPdfSafetyWarning: NonProductionSecurity',
    'RESULT: SUCCESS'
)

Invoke-Checked 'Presentation Only Pilot Mode' {
    & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf -Policy PresentationOnly
} @(
    'PolicyProfile: PresentationOnly',
    'KeepCapsulePolicy: DENIED',
    'SecurityModeWarning: NON_PRODUCTION_SECURITY',
    'RESULT: SUCCESS'
)

Invoke-Checked 'macOS Guest Policy' {
    & (Join-Path $root 'tools\run-pilot-windows-to-mac-closed-pdf.ps1') -SmokeTest -Policy CriticalInfrastructure
} @(
    'TargetAblage: Ablage macOS',
    'PolicyProfile: CriticalInfrastructure',
    'WindowsToMacClosedPdf: SUCCESS',
    'RESULT: SUCCESS'
)

Invoke-Checked 'iPad Guest Policy' {
    & (Join-Path $root 'tools\run-pilot-windows-to-ipad-closed-pdf.ps1') -SmokeTest -Policy TrustedPersonalDevices
} @(
    'TargetAblage: Ablage iPad',
    'PolicyProfile: TrustedPersonalDevices',
    'WindowsToiPadClosedPdf: SUCCESS',
    'RESULT: SUCCESS'
)

Invoke-Checked 'UWB Target Policy' {
    & (Join-Path $root 'tools\run-windows-pdf-frame-pilot.ps1') -SmokeTest -UseGlassEdge -UseUwbSim -UwbProfile MovingCloser -PlaySequence -Policy TrustedPersonalDevices
} @(
    'ProximityMode: UwbSim',
    'UwbProfile: MovingCloser',
    'UwbSelectedTarget: OK',
    'CrossDeviceAuditEventsPresent: SUCCESS',
    'RESULT: SUCCESS'
)

Write-Output 'CriticalInfrastructurePolicy: SUCCESS'
Write-Output 'TrustedPersonalDevicesPolicy: SUCCESS'
Write-Output 'PresentationOnlyPolicy: SUCCESS'
Write-Output 'macOSGuestPolicy: SUCCESS'
Write-Output 'iPadGuestPolicy: SUCCESS'
Write-Output 'UwbTargetPolicy: SUCCESS'
Write-Output 'CrossDevicePolicyRegression: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
