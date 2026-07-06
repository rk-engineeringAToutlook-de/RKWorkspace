# UWB Hardware Next Task

## Goal

Move from UWB simulation to first hardware-backed proximity validation.

## Inputs

- `Docs/Proximity/UwbRequirements.md`
- `Docs/Proximity/UWB_PrivacyConsent.md`
- `Docs/Hardware/AblageAnchorDongle_MVP.md`
- `src/Shell/RKWorkspace.Shell/Ablage/UwbProximityProvider.cs`

## Required work

- Select first UWB hardware candidate.
- Define pairing and consent behavior.
- Map hardware distance/angle to `AblageDistance`.
- Add hardware-provider stub next to simulator.
- Keep simulator as fallback.

## Done when

```text
UwbProviderStatus: HardwareCandidate
NearestAblageSelected: OK
NoFileIngress: SUCCESS
RESULT: SUCCESS
```
