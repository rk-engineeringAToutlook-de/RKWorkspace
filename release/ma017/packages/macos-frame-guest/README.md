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

Fuer den sichtbaren Windows-Glasrand-Livefluss wartet macOS schon vor dem Ablegen:

~~~bash
swift run MacPdfFrameGuest --host WINDOWS_IP --port 57120 --wait-for-placement
~~~

## macOS als Owner starten

Fuer den Gegenweg Mac -> Windows:

~~~bash
tools/run-macos-real-pdf-glass-portal-owner.sh --target-host 192.168.163.11
~~~

Das Fenster laedt eine echte PDF, nimmt sie per langem Halten in die Hand, oeffnet den Glasrand und gibt danach nur den PNG-Frame frei. Die Original-PDF bleibt auf macOS.

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
