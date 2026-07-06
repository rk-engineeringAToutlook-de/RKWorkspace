param(
    [switch] $SmokeTest,
    [string] $PdfPath,
    [switch] $OwnerVisible,
    [switch] $GuestVisible
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.WindowsPdfFramePilot\RKWorkspace.WindowsPdfFramePilot.csproj'
$arguments = @()

if ($SmokeTest) {
    $arguments += '--smoke-test'
}

if (-not [string]::IsNullOrWhiteSpace($PdfPath)) {
    $arguments += '--pdf-path'
    $arguments += $PdfPath
}

if ($OwnerVisible) {
    $arguments += '--owner-visible'
}

if ($GuestVisible) {
    $arguments += '--guest-visible'
}

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$output = dotnet run --project $project -- @arguments 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Output $_ }
if ($exitCode -ne 0) {
    exit $exitCode
}

if ($SmokeTest) {
    $text = $output -join [Environment]::NewLine
    $required = @(
        'SamplePdf: OK',
        'OwnerSurface: STARTED',
        'GuestSurface: STARTED',
        'PdfOriginalRegistered: OK',
        'CarryLease: Active',
        'OwnerLocked: OK',
        'FrameSession: Active',
        'GuestFrame: OK',
        'GuestHasPdfFile: NO',
        'GuestHasOriginalPath: NO',
        'GuestHasCopiedPdfBytes: NO',
        'Rueckgabe: SUCCESS',
        'OwnerReturnedStatus: zurueckgegeben',
        'Recovery: SUCCESS',
        'NoFileIngress: SUCCESS',
        'RESULT: SUCCESS'
    )

    foreach ($line in $required) {
        if (-not $text.Contains($line)) {
            throw "Windows PDF Frame Pilot Smoke failed because output did not contain: $line"
        }
    }
}

exit 0
