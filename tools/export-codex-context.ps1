$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$exportRoot = Join-Path $root 'release\codex-context'
$staging = Join-Path $env:TEMP "RKWorkspace_Context_$timestamp"
$zipPath = Join-Path $exportRoot "RKWorkspace_Context_$timestamp.zip"

New-Item -ItemType Directory -Path $exportRoot -Force | Out-Null
Remove-Item -LiteralPath $staging -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $staging -Force | Out-Null

function Copy-ContextFile {
    param(
        [Parameter(Mandatory = $true)]
        [string] $RelativePath
    )

    $source = Join-Path $root $RelativePath
    if (-not (Test-Path $source)) {
        return
    }

    $target = Join-Path $staging $RelativePath
    New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
    Copy-Item -LiteralPath $source -Destination $target -Force
}

$files = @(
    'README.md',
    'Docs\Nordstern.md',
    'Docs\WorkspaceShell.md',
    'Docs\GlassEdgeNearestAblage.md',
    'Docs\AblageProximityAndDistance.md',
    'Docs\AblageAnchorDongle.md',
    'Docs\GestureStrategy.md',
    'Docs\Roadmap\RKWorkspace_Roadmap.md',
    'Docs\Protocol\RKWP_ProtocolFoundation.md',
    'Docs\Protocol\RKWP_SecurityModel.md',
    'Docs\Protocol\RKWP_TransportProfiles.md',
    'Docs\Protocol\RKWP_OwnershipAndLease.md',
    'Docs\Protocol\RKWP_FrameSession.md',
    'Docs\Protocol\RKWP_InputChannel.md',
    'Docs\Protocol\RKWP_ChangeSetAndReturn.md',
    'Docs\Protocol\RKWP_ObjectKindRules.md',
    'Docs\Protocol\RKWP_OwnershipTransfer.md',
    'Docs\Protocol\RKWP_PlatformStrategy.md',
    'Docs\ObjectAdapters\WindowsObjectAdapters.md',
    'Docs\Platform\macOS_SurfaceStarterKit.md',
    'Docs\Codex\CODEX_ONBOARDING.md',
    'Docs\Codex\CURRENT_CONTEXT.md',
    'Docs\Codex\PLATFORM_HANDOFF.md',
    'Docs\Codex\PLATFORM_STATUS.md',
    'Docs\Codex\PlatformTasks\macOS.md',
    'Spec\TestStrategy.md',
    'Spec\ProductPhilosophy.md'
)

$files | ForEach-Object { Copy-ContextFile $_ }

git -C $root status --short --branch | Set-Content -Path (Join-Path $staging 'git-status.txt') -Encoding UTF8
git -C $root log --oneline -30 | Set-Content -Path (Join-Path $staging 'git-log-last-30.txt') -Encoding UTF8
git -C $root ls-files | Where-Object { $_ -notmatch '(^|/)(bin|obj)/' } | Set-Content -Path (Join-Path $staging 'project-tree.txt') -Encoding UTF8

@'
.\tools\run-tests.ps1
.\tools\run-studio.ps1 -SmokeTest
.\tools\run-rkwp-tests.ps1
.\tools\run-pdf-frame-smoke.ps1
.\tools\export-codex-context.ps1
'@ | Set-Content -Path (Join-Path $staging 'test-commands.txt') -Encoding UTF8

@'
Windows: RKWP Frame Presenter an Native Glass Edge anbinden.
macOS: Surface Host, Gesten und FrameOnly View planen.
iOS/iPadOS: Tablet-/Phone-Ablage mit TouchHold und FrameOnly View planen.
Android: Touch/Haptik und FrameOnly View planen.
Linux: Minimal Surface Host fuer Desktop/Industrie planen.
'@ | Set-Content -Path (Join-Path $staging 'platform-next-steps.txt') -Encoding UTF8

Compress-Archive -Path (Join-Path $staging '*') -DestinationPath $zipPath -Force
Remove-Item -LiteralPath $staging -Recurse -Force

Write-Host 'RK Workspace Codex Context Export'
Write-Host "Output: $zipPath"
Write-Host 'RESULT: SUCCESS'
