# Glass Edge Nearest Ablage

Status: Accepted  
Datum: 2026-07-05

## Ziel

Wenn ein Mensch ein digitales Ding nimmt, zeigt RK Workspace genau eine gläserne Kante. Diese Kante liegt an der Richtung, in der die naechste passende Ablage liegt.

Nicht mehrere Ziele. Nicht mehrere Bubbles. Nicht Radar.

## Ablauf

1. Das Ding wird genommen.
2. `IAblageProximityProvider` liefert eine Raumkarte.
3. `INearestAblageSelector` waehlt genau eine Ablage anhand von Distanz, Confidence und Hysterese.
4. `GlassEdge` erscheint an `Left`, `Right`, `Up` oder `Down`.
5. Das Ding wird in diese Kante gefuehrt.
6. `WorkspaceSurfaceHandoff` beschreibt Quelle, Ziel, Gegenkante und Zielposition.
7. Die Zielablage zeigt einen Ghost an der passenden Gegenkante.

## V1 Auswahl

Die simulierte Raumkarte setzt:

- aktuelle Ablage: Windows
- naechste Ablage: macOS rechts
- weitere Ablagen: iPhone oben, Tablet diagonal unten rechts, Android links

Damit erscheint im Windows-Test eine rechte gläserne Kante.

## Manual Map

MA007.13 fuegt die manuelle Raumkarte als vorbereitete Quelle hinzu. Sie ist fuer den ersten echten Raumaufbau gedacht, bevor BLE, UWB oder Dongle-Messung existieren.

MA009.05 macht diese Quelle per Tool nutzbar:

```powershell
.\tools\run-manual-map.ps1 -Set -Ablage macOS -Direction Right -Distance Near
.\tools\run-manual-map.ps1 -List
```

Beispiel:

- macOS rechts, nah
- iPad oben, mittel
- iPhone unten, nah
- Monitor links, weit

Auch bei mehreren Eintraegen gilt: Die Shell zeigt nur eine gläserne Kante. Die anderen Ablagen bleiben technisch bekannt, aber nicht visuell dominant.

Die Provider-Prioritaet ist:

1. ManualMap, wenn Eintraege vorhanden sind.
2. Simulated.
3. spaeter BLE/UWB/Dongle.

## Entfernung

Entfernung steuert:

- Deckkraft
- Dicke
- Glasreflex
- Namensanzeige
- Aktivierungsschwelle

Je naeher die Ablage, desto praesenter die Kante.

## Anti-Flicker

Die Kante darf nicht springen, nur weil zwei Ablagen aehnlich nah sind. Der Selector nutzt:

- `MinimumConfidence`
- `DistanceHysteresis`
- `StableNearestDuration`
- `EdgeSwitchDelay`

Ein klares neues Ziel darf wechseln. Eine kleine Schwankung bleibt bei der bisherigen Kante.

## Eintrittsvarianten

Taste `1`: Whole Edge  
Taste `2`: Focus Point  
Taste `3`: Directional Slot

Der Owner entscheidet spaeter, welche Variante sich am natuerlichsten anfuehlt.

## Verbindung zu RKWP

Ab MA007.00 ist die gläserne Kante nicht nur ein visueller Zielhinweis. Sie ist der Einstieg in eine RKWP-Session:

1. Naechste Ablage wird ueber Proximity bestimmt.
2. Glass Edge zeigt genau diese Richtung.
3. Bei erfolgreichem Ablegen wird fuer kritische Objekte zuerst `FrameOnly` angefragt.
4. Der Owner bleibt Owner.
5. Die Zielablage zeigt eine `FrameSession`, keine Originaldatei.

Damit bleibt die Wahrnehmung raeumlich, waehrend die technische Ownership sauber bleibt.

## MA007.04 Integration

`GlassEdgePdfFrameDemo` verbindet diesen Pfad mit `samples/Objects/Rechnung.pdf`. Die simulierte naechste Ablage ist macOS rechts, die rechte Glass Edge wird aktiv und loest einen Original-Owned PDF-Frame aus.

Der Demo-Smoke prueft:

- echte Sample-PDF existiert,
- naechste Ablage ist bestimmt,
- Glass Edge ist aktiv,
- CarryLease und FrameSession sind aktiv,
- Owner ist gesperrt,
- Guest sieht nur den Frame,
- No File Ingress ist erfolgreich,
- Rueckgabe und Recovery sind erfolgreich.

## MA008.04 E2E Integration

`GlassEdgePdfFrameE2E` verbindet den Glass-Edge-Hauptpfad mit dem lokalen Windows PDF Frame E2E-Slice. Der Smoke erzeugt echte RKWP-Eventnachrichten fuer:

- `GlassEdgeAppearing`
- `GlassEdgeActive`
- `ObjectEnteringEdge`
- `ObjectInTransit`
- `ObjectEmerging`
- `ObjectPlaced`
- `FrameSessionOpen`
- `FrameSessionReady`

Die Kante bleibt ein raeumlicher Hinweis. Technisch entsteht daraus eine Original-Owned FrameSession auf der naechsten Ablage, keine Dateiuebertragung.
