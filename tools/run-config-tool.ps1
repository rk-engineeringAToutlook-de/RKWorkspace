param(
    [switch] $Validate,
    [switch] $Show,
    [switch] $CreateSample,
    [switch] $SmokeTest,
    [string] $Config
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.ConfigTool\RKWorkspace.ConfigTool.csproj'

if ([string]::IsNullOrWhiteSpace($Config)) {
    $Config = Join-Path $root 'config\samples\rkworkspace.sample.json'
}

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$arguments = @('--config', $Config)
if ($SmokeTest) {
    $arguments += '--smoke-test'
}
elseif ($Show) {
    $arguments += '--show'
}
elseif ($CreateSample) {
    $arguments += '--create-sample'
}
else {
    $arguments += '--validate'
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
        'SampleConfigLoaded: OK',
        'DevelopmentConfig: OK',
        'InvalidConfigRejected: OK',
        'CriticalPolicy: OK',
        'RkwpDevConfig: OK',
        'ManualMapConfig: OK',
        'LocalSecretsNotRequired: OK',
        'ConfigToolSmoke: SUCCESS',
        'RESULT: SUCCESS'
    )

    foreach ($line in $required) {
        if (-not $text.Contains($line)) {
            throw "Config tool smoke failed because output did not contain: $line"
        }
    }
}

exit 0
