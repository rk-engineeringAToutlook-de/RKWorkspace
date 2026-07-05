# Android Surface Plan

Status: Docs/stub only
Datum: 2026-07-05

## Ziel

Native Android Surface App mit FrameGuestSurface, Touch, Haptik und Glass Edge vorbereiten.

## Minimaler Ablauf

1. App startet als mobile Ablage.
2. RKWP Session verbindet spaeter.
3. FrameSession wird angezeigt.
4. Keine Originaldatei wird gespeichert.
5. Return/Close invalidiert den Frame.

## Nicht erlaubt

- Content URI still in lokale freie Datei kopieren
- Ownership Transfer als Default
- globale Accessibility-Erfassung als erster Pfad
