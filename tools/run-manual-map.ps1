param(
    [switch] $List,
    [switch] $Show,
    [switch] $Set,
    [switch] $Remove,
    [switch] $Clear,
    [string] $Import,
    [string] $Export,
    [switch] $Validate,
    [switch] $SmokeTest,
    [switch] $Help,
    [string] $Ablage,
    [string] $Direction,
    [string] $Distance,
    [double] $DistanceMeters = [double]::NaN,
    [double] $Confidence = [double]::NaN,
    [string] $ConfigPath
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.ManualMapTool\RKWorkspace.ManualMapTool.csproj'
$arguments = @()

if ($List) { $arguments += '--list' }
if ($Show) { $arguments += '--show' }
if ($Set) { $arguments += '--set' }
if ($Remove) { $arguments += '--remove' }
if ($Clear) { $arguments += '--clear' }
if ($Validate) { $arguments += '--validate' }
if ($SmokeTest) { $arguments += '--smoke-test' }
if ($Help) { $arguments += '--help' }
if (-not [string]::IsNullOrWhiteSpace($Import)) {
    $arguments += '--import'
    $arguments += $Import
}
if (-not [string]::IsNullOrWhiteSpace($Export)) {
    $arguments += '--export'
    $arguments += $Export
}
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
if (-not [double]::IsNaN($DistanceMeters)) {
    $arguments += '--distance-meters'
    $arguments += $DistanceMeters.ToString([System.Globalization.CultureInfo]::InvariantCulture)
}
if (-not [double]::IsNaN($Confidence)) {
    $arguments += '--confidence'
    $arguments += $Confidence.ToString([System.Globalization.CultureInfo]::InvariantCulture)
}
if (-not [string]::IsNullOrWhiteSpace($ConfigPath)) {
    $arguments += '--config-path'
    $arguments += $ConfigPath
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
        'ManualMapSet: OK',
        'ManualMapList: OK',
        'ManualMapExport: OK',
        'ManualMapImport: OK',
        'ManualMapRemove: OK',
        'ManualMapClear: OK',
        'ManualMapValidate: SUCCESS',
        'SelectorUsesMap: OK',
        'NearestAblage: Ablage macOS',
        'EdgeDirection: Right',
        'InvalidValuesRejected: OK',
        'ManualMapSmoke: SUCCESS',
        'RESULT: SUCCESS'
    )

    foreach ($line in $required) {
        if (-not $text.Contains($line)) {
            throw "Manual Map Smoke failed because output did not contain: $line"
        }
    }
}

exit 0
