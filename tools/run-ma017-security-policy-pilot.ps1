param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma017\reports\security-policy-pilot-report.md'

function Invoke-Ma017SecurityStep {
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

    return $text
}

Write-Host 'RK Workspace MA017 Security/Policy Pilot'
Write-Host '----------------------------------------'

Invoke-Ma017SecurityStep 'No File Ingress Report' {
    & (Join-Path $root 'tools\run-no-file-ingress-report.ps1') -SmokeTest
} @(
    'NoFileIngressReport: SUCCESS',
    'RESULT: SUCCESS'
)

Invoke-Ma017SecurityStep 'Closed PDF Critical Policy' {
    & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf -Policy CriticalInfrastructure
} @(
    'LifecycleMode: ClosedPdfCapsule',
    'PolicyProfile: CriticalInfrastructure',
    'KeepCapsulePolicy: DENIED',
    'UnauthorizedCapsuleOpen: DENIED',
    'UnauthorizedOpenFrameInput: DENIED',
    'SecurityModeWarning: NONE',
    'RESULT: SUCCESS'
)

Invoke-Ma017SecurityStep 'Open PDF Critical Policy' {
    & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -OpenPdf -Policy CriticalInfrastructure
} @(
    'LifecycleMode: OpenPdfFrame',
    'PolicyProfile: CriticalInfrastructure',
    'OpenFrame: OK',
    'KeepCapsulePolicy: DENIED',
    'UnauthorizedCapsuleOpen: DENIED',
    'UnauthorizedOpenFrameInput: DENIED',
    'SecurityModeWarning: NONE',
    'RESULT: SUCCESS'
)

Invoke-Ma017SecurityStep 'Cross-Device Security Regression' {
    & (Join-Path $root 'tools\run-cross-device-security-regression.ps1') -SmokeTest
} @(
    'ClosedPdfSecurity: SUCCESS',
    'OpenPdfSecurity: SUCCESS',
    'UnauthorizedKeepCapsule: DENIED',
    'StaleLeaseRecovery: SUCCESS',
    'CrossDeviceSecurityRegression: SUCCESS',
    'RESULT: SUCCESS'
)

Invoke-Ma017SecurityStep 'Cross-Device Policy Regression' {
    & (Join-Path $root 'tools\run-cross-device-policy-regression.ps1') -SmokeTest
} @(
    'CriticalInfrastructurePolicy: SUCCESS',
    'TrustedPersonalDevicesPolicy: SUCCESS',
    'PresentationOnlyPolicy: SUCCESS',
    'CrossDevicePolicyRegression: SUCCESS',
    'RESULT: SUCCESS'
)

$reportDirectory = Split-Path -Parent $reportPath
if (-not (Test-Path $reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory | Out-Null
}

$report = @(
    '# MA017 Security/Policy Pilot Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Umfang',
    '',
    '- Closed PDF Capsule No File Ingress.',
    '- Open PDF Frame No File Ingress.',
    '- alle PDF-Lifecycle-Faelle: Closed, Open, KeepCapsule, Return, Recovery.',
    '- Policy-Felder: AllowCapsule, AllowOpenFrame und AllowKeepCapsule.',
    '- CriticalInfrastructure Policy fuer PDF Lifecycle.',
    '- TrustedPersonalDevices Policy fuer PDF Lifecycle.',
    '- Unauthorized Capsule Open Audit.',
    '- Unauthorized OpenFrame Input Audit.',
    '',
    '## Ergebnis',
    '',
    '- NoFileIngressReport: SUCCESS',
    '- ClosedPdfCriticalPolicy: SUCCESS',
    '- OpenPdfCriticalPolicy: SUCCESS',
    '- UnauthorizedCapsuleOpen: DENIED',
    '- UnauthorizedOpenFrameInput: DENIED',
    '- CrossDeviceSecurityRegression: SUCCESS',
    '- CrossDevicePolicyRegression: SUCCESS',
    '- MA017SecurityPolicyPilot: SUCCESS'
)

Set-Content -Path $reportPath -Value $report -Encoding UTF8

if ($SmokeTest) {
    Write-Host 'MA017SecurityPolicySmoke: SUCCESS'
}

Write-Host 'MA017SecurityPolicyPilot: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
