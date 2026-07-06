# RKWP SecureDev Transport

Status: Draft  
Datum: 2026-07-06  
Arbeitsauftrag: MA011.03

## Ziel

`SecureDevTransport` verbindet den bisherigen Development-Transport mit Ablage-Identitaeten und dem Secure Session Path. Er ist ein Spike fuer echte sichere Kommunikation, aber noch kein produktiver TLS-Transport.

## Aktuelles Profil

| Feld | Wert |
| --- | --- |
| TransportProfile | `SecureDev/NamedPipeDevFallback` |
| SecurityMode | `DevelopmentAuthenticated` |
| SecureSessionRequired | `true` |
| TLS | nein |
| Produktion | nein |

Der Spike nutzt lokal `NamedPipeDev` als Traeger. Die RKWP-Sitzung wird strukturell gegenseitig authentisiert: Owner- und Guest-Ablage-Identitaeten werden aus dem lokalen Ablage Identity Store geladen, ausgetauscht und in eine `RkwpSecureSession` gebunden.

## Smoke-Test

```powershell
.\tools\run-rkwp-securedev-smoke.ps1
```

Der Smoke prueft:

- Owner Identity vorhanden
- Guest Identity vorhanden
- Transport startet
- Guest verbindet
- `AblageHello`
- Identity Exchange
- SecureDev Handshake
- Session active
- Heartbeat
- FrameUpdate
- Shutdown
- klar markierter TLS-Fallback

## Scripts

```powershell
.\tools\run-rkwp-securedev-owner.ps1
.\tools\run-rkwp-securedev-guest.ps1
.\tools\run-rkwp-securedev-smoke.ps1
```

Owner/Guest zeigen aktuell den vorbereiteten Status. Der vollstaendige lokale Durchlauf liegt im Smoke-Test, damit AP043 reproduzierbar ohne zwei manuell gestartete Prozesse bleibt.

## TLS-Blocker

Echtes TLS ist in diesem Arbeitspaket noch nicht umgesetzt. Der Grund ist nicht ein Protokollproblem, sondern der noch fehlende produktive Zertifikats- und Trust-Store:

- keine produktive Zertifikatskette
- keine finale Zertifikatsrotation
- keine plattformuebergreifende Trust-Verteilung
- kein finaler Revocation-Pfad fuer echte Geraete
- noch keine OS-spezifischen TLS-Hosts fuer macOS/iOS/Windows

Deshalb ist der Fallback verpflichtend sichtbar:

```text
TLS: NO
TransportProfile: SecureDev/NamedPipeDevFallback
SecurityMode: DevelopmentAuthenticated
```

## Nicht-Ziele

- keine Dateiuebertragung
- kein produktives Pairing
- keine produktive Verschluesselung
- keine Discovery
- kein OS-Zertifikatsspeicher

Der Zweck ist, Transport, Identity, Secure Session und No File Ingress in einem testbaren Development-Pfad zusammenzufuehren.
