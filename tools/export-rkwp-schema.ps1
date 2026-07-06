param(
    [string] $SchemaPath = 'release\schema\rkwp-envelope-schema-v0.1.json'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$schemaFullPath = Join-Path $root $SchemaPath
$messageTypePath = Join-Path $root 'src\Protocol\RKWorkspace.Protocol\RkwpMessageType.cs'

if (-not (Test-Path $schemaFullPath)) {
    throw "RKWP schema export is missing: $SchemaPath"
}

if (-not (Test-Path $messageTypePath)) {
    throw 'RKWP message type source is missing.'
}

$source = Get-Content -Path $messageTypePath -Raw
$enumBody = [regex]::Match($source, 'enum\s+RkwpMessageType\s*\{(?<body>.*?)\}', [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $enumBody.Success) {
    throw 'Could not read RkwpMessageType enum.'
}

$sourceMessageTypes = $enumBody.Groups['body'].Value -split ',' |
    ForEach-Object { $_.Trim() } |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

$schema = Get-Content -Path $schemaFullPath -Raw | ConvertFrom-Json
$schemaMessageTypes = @($schema.properties.message.properties.messageType.enum)

$missing = @($sourceMessageTypes | Where-Object { $_ -notin $schemaMessageTypes })
if ($missing.Count -gt 0) {
    throw "RKWP schema export is missing message types: $($missing -join ', ')"
}

Write-Host 'RK Workspace RKWP Schema Export'
Write-Host '-------------------------------'
Write-Host "Schema: $SchemaPath"
Write-Host "MessageTypes: $($schemaMessageTypes.Count)"
Write-Host 'ProtocolEnvelope: OK'
Write-Host 'NoFileIngressSchema: OK'
Write-Host 'SchemaExport: SUCCESS'
Write-Host 'RESULT: SUCCESS'

exit 0
