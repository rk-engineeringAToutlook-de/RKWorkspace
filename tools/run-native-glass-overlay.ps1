param(
    [switch] $SmokeTest,
    [string] $SourcePdfPath,
    [string] $PlacementSignalPath
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

dotnet build $nativeGlassProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $nativeGlassProject -- @nativeGlassArgs
exit $LASTEXITCODE
