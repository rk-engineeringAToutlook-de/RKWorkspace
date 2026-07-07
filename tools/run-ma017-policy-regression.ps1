param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma017\reports\policy-regression-report.md'

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Label,

        [Parameter(Mandatory = $true)]
        [scriptblock] $Command,

        [string[]] $Required = @()
    )

    Write-Host "== $Label =="
    $global:LASTEXITCODE = 0
    $output = & $Command 2>&1
    $exitCode = $LASTEXITCODE
    $output | ForEach-Object { Write-Host ([string] $_) }

    if ($exitCode -ne 0) {
        throw "$Label failed with exit code $exitCode."
    }

    $text = ($output | ForEach-Object { [string] $_ }) -join [Environment]::NewLine
    foreach ($line in $Required) {
        if (-not $text.Contains($line)) {
            throw "$Label failed because output did not contain: $line"
        }
    }
}

Write-Host 'RK Workspace MA017 Policy Regression'
Write-Host '------------------------------------'

Invoke-Checked -Label 'Cross-Device Policy Regression' -Command {
    & (Join-Path $root 'tools\run-cross-device-policy-regression.ps1') -SmokeTest
} -Required @(
    'CriticalInfrastructurePolicy: SUCCESS',
    'TrustedPersonalDevicesPolicy: SUCCESS',
    'PresentationOnlyPolicy: SUCCESS',
    'macOSGuestPolicy: SUCCESS',
    'iPadGuestPolicy: SUCCESS',
    'UwbTargetPolicy: SUCCESS',
    'CrossDevicePolicyRegression: SUCCESS',
    'RESULT: SUCCESS'
)

$content = @(
    '# MA017 Policy Regression Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Ergebnis',
    '',
    '- CriticalInfrastructurePolicy: SUCCESS',
    '- TrustedPersonalDevicesPolicy: SUCCESS',
    '- PresentationOnlyPolicy: SUCCESS',
    '- macOSGuestPolicy: SUCCESS',
    '- iPadGuestPolicy: SUCCESS',
    '- UwbTargetPolicy: SUCCESS',
    '- MA017PolicyRegression: SUCCESS',
    '',
    '## Gate',
    '',
    '```powershell',
    '.\tools\run-ma017-policy-regression.ps1 -SmokeTest',
    '```',
    '',
    '## Result',
    '',
    '```text',
    'RESULT: SUCCESS',
    '```'
)

Set-Content -Path $reportPath -Value $content -Encoding UTF8

Write-Host 'MA017PolicyRegression: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
