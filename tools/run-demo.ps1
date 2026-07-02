$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$demoProject = Join-Path $root 'src\Demo\RKWorkspace.Core.Demo\RKWorkspace.Core.Demo.csproj'

dotnet build $demoProject
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $demoProject
exit $LASTEXITCODE
