# Ablage Identity Store

Status: Draft  
Datum: 2026-07-06  
Arbeitsauftrag: MA011.02

## Ziel

Ablagen brauchen fuer Dev- und spaetere SecureDev-Tests eine stabile technische Identitaet. Der Ablage Identity Store erzeugt lokale Development-Identitaeten, ohne private Schluessel oder lokale Zertifikate in Git oder Context Packs aufzunehmen.

## Lokaler Pfad

```text
.rkworkspace-dev/identities/
```

Der Pfad ist durch `.gitignore` abgedeckt. Er darf nicht versioniert werden.

## Script

```powershell
.\tools\init-ablage-identity.ps1 -AblageName "Windows Owner" -Platform Windows
.\tools\init-ablage-identity.ps1 -AblageName "Windows Owner" -Platform Windows -Show
.\tools\init-ablage-identity.ps1 -AblageName "Windows Owner" -Platform Windows -Force
```

## Identity Record

Eine lokale Identity enthaelt:

- AblageId
- DisplayName
- Platform
- SurfaceType
- CreatedAt
- PublicKey
- PrivateKeyPath
- TrustLevel
- PairingState
- Certificate Reference
- DevelopmentOnly

Der private Dev-Key liegt in einer separaten Datei im gleichen lokalen Store und wird nicht ins Context Pack kopiert.

## Production-Grenze

Der Store ist noch kein produktiver Trust Store. Er ist ein lokaler Dev-Baustein fuer:

- DevPairing
- SecureDevTransport
- macOS/iOS Handoff
- Windows Owner/Guest Piloten

Produktiv fehlen weiterhin:

- echte Zertifikatskette
- Trust Store
- Key Rotation
- Revocation Distribution
- Owner Pairing UI

## Verifikation

```powershell
.\tools\init-ablage-identity.ps1 -AblageName "Windows Owner" -Platform Windows
.\tools\run-rkwp-tests.ps1
.\tools\export-codex-context.ps1
.\tools\run-tests.ps1
```
