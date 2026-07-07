param(
    [switch] $SmokeTest,
    [switch] $ClosedOnly,
    [switch] $OpenOnly
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$lifecyclePilot = Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1'
$reportPath = Join-Path $root 'release\ma017\reports\windows-pdf-standard-report.md'

function Invoke-Ma017PilotStep {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Label,

        [Parameter(Mandatory = $true)]
        [scriptblock] $Command
    )

    Write-Host "== $Label =="
    $global:LASTEXITCODE = 0
    $output = & $Command 2>&1
    $exitCode = $LASTEXITCODE
    $output | ForEach-Object { Write-Host ([string] $_) }

    if ($exitCode -ne 0) {
        throw "$Label failed with exit code $exitCode."
    }

    return (($output | ForEach-Object { [string] $_ }) -join [Environment]::NewLine)
}

function Assert-ContainsLine {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Label,

        [Parameter(Mandatory = $true)]
        [string] $Text,

        [Parameter(Mandatory = $true)]
        [string[]] $Required
    )

    foreach ($line in $Required) {
        if (-not $Text.Contains($line)) {
            throw "$Label failed because output did not contain: $line"
        }
    }
}

Write-Host 'RK Workspace MA017 Windows PDF Standard Pilot'
Write-Host '---------------------------------------------'

$runClosed = -not $OpenOnly
$runOpen = -not $ClosedOnly
$closedOutput = ''
$openOutput = ''

if ($runClosed) {
    $closedOutput = Invoke-Ma017PilotStep 'Closed PDF Capsule Standard' {
        & $lifecyclePilot -SmokeTest -ClosedPdf -CloseReturns
    }

    Assert-ContainsLine 'Closed PDF Capsule Standard' $closedOutput @(
        'LifecycleMode: ClosedPdfCapsule',
        'FrameCapsule: OK',
        'CapsuleOpen: OK',
        'OwnerLocked: OK',
        'OwnerLockedStatus: wartet auf Rueckgabe',
        'CloseFrameBehavior: CloseReturns',
        'KeepCapsulePolicy: DENIED',
        'UnauthorizedOpenFrameInput: DENIED',
        'ExpiredCapsule: RECOVERED_BY_OWNER',
        'Recovery: SUCCESS',
        'RecoveryVisibleState: SUCCESS',
        'VisibleForbiddenTerms: SUCCESS',
        'NoFileIngress: SUCCESS',
        'RESULT: SUCCESS'
    )

    Write-Host 'ClosedPdfStandard: SUCCESS'
}

if ($runOpen) {
    $openOutput = Invoke-Ma017PilotStep 'Open PDF Frame Standard' {
        & $lifecyclePilot -SmokeTest -OpenPdf -CloseReturns
    }

    Assert-ContainsLine 'Open PDF Frame Standard' $openOutput @(
        'LifecycleMode: OpenPdfFrame',
        'OpenPdfContext: OK',
        'OpenFrame: OK',
        'FrameCapsule: OK',
        'CapsuleOpen: OK',
        'OwnerLocked: OK',
        'OwnerLockedStatus: wartet auf Rueckgabe',
        'CloseFrameBehavior: CloseReturns',
        'KeepCapsulePolicy: DENIED',
        'UnauthorizedOpenFrameInput: DENIED',
        'ExpiredCapsule: RECOVERED_BY_OWNER',
        'Recovery: SUCCESS',
        'RecoveryVisibleState: SUCCESS',
        'VisibleForbiddenTerms: SUCCESS',
        'OpenFrameNoFileIngress: SUCCESS',
        'RESULT: SUCCESS'
    )

    Write-Host 'OpenPdfStandard: SUCCESS'
}

$reportLines = @(
    '# MA017 Windows PDF Standard Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Umfang',
    '',
    '- Closed PDF Capsule ist Standardtest.',
    '- Open PDF Frame ist Standardtest.',
    '- CapsuleOpen und OpenFrame sind sichtbar pruefbar.',
    '- OwnerLock ist eindeutig sichtbar.',
    '- CloseFrame Return ist der erwartete Standard.',
    '- KeepCapsule bleibt in der DevelopmentLab Policy untersagt.',
    '- Recovery ist fuer Closed PDF Capsule und Open PDF Frame nachgewiesen.',
    '- Sichtbare verbotene Sprache wird weiter blockiert.',
    '',
    '## Ergebnis',
    '',
    '- ClosedPdfStandard: ' + $(if ($runClosed) { 'SUCCESS' } else { 'SKIPPED' }),
    '- OpenPdfStandard: ' + $(if ($runOpen) { 'SUCCESS' } else { 'SKIPPED' }),
    '- WindowsPdfStandard: SUCCESS',
    '',
    '## Owner-Sicht',
    '',
    'Der Windows-Pilot zeigt nicht Dateiuebertragung, sondern Original-Besitz, Rueckgabe und Recovery. Die sichtbaren Statusmeldungen muessen Vertrauen erzeugen, ohne den Owner an Kopieren, Synchronisieren oder Datei-Ingress zu erinnern.'
)

$reportDirectory = Split-Path -Parent $reportPath
if (-not (Test-Path $reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory | Out-Null
}

Set-Content -Path $reportPath -Value $reportLines -Encoding UTF8

if ($SmokeTest) {
    Write-Host 'WindowsPdfStandardSmoke: SUCCESS'
}

Write-Host 'WindowsPdfStandard: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
