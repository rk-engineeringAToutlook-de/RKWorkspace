# MA017 Proximity UWB Experience Test

Status: Draft
Datum: 2026-07-07

## Zweck

Dieser Test prueft, ob Proximity, UWB und Dongle-Simulation als Human Experience verstaendlich bleiben.

Der Owner soll nicht technische Entfernungswerte sehen. Er soll erleben, dass RK Workspace die naechste Ablage erkennt.

## HX-Bezug

- HX-000: Ich bin in meinem Arbeitsraum.
- HX-001: Das gehoert zu meiner Arbeit.

## Sollgefuehl

```text
RK Workspace weiss ruhig, welche Ablage als naechstes gemeint ist.
```

## Testablauf

1. Proximity-Fusion ermittelt die naechste Ablage.
2. Glass Edge zeigt nur diese eine Ablage.
3. UWB- oder Dongle-Vertrauen beeinflusst Intensitaet, nicht das mentale Modell.
4. Owner kann die Zielrichtung erkennen, ohne Meterwerte zu lesen.
5. Bei unsicherer Naehe bleibt die UI zurueckhaltend.

## Beobachtung

Gruen:

- Owner sagt: Das ist die naechste Ablage.
- Die Kante fuehlt sich sicher, aber nicht aufdringlich an.
- Unsicherheit fuehrt nicht zu falscher Sicherheit.

Gelb:

- Richtung stimmt, Vertrauen ist aber noch schwer zu fuehlen.
- UI braucht zusaetzliche Beruhigung bei Messrauschen.

Rot:

- Mehrere Kanten wirken gleichzeitig aktiv.
- Owner denkt an Sensoren, Meterwerte oder Kalibrierung.
- Falsche Ablage wird als sicher angeboten.

## Mindestnachweis

```text
NearestAblage: Determined
ProximitySource: Fusion
Confidence: VisibleToSystem
OwnerMentalModel: NearestAblage
RESULT: SUCCESS
```
