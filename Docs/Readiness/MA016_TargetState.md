# RK Workspace

# MA016 Target State

Status: Accepted  
Datum: 2026-07-07

## Ziel

MA016 macht RK Workspace bereit fuer den ersten grossen Real-Platform-Pilot. Windows bleibt Owner-Referenz, macOS wird als echte Gegenplattform vorbereitet, iPad/iPhone werden ueber macOS/Xcode installierbar und testbar gemacht.

## Zielzustand

- Windows kann eine geschlossene PDF als Frame-Kapsel nehmen, auf eine andere Ablage legen, dort oeffnen, zurueckgeben und wiederherstellen.
- Windows kann eine bereits geoeffnete PDF als OpenFrame-Kontext in denselben kontrollierten Frame-Pfad bringen.
- macOS erhaelt ein klares START-HERE-Paket fuer RKWP Client, Frame-Kapsel, OpenFrame, Return, Recovery und No File Ingress.
- iPad/iPhone erhalten ein klares Xcode-/USB-/Test-Handoff mit Capsule, OpenFrame, Haptics, Sandbox und No File Ingress.
- Manual Map, UWB-Simulator und spaetere Dongle-Anker werden als Naehequellen fuer Glass Edge und Zielwahl vorbereitet.
- No File Ingress bleibt Gate: kein Guest bekommt freie PDF-Datei, Originalbytes oder Originalpfad.
- Return und Recovery sind fuer Capsule und OpenFrame sichtbar und auditierbar.
- Pilot-Lab, Reports, Runbooks und Context Pack sind so vorbereitet, dass der Owner den naechsten echten Test starten kann.

## Statusgrenzen

MA016 erzeugt keine produktive macOS/iOS-App im Windows-Repo. Plattformnative Arbeit wird als Handoff-Paket, Contract, Config und Runbook vorbereitet. Security bleibt dort, wo SecureDev genutzt wird, klar als Dev-/Lab-Status markiert.
