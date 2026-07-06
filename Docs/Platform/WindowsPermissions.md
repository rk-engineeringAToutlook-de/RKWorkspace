# Windows Permissions

Status: MA010.08

## Ziel

Windows Agent und Shell sollen mit so wenig Rechten wie moeglich laufen.

## Aktueller Stand

MA010.08 Dev-Agent:

- keine Admin-Rechte erforderlich.
- keine Service-Installation.
- kein Autostart.
- kein produktiver Firewall-Eintrag.
- kein Secret Store.

## Spaetere Berechtigungen

Zu pruefen:

- Benutzerkontext fuer sichtbare Surface/Overlay.
- Dienstkontext nur fuer Hintergrundkoordination.
- Firewall-Regel fuer RKWP Secure Transport.
- Zertifikatsspeicher fuer produktive Ablage-Identitaeten.
- lokale Secrets mit DPAPI oder Windows Credential Manager.
- Logs ohne sensitive Originalpfade auf Guest-Seite.
- Update-/Rollback-Rechte.

## Nicht erlaubt

- Admin-Rechte als Default.
- stiller Firewall-Eintrag ohne Owner-Freigabe.
- Speicherung von Owner-PDFs auf Guest.
- globale Hooks ohne explizite Berechtigung.
- Screen Capture ohne klare Purpose- und Exclusion-Regeln.
