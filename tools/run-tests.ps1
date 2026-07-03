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
