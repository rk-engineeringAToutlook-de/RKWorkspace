# Developer Workspace Studio

Dokument-ID: RKWS-DEV-DEVELOPER-STUDIO
Version: 2.1.0
Status: Accepted
Datum: 2026-07-03

## Zweck

Das Developer Workspace Studio ist eine erste sichtbare Core-Testoberflaeche fuer RK Workspace. Es dient Entwicklung, Diagnose, Tests, Demonstration und Core-Visualisierung.

Das Studio ist keine Endanwender-GUI und kein Produktagent. Der interaktive Workspace-Prototyp ist ebenfalls nur ein Entwicklungsprototyp.

## Technologie

Die erste Version verwendet Windows Forms auf .NET 8:

- einfache Windows-faehige Desktop-Technologie
- direkte Projektanbindung an `src/Core/RKWorkspace.Core.csproj`
- keine Web-App
- keine Mobile-GUI
- keine komplexe UI-Architektur

## Start

```powershell
.\tools\run-studio.ps1
```

Automatisierter Smoke-Test ohne dauerhaft offene GUI:

```powershell
.\tools\run-studio.ps1 -SmokeTest
```

## Layout

Die Oberflaeche ist in Registerkarten gegliedert. Die erste Registerkarte heisst `First Contact` und zeigt nur einen reduzierten Erstkontakt-Test:

- ein Ding
- zwei Arbeitsflaechen
- kurze Hinweise
- lokale Messwerte
- keine Optionen, Menues oder technischen Schaltflaechen innerhalb der Testflaeche

Die zweite Registerkarte enthaelt das bestehende Developer Studio:

- Oben: Interaktiver Workspace-Prototyp mit Workspace A links, Workspace B rechts und einer sichtbaren Textkarte.
- Mitte links: Workspace-Liste mit Name, Type, Position, State, Trusted und Priority.
- Mitte: Transfer Objects mit Object Type, Display Name, State, Source und Target.
- Mitte rechts: Agents und Diagnostics mit Runtime State, Plugin Count, Workspace Count, Transfer Object Count, Capabilities, Last Result und Last Error.
- Unten: Log und Transfer-History.
- Separat: Multi Window Prototype mit zwei echten Arbeitsflaechen, gemeinsamem Core-Kontext, Objektkarten, Arbeitsflaechenvorschau, Durchgangszonen, Ghost-Kontinuitaet, History, Diagnostics, UX-Diagnose und Log je Fenster.

Die dritte Registerkarte heisst `Human Experience Lab`. Sie dient der lokalen Validierung von Human Experiences. Dort werden nicht Animationen verglichen, sondern Experimente einer HX zugeordnet, bewertet und als Evolution nachvollziehbar gemacht.

## Aktionen

Minimal verfuegbare Aktionen:

- First Contact
- Start Runtime
- Add Demo Workspaces
- Create Text Object
- Transfer Right
- Reset
- Run Full Demo
- Reset Interactive Demo
- Run Full Interactive Demo
- Open Multi Window Prototype
- Human Experience Lab: aktive HX sehen, Experiment waehlen, Owner-Bewertung speichern, Timeline und Dashboard pruefen

`Run Full Demo` startet die Runtime, erzeugt `RKWS-Demo-Laptop` und `RKWS-Demo-Display-Right`, erzeugt ein Textobjekt `Hallo von RK Workspace`, fuehrt einen Transfer nach rechts aus und erwartet `SUCCESS`.

`Run Full Interactive Demo` initialisiert denselben interaktiven Zustand, simuliert Greifen, Reaktion der rechten Arbeitsflaeche und Ablegen und erwartet `InteractiveDemo: SUCCESS`.

`Open Multi Window Prototype` oeffnet zwei echte Windows-Forms-Arbeitsflaechen fuer den linken und rechten Arbeitsplatz. Beide Fenster teilen sich denselben `MultiWindowWorkspaceContext` und aktualisieren sich bei Core-Aenderungen gegenseitig. Tooltips erklaeren Arbeitsflaechen, Dinge, Diagnostics, History, Log und die wichtigsten Aktionen.

Das `Human Experience Lab` zeigt HX-000 bis HX-003 dauerhaft, fuehrt Experimente als `Experiment 001`, `Experiment 002` usw. und speichert Owner-Bewertungen lokal. Jede Bewertung erzeugt ein gezieltes Folgeexperiment innerhalb derselben HX. Die fruehere Digitale-Physik-Logik bleibt als Darstellungsgrundlage im Studio vorhanden, ist aber nicht mehr der fuehrende sichtbare Experimentierprozess.

## First Contact

`First Contact` ist der reduzierte Owner-Test fuer die erste Begegnung mit RK Workspace. Er misst, ob eine neue Person ohne Erklaerung innerhalb von 30 Sekunden ein Ding nimmt, nach rechts traegt und dort ablegt.

Die Testflaeche zeigt nur:

- `Nimm dieses Objekt.`
- ein wertig wirkendes Ding
- eine linke Arbeitsflaeche
- eine rechte Arbeitsflaeche

Beim Greifen hebt sich das Ding, erhaelt Tiefe, Schatten und die aktuelle Tragephysik aus dem Lab. Beim Tragen wird der Hintergrund ruhiger. Die rechte Arbeitsflaeche zeigt `Hier ablegen`, sobald sie als Ablegeort erreicht wird. Nach Erfolg liegt das Ding sichtbar rechts; es gibt kein Popup und keine technische Transfermeldung in der Testflaeche.

Der Button `First Contact` setzt den Test zurueck und oeffnet die First-Contact-Registerkarte. Die Messwerte bleiben lokal im laufenden Studio:

- Zeit bis Greifen
- Zeit bis Ablegen
- Fehlversuche
- Abbrueche
- unnoetige Klicks

## Interaktiver Workspace-Prototyp

Der interaktive Bereich zeigt:

- `Workspace A / Laptop` links
- `Workspace B / Display Right` rechts
- `Text Object` mit `Hallo von RK Workspace`

Die Textkarte kann mit der Maus gegriffen und nach rechts gezogen werden. Beim Ziehen ueber Workspace B wird das Ziel hervorgehoben. Beim Loslassen ueber Workspace B erzeugt das Studio einen `TransferRequest` und fuehrt `TransferEngine.ExecuteLogicalTransfer()` aus. Nach Erfolg liegt die Karte sichtbar in Workspace B, Diagnostics zeigen `Last Result: SUCCESS`, und die History enthaelt den abgeschlossenen Core-Ablauf.

## Multi Window Workspace-Prototyp

Der Multi Window Prototype zeigt zwei echte OS-Fenster:

- `Window A / Laptop`
- `Window B / Display Right`

Beide Fenster verwenden denselben laufenden Core-Kontext mit `RuntimeEngine`, `WorkspaceRegistry`, `CapabilityManager`, `TransferObjectManager` und `TransferEngine`. Die linke Arbeitsflaeche enthaelt mehrere Dinge: zwei Texte, ein PDF, ein Bild und einen Link. Die rechte Arbeitsflaeche ist der logische Ort zum Ablegen.

Die Objektkarten koennen per Maus zwischen zwei Arbeitsflaechen bewegt werden. Beim Greifen wechselt die Karte in den Zustand `Genommen`: sie loest sich sichtbar, bekommt Tiefe, Gewicht oder Grip. Die Trage-Generation bestimmt, ob das Objekt direkt, mit Nachlauf, Feder oder spuerbarer Masse reagiert. Der Bildschirmrand wird als Durchgang behandelt: er wird weicher, die andere Arbeitsflaeche nimmt das Objekt an, und ein Ghost-Objekt zeigt Kontinuitaet. Beim Ablegen setzt das Objekt ruhig auf der anderen Arbeitsflaeche auf. Intern nutzt der erste erfolgreiche Ablauf weiterhin `TransferEngine.ExecuteLogicalTransfer()`, aber die sichtbare Sprache beschreibt nehmen, tragen und ablegen.

## Human Experience Lab

Das Human Experience Lab ist die fuehrende Registerkarte fuer neue UX-Entscheidungen. Es zeigt:

- Aktive Human Experience
- Experiment-Modus
- Human Experience Timeline
- Human Experience Dashboard
- Experiment-Historie
- Beobachtungsprotokoll

Aktuell sichtbare HX:

- HX-000: Ich bin in meinem Arbeitsraum.
- HX-001: Das gehoert zu meiner Arbeit.
- HX-002: Ich habe etwas in meiner Hand.
- HX-003: Ich trage etwas.

Der Owner bewertet jedes Experiment nur mit:

- Gruen: Das fuehlt sich richtig an.
- Gelb: Fast.
- Rot: Nein.

Gespeichert werden HX, Experiment, Datum, Bewertung, Kommentar, Dauer und Wiederholungen. Speicherort:

```text
%LOCALAPPDATA%\RKWorkspace\human-experience-lab.json
```

Die Historie bleibt erhalten. Experimente werden nicht geloescht und nicht ueberschrieben.

## Core-Anbindung

Das Studio verwendet echte Core-Komponenten:

- `RuntimeEngine`
- `WorkspaceRegistry`
- `CapabilityManager`
- `TransferObjectManager`
- `TransferEngine`
- Core-Modelle fuer Workspaces, Capabilities und Transfer Objects

Der Ablauf wird nicht als separate Studio-Logik dupliziert. Das Studio ruft den vorhandenen Core auf und visualisiert dessen Zustand.

## IPC-Hinweis

Developer Studio zeigt lokale Simulationen, Dual-Agent-Diagnose, den interaktiven Workspace-Prototyp, den Multi Window Workspace Prototype und das Human Experience Lab. Die interaktiven Demos verwenden keinen Local-IPC-Kanal und keine Netzwerkfunktion.

Die Digitale Physik veraendert ausschliesslich Darstellung und Timing im Studio. Sie veraendert keine Core-Komponenten und keinen Transport. Die Evolutionslogik arbeitet nur auf Studio-Varianten, lokaler Bewertung und lokaler Statistik.

Das Human Experience Lab veraendert ebenfalls keine Core-Komponenten und keinen Transport. Es speichert nur lokale Experimente und Beobachtungen.

## Nicht-Ziele

- keine Netzwerkfunktion
- keine Betriebssystemintegration
- keine Firmwarefunktion
- keine Hardwarefunktion
- keine Cloud-Funktion
- keine produktive Endanwender-GUI
- keine stabilen UI-Automation-Tests

## Bekannte Einschraenkungen

- Das Studio ist ein Windows-Desktop-Tool.
- Der Smoke-Test prueft den Core-Ablauf ohne sichtbares Fenster.
- Der Smoke-Test prueft den interaktiven Demo-Ablauf viewmodelbasiert ohne echte UI-Automation.
- Der Smoke-Test prueft den Multi-Window-Ablauf, `Run Full Demo`, EdgeTarget-Logik, Roundtrip B nach A, UX-Diagnostics, Workspace Illusion und `WorkspaceSessionCandidate` viewmodelbasiert ohne echte UI-Automation.
- Der Smoke-Test prueft die Digitale Physik viewmodelbasiert: Variantenanzahl inklusive 12 Tragevarianten, Live-Wechsel, Bewertung, automatische Folgegeneration und Anwendung im Multi-Window-Kontext.
- Der Smoke-Test prueft First Contact viewmodelbasiert: Startzustand, Greifen, Ablegen, lokale Messwerte und 30-Sekunden-Erfolg.
- Der Smoke-Test prueft das Human Experience Lab viewmodelbasiert: aktive HX, Experiment-Zuordnung, lokale Beobachtung, gezielte Evolution und Dashboard.
- Die Randlogik ist vorbereitet, aber noch keine echte Monitorerkennung oder Betriebssystem-Randbindung.
- Es gibt noch keine Persistenz und keine gespeicherten Studio-Profile.
- Das Log ist nur eine In-Memory-Ansicht.
- Das Studio nutzt Demo-Daten und keine automatische Discovery.
- Das Studio spricht weiterhin nicht selbst mit dem Local-IPC-Kanal.
- Pick-Carry-Place im Single-Window-Prototyp ist nur fuer das Textding von links nach rechts vorgesehen.
- Pick-Carry-Place im Multi-Window-Prototyp ist fuer Demo-Dinge zwischen linker und rechter Arbeitsflaeche vorgesehen.
- `WorkspaceSessionCandidate` ist vorbereitet, aber noch keine Live-Workspace-Session.
- Die Workspace-Illusion ist optisch; echte OS-Hot-Zones, Monitoruebertritt und Live-Sessions sind noch nicht implementiert.
- Das Workspace Experience Lab ist ein internes Experimentierlabor, kein Produkt und kein Endanwenderwerkzeug.
- Das Human Experience Lab ist das fuehrende interne Validierungslabor fuer Wahrnehmung, kein Produkt und kein Endanwenderwerkzeug.
- First Contact ist ein Owner-Testmodus, kein Produktmodus und keine echte Studie mit externer Telemetrie.
- Lab-Bewertungen, Evolutionsschritt und lokale Statistik werden in `%LOCALAPPDATA%\RKWorkspace\workspace-experience-lab.json` gespeichert.
- Human-Experience-Experimente und Beobachtungen werden in `%LOCALAPPDATA%\RKWorkspace\human-experience-lab.json` gespeichert.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 2.1.0 | 2026-07-03 | HX-LAB-001 Human Experience Lab als fuehrende dritte Registerkarte dokumentiert. |
| 2.0.0 | 2026-07-03 | MA005.04 First Contact mit reduzierter Erstkontakt-Testflaeche dokumentiert. |
| 1.9.0 | 2026-07-03 | DP-001 Digitale Physik mit Tragevarianten und Pick-Carry-Place dokumentiert. |
| 1.8.0 | 2026-07-02 | UX Evolution Lab Sprint 1 mit Generationen, Gefuehlsbewertung und Statistik dokumentiert. |
| 1.7.0 | 2026-07-02 | UX-LAB-001 Workspace Experience Lab dokumentiert. |
| 1.6.0 | 2026-07-02 | MA005.03 Workspace Illusion, Edge-Hot-Zones und Candidate-Zustaende dokumentiert. |
| 1.5.0 | 2026-07-02 | MA005.02 Workspace Experience Sprint, Ruecktransfer und UX-Diagnose dokumentiert. |
| 1.4.0 | 2026-07-02 | MA005.01 UX-Feinschliff, EdgeTarget-Logik und Feedback dokumentiert. |
| 1.3.0 | 2026-07-02 | Multi Window Workspace Prototype im Developer Studio dokumentiert. |
| 1.2.0 | 2026-07-02 | Interactive Workspace Prototype, Drag-and-Drop und erweiterten Smoke-Test dokumentiert. |
| 1.1.0 | 2026-07-02 | Hinweis zur spaeteren IPC-Anbindung ergaenzt. |
| 1.0.0 | 2026-07-02 | Developer Workspace Studio fuer MA003.08 dokumentiert. |
