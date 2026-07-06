# iOS/iPadOS Xcode USB Test Plan

Status: Prepared handoff  
Datum: 2026-07-06

## Ziel

macOS-Codex/Xcode soll die mobile Surface App auf einem echten iPhone oder iPad per USB starten koennen.

## Vorbereitung

1. Mac mit Xcode bereitstellen.
2. iPhone oder iPad per USB verbinden.
3. Developer Mode auf dem Geraet aktivieren.
4. Signing/Development Team einrichten.
5. Windows Owner und Mac/iOS-Geraet im selben lokalen Netzwerk betreiben.
6. Local Network Permission bestaetigen.

## Testablauf

1. Windows Owner starten.
2. iOS/iPadOS App auf Geraet starten.
3. AblageIdentity erzeugen.
4. DevTransport Client verbinden.
5. DevPairing vorbereiten.
6. FrameSession empfangen.
7. Frame anzeigen.
8. HapticsPrepared melden.
9. Keine PDF-Datei speichern.
10. Return ausloesen.
11. Logs sichern.

## Erwartete Logs

```text
SurfacePlatform: IOS or IPadOS
SurfaceRole: FrameGuestSurface
AblageHello: OK
FrameSession: Active
HapticsPrepared: OK
GlassEdge: Prepared
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
Return: SUCCESS
```

## Erfolg

Das echte Geraet zeigt einen Frame und beweist, dass keine freie Originaldatei auf dem Geraet entstanden ist.

## Blocker

- Netzwerkfaehiger DevTransport fehlt noch.
- produktive Security fehlt noch.
- native Frame-Darstellung ist noch offen.
