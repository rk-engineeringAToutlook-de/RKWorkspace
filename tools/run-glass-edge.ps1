param(
    [switch] $SmokeTest,
    [switch] $ExportFrames
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Shell\RKWorkspace.Shell.LivingLens.Gpu.Windows\RKWorkspace.Shell.LivingLens.Gpu.Windows.csproj'
$arguments = @()

if ($SmokeTest) {
    $arguments += '--glass-edge-smoke-test'
}
elseif ($ExportFrames) {
    $arguments += '--export-glass-edge-frames'
}

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $project -- @arguments
exit $LASTEXITCODE
