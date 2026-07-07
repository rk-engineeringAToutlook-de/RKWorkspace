param(
    [switch] $SmokeTest,
    [string] $Policy = 'TrustedPersonalDevices'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Output 'RK Workspace MA017 Pilot: Windows to iPad Closed PDF'
Write-Output 'TargetAblage: Ablage iPad'
Write-Output 'Mode: ClosedPdfCapsule'
Write-Output 'XcodeHint: release/ma017/runbooks/iOS_USBInstallRunbook.md'
Write-Output 'NativeExecution: PENDING_XCODE_USB_INSTALL'

& (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest:$SmokeTest -ClosedPdf -UseGlassEdge -UseUwbSim -UwbProfile MovingCloser -PlaySequence -Policy $Policy
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Output 'MA017WindowsToiPadClosedPdf: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
