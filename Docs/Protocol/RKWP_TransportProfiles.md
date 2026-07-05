# RKWP Transport Profiles

Dokument-ID: RKWS-RKWP-TRANSPORT-001
Status: Draft
Datum: 2026-07-05

## Zweck

RKWP ist transportneutral. Das Protocol-Projekt kennt keine Named Pipes, kein HTTP, kein BLE, kein UWB und keine Plattform-API.

## Profile

Spaetere Profile koennen sein:

- LocalLoopback fuer Tests
- NamedPipe fuer lokale Windows-Prozesse
- LAN fuer Desktop-zu-Desktop
- WebRTC fuer mobile Oberflaechen
- BLE/UWB nur fuer Naehe und Richtung, nicht fuer Payload
- USB-Dongle als Ablage-Anker und Trust-/Proximity-Hilfe

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
