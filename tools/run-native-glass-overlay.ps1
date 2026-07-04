param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$nativeGlassProject = Join-Path $root 'src\Shell\RKWorkspace.Shell.NativeGlassOverlay.Windows\RKWorkspace.Shell.NativeGlassOverlay.Windows.csproj'
$nativeGlassArgs = @()

if ($SmokeTest) {
    $nativeGlassArgs += '--smoke-test'
}

dotnet build $nativeGlassProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $nativeGlassProject -- @nativeGlassArgs
exit $LASTEXITCODE
