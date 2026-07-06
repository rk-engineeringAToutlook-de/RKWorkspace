# MA016 Pilot Feedback Taxonomy

Status: Draft
Datum: 2026-07-07

## Zweck

Dieses Dokument definiert die Feedback-Sprache fuer MA016 Owner-Tests. Feedback wird nicht als technischer Fehlerbericht verstanden, sondern als Human-Experience-Beobachtung.

## Grundregel

Jede Beobachtung wird genau einer Feedback-Kategorie zugeordnet. Wenn eine Beobachtung mehrere Ebenen beruehrt, gewinnt die Ebene, die der Owner zuerst wahrgenommen hat.

## Kategorien

### Raumgefuehl

Der Owner beschreibt, ob RK Workspace wie ein zusammenhaengender Arbeitsraum wirkt.

Typische Saetze:

- Ich denke nicht an Geraete.
- Ich weiss, wohin das Objekt gehoert.
- Die Kante fuehlt sich wie eine Oeffnung an.

### Objektbesitz

Der Owner beschreibt, ob das Objekt ihm gehoert, waehrend er es nimmt oder traegt.

Typische Saetze:

- Das Objekt bleibt bei mir.
- Ich habe Kontrolle.
- Es fuehlt sich nicht wie Kopieren an.

### Vertrauen

Der Owner beschreibt, ob er glaubt, dass das Original geschuetzt bleibt.

Typische Saetze:

- Ich habe keine Angst, die Datei zu verlieren.
- Ich verstehe, wo das Original liegt.
- Der offene Frame fuehlt sich sicher an.

### Naehe und Richtung

Der Owner beschreibt, ob die vorgeschlagene Ablage logisch wirkt.

Typische Saetze:

- Die richtige Kante reagiert.
- Die Richtung ist nachvollziehbar.
- Die falsche Ablage lenkt mich nicht ab.

### Unterbrechung und Recovery

Der Owner beschreibt, ob ein Abbruch, Timeout oder Verlust beherrschbar wirkt.

Typische Saetze:

- Ich weiss, dass ich zurueck kann.
- Das System laesst mich nicht allein.
- Der Zustand wirkt aufraeumbar.

### Plattformgefuehl

Der Owner beschreibt, ob Windows, macOS, iPad oder iPhone als Arbeitsflaechen statt als technische Geraete erlebt werden.

Typische Saetze:

- Das iPad wirkt wie eine Ablage.
- Der Mac fuehlt sich nicht wie ein fremdes System an.
- Ich arbeite einfach weiter.

## Nicht erlaubte Kategorien

Diese Begriffe duerfen nur als technische Notiz im Nachgang stehen, nicht als primaere Owner-Bewertung:

- Paketverlust
- Latenz
- Renderer
- Transport
- Zertifikat
- Prozess
- Socket

## Auswertung

Jede Testnotiz enthaelt:

- Kategorie
- Owner-Satz
- Bewertung: Gruen, Gelb oder Rot
- betroffene HX
- naechster Experimentvorschlag

