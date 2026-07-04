param(
    [switch] $SmokeTest,
    [int] $Port = 0
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$spatialTrayProject = Join-Path $root 'src\Shell\RKWorkspace.Shell.SpatialTray\RKWorkspace.Shell.SpatialTray.csproj'
$spatialTrayArgs = @()

if ($SmokeTest) {
    $spatialTrayArgs += '--mobile-smoke-test'
}

if ($Port -gt 0) {
    $spatialTrayArgs += '--port'
    $spatialTrayArgs += $Port.ToString()
}

dotnet build $spatialTrayProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

if (-not $SmokeTest) {
    Write-Host 'Mobile Spatial Surface oeffnen:'
}

dotnet run --project $spatialTrayProject -- @spatialTrayArgs
exit $LASTEXITCODE
