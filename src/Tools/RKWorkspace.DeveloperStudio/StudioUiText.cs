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
            "Dragging cancelled" => "Ziehen abgebrochen",
            "Dragging started" => "Ziehen gestartet",
            "Dual agents already running" => "Zwei Agenten laufen bereits",
            "Dual agents running" => "Zwei Agenten laufen",
            "Dual agents stopped" => "Zwei Agenten gestoppt",
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
            "No dual agents running" => "Keine zwei Agenten aktiv",
            "Object cannot be dropped here." => "Kein gueltiges Ziel.",
            "None" => "Keine",
            "Not run" => "Noch nicht ausgefuehrt",
            "Object returned to Workspace A" => "Objekt wurde zu Arbeitsflaeche A zurueckgelegt",
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
            "Start Dual Agents" => "Zwei Agenten starten",
            "Start Runtime" => "Runtime starten",
            "Studio Created" => "Studio erstellt",
            "Stopped" => "Gestoppt",
            "Stopping" => "Stoppt",
            "SUCCESS" => "ERFOLG",
            "Target highlighted" => "Ziel markiert",
            "Target highlight cleared" => "Zielmarkierung entfernt",
            "Text" => "Text",
            "Text Object" => "Textobjekt",
            "Transfer completed" => "Transfer abgeschlossen",
            "Transfer failed" => "Transfer fehlgeschlagen",
            "Transfer successfully completed" => "Transfer erfolgreich abgeschlossen",
            "Transfer received" => "Transfer empfangen",
            "Transfer Right" => "Nach rechts uebertragen",
            "Transfer started" => "Transfer gestartet",
            "Validated" => "Geprueft",
            "Workspace A / Laptop" => "Arbeitsflaeche A / Laptop",
            "Workspace B / Display Right" => "Arbeitsflaeche B / Anzeige rechts",
            "Runtime reset" => "Runtime zurueckgesetzt",
            _ when value.StartsWith("FAILED:", StringComparison.OrdinalIgnoreCase) =>
                "FEHLER:" + value["FAILED:".Length..],
            _ when value.StartsWith("Created ", StringComparison.Ordinal) =>
                "Erstellt: " + value["Created ".Length..],
            _ when value.StartsWith("Transfer completed to ", StringComparison.Ordinal) =>
                "Transfer abgeschlossen nach " + value["Transfer completed to ".Length..],
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
            "AgentId" => "Agent-ID",
            "Description" => "Beschreibung",
            "DisplayName" => "Anzeigename",
            "Error" => "Fehler",
            "Location" => "Ort",
            "MimeType" => "MIME-Typ",
            "Name" => "Name",
            "ObjectId" => "Objekt-ID",
            "ObjectType" => "Objekttyp",
            "Position" => "Position",
            "Priority" => "Prioritaet",
            "Result" => "Ergebnis",
            "Runtime" => "Runtime",
            "Source" => "Quelle",
            "State" => "Status",
            "Status" => "Status",
            "Target" => "Ziel",
            "Time" => "Zeit",
            "Trusted" => "Vertraut",
            "Type" => "Typ",
            "Workspace" => "Arbeitsflaeche",
            "WorkspaceId" => "Workspace-ID",
            _ => propertyName
        };
    }
}
