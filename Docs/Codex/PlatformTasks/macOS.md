# macOS Platform Tasks

## Plattformziel

macOS wird die erste echte Gegenplattform zum Windows-Owner-Pfad.

Ziel ist eine macOS Ablage als `FrameGuestSurface`, die RKWP-Frames anzeigen kann, ohne eine Datei zu uebernehmen.

## Aktueller Stand

- Stub: `src/Surfaces/RKWorkspace.Surface.macOS`
- Starter Kit: `Docs/Platform/macOS_SurfaceStarterKit.md`
- gemeinsame Contracts: `src/Surfaces/RKWorkspace.Surface.Abstractions`
- Protokollgrundlage: `src/Protocol/RKWorkspace.Protocol`
- kein nativer macOS-Build in diesem Windows-Thread

## Relevante Surface Contracts

- `ISurfaceHost`
- `ISurfaceGestureProvider`
- `ISurfaceOverlay`
- `ISurfaceFramePresenter`
- `ISurfaceHapticsProvider`
- `ISurfaceSecurityContext`
- `ISurfaceInputChannel`
- `SurfacePlatform.MacOS`
- `SurfaceCapabilities.FramePresentation`

## RKWP-Pflichtsemantik

- OriginalOwned bleibt Default.
- FrameOnly bedeutet: macOS zeigt Frame, aber speichert keine Originaldatei.
- No File Ingress ist Pflicht.
- Input ist policygebunden.
- ChangeSets sind Vorschlaege, keine stille Originalaenderung.
- Ownership Transfer ist optional und braucht Policy/Bestätigung.

## Berechtigungen

macOS-Codex muss pruefen:

- Accessibility fuer globale Gesten und Eingaben
- Screen Recording / Capture Permissions fuer sichtbare Oberflaechen
- Sandbox und Entitlements
- Security-Scoped Access fuer vom Benutzer gewaehlte Dateien
- Trackpad-Gesten
- Drei-Finger-Langdruck oder alternative Geste
- Force Touch / Trackpad Feedback als spaeterer Haptikpfad

## Technologieoptionen

- Swift/AppKit fuer native Overlay-/Window-Kontrolle
- SwiftUI fuer einfache Surface-App
- Metal/CoreAnimation fuer spaetere hochwertige Glass Edge
- .NET/MAUI nur pruefen, wenn es die Human Experience nicht stoert

## GitHub und Context Pack

macOS arbeitet ueber GitHub-Synchronisation und Context-Pack. Kein lokaler Windows-Pfad wird als macOS-Implementierung ausgegeben.

Context Pack erzeugen:

```powershell
.\tools\export-codex-context.ps1
```

Zusaetzlicher MA008-Handoff fuer den ersten Windows-zu-macOS-Test:

- `Docs/Readiness/WindowsToMac_DevTransportPlan.md`
- `release/handoff/WindowsToMac_MA008_Handoff.md`
- `tools/run-windows-owner-for-mac.ps1`

Der aktuelle Windows-DevTransport ist `NamedPipeDev` und lokal verifiziert. macOS-Codex muss fuer echten Cross-Device-Betrieb ein netzwerkfaehiges Development-Profil oder einen klar begrenzten Adapter bauen.

## Kopierbarer naechster macOS-Codex-Auftrag

```text
RK Workspace macOS Codex Auftrag:

Baue eine macOS Frame Guest Surface, die RKWP Frames anzeigen kann, ohne die Originaldatei zu uebernehmen.

Vorgehen:

1. GitHub-Repository synchronisieren und den aktuellen MA007/MA008-Branch pruefen.
2. Context Pack lesen, besonders:
   - Docs/Codex/CURRENT_CONTEXT.md
   - Docs/Platform/macOS_SurfaceStarterKit.md
   - Docs/Protocol/RKWP_ProtocolFoundation.md
   - Docs/Protocol/RKWP_OwnershipAndLease.md
   - Docs/Protocol/RKWP_FrameSession.md
   - Docs/Protocol/RKWP_InputChannel.md
   - Docs/Protocol/RKWP_ChangeSetAndReturn.md
   - src/Surfaces/RKWorkspace.Surface.Abstractions
3. macOS-Berechtigungen pruefen:
   - Accessibility
   - Screen Recording
   - Sandbox / security-scoped access
   - Trackpad-Gesten
4. Minimal-App oder Agent bauen:
   - SurfaceHost
   - FrameGuestSurface
   - einfache Frame-Anzeige
   - Glass Edge am Rand als vorbereiteter visueller Hinweis
5. RKWP-Semantik einhalten:
   - keine PDF-Datei speichern
   - keine Originalbytes als Datei materialisieren
   - FrameOnly respektieren
   - No File Ingress pruefen
6. Test mit Windows Owner vorbereiten:
   - Windows besitzt PDF
   - macOS zeigt Frame
   - Windows bleibt Owner
   - macOS gibt Frame zurueck
   - Recovery testen
7. Windows-Handoff lesen:
   - Docs/Readiness/WindowsToMac_DevTransportPlan.md
   - release/handoff/WindowsToMac_MA008_Handoff.md
8. DevTransport-Luecke sauber schliessen:
   - aktueller Windows-Pfad: NamedPipeDev lokal
   - Ziel: lokales Netzwerkprofil fuer Windows Owner zu macOS Guest
   - No File Ingress darf dadurch nicht gelockert werden

Nicht bauen:

- keine globale Dateierfassung als erste Annahme
- kein Ownership Transfer als Default
- keine Dateiuebertragung als Erfolgspfad
```

## Aktuelle Blocker

- keine macOS-Entwicklungsumgebung in diesem Windows-Thread
- noch keine native macOS-App
- noch kein netzwerkfaehiger DevTransport-Cross-Device-Test
- keine produktive Capture-/Overlay-Berechtigungsstrategie
