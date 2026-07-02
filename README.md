# RK Workspace

Dokument-ID: RKWS-README-001  
Version: 2.5.0
Status: Accepted  
Datum: 2026-07-02

RK Workspace (RKWS) ist ein eigenstaendiges Software- und Hardwareprodukt fuer raeumlich gedachte digitale Arbeitsflaechen. Das Projekt ist kein Bestandteil von RKOS und wird mit eigener Roadmap, eigener Dokumentation, eigenen Releases und eigener Architektur gefuehrt.

Die zentrale Produktidee ist einfach: Der Benutzer soll nicht "Datei an Geraet senden" denken, sondern "dieses Objekt nach rechts verschieben". RK Workspace modelliert deshalb Arbeitsflaechen statt Geraete. Ein Windows-Laptop, ein MacBook, ein iPad, ein Linux-Rechner, ein Monitor mit Dongle, ein KVM-Arbeitsplatz oder ein Industrie-Leitstand koennen alle Arbeitsflaechen sein.

## Status

Die Architecture Baseline v1.0 ist veroeffentlicht. Die produktive Core-Entwicklung laeuft in MA003. Der aktuelle Stand enthaelt die freigegebene Architekturgrundlage und die ersten plattformneutralen Core-Bausteine:

- Produktvision und Architektur sind dokumentiert.
- Spec-Dokumente fuer Objektmodell, Arbeitsflaechenmodell, Kommunikation, Plugins, Capabilities, UX, Hardware, Firmware und Tests sind angelegt.
- ADRs und Decision-Log sind eingerichtet.
- CI/CD-Grundstruktur und GitHub-Templates sind vorbereitet.
- Der plattformneutrale Core enthaelt erste Modelle und Richtungslogik.
- Der Plugin Manager ist als erste produktive Core-Komponente angelegt.
- Der Capability Manager ist als zweite produktive Core-Komponente angelegt.
- Die Workspace Registry ist als dritte produktive Core-Komponente angelegt.
- Der Transfer Object Manager ist als vierte produktive Core-Komponente angelegt.
- Die Transfer Engine Runtime ist als fuenfte produktive Core-Komponente angelegt.
- Der Core Runtime Orchestrator ist als zentrale Lebenszyklussteuerung angelegt.
- Das Developer Workspace Studio ist als erste sichtbare Core-Testoberflaeche angelegt.
- Die Workspace Agent Runtime ist als erster echter RK Workspace Konsolenprozess angelegt.
- Die Dual Local Agent Simulation startet zwei unabhaengige LocalOnly-Agenten und simuliert einen logischen Transfer ueber einen Harness.
- Der Local IPC Two Process Test startet zwei echte Agent-Prozesse und verbindet sie lokal ueber die Transport Abstraction Layer.
- Named Pipes sind ab MA004.04 nur noch eine Transport-Implementierung hinter neutralen Transport-Schnittstellen.
- Das Developer Workspace Studio enthaelt einen interaktiven Workspace-Prototyp: Textobjekt greifen, nach rechts ziehen und ueber die Transfer Engine abschliessen.
- Das Developer Workspace Studio kann zwei echte Workspace-Fenster oeffnen und Transferobjekte per Drag-and-Drop ueber denselben Core-Kontext bewegen.
- Der Multi-Window-Prototyp zeigt aktives Drag-Feedback, Zielhinweise, Randziel-Vorschlaege und klares Erfolgs-/Fehlerfeedback.
- Der Workspace Experience Sprint verbessert die Multi-Window-UX mit Monitor-Optik, Objektkarten, Workspace Preview, Ruecktransfer und lokaler UX-Diagnose.
- Der Workspace Illusion Sprint verbessert Greifzustand, Rand-Hot-Zones, Edge-Lock, Ghost-Uebergang und sichtbare `WorkspaceSessionCandidate`-Zustaende.
- Das Workspace Experience Lab erlaubt Live-Vergleich von Greifen, Rand, Uebergang, Ablegen und Preview mit lokaler Bewertung.
- UX Evolution Lab Sprint 1 ersetzt feste A/B-Varianten durch Generationen, lokale Statistik und automatische Folgegenerationen nach Gefuehlsbewertung.
- Ein separates Integration-Test-Projekt prueft die Core-Komponenten gemeinsam ueber Runtime Engine und Transfer Engine.
- Ein Core Demo Runner zeigt den aktuellen End-to-End-Core-Ablauf sichtbar ueber Runtime Engine und Transfer Engine in der Konsole.
- Eine lokale Simulation erzeugt zwei Arbeitsflaechen und plant einen Texttransfer von A nach B mit vollstaendigem Log.
- Unit-Tests und Integration-Tests pruefen Core-Regeln, Simulation, Plugin Manager, Capability Manager, Workspace Registry, Transfer Object Manager, Transfer Engine und Runtime Engine.

## Einstieg fuer Entwickler

Neue Entwickler beginnen mit diesen Dokumenten:

1. `Docs/00_ProductVision.md`
2. `Docs/Glossary.md`
3. `Docs/Architecture/ArchitectureBaseline_v1.0.md`
4. `Spec/README.md`
5. `Docs/ADR/README.md`
6. `Docs/Architecture/OpenIssuesBeforeMA003.md`

Die Spezifikationen liegen in `Spec/`. Die ADRs liegen in `Docs/ADR/`. Die Architektur- und Freigabeberichte liegen in `Docs/Architecture/`.

## V1 Ziel

V1 soll beweisen, dass RK Workspace als Arbeitsflaechen-Erweiterung tragfaehig ist. Der erste Prototyp muss zwei Arbeitsflaechen lokal modellieren, koppeln, in einer manuellen Raumkarte anordnen, ein neutrales Transferobjekt erzeugen und einen Richtungstransfer planen koennen. Texttransfer ist der erste konkrete Nachweis. Dateien, PDFs, Bilder, Links, Ordner und Clipboard werden im Modell vorbereitet, aber noch nicht als echte plattformweite Agentenfunktion umgesetzt.

## Nicht-Ziele fuer diesen Stand

Dieser Stand baut keine vollstaendigen Plattformanwendungen, keine globale Gestenerkennung, keine plattformuebergreifende Maussteuerung, keine Bildschirmuebertragung, kein PCB-Layout und keine Serienhardware. Bluetooth wird nicht als Datenkanal vorgesehen. UWB wird als spaetere Positionshilfe behandelt, nicht als Transport fuer Payloads.

## Projektstruktur

```mermaid
flowchart TB
    Docs["Docs und ADR"] --> Spec["Spec"]
    Spec --> Core["src/Core"]
    Core --> Runtime["Runtime"]
    Runtime --> Managers["Plugin / Capability / Workspace / Transfer Object"]
    Spec --> Plugins["Plugin Architecture"]
    Plugins --> Capabilities["Capability System"]
    Plugins --> Layers["Layer Model"]
    Core --> Studio["Developer Studio"]
    Core --> Agents["Local Agents"]
    Agents --> Harness["Dual Agent Harness"]
    Agents --> Transport["Transport Abstraction Layer"]
    Transport --> Ipc["Named Pipe Transport"]
    Core --> Simulation["tools/LocalSimulation"]
    Simulation --> Tests["tests"]
    Spec --> Future["Spaetere Plattformen und Hardware"]
```

```text
Docs/                 Produkt-, Architektur-, ADR- und Entscheidungsdokumente
Spec/                 Spezifikationen fuer Modelle, UX, Kommunikation und Tests
src/Core/             Plattformneutraler Core
src/Core/Plugins/     Plattformneutraler Plugin Manager und Plugin-Vertraege
src/Core/Capabilities/ Plattformneutraler Capability Manager und Capability-Vertraege
src/Core/Workspaces/  Plattformneutrale Workspace Registry und Workspace-Vertraege
src/Core/TransferObjects/ Plattformneutraler Transfer Object Manager und Objekt-Vertraege
src/Core/Transfers/   Plattformneutrale Transfer Engine Runtime und Transfer-Vertraege
src/Core/Runtime/     Plattformneutraler Core Runtime Orchestrator
src/Communication/RKWorkspace.Transport/ Neutrale Transport-Schnittstellen und Transport-Nachrichten
src/Communication/RKWorkspace.Transport.NamedPipes/ Lokale Named-Pipe-Transportimplementierung
src/Communication/RKWorkspace.LocalIpc/ Kompatibilitaetsschicht fuer lokale IPC auf Basis der TAL
src/Demo/RKWorkspace.Core.Demo/ Plattformneutraler Core Demo Runner ohne GUI und Netzwerk
src/Tools/RKWorkspace.DeveloperStudio/ Developer-Diagnoseoberflaeche mit interaktivem Workspace-Prototyp
src/Agents/RKWorkspace.Agent/ LocalOnly Workspace Agent Runtime als Konsolenprozess
tools/DualAgentHarness/ Dual Local Agent Simulation ohne Netzwerk und IPC
tools/LocalIpcHarness/ Zwei-Prozess-Harness fuer lokale Named-Pipe-IPC
src/Windows/          Reserviert fuer spaeteren Windows-Agent
src/macOS/            Reserviert fuer spaeteren macOS-Agent
src/Linux/            Reserviert fuer spaeteren Linux-Agent
src/iOS/              Reserviert fuer spaetere iOS-App
src/Android/          Reserviert fuer spaetere Android-App
firmware/             Firmware-Vorbereitung fuer spaetere Dongles
hardware/             Hardware-Anforderungen und Board-Notizen
PCB/                  Reserviert fuer spaeteres PCB-Layout
Mechanical/           Reserviert fuer spaetere Mechanik
tests/                Unit-, Integrations-, Protokoll- und Hardwaretests
tests/Integration/RKWorkspace.Core.IntegrationTests/ Erster Core-Integrationstest ohne Netzwerk
tools/                Lokale Simulation und Hilfsskripte
.github/              CI, Issue-Templates und PR-Template
```

Die aelteren Agent/App-Verzeichnisse `src/Agent.Windows`, `src/Agent.macOS`, `src/Agent.Linux`, `src/App.iOS` und `src/App.Android` bleiben als Kompatibilitaetsreservierung fuer die erste Projektstruktur erhalten. Die neue kanonische Plattformstruktur ist `src/Windows`, `src/macOS`, `src/Linux`, `src/iOS` und `src/Android`.

## Ausfuehren

```powershell
dotnet build .\src\Core\RKWorkspace.Core.csproj
dotnet run --project .\tests\Unit\RKWorkspace.Core.Tests\RKWorkspace.Core.Tests.csproj
dotnet run --project .\tools\LocalSimulation\RKWorkspace.LocalSimulation.csproj
.\tools\run-demo.ps1
.\tools\run-studio.ps1
.\tools\run-agent.ps1 -Once
.\tools\run-dual-agent.ps1
.\tools\run-local-ipc.ps1
```

Oder gesammelt:

```powershell
.\tools\run-tests.ps1
```

## Arbeitsregel

Die Entwicklungsreihenfolge ist Vision, Requirements, Architektur, Spezifikation, Simulation, Core, Plattformdienste, Hardware, Firmware, Tests und Produktion. Kein Schritt wird uebersprungen. Architektur, Dokumentation und Tests besitzen denselben Stellenwert wie Quellcode.

Vor Master-Arbeitsauftrag 003 duerfen keine Plattformagenten, keine GUI, keine Firmware, keine Hardwarelayouts, keine Netzwerkimplementierung und keine Betriebssystemintegration gebaut werden.

## Naechster Entwicklungsschritt

MA003 entwickelt den plattformneutralen Core und erste produktive Komponenten auf Grundlage der freigegebenen Architecture Baseline v1.0. MA003.01 liefert den Plugin Manager. MA003.02 liefert den Capability Manager. MA003.03 liefert die Workspace Registry mit logischer Zielauswahl V1. MA003.04 liefert den Transfer Object Manager. MA003.05 liefert den ersten vollstaendigen Core Integration Test ohne Netzwerk und ohne Betriebssystemabhaengigkeit. MA003.06 liefert den Core Demo Runner als sichtbaren Konsolenablauf. MA003.07 liefert die Transfer Engine Runtime und den Core Runtime Orchestrator. MA003.08 liefert das Developer Workspace Studio. MA004.01 liefert die Workspace Agent Runtime. MA004.02 liefert die Dual Local Agent Simulation. MA004.03 liefert den Local IPC Two Process Test. MA004.04 liefert die Transport Abstraction Layer. MA004.X liefert den Interactive Workspace Prototype im Developer Studio. MA005.00 liefert den Multi Window Workspace Prototype mit zwei echten Workspace-Fenstern. MA005.01 liefert Drag-Feedback und Randziel-Logik. MA005.02 liefert den Workspace Experience Sprint fuer ein natuerlicheres Arbeitsflaechen-Gefuehl. MA005.03 liefert den Workspace Illusion Sprint fuer Greifen, Randuebertritt und durchgehenden Arbeitsraum. UX-LAB-001 liefert ein internes Workspace Experience Lab fuer Live-Variantenvergleich und lokale Bewertung. UX Evolution Lab Sprint 1 erzeugt viele Generationen, bewertet sie ueber drei Gefuehlsbuttons und fuehrt lokal Statistik, damit der Owner die natuerlichste Kombination entscheiden kann.

Der Core Demo Runner ist kein Produktagent, keine GUI, kein Netzwerkdienst und kein Plattformadapter. Er startet die plattformneutrale Runtime Engine und fuehrt danach nur den aktuellen Core-Ablauf sichtbar ueber die Transfer Engine aus.

Das Developer Workspace Studio ist ebenfalls kein Produktagent und keine Endanwender-GUI. Es ist ein separates Developer-Tool fuer Diagnose, Tests, Demonstration und Core-Visualisierung.

Der Interactive Workspace Prototype im Studio testet erstmals das Bediengefuehl: Ein sichtbares Textobjekt wird von Workspace A nach Workspace B gezogen. Beim Ablegen nutzt das Studio die vorhandene Transfer Engine; es gibt weiterhin keine Netzwerkfunktion, keine Discovery, keine Hardware, keine Firmware und keine Cloud.

Der Multi Window Workspace Prototype erweitert diesen Bedienversuch auf zwei echte Betriebssystemfenster. Window A und Window B teilen sich denselben Core-Kontext, zeigen eigene Transferobjekte, History, Diagnostics und Logs und fuehren Drag-and-Drop ueber `TransferEngine.ExecuteLogicalTransfer()` aus. MA005.01 ergaenzt Drag-Hervorhebung, Statushinweise, Zieltext `Hier ablegen`, Success-/Fehlerfeedback und eine gekapselte Fensterrand-Logik, die rechts Workspace B und links Workspace A vorschlaegt. MA005.02 verbessert das Arbeitsflaechen-Gefuehl mit Monitor-Kopf, echten Objektkarten fuer Text, PDF, Bild und Link, Workspace Preview am Rand, sichtbarer Zielanimation, Ruecktransfer B nach A, UX-Diagnosewerten und vorbereitetem `WorkspaceSessionCandidate`. MA005.03 reagiert auf Owner-Feedback, dass sich der Prototyp noch zu sehr wie eine App mit zwei Kaestchen anfuehlt: Greifzustand, pulsierende Rand-Hot-Zones, Edge-Lock, Ghost-Objekt im Rand und Candidate-Zustaende sollen einen durchgehenden Arbeitsraum andeuten. UX-LAB-001 macht diese Darstellung variierbar: Greifen, Rand, Uebergang, Ablegen, Preview, Animation und Geschwindigkeit koennen live verglichen und lokal bewertet werden. UX Evolution Lab Sprint 1 erweitert das Lab auf Generationen: 24 Greif-, 24 Rand-, 24 Uebergangs-, 20 Ablege- und 8 Vorschauvarianten, drei Gefuehlsbuttons und lokale Statistik. Er bleibt lokal: kein IPC, kein Netzwerk, keine Discovery, keine Persistenz ausser Lab-Bewertungen und Lab-Statistik, keine echte Monitorerkennung und keine Plattformadapter im Core.

Die Workspace Agent Runtime ist noch kein Betriebssystemdienst. Sie ist ein LocalOnly-Konsolenprozess ohne Netzwerk, Discovery, GUI, Persistenz, Firmware, Hardware oder Cloud.

Die Dual Local Agent Simulation ist noch keine Prozesskommunikation. Zwei AgentRuntime-Instanzen laufen parallel im selben Harness, behalten getrennte Runtime- und Manager-Instanzen und simulieren den Transfer logisch ohne Netzwerk, Discovery oder IPC.

Der Local IPC Two Process Test ist die erste echte Prozesskommunikation. Seit MA004.04 laeuft er ueber die Transport Abstraction Layer. Die aktuelle Implementierung verwendet Named Pipes lokal auf demselben Rechner, aber Agent und Harness kommunizieren ueber `ITransport`, `ITransportClient`, `ITransportServer` und `TransportMessage`. Es gibt weiterhin keine TCP-/UDP-Ports, keine Discovery, keine Dienste und keine Netzwerkkommunikation ueber Rechnergrenzen.

Nach dem Multi Window Workspace Prototype wird zuerst die Bedienung ueber echte Fenster gemeinsam geprueft. Danach wird entschieden, ob echte Monitor-/Rand-Erkennung, Local Discovery oder echter Netzwerktransfer begonnen wird. Discovery kommt weiterhin nach der TAL, damit Agenten spaeter einen Transport auswaehlen koennen, ohne an Named Pipes, TCP, WebSocket, USB, BLE oder Cloud Relay gekoppelt zu sein.

## Querverweise

- `Docs/Architecture/ArchitectureBaseline_v1.0.md`
- `Docs/Architecture/ArchitectureFreeze.md`
- `Docs/Architecture/ArchitectureBaseline.md`
- `Docs/Architecture/GitInitialCommitReadiness.md`
- `Docs/Architecture/GitReleaseReadiness.md`
- `Docs/Architecture/ArchitectureReview.md`
- `Docs/Architecture/OpenIssuesBeforeMA003.md`
- `Docs/Glossary.md`
- `Spec/PluginArchitecture.md`
- `Spec/CapabilityModel.md`
- `Spec/LayerModel.md`
- `Spec/RuntimeArchitecture.md`
- `Spec/VersionV0.1.md`
- `Docs/Development/DeveloperWorkspaceStudio.md`
- `Docs/Development/InteractiveWorkspacePrototype.md`
- `Docs/Development/MultiWindowWorkspacePrototype.md`
- `Docs/Development/WorkspaceExperienceSprint.md`
- `Docs/Development/WorkspaceIllusionSprint.md`
- `Docs/Development/WorkspaceExperienceLab.md`
- `Docs/Development/UXEvolutionLabSprint1.md`
- `Docs/Development/WorkspaceAgentRuntime.md`
- `Docs/Development/DualAgentSimulation.md`
- `Docs/Development/LocalIpcTwoProcessTest.md`
- `Docs/Development/TransportAbstractionLayer.md`
- `Docs/Development/MA003_Progress.md`
- `Docs/ADR/README.md`

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 2.5.0 | 2026-07-02 | UX Evolution Lab Sprint 1 mit Generationen, Statistik und Folgegenerationen dokumentiert. |
| 2.4.0 | 2026-07-02 | UX-LAB-001 Workspace Experience Lab dokumentiert. |
| 2.3.0 | 2026-07-02 | MA005.03 Workspace Illusion Sprint dokumentiert. |
| 2.2.0 | 2026-07-02 | MA005.02 Workspace Experience Sprint dokumentiert. |
| 2.1.0 | 2026-07-02 | MA005.01 UX-Feinschliff fuer Multi-Window-Drag-Feedback dokumentiert. |
| 2.0.0 | 2026-07-02 | MA005.00 Multi Window Workspace Prototype dokumentiert. |
| 1.9.0 | 2026-07-02 | Interactive Workspace Prototype im Developer Studio dokumentiert. |
| 1.8.0 | 2026-07-02 | MA004.04 Transport Abstraction Layer dokumentiert. |
| 1.7.0 | 2026-07-02 | MA004.03 Local IPC Two Process Test dokumentiert. |
| 1.6.0 | 2026-07-02 | MA004.02 Dual Local Agent Simulation und Harness dokumentiert. |
| 1.5.0 | 2026-07-02 | MA004.01 Workspace Agent Runtime und Startscript dokumentiert. |
| 1.4.0 | 2026-07-02 | Developer Workspace Studio und Startscript dokumentiert. |
| 1.3.0 | 2026-07-02 | Core Runtime Orchestrator und Runtime-Dokumentation ergaenzt. |
| 1.2.0 | 2026-07-02 | MA003.07 Transfer Engine Runtime dokumentiert. |
| 1.1.0 | 2026-07-02 | MA003.06 Core Demo Runner und Startscript dokumentiert. |
| 1.0.0 | 2026-07-02 | MA003.05 Core Integration Tests fuer alle vier produktiven Core-Komponenten ergaenzt. |
| 0.9.0 | 2026-07-02 | MA003.04 Transfer Object Manager als vierte produktive Core-Komponente ergaenzt. |
| 0.8.0 | 2026-07-02 | MA003.03 Workspace Registry als dritte produktive Core-Komponente ergaenzt. |
| 0.7.0 | 2026-07-02 | MA003.02 Capability Manager als zweite produktive Core-Komponente ergaenzt. |
| 0.6.0 | 2026-07-02 | MA003.01 Plugin Manager als erste produktive Core-Komponente ergaenzt. |
| 0.5.0 | 2026-07-02 | Developer-Onboarding, Baseline-v1.0 und Git-Readiness-Verweise fuer RKWS-0530 ergaenzt. |
| 0.4.0 | 2026-07-02 | Architecture Baseline Completion 002A verlinkt. |
| 0.3.0 | 2026-07-02 | Dokumentstandard und Architektur-Freeze-Verweise ergaenzt. |
| 0.2.0 | 2026-07-02 | Master-Arbeitsauftrag 001 Struktur ergaenzt. |
| 0.1.0 | 2026-07-02 | README angelegt. |
