# iOS and iPadOS Platform Tasks

Status: Prepared  
Datum: 2026-07-06

## Plattformziel

iPhone und iPad werden als mobile Ablagen im Arbeitsraum vorbereitet. Sie haben keinen eigenen Codex; Entwicklung und Test laufen ueber macOS-Codex, Xcode und echte USB-angeschlossene Geraete.

Die erste native Surface App soll:

- RKWP Frames anzeigen.
- No File Ingress respektieren.
- Haptik subtil vorbereiten.
- Touch-Gesten erkennen.
- eine Glass Edge am Rand simulieren.
- Logs fuer FrameOnly, Return und No File Ingress erzeugen.

Sie ist kein Dateiempfaenger und kein Sync-Ziel.

## Aktueller Handoff

Direkt nutzbare Handoff-Datei:

```text
release/handoff/iOS_iPadOS_Codex_MA009_SurfaceApp.md
```

Vorheriger MA008-Handoff bleibt Referenz:

```text
release/handoff/iOS_iPadOS_MA008_Handoff.md
```

## Vor dem Bauen Lesen

- `Docs/Codex/CURRENT_CONTEXT.md`
- `Docs/Platform/iOS_iPadOS_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.iOS/README.md`
- `src/Surfaces/RKWorkspace.Surface.iOS/iOS_SurfaceApp_Design.md`
- `src/Surfaces/RKWorkspace.Surface.iOS/iOS_Haptics_Gesture_Plan.md`
- `src/Surfaces/RKWorkspace.Surface.iOS/iOS_Xcode_USB_TestPlan.md`
- `src/Surfaces/RKWorkspace.Surface.iOS/iOS_Sandbox_ObjectSources.md`
- `Docs/Protocol/RKWP_ProtocolFoundation.md`
- `Docs/Protocol/RKWP_SecureSession.md`
- `Docs/Protocol/RKWP_AblageIdentityAndTrust.md`
- `Docs/Protocol/RKWP_OwnershipAndLease.md`
- `Docs/Protocol/RKWP_FrameSession.md`
- `Docs/Protocol/RKWP_InputChannel.md`
- `Docs/Protocol/RKWP_ChangeSetAndReturn.md`
- `src/Surfaces/RKWorkspace.Surface.Abstractions`

## Relevante Surface Contracts

- `ISurfaceHost`
- `ISurfaceOverlay`
- `ISurfaceGestureProvider`
- `ISurfaceFramePresenter`
- `ISurfaceInputChannel`
- `ISurfaceHapticsProvider`
- `ISurfaceSecurityContext`
- `SurfacePlatform.IOS`
- `SurfacePlatform.IPadOS`

## iOS/iPadOS Regeln

iOS/iPadOS kann nicht beliebige App-Inhalte global greifen. RK Workspace respektiert Sandbox und Plattformgrenzen.

Erste erlaubte Quellen:

- RK Workspace App
- Share Extension
- Document Picker
- Pasteboard bewusst und begrenzt
- eigene Surface

Nicht als erste Annahme:

- globale Erfassung beliebiger App-Inhalte.
- Umgehen der Sandbox.
- stille Dateiuebernahme.
- PWA als finaler Gefuehlspfad.

Native App ist Ziel. Browser-Chrome ist nur ein Uebergang fuer sehr fruehe Prototypen.

## Haptik

Haptik dient nur der menschlichen Rueckmeldung:

- Geste erkannt.
- Ding genommen.
- Frame angekommen.
- Glass Edge aktiv.
- Rueckgabe.
- Fehler oder Denied.

Keine starke Haptik. Subtil, kurz und kontrollierbar.

## Kopierbarer Auftrag

Der vollstaendige kopierbare Auftrag steht in:

```text
release/handoff/iOS_iPadOS_Codex_MA009_SurfaceApp.md
```

Kernauftrag:

```text
Baue eine iOS/iPadOS RK Workspace Surface App.
```

## Blocker

- Xcode/macOS-Codex fehlt in diesem Windows-Thread.
- echtes iPhone/iPad muss per USB getestet werden.
- DevLan-Lab-Profil ist auf Windows vorbereitet, aber der echte mobile Client fehlt.
- finale native PDF-/Frame-Darstellung ist offen.
- Drei-Finger-Geste muss gegen iOS/iPadOS-Systemgesten validiert werden.

## MA010.05 Compatibility Harness

Windows stellt einen iOS/iPadOS-Guest-Kompatibilitaets-Harness bereit:

```powershell
.\tools\run-ios-guest-compat.ps1 -SmokeTest
.\tools\run-ios-guest-compat.ps1 -ReplaySample
```

Der Harness ist keine iOS-App. Er ist der Vertrag fuer macOS-Codex und Xcode:

- `PrimaryPlatform: IPadOS`
- `PhonePlatform: IOS`
- FrameView aktiv.
- TouchInput, Haptics und Glass Edge geplant.
- Share Extension, Document Picker und begrenztes Pasteboard geplant.
- GlobalAppCapture false.
- No File Ingress aktiv.
- OwnershipTransfer standardmaessig aus.

Der native Xcode-Build muss diese Kriterien auf echtem iPhone/iPad bestaetigen.
