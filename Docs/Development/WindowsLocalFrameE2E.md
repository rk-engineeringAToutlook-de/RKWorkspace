# Windows Local Frame E2E

Status: Draft  
Datum: 2026-07-05

## Ziel

MA008.03 fuehrt den ersten lokalen Windows-End-to-End-Test fuer den RKWP Original-Owned Frame-Pfad ein.

Der Test beweist nicht Dateiuebertragung. Er beweist:

- Windows Owner besitzt die PDF.
- Windows Guest ist eine gepaarte Dev-Ablage.
- RKWP Dev Transport verbindet beide Ablagen lokal.
- Der Owner erstellt eine CarryLease und eine FrameSession.
- Die Guest-Ablage sieht nur einen PDF-Frame.
- Die Guest-Ablage bekommt keine PDF-Datei, keinen Originalpfad und keine Originalbytes.
- Rueckgabe und Recovery funktionieren.

## Ausfuehren

```powershell
.\tools\run-windows-local-frame-e2e.ps1 -SmokeTest
.\tools\run-windows-local-frame-e2e.ps1 -PdfPath "samples\Objects\Rechnung.pdf"
```

## Komponenten

Der Smoke nutzt:

- `AblageIdentity`
- `DevAblagePairingService`
- `AblageTrustGate`
- `RkwpDevTransport`
- `PdfFrameDocument`
- `ThingOwnership`
- `CarryLease`
- `FrameSession`
- `PdfGuestFrame`

## Erwartete Ausgabe

Der Smoke muss melden:

- `OwnerStarted: OK`
- `GuestStarted: OK`
- `DevPairing: SUCCESS`
- `TransportConnected: SUCCESS`
- `PdfOriginalRegistered: OK`
- `CarryLease: Active`
- `OwnerLocked: OK`
- `OwnerVisibleStatus: wartet auf Rueckgabe`
- `OwnerReturnedStatus: zurueckgegeben`
- `OwnerRecoveryStatus: wieder verfuegbar`
- `FrameSession: Active`
- `GuestFrame: OK`
- `GuestVisibleStatus: liegt hier im Frame`
- `GuestRevokedStatus: nicht verfuegbar`
- `GuestExpiredStatus: Verbindung verloren`
- `VisibleStateLanguage: SUCCESS`
- `NoFileIngress: SUCCESS`
- `Return: SUCCESS`
- `Recovery: SUCCESS`
- `RESULT: SUCCESS`

## Renderer-Status

Echter PDF-Seitenrenderer bleibt noch blockiert. Der aktuelle Test nutzt eine sichere MetadataPreview als Frame-Repräsentation. Das ist erlaubt, solange No File Ingress strikt eingehalten wird.

## Abgrenzung

Der E2E-Test ist lokal und Development-only. Er ersetzt keine produktive Kryptografie, kein produktives Pairing, keine Discovery und keinen echten Cross-Device-Transport.
