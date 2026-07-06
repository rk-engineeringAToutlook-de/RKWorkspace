# macOS Guest Compatibility Harness

Status: MA010.04

## Zweck

Der macOS Guest Compatibility Harness ist kein nativer macOS-Agent. Er ist eine Windows-lauffaehige Spezifikation und ein Protokoll-Smoke fuer den spaeteren macOS Guest.

Er prueft, ob der Windows Owner einen macOS-aehnlichen Guest ueber RKWP DevLan bedienen kann, ohne dass die Zielablage eine freie PDF-Datei erhaelt.

## MA013 Contract-Test

Der Contract-Test macht die spaetere macOS-Kommunikation vor der nativen App verbindlich:

```powershell
.\tools\run-mac-guest-contract.ps1
.\tools\export-rkwp-schema.ps1
```

Die Contract-Datei liegt unter `contracts/macOS-guest/rkwp-macos-guest-contract-v0.1.json`. Sie beschreibt die Reihenfolge `AblageHello`, `AblageCapabilities`, `DevPairing`, `SecureDevHandshake`, `FrameSessionOpen`, `FrameUpdate`, optionales `FrameInput`, `Heartbeat`, `Return`, `Revocation` und `Error`.

Der Test validiert:

- Pflichtfelder und Payload-Schluessel.
- RKWP-MessageTypes gegen den Schema-Export.
- Reihenfolge der macOS-Guest-Flows.
- No-File-Ingress-Verbote fuer Originalpfad, PDF-Bytes und Downloadpfade.
- sichtbare Sprache ohne technische Produktbegriffe.

## Start

Smoke:

```powershell
.\tools\run-mac-guest-compat.ps1 -SmokeTest
```

Replay ohne Socket:

```powershell
.\tools\run-mac-guest-compat.ps1 -ReplaySample
```

Verbindung zu einem laufenden Windows Owner:

```powershell
.\tools\run-mac-guest-compat.ps1 -ConnectToOwner rkwp+tcp-dev://<windows-ip>:57100
```

## Simulierte macOS-Faehigkeiten

- `Platform: MacOS`
- `FrameView: OK`
- `FrameInput: OFF`
- `Haptics: PLANNED`
- `GlassEdge: PLANNED`
- `NoFileIngressCapability: OK`
- `OwnershipTransfer: OFF`

## Smoke-Ablauf

1. macOS Guest Identity wird erzeugt.
2. Capabilities werden gemeldet.
3. lokaler Owner wird simuliert.
4. `AblageHello` wird gesendet.
5. `AblageCapabilities` werden ausgetauscht.
6. `FrameSessionReady` wird bestaetigt.
7. Heartbeat wird bestaetigt.
8. Return / FrameClose wird bestaetigt.
9. No File Ingress wird geprueft.

## No File Ingress

Der Harness meldet nur dann Erfolg, wenn gilt:

```text
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
```

## Offene echte macOS-Punkte

- native macOS App fehlt.
- Xcode/AppKit/Swift-Implementierung fehlt.
- echte Local-Network-/Firewall-Prompts muessen auf macOS getestet werden.
- produktive TLS/mutual auth fehlt.
- echter PDF-Frame-Renderer ist noch nicht final entschieden.
