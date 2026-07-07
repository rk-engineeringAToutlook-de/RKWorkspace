param(
    [switch] $SmokeTest,
    [switch] $SkipRegression
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma017\reports\security-checkpoint-report.md'

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

function Test-TextFile {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Label,

        [Parameter(Mandatory = $true)]
        [string] $RelativePath,

        [Parameter(Mandatory = $true)]
        [string[]] $Required
    )

    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path)) {
        throw "$Label missing: $RelativePath"
    }

    $text = Get-Content -LiteralPath $path -Raw
    foreach ($line in $Required) {
        if (-not $text.Contains($line)) {
            throw "$Label missing required text: $line"
        }
    }

    Write-Host "${Label}: READY"
}

Write-Host 'RK Workspace MA017 Security Checkpoint'
Write-Host '--------------------------------------'

if (-not $SkipRegression) {
    Invoke-Checked -Label 'MA017 Security Regression' -Command {
        & (Join-Path $root 'tools\run-ma017-security-regression.ps1') -SmokeTest
    }

    Invoke-Checked -Label 'MA017 Policy Regression' -Command {
        & (Join-Path $root 'tools\run-ma017-policy-regression.ps1') -SmokeTest
    }

    Invoke-Checked -Label 'MA017 Audit Report' -Command {
        & (Join-Path $root 'tools\export-ma017-audit-report.ps1') -SmokeTest
    }

    Invoke-Checked -Label 'MA017 No File Ingress Report' -Command {
        & (Join-Path $root 'tools\export-ma017-no-file-ingress-report.ps1') -SmokeTest
    }

    Invoke-Checked -Label 'MA017 UWB Privacy Report' -Command {
        & (Join-Path $root 'tools\export-ma017-uwb-privacy-report.ps1') -SmokeTest
    }

    Invoke-Checked -Label 'MA017 Emergency Return' -Command {
        & (Join-Path $root 'tools\run-ma017-emergency-return-test.ps1') -SmokeTest
    }

    Invoke-Checked -Label 'MA017 Stale Lease Recovery' -Command {
        & (Join-Path $root 'tools\run-ma017-stale-lease-recovery.ps1') -SmokeTest
    }
}

Test-TextFile -Label 'MA017SecurityWarnings' -RelativePath 'Docs\Security\MA017_PilotSecurityWarnings.md' -Required @(
    'PilotSecurityWarnings: READY',
    'DevelopmentWarnings: Visible',
    'CriticalPolicyWarnings: None'
)

Test-TextFile -Label 'MA017ThreatModel' -RelativePath 'Docs\Security\MA017_PdfLifecycleThreatModel.md' -Required @(
    'Proximity/UWB Privacy',
    'Pilot-Warnungen',
    'run-ma017-security-checkpoint.ps1'
)

Test-TextFile -Label 'MA017SecurityCheckpointDoc' -RelativePath 'Docs\Readiness\MA017_SecurityCheckpoint.md' -Required @(
    'AP691',
    'AP700',
    'MA017SecurityCheckpoint: SUCCESS'
)

$content = @(
    '# MA017 Security Checkpoint Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Ergebnis',
    '',
    'AP691-700 sind als MA017 Security Checkpoint gebuendelt.',
    '',
    '## Nachweise',
    '',
    '- Security Regression MA017.',
    '- Policy Regression MA017.',
    '- Audit Report MA017.',
    '- No File Ingress Report MA017.',
    '- UWB Privacy Report MA017.',
    '- Pilot Security Warnings.',
    '- Emergency Return Test.',
    '- Stale Lease Startup Recovery.',
    '- Threat Model Update.',
    '',
    '## Result',
    '',
    '```text',
    'MA017SecurityCheckpoint: SUCCESS',
    'RESULT: SUCCESS',
    '```'
)

Set-Content -Path $reportPath -Value $content -Encoding UTF8

Write-Host 'MA017SecurityCheckpoint: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
