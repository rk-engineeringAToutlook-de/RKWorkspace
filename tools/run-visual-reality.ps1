param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$visualRealityProject = Join-Path $root 'src\Shell\RKWorkspace.Shell.VisualReality.Windows\RKWorkspace.Shell.VisualReality.Windows.csproj'
$visualRealityArgs = @()

if ($SmokeTest) {
    $visualRealityArgs += '--smoke-test'
}

dotnet build $visualRealityProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $visualRealityProject -- @visualRealityArgs
exit $LASTEXITCODE
