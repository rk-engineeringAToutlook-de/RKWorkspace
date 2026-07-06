param(
    [string] $PdfPath
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($PdfPath)) {
    $PdfPath = Join-Path $root 'samples\Objects\Rechnung.pdf'
}

if (-not (Test-Path -LiteralPath $PdfPath)) {
    throw "PDF Frame Smoke failed because sample PDF does not exist: $PdfPath"
}

Write-Host 'RK Workspace PDF Frame Smoke'
Write-Host '----------------------------'
Write-Host 'SamplePdf: OK'

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

$combinedText = (($ownerOutput + $guestOutput) -join [Environment]::NewLine)
if (-not $combinedText.Contains('FrameRepresentation: OK')) {
    throw 'PDF Frame Smoke failed because output did not contain FrameRepresentation: OK.'
}

if (-not $combinedText.Contains('GuestHasPdfFile: NO')) {
    throw 'PDF Frame Smoke failed because output did not contain GuestHasPdfFile: NO.'
}

if (-not $combinedText.Contains('OriginalFileBytes: NO')) {
    throw 'PDF Frame Smoke failed because output did not contain OriginalFileBytes: NO.'
}

if (-not $combinedText.Contains('Zurueckgegeben')) {
    throw 'PDF Frame Smoke failed because output did not contain Zurueckgegeben.'
}

if (-not $combinedText.Contains('OwnerVisibleStatus: wartet auf Rueckgabe')) {
    throw 'PDF Frame Smoke failed because owner visible status was not shown.'
}

if (-not $combinedText.Contains('GuestVisibleStatus: liegt hier im Frame')) {
    throw 'PDF Frame Smoke failed because guest visible status was not shown.'
}

if (-not $combinedText.Contains('VisibleStateLanguage: SUCCESS')) {
    throw 'PDF Frame Smoke failed because visible state language validation did not pass.'
}

Write-Output 'NoFileIngress: SUCCESS'
Write-Output 'PdfFrameSmoke: SUCCESS'
Write-Output 'RESULT: SUCCESS'
