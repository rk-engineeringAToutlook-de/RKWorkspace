# iPad and iPhone Surface Test Plan

Status: Prepared
Datum: 2026-07-05

## Ziel

iPad und iPhone werden als mobile Ablagen im Arbeitsraum vorbereitet.

Der erste Test beweist:

- Windows bleibt Owner der PDF.
- iPad/iPhone zeigt nur einen RKWP Frame.
- iPad/iPhone speichert keine freie PDF-Datei.
- Haptik und Glass Edge sind vorbereitet.
- Rueckgabe und Recovery bleiben nachvollziehbar.

## Grundsatz

iPad und iPhone haben keinen eigenen Codex. Die native Surface App wird ueber macOS-Codex und Xcode gebaut.

PWA ist nur ein Uebergang fuer fruehe Sichttests. Der Produktpfad ist eine native RK Workspace Surface App.

## Mobile Ablage

Die mobile App ist keine Dateiverwaltung und kein Sync-Ziel. Sie ist eine Ablage im Arbeitsraum.

Erste Rolle:

- FrameGuestSurface
- mobile Glass Edge am Rand
- haptische Rueckmeldung
- Touch-Geste fuer Nehmen/Tragen/Ablegen
- No File Ingress Proof

## Minimaler erster iPad-Test

Szenario:

1. Windows besitzt `samples/Objects/Rechnung.pdf`.
2. iPad zeigt einen PDF-Frame.
3. iPad bekommt keine PDF-Datei.
4. Windows bleibt Owner.
5. iPad sendet Heartbeat.
6. iPad gibt den Frame zurueck.
7. Windows bestaetigt Return oder Recovery.

Erfolgskriterien:

- `GuestHasPdfFile: NO`
- `GuestHasOriginalPath: NO`
- `OriginalFileBytes: NO`
- `NoFileIngress: SUCCESS`
- `FrameSession: Active`
- `Return: SUCCESS`

## Xcode ueber macOS-Codex

macOS-Codex soll:

1. Context Pack lesen.
2. Xcode-Projekt anlegen.
3. iOS/iPadOS Surface App starten.
4. USB-Testgeraet auswaehlen.
5. Local Network Permission vorbereiten.
6. RKWP DevTransport-Client vorbereiten.
7. Frame anzeigen.
8. Haptik bei Frame-Ankunft ausloesen.
9. Logs fuer No File Ingress erzeugen.
10. Return und Recovery melden.

## USB-Testpfad

Erster Hardwarepfad:

- iPad/iPhone per USB an Mac anschliessen.
- Xcode Development Signing aktivieren.
- App direkt auf Geraet starten.
- Windows und Mac im selben lokalen Netzwerk halten.
- iPad/iPhone im selben WLAN wie Windows/Mac betreiben.
- Local Network Permission auf iOS bestaetigen.

Dieser Pfad ersetzt keine spaetere TestFlight- oder Produktivverteilung. Er ist nur fuer Development.

## RKWP Frame anzeigen

Die mobile Surface darf anzeigen:

- DisplayName
- ThingId
- FrameSessionId
- PageNumber
- RendererStatus
- sichere Frame-Repraesentation
- policygebundene Input-Moeglichkeiten

Die mobile Surface darf nicht speichern:

- Originalpfad
- Originalbytes
- freie PDF-Datei
- automatische Kopie im App-Container
- stille Version ohne Owner-Entscheidung

## Haptik

Haptik ist menschliche Rueckmeldung, keine Protokollsemantik.

Vorbereiten:

- Frame angekommen
- Ding wurde genommen
- Edge ist erreichbar
- Ding liegt im Frame
- Return abgeschlossen
- Fehler oder Policy-Denial

## Gesten

Zu pruefen:

- TouchHold
- Drei-Finger-Geste
- LongPress
- Drag zur Glass Edge
- Cancel ueber Escape-Ersatz oder Zurueck-Geste

Drei-Finger-Gesten muessen gegen iPadOS-Multitasking- und Textgesten geprueft werden. TouchHold bleibt der Fallback.

## Glass Edge am Rand

Die mobile App zeigt genau eine Gegenkante zur naechsten Ablage.

Erste Umsetzung:

- ruhige Glass Edge
- kein Radar
- keine Mehrfach-Bubbles
- keine technische Geraetesprache
- optional kleine Zielvorschau

## Erste Objektquellen

Erlaubt fuer erste mobile Tests:

- RK Workspace App
- Document Picker
- Share Extension
- bewusst begrenztes Pasteboard

Nicht als erste Annahme:

- globale App-Erfassung
- Umgehen der Sandbox
- stille Uebernahme fremder App-Inhalte
- Dateiuebertragung als Erfolgspfad

## Offene Blocker

- Kein Xcode in diesem Windows-Thread.
- Native iOS/iPadOS App fehlt.
- Netzwerkfaehiger DevTransport zu iOS/iPadOS fehlt.
- PDF-Renderer/FramePresenter muss nativ entschieden werden.
- finale Touch-Geste muss auf echter Hardware geprueft werden.
- Local Network Permission muss auf echter Hardware bestaetigt werden.
