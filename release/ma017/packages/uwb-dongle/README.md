# MA017 UWB/Dongle Package

Status: Draft
Datum: 2026-07-07

## Zweck

Dieses Paket bereitet den Proximity-Hardware-Pfad fuer MA017 vor.

UWB und Dongle liefern nur Naehe, Richtung und Confidence. Sie transportieren keine Daten.

## Enthaltene Gates

- `tools/run-dongle-sim.ps1 -SmokeTest -UseFusion -Profile MovingCloser`
- `tools/run-ma017-uwb-dongle-pilot.ps1 -SmokeTest`
- `tools/export-ma017-uwb-privacy-report.ps1 -SmokeTest`

## Nachweise

- `Docs/Hardware/DongleAnchorMvp_MA017.md`
- `Docs/Security/MA017_ProximityPrivacy.md`
- `release/ma017/reports/uwb-dongle-report.md`
- `release/ma017/reports/uwb-privacy-report.md`

## Ergebnis

```text
UwbDonglePackage: READY
PrivacyMode: EphemeralLab
NearestAblage: Determined
RESULT: SUCCESS
```
