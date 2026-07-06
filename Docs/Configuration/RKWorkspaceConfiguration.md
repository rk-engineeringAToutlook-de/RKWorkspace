# RK Workspace Configuration

Status: MA010.09

## Ziel

RK Workspace erhaelt ein einheitliches Konfigurationssystem fuer:

- Ablage Identity.
- RKWP Transport.
- Security Mode.
- Policy Profile.
- Proximity / Manual Map.
- Frame Cache.
- Surface.
- Gesture.

## Projekt

```text
src/Configuration/RKWorkspace.Configuration/
src/Tools/RKWorkspace.ConfigTool/
tools/run-config-tool.ps1
```

## Start

```powershell
.\tools\run-config-tool.ps1 -Validate
.\tools\run-config-tool.ps1 -Show
.\tools\run-config-tool.ps1 -CreateSample
.\tools\run-config-tool.ps1 -SmokeTest
```

## Samples

Versionierte Samples:

```text
config/samples/rkworkspace.sample.json
config/samples/ablage.sample.json
config/samples/rkwp-dev.sample.json
config/samples/policy-critical.sample.json
config/samples/manual-ablage-map.sample.json
```

Lokale echte Configs liegen unter `config/` und werden nicht versioniert.

## Validierung

Der Smoke prueft:

- Sample Config laedt.
- ungueltige Config wird abgelehnt.
- Critical Policy validiert.
- Development Config validiert.
- ManualMap Config existiert.
- lokale Secrets werden nicht erwartet.

## Sicherheitsregeln

- `NoFileIngress` muss true bleiben.
- `AllowGuestFileIngress` muss false bleiben.
- `CriticalInfrastructure` erfordert SecureSession.
- `CriticalInfrastructure` blockiert OwnershipTransfer.
- Ports muessen im gueltigen Bereich liegen.
- `DevelopmentInsecure` erzeugt eine Warnung und bleibt Lab-only.
