# RK Workspace

# Mobile Battery and Performance Plan

Status: Accepted  
Datum: 2026-07-06

## Ziel

iPhone, iPad und Android-Geraete duerfen nicht wie dauerhafte Render-Clients behandelt werden. Mobile Surfaces muessen ruhig, batterieschonend und trotzdem reaktionsfaehig sein.

## Frame Rate

Default ist adaptiv. Bei ruhendem Frame wird stark reduziert. Bei Greifen, Scroll, Zoom oder Return wird kurz erhoeht. Es gibt kein dauerhaftes 60-FPS-Ziel ohne aktive Human Experience.

## Haptics

Haptik wird nur fuer Pick, EdgeNear, EdgeEnter, FrameArrived, Return, Denied und ConnectionLost genutzt. Dauerhafte Vibration ist verboten.

## Network

Mobile nutzt kleine Frame-/Tile-Updates, Heartbeat mit Backoff und klare Reconnect-Zustaende. Schlechte Netze reduzieren Qualitaet, nicht Sicherheit.

## Cache

Mobile Caches sind sessiongebunden. Keine Originaldatei, kein Originalpfad, keine freie PDF-Kopie.

## Background

Im Hintergrund bleiben nur Lease-/Recovery-Signale aktiv, soweit das Betriebssystem es erlaubt. UI-Rendering stoppt.

## iOS

iOS/iPadOS setzt auf strikte Sandbox, TestFlight/Dev-Install, keine freie Dateimaterialisierung und spaeter lokale Netzwerkberechtigung.

## Android

Android setzt auf Foreground-Service nur fuer aktive Sessions, klare Notification-Policy und eigene Sandbox fuer Frame-Repräsentationen.
