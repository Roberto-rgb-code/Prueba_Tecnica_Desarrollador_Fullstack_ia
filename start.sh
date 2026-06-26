#!/usr/bin/env bash
# Toka - Arranque a prueba de errores (Linux / macOS)
# Uso: ./start.sh
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")"

export DOCKER_BUILDKIT=0
export COMPOSE_DOCKER_CLI_BUILD=0

echo "=== Toka User Management ==="

if ! docker info >/dev/null 2>&1; then
  echo "ERROR: Docker no esta corriendo. Inicia Docker y reintenta."
  exit 1
fi

echo "Construyendo e iniciando (primera vez: 15-25 min, NO cierres la terminal)..."
docker compose up -d --build

# Segundo 'up' para arrancar cualquier contenedor que quedo en estado 'Created'
echo "Asegurando que todos los servicios arranquen..."
docker compose up -d >/dev/null

echo ""
echo "=== Estado de los contenedores ==="
docker compose ps

echo ""
echo "Listo. Espera 2-3 min y abre: http://localhost:3000"
echo "Ver logs:  docker compose logs -f"
echo "Detener:   docker compose down"
