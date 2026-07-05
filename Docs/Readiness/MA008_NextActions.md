# MA008 Next Actions

Status: Draft  
Datum: 2026-07-05  
Branch: `feature/ma008-rkwp-devtransport-e2e-frame`

## Ziel

Dieses Dokument legt die naechsten konkreten Auftraege nach MA008 fest. Reihenfolge ist wichtiger als Menge: erst sichtbarer FrameOnly-Pfad, dann echte Plattformgrenze, dann produktive Security.

## 1. Windows: Lokales PDF Frame UI Verbessern

Ziel:

- PDF liegt weiter beim Windows Owner.
- lokale Guest Surface zeigt eine bessere Frame-Darstellung.
- No File Ingress bleibt PASS.
- Return, Recovery und Audit bleiben sichtbar.

Startpunkte:

- `src/Tools/RKWorkspace.WindowsLocalFrameE2E/`
- `src/Frame/RKWorkspace.Frame.Pdf/`
- `Docs/Development/WindowsLocalFrameE2E.md`
- `tools/run-windows-local-frame-e2e.ps1 -SmokeTest`

Erfolg:

- sichtbarer Frame ist fuer Owner-Test ausreichend.
- keine Originaldatei wird auf Guest erzeugt.

## 2. macOS: Frame Guest Surface Bauen

Ziel:

- native macOS Surface App zeigt RKWP Frame.
- macOS sendet AblageIdentity.
- DevPairing ist sichtbar.
- macOS speichert keine Original-PDF.

Startpunkte:

- `Docs/Codex/PlatformTasks/macOS.md`
- `Docs/Platform/macOS_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.macOS/`
- `Docs/Readiness/WindowsToMac_DevTransportPlan.md`
- `release/handoff/WindowsToMac_MA008_Handoff.md`

Blocker:

- netzwerkfaehiger DevTransport fehlt noch.

## 3. iOS/iPadOS: Xcode Surface App Bauen

Ziel:

- iPad/iPhone wird zur echten Ablage.
- Surface App zeigt FrameOnly-Inhalt.
- Touch/Haptik sind vorbereitet.
- iOS bekommt keine Originaldatei.

Startpunkte:

- `Docs/Codex/PlatformTasks/iOS_iPadOS.md`
- `Docs/Platform/iOS_iPadOS_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.iOS/`
- `Docs/Readiness/iPad_iPhone_Surface_TestPlan.md`
- `release/handoff/iOS_iPadOS_MA008_Handoff.md`

Blocker:

- native Xcode-App muss auf macOS entstehen.
- Local Network Permission und reales Geraet muessen getestet werden.

## 4. Windows/macOS: DevTransport Cross-Device Test

Ziel:

- RKWP laeuft zwischen zwei echten Geraeten im lokalen LAN.
- Windows Owner und macOS Guest tauschen Hello, Capabilities, Lease, FrameUpdate, Heartbeat und Return aus.
- No File Ingress bleibt beweisbar.

Startpunkte:

- `Docs/Protocol/RKWP_DevTransport.md`
- `Docs/Protocol/RKWP_TransportProfiles.md`
- `Docs/Readiness/WindowsToMac_DevTransportPlan.md`
- `src/Communication/RKWorkspace.Transport.Dev/`

Naechster technischer Schritt:

- Development-Profil fuer TCP/WebSocket im privaten LAN einfuehren.

## 5. Security: Echte Verschluesselung / Mutual Auth

Ziel:

- `DevelopmentInsecure` bleibt nur fuer lokale Tests.
- Production erfordert echte Session Protection.
- Mutual Authentication und Key Material werden nicht simuliert.

Startpunkte:

- `Docs/Security/RKWP_SecurityGate.md`
- `Docs/Protocol/RKWP_SecurityModel.md`
- `src/Protocol/RKWorkspace.Protocol/Security/`

Blocker:

- produktives Kryptografie-Design.
- Pairing UI und Trust Store.

## 6. Proximity: Manual Map UI

Ziel:

- Owner kann Ablagen manuell im Raum anordnen.
- naechste Ablage steuert genau eine Glass Edge.
- Distanz und Confidence sind sichtbar, aber nicht technisch dominant.

Startpunkte:

- `Docs/AblageProximityAndDistance.md`
- `src/Shell/RKWorkspace.Shell/`
- `tools/run-glass-edge.ps1 -SmokeTest`

## 7. Dongle: Hardware Requirements

Ziel:

- Dongle als spaeterer Ablage-Anker spezifizieren.
- BLE/UWB/USB, Identity, Pairing und Recovery beschreiben.
- keine Firmware implementieren, bevor Requirements stehen.

Startpunkte:

- `Docs/AblageAnchorDongle.md`
- `Docs/AblageProximityAndDistance.md`
- `Docs/Protocol/RKWP_TransportProfiles.md`

## 8. UX: Glass Edge Feinschliff Nach Funktionierendem Frame

Ziel:

- genau eine naechste Glass Edge.
- Glas bleibt Hinweis auf Raum/Fortsetzung, nicht Effektshow.
- Reinziehen in die Kante folgt dem echten Ziel.

Regel:

- UX-Feinschliff erst priorisieren, nachdem FrameOnly ueber echte Plattformgrenze sichtbar funktioniert.

## 9. PDF: Echter Renderer Oder Besserer Frame Provider

Ziel:

- Gast sieht lesbare PDF-Seiten, ohne Originaldatei zu besitzen.
- Renderer darf keine Originalbytes als freie Datei materialisieren.

Startpunkte:

- `src/Frame/RKWorkspace.Frame.Pdf/`
- `Docs/Development/PdfFrameInteraction.md`
- `tools/run-pdf-frame-smoke.ps1`

## 10. Diagnostics: Audit Viewer Erweitern

Ziel:

- Sessions, Leases, FrameSessions, Denials, Recovery und Security Gate sichtbar machen.
- Cross-Device-Test spaeter nachvollziehbar auswerten.

Startpunkte:

- `src/Tools/RKWorkspace.RkwpDiagnostics/`
- `Docs/Development/RKWP_Diagnostics.md`
- `tools/run-rkwp-diagnostics.ps1 -SmokeTest`

## Empfohlene Reihenfolge

1. Windows Local Frame UI sichtbar verbessern.
2. Netzwerkfaehigen DevTransport fuer Development einfuehren.
3. macOS Frame Guest Surface bauen.
4. Windows-to-macOS Cross-Device-Smoke durchfuehren.
5. iPad/iPhone Surface App ueber Xcode bauen.
6. Security und Trust produktiv haerten.
7. Proximity/Dongle realisieren.

## Entscheidungspunkt

Der erste echte Testing-Start ist sinnvoll, sobald Windows Owner und eine zweite echte Plattform denselben FrameOnly-Vertrag einhalten. Bis dahin ist der lokale Windows-E2E-Test die Referenz.
