$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.RkwpSecureDevHarness\RKWorkspace.RkwpSecureDevHarness.csproj'

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$output = dotnet run --project $project -- --smoke-test 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Output $_ }
if ($exitCode -ne 0) {
    exit $exitCode
}

$text = $output -join [Environment]::NewLine
$required = @(
    'OwnerIdentity: OK',
    'GuestIdentity: OK',
    'SecurityMode: DevelopmentAuthenticated',
    'TransportProfile: SecureDev/NamedPipeDevFallback',
    'SecureSessionRequired: True',
    'TLS: NO',
    'HandshakeState: Active',
    'TransportStarted: OK',
    'GuestConnected: OK',
    'AblageHello: OK',
    'IdentityExchange: OK',
    'SecureDevHandshake: OK',
    'SessionActive: OK',
    'Heartbeat: OK',
    'FrameUpdate: OK',
    'Shutdown: OK',
    'FallbackClearlyMarked: OK',
    'RESULT: SUCCESS'
)

foreach ($line in $required) {
    if (-not $text.Contains($line)) {
        throw "RKWP SecureDev smoke failed because output did not contain: $line"
    }
}

exit 0
