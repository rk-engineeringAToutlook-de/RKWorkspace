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
