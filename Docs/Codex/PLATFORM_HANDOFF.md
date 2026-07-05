# Platform Handoff

Status: Draft
Datum: 2026-07-05

## Ziel

Plattform-Threads koennen ab MA007.00 parallel arbeiten, wenn sie die gemeinsame RKWP-Semantik nicht veraendern.

## Gemeinsame Pakete

- `RKWorkspace.Protocol`
- `RKWorkspace.Frame.Pdf`
- `RKWorkspace.Surface.Abstractions`

## Plattformaufgaben

- Windows: native Frame-Presentation und Glass Edge mit realem Desktop.
- macOS: Trackpad-/Maus-Gesten und transparente Surface.
- iOS/iPadOS: Tablet-/Phone-Ablage mit TouchHold, FrameView und Gegenkante.
- Android: Tablet-/Phone-Ablage mit Haptik und FrameView.
- Linux: minimaler Desktop-/Industriepfad.

## Unantastbar

Kein Plattformadapter darf im FrameOnly-Modus eine Originaldatei auf den Gast schreiben.

## GitHub

GitHub ist die zentrale Synchronisation fuer Plattformzweige. Plattform-Threads arbeiten auf klar benannten Feature-Branches und werden erst nach Owner-Freigabe gepusht oder gemergt.
