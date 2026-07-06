# macOS Frame Guest UI

Status: MA011.08 handoff  
Datum: 2026-07-06

## Ziel

Die macOS-App ist fuer V1 eine minimale Guest Surface. Sie ist kein finales Produktdesign.

## Sichtbare Bereiche

- Ablage-Status: `Ablage macOS`
- Frame-Status: `liegt hier im Frame`
- Verbindung: `verbunden` oder `Verbindung verloren`
- Aktion: `zurueckgeben`
- Sicherheitsstatus: `keine PDF-Datei vorhanden`

## Sichtbare Sprache

Erlaubt:

- Ablage
- Frame
- liegt hier im Frame
- zurueckgeben
- nicht verfuegbar
- Verbindung verloren

Nicht als Primaersprache verwenden:

- Transfer
- Upload
- Download
- Sync
- Server
- Client
- Device
- Agent

## Darstellung

V1 kann ein einzelnes macOS-Fenster nutzen:

```text
+----------------------------------------------------+
| RK Workspace macOS Ablage                          |
| Status: verbunden                                  |
|                                                    |
| [ FramePresenterView: erste PDF-Seite als Frame ]  |
|                                                    |
| keine PDF-Datei vorhanden              zurueckgeben |
+----------------------------------------------------+
```

Spaeter ersetzt die Workspace Shell diese Testoberflaeche durch Overlay-/Surface-Verhalten.
