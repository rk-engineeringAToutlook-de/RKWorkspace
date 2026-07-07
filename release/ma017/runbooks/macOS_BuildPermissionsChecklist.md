# macOS Build and Permissions Checklist - MA017

Status: final
Datum: 2026-07-07

## Xcode

- SwiftUI App Target angelegt.
- Bundle Identifier lokal eindeutig.
- Signing lokal auf Development.
- macOS Deployment Target dokumentiert.
- `RKWPModels.swift` eingebunden.
- Sample JSONs als Test Fixtures eingebunden.

## Sandbox

- App Sandbox aktiv.
- Kein Downloads-Zugriff fuer den Pilot.
- Kein Documents-Zugriff fuer den Pilot.
- Kein temporarer Export der Original-PDF.
- Keine Security-Scoped Bookmarks fuer Original-PDF.
- Frame Cache nur MemoryOnly.

## Netzwerk/Transport

- DevTransport nur lokal oder explizit konfigurierte Windows Owner Adresse.
- Keine Cloud.
- Kein externer Server.
- Keine automatische Discovery im MA017 Handoff.

## Smoke Output

~~~text
MacGuestIdentity: OK
FrameView: OK
FrameInput: OFF
NoFileIngressCapability: OK
Return: SUCCESS
NoFileIngress: SUCCESS
RESULT: SUCCESS
~~~
