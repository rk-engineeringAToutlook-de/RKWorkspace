param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Test-Document {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Label,

        [Parameter(Mandatory = $true)]
        [string] $RelativePath,

        [Parameter(Mandatory = $true)]
        [string[]] $RequiredText
    )

    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path)) {
        throw "$Label missing: $RelativePath"
    }

    $content = Get-Content -LiteralPath $path -Raw
    foreach ($text in $RequiredText) {
        if ($content -notlike "*$text*") {
            throw "$Label missing required text '$text' in $RelativePath"
        }
    }

    Write-Host "${Label}: READY"
}

Write-Host 'RK Workspace MA017 Human Experience Check'
Write-Host '-----------------------------------------'

$checks = @(
    @{
        Label = 'MA017ClosedPdfHX'
        Path = 'Docs\HumanExperience\MA017_ClosedPdfExperienceTest.md'
        Required = @('geschlossene PDF', 'HX-001', 'No File Ingress', 'RESULT: SUCCESS')
    },
    @{
        Label = 'MA017OpenPdfHX'
        Path = 'Docs\HumanExperience\MA017_OpenPdfExperienceTest.md'
        Required = @('geoeffnete PDF', 'Original-Owned Frame', 'GuestIngress: None', 'RESULT: SUCCESS')
    },
    @{
        Label = 'MA017FrameCapsuleHX'
        Path = 'Docs\HumanExperience\MA017_FrameCapsuleExperienceTest.md'
        Required = @('Kapsel', 'Frame', 'FrameCapsuleDistinction', 'RESULT: SUCCESS')
    },
    @{
        Label = 'MA017macOSGuestHX'
        Path = 'Docs\HumanExperience\MA017_macOSGuestExperienceTest.md'
        Required = @('macOS Guest', 'Ablage', 'GuestRole: FrameOnly', 'RESULT: SUCCESS')
    },
    @{
        Label = 'MA017iPadHapticHX'
        Path = 'Docs\HumanExperience\MA017_iPadHapticExperienceTest.md'
        Required = @('iPad-Haptik', 'HX-001A', 'iPadHaptics: Mapped', 'RESULT: SUCCESS')
    },
    @{
        Label = 'MA017ProximityUwbHX'
        Path = 'Docs\HumanExperience\MA017_ProximityUwbExperienceTest.md'
        Required = @('Proximity', 'UWB', 'NearestAblage', 'RESULT: SUCCESS')
    },
    @{
        Label = 'MA017GlassEdgeFeelingCriteria'
        Path = 'Docs\HumanExperience\MA017_GlassEdgeFeelingCriteria.md'
        Required = @('Eine Kante', 'naechste Ablage', 'GlassEdgeMode: NearestOnly', 'RESULT: SUCCESS')
    },
    @{
        Label = 'MA017OriginalOwnedFrameTrustCriteria'
        Path = 'Docs\HumanExperience\MA017_OriginalOwnedFrameTrustCriteria.md'
        Required = @('Original-Owned Frames', 'GuestIngress: None', 'OriginalOwnedFrameTrust: Ready', 'RESULT: SUCCESS')
    },
    @{
        Label = 'MA017OwnerInterviewGuide'
        Path = 'Docs\HumanExperience\MA017_OwnerInterviewGuide.md'
        Required = @('Owner-Interview', 'Startfrage', 'Abschlussfrage', 'Naechste Hypothese')
    },
    @{
        Label = 'MA017HumanExperienceCheckpoint'
        Path = 'Docs\Readiness\MA017_HumanExperienceCheckpoint.md'
        Required = @('AP681', 'AP690', 'MA017HumanExperienceCheckpoint: SUCCESS', 'RESULT: SUCCESS')
    },
    @{
        Label = 'MA017HumanExperienceReport'
        Path = 'release\ma017\reports\human-experience-report.md'
        Required = @('AP681-690', 'Owner-Testunterlagen', 'run-ma017-hx-check.ps1', 'RESULT: SUCCESS')
    }
)

foreach ($check in $checks) {
    Test-Document -Label $check.Label -RelativePath $check.Path -RequiredText $check.Required
}

Write-Host 'MA017HumanExperienceCheckpoint: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
