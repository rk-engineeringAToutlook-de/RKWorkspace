param(
    [string] $WindowsOwnerAddress,
    [string] $BindAddress = '0.0.0.0',
    [int] $Port = 57120,
    [string] $TargetDisplayName = 'Ablage macOS',
    [string] $TargetDirection = 'Right',
    [double] $TargetDistanceMeters = 0.30,
    [string] $TargetDistanceSource = 'ManualMap',
    [switch] $SmokeTest,
    [switch] $StopOnly
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$ownerProject = Join-Path $root 'src\Tools\RKWorkspace.MacPdfFrameOwnerHost\RKWorkspace.MacPdfFrameOwnerHost.csproj'
$overlayProject = Join-Path $root 'src\Shell\RKWorkspace.Shell.NativeGlassOverlay.Windows\RKWorkspace.Shell.NativeGlassOverlay.Windows.csproj'
$ownerExe = Join-Path $root 'src\Tools\RKWorkspace.MacPdfFrameOwnerHost\bin\Debug\net8.0\RKWorkspace.MacPdfFrameOwnerHost.exe'
$overlayExe = Join-Path $root 'src\Shell\RKWorkspace.Shell.NativeGlassOverlay.Windows\bin\Debug\net8.0-windows\RKWorkspace.Shell.NativeGlassOverlay.Windows.exe'
$signalPath = Join-Path $env:TEMP 'rkws-ma017-real-pdf-placement-ready.signal'
$diagnosticsPath = Join-Path $env:TEMP 'rkws-native-glass-overlay-diagnostics.log'
$ownerOut = Join-Path $env:TEMP 'rkws-ma017-pdf-owner.out.log'
$ownerErr = Join-Path $env:TEMP 'rkws-ma017-pdf-owner.err.log'

function Join-ProcessArguments {
    param([Parameter(Mandatory = $true)][string[]] $Arguments)

    ($Arguments | ForEach-Object {
        if ($_ -match '[\s"]') {
            '"' + $_.Replace('\', '\\').Replace('"', '\"') + '"'
        } else {
            $_
        }
    }) -join ' '
}

function Get-DefaultOwnerAddress {
    $candidates = @(Get-NetIPAddress -AddressFamily IPv4 |
        Where-Object {
            $_.IPAddress -notlike '127.*' `
                -and $_.PrefixOrigin -ne 'WellKnown' `
                -and $_.InterfaceAlias -notmatch 'VMware|Virtual|Loopback|Talk2m|eCatcher|Docker|Hyper-V'
        })

    $address = $candidates |
        Sort-Object @{
            Expression = {
                if ($_.InterfaceAlias -match 'WLAN|Wi-Fi|Wireless') { 0 }
                elseif ($_.InterfaceAlias -match 'Ethernet') { 1 }
                else { 2 }
            }
        }, InterfaceMetric |
        Select-Object -First 1 -ExpandProperty IPAddress

    if ([string]::IsNullOrWhiteSpace($address)) {
        return '127.0.0.1'
    }

    $address
}

function Stop-RKWorkspacePilotProcesses {
    param([int] $Port)

    Get-Process -Name 'RKWorkspace.Shell.NativeGlassOverlay.Windows' -ErrorAction SilentlyContinue |
        ForEach-Object {
            Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
        }

    Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue |
        ForEach-Object {
            $process = Get-Process -Id $_.OwningProcess -ErrorAction SilentlyContinue
            if ($null -eq $process) {
                return
            }

            if ($process.ProcessName -like 'RKWorkspace.*' -or $process.ProcessName -eq 'dotnet') {
                Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
            }
        }
}

if ([string]::IsNullOrWhiteSpace($WindowsOwnerAddress)) {
    $WindowsOwnerAddress = Get-DefaultOwnerAddress
}

Write-Host 'RK Workspace MA017 Produkt-Pilot: echte PDF ueber Glaskante'
Write-Host '----------------------------------------------------------'
Write-Host "WindowsOwnerAddressForMac: $WindowsOwnerAddress"
Write-Host "Target: $TargetDisplayName / $TargetDirection / $TargetDistanceMeters m / $TargetDistanceSource"
Write-Host "PlacementSignal: $signalPath"
Write-Host "OverlayDiagnostics: $diagnosticsPath"
Write-Host "OwnerLog: $ownerOut"
Write-Host "OwnerErrorLog: $ownerErr"

Stop-RKWorkspacePilotProcesses -Port $Port

if ($StopOnly) {
    Write-Host 'RESULT: STOPPED'
    exit 0
}

if (Test-Path -LiteralPath $signalPath) {
    Remove-Item -LiteralPath $signalPath -Force
}

if (Test-Path -LiteralPath $diagnosticsPath) {
    Remove-Item -LiteralPath $diagnosticsPath -Force
}

Write-Host ''
Write-Host 'Baue Windows Owner Host...'
dotnet build $ownerProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host 'Baue natives Glas-Overlay...'
dotnet build $overlayProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

if ($SmokeTest) {
    & (Join-Path $root 'tools\run-macos-pdf-frame-owner.ps1') `
        -SmokeTest `
        -MemoryPdfFrame `
        -BindAddress $BindAddress `
        -Port $Port `
        -WaitForPlacement `
        -PlacementSignalPath $signalPath
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    & (Join-Path $root 'tools\run-macos-pdf-frame-owner.ps1') `
        -DynamicPlacementSmokeTest `
        -PdfPath (Join-Path $root 'samples\Objects\Rechnung.pdf') `
        -BindAddress $BindAddress `
        -Port $Port `
        -WaitForPlacement `
        -DynamicPdfFromPlacementSignal `
        -MemoryPdfFrame `
        -PlacementSignalPath $signalPath
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    & (Join-Path $root 'tools\run-native-glass-overlay.ps1') `
        -SmokeTest `
        -RealPdfGesture `
        -ContextListener `
        -InstantPlacementSignal `
        -PlacementSignalPath $signalPath `
        -DiagnosticsPath $diagnosticsPath `
        -TargetDisplayName $TargetDisplayName `
        -TargetDirection $TargetDirection `
        -TargetDistanceMeters $TargetDistanceMeters `
        -TargetDistanceSource $TargetDistanceSource
    exit $LASTEXITCODE
}

& (Join-Path $root 'tools\enable-windows-pdf-context.ps1')
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$ownerArgs = @(
    '--bind-address', $BindAddress,
    '--port', [string]$Port,
    '--wait-for-placement',
    '--dynamic-pdf-from-placement-signal',
    '--memory-pdf-frame',
    '--placement-signal', $signalPath
)

$overlayArgs = @(
    '--real-pdf-gesture',
    '--context-listener',
    '--instant-placement-signal',
    '--placement-signal', $signalPath,
    '--diagnostics', $diagnosticsPath,
    '--target-name', $TargetDisplayName,
    '--target-direction', $TargetDirection,
    '--target-distance-meters', $TargetDistanceMeters.ToString([System.Globalization.CultureInfo]::InvariantCulture),
    '--target-distance-source', $TargetDistanceSource
)

Write-Host ''
Write-Host 'Starte Windows Owner Host im Hintergrund...'
$ownerProcess = Start-Process `
    -FilePath $ownerExe `
    -ArgumentList (Join-ProcessArguments $ownerArgs) `
    -WindowStyle Hidden `
    -RedirectStandardOutput $ownerOut `
    -RedirectStandardError $ownerErr `
    -PassThru

Start-Sleep -Milliseconds 700

Write-Host 'Starte native Glaskante...'
$overlayProcess = Start-Process `
    -FilePath $overlayExe `
    -ArgumentList (Join-ProcessArguments $overlayArgs) `
    -WindowStyle Normal `
    -PassThru

Write-Host ''
Write-Host 'Auf macOS jetzt starten:'
Write-Host '  cd <GitHub-Repo>/release/ma017/packages/macos-frame-guest'
Write-Host "  swift run MacPdfFrameGuest --host $WindowsOwnerAddress --port $Port --wait-for-placement"
Write-Host ''
Write-Host 'Windows-Testablauf:'
Write-Host '  1. Auf macOS muss stehen: wartet auf Ablage am Glasrand.'
Write-Host '  2. Auf Windows irgendeine eigene PDF im Explorer/Desktop rechtsklicken.'
Write-Host '  3. "Mit RK Workspace nehmen" auswaehlen.'
Write-Host '  4. Die PDF muss sofort als Handobjekt erscheinen.'
Write-Host '  5. Zur rechten Glaskante bewegen und dort ablegen.'
Write-Host '  6. Auf macOS muss die PDF als PDF-Frame erscheinen.'
Write-Host ''
Write-Host "OwnerPid: $($ownerProcess.Id)"
Write-Host "OverlayPid: $($overlayProcess.Id)"
Write-Host 'RESULT: READY'
