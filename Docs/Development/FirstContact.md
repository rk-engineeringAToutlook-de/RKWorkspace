# First Contact

Dokument-ID: RKWS-DEV-FIRST-CONTACT
Version: 1.0.0
Status: Accepted
Datum: 2026-07-03

## Ziel

First Contact beantwortet eine einfache Frage:

```text
Versteht ein neuer Mensch RK Workspace ohne Erklaerung?
```

Der Test ist erfolgreich, wenn eine neue Person innerhalb von 30 Sekunden ein digitales Ding nimmt, nach rechts traegt und dort ablegt.

## Testfluss

Der Test startet im Developer Studio ueber die Registerkarte `First Contact` oder den Button `First Contact`.

Der Startzustand ist bewusst reduziert:

- ein sichtbares Ding
- zwei Arbeitsflaechen
- keine Optionen
- keine Einstellungen
- keine langen Texte
- keine technischen Begriffe wie Agent, Transfer oder Workspace A/B

Die sichtbaren Hinweise sind kurz:

- `Nimm dieses Objekt.`
- `Trage es nach rechts.`
- `Lege es hier ab.`
- `Es liegt jetzt dort.`

## Bediengefuehl

Beim Greifen hebt sich das Ding optisch von der Arbeitsflaeche ab. Es bekommt Schatten, Tiefe und eine leichte Tragephysik. Der Hintergrund wird ruhiger, damit nicht der Cursor, sondern das Ding im Mittelpunkt steht.

Der rechte Bereich wirkt nicht wie ein technisches Ziel, sondern wie eine Arbeitsflaeche zum Weiterarbeiten. Der Zwischenraum deutet einen Durchgang an.

Beim Ablegen erscheint kein Popup und keine technische Erfolgsmeldung in der Testflaeche. Das Ding liegt sichtbar rechts.

## Messung

Die First-Contact-Session misst lokal:

- Zeit bis zum ersten Greifen
- Zeit bis zum erfolgreichen Ablegen
- Fehlversuche
- Abbrueche
- unnoetige Klicks
- Erfolg innerhalb von 30 Sekunden

Die Werte bleiben lokal im laufenden Studio. Es gibt keine Cloud, keine Netzwerkfunktion und keine produktive Telemetrie.

## Owner Test

Der Owner beobachtet den ersten Lauf ohne technische Einweisung. Entscheidend ist nicht, ob der Ablauf technisch korrekt ist, sondern ob er sich sofort verstaendlich anfuehlt.

Die Leitfragen bleiben:

1. Hat die Person verstanden, dass sie etwas nehmen kann?
2. Hat sie das Ding nach rechts getragen?
3. Hat sie es dort abgelegt, ohne nach einer Transferfunktion zu suchen?

## Smoke-Test

Der Studio-Smoke-Test prueft den First-Contact-Pfad viewmodelbasiert:

```powershell
.\tools\run-studio.ps1 -SmokeTest
```

Erwartete Signale:

```text
FirstContactStarted: SUCCESS
FirstContactGrip: SUCCESS
FirstContactPlace: SUCCESS
FirstContactMetrics: SUCCESS
FirstContact: SUCCESS
RESULT: SUCCESS
```

## Grenzen

- Kein echter neuer Benutzer wird automatisch simuliert.
- Keine echte Blick- oder Mauspfad-Auswertung.
- Keine gespeicherte Studie.
- Keine echte Monitorerkennung.
- Keine OS-weite Interaktion.
- Keine Aenderung an Core, Runtime, Transfer Engine, Agent, IPC, Transport oder Discovery.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-03 | MA005.04 First Contact dokumentiert. |
