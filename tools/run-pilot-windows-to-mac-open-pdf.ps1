param(
    [switch] $SmokeTest,
    [int] $Page = 1,
    [double] $Zoom = 1.25,
    [string] $ViewerName = 'Windows PDF Viewer',
    [string] $Policy = 'DevelopmentLab'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Output 'RK Workspace Pilot: Windows to macOS Open PDF'
Write-Output 'TargetAblage: Ablage macOS'
Write-Output 'Mode: OpenPdfFrame'
Write-Output 'Handoff: release/ma016/handoff/macOS_START_HERE.md'
Write-Output 'Note: This Windows command prepares the owner side; native macOS execution happens in Xcode.'

& (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest:$SmokeTest -OpenPdf -Page $Page -Zoom $Zoom -ViewerName $ViewerName -UseGlassEdge -UseProximityFusion -UwbProfile Static -PlaySequence -Policy $Policy
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Output 'WindowsToMacOpenPdf: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
