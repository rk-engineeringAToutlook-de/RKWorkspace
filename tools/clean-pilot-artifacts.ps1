param(
    [switch] $DryRun,
    [switch] $SmokeTest,
    [switch] $IncludePackages
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Split-Path -Parent $PSScriptRoot)).Path
$targets = @()

$directoriesToClean = @(
    'release\ma016\logs',
    'release\ma016\context'
)

foreach ($relative in $directoriesToClean) {
    $directory = Join-Path $root $relative
    if (Test-Path $directory) {
        $targets += Get-ChildItem -Path $directory -Force -ErrorAction SilentlyContinue | Where-Object { $_.Name -ne '.gitkeep' }
    }
}

$directoriesToRemove = @(
    'release\ma016\temp',
    'release\ma016\frame-cache'
)

foreach ($relative in $directoriesToRemove) {
    $directory = Join-Path $root $relative
    if (Test-Path $directory) {
        $targets += Get-Item -LiteralPath $directory
    }
}

if ($IncludePackages) {
    $packageDir = Join-Path $root 'release\ma016\packages'
    if (Test-Path $packageDir) {
        $targets += Get-ChildItem -Path $packageDir -File -Filter '*.zip' -ErrorAction SilentlyContinue
    }
}

foreach ($target in $targets) {
    $resolved = (Resolve-Path -LiteralPath $target.FullName).Path
    if (-not $resolved.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to clean outside repository: $resolved"
    }
}

Write-Output 'RK Workspace MA016 Pilot Artifact Cleanup'
Write-Output '-----------------------------------------'
Write-Output "DryRun: $($DryRun -or $SmokeTest)"
Write-Output "Targets: $($targets.Count)"
Write-Output "IncludePackages: $IncludePackages"

if (-not ($DryRun -or $SmokeTest)) {
    $targets | ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force }
}

Write-Output 'PilotArtifactCleanup: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
