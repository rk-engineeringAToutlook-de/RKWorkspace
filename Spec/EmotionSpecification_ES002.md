# Emotion Specification ES-002

Dokument-ID: RKWS-SPEC-EMOTION-ES002
Version: 1.0.0
Status: Accepted
Datum: 2026-07-03

## Status

Fuehrendes UX-Dokument.

Keine Implementierung.

Keine GUI-Aenderung.

Keine Architekturaenderung.

Dieses Dokument definiert das zweite zentrale Gefuehl von RK Workspace.

## Leitsatz

RK Workspace uebertraegt keine Daten.

RK Workspace laesst Menschen digitale Dinge nehmen, tragen und ablegen.

## Zielgefuehl

Der Benutzer soll nach dem Greifen nicht denken:

> Das Objekt haengt am Cursor.

Sondern:

> Ich trage dieses digitale Ding durch meinen Arbeitsraum.

## Unterschied zu ES-001

ES-001 beschreibt den Moment:

> Ich habe etwas in meiner Hand.

ES-002 beschreibt die Bewegung danach:

> Ich bewege mich mit etwas in meiner Hand durch den Raum.

Greifen ist Besitz.

Tragen ist Bewegung mit Verantwortung.

## Mentales Modell

Vor dem Tragen muessen HX-001 und ES-001 erfuellt sein: Das Objekt gehoert zur aktuellen Arbeit und befindet sich bereits in der digitalen Hand.

Das Objekt befindet sich nicht mehr auf einer Arbeitsflaeche.

Es befindet sich in der digitalen Hand des Benutzers.

Waehrend des Tragens gehoert es keinem Geraet, keiner App und keiner Ablage.

Der Benutzer bewegt nicht eine Datei.

Der Benutzer traegt einen digitalen Gegenstand.

## Wahrnehmungsziele

Der Benutzer muss spueren:

1. Das Objekt bleibt bei mir.
2. Es ist nicht am Cursor festgeklebt.
3. Es hat leichte Traegheit.
4. Es folgt meiner Bewegung natuerlich.
5. Der Hintergrund ist weniger wichtig.
6. Ich kann mich mit dem Objekt im Raum bewegen.
7. Das Objekt besitzt waehrend des Tragens Praesenz.

## Was vermieden werden muss

Das Tragen darf nicht wirken wie:

- normales Drag & Drop
- Mauszeiger mit Icon
- schwebendes Windows-Element
- Dateiverschieben
- technischer Effekt
- ruckelnde Animation
- uebertriebene Spielerei
- blinkender Hinweis

Wenn der Benutzer denkt:

> Ich ziehe etwas.

ist das Experiment nicht gelungen.

Wenn der Benutzer denkt:

> Ich trage etwas.

ist das Experiment erfolgreich.

## Digitale Traegheit

Tragen braucht digitale Traegheit.

Das Objekt darf dem Cursor nicht exakt 1:1 folgen.

Es soll leicht verzoegert reagieren.

Aber:

- niemals schwammig
- niemals langsam
- niemals unpraezise
- niemals frustrierend

Die Traegheit soll psychologisch sein, nicht stoerend.

## Digitale Haptik beim Tragen

Da keine echte Haptik vorhanden ist, muss das Tragen optisch spuerbar werden.

Moegliche Mittel:

- leichte Federbewegung
- dezenter Schatten
- minimale Verzoegerung
- stabile Objektform
- sanftes Nachziehen
- ruhiger Hintergrund

Nicht:

- starkes Wackeln
- hektische Bewegung
- starkes Pulsieren
- grelles Leuchten

## Raumgefuehl

Waehrend des Tragens soll der Benutzer nicht mehr an Fenster denken.

Der Arbeitsraum soll wichtiger wirken als einzelne Apps.

Geraete sind nur Ablagen.

Der digitale Gegenstand wird durch den Raum getragen.

Nicht zwischen Fenstern verschoben.

## Phasen des Tragens

Das Tragen besteht aus drei Phasen.

### Phase 1 - Stabilisierung

Nach dem Greifen stabilisiert sich das Objekt in der digitalen Hand.

### Phase 2 - Bewegung

Das Objekt folgt der Handbewegung mit natuerlicher Traegheit.

### Phase 3 - Zielsuche

Der Raum darf moegliche Ablagen andeuten, ohne den Benutzer zu ueberfordern.

## Erfolgsbedingungen

ES-002 ist erfolgreich, wenn der Owner sagt:

- Es fuehlt sich nicht mehr wie Ziehen an.
- Das Objekt wirkt nicht am Cursor festgeklebt.
- Ich habe das Gefuehl, dass ich es mitnehme.
- Die Bewegung wirkt natuerlich.
- Ich denke weniger an Fenster oder Geraete.
- Ich fuehle, dass ich mich mit dem Objekt durch meinen Raum bewege.

## Testfragen fuer den Owner

Nach jedem Experiment beantwortet der Owner:

1. Fuehlte es sich an, als wuerde ich etwas tragen?
2. Wirkte das Objekt zu stark am Cursor befestigt?
3. War die Traegheit angenehm oder stoerend?
4. Hat das Objekt Praesenz?
5. Habe ich an Geraete/Fenster gedacht oder an meinen Arbeitsraum?

Bewertung:

- Gruen: Das fuehlt sich richtig an.
- Gelb: Fast, aber noch nicht.
- Rot: Falsch.

## Codex-Regeln

Codex entscheidet nicht, welche Variante richtig ist.

Codex erzeugt Experimente.

Der Owner entscheidet nach Gefuehl.

ES-002 ist normativ.

Zukuenftige Carry-/Drag-/Move-Implementierungen muessen mit ES-002 vereinbar sein.

## Querverweise

- `Docs/Nordstern.md`
- `Spec/HumanExperienceSpecification_HX001.md`
- `Spec/EmotionSpecification_ES001.md`
- `Spec/UX.md`
- `Spec/ProductPhilosophy.md`
- `Docs/Development/DigitalPhysicsSprint.md`
- `Docs/Development/WorkspaceExperienceLab.md`
- `Docs/Development/FirstContact.md`

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-03 | ES-002 als zweite Emotion Specification angelegt. |
