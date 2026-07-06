$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$exportRoot = Join-Path $root 'release\codex-context'
$staging = Join-Path $env:TEMP "RKWorkspace_Context_$timestamp"
$zipPath = Join-Path $exportRoot "RKWorkspace_Context_$timestamp.zip"
$latestZipPath = Join-Path $exportRoot 'RKWorkspace_Context_latest.zip'

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
    'Docs\Proximity\ManualAblageMap.md',
    'Docs\AblageAnchorDongle.md',
    'Docs\GestureStrategy.md',
    'Docs\Roadmap\RKWorkspace_Roadmap.md',
    'Docs\Development\WindowsLocalFrameE2E.md',
    'Docs\Development\GlassEdgePdfFrameE2E.md',
    'Docs\Development\PdfFrameInteraction.md',
    'Docs\Development\RKWP_Diagnostics.md',
    'Docs\Development\OwnerGuestFrameStateUX.md',
    'Docs\Protocol\RKWP_ProtocolFoundation.md',
    'Docs\Protocol\RKWP_SecurityModel.md',
    'Docs\Protocol\RKWP_SecureSession.md',
    'Docs\Protocol\RKWP_AblageIdentityAndTrust.md',
    'Docs\Protocol\RKWP_Pairing.md',
    'Docs\Protocol\RKWP_TransportProfiles.md',
    'Docs\Protocol\RKWP_DevTransport.md',
    'Docs\Protocol\RKWP_OwnershipAndLease.md',
    'Docs\Protocol\RKWP_FrameSession.md',
    'Docs\Protocol\RKWP_InputChannel.md',
    'Docs\Protocol\RKWP_ChangeSetAndReturn.md',
    'Docs\Protocol\RKWP_ObjectKindRules.md',
    'Docs\Protocol\RKWP_OwnershipTransfer.md',
    'Docs\Protocol\RKWP_PlatformStrategy.md',
    'Docs\Protocol\RKWP_EmergencyReturn.md',
    'Docs\Protocol\RKWP_ProductTransportPlan.md',
    'Docs\Policy\RKWP_PolicyProfiles.md',
    'Docs\Performance\RKWP_PerformanceBaseline.md',
    'Docs\Performance\PdfFramePerformance.md',
    'Docs\Security\RKWP_SecurityGate.md',
    'Docs\Security\RKWP_DevCertificates.md',
    'Docs\Security\RKWP_SecureSessionPath.md',
    'Docs\Security\AblageIdentityStore.md',
    'Docs\Security\RKWP_CryptoDecision.md',
    'Docs\Security\RKWP_MutualAuthenticationModel.md',
    'Docs\Security\RKWP_CertificateProvisioning.md',
    'Docs\Security\RKWP_SecretsStorageByPlatform.md',
    'Docs\Security\RendererSandboxModel.md',
    'Docs\Audit\RKWP_AuditTamperResistance.md',
    'Docs\UX\AblagePairingExperience.md',
    'Docs\ObjectAdapters\WindowsObjectAdapters.md',
    'Docs\ObjectAdapters\AppWindowInteractiveFrame.md',
    'Docs\ObjectAdapters\BrowserTabStrategy.md',
    'Docs\ObjectAdapters\EmailDraftAdapterRoadmap.md',
    'Docs\Readiness\MA007_ReadinessReview.md',
    'Docs\Readiness\MA009_ReadinessReview.md',
    'Docs\Readiness\MA009_RealLabTestPlan.md',
    'Docs\Readiness\MA009_NextCodexActions.md',
    'Docs\Readiness\MA010_ReadinessReview.md',
    'Docs\Readiness\MA010_PilotTestPlan.md',
    'Docs\Readiness\MA010_NextPlatformCodexActions.md',
    'Docs\Readiness\MA011_ReadinessReview.md',
    'Docs\Readiness\MA011_WindowsToMac_FirstRealTest.md',
    'Docs\Readiness\MA011_iPad_FirstRealTest.md',
    'Docs\Readiness\MA011_NextCodexActions.md',
    'Docs\Readiness\WindowsToMac_FirstRealTestGate.md',
    'Docs\Readiness\WindowsToiPad_FirstRealTestGate.md',
    'Docs\Readiness\iOS_MA013_ReadinessGate.md',
    'Docs\Readiness\MA013_SecurityReadinessGate.md',
    'Docs\Readiness\PdfFrameRendererReadiness.md',
    'Docs\Readiness\ObjectAdapterReadiness.md',
    'Docs\Readiness\CrossDeviceTestPlan_Windows_macOS_iPad.md',
    'Docs\Readiness\WindowsToMac_DevTransportPlan.md',
    'Docs\Readiness\WindowsToMac_LanTestPlan.md',
    'Docs\Readiness\iPad_iPhone_Surface_TestPlan.md',
    'Docs\Readiness\NextCodexActions.md',
    'Docs\Platform\macOS_SurfaceStarterKit.md',
    'Docs\Platform\macOS_RepositoryBootstrap.md',
    'Docs\Platform\macOS_FrameGuestUI.md',
    'Docs\Platform\macOS_NoFileIngressChecklist.md',
    'Docs\Platform\macOS_ReturnAndRecovery.md',
    'Docs\Platform\macOS_FrameRenderingStrategy.md',
    'Docs\Platform\macOS_DevAgentPackaging.md',
    'Docs\Platform\macOS_GestureAndGlassEdge.md',
    'src\Surfaces\RKWorkspace.Surface.macOS\README.md',
    'src\Surfaces\RKWorkspace.Surface.macOS\macos-project-layout.md',
    'src\Surfaces\RKWorkspace.Surface.macOS\macos-rkwp-client-flow.md',
    'src\Surfaces\RKWorkspace.Surface.macOS\macos-rkwp-client-architecture.md',
    'src\Surfaces\RKWorkspace.Surface.macOS\macos-rkwp-message-flow.md',
    'src\Surfaces\RKWorkspace.Surface.macOS\macos-rkwp-devtransport-client.md',
    'src\Surfaces\RKWorkspace.Surface.macOS\macos-rkwp-securedev-client.md',
    'src\Surfaces\RKWorkspace.Surface.macOS\macos-frame-guest-ui.md',
    'src\Surfaces\RKWorkspace.Surface.macOS\macos-frame-guest-ui-spec.md',
    'src\Surfaces\RKWorkspace.Surface.macOS\macos-permissions-checklist.md',
    'src\Surfaces\RKWorkspace.Surface.macOS\macos-build-commands.md',
    'src\Surfaces\RKWorkspace.Surface.macOS\macos-test-plan.md',
    'src\Surfaces\RKWorkspace.Surface.macOS\macOS_FrameGuestSurface_Design.md',
    'src\Surfaces\RKWorkspace.Surface.macOS\macOS_Permissions_Checklist.md',
    'src\Surfaces\RKWorkspace.Surface.macOS\macOS_Build_Notes.md',
    'src\Surfaces\RKWorkspace.Surface.iOS\README.md',
    'src\Surfaces\RKWorkspace.Surface.iOS\ios-xcode-project-layout.md',
    'src\Surfaces\RKWorkspace.Surface.iOS\ios-rkwp-client-flow.md',
    'src\Surfaces\RKWorkspace.Surface.iOS\ios-frame-guest-ui.md',
    'src\Surfaces\RKWorkspace.Surface.iOS\ios-haptics-plan.md',
    'src\Surfaces\RKWorkspace.Surface.iOS\ios-gesture-plan.md',
    'src\Surfaces\RKWorkspace.Surface.iOS\ios-usb-test-plan.md',
    'src\Surfaces\RKWorkspace.Surface.iOS\ios-sandbox-sources.md',
    'src\Surfaces\RKWorkspace.Surface.iOS\iOS_SurfaceApp_Design.md',
    'src\Surfaces\RKWorkspace.Surface.iOS\iOS_Haptics_Gesture_Plan.md',
    'src\Surfaces\RKWorkspace.Surface.iOS\iOS_Xcode_USB_TestPlan.md',
    'src\Surfaces\RKWorkspace.Surface.iOS\iOS_Sandbox_ObjectSources.md',
    'Docs\Platform\iOS_iPadOS_SurfaceStarterKit.md',
    'Docs\Platform\iOS_XcodeProjectBootstrap.md',
    'Docs\Platform\iOS_RKWPClientFlow.md',
    'Docs\Platform\iOS_FramePresenter.md',
    'Docs\Platform\iOS_HapticsAndGesturePrototype.md',
    'Docs\Platform\iOS_NoFileIngressSandboxChecklist.md',
    'Docs\Platform\iOS_USBDeviceTestRunbook.md',
    'Docs\Platform\iOS_ReturnAndRecovery.md',
    'Docs\Platform\iOS_ShareExtensionObjectSource.md',
    'Docs\Platform\Android_SurfaceStarterKit.md',
    'Docs\Platform\Linux_SurfaceStarterKit.md',
    'Docs\Codex\CODEX_ONBOARDING.md',
    'Docs\Codex\CURRENT_CONTEXT.md',
    'Docs\Codex\PLATFORM_HANDOFF.md',
    'Docs\Codex\PLATFORM_STATUS.md',
    'Docs\Codex\PlatformTasks\macOS.md',
    'Docs\Codex\PlatformTasks\iOS_iPadOS.md',
    'Docs\Codex\PlatformTasks\Android.md',
    'Docs\Codex\PlatformTasks\Linux.md',
    'Spec\TestStrategy.md',
    'Spec\ProductPhilosophy.md',
    'Docs\Configuration\RKWorkspaceConfiguration.md',
    'Docs\Development\WindowsPdfFramePilot.md',
    'Docs\Development\MacGuestCompatibilityHarness.md',
    'contracts\macOS-guest\rkwp-macos-guest-contract-v0.1.json',
    'tests\Contract\RKWorkspace.MacGuest.Contracts\RKWorkspace.MacGuest.Contracts.csproj',
    'tests\Contract\RKWorkspace.MacGuest.Contracts\Program.cs',
    'tests\Unit\RKWorkspace.Protocol.Tests\Program.cs',
    'Docs\Development\iOSGuestCompatibilityHarness.md',
    'Docs\Development\WindowsOwnerForMac.md',
    'Docs\Frame\PdfFrameRendererDecision.md',
    'Docs\Frame\PdfRendererProductDecision.md',
    'Docs\Frame\PdfFrameRendererAbstraction.md',
    'Docs\Frame\PdfAnnotationChangeSet.md',
    'Docs\Frame\FrameCachePolicy.md',
    'Docs\Platform\WindowsAgentInstallationPlan.md',
    'Docs\Platform\WindowsServicePlan.md',
    'Docs\Platform\WindowsPermissions.md',
    'release\MA007_READINESS_SUMMARY.md',
    'release\MA009_READINESS_SUMMARY.md',
    'release\MA010_READINESS_SUMMARY.md',
    'release\MA011_READINESS_SUMMARY.md',
    'release\handoff\WindowsToMac_MA008_Handoff.md',
    'release\handoff\WindowsToMac_MA009_Handoff.md',
    'release\handoff\macOS_Codex_MA009_FrameGuestSurface.md',
    'release\handoff\macOS_Codex_MA013_Bootstrap.md',
    'release\handoff\macOS_Codex_MA013_RKWPClient.md',
    'release\handoff\WindowsToMac_FirstRealTestGate.md',
    'release\handoff\iOS_XcodeProjectBootstrap_MA013.md',
    'release\handoff\WindowsToiPad_FirstRealTestGate.md',
    'release\handoff\iOS_iPadOS_Codex_MA009_SurfaceApp.md',
    'release\handoff\iOS_iPadOS_MA008_Handoff.md',
    'release\schema\rkwp-envelope-schema-v0.1.json',
    'config\samples\manual-ablage-map.sample.json',
    'config\samples\rkworkspace.sample.json',
    'config\samples\ablage.sample.json',
    'config\samples\rkwp-dev.sample.json',
    'config\samples\rkwp-dev-lan-owner.sample.json',
    'config\samples\rkwp-dev-lan-guest.sample.json',
    'config\samples\policy-critical.sample.json',
    'src\Shell\RKWorkspace.Shell\Ablage\ManualAblageMap.cs',
    'src\Frame\RKWorkspace.Frame.Pdf\OwnerGuestFrameStateUx.cs',
    'src\Frame\RKWorkspace.Frame.Pdf\PdfDocumentFrameState.cs',
    'src\Frame\RKWorkspace.Frame.Pdf\PdfFrameTiles.cs',
    'src\Frame\RKWorkspace.Frame.Pdf\PdfAnnotationChangeSet.cs',
    'src\Frame\RKWorkspace.Frame.Pdf\PdfTextExtraction.cs',
    'src\Frame\RKWorkspace.Frame.Pdf\PdfFrameValidation.cs',
    'src\Adapters\RKWorkspace.ObjectAdapter.Windows\WindowsObjectAdapters.cs',
    'src\Tools\RKWorkspace.WindowsObjectAdapterSmoke\RKWorkspace.WindowsObjectAdapterSmoke.csproj',
    'src\Tools\RKWorkspace.WindowsObjectAdapterSmoke\Program.cs',
    'src\Protocol\RKWorkspace.Protocol\Ownership\RkwpPolicyProfiles.cs',
    'src\Tools\RKWorkspace.RkwpPerfHarness\RKWorkspace.RkwpPerfHarness.csproj',
    'src\Tools\RKWorkspace.RkwpPerfHarness\Program.cs',
    'tools\run-windows-owner-for-mac.ps1',
    'tools\run-windows-pdf-owner-securedev.ps1',
    'tools\run-mac-guest-contract.ps1',
    'tools\export-rkwp-schema.ps1',
    'tools\init-dev-rkwp-identity.ps1',
    'tools\init-ablage-identity.ps1',
    'tools\run-rkwp-diagnostics.ps1',
    'tools\run-manual-map.ps1',
    'tools\run-policy-profile.ps1',
    'tools\run-rkwp-perf.ps1',
    'tools\run-rkwp-lan-smoke.ps1',
    'tools\run-security-regression.ps1',
    'tools\run-windows-pdf-frame-pilot.ps1',
    'tools\run-windows-object-adapter.ps1',
    'tools\run-config-tool.ps1',
    'tools\run-windows-agent-dev.ps1',
    'tools\run-mac-guest-compat.ps1',
    'tools\run-ios-guest-compat.ps1'
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
.\tools\run-windows-owner-for-mac.ps1 -InfoOnly
.\tools\run-rkwp-diagnostics.ps1 -SmokeTest
.\tools\run-rkwp-perf.ps1 -SmokeTest
.\tools\run-rkwp-lan-smoke.ps1
.\tools\run-windows-pdf-frame-pilot.ps1 -SmokeTest
.\tools\run-config-tool.ps1 -SmokeTest
.\tools\run-windows-agent-dev.ps1 -SmokeTest
.\tools\run-mac-guest-compat.ps1 -SmokeTest
.\tools\run-ios-guest-compat.ps1 -SmokeTest
.\tools\export-codex-context.ps1
'@ | Set-Content -Path (Join-Path $staging 'test-commands.txt') -Encoding UTF8

@'
Windows: RKWP Frame Presenter an Native Glass Edge anbinden.
Windows: Baue sichtbaren Windows PDF Owner/Guest Pilot weiter aus und stabilisiere Frame UI.
macOS: Baue macOS Frame Guest Surface mit RKWP DevLan Client.
iOS/iPadOS: Baue iPad/iPhone RK Workspace Surface App, die RKWP Frame anzeigen kann, Haptik unterstuetzt und No File Ingress respektiert.
Android: Baue Android Frame Guest Surface mit RKWP DevTransport.
Linux: Baue Linux Frame Guest Surface unter Wayland/X11-Beruecksichtigung.
Hardware: Definiere Dongle Hardware MVP fuer Ablage-Anker mit BLE/UWB/USB.
'@ | Set-Content -Path (Join-Path $staging 'platform-next-steps.txt') -Encoding UTF8

Compress-Archive -Path (Join-Path $staging '*') -DestinationPath $zipPath -Force
Copy-Item -LiteralPath $zipPath -Destination $latestZipPath -Force
Remove-Item -LiteralPath $staging -Recurse -Force

Write-Host 'RK Workspace Codex Context Export'
Write-Host "Output: $zipPath"
Write-Host "Latest: $latestZipPath"
Write-Host 'RESULT: SUCCESS'
