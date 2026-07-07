# macOS Minimal Guest App Task - MA017

Status: final
Datum: 2026-07-07

## Mission

Erstelle in Xcode eine minimale native macOS App, die als RK Workspace Guest Ablage laeuft.

Nicht Ziel:

- kein Produkt-UX-Finish.
- keine produktive Security.
- keine Dateiuebernahme.
- keine globale macOS Overlay-Integration.

## Muss-Funktionen

- lokale Ablage-Identitaet aus `macos-guest.sample.json` laden.
- RKWP Envelope aus dem Swift Bundle decodieren.
- `AblageHello` senden.
- `AblageCapabilities` mit `frameOnly=true` und `noFileIngress=true` melden.
- `FrameCapsule` sichtbar anzeigen.
- `OpenFrame` sichtbar anzeigen.
- `CarryLeaseReturn` senden.
- Revocation anzeigen.
- Recovery anzeigen.
- MemoryOnly Frame Cache verwenden.
- NoFileIngress als pruefbare Assertions ausgeben.

## Sichtbare Sprache

Erlaubt:

- Ablage
- Ding
- Kapsel
- Frame
- liegt hier
- zurueckgeben
- Verbindung verloren
- wiederhergestellt
- nicht verfuegbar

Verboten:

- Transfer
- Upload
- Download
- Sync
- Server
- Client
- Endpoint
- Device
- Agent

## Required Smoke Output

~~~text
macOSGuestAblage: STARTED
FrameCapsule: OK
OpenFrame: OK
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
GuestHasCopiedPdfBytes: NO
FrameCache: MemoryOnly
Return: SUCCESS
Recovery: SUCCESS
RESULT: SUCCESS
~~~
