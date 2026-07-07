param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Host 'RK Workspace Policy Regression'
Write-Host '------------------------------'

& (Join-Path $root 'tools\run-ma017-policy-regression.ps1') -SmokeTest
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host 'PolicyRegression: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
