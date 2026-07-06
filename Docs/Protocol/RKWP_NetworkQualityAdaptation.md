# RK Workspace

# RKWP Network Quality Adaptation

Status: Accepted  
Datum: 2026-07-06

## Ziel

RKWP muss schlechte lokale Netzwerke behandeln, ohne No File Ingress, Lease-Bindung oder Owner-Kontrolle zu schwaechen.

## Low Bandwidth

Bei geringer Bandbreite werden Tile-Qualitaet, Preview-Frequenz und nicht sichtbare Updates reduziert. Control Messages bleiben priorisiert.

## High Latency

Bei hoher Latenz werden Heartbeat-Timeouts adaptiv, aber nicht beliebig. Der Owner muss erkennen, ob ein Frame aktiv, wartend, zurueckgegeben oder verloren ist.

## Frame Rate Reduce

Frame Rate wird nur fuer Darstellung reduziert. Security-, Return-, Revocation- und Recovery-Signale behalten Vorrang.

## Tile Quality

Tiles erhalten Qualitaetsstufen:

- Text/UI scharf
- Vorschau reduziert
- Bewegung reduziert
- Hintergrund niedrig

## Reconnect

Reconnect nutzt AblageIdentity, SessionId, LeaseId, PolicyHash und Audit. Fremde oder alte Sessions duerfen keinen Frame wiederbeleben.

## Recovery

Wenn Reconnect scheitert, gewinnt der Owner. Guest-Surface wird ungueltig, der Owner kann den Frame zurueckholen und Audit muss die Ursache zeigen.
