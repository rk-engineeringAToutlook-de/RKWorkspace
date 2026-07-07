# MA017 Pilot Acceptance Criteria

Status: Draft
Datum: 2026-07-07

## Zweck

Dieses Dokument definiert die Abnahmekriterien fuer den MA017-Pilot.

## Muss-Kriterien

- Windows Owner kann Closed PDF Capsule erzeugen.
- Windows Owner kann Open PDF Frame erzeugen.
- Guest-Ablage erhaelt keine freie Originaldatei.
- Guest-Ablage erhaelt keinen Originalpfad.
- Guest-Ablage erhaelt keine kopierten PDF-Bytes.
- Return und Recovery funktionieren.
- Security/Policy Checkpoint ist gruen.
- Human Experience Checkpoint ist gruen.
- Packaging Checkpoint ist gruen.
- Performance/Stability Checkpoint ist gruen.

## Darf-Kriterien

- macOS und iOS duerfen als externe Handoff-Pfade noch nicht nativ ausgefuehrt sein.
- UWB/Dongle darf simuliert sein.
- Security darf im Lab Development-Warnungen zeigen, wenn sie sichtbar und korrekt benannt sind.

## Nicht akzeptabel

- Owner glaubt, eine Datei sei kopiert worden.
- No File Ingress wird durch UI oder Report widersprochen.
- Mehrere Zielkanten konkurrieren im Hauptpfad.
- Reports behaupten native Plattformausfuehrung ohne Xcode-Nachweis.

## Result

```text
MA017PilotAcceptanceCriteria: READY
RESULT: SUCCESS
```
