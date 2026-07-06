param(
    [Parameter(Mandatory = $true)]
    [string] $AblageName,
    [ValidateSet('Windows', 'macOS', 'MacOS', 'iOS', 'IOS', 'iPadOS', 'IPadOS', 'Android', 'Linux')]
    [string] $Platform = 'Windows',
    [string] $SurfaceType = 'DevelopmentSurface',
    [switch] $Force,
    [switch] $Show
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$storeRoot = Join-Path $root '.rkworkspace-dev\identities'

function ConvertTo-AblageId {
    param(
        [string] $Name,
        [string] $PlatformName
    )

    $normalized = ($Name.Trim().ToLowerInvariant() -replace '[^a-z0-9]+', '-').Trim('-')
    if ([string]::IsNullOrWhiteSpace($normalized)) {
        $normalized = 'ablage'
    }

    return "ablage-$($PlatformName.ToLowerInvariant())-$normalized"
}

New-Item -ItemType Directory -Path $storeRoot -Force | Out-Null

if ($Show) {
    Write-Host 'RK Workspace Ablage Identity Store'
    Write-Host "StoreRoot: $storeRoot"
    Get-ChildItem -Path $storeRoot -Filter '*.identity.json' -ErrorAction SilentlyContinue | ForEach-Object {
        $identity = Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json
        Write-Host "AblageId: $($identity.ablageId)"
        Write-Host "DisplayName: $($identity.displayName)"
        Write-Host "Platform: $($identity.platform)"
        Write-Host "IdentityPath: $($_.FullName)"
    }
    Write-Host 'RESULT: SUCCESS'
    exit 0
}

$ablageId = ConvertTo-AblageId -Name $AblageName -PlatformName $Platform
$identityPath = Join-Path $storeRoot "$ablageId.identity.json"
$privateKeyPath = Join-Path $storeRoot "$ablageId.private-key.dev.json"

if ((Test-Path $identityPath) -and -not $Force) {
    $existing = Get-Content -LiteralPath $identityPath -Raw | ConvertFrom-Json
    Write-Host 'RK Workspace Ablage Identity'
    Write-Host "AblageId: $($existing.ablageId)"
    Write-Host "DisplayName: $($existing.displayName)"
    Write-Host "Platform: $($existing.platform)"
    Write-Host "IdentityPath: $identityPath"
    Write-Host "PrivateKeyPath: $privateKeyPath"
    Write-Host 'Created: false'
    Write-Host 'Overwritten: false'
    Write-Host 'Security: DEVELOPMENT ONLY'
    Write-Host 'RESULT: SUCCESS'
    exit 0
}

$createdAt = [DateTimeOffset]::UtcNow
$keyId = "dev-key-$ablageId-$([Guid]::NewGuid().ToString('N'))"
$publicSeed = "$ablageId|public|$($createdAt.ToString('O'))|$([Guid]::NewGuid().ToString('N'))"
$privateSeed = "$ablageId|private|$($createdAt.ToString('O'))|$([Guid]::NewGuid().ToString('N'))"
$publicKey = [Convert]::ToBase64String([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($publicSeed)))
$privateKey = [Convert]::ToBase64String([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($privateSeed)))
$thumbprint = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes("$ablageId|$publicKey|$($createdAt.ToString('O'))")))

$identity = [ordered]@{
    warning = 'Development identity only. Do not commit. Do not use for production.'
    ablageId = $ablageId
    displayName = $AblageName
    platform = $Platform
    surfaceType = $SurfaceType
    createdAt = $createdAt.ToString('O')
    publicKey = [ordered]@{
        keyId = $keyId
        algorithm = 'DEV-KEY-STRUCTURAL'
        encodedPublicKey = $publicKey
        createdAt = $createdAt.ToString('O')
    }
    privateKeyPath = (Split-Path -Leaf $privateKeyPath)
    trustLevel = 'DevTrusted'
    pairingState = 'Paired'
    certificate = [ordered]@{
        certificateId = "dev-cert-$ablageId-$([Guid]::NewGuid().ToString('N'))"
        thumbprint = $thumbprint
        publicKeyId = $keyId
        createdAt = $createdAt.ToString('O')
        developmentOnly = $true
    }
    developmentOnly = $true
}

$private = [ordered]@{
    warning = 'Development private key only. Never commit. Regenerate freely.'
    ablageId = $ablageId
    keyId = $keyId
    algorithm = 'DEV-KEY-STRUCTURAL'
    privateKey = $privateKey
    developmentOnly = $true
    createdAt = $createdAt.ToString('O')
}

$identity | ConvertTo-Json -Depth 8 | Set-Content -Path $identityPath -Encoding UTF8
$private | ConvertTo-Json -Depth 8 | Set-Content -Path $privateKeyPath -Encoding UTF8

git -C $root check-ignore -q '.rkworkspace-dev/identities/example.identity.json'
$ignored = $LASTEXITCODE -eq 0
if (-not $ignored) {
    Write-Error '.rkworkspace-dev/identities is not ignored by Git.'
}

Write-Host 'RK Workspace Ablage Identity'
Write-Host "AblageId: $ablageId"
Write-Host "DisplayName: $AblageName"
Write-Host "Platform: $Platform"
Write-Host "SurfaceType: $SurfaceType"
Write-Host "IdentityPath: $identityPath"
Write-Host "PrivateKeyPath: $privateKeyPath"
Write-Host "Created: true"
Write-Host "Overwritten: $($Force.IsPresent)"
Write-Host 'Security: DEVELOPMENT ONLY'
Write-Host "GitIgnored: $ignored"
Write-Host 'RESULT: SUCCESS'
