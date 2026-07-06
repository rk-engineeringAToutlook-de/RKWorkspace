param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'

Write-Host 'RK Workspace Windows Agent Dev Uninstall'
Write-Host '----------------------------------------'
Write-Host 'InstallMode: DevelopmentOnly'
Write-Host 'ServiceInstall: NOT_PERFORMED'
Write-Host 'Action: NothingToRemove'
Write-Host 'RequiresAdmin: NO'

if ($SmokeTest) {
    Write-Host 'UninstallSmoke: SUCCESS'
    Write-Host 'RESULT: SUCCESS'
}
