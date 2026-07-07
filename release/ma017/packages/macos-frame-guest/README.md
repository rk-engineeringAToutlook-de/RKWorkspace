# macOS PDF Frame Guest

Status: first visible pilot for MA017 cross-device PDF frame viewing.

Start here for a separate macOS Codex instance:

```text
../../handoff/MACOS_CODEX_REAL_PDF_FRAME_START_HERE.md
```

## Ziel

Windows bleibt Owner der echten PDF. macOS zeigt nur einen gerenderten PNG-Frame im Speicher.

macOS darf nicht:

- die Original-PDF speichern.
- den Originalpfad erhalten.
- Originalbytes erhalten.
- eine freie PDF-Datei in Finder, Downloads, tmp oder Cache erzeugen.

## Windows starten

Auf Windows:

~~~powershell
cd "E:\HiDrive\users\RK Workspace\RKWorkspace"
.\tools\run-macos-pdf-frame-owner.ps1
~~~

Die Ausgabe nennt die Windows-IP, die der Mac verwenden soll.

## macOS starten

Auf dem Mac:

~~~bash
cd "/path/to/RKWorkspace/release/ma017/packages/macos-frame-guest"
swift run MacPdfFrameGuest --host WINDOWS_IP --port 57120
~~~

Danach im macOS-Fenster `Frame holen` druecken. Die PDF-Seite muss sichtbar erscheinen.

## Erwartung

~~~text
macOSGuestAblage: STARTED
FrameView: OK
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
FrameCache: MemoryOnly
NoFileIngress: SUCCESS
~~~
