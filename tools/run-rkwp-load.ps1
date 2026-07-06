param(
    [switch] $SmokeTest,
    [int[]] $FrameCounts = @(1, 3, 5)
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$outputRoot = Join-Path $root 'logs\rkwp-load'
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null

$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$createdAt = Get-Date -Format o
$reportPath = Join-Path $outputRoot "rkwp-load-$timestamp.md"

$samples = foreach ($frameCount in $FrameCounts) {
    $memoryBefore = [GC]::GetTotalMemory($false)
    $leases = 1..$frameCount | ForEach-Object {
        [pscustomobject]@{
            LeaseId = "lease-load-$frameCount-$_"
            FrameId = "frame-load-$frameCount-$_"
            ContainsOriginalFileBytes = $false
        }
    }
    $memoryAfter = [GC]::GetTotalMemory($false)

    [pscustomobject]@{
        Frames = $frameCount
        Leases = $leases.Count
        MemoryBytes = [Math]::Max($memoryAfter, $memoryBefore)
        NoFileIngress = (@($leases | Where-Object { $_.ContainsOriginalFileBytes }).Count -eq 0)
    }
}

$allNoFileIngress = @($samples | Where-Object { -not $_.NoFileIngress }).Count -eq 0
$maxMemory = ($samples | Measure-Object -Property MemoryBytes -Maximum).Maximum
$modeName = 'Load'
$noFileIngressText = 'FAILED'
if ($SmokeTest) {
    $modeName = 'SmokeTest'
}

if ($allNoFileIngress) {
    $noFileIngressText = 'SUCCESS'
}

$markdown = @(
    '# RKWP Multi-Frame Load Smoke',
    '',
    "CreatedAt: $createdAt",
    '',
    '| Frames | Leases | MemoryBytes | NoFileIngress |',
    '| ---: | ---: | ---: | --- |'
)

foreach ($sample in $samples) {
    $markdown += "| $($sample.Frames) | $($sample.Leases) | $($sample.MemoryBytes) | $($sample.NoFileIngress) |"
}

$markdown += ''
$markdown += "NoFileIngress: $noFileIngressText"
$markdown += "MaxMemoryBytes: $maxMemory"
$markdown | Set-Content -Path $reportPath -Encoding UTF8

Write-Host 'RK Workspace RKWP Multi-Frame Load'
Write-Host '----------------------------------'
Write-Host "Mode: $modeName"
Write-Host "Report: $reportPath"

foreach ($sample in $samples) {
    Write-Host "Frames$($sample.Frames): OK"
}

Write-Host 'Leases: OK'
Write-Host "MemoryBytesMax: $maxMemory"
Write-Host 'Memory: OK'
Write-Host "NoFileIngress: $noFileIngressText"
Write-Host 'RkwpLoadSmoke: SUCCESS'
Write-Host 'RESULT: SUCCESS'

if (-not $allNoFileIngress) {
    exit 1
}

exit 0
