param(
    [string] $ContractPath = 'contracts\macOS-guest\rkwp-macos-guest-contract-v0.1.json',
    [string] $SchemaPath = 'release\schema\rkwp-envelope-schema-v0.1.json'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'tests\Contract\RKWorkspace.MacGuest.Contracts\RKWorkspace.MacGuest.Contracts.csproj'
$contract = Join-Path $root $ContractPath
$schema = Join-Path $root $SchemaPath

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$output = dotnet run --project $project -- --contract $contract --schema $schema 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Output $_ }
if ($exitCode -ne 0) {
    exit $exitCode
}

$text = $output -join [Environment]::NewLine
$required = @(
    'MessageOrder: OK',
    'RequiredFields: OK',
    'Schema: OK',
    'AblageHello: OK',
    'AblageCapabilities: OK',
    'DevPairing: OK',
    'SecureDevHandshake: OK',
    'FrameSessionOpen: OK',
    'FrameUpdate: OK',
    'FrameInput: OK',
    'Heartbeat: OK',
    'Return: OK',
    'Revocation: OK',
    'Error: OK',
    'NoFileIngress: SUCCESS',
    'RESULT: SUCCESS'
)

foreach ($line in $required) {
    if (-not $text.Contains($line)) {
        throw "macOS Guest contract test failed because output did not contain: $line"
    }
}

exit 0
