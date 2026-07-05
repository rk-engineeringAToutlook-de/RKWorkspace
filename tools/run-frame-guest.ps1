$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.FrameGuestSurface\RKWorkspace.FrameGuestSurface.csproj'

Write-Host 'Frame Guest Surface Smoke'
Write-Host '-------------------------'
dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$output = dotnet run --project $project 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Host $_ }
if ($exitCode -ne 0) {
    exit $exitCode
}

$text = $output -join [Environment]::NewLine
if (-not $text.Contains('GuestHasPdfFile: NO')) {
    throw 'Frame Guest Surface Smoke failed because output did not contain GuestHasPdfFile: NO.'
}

if (-not $text.Contains('Frame geoeffnet')) {
    throw 'Frame Guest Surface Smoke failed because output did not contain Frame geoeffnet.'
}

if (-not $text.Contains('Liegt hier im Frame')) {
    throw 'Frame Guest Surface Smoke failed because output did not contain Liegt hier im Frame.'
}

if (-not $text.Contains('FrameOnly: OK')) {
    throw 'Frame Guest Surface Smoke failed because output did not contain FrameOnly: OK.'
}

if (-not $text.Contains('Zurueckgegeben')) {
    throw 'Frame Guest Surface Smoke failed because output did not contain Zurueckgegeben.'
}

if (-not $text.Contains('RESULT: SUCCESS')) {
    throw 'Frame Guest Surface Smoke failed because output did not contain RESULT: SUCCESS.'
}
