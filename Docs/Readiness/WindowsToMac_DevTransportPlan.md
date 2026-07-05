# Windows to macOS DevTransport Plan

Status: Prepared
Datum: 2026-07-05

## Ziel

Der erste echte Cross-Device-Test verbindet einen Windows Owner mit einer macOS Guest Ablage.

Der Test beweist nicht Dateiuebertragung. Er beweist:

- Windows bleibt Owner der PDF.
- macOS zeigt nur einen Frame.
- No File Ingress bleibt gueltig.
- DevPairing und AblageIdentity begrenzen die Zielablage.
- Rueckgabe, Heartbeat und Recovery sind nachvollziehbar.

## Rollen

| Rolle | Plattform | Aufgabe |
| --- | --- | --- |
| Windows Owner | Windows | registriert Original-Owned PDF, erstellt CarryLease und FrameSession |
| macOS Guest | macOS | zeigt FrameGuestSurface ohne Originaldatei |
| RKWP DevTransport | Development | transportiert Hello, Capabilities, Lease, FrameUpdate, Heartbeat und Return |
| DevPairing | Development | koppelt macOS nur als bewusst freigegebene Ablage |

## Aktuelle Windows-Basis

Der Windows-Pfad ist lokal verifiziert:

```powershell
.\tools\run-windows-local-frame-e2e.ps1 -SmokeTest
```

Dieser Smoke prueft Windows Owner, Windows Guest, DevPairing, `NamedPipeDev`, PDF FrameOnly, No File Ingress, Rueckgabe und Recovery.

## DevTransport Status

Aktiv implementiert:

- `NamedPipeDev`
- lokaler Windows-End-to-End-Smoke
- RKWP Nachrichtenfluss
- `AblageHello`
- `AblageCapabilities`
- `CarryLeaseHeartbeat`
- `FrameUpdate`
- `CarryLeaseReturn`

Noch offen fuer echten Windows-zu-macOS-Test:

- Netzwerkfaehiges DevTransport-Profil fuer Cross-Device, zum Beispiel TCP/WebSocket im lokalen LAN.
- macOS Guest Surface.
- macOS RKWP Client/Adapter.
- produktive Session Protection.

## AblageIdentity und Pairing

Der macOS Guest muss eine eigene AblageIdentity senden:

- AblageId
- DisplayName
- Platform: macOS
- Capabilities: FrameOnly, PdfGuest, NoFileIngress, Return, Heartbeat
- PairingState: PairingRequested oder Paired

Windows darf eine Lease erst erstellen, wenn die macOS Ablage mindestens fuer den Dev-Test freigegeben ist.

Unknown, Untrusted, Revoked, Denied und Pending blockieren Lease und Frame.

## No File Ingress

macOS darf im FrameOnly-Pfad nicht erhalten:

- Originalpfad der Windows-PDF
- Originalbytes der Windows-PDF
- freie lokale PDF-Datei
- stillen Ownership-Wechsel

Erlaubt sind:

- FrameSessionId
- ThingId
- Metadaten
- ContentHash
- Frame-Repraesentation
- policygebundene Input-Events
- ChangeSet-Vorschlaege

## Notwendige Ports

Der aktuelle Windows-Smoke nutzt `NamedPipeDev` und benoetigt keinen Netzwerkport.

Fuer den ersten echten Cross-Device-Test wird ein lokaler Development-Port reserviert:

```text
TCP 43707
```

Dieser Port ist noch Planungsstand, bis ein netzwerkfaehiges DevTransport-Profil implementiert ist.

## Firewall-Hinweise

Windows:

- Eingehende Verbindung fuer den spaeteren DevTransport-Port erlauben.
- Nur privates lokales Netzwerk verwenden.
- Kein Internet-Freigabepfad.
- DevMode klar sichtbar loggen.

macOS:

- Lokales Netzwerk erlauben.
- App Sandbox/Network Client Berechtigung pruefen.
- Firewall-Prompt dokumentieren.
- Keine Dateiablage fuer FrameOnly.

## macOS Build-Hinweise

macOS-Codex soll zuerst eine minimale FrameGuestSurface bauen:

1. Swift/AppKit oder SwiftUI Minimal-App starten.
2. AblageIdentity erzeugen.
3. DevPairing anfordern.
4. RKWP FrameUpdate entgegennehmen.
5. Frame ohne Originaldatei anzeigen.
6. Scroll/Zoom/Annotation nur policygebunden als Input/ChangeSet senden.
7. Return ausloesen.
8. No File Ingress im Log beweisen.

## Testschritte

1. Windows Owner vorbereiten:

```powershell
.\tools\run-windows-owner-for-mac.ps1
```

2. macOS Guest mit Context Pack starten:

```text
release/codex-context/RKWorkspace_Context_latest.zip
```

3. macOS liest Handoff:

```text
release/handoff/WindowsToMac_MA008_Handoff.md
```

4. macOS sendet `AblageHello`.
5. Windows prueft AblageIdentity und Pairing.
6. Windows erstellt FrameOnly CarryLease.
7. Windows sendet FrameUpdate.
8. macOS zeigt Frame.
9. macOS bestaetigt No File Ingress.
10. macOS sendet Heartbeat.
11. macOS sendet Return oder ChangeSet.
12. Windows schliesst Lease/Frame oder Recovery.

## Erwartete Logs

Windows:

```text
OwnerAblageId: ablage-windows-owner
ExpectedGuestPlatform: macOS
TransportProfile: NamedPipeDev local, network profile pending
SecurityStatus: DevMode
NoFileIngress: REQUIRED
```

macOS:

```text
AblageHello: OK
DevPairing: Requested/Paired
FrameSession: Active
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
Return: SUCCESS
```

## Erfolgskriterien

- Windows bleibt Owner.
- macOS ist als Ablage identifiziert.
- DevPairing ist bewusst freigegeben.
- FrameOnly ist aktiv.
- macOS speichert keine Originaldatei.
- Heartbeat ist sichtbar.
- Rueckgabe oder Recovery ist erfolgreich.
- Alle Logs enthalten SessionId, LeaseId und FrameSessionId.

## Blocker

- macOS native FrameGuestSurface fehlt.
- Cross-Device DevTransport ist noch nicht implementiert.
- PDF-Renderer ist aktuell noch als RendererBlocked markiert.
- produktive Security ist nicht aktiv.
- Firewall/Local-Network Permissions muessen auf echter Hardware getestet werden.
