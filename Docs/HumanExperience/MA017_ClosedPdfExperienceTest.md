# MA017 Closed PDF Experience Test

Status: Draft
Datum: 2026-07-07

## Zweck

Dieser Test prueft die Human Experience fuer eine geschlossene PDF-Kapsel im MA017-Pilotpfad.

Die geschlossene PDF darf nicht wie Dateiuebertragung wirken. Sie muss wie ein geschuetztes digitales Ding wirken, das an eine andere Ablage gegeben werden kann, ohne das Original aus der Owner-Ablage zu verlieren.

## HX-Bezug

- HX-000: Ich bin in meinem Arbeitsraum.
- HX-001: Das gehoert zu meiner Arbeit.
- HX-001A: Das Objekt antwortet mir.
- HX-002: Ich habe etwas in meiner Hand.

## Sollgefuehl

```text
Ich gebe ein geschuetztes Ding weiter. Ich sende keine Datei.
```

## Testablauf

1. Owner nimmt eine geschlossene PDF-Kapsel auf Windows.
2. Glass Edge zeigt genau die naechste Ablage.
3. Owner legt die Kapsel in die Kante.
4. Die Zielablage zeigt nur die Kapsel, nicht die Originaldatei.
5. No File Ingress bleibt sichtbar und pruefbar.

## Beobachtung

Gruen:

- Owner sagt: Das Original bleibt bei mir.
- Owner nimmt die Kapsel als geschuetztes Objekt wahr.
- Owner denkt nicht an Kopieren, Senden oder Sync.

Gelb:

- Owner versteht den Schutz, muss aber nachfragen.
- Die Kapsel wirkt noch zu technisch.
- Der Rueckweg ist logisch, aber nicht gefuehlt.

Rot:

- Owner glaubt, dass die PDF-Datei auf dem Zielgeraet liegt.
- Owner nennt den Vorgang Dateiuebertragung.
- Owner verliert Vertrauen in No File Ingress.

## Mindestnachweis

Der Test ist bereit, wenn Pilot-Script, Policy-Check und Owner-Fragen denselben Zustand zeigen:

```text
ClosedPdf: Protected
OriginalOwnership: Owner
GuestIngress: None
RESULT: SUCCESS
```
