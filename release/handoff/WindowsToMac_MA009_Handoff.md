# Windows to macOS MA009 Handoff

Status: Prepared  
Datum: 2026-07-06

## Aktueller Windows-Stand

Branch:

```text
feature/ma009-secure-cross-device-frame-foundation
```

Ausgangscommit fuer diesen Handoff:

```text
8d93a51 feat(protocol): add secure session spike
```

Der finale AP022-Commit steht nach Abschluss dieses Arbeitspakets in:

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

## Windows Owner Start

Info:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -InfoOnly
```

Smoke:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -SmokeTest
```

Owner im aktuellen DevLan-Modus:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -PdfPath samples/Objects/Rechnung.pdf -Port 57100 -AllowDevPairing
```

## Erwartete Windows-Owner-Ausgabe

```text
RK Workspace Windows Owner for macOS
Windows Owner AblageId: ablage-windows-owner
ExpectedGuestAblageId: ablage-macos-guest
ExpectedGuestPlatform: macOS
TransportProfile: LocalNetworkDev
DevLanUrl: rkwp+tcp-dev://<windows-host>:57100
Port: 57100
SecurityMode: DevelopmentInsecure
PdfPath: samples/Objects/Rechnung.pdf
PdfObjectId: pdf-...
LeaseMode: FrameOnly
NoFileIngress: REQUIRED
ExpectedGuest: macOS
```

## RKWP Message Flow

Der erste Windows-to-macOS-Flow bleibt FrameOnly:

1. `AblageHello`
2. `AblageCapabilities`
3. DevPairing / Trust
4. `CarryLeaseRequested`
5. `CarryLeaseGranted`
6. `FrameSessionOpen`
7. `FrameSessionReady`
8. `FrameUpdate`
9. `CarryLeaseHeartbeat`
10. `FrameClose` / `CarryLeaseReturn`

## macOS Guest Aufgabe

Baue eine macOS FrameGuestSurface, die:

- eine eigene AblageIdentity erzeugt.
- DevPairing mit Windows anfordert.
- RKWP DevTransport Client nutzt.
- FrameOnly akzeptiert.
- den PDF Frame anzeigt.
- keine PDF-Datei speichert.
- keinen Windows-Originalpfad speichert.
- keine Originalbytes als freie Datei materialisiert.
- Heartbeat sendet.
- Return sendet.
- Recovery sichtbar loggt.

## No File Ingress Kriterien

PASS nur wenn macOS meldet:

```text
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
```

FAIL wenn macOS eine freie PDF-Datei schreibt, den Windows-Pfad erhaelt oder Ownership ohne Policy wechselt.

## Rueckgabe-Kriterien

- macOS sendet Return.
- Windows schliesst CarryLease und FrameSession.
- Owner ist wieder verfuegbar.
- Audit enthaelt FrameReturned.

## Recovery-Test

- macOS Heartbeat stoppen.
- Windows erkennt Heartbeat-Verlust.
- Owner fuehrt Recovery aus.
- Guest Frame gilt als ungueltig.
- Audit enthaelt RecoveredByOwner.

## macOS Berechtigungen

Fuer den ersten nativen macOS-Test pruefen:

- App Sandbox: Network Client.
- Network Server nur falls macOS spaeter Host sein soll.
- Local Network / Firewall Prompt dokumentieren.
- Accessibility spaeter fuer globale Gesten.
- Screen Recording spaeter nur fuer echte Objekt-/Fenstererkennung.
- keine Dateiablage fuer FrameOnly.

## Build-Hinweise

macOS-Codex soll lesen:

- `Docs/Codex/PlatformTasks/macOS.md`
- `Docs/Platform/macOS_SurfaceStarterKit.md`
- `Docs/Protocol/RKWP_SecureSession.md`
- `Docs/Protocol/RKWP_DevTransport.md`
- `Docs/Protocol/RKWP_FrameSession.md`
- `Docs/Protocol/RKWP_OwnershipAndLease.md`
- `Docs/Protocol/RKWP_AblageIdentityAndTrust.md`
- `Docs/Readiness/WindowsToMac_DevTransportPlan.md`

## Offene Blocker

- echtes netzwerkfaehiges DevTransport-Profil ist als DevLan-Lab-Profil vorbereitet.
- macOS native FrameGuestSurface fehlt.
- produktive TLS/mutual auth fehlt.
- PDF-Renderer ist noch nicht final.
- Firewall und Berechtigungen muessen auf echter Hardware getestet werden.
