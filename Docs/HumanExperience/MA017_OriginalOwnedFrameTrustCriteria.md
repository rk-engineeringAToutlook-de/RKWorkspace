# MA017 Original-Owned Frame Trust Criteria

Status: Draft
Datum: 2026-07-07

## Zweck

Dieses Dokument definiert die Vertrauenskriterien fuer Original-Owned Frames im MA017-Pilot.

Der Owner muss fuehlen und verstehen, dass das Original bei ihm bleibt.

## Vertrauenssatz

```text
Das Ziel sieht und bedient eine Darstellung. Es besitzt nicht mein Original.
```

## Gruen-Kriterien

- Owner kann erklaeren, wo das Original bleibt.
- Guest sieht keinen Dateipfad zur Original-PDF.
- Frame wirkt bedienbar, aber nicht besitzend.
- Rueckweg ist sichtbar oder logisch klar.
- Fehlerfall beruhigt statt zu verunsichern.

## Gelb-Kriterien

- Owner versteht das Modell, braucht aber technische Erklaerung.
- Frame und Kapsel sind noch nicht klar genug getrennt.
- Guest-Rolle ist richtig, aber nicht spuerbar.

## Rot-Kriterien

- Owner glaubt an Dateikopie.
- Owner vermutet Datenverlust oder Kontrollverlust.
- Guest wirkt wie Besitzer.
- Security-Policy und Owner-Gefuehl widersprechen sich.

## No File Ingress

No File Ingress ist nicht nur eine Security-Regel. Es ist ein Vertrauenssignal.

Im Pilot muss daher jede Frame-Session zeigen:

```text
OriginalOwnership: Owner
GuestIngress: None
Policy: Enforced
Audit: Present
```

## Mindestnachweis

```text
OriginalOwnedFrameTrust: Ready
OwnerControl: Preserved
GuestFileIngress: None
RESULT: SUCCESS
```
