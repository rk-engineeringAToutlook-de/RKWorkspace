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

Write-Output 'RK Workspace Cross-Device Security Regression'
Write-Output '---------------------------------------------'

Invoke-Checked 'Base RKWP Security Regression' {
    & (Join-Path $root 'tools\run-security-regression.ps1')
}

Invoke-Checked 'macOS Guest Security' {
    & (Join-Path $root 'tools\run-mac-guest-compat.ps1') -SmokeTest
} @(
    'NoFileIngress: SUCCESS',
    'RESULT: SUCCESS'
)

Invoke-Checked 'iPad Guest Security' {
    & (Join-Path $root 'tools\run-ios-guest-compat.ps1') -SmokeTest
} @(
    'NoFileIngress: SUCCESS',
    'RESULT: SUCCESS'
)

Invoke-Checked 'Closed PDF Critical Security' {
    & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf -Policy CriticalInfrastructure
} @(
    'LifecycleMode: ClosedPdfCapsule',
    'PolicyProfile: CriticalInfrastructure',
    'NoFileIngress: SUCCESS',
    'SecurityModeWarning: NONE',
    'CrossDeviceAuditEventsPresent: SUCCESS',
    'RESULT: SUCCESS'
)

Invoke-Checked 'Open PDF Critical Security' {
    & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -OpenPdf -Policy CriticalInfrastructure
} @(
    'LifecycleMode: OpenPdfFrame',
    'PolicyProfile: CriticalInfrastructure',
    'OpenFrame: OK',
    'NoFileIngress: SUCCESS',
    'SecurityModeWarning: NONE',
    'CrossDeviceAuditEventsPresent: SUCCESS',
    'RESULT: SUCCESS'
)

Invoke-Checked 'Unauthorized KeepCapsule' {
    & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf -KeepCapsule -Policy CriticalInfrastructure
} @(
    'PolicyProfile: CriticalInfrastructure',
    'CloseFrameBehavior: KeepCapsule',
    'KeepCapsulePolicy: DENIED',
    'FinalCapsuleState: Returned',
    'RESULT: SUCCESS'
)

Invoke-Checked 'Stale Lease Recovery' {
    & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf -Policy TrustedPersonalDevices
} @(
    'ExpiredCapsule: RECOVERED_BY_OWNER',
    'Recovery: SUCCESS',
    'RESULT: SUCCESS'
)

Write-Output 'macOSGuestSecurity: READY'
Write-Output 'iPadGuestSecurity: READY'
Write-Output 'ClosedPdfSecurity: SUCCESS'
Write-Output 'OpenPdfSecurity: SUCCESS'
Write-Output 'UnauthorizedKeepCapsule: DENIED'
Write-Output 'StaleLeaseRecovery: SUCCESS'
Write-Output 'CrossDeviceSecurityRegression: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
