# RKWP Pairing

Dokument-ID: RKWS-RKWP-PAIRING-001
Status: Draft
Datum: 2026-07-05

## Ziel

Pairing verbindet zwei Ablagen nicht als Geraete, sondern als Orte im Arbeitsraum. Eine Ablage darf ein digitales Ding erst erleben, wenn Identitaet, Trust und Policy zusammenpassen.

## V1 Scope

MA008.02 implementiert nur die Grundlage:

- Pairing-Zustaende
- Pairing-Anfrage
- Pairing-Entscheidung
- Dev-only Pairing-Service
- Trust-Gate fuer Lease und Frame

Noch nicht enthalten:

- produktive Kryptografie
- echter Trust Store
- UI fuer Owner-Bestaetigung
- Cloud-Sync
- Discovery
- automatische Geraetekopplung

## Ablauf im Dev-Modus

1. Eine Ablage sendet `AblageHello` mit Identitaet.
2. Die Gegenseite liest `AblageCapabilities`.
3. Eine Dev-Pairing-Anfrage wird erzeugt.
4. Der Owner oder Test entscheidet.
5. Approved fuehrt zu `DevTrusted` und `Paired`.
6. Denied fuehrt zu `Untrusted` und `Denied`.
7. Das Trust-Gate entscheidet danach ueber Lease und Frame.

## Sicherheitsregel

Pairing ist keine automatische Freigabe. Selbst eine gepaarte Ablage darf nur das tun, was die aktive `AblageTrustPolicy` erlaubt.

Wenn `RequireSecureSession` aktiv ist, reicht `DevelopmentInsecure` nicht aus.

## Beziehung zu Proximity

Proximity bestimmt die naechste Ablage. Pairing bestimmt, ob sie vertrauenswuerdig genug ist.

Eine Ablage kann nah sein und trotzdem keine Lease bekommen.

## Beziehung zu Ownership

Pairing uebertraegt kein Original. FrameOnly bleibt Default. CopyOut, ForkVersion oder MoveOwnership bleiben eigene bewusste Entscheidungen und duerfen nicht aus Pairing abgeleitet werden.
