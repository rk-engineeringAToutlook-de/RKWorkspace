# RK Workspace

# Frame Compression Strategy

Status: Accepted  
Datum: 2026-07-06

## Ziel

Frame-Kompression soll Bandbreite und Batterie schonen, ohne den Eindruck eines realen digitalen Gegenstands zu zerstoeren.

## PNG

PNG bleibt fuer scharfe UI, Text, Dokumentkanten und kleine Tiles geeignet. Nachteil ist Groesse.

## JPEG

JPEG ist fuer fotoartige Vorschau denkbar, aber nicht fuer Text oder UI-Kanten. Artefakte duerfen die Lesbarkeit nicht verschlechtern.

## WebP Planned

WebP ist fuer mobile und gemischte Inhalte geplant, muss aber plattformuebergreifend verfuegbar und sicher decodierbar sein.

## zstd Planned

zstd ist fuer Metadaten, JSON-Frames und eventuell Tile-Batches geplant. Es ersetzt keine Bildkompression.

## Tiles

Tiles sind der Standardpfad fuer groessere Dokumente. Sichtbare Tiles haben Prioritaet, Rand-Tiles folgen, unsichtbare Tiles warten.

## Quality Policy

Policy entscheidet, ob ein Frame lesbar, preview-only oder bewegungsoptimiert sein darf. Keine Kompression darf Originaldatei oder Originalpfad zum Guest bringen.
