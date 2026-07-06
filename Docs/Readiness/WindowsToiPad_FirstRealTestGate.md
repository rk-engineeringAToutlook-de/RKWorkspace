# Windows to iPad First Real Test Gate

Status: MA013.17 readiness gate  
Datum: 2026-07-06

## Ziel

Der erste Windows-zu-iPad-Test wird vorbereitet.

## Voraussetzungen

- Windows Owner laeuft.
- macOS mit Xcode ist vorhanden.
- iPad ist per USB installierbar.
- iPad und Windows sind im gleichen Netzwerk.
- Local Network Permission ist bestaetigt.

## Ablauf

1. Windows Owner starten.
2. iPad App per Xcode installieren.
3. iPad AblageIdentity erzeugen.
4. DevTransport verbinden.
5. DevPairing bestaetigen.
6. PDF Frame oeffnen.
7. Haptics pruefen.
8. No File Ingress pruefen.
9. Return pruefen.
10. Recovery pruefen.

## Erfolgskriterien

```text
PrimaryPlatform: IPadOS
FrameView: OK
DevPairing: SUCCESS
FrameSessionOpen: OK
FrameUpdate: OK
Haptics: OK
GuestHasPdfFile: NO
NoFileIngress: SUCCESS
Return: SUCCESS
Recovery: SUCCESS
```

## Blocker

- native Xcode-App fehlt noch.
- echte USB-Hardwarepruefung fehlt.
- produktive Crypto fehlt.
- mobile Performance offen.
