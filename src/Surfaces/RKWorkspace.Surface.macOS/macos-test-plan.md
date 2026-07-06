# macOS Test Plan

Status: MA011.08 handoff  
Datum: 2026-07-06

## Test 1: Lokaler App-Start

Erwartung:

- App startet.
- AblageIdentity existiert.
- UI zeigt `Ablage macOS`.
- Logs werden geschrieben.

## Test 2: Windows Owner Verbindung

Windows:

```powershell
.\tools\run-windows-owner-for-mac.ps1
```

macOS:

```bash
open RKWorkspaceMacGuest.app --args --owner-url rkwp-devlan://WINDOWS_HOST:57100 --lab-mode
```

Erwartung:

- `AblageHello: OK`
- Capabilities gesendet.
- Pairing im Lab-Modus akzeptiert.

## Test 3: Frame anzeigen

Erwartung:

- `FrameSessionOpen: OK`
- `FrameSessionReady: OK`
- `FrameUpdate: OK`
- UI zeigt `liegt hier im Frame`
- PDF wird als Frame angezeigt.

## Test 4: No File Ingress

Pruefung:

```bash
find ~/Library/Containers -name "*.pdf"
find ~/Library/Application\ Support/RKWorkspace -name "*.pdf"
```

Erwartung:

```text
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
```

## Test 5: Return und Recovery

Erwartung:

- `zurueckgeben` sendet Return.
- Windows Owner zeigt wieder verfuegbar.
- Verbindungsverlust setzt UI auf `Verbindung verloren`.
- Windows Owner kann Recovery ausfuehren.
