# iOS/iPadOS Sandbox and Object Sources

Status: Prepared handoff  
Datum: 2026-07-06

## Grundsatz

iOS/iPadOS erlaubt kein beliebiges globales Greifen fremder App-Inhalte. RK Workspace respektiert das.

## Erlaubte erste Quellen

### RK Workspace App

Eigene Surface, eigene Frames, eigener Zustand.

### Share Extension

Bewusst vom Benutzer gewaehlte Inhalte koennen spaeter in RK Workspace sichtbar werden. Kein stiller Zugriff.

### Document Picker

Nur durch explizite Auswahl. Security-scoped Zugriff beachten.

### Pasteboard

Nur bewusst und begrenzt. Pasteboard ist kein Dauerkanal und kein heimlicher Ingress.

### Eigene Surface

Der erste mobile Test zeigt Windows-PDF als Frame, nicht als lokale Datei.

## Nicht erlaubt als erste Annahme

- globale Erfassung beliebiger App-Inhalte.
- Umgehen der Sandbox.
- stille Kopie aus fremden Apps.
- automatischer Ownership Transfer.

## No File Ingress Regeln

- keine freie PDF in App-Container.
- keine Originalbytes als Datei.
- kein Originalpfad als lokale Datei.
- FrameOnly respektieren.
- Return schliesst die lokale Frame-Sicht.
