# macOS Frame Rendering Strategy

Status: MA013.07 strategy  
Datum: 2026-07-06

## Ziel

macOS zeigt Owner-gerenderte Frames. macOS rendert nicht das Original-PDF.

## V1 Frame-Format

V1 akzeptiert:

- PNG Frame.
- JPEG Frame.
- Bitmap Frame.

Tile Updates kommen spaeter. Fuer den ersten Test reicht ein kompletter Page Frame.

## Warum kein PDF-Rendering auf macOS

Der Owner bleibt Besitzer des Originals. Wenn macOS selbst das PDF rendert, steigt das Risiko, dass Originalbytes oder Pfade auf macOS materialisiert werden. Der sichere V1-Pfad ist:

```text
Windows Owner rendert Frame
macOS empfaengt FrameUpdate
macOS zeigt Bitmap im Speicher
macOS speichert keine PDF-Datei
```

## Retina und Scaling

- `FrameUpdate` enthaelt Pixelbreite, Pixelhoehe und ScaleFactor.
- macOS passt das Bild an die View-Groesse an.
- Retina-Displays bevorzugen 2x-Frames.
- UI darf zoomen, solange nur Frame-Koordinaten an den Owner gesendet werden.

## Color Management

- V1 nutzt sRGB.
- Der Owner kann spaeter ICC/ColorProfile-Metadaten senden.
- macOS darf keine Originaldatei fuer Farbprofile oeffnen.

## Performance

- letzter Frame im Speicher halten.
- vorherigen Frame nach Replace freigeben.
- grosse Frames spaeter als Tiles.
- kein Disk Cache.

## Security

- `containsOriginalFileBytes=false` ist Pflicht.
- `hasOriginalPath=false` ist Pflicht.
- Cache wird bei Return, Revocation und Recovery-Abbruch geloescht.
