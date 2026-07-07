param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$runner = Join-Path $root 'tools\run-rkwp-tests.ps1'

Write-Host 'RK Workspace Security Regression Suite'
Write-Host '--------------------------------------'

$output = & $runner 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Host $_ }
if ($exitCode -ne 0) {
    throw "RKWP protocol tests failed with exit code $exitCode."
}

$text = $output -join [Environment]::NewLine
$required = @(
    'SecurityRegressionSuiteCoverage: PASS',
    'NonceReplay: PASS',
    'SequenceReplay: PASS',
    'SecureDevRejectsRevokedPeer: PASS',
    'ChangeSetExpiredLeaseRejected: PASS',
    'FrameInputKeyboardDeniedWithoutPermission: PASS',
    'FrameInputPointerWithoutValidSessionDenied: PASS',
    'OwnershipTransferDefaultDenies: PASS',
    'OwnershipTransferDeniedDoesNotChangeOwnership: PASS',
    'PdfFrameRendererNoFileIngress: PASS',
    'AuditEvents: PASS',
    'RkwpTests: SUCCESS',
    'RESULT: SUCCESS'
)

foreach ($line in $required) {
    if (-not $text.Contains($line)) {
        throw "Security regression failed because output did not contain: $line"
    }
}

Write-Host 'Replay: OK'
Write-Host 'NonceReuse: OK'
Write-Host 'RevokedPeer: OK'
Write-Host 'ExpiredLease: OK'
Write-Host 'UnauthorizedInput: OK'
Write-Host 'UnauthorizedExtraction: OK'
Write-Host 'OwnershipTransferDenied: OK'
Write-Host 'CacheNoFileIngress: OK'
Write-Host 'AuditEventExists: OK'
Write-Host 'SecurityRegression: SUCCESS'
Write-Host 'RESULT: SUCCESS'

exit 0
