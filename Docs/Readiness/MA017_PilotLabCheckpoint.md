# MA017 Pilot Lab Checkpoint

Status: AP671-680 ready.
Datum: 2026-07-07

## Umgesetzt

- `run-ma017-pilot-lab.ps1`
- `configure-ma017-pilot.ps1`
- Sample Configs fuer Windows local, Windows-to-Mac, Windows-to-iPad, UWB und Dongle
- `export-ma017-pilot-report.ps1`
- `record-ma017-feedback.ps1`
- `export-ma017-feedback-report.ps1`
- `cleanup-ma017-pilot.ps1`
- `run-ma017-repeatability.ps1`
- `Docs/Testing/MA017_PilotRiskChecklist.md`

## Smoke Commands

```powershell
.\tools\configure-ma017-pilot.ps1 -Preset WindowsToMac -SmokeTest -Force
.\tools\export-ma017-pilot-report.ps1 -SmokeTest
.\tools\record-ma017-feedback.ps1 -SmokeTest -Scenario WindowsToMac -Rating Yellow
.\tools\export-ma017-feedback-report.ps1 -SmokeTest
.\tools\run-ma017-repeatability.ps1 -SmokeTest -Count 2
.\tools\cleanup-ma017-pilot.ps1 -SmokeTest
.\tools\run-ma017-pilot-lab.ps1 -SmokeTest
```

## Akzeptanz

- Pilot Lab kann alle Basisaufgaben orchestrieren.
- Feedback wird lokal gespeichert oder im Smoke temporaer gefuehrt.
- Reports liegen unter `release/ma017/reports`.
- Cleanup arbeitet nur innerhalb des Repositorys.
- Repeatability prueft den Windows PDF Lifecycle mehrfach.

## Ergebnis

MA017 Pilot Lab Orchestration: READY
