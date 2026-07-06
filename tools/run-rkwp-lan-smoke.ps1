param(
    [string] $BindAddress = '127.0.0.1',
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

$output = dotnet run --project $project -- --smoke-test --bind-address $BindAddress --host $TargetHost --port $Port 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Output $_ }
if ($exitCode -ne 0) {
    exit $exitCode
}

$text = $output -join [Environment]::NewLine
$required = @(
    'OwnerServerStarted: OK',
    'GuestClientConnected: OK',
    'AblageHello: OK',
    'AblageCapabilities: OK',
    'DevPairing: OK',
    'SecureSessionSpike: PREPARED',
    'Heartbeat: OK',
    'FrameUpdate: OK',
    'Disconnect: OK',
    'NoFileIngress: SUCCESS',
    'RESULT: SUCCESS'
)

foreach ($line in $required) {
    if (-not $text.Contains($line)) {
        throw "RKWP DevLan smoke failed because output did not contain: $line"
    }
}

exit 0
