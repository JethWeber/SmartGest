#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
DB="${SMARTGEST_DB:-$HOME/.local/share/SmartGest/smartgest.db}"
BACKUP_DIR="${SMARTGEST_BACKUP_DIR:-$(dirname "$DB")/Backups}"
mkdir -p "$BACKUP_DIR"
STAMP="$(date +%Y%m%d_%H%M%S)"
cp "$DB" "$BACKUP_DIR/smartgest_manual_$STAMP.db"
echo "Backup criado: $BACKUP_DIR/smartgest_manual_$STAMP.db"
