# RKWS Communication Specification

Dokument-ID: RKWS-SPEC-COMM-001  
Version: 0.2.0  
Status: Accepted  
Datum: 2026-07-02

## Zweck

Dieses Dokument beschreibt die Kommunikationsprinzipien. Das normative Nachrichtenmodell steht in `Spec/Protocol.md`.

## Separation

Discovery, Position und Payload-Transport sind getrennt. BLE kann Discovery unterstuetzen. UWB kann Position unterstuetzen. Nutzdaten laufen ueber LAN, WLAN oder spaeter optional ueber Cloud-Fallback.

## Diagramm

```mermaid
flowchart TB
    Discovery["Discovery"] --> Core["Core"]
    Position["Position"] --> Core
    Core --> Protocol["Protocol"]
    Protocol --> Transport["Payload Transport"]
```

## Protocol Phases

1. Discovery
2. Announcement
3. Heartbeat
4. Capability Exchange
5. Pairing
6. Authentication
7. Encryption
8. Workspace Advertisement
9. Target Selection
10. Transfer Request
11. Transfer Accept or Reject
12. Transfer Progress
13. Transfer Finished or Failed
14. Version Negotiation
15. Error Handling and Retry

## Implementation Status

Noch keine echte Kommunikation ist implementiert. Der aktuelle Stand simuliert Transferplanung lokal.

## Querverweise

- `Spec/Protocol.md`
- `Spec/SecurityModel.md`
- `Docs/ADR/ADR-0004-separate-discovery-and-data-transfer.md`

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 0.2.0 | 2026-07-02 | Auf RKWS-0240 und `Spec/Protocol.md` erweitert. |
| 0.1.0 | 2026-07-02 | Erste Kommunikationsspezifikation angelegt. |
