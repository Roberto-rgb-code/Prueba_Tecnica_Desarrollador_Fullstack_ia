#!/usr/bin/env bash
# Levanta el stack (imagen preconstruida desde GHCR; sin compilar localmente)
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

export DOCKER_BUILDKIT=0
export COMPOSE_DOCKER_CLI_BUILD=0

echo "=== Toka User Management ==="
echo "Descargando imagen preconstruida (no compila en tu PC)..."

if ! docker compose -f docker-compose.single.yml pull toka ollama; then
  if [ "${TOKA_BUILD:-0}" = "1" ]; then
    echo "Compilando localmente (30+ min, requiere ~20 GB disco)..."
    docker compose -f docker-compose.single.yml build toka
  else
    echo "ERROR: no se pudo descargar la imagen. Reintenta o usa: TOKA_BUILD=1 ./scripts/run-single-container.sh"
    exit 1
  fi
fi

docker compose -f docker-compose.single.yml up -d

echo ""
echo "=== Sistema iniciado ==="
echo "  App:  http://localhost:3000"
echo ""
echo "Espera 2-3 minutos (SQL Server arranca dentro del contenedor)."
echo "Ver logs:  docker compose -f docker-compose.single.yml logs -f"
echo "Detener:   docker compose -f docker-compose.single.yml down"
