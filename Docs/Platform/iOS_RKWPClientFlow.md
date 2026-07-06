# iOS RKWP Client Flow

Status: MA013.12 specification  
Datum: 2026-07-06

## Ziel

Die iOS/iPadOS-App versteht den RKWP Dev/SecureDev Client Flow, bevor echte native Implementierung startet.

## Flow

1. AblageIdentity erzeugen oder laden.
2. Windows Owner verbinden.
3. `AblageHello` senden.
4. `AblageCapabilities` verarbeiten.
5. DevPairing bestaetigen.
6. SecureDev Session aufbauen.
7. `FrameSessionOpen` empfangen.
8. `FrameUpdate` anzeigen.
9. `HapticHint` optional umsetzen.
10. Heartbeat senden.
11. `CarryLeaseReturn` senden.
12. `CarryLeaseRevoked` verarbeiten.

## Identity

Die mobile Ablage besitzt eine eigene Development Identity. Private Schluessel bleiben im iOS Keychain/geschuetzten lokalen Speicher und werden nie in Git oder Context Packs kopiert.

## FrameOnly

iOS empfaengt nur Frame-Daten. Das Original bleibt auf Windows.

Pflicht:

```text
containsOriginalFileBytes=false
hasOriginalPath=false
noFileIngress=true
```

## Return

Return sendet `CarryLeaseReturn` und leert den MemoryOnly Cache.

## Revocation

Revocation invalidiert den Frame, leert den Cache und zeigt `nicht verfuegbar` oder `Verbindung verloren`.
