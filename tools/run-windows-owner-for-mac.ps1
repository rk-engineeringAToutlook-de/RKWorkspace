param(
    [switch] $InfoOnly,
    [string] $Endpoint = 'rkws-windows-owner-macos',
    [int] $PlannedNetworkPort = 43707,
    [string] $PdfPath
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.RkwpTransportHarness\RKWorkspace.RkwpTransportHarness.csproj'

if ([string]::IsNullOrWhiteSpace($PdfPath)) {
    $PdfPath = Join-Path $root 'samples\Objects\Rechnung.pdf'
}

Write-Host 'RK Workspace Windows Owner for macOS'
Write-Host '------------------------------------'
Write-Host 'OwnerAblageId: ablage-windows-owner'
Write-Host 'ExpectedGuestAblageId: ablage-macos-guest'
Write-Host 'ExpectedGuestPlatform: macOS'
Write-Host 'TransportProfile: NamedPipeDev'
Write-Host "DevTransportUrl: dev+namedpipe://$Endpoint"
Write-Host "PlannedNetworkPort: $PlannedNetworkPort"
Write-Host "PdfPath: $PdfPath"
Write-Host 'SessionInfo: Development FrameOnly Owner'
Write-Host 'SecurityStatus: DevMode'
Write-Host 'NoFileIngress: REQUIRED'
Write-Host 'CrossDeviceNetworkTransport: PENDING'

if ($InfoOnly) {
    Write-Host 'StartCommand: .\tools\run-windows-owner-for-mac.ps1'
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
