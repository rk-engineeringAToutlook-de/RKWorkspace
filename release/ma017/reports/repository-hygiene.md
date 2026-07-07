# MA017 Repository Hygiene Report

Status: Draft
Datum: 2026-07-07

## Zweck

Dieser Report dokumentiert die Repository-Hygiene fuer MA017 Packaging.

## Regeln

- keine `bin/`-Artefakte versionieren
- keine `obj/`-Artefakte versionieren
- keine lokalen Secrets in Context-Packages
- keine freien Original-PDFs aus Guest-Pfaden
- Reports deterministisch halten, soweit sie committed werden

## Gate

```powershell
.\tools\check-ma017-repository-hygiene.ps1 -AllowDirty
```

## Ergebnis

```text
MA017RepositoryHygiene: SUCCESS
RESULT: SUCCESS
```
