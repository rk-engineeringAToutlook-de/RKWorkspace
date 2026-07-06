# MA009 Real Lab Test Plan

Status: Draft  
Datum: 2026-07-06  
Branch: `feature/ma009-secure-cross-device-frame-foundation`

## Ziel

Der erste Real-Lab-Test prueft nicht Dateiuebertragung. Er prueft:

- Windows bleibt Original-Owner.
- Eine Gastablage sieht einen Frame.
- Die Gastablage bekommt keine freie PDF-Datei.
- Rueckgabe und Recovery funktionieren.
- Logs zeigen nachvollziehbar, was passiert ist.

## Test 1: Windows Owner + Windows Local Guest

Zweck:

Lokale Referenz vor echter Plattformgrenze.

Start:

```powershell
.\tools\run-windows-local-frame-e2e.ps1 -SmokeTest
```

Sichtbarer MA010.01-Pilot:

```powershell
.\tools\run-windows-pdf-frame-pilot.ps1 -SmokeTest
.\tools\run-windows-pdf-frame-pilot.ps1 -OwnerVisible -GuestVisible
```

Der Pilot zeigt zwei lokale Ablagen und dieselbe FrameOnly-Regel wie der E2E-Smoke: Owner bleibt Eigentuemer, Guest sieht nur den Frame, Rueckgabe und Recovery bleiben sichtbar.

Benoetigt:

- Windows PC/Laptop
- Branch `feature/ma009-secure-cross-device-frame-foundation`
- Sample-PDF `samples\Objects\Rechnung.pdf`

Erfolg:

- `DevPairing: SUCCESS`
- `CarryLease: Active`
- `FrameSession: Active`
- `GuestVisibleStatus: liegt hier im Frame`
- `NoFileIngress: SUCCESS`
- `Return: SUCCESS`
- `Recovery: SUCCESS`
- `RESULT: SUCCESS`

Sicherheitsstatus:

Development-only. Noch keine produktive Kryptografie.

## Test 2: Windows Owner + macOS Guest

Zweck:

Erste echte Plattformgrenze.

Start Windows Owner:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -InfoOnly
.\tools\run-windows-owner-for-mac.ps1
```

macOS-Codex liest:

```text
release/codex-context/RKWorkspace_Context_latest.zip
release/handoff/WindowsToMac_MA009_Handoff.md
release/handoff/macOS_Codex_MA009_FrameGuestSurface.md
Docs/Codex/PlatformTasks/macOS.md
Docs/Platform/macOS_SurfaceStarterKit.md
```

Benoetigt:

- Windows Owner im selben LAN wie macOS
- macOS mit Codex/Xcode-geeigneter Entwicklungsumgebung
- netzwerkfaehiges RKWP Development-Profil oder passender lokaler Bridge-Schritt

Erfolg:

- macOS erzeugt AblageIdentity.
- macOS paart sich im DevMode sichtbar.
- Windows bleibt PDF Owner.
- macOS zeigt nur Frame.
- macOS speichert keine PDF-Datei.
- macOS sendet Return.
- Recovery bei Heartbeat-Verlust ist nachvollziehbar.

Blocker:

- Native macOS Frame Guest Surface fehlt.
- Netzwerkfaehiger DevTransport ist noch nicht implementiert.
- Produktive Security fehlt.

## Test 3: Windows Owner + iPad Guest

Zweck:

Erster mobiler Ablage-Test.

iOS/iPadOS-Handoff:

```text
release/handoff/iOS_iPadOS_Codex_MA009_SurfaceApp.md
Docs/Codex/PlatformTasks/iOS_iPadOS.md
Docs/Platform/iOS_iPadOS_SurfaceStarterKit.md
src/Surfaces/RKWorkspace.Surface.iOS/
```

Benoetigt:

- macOS-Codex
- Xcode
- iPad oder iPhone per USB
- Apple Developer Signing fuer lokales Testen
- Local Network Permission fuer spaeteren RKWP-DevTransport

Erfolg:

- iPad/iPhone zeigt Frame.
- keine Originaldatei wird gespeichert.
- Haptik/Geste ist vorbereitet.
- Return und Recovery sind sichtbar.

Blocker:

- Native Xcode-App fehlt.
- RKWP Client fuer iOS fehlt.
- echtes Local-Network-Verhalten muss auf Geraet getestet werden.

## Gemeinsames No-File-Ingress-Kriterium

Jeder Real-Lab-Test muss nachweisen:

- GuestHasPdfFile: NO
- GuestHasOriginalPath: NO
- OriginalFileBytes: NO
- NoFileIngress: SUCCESS

## Gemeinsame Rueckgabe-Kriterien

Jeder Real-Lab-Test muss zeigen:

- Guest kann Frame zurueckgeben.
- Owner wird wieder verfuegbar.
- Recovery sperrt keinen Owner dauerhaft.
- Audit/Diagnostics zeigen Ereignisse.

## Context Pack

Stabiler Pfad:

```text
release/codex-context/RKWorkspace_Context_latest.zip
```

Dieser Pfad wird vor jedem Plattform-Handoff neu erzeugt.

## Empfehlung

Nicht mit iPad starten. Zuerst Windows Local E2E als Referenz, danach macOS Guest Surface, danach iPad/iPhone Surface App.
