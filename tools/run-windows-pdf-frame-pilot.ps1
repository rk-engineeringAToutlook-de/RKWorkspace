param(
    [switch] $SmokeTest,
    [string] $PdfPath,
    [switch] $OwnerVisible,
    [switch] $GuestVisible,
    [switch] $Debug,
    [switch] $UseGlassEdge,
    [switch] $UseManualMap,
    [switch] $PlaySequence
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

if ($Debug) {
    $arguments += '--debug'
}

if ($UseGlassEdge) {
    $arguments += '--use-glass-edge'
}

if ($UseManualMap) {
    $arguments += '--use-manual-map'
}

if ($PlaySequence) {
    $arguments += '--play-sequence'
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
        'OwnerLeasedStatus: ausgeliehen',
        'OwnerReturnedStatus: wieder verfuegbar',
        'OwnerRecoveryStatus: wiederhergestellt',
        'Recovery: SUCCESS',
        'ReturnVisibleState: SUCCESS',
        'RecoveryVisibleState: SUCCESS',
        'NoFileIngress: SUCCESS',
        'No File Ingress Status: sichtbar im Log',
        'VisibleForbiddenTerms: SUCCESS',
        'RESULT: SUCCESS'
    )

    foreach ($line in $required) {
        if (-not $text.Contains($line)) {
            throw "Windows PDF Frame Pilot Smoke failed because output did not contain: $line"
        }
    }

    if ($UseGlassEdge) {
        $glassEdgeRequired = @(
            'UseGlassEdge: YES',
            'NearestAblage: Ablage Windows Guest',
            'GlassEdgeAppearing: OK',
            'GlassEdgeActive: OK',
            'ObjectEnteringEdge: OK',
            'ObjectInTransit: OK',
            'ObjectEmerging: OK',
            'ObjectPlaced: OK',
            'FrameSessionOpen: OK',
            'FrameSessionReady: OK',
            'PlaySequence: SUCCESS',
            'NearestAblageSelected: OK',
            'GlassEdgeIntegration: SUCCESS',
            'GlassEdgePlaySequence: SUCCESS'
        )

        foreach ($line in $glassEdgeRequired) {
            if (-not $text.Contains($line)) {
                throw "Windows PDF Glass Edge Pilot Smoke failed because output did not contain: $line"
            }
        }
    }
}

exit 0
