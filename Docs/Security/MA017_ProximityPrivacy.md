# MA017 Proximity Privacy

Status: Pilot rule.

## Grundsatz

Proximity sagt nur, welche Ablage wahrscheinlich am naechsten ist. Proximity sagt nicht, wem ein Objekt gehoert, und transportiert keine Daten.

## Erlaubt

- AblageId
- DongleId
- ephemere BeaconId fuer Labortests
- Richtung
- Distanzklasse
- optionale Meterdistanz
- Confidence
- ProviderStatus
- kurzer Diagnosezeitstempel

## Verboten

- Originaldateien
- PDF-Bytes
- Frame-Inhalte
- personenbezogene Bewegungs-Historie
- versteckte Hintergrundmessung
- Cloud-Auswertung fuer Pilot-Proximity

## Labormodus

MA017 nutzt `PrivacyMode: EphemeralLab`.

Das bedeutet:

- Beacon-IDs sind fuer den Testpfad gedacht.
- Logs duerfen nur Diagnosebeweise enthalten.
- Keine Langzeit-Ortung wird erzeugt.
- Der Owner kann den Pfad abschalten, indem UWB/Dongle nicht aktiviert wird.

## Verbindung zu Security

UWB, BLE, WiFi und Dongle liefern nur Signale. RKWP entscheidet weiterhin:

- Secure Session
- CarryLease
- FrameSession
- Return
- Recovery
- Policy

## Ergebnis

MA017 Proximity Privacy: READY
