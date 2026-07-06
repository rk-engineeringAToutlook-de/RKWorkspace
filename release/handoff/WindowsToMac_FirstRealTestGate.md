# Windows to macOS First Real Test Gate

Status: MA013.10 handoff  
Datum: 2026-07-06

## Auftrag

Bereite den ersten echten Windows-zu-macOS-Test vor. Windows bleibt Owner. macOS zeigt nur den Frame.

## Windows

Vorab lokal pruefen:

```powershell
.\tools\run-windows-pdf-owner-securedev.ps1 -SmokeTest
.\tools\run-mac-guest-contract.ps1
```

## macOS

Baue eine native macOS Frame Guest Surface anhand:

- `Docs/Platform/macOS_RepositoryBootstrap.md`
- `Docs/Platform/macOS_FrameGuestUI.md`
- `Docs/Platform/macOS_NoFileIngressChecklist.md`
- `Docs/Platform/macOS_ReturnAndRecovery.md`
- `Docs/Platform/macOS_FrameRenderingStrategy.md`
- `Docs/Platform/macOS_DevAgentPackaging.md`
- `Docs/Platform/macOS_GestureAndGlassEdge.md`
- `contracts/macOS-guest/rkwp-macos-guest-contract-v0.1.json`

## Erfolg

```text
FrameSessionOpen: OK
FrameUpdate: OK
PDF liegt hier im Frame.
NoFileIngress: SUCCESS
Return: SUCCESS
Recovery: SUCCESS
```
