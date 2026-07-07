param(
    [switch] $SmokeTest,
    [int] $Page = 1,
    [double] $Zoom = 1.25,
    [string] $ViewerName = 'Windows PDF Viewer',
    [string] $Policy = 'TrustedPersonalDevices'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Output 'RK Workspace MA017 Pilot: Windows to macOS Open PDF'
Write-Output 'TargetAblage: Ablage macOS'
Write-Output 'Mode: OpenPdfFrame'
Write-Output 'Handoff: release/ma017/handoff/macOS_START_HERE.md'
Write-Output 'NativeExecution: PENDING_EXTERNAL_MACOS_XCODE'

& (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest:$SmokeTest -OpenPdf -Page $Page -Zoom $Zoom -ViewerName $ViewerName -UseGlassEdge -UseProximityFusion -UwbProfile Static -PlaySequence -Policy $Policy
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Output 'MA017WindowsToMacOpenPdf: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
