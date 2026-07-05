param(
    [switch] $SmokeTest,
    [string] $ReadLog
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.RkwpDiagnostics\RKWorkspace.RkwpDiagnostics.csproj'
$arguments = @()

if ($SmokeTest) {
    $arguments += '--smoke-test'
}

if (-not [string]::IsNullOrWhiteSpace($ReadLog)) {
    $arguments += '--read-log'
    $arguments += $ReadLog
}

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $project -- @arguments
exit $LASTEXITCODE
