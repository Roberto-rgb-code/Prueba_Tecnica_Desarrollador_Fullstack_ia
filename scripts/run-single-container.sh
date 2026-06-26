#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

echo "=== Toka User Management ==="
echo "Levantando contenedores (la 1.a vez puede tardar 10-15 min)..."

docker compose -f docker-compose.single.yml up -d

echo ""
echo "=== Contenedores iniciados ==="
echo "  App:  http://localhost:3000"
echo ""
echo "Espera 2-3 min y abre el navegador."
echo "Ver logs: docker compose -f docker-compose.single.yml logs -f"
