param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Output 'RK Workspace Pilot: Windows All PDF Lifecycle'
Write-Output '---------------------------------------------'

$commands = @(
    @{ Label = 'ClosedPdfCapsule'; Command = { & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf } },
    @{ Label = 'OpenPdfFrame'; Command = { & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -OpenPdf } },
    @{ Label = 'WindowsToMacClosed'; Command = { & (Join-Path $root 'tools\run-pilot-windows-to-mac-closed-pdf.ps1') -SmokeTest } },
    @{ Label = 'WindowsToMacOpen'; Command = { & (Join-Path $root 'tools\run-pilot-windows-to-mac-open-pdf.ps1') -SmokeTest } },
    @{ Label = 'WindowsToiPadClosed'; Command = { & (Join-Path $root 'tools\run-pilot-windows-to-ipad-closed-pdf.ps1') -SmokeTest } },
    @{ Label = 'WindowsToiPadOpen'; Command = { & (Join-Path $root 'tools\run-pilot-windows-to-ipad-open-pdf.ps1') -SmokeTest } }
)

foreach ($command in $commands) {
    Write-Output "== $($command.Label) =="
    & $command.Command
    if ($LASTEXITCODE -ne 0) {
        throw "$($command.Label) failed with exit code $LASTEXITCODE."
    }
}

Write-Output 'WindowsAllPdfLifecyclePilot: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
