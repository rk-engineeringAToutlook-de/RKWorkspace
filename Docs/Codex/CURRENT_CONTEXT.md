# Current Codex Context

Datum: 2026-07-05

## Branch

```text
feature/ma007-00-rkwp-original-owned-frame
```

## Aktueller Auftrag

MA007.00 fuehrt das Original-Owned Frame Protocol ein.

## Implementierte Schichten

- `src/Protocol/RKWorkspace.Protocol`
- `src/Protocol/RKWorkspace.Protocol/Ownership`
- `src/Frame/RKWorkspace.Frame.Pdf`
- `src/Surfaces/RKWorkspace.Surface.Abstractions`
- `tests/Unit/RKWorkspace.Protocol.Tests`
- `src/Tools/RKWorkspace.PdfFrameOwner`
- `src/Tools/RKWorkspace.FrameGuestSurface`

## Semantik

Ein PDF wird nicht auf die Gastablage kopiert. Der Owner erzeugt eine FrameSession. Die Gastablage sieht eine Frame-Repräsentation ohne Originalpfad und ohne Originalbytes.

## Neuer Smoke

```powershell
.\tools\run-rkwp-tests.ps1
.\tools\run-pdf-frame-smoke.ps1
```
