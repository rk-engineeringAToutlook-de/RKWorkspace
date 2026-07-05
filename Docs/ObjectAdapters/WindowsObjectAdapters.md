# Windows Object Adapters

Dokument-ID: RKWS-ADAPTER-WINDOWS-001
Status: Draft
Datum: 2026-07-05

## Ziel

Windows ist der erste aktive Objektquellenpfad fuer echte digitale Dinge.

Die Adapter uebersetzen Windows-Quellen in RK Workspace Objects. Sie uebertragen keine Datei an eine Gastablage und wechseln keinen Besitz.

## Projekt

```text
src/Adapters/RKWorkspace.ObjectAdapter.Windows/
```

Enthalten:

- `IObjectSourceAdapter`
- `IObjectPreviewProvider`
- `IObjectFrameProvider`
- `IObjectPolicyClassifier`
- `ObjectCaptureResult`
- `ObjectCaptureMode`
- `WindowsFileReferenceAdapter`
- `WindowsClipboardTextAdapter`
- `WindowsScreenshotRegionAdapter`
- `WindowsWindowSnapshotAdapter`

## Capture Modes

- FileReference
- ClipboardText
- ClipboardImage
- ScreenshotRegion
- WindowSnapshot
- AppSpecific
- Unknown

## WindowsFileReferenceAdapter

Der erste echte Pfad ist eine PDF-Datei ueber Dateipfad.

Der Adapter prueft:

- Pfad ist vorhanden.
- Datei existiert.
- `.pdf` wird als `ObjectKind.PdfDocument` erkannt.
- OwnerAblage bleibt Owner.
- OriginReference zeigt auf die Windows-Dateireferenz.
- DefaultMode kommt aus `ObjectKindRules`.
- GuestFileCreated bleibt false.

Der Adapter darf nicht:

- PDF auf die Gastablage kopieren.
- PDF-Bytes als freie Datei an die Gastablage geben.
- Ownership aendern.

## Clipboard Text

`WindowsClipboardTextAdapter` ist als sicherer Testpfad vorbereitet. Der Test liest nicht aus dem echten Betriebssystem-Clipboard, sondern nimmt Text als Parameter entgegen.

Ergebnis:

- `ObjectKind.Text`
- `OriginReference = WindowsClipboardText`
- `OwnershipMode.FrameOnly`
- kein Guest File

## Screenshot Region

`WindowsScreenshotRegionAdapter` ist ein Stub.

Er meldet:

- Status Prepared
- `ObjectKind.ScreenshotRegion`
- keine echte Region Capture Pflicht in diesem Slice
- kein Guest File

## Window Snapshot

`WindowsWindowSnapshotAdapter` ist ein Stub.

Er meldet:

- Status Prepared
- `ObjectKind.SettingsWindow`
- `OwnershipMode.InteractiveFrame`
- keine OwnershipTransfer-Unterstuetzung
- kein Guest File

## Security

Alle Adapter erzeugen eine OriginalThing-Referenz. Sie sind Quellen fuer FrameOnly/OriginalOwned-Flows, keine Dateiuebergabe.

No File Ingress bleibt Pflicht:

- `GuestFileCreated = false`
- `GuestHasNoFileIngress = true`
- Default bleibt OriginalOwned Frame

## Offene Punkte

- echte Explorer-Integration
- echtes Clipboard-Lesen mit klarer Zustimmung
- echte Screenshot-Region
- echte Window Snapshot/SettingsWindow-Integration
- Preview-/FrameProvider an echte Renderer koppeln
- Adapter-Policy pro Firmenumgebung
