# Levanta TODO en un solo contenedor (desde la raiz del repo)
$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $RepoRoot

$env:DOCKER_BUILDKIT = "0"
$env:COMPOSE_DOCKER_CLI_BUILD = "0"

Write-Host "=== Toka - Contenedor unico ===" -ForegroundColor Cyan
Write-Host "Incluye: SQL Server, MongoDB, Redis, RabbitMQ, Ollama (LLM local), microservicios, Gateway y Frontend" -ForegroundColor Gray
Write-Host "Build puede tardar 10-20 min la primera vez (descarga modelos Ollama)..." -ForegroundColor Yellow

docker compose -f docker-compose.single.yml up --build -d

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=== Sistema iniciado ===" -ForegroundColor Green
    Write-Host "  App:  http://localhost:3000"
    Write-Host ""
    Write-Host "Espera 2-3 minutos a que SQL Server y servicios inicien." -ForegroundColor Yellow
    Write-Host "Verificar: curl http://localhost:3000/health" -ForegroundColor Cyan
    Write-Host "Ver logs: docker compose -f docker-compose.single.yml logs -f" -ForegroundColor Gray
    Write-Host "Detener:  docker compose -f docker-compose.single.yml down" -ForegroundColor Gray
}
