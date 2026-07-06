# RK Workspace

# Performance Dashboard Plan

Status: Accepted  
Datum: 2026-07-06

## Ziel

Das Dashboard soll Performance fuer Entwickler sichtbar machen, ohne ein Produkt-UI zu werden.

## Panels

- Latency: Transport, Heartbeat, FrameUpdate, Return, Recovery.
- FPS: aktive Surface, ruhende Surface, mobile Surface.
- Frame size: FrameUpdate, Tile, PDF first page, next page.
- Memory: Frame cache, renderer, Guest Surface, Diagnostics.
- Lease count: active, returned, expired, recovered.
- Errors: policy denied, reconnect failed, renderer blocked, no file ingress violation.

## Quelle

Erste Quelle sind `run-rkwp-perf.ps1`, `run-rkwp-load.ps1`, Diagnostics-Audit und spaeter echte Plattform-Telemetrie. Keine personenbezogenen Inhalte, keine Dateiinhalte und keine Originalpfade duerfen im Dashboard erscheinen.
