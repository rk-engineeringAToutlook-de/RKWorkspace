param(
    [switch] $SmokeTest,
    [switch] $StartOwnerHost,
    [switch] $SkipGlassEdgePreflight,
    [string] $WindowsOwnerAddress = '192.168.163.11',
    [string] $BindAddress = '0.0.0.0',
    [int] $Port = 57120,
    [int] $Page = 1,
    [int] $Width = 1400,
    [string] $Policy = 'TrustedPersonalDevices',
    [string] $UwbProfile = 'Static'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Invoke-CheckedStep {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Label,

        [Parameter(Mandatory = $true)]
        [scriptblock] $Command,

        [string[]] $Required = @('RESULT: SUCCESS')
    )

    Write-Output "== $Label =="
    $output = & $Command 2>&1
    $exitCode = $LASTEXITCODE
    $output | ForEach-Object { Write-Output $_ }
    if ($exitCode -ne 0) {
        throw "$Label failed with exit code $exitCode."
    }

    $text = $output -join [Environment]::NewLine
    foreach ($line in $Required) {
        if (-not $text.Contains($line)) {
            throw "$Label failed because output did not contain: $line"
        }
    }
}

Write-Output 'RK Workspace MA017 Windows to macOS Glass Frame Pilot'
Write-Output '-----------------------------------------------------'
Write-Output 'Goal: Windows owns Rechnung.pdf; macOS shows only a memory-only PNG Frame.'
Write-Output 'AblageFlow: Windows -> Glass Edge -> Ablage macOS Frame'
Write-Output "WindowsOwnerAddressForMac: $WindowsOwnerAddress"
Write-Output "OwnerListen: $BindAddress`:$Port"
Write-Output "UwbProfile: $UwbProfile"
Write-Output "Policy: $Policy"

if (-not $SkipGlassEdgePreflight) {
    Invoke-CheckedStep 'Distance + Glass Edge preflight' {
        & (Join-Path $root 'tools\run-windows-pdf-frame-pilot.ps1') `
            -SmokeTest `
            -OpenPdf `
            -UseGlassEdge `
            -UseProximityFusion `
            -UwbProfile $UwbProfile `
            -PlaySequence `
            -Policy $Policy
    } @(
        'UseGlassEdge: YES',
        'ProximityMode: Fusion',
        "UwbProfile: $UwbProfile",
        'NearestAblageSelected: OK',
        'GlassEdgeActive: OK',
        'ObjectEnteringEdge: OK',
        'ObjectPlaced: OK',
        'OpenFrameNoFileIngress: SUCCESS',
        'NoFileIngress: SUCCESS',
        'RESULT: SUCCESS'
    )

    Write-Output 'GlassPortalBar: READY'
    Write-Output 'DistanceSelection: READY'
}

if ($SmokeTest) {
    Invoke-CheckedStep 'Windows owner rendered-frame smoke' {
        & (Join-Path $root 'tools\run-macos-pdf-frame-owner.ps1') `
            -SmokeTest `
            -BindAddress $BindAddress `
            -Port $Port `
            -Page $Page `
            -Width $Width
    } @(
        'RendererName: PopplerPdfFrameRenderer',
        'FrameFormat: PngFrame',
        'GuestHasPdfFile: NO',
        'GuestHasOriginalPath: NO',
        'OriginalFileBytes: NO',
        'NoFileIngress: SUCCESS',
        'RESULT: SUCCESS'
    )

    Write-Output 'MacGuestCommand:'
    Write-Output "  cd release/ma017/packages/macos-frame-guest"
    Write-Output "  swift run MacPdfFrameGuest --host $WindowsOwnerAddress --port $Port --auto-open"
    Write-Output 'MA017WindowsToMacGlassFramePilot: READY'
    Write-Output 'RESULT: SUCCESS'
    exit 0
}

Write-Output 'Next on macOS:'
Write-Output "  cd release/ma017/packages/macos-frame-guest"
Write-Output "  swift run MacPdfFrameGuest --host $WindowsOwnerAddress --port $Port --auto-open"
Write-Output ''
Write-Output 'Then keep this Windows owner host running:'
Write-Output "  .\tools\run-macos-pdf-frame-owner.ps1 -BindAddress $BindAddress -Port $Port -Page $Page -Width $Width"
Write-Output ''

if ($StartOwnerHost) {
    & (Join-Path $root 'tools\run-macos-pdf-frame-owner.ps1') `
        -BindAddress $BindAddress `
        -Port $Port `
        -Page $Page `
        -Width $Width
    exit $LASTEXITCODE
}

Write-Output 'OwnerHost: NOT_STARTED'
Write-Output 'Run again with -StartOwnerHost when the macOS Guest is ready to connect.'
Write-Output 'RESULT: READY'
exit 0
