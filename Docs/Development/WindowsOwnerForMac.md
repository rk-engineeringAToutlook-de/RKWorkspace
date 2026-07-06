# Windows Owner For Mac

Status: MA010.03

## Zweck

Windows startet als Owner-Ablage fuer den ersten echten macOS-Guest-Test. Die echte PDF bleibt auf Windows. macOS soll spaeter nur einen RKWP Frame sehen und keine freie PDF-Datei erhalten.

## Start

Smoke:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -SmokeTest
```

Lab Owner:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -PdfPath samples/Objects/Rechnung.pdf -Port 57100 -AllowDevPairing
```

Optional:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -BindAddress 0.0.0.0 -Port 57100 -OwnerAblageId ablage-windows-owner -ExpectedGuestAblageId ablage-macos-guest -SecurityMode DevelopmentInsecure
```

## Ausgabe

Das Script meldet:

- WindowsOwnerAblageId
- ExpectedGuestAblageId
- DevLanUrl
- Port
- SecurityMode
- PdfPath
- PdfObjectId
- LeaseMode FrameOnly
- NoFileIngressRequired
- Handoff-Pfad
- Context-Pack-Pfad

## Smoke-Gates

- PDF existiert.
- AblageIdentity existiert oder wird im Dev-Ordner erzeugt.
- DevPairing ist vorbereitet.
- FrameSession ist vorbereitet.
- DevLan Server/Client laufen im Loopback-Smoke.
- Owner kann ohne echten Guest sauber beendet werden.
- Shutdown ist sauber.
- `RESULT: SUCCESS`.

## Offene Punkte

- echter macOS DevLan Client fehlt noch.
- produktives TLS/mutual auth fehlt.
- macOS Local Network/Firewall muss auf echter Hardware getestet werden.
- echter PDF-Seitenrenderer bleibt noch blockiert.
