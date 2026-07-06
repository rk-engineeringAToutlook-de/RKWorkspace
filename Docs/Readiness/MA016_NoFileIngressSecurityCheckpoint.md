# MA016 No File Ingress Security Checkpoint

Status: implemented for the Windows lab pilot.

## Scope

This checkpoint covers AP471 to AP480:

- No File Ingress for FrameCapsule
- No File Ingress for OpenFrame
- MemoryOnly cache for Capsule and OpenFrame
- CriticalInfrastructure policy hardening
- TrustedPersonalDevices policy for Capsule/OpenFrame and KeepCapsule
- unauthorized capsule open denial
- expired capsule owner recovery
- generated No File Ingress report

## Security Rules

The guest ablage receives no standalone PDF file, no original path, and no original PDF bytes.

Allowed guest material is restricted to:

- controlled frame metadata
- rendered frame data
- policy-allowed inputs
- audit-visible lifecycle events

## Policy Summary

- CriticalInfrastructure: Capsule allowed, OpenFrame allowed, KeepCapsule denied.
- OfficeDefault: Capsule allowed, OpenFrame allowed, KeepCapsule denied.
- DevelopmentLab: Capsule allowed, OpenFrame allowed, KeepCapsule allowed for lab testing.
- PresentationOnly: Capsule allowed, OpenFrame denied.
- TrustedPersonalDevices: Capsule allowed, OpenFrame allowed, KeepCapsule allowed with confirmation-bound ownership options.

Unauthorized capsule opening is denied across profiles.

## Evidence Commands

```powershell
.\tools\run-windows-pdf-lifecycle-pilot.ps1 -SmokeTest -ClosedPdf
.\tools\run-windows-pdf-lifecycle-pilot.ps1 -SmokeTest -OpenPdf
.\tools\run-no-file-ingress-report.ps1
.\tools\run-rkwp-tests.ps1
```

Expected indicators:

- `GuestHasPdfFile: NO`
- `GuestHasOriginalPath: NO`
- `GuestHasCopiedPdfBytes: NO`
- `CapsuleNoFileIngress: SUCCESS`
- `OpenFrameNoFileIngress: SUCCESS`
- `CapsuleCache: MemoryOnly`
- `OpenFrameCache: MemoryOnly`
- `UnauthorizedCapsuleOpen: DENIED`
- `ExpiredCapsule: RECOVERED_BY_OWNER`

## Report

The report is generated at:

```text
release/ma016/reports/no-file-ingress-report.md
```

## Result

No File Ingress Security Checkpoint: READY
