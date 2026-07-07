param(
    [switch] $Disable,
    [switch] $SkipContextMenu,
    [switch] $SkipSendTo
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$launcher = Join-Path $root 'tools\invoke-windows-pdf-context-pick.ps1'
$contextKey = 'HKCU:\Software\Classes\SystemFileAssociations\.pdf\shell\RKWorkspacePick'
$commandKey = Join-Path $contextKey 'command'
$sendToDirectory = Join-Path $env:APPDATA 'Microsoft\Windows\SendTo'
$sendToShortcut = Join-Path $sendToDirectory 'RK Workspace nehmen.lnk'

if (-not (Test-Path -LiteralPath $launcher)) {
    throw "Launcher nicht gefunden: $launcher"
}

if ($Disable) {
    if (Test-Path -LiteralPath $contextKey) {
        Remove-Item -LiteralPath $contextKey -Recurse -Force
    }

    if (Test-Path -LiteralPath $sendToShortcut) {
        Remove-Item -LiteralPath $sendToShortcut -Force
    }

    Write-Host 'RK Workspace Windows-Kontext entfernt.'
    Write-Host 'Portable Nutzung bleibt weiterhin moeglich:'
    Write-Host "  .\tools\run-windows-pdf-context-pick.ps1 -PdfPath <PDF>"
    exit 0
}

if (-not $SkipContextMenu) {
    New-Item -Path $contextKey -Force | Out-Null
    New-Item -Path $commandKey -Force | Out-Null
    New-ItemProperty -Path $contextKey -Name 'MUIVerb' -Value 'Mit RK Workspace nehmen' -PropertyType String -Force | Out-Null
    New-ItemProperty -Path $contextKey -Name 'Icon' -Value 'imageres.dll,-102' -PropertyType String -Force | Out-Null
    Set-Item -Path $commandKey -Value "powershell.exe -NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$launcher`" -PdfPath `"%1`""
}

if (-not $SkipSendTo) {
    New-Item -ItemType Directory -Path $sendToDirectory -Force | Out-Null
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($sendToShortcut)
    $shortcut.TargetPath = "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe"
    $shortcut.Arguments = "-NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$launcher`" -PdfPath"
    $shortcut.WorkingDirectory = $root
    $shortcut.Description = 'RK Workspace: markierte PDF nehmen'
    $shortcut.Save()
}

Write-Host 'RK Workspace Windows-Kontext vorbereitet.'
Write-Host '----------------------------------------'
Write-Host 'Explorer-Kontextmenue: Mit RK Workspace nehmen'
Write-Host 'SendTo-Fallback: RK Workspace nehmen'
Write-Host 'Keine Admin-Rechte verwendet.'
Write-Host 'Keine Shell-Extension installiert.'
Write-Host 'Kontextklick sendet nur an die laufende Shell; er startet keinen Pilot.'
Write-Host 'Gekapselter Fallback bleibt:'
Write-Host "  .\tools\run-windows-pdf-context-pick.ps1 -PdfPath <PDF>"
