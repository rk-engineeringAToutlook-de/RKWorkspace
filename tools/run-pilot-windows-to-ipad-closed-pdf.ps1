param(
    [switch] $SmokeTest,
    [string] $Policy = 'DevelopmentLab'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Output 'RK Workspace Pilot: Windows to iPad Closed PDF'
Write-Output 'TargetAblage: Ablage iPad'
Write-Output 'Mode: ClosedPdfCapsule'
Write-Output 'XcodeHint: Install iPad app with Docs/Testing/MA016_iOS_USBInstallRunbook.md'

& (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest:$SmokeTest -ClosedPdf -UseGlassEdge -UseUwbSim -UwbProfile MovingCloser -PlaySequence -Policy $Policy
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Output 'WindowsToiPadClosedPdf: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
