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
