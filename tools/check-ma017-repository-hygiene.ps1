param(
    [switch] $AllowDirty
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Host 'RK Workspace MA017 Repository Hygiene'
Write-Host '-------------------------------------'

$buildArtifacts = @(Get-ChildItem -Path $root -Directory -Recurse -Force -Include bin,obj)
$trackedBuildArtifacts = @(git -C $root ls-files | Where-Object { $_ -match '(^|/)(bin|obj)/' })
$status = @(git -C $root status --short)
$secretFindings = @(git -C $root ls-files | Where-Object { $_ -match '(^|[/\\])(\.env|secrets?|tokens?|passwords?)([/\\]|$)|\.(pfx|pem)$' })

if ($trackedBuildArtifacts.Count -gt 0) {
    throw "Tracked build artifacts found: $($trackedBuildArtifacts -join ', ')"
}

if ($secretFindings.Count -gt 0) {
    throw "Potential secret file tracked: $($secretFindings -join ', ')"
}

if (-not $AllowDirty -and $status.Count -gt 0) {
    throw 'Working tree is dirty. Use -AllowDirty only during pre-commit packaging validation.'
}

Write-Host "BuildArtifactDirectories: $($buildArtifacts.Count)"
Write-Host "TrackedBuildArtifacts: $($trackedBuildArtifacts.Count)"
Write-Host "DirtyEntries: $($status.Count)"
Write-Host "PotentialSecretTracked: $($secretFindings.Count)"
Write-Host 'MA017RepositoryHygiene: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
