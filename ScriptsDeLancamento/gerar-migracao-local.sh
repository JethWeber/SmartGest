#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

if ! command -v dotnet-ef >/dev/null 2>&1 && ! dotnet ef --version >/dev/null 2>&1; then
  echo "Instale o EF CLI: dotnet tool install --global dotnet-ef"
  exit 1
fi

NAME="${1:-SchemaUpdate}"

dotnet ef migrations add "$NAME" \
  --project SmartGest.Migrations/SmartGest.Migrations.csproj \
  --startup-project SmartGest.Migrations/SmartGest.Migrations.csproj

echo
echo "Migração criada. Revise os ficheiros em SmartGest.Migrations/Migrations/ antes de publicar."
