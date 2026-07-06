# iOS/iPadOS Sandbox Sources

Status: MA011.09 planning baseline  
Datum: 2026-07-06

## Ziel

iOS/iPadOS darf keine globale App-Erfassung bauen. Alle Quellen muessen bewusst, sandbox-konform und menschenlesbar sein.

## Erlaubte Quellen spaeter

### RK Workspace App

Die eigene App ist die erste Quelle und Ablage. Sie zeigt Frames und schreibt Logs.

### Share Extension

Spaeter koennen Inhalte bewusst an RK Workspace uebergeben werden. Share Extension ist explizit, nicht global.

### Document Picker

Der Benutzer waehlt bewusst ein Dokument aus. Fuer FrameGuestSurface ist das nicht der erste Testpfad.

### Pasteboard

Nur bewusst begrenzt und transparent. Kein dauerhaftes Hintergrundlesen.

### Files Provider

Spaeter denkbar, aber nicht fuer den ersten FrameGuestSurface-Test.

## Ausdruecklich ausgeschlossen

- globale App-Erfassung.
- heimliches Files-Scanning.
- dauerhafte Kopie fremder Originaldateien.
- Originalpfad aus Windows anzeigen.
- PDF als freie iOS-Datei materialisieren.

## Bezug zu No File Ingress

Die iOS/iPadOS App ist fuer den ersten Test nur Gast. Sie bekommt einen Frame, keine Datei. Jede spaetere Quelle muss beweisen, dass sie Original-Owned und No File Ingress respektiert.
