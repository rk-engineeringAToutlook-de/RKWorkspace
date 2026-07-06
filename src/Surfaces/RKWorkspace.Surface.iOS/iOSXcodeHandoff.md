# iOS Xcode Handoff

Status: Docs/stub only
Datum: 2026-07-05

## Auftrag fuer macOS-Codex

Baue eine native iOS/iPadOS RK Workspace Surface App in Xcode.

## Muss enthalten

- einfache mobile Ablage
- RKWP Frame Anzeige
- Haptik vorbereitet
- TouchHold oder Drei-Finger-Geste pruefen
- Glass Edge am Rand simulieren
- No File Ingress Log
- keine freie Datei speichern
- RKWP DevLan/DevTransport Client vorbereiten
- Local Network Permission pruefen
- Document Picker, Share Extension und bewusst begrenztes Pasteboard vorbereiten
- keine globale App-Erfassung

## Testziel

Windows besitzt PDF.
iPad/iPhone zeigt Frame.
iPad/iPhone bekommt keine PDF-Datei.
Windows bleibt Owner.
Frame kann zurueckgegeben werden.

## Windows-Kompatibilitaetspruefung

Vor dem nativen Build kann Windows den Vertrag pruefen:

```powershell
.\tools\run-ios-guest-compat.ps1 -SmokeTest
```
