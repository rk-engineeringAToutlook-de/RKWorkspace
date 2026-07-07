# MA017 Go / No-Go Criteria

Status: Draft
Datum: 2026-07-07

## Windows

GO:

- Closed PDF Capsule gruen.
- Open PDF Frame gruen.
- No File Ingress gruen.
- Owner Lock sichtbar.

NO-GO:

- Gast erhaelt Originalpfad oder PDF-Bytes.
- Recovery laesst Owner ohne Kontrolle.

## macOS

GO:

- native Guest Surface startet.
- Contract wird gelesen.
- Frame wird angezeigt.
- No File Ingress bleibt nachvollziehbar.

NO-GO:

- Sandbox oder Berechtigung erzwingt Dateiimport.
- Rueckgabe fehlt.

## iPad / iPhone

GO:

- Xcode/USB-Install laeuft.
- Frame Presenter zeigt Objekt.
- Haptik stoert nicht.
- Touch-Interaktion verletzt keine Owner-Regel.

NO-GO:

- iOS Sandbox erzeugt eine freie Originalkopie.
- Owner versteht nicht, wo das Original bleibt.

## Grosstest

GO:

- mindestens zwei reale Gastablaegen koennen nacheinander getestet werden.
- Proximity waehlt stabil die naechste Ablage.
- Owner bewertet mindestens einen Pfad Gruen oder stabiles Gelb.

NO-GO:

- mehrere Ziele konkurrieren sichtbar.
- Security oder No File Ingress wird unsicher.

