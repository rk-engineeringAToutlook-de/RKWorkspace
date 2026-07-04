param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$gpuLensProject = Join-Path $root 'src\Shell\RKWorkspace.Shell.LivingLens.Gpu.Windows\RKWorkspace.Shell.LivingLens.Gpu.Windows.csproj'
$gpuLensArgs = @()

if ($SmokeTest) {
    $gpuLensArgs += '--smoke-test'
}

dotnet build $gpuLensProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $gpuLensProject -- @gpuLensArgs
exit $LASTEXITCODE
