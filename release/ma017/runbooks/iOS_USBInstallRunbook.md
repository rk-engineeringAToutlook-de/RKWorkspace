# iOS USB Install Runbook - MA017

Status: final
Datum: 2026-07-07

## Ziel

Installiere die mobile Guest Ablage ueber Xcode per USB auf iPhone oder iPad.

## Voraussetzungen

- macOS Host mit Xcode.
- iPhone oder iPad per USB verbunden.
- Developer Mode auf dem Geraet aktiv.
- Apple Development Team in Xcode konfiguriert.
- Windows Owner im gleichen Labornetz.

## Schritte

1. Xcode Projekt oeffnen.
2. Physisches iPad oder iPhone auswaehlen.
3. `release/ma017/config/ios-guest.sample.json` als lokale Konfiguration einbinden.
4. Build and Run ausfuehren.
5. App zeigt `Ablage`.
6. Capsule Smoke starten.
7. OpenFrame Smoke starten.
8. Haptics pruefen.
9. Return ausloesen.
10. App Container ueber Xcode Devices and Simulators laden.
11. Documents, Library/Caches, tmp, App Group und Files Export pruefen.

## Erwarteter Output

~~~text
iOSGuestAblage: STARTED
FrameCapsule: OK
OpenFrame: OK
HapticsPrepared: OK
DocumentsOriginalPdf: NO
CachesOriginalPdf: NO
TempOriginalPdf: NO
FilesAppOriginalPdf: NO
GuestHasPdfFile: NO
NoFileIngress: SUCCESS
Return: SUCCESS
Recovery: SUCCESS
RESULT: SUCCESS
~~~
