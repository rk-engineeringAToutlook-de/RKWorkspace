param(
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$reportPath = Join-Path $root 'release\ma017\reports\documentation-checkpoint-report.md'

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

Write-Host 'RK Workspace MA017 Documentation Checkpoint'
Write-Host '-------------------------------------------'

Test-TextFile -Label 'MA017ReadinessReview' -RelativePath 'Docs\Readiness\MA017_ReadinessReview.md' -Required @('MA017ReadinessReview: READY', 'RESULT: SUCCESS')
Test-TextFile -Label 'MA017GoNoGoDecision' -RelativePath 'Docs\Readiness\MA017_GoNoGoDecision.md' -Required @('MA017GoNoGoDecision: READY', 'ConditionalGoForControlledPilot')
Test-TextFile -Label 'MA017RealTestRunbook' -RelativePath 'Docs\Testing\MA017_RealTestRunbook.md' -Required @('MA017RealTestRunbook: READY', 'run-ma017-smoke.ps1')
Test-TextFile -Label 'MA017PilotAcceptanceCriteria' -RelativePath 'Docs\Readiness\MA017_PilotAcceptanceCriteria.md' -Required @('MA017PilotAcceptanceCriteria: READY', 'No File Ingress')
Test-TextFile -Label 'MA017FailureTaxonomy' -RelativePath 'Docs\Readiness\MA017_FailureTaxonomy.md' -Required @('MA017FailureTaxonomy: READY', 'F1 No File Ingress Violation')
Test-TextFile -Label 'MA017NextActions' -RelativePath 'Docs\Readiness\MA017_NextActions.md' -Required @('MA017NextActions: READY', 'MA017 Final Verification Plan')
Test-TextFile -Label 'MA017RoadmapUpdate' -RelativePath 'Docs\Roadmap\RKWorkspace_Roadmap.md' -Required @('MA017 Documentation Checkpoint', 'MA017 final verification')
Test-TextFile -Label 'MA017ReadmeUpdate' -RelativePath 'README.md' -Required @('MA017 Pilotstand', 'run-ma017-documentation-checkpoint.ps1')
Test-TextFile -Label 'MA017GlossaryUpdate' -RelativePath 'Docs\Glossary.md' -Required @('Pilot Acceptance Criteria', 'Failure Taxonomy', 'Go/No-Go Decision')
Test-TextFile -Label 'MA017DocumentationCheckpoint' -RelativePath 'Docs\Readiness\MA017_DocumentationCheckpoint.md' -Required @('AP721', 'AP730', 'MA017DocumentationCheckpoint: SUCCESS')

$content = @(
    '# MA017 Documentation Checkpoint Report',
    '',
    'Status: Verified',
    'Datum: 2026-07-07',
    '',
    '## Result',
    '',
    '```text',
    'MA017DocumentationCheckpoint: SUCCESS',
    'RESULT: SUCCESS',
    '```'
)

Set-Content -Path $reportPath -Value $content -Encoding UTF8

Write-Host 'MA017DocumentationCheckpoint: SUCCESS'
Write-Host 'RESULT: SUCCESS'
exit 0
