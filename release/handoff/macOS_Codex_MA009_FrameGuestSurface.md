# macOS Codex Auftrag: MA009 Frame Guest Surface

Status: Prepared, updated by MA011.08  
Datum: 2026-07-06

## Kopierbarer Auftrag

```text
Baue eine macOS RK Workspace Frame Guest Surface.

Ziel:

Windows bleibt Owner einer echten PDF. macOS wird Guest-Ablage und zeigt nur einen RKWP Frame. macOS darf keine freie PDF-Datei, keinen Windows-Originalpfad und keine Originalbytes als Datei speichern.

Arbeite auf dem aktuellen Branch:

feature/ma009-secure-cross-device-frame-foundation

Nutze das Context Pack:

release/codex-context/RKWorkspace_Context_latest.zip

Lies zuerst:

1. Docs/Codex/CURRENT_CONTEXT.md
2. release/handoff/WindowsToMac_MA009_Handoff.md
3. Docs/Platform/macOS_SurfaceStarterKit.md
4. src/Surfaces/RKWorkspace.Surface.macOS/README.md
5. src/Surfaces/RKWorkspace.Surface.macOS/macOS_FrameGuestSurface_Design.md
6. src/Surfaces/RKWorkspace.Surface.macOS/macOS_Permissions_Checklist.md
7. src/Surfaces/RKWorkspace.Surface.macOS/macOS_Build_Notes.md
8. Docs/Protocol/RKWP_ProtocolFoundation.md
9. Docs/Protocol/RKWP_SecureSession.md
10. Docs/Protocol/RKWP_AblageIdentityAndTrust.md
11. Docs/Protocol/RKWP_OwnershipAndLease.md
12. Docs/Protocol/RKWP_FrameSession.md
13. Docs/Protocol/RKWP_InputChannel.md
14. Docs/Protocol/RKWP_ChangeSetAndReturn.md
15. src/Surfaces/RKWorkspace.Surface.Abstractions

Aufgaben:

1. GitHub-Repo oeffnen oder aktuellen Branch holen.
2. Context Pack lesen.
3. RKWP Protocol lesen.
4. Surface Abstractions lesen.
5. macOS Berechtigungen pruefen.
6. Minimalen macOS Host / App / Agent anlegen.
7. RKWP DevTransport Client vorbereiten.
8. AblageIdentity erzeugen.
9. DevPairing mit Windows Owner vorbereiten.
10. FrameSession empfangen.
11. PDF Frame anzeigen.
12. Keine PDF-Datei speichern.
13. No File Ingress pruefen.
14. Frame zurueckgeben.
15. Logs schreiben.
16. Build/Test berichten.

Sichtbare Benutzersprache:

- Ablage
- Ding
- Frame
- liegt hier im Frame
- zurueckgeben
- nicht verfuegbar

Nicht als sichtbare Primaersprache verwenden:

- Transfer
- Upload
- Download
- Sync
- Server
- Client
- Device
- Agent

Erfolg:

macOS zeigt einen Frame und meldet:

GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
Return: SUCCESS

Offene Blocker ehrlich melden:

- falls Netzwerk-DevTransport fehlt
- falls PDF-Renderer nur Mock ist
- falls macOS Permission fehlt
- falls echte Windows-Verbindung noch nicht moeglich ist
```

## Windows Owner Befehl

```powershell
.\tools\run-windows-owner-for-mac.ps1 -SmokeTest
.\tools\run-windows-owner-for-mac.ps1
```

## Erwarteter Flow

1. Windows Owner startet.
2. macOS Guest verbindet.
3. macOS sendet `AblageHello`.
4. Windows prueft Identity und Pairing.
5. Windows erstellt CarryLease und FrameSession.
6. macOS zeigt Frame.
7. macOS sendet Heartbeat.
8. macOS sendet Return.
9. Windows schliesst Frame oder Recovery.

## Blocker Aus Windows-Sicht

- Netzwerkfaehiger DevTransport fehlt noch.
- macOS App fehlt noch.
- produktive Security fehlt noch.
- PDF Renderer ist noch nicht final.

## MA011.08 Konkretes Build-Layout

Dieser Handoff wurde durch MA011.08 konkretisiert. macOS-Codex soll zusaetzlich zu den oben genannten Grundlagen diese Dateien lesen:

1. `src/Surfaces/RKWorkspace.Surface.macOS/macos-project-layout.md`
2. `src/Surfaces/RKWorkspace.Surface.macOS/macos-rkwp-client-flow.md`
3. `src/Surfaces/RKWorkspace.Surface.macOS/macos-frame-guest-ui.md`
4. `src/Surfaces/RKWorkspace.Surface.macOS/macos-permissions-checklist.md`
5. `src/Surfaces/RKWorkspace.Surface.macOS/macos-build-commands.md`
6. `src/Surfaces/RKWorkspace.Surface.macOS/macos-test-plan.md`

Bevorzugter erster Build ist eine native Swift/Xcode-App unter:

```text
apps/macos/RKWorkspaceMacGuest/
```

Die App soll minimal bleiben:

- `AblageIdentity` lokal laden oder erzeugen.
- RKWP DevLan/SecureDev Client verbinden.
- `AblageHello` senden.
- `FrameSession` empfangen.
- PDF-Frame anzeigen.
- keine PDF-Datei, keinen Originalpfad und keine Originalbytes als Datei speichern.
- `Return` senden.
- Heartbeat senden.
- Logs lokal schreiben.

Option B ist .NET MAUI oder Avalonia, falls macOS-Codex damit schneller einen stabilen Minimal-Host bauen kann. Die bevorzugte fachliche Referenz bleibt aber die native Swift/Xcode-Struktur, weil sie Berechtigungen, Sandbox und spaetere macOS-Integration am direktesten abbildet.

Windows-Verifikation fuer diesen Handoff:

```powershell
.\tools\export-codex-context.ps1
.\tools\run-tests.ps1
```

Blocker bleiben ehrlich zu melden:

- falls echter Netzwerk-DevTransport auf macOS noch fehlt.
- falls PDF-Anzeige nur Mock ist.
- falls macOS-Permissions oder Signing lokal fehlen.
- falls Windows Owner und macOS Guest noch nicht produktiv gekoppelt sind.
