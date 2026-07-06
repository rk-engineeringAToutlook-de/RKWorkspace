# iOS Return und Recovery

Status: MA013.18 specification  
Datum: 2026-07-06

## Ziel

iOS/iPadOS gibt Frames sauber zurueck und respektiert Recovery.

## Return Action

- Benutzer waehlt `zurueckgeben` oder nutzt eine spaetere Geste.
- App sendet `CarryLeaseReturn`.
- FrameView deaktiviert Eingaben.
- MemoryOnly Cache wird geloescht.
- Haptik bestaetigt Rueckgabe.

## App Background

Wenn App in den Hintergrund geht:

- Heartbeat als pausiert markieren.
- Frame nicht auf Disk schreiben.
- Recovery-Fenster beachten.

## App Killed

Beim Neustart:

- keine lokale PDF wiederherstellen.
- aktive Lease beim Owner klaeren.
- Cache leer starten.

## Network Lost

- `Verbindung verloren` anzeigen.
- nicht versuchen, eine Datei lokal zu materialisieren.
- nach Wiederverbindung Session pruefen.

## Owner Revokes

- Frame invalidieren.
- Cache loeschen.
- Haptik optional kurz.
- Audit schreiben.

## Audit

Pflicht:

- ReturnRequested.
- CacheCleared.
- ConnectionLost.
- RecoveryAttempted.
- RevocationHandled.
