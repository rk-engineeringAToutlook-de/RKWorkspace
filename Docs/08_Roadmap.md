# 08 Roadmap

Dokument-ID: RKWS-DOC-08  
Version: 0.2.0  
Status: Accepted  
Datum: 2026-07-02

## M0 Foundation

M0 bereitet das Repository fuer den ersten privaten Remote-Push vor. Dazu gehoeren Projektstruktur, README, Produktvision, Systemarchitektur, Spec-Struktur, ADRs, GitHub-Templates, CI-Grundlage, Core-Modelle, lokale Simulation und Unit-Tests. Der Abschluss von M0 ist erreicht, wenn die Simulation A nach B mit vollstaendigem Log laeuft und der Test-Runner gruen ist.

## M1 Requirements and Protocol Specification

M1 schaerft Requirements und Kommunikationsspezifikation. Discovery, Pairing, Session-Sicherheit, Transfer-Angebote, Fehlerfaelle und Logging werden normativ beschrieben. In dieser Phase wird noch kein produktiver Netzwerktransport implementiert, solange Nachrichtenmodell und Sicherheitsannahmen nicht abgenommen sind.

## M2 Local Discovery

M2 implementiert lokale Discovery fuer Smart Devices. Kandidaten sind mDNS, UDP-Broadcast und BLE-Advertising als Ergaenzung. BLE bleibt Discovery und wird nicht fuer Payloads genutzt. Der Meilenstein ist erfolgreich, wenn zwei Arbeitsflaechen im lokalen Netzwerk reproduzierbar gefunden und mit Capabilities angezeigt werden.

## M3 Pairing and Trust

M3 implementiert Pairing, Trust-State-Persistenz, Fingerprint-Anzeige, Ablehnung, Widerruf und erneutes Pairing. Dieser Meilenstein muss vor echtem Payload-Transfer abgeschlossen sein, damit keine ungepaarten Ziele Daten erhalten koennen.

## M4 Secure Text Transfer

M4 uebertraegt Text ueber einen sicheren lokalen Kanal. Der Ablauf nutzt die vorhandene Raumkarte und Richtungserkennung. Logging und Fehlerbehandlung werden so ausgelegt, dass spaeter Datei- und PDF-Transfer darauf aufbauen koennen.

## M5 File, PDF, Image and Link Transfer

M5 erweitert den Payload-Umfang. Dateien, PDFs, Bilder, Links und Ordner werden als Transferobjekte verarbeitet. Checksums, Groesse, MIME-Type, Zielablage, Konfliktbehandlung und Abbruchfaelle werden getestet.

## M6 Platform Agents

M6 beginnt mit dem Windows-Agent und erweitert danach macOS und Linux. Mobile Apps folgen nach Stabilisierung des Desktop-Fundaments. Plattformagenten bleiben Adapter und duerfen den Core nicht mit OS-spezifischen Regeln belasten.

## M7 Dongle and UWB Research

M7 startet Hardware-Experimente. ESP32-S3, UWB-Entwicklungskits, USB-C, sichere Identitaet und Firmware-Updatepfade werden bewertet. PCB-Entwurf beginnt erst nach erfolgreicher Prototypenbewertung.

## M8 Product Hardening

M8 umfasst Installationspfade, Update-Strategie, Diagnose, Recovery, Dokumentation, Security Review und Release-Prozess. Erst danach wird aus dem Prototyp ein produktionsnahes Produkt.

## Diagramm

```mermaid
flowchart LR
    M0["M0 Foundation"] --> M1["M1 Spec"]
    M1 --> M2["M2 Discovery"]
    M2 --> M3["M3 Pairing"]
    M3 --> M4["M4 Text"]
    M4 --> M5["M5 Files"]
    M5 --> M6["M6 Agents"]
    M6 --> M7["M7 Hardware"]
    M7 --> M8["M8 Hardening"]
```

## Querverweise

- `Spec/VersionV0.1.md`
- `Docs/Architecture/ArchitectureFreeze.md`
- `Spec/TestStrategy.md`

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 0.2.0 | 2026-07-02 | V0.1-Definition und Architektur-Freeze verlinkt. |
| 0.1.0 | 2026-07-02 | Roadmap angelegt. |
