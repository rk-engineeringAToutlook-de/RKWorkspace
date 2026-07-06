# Windows To macOS LAN Test Plan

Status: MA010.02

## Ziel

Der erste echte Windows-zu-macOS-Test soll eine Windows Owner-Ablage und eine macOS Guest-Ablage im selben lokalen Netz verbinden. DevLan ist dafuer das Development-Profil.

## Voraussetzungen

- Windows und macOS im selben LAN.
- Windows Firewall erlaubt den gewaehlten Port.
- Beide Seiten kennen Owner- und Guest-AblageId.
- DevPairing ist fuer den Lab-Test erlaubt.
- SecurityMode bleibt `DevelopmentInsecure`, bis produktive Secure Session fertig ist.

## Windows Owner Start

```powershell
.\tools\run-rkwp-lan-owner.ps1 -BindAddress 0.0.0.0 -Port 57100 -AllowDevPairing
```

Fuer den PDF-Owner-Pfad:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -PdfPath samples/Objects/Rechnung.pdf -Port 57100 -AllowDevPairing
```

Windows zeigt:

- `rkwp+tcp-dev://<windows-ip>:57100`
- PdfObjectId der Owner-PDF
- LeaseMode FrameOnly
- NoFileIngressRequired
- Development-Warnung
- wartenden Owner-Server

## macOS Guest Start

Spaeter auf macOS:

```powershell
.\tools\run-rkwp-lan-guest.ps1 -Host <windows-ip> -Port 57100
```

Der Windows-Smoke kann lokal simulieren:

```powershell
.\tools\run-rkwp-lan-smoke.ps1
```

## Testschritte

1. Windows Owner starten.
2. macOS Guest mit Windows-IP verbinden.
3. `AblageHello` pruefen.
4. `AblageCapabilities` pruefen.
5. DevPairing im Lab erlauben.
6. SecureSession-Spike-Felder sichtbar halten.
7. Heartbeat senden.
8. FrameUpdate senden.
9. Disconnect und Recovery nachvollziehen.
10. No File Ingress pruefen.

## Erwartetes Ergebnis

- Verbindung steht im LAN.
- RKWP-Nachrichten laufen.
- Owner bleibt Eigentuemer.
- Guest bekommt nur Frame-Daten.
- Kein Originalpfad, keine freie Datei, keine Originalbytes auf Guest.

## Blocker

- kein finales TLS
- kein produktiver Trust Store
- keine native macOS Frame Guest Surface in diesem Windows-Thread
- keine finale Pairing UI
- Firewall/Local-Network-Prompts muessen auf echten Geraeten bestaetigt werden
