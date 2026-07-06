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
- `WindowsExplorerSelectionAdapter`
- `WindowsClipboardTextAdapter`
- `WindowsClipboardImageAdapter`
- `WindowsScreenshotRegionAdapter`
- `WindowsWindowSnapshotAdapter`
- `WindowsRemoteSessionAdapter`

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

## Clipboard Image

`WindowsClipboardImageAdapter` ist als sicherer Stub vorbereitet. Er erzeugt `ObjectKind.Image`, bleibt `FrameOnly` und fuehrt keinen automatischen Ownership Transfer aus.

Ergebnis:

- `ObjectKind.Image`
- `OriginReference = WindowsClipboardImage`
- `OwnershipMode.FrameOnly`
- kein Guest File

## Screenshot Region

`WindowsScreenshotRegionAdapter` ist ein Stub.

Er meldet:

- Status Prepared
- `ObjectKind.ScreenshotRegion`
- keine echte Region Capture Pflicht in diesem Slice
- kein Guest File

Ab MA013.43 kann eine Region als Metadaten-Spike mit Bounds und SnapshotExport-Policy simuliert werden. Ohne Policy faellt der Default auf FrameOnly zurueck.

## Window Snapshot

`WindowsWindowSnapshotAdapter` ist ein Stub.

Er meldet:

- Status Prepared
- `ObjectKind.SettingsWindow`
- `OwnershipMode.InteractiveFrame`
- keine OwnershipTransfer-Unterstuetzung
- kein Guest File

Ab MA013.44 enthalten Window-Snapshots Titel, Bounds und geplantes Window-Handle als Metadaten. SettingsWindow bleibt nicht transferierbar.

## Explorer Selection

`WindowsExplorerSelectionAdapter` nutzt im Pilot einen stabilen Pfad-Fallback. UIA/Shell-Auswahl ist dokumentiert, aber noch nicht Produktpfad.

## Remote Session

`WindowsRemoteSessionAdapter` erzeugt `ObjectKind.RemoteSession` als FrameOnly. `SessionHandoff` ist nur erlaubt, wenn Policy und Ziel-Capability `SessionHandoff` vorhanden sind.

## Security

Alle Adapter erzeugen eine OriginalThing-Referenz. Sie sind Quellen fuer FrameOnly/OriginalOwned-Flows, keine Dateiuebergabe.

No File Ingress bleibt Pflicht:

- `GuestFileCreated = false`
- `GuestHasNoFileIngress = true`
- Default bleibt OriginalOwned Frame

## Offene Punkte

- echte Explorer-Integration
- echtes Clipboard-Lesen mit klarer Zustimmung
- echtes Clipboard-Image-Capture
- echte Screenshot-Region
- echte Window Snapshot/SettingsWindow-Integration
- RemoteSession-Handoff mit echter Gegenstelle
- Preview-/FrameProvider an echte Renderer koppeln
- Adapter-Policy pro Firmenumgebung
