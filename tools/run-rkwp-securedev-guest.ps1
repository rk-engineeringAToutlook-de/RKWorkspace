param(
    [string] $Endpoint = 'rkws-rkwp-securedev-owner'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.RkwpSecureDevHarness\RKWorkspace.RkwpSecureDevHarness.csproj'

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $project -- --guest --endpoint $Endpoint
exit $LASTEXITCODE
