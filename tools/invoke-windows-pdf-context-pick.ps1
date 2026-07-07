param(
    [Parameter(Mandatory = $true)]
    [string] $PdfPath,
    [string] $PipeName = 'RKWorkspace.NativeGlassOverlay.PdfPick',
    [int] $TimeoutMs = 1500
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Core

if ([string]::IsNullOrWhiteSpace($PdfPath)) {
    exit 2
}

$resolvedPdfPath = [System.IO.Path]::GetFullPath($PdfPath)
if (-not [System.IO.File]::Exists($resolvedPdfPath)) {
    exit 3
}

if (-not $resolvedPdfPath.EndsWith('.pdf', [System.StringComparison]::OrdinalIgnoreCase)) {
    exit 4
}

$client = [System.IO.Pipes.NamedPipeClientStream]::new(
    '.',
    $PipeName,
    [System.IO.Pipes.PipeDirection]::InOut,
    [System.IO.Pipes.PipeOptions]::None)

try {
    $client.Connect($TimeoutMs)
    $writer = [System.IO.StreamWriter]::new($client, [System.Text.Encoding]::UTF8, 1024, $true)
    $reader = [System.IO.StreamReader]::new($client, [System.Text.Encoding]::UTF8, $false, 1024, $true)
    $writer.AutoFlush = $true

    $payload = @{ pdfPath = $resolvedPdfPath } | ConvertTo-Json -Compress
    $writer.WriteLine($payload)
    $response = $reader.ReadLine()
    if ([string]::IsNullOrWhiteSpace($response)) {
        exit 5
    }

    $json = $response | ConvertFrom-Json
    if ($json.ok -eq $true) {
        exit 0
    }

    exit 6
}
catch {
    exit 7
}
finally {
    if ($reader) { $reader.Dispose() }
    if ($writer) { $writer.Dispose() }
    $client.Dispose()
}
