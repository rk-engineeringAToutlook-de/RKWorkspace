param()

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$agentProject = Join-Path $root 'src\Agents\RKWorkspace.Agent\RKWorkspace.Agent.csproj'
$harnessProject = Join-Path $root 'tools\LocalIpcHarness\RKWorkspace.LocalIpcHarness.csproj'

dotnet build $agentProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet build $harnessProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --no-build --project $harnessProject
exit $LASTEXITCODE
