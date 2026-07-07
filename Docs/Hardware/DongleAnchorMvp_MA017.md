# MA017 Dongle Anchor MVP

Status: AP656/AP657 planning and simulation.

## Leitsatz

Der Dongle ist kein Transferstick. Der Dongle ist ein Ablage-Anker.

## MVP

Der erste Dongle-MVP liefert nur:

- stabile DongleId
- zugeordnete AblageId
- BLE Presence
- optionale UWB-Ranging-Faehigkeit
- Richtung oder Richtungshinweis
- Distanz
- Confidence
- ProviderStatus
- FirmwareVersion
- USB-Stromversorgung

## Simulation

MA017 fuehrt `SimulatedDongleAnchorProvider` ein. Er liefert:

- `DongleId`
- `PrivacyMode`
- `BleAnchor`
- `UwbAnchor`
- mehrere AnchorReadings
- eine naechste Ablage ueber `NearestAblageSelector`

Smoke:

```powershell
.\tools\run-dongle-sim.ps1 -SmokeTest -UseFusion -Profile MovingCloser
```

## Nicht-Ziele

- keine Dateiablage
- kein Massenspeicher
- kein Copy-Out
- kein Trust-By-Proximity
- kein automatischer Ownership Transfer

## Naechster Hardware-Schritt

Erst nach stabilem MA017-Pilot:

1. Dev-Kit auswaehlen.
2. BLE-Beacon messen.
3. UWB-Ranging messen.
4. Werte in `AblageProximitySnapshot` einspeisen.
5. gegen Manual Map und UWB-Simulation vergleichen.
