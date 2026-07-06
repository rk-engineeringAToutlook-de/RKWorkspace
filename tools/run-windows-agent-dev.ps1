param(
    [switch] $Start,
    [switch] $Stop,
    [switch] $Status,
    [switch] $Identity,
    [switch] $Transport,
    [switch] $FrameOwner,
    [switch] $GuestSurface,
    [switch] $SmokeTest,
    [string] $AblageId = 'ablage-windows-dev-agent',
    [string] $BindAddress = '127.0.0.1',
    [int] $Port = 57120
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Agents\RKWorkspace.Agent.Windows\RKWorkspace.Agent.Windows.csproj'

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$arguments = @('--ablage-id', $AblageId, '--bind-address', $BindAddress, '--port', $Port.ToString())
if ($Start) { $arguments += '--start' }
if ($Stop) { $arguments += '--stop' }
if ($Status) { $arguments += '--status' }
if ($Identity) { $arguments += '--identity' }
if ($Transport) { $arguments += '--transport' }
if ($FrameOwner) { $arguments += '--frame-owner' }
if ($GuestSurface) { $arguments += '--guest-surface' }
if ($SmokeTest) {
    $arguments += '--smoke-test'
}

$output = dotnet run --project $project -- @arguments 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Output $_ }
if ($exitCode -ne 0) {
    exit $exitCode
}

if ($SmokeTest) {
    $text = $output -join [Environment]::NewLine
    $required = @(
        'AgentStarted: OK',
        'NoInstallationRequired: OK',
        'AblageIdentity: OK',
        'IdentityMode: OK',
        'TransportMode: OK',
        'FrameOwnerMode: OK',
        'GuestSurfaceMode: OK',
        'StatusMode: OK',
        'StopMode: OK',
        'RkwpComponentsReachable: OK',
        'Shutdown: OK',
        'WindowsAgentDevSmoke: SUCCESS',
        'RESULT: SUCCESS'
    )

    foreach ($line in $required) {
        if (-not $text.Contains($line)) {
            throw "Windows Agent Dev smoke failed because output did not contain: $line"
        }
    }
}

exit 0
