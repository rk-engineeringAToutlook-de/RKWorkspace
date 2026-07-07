#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PACKAGE="$ROOT/release/ma017/packages/macos-frame-guest"

cd "$PACKAGE"
swift run MacDesktopPortalOwner "$@"
