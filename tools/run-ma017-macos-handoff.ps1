param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma017\reports\macos-handoff-report.md'

$requiredFiles = @(
    'release\ma017\handoff\macOS_START_HERE.md',
    'release\ma017\handoff\macOS_MinimalGuestAppTask.md',
    'release\ma017\handoff\macOS_CHECKPOINT.md',
    'release\ma017\packages\apple-rkwp-swift-bundle\README.md',
    'release\ma017\packages\apple-rkwp-swift-bundle\RKWPModels.swift',
    'release\ma017\packages\apple-rkwp-swift-bundle\samples\capsule-created.json',
    'release\ma017\packages\apple-rkwp-swift-bundle\samples\openframe-started.json',
    'release\ma017\packages\apple-rkwp-swift-bundle\samples\no-file-ingress-result.json',
    'release\ma017\config\macos-guest.sample.json',
    'release\ma017\schemas\macos-capsule-contract.v0.2.json',
    'release\ma017\schemas\macos-openframe-contract.v0.2.json',
    'release\ma017\schemas\macos-no-file-ingress-contract.v0.2.json',
    'release\ma017\runbooks\macOS_PilotRunbook.md',
    'release\ma017\runbooks\macOS_BuildPermissionsChecklist.md'
)

Write-Host 'RK Workspace MA017 macOS Handoff'
Write-Host '--------------------------------'

foreach ($relativePath in $requiredFiles) {
    $path = Join-Path $root $relativePath
    if (-not (Test-Path $path)) {
        throw "Missing required macOS handoff file: $relativePath"
    }
}

$configPath = Join-Path $root 'release\ma017\config\macos-guest.sample.json'
$config = Get-Content -Path $configPath -Raw | ConvertFrom-Json
if ($config.platform -ne 'macOS' -or -not $config.framePolicy.noFileIngress -or -not $config.framePolicy.memoryOnlyCache) {
    throw 'macOS sample config does not enforce macOS NoFileIngress MemoryOnly frame policy.'
}

$schemaFiles = @(
    'release\ma017\schemas\macos-capsule-contract.v0.2.json',
    'release\ma017\schemas\macos-openframe-contract.v0.2.json',
    'release\ma017\schemas\macos-no-file-ingress-contract.v0.2.json'
)

foreach ($relativePath in $schemaFiles) {
    $schemaText = Get-Content -Path (Join-Path $root $relativePath) -Raw
    foreach ($forbidden in @('pdfBytes', 'originalBytes', 'originalFileBytes', 'originalPath', 'localPdfPath', 'downloadPath', 'filePath')) {
        if (-not $schemaText.Contains($forbidden)) {
            throw "$relativePath does not name forbidden payload key $forbidden."
        }
    }
}

$output = & (Join-Path $root 'tools\run-mac-guest-compat.ps1') -SmokeTest 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Host ([string] $_) }
if ($exitCode -ne 0) {
    throw "macOS compatibility harness failed with exit code $exitCode."
}

$text = ($output | ForEach-Object { [string] $_ }) -join [Environment]::NewLine
foreach ($line in @(
    'MacGuestIdentity: OK',
    'Platform: MacOS',
    'FrameView: OK',
    'FrameInput: OFF',
    'NoFileIngressCapability: OK',
    'Return: SUCCESS',
    'GuestHasPdfFile: NO',
    'GuestHasOriginalPath: NO',
    'OriginalFileBytes: NO',
    'NoFileIngress: SUCCESS',
    'RESULT: SUCCESS'
)) {
    if (-not $text.Contains($line)) {
        throw "macOS compatibility output did not contain: $line"
    }
}

$reportDirectory = Split-Path -Parent $reportPath
if (-not (Test-Path $reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory | Out-Null
}

$report = @(
    '# MA017 macOS Handoff Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Ergebnis',
    '',
    '- macOS START HERE: OK',
    '- Apple Swift/RKWP Bundle: OK',
    '- macOS Config Sample: OK',
    '- Capsule Contract: OK',
    '- OpenFrame Contract: OK',
    '- No File Ingress Contract: OK',
    '- Pilot Runbook: OK',
    '- Build-/Permissions-Checkliste: OK',
    '- macOS Compatibility Harness: SUCCESS',
    '- MA017macOSHandoff: SUCCESS'
)

Set-Content -Path $reportPath -Value $report -Encoding UTF8

if ($SmokeTest) {
    Write-Host 'MA017macOSHandoffSmoke: SUCCESS'
}

Write-Host 'MA017macOSHandoff: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
