# Windows to macOS MA008 Handoff

Status: Prepared
Datum: 2026-07-05

## Aktueller Windows-Stand

Branch:

```text
feature/ma008-rkwp-devtransport-e2e-frame
```

Ausgangscommit fuer diesen Handoff:

```text
7614468 feat(frame): add pdf frame interaction changesets
```

Der finale AP016-Commit steht nach Abschluss dieses Arbeitspakets in:

```powershell
git log --oneline -1
```

## Context Pack

Stabiler Pfad:

```text
release/codex-context/RKWorkspace_Context_latest.zip
```

Das Pack wird mit folgendem Befehl erzeugt:

```powershell
.\tools\export-codex-context.ps1
```

## Benoetigte Projekte

- `src/Protocol/RKWorkspace.Protocol`
- `src/Protocol/RKWorkspace.Protocol/Identity`
- `src/Protocol/RKWorkspace.Protocol/Ownership`
- `src/Frame/RKWorkspace.Frame.Pdf`
- `src/Communication/RKWorkspace.Transport`
- `src/Communication/RKWorkspace.Transport.Dev`
- `src/Tools/RKWorkspace.WindowsLocalFrameE2E`
- `src/Tools/RKWorkspace.FrameGuestSurface`
- `src/Surfaces/RKWorkspace.Surface.Abstractions`
- `src/Surfaces/RKWorkspace.Surface.macOS`

## Benoetigte Dokumente

- `Docs/Codex/CURRENT_CONTEXT.md`
- `Docs/Readiness/WindowsToMac_DevTransportPlan.md`
- `Docs/Readiness/CrossDeviceTestPlan_Windows_macOS_iPad.md`
- `Docs/Codex/PlatformTasks/macOS.md`
- `Docs/Platform/macOS_SurfaceStarterKit.md`
- `Docs/Protocol/RKWP_ProtocolFoundation.md`
- `Docs/Protocol/RKWP_DevTransport.md`
- `Docs/Protocol/RKWP_AblageIdentityAndTrust.md`
- `Docs/Protocol/RKWP_Pairing.md`
- `Docs/Protocol/RKWP_OwnershipAndLease.md`
- `Docs/Protocol/RKWP_FrameSession.md`
- `Docs/Protocol/RKWP_InputChannel.md`
- `Docs/Protocol/RKWP_ChangeSetAndReturn.md`

## Windows Owner starten

Info pruefen:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -InfoOnly
```

Owner im aktuellen lokalen DevTransport-Modus starten:

```powershell
.\tools\run-windows-owner-for-mac.ps1
```

Wichtig: Der aktuelle Transport ist `NamedPipeDev` und damit Windows-lokal. Fuer echten macOS-Zugriff ist ein netzwerkfaehiges DevTransport-Profil noch zu bauen.

## Erwartete Windows-Owner-Ausgabe

```text
RK Workspace Windows Owner for macOS
OwnerAblageId: ablage-windows-owner
ExpectedGuestAblageId: ablage-macos-guest
ExpectedGuestPlatform: macOS
TransportProfile: NamedPipeDev
DevTransportUrl: dev+namedpipe://rkws-windows-owner-macos
PlannedNetworkPort: 43707
PdfPath: samples/Objects/Rechnung.pdf
SecurityStatus: DevMode
NoFileIngress: REQUIRED
```

## Naechster macOS-Auftrag

Baue eine macOS FrameGuestSurface, die sich als Ablage meldet, RKWP FrameUpdates annimmt und eine PDF als Frame anzeigt, ohne Originalpfad, Originalbytes oder freie lokale PDF-Datei zu materialisieren.

Pflicht:

- AblageIdentity senden.
- DevPairing anfordern.
- FrameOnly akzeptieren.
- No File Ingress beweisen.
- Heartbeat senden.
- Return senden.
- ChangeSet nur als Vorschlag senden.

Nicht bauen:

- keine Dateiuebertragung als Erfolgspfad
- kein Ownership Transfer als Default
- kein Speichern der Owner-PDF auf macOS

## Testablauf

1. Windows Context Pack erzeugen.
2. Windows Owner starten.
3. macOS Guest startet FrameGuestSurface.
4. macOS sendet `AblageHello`.
5. Windows prueft Identity/Pairing.
6. Windows sendet CarryLease und FrameUpdate.
7. macOS zeigt Frame.
8. macOS loggt No File Ingress.
9. macOS sendet Heartbeat.
10. macOS sendet Return.
11. Windows bestaetigt Rueckgabe oder Recovery.

## No File Ingress Kriterien

PASS nur wenn:

- `GuestHasPdfFile: NO`
- `GuestHasOriginalPath: NO`
- `OriginalFileBytes: NO`
- `NoFileIngress: SUCCESS`
- Windows Owner bleibt Owner.

FAIL wenn:

- macOS eine PDF-Datei schreibt.
- macOS den Windows-Pfad erhaelt.
- macOS Originalbytes als freie Datei materialisiert.
- Ownership ohne explizite Policy wechselt.

## Offene Blocker

- macOS native Surface fehlt.
- Cross-Device DevTransport fehlt.
- produktive Session Protection fehlt.
- macOS Berechtigungen und Firewall muessen auf echter Hardware geprueft werden.
