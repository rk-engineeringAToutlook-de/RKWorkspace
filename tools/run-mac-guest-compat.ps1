param(
    [switch] $SmokeTest,
    [string] $ConnectToOwner,
    [switch] $ReplaySample,
    [string] $BindAddress = '127.0.0.1',
    [Alias('Host')]
    [string] $TargetHost = '127.0.0.1',
    [int] $Port = 57101
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.MacGuestCompatibilityHarness\RKWorkspace.MacGuestCompatibilityHarness.csproj'

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$arguments = @()
if ($ReplaySample) {
    $arguments += '--replay-sample'
}
elseif (-not [string]::IsNullOrWhiteSpace($ConnectToOwner)) {
    $arguments += @('--connect-to-owner', $ConnectToOwner)
}
else {
    $arguments += @('--smoke-test', '--bind-address', $BindAddress, '--host', $TargetHost, '--port', $Port.ToString())
}

$output = dotnet run --project $project -- @arguments 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Output $_ }
if ($exitCode -ne 0) {
    exit $exitCode
}

if ($SmokeTest -or (-not $ReplaySample -and [string]::IsNullOrWhiteSpace($ConnectToOwner))) {
    $text = $output -join [Environment]::NewLine
    $required = @(
        'MacGuestIdentity: OK',
        'Platform: MacOS',
        'FrameView: OK',
        'FrameInput: OFF',
        'Haptics: PLANNED',
        'GlassEdge: PLANNED',
        'NoFileIngressCapability: OK',
        'OwnershipTransfer: OFF',
        'SimulatedOwner: OK',
        'AblageHello: OK',
        'AblageCapabilities: OK',
        'FrameSessionReady: OK',
        'Heartbeat: OK',
        'Return: SUCCESS',
        'GuestHasPdfFile: NO',
        'GuestHasOriginalPath: NO',
        'OriginalFileBytes: NO',
        'NoFileIngress: SUCCESS',
        'RESULT: SUCCESS'
    )

    foreach ($line in $required) {
        if (-not $text.Contains($line)) {
            throw "macOS Guest compatibility smoke failed because output did not contain: $line"
        }
    }
}

exit 0
