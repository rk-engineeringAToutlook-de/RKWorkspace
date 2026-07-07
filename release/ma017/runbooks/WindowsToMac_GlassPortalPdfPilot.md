# MA017 Windows to macOS Glass Portal PDF Pilot

Status: pilot path
Datum: 2026-07-07

## Ziel

Windows bleibt Owner der echten PDF. macOS bekommt keine PDF-Datei, keinen Originalpfad und keine Originalbytes.

Der Ablauf soll sich fuer den Menschen so anfuehlen:

```text
PDF auf Windows nehmen.
Nahe macOS-Ablage wird erkannt.
Ein glasiger Portalbalken erscheint.
Das Ding geht in die Kante.
macOS zeigt nur den Frame.
```

## Was heute echt ist

- Windows kann Entfernung/naechste Ablage ueber Proximity-Fusion/UWB-Simulation bestimmen.
- Windows kann den Glass-Edge-Eventflow pruefen.
- Windows kann `Rechnung.pdf` owner-seitig als PNG-Frame rendern.
- macOS kann den PNG-Frame nativ anzeigen.
- Rueckgabe sendet `CarryLeaseReturn`.

## Grenze

Der native Windows-Glasbalken und der echte macOS-RKWP-Frame-Owner sind noch nicht zu einem vollautomatischen Drag-in-die-Kante-Produktfluss verbunden. Dieser Runbook-Pfad verbindet sie als kontrollierten Pilot:

1. Glass/Entfernung wird Windows-seitig validiert.
2. Windows Owner Host wird fuer macOS gestartet.
3. macOS Guest zeigt den echten Frame.

## Windows Preflight

Auf Windows:

```powershell
cd "E:\HiDrive\users\RK Workspace\RKWorkspace"
git fetch origin
git pull --ff-only
.\tools\run-ma017-windows-to-mac-glass-frame-pilot.ps1 -SmokeTest -WindowsOwnerAddress 192.168.163.11
```

Erwartet:

```text
GlassPortalBar: READY
DistanceSelection: READY
RendererName: PopplerPdfFrameRenderer
FrameFormat: PngFrame
NoFileIngress: SUCCESS
RESULT: SUCCESS
```

## Echter Lauf

Terminal 1 auf Windows:

```powershell
cd "E:\HiDrive\users\RK Workspace\RKWorkspace"
.\tools\run-ma017-windows-to-mac-glass-frame-pilot.ps1 -StartOwnerHost -WindowsOwnerAddress 192.168.163.11
```

Terminal auf macOS:

```bash
cd "/Users/rkeng/HiDrive/users/rk-engineering/users/RK Workspace/RKWorkspace-github"
git fetch origin
git pull --ff-only
cd release/ma017/packages/macos-frame-guest
swift run MacPdfFrameGuest --host 192.168.163.11 --port 57120 --auto-open
```

Erwartet auf macOS:

```text
AblageHello: OK
AblageCapabilities: OK
FrameView: OK
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
FrameCache: MemoryOnly
NoFileIngress: SUCCESS
```

Rueckgabe-Test auf macOS:

```bash
RKWS_AUTO_RETURN_AFTER=1 RKWS_EXIT_AFTER_RETURN=1 swift run MacPdfFrameGuest --host 192.168.163.11 --port 57120 --auto-open
```

Erwartet:

```text
CarryLeaseReturn: OK
Return: SUCCESS
```

## Sicherheitsregeln

- Kein Browser.
- Kein Spatial Tray.
- Kein PDF-Download.
- Kein Originalpfad auf macOS.
- Keine Originalbytes auf macOS.
- Frame Cache bleibt MemoryOnly.

## Result

```text
MA017WindowsToMacGlassPortalPdfPilot: READY
```
