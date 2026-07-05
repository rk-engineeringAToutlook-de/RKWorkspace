# Mobile Spatial Surface

Dokument-ID: RKWS-MOBILE-SPATIAL-SURFACE
Version: 1.1.0
Status: Accepted
Datum: 2026-07-04

## Ziel

Mobile Spatial Surface beschreibt den Tablet-/iPhone-Testpfad fuer RK Workspace.

Das mobile Geraet ist keine Upload-App und keine Liste von Zielen. Es ist eine mobile Ablage im Raum.

## Start

```powershell
.\tools\run-mobile-spatial-surface.ps1
```

Smoke-Test:

```powershell
.\tools\run-mobile-spatial-surface.ps1 -SmokeTest
```

MA006.13 ergaenzt den neuen Glass-Edge-Testpfad:

```powershell
.\tools\run-mobile-glass-edge.ps1
.\tools\run-mobile-glass-edge.ps1 -SmokeTest
```

Dieser Pfad zeigt nach einer Geste genau eine gläserne Kante zur naechsten Ablage und bereitet Gegenkante, Ghost und Haptik vor.

Das Script gibt eine URL aus:

```text
http://<ip>:5099/mobile
```

Das Tablet oder iPhone muss im selben WLAN sein.

## Geste

Fuer V1 gilt:

```text
langer Touch
```

Erst danach wird `MobileSpatialMode` aktiv und die Ablage-Linsen erscheinen.

## Distanz

Parameter:

- `MobileLensScaleByDistance`
- `MobileLensOpacityByDistance`
- `MobileLensNameRevealThreshold`
- `MobileLensActivationThreshold`

Weit bedeutet:

- kleine Linse.
- schwache Praesenz.
- Name nicht lesbar.

Nah bedeutet:

- groessere Linse.
- staerkere Praesenz.
- Name lesbar.

Sehr nah bedeutet:

- Linse oeffnet sich.
- `Hier ablegen` wird moeglich.

## Haptik

Der mobile Prototyp versucht `navigator.vibrate`.

Wenn die Plattform das nicht unterstuetzt, ist das kein Fehler. Dann uebernimmt optische Haptik.

## Nicht-Ziele

- keine native Mobile-App.
- keine Discovery.
- kein Pairing.
- keine echte Payload.
- keine Sender-/Empfaenger-Logik.

## Finales Ziel

Final soll das Tablet oder iPhone nicht wie Browser wirken.

Es soll sich anfuehlen wie:

```text
Ich halte eine Ablage in meinem Arbeitsraum.
```
