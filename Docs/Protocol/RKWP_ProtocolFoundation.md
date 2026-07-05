# RKWP Protocol Foundation

Dokument-ID: RKWS-RKWP-FOUNDATION-001
Status: Draft
Datum: 2026-07-05

## Zweck

RKWP ist das kuenftige Workspace-Protokoll fuer RK Workspace. Es beschreibt nicht "Dateien senden", sondern kontrollierte Beziehungen zwischen Ablagen, digitalen Dingen, Leases und Frames.

MA007.00 legt nur die Foundation:

- versionierte Nachrichten
- Session- und Envelope-Modell
- Carry-Lease-Nachrichten
- Frame-Session-Nachrichten
- Ownership-Transfer-Entscheidungen
- Security-Platzhalter fuer Entwicklung

Noch nicht enthalten:

- echte Netzwerktransporte
- Pairing
- Discovery
- produktive Verschluesselung
- echte OS-Hooks
- Datei-Ingress auf dem Gast

## Nachrichtengrundlage

Der Code liegt in:

```text
src/Protocol/RKWorkspace.Protocol
```

Zentrale Typen:

- `RkwpVersion`
- `RkwpMessageType`
- `RkwpMessage`
- `RkwpEnvelope`
- `RkwpSession`
- `RkwpMessageValidator`
- `IRkwpSessionProtector`

Jede Nachricht besitzt mindestens:

- MessageId
- MessageType
- ProtocolVersion
- SessionId
- SourceAblageId
- TargetAblageId
- Timestamp
- SequenceNumber
- Nonce

Lease-gebundene Nachrichten tragen zusaetzlich `LeaseId`.

## Grundregel

RKWP transportiert in MA007.00 keine Originaldatei in die Gastablage. Ein Gast bekommt eine Darstellung, eine Eingabemoeglichkeit und einen Rueckweg. Das Original bleibt beim Owner.

## Smoke

```powershell
.\tools\run-rkwp-tests.ps1
.\tools\run-pdf-frame-smoke.ps1
```

Beide Skripte muessen mit `RESULT: SUCCESS` enden.
