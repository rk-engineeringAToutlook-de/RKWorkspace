#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PACKAGE="$ROOT/release/ma017/packages/macos-frame-guest"
LABEL="de.rkworkspace.mac-desktop-portal-owner"
PLIST="$HOME/Library/LaunchAgents/$LABEL.plist"
FOREGROUND=0
ARGS=()

cd "$PACKAGE"

for arg in "$@"; do
  if [[ "$arg" == "--smoke-test" ]]; then
    swift run MacDesktopPortalOwner "$@"
    exit $?
  fi

  if [[ "$arg" == "--foreground" ]]; then
    FOREGROUND=1
    continue
  fi

  ARGS+=("$arg")
done

swift build --product MacDesktopPortalOwner >/dev/null
BIN="$(swift build --show-bin-path)/MacDesktopPortalOwner"

if [[ "$FOREGROUND" == "1" ]]; then
  exec "$BIN" "${ARGS[@]}"
fi

mkdir -p "$HOME/Library/LaunchAgents"
python3 - "$PLIST" "$BIN" "$PACKAGE" "${ARGS[@]}" <<'PY'
import plistlib
import sys

plist_path = sys.argv[1]
binary = sys.argv[2]
working_directory = sys.argv[3]
args = sys.argv[4:]
label = "de.rkworkspace.mac-desktop-portal-owner"

data = {
    "Label": label,
    "ProgramArguments": [binary, *args],
    "WorkingDirectory": working_directory,
    "RunAtLoad": True,
    "KeepAlive": True,
    "StandardOutPath": "/tmp/rkws-macos-desktop-portal-owner.launchd.out.log",
    "StandardErrorPath": "/tmp/rkws-macos-desktop-portal-owner.launchd.err.log",
}

with open(plist_path, "wb") as handle:
    plistlib.dump(data, handle)
PY

uid="$(id -u)"
launchctl bootout "gui/$uid" "$PLIST" 2>/dev/null || true
launchctl bootstrap "gui/$uid" "$PLIST"
launchctl kickstart -k "gui/$uid/$LABEL"
echo "MacDesktopPortalOwner: STARTED_BACKGROUND"
