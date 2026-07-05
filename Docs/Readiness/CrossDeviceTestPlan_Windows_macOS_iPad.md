# Cross-Device Test Plan: Windows, macOS, iPad

Status: Draft  
Datum: 2026-07-05

## Ziel

Der erste echte Cross-Device-Test beweist nicht Dateiuebertragung. Er beweist:

- Original bleibt beim Owner.
- Guest sieht nur einen Frame.
- No File Ingress bleibt gueltig.
- Rueckgabe und Recovery sind nachvollziehbar.

## Gemeinsame Voraussetzungen

- Gemeinsamer Branch oder klar synchronisierte Commits.
- RKWP Protocol kompatibel auf allen beteiligten Plattformen.
- Lokales Netzwerk fuer Development-Transport.
- Produktive Kryptografie ist noch nicht aktiv; Test nur in Development-Lab.
- Logs muessen LeaseId, FrameSessionId, OwnerAblageId und GuestAblageId enthalten.

## Szenario A: Windows besitzt PDF, macOS zeigt PDF-Frame

| Feld | Inhalt |
| --- | --- |
| benoetigte Plattform | Windows Owner, macOS Guest |
| benoetigter Agent | Windows PDF Owner Tool, macOS FrameGuestSurface |
| Berechtigungen | Windows Dateizugriff auf Sample-PDF, macOS Screen/UI- und App-Sandbox-Pruefung |
| Netzwerk | lokales LAN oder lokaler Dev-Transport |
| RKWP Transport | Development-Profil, spaeter LAN/WebRTC |
| Security | Nonce, Sequence, LeaseBinding, PolicyBinding, Development Protector markiert als unsicher |
| Vorbereitung | `Docs/Readiness/WindowsToMac_DevTransportPlan.md` und `release/handoff/WindowsToMac_MA008_Handoff.md` |
| Windows Startpunkt | `tools/run-windows-owner-for-mac.ps1` |
| bekannte Blocker | macOS FrameGuestSurface und Cross-Device DevTransport noch nicht implementiert |

Testschritte:

1. Windows waehlt `samples/Objects/Rechnung.pdf` als Original-Owned Object.
2. Windows erzeugt CarryLease und FrameSession.
3. macOS zeigt PDF-Frame.
4. macOS darf keinen Originalpfad und keine Originalbytes erhalten.
5. macOS sendet Rueckgabe oder ChangeSet.
6. Windows bestaetigt Recovery/Rueckgabe.

Erfolgskriterien:

- Windows bleibt Owner.
- macOS zeigt Frame.
- No File Ingress ist PASS.
- Lease ist aktiv und gebunden.
- Rueckgabe/Recovery ist PASS.

AP016 konkretisiert dieses Szenario als ersten Handoff-Test. Windows liefert dafuer einen Owner-Startpunkt, ein Context Pack und eine Handoff-Datei. Der aktuelle `NamedPipeDev`-Transport bleibt lokal; fuer den echten macOS-Test muss ein netzwerkfaehiges Development-Profil ergaenzt werden.

## Szenario B: Windows besitzt PDF, iPad zeigt PDF-Frame

| Feld | Inhalt |
| --- | --- |
| benoetigte Plattform | Windows Owner, iPad Guest |
| benoetigter Agent | Windows PDF Owner Tool, iPad RK Workspace Surface App |
| Berechtigungen | iOS/iPadOS lokale App, Netzwerkzugriff, keine Dateispeicherung im FrameOnly-Pfad |
| Netzwerk | gleiches WLAN, spaeter WebRTC |
| RKWP Transport | Development-Profil ueber iOS-App |
| Security | Session-Nonce, Sequence, LeaseBinding, PolicyBinding |
| Vorbereitung | `Docs/Readiness/iPad_iPhone_Surface_TestPlan.md` und `release/handoff/iOS_iPadOS_MA008_Handoff.md` |
| Testpfad | macOS-Codex, Xcode, USB-Testgeraet |
| bekannte Blocker | native iPad-App und netzwerkfaehiger DevTransport fehlen noch |

Testschritte:

1. Windows erzeugt Original-Owned PDF FrameSession.
2. iPad Surface akzeptiert FrameOnly.
3. iPad zeigt Frame.
4. iPad gibt zurueck oder verwirft.
5. Windows prueft Audit, Lease und Recovery.

Erfolgskriterien:

- iPad speichert keine PDF-Datei.
- Windows bleibt Owner.
- Touch/Haptik kann vorbereitet sein, darf aber Ownership nicht veraendern.
- Recovery-Test ist PASS.

AP017 konkretisiert dieses Szenario als iPad/iPhone-Handoff. PWA bleibt nur Uebergang; der Zielpfad ist eine native Surface App aus Xcode.

## Szenario C: macOS besitzt PDF, Windows zeigt PDF-Frame

| Feld | Inhalt |
| --- | --- |
| benoetigte Plattform | macOS Owner, Windows Guest |
| benoetigter Agent | macOS PDF Owner Adapter, Windows FrameGuestSurface |
| Berechtigungen | macOS Dokumentzugriff, Windows Surface View |
| Netzwerk | lokales LAN |
| RKWP Transport | Development-Profil |
| Security | Development Protector, spaeter produktive Session Protection |
| bekannte Blocker | macOS Owner Adapter fehlt |

Testschritte:

1. macOS nimmt PDF als Original-Owned Object.
2. macOS erzeugt FrameOnly Session.
3. Windows zeigt Frame.
4. Windows darf keine PDF-Datei erhalten.
5. Windows gibt Session zurueck.

Erfolgskriterien:

- macOS bleibt Owner.
- Windows zeigt nur Frame.
- No File Ingress ist PASS.

## Szenario D: iPad besitzt Dokument, Windows zeigt Frame

| Feld | Inhalt |
| --- | --- |
| benoetigte Plattform | iPad Owner, Windows Guest |
| benoetigter Agent | iPad RK Workspace Surface App, Windows FrameGuestSurface |
| Berechtigungen | iOS Document Picker/Share Extension, Windows View Surface |
| Netzwerk | gleiches WLAN |
| RKWP Transport | Development-Profil, spaeter WebRTC |
| Security | LeaseBinding und PolicyBinding |
| bekannte Blocker | iPad Owner-App und Transport fehlen |

Testschritte:

1. iPad nimmt Dokument in RK Workspace App.
2. iPad erstellt FrameOnly Session.
3. Windows zeigt Frame.
4. Windows erhaelt keine Originaldatei.
5. iPad beendet oder erneuert Lease.

Erfolgskriterien:

- iPad bleibt Owner.
- Windows hat nur Framezugriff.
- Recovery bei Verbindungsabbruch ist nachvollziehbar.

## Reihenfolge Fuer Den Ersten Realen Test

1. Windows Owner + Windows Local Guest Surface End-to-End. MA008.03 liefert diesen lokalen Smoke mit DevPairing, NamedPipeDev, FrameOnly, No File Ingress, Return und Recovery.
2. Windows Owner + macOS Guest Surface. MA008.06 liefert Plan, Handoff, Windows Owner Script und offene Blocker.
3. Windows Owner + iPad Guest Surface. MA008.07 liefert iPad/iPhone Testplan, Xcode-Handoff und USB-Hinweise.
4. macOS Owner + Windows Guest.
5. iPad Owner + Windows Guest.

Diese Reihenfolge minimiert Risiko: zuerst ein echter Frame auf einer lokalen zweiten Surface, danach echte Plattformgrenzen.
