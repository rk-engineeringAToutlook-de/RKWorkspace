# RK Workspace - Real PDF Glass Portal Live Sync

## Status

Immediate coordination task for Windows Codex and macOS Codex.

This file exists because the current pieces are not yet the live test the Owner wants.

## Owner Correction

The Owner does not want:

- the old demo object
- a fake card
- a console-only smoke
- a manual `Frame holen` product path
- a disconnected Windows glass demo
- a disconnected macOS frame viewer

The Owner wants:

```text
Take a real PDF on Windows.
The Windows glass portal edge opens because macOS is the nearest Ablage.
The real PDF is placed into that edge.
macOS shows the corresponding receiving glass edge.
The PDF appears on macOS as a Frame.
The original PDF remains owned by Windows.
No File Ingress remains mandatory.
```

## Real PDF Source

The live test must use an Owner-selected real PDF:

```powershell
.\tools\run-ma017-real-pdf-glass-portal-live.ps1
```

If no `-PdfPath` is supplied, Windows opens a PDF file picker.

Optional explicit path:

```powershell
.\tools\run-ma017-real-pdf-glass-portal-live.ps1 -PdfPath "C:\Users\<owner>\Documents\<pdf>.pdf"
```

`samples/Objects/Rechnung.pdf` is allowed only as smoke-test fallback.

The chosen PDF must be treated as a real source file, not as a hard-coded visual toy.

Allowed for first live slice:

```text
Real PDF source card representing the actual PDF file.
```

Not allowed:

```text
Unbound fake digital thing.
```

## Current Truth

Already available:

- Windows can render the real PDF as a PNG Frame.
- Windows Owner Host can serve that PNG Frame.
- macOS Guest can display that PNG Frame.
- Windows Glass Edge event flow exists as a pilot/smoke.
- Windows Native Glass Overlay exists as a visual overlay.

Not yet wired:

- Windows real PDF pick/drop in the visible glass overlay.
- Windows overlay drop triggering the frame session.
- macOS visible receiving glass edge reacting to the incoming placement.
- One single live flow from pick to macOS appearance.

## Required Architecture For The Live Slice

Use a two-phase handshake:

```text
macOS Guest starts and waits.
Windows Overlay starts and owns real PDF source.
Owner picks PDF on Windows.
Windows nearest-Ablage logic selects macOS.
Windows glass edge opens.
Owner releases PDF into edge.
Windows marks FrameReady.
macOS receives FrameReady/OpenFrame.
macOS receiving edge opens.
macOS displays the PNG Frame.
```

## Windows Codex Tasks

1. Stop using the old unbound demo object for this test.
2. Add or adapt a visible Windows live pilot that uses `samples/Objects/Rechnung.pdf` as the source.
3. The Windows object may be a card, but it must be explicitly bound to the real PDF path.
4. When the object is picked, glass edge activation must be driven by nearest-Ablage/proximity.
5. The selected target must be macOS.
6. On release into the glass edge, mark the frame as ready for macOS.
7. Keep Windows as Original Owner.
8. Send only frame representation to macOS.
9. Do not send original PDF bytes.
10. Do not send original PDF path to macOS.
11. Provide one visible start script for the Owner:

```powershell
.\tools\run-ma017-real-pdf-glass-portal-live.ps1
```

The script must start the Windows visible PDF/Glass overlay and the Windows Owner Host in the correct order.
It must pass the selected PDF path to both the overlay and the owner host.

## macOS Codex Tasks

1. macOS app must be able to start in waiting mode.
2. Waiting mode must not immediately show the PDF frame unless Windows has placed it.
3. macOS must show a receiving glass edge or receiving surface state.
4. When Windows marks the frame ready, macOS opens the receiving edge and displays the frame.
5. The visible macOS wording must remain human:

Allowed:

```text
Ablage
Ding
Frame
liegt hier
zurueckgeben
bereit
```

Forbidden:

```text
Transfer
Upload
Download
Sync
Server
Client
Endpoint
Device
Agent
```

6. macOS must continue to enforce:

```text
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
FrameCache: MemoryOnly
NoFileIngress: SUCCESS
```

7. Provide one visible start command for the Owner:

```bash
swift run MacPdfFrameGuest --host 192.168.163.11 --port 57120 --wait-for-placement
```

## Shared Protocol Rule

The current Dev-LAN Owner Host is request/response. For the live slice, use one of these simple approaches:

Preferred:

```text
macOS long-polls Windows for placement-ready FrameUpdate.
Windows replies only after the Windows glass edge drop.
```

Acceptable for first live slice:

```text
macOS polls Windows every 500 ms for placement state.
Windows replies "not ready" until drop.
After drop, Windows replies with PNG FrameUpdate.
```

Not acceptable:

```text
macOS receives the frame before the Windows PDF is placed.
```

## Live Test Script Contract

Windows:

```powershell
cd "E:\HiDrive\users\RK Workspace\RKWorkspace"
git pull --ff-only
.\tools\run-ma017-real-pdf-glass-portal-live.ps1
```

macOS:

```bash
cd "<repo>"
git pull --ff-only
cd release/ma017/packages/macos-frame-guest
swift run MacPdfFrameGuest --host 192.168.163.11 --port 57120 --wait-for-placement
```

Owner action:

```text
1. See real PDF source on Windows.
2. Pick Rechnung.pdf.
3. Windows glass edge opens toward macOS.
4. Drag/release into edge.
5. macOS receiving glass edge opens.
6. Rechnung.pdf appears on macOS as Frame.
```

## Acceptance Criteria

The live test is accepted only if:

- The visible Windows source is the Owner-selected real PDF.
- The old fake demo object is not shown.
- The Windows glass edge opens when the PDF is picked and macOS is selected as nearest Ablage.
- The macOS app is already waiting before the drop.
- macOS reacts after the Windows drop, not before.
- macOS shows a receiving glass edge or equivalent receiving state.
- macOS displays the PDF frame.
- Windows remains original owner.
- macOS has no PDF file, no original path, no original bytes.
- Return remains available.

## Non-Acceptance

Do not present the following as the requested live test:

- `run-native-glass-overlay.ps1` alone.
- `run-macos-pdf-frame-owner.ps1` alone.
- `swift run MacPdfFrameGuest --auto-open` alone.
- console-only smoke success.
- a disconnected visual demo with `Rechnung.pdf` as plain text.

## Summary

```text
This is not a smoke test anymore.
This is the first real human-visible Windows-to-macOS PDF placement pilot.
```
