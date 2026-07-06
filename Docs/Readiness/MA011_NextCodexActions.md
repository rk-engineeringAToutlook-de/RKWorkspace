# MA011 Next Codex Actions

Status: Draft  
Datum: 2026-07-06

## Windows

- Windows Owner fuer macOS real starten.
- `run-windows-owner-for-mac.ps1` mit echter macOS Guest Surface testen.
- Windows PDF Frame UI weiter verbessern.
- SecureDevTransport stabilisieren und DevLan-Pfad haerten.
- PDF Renderer Packaging pruefen.

## macOS

- Context Pack lesen.
- `release/handoff/macOS_Codex_MA009_FrameGuestSurface.md` lesen.
- Native macOS Frame Guest Surface bauen.
- AblageIdentity laden/erzeugen.
- RKWP DevLan/SecureDev Client verbinden.
- FrameSession anzeigen.
- No File Ingress beweisen.
- Return und Heartbeat senden.

## iOS/iPadOS

- Xcode Surface App bauen.
- USB-Test auf iPad/iPhone durchfuehren.
- Local Network Permission pruefen.
- Haptik fuer FrameReady/Return aktivieren.
- Touch-Geste fuer Return testen.
- No File Ingress Probe schreiben.

## Security

- produktive Kryptografie auswaehlen.
- TLS/mTLS oder Noise/QUIC-Variante bewerten.
- Trust Store und Revocation produktiv definieren.
- DevelopmentAuthenticated klar vom Produktpfad trennen.

## PDF

- finalen Renderer auswaehlen.
- PDFium/MuPDF/Poppler Packaging bewerten.
- Speicher- und Cache-Regeln produktiv haerten.
- Annotation/ChangeSet spaeter gegen Renderer testen.

## Proximity

- echte Distanzquellen definieren.
- BLE/UWB/USB/Dongle Kandidaten bewerten.
- ManualMap bleibt Labor-Fallback.

## Dongle

- Hardware-MVP definieren.
- Rolle: Ablage-Anker, Distanzsignal, Trust-Helfer oder Kombination.
- Kein Dongle bauen, bevor der erste echte Windows-to-macOS-Test ausgewertet ist.

## Empfehlung Fuer Naechsten Schritt

Der naechste praktische Schritt ist:

1. Windows Owner starten.
2. macOS-Codex mit Context Pack ausstatten.
3. minimale macOS Guest Surface bauen.
4. echte FrameSession zwischen Windows und macOS pruefen.
5. erst danach iPad/iPhone ueber Xcode anschliessen.
