param(
    [switch] $List,
    [string] $Show,
    [switch] $Validate,
    [switch] $SmokeTest,
    [switch] $Help
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.PolicyProfile\RKWorkspace.PolicyProfile.csproj'
$arguments = @()

if ($List) { $arguments += '--list' }
if (-not [string]::IsNullOrWhiteSpace($Show)) {
    $arguments += '--show'
    $arguments += $Show
}
if ($Validate) { $arguments += '--validate' }
if ($SmokeTest) { $arguments += '--smoke-test' }
if ($Help) { $arguments += '--help' }

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $project -- @arguments
exit $LASTEXITCODE
