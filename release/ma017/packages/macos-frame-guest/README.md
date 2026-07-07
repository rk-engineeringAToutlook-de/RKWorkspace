# macOS PDF Frame Guest

Status: live pilot for MA017 cross-device transient PDF frame viewing.

Start here for a separate macOS Codex instance:

```text
../../handoff/MACOS_CODEX_REAL_PDF_FRAME_START_HERE.md
```

## Ziel

Windows bleibt Owner der echten PDF. macOS zeigt die PDF nur als fluechtige Memory-Lease in einem nativen Frame.

macOS darf nicht:

- die PDF als Finder-Datei, Download, Cache-Datei oder tmp-Datei speichern.
- den Originalpfad erhalten.
- die PDF dauerhaft behalten.
- Speichern, Exportieren oder Ablegen aus dem Frame anbieten.

macOS darf:

- die PDF-Bytes nur im Speicher des Frame-Fensters halten.
- Textauswahl/Kopieren im Frame erlauben.
- die Frame-Lease beim Schliessen, Zurueckgeben oder Verbindungsverlust verwerfen.
- mehrere uebergebene PDFs parallel in eigenen Frame-Fenstern anzeigen.

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

Der Guest bleibt im Hintergrund aktiv. Es gibt im Idle kein Wartefenster und keine dauerhafte Glaskante. Ein neues Frame-Fenster oeffnet erst, wenn Windows eine gueltige fluechtige PDF-Lease committed.

Fuer den sichtbaren Windows-Glasrand-Livefluss wartet macOS schon vor dem Ablegen:

~~~bash
swift run MacPdfFrameGuest --host WINDOWS_IP --port 57120 --wait-for-placement
~~~

Fuer den Live-Pilot als dauerhafte macOS-Ablage wird die App als lokales `.app`-Paket mit eigener Netzwerkfreigabe gestartet:

~~~bash
cd "/path/to/RKWorkspace"
tools/run-macos-pdf-frame-guest.sh --host 192.168.163.11 --port 57120 --wait-for-placement --direction Left
~~~

Der Dienst laeuft als interaktive LaunchAgent-App weiter. Die macOS-Abfrage fuer lokale Netzwerke muss mit `Erlauben` bestaetigt werden. Die Glaskante liegt als Desktop-Overlay an der erkannten Windows-Seite, nicht im PDF-Frame.

Live-Verhalten:

- Der Listener bleibt nach jedem geschlossenen Fenster aktiv.
- Jede neue PDF-Lease oeffnet ein eigenes neues Frame-Fenster.
- Schliessen eines Fensters gibt genau diese Lease an Windows zurueck und verwirft die macOS-Speicherkopie.
- Die Glaskante erscheint nur als Portalimpuls bei echter Carry-/Transfer-Aktivitaet oder gueltiger Frame-Lease.
- Der Portalimpuls oeffnet schnell und klingt langsam wieder aus.

## macOS Desktop-Owner starten

Fuer den Gegenweg Mac -> Windows:

~~~bash
tools/run-macos-real-pdf-glass-portal-owner.sh --target-host 192.168.163.11
~~~

Der Prozess bleibt im Hintergrund. `Option` gedrueckt halten und kurz auf eine geoeffnete PDF oder eine PDF-Auswahl im Finder halten nimmt genau dieses Ding in die Hand. Fallback: `Option` + Leertaste, waehrend der Mauszeiger ueber der PDF liegt. Das Ding liegt danach klein ueber dem Desktop, der Glasrand oeffnet sich in Richtung der naechsten Ablage und macOS gibt nur den PNG-Frame frei. Die Original-PDF bleibt auf macOS.

## Erwartung

~~~text
macOSGuestAblage: STARTED
FrameView: OK
TransientPdfLease: OK
GuestPersistedPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
PDFCache: MemoryOnly
TextSelection: OK
NoDiskPdf: SUCCESS
~~~
