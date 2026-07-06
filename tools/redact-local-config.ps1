param(
    [string[]] $Path,
    [string] $OutputDirectory,
    [switch] $DryRun,
    [switch] $SmokeTest
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Split-Path -Parent $PSScriptRoot)).Path

function Redact-Value {
    param([object] $Value)

    if ($null -eq $Value) {
        return $null
    }

    if ($Value -is [System.Array]) {
        $items = @()
        foreach ($item in $Value) {
            $items += (Redact-Value -Value $item)
        }
        return $items
    }

    if ($Value -is [pscustomobject]) {
        $result = [ordered]@{}
        foreach ($property in $Value.PSObject.Properties) {
            if ($property.Name -match '(?i)(password|secret|token|credential|private|certificate|identity|apiKey|key)') {
                $result[$property.Name] = 'REDACTED'
            }
            else {
                $result[$property.Name] = Redact-Value -Value $property.Value
            }
        }
        return [pscustomobject]$result
    }

    return $Value
}

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $root 'release\ma016\context\redacted-config'
}

$createdSmokeFile = $null
if ($SmokeTest) {
    $createdSmokeFile = Join-Path $env:TEMP 'rkworkspace-local-config-smoke.json'
    @'
{
  "endpoint": "localhost",
  "token": "placeholder-token-value-123456",
  "nested": {
    "client_secret": "placeholder-client-secret-123456",
    "mode": "SmokeTest"
  }
}
'@ | Set-Content -Path $createdSmokeFile -Encoding UTF8
    $Path = @($createdSmokeFile)
}

if ($null -eq $Path -or $Path.Count -eq 0) {
    $Path = @()
    $Path += Get-ChildItem -Path (Join-Path $root 'config') -File -Filter '*.json' -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName
    $Path += Get-ChildItem -Path (Join-Path $root 'release\ma016\config') -File -Include '*.local.json', '*.secret.json', '*identity*.json' -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$processed = 0
foreach ($configPath in $Path) {
    if (-not (Test-Path $configPath)) {
        continue
    }

    $resolved = (Resolve-Path -LiteralPath $configPath).Path
    if (-not $SmokeTest -and -not $resolved.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to redact outside repository without SmokeTest: $resolved"
    }

    $json = Get-Content -LiteralPath $resolved -Raw | ConvertFrom-Json
    $redacted = Redact-Value -Value $json
    $target = Join-Path $OutputDirectory ((Split-Path -Leaf $resolved) -replace '\.json$', '.redacted.json')
    if (-not ($DryRun -or $SmokeTest)) {
        $redacted | ConvertTo-Json -Depth 40 | Set-Content -Path $target -Encoding UTF8
    }
    elseif ($SmokeTest) {
        $tempTarget = Join-Path $env:TEMP 'rkworkspace-local-config-smoke.redacted.json'
        $redacted | ConvertTo-Json -Depth 40 | Set-Content -Path $tempTarget -Encoding UTF8
        $redactedText = Get-Content -LiteralPath $tempTarget -Raw
        if ($redactedText -match 'placeholder-token|placeholder-client') {
            throw 'Smoke redaction left a secret in output.'
        }
    }
    $processed++
}

if ($createdSmokeFile -and (Test-Path $createdSmokeFile)) {
    Remove-Item -LiteralPath $createdSmokeFile -Force
}

Write-Output 'RK Workspace Local Config Redaction'
Write-Output '-----------------------------------'
Write-Output "DryRun: $($DryRun -or $SmokeTest)"
Write-Output "Processed: $processed"
Write-Output "OutputDirectory: $OutputDirectory"
Write-Output 'LocalConfigRedaction: SUCCESS'
Write-Output 'RESULT: SUCCESS'
exit 0
