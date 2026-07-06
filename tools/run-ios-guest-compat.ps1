param(
    [switch] $SmokeTest,
    [switch] $ReplaySample
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.iOSGuestCompatibilityHarness\RKWorkspace.iOSGuestCompatibilityHarness.csproj'

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$arguments = @()
if ($ReplaySample) {
    $arguments += '--replay-sample'
}
else {
    $arguments += '--smoke-test'
}

$output = dotnet run --project $project -- @arguments 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Output $_ }
if ($exitCode -ne 0) {
    exit $exitCode
}

if ($SmokeTest -or -not $ReplaySample) {
    $text = $output -join [Environment]::NewLine
    $required = @(
        'iOSGuestIdentity: OK',
        'PrimaryPlatform: IPadOS',
        'PhonePlatform: IOS',
        'FrameView: OK',
        'TouchInput: PLANNED',
        'Haptics: PLANNED',
        'NoFileIngressCapability: OK',
        'GlobalAppCapture: FALSE',
        'XcodeBridge: OK',
        'GuestHasPdfFile: NO',
        'GuestHasOriginalPath: NO',
        'OriginalFileBytes: NO',
        'NoFileIngress: SUCCESS',
        'RESULT: SUCCESS'
    )

    foreach ($line in $required) {
        if (-not $text.Contains($line)) {
            throw "iOS/iPadOS guest compatibility smoke failed because output did not contain: $line"
        }
    }
}

exit 0
