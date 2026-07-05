# RKWP Transport Profiles

Dokument-ID: RKWS-RKWP-TRANSPORT-001
Status: Draft
Datum: 2026-07-05

## Zweck

RKWP ist transportneutral. Das Protocol-Projekt kennt keine Named Pipes, kein HTTP, kein BLE, kein UWB und keine Plattform-API.

## Profile

Spaetere Profile koennen sein:

- LocalLoopback fuer Tests
- NamedPipeDev fuer lokale Windows-Prozesse
- LocalNetworkDev fuer lokale Labornetze
- WebSocketDev fuer spaetere Dev-Experimente ohne Browser-UI
- FutureTcpTls fuer produktivere Desktop-zu-Desktop-Pfade
- WebRTC fuer mobile Oberflaechen
- BLE/UWB nur fuer Naehe und Richtung, nicht fuer Payload
- USB-Dongle als Ablage-Anker und Trust-/Proximity-Hilfe

## MA008.01 Dev Transport

Der erste aktive Dev-Transport ist `NamedPipeDev` in `RKWorkspace.Transport.Dev`.

Er prueft lokal:

- `AblageHello`
- `AblageCapabilities`
- SessionId-Aushandlung
- `CarryLeaseHeartbeat`
- `FrameUpdate`
- Error Message
- Timeout
- Disconnect

Script:

```powershell
.\tools\run-rkwp-transport.ps1 -SmokeTest
```

Dieser Pfad ist nur Entwicklung. Er ist kein produktives Transportprofil.

## Proximity ist kein Payload-Transport

MA007.13 ordnet Entfernung und Richtung bewusst ausserhalb des Payload-Transports ein. Proximity-Quellen koennen sein:

- `Simulated`
- `ManualMap`
- `BLE`
- `WiFi`
- `Dongle`
- `UWB`
- `SensorFusion`

Diese Quellen duerfen helfen, die naechste Ablage zu finden. Sie duerfen keine Originaldatei transportieren und keine Ownership-Entscheidung ersetzen.

Die Glass Edge nutzt Proximity nur fuer:

- Richtung der einen sichtbaren Kante
- Distanzklasse
- Confidence
- spaetere Gegenkante auf der Zielablage

## Reihenfolge

MA007.00 definiert nur Nachrichten und Ownership. Transportprofile werden erst implementiert, wenn Ownership und FrameOnly verifiziert sind.

Ab MA007.03 muss jedes spaetere Transportprofil die Security-Vertraege tragen koennen:

- Nonce und monotone SequenceNumber bleiben transportuebergreifend erhalten.
- Session Protector muss Authentisierung, Integritaet und spaeter Verschluesselung abbilden.
- LeaseId und SessionId duerfen vom Transport nicht umgeschrieben werden.
- Audit- und Revocation-Ereignisse muessen auch bei Verbindungsabbruch nachvollziehbar bleiben.
- Produktive kritische Umgebungen duerfen `DevelopmentInsecure` nicht akzeptieren.

## Nicht-Ziele

- kein Datei-Streaming
- kein Clipboard-Sync
- keine Bildschirmuebertragung
- keine Cloud
- keine produktive Discovery

Transport ist spaeter nur der Weg. Die Semantik bleibt: Original bleibt beim Owner.
