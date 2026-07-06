# macOS Project Layout

Status: MA011.08 handoff  
Datum: 2026-07-06

## Ziel

macOS-Codex soll aus diesem Layout eine minimale macOS Guest Surface bauen koennen. Windows-Codex legt kein Xcode-Projekt an und baut macOS nicht, definiert aber die Struktur.

## Bevorzugte Option A: Native Swift/Xcode

Empfohlen fuer den ersten echten Test:

```text
apps/macos/RKWorkspaceMacGuest/
  RKWorkspaceMacGuest.xcodeproj
  RKWorkspaceMacGuest/
    RKWorkspaceMacGuestApp.swift
    App/
      AppState.swift
      AppLog.swift
      Settings.swift
    Identity/
      AblageIdentityStore.swift
      AblageIdentityModel.swift
    RKWP/
      RkwpClient.swift
      RkwpEnvelope.swift
      RkwpMessageType.swift
      RkwpDevLanConnection.swift
      RkwpSecureDevHandshake.swift
    Frame/
      FrameSessionModel.swift
      FramePresenterView.swift
      PdfFrameImageView.swift
      FrameReturnController.swift
    Diagnostics/
      NoFileIngressAudit.swift
      SessionDiagnostics.swift
  RKWorkspaceMacGuestTests/
    RkwpClientTests.swift
    NoFileIngressTests.swift
```

## Option B: .NET MAUI oder Avalonia

Nur nutzen, wenn macOS-Codex damit schneller einen stabilen Host bauen kann. Die Semantik bleibt gleich:

- AblageIdentity laden/erzeugen.
- RKWP Client verbinden.
- FrameSession anzeigen.
- Keine PDF-Datei materialisieren.
- Heartbeat und Return senden.

## V1-Entscheidung

Fuer den ersten echten Windows-zu-macOS-Test ist Option A bevorzugt: native Swift/Xcode App oder ein minimaler nativer macOS Host. Ziel ist nicht finale UX, sondern ein sicherer Frame Guest.
