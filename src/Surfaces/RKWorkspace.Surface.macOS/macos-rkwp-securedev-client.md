# macOS RKWP SecureDev Client

Status: MA013.02 architecture stub  
Datum: 2026-07-06

## Ziel

SecureDev ist der Entwicklungsmodus fuer authentifizierte Laborverbindungen. Er ersetzt keine produktive Kryptografie, erzwingt aber bereits Identitaet, Trust-Status und Policy-Bindung.

## Startzustand

macOS startet als:

```text
SecurityMode: DevelopmentAuthenticated
TransportProfile: SecureDev/DevLan
SurfaceRole: FrameGuestSurface
```

## Handshake

```text
connect
  -> AblageHello mit Identity
  <- IdentityChallenge / TrustStatus
  -> DevPairing oder KnownAblage proof
  <- SecureSession active
```

## Pflichten

- AblageIdentity mitschicken.
- unbekannte Windows Owner nicht still akzeptieren.
- Revoked/Denied respektieren.
- SessionId und LeaseId binden.
- SequenceNumber und Nonce pruefen, sobald implementiert.
- Policy Binding beachten.
- Audit-Events schreiben.

## Nicht Produktiv

SecureDev darf klar melden:

```text
TLS: NO
TLSStatus: Development fallback
```

Aber macOS darf daraus keine Produktfreigabe ableiten.

## FrameOnly Gate

Auch im SecureDev-Modus gilt:

- kein PDF-Datei-Ingress.
- kein Originalpfad.
- keine Originalbytes.
- Return statt Besitzuebernahme.

## Offene Implementierungsdetails

- finales TLS/mTLS oder Alternative auswaehlen.
- macOS Keychain-Integration fuer produktive Identitaeten.
- Trust UI fuer Pairing.
- Revocation UI.
- Zertifikatrotation.
