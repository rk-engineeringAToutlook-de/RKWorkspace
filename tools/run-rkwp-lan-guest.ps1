param(
    [Alias('Host')]
    [string] $TargetHost = '127.0.0.1',
    [int] $Port = 57100
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.RkwpDevLanHarness\RKWorkspace.RkwpDevLanHarness.csproj'

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $project -- --guest --host $TargetHost --port $Port
exit $LASTEXITCODE
