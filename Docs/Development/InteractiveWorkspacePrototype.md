# Interactive Workspace Prototype

Dokument-ID: RKWS-DEV-INTERACTIVE-WORKSPACE-PROTOTYPE
Version: 1.1.0
Status: Accepted
Datum: 2026-07-02

## Zweck

Der Interactive Workspace Prototype macht die Kernidee von RK Workspace erstmals direkt bedienbar: Ein sichtbares Textobjekt liegt in Workspace A, wird mit der Maus gegriffen, nach rechts gezogen und auf Workspace B abgelegt.

Der Prototyp dient dem Bediengefuehl, der Diagnose und der fruehen Validierung des Core-Ablaufs. Er ist kein Endanwenderprodukt und keine produktive GUI.

## Umfang

Der Prototyp erweitert das bestehende Developer Workspace Studio:

- zwei sichtbare Arbeitsflaechen als grosse Bereiche
- `Workspace A / Laptop` links
- `Workspace B / Display Right` rechts
- sichtbare Textkarte `Text Object`
- Drag mit der Maus
- Ziel-Hervorhebung beim Ziehen ueber Workspace B
- Drop auf Workspace B loest den Core-Transfer aus
- History und Diagnostics zeigen den abgeschlossenen Transfer
- Reset fuer einen neuen interaktiven Lauf
- automatischer Full-Interactive-Demo-Lauf fuer Smoke-Tests

## Core-Anbindung

Der Drop auf Workspace B veraendert nicht nur die UI. Das Studio erzeugt einen `TransferRequest` mit:

- Source = `RKWS-Demo-Laptop`
- Target = `RKWS-Demo-Display-Right`
- Direction = `Right`
- TransferObject = Text Object

Danach wird `TransferEngine.ExecuteLogicalTransfer()` verwendet. Der erfolgreiche Lauf setzt das Transferobjekt auf `Completed`, aktualisiert die Target-Metadaten und schreibt die Transfer-History.

## Log und Feedback

Beim manuellen Ziehen und beim automatischen Full-Interactive-Demo-Lauf werden diese Logpunkte geschrieben:

- `Dragging started`
- `Target highlighted`
- `Transfer completed`

Nach Erfolg liegt die Textkarte sichtbar in Workspace B. Diagnostics melden `Last Result: SUCCESS`. Die History enthaelt unter anderem `Created`, `State:Validated`, `MetadataUpdated`, `State:Prepared` und `State:Completed`.

Der Single-Window-Prototyp bleibt bewusst einfach. Der UX-Feinschliff fuer Fensterrand-Vorschlaege, `Hier ablegen`-Hinweise und Cross-Window-Feedback liegt im Multi Window Workspace Prototype.

## Grenzen

Der Prototyp fuehrt nicht ein:

- keine Netzwerkfunktion
- keine Discovery
- kein Pairing
- keine Hardware
- keine Firmware
- keine Cloud
- keine Endanwender-GUI
- keine echte Payload-Uebertragung
- keine stabile UI-Automation
- keine Fensterrand- oder Monitorerkennung im Single-Window-Prototyp

## Smoke-Test

Der Smoke-Test laeuft viewmodelbasiert:

```powershell
.\tools\run-studio.ps1 -SmokeTest
```

Er prueft:

- Studio-ViewModel kann den interaktiven Demo-Zustand initialisieren.
- `Run Full Interactive Demo` fuehrt den A-nach-B-Transfer aus.
- Das Objekt liegt danach in Workspace B.
- Der Objektstatus ist `Completed`.
- Die History enthaelt `State:Completed`.
- Ergebnis ist `InteractiveDemo: SUCCESS` und `RESULT: SUCCESS`.
- Ab MA005.01 prueft derselbe Studio-Smoke zusaetzlich `Run Full Demo`, Multi-Window und EdgeTarget-Logik.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.1.0 | 2026-07-02 | MA005.01 Abgrenzung zum Multi-Window-UX-Feinschliff ergaenzt. |
| 1.0.0 | 2026-07-02 | Interactive Workspace Prototype dokumentiert. |
