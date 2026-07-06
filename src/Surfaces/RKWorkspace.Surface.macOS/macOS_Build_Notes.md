# macOS Build Notes

Status: Prepared handoff  
Datum: 2026-07-06

## Empfohlener Start

1. GitHub-Repo klonen oder aktualisieren.
2. Branch `feature/ma009-secure-cross-device-frame-foundation` holen.
3. Context Pack lesen.
4. Minimal-App in Xcode anlegen.
5. RKWP-Protokollmodelle entweder portieren oder ueber einen klaren Dev-Client anbinden.

## Minimaler Build

Erster Build darf klein sein:

- eine App startet.
- AblageIdentity wird angezeigt oder geloggt.
- ein Mock-Frame kann sichtbar werden.
- No File Ingress Status wird geloggt.

Danach erst DevTransport anbinden.

## DevTransport

Aktueller Windows-Pfad:

```text
dev+namedpipe://rkws-windows-owner-macos
```

Dieser ist Windows-lokal. Fuer echten macOS-Test braucht es:

```text
rkwp+tcp-dev://<windows-host>:43707
```

Das Netzwerkprofil ist noch zu bauen.

## Build-Bericht

macOS-Codex soll melden:

- Xcode-Version.
- macOS-Version.
- Branch und Commit.
- Berechtigungen.
- Buildstatus.
- ob Frame sichtbar war.
- No File Ingress Status.
- Rueckgabe/Recovery Status.
