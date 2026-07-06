param(
    [switch] $SmokeTest,
    [string] $Policy = 'DevelopmentLab'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Output 'RK Workspace Pilot: Windows to macOS Closed PDF'
Write-Output 'TargetAblage: Ablage macOS'
Write-Output 'Mode: ClosedPdfCapsule'
Write-Output 'Handoff: release/ma016/handoff/macOS_START_HERE.md'
Write-Output 'Note: This Windows command prepares the owner side; native macOS execution happens in Xcode.'

& (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest:$SmokeTest -ClosedPdf -UseGlassEdge -UseProximityFusion -UwbProfile Static -PlaySequence -Policy $Policy
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Output 'WindowsToMacClosedPdf: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
