param()

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$harnessProject = Join-Path $root 'tools\DualAgentHarness\RKWorkspace.DualAgentHarness.csproj'

dotnet build $harnessProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $harnessProject
exit $LASTEXITCODE
