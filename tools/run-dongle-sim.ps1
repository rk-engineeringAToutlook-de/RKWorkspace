param(
    [switch] $SmokeTest,
    [string] $Profile = 'MovingCloser',
    [switch] $UseFusion,
    [double] $ConfidenceThreshold = 0.70,
    [string] $DongleId = 'dongle-lab-anchor-01'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.DongleAnchorSim\RKWorkspace.DongleAnchorSim.csproj'
$arguments = @(
    '--profile',
    $Profile,
    '--confidence-threshold',
    ([string]::Format([Globalization.CultureInfo]::InvariantCulture, '{0}', $ConfidenceThreshold)),
    '--dongle-id',
    $DongleId
)

if ($SmokeTest) {
    $arguments += '--smoke-test'
}

if ($UseFusion) {
    $arguments += '--use-fusion'
}

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
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
        'RK Workspace Dongle Anchor Simulation',
        'Provider: SimulatedDongleAnchorProvider',
        'ProviderStatus: Simulated',
        'PrivacyMode: EphemeralLab',
        'BleAnchor: OK',
        'UwbAnchor: OK',
        'NearestAblage: Ablage iPad via Dongle',
        'EdgeDirection: Right',
        'DirectionDistanceConfidence: OK',
        'DongleAnchorCli: SUCCESS',
        'RESULT: SUCCESS'
    )

    if ($UseFusion) {
        $required += 'ManualMapFusion: OK'
    }

    foreach ($line in $required) {
        if (-not $text.Contains($line)) {
            throw "Dongle simulation smoke failed because output did not contain: $line"
        }
    }
}

exit 0
