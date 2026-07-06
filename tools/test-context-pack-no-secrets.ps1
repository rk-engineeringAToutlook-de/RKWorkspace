param(
    [switch] $SmokeTest,
    [string] $ContextZip
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Split-Path -Parent $PSScriptRoot)).Path

if ([string]::IsNullOrWhiteSpace($ContextZip)) {
    $ContextZip = Join-Path $root 'release\codex-context\RKWorkspace_Context_latest.zip'
}

if (-not (Test-Path $ContextZip)) {
    & (Join-Path $root 'tools\export-codex-context.ps1')
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

if (-not (Test-Path $ContextZip)) {
    throw "Context pack not found: $ContextZip"
}

$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$staging = Join-Path $env:TEMP "RKWorkspace_ContextSecretScan_$timestamp"
Remove-Item -LiteralPath $staging -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $staging -Force | Out-Null
Expand-Archive -Path $ContextZip -DestinationPath $staging -Force

$patterns = @(
    @{ Name = 'private-key-block'; Regex = '-----BEGIN ([A-Z ]+)?PRIVATE KEY-----' },
    @{ Name = 'github-token'; Regex = 'gh[pousr]_[A-Za-z0-9_]{20,}' },
    @{ Name = 'aws-access-key'; Regex = 'AKIA[0-9A-Z]{16}' },
    @{ Name = 'long-secret-assignment'; Regex = '(?i)(password|token|secret|client_secret|private_key)\s*[:=]\s*["'']?(?!REDACTED|<|CHANGE_ME|sample|example|dev-only|not-set|null|false|true)[A-Za-z0-9_\-+/=.]{16,}' }
)

$findings = @()
$files = Get-ChildItem -Path $staging -File -Recurse -ErrorAction SilentlyContinue
foreach ($file in $files) {
    try {
        $text = Get-Content -LiteralPath $file.FullName -Raw -ErrorAction Stop
    }
    catch {
        continue
    }

    foreach ($pattern in $patterns) {
        $matches = [regex]::Matches($text, $pattern.Regex)
        foreach ($match in $matches) {
            $line = ($text.Substring(0, $match.Index) -split "`r?`n").Count
            $lineText = (($text -split "`r?`n")[$line - 1])
            if ($lineText -match '(?i)(placeholder|sample|example|redacted|dev-only|not-set|dummy)') {
                continue
            }

            $relative = $file.FullName.Substring($staging.Length).TrimStart('\', '/')
            $findings += "${relative}:$line $($pattern.Name)"
        }
    }
}

Remove-Item -LiteralPath $staging -Recurse -Force -ErrorAction SilentlyContinue

Write-Output 'RK Workspace Context Pack Secret Scan'
Write-Output '-------------------------------------'
Write-Output "ContextZip: $ContextZip"
Write-Output "FilesScanned: $($files.Count)"
Write-Output "Findings: $($findings.Count)"

if ($findings.Count -gt 0) {
    $findings | ForEach-Object { Write-Output "Finding: $_" }
    throw 'Context pack secret scan failed.'
}

Write-Output 'ContextPackNoSecrets: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
