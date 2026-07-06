# MA016 Windows PDF Lifecycle Checkpoint

Status: implemented for the Windows lab pilot.

## Scope

This checkpoint covers AP461 to AP470:

- Closed PDF Capsule pilot
- Open PDF Frame pilot
- FrameCapsule registry entry
- stale capsule recovery
- CloseReturns and KeepCapsule policy decision
- manual OpenPdfContext parameters
- lifecycle audit events
- visible owner/guest status language

## Pilot Commands

```powershell
.\tools\run-windows-pdf-lifecycle-pilot.ps1 -SmokeTest -ClosedPdf
.\tools\run-windows-pdf-lifecycle-pilot.ps1 -SmokeTest -OpenPdf -Page 1 -Zoom 1.25 -ViewerName 'Windows PDF Viewer'
.\tools\run-windows-pdf-frame-pilot.ps1 -SmokeTest -OpenPdf -UseGlassEdge -PlaySequence
```

## Lifecycle States

The Windows pilot now reports:

- `LifecycleMode: ClosedPdfCapsule`
- `LifecycleMode: OpenPdfFrame`
- `FrameCapsule: OK`
- `CapsuleOpen: OK`
- `CloseFrameBehavior: CloseReturns`
- `CloseFrameBehavior: KeepCapsule`
- `OpenPdfContext: OK`
- `OpenFrame: OK`

## Human Language

Visible pilot language stays in the RK Workspace vocabulary:

- Ablage
- Ding
- Kapsel
- Frame
- zurueckgeben
- Verbindung verloren
- wiederhergestellt

The pilot remains a lab tool. It is not the final product UI.

## Result

Windows PDF Lifecycle Checkpoint: READY
