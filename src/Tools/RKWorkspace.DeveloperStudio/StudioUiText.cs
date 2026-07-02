namespace RKWorkspace.DeveloperStudio;

internal static class StudioUiText
{
    public static string Display(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (value.Contains(", ", StringComparison.Ordinal))
        {
            return string.Join(", ", value.Split(", ", StringSplitOptions.None).Select(Display));
        }

        return value switch
        {
            "Add Demo Workspaces" => "Demo-Arbeitsflaechen",
            "Already running" => "Laeuft bereits",
            "Available" => "Verfuegbar",
            "Clipboard" => "Zwischenablage",
            "Center" => "Mitte",
            "Completed" => "Abgeschlossen",
            "Created" => "Erstellt",
            "Create Text Object" => "Textobjekt erstellen",
            "Demo text object already exists" => "Demo-Textobjekt existiert bereits",
            "Demo workspaces available" => "Demo-Arbeitsflaechen verfuegbar",
            "DisplayNode" => "Anzeige-Knoten",
            "Display" => "Anzeige",
            "Dragging cancelled" => "Greifen abgebrochen",
            "Dragging started" => "Greifen gestartet",
            "Dual agents already running" => "Zwei lokale Flaechen laufen bereits",
            "Dual agents running" => "Zwei lokale Flaechen laufen",
            "Dual agents stopped" => "Zwei lokale Flaechen gestoppt",
            "Encryption" => "Verschluesselung",
            "FAILED" => "FEHLER",
            "Failed" => "Fehlgeschlagen",
            "Image" => "Bild",
            "Hier ablegen" => "Hier ablegen",
            "Interactive demo initialized" => "Interaktive Demo initialisiert",
            "Interactive demo ready" => "Interaktive Demo bereit",
            "Keyboard" => "Tastatur",
            "Left" => "Links",
            "Link" => "Link",
            "Logging" => "Protokollierung",
            "Missing" => "Fehlt",
            "Mouse" => "Maus",
            "Multi Window Created" => "Multi Window erstellt",
            "Multi Window Reset" => "Multi Window zurueckgesetzt",
            "No dual agents running" => "Keine zwei lokalen Flaechen aktiv",
            "Object cannot be dropped here." => "Kein gueltiges Ziel.",
            "None" => "Keine",
            "Not run" => "Noch nicht ausgefuehrt",
            "Object returned to Workspace A" => "Objekt liegt wieder auf der linken Arbeitsflaeche",
            "OfflineMode" => "Offline-Modus",
            "Pairing" => "Kopplung",
            "PDF" => "PDF",
            "Paused" => "Pausiert",
            "Prepared" => "Vorbereitet",
            "Ready" => "Bereit",
            "Ready to receive" => "Bereit zum Empfangen",
            "Reset" => "Zuruecksetzen",
            "Reset Interactive Demo" => "Interaktive Demo zuruecksetzen",
            "Right" => "Rechts",
            "Run Full Interactive Demo" => "Interaktive Demo ausfuehren",
            "Running" => "Laeuft",
            "SmartDevice" => "Smart Device",
            "Start Dual Agents" => "Zwei lokale Flaechen starten",
            "Stop Dual Agents" => "Zwei lokale Flaechen stoppen",
            "Start Runtime" => "Runtime starten",
            "Studio Created" => "Studio erstellt",
            "Stopped" => "Gestoppt",
            "Stopping" => "Stoppt",
            "SUCCESS" => "ERFOLG",
            "Target highlighted" => "Ablageflaeche reagiert",
            "Target highlight cleared" => "Ablageflaeche ruhig",
            "Text" => "Text",
            "Text Object" => "Textobjekt",
            "Transfer completed" => "Abgelegt",
            "Transfer failed" => "Ablegen fehlgeschlagen",
            "Transfer successfully completed" => "Erfolgreich abgelegt",
            "Transfer received" => "Ding angekommen",
            "Transfer Right" => "Rechts ablegen",
            "Transfer started" => "Aufgenommen",
            "Validated" => "Geprueft",
            "Workspace A / Laptop" => "Mein Arbeitsplatz links",
            "Workspace B / Display Right" => "Mein Arbeitsplatz rechts",
            "RKWS-MultiWindow-Workspace-A" => "Mein Arbeitsplatz links",
            "RKWS-MultiWindow-Workspace-B" => "Mein Arbeitsplatz rechts",
            "Candidate" => "Durchgang vorgeschlagen",
            "EdgeLocked" => "Durchgang nimmt an",
            "Cancelled" => "Abgebrochen",
            "Runtime reset" => "Runtime zurueckgesetzt",
            _ when value.StartsWith("FAILED:", StringComparison.OrdinalIgnoreCase) =>
                "FEHLER:" + value["FAILED:".Length..],
            _ when value.StartsWith("Created ", StringComparison.Ordinal) =>
                "Erstellt: " + value["Created ".Length..],
            _ when value.StartsWith("Transfer completed to ", StringComparison.Ordinal) =>
                "Abgelegt auf " + Display(value["Transfer completed to ".Length..]),
            _ when value.EndsWith(" Object", StringComparison.Ordinal) =>
                $"{Display(value[..^" Object".Length])}-Objekt",
            _ when value.StartsWith("State:", StringComparison.Ordinal) =>
                $"Status:{Display(value["State:".Length..])}",
            _ => value
        };
    }

    public static string Header(string propertyName)
    {
        return propertyName switch
        {
            "Action" => "Aktion",
            "AgentId" => "Lokal-ID",
            "Description" => "Beschreibung",
            "DisplayName" => "Anzeigename",
            "Error" => "Fehler",
            "Location" => "Ort",
            "MimeType" => "MIME-Typ",
            "Name" => "Name",
            "ObjectId" => "Ding-ID",
            "ObjectType" => "Dingart",
            "Position" => "Position",
            "Priority" => "Prioritaet",
            "Result" => "Ergebnis",
            "Runtime" => "Runtime",
            "Source" => "Von",
            "State" => "Status",
            "Status" => "Status",
            "Target" => "Nach",
            "Time" => "Zeit",
            "Trusted" => "Vertraut",
            "Type" => "Typ",
            "Workspace" => "Arbeitsflaeche",
            "WorkspaceId" => "Arbeitsflaeche-ID",
            _ => propertyName
        };
    }
}
