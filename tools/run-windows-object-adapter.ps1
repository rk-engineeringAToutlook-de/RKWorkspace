param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.WindowsObjectAdapterSmoke\RKWorkspace.WindowsObjectAdapterSmoke.csproj'

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$output = dotnet run --project $project -- 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Output $_ }
if ($exitCode -ne 0) {
    exit $exitCode
}

if ($SmokeTest) {
    $text = $output -join [Environment]::NewLine
    $required = @(
        'SamplePdf: OK',
        'ExplorerSelection: OK',
        'ExplorerObjectKind: PdfDocument',
        'ClipboardText: OK',
        'ClipboardImage: OK',
        'ScreenshotRegion: OK',
        'WindowSnapshot: OK',
        'RemoteSession: OK',
        'NoFileIngress: SUCCESS',
        'NoAutoOwnershipTransfer: SUCCESS',
        'RESULT: SUCCESS'
    )

    foreach ($line in $required) {
        if (-not $text.Contains($line)) {
            throw "Windows Object Adapter Smoke failed because output did not contain: $line"
        }
    }
}

exit 0
