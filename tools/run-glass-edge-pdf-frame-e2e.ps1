param(
    [switch] $SmokeTest,
    [switch] $ClosedPdfCapsuleTest,
    [switch] $OpenPdfFrameTest,
    [switch] $UseManualMap,
    [string] $TargetAblage,
    [string] $PdfPath,
    [switch] $OwnerVisible
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.GlassEdgePdfFrameE2E\RKWorkspace.GlassEdgePdfFrameE2E.csproj'
$arguments = @()

if ($SmokeTest) {
    $arguments += '--smoke-test'
}

if ($ClosedPdfCapsuleTest) {
    Write-Host 'ClosedPdfCapsuleTest: ENABLED'
}

if ($OpenPdfFrameTest) {
    Write-Host 'OpenPdfFrameTest: ENABLED'
}

if ($UseManualMap) {
    $arguments += '--use-manual-map'
}

if (-not [string]::IsNullOrWhiteSpace($TargetAblage)) {
    $arguments += '--target-ablage'
    $arguments += $TargetAblage
}

if (-not [string]::IsNullOrWhiteSpace($PdfPath)) {
    $arguments += '--pdf-path'
    $arguments += $PdfPath
}

if ($OwnerVisible) {
    $arguments += '--owner-visible'
}

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $project -- @arguments
exit $LASTEXITCODE
