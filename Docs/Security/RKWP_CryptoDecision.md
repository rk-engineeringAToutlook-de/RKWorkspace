# RKWP Crypto Decision

Status: MA013.21 decision draft  
Datum: 2026-07-06

## Ziel

RKWP braucht eine konkrete Richtung fuer echte Kryptografie. DevTransport bleibt Development. Der Produktpfad braucht einen mutually authenticated secure channel.

## Bewertung

| Option | Bewertung | Ergebnis |
| --- | --- | --- |
| TLS 1.3 | breit unterstuetzt, standardisiert, gute Plattformunterstuetzung | Produktkandidat |
| mTLS | echte gegenseitige Authentisierung, passt zu AblageIdentity | bevorzugter Produktpfad |
| Noise Protocol Framework | stark fuer eigene Protokolle, aber mehr Eigenverantwortung | spaeter pruefen |
| libsodium | gute Primitive, aber kein fertiger Transport | Baustein, nicht allein Transport |
| Platform native crypto | gut fuer Key Storage und Signaturen | Pflicht je Plattform |
| QUIC/TLS | moderner Transport, mobile-freundlich | spaeterer Kandidat |
| WebSocket over TLS | einfacher App-Kompatibilitaetspfad | moeglicher Lab-/Fallbackpfad |
| Certificate pinning | sinnvoll gegen falsche Trust Chains | pruefen |
| Key rotation | Pflicht fuer Produkt | Roadmap |
| Device/Dongle Identity | Bindung an Ablage oder Dongle | Roadmap |

## Entscheidung fuer den naechsten Schritt

- DevTransport bleibt nur Development.
- SecureDev bleibt DevelopmentAuthenticated und ist kein Produktversprechen.
- Produktpfad startet mit mTLS/TLS 1.3 oder aequivalentem mutually authenticated secure channel.
- Finale Wahl bleibt von Plattformtests abhaengig: Windows, macOS, iOS, Android, Linux.

## Plattformkriterien

- Windows: .NET TLS, DPAPI/Credential Manager fuer lokale Secrets.
- macOS/iOS: Keychain fuer private Identitaet.
- Android: Android Keystore.
- Linux: Secret Service/keyring oder produktiver eigener Store.
- Dongle: secure element optional.

## Referenzen

- RFC 8446 TLS 1.3: https://datatracker.ietf.org/doc/html/rfc8446
- Apple Keychain Services: https://developer.apple.com/documentation/security/keychain-services
- Android Keystore System: https://developer.android.com/privacy-and-security/keystore
- Microsoft DPAPI/CryptProtectData: https://learn.microsoft.com/en-us/windows/win32/api/dpapi/nf-dpapi-cryptprotectdata
