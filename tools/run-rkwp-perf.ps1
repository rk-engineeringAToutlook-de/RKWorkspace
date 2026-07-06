param(
    [switch] $SmokeTest,
    [string] $PdfPath,
    [int] $Iterations = 0
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.RkwpPerfHarness\RKWorkspace.RkwpPerfHarness.csproj'
$arguments = @()

if ($SmokeTest) {
    $arguments += '--smoke-test'
}

if (-not [string]::IsNullOrWhiteSpace($PdfPath)) {
    $arguments += '--pdf-path'
    $arguments += $PdfPath
}

if ($Iterations -gt 0) {
    $arguments += '--iterations'
    $arguments += $Iterations.ToString()
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
        'Mode: SmokeTest',
        'Iterations: 10',
        'PerfSamples: OK',
        'PdfRenderFirstPageAverageMs:',
        'PdfRenderNextPageAverageMs:',
        'PdfTileGenerationAverageMs:',
        'PdfFrameSizeBytesAverage:',
        'MemorySnapshotBytesAverage:',
        'NoFileIngress: SUCCESS',
        'RESULT: SUCCESS'
    )

    foreach ($line in $required) {
        if (-not $text.Contains($line)) {
            throw "RKWP Performance Smoke failed because output did not contain: $line"
        }
    }

    $jsonLine = $output | Where-Object { $_ -like 'Json: *' } | Select-Object -First 1
    $markdownLine = $output | Where-Object { $_ -like 'Markdown: *' } | Select-Object -First 1
    if ($null -eq $jsonLine -or $null -eq $markdownLine) {
        throw 'RKWP Performance Smoke failed because report paths were not printed.'
    }

    $jsonPath = $jsonLine.Substring(6).Trim()
    $markdownPath = $markdownLine.Substring(10).Trim()
    if (-not (Test-Path -LiteralPath $jsonPath)) {
        throw "RKWP Performance Smoke failed because JSON report does not exist: $jsonPath"
    }

    if (-not (Test-Path -LiteralPath $markdownPath)) {
        throw "RKWP Performance Smoke failed because Markdown report does not exist: $markdownPath"
    }
}

exit 0
