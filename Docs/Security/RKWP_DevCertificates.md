# RKWP Dev Certificates

Dokument-ID: RKWS-RKWP-DEV-CERT-001  
Status: Draft  
Datum: 2026-07-06

## Zweck

Dev-Zertifikate machen RKWP Secure-Session-Tests lokal nachvollziehbar. Sie sind nur fuer Development und Test.

Sie sind keine produktive Identitaet.

## Script

Lokale Dev-Identitaet erzeugen:

```powershell
.\tools\init-dev-rkwp-identity.ps1
```

Optionale Parameter:

```powershell
.\tools\init-dev-rkwp-identity.ps1 -AblageId windows-owner -DisplayName "Windows Owner Ablage" -Platform Windows
```

Das Script erzeugt:

```text
.rkworkspace-dev/ablage-identity.json
.rkworkspace-dev/ablage-private-key.dev.json
```

## Git-Regel

`.rkworkspace-dev/` ist in `.gitignore` eingetragen.

Dev-Identitaeten, Dev-Zertifikate und private Dev-Keys duerfen nicht committed werden und duerfen nicht ins Context Pack.

## Inhalt

Die Development-Dateien enthalten:

- AblageId
- DisplayName
- Platform
- Dev KeyId
- Dev CertificateId
- PublicKey
- PrivateKey
- Thumbprint
- Laufzeit
- Warnung `Development only`

## Sicherheit

Diese Dateien sind absichtlich regenerierbar. Sie sollen lokale Tests ermoeglichen, nicht produktiven Trust herstellen.

Produktiv benoetigt RKWP spaeter:

- echten Trust Store.
- echte Certificate Chain oder anderes Mutual-Auth-Verfahren.
- echte Session-Key-Aushandlung.
- Schluesselrotation.
- Revocation.
- Audit-Schutz gegen Manipulation.

## Test

AP021 prueft:

- Script erzeugt lokale Dev-Identitaet.
- Git ignoriert `.rkworkspace-dev/`.
- RKWP-Tests erzeugen Dev-Zertifikate im Speicher.
- Mutual Dev Authentication funktioniert strukturell.
