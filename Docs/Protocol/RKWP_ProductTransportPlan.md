# RKWP Product Transport Plan

Status: MA013.26 plan  
Datum: 2026-07-06

## Ziel

DevTransport reicht nicht fuer Produktbetrieb. RKWP braucht einen echten sicheren Transport.

## Kandidaten

| Transport | Nutzen | Risiko |
| --- | --- | --- |
| TCP/TLS | breit verfuegbar | Firewall/NAT |
| mTLS | gegenseitige Authentisierung | Zertifikatsmanagement |
| WebSocket/TLS | App-/Proxy-freundlich | Semantik muss sauber bleiben |
| QUIC/TLS | mobile und reconnect-freundlich | Plattformreife pruefen |

## Produkt-Mindestanforderungen

- gegenseitige Authentisierung.
- Replay-Schutz.
- Session Binding.
- Policy Binding.
- Audit Binding.
- Revocation.
- No File Ingress bleibt unabhaengig vom Transport.

## Netzwerk

- Local Network als erster Produktpfad.
- Firewall-Prompts pro Plattform dokumentieren.
- NAT spaeter.
- Offline: keine neue Lease, nur Recovery/Return-Regeln.

## Enterprise Policy

Enterprise kann Transportprofile erlauben oder blockieren.

## Naechster Spike

Ein ProductTransport-Spike soll mTLS/TLS 1.3 mit AblageIdentity und Trust Store verbinden.
