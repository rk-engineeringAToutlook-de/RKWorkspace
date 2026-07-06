# Proximity Readiness

Status: Draft  
Datum: 2026-07-06

## Umfang

AP160 fasst die Proximity- und Hardware-Readiness fuer MA013.51 bis MA013.60 zusammen.

Dieser Block bereitet Entfernung, Richtung und Zielauswahl fuer den naechsten realen Cross-Device-Test vor. Er aendert keine RKWP-Besitzlogik und keine Frame-Policy.

## Erledigt

- Manual Map bleibt verbindliche Laborquelle.
- Developer-Studio-UI-Plan fuer Manual Map ist dokumentiert.
- BLE Discovery Spike ist dokumentiert.
- WiFi Presence Provider ist als simulierter Shell-Modell-Slice vorhanden.
- UWB Requirements sind dokumentiert.
- Ablage Anchor Dongle MVP ist dokumentiert.
- Dongle Firmware Architecture ist dokumentiert.
- USB Dongle Control Protocol ist dokumentiert.
- Sensor Fusion Roadmap ist dokumentiert.
- Edge Selection User Control ist als Shell-Modell-Slice vorbereitet.

## Produktregel

Das sichtbare Produkt zeigt weiterhin:

```text
genau eine gläserne Kante zur naechsten sinnvollen Ablage
```

Nicht:

- mehrere Bubbles
- Radar
- Geraeteuebersicht
- technische Discovery-Liste

## Offene Blocker

- echte BLE-Daten fehlen
- echte UWB-Hardware fehlt
- Dongle-Hardware ist noch nicht beschafft
- iOS/Android Hintergrundrechte sind noch nicht validiert
- echte Sensorfusion ist noch nicht implementiert
- Studio-Manual-Map-UI ist geplant, aber noch nicht sichtbar umgesetzt

## Verifikation

Fuer diesen Block gelten:

```powershell
.\tools\run-studio.ps1 -SmokeTest
.\tools\run-rkwp-tests.ps1
.\tools\run-tests.ps1
.\tools\export-codex-context.ps1
```

Erwartung:

- Build erfolgreich
- 0 Warnungen
- 0 Fehler
- Studio Smoke erfolgreich
- RKWP Tests erfolgreich
- Foundation Tests erfolgreich
- Codex Context Export erfolgreich

## Naechster Schritt

Nach diesem Block kann der Cross-Device-Pfad mit besserem Proximity-Kontext weitergefuehrt werden:

1. Windows Owner.
2. naechste Ablage ueber Manual Map oder simulierte Presence.
3. eine Glass Edge.
4. RKWP CarryLease.
5. Original-Owned Frame auf macOS/iPad/weiteren Surfaces.
