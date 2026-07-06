param(
    [switch] $SmokeTest,
    [string] $AblageId = 'ablage-windows-dev-agent',
    [string] $BindAddress = '127.0.0.1',
    [int] $Port = 57120
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Agents\RKWorkspace.Agent.Windows\RKWorkspace.Agent.Windows.csproj'

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$arguments = @('--ablage-id', $AblageId, '--bind-address', $BindAddress, '--port', $Port.ToString())
if ($SmokeTest) {
    $arguments += '--smoke-test'
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
        'AgentStarted: OK',
        'NoInstallationRequired: OK',
        'AblageIdentity: OK',
        'RkwpComponentsReachable: OK',
        'Shutdown: OK',
        'WindowsAgentDevSmoke: SUCCESS',
        'RESULT: SUCCESS'
    )

    foreach ($line in $required) {
        if (-not $text.Contains($line)) {
            throw "Windows Agent Dev smoke failed because output did not contain: $line"
        }
    }
}

exit 0
