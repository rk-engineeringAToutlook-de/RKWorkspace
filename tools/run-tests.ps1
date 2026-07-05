$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Label,

        [Parameter(Mandatory = $true)]
        [scriptblock] $Command
    )

    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$Label failed with exit code $LASTEXITCODE."
    }
}

Write-Host 'Build'
Write-Host '-----'
Invoke-Checked 'Build' { dotnet build (Join-Path $root 'src\Core\RKWorkspace.Core.csproj') }

Write-Host ''
Write-Host 'Unit Tests'
Write-Host '----------'
Invoke-Checked 'Unit Tests' { dotnet run --project (Join-Path $root 'tests\Unit\RKWorkspace.Core.Tests\RKWorkspace.Core.Tests.csproj') }

Write-Host ''
Write-Host 'Integration Tests'
Write-Host '-----------------'
Invoke-Checked 'Integration Tests' { dotnet run --project (Join-Path $root 'tests\Integration\RKWorkspace.Core.IntegrationTests\RKWorkspace.Core.IntegrationTests.csproj') }

Write-Host ''
Write-Host 'Simulation'
Write-Host '----------'
Invoke-Checked 'Simulation' { dotnet run --project (Join-Path $root 'tools\LocalSimulation\RKWorkspace.LocalSimulation.csproj') }

Write-Host ''
Write-Host 'Demo Test'
Write-Host '---------'
$demoOutput = & (Join-Path $root 'tools\run-demo.ps1') 2>&1
$demoExitCode = $LASTEXITCODE
$demoOutput | ForEach-Object { Write-Host $_ }
if ($demoExitCode -ne 0) {
    throw "Demo Test failed with exit code $demoExitCode."
}

$demoText = $demoOutput -join [Environment]::NewLine
if (-not $demoText.Contains('RESULT: SUCCESS')) {
    throw "Demo Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'Agent Smoke Test'
Write-Host '----------------'
$agentOutput = & (Join-Path $root 'tools\run-agent.ps1') -Once 2>&1
$agentExitCode = $LASTEXITCODE
$agentOutput | ForEach-Object { Write-Host $_ }
if ($agentExitCode -ne 0) {
    throw "Agent Smoke Test failed with exit code $agentExitCode."
}

$agentText = $agentOutput -join [Environment]::NewLine
if (-not $agentText.Contains('RK Workspace Agent')) {
    throw "Agent Smoke Test failed because output did not contain RK Workspace Agent."
}

if (-not $agentText.Contains('State: Running')) {
    throw "Agent Smoke Test failed because output did not contain State: Running."
}

if (-not $agentText.Contains('stopped cleanly')) {
    throw "Agent Smoke Test failed because output did not contain stopped cleanly."
}

Write-Host ''
Write-Host 'Dual Agent Harness Test'
Write-Host '-----------------------'
$dualAgentOutput = & (Join-Path $root 'tools\run-dual-agent.ps1') 2>&1
$dualAgentExitCode = $LASTEXITCODE
$dualAgentOutput | ForEach-Object { Write-Host $_ }
if ($dualAgentExitCode -ne 0) {
    throw "Dual Agent Harness Test failed with exit code $dualAgentExitCode."
}

$dualAgentText = $dualAgentOutput -join [Environment]::NewLine
if (-not $dualAgentText.Contains('RK Workspace Dual Agent Harness')) {
    throw "Dual Agent Harness Test failed because output did not contain RK Workspace Dual Agent Harness."
}

if (-not $dualAgentText.Contains('Agent A: rkws-agent-a')) {
    throw "Dual Agent Harness Test failed because output did not contain Agent A."
}

if (-not $dualAgentText.Contains('Agent B: rkws-agent-b')) {
    throw "Dual Agent Harness Test failed because output did not contain Agent B."
}

if (-not $dualAgentText.Contains('Transfer Result: SUCCESS')) {
    throw "Dual Agent Harness Test failed because output did not contain Transfer Result: SUCCESS."
}

if (-not $dualAgentText.Contains('RESULT: SUCCESS')) {
    throw "Dual Agent Harness Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'Local IPC Two Process Test'
Write-Host '--------------------------'
$localIpcScript = Join-Path $root 'tools\run-local-ipc.ps1'
$localIpcStdOut = [System.IO.Path]::GetTempFileName()
$localIpcStdErr = [System.IO.Path]::GetTempFileName()
try {
    $localIpcProcess = Start-Process `
        -FilePath 'powershell' `
        -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$localIpcScript`"" `
        -RedirectStandardOutput $localIpcStdOut `
        -RedirectStandardError $localIpcStdErr `
        -PassThru

    if (-not $localIpcProcess.WaitForExit(60000)) {
        try {
            $localIpcProcess.Kill()
        }
        catch {
        }

        throw 'Local IPC Two Process Test timed out after 60 seconds.'
    }

    $localIpcOutput = @()
    if (Test-Path $localIpcStdOut) {
        $localIpcOutput += Get-Content $localIpcStdOut
    }

    if (Test-Path $localIpcStdErr) {
        $localIpcOutput += Get-Content $localIpcStdErr
    }

    $localIpcOutput | ForEach-Object { Write-Host $_ }
    if ($localIpcProcess.ExitCode -ne 0) {
        throw "Local IPC Two Process Test failed with exit code $($localIpcProcess.ExitCode)."
    }

    $localIpcText = $localIpcOutput -join [Environment]::NewLine
    if (-not $localIpcText.Contains('RK Workspace Local IPC Harness')) {
        throw "Local IPC Two Process Test failed because output did not contain RK Workspace Local IPC Harness."
    }

    if (-not $localIpcText.Contains('AgentHello: OK')) {
        throw "Local IPC Two Process Test failed because output did not contain AgentHello: OK."
    }

    if (-not $localIpcText.Contains('StatusRequest: OK')) {
        throw "Local IPC Two Process Test failed because output did not contain StatusRequest: OK."
    }

    if (-not $localIpcText.Contains('TransferRequest: OK')) {
        throw "Local IPC Two Process Test failed because output did not contain TransferRequest: OK."
    }

    if (-not $localIpcText.Contains('TransferResponse: SUCCESS')) {
        throw "Local IPC Two Process Test failed because output did not contain TransferResponse: SUCCESS."
    }

    if (-not $localIpcText.Contains('RESULT: SUCCESS')) {
        throw "Local IPC Two Process Test failed because output did not contain RESULT: SUCCESS."
    }
}
finally {
    Remove-Item -LiteralPath $localIpcStdOut -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $localIpcStdErr -Force -ErrorAction SilentlyContinue
}

Write-Host ''
Write-Host 'Workspace Shell Smoke Test'
Write-Host '--------------------------'
$shellOutput = & (Join-Path $root 'tools\run-shell.ps1') -Once 2>&1
$shellExitCode = $LASTEXITCODE
$shellOutput | ForEach-Object { Write-Host $_ }
if ($shellExitCode -ne 0) {
    throw "Workspace Shell Smoke Test failed with exit code $shellExitCode."
}

$shellText = $shellOutput -join [Environment]::NewLine
if (-not $shellText.Contains('RK Workspace Shell')) {
    throw "Workspace Shell Smoke Test failed because output did not contain RK Workspace Shell."
}

if (-not $shellText.Contains('State: Running')) {
    throw "Workspace Shell Smoke Test failed because output did not contain State: Running."
}

if (-not $shellText.Contains('CarryState: Empty')) {
    throw "Workspace Shell Smoke Test failed because output did not contain CarryState: Empty."
}

if (-not $shellText.Contains('RESULT: SUCCESS')) {
    throw "Workspace Shell Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'Workspace Overlay Smoke Test'
Write-Host '----------------------------'
$overlayOutput = & (Join-Path $root 'tools\run-shell.ps1') -OverlaySmokeTest 2>&1
$overlayExitCode = $LASTEXITCODE
$overlayOutput | ForEach-Object { Write-Host $_ }
if ($overlayExitCode -ne 0) {
    throw "Workspace Overlay Smoke Test failed with exit code $overlayExitCode."
}

$overlayText = $overlayOutput -join [Environment]::NewLine
if (-not $overlayText.Contains('RK Workspace Overlay Smoke Test')) {
    throw "Workspace Overlay Smoke Test failed because output did not contain RK Workspace Overlay Smoke Test."
}

if (-not $overlayText.Contains('OverlaySmoke: SUCCESS')) {
    throw "Workspace Overlay Smoke Test failed because output did not contain OverlaySmoke: SUCCESS."
}

if (-not $overlayText.Contains('RESULT: SUCCESS')) {
    throw "Workspace Overlay Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'Spatial Room Smoke Test'
Write-Host '-----------------------'
$spatialTrayOutput = & (Join-Path $root 'tools\run-spatial-tray.ps1') -SmokeTest 2>&1
$spatialTrayExitCode = $LASTEXITCODE
$spatialTrayOutput | ForEach-Object { Write-Host $_ }
if ($spatialTrayExitCode -ne 0) {
    throw "Spatial Room Smoke Test failed with exit code $spatialTrayExitCode."
}

$spatialTrayText = $spatialTrayOutput -join [Environment]::NewLine
if (-not $spatialTrayText.Contains('Spatial Room Smoke Test')) {
    throw "Spatial Room Smoke Test failed because output did not contain Spatial Room Smoke Test."
}

if (-not $spatialTrayText.Contains('Monitor preview: OK')) {
    throw "Spatial Room Smoke Test failed because output did not contain Monitor preview: OK."
}

if (-not $spatialTrayText.Contains('Pick from monitor: OK')) {
    throw "Spatial Room Smoke Test failed because output did not contain Pick from monitor: OK."
}

if (-not $spatialTrayText.Contains('RESULT: SUCCESS')) {
    throw "Spatial Room Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'Mobile Spatial Surface Smoke Test'
Write-Host '---------------------------------'
$mobileSpatialOutput = & (Join-Path $root 'tools\run-mobile-spatial-surface.ps1') -SmokeTest 2>&1
$mobileSpatialExitCode = $LASTEXITCODE
$mobileSpatialOutput | ForEach-Object { Write-Host $_ }
if ($mobileSpatialExitCode -ne 0) {
    throw "Mobile Spatial Surface Smoke Test failed with exit code $mobileSpatialExitCode."
}

$mobileSpatialText = $mobileSpatialOutput -join [Environment]::NewLine
if (-not $mobileSpatialText.Contains('RK Workspace Mobile Spatial Surface Smoke Test')) {
    throw "Mobile Spatial Surface Smoke Test failed because output did not contain RK Workspace Mobile Spatial Surface Smoke Test."
}

if (-not $mobileSpatialText.Contains('MobileSpatialMode: OK')) {
    throw "Mobile Spatial Surface Smoke Test failed because output did not contain MobileSpatialMode: OK."
}

if (-not $mobileSpatialText.Contains('LensesAfterGesture: OK')) {
    throw "Mobile Spatial Surface Smoke Test failed because output did not contain LensesAfterGesture: OK."
}

if (-not $mobileSpatialText.Contains('DistanceScaling: OK')) {
    throw "Mobile Spatial Surface Smoke Test failed because output did not contain DistanceScaling: OK."
}

if (-not $mobileSpatialText.Contains('MobileSpatialSurfaceSmoke: SUCCESS')) {
    throw "Mobile Spatial Surface Smoke Test failed because output did not contain MobileSpatialSurfaceSmoke: SUCCESS."
}

if (-not $mobileSpatialText.Contains('RESULT: SUCCESS')) {
    throw "Mobile Spatial Surface Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'Native Spatial Overlay Smoke Test'
Write-Host '---------------------------------'
$nativeOverlayOutput = & (Join-Path $root 'tools\run-native-overlay.ps1') -SmokeTest 2>&1
$nativeOverlayExitCode = $LASTEXITCODE
$nativeOverlayOutput | ForEach-Object { Write-Host $_ }
if ($nativeOverlayExitCode -ne 0) {
    throw "Native Spatial Overlay Smoke Test failed with exit code $nativeOverlayExitCode."
}

$nativeOverlayText = $nativeOverlayOutput -join [Environment]::NewLine
if (-not $nativeOverlayText.Contains('RK Workspace Native Spatial Overlay Smoke Test')) {
    throw "Native Spatial Overlay Smoke Test failed because output did not contain RK Workspace Native Spatial Overlay Smoke Test."
}

if (-not $nativeOverlayText.Contains('BrowserSurface: NONE')) {
    throw "Native Spatial Overlay Smoke Test failed because output did not contain BrowserSurface: NONE."
}

if (-not $nativeOverlayText.Contains('NativeOverlaySmoke: SUCCESS')) {
    throw "Native Spatial Overlay Smoke Test failed because output did not contain NativeOverlaySmoke: SUCCESS."
}

if (-not $nativeOverlayText.Contains('RESULT: SUCCESS')) {
    throw "Native Spatial Overlay Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'Visual Reality Smoke Test'
Write-Host '-------------------------'
$visualRealityOutput = & (Join-Path $root 'tools\run-visual-reality.ps1') -SmokeTest 2>&1
$visualRealityExitCode = $LASTEXITCODE
$visualRealityOutput | ForEach-Object { Write-Host $_ }
if ($visualRealityExitCode -ne 0) {
    throw "Visual Reality Smoke Test failed with exit code $visualRealityExitCode."
}

$visualRealityText = $visualRealityOutput -join [Environment]::NewLine
if (-not $visualRealityText.Contains('RK Workspace Visual Reality Smoke Test')) {
    throw "Visual Reality Smoke Test failed because output did not contain RK Workspace Visual Reality Smoke Test."
}

if (-not $visualRealityText.Contains('BrowserSurface: NONE')) {
    throw "Visual Reality Smoke Test failed because output did not contain BrowserSurface: NONE."
}

if (-not $visualRealityText.Contains('LensVariants: 5')) {
    throw "Visual Reality Smoke Test failed because output did not contain LensVariants: 5."
}

if (-not $visualRealityText.Contains('ReferenceDirection: OK')) {
    throw "Visual Reality Smoke Test failed because output did not contain ReferenceDirection: OK."
}

if (-not $visualRealityText.Contains('VisualRealitySmoke: SUCCESS')) {
    throw "Visual Reality Smoke Test failed because output did not contain VisualRealitySmoke: SUCCESS."
}

if (-not $visualRealityText.Contains('RESULT: SUCCESS')) {
    throw "Visual Reality Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'Living Lens Smoke Test'
Write-Host '----------------------'
$livingLensOutput = & (Join-Path $root 'tools\run-living-lens.ps1') -SmokeTest 2>&1
$livingLensExitCode = $LASTEXITCODE
$livingLensOutput | ForEach-Object { Write-Host $_ }
if ($livingLensExitCode -ne 0) {
    throw "Living Lens Smoke Test failed with exit code $livingLensExitCode."
}

$livingLensText = $livingLensOutput -join [Environment]::NewLine
if (-not $livingLensText.Contains('RK Workspace Living Lens Smoke Test')) {
    throw "Living Lens Smoke Test failed because output did not contain RK Workspace Living Lens Smoke Test."
}

if (-not $livingLensText.Contains('BrowserSurface: NONE')) {
    throw "Living Lens Smoke Test failed because output did not contain BrowserSurface: NONE."
}

if (-not $livingLensText.Contains('PerPixelAlpha: OK')) {
    throw "Living Lens Smoke Test failed because output did not contain PerPixelAlpha: OK."
}

if (-not $livingLensText.Contains('ColorKeyTransparency: NONE')) {
    throw "Living Lens Smoke Test failed because output did not contain ColorKeyTransparency: NONE."
}

if (-not $livingLensText.Contains('LensVariants: 5')) {
    throw "Living Lens Smoke Test failed because output did not contain LensVariants: 5."
}

if (-not $livingLensText.Contains('DefaultGlassLens: OK')) {
    throw "Living Lens Smoke Test failed because output did not contain DefaultGlassLens: OK."
}

if (-not $livingLensText.Contains('RealBubbleLens: OK')) {
    throw "Living Lens Smoke Test failed because output did not contain RealBubbleLens: OK."
}

if (-not $livingLensText.Contains('PrimaryEdgeLens: OK')) {
    throw "Living Lens Smoke Test failed because output did not contain PrimaryEdgeLens: OK."
}

if (-not $livingLensText.Contains('NoWhiteAblageFrame: OK')) {
    throw "Living Lens Smoke Test failed because output did not contain NoWhiteAblageFrame: OK."
}

if (-not $livingLensText.Contains('LensAbsorption: OK')) {
    throw "Living Lens Smoke Test failed because output did not contain LensAbsorption: OK."
}

if (-not $livingLensText.Contains('NoAutoAbsorption: OK')) {
    throw "Living Lens Smoke Test failed because output did not contain NoAutoAbsorption: OK."
}

if (-not $livingLensText.Contains('LensRelaxAway: OK')) {
    throw "Living Lens Smoke Test failed because output did not contain LensRelaxAway: OK."
}

if (-not $livingLensText.Contains('PullOutFromLens: OK')) {
    throw "Living Lens Smoke Test failed because output did not contain PullOutFromLens: OK."
}

if (-not $livingLensText.Contains('ExportFrames: OK')) {
    throw "Living Lens Smoke Test failed because output did not contain ExportFrames: OK."
}

if (-not $livingLensText.Contains('ExtremeFxMode: OK')) {
    throw "Living Lens Smoke Test failed because output did not contain ExtremeFxMode: OK."
}

if (-not $livingLensText.Contains('ExtremePresets: OK')) {
    throw "Living Lens Smoke Test failed because output did not contain ExtremePresets: OK."
}

if (-not $livingLensText.Contains('IntensitySwitching: OK')) {
    throw "Living Lens Smoke Test failed because output did not contain IntensitySwitching: OK."
}

if (-not $livingLensText.Contains('Timing2400: OK')) {
    throw "Living Lens Smoke Test failed because output did not contain Timing2400: OK."
}

if (-not $livingLensText.Contains('DebugDefaultHidden: OK')) {
    throw "Living Lens Smoke Test failed because output did not contain DebugDefaultHidden: OK."
}

if (-not $livingLensText.Contains('LivingLensSmoke: SUCCESS')) {
    throw "Living Lens Smoke Test failed because output did not contain LivingLensSmoke: SUCCESS."
}

if (-not $livingLensText.Contains('RESULT: SUCCESS')) {
    throw "Living Lens Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'GPU Living Lens Smoke Test'
Write-Host '--------------------------'
$gpuLivingLensOutput = & (Join-Path $root 'tools\run-gpu-lens.ps1') -SmokeTest 2>&1
$gpuLivingLensExitCode = $LASTEXITCODE
$gpuLivingLensOutput | ForEach-Object { Write-Host $_ }
if ($gpuLivingLensExitCode -ne 0) {
    throw "GPU Living Lens Smoke Test failed with exit code $gpuLivingLensExitCode."
}

$gpuLivingLensText = $gpuLivingLensOutput -join [Environment]::NewLine
if (-not $gpuLivingLensText.Contains('RK Workspace GPU Living Lens Smoke Test')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain RK Workspace GPU Living Lens Smoke Test."
}

if (-not $gpuLivingLensText.Contains('GpuComposition: READY')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain GpuComposition: READY."
}

if (-not $gpuLivingLensText.Contains('DesktopSampling: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain DesktopSampling: OK."
}

if (-not $gpuLivingLensText.Contains('DesktopRefraction: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain DesktopRefraction: OK."
}

if (-not $gpuLivingLensText.Contains('HlslShaderContract: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain HlslShaderContract: OK."
}

if (-not $gpuLivingLensText.Contains('NoPaperAxisSpin: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain NoPaperAxisSpin: OK."
}

if (-not $gpuLivingLensText.Contains('RectangularThing: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain RectangularThing: OK."
}

if (-not $gpuLivingLensText.Contains('RectangularShadow: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain RectangularShadow: OK."
}

if (-not $gpuLivingLensText.Contains('GentleCarryTilt: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain GentleCarryTilt: OK."
}

if (-not $gpuLivingLensText.Contains('SoftShadow: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain SoftShadow: OK."
}

if (-not $gpuLivingLensText.Contains('PortalEdgePull: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain PortalEdgePull: OK."
}

if (-not $gpuLivingLensText.Contains('PortalEdgeSqueeze: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain PortalEdgeSqueeze: OK."
}

if (-not $gpuLivingLensText.Contains('PortalEdgeApexSqueeze: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain PortalEdgeApexSqueeze: OK."
}

if (-not $gpuLivingLensText.Contains('NoTwistPortalFunnel: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain NoTwistPortalFunnel: OK."
}

if (-not $gpuLivingLensText.Contains('TiltDampingNearTunnel: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain TiltDampingNearTunnel: OK."
}

if (-not $gpuLivingLensText.Contains('TunnelDepthLayers: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain TunnelDepthLayers: OK."
}

if (-not $gpuLivingLensText.Contains('PremiumTunnelVisual: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain PremiumTunnelVisual: OK."
}

if (-not $gpuLivingLensText.Contains('PremiumTunnelRefraction: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain PremiumTunnelRefraction: OK."
}

if (-not $gpuLivingLensText.Contains('PremiumTunnelAperture: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain PremiumTunnelAperture: OK."
}

if (-not $gpuLivingLensText.Contains('EightTunnelField: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain EightTunnelField: OK."
}

if (-not $gpuLivingLensText.Contains('CornerAndEdgeTunnels: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain CornerAndEdgeTunnels: OK."
}

if (-not $gpuLivingLensText.Contains('CenterStartObject: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain CenterStartObject: OK."
}

if (-not $gpuLivingLensText.Contains('VectorSuctionCenter: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain VectorSuctionCenter: OK."
}

if (-not $gpuLivingLensText.Contains('ThroatSuctionTarget: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain ThroatSuctionTarget: OK."
}

if (-not $gpuLivingLensText.Contains('NoApexOvershoot: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain NoApexOvershoot: OK."
}

if (-not $gpuLivingLensText.Contains('StableTunnelTargetLock: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain StableTunnelTargetLock: OK."
}

if (-not $gpuLivingLensText.Contains('ThroatPointCollapse: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain ThroatPointCollapse: OK."
}

if (-not $gpuLivingLensText.Contains('ThreePremiumLensLooks: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain ThreePremiumLensLooks: OK."
}

if (-not $gpuLivingLensText.Contains('FiveExtremeFxPresets: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain FiveExtremeFxPresets: OK."
}

if (-not $gpuLivingLensText.Contains('GlassBubbleLook: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain GlassBubbleLook: OK."
}

if (-not $gpuLivingLensText.Contains('WaterLensLook: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain WaterLensLook: OK."
}

if (-not $gpuLivingLensText.Contains('WormholeLook: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain WormholeLook: OK."
}

if (-not $gpuLivingLensText.Contains('GravityWellLook: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain GravityWellLook: OK."
}

if (-not $gpuLivingLensText.Contains('PortalAbsorptionLook: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain PortalAbsorptionLook: OK."
}

if (-not $gpuLivingLensText.Contains('HybridLook: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain HybridLook: OK."
}

if (-not $gpuLivingLensText.Contains('LiveLookSwitch: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain LiveLookSwitch: OK."
}

if (-not $gpuLivingLensText.Contains('ExtremeFxMode: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain ExtremeFxMode: OK."
}

if (-not $gpuLivingLensText.Contains('IntensitySwitching: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain IntensitySwitching: OK."
}

if (-not $gpuLivingLensText.Contains('Timing2400: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain Timing2400: OK."
}

if (-not $gpuLivingLensText.Contains('DebugDefaultHidden: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain DebugDefaultHidden: OK."
}

if (-not $gpuLivingLensText.Contains('CompactCarryCard: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain CompactCarryCard: OK."
}

if (-not $gpuLivingLensText.Contains('CleanDesktopPlate: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain CleanDesktopPlate: OK."
}

if (-not $gpuLivingLensText.Contains('SelfSamplingEchoSuppression: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain SelfSamplingEchoSuppression: OK."
}

if (-not $gpuLivingLensText.Contains('SoftFresnelEdge: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain SoftFresnelEdge: OK."
}

if (-not $gpuLivingLensText.Contains('SmoothPickupScale: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain SmoothPickupScale: OK."
}

if (-not $gpuLivingLensText.Contains('SmoothLensApproach: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain SmoothLensApproach: OK."
}

if (-not $gpuLivingLensText.Contains('UltraFineGlassOptics: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain UltraFineGlassOptics: OK."
}

if (-not $gpuLivingLensText.Contains('HighResolutionVectorOptics: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain HighResolutionVectorOptics: OK."
}

if (-not $gpuLivingLensText.Contains('LiveDesktopRefraction: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain LiveDesktopRefraction: OK."
}

if (-not $gpuLivingLensText.Contains('CaptureExclusion: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain CaptureExclusion: OK."
}

if (-not $gpuLivingLensText.Contains('LensCenterLock: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain LensCenterLock: OK."
}

if (-not $gpuLivingLensText.Contains('MicroGlassHighlights: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain MicroGlassHighlights: OK."
}

if (-not $gpuLivingLensText.Contains('PhysicalGlassMaterial: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain PhysicalGlassMaterial: OK."
}

if (-not $gpuLivingLensText.Contains('GlassThickness: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain GlassThickness: OK."
}

if (-not $gpuLivingLensText.Contains('ChromaticEdge: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain ChromaticEdge: OK."
}

if (-not $gpuLivingLensText.Contains('LensContactShadow: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain LensContactShadow: OK."
}

if (-not $gpuLivingLensText.Contains('GlassCaustics: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain GlassCaustics: OK."
}

if (-not $gpuLivingLensText.Contains('SpecularGlassSweeps: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain SpecularGlassSweeps: OK."
}

if (-not $gpuLivingLensText.Contains('CompiledPixelShader: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain CompiledPixelShader: OK."
}

if (-not $gpuLivingLensText.Contains('NativeShaderLayer: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain NativeShaderLayer: OK."
}

if (-not $gpuLivingLensText.Contains('ShaderMaterialRefraction: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain ShaderMaterialRefraction: OK."
}

if (-not $gpuLivingLensText.Contains('EdgeContinuation: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain EdgeContinuation: OK."
}

if (-not $gpuLivingLensText.Contains('LensAppearsOnPick: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain LensAppearsOnPick: OK."
}

if (-not $gpuLivingLensText.Contains('DropRequiresRelease: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain DropRequiresRelease: OK."
}

if (-not $gpuLivingLensText.Contains('PullOutFromLens: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain PullOutFromLens: OK."
}

if (-not $gpuLivingLensText.Contains('PerspectiveTrapezoid: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain PerspectiveTrapezoid: OK."
}

if (-not $gpuLivingLensText.Contains('CarryShadowOnly: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain CarryShadowOnly: OK."
}

if (-not $gpuLivingLensText.Contains('ShadowSuction: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain ShadowSuction: OK."
}

if (-not $gpuLivingLensText.Contains('ShadowTunnelSuction: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain ShadowTunnelSuction: OK."
}

if (-not $gpuLivingLensText.Contains('CalmRestingObjectInTunnel: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain CalmRestingObjectInTunnel: OK."
}

if (-not $gpuLivingLensText.Contains('TransitTimeoutMs: 10000')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain TransitTimeoutMs: 10000."
}

if (-not $gpuLivingLensText.Contains('TransitCountdown: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain TransitCountdown: OK."
}

if (-not $gpuLivingLensText.Contains('RetakeResetsTransitTimer: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain RetakeResetsTransitTimer: OK."
}

if (-not $gpuLivingLensText.Contains('RemotePlacement: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain RemotePlacement: OK."
}

if (-not $gpuLivingLensText.Contains('TunnelAutoClose: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain TunnelAutoClose: OK."
}

if (-not $gpuLivingLensText.Contains('TunnelClosedAfterTransit: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain TunnelClosedAfterTransit: OK."
}

if (-not $gpuLivingLensText.Contains('RemoteGestureRequired: OK')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain RemoteGestureRequired: OK."
}

if (-not $gpuLivingLensText.Contains('TransitState: Closed')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain TransitState: Closed."
}

if (-not $gpuLivingLensText.Contains('GpuLivingLensSmoke: SUCCESS')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain GpuLivingLensSmoke: SUCCESS."
}

if (-not $gpuLivingLensText.Contains('RESULT: SUCCESS')) {
    throw "GPU Living Lens Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'Native Glass Overlay Smoke Test'
Write-Host '-------------------------------'
$nativeGlassOutput = & (Join-Path $root 'tools\run-native-glass-overlay.ps1') -SmokeTest 2>&1
$nativeGlassExitCode = $LASTEXITCODE
$nativeGlassText = $nativeGlassOutput -join [Environment]::NewLine
$nativeGlassOutput | ForEach-Object { Write-Host $_ }
if ($nativeGlassExitCode -ne 0) {
    throw "Native Glass Overlay Smoke Test failed with exit code $nativeGlassExitCode."
}

if (-not $nativeGlassText.Contains('RK Workspace Native Glass Overlay Smoke Test')) {
    throw "Native Glass Overlay Smoke Test failed because output did not contain RK Workspace Native Glass Overlay Smoke Test."
}

if (-not $nativeGlassText.Contains('BrowserSurface: NONE')) {
    throw "Native Glass Overlay Smoke Test failed because output did not contain BrowserSurface: NONE."
}

if (-not $nativeGlassText.Contains('SyntheticStage: NONE')) {
    throw "Native Glass Overlay Smoke Test failed because output did not contain SyntheticStage: NONE."
}

if (-not $nativeGlassText.Contains('TransparentDesktop: OK')) {
    throw "Native Glass Overlay Smoke Test failed because output did not contain TransparentDesktop: OK."
}

if (-not $nativeGlassText.Contains('DesktopSampling: OK')) {
    throw "Native Glass Overlay Smoke Test failed because output did not contain DesktopSampling: OK."
}

if (-not $nativeGlassText.Contains('RealDesktopOnly: OK')) {
    throw "Native Glass Overlay Smoke Test failed because output did not contain RealDesktopOnly: OK."
}

if (-not $nativeGlassText.Contains('NativeGlassOverlaySmoke: SUCCESS')) {
    throw "Native Glass Overlay Smoke Test failed because output did not contain NativeGlassOverlaySmoke: SUCCESS."
}

if (-not $nativeGlassText.Contains('RESULT: SUCCESS')) {
    throw "Native Glass Overlay Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'Real3D Lens Smoke Test'
Write-Host '----------------------'
$real3dLensOutput = & (Join-Path $root 'tools\run-real3d-lens.ps1') -SmokeTest 2>&1
$real3dLensExitCode = $LASTEXITCODE
$real3dLensText = $real3dLensOutput -join [Environment]::NewLine
$real3dLensOutput | ForEach-Object { Write-Host $_ }
if ($real3dLensExitCode -ne 0) {
    throw "Real3D Lens Smoke Test failed with exit code $real3dLensExitCode."
}

if (-not $real3dLensText.Contains('RK Workspace Real3D Lens Smoke Test')) {
    throw "Real3D Lens Smoke Test failed because output did not contain RK Workspace Real3D Lens Smoke Test."
}

if (-not $real3dLensText.Contains('WebGLRenderer: OK')) {
    throw "Real3D Lens Smoke Test failed because output did not contain WebGLRenderer: OK."
}

if (-not $real3dLensText.Contains('PhysicalGlass: OK')) {
    throw "Real3D Lens Smoke Test failed because output did not contain PhysicalGlass: OK."
}

if (-not $real3dLensText.Contains('EnvironmentLighting: OK')) {
    throw "Real3D Lens Smoke Test failed because output did not contain EnvironmentLighting: OK."
}

if (-not $real3dLensText.Contains('SoftShadows: OK')) {
    throw "Real3D Lens Smoke Test failed because output did not contain SoftShadows: OK."
}

if (-not $real3dLensText.Contains('Real3DTunnel: OK')) {
    throw "Real3D Lens Smoke Test failed because output did not contain Real3DTunnel: OK."
}

if (-not $real3dLensText.Contains('DesktopLiveTexture: OK')) {
    throw "Real3D Lens Smoke Test failed because output did not contain DesktopLiveTexture: OK."
}

if (-not $real3dLensText.Contains('DeformingDigitalThing: OK')) {
    throw "Real3D Lens Smoke Test failed because output did not contain DeformingDigitalThing: OK."
}

if (-not $real3dLensText.Contains('Real3DLensSmoke: SUCCESS')) {
    throw "Real3D Lens Smoke Test failed because output did not contain Real3DLensSmoke: SUCCESS."
}

if (-not $real3dLensText.Contains('RESULT: SUCCESS')) {
    throw "Real3D Lens Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'Glass Edge Nearest Ablage Smoke Test'
Write-Host '------------------------------------'
$glassEdgeOutput = & (Join-Path $root 'tools\run-glass-edge.ps1') -SmokeTest 2>&1
$glassEdgeExitCode = $LASTEXITCODE
$glassEdgeText = $glassEdgeOutput -join [Environment]::NewLine
$glassEdgeOutput | ForEach-Object { Write-Host $_ }
if ($glassEdgeExitCode -ne 0) {
    throw "Glass Edge Nearest Ablage Smoke Test failed with exit code $glassEdgeExitCode."
}

if (-not $glassEdgeText.Contains('RK Workspace Glass Edge Nearest Ablage Smoke Test')) {
    throw "Glass Edge Smoke Test failed because output did not contain RK Workspace Glass Edge Nearest Ablage Smoke Test."
}

if (-not $glassEdgeText.Contains('SingleGlassEdge: OK')) {
    throw "Glass Edge Smoke Test failed because output did not contain SingleGlassEdge: OK."
}

if (-not $glassEdgeText.Contains('NearestDirection: OK')) {
    throw "Glass Edge Smoke Test failed because output did not contain NearestDirection: OK."
}

if (-not $glassEdgeText.Contains('CrossPlatformSurfaces: OK')) {
    throw "Glass Edge Smoke Test failed because output did not contain CrossPlatformSurfaces: OK."
}

if (-not $glassEdgeText.Contains('ProximityTests: SUCCESS')) {
    throw "Glass Edge Smoke Test failed because output did not contain ProximityTests: SUCCESS."
}

if (-not $glassEdgeText.Contains('GlassEdgeSmoke: SUCCESS')) {
    throw "Glass Edge Smoke Test failed because output did not contain GlassEdgeSmoke: SUCCESS."
}

if (-not $glassEdgeText.Contains('RESULT: SUCCESS')) {
    throw "Glass Edge Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'Glass Edge PDF Frame Demo Smoke Test'
Write-Host '------------------------------------'
$glassEdgePdfOutput = & (Join-Path $root 'tools\run-glass-edge-pdf-frame-demo.ps1') -SmokeTest 2>&1
$glassEdgePdfExitCode = $LASTEXITCODE
$glassEdgePdfText = $glassEdgePdfOutput -join [Environment]::NewLine
$glassEdgePdfOutput | ForEach-Object { Write-Host $_ }
if ($glassEdgePdfExitCode -ne 0) {
    throw "Glass Edge PDF Frame Demo Smoke Test failed with exit code $glassEdgePdfExitCode."
}

if (-not $glassEdgePdfText.Contains('RK Workspace Glass Edge PDF Frame Demo')) {
    throw "Glass Edge PDF Frame Demo Smoke Test failed because output did not contain RK Workspace Glass Edge PDF Frame Demo."
}

if (-not $glassEdgePdfText.Contains('GlassEdge: Active')) {
    throw "Glass Edge PDF Frame Demo Smoke Test failed because output did not contain GlassEdge: Active."
}

if (-not $glassEdgePdfText.Contains('NoFileIngress: SUCCESS')) {
    throw "Glass Edge PDF Frame Demo Smoke Test failed because output did not contain NoFileIngress: SUCCESS."
}

if (-not $glassEdgePdfText.Contains('GlassEdgePdfFrameDemo: SUCCESS')) {
    throw "Glass Edge PDF Frame Demo Smoke Test failed because output did not contain GlassEdgePdfFrameDemo: SUCCESS."
}

if (-not $glassEdgePdfText.Contains('RESULT: SUCCESS')) {
    throw "Glass Edge PDF Frame Demo Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'Mobile Glass Edge Smoke Test'
Write-Host '----------------------------'
$mobileGlassEdgeOutput = & (Join-Path $root 'tools\run-mobile-glass-edge.ps1') -SmokeTest 2>&1
$mobileGlassEdgeExitCode = $LASTEXITCODE
$mobileGlassEdgeText = $mobileGlassEdgeOutput -join [Environment]::NewLine
$mobileGlassEdgeOutput | ForEach-Object { Write-Host $_ }
if ($mobileGlassEdgeExitCode -ne 0) {
    throw "Mobile Glass Edge Smoke Test failed with exit code $mobileGlassEdgeExitCode."
}

if (-not $mobileGlassEdgeText.Contains('RK Workspace Mobile Glass Edge Smoke Test')) {
    throw "Mobile Glass Edge Smoke Test failed because output did not contain RK Workspace Mobile Glass Edge Smoke Test."
}

if (-not $mobileGlassEdgeText.Contains('SingleGlassEdge: OK')) {
    throw "Mobile Glass Edge Smoke Test failed because output did not contain SingleGlassEdge: OK."
}

if (-not $mobileGlassEdgeText.Contains('NearestAblage: OK')) {
    throw "Mobile Glass Edge Smoke Test failed because output did not contain NearestAblage: OK."
}

if (-not $mobileGlassEdgeText.Contains('MobileGlassEdgeSmoke: SUCCESS')) {
    throw "Mobile Glass Edge Smoke Test failed because output did not contain MobileGlassEdgeSmoke: SUCCESS."
}

if (-not $mobileGlassEdgeText.Contains('RESULT: SUCCESS')) {
    throw "Mobile Glass Edge Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'RKWP Dev Transport Smoke Test'
Write-Host '-----------------------------'
$rkwpTransportOutput = & (Join-Path $root 'tools\run-rkwp-transport.ps1') -SmokeTest 2>&1
$rkwpTransportExitCode = $LASTEXITCODE
$rkwpTransportText = $rkwpTransportOutput -join [Environment]::NewLine
$rkwpTransportOutput | ForEach-Object { Write-Host $_ }
if ($rkwpTransportExitCode -ne 0) {
    throw "RKWP Dev Transport Smoke Test failed with exit code $rkwpTransportExitCode."
}

if (-not $rkwpTransportText.Contains('Transport: NamedPipeDev')) {
    throw "RKWP Dev Transport Smoke Test failed because output did not contain Transport: NamedPipeDev."
}

if (-not $rkwpTransportText.Contains('AblageHello: OK')) {
    throw "RKWP Dev Transport Smoke Test failed because output did not contain AblageHello: OK."
}

if (-not $rkwpTransportText.Contains('FrameUpdate: OK')) {
    throw "RKWP Dev Transport Smoke Test failed because output did not contain FrameUpdate: OK."
}

if (-not $rkwpTransportText.Contains('CarryLeaseHeartbeat: OK')) {
    throw "RKWP Dev Transport Smoke Test failed because output did not contain CarryLeaseHeartbeat: OK."
}

if (-not $rkwpTransportText.Contains('RESULT: SUCCESS')) {
    throw "RKWP Dev Transport Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'RKWP Protocol Smoke Test'
Write-Host '------------------------'
$rkwpOutput = & (Join-Path $root 'tools\run-rkwp-tests.ps1') 2>&1
$rkwpExitCode = $LASTEXITCODE
$rkwpText = $rkwpOutput -join [Environment]::NewLine
$rkwpOutput | ForEach-Object { Write-Host $_ }
if ($rkwpExitCode -ne 0) {
    throw "RKWP Protocol Smoke Test failed with exit code $rkwpExitCode."
}

if (-not $rkwpText.Contains('RkwpTests: SUCCESS')) {
    throw "RKWP Protocol Smoke Test failed because output did not contain RkwpTests: SUCCESS."
}

if (-not $rkwpText.Contains('RESULT: SUCCESS')) {
    throw "RKWP Protocol Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'PDF Frame Smoke Test'
Write-Host '--------------------'
$pdfFrameOutput = & (Join-Path $root 'tools\run-pdf-frame-smoke.ps1') 2>&1
$pdfFrameExitCode = $LASTEXITCODE
$pdfFrameText = $pdfFrameOutput -join [Environment]::NewLine
$pdfFrameOutput | ForEach-Object { Write-Host $_ }
if ($pdfFrameExitCode -ne 0) {
    throw "PDF Frame Smoke Test failed with exit code $pdfFrameExitCode."
}

if (-not $pdfFrameText.Contains('PdfFrameSmoke: SUCCESS')) {
    throw "PDF Frame Smoke Test failed because output did not contain PdfFrameSmoke: SUCCESS."
}

if (-not $pdfFrameText.Contains('RESULT: SUCCESS')) {
    throw "PDF Frame Smoke Test failed because output did not contain RESULT: SUCCESS."
}

Write-Host ''
Write-Host 'Windows Local Frame E2E Smoke Test'
Write-Host '----------------------------------'
$windowsLocalFrameOutput = & (Join-Path $root 'tools\run-windows-local-frame-e2e.ps1') -SmokeTest 2>&1
$windowsLocalFrameExitCode = $LASTEXITCODE
$windowsLocalFrameText = $windowsLocalFrameOutput -join [Environment]::NewLine
$windowsLocalFrameOutput | ForEach-Object { Write-Host $_ }
if ($windowsLocalFrameExitCode -ne 0) {
    throw "Windows Local Frame E2E Smoke Test failed with exit code $windowsLocalFrameExitCode."
}

if (-not $windowsLocalFrameText.Contains('DevPairing: SUCCESS')) {
    throw "Windows Local Frame E2E Smoke Test failed because output did not contain DevPairing: SUCCESS."
}

if (-not $windowsLocalFrameText.Contains('TransportConnected: SUCCESS')) {
    throw "Windows Local Frame E2E Smoke Test failed because output did not contain TransportConnected: SUCCESS."
}

if (-not $windowsLocalFrameText.Contains('NoFileIngress: SUCCESS')) {
    throw "Windows Local Frame E2E Smoke Test failed because output did not contain NoFileIngress: SUCCESS."
}

if (-not $windowsLocalFrameText.Contains('Return: SUCCESS')) {
    throw "Windows Local Frame E2E Smoke Test failed because output did not contain Return: SUCCESS."
}

if (-not $windowsLocalFrameText.Contains('Recovery: SUCCESS')) {
    throw "Windows Local Frame E2E Smoke Test failed because output did not contain Recovery: SUCCESS."
}

if (-not $windowsLocalFrameText.Contains('RESULT: SUCCESS')) {
    throw "Windows Local Frame E2E Smoke Test failed because output did not contain RESULT: SUCCESS."
}
