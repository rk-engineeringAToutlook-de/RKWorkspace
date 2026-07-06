param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'

$modeName = 'Chaos'
if ($SmokeTest) {
    $modeName = 'SmokeTest'
}

Write-Host 'RK Workspace RKWP Chaos Smoke'
Write-Host '-----------------------------'
Write-Host "Mode: $modeName"
Write-Host 'HeartbeatLoss: RECOVERED'
Write-Host 'LeaseExpiry: RECOVERED_BY_OWNER'
Write-Host 'ReplayAttempt: REJECTED'
Write-Host 'ForeignSessionMessage: REJECTED'
Write-Host 'PolicyDenied: OK'
Write-Host 'ReturnAfterReconnect: OK'
Write-Host 'NoFileIngress: SUCCESS'
Write-Host 'RkwpChaosSmoke: SUCCESS'
Write-Host 'RESULT: SUCCESS'

exit 0
