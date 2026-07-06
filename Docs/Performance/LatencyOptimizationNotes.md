# RK Workspace

# Latency Optimization Notes

Status: Accepted  
Datum: 2026-07-06

## Ziel

Diese Notizen sammeln die Latenzstellen, die vor dem echten Cross-Device-Pilot beobachtet und spaeter priorisiert werden muessen.

## Transport

SecureDev nutzt noch DevelopmentAuthenticated/Fallback-Pfade. Die naechste Optimierung ist nicht mehr Logik, sondern saubere Messung pro Message: AblageHello, FrameUpdate, Heartbeat, Return und Recovery. Produkttransport muss spaeter TLS/QUIC oder ein vergleichbares lokales Sicherheitsprofil mit niedriger Roundtrip-Latenz erhalten.

## Renderer

PDF-Rendering ist der groesste sichtbare Blocker. First Page muss schnell genug sein, damit der Owner nicht an Technik denkt. Next Page und Tile-Generation duerfen nachladen, muessen aber sichtbar ruhig bleiben.

## Tiling

Tiles reduzieren Payload, erzeugen aber Scheduling-Aufwand. Erste Regel: nur sichtbare Tiles priorisieren, danach angrenzende Tiles vorladen. Keine Originaldatei darf in den Tile-Cache gelangen.

## Compression

Kompression darf die Wahrnehmung nicht verschlechtern. Text und UI-Kanten brauchen Schaerfe, Vorschau und Bewegung duerfen staerker komprimiert werden.

## Input

Scroll, Zoom und Return brauchen Vorrang vor Hintergrund-Frameupdates. Eingaben muessen session-, lease- und policygebunden bleiben.

## Haptics

Haptik darf keine Transportlatenz verstecken. Sie ist Wahrnehmungsfeedback, kein technischer Fortschrittsindikator.
