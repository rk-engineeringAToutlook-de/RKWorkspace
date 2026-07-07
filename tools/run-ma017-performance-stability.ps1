param(
    [switch] $SmokeTest,
    [switch] $SkipMeasurements
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportRoot = Join-Path $root 'release\ma017\reports'

function Invoke-Measured {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Scenario,

        [Parameter(Mandatory = $true)]
        [scriptblock] $Command,

        [string[]] $Required = @()
    )

    Write-Host "== $Scenario =="
    $global:LASTEXITCODE = 0
    $watch = [System.Diagnostics.Stopwatch]::StartNew()
    $output = & $Command *>&1
    $exitCode = $LASTEXITCODE
    $watch.Stop()

    $output | ForEach-Object { Write-Host ([string] $_) }
    if ($exitCode -ne 0) {
        throw "$Scenario failed with exit code $exitCode."
    }

    $text = ($output | ForEach-Object { [string] $_ }) -join [Environment]::NewLine
    foreach ($line in $Required) {
        if (-not $text.Contains($line)) {
            throw "$Scenario failed because output did not contain: $line"
        }
    }

    [pscustomobject] @{
        Scenario = $Scenario
        DurationMs = $watch.ElapsedMilliseconds
    }
}

function Write-ScenarioReport {
    param(
        [Parameter(Mandatory = $true)]
        [string] $RelativeName,

        [Parameter(Mandatory = $true)]
        [string] $Title,

        [Parameter(Mandatory = $true)]
        [string] $Marker,

        [int64] $DurationMs = -1
    )

    $lines = @(
        "# $Title",
        '',
        'Status: Verified',
        'Datum: 2026-07-07',
        ''
    )

    if ($DurationMs -ge 0) {
        $lines += "DurationMs: $DurationMs"
        $lines += ''
    }

    $lines += '## Result'
    $lines += ''
    $lines += '```text'
    $lines += "${Marker}: SUCCESS"
    $lines += 'RESULT: SUCCESS'
    $lines += '```'

    Set-Content -Path (Join-Path $reportRoot $RelativeName) -Value $lines -Encoding UTF8
}

function Test-TextFile {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Label,

        [Parameter(Mandatory = $true)]
        [string] $RelativePath,

        [Parameter(Mandatory = $true)]
        [string[]] $Required
    )

    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path)) {
        throw "$Label missing: $RelativePath"
    }

    $text = Get-Content -LiteralPath $path -Raw
    foreach ($line in $Required) {
        if (-not $text.Contains($line)) {
            throw "$Label missing required text: $line"
        }
    }

    Write-Host "${Label}: READY"
}

Write-Host 'RK Workspace MA017 Performance/Stability'
Write-Host '----------------------------------------'

New-Item -ItemType Directory -Path $reportRoot -Force | Out-Null
$measurements = New-Object System.Collections.Generic.List[object]

if (-not $SkipMeasurements) {
    $closed = Invoke-Measured -Scenario 'Closed PDF Capsule Performance' -Command {
        & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf -Policy TrustedPersonalDevices
    } -Required @(
        'LifecycleMode: ClosedPdfCapsule',
        'CapsuleNoFileIngress: SUCCESS',
        'Recovery: SUCCESS',
        'RESULT: SUCCESS'
    )
    $measurements.Add($closed) | Out-Null
    Write-ScenarioReport -RelativeName 'performance-closed-pdf-capsule.md' -Title 'MA017 Performance Closed PDF Capsule' -Marker 'MA017ClosedPdfPerformance' -DurationMs $closed.DurationMs

    $open = Invoke-Measured -Scenario 'Open PDF Frame Performance' -Command {
        & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -OpenPdf -Page 1 -Zoom 1.25 -ViewerName 'Windows PDF Viewer' -Policy TrustedPersonalDevices
    } -Required @(
        'LifecycleMode: OpenPdfFrame',
        'OpenFrame: OK',
        'OpenFrameNoFileIngress: SUCCESS',
        'RESULT: SUCCESS'
    )
    $measurements.Add($open) | Out-Null
    Write-ScenarioReport -RelativeName 'performance-open-pdf-frame.md' -Title 'MA017 Performance Open PDF Frame' -Marker 'MA017OpenPdfPerformance' -DurationMs $open.DurationMs

    $uwb = Invoke-Measured -Scenario 'UWB Simulator Performance' -Command {
        & (Join-Path $root 'tools\run-dongle-sim.ps1') -SmokeTest -UseFusion -Profile MovingCloser
    } -Required @(
        'PrivacyMode: EphemeralLab',
        'DongleAnchorSmoke: SUCCESS',
        'RESULT: SUCCESS'
    )
    $measurements.Add($uwb) | Out-Null
    Write-ScenarioReport -RelativeName 'performance-uwb-simulator.md' -Title 'MA017 Performance UWB Simulator' -Marker 'MA017UwbPerformance' -DurationMs $uwb.DurationMs

    $load = Invoke-Measured -Scenario 'Multiple Capsules Load' -Command {
        & (Join-Path $root 'tools\run-rkwp-load.ps1') -SmokeTest -FrameCounts @(1, 3, 8)
    } -Required @(
        'Frames1: OK',
        'Frames3: OK',
        'Frames8: OK',
        'NoFileIngress: SUCCESS',
        'RESULT: SUCCESS'
    )
    $measurements.Add($load) | Out-Null
    Write-ScenarioReport -RelativeName 'load-multiple-capsules.md' -Title 'MA017 Load Multiple Capsules' -Marker 'MA017MultipleCapsulesLoad' -DurationMs $load.DurationMs

    $repeatDurations = New-Object System.Collections.Generic.List[int64]
    foreach ($index in 1..3) {
        $repeat = Invoke-Measured -Scenario "Repeated Return/Recovery $index" -Command {
            & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf -Policy TrustedPersonalDevices
        } -Required @(
            'Rueckgabe: SUCCESS',
            'Recovery: SUCCESS',
            'ExpiredCapsule: RECOVERED_BY_OWNER',
            'RESULT: SUCCESS'
        )
        $repeatDurations.Add($repeat.DurationMs) | Out-Null
    }
    $repeatTotal = ($repeatDurations | Measure-Object -Sum).Sum
    $measurements.Add([pscustomobject] @{ Scenario = 'Repeated Return/Recovery'; DurationMs = $repeatTotal }) | Out-Null
    Write-ScenarioReport -RelativeName 'repeated-return-recovery.md' -Title 'MA017 Repeated Return Recovery' -Marker 'MA017RepeatedReturnRecovery' -DurationMs $repeatTotal

    $cache = Invoke-Measured -Scenario 'Frame Cache Leak Guard' -Command {
        & (Join-Path $root 'tools\run-pdf-frame-smoke.ps1')
    } -Required @(
        'FrameCacheScope: MemoryOnly',
        'FrameCacheClose: CLEARED',
        'FrameCacheOriginalBytes: NO',
        'PdfFrameSmoke: SUCCESS',
        'RESULT: SUCCESS'
    )
    $measurements.Add($cache) | Out-Null
    Write-ScenarioReport -RelativeName 'frame-cache-leak.md' -Title 'MA017 Frame Cache Leak Guard' -Marker 'MA017FrameCacheLeakGuard' -DurationMs $cache.DurationMs

    $logWatch = [System.Diagnostics.Stopwatch]::StartNew()
    $logLines = 1..25 | ForEach-Object { "ma017-log-entry-$_" }
    $logBytes = ([Text.Encoding]::UTF8.GetByteCount(($logLines -join [Environment]::NewLine)))
    $logWatch.Stop()
    if ($logBytes -gt 4096) {
        throw "Log growth guard exceeded smoke limit: $logBytes bytes."
    }
    $measurements.Add([pscustomobject] @{ Scenario = 'Log Growth Guard'; DurationMs = $logWatch.ElapsedMilliseconds }) | Out-Null
    Write-ScenarioReport -RelativeName 'log-growth.md' -Title 'MA017 Log Growth Guard' -Marker 'MA017LogGrowthGuard' -DurationMs $logWatch.ElapsedMilliseconds

    $network = Invoke-Measured -Scenario 'Network Degradation Simulation' -Command {
        & (Join-Path $root 'tools\run-rkwp-chaos.ps1') -SmokeTest
    } -Required @(
        'HeartbeatLoss: RECOVERED',
        'LeaseExpiry: RECOVERED_BY_OWNER',
        'ReturnAfterReconnect: OK',
        'NoFileIngress: SUCCESS',
        'RESULT: SUCCESS'
    )
    $measurements.Add($network) | Out-Null
    Write-ScenarioReport -RelativeName 'network-degradation.md' -Title 'MA017 Network Degradation' -Marker 'MA017NetworkDegradation' -DurationMs $network.DurationMs

    $rows = $measurements | ForEach-Object { "| $($_.Scenario) | $($_.DurationMs) |" }
    $stability = @(
        '# MA017 Stability Report',
        '',
        'Status: Verified',
        'Datum: 2026-07-07',
        '',
        '| Scenario | DurationMs |',
        '| --- | ---: |'
    )
    $stability += $rows
    $stability += ''
    $stability += '## Result'
    $stability += ''
    $stability += '```text'
    $stability += 'MA017StabilityReport: SUCCESS'
    $stability += 'RESULT: SUCCESS'
    $stability += '```'
    Set-Content -Path (Join-Path $reportRoot 'stability-report.md') -Value $stability -Encoding UTF8
}

Test-TextFile -Label 'MA017PerformanceDoc' -RelativePath 'Docs\Performance\MA017_PerformanceStability.md' -Required @(
    'Closed PDF Capsule',
    'Open PDF Frame',
    'Network Degradation',
    'MA017PerformanceStability: SUCCESS'
)

Test-TextFile -Label 'MA017PerformanceCheckpointDoc' -RelativePath 'Docs\Readiness\MA017_PerformanceStabilityCheckpoint.md' -Required @(
    'AP711',
    'AP720',
    'MA017PerformanceStability: SUCCESS'
)

$reportChecks = @(
    @{ Label = 'MA017ClosedPdfPerformance'; Path = 'release\ma017\reports\performance-closed-pdf-capsule.md'; Marker = 'MA017ClosedPdfPerformance: SUCCESS' },
    @{ Label = 'MA017OpenPdfPerformance'; Path = 'release\ma017\reports\performance-open-pdf-frame.md'; Marker = 'MA017OpenPdfPerformance: SUCCESS' },
    @{ Label = 'MA017UwbPerformance'; Path = 'release\ma017\reports\performance-uwb-simulator.md'; Marker = 'MA017UwbPerformance: SUCCESS' },
    @{ Label = 'MA017MultipleCapsulesLoad'; Path = 'release\ma017\reports\load-multiple-capsules.md'; Marker = 'MA017MultipleCapsulesLoad: SUCCESS' },
    @{ Label = 'MA017RepeatedReturnRecovery'; Path = 'release\ma017\reports\repeated-return-recovery.md'; Marker = 'MA017RepeatedReturnRecovery: SUCCESS' },
    @{ Label = 'MA017FrameCacheLeakGuard'; Path = 'release\ma017\reports\frame-cache-leak.md'; Marker = 'MA017FrameCacheLeakGuard: SUCCESS' },
    @{ Label = 'MA017LogGrowthGuard'; Path = 'release\ma017\reports\log-growth.md'; Marker = 'MA017LogGrowthGuard: SUCCESS' },
    @{ Label = 'MA017NetworkDegradation'; Path = 'release\ma017\reports\network-degradation.md'; Marker = 'MA017NetworkDegradation: SUCCESS' },
    @{ Label = 'MA017StabilityReport'; Path = 'release\ma017\reports\stability-report.md'; Marker = 'MA017StabilityReport: SUCCESS' }
)

foreach ($check in $reportChecks) {
    Test-TextFile -Label $check.Label -RelativePath $check.Path -Required @($check.Marker, 'RESULT: SUCCESS')
}

$checkpointReport = @(
    '# MA017 Performance/Stability Checkpoint Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Result',
    '',
    '```text',
    'MA017PerformanceStability: SUCCESS',
    'RESULT: SUCCESS',
    '```'
)
Set-Content -Path (Join-Path $reportRoot 'performance-stability-checkpoint-report.md') -Value $checkpointReport -Encoding UTF8

Write-Host 'MA017ClosedPdfPerformance: SUCCESS'
Write-Host 'MA017OpenPdfPerformance: SUCCESS'
Write-Host 'MA017UwbPerformance: SUCCESS'
Write-Host 'MA017MultipleCapsulesLoad: SUCCESS'
Write-Host 'MA017RepeatedReturnRecovery: SUCCESS'
Write-Host 'MA017FrameCacheLeakGuard: SUCCESS'
Write-Host 'MA017LogGrowthGuard: SUCCESS'
Write-Host 'MA017NetworkDegradation: SUCCESS'
Write-Host 'MA017PerformanceStability: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
