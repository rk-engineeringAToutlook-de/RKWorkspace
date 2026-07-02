param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$studioProject = Join-Path $root 'src\Tools\RKWorkspace.DeveloperStudio\RKWorkspace.DeveloperStudio.csproj'

dotnet build $studioProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

if ($SmokeTest) {
    $env:RKWS_STUDIO_SMOKE_TEST = '1'
    try {
        dotnet run --project $studioProject
        exit $LASTEXITCODE
    }
    finally {
        Remove-Item Env:\RKWS_STUDIO_SMOKE_TEST -ErrorAction SilentlyContinue
    }
}

dotnet run --project $studioProject
exit $LASTEXITCODE
