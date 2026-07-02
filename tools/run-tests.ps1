$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot

dotnet build (Join-Path $root 'src\Core\RKWorkspace.Core.csproj')
dotnet run --project (Join-Path $root 'tests\Unit\RKWorkspace.Core.Tests\RKWorkspace.Core.Tests.csproj')
dotnet run --project (Join-Path $root 'tools\LocalSimulation\RKWorkspace.LocalSimulation.csproj')
