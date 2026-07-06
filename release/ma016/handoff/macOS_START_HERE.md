# macOS START HERE - MA016

Status: handoff for macOS Codex/Xcode execution.

## Purpose

Build the first macOS Guest Ablage that can show Windows-owned PDF things as controlled RKWP frames.

The macOS app must not receive, copy, export, or own the original PDF by default.

## Read First

1. `Docs/Platform/macOS_SurfaceStarterKit.md`
2. `Docs/Platform/macOS_NoFileIngressChecklist.md`
3. `Docs/Platform/macOS_ReturnAndRecovery.md`
4. `release/ma016/handoff/macOS_MinimalGuestAppTask.md`
5. `release/ma016/config/macos-guest.sample.json`
6. `release/ma016/schemas/macos-capsule-contract.v0.1.json`
7. `release/ma016/schemas/macos-openframe-contract.v0.1.json`
8. `release/ma016/schemas/macos-no-file-ingress-contract.v0.1.json`

## Product Language

Visible text should use:

- Ablage
- Ding
- Kapsel
- Frame
- liegt hier
- zurueckgeben
- Verbindung verloren
- wiederhergestellt

Visible text must avoid:

- Transfer
- Upload
- Download
- Sync
- Server
- Client
- Endpoint
- Device
- Agent

## First Test Goal

1. Windows owns `Rechnung.pdf`.
2. macOS appears as a Guest Ablage.
3. Windows opens a FrameOnly lease.
4. macOS shows a Capsule or OpenFrame.
5. macOS proves No File Ingress.
6. macOS sends Return.
7. Windows confirms Return or Recovery.

## Security Rule

No File Ingress is mandatory. Any implementation that writes the original PDF to the macOS sandbox, Downloads, tmp, or a local export path fails MA016.

## Build Note

This repository prepares the handoff. Xcode execution happens on macOS.
