param(
    [switch] $SmokeTest,
    [int] $Page = 1,
    [double] $Zoom = 1.15,
    [string] $ViewerName = 'Windows PDF Viewer',
    [string] $Policy = 'DevelopmentLab'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Output 'RK Workspace Pilot: Windows to iPad Open PDF'
Write-Output 'TargetAblage: Ablage iPad'
Write-Output 'Mode: OpenPdfFrame'
Write-Output 'XcodeHint: Install iPad app with Docs/Testing/MA016_iOS_USBInstallRunbook.md'

& (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest:$SmokeTest -OpenPdf -Page $Page -Zoom $Zoom -ViewerName $ViewerName -UseGlassEdge -UseUwbSim -UwbProfile MovingCloser -PlaySequence -Policy $Policy
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Output 'WindowsToiPadOpenPdf: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
