param(
    [switch] $SmokeTest,
    [switch] $ClosedPdf,
    [switch] $OpenPdf,
    [string] $PdfPath,
    [string] $OpenPdfPath,
    [int] $Page = 1,
    [double] $Zoom = 1.0,
    [string] $ViewerName = 'Windows PDF Viewer',
    [switch] $CloseReturns,
    [switch] $KeepCapsule,
    [string] $Policy = 'DevelopmentLab',
    [switch] $UseGlassEdge,
    [switch] $UseManualMap,
    [switch] $UseUwbSim,
    [switch] $UseProximityFusion,
    [string] $UwbProfile = 'Static',
    [double] $ConfidenceThreshold = 0.70,
    [double] $DistanceHysteresis = 0.24,
    [switch] $PlaySequence,
    [switch] $Debug
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$framePilot = Join-Path $root 'tools\run-windows-pdf-frame-pilot.ps1'
$parameters = @{
    Page = $Page
    Zoom = $Zoom
    ViewerName = $ViewerName
    Policy = $Policy
}

if ($SmokeTest) { $parameters.SmokeTest = $true }
if ($OpenPdf) { $parameters.OpenPdf = $true }
elseif ($ClosedPdf) { $parameters.ClosedPdf = $true }
else { $parameters.ClosedPdf = $true }
if (-not [string]::IsNullOrWhiteSpace($PdfPath)) { $parameters.PdfPath = $PdfPath }
if (-not [string]::IsNullOrWhiteSpace($OpenPdfPath)) { $parameters.OpenPdfPath = $OpenPdfPath }
if ($KeepCapsule) { $parameters.KeepCapsule = $true }
elseif ($CloseReturns) { $parameters.CloseReturns = $true }
if ($UseGlassEdge) { $parameters.UseGlassEdge = $true }
if ($UseManualMap) { $parameters.UseManualMap = $true }
if ($UseUwbSim) { $parameters.UseUwbSim = $true }
if ($UseProximityFusion) { $parameters.UseProximityFusion = $true }
$parameters.UwbProfile = $UwbProfile
$parameters.ConfidenceThreshold = $ConfidenceThreshold
$parameters.DistanceHysteresis = $DistanceHysteresis
if ($PlaySequence) { $parameters.PlaySequence = $true }
if ($Debug) { $parameters.Debug = $true }

$output = & $framePilot @parameters 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Output $_ }
if ($exitCode -ne 0) {
    exit $exitCode
}

if ($SmokeTest) {
    $text = $output -join [Environment]::NewLine
    $expectedMode = if ($OpenPdf) { 'LifecycleMode: OpenPdfFrame' } else { 'LifecycleMode: ClosedPdfCapsule' }
    $required = @(
        $expectedMode,
        'FrameCapsule: OK',
        'CapsuleOpen: OK',
        'CapsuleNoFileIngress: SUCCESS',
        'OpenFrameNoFileIngress: SUCCESS',
        'CapsuleCache: MemoryOnly',
        'OpenFrameCache: MemoryOnly',
        'UnauthorizedCapsuleOpen: DENIED',
        'ExpiredCapsule: RECOVERED_BY_OWNER',
        'RESULT: SUCCESS'
    )

    if ($OpenPdf) {
        $required += @('OpenPdfContext: OK', 'OpenFrame: OK')
    }

    foreach ($line in $required) {
        if (-not $text.Contains($line)) {
            throw "Windows PDF Lifecycle Smoke failed because output did not contain: $line"
        }
    }
}

exit 0
