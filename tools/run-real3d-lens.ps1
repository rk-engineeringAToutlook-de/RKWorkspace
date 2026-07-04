param(
    [switch] $SmokeTest,
    [switch] $Open,
    [int] $Port = 0
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$real3dProject = Join-Path $root 'src\Shell\RKWorkspace.Shell.Real3D.Lens.Web\RKWorkspace.Shell.Real3D.Lens.Web.csproj'
$real3dArgs = @()

if ($SmokeTest) {
    $real3dArgs += '--smoke-test'
}

if ($Port -gt 0) {
    $real3dArgs += '--port'
    $real3dArgs += $Port.ToString()
}

dotnet build $real3dProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

if ($Open -and -not $SmokeTest) {
    $openPort = if ($Port -gt 0) { $Port } else { 5137 }
    Start-Process "http://localhost:$openPort/real3d"
}

dotnet run --project $real3dProject -- @real3dArgs
exit $LASTEXITCODE
