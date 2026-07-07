param(
    [string] $PdfPath,
    [string] $WindowsOwnerAddress,
    [string] $BindAddress = '0.0.0.0',
    [int] $Port = 57120,
    [int] $Page = 1,
    [int] $Width = 1400,
    [switch] $PickImmediately,
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$signalPath = Join-Path $env:TEMP 'rkws-ma017-real-pdf-placement-ready.signal'
$diagnosticsPath = Join-Path $env:TEMP 'rkws-native-glass-overlay-diagnostics.log'

function Quote-PsString {
    param([Parameter(Mandatory = $true)][string] $Value)
    "'" + $Value.Replace("'", "''") + "'"
}

function Select-RealPdf {
    $dialogScript = @'
Add-Type -AssemblyName System.Windows.Forms
$dialog = New-Object System.Windows.Forms.OpenFileDialog
$dialog.Title = 'RK Workspace: echte PDF auswählen'
$dialog.Filter = 'PDF-Dateien (*.pdf)|*.pdf'
$dialog.Multiselect = $false
if ($dialog.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {
    $dialog.FileName
}
'@

    $selected = & powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -Command $dialogScript
    if ([string]::IsNullOrWhiteSpace($selected)) {
        throw 'Keine PDF ausgewählt. Der echte Live-Pilot braucht eine PDF von dir.'
    }

    $selected.Trim()
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

function Stop-OldPilotProcess {
    param([int] $Port)

    $connections = @(Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue)
    foreach ($connection in $connections) {
        $process = Get-Process -Id $connection.OwningProcess -ErrorAction SilentlyContinue
        if ($null -eq $process) {
            continue
        }

        if ($process.ProcessName -like 'RKWorkspace.*' -or $process.ProcessName -eq 'dotnet') {
            Write-Host ("Stoppe alten Pilot-Prozess auf Port {0}: {1} ({2})" -f $Port, $process.ProcessName, $process.Id)
            Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        }
    }

    Get-Process -Name 'RKWorkspace.Shell.NativeGlassOverlay.Windows' -ErrorAction SilentlyContinue |
        ForEach-Object {
            Write-Host ("Stoppe altes Glas-Overlay: {0}" -f $_.Id)
            Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
        }
}

if ([string]::IsNullOrWhiteSpace($PdfPath)) {
    if ($SmokeTest) {
        $PdfPath = Join-Path $root 'samples\Objects\Rechnung.pdf'
    } else {
        $PdfPath = $null
    }
}

$useRealGesture = [string]::IsNullOrWhiteSpace($PdfPath)
if (-not $useRealGesture) {
    $PdfPath = (Resolve-Path -LiteralPath $PdfPath).Path
    if (-not $PdfPath.EndsWith('.pdf', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Die ausgewählte Datei ist keine PDF: $PdfPath"
    }
}

if ([string]::IsNullOrWhiteSpace($WindowsOwnerAddress)) {
    $WindowsOwnerAddress = Get-DefaultOwnerAddress
}

if (Test-Path -LiteralPath $signalPath) {
    Remove-Item -LiteralPath $signalPath -Force
}

Write-Host 'RK Workspace MA017 echter PDF-Glasportal-Livepilot'
Write-Host '--------------------------------------------------'
Write-Host ("PDF: " + ($(if ($useRealGesture) { 'wartet auf echte PDF-Geste (PDF markieren, Strg+Alt+Leertaste druecken; F9 / F8 / Strg+Alt+P sind Fallback)' } else { $PdfPath })))
Write-Host "WindowsOwnerAddressForMac: $WindowsOwnerAddress"
Write-Host "OwnerListen: $BindAddress`:$Port"
Write-Host "PlacementSignal: $signalPath"
Write-Host "Diagnostics: $diagnosticsPath"
Write-Host 'Original bleibt auf Windows: JA'
Write-Host 'macOS bekommt nur PNG-Frame im Speicher: JA'

if ($SmokeTest) {
    & (Join-Path $root 'tools\run-macos-pdf-frame-owner.ps1') `
        -SmokeTest `
        -PdfPath $PdfPath `
        -BindAddress $BindAddress `
        -Port $Port `
        -Page $Page `
        -Width $Width `
        -WaitForPlacement `
        -PlacementSignalPath $signalPath
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    & (Join-Path $root 'tools\run-macos-pdf-frame-owner.ps1') `
        -PlacementGatingSmokeTest `
        -PdfPath $PdfPath `
        -BindAddress $BindAddress `
        -Port $Port `
        -Page $Page `
        -Width $Width `
        -WaitForPlacement `
        -PlacementSignalPath $signalPath
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    & (Join-Path $root 'tools\run-native-glass-overlay.ps1') `
        -SmokeTest `
        -SourcePdfPath $PdfPath `
        -PlacementSignalPath $signalPath
    exit $LASTEXITCODE
}

Stop-OldPilotProcess -Port $Port

$ownerParts = @(
    'Set-Location', (Quote-PsString $root) + ';',
    '.\tools\run-macos-pdf-frame-owner.ps1',
    '-BindAddress', (Quote-PsString $BindAddress),
    '-Port', $Port,
    '-Page', $Page,
    '-Width', $Width,
    '-WaitForPlacement',
    '-PlacementSignalPath', (Quote-PsString $signalPath)
)

if ($useRealGesture) {
    $ownerParts += '-DynamicPdfFromPlacementSignal'
} else {
    $ownerParts += '-PdfPath'
    $ownerParts += (Quote-PsString $PdfPath)
}

$ownerCommand = $ownerParts -join ' '

$overlayParts = @(
    'Set-Location', (Quote-PsString $root) + ';',
    '.\tools\run-native-glass-overlay.ps1',
    '-PlacementSignalPath', (Quote-PsString $signalPath),
    '-DiagnosticsPath', (Quote-PsString $diagnosticsPath),
    '-TargetDisplayName', (Quote-PsString 'Ablage macOS'),
    '-TargetDirection', (Quote-PsString 'Right')
)

if ($useRealGesture) {
    $overlayParts += '-RealPdfGesture'
} else {
    $overlayParts += '-SourcePdfPath'
    $overlayParts += (Quote-PsString $PdfPath)
    if ($PickImmediately) {
        $overlayParts += '-PickImmediately'
    }
}

$overlayCommand = $overlayParts -join ' '

Write-Host ''
Write-Host 'Starte Windows Owner Host...'
Start-Process powershell.exe -ArgumentList @('-NoExit', '-ExecutionPolicy', 'Bypass', '-Command', $ownerCommand) -WindowStyle Normal
Start-Sleep -Seconds 3

Write-Host 'Starte Windows Glas-Overlay...'
Start-Process powershell.exe -ArgumentList @('-NoExit', '-ExecutionPolicy', 'Bypass', '-Command', $overlayCommand) -WindowStyle Normal

Write-Host ''
Write-Host 'Auf macOS starten:'
Write-Host '  cd <GitHub-Repo>/release/ma017/packages/macos-frame-guest'
Write-Host "  swift run MacPdfFrameGuest --host $WindowsOwnerAddress --port $Port --wait-for-placement"
Write-Host ''
Write-Host 'Ablauf:'
Write-Host '  1. macOS-Befehl starten; dort steht: wartet auf Ablage.'
Write-Host '  2. Auf Windows irgendeine echte PDF im Explorer/Desktop markieren.'
Write-Host '  3. Strg+Alt+Leertaste drücken: die PDF wird virtuell genommen. Fallback: F9, F8 oder Strg+Alt+P.'
Write-Host '  4. Maus zur rechten Glaskante bewegen und einmal klicken.'
Write-Host '  5. Wenn du sie 10 Sekunden nicht wieder nimmst, erscheint der Frame auf macOS.'
Write-Host 'RESULT: READY'
