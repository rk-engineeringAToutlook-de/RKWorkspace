# MA016 macOS First Pilot Runbook

## Goal

Run the first Windows-owner to macOS-guest PDF Frame pilot.

## Prerequisites

- Windows repo on branch `feature/ma016-ma017-real-platform-pilot`.
- Windows smoke passes: `.\tools\run-ma016-smoke.ps1 -SkipHeavy`.
- macOS Xcode project follows `release/ma016/handoff/macOS_START_HERE.md`.
- macOS config is based on `release/ma016/config/macos-guest.sample.json`.

## Windows Commands

```powershell
.\tools\run-windows-pdf-lifecycle-pilot.ps1 -SmokeTest -ClosedPdf
.\tools\run-windows-pdf-lifecycle-pilot.ps1 -SmokeTest -OpenPdf
.\tools\run-windows-owner-for-mac.ps1 -InfoOnly
```

## macOS Commands

```bash
swift test
open RKWorkspaceMacGuest.xcodeproj
```

Run the app from Xcode and verify the smoke output:

```text
macOSGuestAblage: STARTED
FrameCapsule: OK
OpenFrame: OK
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
GuestHasCopiedPdfBytes: NO
Return: SUCCESS
Recovery: SUCCESS
RESULT: SUCCESS
```

## No File Ingress Manual Checks

```bash
find "$HOME/Library/Containers" -iname "*.pdf"
find "$TMPDIR" -iname "*.pdf"
find "$HOME/Downloads" -iname "*.pdf"
```

No new PDF created by RK Workspace is allowed.

## Pass Criteria

- macOS appears as Ablage.
- Windows remains owner.
- Closed PDF Capsule is visible.
- OpenFrame is visible.
- Return works.
- Recovery works.
- No File Ingress is proven.

## Blocker Handling

If native transport is unavailable, use local message replay from the Apple RKWP bundle and record the blocker in the pilot log.
