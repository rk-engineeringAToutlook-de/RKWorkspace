# RKWP Mutual Authentication Model

Status: MA013.22 model  
Datum: 2026-07-06

## Ziel

Ablagen muessen sich gegenseitig authentisieren, bevor ein kritisches Ding als Frame erscheint.

## Modell

Jede Ablage besitzt:

- AblageIdentity.
- Public Key.
- Private Key im Plattform-Secret-Store.
- optionales Zertifikat.
- Trust State.
- Pairing State.

## Trust Store

Der Trust Store enthaelt:

- bekannte AblageId.
- Public Key oder Zertifikat-Fingerprint.
- Trust Level.
- Ablaufdatum.
- Revocation State.
- Pairing Metadata.

## Pairing

Pairing ist die menschliche Freigabe fuer technischen Trust.

Phasen:

1. unknown.
2. pairing requested.
3. pending.
4. paired.
5. denied.
6. revoked.

## Rotation und Expiration

- Zertifikate/Keys laufen ab.
- Rotation wird vor Ablauf angekuendigt.
- alte Keys bleiben nur fuer Recovery-Zeitfenster gueltig.
- revoked Keys duerfen keine neue Lease erhalten.

## Dongle Identity

Ein spaeterer Dongle kann eine zusaetzliche Vertrauenswurzel sein. Er ersetzt nicht automatisch Nutzerbestaetigung.

## Enterprise Trust

Enterprise kann Trust vorprovisionieren. Produkt-UX muss trotzdem anzeigen, welche Ablage vertrauenswuerdig ist.

## Dev Trust

Dev Trust bleibt sichtbar als Development. Er darf nicht mit Production Trust verwechselt werden.
