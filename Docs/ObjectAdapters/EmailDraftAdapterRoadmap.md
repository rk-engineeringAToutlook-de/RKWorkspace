# Email Draft Adapter Roadmap

Status: MA013.49  
Datum: 2026-07-06

## Ziel

E-Mail-Entwuerfe sollen spaeter als Arbeitsobjekte erscheinen, ohne Mailbox-Besitz, Accounts oder vertrauliche Inhalte unkontrolliert zu kopieren.

## Quellen

- Outlook Desktop / Microsoft 365
- Apple Mail
- Webmail
- `.eml` Export
- `.msg` Export

## Pfade

| Pfad | Beschreibung | Status |
| --- | --- | --- |
| Interactive frame | Entwurf bleibt in Mail-App, Guest sieht/bedient Frame | Zielpfad fuer Arbeitsgefuehl |
| Draft handoff | Entwurf wird kontrolliert an andere Ablage uebergeben | nur mit Policy |
| `.eml/.msg` export | expliziter Datei-/Snapshot-Export | confirmation required |
| Webmail reference | URL/Tab-Referenz | BrowserTabStrategy beachten |

## Regeln

- Kein automatischer Versand.
- Kein automatischer Ownership Transfer.
- Attachments bleiben eigene Objekte.
- CopyOut nur mit Bestaetigung.
- Audit bei Export, Handoff und Policy-Deny.

## Empfehlung

Fuer den ersten Pilot nur Roadmap und Frame-Metadaten. Produktpfad spaeter ueber Outlook/Apple-Mail-spezifische Adapter plus klare User-Bestaetigung.
