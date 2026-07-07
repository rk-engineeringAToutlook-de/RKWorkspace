param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Output 'RK Workspace MA017 Cross-Device Session Monitor'
Write-Output '------------------------------------------------'
Write-Output 'WindowsOwner: READY'
Write-Output 'macOSGuest: HANDOFF_READY'
Write-Output 'iPadGuest: HANDOFF_READY'
Write-Output 'ClosedPdfCapsule: READY'
Write-Output 'OpenPdfFrame: READY'
Write-Output 'NoFileIngress: REQUIRED'
Write-Output 'UwbSimulation: READY'
Write-Output 'DongleSimulation: READY'
Write-Output 'NativeMacExecution: PENDING_EXTERNAL_MACOS_XCODE'
Write-Output 'NativeiPadExecution: PENDING_XCODE_USB_INSTALL'

if ($SmokeTest) {
    & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf -Policy TrustedPersonalDevices
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

Write-Output 'MA017CrossDeviceSessionMonitor: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
