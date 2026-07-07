param(
    [Parameter(Position = 0)]
    [string] $PdfPath,
    [int] $Port = 57120,
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot

if ($SmokeTest) {
    & (Join-Path $root 'tools\run-ma017-real-pdf-glass-portal-live.ps1') -SmokeTest
    exit $LASTEXITCODE
}

if ([string]::IsNullOrWhiteSpace($PdfPath)) {
    throw 'Keine PDF uebergeben. Kontextaufruf erwartet einen PDF-Dateipfad.'
}

$resolvedPdfPath = (Resolve-Path -LiteralPath $PdfPath).Path
if (-not $resolvedPdfPath.EndsWith('.pdf', [StringComparison]::OrdinalIgnoreCase)) {
    throw "Die Kontextdatei ist keine PDF: $resolvedPdfPath"
}

Write-Host 'RK Workspace Windows-Kontext: PDF nehmen'
Write-Host '----------------------------------------'
Write-Host "PDF: $resolvedPdfPath"
Write-Host 'Modus: portable, keine Shell-Extension, keine Admin-Rechte'

& (Join-Path $root 'tools\run-ma017-real-pdf-glass-portal-live.ps1') `
    -PdfPath $resolvedPdfPath `
    -Port $Port `
    -PickImmediately

exit $LASTEXITCODE
