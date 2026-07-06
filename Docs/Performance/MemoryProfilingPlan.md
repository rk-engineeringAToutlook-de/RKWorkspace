# RK Workspace

# Memory Profiling Plan

Status: Accepted  
Datum: 2026-07-06

## Ziel

Der erste Real-Pilot darf keine unkontrollierten Frame-, PDF- oder Cache-Reste hinterlassen. Dieses Dokument definiert die Speicherbereiche, die gemessen werden muessen.

## Frame Cache

Der Frame Cache bleibt memory-scoped, solange kein Produkt-Sandboxmodell akzeptiert ist. Nach Return, Revocation, Recovery und Session-Ende muss der Cache leer sein.

## PDF Renderer

Der Renderer darf PDF-Metadaten, gerenderte Seiten und Tiles halten, aber keine freie Originaldatei an Guest-Surfaces weitergeben. Profiling muss First Page, Next Page und Multi-Page Navigation getrennt messen.

## Guest Surface

Guest-Surfaces speichern Frame-Repräsentationen nur so lange, wie Lease und Policy gueltig sind. Offline/Reconnect darf keine verwaisten Frames erzeugen.

## Leaks

Die wichtigsten Leak-Kandidaten sind:

- offene FrameSessions
- nicht beendete CarryLeases
- Tile-Cache ohne Eviction
- Diagnostics/Audit-Handles
- mobile Surface-Preview-Bilder

## Cache Eviction

Eviction-Regeln:

- Return loescht Guest-Frame-Caches.
- Revocation loescht Guest-Frame-Caches sofort.
- Recovery loescht verwaiste Guest-States.
- Memory pressure reduziert Tile-Qualitaet vor Session-Abbruch.
