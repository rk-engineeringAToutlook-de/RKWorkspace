# iOS and iPadOS MA008 Handoff

Status: Prepared
Datum: 2026-07-05

## Aktueller Windows-Stand

Branch:

```text
feature/ma008-rkwp-devtransport-e2e-frame
```

Ausgangscommit fuer diesen Handoff:

```text
6f5b2e0 docs(readiness): prepare windows macos devtransport handoff
```

Der finale AP017-Commit steht nach Abschluss dieses Arbeitspakets in:

```powershell
git log --oneline -1
```

## Context Pack

Stabiler Pfad:

```text
release/codex-context/RKWorkspace_Context_latest.zip
```

Context Pack erzeugen:

```powershell
.\tools\export-codex-context.ps1
```

## Benoetigte Dokumente

- `Docs/Codex/CURRENT_CONTEXT.md`
- `Docs/Readiness/iPad_iPhone_Surface_TestPlan.md`
- `Docs/Readiness/CrossDeviceTestPlan_Windows_macOS_iPad.md`
- `Docs/Codex/PlatformTasks/iOS_iPadOS.md`
- `Docs/Platform/iOS_iPadOS_SurfaceStarterKit.md`
- `Docs/Protocol/RKWP_ProtocolFoundation.md`
- `Docs/Protocol/RKWP_DevTransport.md`
- `Docs/Protocol/RKWP_AblageIdentityAndTrust.md`
- `Docs/Protocol/RKWP_OwnershipAndLease.md`
- `Docs/Protocol/RKWP_FrameSession.md`
- `Docs/Protocol/RKWP_InputChannel.md`
- `Docs/Protocol/RKWP_ChangeSetAndReturn.md`

## Benoetigte Stubs

- `src/Surfaces/RKWorkspace.Surface.iOS/`
- `src/Surfaces/RKWorkspace.Surface.iOS_iPadOS/`
- `src/Surfaces/RKWorkspace.Surface.Abstractions/`

## Naechster macOS-/Xcode-Auftrag

Baue eine native iOS/iPadOS RK Workspace Surface App in Xcode.

Pflicht:

- Xcode-Projekt anlegen.
- Surface App auf iPad/iPhone per USB starten.
- RKWP DevTransport-Client vorbereiten.
- Frame anzeigen.
- Haptik bei Frame-Ankunft vorbereiten.
- TouchHold und Drei-Finger-Geste pruefen.
- Glass Edge am Rand anzeigen.
- No File Ingress loggen.
- keine freie PDF-Datei speichern.

Nicht bauen:

- keine globale App-Erfassung als erste Annahme
- kein Ownership Transfer als Default
- keine Dateiuebertragung als Erfolgspfad
- PWA nicht als Produktpfad behandeln

## Minimaler Test

```text
Windows besitzt PDF.
iPad zeigt PDF-Frame.
iPad bekommt keine PDF-Datei.
Windows bleibt Owner.
iPad gibt zurueck.
```

## Erwartete Logs

```text
SurfacePlatform: IPadOS oder IOS
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

## USB-Test Hinweise

- Mac mit Xcode verwenden.
- iPad/iPhone per USB verbinden.
- Developer Mode auf Geraet aktivieren.
- Signing/Development Team konfigurieren.
- Windows, Mac und iPad/iPhone im selben lokalen Netzwerk betreiben.
- Local Network Permission bestaetigen.
- Keine PDF in Dateien-App, Downloads oder App-Container schreiben.

## Offene Blocker

- Kein Xcode in diesem Windows-Thread.
- Native mobile App fehlt.
- Netzwerkfaehiger DevTransport zu iOS/iPadOS fehlt.
- PDF FramePresenter muss nativ entschieden werden.
- Drei-Finger-Geste kann mit OS-Gesten kollidieren.
