param(
    [switch] $DryRun,
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Split-Path -Parent $PSScriptRoot)).Path
$targets = Get-ChildItem -Path $root -Directory -Recurse -Include bin,obj -ErrorAction SilentlyContinue

foreach ($target in $targets) {
    $resolved = (Resolve-Path -LiteralPath $target.FullName).Path
    if (-not $resolved.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove outside repository: $resolved"
    }
}

Write-Output 'RK Workspace Build Artifact Cleanup'
Write-Output '-----------------------------------'
Write-Output "DryRun: $($DryRun -or $SmokeTest)"
Write-Output "Targets: $($targets.Count)"

if (-not ($DryRun -or $SmokeTest)) {
    $targets | ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force }
}

Write-Output 'BuildArtifactCleanup: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
