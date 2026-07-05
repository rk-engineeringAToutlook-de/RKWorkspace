# MA007 Readiness Review

Status: Draft  
Datum: 2026-07-05  
Branch: `feature/ma007-followup-original-owned-frame-platforms`

## Zweck

Diese Review schliesst die MA007-Folgephase fachlich ab. Sie trennt klar zwischen gebaut, vorbereitet, simuliert und noch nicht testbereit.

## Gate Status

| Bereich | Status | Begruendung |
| --- | --- | --- |
| RKWP Protocol Status | Done | Protocol Foundation, Envelope, Version, Validation, ObjectKind-Regeln und RKWP-Testharness sind vorhanden. |
| Ownership Status | Done | Original-Owned ist Default. Ownership Transfer ist nicht Default und nur per expliziter Decision materialisierbar. |
| CarryLease Status | Done | Lease-Bindung, Recovery, Heartbeat-Grace und Revocation sind in Protocol-Tests abgedeckt. |
| FrameSession Status | Done | FrameOnly-Sessions, Guest-State, Revocation und Input-Policy sind implementiert und getestet. |
| PDF Frame Status | Partial | Sample-PDF, FrameOnly-Modell und Smoke-Test existieren. Echtes plattformnatives PDF-Seitenrendering bleibt naechster Renderer-Schritt. |
| No File Ingress Status | Done | Guest erhaelt keine Originaldatei, keinen Originalpfad und keine Originalbytes im FrameOnly-Pfad. |
| Security Status | Partial | Development Protector, Nonce, Sequence, Lease-/Policy-Bindung und Audit sind getestet. Produktive Kryptografie, Pairing und Trust Stores sind geplant. |
| Surface Abstractions Status | Done | Gemeinsame Surface-Contracts fuer Host, Overlay, Gesture, Haptics, Proximity, FramePresenter, Input, Placement, ObjectAdapter und Security sind vorhanden. |
| Windows Surface Status | Partial | Windows ist primaerer Testpfad mit Native/Glass/Lens-Spikes, Windows Object Source Adapter und PDF Owner Tools. End-to-End Frame Presenter an Native Glass Edge fehlt noch. |
| macOS Surface Status | Planned | Starter Kit, Permissions und FrameGuestSurface-Plan existieren. Native macOS-App muss im macOS/Xcode-Kontext gebaut werden. |
| iOS/iPadOS Status | Planned | Starter Kit, Xcode-Handoff, Touch/Haptik und Surface-App-Plan existieren. Native App fehlt noch. |
| Android Status | Planned | Starter Kit fuer native Surface App, Intent/Content URI/Sharesheet und Haptik existiert. Android Studio/Gradle-Implementierung folgt spaeter. |
| Linux Status | Planned | Starter Kit fuer X11/Wayland/Portals und Headless-/Industriepfade existiert. Minimal Surface Host folgt spaeter. |
| Proximity Status | Done | Simulated Provider, Manual Map Provider, `AblageProximitySource`, Distance/Confidence-Auswahl, Hysterese und Single-Edge-Smoke sind vorhanden. |
| Dongle Status | Planned | Ablage Anchor Dongle ist als Identitaets-/Trust-/Proximity-Anker dokumentiert, aber keine Hardware oder Firmware existiert. |
| Gesture Status | Partial | Surface-Gesture-Contracts existieren. Plattformnative Gesten sind fuer Windows/macOS/iOS/Android noch umzusetzen. |
| Roadmap Status | Done | Roadmap beschreibt Original-Owned, Cross-Platform-Surfaces, Proximity, Dongle und Alpha-Pfad. |
| Context Pack Status | Partial | Export-Skript existiert und wird in AP010 erneut ausgefuehrt. Der konkrete ZIP-Pfad wird nach Export in `CURRENT_CONTEXT` und Release Summary dokumentiert. |

## Was Funktioniert Real

- Lokale .NET-Projekte bauen und testen.
- RKWP Protocol-Tests laufen lokal.
- PDF Frame Smoke nutzt echte Sample-PDF-Datei, aber noch keinen produktiven plattformnativen Renderer.
- Windows Object Source Adapter erkennt lokale Dateireferenzen und haelt Original-Owned.
- Glass Edge/Proximity-Smoke waehlt genau eine Zielkante.

## Was Ist Vorbereitet

- macOS, iOS/iPadOS, Android und Linux haben Starter Kits.
- Manual Map kann reale Raumpositionen vor Sensorik modellieren.
- Dongle/BLE/UWB sind als spaetere Proximity-Quellen eingeordnet.
- Context Pack kann Plattform-Codex-Threads versorgen.

## Was Ist Simuliert

- Cross-Device-Transport.
- Zielablaege ausserhalb Windows.
- Glass Edge Remote-Gegenkante.
- Proximity-Entfernung ausser Manual Map.
- Mobile und macOS FrameGuestSurface.

## Offene Blocker

- Keine produktive Kryptografie.
- Kein reales Pairing/Trust Store.
- Kein echter Cross-Device-Transport zwischen Windows, macOS und iPad.
- Kein nativer PDF-Renderer auf Gastplattformen.
- Keine echte BLE/UWB/Dongle-Hardware.

## Gate Entscheidung

MA007 ist bereit fuer den ersten geplanten Cross-Device-Slice, aber noch nicht fuer produktive Nutzung. Der naechste reale Test sollte Windows als PDF Owner und eine zweite Surface als FrameOnly Guest verbinden, ohne Originaldatei auf dem Guest zu erzeugen.
