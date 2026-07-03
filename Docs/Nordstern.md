# RK Workspace Nordstern

Dokument-ID: RKWS-NORDSTERN-001
Version: 1.1.0
Status: Accepted
Datum: 2026-07-03

## Hierarchie

Der Nordstern folgt HX-000.

HX-000 definiert den ersten Moment:

> Das ist mein Arbeitsraum.

Der Nordstern beschreibt die daraus folgende Richtung:

> Natuerlich nehme ich das einfach in die Hand und lege es dort ab.

Wenn Nordstern und HX im Widerspruch stehen, gewinnt HX.

## Warum dieses Dokument existiert

Dieses Dokument beschreibt nicht, wie RK Workspace programmiert wird.

Es beschreibt, warum RK Workspace ueberhaupt existiert.

Wenn zukuenftige Architekturentscheidungen, Implementierungen oder Funktionen diesem Dokument widersprechen, ist dieses Dokument wichtiger als der Code, solange es HX-000 nicht widerspricht.

## Unsere Vision

RK Workspace entwickelt keine Software zum Uebertragen von Dateien.

RK Workspace entwickelt eine neue Art, mit digitalen Informationen umzugehen.

Unser Ziel ist, dass digitale Informationen dieselben Eigenschaften bekommen wie physische Gegenstaende.

## Der wichtigste Satz des gesamten Projektes

> Der Benutzer nimmt digitale Dinge in die Hand, traegt sie durch seinen Arbeitsraum und legt sie dort ab, wo er weiterarbeiten moechte.

Nicht uebertragen.

Nicht kopieren.

Nicht verschieben.

Nehmen.

Tragen.

Ablegen.

## Der Benutzer

Der Benutzer arbeitet niemals mit Geraeten.

Der Benutzer arbeitet niemals mit Betriebssystemen.

Der Benutzer arbeitet niemals mit Dateien.

Der Benutzer arbeitet ausschliesslich in seinem Arbeitsraum.

## Geraete

Geraete sind keine Computer.

Geraete sind keine Plattformen.

Geraete sind lediglich Orte, an denen digitale Dinge abgelegt werden koennen.

Geraete sind Ablagen.

Nicht mehr.

Nicht weniger.

## Der digitale Raum

RK Workspace verbindet keine Geraete.

RK Workspace erschafft einen gemeinsamen digitalen Raum.

In diesem Raum existieren:

- digitale Gegenstaende
- Arbeitsflaechen
- Menschen

Nicht:

- Windows
- macOS
- Linux
- iPhone
- Android

Diese Systeme sind technische Details.

Nicht Bestandteil der Benutzererfahrung.

## Digitale Gegenstaende

Digitale Informationen sind keine Dateien.

Sie sind Gegenstaende.

Sie besitzen:

- Bedeutung
- Ort
- Besitzer
- Geschichte

Ein digitaler Gegenstand gehoert niemals einem Geraet.

Er gehoert immer dem Menschen, der ihn gerade traegt.

## Die digitale Hand

Das wichtigste Objekt in RK Workspace ist nicht der Mauszeiger.

Nicht der Cursor.

Nicht das Fenster.

Nicht die Datei.

Das wichtigste Objekt ist die digitale Hand.

Wenn der Benutzer etwas greift, befindet sich dieses Objekt nicht mehr auf einem Geraet.

Es befindet sich in seiner Hand.

## Der Arbeitsraum

Der Arbeitsraum endet nicht am Bildschirmrand.

Der Arbeitsraum endet nicht am Monitor.

Der Arbeitsraum endet nicht am Geraet.

Der Arbeitsraum umfasst alle Arbeitsflaechen, die dem Benutzer momentan zur Verfuegung stehen.

## Uebergaenge

Es existieren keine Transfers.

Es existieren keine Dateiuebertragungen.

Es existieren keine Spruenge.

Es existieren ausschliesslich Uebergaenge.

Digitale Gegenstaende werden durch den Raum getragen.

Nicht zwischen Geraeten kopiert.

## Geraete duerfen verschwinden

Wenn der Benutzer waehrend der Arbeit ueber Geraete nachdenkt, haben wir unser Ziel verfehlt.

Der Benutzer soll ausschliesslich ueber seine Aufgabe nachdenken.

Nicht ueber:

- Bluetooth
- WLAN
- Netzwerk
- Discovery
- Pairing
- Betriebssysteme

## Der digitale Raum ist wichtiger als die Software

Der Core dient dem digitalen Raum.

Die Runtime dient dem digitalen Raum.

Die Agenten dienen dem digitalen Raum.

Discovery dient dem digitalen Raum.

Transport dient dem digitalen Raum.

Der Dongle dient dem digitalen Raum.

Nichts davon ist Selbstzweck.

## Jede neue Funktion muss eine Frage beantworten

Nicht:

Welche Klasse?

Nicht:

Welches Interface?

Nicht:

Welches Protokoll?

Sondern:

> Was soll der Mensch in diesem Moment fuehlen?

Wenn diese Frage nicht beantwortet werden kann, ist die Funktion noch nicht bereit.

## Die wichtigste Architekturregel

Code folgt dem Gefuehl.

Nicht umgekehrt.

## Der wichtigste UX-Leitsatz

Der Benutzer darf niemals denken:

> Ich uebertrage gerade eine Datei.

Er soll denken:

> Ich nehme etwas.

## Unser Massstab

Nicht:

Laeuft der Code?

Nicht:

Sind alle Tests gruen?

Nicht:

Ist die Architektur sauber?

Sondern:

> Hat der Benutzer vergessen, dass er zwischen mehreren Geraeten arbeitet?

Wenn die Antwort "Nein" lautet, ist RK Workspace noch nicht fertig.

## Unser Kompass

Wann immer wir uns im Projekt verlieren, stellen wir nur eine einzige Frage:

> Macht dieser Schritt den digitalen Raum groesser oder bauen wir gerade nur mehr Software?

Wenn wir nur mehr Software bauen, gehen wir in die falsche Richtung.

Wenn wir den digitalen Raum erweitern, gehen wir in die richtige Richtung.

## Unser Versprechen

Wir werden niemals versuchen, bestehende Bedienkonzepte einfach zu kopieren.

Wir werden den Mut haben, eine neue Art des Arbeitens zu entwickeln.

Nicht, weil sie neu ist.

Sondern, weil sie sich natuerlicher anfuehlt.

## Der Nordstern

Wenn irgendwann jeder Benutzer sagt:

> Natuerlich nehme ich das einfach in die Hand und lege es dort ab.

Dann ist RK Workspace fertig.

Nicht frueher.

## Querverweise

- `Spec/HumanExperienceSpecification_HX000.md`
- `Spec/HumanExperienceSpecification_HX001.md`
- `Spec/EmotionSpecification_ES001.md`
- `Spec/EmotionSpecification_ES002.md`
- `Spec/ProductPhilosophy.md`

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.1.0 | 2026-07-03 | HX-000 als uebergeordnete Human Experience vor dem Nordstern verankert. |
| 1.0.0 | 2026-07-03 | Nordstern als oberste Projektorientierung angelegt. |
