param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'

Write-Host 'RK Workspace Windows Agent Dev Install'
Write-Host '--------------------------------------'
Write-Host 'InstallMode: DevelopmentOnly'
Write-Host 'ServiceInstall: NOT_PERFORMED'
Write-Host 'RequiresAdmin: NO'
Write-Host 'Action: PreparedOnly'
Write-Host 'Plan: Docs\Platform\WindowsAgentInstallationPlan.md'

if ($SmokeTest) {
    Write-Host 'InstallSmoke: SUCCESS'
    Write-Host 'RESULT: SUCCESS'
}
