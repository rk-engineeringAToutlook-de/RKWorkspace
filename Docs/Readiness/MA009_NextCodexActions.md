# MA009 Next Codex Actions

Status: Draft  
Datum: 2026-07-06  
Branch: `feature/ma009-secure-cross-device-frame-foundation`

## 1. Windows: lokale Frame-UX mit echter PDF sichtbar verbessern

Ziel:

- Windows Owner bleibt Original.
- lokale Guest Surface zeigt einen besser lesbaren Frame.
- Owner/Guest State UX bleibt sichtbar.
- No File Ingress bleibt PASS.

Startpunkte:

- `src/Frame/RKWorkspace.Frame.Pdf/`
- `src/Tools/RKWorkspace.WindowsLocalFrameE2E/`
- `Docs/Development/OwnerGuestFrameStateUX.md`
- `tools/run-windows-local-frame-e2e.ps1 -SmokeTest`

## 2. macOS: FrameGuestSurface bauen

Ziel:

- native macOS Surface zeigt RKWP Frame.
- macOS erzeugt AblageIdentity.
- macOS speichert keine Original-PDF.
- Return und Recovery werden sichtbar.

Startpunkte:

- `release/handoff/macOS_Codex_MA009_FrameGuestSurface.md`
- `release/handoff/WindowsToMac_MA009_Handoff.md`
- `Docs/Codex/PlatformTasks/macOS.md`
- `Docs/Platform/macOS_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.macOS/`

## 3. iOS/iPadOS: Xcode Surface App bauen

Ziel:

- iPad/iPhone wird echte Ablage.
- Surface App zeigt FrameOnly-Inhalt.
- Touch/Haptik sind vorbereitet.
- keine Originaldatei wird gespeichert.

Startpunkte:

- `release/handoff/iOS_iPadOS_Codex_MA009_SurfaceApp.md`
- `Docs/Codex/PlatformTasks/iOS_iPadOS.md`
- `Docs/Platform/iOS_iPadOS_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.iOS/`

## 4. Security: echte TLS/mutual auth auswaehlen

Ziel:

- DevelopmentInsecure bleibt Labor.
- Production nutzt echte Verschluesselung und echte gegenseitige Authentisierung.
- Trust Store und Pairing UI werden vorbereitet.

Startpunkte:

- `Docs/Protocol/RKWP_SecureSession.md`
- `Docs/Protocol/RKWP_SecurityModel.md`
- `Docs/Security/RKWP_DevCertificates.md`
- `src/Protocol/RKWorkspace.Protocol/Security/`

## 5. PDF: finalen Renderer auswaehlen

Ziel:

- Gast sieht lesbare Seiten.
- No File Ingress bleibt unverletzt.
- Renderer laeuft ownerseitig oder streng adapterisiert.

Startpunkte:

- `Docs/Development/RealPdfFrameViewer.md`
- `src/Frame/RKWorkspace.Frame.Pdf/`
- `tools/run-pdf-frame-smoke.ps1`

## 6. Proximity: Manual Map UI verbessern

Ziel:

- Owner kann Ablagen einfacher anordnen.
- immer nur die naechste Ablage aktiviert eine Glass Edge.
- Entfernungen bleiben nachvollziehbar.

Startpunkte:

- `Docs/Proximity/ManualAblageMap.md`
- `tools/run-manual-map.ps1`
- `Docs/GlassEdgeNearestAblage.md`

## 7. Dongle: Hardware Requirements definieren

Ziel:

- Ablage Anchor Dongle als Identitaets- und Proximity-Anker spezifizieren.
- BLE/UWB/USB/Trust/Firmware-Update abgrenzen.
- noch keine Firmware implementieren.

Startpunkte:

- `Docs/AblageAnchorDongle.md`
- `Docs/AblageProximityAndDistance.md`
- `Docs/Protocol/RKWP_TransportProfiles.md`

## 8. UX: Glass Edge erst nach Frame-Stabilitaet weiter polieren

Ziel:

- keine weitere Effektarbeit, bevor FrameOnly ueber echte Plattformgrenze funktioniert.
- Glass Edge bleibt eine klare glaeserne Kante zur naechsten Ablage.

Startpunkte:

- `Docs/GlassEdgeNearestAblage.md`
- `tools/run-glass-edge.ps1 -SmokeTest`
- `tools/run-glass-edge-pdf-frame-e2e.ps1 -SmokeTest`

## 9. Performance: Frame Streaming optimieren

Ziel:

- nach echtem Renderer und echter zweiter Plattform erneut messen.
- FrameUpdate-Groesse und Frequenz optimieren.

Startpunkte:

- `Docs/Performance/RKWP_PerformanceBaseline.md`
- `tools/run-rkwp-perf.ps1 -SmokeTest`

## 10. Audit: persistente Logs und Viewer verbessern

Ziel:

- Real-Lab-Test nachtraeglich auswertbar machen.
- Session, Lease, Frame, Return, Recovery, Denials und Security-Events sichtbar machen.

Startpunkte:

- `src/Tools/RKWorkspace.RkwpDiagnostics/`
- `Docs/Development/RKWP_Diagnostics.md`
- `tools/run-rkwp-diagnostics.ps1 -SmokeTest`

## Empfohlene Reihenfolge

1. Windows Local Frame UX sichtbar verbessern.
2. Netzwerkfaehiges Development-Transportprofil bauen.
3. macOS FrameGuestSurface bauen.
4. Windows-to-macOS Real-Lab-Test durchfuehren.
5. iPad/iPhone Surface App ueber Xcode bauen.
6. PDF Renderer auswaehlen.
7. Security produktiv haerten.
8. Proximity/Dongle realisieren.
