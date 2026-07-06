# iOS and iPadOS START HERE - MA016

Status: handoff for Xcode execution through macOS.

## Purpose

Prepare iPhone and iPad as mobile RK Workspace Ablagen.

The first app shows FrameCapsules and OpenFrames. It must not store the original PDF as a free file.

## Read First

1. `Docs/Platform/iOS_iPadOS_SurfaceStarterKit.md`
2. `Docs/Platform/iOS_NoFileIngressSandboxChecklist.md`
3. `Docs/Platform/iOS_HapticsAndGesturePrototype.md`
4. `release/ma016/config/ios-guest.sample.json`
5. `release/ma016/schemas/ios-capsule-contract.v0.1.json`
6. `release/ma016/schemas/ios-openframe-contract.v0.1.json`
7. `release/ma016/schemas/ios-no-file-ingress-sandbox-contract.v0.1.json`
8. `Docs/Testing/MA016_iOS_USBInstallRunbook.md`

## First Pilot Goal

- Install the app through Xcode over USB.
- Show a Windows-owned PDF as FrameCapsule or OpenFrame.
- Use haptics only as subtle human feedback.
- Prove No File Ingress in Documents, Caches, tmp, Files export, and shared containers.
- Return the frame to the owner.

## Visible Language

Use: Ablage, Ding, Kapsel, Frame, liegt hier, zurueckgeben, wiederhergestellt.

Avoid: Transfer, Upload, Download, Sync, Device, Agent, Endpoint, Server, Client.
