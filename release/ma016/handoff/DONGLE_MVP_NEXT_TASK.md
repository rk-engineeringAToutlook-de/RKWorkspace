# Dongle MVP Next Task

## Goal

Define and prototype the Ablage Anchor Dongle MVP path.

## Inputs

- `Docs/Hardware/AblageAnchorDongle_MVP.md`
- `Docs/Hardware/DongleFirmwareArchitecture.md`
- `Docs/Hardware/UsbDongleControlProtocol.md`
- `release/ma016/config/dongle-prep.sample.json`

## Required work

- Define minimal BLE/UWB/USB identity.
- Define firmware heartbeat.
- Define local Windows dongle discovery stub.
- Define manufacturing-free lab provisioning.
- Keep all hardware state out of Core runtime.

## Done when

```text
DongleIdentity: OK
AnchorHeartbeat: OK
UwbSimulationFallback: OK
RESULT: SUCCESS
```
