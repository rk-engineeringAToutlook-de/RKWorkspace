# MA017 Performance and Stability

Status: Draft
Datum: 2026-07-07

## Zweck

MA017 misst keine Produktperformance. MA017 prueft, ob der Pilotpfad stabil genug fuer Owner-Tests ist.

## Abgedeckte Szenarien

- Closed PDF Capsule.
- Open PDF Frame.
- UWB/Dongle Simulator.
- mehrere Kapseln.
- wiederholte Return/Recovery-Zyklen.
- Frame Cache Leak Guard.
- Log Growth Guard.
- Network Degradation Simulation.

## Gate

Volle Messung:

```powershell
.\tools\run-ma017-performance-stability.ps1 -SmokeTest
```

Schneller Gesamt-Smoke:

```powershell
.\tools\run-ma017-performance-stability.ps1 -SmokeTest -SkipMeasurements
```

## Erfolgskriterien

- Build 0 Warnungen in den gemessenen Build-Pfaden.
- Alle gemessenen Smokes melden `RESULT: SUCCESS`.
- No File Ingress bleibt in jedem PDF-Pfad erhalten.
- Cache bleibt MemoryOnly und wird geschlossen.
- Repeated Return/Recovery bleibt stabil.
- Network Degradation fuehrt zu Recovery, nicht Kontrollverlust.

## Result

```text
MA017PerformanceStability: SUCCESS
RESULT: SUCCESS
```
