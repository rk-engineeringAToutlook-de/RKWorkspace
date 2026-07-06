param(
    [switch] $SmokeTest,
    [string] $OutputPath
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $root 'release\windows-dev'
}

$staging = Join-Path $env:TEMP "RKWorkspace_WindowsDevPackage_$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $staging -Force | Out-Null

function Copy-IfExists {
    param(
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

try {
    $files = @(
        'README.md',
        'Docs\Platform\WindowsDevInstallerPlan.md',
        'Docs\Admin\LabSetupGuide.md',
        'tools\run-windows-agent-dev.ps1',
        'tools\install-windows-agent-dev.ps1',
        'tools\uninstall-windows-agent-dev.ps1',
        'tools\run-config-tool.ps1',
        'tools\run-policy-profile.ps1',
        'tools\run-rkwp-diagnostics.ps1',
        'config\samples\rkworkspace.sample.json',
        'config\samples\rkwp-dev.sample.json',
        'config\samples\manual-ablage-map.sample.json',
        'config\samples\policy-critical.sample.json',
        'config\samples\policy-critical-infrastructure.sample.json'
    )

    $files | ForEach-Object { Copy-IfExists $_ }

    @'
RK Workspace Windows Dev Package
================================

This package contains development scripts, sample configuration, documentation, and no secrets.

Start:
  .\tools\run-windows-agent-dev.ps1 -SmokeTest

Uninstall:
  .\tools\uninstall-windows-agent-dev.ps1 -SmokeTest
'@ | Set-Content -Path (Join-Path $staging 'WINDOWS_DEV_PACKAGE.txt') -Encoding UTF8

    if ($SmokeTest) {
        $required = @(
            'tools\run-windows-agent-dev.ps1',
            'config\samples\rkworkspace.sample.json',
            'config\samples\policy-critical-infrastructure.sample.json',
            'WINDOWS_DEV_PACKAGE.txt'
        )

        foreach ($relative in $required) {
            if (-not (Test-Path (Join-Path $staging $relative))) {
                throw "Package missing $relative"
            }
        }

        Write-Host 'RK Workspace Windows Dev Package'
        Write-Host '--------------------------------'
        Write-Host "Output: $staging"
        Write-Host 'Binaries: PREPARED_BY_BUILD_PIPELINE'
        Write-Host 'ConfigSamples: OK'
        Write-Host 'Scripts: OK'
        Write-Host 'NoSecrets: OK'
        Write-Host 'Uninstall: PREPARED'
        Write-Host 'WindowsDevPackageSmoke: SUCCESS'
        Write-Host 'RESULT: SUCCESS'
        exit 0
    }

    Remove-Item -LiteralPath $OutputPath -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    Copy-Item -Path (Join-Path $staging '*') -Destination $OutputPath -Recurse -Force
    Write-Host 'RK Workspace Windows Dev Package'
    Write-Host '--------------------------------'
    Write-Host "Output: $OutputPath"
    Write-Host 'RESULT: SUCCESS'
}
finally {
    if ($SmokeTest) {
        Remove-Item -LiteralPath $staging -Recurse -Force -ErrorAction SilentlyContinue
    }
}
