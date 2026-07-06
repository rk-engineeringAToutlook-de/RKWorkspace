# iOS USB Device Test Runbook

Status: MA013.16 runbook  
Datum: 2026-07-06

## Ziel

Owner testet iPhone/iPad ueber macOS, Xcode und USB.

## Vorbereitung

1. Mac mit aktuellem Xcode vorbereiten.
2. Repository oder Context-Pack auf dem Mac bereitstellen.
3. iPhone/iPad per USB anschliessen.
4. Developer Mode aktivieren.
5. `Trust This Computer` bestaetigen.
6. Signing Team in Xcode setzen.

## App installieren

1. Xcode-Projekt oeffnen.
2. Geraet auswaehlen.
3. Build and Run.
4. Logs in Xcode Console oeffnen.

## Windows Owner

Auf Windows:

```powershell
.\tools\run-windows-pdf-owner-securedev.ps1 -SmokeTest
.\tools\run-windows-owner-for-mac.ps1 -InfoOnly
```

## Netzwerk

- iPhone/iPad und Windows im gleichen WLAN/LAN.
- Windows IP bestimmen.
- Local Network Permission in iOS bestaetigen.
- Port `57100` fuer Lab-Verbindung nutzen.

## Test

1. App starten.
2. RKWP Verbindung aufbauen.
3. Frame anzeigen.
4. Haptics pruefen.
5. No File Ingress pruefen.
6. Return ausloesen.
7. Recovery/ConnectionLost simulieren.

## Erfolg

```text
FrameView: OK
Haptics: OK
GuestHasPdfFile: NO
NoFileIngress: SUCCESS
Return: SUCCESS
```
