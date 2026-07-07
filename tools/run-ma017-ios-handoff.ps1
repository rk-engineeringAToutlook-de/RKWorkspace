param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma017\reports\ios-handoff-report.md'

$requiredFiles = @(
    'release\ma017\handoff\iOS_START_HERE.md',
    'release\ma017\handoff\iOS_XcodeMinimalAppTask.md',
    'release\ma017\handoff\iOS_iPad_CHECKPOINT.md',
    'release\ma017\packages\apple-rkwp-swift-bundle\RKWPModels.swift',
    'release\ma017\config\ios-guest.sample.json',
    'release\ma017\schemas\ios-capsule-contract.v0.2.json',
    'release\ma017\schemas\ios-openframe-contract.v0.2.json',
    'release\ma017\schemas\ios-haptics-contract.v0.2.json',
    'release\ma017\schemas\ios-no-file-ingress-sandbox-contract.v0.2.json',
    'release\ma017\runbooks\iOS_USBInstallRunbook.md',
    'release\ma017\runbooks\iOS_iPad_PilotRunbook.md'
)

Write-Host 'RK Workspace MA017 iOS/iPad Handoff'
Write-Host '-----------------------------------'

foreach ($relativePath in $requiredFiles) {
    $path = Join-Path $root $relativePath
    if (-not (Test-Path $path)) {
        throw "Missing required iOS handoff file: $relativePath"
    }
}

$config = Get-Content -Path (Join-Path $root 'release\ma017\config\ios-guest.sample.json') -Raw | ConvertFrom-Json
if ($config.platform -ne 'iPadOS' -or -not $config.framePolicy.noFileIngress -or -not $config.framePolicy.memoryOnlyCache) {
    throw 'iOS sample config does not enforce iPadOS NoFileIngress MemoryOnly frame policy.'
}

if ($config.sandbox.documentsOriginalPdf -or $config.sandbox.cachesOriginalPdf -or $config.sandbox.tempOriginalPdf -or $config.sandbox.filesAppOriginalPdf) {
    throw 'iOS sample config allows original PDF materialization in sandbox.'
}

foreach ($relativePath in @(
    'release\ma017\schemas\ios-capsule-contract.v0.2.json',
    'release\ma017\schemas\ios-openframe-contract.v0.2.json',
    'release\ma017\schemas\ios-no-file-ingress-sandbox-contract.v0.2.json'
)) {
    $schemaText = Get-Content -Path (Join-Path $root $relativePath) -Raw
    foreach ($forbidden in @('pdfBytes', 'originalBytes', 'originalFileBytes', 'originalPath', 'localPdfPath', 'downloadPath', 'filePath')) {
        if (-not $schemaText.Contains($forbidden)) {
            throw "$relativePath does not name forbidden payload key $forbidden."
        }
    }
}

$hapticText = Get-Content -Path (Join-Path $root 'release\ma017\schemas\ios-haptics-contract.v0.2.json') -Raw
foreach ($pattern in @('softArrival', 'softConfirm', 'softComplete', 'softNotice', 'firmButShort')) {
    if (-not $hapticText.Contains($pattern)) {
        throw "iOS haptics contract does not contain pattern $pattern."
    }
}

$output = & (Join-Path $root 'tools\run-ios-guest-compat.ps1') -SmokeTest 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Host ([string] $_) }
if ($exitCode -ne 0) {
    throw "iOS/iPad compatibility harness failed with exit code $exitCode."
}

$text = ($output | ForEach-Object { [string] $_ }) -join [Environment]::NewLine
foreach ($line in @(
    'iOSGuestIdentity: OK',
    'PrimaryPlatform: IPadOS',
    'PhonePlatform: IOS',
    'FrameView: OK',
    'Haptics: PLANNED',
    'NoFileIngressCapability: OK',
    'GlobalAppCapture: FALSE',
    'XcodeBridge: OK',
    'GuestHasPdfFile: NO',
    'GuestHasOriginalPath: NO',
    'OriginalFileBytes: NO',
    'NoFileIngress: SUCCESS',
    'RESULT: SUCCESS'
)) {
    if (-not $text.Contains($line)) {
        throw "iOS/iPad compatibility output did not contain: $line"
    }
}

$reportDirectory = Split-Path -Parent $reportPath
if (-not (Test-Path $reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory | Out-Null
}

$report = @(
    '# MA017 iOS/iPad Handoff Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Ergebnis',
    '',
    '- iOS START HERE: OK',
    '- Xcode Minimal App Task: OK',
    '- iOS Config Sample: OK',
    '- Capsule Contract: OK',
    '- OpenFrame Contract: OK',
    '- Haptics Contract: OK',
    '- USB Install Runbook: OK',
    '- No File Ingress Sandbox Contract: OK',
    '- iOS/iPad Compatibility Harness: SUCCESS',
    '- MA017iOSHandoff: SUCCESS'
)

Set-Content -Path $reportPath -Value $report -Encoding UTF8

if ($SmokeTest) {
    Write-Host 'MA017iOSHandoffSmoke: SUCCESS'
}

Write-Host 'MA017iOSHandoff: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
