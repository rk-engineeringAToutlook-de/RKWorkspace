# Windows Owner Hardening Task

## Goal

Harden the Windows owner path from pilot-grade orchestration toward a reliable real test host.

## Scope

- PDF owner lifecycle.
- FrameCapsule/OpenFrame state.
- Owner safety guard.
- Recovery and return.
- Glass Edge target selection.
- Log redaction for owner paths.

## Required work

- Add stricter owner path redaction in all reports.
- Split dev warnings from user-facing state.
- Add negative tests for blocked local materialization.
- Add owner recovery drill script for repeated stale leases.
- Preserve `CriticalInfrastructure` strict behavior.

## Done when

```text
OwnerPdfSafetyGuard: OK
UnauthorizedCapsuleOpen: DENIED
ExpiredCapsule: RECOVERED_BY_OWNER
NoFileIngress: SUCCESS
RESULT: SUCCESS
```
