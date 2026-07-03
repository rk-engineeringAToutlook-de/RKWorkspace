# Human Experience Specification HX-000

Dokument-ID: RKWS-SPEC-HUMAN-EXPERIENCE-HX000
Version: 1.0.0
Status: Accepted
Datum: 2026-07-03

## Status

Normatives Fuehrungsdokument.

Dieses Dokument steht ueber:

- README
- Nordstern
- Architektur
- ADR
- UX
- GUI
- Code

Wenn Code und HX im Widerspruch stehen, gewinnt immer HX.

## Warum HX existiert

RK Workspace wird nicht aus Sicht eines Computers entwickelt.

RK Workspace wird aus Sicht eines Menschen entwickelt.

Alle technischen Entscheidungen dienen ausschliesslich dazu, eine menschliche Erfahrung entstehen zu lassen.

## Der erste Moment

Bevor ein Benutzer etwas greifen kann, muss er einen Arbeitsraum erleben.

Nicht mehrere Geraete.

Nicht mehrere Fenster.

Nicht mehrere Betriebssysteme.

Sondern:

> Einen einzigen Raum.

## Ziel

Der Benutzer soll beim ersten Blick nicht denken:

> Ich arbeite mit Windows.

Nicht:

> Ich arbeite mit macOS.

Nicht:

> Ich arbeite mit einem Handy.

Sondern:

> Das ist mein Arbeitsraum.

## Was der Benutzer wahrnimmt

Der Benutzer sieht:

Arbeitsflaechen.

Nicht Geraete.

Der Benutzer sieht:

Ablagen.

Nicht Computer.

Der Benutzer sieht:

Moeglichkeiten.

Nicht Programme.

## Geraete

Geraete besitzen keine Bedeutung.

Sie existieren.

Aber sie sind nicht Mittelpunkt.

Sie sind lediglich Orte, an denen digitale Dinge abgelegt werden koennen.

## Der digitale Raum

Der Arbeitsraum endet niemals am Monitor.

Er endet niemals am Fenster.

Er endet niemals am Betriebssystem.

Der Arbeitsraum ist groesser.

Alle spaeteren Funktionen muessen dieses Gefuehl unterstuetzen.

## Was ausdruecklich vermieden werden muss

Der Benutzer darf niemals denken:

- Jetzt arbeite ich auf dem Mac.
- Jetzt arbeite ich auf Windows.
- Jetzt arbeite ich auf dem Handy.
- Jetzt muss ich uebertragen.
- Jetzt muss ich synchronisieren.

Wenn solche Gedanken entstehen, ist HX-000 nicht erfuellt.

## Erfolgsbedingung

HX-000 ist erfuellt, wenn der Benutzer sagt:

> Ich arbeite einfach.

Nicht:

> Ich arbeite auf Geraet A.

Nicht:

> Ich arbeite auf Geraet B.

## Architekturregel

Alle zukuenftigen Funktionen muessen zuerst beantworten:

> Unterstuetzt diese Funktion meinen Arbeitsraum?

Oder:

> Erinnert sie mich wieder an Geraete?

Wenn sie an Geraete erinnert, muss sie ueberarbeitet werden.

## Testfragen

Der Owner beantwortet ausschliesslich:

1. Habe ich gerade an Geraete gedacht?
2. Habe ich an Fenster gedacht?
3. Habe ich an Betriebssysteme gedacht?
4. Oder hatte ich das Gefuehl, dass alles einfach mein Arbeitsraum ist?

## Fuer Codex

Vor jeder neuen Implementierung ist zu pruefen:

Welche HX wird durch diese Funktion unterstuetzt?

Wenn keine HX unterstuetzt wird, wird nicht implementiert.

## Der Nordstern

RK Workspace beginnt nicht am Bildschirm.

RK Workspace beginnt nicht am Geraet.

RK Workspace beginnt nicht im Betriebssystem.

RK Workspace beginnt in der Wahrnehmung des Menschen.

## Querverweise

- `Docs/Nordstern.md`
- `Spec/HumanExperienceSpecification_HX001.md`
- `Spec/UX.md`
- `Spec/ProductPhilosophy.md`
- `Docs/Architecture/README.md`
- `Docs/ADR/README.md`

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-03 | HX-000 als oberste Human Experience Specification angelegt. |
