# iOS Surface Plan

Status: Docs/stub only
Datum: 2026-07-05

## Ziel

Eine native iOS/iPadOS Surface App zeigt RKWP Frames an und bleibt FrameOnly.

## Minimaler Ablauf

1. App startet als mobile Ablage.
2. RKWP DevTransport verbindet spaeter.
3. FrameSession wird empfangen.
4. Frame wird angezeigt.
5. Keine Originaldatei wird gespeichert.
6. Haptik signalisiert Ankunft.
7. Return/Close gibt den Frame zurueck.

## Quellen

- RK Workspace App
- Document Picker
- Share Extension
- Pasteboard bewusst und begrenzt

## Nicht erlaubt

- globale App-Inhalte ohne Benutzerkontext greifen
- Originaldatei als lokale freie Datei speichern
- Ownership Transfer als Default
