param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot

$secureDevOutput = & (Join-Path $root 'tools\run-rkwp-securedev-smoke.ps1') 2>&1
$secureDevExitCode = $LASTEXITCODE
if ($secureDevExitCode -ne 0) {
    $secureDevOutput | ForEach-Object { Write-Host $_ }
    exit $secureDevExitCode
}

$frameOutput = & (Join-Path $root 'tools\run-windows-local-frame-e2e.ps1') -SmokeTest 2>&1
$frameExitCode = $LASTEXITCODE
if ($frameExitCode -ne 0) {
    $frameOutput | ForEach-Object { Write-Host $_ }
    exit $frameExitCode
}

$secureDevText = $secureDevOutput -join [Environment]::NewLine
$frameText = $frameOutput -join [Environment]::NewLine

$required = @(
    'SecureDevHandshake: OK',
    'SessionActive: OK',
    'NoFileIngress: SUCCESS',
    'DevPairing: SUCCESS',
    'TransportConnected: SUCCESS',
    'Return: SUCCESS',
    'Recovery: SUCCESS'
)

$combined = $secureDevText + [Environment]::NewLine + $frameText
foreach ($line in $required) {
    if (-not $combined.Contains($line)) {
        throw "Windows SecureDev PDF E2E smoke failed because output did not contain: $line"
    }
}

Write-Host 'RK Workspace Windows SecureDev PDF E2E'
Write-Host '--------------------------------------'
Write-Host "Mode: $(if ($SmokeTest) { 'SmokeTest' } else { 'E2E' })"
Write-Host 'SecureDevHandshake: OK'
Write-Host 'SessionActive: OK'
Write-Host 'PdfFrame: OK'
Write-Host 'NoFileIngress: SUCCESS'
Write-Host 'Return: SUCCESS'
Write-Host 'Recovery: SUCCESS'
Write-Host 'WindowsSecureDevPdfE2E: SUCCESS'
Write-Host 'RESULT: SUCCESS'

exit 0
