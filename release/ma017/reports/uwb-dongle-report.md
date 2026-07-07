# MA017 UWB/Dongle Pilot Report

Status: generated checkpoint content.
Datum: 2026-07-07

## Scope

AP651-660 aktiviert UWB-/Dongle-Proximity fuer MA017 als Simulation.

## Ergebnisse

- UWB Simulator: enabled in Windows PDF Glass Edge pilot.
- Manual Map + UWB Fusion: verified by pilot smoke.
- Dongle Provider: `SimulatedDongleAnchorProvider`.
- CLI: `tools/run-dongle-sim.ps1`.
- Pilot wrapper: `tools/run-ma017-uwb-dongle-pilot.ps1`.
- Privacy: `PrivacyMode: EphemeralLab`.
- Product rule: exactly one nearest Ablage.

## Evidence Commands

```powershell
.\tools\run-dongle-sim.ps1 -SmokeTest -UseFusion -Profile MovingCloser
.\tools\run-ma017-uwb-dongle-pilot.ps1 -SmokeTest
.\tools\run-ma017-smoke.ps1 -SkipHeavy
```

## Known Limits

- no real UWB hardware
- no BLE scan
- no USB dongle control channel
- no Android/iPhone native UWB runtime yet

## Decision

UWB/Dongle pilot simulation is ready for MA017 continuation.
