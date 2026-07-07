# MA017 Pilot Risk Checklist

Status: AP679 pilot checklist.

## Vor jedem Pilot

- Arbeitsbaum sauber oder bewusst dokumentiert.
- `.\tools\run-ma017-smoke.ps1 -SkipHeavy` erfolgreich.
- Owner weiss, ob der Lauf lokal, macOS oder iPad betrifft.
- No File Ingress ist Pflicht.
- Recovery ist Pflicht.
- Native macOS/iPad Ausfuehrung darf als pending markiert werden.

## Sicherheitsrisiken

- Original-PDF erscheint auf Gastablage als Datei.
- Originalpfad wird auf Gastablage sichtbar.
- Gast erhaelt Originalbytes statt Frame/Kapsel.
- KeepCapsule wird unerlaubt erlaubt.
- Expired Capsule wird nicht durch Owner recovered.
- Dev-/Lab-Sicherheitswarnungen werden versteckt.

## Proximity-Risiken

- mehr als eine glaeserne Kante sichtbar.
- UWB/Dongle springt zwischen Ablagen.
- Manual Map widerspricht UWB ohne Diagnose.
- Confidence wird nicht sichtbar im Developer-Pfad.
- PrivacyMode ist nicht `EphemeralLab`.

## Human Experience Risiken

- Owner denkt an Dateiuebertragung statt an Ablegen.
- Owner vertraut der Rueckgabe nicht.
- Owner verliert das Gefuehl, dass Windows Originalbesitzer bleibt.
- macOS/iPad wirkt wie ein technischer Client statt wie eine Ablage.

## Abbruch

Sofort abbrechen bei:

- `NoFileIngress` nicht erfolgreich.
- `UnauthorizedCapsuleOpen` nicht denied.
- `ExpiredCapsule` nicht recovered.
- unerklaerter Datei- oder Byte-Materialisierung.

## Ergebnis

RiskChecklist: READY
