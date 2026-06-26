#!/usr/bin/env bash
# Levanta TODO en un solo contenedor (Linux / macOS)
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

export DOCKER_BUILDKIT=0
export COMPOSE_DOCKER_CLI_BUILD=0

echo "=== Toka - Contenedor unico ==="
echo "Incluye: SQL Server, MongoDB, Redis, RabbitMQ, Ollama (LLM local), microservicios, Gateway y Frontend"
echo "Build puede tardar 10-20 min la primera vez (descarga modelos Ollama)..."

docker compose -f docker-compose.single.yml up --build -d

echo ""
echo "=== Sistema iniciado ==="
echo "  App:  http://localhost:3000"
echo ""
echo "Espera 2-3 minutos a que SQL Server y servicios inicien."
echo "Verificar: ./scripts/verify-stack.sh"
echo "Ver logs:  docker compose -f docker-compose.single.yml logs -f"
echo "Detener:   docker compose -f docker-compose.single.yml down"
