# macOS Surface Starter Kit

Dokument-ID: RKWS-PLATFORM-MACOS-001
Status: Draft
Datum: 2026-07-05

## Ziel

macOS wird als erste echte Gegenplattform vorbereitet.

Die erste macOS Surface ist eine Gastablage fuer RKWP Frames. Sie zeigt ein digitales Ding als Frame an, ohne die Originaldatei zu besitzen oder als freie Datei zu speichern.

## Produktrolle

macOS Surface ist nicht Owner-Logik und nicht Protokollquelle. Sie ist eine Ablage im Arbeitsraum.

Erste Rolle:

- FrameGuestSurface
- Glass Edge am Rand
- Frame anzeigen
- Input policygebunden vorbereiten
- No File Ingress beweisen

## No File Ingress

Pflicht:

- keine PDF-Datei speichern
- keine Originalbytes als Datei schreiben
- kein automatischer Besitzwechsel
- FrameOnly respektieren
- Return und Recovery unterstuetzen

## Native Optionen

Empfohlen zuerst pruefen:

- Swift/AppKit fuer Fenster, Overlay und Berechtigungen
- SwiftUI fuer einfache Surface-App
- CoreAnimation/Metal fuer spaetere Glass Edge
- Trackpad-Gesten ueber native APIs
- haptisches Feedback ueber Trackpad, falls verfuegbar

.NET/MAUI bleibt eine Option, aber nur wenn transparente Surface, Gesten und Human Experience nicht leiden.

## Berechtigungen

macOS-Codex muss klaeren:

- Accessibility fuer globale Gesten
- Screen Recording fuer sichtbare Inhalte/Overlay
- Sandbox und Entitlements
- Security-Scoped Access fuer Dateien
- Network Local Access fuer DevTransport
- Trackpad-Gesten und Force Touch

## Erste Surface

Minimaler Start:

1. App oder Agent startet.
2. SurfaceId wird angezeigt oder geloggt.
3. RKWP DevTransport kann spaeter verbinden.
4. FrameSession wird angenommen.
5. Frame-Darstellung wird gezeigt.
6. Keine Datei wird lokal materialisiert.
7. Return/Close invalidiert den Frame.

## Erste Gesten

Zu pruefen:

- Maus-Drag als Fallback
- Trackpad Hold/Drag
- Drei-Finger-Langdruck
- Force Touch / Haptik
- Escape/Cancel

## Glass Edge

Die macOS Surface zeigt spaeter eine Glass Edge an dem Rand, der zur naechsten Ablage zeigt. In V1 reicht eine vorbereitete Visualisierung; die Proximity-Logik kommt aus RKWP/Surface-Contracts.

## Testpfad

Erster echter Test:

Windows besitzt PDF.
macOS zeigt PDF-Frame.
macOS bekommt keine PDF-Datei.
Windows bleibt Owner.
macOS gibt Frame zurueck.

MA008.06 liefert dafuer:

- `Docs/Readiness/WindowsToMac_DevTransportPlan.md`
- `release/handoff/WindowsToMac_MA008_Handoff.md`
- `tools/run-windows-owner-for-mac.ps1`

Windows kann den Owner-Status mit folgendem Befehl ausgeben:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -InfoOnly
```

Der echte macOS-Test braucht ein netzwerkfaehiges Development-Transportprofil. `NamedPipeDev` bleibt der lokal verifizierte Windows-Pfad.

## Offene Blocker

- macOS-Codex/Xcode-Umgebung fehlt in diesem Windows-Thread.
- netzwerkfaehiger DevTransport fuer Cross-Device-Test fehlt noch.
- echter PDF-Renderer ist noch offen.
- finale Overlay-/Permission-Strategie ist offen.
