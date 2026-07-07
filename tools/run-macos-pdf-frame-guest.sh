#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PACKAGE="$ROOT/release/ma017/packages/macos-frame-guest"
LABEL="de.rkworkspace.mac-pdf-frame-guest"
APP_DIR="$HOME/Applications/RKWorkspace/MacPdfFrameGuest.app"
APP_BIN="$APP_DIR/Contents/MacOS/MacPdfFrameGuest"
PLIST="$HOME/Library/LaunchAgents/$LABEL.plist"
LOG_DIR="$HOME/Library/Logs/RKWorkspace"
FOREGROUND=0
ARGS=()

cd "$PACKAGE"

if [[ "$#" == "0" ]]; then
  ARGS=(--host 192.168.163.11 --port 57120 --wait-for-placement)
fi

for arg in "$@"; do
  if [[ "$arg" == "--smoke-test" ]]; then
    swift run MacPdfFrameGuest "$@"
    exit $?
  fi

  if [[ "$arg" == "--foreground" ]]; then
    FOREGROUND=1
    continue
  fi

  ARGS+=("$arg")
done

swift build --product MacPdfFrameGuest >/dev/null
BIN="$(swift build --show-bin-path)/MacPdfFrameGuest"

mkdir -p "$APP_DIR/Contents/MacOS" "$APP_DIR/Contents/Resources" "$LOG_DIR" "$HOME/Library/LaunchAgents"
cp "$BIN" "$APP_BIN"
chmod +x "$APP_BIN"

python3 - "$APP_DIR/Contents/Info.plist" <<'PY'
import plistlib
import sys

info = {
    "CFBundleIdentifier": "de.rkworkspace.MacPdfFrameGuest",
    "CFBundleName": "MacPdfFrameGuest",
    "CFBundleDisplayName": "RK Workspace Ablage",
    "CFBundleExecutable": "MacPdfFrameGuest",
    "CFBundlePackageType": "APPL",
    "CFBundleVersion": "1",
    "CFBundleShortVersionString": "0.1",
    "NSLocalNetworkUsageDescription": "RK Workspace verbindet sich lokal mit der Windows-Ablage und uebertraegt nur den PDF-Frame.",
}

with open(sys.argv[1], "wb") as handle:
    plistlib.dump(info, handle)
PY

if [[ "$FOREGROUND" == "1" ]]; then
  exec "$APP_BIN" "${ARGS[@]}"
fi

python3 - "$PLIST" "$APP_BIN" "$PACKAGE" "$LOG_DIR" "${ARGS[@]}" <<'PY'
import plistlib
import sys

plist_path = sys.argv[1]
binary = sys.argv[2]
working_directory = sys.argv[3]
log_dir = sys.argv[4]
args = sys.argv[5:]
label = "de.rkworkspace.mac-pdf-frame-guest"

data = {
    "Label": label,
    "ProgramArguments": [binary, *args],
    "WorkingDirectory": working_directory,
    "RunAtLoad": True,
    "KeepAlive": True,
    "LimitLoadToSessionType": "Aqua",
    "ProcessType": "Interactive",
    "StandardOutPath": f"{log_dir}/mac-pdf-frame-guest.log",
    "StandardErrorPath": f"{log_dir}/mac-pdf-frame-guest.err.log",
}

with open(plist_path, "wb") as handle:
    plistlib.dump(data, handle)
PY

uid="$(id -u)"
launchctl bootout "gui/$uid" "$PLIST" 2>/dev/null || true
launchctl bootstrap "gui/$uid" "$PLIST"
launchctl kickstart -k "gui/$uid/$LABEL"
echo "MacPdfFrameGuest: STARTED_BACKGROUND"
