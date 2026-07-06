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

Ab MA008.09 muss jedes Transportprofil vor produktiver Freigabe durch `RkwpSecurityGate` bewertet werden:

- Development und Test duerfen unsichere Profile nur sichtbar markiert verwenden.
- Staging soll Production spiegeln.
- Production lehnt `DevelopmentInsecure`, fehlenden Audit, fehlenden Replay-Schutz und fehlendes Policy Binding ab.
- `NamedPipeDev` bleibt Development-only.

Ab MA009.01 kommt der Secure-Session-Spike hinzu:

- Transportprofile muessen Handshake-Nachrichten tragen koennen.
- Ablage-Identitaeten muessen vor Lease/Frame geprueft werden.
- Dev-Zertifikate duerfen nur in Development/Test verwendet werden.
- SessionId, LeaseId, PolicyId und PolicyVersion duerfen nicht vom Transport veraendert werden.
- Replay-Schutz bleibt transportunabhaengig im RKWP-Protokoll.
- `LocalNetworkDev` muss dieselben Security-Felder tragen wie `NamedPipeDev`.

## MA010.02 DevLan

MA010.02 fuehrt `LocalNetworkDev` als erstes TCP-basiertes Lab-Profil ein.

Projekt:

```text
src/Communication/RKWorkspace.RkwpTransport.DevLan/
```

Scripts:

```powershell
.\tools\run-rkwp-lan-owner.ps1 -Port 57100 -AllowDevPairing
.\tools\run-rkwp-lan-guest.ps1 -Host 192.168.x.x -Port 57100
.\tools\run-rkwp-lan-smoke.ps1
```

Der Smoke prueft Owner Server, Guest Client, AblageHello, Capabilities, DevPairing, SecureSession-Spike-Markierung, Heartbeat, FrameUpdate, Disconnect und No File Ingress.

DevLan ist Development-only und ersetzt kein finales TLS.

## Windows zu macOS Development Profil

MA009.02 bereitet Windows als Owner fuer macOS vor:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -SmokeTest
```

Aktuell real:

- `dev+namedpipe://rkws-windows-owner-macos` fuer lokalen Windows-DevTransport.
- Smoke-Test ohne Hanger.
- PDF-Pfad und AblageIdentity werden geprueft.

Geplant fuer echte Geraete:

```text
rkwp+tcp-dev://<windows-host>:43707
```

Dieser Netzwerkpfad ist noch nicht implementiert. Er muss spaeter Secure Session, AblageIdentity, Trust, FrameOnly und No File Ingress tragen.

## Nicht-Ziele

- kein Datei-Streaming
- kein Clipboard-Sync
- keine Bildschirmuebertragung
- keine Cloud
- keine produktive Discovery

Transport ist spaeter nur der Weg. Die Semantik bleibt: Original bleibt beim Owner.
