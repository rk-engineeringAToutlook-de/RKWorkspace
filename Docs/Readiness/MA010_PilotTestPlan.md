# MA010 Pilot Test Plan

Status: Draft  
Datum: 2026-07-06  
Branch: `feature/ma010-real-cross-device-frame-foundation`

## Gemeinsame Pilotregeln

Alle Piloten muessen die RK Workspace Semantik erhalten:

- Das Original bleibt auf der Originalablage.
- Die Zielablage erhaelt keine freie Datei.
- Die Zielablage sieht nur einen kontrollierten Frame.
- Rueckgabe und Recovery muessen testbar bleiben.
- Sichtbare Sprache vermeidet Transfer, Upload, Download, Senden, Empfangen, Sync, Server, Client, Endpoint, Device, Geraet und Agent.

## Owner-Lab-Raumkarte

MA011.07 ergaenzt fuer echte Lab-Aufbauten eine lokale Manual Map:

```powershell
.\tools\run-manual-map.ps1 -Set -Ablage macOS -Direction Right -Distance Near -Confidence 0.9
.\tools\run-manual-map.ps1 -Set -Ablage iPad -Direction Up -Distance Medium -Confidence 0.7
.\tools\run-manual-map.ps1 -Set -Ablage iPhone -Direction Down -Distance Near -Confidence 0.86
.\tools\run-manual-map.ps1 -Set -Ablage Linux -Direction Left -Distance Far -Confidence 0.72
.\tools\run-manual-map.ps1 -Validate
```

Lokale Config:

```text
config/manual-ablage-map.json
```

Dieser Pfad wird nicht versioniert. Das versionierte Beispiel liegt unter:

```text
config/samples/manual-ablage-map.sample.json
```

Der `NearestAblageSelector` nutzt die Karte, waehlt aber immer nur eine naechste Ablage fuer die Glass Edge.

## Pilot 1: Windows Lokal Owner/Guest Mit Echter PDF

Ziel: Windows zeigt lokal, dass eine echte PDF als Original-Owned Frame auf einer zweiten Ablage sichtbar wird, ohne Datei-Ingress.

Geraete: ein Windows-Entwicklungsrechner.

Ablagen:

- Ablage Windows Owner
- Ablage Windows Guest

Benoetigte Software:

- .NET SDK
- PowerShell
- RK Workspace Repo
- Sample-PDF `samples/Objects/Rechnung.pdf`

Berechtigungen:

- normale Benutzerrechte
- keine Admin-Rechte
- kein Netzwerk erforderlich

Startbefehle:

```powershell
.\tools\run-windows-pdf-frame-pilot.ps1 -SmokeTest
.\tools\run-windows-local-frame-e2e.ps1 -SmokeTest
.\tools\run-pdf-frame-smoke.ps1
```

Testschritte:

1. Sample-PDF auf Owner-Ablage registrieren.
2. Owner- und Guest-Ablage starten.
3. CarryLease oeffnen.
4. FrameSession oeffnen.
5. Guest zeigt `PDF liegt hier im Frame`.
6. Owner zeigt `wartet auf Rueckgabe`.
7. Rueckgabe ausloesen.
8. Heartbeat-Loss/Recovery simulieren.

No File Ingress Pruefung:

- GuestHasPdfFile: NO
- GuestHasOriginalPath: NO
- GuestHasCopiedPdfBytes: NO
- OriginalFileBytes: NO

Rueckgabe: Guest gibt Frame zurueck; Owner zeigt `wieder verfuegbar`.

Recovery: Heartbeat-/Lease-Verlust fuehrt zur Owner-Recovery und sperrt den Guest Frame.

Erwartetes Ergebnis: `RESULT: SUCCESS`, No File Ingress SUCCESS, Build 0 Warnungen / 0 Fehler.

Blocker: echter PDF-Seitenrenderer ist noch nicht final; Placeholder/MetadataPreview ist erlaubt.

## Pilot 2: Windows Owner Zu macOS Guest

Ziel: Windows besitzt die PDF, macOS zeigt als echte Gegenplattform einen FrameOnly Guest View.

Geraete:

- Windows Owner Rechner
- macOS Guest Rechner im selben Labor-Netz

Ablagen:

- Ablage Windows Owner
- Ablage macOS Guest

Benoetigte Software:

- Windows: RK Workspace Repo, .NET SDK, PowerShell
- macOS: macOS-Codex, Xcode/Swift oder passende native Host-Umgebung, RKWP DevLan Client

Berechtigungen:

- Windows Firewall Freigabe fuer DevLan-Port im Labor
- macOS Local Network Berechtigung
- keine produktiven Zertifikate im Dev/Lab-Modus

Startbefehle Windows:

```powershell
.\tools\run-rkwp-lan-owner.ps1 -Port 57100
.\tools\run-windows-owner-for-mac.ps1 -PdfPath samples/Objects/Rechnung.pdf -Port 57100 -AllowDevPairing
```

Startbefehl macOS-Codex-Auftrag:

```text
Baue macOS Frame Guest Surface mit RKWP DevLan Client.
```

Testschritte:

1. Windows Owner starten und DevLan URL notieren.
2. macOS Guest mit AblageIdentity starten.
3. DevPairing im Labor erlauben.
4. macOS sendet AblageHello und Capabilities.
5. Windows oeffnet CarryLease und FrameSession.
6. macOS zeigt FrameOnly PDF-Ansicht.
7. Rueckgabe aus macOS ausloesen.
8. Netzwerkunterbrechung fuer Recovery testen.

No File Ingress Pruefung:

- macOS speichert keine PDF-Datei.
- macOS kennt keinen Originalpfad.
- macOS bekommt nur FrameUpdates.

Rueckgabe: macOS sendet Return/FrameClose; Windows Owner wird wieder verfuegbar.

Recovery: Verbindungsabbruch sperrt macOS Frame und gibt Owner nach Grace/Recovery frei.

Erwartetes Ergebnis: Windows Owner bleibt Eigentumer; macOS zeigt kontrollierten Frame.

Blocker: native macOS Surface und echter DevLan Client muessen auf macOS gebaut werden.

## Pilot 3: Windows Owner Zu iPad/iPhone Guest

Ziel: Windows besitzt die PDF, iPad/iPhone zeigt einen RK Workspace Frame in einer nativen Surface App.

Geraete:

- Windows Owner Rechner
- Mac fuer Xcode/macOS-Codex
- iPad oder iPhone per USB und im selben Labor-Netz

Ablagen:

- Ablage Windows Owner
- Ablage iPad/iPhone

Benoetigte Software:

- Windows: RK Workspace Repo, DevLan Owner
- macOS: Xcode, macOS-Codex
- iOS/iPadOS: RK Workspace Surface App Testbuild

Berechtigungen:

- iOS Local Network Permission
- USB Developer Trust
- keine globale App-Erfassung
- optional Haptikberechtigung je Plattformmodell

Startbefehle Windows:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -PdfPath samples/Objects/Rechnung.pdf -Port 57100 -AllowDevPairing
.\tools\run-ios-guest-compat.ps1 -SmokeTest
```

Startbefehl iOS/iPadOS ueber macOS/Xcode-Codex:

```text
Baue iPad/iPhone RK Workspace Surface App, die RKWP Frame anzeigen kann, Haptik unterstuetzt und No File Ingress respektiert.
```

Testschritte:

1. Windows Owner als PDF Frame Owner starten.
2. iOS/iPadOS App per Xcode auf echtes Geraet installieren.
3. iOS/iPadOS AblageIdentity erzeugen.
4. DevPairing im Labor ausfuehren.
5. FrameOnly PDF anzeigen.
6. Touch/Haptik fuer Greifen/Zurueckgeben pruefen.
7. App beenden oder Netzwerk trennen und Recovery beobachten.

No File Ingress Pruefung:

- keine PDF im App-Dokumentenordner
- keine PDF ueber Files-App sichtbar
- kein Originalpfad
- nur Frame/Cache nach Policy

Rueckgabe: App sendet Rueckgabe; Windows Owner wird wieder verfuegbar.

Recovery: App-Verlust oder Netzwerkverlust macht Frame ungueltig und Owner stellt Zustand wieder her.

Erwartetes Ergebnis: iPad/iPhone wird echte Ablage fuer FrameOnly-Arbeit, nicht Dateispeicher.

Blocker: native App muss ueber macOS/Xcode gebaut werden; Windows-Codex kann iOS nicht direkt bauen.

## Pilot 4: macOS Owner Zu Windows Guest

Ziel: macOS besitzt spaeter ein Original, Windows zeigt einen kontrollierten Guest Frame.

Geraete:

- macOS Owner Rechner
- Windows Guest Rechner

Ablagen:

- Ablage macOS Owner
- Ablage Windows Guest

Benoetigte Software:

- macOS Owner Surface/Agent
- Windows Frame Guest Host
- RKWP DevLan/Secure Session

Berechtigungen:

- macOS Dateizugriff nur fuer Owner-Quelle
- Windows Firewall fuer Guest-Verbindung
- Labor-Pairing

Startbefehle aktuell:

```powershell
.\tools\run-mac-guest-compat.ps1 -SmokeTest
.\tools\run-windows-local-frame-e2e.ps1 -SmokeTest
```

Geplanter macOS-Codex-Auftrag:

```text
Baue macOS Frame Guest Surface mit RKWP DevLan Client.
```

Testschritte:

1. macOS Owner registriert OriginalThing.
2. Windows Guest sendet Capabilities.
3. macOS oeffnet CarryLease und FrameSession.
4. Windows zeigt FrameOnly.
5. Windows gibt zurueck.
6. Recovery wird durch Guest-Verlust getestet.

No File Ingress Pruefung:

- Windows Guest speichert keine Originaldatei.
- Windows Guest bekommt keinen macOS Originalpfad.
- Windows Guest Cache ist nur FrameCache nach Policy.

Rueckgabe: Windows sendet Return; macOS Owner wird wieder verfuegbar.

Recovery: Verlust der Windows-Verbindung macht Frame ungueltig.

Erwartetes Ergebnis: Symmetrie der RKWP-Semantik ueber Plattformgrenzen.

Blocker: macOS Owner ist noch nicht implementiert.

## Pilot 5: iPad/iPhone Owner Zu Windows Guest Ueber RK Workspace App

Ziel: iPad/iPhone wird spaeter selbst Owner fuer ein Ding aus der RK Workspace App, Document Picker, Share Extension oder Pasteboard. Windows zeigt nur FrameOnly.

Geraete:

- iPad/iPhone
- Mac fuer Xcode/macOS-Codex
- Windows Guest Rechner

Ablagen:

- Ablage iPad/iPhone Owner
- Ablage Windows Guest

Benoetigte Software:

- RK Workspace iOS/iPadOS Surface App
- Windows Frame Guest Host
- RKWP DevLan/Secure Session

Berechtigungen:

- iOS App Sandbox
- Document Picker / Share Extension / Pasteboard nur als explizite Quelle
- Local Network Permission
- kein globales App-Capture

Startbefehle aktuell:

```powershell
.\tools\run-ios-guest-compat.ps1 -SmokeTest
.\tools\run-windows-local-frame-e2e.ps1 -SmokeTest
```

Geplanter iOS/iPadOS-Codex-Auftrag:

```text
Baue iPad/iPhone RK Workspace Surface App, die RKWP Frame anzeigen kann, Haptik unterstuetzt und No File Ingress respektiert.
```

Testschritte:

1. iOS App waehlt Ding ueber erlaubte Quelle.
2. iOS bleibt Owner.
3. Windows Guest koppelt sich im Labor.
4. iOS oeffnet FrameSession.
5. Windows zeigt Frame.
6. Windows gibt zurueck.
7. App-Unterbrechung/Netzverlust fuer Recovery testen.

No File Ingress Pruefung:

- Windows Guest bekommt keine freie Originaldatei.
- Windows Guest bekommt nur Frame.
- iOS Owner entscheidet ueber spaetere Besitzuebernahme.

Rueckgabe: Windows gibt Frame zurueck; iOS Owner zeigt Ding wieder als verfuegbar.

Recovery: Verlust der Windows Guest Session fuehrt zur Owner-Recovery in der iOS App.

Erwartetes Ergebnis: iPad/iPhone kann spaeter Ursprung einer Original-Owned Session sein.

Blocker: iOS Owner-Funktion ist noch konzeptionell; zuerst iOS Guest bauen.
