# RK Workspace - macOS Codex Handoff

# MA017 Real macOS PDF Frame Guest

## Status

This handoff is for a separate Codex instance running on macOS.

The goal is not a browser prototype, not Spatial Tray, and not the old tablet/monitor web surface.

The goal is:

```text
Windows owns the real PDF.
macOS shows only a rendered Frame.
macOS must not receive, store, copy, or expose the original PDF file.
```

## Coordination Rule

Windows Codex and macOS Codex exchange project state only through GitHub.

Read this before making changes:

```text
release/ma017/handoff/CODEX_GITHUB_COORDINATION_RULES.md
```

Do not rely on HiDrive file sync, copied files, or uncommitted working-tree changes as shared state.

## Repository

GitHub:

```text
https://github.com/rk-engineeringAToutlook-de/RKWorkspace.git
```

Branch:

```text
feature/ma016-ma017-real-platform-pilot
```

Important paths:

```text
release/ma017/handoff/CODEX_GITHUB_COORDINATION_RULES.md
release/ma017/packages/macos-frame-guest/
release/ma017/packages/macos-frame-guest/README.md
release/ma017/packages/macos-frame-guest/Package.swift
release/ma017/packages/macos-frame-guest/Sources/MacPdfFrameGuest/MacPdfFrameGuestApp.swift
release/ma017/handoff/macOS_START_HERE.md
release/ma017/handoff/macOS_MinimalGuestAppTask.md
release/ma017/packages/apple-rkwp-swift-bundle/
release/ma017/config/macos-guest.sample.json
```

Windows owner paths:

```text
tools/run-macos-pdf-frame-owner.ps1
src/Tools/RKWorkspace.MacPdfFrameOwnerHost/
samples/Objects/Rechnung.pdf
```

## What Already Exists

Windows now has a first real owner host:

- renders `samples/Objects/Rechnung.pdf` owner-side
- uses `PopplerPdfFrameRenderer`
- exposes only a PNG frame over RKWP Dev-LAN
- does not send the PDF file
- does not send original PDF bytes
- does not send the original file path

Verified on Windows:

```text
Build: 0 warnings, 0 errors
RendererName: PopplerPdfFrameRenderer
FrameFormat: PngFrame
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
DevLanFrameUpdate: SUCCESS
```

macOS has a first SwiftUI package skeleton:

```text
release/ma017/packages/macos-frame-guest/
```

This package is intentionally minimal. The macOS Codex instance must compile it on macOS, fix any macOS-specific issues, and make it visibly show the frame.

## Windows Owner Must Be Running

On Windows, keep this running:

```powershell
cd "E:\HiDrive\users\RK Workspace\RKWorkspace"
.\tools\run-macos-pdf-frame-owner.ps1 -BindAddress 0.0.0.0 -Port 57120
```

The likely Windows WLAN address is:

```text
192.168.163.11
```

If the Mac is in a different network, use the Windows address printed by the script.

If Windows Firewall asks, allow access on private networks.

## macOS Human Setup

On the Mac:

1. Open Terminal.
2. Verify Xcode command line tools:

```bash
xcode-select --install
swift --version
```

3. Clone or update the repository:

```bash
git clone https://github.com/rk-engineeringAToutlook-de/RKWorkspace.git
cd RKWorkspace
git checkout feature/ma016-ma017-real-platform-pilot
git pull
```

If the repository is private, authenticate locally through GitHub, Git Credential Manager, GitHub Desktop, or SSH. Do not paste tokens into Codex chat.

4. Test whether the Mac sees the Windows owner:

```bash
nc -vz 192.168.163.11 57120
```

Expected:

```text
succeeded
```

5. Start the macOS guest:

```bash
cd release/ma017/packages/macos-frame-guest
swift run MacPdfFrameGuest --host 192.168.163.11 --port 57120
```

6. In the macOS window, click:

```text
Frame holen
```

Expected:

```text
The visible page of Rechnung.pdf appears in the macOS frame window.
```

## Codex Auftrag For macOS Instance

Paste this task into the macOS Codex instance:

```text
You are working on RK Workspace on macOS.

Goal:
Build and verify the first native macOS PDF Frame Guest for MA017.

Do not build:
- browser UI
- Spatial Tray
- web host
- PWA
- file transfer
- PDF download
- ownership transfer
- Finder file export

Use:
- release/ma017/packages/macos-frame-guest/
- release/ma017/handoff/MACOS_CODEX_REAL_PDF_FRAME_START_HERE.md
- release/ma017/handoff/macOS_START_HERE.md
- release/ma017/handoff/macOS_MinimalGuestAppTask.md
- release/ma017/packages/apple-rkwp-swift-bundle/
- release/ma017/config/macos-guest.sample.json

Windows Owner:
The Windows machine should be running:
.\tools\run-macos-pdf-frame-owner.ps1 -BindAddress 0.0.0.0 -Port 57120

Connect to:
192.168.163.11:57120

Tasks:
1. Build the Swift package:
   cd release/ma017/packages/macos-frame-guest
   swift build
2. Run it:
   swift run MacPdfFrameGuest --host 192.168.163.11 --port 57120
3. Fix any macOS compile/runtime issues.
4. Make the native macOS window visibly show the received PNG frame.
5. Keep the frame memory-only.
6. Verify that the macOS app never writes the original PDF, original bytes, or original path.
7. Add a small local verification path if useful, but do not turn this into a browser or web app.
8. Keep visible wording human:
   allowed: Ablage, Ding, Frame, liegt hier, zurueckgeben, nicht verfuegbar
   forbidden: Transfer, Upload, Download, Sync, Server, Client, Endpoint, Device, Agent
9. Report exact commands, result, and any blockers.

Definition of Done:
macOS window opens.
Frame holen receives a FrameUpdate.
Rechnung.pdf page is visible as image/frame.
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
FrameCache: MemoryOnly
NoFileIngress: SUCCESS
Return button sends CarryLeaseReturn or is clearly prepared.
No browser, no Spatial Tray, no PDF file on macOS.
```

## Acceptance

MA017 real macOS frame pilot is acceptable when:

- Windows owner still has the real PDF.
- macOS shows a visible frame of the PDF.
- macOS does not have the PDF file.
- macOS does not receive the owner path.
- macOS does not receive original PDF bytes.
- user can close or return the frame.
- the app is native macOS, not browser-based.

## Known Constraints

This is still a development pilot:

- no production security
- no automatic discovery
- no final pairing UX
- no code signing finalization
- no App Store packaging
- no final glass edge UX

The current mission is only:

```text
real PDF on Windows
visible frame on macOS
No File Ingress
```
