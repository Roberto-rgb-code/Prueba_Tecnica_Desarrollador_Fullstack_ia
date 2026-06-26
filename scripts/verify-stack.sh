#!/usr/bin/env bash
# Verifica que el stack single-container responde correctamente
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

BASE_URL="${BASE_URL:-http://localhost:3000}"

echo "=== Verificacion Toka ==="

echo ""
echo "[1/4] Contenedores Docker..."
if ! docker compose -f docker-compose.single.yml ps --status running 2>/dev/null | grep -q toka; then
  echo "  ERROR: No hay contenedores levantados. Ejecuta ./scripts/run-single-container.sh"
  exit 1
fi
docker compose -f docker-compose.single.yml ps

echo ""
echo "[2/4] Health check (hasta 3 min)..."
ok=0
for _ in $(seq 1 36); do
  if curl -sf "$BASE_URL/health" >/dev/null 2>&1; then ok=1; break; fi
  sleep 5
done
if [ "$ok" -ne 1 ]; then
  echo "  ERROR: /health no responde. Revisa: docker compose -f docker-compose.single.yml logs -f toka"
  exit 1
fi
echo "  OK - /health responde 200"

echo ""
echo "[3/4] API Gateway (roles protegido)..."
code=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/roles" || true)
if [ "$code" = "200" ]; then
  echo "  OK - /api/roles responde 200"
elif [ "$code" = "401" ]; then
  echo "  OK - /api/roles responde 401 (requiere login, gateway operativo)"
else
  echo "  ADVERTENCIA: /api/roles respondio HTTP $code"
fi

echo ""
echo "[4/4] Agente IA (pregunta sobre roles)..."
response=$(curl -sf -X POST "$BASE_URL/api/agent/query" \
  -H "Content-Type: application/json" \
  -d '{"question":"Que roles existen en el sistema?"}' \
  --max-time 120) || { echo "  ERROR: Agente IA no respondio"; exit 1; }

if echo "$response" | grep -qiE 'Admin|User|rol'; then
  echo "  OK - Respuesta coherente sobre roles"
else
  echo "  ADVERTENCIA: Respuesta inesperada. Revisa contenedor ollama."
fi

echo ""
echo "=== Stack operativo ==="
echo "Abre $BASE_URL y registrate para usar la UI completa."
