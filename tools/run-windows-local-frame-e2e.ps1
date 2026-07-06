param(
    [switch] $SmokeTest,
    [string] $PdfPath
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.WindowsLocalFrameE2E\RKWorkspace.WindowsLocalFrameE2E.csproj'
$arguments = @()

if ($SmokeTest) {
    $arguments += '--smoke-test'
}

if (-not [string]::IsNullOrWhiteSpace($PdfPath)) {
    $arguments += '--pdf-path'
    $arguments += $PdfPath
}

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$output = dotnet run --project $project -- @arguments 2>&1
$exitCode = $LASTEXITCODE
$output | ForEach-Object { Write-Output $_ }
if ($exitCode -ne 0) {
    exit $exitCode
}

if ($SmokeTest) {
    $text = $output -join [Environment]::NewLine
    $required = @(
        'OwnerVisibleStatus: wartet auf Rueckgabe',
        'OwnerReturnedStatus: zurueckgegeben',
        'OwnerRecoveryStatus: wieder verfuegbar',
        'GuestVisibleStatus: liegt hier im Frame',
        'GuestRevokedStatus: nicht verfuegbar',
        'GuestExpiredStatus: Verbindung verloren',
        'VisibleStateLanguage: SUCCESS',
        'RESULT: SUCCESS'
    )

    foreach ($line in $required) {
        if (-not $text.Contains($line)) {
            throw "Windows Local Frame E2E Smoke failed because output did not contain: $line"
        }
    }
}

exit 0
