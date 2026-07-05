$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'tests\Unit\RKWorkspace.Protocol.Tests\RKWorkspace.Protocol.Tests.csproj'

Write-Host 'RKWP Protocol Test Harness'
Write-Host '--------------------------'
dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$output = dotnet run --project $project 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Output $_ }
if ($exitCode -ne 0) {
    exit $exitCode
}

$text = $output -join [Environment]::NewLine
if (-not $text.Contains('RkwpTests: SUCCESS')) {
    throw 'RKWP Protocol Test Harness failed because output did not contain RkwpTests: SUCCESS.'
}

if (-not $text.Contains('RESULT: SUCCESS')) {
    throw 'RKWP Protocol Test Harness failed because output did not contain RESULT: SUCCESS.'
}
