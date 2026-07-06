param(
    [string] $AblageId = 'windows-owner',
    [string] $DisplayName = 'Windows Owner Ablage',
    [string] $Platform = 'Windows'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$devRoot = Join-Path $root '.rkworkspace-dev'
$identityPath = Join-Path $devRoot 'ablage-identity.json'
$privateKeyPath = Join-Path $devRoot 'ablage-private-key.dev.json'

New-Item -ItemType Directory -Path $devRoot -Force | Out-Null

$rsa = [System.Security.Cryptography.RSA]::Create(2048)
try {
    $privateKey = [Convert]::ToBase64String($rsa.ExportRSAPrivateKey())
    $publicKey = [Convert]::ToBase64String($rsa.ExportSubjectPublicKeyInfo())
    $createdAt = [DateTimeOffset]::UtcNow
    $keyId = "dev-key-$AblageId-$([Guid]::NewGuid().ToString('N'))"
    $thumbprintBytes = [System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes("$AblageId|$publicKey|$($createdAt.ToString('O'))"))
    $thumbprint = [Convert]::ToHexString($thumbprintBytes)

    $identity = [ordered]@{
        warning = 'Development identity only. Do not commit. Do not use for production.'
        ablageId = $AblageId
        displayName = $DisplayName
        platform = $Platform
        trustLevel = 'DevTrusted'
        pairingState = 'Paired'
        keyId = $keyId
        certificateId = "dev-cert-$AblageId-$([Guid]::NewGuid().ToString('N'))"
        thumbprint = $thumbprint
        publicKey = $publicKey
        createdAt = $createdAt.ToString('O')
        expiresAt = $createdAt.AddDays(30).ToString('O')
        developmentOnly = $true
    }

    $private = [ordered]@{
        warning = 'Development private key only. Never commit. Regenerate freely.'
        ablageId = $AblageId
        keyId = $keyId
        privateKey = $privateKey
        developmentOnly = $true
        createdAt = $createdAt.ToString('O')
    }

    $identity | ConvertTo-Json -Depth 8 | Set-Content -Path $identityPath -Encoding UTF8
    $private | ConvertTo-Json -Depth 8 | Set-Content -Path $privateKeyPath -Encoding UTF8
}
finally {
    $rsa.Dispose()
}

git -C $root check-ignore -q '.rkworkspace-dev/ablage-identity.json'
$ignored = $LASTEXITCODE -eq 0

Write-Host 'RK Workspace Dev RKWP Identity'
Write-Host "AblageId: $AblageId"
Write-Host "IdentityPath: $identityPath"
Write-Host "PrivateKeyPath: $privateKeyPath"
Write-Host 'Security: DEVELOPMENT ONLY'
Write-Host "GitIgnored: $ignored"
if (-not $ignored) {
    Write-Error '.rkworkspace-dev is not ignored by Git.'
}
Write-Host 'RESULT: SUCCESS'
