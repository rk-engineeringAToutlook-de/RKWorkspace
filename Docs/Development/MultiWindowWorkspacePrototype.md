# Multi Window Workspace Prototype

Dokument-ID: RKWS-DEV-MULTI-WINDOW-WORKSPACE-PROTOTYPE
Version: 1.0.0
Status: Accepted
Datum: 2026-07-02

## Zweck

Der Multi Window Workspace Prototype prueft erstmals zwei echte Betriebssystemfenster als getrennte Arbeitsflaechen. Das Ziel ist die Validierung des Bediengefuehls, nicht Infrastrukturentwicklung.

Der Benutzer kann ein Core-Transferobjekt aus Fenster A greifen, in Fenster B loslassen und so einen logischen Transfer ausloesen.

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
7. Der Drop loest `TransferEngine.ExecuteLogicalTransfer()` aus.
8. Nach Erfolg erscheint das Objekt in Fenster B.

## Feedback

Fenster B zeigt beim Drag:

- Zielhervorhebung
- Statusanzeige
- Logeintrag `Target highlighted`

Nach Drop:

- Fenster A loggt `Transfer started` und `Transfer completed`
- Fenster B loggt `Transfer received` und `Transfer completed`
- Fenster B zeigt eine kurze Transferanimation
- History enthaelt `State:Completed`
- Diagnostics melden `Last=SUCCESS`

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

Beide Fenster laufen im selben Prozess und verwenden denselben Core-Kontext.

## Erkenntnisse

Erste beobachtbare UX-Fragen:

- Zielhervorhebung muss sehr eindeutig bleiben, weil Fenstergrenzen beim Drag schnell Aufmerksamkeit binden.
- Objektkarten muessen knapp, aber eindeutig beschriftet sein.
- Die Transferanimation reicht als technischer Nachweis, sollte spaeter weicher und raeumlicher werden.
- Der Benutzer braucht sichtbare Rueckmeldung in beiden Fenstern, nicht nur im Zielfenster.

## Verbesserungen

Moegliche naechste GUI-Verbesserungen:

- groessere Objektkarten mit Icon pro Objekttyp
- bessere Drag-Schatten ueber Fenstergrenzen hinweg
- klareres "Drop accepted"-Feedback
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
- `MultiWindow: SUCCESS` und `RESULT: SUCCESS` werden ausgegeben.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-02 | Multi Window Workspace Prototype dokumentiert. |
