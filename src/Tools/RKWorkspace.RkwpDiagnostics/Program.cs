using RKWorkspace.Protocol;
using RKWorkspace.Protocol.Diagnostics;

var options = RkwpDiagnosticsOptions.Parse(args, FindRoot());
var store = new JsonlRkwpAuditLogStore();

try
{
    if (options.ShowHelp)
    {
        PrintHelp();
        return 0;
    }

    if (options.SmokeTest)
    {
        var logPath = CreateSmokeLog(options.Root, store);
        var records = store.ReadAll(logPath);
        var diagnostics = RkwpSessionDiagnostics.FromEvents(records);
        Print("SmokeTest", logPath, records, diagnostics);
        PrintAuditList(records);
        PrintSessionView(records);
        PrintLeaseView(records);
        PrintViolations(records);
        var exportPath = Path.Combine(Path.GetTempPath(), $"rkws-diagnostics-{Guid.NewGuid():N}.md");
        ExportMarkdown(exportPath, logPath, records, diagnostics);
        var markdownOk = File.Exists(exportPath) && File.ReadAllText(exportPath).Contains("RK Workspace Diagnostics", StringComparison.OrdinalIgnoreCase);
        File.Delete(exportPath);
        var success = diagnostics.ActiveSessions == 1 &&
                      diagnostics.FrameSessions == 1 &&
                      diagnostics.Heartbeats == 1 &&
                      diagnostics.PolicyDeniedEvents == 1 &&
                      diagnostics.RecoveredLeases == 1 &&
                      diagnostics.NoFileIngressPassed &&
                      markdownOk;
        Console.WriteLine("AuditList: OK");
        Console.WriteLine("SessionView: OK");
        Console.WriteLine("LeaseView: OK");
        Console.WriteLine("ViolationsView: OK");
        Console.WriteLine($"MarkdownExport: {(markdownOk ? "OK" : "FAILED")}");
        Console.WriteLine($"RkwpDiagnosticsSmoke: {(success ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"RESULT: {(success ? "SUCCESS" : "FAILED")}");
        return success ? 0 : 1;
    }

    var readPath = options.ReadLogPath ?? FindLatestLog(options.Root);
    if (readPath is null)
    {
        Console.WriteLine("RK Workspace RKWP Diagnostics");
        Console.WriteLine("-----------------------------");
        Console.WriteLine("Mode: Latest");
        Console.WriteLine("AuditLog: NONE");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    var readRecords = store.ReadAll(readPath);
    var readDiagnostics = RkwpSessionDiagnostics.FromEvents(readRecords);
    Print(options.ReadLogPath is null ? "Latest" : "ReadLog", readPath, readRecords, readDiagnostics);
    if (options.AuditList)
    {
        PrintAuditList(readRecords);
    }

    if (options.Session)
    {
        PrintSessionView(readRecords);
    }

    if (options.Lease)
    {
        PrintLeaseView(readRecords);
    }

    if (options.Violations)
    {
        PrintViolations(readRecords);
    }

    if (!string.IsNullOrWhiteSpace(options.ExportMarkdownPath))
    {
        ExportMarkdown(options.ExportMarkdownPath, readPath, readRecords, readDiagnostics);
        Console.WriteLine($"MarkdownExport: {Path.GetFullPath(options.ExportMarkdownPath)}");
    }

    Console.WriteLine("RESULT: SUCCESS");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine("RK Workspace RKWP Diagnostics");
    Console.WriteLine("-----------------------------");
    Console.WriteLine($"RESULT: FAILED - {ex.Message}");
    return 1;
}

static void PrintHelp()
{
    Console.WriteLine("RK Workspace RKWP Diagnostics");
    Console.WriteLine("-----------------------------");
    Console.WriteLine("Usage:");
    Console.WriteLine("  run-rkwp-diagnostics.ps1");
    Console.WriteLine("  run-rkwp-diagnostics.ps1 -SmokeTest");
    Console.WriteLine("  run-rkwp-diagnostics.ps1 -ReadLog <path>");
    Console.WriteLine("  run-rkwp-diagnostics.ps1 -AuditList -Session -Lease -Violations");
    Console.WriteLine("  run-rkwp-diagnostics.ps1 -ExportMarkdown diagnostics.md");
}

static string CreateSmokeLog(string root, JsonlRkwpAuditLogStore store)
{
    var now = DateTimeOffset.UtcNow;
    var sessionId = $"rkwp-session-{Guid.NewGuid():N}";
    var leaseId = $"lease-{Guid.NewGuid():N}";
    var frameSessionId = $"frame-{Guid.NewGuid():N}";
    var thingId = "pdf-7244f2ee54f0b3cc";
    const string owner = "ablage-windows-owner";
    const string guest = "ablage-macos-guest";

    var records = new[]
    {
        RkwpAuditLogRecord.Create(
            RkwpAuditEventType.SessionStarted,
            sessionId,
            owner,
            guest,
            "Development session started.",
            now),
        RkwpAuditLogRecord.Create(
            RkwpAuditEventType.LeaseGranted,
            sessionId,
            owner,
            guest,
            "FrameOnly lease granted.",
            now.AddMilliseconds(10),
            leaseId,
            thingId: thingId,
            metadata: new Dictionary<string, string> { ["mode"] = "FrameOnly" }),
        RkwpAuditLogRecord.Create(
            RkwpAuditEventType.FrameOpened,
            sessionId,
            owner,
            guest,
            "PDF frame opened.",
            now.AddMilliseconds(20),
            leaseId,
            frameSessionId,
            thingId),
        RkwpAuditLogRecord.Create(
            RkwpAuditEventType.NoFileIngressChecked,
            sessionId,
            owner,
            guest,
            "Guest frame contains no original file.",
            now.AddMilliseconds(30),
            leaseId,
            frameSessionId,
            thingId,
            metadata: new Dictionary<string, string>
            {
                ["status"] = "success",
                ["guestHasPdfFile"] = "false",
                ["guestHasOriginalPath"] = "false",
                ["originalFileBytes"] = "false"
            }),
        RkwpAuditLogRecord.Create(
            RkwpAuditEventType.Heartbeat,
            sessionId,
            guest,
            owner,
            "Carry lease heartbeat.",
            now.AddMilliseconds(40),
            leaseId,
            frameSessionId,
            thingId),
        RkwpAuditLogRecord.Create(
            RkwpAuditEventType.PolicyDenied,
            sessionId,
            owner,
            guest,
            "Keyboard input denied by FramePolicy.",
            now.AddMilliseconds(50),
            leaseId,
            frameSessionId,
            thingId,
            RkwpAuditSeverity.Warning,
            new Dictionary<string, string> { ["input"] = "KeyboardText" }),
        RkwpAuditLogRecord.Create(
            RkwpAuditEventType.FrameReturned,
            sessionId,
            guest,
            owner,
            "Frame returned.",
            now.AddMilliseconds(60),
            leaseId,
            frameSessionId,
            thingId),
        RkwpAuditLogRecord.Create(
            RkwpAuditEventType.RecoveredByOwner,
            sessionId,
            owner,
            guest,
            "Owner recovery verified.",
            now.AddMilliseconds(70),
            leaseId,
            frameSessionId,
            thingId)
    };

    var directory = Path.Combine(root, "logs", "rkwp-audit");
    var logPath = Path.Combine(directory, $"rkwp-audit-smoke-{DateTime.UtcNow:yyyyMMdd-HHmmss}.jsonl");
    store.WriteAll(logPath, records);
    return logPath;
}

static void Print(string mode, string logPath, IReadOnlyList<RkwpAuditLogRecord> records, RkwpSessionDiagnostics diagnostics)
{
    Console.WriteLine("RK Workspace RKWP Diagnostics");
    Console.WriteLine("-----------------------------");
    Console.WriteLine($"Mode: {mode}");
    Console.WriteLine($"AuditLog: {logPath}");
    Console.WriteLine($"Events: {records.Count}");
    Console.WriteLine($"ActiveSessions: {diagnostics.ActiveSessions}");
    Console.WriteLine($"ActiveLeases: {diagnostics.ActiveLeases}");
    Console.WriteLine($"ExpiredLeases: {diagnostics.ExpiredLeases}");
    Console.WriteLine($"RecoveredLeases: {diagnostics.RecoveredLeases}");
    Console.WriteLine($"FrameSessions: {diagnostics.FrameSessions}");
    Console.WriteLine($"Heartbeats: {diagnostics.Heartbeats}");
    Console.WriteLine($"PolicyDenied: {diagnostics.PolicyDeniedEvents}");
    Console.WriteLine($"SecurityViolations: {diagnostics.SecurityViolations}");
    Console.WriteLine($"OwnershipTransferRequests: {diagnostics.OwnershipTransferRequests}");
    Console.WriteLine($"Revocations: {diagnostics.Revocations}");
    Console.WriteLine($"NoFileIngress: {(diagnostics.NoFileIngressPassed ? "SUCCESS" : "UNKNOWN")}");
}

static void PrintAuditList(IReadOnlyList<RkwpAuditLogRecord> records)
{
    Console.WriteLine("Audit List");
    Console.WriteLine("----------");
    foreach (var record in records.Take(20))
    {
        Console.WriteLine($"{record.Timestamp:O} {record.EventType} {record.Severity} {record.Message}");
    }
}

static void PrintSessionView(IReadOnlyList<RkwpAuditLogRecord> records)
{
    Console.WriteLine("Sessions");
    Console.WriteLine("--------");
    foreach (var group in records.GroupBy(record => record.SessionId).OrderBy(group => group.Key))
    {
        Console.WriteLine($"{group.Key}: Events={group.Count()} Last={group.Max(record => record.Timestamp):O}");
    }
}

static void PrintLeaseView(IReadOnlyList<RkwpAuditLogRecord> records)
{
    Console.WriteLine("Leases");
    Console.WriteLine("------");
    foreach (var group in records.Where(record => !string.IsNullOrWhiteSpace(record.LeaseId)).GroupBy(record => record.LeaseId).OrderBy(group => group.Key))
    {
        Console.WriteLine($"{group.Key}: Events={group.Count()} Last={group.Max(record => record.Timestamp):O}");
    }
}

static void PrintViolations(IReadOnlyList<RkwpAuditLogRecord> records)
{
    Console.WriteLine("Violations");
    Console.WriteLine("----------");
    var violations = records
        .Where(record => record.EventType is RkwpAuditEventType.PolicyDenied or RkwpAuditEventType.SecurityViolation)
        .ToArray();
    if (violations.Length == 0)
    {
        Console.WriteLine("NONE");
        return;
    }

    foreach (var record in violations)
    {
        Console.WriteLine($"{record.EventType}: {record.Message}");
    }
}

static void ExportMarkdown(string path, string logPath, IReadOnlyList<RkwpAuditLogRecord> records, RkwpSessionDiagnostics diagnostics)
{
    var directory = Path.GetDirectoryName(Path.GetFullPath(path));
    if (!string.IsNullOrWhiteSpace(directory))
    {
        Directory.CreateDirectory(directory);
    }

    var lines = new List<string>
    {
        "# RK Workspace Diagnostics",
        string.Empty,
        $"AuditLog: `{logPath}`",
        $"Events: {records.Count}",
        $"ActiveSessions: {diagnostics.ActiveSessions}",
        $"FrameSessions: {diagnostics.FrameSessions}",
        $"PolicyDenied: {diagnostics.PolicyDeniedEvents}",
        $"SecurityViolations: {diagnostics.SecurityViolations}",
        $"NoFileIngress: {(diagnostics.NoFileIngressPassed ? "SUCCESS" : "UNKNOWN")}",
        string.Empty,
        "## Recent Events"
    };
    lines.AddRange(records.Take(20).Select(record => $"- `{record.Timestamp:O}` {record.EventType}: {record.Message}"));
    File.WriteAllLines(path, lines);
}

static string? FindLatestLog(string root)
{
    var directory = Path.Combine(root, "logs", "rkwp-audit");
    if (!Directory.Exists(directory))
    {
        return null;
    }

    return Directory
        .EnumerateFiles(directory, "*.jsonl")
        .OrderByDescending(File.GetLastWriteTimeUtc)
        .FirstOrDefault();
}

static string FindRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
    {
        directory = directory.Parent;
    }

    if (directory is null)
    {
        throw new InvalidOperationException("Repository root could not be located.");
    }

    return directory.FullName;
}

internal sealed record RkwpDiagnosticsOptions(
    string Root,
    bool SmokeTest,
    string? ReadLogPath,
    bool ShowHelp,
    bool AuditList,
    bool Session,
    bool Lease,
    bool Violations,
    string? ExportMarkdownPath)
{
    public static RkwpDiagnosticsOptions Parse(string[] args, string root)
    {
        var smokeTest = false;
        var readLogPath = (string?)null;
        var showHelp = false;
        var auditList = false;
        var session = false;
        var lease = false;
        var violations = false;
        var exportMarkdownPath = (string?)null;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "-Help", StringComparison.OrdinalIgnoreCase))
            {
                showHelp = true;
                continue;
            }

            if (string.Equals(arg, "--smoke-test", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "-SmokeTest", StringComparison.OrdinalIgnoreCase))
            {
                smokeTest = true;
                continue;
            }

            if ((string.Equals(arg, "--read-log", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(arg, "-ReadLog", StringComparison.OrdinalIgnoreCase)) &&
                index + 1 < args.Length)
            {
                readLogPath = Path.GetFullPath(args[++index]);
                continue;
            }

            if (string.Equals(arg, "--audit-list", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "-AuditList", StringComparison.OrdinalIgnoreCase))
            {
                auditList = true;
                continue;
            }

            if (string.Equals(arg, "--session", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "-Session", StringComparison.OrdinalIgnoreCase))
            {
                session = true;
                continue;
            }

            if (string.Equals(arg, "--lease", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "-Lease", StringComparison.OrdinalIgnoreCase))
            {
                lease = true;
                continue;
            }

            if (string.Equals(arg, "--violations", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "-Violations", StringComparison.OrdinalIgnoreCase))
            {
                violations = true;
                continue;
            }

            if ((string.Equals(arg, "--export-markdown", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(arg, "-ExportMarkdown", StringComparison.OrdinalIgnoreCase)) &&
                index + 1 < args.Length)
            {
                exportMarkdownPath = Path.GetFullPath(args[++index]);
            }
        }

        return new RkwpDiagnosticsOptions(root, smokeTest, readLogPath, showHelp, auditList, session, lease, violations, exportMarkdownPath);
    }
}
