# Modo multi-contenedor (desde la raiz del repo)
$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $RepoRoot

$env:DOCKER_BUILDKIT = "0"
$env:COMPOSE_DOCKER_CLI_BUILD = "0"

Write-Host "=== Toka - Docker Compose ===" -ForegroundColor Cyan
Write-Host "BuildKit desactivado para compatibilidad con rutas Windows." -ForegroundColor Gray

docker compose up --build -d

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=== Sistema en contenedores ===" -ForegroundColor Green
    Write-Host "  Frontend:  http://localhost:3000"
    Write-Host "  Gateway:   http://localhost:5000"
    Write-Host "  RabbitMQ:  http://localhost:15672 (guest/guest)"
    Write-Host ""
    Write-Host "Espera 2-3 min a que SQL Server termine de iniciar." -ForegroundColor Yellow
    Write-Host "Ver logs: docker compose logs -f" -ForegroundColor Gray
}
