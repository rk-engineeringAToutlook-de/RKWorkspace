# MA016 Security/Policy Checkpoint

## Status

AP521-530 harden the Windows-led cross-device pilot with explicit security and policy regressions. Native macOS/iPad execution remains a platform handoff, but the owner-side contracts, warnings and reports are now testable on Windows.

## Implemented

- Cross-device security regression script.
- Cross-device policy regression script.
- Cross-device audit report export.
- Pilot warnings for DevMode and non-production security.
- Owner PDF safety guard for private paths, network paths, large PDFs and DevMode.
- macOS and iOS/iPadOS No File Ingress report templates.
- Security owner checklist.
- `CriticalInfrastructure` and `TrustedPersonalDevices` pilot modes verified through `-Policy`.

## Required proof lines

```text
CrossDeviceSecurityRegression: SUCCESS
CrossDevicePolicyRegression: SUCCESS
CrossDeviceAudit: SUCCESS
NoFileIngress: SUCCESS
UnauthorizedKeepCapsule: DENIED
StaleLeaseRecovery: SUCCESS
```

## Current limitations

- SecureDev is still a development security path, not final production security.
- macOS and iOS/iPadOS native proofs require Xcode-side execution.
- UWB is simulated; hardware UWB remains a later pilot path.

## Decision

The project can continue to the MA016 Pilot Lab block after these regressions are green. No push is performed from this checkpoint.
