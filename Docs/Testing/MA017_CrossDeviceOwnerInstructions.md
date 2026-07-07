# MA017 Cross-Device Owner Instructions

## Ziel

Diese Anleitung fuehrt den Owner durch die ersten MA017 Cross-Device PDF-Piloten.

Windows bleibt der Owner. macOS und iPad/iPhone sind vorbereitete Gastablaegen.

## Vor dem Start

```powershell
.\tools\run-ma017-smoke.ps1 -SkipHeavy
.\tools\run-ma017-cross-device-session-monitor.ps1 -SmokeTest
```

## Windows zu macOS - Closed PDF

```powershell
.\tools\run-ma017-windows-to-mac-closed-pdf.ps1 -SmokeTest
```

Danach auf macOS weiter mit:

```text
release/ma017/handoff/macOS_START_HERE.md
```

## Windows zu macOS - Open PDF

```powershell
.\tools\run-ma017-windows-to-mac-open-pdf.ps1 -SmokeTest
```

## Windows zu iPad - Closed PDF

```powershell
.\tools\run-ma017-windows-to-ipad-closed-pdf.ps1 -SmokeTest
```

Danach die iPad-App ueber Xcode/USB vorbereiten:

```text
release/ma017/runbooks/iOS_USBInstallRunbook.md
```

## Windows zu iPad - Open PDF

```powershell
.\tools\run-ma017-windows-to-ipad-open-pdf.ps1 -SmokeTest
```

## Vollstaendiger lokaler Windows-Lifecycle

```powershell
.\tools\run-ma017-windows-all-pdf-lifecycle.ps1 -SmokeTest
```

## Erwartete Marker

- `RESULT: SUCCESS`
- `NoFileIngress: SUCCESS`
- `FrameCapsule: OK`
- `OpenFrame: OK`
- `UnauthorizedCapsuleOpen: DENIED`
- `ExpiredCapsule: RECOVERED_BY_OWNER`

## Owner-Regel

Wenn macOS oder iPad noch nicht nativ laeuft, ist das kein Sicherheitsfehler. Der Zustand wird als `PENDING_EXTERNAL_MACOS_XCODE` oder `PENDING_XCODE_USB_INSTALL` dokumentiert.
