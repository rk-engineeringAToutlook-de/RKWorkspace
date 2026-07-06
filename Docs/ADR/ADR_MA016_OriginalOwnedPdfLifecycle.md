# ADR MA016 Original-Owned PDF Lifecycle

Dokument-ID: RKWS-ADR-MA016-PDF-LIFECYCLE
Version: 1.0.0
Status: Accepted
Datum: 2026-07-07

## Problemstellung

RK Workspace soll reale PDFs zwischen Ablagen erlebbar machen, ohne dass der Benutzer Dateiuebertragung, Kopieren oder Kontrollverlust wahrnimmt.

Ein klassischer Datei-Transfer wuerde das Original auf eine Gastablage bringen und damit die Human Experience sowie die Sicherheitslinie von MA016 brechen.

## Moegliche Alternativen

1. PDF-Datei auf die Gastablage kopieren.
2. PDF-Datei streamen und lokal cachen.
3. Original-Owned Lifecycle mit Closed PDF Capsule und Open PDF Frame verwenden.

## Bewertung der Alternativen

Eine Kopie ist technisch einfach, verletzt aber No File Ingress und erzeugt Besitzunklarheit.

Ein Stream mit lokalem Cache kann sicherer wirken, erzeugt aber weiterhin das Risiko, dass PDF-Bytes auf der Gastablage materialisiert werden.

Der Original-Owned Lifecycle haelt das Original beim Owner. Die Gastablage bekommt nur eine Kapsel oder einen Frame, gebunden an Lease, Policy, Audit und Recovery.

## Getroffene Entscheidung

MA016 verwendet Original-Owned PDF Lifecycle als Produktpfad:

- Closed PDF Capsule fuer geschuetztes Ablegen.
- Open PDF Frame fuer Weiterarbeiten.
- Owner Lock, solange der Gast ein kontrolliertes Erlebnis hat.
- No File Ingress als harte Regel fuer Gastablaegen.
- Return und Recovery als verpflichtende Abschlusszustaende.

## Konsequenzen

PDFs werden im Pilot nicht als freie Dateien an Gastablaegen erzeugt. Jede spaetere Materialisierung muss als eigene Ownership-Transfer-Entscheidung modelliert werden.

## Risiken

- Frame-Darstellung kann ohne gute UX wie Remote-Anzeige wirken.
- OpenFrame braucht klare Rollen, damit der Owner Vertrauen behaelt.
- Renderer-Performance kann das Gefuehl von Weiterarbeiten stoeren.

## Offene Punkte

- Produktiver PDF Renderer.
- Native macOS und iOS/iPadOS Frame Presenter.
- Security Production Path jenseits SecureDev.
- UX-Entscheidung, wann Closed Capsule oder OpenFrame als Standard wirkt.

## Querverweise

- `Spec\ProductPhilosophy.md`
- `Spec\UX.md`
- `Docs\HumanExperience\MA016_ClosedOpenPdfFeelingComparison.md`
- `Docs\Readiness\MA016_CompletionReport.md`

