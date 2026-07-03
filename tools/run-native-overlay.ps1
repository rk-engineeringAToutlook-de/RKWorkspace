param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$nativeOverlayProject = Join-Path $root 'src\Shell\RKWorkspace.Shell.NativeOverlay.Windows\RKWorkspace.Shell.NativeOverlay.Windows.csproj'
$nativeOverlayArgs = @()

if ($SmokeTest) {
    $nativeOverlayArgs += '--smoke-test'
}

dotnet build $nativeOverlayProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $nativeOverlayProject -- @nativeOverlayArgs
exit $LASTEXITCODE
