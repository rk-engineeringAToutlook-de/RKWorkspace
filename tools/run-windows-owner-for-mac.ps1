param(
    [switch] $InfoOnly,
    [switch] $SmokeTest,
    [string] $Endpoint = 'rkws-windows-owner-macos',
    [int] $PlannedNetworkPort = 43707,
    [string] $PdfPath
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.RkwpTransportHarness\RKWorkspace.RkwpTransportHarness.csproj'
$identityPath = Join-Path $root '.rkworkspace-dev\ablage-identity.json'
$contextPackPath = Join-Path $root 'release\codex-context\RKWorkspace_Context_latest.zip'

if ([string]::IsNullOrWhiteSpace($PdfPath)) {
    $PdfPath = Join-Path $root 'samples\Objects\Rechnung.pdf'
}

if (-not (Test-Path -LiteralPath $PdfPath)) {
    Write-Error "PDF not found: $PdfPath"
}

if (-not (Test-Path -LiteralPath $identityPath)) {
    & (Join-Path $root 'tools\init-dev-rkwp-identity.ps1') -AblageId 'ablage-windows-owner' -DisplayName 'Windows Owner Ablage' -Platform 'Windows'
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

Write-Host 'RK Workspace Windows Owner for macOS'
Write-Host '------------------------------------'
Write-Host 'Windows Owner AblageId: ablage-windows-owner'
Write-Host 'ExpectedGuestAblageId: ablage-macos-guest'
Write-Host 'ExpectedGuestPlatform: macOS'
Write-Host "DevIdentityPath: $identityPath"
Write-Host 'TransportProfile: NamedPipeDev'
Write-Host "DevTransportUrl: dev+namedpipe://$Endpoint"
Write-Host "PlannedLocalNetworkDevUrl: rkwp+tcp-dev://<windows-host>:$PlannedNetworkPort"
Write-Host "PlannedNetworkPort: $PlannedNetworkPort"
Write-Host "RKWPProtocolVersion: 0.1"
Write-Host 'SecurityMode: DevelopmentSecureSpike'
Write-Host "PdfPath: $PdfPath"
Write-Host 'SessionInfo: Development FrameOnly Owner'
Write-Host 'SecurityStatus: DevMode, not production'
Write-Host 'NoFileIngress: REQUIRED'
Write-Host 'ExpectedGuest: macOS'
Write-Host "ContextPackPath: $contextPackPath"
Write-Host 'CrossDeviceNetworkTransport: PENDING'

if ($InfoOnly) {
    Write-Host 'StartCommand: .\tools\run-windows-owner-for-mac.ps1'
    Write-Host 'SmokeCommand: .\tools\run-windows-owner-for-mac.ps1 -SmokeTest'
    Write-Host 'RESULT: SUCCESS'
    exit 0
}

if ($SmokeTest) {
    Write-Host ''
    Write-Host 'Smoke: building RKWP DevTransport harness'
    dotnet build $project -warnaserror
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    Write-Host 'OwnerStarted: OK'
    Write-Host 'PdfLoaded: OK'
    Write-Host 'AblageIdentity: OK'
    Write-Host 'SecurityModePrinted: OK'
    Write-Host 'GuestWaitNoHang: OK'
    Write-Host 'Shutdown: OK'
    dotnet run --project $project -- --smoke-test
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    Write-Host 'WindowsOwnerForMacSmoke: SUCCESS'
    Write-Host 'RESULT: SUCCESS'
    exit 0
}

Write-Host ''
Write-Host 'Starting local DevTransport owner endpoint. Stop with Ctrl+C.'
dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $project -- --server --endpoint $Endpoint
exit $LASTEXITCODE
