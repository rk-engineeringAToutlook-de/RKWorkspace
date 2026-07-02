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
