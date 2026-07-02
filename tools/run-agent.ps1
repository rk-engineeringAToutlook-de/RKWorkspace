param(
    [switch] $Once,
    [switch] $Status,
    [switch] $Demo,
    [switch] $NoDemo
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$agentProject = Join-Path $root 'src\Agents\RKWorkspace.Agent\RKWorkspace.Agent.csproj'
$agentArgs = @()

if ($Once) {
    $agentArgs += '--once'
}

if ($Status) {
    $agentArgs += '--status'
}

if ($Demo) {
    $agentArgs += '--demo'
}

if ($NoDemo) {
    $agentArgs += '--no-demo'
}

dotnet build $agentProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $agentProject -- @agentArgs
exit $LASTEXITCODE
