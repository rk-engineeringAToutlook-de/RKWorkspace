# Edge Selection User Control

Status: Draft  
Datum: 2026-07-06

## Ziel

AP159 bereitet vor, dass der Owner die automatische naechste Ablage uebersteuern kann, ohne die Entfernungsthematik zu entfernen.

## Implementierter Modell-Slice

Der erste Code-Slice liegt in:

```text
src/Shell/RKWorkspace.Shell/Ablage/EdgeSelectionOverride.cs
```

Er enthaelt:

- `EdgeOverrideKind`
- `EdgeOverride`
- `EdgeSelectionDecision`
- `EdgeSelectionOverrideResolver`

## Override-Arten

`None`:

- automatische naechste Ablage gewinnt

`PreferredAblage`:

- vom Owner bevorzugte Ablage gewinnt, solange sie im Snapshot vorhanden und verfuegbar ist

`TemporaryOverride`:

- temporäre Ablage gewinnt nur innerhalb eines Zeitfensters
- danach faellt die Auswahl automatisch zurueck

## Regel

Ein Override darf nur ein Ziel aus dem aktuellen `AblageProximitySnapshot` auswaehlen. Existiert das Ziel nicht oder ist es nicht verfuegbar, gewinnt wieder die automatische Auswahl.

## UX

Sichtbar soll spaeter nicht ein technischer Schalter sein, sondern eine einfache Entscheidung:

```text
Diese Ablage jetzt bevorzugen
```

oder:

```text
Nur diesmal dorthin
```

## Verbindung zur Single Glass Edge

Auch mit Override bleibt genau eine gläserne Kante sichtbar. Der Override aendert nur, welche Ablage hinter dieser Kante liegt.

## Tests

Dieser Block wird ueber Build und Foundation-Tests abgesichert:

```powershell
.\tools\run-rkwp-tests.ps1
.\tools\run-tests.ps1
```

Ein eigener Shell-Proximity-Test wird eingefuehrt, sobald mehrere echte Provider gemeinsam aktiv sind.
