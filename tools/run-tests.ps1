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

    if (-not $localIpcProcess.WaitForExit(30000)) {
        try {
            $localIpcProcess.Kill()
        }
        catch {
        }

        throw 'Local IPC Two Process Test timed out after 30 seconds.'
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

if (-not $livingLensText.Contains('LivingLensSmoke: SUCCESS')) {
    throw "Living Lens Smoke Test failed because output did not contain LivingLensSmoke: SUCCESS."
}

if (-not $livingLensText.Contains('RESULT: SUCCESS')) {
    throw "Living Lens Smoke Test failed because output did not contain RESULT: SUCCESS."
}
