# macOS Surface Starter Kit

Dokument-ID: RKWS-PLATFORM-MACOS-001  
Status: Draft  
Datum: 2026-07-06

## Ziel

macOS wird als erste echte Gegenplattform vorbereitet. Die erste macOS Surface ist eine Gastablage fuer RKWP Frames.

Sie zeigt ein digitales Ding als Frame an, ohne die Originaldatei zu besitzen oder als freie Datei zu speichern.

## Produktrolle

macOS Surface ist:

- Frame Guest Surface.
- Ablage im Arbeitsraum.
- Anzeige- und Eingabeflaeche fuer RKWP Frame.

macOS Surface ist nicht:

- Owner der Windows-PDF.
- Dateiempfaenger.
- Sync-Ziel.
- Standard-Ort fuer Ownership Transfer.

## Minimaler Aufbau

1. Native macOS App oder Host startet.
2. AblageIdentity wird erzeugt.
3. RKWP DevTransport Client verbindet.
4. DevPairing wird angefordert.
5. FrameSession wird empfangen.
6. PDF Frame wird angezeigt.
7. No File Ingress wird geprueft.
8. Heartbeat wird gesendet.
9. Return wird gesendet.
10. Logs werden geschrieben.

## No File Ingress

Pflicht:

- keine PDF-Datei speichern.
- keinen Windows-Originalpfad als lokale Datei behandeln.
- keine Originalbytes als freie Datei materialisieren.
- FrameOnly respektieren.
- Return und Recovery unterstuetzen.

## Native Optionen

Empfohlen:

- Swift/AppKit fuer Fenster, Overlay und Berechtigungen.
- SwiftUI fuer einfache Surface-App.
- PDFKit nur kontrolliert, wenn kein File Ingress entsteht.
- CoreAnimation/Metal spaeter fuer Glass Edge.
- Trackpad-Gesten und Force Touch spaeter.

## Berechtigungen

Siehe:

```text
src/Surfaces/RKWorkspace.Surface.macOS/macOS_Permissions_Checklist.md
```

## Handoff

Der vollstaendige macOS-Codex-Auftrag liegt hier:

```text
release/handoff/macOS_Codex_MA009_FrameGuestSurface.md
```

Windows Owner Handoff:

```text
release/handoff/WindowsToMac_MA009_Handoff.md
```

## Testpfad

Erster echter Test:

1. Windows besitzt PDF.
2. macOS meldet sich als Ablage.
3. Windows gibt FrameOnly frei.
4. macOS zeigt PDF-Frame.
5. macOS bekommt keine PDF-Datei.
6. macOS sendet Return.
7. Windows bestaetigt Rueckgabe oder Recovery.

## Kompatibilitaets-Harness

Bis eine echte macOS/Xcode-Implementierung existiert, prueft Windows den macOS-Guest-Vertrag lokal:

```powershell
.\tools\run-mac-guest-compat.ps1 -SmokeTest
```

Gegen einen laufenden Windows Owner:

```powershell
.\tools\run-mac-guest-compat.ps1 -ConnectToOwner rkwp+tcp-dev://<windows-ip>:57100
```

Der Harness simuliert:

- macOS AblageIdentity.
- macOS Capabilities.
- FrameGuestSurface.
- FrameSessionReady.
- Heartbeat.
- FrameClose / Return.
- Recovery-Verhalten.
- No File Ingress.

## Offene Blocker

- macOS-Codex/Xcode-Umgebung fehlt in diesem Windows-Thread.
- RKWP DevLan ist fuer das Labor vorbereitet, aber der echte native macOS-Client fehlt.
- echter PDF-Renderer ist noch offen.
- finale Overlay-/Permission-Strategie ist offen.
