# macOS Build Commands

Status: MA011.08 handoff  
Datum: 2026-07-06

## Native Swift/Xcode

Beispielbefehle fuer macOS-Codex:

```bash
cd apps/macos/RKWorkspaceMacGuest
xcodebuild -scheme RKWorkspaceMacGuest -configuration Debug build
xcodebuild -scheme RKWorkspaceMacGuest -configuration Debug test
```

Falls ein Workspace genutzt wird:

```bash
xcodebuild -workspace RKWorkspaceMacGuest.xcworkspace -scheme RKWorkspaceMacGuest -configuration Debug build
```

## Start mit Testargumenten

```bash
open build/Debug/RKWorkspaceMacGuest.app --args \
  --owner-url rkwp-devlan://WINDOWS_HOST:57100 \
  --ablage-name "Ablage macOS" \
  --lab-mode
```

## Option B: .NET/Avalonia

Nur falls gewaehlt:

```bash
dotnet build apps/macos/RKWorkspaceMacGuest/RKWorkspaceMacGuest.csproj
dotnet test apps/macos/RKWorkspaceMacGuest.Tests/RKWorkspaceMacGuest.Tests.csproj
```

## Windows-Hinweis

Windows-Codex kann diese Befehle nicht ausfuehren. Auf Windows wird nur geprueft, dass die Handoff-Dateien im Context Pack enthalten sind und der bestehende Windows-Build gruen bleibt.
