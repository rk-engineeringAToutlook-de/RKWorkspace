# MA017 Real Test Start Packet

Status: Ready
Datum: 2026-07-07

## Startreihenfolge

1. Windows lokal geschlossene PDF testen.
2. Windows lokal geoeffnete PDF testen.
3. No File Ingress pruefen.
4. Rueckgabe und Recovery pruefen.
5. Glass Edge und naechste Ablage pruefen.
6. UWB/Manual Map/Dongle Simulation pruefen.
7. macOS-Codex mit Handoff-Paket starten.
8. iPad/iPhone ueber Xcode vorbereiten.

## Befehle

```powershell
.\tools\run-ma017-overall-acceptance.ps1 -SkipHeavy
.\tools\run-ma017-windows-all-pdf-lifecycle.ps1 -SmokeTest
.\tools\run-ma017-uwb-dongle-pilot.ps1 -SmokeTest
.\tools\run-ma017-final-repository-hygiene.ps1 -AllowDirty
```

## Owner-Fragen

- Fuehlt sich das Original weiter beim Owner sicher an?
- Ist auf der Gastablage klar, dass dort keine Datei angekommen ist?
- Ist Rueckgabe/Recovery verstaendlich?
- Zeigt Glass Edge die naechste Ablage nachvollziehbar an?
- Reicht die Handoff-Struktur fuer macOS und iOS/iPadOS?

## Result

```text
MA017RealTestStartPacket: READY
RESULT: SUCCESS
```
