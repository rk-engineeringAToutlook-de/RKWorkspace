param(
    [switch] $SmokeTest,
    [string] $SourcePdfPath,
    [string] $PlacementSignalPath,
    [switch] $RealPdfGesture,
    [switch] $PickImmediately,
    [switch] $ContextListener,
    [switch] $InstantPlacementSignal,
    [string] $DiagnosticsPath,
    [string] $TargetDisplayName,
    [string] $TargetDirection,
    [double] $TargetDistanceMeters = -1,
    [string] $TargetDistanceSource,
    [string] $ContextPipeName
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$nativeGlassProject = Join-Path $root 'src\Shell\RKWorkspace.Shell.NativeGlassOverlay.Windows\RKWorkspace.Shell.NativeGlassOverlay.Windows.csproj'
$nativeGlassArgs = @()

if ($SmokeTest) {
    $nativeGlassArgs += '--smoke-test'
}

if (-not [string]::IsNullOrWhiteSpace($SourcePdfPath)) {
    $nativeGlassArgs += '--source-pdf'
    $nativeGlassArgs += $SourcePdfPath
}

if (-not [string]::IsNullOrWhiteSpace($PlacementSignalPath)) {
    $nativeGlassArgs += '--placement-signal'
    $nativeGlassArgs += $PlacementSignalPath
}

if ($RealPdfGesture) {
    $nativeGlassArgs += '--real-pdf-gesture'
}

if ($PickImmediately) {
    $nativeGlassArgs += '--pick-immediately'
}

if ($ContextListener) {
    $nativeGlassArgs += '--context-listener'
}

if ($InstantPlacementSignal) {
    $nativeGlassArgs += '--instant-placement-signal'
}

if (-not [string]::IsNullOrWhiteSpace($DiagnosticsPath)) {
    $nativeGlassArgs += '--diagnostics'
    $nativeGlassArgs += $DiagnosticsPath
}

if (-not [string]::IsNullOrWhiteSpace($TargetDisplayName)) {
    $nativeGlassArgs += '--target-name'
    $nativeGlassArgs += $TargetDisplayName
}

if (-not [string]::IsNullOrWhiteSpace($TargetDirection)) {
    $nativeGlassArgs += '--target-direction'
    $nativeGlassArgs += $TargetDirection
}

if ($TargetDistanceMeters -ge 0) {
    $nativeGlassArgs += '--target-distance-meters'
    $nativeGlassArgs += $TargetDistanceMeters.ToString([System.Globalization.CultureInfo]::InvariantCulture)
}

if (-not [string]::IsNullOrWhiteSpace($TargetDistanceSource)) {
    $nativeGlassArgs += '--target-distance-source'
    $nativeGlassArgs += $TargetDistanceSource
}

if (-not [string]::IsNullOrWhiteSpace($ContextPipeName)) {
    $nativeGlassArgs += '--context-pipe'
    $nativeGlassArgs += $ContextPipeName
}

dotnet build $nativeGlassProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $nativeGlassProject -- @nativeGlassArgs
exit $LASTEXITCODE
