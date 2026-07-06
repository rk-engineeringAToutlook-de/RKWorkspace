param(
    [switch] $DryRun,
    [switch] $SmokeTest,
    [switch] $IncludeFeedback
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Split-Path -Parent $PSScriptRoot)).Path
$logDir = Join-Path $root 'release\ma016\logs'
$patterns = @('*.tmp', '*smoke*.json', '*smoke*.jsonl', 'active-pilot.json')
if ($IncludeFeedback) {
    $patterns += 'owner-feedback.jsonl'
}

$targets = @()
foreach ($pattern in $patterns) {
    if (Test-Path $logDir) {
        $targets += Get-ChildItem -Path $logDir -File -Filter $pattern -ErrorAction SilentlyContinue
    }
}

foreach ($target in $targets) {
    $resolved = (Resolve-Path -LiteralPath $target.FullName).Path
    if (-not $resolved.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to clean outside repository: $resolved"
    }
}

Write-Output 'RK Workspace MA016 Pilot Cleanup'
Write-Output '--------------------------------'
Write-Output "DryRun: $($DryRun -or $SmokeTest)"
Write-Output "Targets: $($targets.Count)"

if (-not ($DryRun -or $SmokeTest)) {
    $targets | ForEach-Object { Remove-Item -LiteralPath $_.FullName -Force }
}

Write-Output 'PilotCleanup: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
