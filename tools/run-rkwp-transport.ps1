param(
    [switch] $Server,
    [switch] $Client,
    [switch] $SmokeTest,
    [string] $Endpoint = 'rkws-rkwp-dev'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.RkwpTransportHarness\RKWorkspace.RkwpTransportHarness.csproj'
$arguments = @()

if ($Server) {
    $arguments += '--server'
    $arguments += '--endpoint'
    $arguments += $Endpoint
}
elseif ($Client) {
    $arguments += '--client'
    $arguments += '--endpoint'
    $arguments += $Endpoint
}
else {
    $arguments += '--smoke-test'
}

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $project -- @arguments
exit $LASTEXITCODE
