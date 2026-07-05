# RKWP Security Model

Dokument-ID: RKWS-RKWP-SECURITY-001
Status: Draft
Datum: 2026-07-05

## Ziel

RKWP muss spaeter produktiv gegen Replay, fremde Ablagen, Lease-Missbrauch und unerlaubte Materialisierung geschuetzt werden. MA007.00 implementiert noch keine produktive Kryptografie, legt aber die Pflichtfelder und die Schutzstelle fest.

## Entwicklungsmodus

`DevelopmentRkwpSessionProtector` erzeugt ein nicht-produktives `DEV-` AuthTag. Es dient nur dazu, die Protocol-Pipeline lokal testbar zu machen.

Wichtig:

- Development AuthTags sind keine Security.
- Eine Session mit `SecureSessionRequired` darf nicht vom Development Protector erfuellt werden.
- Produktive Profile muessen spaeter Session-Schluessel, Authenticated Encryption und Replay-Schutz enthalten.

## Mindestanforderungen fuer Produktion

- verschluesselte Session
- gegenseitige Ablage-Authentifizierung
- Nonce/SequenceNumber-Replay-Schutz
- Lease-spezifische Berechtigungen
- Revocation fuer Frame und Carry Lease
- Audit fuer Ownership-Entscheidungen

## Original-Owned Schutz

Das wichtigste Security-Ziel ist nicht Verschluesselung allein. Das wichtigste Ziel ist: Der Gast darf nicht unbemerkt zur neuen Quelle der Wahrheit werden.

Deshalb gilt:

- FrameOnly ist Default.
- Guest Frame enthaelt keine Originaldatei.
- CopyOut, ForkVersion und MoveOwnership brauchen spaeter explizite Bestaetigung und Policy.
- Lease-Verlust fuehrt zur Owner-Recovery.
