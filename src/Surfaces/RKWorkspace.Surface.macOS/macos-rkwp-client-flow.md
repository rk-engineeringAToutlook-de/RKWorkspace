# macOS RKWP Client Flow

Status: MA011.08 handoff  
Datum: 2026-07-06

## Minimaler Ablauf

1. App startet als `Ablage macOS Guest`.
2. AblageIdentity aus `Application Support/RKWorkspace/identity.json` laden oder erzeugen.
3. Windows Owner Adresse aus Settings oder Testargument lesen.
4. RKWP DevLan oder SecureDev Client verbinden.
5. `AblageHello` senden.
6. Capabilities senden:
   - `FrameOnly`
   - `PdfGuest`
   - `NoFileIngress`
   - `Return`
   - `Heartbeat`
7. SecureDev/DevPairing im Lab-Modus bestaetigen.
8. `FrameSessionOpen` empfangen.
9. `FrameSessionReady` empfangen.
10. `FrameUpdate` empfangen und anzeigen.
11. Heartbeat senden, solange der Frame aktiv ist.
12. Return senden, wenn der Owner-Test `zurueckgeben` ausloest.
13. Bei Verbindungsverlust Frame auf `Verbindung verloren` setzen.

## No File Ingress

macOS darf nicht:

- PDF in Downloads, Documents, Temp oder App Container als freie Datei speichern.
- Windows Originalpfad als lokale Datei behandeln.
- Originalbytes in eine `.pdf` schreiben.

Erlaubt:

- Owner-gerenderte PNG-/Bitmap-Frames im Arbeitsspeicher anzeigen.
- Nicht-persistente Frame-Pixel nach CachePolicy halten.
- Diagnostik-Logs ohne Originalinhalt schreiben.

## Erwartete Logzeilen

```text
AblageHello: OK
FrameSessionOpen: OK
FrameSessionReady: OK
FrameUpdate: OK
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
Return: SUCCESS
```
