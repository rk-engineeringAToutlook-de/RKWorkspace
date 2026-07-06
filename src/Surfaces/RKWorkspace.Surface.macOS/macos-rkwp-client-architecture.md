# macOS RKWP Client Architecture

Status: MA013.02 architecture stub  
Datum: 2026-07-06

## Ziel

Der macOS RKWP Client verbindet eine macOS Ablage mit einem Windows Owner. macOS ist im ersten Test Guest und zeigt nur einen Frame.

## Komponenten

```text
MacGuestApp
  -> AblageIdentityStore
  -> RkwpClientRuntime
  -> DevTransportClient
  -> SecureDevSession
  -> FrameSessionReceiver
  -> FramePresenter
  -> HeartbeatLoop
  -> ReturnController
  -> AuditLogWriter
```

## AblageIdentity

macOS erzeugt oder laedt eine lokale `AblageIdentity` aus dem App-Support-Verzeichnis:

```text
~/Library/Application Support/RKWorkspace/identity/ablage-identity.json
```

Die Identitaet wird nicht in Git eingecheckt und nicht in das Context Pack exportiert.

## Client Runtime

Die Runtime verwaltet:

- Verbindungszustand.
- aktuelle SessionId.
- aktuelle LeaseId.
- aktuellen FrameSession-Status.
- letzte Heartbeat-Zeit.
- letzten Return-Status.
- No-File-Ingress-Pruefstatus.

## No File Ingress

Der Client darf:

- Frame-Repräsentationen im Speicher anzeigen.
- Metadaten und Audit-Events schreiben.

Der Client darf nicht:

- PDF-Datei speichern.
- Originalpfad anzeigen.
- Originalbytes als Datei schreiben.
- freie macOS-Datei aus dem Frame erzeugen.

## Audit

macOS schreibt ein lokales Dev-Auditlog:

```text
~/Library/Logs/RKWorkspace/rkwp-macos-guest.jsonl
```

Pflichtfelder:

- timestamp.
- event.
- sessionId.
- leaseId falls vorhanden.
- frameSessionId falls vorhanden.
- sourceAblageId.
- targetAblageId.
- noFileIngressStatus.

Keine Originalbytes im Log.
