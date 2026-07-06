# iOS Haptics und Gesture Prototype

Status: MA013.14 plan  
Datum: 2026-07-06

## Ziel

iOS/iPadOS nutzt Haptik als menschliche Rueckmeldung, nicht als Effekt.

## Haptikpunkte

- Geste erkannt: sehr kurz und weich.
- Frame kommt an: ruhiger, positiver Impuls.
- Glass Edge aktiv: subtiler Hinweis.
- Rueckgabe: kurzer Abschlussimpuls.
- Denied: klar, aber nicht aggressiv.
- Verbindung verloren: zurueckhaltend, einmalig.

## Gesten

Zu pruefen:

- Drei-Finger-Langdruck.
- Long Press Fallback.
- Two-Finger Hold Fallback.
- Debug Button nur fuer Lab/Tests.

## Regeln

- keine starke Dauerhaptik.
- keine technische Erfolgsinszenierung.
- Systemgesten respektieren.
- Accessibility nicht verschlechtern.

## Owner-Testfragen

- Fuehlt sich das Ding angekommen an?
- Fuehlt sich Rueckgabe abgeschlossen an?
- Stoert die Haptik?
