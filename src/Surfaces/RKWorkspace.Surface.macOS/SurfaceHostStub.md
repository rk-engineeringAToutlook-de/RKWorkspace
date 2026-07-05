# macOS Surface Host Stub

Status: Docs/stub only
Datum: 2026-07-05

## Zweck

Dieser Stub beschreibt den spaeteren macOS Surface Host. Er ist keine buildbare macOS-App und bricht den Windows-Build nicht.

## Spaetere Aufgaben

- native macOS-App oder Agent starten
- SurfaceIdentity bereitstellen
- RKWP DevTransport verbinden
- FrameGuestSurface hosten
- Glass Edge vorbereiten
- Return/Close/Recovery signalisieren

## Pflichtregeln

- keine Originaldatei auf macOS speichern
- keine Ownership-Entscheidung lokal erfinden
- FrameOnly respektieren
- Input nur ueber RKWP Input Channel und Policy
