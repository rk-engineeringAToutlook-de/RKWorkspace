param(
    [switch] $List,
    [switch] $Set,
    [switch] $Clear,
    [switch] $SmokeTest,
    [switch] $Help,
    [string] $Ablage,
    [string] $Direction,
    [string] $Distance,
    [Nullable[double]] $DistanceMeters,
    [Nullable[double]] $Confidence,
    [string] $ConfigPath
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.ManualMapTool\RKWorkspace.ManualMapTool.csproj'
$arguments = @()

if ($List) { $arguments += '--list' }
if ($Set) { $arguments += '--set' }
if ($Clear) { $arguments += '--clear' }
if ($SmokeTest) { $arguments += '--smoke-test' }
if ($Help) { $arguments += '--help' }
if (-not [string]::IsNullOrWhiteSpace($Ablage)) {
    $arguments += '--ablage'
    $arguments += $Ablage
}
if (-not [string]::IsNullOrWhiteSpace($Direction)) {
    $arguments += '--direction'
    $arguments += $Direction
}
if (-not [string]::IsNullOrWhiteSpace($Distance)) {
    $arguments += '--distance'
    $arguments += $Distance
}
if ($DistanceMeters.HasValue) {
    $arguments += '--distance-meters'
    $arguments += $DistanceMeters.Value.ToString([System.Globalization.CultureInfo]::InvariantCulture)
}
if ($Confidence.HasValue) {
    $arguments += '--confidence'
    $arguments += $Confidence.Value.ToString([System.Globalization.CultureInfo]::InvariantCulture)
}
if (-not [string]::IsNullOrWhiteSpace($ConfigPath)) {
    $arguments += '--config-path'
    $arguments += $ConfigPath
}

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $project -- @arguments
exit $LASTEXITCODE
