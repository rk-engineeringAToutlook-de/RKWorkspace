# USB Dongle Control Protocol

Status: Draft  
Datum: 2026-07-06

## Ziel

AP157 beschreibt einen spaeteren USB-Control-Kanal fuer den Ablage Anchor Dongle. Der Kanal dient Provisioning, Diagnose und Firmwarepflege, nicht Nutzdaten.

## Grundregel

Der Dongle darf sich nicht als Laufwerk fuer RK Workspace Objekte verhalten.

Erlaubt:

- Status lesen
- Version lesen
- DongleId lesen
- AblageId setzen
- Provisioning starten
- Beacon konfigurieren
- Diagnose lesen
- Firmware-Update starten
- Reset ausloesen

Nicht erlaubt:

- PDF speichern
- Datei kopieren
- Objekt materialisieren
- Frame-Daten cachen
- Ownership transferieren

## Nachrichten

Geplante Befehle:

```text
GetStatus
GetIdentity
SetAblageBinding
StartPairingHint
SetBeaconMode
ReadDiagnostics
BeginFirmwareUpdate
AbortFirmwareUpdate
ResetDevice
```

## Antwortfelder

Jede Antwort enthaelt:

- Command
- Status
- ErrorCode
- FirmwareVersion
- DongleId
- Timestamp

## Transport

Der genaue USB-Stack bleibt offen:

- HID fuer einfache Steuerung
- CDC fuer serielle Labordiagnose
- WinUSB/libusb fuer spaeteren Produktpfad

Die Wahl darf die No-File-Ingress-Regel nicht verletzen.

## Einordnung

USB Control ist lokal und optional. BLE/WiFi/UWB koennen Presence liefern, waehrend USB Control die sichere Einrichtung erleichtert.
