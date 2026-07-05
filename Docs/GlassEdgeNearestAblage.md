# Glass Edge Nearest Ablage

Status: Accepted  
Datum: 2026-07-05

## Ziel

Wenn ein Mensch ein digitales Ding nimmt, zeigt RK Workspace genau eine gläserne Kante. Diese Kante liegt an der Richtung, in der die naechste passende Ablage liegt.

Nicht mehrere Ziele. Nicht mehrere Bubbles. Nicht Radar.

## Ablauf

1. Das Ding wird genommen.
2. `IAblageProximityProvider` liefert eine Raumkarte.
3. `INearestAblageSelector` waehlt genau eine Ablage.
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

## Entfernung

Entfernung steuert:

- Deckkraft
- Dicke
- Glasreflex
- Namensanzeige
- Aktivierungsschwelle

Je naeher die Ablage, desto praesenter die Kante.

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
