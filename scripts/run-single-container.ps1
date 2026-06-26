# Levanta TODO en un solo contenedor
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $Root

$env:DOCKER_BUILDKIT = "0"
$env:COMPOSE_DOCKER_CLI_BUILD = "0"

Write-Host "=== Toka - Contenedor unico ===" -ForegroundColor Cyan
Write-Host "Incluye: SQL Server, MongoDB, Redis, RabbitMQ, 6 microservicios, Gateway y Frontend" -ForegroundColor Gray
Write-Host "Build puede tardar 10-15 min la primera vez..." -ForegroundColor Yellow

docker compose -f docker-compose.single.yml up --build -d

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=== Sistema iniciado ===" -ForegroundColor Green
    Write-Host "  App:  http://localhost:3000"
    Write-Host ""
    Write-Host "Espera 2-3 minutos a que SQL Server y servicios inicien." -ForegroundColor Yellow
    Write-Host "Ver logs: docker compose -f docker-compose.single.yml logs -f" -ForegroundColor Gray
    Write-Host "Detener:  docker compose -f docker-compose.single.yml down" -ForegroundColor Gray
}
