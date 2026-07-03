# Workspace Adapter Model

Dokument-ID: RKWS-WORKSPACE-ADAPTER-MODEL
Version: 1.0.0
Status: Accepted
Datum: 2026-07-03

## Zweck

Workspace Adapter sind die spaetere Bruecke zwischen Anwendungen und Workspace Shell.

Sie besitzen genau eine Aufgabe:

```text
Anwendungen in Workspace Objects uebersetzen.
```

Nicht mehr.

## Was Adapter nicht sind

Adapter sind nicht:

- das Produkt
- die Shell
- der Core
- ein Transferdienst
- eine Discovery-Schicht
- ein Pairing-System
- ein Transportkanal

## Was Adapter tun

Ein Adapter erkennt spaeter, dass eine Anwendung oder Ablage ein digitales Ding bereitstellen kann.

Er uebersetzt dieses Ding in ein Workspace Object.

Beispiele:

- Explorer-Datei wird Workspace Object.
- Browser-Tab wird Workspace Object.
- PDF-Seite wird Workspace Object.
- Word-Dokument wird Workspace Object.
- Outlook-Mail wird Workspace Object.
- WinCC-/PCS7-Kontext wird Workspace Object.

## Ownership-Regel

Workspace Objects gehoeren nicht Adapter.

Workspace Objects gehoeren nicht Anwendungen.

Workspace Objects gehoeren nicht Geraeten.

Workspace Objects gehoeren Workspace Sessions.

Der Adapter liefert nur die Uebersetzung.

## Adapter-Vertrag V0

MA006.00 implementiert noch keinen Adapter-Vertrag.

Der spaetere Vertrag muss mindestens beantworten:

- Welches digitale Ding wird sichtbar?
- Welche Wahrnehmung wird dadurch unterstuetzt?
- Zu welcher Workspace Session gehoert das Ding?
- Welche Anwendung oder Ablage war Quelle?
- Welche Capabilities sind fuer spaeteres Arbeiten relevant?

## Grenzen in MA006.00

Noch nicht implementiert:

- Explorer Adapter
- Browser Adapter
- PDF Adapter
- Word Adapter
- Outlook Adapter
- WinCC Adapter
- PCS7 Adapter
- Live Hooks
- Overlay-Rendering
- globale Eingaben

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-03 | MA006.00 Workspace Adapter Model dokumentiert. |
