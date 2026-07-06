param(
    [string] $BindAddress = '0.0.0.0',
    [int] $Port = 57100,
    [string] $SessionId = '',
    [switch] $AllowDevPairing
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.RkwpDevLanHarness\RKWorkspace.RkwpDevLanHarness.csproj'
$arguments = @('--owner', '--bind-address', $BindAddress, '--port', $Port.ToString())

if (-not [string]::IsNullOrWhiteSpace($SessionId)) {
    $arguments += '--session-id'
    $arguments += $SessionId
}

if ($AllowDevPairing) {
    $arguments += '--allow-dev-pairing'
}

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $project -- @arguments
exit $LASTEXITCODE
