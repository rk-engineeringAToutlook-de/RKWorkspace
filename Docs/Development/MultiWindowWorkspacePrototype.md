# Multi Window Workspace Prototype

Dokument-ID: RKWS-DEV-MULTI-WINDOW-WORKSPACE-PROTOTYPE
Version: 1.2.0
Status: Accepted
Datum: 2026-07-02

## Zweck

Der Multi Window Workspace Prototype prueft erstmals zwei echte Betriebssystemfenster als getrennte Arbeitsflaechen. Das Ziel ist die Validierung des Bediengefuehls, nicht Infrastrukturentwicklung.

Der Benutzer kann ein Core-Transferobjekt aus Fenster A greifen, in Fenster B loslassen und spaeter wieder nach Fenster A zurueckziehen. Der erste Transfer nutzt den Core-Transferablauf, der Ruecktransfer im Developer Studio aktualisiert die logische Objektposition fuer die UX-Bewertung.

## Architektur

```text
Developer Studio
  |
  +-- Workspace Window A
  |
  +-- Workspace Window B
  |
  +-- gemeinsamer MultiWindowWorkspaceContext
        |
        +-- RuntimeEngine
        +-- WorkspaceRegistry
        +-- CapabilityManager
        +-- TransferObjectManager
        +-- TransferEngine
```

Beide Fenster sind echte `Form`-Instanzen. Sie verwenden denselben Runtime-Kontext und dieselben Core-Manager. Es gibt keine doppelte Objektverwaltung.

## Workspaces

Fenster A:

- Workspace Name: `Workspace A / Laptop`
- Workspace Type: `SmartDevice`
- Position: `Center`
- Status: `Available`

Fenster B:

- Workspace Name: `Workspace B / Display Right`
- Workspace Type: `DisplayNode`
- Position: `Right`
- Status: `Available`

## Objekte

Der Prototyp erzeugt mindestens:

- zwei Textobjekte
- ein PDF-Objekt
- ein Bildobjekt
- ein Linkobjekt

Alle Objekte sind reine Core-Objekte. Es gibt keine echte Datei-, PDF-, Bild- oder Link-Payload-Uebertragung.

## Bedienung

1. Developer Studio starten.
2. `Open Multi Window Prototype` ausfuehren.
3. Fenster A und Fenster B erscheinen als getrennte Betriebssystemfenster.
4. Objektkarte in Fenster A greifen.
5. Objekt ueber Fenster B loslassen.
6. Fenster B wird beim Drag als Ziel hervorgehoben.
7. Der Drop loest fuer noch nicht abgeschlossene Objekte `TransferEngine.ExecuteLogicalTransfer()` aus.
8. Nach Erfolg erscheint das Objekt in Fenster B.
9. Dasselbe Objekt kann in Fenster B erneut gegriffen und nach Fenster A zurueckgezogen werden.

Beim Ziehen zeigt das Quellfenster einen aktiven Kartenrahmen, eine Statusmeldung und Randziel-Vorschlaege. Der rechte Fensterrand schlaegt Workspace B vor, der linke Fensterrand Workspace A. Eine Workspace Preview zeigt Zielname, Objektanzahl und Status. Diese Logik ist in `MultiWindowWorkspaceContext` gekapselt, damit spaeter echte Monitor- und Bildschirmrand-Erkennung angebunden werden kann.

## Feedback

Fenster B zeigt beim Drag:

- Zielhervorhebung
- Statusanzeige
- Text `Hier ablegen`
- Logeintrag `Zielarbeitsflaeche erkannt`

Nach Drop:

- Fenster A loggt `Objekt wird gezogen` und `Transfer erfolgreich abgeschlossen`
- Fenster B loggt `Transfer received` und `Transfer erfolgreich abgeschlossen`
- Fenster B zeigt eine kurze Transferanimation
- das Ziel leuchtet kurz als Success-Pulse auf
- History enthaelt `State:Completed`
- Diagnostics melden `Ergebnis=ERFOLG` und den letzten Transfer
- UX-Diagnostics zeigen Drag Starts, Zielerkennung, Drops, Transferzeit und Erfolgsquote
- ungueltige Drops loggen `Kein gueltiges Ziel` und lassen das Objekt im Quellfenster

## Grenzen

Der Prototyp fuehrt nicht ein:

- keine Discovery
- kein Netzwerk
- kein LAN
- kein Bluetooth
- kein UWB
- keine Hardware
- keine Firmware
- keine Cloud
- keine Persistenz
- keine Prozesskommunikation
- keine echte Monitorerkennung
- keine Betriebssystem-Hot-Zones ausserhalb der Fenster
- keine Live-Workspace-Session
- keine echte Session-Aushandlung

Beide Fenster laufen im selben Prozess und verwenden denselben Core-Kontext.

`WorkspaceSessionCandidate` ist als interne Vorbereitung vorhanden. Er beschreibt Objekt, Quelle, Ziel und Richtung, wird aber noch nicht fuer Live-Workspace-Funktionen verwendet.

## Erkenntnisse

Erste beobachtbare UX-Fragen:

- Zielhervorhebung muss sehr eindeutig bleiben, weil Fenstergrenzen beim Drag schnell Aufmerksamkeit binden.
- Objektkarten muessen knapp, aber eindeutig beschriftet sein.
- Die Transferanimation reicht als technischer Nachweis, sollte spaeter weicher und raeumlicher werden.
- Der Benutzer braucht sichtbare Rueckmeldung in beiden Fenstern, nicht nur im Zielfenster.
- Randziel-Vorschlaege helfen beim mentalen Modell, ersetzen aber noch keine echte Monitorerkennung.

## Verbesserungen

Moegliche naechste GUI-Verbesserungen:

- groessere Objektkarten mit Icon pro Objekttyp
- bessere Drag-Schatten ueber Fenstergrenzen hinweg
- echte Monitor- und Bildschirmrand-Erkennung
- Undo oder Reset pro Objekt
- ergonomischere Fensterpositionierung
- optionaler Beobachtungsmodus fuer Transfer-History

## Smoke-Test

Der automatisierte Smoke-Test bleibt viewmodelbasiert:

```powershell
.\tools\run-studio.ps1 -SmokeTest
```

Er prueft:

- Multi-Window-Kontext wird initialisiert.
- fuenf Core-Objekte werden erzeugt.
- ein Objekt wird von Workspace A nach Workspace B transferiert.
- Source-Fenster sieht danach vier Objekte.
- Target-Fenster sieht danach ein Objekt.
- `Run Full Demo` wird erfolgreich ausgefuehrt.
- EdgeTarget-Logik liefert links Workspace A, rechts Workspace B und in der Mitte kein Ziel.
- Roundtrip A -> B -> A ist erfolgreich.
- nach dem Roundtrip liegen wieder fuenf Objekte in Workspace A.
- UX-Diagnostics melden Drag-Start, Zielerkennung, Drop und 100 Prozent Erfolgsquote.
- `WorkspaceSessionCandidate` ist vorbereitet.
- `RoundTrip: SUCCESS`, `UxDiagnostics: SUCCESS`, `WorkspaceSessionCandidate: READY`
- `MultiWindow: SUCCESS` und `RESULT: SUCCESS` werden ausgegeben.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.2.0 | 2026-07-02 | MA005.02 Workspace Experience Sprint, Ruecktransfer, Preview und UX-Diagnostics dokumentiert. |
| 1.1.0 | 2026-07-02 | MA005.01 Drag-Feedback, Randziel-Logik und Erfolgs-/Fehlerfeedback dokumentiert. |
| 1.0.0 | 2026-07-02 | Multi Window Workspace Prototype dokumentiert. |
