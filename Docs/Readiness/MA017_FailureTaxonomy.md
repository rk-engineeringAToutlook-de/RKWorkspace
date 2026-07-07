# MA017 Failure Taxonomy

Status: Draft
Datum: 2026-07-07

## Zweck

Diese Taxonomie klassifiziert Fehler im MA017-Pilot.

## Kategorien

### F1 No File Ingress Violation

Guest besitzt Originaldatei, Originalpfad oder kopierte PDF-Bytes.

Reaktion:

- Sofort No-Go.
- Security-Report aktualisieren.
- Pilot nicht fortsetzen.

### F2 Ownership Confusion

Owner versteht nicht, wo das Original bleibt.

Reaktion:

- Human Experience erneut testen.
- Owner/Guest-Sprache schaerfen.

### F3 Return/Recovery Failure

Rueckgabe oder Recovery funktionieren nicht reproduzierbar.

Reaktion:

- Stale Lease Recovery erneut ausfuehren.
- Emergency Return Report pruefen.

### F4 Proximity Misleading

Falsche Ablage wird als naechste Ablage angeboten.

Reaktion:

- Proximity-Fusion und UWB/Dongle-Confidence pruefen.
- Glass Edge nicht aktivieren, wenn Confidence zu schwach ist.

### F5 Platform Handoff Gap

macOS oder iOS Handoff ist unvollstaendig.

Reaktion:

- Nicht als native Ausfuehrung markieren.
- Xcode-Handoff-Task nachziehen.

### F6 Performance/Stability Regression

Pilotpfad wird instabil, erzeugt Log-Wachstum, Cache-Leak oder Recovery-Verlust.

Reaktion:

- Performance/Stability Checkpoint erneut ausfuehren.

## Result

```text
MA017FailureTaxonomy: READY
RESULT: SUCCESS
```
