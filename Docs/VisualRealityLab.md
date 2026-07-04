# Visual Reality Lab

Dokument-ID: RKWS-VISUAL-REALITY-LAB
Version: 1.3.0
Status: Accepted
Datum: 2026-07-04

## Ziel

Das Visual Reality Lab ist ein isolierter nativer Windows-Spike fuer lebendige Ablage-Linsen ueber dem echten Desktop.

Es ist kein Produkt.

Es ist kein Web-Prototyp.

Es ist kein Developer Studio.

Es testet nur eine Frage:

```text
Wirkt eine Ablage-Linse wie eine raeumliche Oeffnung?
```

Der erste WinForms/GDI+-Spike hat diese Frage fuer den Owner nicht positiv beantwortet. Er bleibt als technischer Smoke-Test erhalten, ist aber kein gueltiger visueller Zielpfad mehr.

## Projekt

```text
src/Shell/RKWorkspace.Shell.VisualReality.Windows
```

Der neutrale Shell-Core bleibt frei von Windows-Renderinglogik. Das Lab referenziert nur die Shell Runtime und kapselt die WinForms-Darstellung im Windows-Projekt.

## Start

```powershell
.\tools\run-visual-reality.ps1
```

Smoke-Test:

```powershell
.\tools\run-visual-reality.ps1 -SmokeTest
```

## Bedienung

- `Ctrl + Alt + Space`: Ding greifen.
- Linke Maustaste auf `Rechnung.pdf`: Ding greifen.
- `1`: Glasbrunnen-Portal.
- `2`: Glasmaterial.
- `3`: Raumbrunnen.
- `4`: Ruhiges Portal.
- `5`: Minimaler Raumriss.
- `Esc`: Overlay sicher beenden.

## Sichtbarer Ablauf

```text
Desktop bleibt sichtbar
Ding liegt ueber dem Desktop
Greifen
Linsen tauchen langsam auf
Ding antwortet vektoriell
Linse wird bei Naehe lesbar
Linse oeffnet sich
Mini-Ablage erscheint
Ding gleitet in die Linse
Ghost kommt im Ziel heraus
```

## Smoke-Test

Der Smoke-Test prueft:

- natives Overlay startet.
- kein Browser/WebView.
- Desktop bleibt sichtbar.
- fuenf Linsenvarianten existieren.
- Varianten sind umschaltbar.
- langsames Erscheinen ist vorbereitet.
- Linsen besitzen subtile Lebendigkeit.
- Referenzrichtung Glaslinse plus Gravitationsbrunnen plus ruhiges Portal ist vorbereitet.
- Ding wird kompakter und teilverdeckt.
- Ding reagiert vektorbasiert.
- diagonale Bewegung wird unterstuetzt.
- Linse oeffnet sich.
- Mini-Ablage wird sichtbar.
- Ding gleitet in die Linse.
- Ghost kommt auf Zielseite heraus.
- `Esc` beendet sicher.

Erwartete Ausgabe:

```text
ReferenceDirection: OK
VisualRealitySmoke: SUCCESS
RESULT: SUCCESS
```

## Owner-Bewertung Nach Ersttest

Der technische Smoke-Test war erfolgreich.

Der visuelle Owner-Test war nicht erfolgreich.

Owner-Bewertung:

```text
Totale grafische Katastrophe.
```

Diese Bewertung ist verbindlich. Sie bedeutet:

- nicht weiter an derselben C#-Darstellung polieren.
- nicht den Anspruch an die Linse reduzieren.
- zuerst Blueprint, visuelle Mockups, Storyboards oder Renderer-Entscheidung klaeren.

Der aktuelle Spike beweist nur:

```text
Ein natives transparentes Testfenster kann gestartet und automatisch geprueft werden.
```

Er beweist nicht:

```text
Die Ablage-Linse fuehlt sich echt an.
```

## Abgrenzung

Das Lab ersetzt nicht:

- Spatial Room Smoke-Test.
- Native Spatial Overlay Smoke-Test.
- Developer Studio.
- Shell Runtime Host.

Das Lab bleibt als technischer Experimentierraum erhalten. Nach dem Owner-Test ist der konkrete WinForms/GDI+-Spike aber kein akzeptierter Human-Experience-Pfad fuer visuelle Realitaet.

Der naechste gueltige Schritt ist in `Docs/VisualRealityBlueprint.md` definiert: fuenf ernsthafte Linsenrichtungen, drei digitale-Hand-Varianten, drei Storyboards, Zielvision fuer den Portaluebergang und eine Renderer-Entscheidung.

## Owner-Referenzboard

Die Owner-Referenzen sind dauerhaft gesichert in:

```text
Docs/Assets/VisualReality/
```

Sie definieren die neue visuelle Richtung:

```text
Glaslinse
+
Gravitationsbrunnen
+
ruhiges Portal
```

Diese Richtung ersetzt die bisherige Bubble-/Status-/Effektoptik. Die Referenzen duerfen nicht als Weltraumhintergrund verstanden werden. Der reale Desktop bleibt sichtbar; die Linse erzeugt nur Tiefe und Oeffnung im Raum.

## Implementierter Slice Nach Referenzboard

Der aktuelle Slice baut die Richtung erstmals sichtbar:

- Variante 1: Glasbrunnen-Portal als Standard.
- Variante 2: Glasmaterial als ruhige Materialprobe.
- Variante 3: Raumbrunnen als Einzugs-/Tiefenprobe.
- Variante 4: ruhiges Portal als Oeffnungsprobe.
- Variante 5: minimaler Raumriss als reduzierte Gegenprobe.

Die bisherige Seifenblasen-/Wasser-Optik ist nicht mehr der Implementierungsstandard.

## Owner-Test

Der Owner prueft ausschliesslich:

1. Sehe ich noch App/Web/Browser?
2. Wirken die Linsen hochwertiger als gruene Punkte?
3. Leben die Linsen?
4. Entsteht Tiefe?
5. Oeffnet sich eine Linse wie ein Portal?
6. Wird das Ding beim Greifen kompakter/teilverdeckt?
7. Gleitet das Ding in die Linse?
8. Kommt es auf der anderen Seite heraus?
9. Gibt es wenigstens einen Moment von Raumgefuehl?

## Bekannte Grenzen

- Renderer ist WinForms/GDI+ und nicht final.
- Visueller Owner-Test des ersten Spikes nicht bestanden.
- Keine weitere C#-UI-Politur ohne vorheriges Entscheidungs-Gate.
- Per-Pixel-Transparenz, echte Brechung, Blur und Shader sind nur angedeutet.
- Linsenpositionen sind simuliert.
- Keine echte Monitor- oder Raumvermessung.
- Keine echte Payload.
- Keine echte mobile Haptik.
- Keine finalen Materialien.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.3.0 | 2026-07-04 | Visual-Reality-Slice in Richtung Glasbrunnen-Portal implementiert, Tastenbelegung aktualisiert und ReferenceDirection-Smoke dokumentiert. |
| 1.2.0 | 2026-07-04 | Owner-Referenzboard als neue visuelle Richtung fuer Glaslinse, Gravitationsbrunnen und ruhiges Portal verankert. |
| 1.1.0 | 2026-07-04 | Owner-Bewertung des ersten WinForms/GDI+-Spikes als visuellen Fehlschlag dokumentiert und naechstes Entscheidungs-Gate auf den Visual Reality Blueprint verlagert. |
| 1.0.0 | 2026-07-04 | MA006.09 Visual Reality Lab mit fuenf Living-Lens-Hypothesen dokumentiert. |
