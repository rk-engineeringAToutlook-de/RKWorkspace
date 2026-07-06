# RKWP Secure Session Path

Status: Draft  
Datum: 2026-07-06  
Arbeitsauftrag: MA011.01

## Ziel

Der Secure Session Path trennt Development-, Test- und Production-Sicherheit deutlicher als der bisherige Laborpfad. RK Workspace darf weiterhin DevelopmentInsecure fuer lokale Smokes nutzen, aber jede Policy mit SecureSessionRequired und jede Production-Umgebung lehnt diesen Modus ab.

## Security Modes

| Mode | Zweck | Produktion |
| --- | --- | --- |
| `DevelopmentInsecure` | lokale Labor- und Smoke-Tests mit sichtbarer Warnung | nein |
| `DevelopmentAuthenticated` | strukturell authentisierte Development-Session | nein |
| `TestSecure` | SecureDevTransport- und Test-Sicherheitsprofil | nein |
| `ProductionSecure` | vorbereitetes produktives Sicherheitsprofil | geplant |
| `ProductionRequired` | Policy verlangt produktionsfaehigen Schutz | ja, sobald echte Kryptografie steht |

Die alten Namen `Authenticated`, `Encrypted` und `EncryptedAndAuthenticated` bleiben als Kompatibilitaetsalias erhalten.

## Session States

Der Zielzustand fuer produktive Sitzungen ist:

```text
Uninitialized
HandshakeStarted
IdentityExchanged
Authenticated
SessionKeyPrepared
Active
```

`SessionKeyEstablished` bleibt als Kompatibilitaetsalias erhalten.

## Guard-Regeln

`RkwpSecureSessionPath` prueft:

- DevelopmentInsecure ist nur erlaubt, wenn die Umgebung es explizit erlaubt.
- SecureSessionRequired blockiert DevelopmentInsecure.
- Production blockiert DevelopmentInsecure.
- Heartbeat und Revocation muessen an eine aktive authentisierte Session gebunden sein.
- Nonce und SequenceNumber werden ueber `RkwpReplayProtectionState` geprueft.
- LeaseId und PolicyId muessen zur aktiven Session passen.

## Was Noch Nicht Produktiv Ist

- echte TLS-/Cipher-Suite-Auswahl
- produktive Zertifikatskette
- echter Trust Store
- produktive Revocation-Verteilung
- Pairing UI fuer nicht-technische Owner

## Verifikation

Pflichtbefehle:

```powershell
.\tools\run-rkwp-tests.ps1
.\tools\run-tests.ps1
```

Die Tests decken Development/Production-Gate, SecureSessionRequired, Nonce/Sequence-Replay, Lease/Policy-Bindung sowie unauthentisierte Heartbeat-/Revocation-Ablehnung ab.

## MA011.03 SecureDevTransport

Der erste Transport, der den Secure Session Path sichtbar zusammenfuehrt, ist `SecureDev`.

Aktuell real:

- Owner- und Guest-Ablage-Identitaeten aus dem lokalen Identity Store
- gegenseitiger Identity Exchange
- `RkwpSecureSession.EstablishDevelopment`
- `SecurityMode: DevelopmentAuthenticated`
- `SecureSessionRequired: true`
- Heartbeat und FrameUpdate innerhalb der ausgehandelten Session

Noch nicht produktiv:

- TLS ist noch nicht aktiv.
- Der Transport nutzt `NamedPipeDev` als klar markierten Fallback.
- Der produktive Zertifikats-/Trust-Pfad ist vorbereitet, aber nicht abgeschlossen.

Pflichtscript:

```powershell
.\tools\run-rkwp-securedev-smoke.ps1
```
