param(
    [switch] $SmokeTest,
    [switch] $AllowDirty
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Split-Path -Parent $PSScriptRoot)).Path

$status = git -C $root status --short
if ($LASTEXITCODE -ne 0) {
    throw 'Unable to read git status.'
}

$buildArtifacts = Get-ChildItem -Path $root -Directory -Recurse -Include bin,obj -ErrorAction SilentlyContinue
$untrackedDanger = @()
foreach ($line in $status) {
    if ($line -match '^\?\?\s+(.+)$') {
        $path = $Matches[1]
        if ($path -match '(?i)(\.pfx$|\.p12$|\.pem$|\.key$|\.crt$|\.cer$|\.secret\.json$|\.local\.json$|identity.*\.json$)') {
            $untrackedDanger += $path
        }
    }
}

Write-Output 'RK Workspace MA016 Final Status Guard'
Write-Output '-------------------------------------'
Write-Output "DirtyEntries: $($status.Count)"
Write-Output "BuildArtifactDirectories: $($buildArtifacts.Count)"
Write-Output "PotentialSecretUntracked: $($untrackedDanger.Count)"

if ($untrackedDanger.Count -gt 0) {
    $untrackedDanger | ForEach-Object { Write-Output "PotentialSecret: $_" }
    throw 'Potential local secret or certificate is untracked.'
}

if (-not ($SmokeTest -or $AllowDirty)) {
    if ($status.Count -gt 0) {
        $status | ForEach-Object { Write-Output $_ }
        throw 'Git status is not clean.'
    }

    if ($buildArtifacts.Count -gt 0) {
        $buildArtifacts | Select-Object -ExpandProperty FullName | ForEach-Object { Write-Output "BuildArtifact: $_" }
        throw 'Build artifacts are present.'
    }
}

Write-Output 'MA016FinalStatusGuard: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
