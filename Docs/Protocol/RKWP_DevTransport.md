# RKWP Dev Transport

Dokument-ID: RKWS-RKWP-DEVTRANSPORT-001  
Status: Draft  
Datum: 2026-07-05

## Zweck

MA008.01 fuehrt den ersten lokalen Dev-Transport fuer RKWP-End-to-End-Tests ein. Er verbindet zwei Ablagen kontrolliert, ohne die RKWP-Semantik zu veraendern.

Der Transport entscheidet nicht ueber:

- Ownership
- FrameOnly
- Policy
- CopyOut
- MoveOwnership
- ChangeSet
- Recovery

Diese Entscheidungen bleiben im RKWP-Protokoll.

## Gewaehlter V1-Transport

V1 nutzt `NamedPipeDev`.

Warum:

- lokal reproduzierbar
- keine Browser-UI
- kein Produkt-Sicherheitsversprechen
- gut fuer Windows Owner + Windows Guest
- kompatibel mit bestehender Transport-Abstraction

Vorbereitete Modi:

- `LocalLoopback`
- `LocalNetworkDev`
- `NamedPipeDev`
- `WebSocketDev`
- `FutureTcpTls`
- `FutureUsbDongle`

## Komponenten

- `IRkwpTransport`
- `IRkwpTransportServer`
- `IRkwpTransportClient`
- `RkwpTransportMessage`
- `RkwpTransportConnection`
- `RkwpTransportDiagnostics`
- `RkwpTransportException`
- `RKWorkspace.Transport.Dev`
- `RKWorkspace.RkwpTransportHarness`

## Smoke-Test

Script:

```powershell
.\tools\run-rkwp-transport.ps1 -SmokeTest
```

Der Smoke prueft:

- Server startet.
- Client verbindet.
- `AblageHello` wird gesendet.
- `AblageCapabilities` wird ausgetauscht.
- `SessionId` wird vereinbart.
- `CarryLeaseHeartbeat` wird gesendet.
- `FrameUpdate` wird gesendet.
- Error Message wird gesendet.
- Disconnect wird erkannt.
- Timeout wird erkannt.
- falsche Message erzeugt Fehler.
- Smoke-Test haengt nicht.

## CLI

```powershell
.\tools\run-rkwp-transport.ps1 -Server
.\tools\run-rkwp-transport.ps1 -Client
.\tools\run-rkwp-transport.ps1 -SmokeTest
```

Optional:

```powershell
.\tools\run-rkwp-transport.ps1 -Server -Endpoint rkws-rkwp-dev
.\tools\run-rkwp-transport.ps1 -Client -Endpoint rkws-rkwp-dev
```

## Security-Hinweis

`NamedPipeDev` ist nicht produktiv.

Produktiv braucht:

- TLS oder gleichwertige Verschluesselung
- gegenseitige Ablage-Authentisierung
- Session Keys
- Replay-Schutz
- Policy Binding
- Audit
- Revocation
- Trust Store

DevelopmentInsecure darf nie als produktiver Modus ausgegeben werden.

## Bedeutung Fuer Cross-Device

Der Dev-Transport ist der erste Schritt zum echten Test:

1. Windows Owner + Windows Guest lokal.
2. Windows Owner + macOS Guest.
3. Windows Owner + iPad Guest ueber macOS/Xcode.

Er beweist nur Leitung und Nachrichtenfluss. Er beweist noch keine produktive Security und kein echtes Plattform-Handoff.
