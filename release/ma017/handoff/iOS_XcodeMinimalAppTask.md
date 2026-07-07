# iOS Xcode Minimal App Task - MA017

Status: final
Datum: 2026-07-07

## Mission

Erstelle in Xcode eine minimale iPad/iPhone App, die als mobile RK Workspace Guest Ablage laeuft.

## Muss-Funktionen

- `RKWPModels.swift` aus dem Apple Bundle einbinden.
- lokale Konfiguration `ios-guest.sample.json` laden.
- `FrameCapsule` als kontrollierte Kapsel zeigen.
- `OpenFrame` als kontrollierten Frame zeigen.
- subtile Haptics fuer Arrival, Open, Return, Recovery und Denied vorbereiten.
- Return ausloesen.
- Recovery sichtbar machen.
- NoFileIngress gegen Documents, Caches, tmp, Files App Export und App Group pruefen.

## Nicht-Ziele

- kein globales App-Capture.
- keine Files-App-Materialisierung.
- kein Share-Sheet-Export der Original-PDF.
- kein Ownership Transfer.
- keine produktive Discovery.

## Required Smoke Output

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
