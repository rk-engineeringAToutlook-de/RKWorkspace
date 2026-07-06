# Object Adapter Readiness

Status: MA013.50  
Datum: 2026-07-06

## Ziel

MA013.50 fasst zusammen, welche Objektquellen fuer den naechsten Pilot bereit sind.

## Bewertung

| Adapter | Status | Naechster Schritt |
| --- | --- | --- |
| PDF | Pilotbereit | Explorer-Auswahl und PDF FrameOnly verbinden |
| Clipboard Text | Vorbereitet | echtes Clipboard nur mit Zustimmung lesen |
| Clipboard Image | Stub vorbereitet | Bitmap-Capture und FrameRenderer anbinden |
| Screenshot Region | Policy-Spike | echte Region Selection/Capture bauen |
| Window Snapshot | Metadaten-Spike | Fensterhandle, Bounds und Capture pruefen |
| SettingsWindow | Policy simuliert | CriticalPolicy/Input-Deny sichtbar halten |
| RemoteSession | Simulation | SessionHandoff nur mit Capability/Policy |
| Browser | Roadmap | URL reference + Frame snapshot starten |
| Email | Roadmap | Outlook/Apple Mail/Webmail separat bewerten |

## Pflichttests

```powershell
.\tools\run-windows-object-adapter.ps1 -SmokeTest
.\tools\run-rkwp-tests.ps1
.\tools\run-tests.ps1
```

## Gate-Ergebnis

Object Adapter sind fuer den naechsten Windows-Pilot ausreichend vorbereitet. Produktreife ist noch nicht erreicht, weil echte Explorer-/Clipboard-/Capture-/Browser-/Mail-Integrationen bewusst erst nach Policy- und UX-Gate erfolgen.
