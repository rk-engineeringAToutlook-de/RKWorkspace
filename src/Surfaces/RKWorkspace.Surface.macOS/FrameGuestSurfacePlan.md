# macOS Frame Guest Surface Plan

Status: Docs/stub only
Datum: 2026-07-05

## Ziel

macOS zeigt einen RKWP Frame an, ohne die Originaldatei zu uebernehmen.

## Minimaler Ablauf

1. FrameSession empfangen.
2. FrameRepresentation anzeigen.
3. No File Ingress pruefen.
4. Scroll/Zoom nur nach Policy.
5. Annotation spaeter als ChangeSet.
6. Return schliesst den Frame.

## Nicht erlaubt

- PDF-Datei in Downloads, Temp oder App-Container als freie Datei ablegen.
- Originalpfad als lokale Datei behandeln.
- Besitzwechsel ohne OwnershipTransferDecision.

## Erste Renderer-Optionen

- Owner-gerenderte FrameUpdates anzeigen.
- macOS PDFKit nur pruefen, wenn keine Originaldatei auf der Gastablage materialisiert wird.
- Bitmap-/Image-Frame aus sicherem Owner-Pfad bevorzugen.
