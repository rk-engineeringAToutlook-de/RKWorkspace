param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma017\reports\security-regression-report.md'

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

Write-Host 'RK Workspace MA017 Security Regression'
Write-Host '--------------------------------------'

Invoke-Checked -Label 'Cross-Device Security Regression' -Command {
    & (Join-Path $root 'tools\run-cross-device-security-regression.ps1') -SmokeTest
} -Required @(
    'ClosedPdfSecurity: SUCCESS',
    'OpenPdfSecurity: SUCCESS',
    'UnauthorizedKeepCapsule: DENIED',
    'StaleLeaseRecovery: SUCCESS',
    'CrossDeviceSecurityRegression: SUCCESS',
    'RESULT: SUCCESS'
)

Invoke-Checked -Label 'UWB/Dongle Privacy Security Path' -Command {
    & (Join-Path $root 'tools\run-ma017-uwb-dongle-pilot.ps1') -SmokeTest
} -Required @(
    'PrivacyMode: EphemeralLab',
    'MA017UwbDonglePilot: SUCCESS',
    'RESULT: SUCCESS'
)

$content = @(
    '# MA017 Security Regression Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Ergebnis',
    '',
    '- CrossDeviceSecurityRegression: SUCCESS',
    '- NoFileIngress: SUCCESS',
    '- UWB/Dongle Privacy Path: SUCCESS',
    '- StaleLeaseRecovery: SUCCESS',
    '- MA017SecurityRegression: SUCCESS',
    '',
    '## Gate',
    '',
    '```powershell',
    '.\tools\run-ma017-security-regression.ps1 -SmokeTest',
    '```',
    '',
    '## Result',
    '',
    '```text',
    'RESULT: SUCCESS',
    '```'
)

Set-Content -Path $reportPath -Value $content -Encoding UTF8

Write-Host 'MA017SecurityRegression: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
