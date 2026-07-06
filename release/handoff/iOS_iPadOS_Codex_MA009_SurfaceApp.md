# iOS/iPadOS Codex Auftrag: MA009 Surface App

Status: Prepared, updated by MA011.09  
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
4. RKWP DevLan/DevTransport Client einbauen oder vorbereiten.
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

## MA010.05 Compatibility Harness

Windows prueft den erwarteten mobilen Vertrag vorab:

```powershell
.\tools\run-ios-guest-compat.ps1 -SmokeTest
.\tools\run-ios-guest-compat.ps1 -ReplaySample
```

Der native Xcode-Client muss mindestens dieselben Ausgaben bzw. Logs liefern:

```text
iOSGuestIdentity: OK
PrimaryPlatform: IPadOS
PhonePlatform: IOS
FrameView: OK
TouchInput: PLANNED
Haptics: PLANNED
GlobalAppCapture: FALSE
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
```

Die erste Quelle ist die RK Workspace App selbst. Danach folgen Document Picker, Share Extension und bewusst begrenztes Pasteboard. Globale App-Erfassung ist fuer diesen Pfad ausgeschlossen.

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

- DevLan-Lab-Profil ist vorbereitet, aber der echte mobile Client fehlt.
- native iOS/iPadOS App fehlt noch.
- produktive Security fehlt noch.
- PDF-/Frame-Renderer ist noch nicht final.

## MA011.09 Konkreter Xcode-Auftrag

Dieser Handoff wurde durch MA011.09 konkretisiert. macOS-Codex soll zusaetzlich diese Dateien lesen:

1. `src/Surfaces/RKWorkspace.Surface.iOS/ios-xcode-project-layout.md`
2. `src/Surfaces/RKWorkspace.Surface.iOS/ios-rkwp-client-flow.md`
3. `src/Surfaces/RKWorkspace.Surface.iOS/ios-frame-guest-ui.md`
4. `src/Surfaces/RKWorkspace.Surface.iOS/ios-haptics-plan.md`
5. `src/Surfaces/RKWorkspace.Surface.iOS/ios-gesture-plan.md`
6. `src/Surfaces/RKWorkspace.Surface.iOS/ios-usb-test-plan.md`
7. `src/Surfaces/RKWorkspace.Surface.iOS/ios-sandbox-sources.md`

Bevorzugter erster Build ist eine native SwiftUI/Xcode-App unter:

```text
apps/ios/RKWorkspaceIOSSurface/
```

Die App soll minimal koennen:

- SurfaceId erzeugen.
- Dev Identity / `AblageIdentity` erzeugen oder laden.
- RKWP DevLan/SecureDev Client verbinden.
- `AblageHello` senden.
- `FrameSession` empfangen.
- PDF-Frame anzeigen.
- Haptik bei `FrameReady` und `Return` vorbereiten.
- einfache Touch-Geste fuer Return anbieten.

## MA013 Aktualisierung

MA013 macht aus dem bisherigen Handoff einen konkreten Xcode-Startpfad. macOS-Codex soll zusaetzlich lesen:

1. `release/handoff/iOS_XcodeProjectBootstrap_MA013.md`
2. `Docs/Platform/iOS_XcodeProjectBootstrap.md`
3. `Docs/Platform/iOS_RKWPClientFlow.md`
4. `Docs/Platform/iOS_FramePresenter.md`
5. `Docs/Platform/iOS_HapticsAndGesturePrototype.md`
6. `Docs/Platform/iOS_NoFileIngressSandboxChecklist.md`
7. `Docs/Platform/iOS_USBDeviceTestRunbook.md`
8. `Docs/Platform/iOS_ReturnAndRecovery.md`
9. `Docs/Platform/iOS_ShareExtensionObjectSource.md`
10. `Docs/Readiness/WindowsToiPad_FirstRealTestGate.md`
11. `Docs/Readiness/iOS_MA013_ReadinessGate.md`

Windows-seitig muessen vor dem echten iPad-Test gruen sein:

```powershell
.\tools\run-ios-guest-compat.ps1 -SmokeTest
.\tools\run-windows-pdf-owner-securedev.ps1 -SmokeTest
.\tools\export-codex-context.ps1
```
- Drei-Finger-Geste spaeter pruefen, aber nicht blockierend machen.
- keine PDF-Datei, keinen Originalpfad und keine Originalbytes als Datei speichern.

Der erste echte Test erfolgt per USB-Geraet aus Xcode. Zielausgabe:

```text
SurfacePlatform: IOS oder IPadOS
SurfaceRole: FrameGuestSurface
AblageHello: OK
FrameSession: Active
HapticsPrepared: OK
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
Return: SUCCESS
```

Blocker bleiben ehrlich zu melden:

- falls Signing oder Provisioning fehlt.
- falls Local Network Permission RKWP DevLan blockiert.
- falls echter RKWP Mobile Client noch fehlt.
- falls PDF-/Frame-Renderer nur Mock oder MetadataPreview ist.
- falls iOS/iPadOS-Systemgesten mit Drei-Finger-Gesten kollidieren.
