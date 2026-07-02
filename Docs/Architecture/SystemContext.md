# RKWS System Context

Dokument-ID: RKWS-ARCH-CONTEXT-001  
Version: 0.2.0  
Status: Accepted  
Datum: 2026-07-02

## Kontext

RK Workspace sitzt zwischen Benutzerinteraktion, Plattformagenten, Arbeitsflaechenmodell und sicherem Transport. Die Architektur ist arbeitsflaechenzentriert.

```mermaid
C4Context
    title RK Workspace System Context
    Person(user, "Benutzer", "Verschiebt digitale Objekte zwischen Arbeitsflaechen")
    System(rkws, "RK Workspace", "Arbeitsflaechen-Erweiterung")
    System_Ext(os, "Betriebssysteme", "Windows, macOS, Linux, iOS, Android")
    System_Ext(dongle, "RKWS Dongle", "Repraesentiert spezielle Arbeitsflaechen")
    System_Ext(network, "Lokales Netzwerk", "LAN/WLAN fuer Payload-Transport")

    Rel(user, rkws, "Geste oder Einrichtung")
    Rel(rkws, os, "Agent/App Integration")
    Rel(rkws, dongle, "Discovery, Pairing, Position")
    Rel(rkws, network, "Sicherer Transport")
```

Falls ein Markdown-Renderer kein C4-Mermaid unterstuetzt, gilt dieselbe Struktur wie in `Docs/01_SystemArchitecture.md`.

## Querverweise

- `Docs/01_SystemArchitecture.md`
- `Docs/Architecture/ArchitectureFreeze.md`
- `Spec/DisplayNodeModel.md`

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 0.2.0 | 2026-07-02 | Dokumentmetadaten und Querverweise ergaenzt. |
| 0.1.0 | 2026-07-02 | Systemkontext angelegt. |
