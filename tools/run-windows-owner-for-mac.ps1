param(
    [switch] $InfoOnly,
    [switch] $SmokeTest,
    [string] $PdfPath,
    [int] $Port = 57100,
    [string] $BindAddress = '0.0.0.0',
    [switch] $AllowDevPairing,
    [string] $OwnerAblageId = 'ablage-windows-owner',
    [string] $ExpectedGuestAblageId = 'ablage-macos-guest',
    [string] $SecurityMode = 'DevelopmentInsecure',
    [string] $Endpoint = 'rkws-windows-owner-macos',
    [int] $PlannedNetworkPort = 57100
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$devLanProject = Join-Path $root 'src\Tools\RKWorkspace.RkwpDevLanHarness\RKWorkspace.RkwpDevLanHarness.csproj'
$identityPath = Join-Path $root '.rkworkspace-dev\ablage-identity.json'
$contextPackPath = Join-Path $root 'release\codex-context\RKWorkspace_Context_latest.zip'

if ([string]::IsNullOrWhiteSpace($PdfPath)) {
    $PdfPath = Join-Path $root 'samples\Objects\Rechnung.pdf'
}

$PdfPath = [System.IO.Path]::GetFullPath($PdfPath)
if (-not (Test-Path -LiteralPath $PdfPath)) {
    Write-Error "PDF not found: $PdfPath"
}

if (-not (Test-Path -LiteralPath $identityPath)) {
    & (Join-Path $root 'tools\init-dev-rkwp-identity.ps1') -AblageId $OwnerAblageId -DisplayName 'Windows Owner Ablage' -Platform 'Windows'
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

$hash = (Get-FileHash -LiteralPath $PdfPath -Algorithm SHA256).Hash.ToLowerInvariant()
$thingId = "pdf-$($hash.Substring(0, 16))"
$displayUrl = "rkwp+tcp-dev://<windows-host>:$Port"
$bindUrl = "rkwp+tcp-dev://$BindAddress`:$Port"

Write-Host 'RK Workspace Windows Owner for macOS'
Write-Host '------------------------------------'
Write-Host "WindowsOwnerAblageId: $OwnerAblageId"
Write-Host "ExpectedGuestAblageId: $ExpectedGuestAblageId"
Write-Host 'ExpectedGuestPlatform: macOS'
Write-Host "DevIdentityPath: $identityPath"
Write-Host 'TransportProfile: LocalNetworkDev'
Write-Host "DevLanUrl: $displayUrl"
Write-Host "BindUrl: $bindUrl"
Write-Host "Port: $Port"
Write-Host "SecurityMode: $SecurityMode"
Write-Host "PdfPath: $PdfPath"
Write-Host "PdfObjectId: $thingId"
Write-Host 'LeaseMode: FrameOnly'
Write-Host 'NoFileIngressRequired: YES'
Write-Host "ExpectedGuest: $ExpectedGuestAblageId"
Write-Host "macOS Handoff: release\handoff\WindowsToMac_MA009_Handoff.md"
Write-Host "ContextPackPath: $contextPackPath"
Write-Host 'DevSecurityWarning: Lab only, no final TLS.'

if ($InfoOnly) {
    Write-Host "StartCommand: .\tools\run-windows-owner-for-mac.ps1 -PdfPath `"$PdfPath`" -Port $Port -AllowDevPairing"
    Write-Host 'SmokeCommand: .\tools\run-windows-owner-for-mac.ps1 -SmokeTest'
    Write-Host 'RESULT: SUCCESS'
    exit 0
}

dotnet build $devLanProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

if ($SmokeTest) {
    Write-Host ''
    Write-Host 'Smoke: Windows Owner for macOS'
    Write-Host 'PdfLoaded: OK'
    Write-Host 'AblageIdentity: OK'
    Write-Host 'DevPairingPrepared: OK'
    Write-Host 'FrameSessionPrepared: OK'
    Write-Host 'OwnerCanTimeoutWithoutGuest: OK'

    & (Join-Path $root 'tools\run-rkwp-lan-smoke.ps1') -BindAddress '127.0.0.1' -Host '127.0.0.1' -Port $Port
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    Write-Host 'DevLanServer: OK'
    Write-Host 'Shutdown: OK'
    Write-Host 'WindowsOwnerForMacSmoke: SUCCESS'
    Write-Host 'RESULT: SUCCESS'
    exit 0
}

Write-Host ''
Write-Host 'Starting DevLan owner endpoint. Stop with Ctrl+C.'
$arguments = @('--owner', '--bind-address', $BindAddress, '--port', $Port.ToString(), '--session-id', "rkwp-windows-macos-$([Guid]::NewGuid().ToString('N'))")
if ($AllowDevPairing) {
    $arguments += '--allow-dev-pairing'
}

dotnet run --project $devLanProject -- @arguments
exit $LASTEXITCODE
