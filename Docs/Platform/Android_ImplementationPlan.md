# Android Implementation Plan

Status: Draft  
Datum: 2026-07-06

## Ziel

AP185 beschreibt den konkreteren Android-Starter.

## Kotlin App

Empfohlener Start:

- Kotlin
- Jetpack Compose fuer Lab UI
- Foreground Service nur nach UX-/Battery-Review
- lokale Dev Config

## RKWP Client

Der Client muss koennen:

- AblageHello
- SecureDev/DevLan spaeter
- CarryLease lesen
- FrameSession anzeigen
- Return/Recovery akzeptieren

## Haptics

Android nutzt `HapticHint` semantisch:

- Pick
- EdgeNear
- EdgeEnter
- FrameArrived
- Return
- Denied
- ConnectionLost

## Frame

Frame Presenter zeigt keine freie Originaldatei. PDF-Frames koennen als Bild-/Tile-Frame oder spaeter Renderer-Komponente erscheinen.

## No File Ingress

Die App darf keine Originaldatei dauerhaft speichern, wenn nur FrameOnly erlaubt ist.

## Permissions

Zu pruefen:

- Network
- Nearby Devices fuer BLE
- Notifications, falls Foreground Service
- Storage nur vermeiden oder streng begrenzen

## Nicht-Ziele

- keine automatische Hintergrund-Discovery im ersten Schritt
- kein Ownership Transfer
- kein Dateimanager-Ersatz
