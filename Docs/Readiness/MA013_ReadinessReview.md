# RK Workspace

# MA013 Readiness Review

Status: Accepted  
Datum: 2026-07-06

## Ziel

MA013 schliesst die Vorbereitung fuer den ersten echten Cross-Device-Pilot ab. Der Fokus liegt auf Original-Owned Frames, No File Ingress, Windows als Owner, macOS/iPad als naechste Guests, Security, Policy, Audit, Recovery, Proximity und Installationsvorbereitung.

## Bewertung

| Bereich | Status | Bewertung |
| --- | --- | --- |
| Windows local pilot | Done | Lokaler Windows-PDF-Frame-E2E ist als Smoke-Pfad vorhanden. |
| Windows to macOS | Partial | Handoff, Contract und DevLan/SecureDev-Pfad sind vorbereitet; echter macOS-Lauf steht aus. |
| Windows to iPad | Planned | Xcode/TestFlight/USB-Plan und Surface-Design sind vorbereitet. |
| Secure transport | Partial | SecureDev existiert, Produkttransport ist noch zu haerten. |
| PDF renderer | Partial | Renderer und Fallback laufen, Produktqualitaet muss weiter finalisiert werden. |
| No File Ingress | Done | Tests erzwingen FrameOnly und keine Originalbytes auf Guest. |
| Policy | Done | Policy-Profile und Critical-Infrastructure-Pack sind vorbereitet. |
| Audit | Done | Diagnostics, AuditList, Session, Lease und Violations sind vorhanden. |
| Recovery | Done | Return, Recovery und Emergency-Strategien sind dokumentiert und getestet. |
| Proximity | Partial | Manual Map, WiFi Presence, BLE/UWB/Dongle-Roadmap sind vorbereitet. |
| Dongle | Planned | Hardware-MVP und Firmware-Architektur sind spezifiziert, aber nicht gebaut. |
| Object adapters | Partial | Windows-Adapter fuer PDF, Explorer, Clipboard und Stubs sind vorbereitet. |
| UX | Partial | Glass Edge ist Produktpfad, Premium-Lens-Experimente bleiben Labor. |
| Install | Partial | Windows Dev Package existiert; macOS/iOS/Android/Linux Installer sind Plaene. |
| Performance | Partial | E2E-Performance und Multi-Frame-Load sind Smoke-faehig, echte Hardwaremessung fehlt. |

## Blocker

- echter macOS-Guest muss gebaut und gegen Windows getestet werden.
- iPad/iPhone-App muss per Xcode gestartet werden.
- Produkttransport braucht Sicherheitsentscheidung und Härtung.
- PDF-Renderer braucht Produktqualitaet und Sandboxentscheidung.
- Proximity braucht echte Distanzdaten.

## Ergebnis

MA013 ist bereit, vom lokalen Windows-Pilot in den ersten echten Windows-to-macOS-Test ueberzugehen.
