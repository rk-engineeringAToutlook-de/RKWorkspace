# 04 Communication Protocol

Dokument-ID: RKWS-DOC-04  
Version: 0.2.0  
Status: Accepted  
Datum: 2026-07-02

## Status

Dieses Dokument beschreibt das Kommunikationsmodell, aber noch keine Transportimplementierung. RKWS-0010 verlangt ausdruecklich Dokumentation vor Implementierung. Die aktuelle Codebasis plant Transfers lokal und simuliert den Ablauf, versendet aber noch keine Nutzdaten ueber Netzwerk.

## Trennung der Verantwortungen

Discovery, Positionsbestimmung und Datenuebertragung sind getrennte Subsysteme. Discovery kann lokales Netzwerk und Bluetooth LE nutzen. Positionsbestimmung kann spaeter UWB nutzen. Nutzdaten laufen ueber LAN, WLAN oder spaeter optional ueber einen Cloud-Fallback. Bluetooth LE ist kein Payload-Kanal. UWB ist kein Payload-Kanal.

```mermaid
flowchart TB
    Discovery["Discovery<br/>LAN, mDNS, BLE moeglich"]
    Position["Position<br/>manuelle Karte, spaeter UWB"]
    Transport["Payload-Transport<br/>LAN/WLAN, spaeter Cloud-Fallback"]
    Core["RKWS Core"]

    Discovery --> Core
    Position --> Core
    Core --> Transport
```

## Phasenmodell

Ein spaeterer Transfer besteht aus Discovery, Pairing, Session-Aufbau, Transfer-Ankuendigung, Payload-Uebertragung, Abschluss und Protokollierung. V1 simuliert bisher nur die Entscheidungsschritte ab Arbeitsflaechenmodell und Raumkarte.

## Vorlaeufige Nachrichten

```text
HELLO
WORKSPACE_ADVERTISE
PAIR_REQUEST
PAIR_CONFIRM
PAIR_REJECT
SESSION_START
TRANSFER_OFFER
TRANSFER_ACCEPT
TRANSFER_REJECT
TRANSFER_CHUNK
TRANSFER_COMPLETE
SESSION_CLOSE
```

## Transfer-Metadaten

Ein `TRANSFER_OFFER` soll mindestens die Felder aus `TransferObject` enthalten: ObjectId, ObjectType, SourceWorkspaceId, TargetWorkspaceId, CreatedAt, DisplayName, MimeType, Size, PayloadReference, Checksum und EncryptionInfo. Diese Daten sind ausreichend, um dem Ziel vor der Annahme mitzuteilen, was uebertragen werden soll, von welcher Arbeitsflaeche es kommt, wie gross es ist und wie Integritaet und Verschluesselung behandelt werden.

## Sicherheitsannahmen

Pairing erzeugt Vertrauen zwischen Arbeitsflaechen. Eine Session muss verschluesselt sein. Checksums sichern Integritaet, ersetzen aber keine Authentizitaet. Payloads duerfen nicht allein anhand ihres Dateinamens oder MIME-Types vertraut werden. Ein Ziel muss Transfers ablehnen koennen, wenn Trust-State, Capabilities, Groesse, Richtungszuordnung oder Benutzerbestaetigung nicht passen.

## Offene Entscheidungen

mDNS, BLE-Advertising, UDP-Broadcast oder eine Kombination muessen fuer Discovery bewertet werden. Fuer Session-Sicherheit kommen TLS, Noise oder ein anderes modernes Handshake-Protokoll in Frage. Chunk-Groessen, Resume-Verhalten, Quotas und Konflikte bei mehreren Zielarbeitsflaechen in derselben Richtung sind noch Spezifikationsarbeit.

## Querverweise

- `Spec/Communication.md`
- `Spec/Protocol.md`
- `Docs/ADR/ADR-0004-separate-discovery-and-data-transfer.md`

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 0.2.0 | 2026-07-02 | Normative Protokollspezifikation verlinkt. |
| 0.1.0 | 2026-07-02 | Kommunikationsdokument angelegt. |
