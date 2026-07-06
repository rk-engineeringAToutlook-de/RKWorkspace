# MA016 UWB and Proximity Checkpoint

Status: foundation ready.

## Completed Scope

- `IUwbProximityProvider`
- `UwbProviderStatus`
- `SimulatedUwbProximityProvider`
- UWB simulation profiles: static, moving closer, moving away, passing by, noisy signal
- Proximity fusion provider
- Confidence threshold
- Distance hysteresis setting
- provider priority
- Windows PDF Glass Edge pilot switches for UWB simulation and fusion
- iOS UWB/Nearby Interaction handoff
- Android UWB capability handoff
- Dongle UWB/BLE/USB detail
- UWB privacy and consent document

## Smoke Commands

```powershell
.\tools\run-windows-pdf-frame-pilot.ps1 -SmokeTest -UseGlassEdge -UseUwbSim -UwbProfile MovingCloser -PlaySequence
.\tools\run-windows-pdf-frame-pilot.ps1 -SmokeTest -UseGlassEdge -UseProximityFusion -UwbProfile PassingBy -PlaySequence
.\tools\run-rkwp-tests.ps1
```

## Result

UWB/Proximity Foundation: READY
