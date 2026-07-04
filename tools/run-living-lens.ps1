param(
    [switch] $SmokeTest,
    [switch] $ExportFrames
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$livingLensProject = Join-Path $root 'src\Shell\RKWorkspace.Shell.LivingLens.Windows\RKWorkspace.Shell.LivingLens.Windows.csproj'
$livingLensArgs = @()

if ($SmokeTest) {
    $livingLensArgs += '--smoke-test'
}

if ($ExportFrames) {
    $livingLensArgs += '--export-frames'
}

dotnet build $livingLensProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $livingLensProject -- @livingLensArgs
exit $LASTEXITCODE
