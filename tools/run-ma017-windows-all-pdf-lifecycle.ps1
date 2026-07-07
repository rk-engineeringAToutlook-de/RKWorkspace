param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Invoke-PilotStep {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Label,

        [Parameter(Mandatory = $true)]
        [scriptblock] $Command
    )

    Write-Output "== $Label =="
    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$Label failed with exit code $LASTEXITCODE."
    }
}

Write-Output 'RK Workspace MA017 Pilot: Windows All PDF Lifecycle'
Write-Output '---------------------------------------------------'

Invoke-PilotStep 'Windows Local Closed PDF' {
    & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -ClosedPdf -Policy TrustedPersonalDevices
}

Invoke-PilotStep 'Windows Local Open PDF' {
    & (Join-Path $root 'tools\run-windows-pdf-lifecycle-pilot.ps1') -SmokeTest -OpenPdf -Policy TrustedPersonalDevices
}

Invoke-PilotStep 'Windows to macOS Closed PDF' {
    & (Join-Path $root 'tools\run-ma017-windows-to-mac-closed-pdf.ps1') -SmokeTest
}

Invoke-PilotStep 'Windows to macOS Open PDF' {
    & (Join-Path $root 'tools\run-ma017-windows-to-mac-open-pdf.ps1') -SmokeTest
}

Invoke-PilotStep 'Windows to iPad Closed PDF' {
    & (Join-Path $root 'tools\run-ma017-windows-to-ipad-closed-pdf.ps1') -SmokeTest
}

Invoke-PilotStep 'Windows to iPad Open PDF' {
    & (Join-Path $root 'tools\run-ma017-windows-to-ipad-open-pdf.ps1') -SmokeTest
}

Write-Output 'MA017WindowsAllPdfLifecyclePilot: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
