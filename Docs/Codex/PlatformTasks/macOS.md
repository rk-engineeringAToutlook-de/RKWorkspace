# macOS Platform Tasks

Status: Prepared  
Datum: 2026-07-06

## Plattformziel

macOS ist die erste echte Gegenplattform zum Windows-Owner-Pfad. Die erste native macOS-Arbeit ist eine Frame Guest Surface.

Ziel ist nicht Dateiuebertragung. Ziel ist:

- Windows bleibt Owner der PDF.
- macOS wird als Ablage identifiziert.
- macOS zeigt einen RKWP Frame.
- macOS speichert keine freie PDF-Datei.
- macOS sendet Heartbeat und Return.
- No File Ingress ist beweisbar.

## Aktueller Handoff

Direkt nutzbare Handoff-Datei:

```text
release/handoff/macOS_Codex_MA009_FrameGuestSurface.md
```

Windows Owner Handoff:

```text
release/handoff/WindowsToMac_MA009_Handoff.md
```

## Vor dem Bauen Lesen

- `Docs/Codex/CURRENT_CONTEXT.md`
- `Docs/Platform/macOS_SurfaceStarterKit.md`
- `Docs/Protocol/RKWP_ProtocolFoundation.md`
- `Docs/Protocol/RKWP_SecureSession.md`
- `Docs/Protocol/RKWP_AblageIdentityAndTrust.md`
- `Docs/Protocol/RKWP_OwnershipAndLease.md`
- `Docs/Protocol/RKWP_FrameSession.md`
- `Docs/Protocol/RKWP_InputChannel.md`
- `Docs/Protocol/RKWP_ChangeSetAndReturn.md`
- `src/Surfaces/RKWorkspace.Surface.Abstractions`
- `src/Surfaces/RKWorkspace.Surface.macOS`

## Relevante Surface Contracts

- `ISurfaceHost`
- `ISurfaceOverlay`
- `ISurfaceGestureProvider`
- `ISurfaceFramePresenter`
- `ISurfaceInputChannel`
- `ISurfaceHapticsProvider`
- `ISurfaceSecurityContext`
- `SurfacePlatform.MacOS`

## macOS Berechtigungen

Fuer den ersten Frame Guest Test:

- App Sandbox mit Network Client.
- Local Network / Firewall Prompt dokumentieren.
- keine freie Dateiablage fuer Owner-PDF.
- Logs fuer No File Ingress.

Spaeter:

- Accessibility fuer globale Gesten.
- Screen Recording fuer echte Desktop-/Fenstererkennung.
- Security-scoped resources fuer explizit gewaehlt Dateien.
- Trackpad/Force Touch Feedback.
- Notifications optional.

## Technologieoptionen

- Swift/AppKit fuer native Window-/Surface-Kontrolle.
- SwiftUI fuer schnelle Minimal-App.
- PDFKit nur, wenn keine freie Owner-PDF materialisiert wird.
- CoreAnimation/Metal spaeter fuer Glass Edge.
- .NET/MAUI nur pruefen, wenn Surface-Gefuehl und Berechtigungen nicht leiden.

## Kopierbarer Auftrag

Der vollstaendige kopierbare Auftrag steht in:

```text
release/handoff/macOS_Codex_MA009_FrameGuestSurface.md
```

Kernauftrag:

```text
Baue eine macOS RK Workspace Frame Guest Surface.
```

## Blocker

- macOS-Codex/Xcode-Umgebung fehlt in diesem Windows-Thread.
- netzwerkfaehiger DevTransport fuer echte Windows-to-macOS-Verbindung fehlt.
- produktive TLS/mutual auth fehlt.
- nativer PDF-Renderer ist noch nicht final entschieden.
