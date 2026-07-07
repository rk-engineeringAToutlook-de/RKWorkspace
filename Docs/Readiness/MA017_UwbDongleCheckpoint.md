# MA017 UWB/Dongle Checkpoint

Status: AP651-660 pilot simulation ready.
Datum: 2026-07-07

## Ziel

Dieser Checkpoint aktiviert die UWB-/Dongle-Linie fuer MA017, ohne echte Hardware vorauszusetzen.

Das sichtbare Produktverhalten bleibt:

- genau eine naechste Ablage
- genau eine glaeserne Kante
- kein Radar
- keine Geraeteliste
- keine Originaldaten im Proximity-Pfad

## Umgesetzt

- UWB Simulator im Pilot-Lab ueber den Windows PDF Glass Edge Pilot aktiviert.
- Manual Map + UWB Fusion erneut als MA017-Pilotpfad abgesichert.
- `SimulatedDongleAnchorProvider` als Shell-Provider eingefuehrt.
- `run-dongle-sim.ps1` als Dongle Anchor CLI eingefuehrt.
- `run-ma017-uwb-dongle-pilot.ps1` als AP651-660 Smoke eingefuehrt.
- UWB Direction/Distance/Confidence im CLI-Ausgabepfad sichtbar.
- iPhone und Android Capability Mapping dokumentiert.
- Dongle MVP fuer BLE/UWB/USB-Anker konkretisiert.
- Proximity Privacy fuer den Pilotstatus dokumentiert.

## Smoke Commands

```powershell
.\tools\run-dongle-sim.ps1 -SmokeTest -UseFusion -Profile MovingCloser
.\tools\run-ma017-uwb-dongle-pilot.ps1 -SmokeTest
.\tools\run-ma017-smoke.ps1 -SkipHeavy
```

## Akzeptanz

Der Block ist bestanden, wenn:

- `SimulatedDongleAnchorProvider` gebaut wird.
- UWB und Dongle jeweils Direction, Distance und Confidence liefern.
- Fusion aus Manual Map und UWB stabil genau eine naechste Ablage waehlt.
- Dongle-Simulation keine Nutzdaten speichert.
- Privacy Mode `EphemeralLab` sichtbar ist.
- MA017 Smoke gruen bleibt.

## Nicht-Ziele

- keine echte UWB-Hardwaremessung
- kein BLE-Scan
- kein USB-Control-Kanal
- keine Personenortung
- keine produktive Pairing-Hardware

## Ergebnis

MA017 UWB/Dongle Pilot Simulation: READY
