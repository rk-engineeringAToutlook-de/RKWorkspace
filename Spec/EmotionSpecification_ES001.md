# Emotion Specification ES-001

Dokument-ID: RKWS-SPEC-EMOTION-ES001
Version: 1.1.0
Status: Accepted
Datum: 2026-07-03

## Status

Fuehrendes UX-Dokument.

Keine Implementierung.

Keine GUI-Aenderung.

Keine Architekturaenderung.

Dieses Dokument definiert das erste zentrale Gefuehl von RK Workspace.

## Leitsatz

RK Workspace uebertraegt keine Daten.

RK Workspace laesst Menschen digitale Dinge nehmen, tragen und ablegen.

## Zielgefuehl

Der Benutzer soll in dem Moment des Greifens glauben:

> Ich habe dieses digitale Ding jetzt in meiner Hand.

Nicht:

> Ich ziehe ein Objekt.

Nicht:

> Ich verschiebe eine Datei.

Nicht:

> Ich benutze Drag & Drop.

Sondern:

> Ich halte etwas.

## Mentales Modell

Vor dem Greifen muessen HX-000 und HX-001 erfuellt sein: Der Benutzer erlebt einen Arbeitsraum, und das Objekt wirkt nicht wie eine Datei, sondern wie etwas, das gerade zur Arbeit des Benutzers gehoert.

Ein digitales Objekt liegt zunaechst auf einer Arbeitsflaeche.

Beim Greifen verlaesst es diese Arbeitsflaeche.

Es gehoert in diesem Moment nicht mehr dem Geraet.

Es gehoert nicht mehr dem Fenster.

Es gehoert nicht mehr der App.

Es befindet sich in der digitalen Hand des Benutzers.

## Wahrnehmungsziele

Der Benutzer muss unmittelbar wahrnehmen:

1. Das Objekt hat sich geloest.
2. Das Objekt ist jetzt hervorgehoben.
3. Die Arbeitsflaeche tritt in den Hintergrund.
4. Das Objekt wirkt nicht mehr flach.
5. Das Objekt wirkt getragen, nicht gezogen.
6. Der Cursor ist nicht mehr Mittelpunkt.
7. Die Aufmerksamkeit liegt vollstaendig auf dem Objekt.

## Was ausdruecklich vermieden werden muss

Der Greifmoment darf nicht wirken wie:

- normales Drag & Drop
- Dateiverschieben
- Auswahlrahmen
- Markierung
- Windows-Standardverhalten
- Programmfensterinteraktion
- technische Animation
- dekorativer Effekt
- Programmierfehler

Wenn der Benutzer denkt:

> Das ist eine Animation.

ist das Experiment gescheitert.

Wenn der Benutzer denkt:

> Ich habe es.

ist das Experiment erfolgreich.

## Physische Analogie

Der Moment soll sich eher anfuehlen wie:

- ein Blatt Papier vom Tisch nehmen
- eine Karte aus einem Stapel ziehen
- etwas aus einem Korb nehmen
- etwas kurz in der Hand halten
- etwas weiterreichen

Nicht wie:

- eine Datei kopieren
- ein Fenster verschieben
- einen Button druecken
- ein Icon ziehen

## Visuelle Prinzipien

Beim Greifen darf das Objekt:

- leicht kleiner werden
- sich vom Hintergrund loesen
- Schatten bekommen
- Tiefe bekommen
- eine minimale Federbewegung haben
- eine minimale Traegheit zeigen
- leicht ruhig pulsieren
- optisch vom Untergrund getrennt werden

Aber:

Es darf nicht verspielt wirken.

Es darf nicht wie ein Effekt wirken.

Es darf nicht hektisch wirken.

Es darf nicht blinken.

## Digitale Haptik

Da keine echte Haptik vorhanden ist, muss eine optische Haptik erzeugt werden.

Optische Haptik bedeutet:

Der Benutzer sieht eine Reaktion, die sein Gehirn als Widerstand, Griff oder Kontakt interpretiert.

Moegliche Mittel:

- kurzes Nachgeben beim Greifen
- leichtes Zusammenziehen
- Objekt folgt minimal verzoegert
- weicher Schatten
- kurze Stabilisierung nach dem Greifen

Nicht:

- grelles Leuchten
- starkes Blinken
- aggressive Animation
- zufaelliges Wackeln

## Verhalten beim Greifen

Der Greifmoment besteht aus drei Phasen.

### Phase 1 - Beruehren

Der Benutzer beginnt die Interaktion.

Das Objekt reagiert sehr subtil.

Signal:

> Ich bin greifbar.

### Phase 2 - Loesen

Das Objekt loest sich von der Arbeitsflaeche.

Signal:

> Ich bin nicht mehr Teil dieser Ablage.

### Phase 3 - Halten

Das Objekt stabilisiert sich in der digitalen Hand.

Signal:

> Du haeltst mich jetzt.

## Erfolgsbedingungen

ES-001 ist erfolgreich, wenn der Owner sagt:

- Ich habe nicht mehr das Gefuehl, eine Karte anzuklicken.
- Ich habe nicht mehr das Gefuehl, eine Datei zu ziehen.
- Ich habe das Gefuehl, dass sich das Objekt aus der Oberflaeche geloest hat.
- Ich habe das Gefuehl, dass ich das Objekt jetzt halte.
- Ich denke nicht mehr an das Fenster oder die App.

## Testfragen fuer den Owner

Nach jedem Experiment beantwortet der Owner nur diese Fragen:

1. Fuehlte es sich an, als haette ich etwas wirklich gegriffen?
2. Wirkte das Objekt in meiner Hand oder nur am Cursor?
3. Hat sich die Arbeitsflaeche fuer einen Moment unwichtig angefuehlt?
4. War die Reaktion natuerlich oder kuenstlich?
5. Hat irgendetwas wie ein Programmierfehler gewirkt?

Bewertung:

- Gruen: Das fuehlt sich richtig an.
- Gelb: Fast, aber noch nicht.
- Rot: Falsch.

## Codex-Regeln

Codex darf aus dieser Spezifikation noch keine finale Loesung ableiten.

Codex soll Varianten erzeugen.

Nicht optimieren.

Nicht entscheiden.

Nicht bewerten.

Codex baut Experimente.

Der Owner bewertet das Gefuehl.

## Naechster Schritt nach diesem Dokument

Nach Freigabe von ES-001 folgt:

```text
UX-Experiment ES-001-A
```

Ziel:

Fuenf unterschiedliche Greifvarianten bauen, die ausschliesslich dieses Gefuehl testen:

> Ich habe etwas in meiner Hand.

Keine Randlogik.

Kein Uebergang.

Kein Ablegen.

Nur Greifen.

## Querverweise

- `Docs/Nordstern.md`
- `Spec/HumanExperienceSpecification_HX000.md`
- `Spec/HumanExperienceSpecification_HX001.md`
- `Spec/UX.md`
- `Spec/ProductPhilosophy.md`
- `Docs/Development/DigitalPhysicsSprint.md`
- `Docs/Development/FirstContact.md`

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.1.0 | 2026-07-03 | HX-000 als vorausgehende Arbeitsraum-Wahrnehmung ergaenzt. |
| 1.0.0 | 2026-07-03 | ES-001 als erste Emotion Specification angelegt. |
