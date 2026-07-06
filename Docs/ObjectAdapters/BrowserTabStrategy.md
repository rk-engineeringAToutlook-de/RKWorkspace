# Browser Tab Strategy

Status: MA013.48  
Datum: 2026-07-06

## Ziel

Browser-Tabs werden als RK Workspace Objects vorbereitet, ohne dass RK Workspace Browserdaten unkontrolliert kopiert oder Besitz an Webseiten vortaeuscht.

## Optionen

| Option | Beschreibung | Risiko |
| --- | --- | --- |
| URL reference | Nur URL, Titel und Browserprofil-Referenz | Auth/Login-Kontext fehlt auf Guest |
| Frame snapshot | sichtbarer Tab als Frame/Screenshot | Aktualitaet und geschuetzte Inhalte |
| Extension later | Browser-Erweiterung liefert Tab-Metadaten kontrolliert | Store Review, Browser-spezifisch |
| Browser adapter | nativer Adapter pro Browser | Wartung und Rechte |
| Interactive frame | Owner-Browser bleibt aktiv, Guest interagiert per Frame | Input-/Security-Policy kritisch |

## Security

- Kein automatischer Cookie-/Session-Export.
- Kein Passwort-/Token-Material in RKWP.
- URL CopyOut nur mit Policy.
- Interaktion nur mit expliziter FramePolicy.
- Private/Incognito Tabs default blockiert.

## Empfehlung

Start mit URL reference plus optionalem Frame snapshot. Browser Extension und InteractiveFrame folgen erst nach Security- und UX-Validierung.
