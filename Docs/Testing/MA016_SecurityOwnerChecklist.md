# MA016 Security Owner Checklist

## Purpose

This checklist is the owner-facing security gate for MA016 cross-device pilots. It keeps the pilot honest: RK Workspace may show a PDF frame on another Ablage, but it must not silently copy the original file.

## Before a pilot

- Confirm the policy profile:
  - `CriticalInfrastructure` for strict no-keep, no-dev production-like checks.
  - `TrustedPersonalDevices` for personal-device tests where `KeepCapsule` is allowed.
  - `PresentationOnly` for view-only closed capsule checks.
- Run `.\tools\run-cross-device-security-regression.ps1`.
- Run `.\tools\run-cross-device-policy-regression.ps1`.
- Confirm `NoFileIngress: SUCCESS`.
- Confirm no `bin/` or `obj/` artifacts are committed.

## During a pilot

- The guest must show a frame or capsule, not a free PDF file.
- The guest must not show the original owner file path.
- The guest must not receive original PDF bytes.
- The owner must remain able to return or recover the thing.
- DevMode or non-production security warnings must remain visible in logs.

## After a pilot

- Export diagnostics with `.\tools\export-cross-device-diagnostics.ps1 -SmokeTest`.
- Export audit with `.\tools\export-cross-device-audit.ps1 -SmokeTest`.
- Fill the macOS or iOS No File Ingress template when a native run exists.
- Record whether the pilot used Manual Map, UWB simulation or Proximity Fusion.

## Stop conditions

- Any free PDF appears on the guest.
- Any original owner path appears on the guest.
- Any original PDF bytes are cached on the guest.
- `KeepCapsule` is allowed under `CriticalInfrastructure`.
- Stale lease recovery fails.
- Dev/Lab security is used without a visible warning.
