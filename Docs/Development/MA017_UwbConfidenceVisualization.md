# MA017 UWB Confidence Visualization

Status: Development pilot contract.

## Zweck

AP653 macht im Developer-/Pilotpfad sichtbar, welche Richtung, Distanz und Confidence zur Auswahl der naechsten Ablage gefuehrt haben.

## Sichtbare Signale

Die Pilot-CLI zeigt:

- `NearestAblage`
- `EdgeDirection`
- `DistanceKind`
- `DistanceMeters`
- `Confidence`
- `Source`
- `ProviderStatus`

## Regel

Diese Werte sind Diagnoseinformationen. Im spaeteren Produktpfad sieht der Mensch keine technischen Quellen, sondern nur die eine passende glaeserne Kante.

## Aktueller Smoke

```powershell
.\tools\run-dongle-sim.ps1 -SmokeTest -UseFusion -Profile MovingCloser
```

Erwartete Kernausgabe:

```text
EdgeDirection: Right
Confidence: 0.96
DirectionDistanceConfidence: OK
RESULT: SUCCESS
```

## Grenzen

Die Visualisierung ist aktuell textbasiert. Eine grafische Developer-Studio-Ansicht darf spaeter entstehen, muss aber die Produktregel bewahren: sichtbar ist immer nur eine naechste Ablage.
