#!/usr/bin/env bash
set -euo pipefail
DB="${SMARTGEST_DB:-$HOME/.local/share/SmartGest/smartgest.db}"
if [[ ! -f "$DB" ]]; then
  echo "BD não encontrada: $DB"
  exit 1
fi
sqlite3 "$DB" "PRAGMA integrity_check;"
sqlite3 "$DB" "PRAGMA foreign_key_check;"
