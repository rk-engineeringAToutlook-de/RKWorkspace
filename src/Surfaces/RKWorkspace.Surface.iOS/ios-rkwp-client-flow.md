# iOS/iPadOS RKWP Client Flow

Status: MA011.09 planning baseline  
Datum: 2026-07-06

## Ziel

iPad/iPhone verbindet sich als Frame Guest Surface mit einem Windows Owner. Der Windows Owner bleibt Besitzer der echten PDF. iOS/iPadOS zeigt nur den RKWP Frame.

## Startfluss

```text
App Start
  -> AblageIdentity laden oder erzeugen
  -> SurfaceId bereitstellen
  -> RKWP DevLan/SecureDev Client starten
  -> Windows Owner verbinden
  -> AblageHello senden
  -> Pairing / Trust Status auswerten
  -> AblageCapabilities senden
  -> FrameSessionOpen empfangen
  -> Frame anzeigen
  -> Heartbeat senden
  -> Return senden
```

## AblageHello

Der erste Hello enthaelt:

- AblageId.
- SurfaceId.
- Platform `IOS` oder `IPadOS`.
- Role `FrameGuestSurface`.
- Capabilities `FrameView`, `TouchInputPlanned`, `HapticsPrepared`.
- SecurityMode `DevelopmentAuthenticated` fuer Dev.

## FrameSession

Die App akzeptiert nur Frame-Sessions mit:

- gueltiger SessionId.
- gueltiger LeaseId.
- bekannter OwnerAblageId.
- Policy `FrameOnly`.
- Preview/Frame Payload ohne Originaldatei.

## No File Ingress

Verboten:

- PDF-Datei in Documents speichern.
- Originalpfad anzeigen.
- Originalbytes dauerhaft materialisieren.
- Share Sheet als Datei erzeugen.

Erlaubt:

- Frame-Repräsentation im Speicher anzeigen.
- temporare View-Daten im Prozess halten.
- Diagnose-Log ohne Originalbytes schreiben.

## Return

Beim Zurueckgeben sendet die App:

- `ReturnRequested`.
- letzte FrameSessionId.
- letzte LeaseId.
- optional sichtbaren Zustand.
- kein PDF-File und keine Originalbytes.

Danach wird der Frame lokal als `nicht verfuegbar` markiert.

## Heartbeat

Solange der Frame sichtbar ist, sendet iOS/iPadOS regelmaessig Heartbeats. Bei Verbindungsverlust wechselt die Anzeige auf `Verbindung verloren` und speichert weiterhin keine Datei.
