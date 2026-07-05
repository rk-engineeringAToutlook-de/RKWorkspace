param(
    [string] $PdfPath
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.PdfFrameOwner\RKWorkspace.PdfFrameOwner.csproj'

if ([string]::IsNullOrWhiteSpace($PdfPath)) {
    $PdfPath = Join-Path $root 'samples\Objects\Rechnung.pdf'
}

Write-Host 'PDF Frame Owner Smoke'
Write-Host '---------------------'
dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$output = dotnet run --project $project -- --pdf-path $PdfPath 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Output $_ }
if ($exitCode -ne 0) {
    exit $exitCode
}

$text = $output -join [Environment]::NewLine
if (-not $text.Contains('OwnerLocked: OK')) {
    throw 'PDF Frame Owner Smoke failed because output did not contain OwnerLocked: OK.'
}

if (-not $text.Contains('LeaseState: Active')) {
    throw 'PDF Frame Owner Smoke failed because output did not contain LeaseState: Active.'
}

if (-not $text.Contains('FrameState: Active')) {
    throw 'PDF Frame Owner Smoke failed because output did not contain FrameState: Active.'
}

if (-not $text.Contains('FrameRepresentation: OK')) {
    throw 'PDF Frame Owner Smoke failed because output did not contain FrameRepresentation: OK.'
}

if (-not $text.Contains('PDF ist als Frame ausgeliehen')) {
    throw 'PDF Frame Owner Smoke failed because output did not contain the frame-only loan wording.'
}

if (-not $text.Contains('NoFileIngress: OK')) {
    throw 'PDF Frame Owner Smoke failed because output did not contain NoFileIngress: OK.'
}

if (-not $text.Contains('Recovery: OK')) {
    throw 'PDF Frame Owner Smoke failed because output did not contain Recovery: OK.'
}

if (-not $text.Contains('RESULT: SUCCESS')) {
    throw 'PDF Frame Owner Smoke failed because output did not contain RESULT: SUCCESS.'
}
