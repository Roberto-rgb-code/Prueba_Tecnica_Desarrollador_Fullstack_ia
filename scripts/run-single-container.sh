#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

export DOCKER_BUILDKIT=0
export COMPOSE_DOCKER_CLI_BUILD=0

echo "=== Toka User Management ==="
echo "Primera vez: 15-25 min (compila microservicios). No cierres la terminal."

docker compose up -d --build

echo ""
echo "=== Contenedores iniciados ==="
echo "  App:  http://localhost:3000"
echo "Espera 2-3 min. Ver logs: docker compose logs -f"
