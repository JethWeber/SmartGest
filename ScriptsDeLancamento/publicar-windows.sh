#!/usr/bin/env bash
set -euo pipefail

ROOT=$(cd "$(dirname "$0")/.." && pwd)
PUBLISH_DIR="$ROOT/artifacts/publish/win-x64"

rm -rf "$PUBLISH_DIR"
mkdir -p "$PUBLISH_DIR"
cd "$ROOT"

dotnet restore SmartGest.slnx
dotnet publish SmartGest.Desktop/SmartGest.Desktop.csproj --configuration Release --runtime win-x64 --self-contained true --property:PublishTrimmed=false --property:DebugType=None --output "$PUBLISH_DIR"

echo
echo "Publicação Windows criada em:"
echo "  $PUBLISH_DIR"
echo "No Windows, execute: iscc Installer/SmartGest.iss"
