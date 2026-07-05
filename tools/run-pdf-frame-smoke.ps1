param(
    [string] $PdfPath
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($PdfPath)) {
    $PdfPath = Join-Path $root 'samples\Objects\Rechnung.pdf'
}

Write-Host 'RK Workspace PDF Frame Smoke'
Write-Host '----------------------------'

$ownerOutput = & (Join-Path $root 'tools\run-pdf-frame-owner.ps1') -PdfPath $PdfPath 2>&1
$ownerExitCode = $LASTEXITCODE
$ownerOutput | ForEach-Object { Write-Host $_ }
if ($ownerExitCode -ne 0) {
    throw "PDF Frame Smoke failed in owner step with exit code $ownerExitCode."
}

$guestOutput = & (Join-Path $root 'tools\run-frame-guest.ps1') 2>&1
$guestExitCode = $LASTEXITCODE
$guestOutput | ForEach-Object { Write-Host $_ }
if ($guestExitCode -ne 0) {
    throw "PDF Frame Smoke failed in guest step with exit code $guestExitCode."
}

Write-Output 'PdfFrameSmoke: SUCCESS'
Write-Output 'RESULT: SUCCESS'
