param(
    [string] $PdfPath,
    [string] $BindAddress = '0.0.0.0',
    [int] $Port = 57120,
    [int] $Page = 1,
    [int] $Width = 1400,
    [switch] $Once,
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Tools\RKWorkspace.MacPdfFrameOwnerHost\RKWorkspace.MacPdfFrameOwnerHost.csproj'
$arguments = @(
    '--bind-address', $BindAddress,
    '--port', $Port.ToString(),
    '--page', $Page.ToString(),
    '--width', $Width.ToString()
)

if (-not [string]::IsNullOrWhiteSpace($PdfPath)) {
    $arguments += '--pdf-path'
    $arguments += $PdfPath
}

if ($Once) {
    $arguments += '--once'
}

if ($SmokeTest) {
    $arguments += '--smoke-test'
}

Write-Host 'RK Workspace macOS PDF Frame Owner'
Write-Host '----------------------------------'
Write-Host "BindAddress: $BindAddress"
Write-Host "Port: $Port"
Write-Host 'Candidate Windows addresses for macOS:'
Get-NetIPAddress -AddressFamily IPv4 |
    Where-Object { $_.IPAddress -notlike '127.*' -and $_.PrefixOrigin -ne 'WellKnown' } |
    ForEach-Object { Write-Host ("  {0}: {1}" -f $_.InterfaceAlias, $_.IPAddress) }

dotnet build $project -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $project -- @arguments
exit $LASTEXITCODE
