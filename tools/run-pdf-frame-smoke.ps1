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

if (-not $combinedText.Contains('MultiPageNavigation: OK')) {
    throw 'PDF Frame Smoke failed because multipage navigation was not prepared.'
}

if (-not $combinedText.Contains('PageFrameUpdates:')) {
    throw 'PDF Frame Smoke failed because page frame updates were not printed.'
}

$realRenderer = $combinedText.Contains('RendererName: PopplerPdfFrameRenderer')
$blockedRenderer = $combinedText.Contains('RendererStatus: RendererBlocked') -and $combinedText.Contains('IsPlaceholder: True')
if ($realRenderer) {
    if (-not $combinedText.Contains('FrameFormat: PngFrame')) {
        throw 'PDF Frame Smoke failed because real renderer did not produce PngFrame.'
    }

    if (-not $combinedText.Contains('IsPlaceholder: False')) {
        throw 'PDF Frame Smoke failed because real renderer did not report IsPlaceholder: False.'
    }

    if (-not $combinedText.Contains('FrameIsPdfFile: NO')) {
        throw 'PDF Frame Smoke failed because rendered frame was not clearly marked as non-PDF.'
    }
}
elseif ($blockedRenderer) {
    Write-Output 'RendererBlocker: EXPLICIT'
}
else {
    throw 'PDF Frame Smoke failed because renderer was neither real nor explicitly blocked.'
}

if (-not $combinedText.Contains('FrameCacheScope: MemoryOnly')) {
    throw 'PDF Frame Smoke failed because memory-only frame cache was not used.'
}

if (-not $combinedText.Contains('FrameCacheOriginalBytes: NO')) {
    throw 'PDF Frame Smoke failed because frame cache no-file-ingress was not explicit.'
}

if (-not $combinedText.Contains('FrameCacheClose: CLEARED')) {
    throw 'PDF Frame Smoke failed because frame cache was not cleared after frame close.'
}

if (-not $combinedText.Contains('DevInspectableDevelopment: OK')) {
    throw 'PDF Frame Smoke failed because Development DevInspectable cache policy was not allowed.'
}

if (-not $combinedText.Contains('CriticalBlocksDevInspectable: OK')) {
    throw 'PDF Frame Smoke failed because CriticalInfrastructure did not block DevInspectable cache.'
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
