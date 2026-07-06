# RKWP DevLan Transport

Status: MA010.02  
Profil: Development/Lab only

## Zweck

`LocalNetworkDev` bereitet den ersten Windows-zu-macOS-Lab-Test vor. Der Transport traegt RKWP-Nachrichten ueber TCP im lokalen Netz, entscheidet aber nicht ueber Besitz, Lease, Frame, Policy oder No File Ingress.

## Nicht Produktiv

DevLan ist nicht produktiv:

- kein finales TLS
- kein finaler Trust Store
- kein produktives Pairing
- nur Lab/Development
- nicht fuer kritische Umgebungen

Produktive Pfade muessen spaeter ueber Secure Session, gegenseitige Ablage-Authentifizierung, Zertifikatsspeicher, Replay-Schutz, Policy Binding und Audit-Gates laufen.

## Start

Owner:

```powershell
.\tools\run-rkwp-lan-owner.ps1 -Port 57100 -AllowDevPairing
```

Guest:

```powershell
.\tools\run-rkwp-lan-guest.ps1 -Host 192.168.x.x -Port 57100
```

Smoke:

```powershell
.\tools\run-rkwp-lan-smoke.ps1
```

## Unterstuetzte Felder

- BindAddress
- Host
- Port
- AblageId
- DisplayName
- DevIdentity
- SessionId
- DevPairingAllowed
- SecureSessionRequired als vorbereiteter Schalter
- HeartbeatInterval
- GracePeriod
- AllowedPeerIds in Sample-Konfigurationen

## Smoke-Flow

Der Smoke prueft:

1. Owner Server startet.
2. Guest Client verbindet.
3. `AblageHello`.
4. `AblageCapabilities`.
5. DevPairing.
6. SecureSession-Spike ist vorbereitet, DevSecurity-Warnung ist sichtbar.
7. Heartbeat.
8. FrameUpdate.
9. Disconnect.
10. `RESULT: SUCCESS`.

## No File Ingress

DevLan transportiert im Smoke nur RKWP Frame-Nachrichten. `FrameUpdate` bestaetigt `noFileIngress=true`; Originaldateien, Originalpfade und PDF-Bytes werden nicht als Datei auf der Guest-Ablage materialisiert.

## Konfiguration

Sample-Dateien:

```text
config/samples/rkwp-dev-lan-owner.sample.json
config/samples/rkwp-dev-lan-guest.sample.json
```

Lokale echte Konfigurationen duerfen nicht mit Secrets oder echten Peer-Daten in Git landen.
