# RKWP Object Kind Rules

Dokument-ID: RKWS-RKWP-OBJECT-KIND-001
Status: Draft
Datum: 2026-07-05

## Zweck

Nicht jedes digitale Ding darf gleich behandelt werden. Ein PDF, ein Screenshot, ein Browser-Tab und ein Einstellungsfenster haben unterschiedliche Ownership- und Frame-Regeln.

## Default-Regeln in MA007.00

| ObjectKind | Default |
| --- | --- |
| PdfDocument | FrameOnly, View/Scroll/Zoom |
| Image | FrameOnly, View/Zoom |
| Text | InteractiveFrame |
| EmailDraft | InteractiveFrame |
| EmailMessage | FrameOnly |
| BrowserTab | SessionHandoff |
| ExplorerFile | FrameOnly |
| ScreenshotRegion | SnapshotExport |
| SettingsWindow | NotTransferable |

## Wichtige Regel

`SettingsWindow` ist nicht uebertragbar. Es darf nicht als Digital Thing materialisiert werden.

## Spaeter

Die Regeln werden spaeter pro Policy, Anwendung, Firmenumgebung und Sicherheitszustand erweitert.

## MA007.05 PDF Frame Viewer

`PdfDocument` bleibt der erste echte ObjectKind-Test. Der aktuelle Frame zeigt eine sichere `MetadataPreview` und bereitet Scroll/Zoom vor. Ein echter PDF-Renderer muss Owner-seitig oder strikt framegebunden arbeiten und darf keine freie PDF-Datei auf der Gastablage materialisieren.

Ab MA010.06 ist der Renderer-Pfad abstrahiert. `PdfDocument` verwendet `IPdfFrameRenderer`. Der aktuelle `MetadataPreviewDevRenderer` ist explizit `IsPlaceholder: true` und bleibt FrameOnly. Ein spaeterer echter Renderer darf nur gerenderte Frames liefern, keine Original-PDF und keinen Originalpfad.

Ab MA010.07 darf `PdfDocument` gerenderte Frames temporaer cachen, aber nur gemaess `FrameCachePolicy`. Der Cache bleibt `MemoryOnly` als Default und darf nie Original-PDF, Originalbytes oder Originalpfad materialisieren.

## MA007.07 ChangeSet-Regeln

PDF-Annotationen werden als erste ChangeSet-Operation vorbereitet. Die Gastablage erzeugt dabei keine freie PDF-Datei und schreibt nicht direkt in das Original.

MA008.05 macht diese Regel im PDF-Frame konkret: Scroll und Zoom bleiben erlaubte Frame-Eingaben, Annotation erzeugt ein `AnnotationAdded`-ChangeSet. `PdfDocument` bleibt weiterhin `OriginalOwned + FrameOnly`; der Guest bekommt keine PDF-Datei und keinen Originalpfad.

E-Mail-Entwuerfe koennen spaeter adapterbasiert ChangeSets erzeugen. `SettingsWindow` bleibt Sonderfall: Eingaben betreffen das Originalsystem; ein ChangeSet ist dort hoechstens Audit/Protokoll, kein Ownership Transfer.

## MA007.08 Ownership Transfer

Besitzuebernahme ist objektartabhaengig:

- PDF: CopyOut/ForkVersion moeglich, wenn Policy erlaubt; MoveOwnership nur mit starker Bestaetigung.
- SettingsWindow: NotSupported.
- RemoteSession: SessionHandoff nur mit passenden TargetCapabilities.
- SnapshotRegion: SnapshotExport nur nach Policy.

Default bleibt `OriginalOwned + FrameOnly`.

## MA007.09 Windows Object Adapter

Der erste echte Windows-Adapter erkennt PDF-Dateien ueber `WindowsFileReferenceAdapter` als `PdfDocument`. Er erzeugt eine `OriginReference`, bleibt OriginalOwned und erzeugt keine Guest-Datei.

Weitere Windows-Quellen sind vorbereitet:

- ClipboardText als `Text`
- ScreenshotRegion als Stub
- WindowSnapshot/SettingsWindow als Stub

Alle Pfade respektieren No File Ingress.
