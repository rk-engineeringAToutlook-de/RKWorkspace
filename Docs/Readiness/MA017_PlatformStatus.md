# MA017 Platform Status

Status: Draft
Datum: 2026-07-07

## Windows

Status: Ready as Owner Pilot.

Evidence:

- MA016 final verification passed.
- Closed/Open PDF lifecycle works locally.
- Security and policy regressions pass.

Next:

- Owner hardening.
- real platform packaging.

## macOS

Status: Prepared for native execution.

Evidence:

- MA016 handoff is complete.
- native implementation task exists.
- contracts and No File Ingress checklist exist.

Next:

- Xcode project creation.
- Guest Surface first smoke.

## iOS / iPadOS

Status: Prepared for native device execution.

Evidence:

- Xcode/USB runbook exists.
- haptic/gesture plan exists.
- sandbox checklist exists.

Next:

- USB install.
- device frame presenter smoke.

## Android

Status: Surface starter kit prepared, not MA017 first execution target.

## Hardware / UWB / Dongle

Status: Simulated, documented, and smoke-tested.

Evidence:

- UWB Simulator is active in the Windows PDF Glass Edge pilot.
- Manual Map + UWB Fusion is part of the MA017 UWB/Dongle pilot.
- `SimulatedDongleAnchorProvider` exists in the Shell model.
- `run-dongle-sim.ps1` verifies BLE/UWB anchor simulation, confidence, direction and privacy mode.

Next:

- real UWB kit validation.
- dongle MVP task execution.
