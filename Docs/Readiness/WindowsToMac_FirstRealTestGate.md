# Windows to macOS First Real Test Gate

Status: MA013.10 readiness gate  
Datum: 2026-07-06

## Ziel

Dieses Gate fuehrt die Voraussetzungen fuer den ersten echten Windows-zu-macOS-Test zusammen.

## Startreihenfolge

1. Windows Owner vorbereiten.
2. macOS Guest aus aktuellem Context-Pack bauen.
3. Beide Ablagen im gleichen Netzwerk verbinden.
4. AblageIdentity pruefen.
5. DevPairing ausloesen.
6. PDF Frame oeffnen.
7. Return und Recovery pruefen.

## Windows Owner

```powershell
.\tools\run-windows-pdf-owner-securedev.ps1 -SmokeTest
```

Fuer den echten Test nutzt Windows spaeter:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -InfoOnly
```

## macOS Guest

macOS liest:

- `release/schema/rkwp-envelope-schema-v0.1.json`
- `contracts/macOS-guest/rkwp-macos-guest-contract-v0.1.json`
- `release/handoff/macOS_Codex_MA013_RKWPClient.md`

## Netzwerk

- gleicher Layer-2/LAN-Bereich bevorzugt.
- Port: `57100` fuer Windows Owner Plan.
- Firewall auf Windows und macOS pruefen.
- Local Network Permission auf macOS bestaetigen.

## PDF Path

Windows haelt den Originalpfad. macOS darf den Pfad nicht sehen und nicht speichern.

## RKWP Flow

Pflichtfolge:

```text
AblageHello
AblageCapabilities
DevPairing
SecureDevHandshake
FrameSessionOpen
FrameUpdate
Heartbeat
Return
Revocation/Recovery optional
```

## Erfolgskriterien

```text
WindowsOwner: Running
MacGuest: Running
DevPairing: SUCCESS
SecureDevHandshake: OK
FrameSessionOpen: OK
FrameUpdate: OK
PDF liegt hier im Frame.
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
NoFileIngress: SUCCESS
Return: SUCCESS
Recovery: SUCCESS
```

## Blocker

- macOS native App noch nicht gebaut.
- Produktiver TLS-Transport fehlt.
- echte macOS Local-Network-Prompts muessen auf Hardware bestaetigt werden.
- PDF-Frame-Performance muss im echten Netzwerk gemessen werden.
