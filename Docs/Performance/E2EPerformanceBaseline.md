# RK Workspace

# E2E Performance Baseline v2

Status: Accepted  
Datum: 2026-07-06

## Ziel

Diese Baseline beschreibt, welche Performance-Signale fuer den ersten Real-Pilot relevant sind. Sie ersetzt keine Produktmessung auf echter Hardware, aber sie verhindert, dass der RKWP-Frame-Pfad ohne messbare Grundlage weiterentwickelt wird.

## Gemessene Pfade

`tools/run-rkwp-perf.ps1 -SmokeTest` muss folgende Pfade sichtbar melden:

- SecureDev path
- PDF frame path
- Local E2E path
- Frame input path
- Recovery path

## Metriken

- FrameUpdate bytes average
- FrameUpdate frequency
- DevTransport roundtrip
- Heartbeat latency
- Frame open
- Frame return
- Recovery
- No File Ingress overhead
- PDF render first page
- PDF render next page
- PDF tile generation
- PDF frame size
- Memory snapshot

## Bewertung

Der aktuelle Stand ist fuer Labor-Signale geeignet. Er ist noch keine echte FPS-, GPU- oder Netzwerkgarantie fuer Windows, macOS, iOS, Android oder Linux. Das Ziel fuer den naechsten Real-Pilot ist nicht maximale Geschwindigkeit, sondern stabile Wahrnehmung: Frame erscheint, bleibt kontrolliert, laesst sich zurueckgeben und verletzt No File Ingress nicht.

## Gate

Der Performance-Smoke ist erfolgreich, wenn:

- alle Samples erzeugt werden.
- alle E2E-Pfade gemeldet werden.
- PDF-Frame-Metriken vorhanden sind.
- Memory-Snapshot vorhanden ist.
- No File Ingress `SUCCESS` meldet.
- JSON- und Markdown-Berichte geschrieben werden.
