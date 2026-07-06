# iOS/iPadOS Codex Auftrag: MA009 Surface App

Status: Prepared  
Datum: 2026-07-06

## Kopierbarer Auftrag

```text
Baue eine iOS/iPadOS RK Workspace Surface App.

Ziel:

Windows bleibt Owner einer echten PDF. iPhone oder iPad wird mobile Ablage und zeigt nur einen RKWP Frame. iOS/iPadOS darf keine freie PDF-Datei, keinen Windows-Originalpfad und keine Originalbytes als Datei speichern.

Arbeite auf dem aktuellen Branch:

feature/ma009-secure-cross-device-frame-foundation

Nutze das Context Pack:

release/codex-context/RKWorkspace_Context_latest.zip

Lies zuerst:

1. Docs/Codex/CURRENT_CONTEXT.md
2. Docs/Platform/iOS_iPadOS_SurfaceStarterKit.md
3. Docs/Codex/PlatformTasks/iOS_iPadOS.md
4. src/Surfaces/RKWorkspace.Surface.iOS/README.md
5. src/Surfaces/RKWorkspace.Surface.iOS/iOS_SurfaceApp_Design.md
6. src/Surfaces/RKWorkspace.Surface.iOS/iOS_Haptics_Gesture_Plan.md
7. src/Surfaces/RKWorkspace.Surface.iOS/iOS_Xcode_USB_TestPlan.md
8. src/Surfaces/RKWorkspace.Surface.iOS/iOS_Sandbox_ObjectSources.md
9. Docs/Protocol/RKWP_ProtocolFoundation.md
10. Docs/Protocol/RKWP_SecureSession.md
11. Docs/Protocol/RKWP_AblageIdentityAndTrust.md
12. Docs/Protocol/RKWP_OwnershipAndLease.md
13. Docs/Protocol/RKWP_FrameSession.md
14. Docs/Protocol/RKWP_InputChannel.md
15. Docs/Protocol/RKWP_ChangeSetAndReturn.md
16. src/Surfaces/RKWorkspace.Surface.Abstractions

Aufgaben:

1. Xcode-Projekt anlegen.
2. iPhone/iPad als Zielgeraet vorbereiten.
3. App per USB installieren/testen.
4. RKWP DevTransport Client einbauen oder vorbereiten.
5. AblageIdentity erzeugen.
6. Frame anzeigen.
7. Keine Datei speichern.
8. Haptik bei Frame-Ankunft vorbereiten.
9. Drei-Finger-Langdruck pruefen.
10. Glass Edge am Rand simulieren.
11. Logs erzeugen.
12. No File Ingress respektieren.

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

SurfacePlatform: IOS oder IPadOS
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

Offene Blocker ehrlich melden:

- falls Netzwerk-DevTransport fehlt
- falls PDF-/Frame-Renderer nur Mock ist
- falls iOS Permission oder Signing fehlt
- falls echte Windows-Verbindung noch nicht moeglich ist
- falls Drei-Finger-Geste mit iOS/iPadOS-Systemgesten kollidiert
```

## Windows Owner Befehl

```powershell
.\tools\run-windows-owner-for-mac.ps1 -SmokeTest
.\tools\run-windows-owner-for-mac.ps1
```

## Minimaler Test

1. Windows Owner startet.
2. iOS/iPadOS Surface App startet auf echtem Geraet.
3. App sendet `AblageHello`.
4. Windows prueft Identity und Pairing.
5. Windows erstellt CarryLease und FrameSession.
6. iOS/iPadOS zeigt Frame.
7. Haptik meldet Frame-Ankunft.
8. iOS/iPadOS sendet Return.
9. Windows schliesst Frame oder Recovery.

## Blocker Aus Windows-Sicht

- Netzwerkfaehiger DevTransport fehlt noch.
- native iOS/iPadOS App fehlt noch.
- produktive Security fehlt noch.
- PDF-/Frame-Renderer ist noch nicht final.
