# macOS Frame Guest UI Spec

Status: MA013.04 handoff detail  
Datum: 2026-07-06

## Purpose

Build the first native macOS Frame Guest UI for RKWP. It is a lab surface, not the final Workspace Shell.

## Screen

```text
+--------------------------------------------------+
| RK Workspace Ablage                              |
| liegt hier im Frame                              |
|                                                  |
| [ Owner-rendered frame image ]                   |
|                                                  |
| keine PDF-Datei vorhanden          zurueckgeben  |
+--------------------------------------------------+
```

## Required Views

- `FramePresenterView`: displays the latest bitmap frame.
- `ConnectionStateView`: shows `Verbindung verloren` or `wiederhergestellt`.
- `ReturnButton`: sends `CarryLeaseReturn`.
- `NoFileIngressDevBadge`: shows `keine PDF-Datei vorhanden` in development builds.
- `DebugPanel`: optional, hidden by default.

## Interaction

1. App starts in idle state.
2. `AblageHello` and capabilities complete.
3. `FrameSessionOpen` creates a visible frame shell.
4. `FrameUpdate` replaces the current image in memory.
5. Return button sends `CarryLeaseReturn`.
6. Revocation closes or disables the frame and clears memory cache.

## Rules

- Do not write a PDF file.
- Do not show download language.
- Do not expose owner file path.
- Do not infer ownership locally.
- Render only owner-provided frame images.
- Clear frame cache on return, revocation, or connection loss beyond recovery window.

## Acceptance

```text
Platform: MacOS
FrameSession: Active
FrameUpdate: OK
VisibleText: liegt hier im Frame
Return: SUCCESS
GuestHasPdfFile: NO
NoFileIngress: SUCCESS
```
