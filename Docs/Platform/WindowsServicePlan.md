# Windows Service Plan

Status: MA010.08

## Grundsatz

Der Windows Service ist ein spaeterer Produktpfad. MA010.08 installiert keinen Dienst.

## Warum noch kein Service

Vor einer produktiven Service-Installation fehlen noch:

- finale Secure Session.
- produktiver Trust Store.
- Zertifikats- und Secret-Handling.
- Firewall- und Update-Konzept.
- klare Benutzer-/Dienstkontext-Trennung.
- Recovery- und Audit-Hardening.

## Geplanter Service-Umfang

Ein spaeterer Dienst kann:

- RKWP Transport Host starten.
- AblageIdentity und Policy laden.
- Heartbeats ueberwachen.
- Recovery ausfuehren.
- Audit persistieren.
- lokale Surface-/Overlay-Prozesse koordinieren.

Nicht im Dienst laufen sollte:

- sichtbare Glass Edge UI.
- globale Eingabe ohne explizite Berechtigung.
- unsichere Dev-Secrets.
- Benutzerinteraktion ohne Benutzerkontext.

## Dev-Scripts

```powershell
.\tools\install-windows-agent-dev.ps1
.\tools\uninstall-windows-agent-dev.ps1
```

Beide sind aktuell Stubs und melden `ServiceInstall: NOT_PERFORMED`.
