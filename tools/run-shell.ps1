param(
    [switch] $Once,
    [switch] $Status,
    [switch] $OverlayDemo,
    [switch] $OverlaySmokeTest,
    [switch] $Help
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$shellProject = Join-Path $root 'src\Shell\RKWorkspace.Shell.Host\RKWorkspace.Shell.Host.csproj'
$shellArgs = @()

if ($Once) {
    $shellArgs += '--once'
}

if ($Status) {
    $shellArgs += '--status'
}

if ($OverlayDemo) {
    $shellArgs += '--overlay-demo'
}

if ($OverlaySmokeTest) {
    $shellArgs += '--overlay-smoke-test'
}

if ($Help) {
    $shellArgs += '--help'
}

dotnet build $shellProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $shellProject -- @shellArgs
exit $LASTEXITCODE
