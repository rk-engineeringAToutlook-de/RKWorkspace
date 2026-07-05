param(
    [switch] $SmokeTest,
    [string] $PdfPath
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.GlassEdgePdfFrameDemo\RKWorkspace.GlassEdgePdfFrameDemo.csproj'
$arguments = @()

if ($SmokeTest) {
    $arguments += '--smoke-test'
}

if (-not [string]::IsNullOrWhiteSpace($PdfPath)) {
    $arguments += '--pdf-path'
    $arguments += $PdfPath
}

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$output = dotnet run --project $project -- @arguments 2>&1
$exitCode = $LASTEXITCODE
$text = $output -join [Environment]::NewLine
$output | ForEach-Object { Write-Output $_ }

if ($exitCode -ne 0) {
    exit $exitCode
}

if ($SmokeTest) {
    $required = @(
        'RK Workspace Glass Edge PDF Frame Demo',
        'OriginalThing:',
        'NearestAblage:',
        'GlassEdge: Active',
        'CarryLease: Active',
        'FrameSession: Active',
        'OwnerLocked: OK',
        'PDF liegt hier im Frame.',
        'GuestHasPdfFile: NO',
        'GuestHasOriginalPath: NO',
        'GuestHasCopiedPdfBytes: NO',
        'NoFileIngress: SUCCESS',
        'Returned: SUCCESS',
        'Recovery: SUCCESS',
        'Audit: SUCCESS',
        'GlassEdgePdfFrameDemo: SUCCESS',
        'RESULT: SUCCESS'
    )

    foreach ($marker in $required) {
        if (-not $text.Contains($marker)) {
            throw "Glass Edge PDF Frame Demo Smoke failed because output did not contain $marker."
        }
    }

    $forbidden = @(
        'Upload',
        'Download',
        'Datei empfangen',
        'PDF heruntergeladen',
        'Senden',
        'Empfangen'
    )

    foreach ($marker in $forbidden) {
        if ($text.Contains($marker)) {
            throw "Glass Edge PDF Frame Demo Smoke failed because output contained forbidden wording."
        }
    }
}

exit 0
