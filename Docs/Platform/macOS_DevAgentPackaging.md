# macOS Dev Agent Packaging

Status: MA013.08 plan  
Datum: 2026-07-06

## Ziel

macOS bekommt zuerst eine Development-App. Ein dauerhafter Agent oder LaunchAgent folgt spaeter.

## V1 Empfehlung

V1:

- native Swift/AppKit oder SwiftUI App.
- sichtbares Testfenster fuer Frame Guest Surface.
- Local Network Permission.
- AblageIdentity im Application Support.
- Logs im Application Support.
- Config als lokale JSON/Plist.

Spaeter:

- Menu Bar App.
- LaunchAgent.
- Codesigning.
- Notarization.
- Accessibility.
- Screen Recording.

## App vs Agent

| Variante | Zweck | Status |
| --- | --- | --- |
| App | erster sichtbarer macOS-Test | bevorzugt |
| Menu Bar App | dauerhafter Dev-Betrieb | spaeter |
| LaunchAgent | Hintergrundstart | spaeter |

## Berechtigungen

Pflicht fuer V1:

- Local Network.

Noch nicht Pflicht:

- Accessibility.
- Screen Recording.
- Automation.

## Identity Store

Pfad-Vorschlag:

```text
~/Library/Application Support/RKWorkspace/identity/
```

Der private Development-Key darf nie ins Repository und nie in Context Packs.

## Logs

Pfad-Vorschlag:

```text
~/Library/Application Support/RKWorkspace/logs/
```

Keine Originalpfade und keine Originalbytes loggen.
