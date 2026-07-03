# RK Workspace Spatial Carry Tray

Lokaler MA006.03-Web-Prototyp fuer Handy oder Tablet.

Start:

```powershell
.\tools\run-spatial-tray.ps1
```

Smoke-Test:

```powershell
.\tools\run-spatial-tray.ps1 -SmokeTest
```

Der Prototyp enthaelt keine Discovery, kein Pairing, keine Cloud und keine echte Payload-Uebertragung. Die URL wird manuell auf dem Handy oder Tablet geoeffnet.

MA006.03-A ergaenzt:

- digitale Hand mit teilweiser optischer Ueberdeckung
- reduzierte, ruhige Bewegung statt starkem Wabern
- weiches Annaehern an Ablagen
- Ablage-Bubbles mit Distanz-Lesbarkeit
- freies Ablegen beim normalen Loslassen
- explizites `Zuruecklegen` als Abbruch

MA006.04 ergaenzt die Spatial Room Session:

- `/surface/handy`
- `/surface/monitor`
- gemeinsamer Raumzustand fuer alle Ablagen
- ein Ding existiert nur einmal
- Zielablagen sehen eine Preview
- Ding kann von jeder Ablage wieder genommen werden
