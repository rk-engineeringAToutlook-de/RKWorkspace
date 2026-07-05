param(
    [switch] $SmokeTest,
    [int] $Port
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Shell\RKWorkspace.Shell.SpatialTray\RKWorkspace.Shell.SpatialTray.csproj'
$arguments = @()

if ($SmokeTest) {
    $arguments += '--mobile-glass-edge-smoke-test'
}

if ($Port -gt 0) {
    $arguments += '--port'
    $arguments += $Port.ToString()
}

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $project -- @arguments
exit $LASTEXITCODE
