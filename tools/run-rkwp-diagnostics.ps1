param(
    [switch] $SmokeTest,
    [string] $ReadLog,
    [switch] $AuditList,
    [switch] $Session,
    [switch] $Lease,
    [switch] $Violations,
    [string] $ExportMarkdown
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.RkwpDiagnostics\RKWorkspace.RkwpDiagnostics.csproj'
$arguments = @()

if ($SmokeTest) {
    $arguments += '--smoke-test'
}

if (-not [string]::IsNullOrWhiteSpace($ReadLog)) {
    $arguments += '--read-log'
    $arguments += $ReadLog
}
if ($AuditList) { $arguments += '--audit-list' }
if ($Session) { $arguments += '--session' }
if ($Lease) { $arguments += '--lease' }
if ($Violations) { $arguments += '--violations' }
if (-not [string]::IsNullOrWhiteSpace($ExportMarkdown)) {
    $arguments += '--export-markdown'
    $arguments += $ExportMarkdown
}

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $project -- @arguments
exit $LASTEXITCODE
