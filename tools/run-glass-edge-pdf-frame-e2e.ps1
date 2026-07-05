param(
    [switch] $SmokeTest,
    [string] $PdfPath
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.GlassEdgePdfFrameE2E\RKWorkspace.GlassEdgePdfFrameE2E.csproj'
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

dotnet run --project $project -- @arguments
exit $LASTEXITCODE
