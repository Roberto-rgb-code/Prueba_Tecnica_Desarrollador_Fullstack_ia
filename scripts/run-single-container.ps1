# Levanta el stack (imagen preconstruida desde GHCR; sin compilar localmente)
$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $RepoRoot

$env:DOCKER_BUILDKIT = "0"
$env:COMPOSE_DOCKER_CLI_BUILD = "0"

Write-Host "=== Toka User Management ===" -ForegroundColor Cyan
Write-Host "Descargando imagen preconstruida (no compila en tu PC)..." -ForegroundColor Gray

$prevEap = $ErrorActionPreference
$ErrorActionPreference = "Continue"

& docker compose -f docker-compose.single.yml pull toka ollama 2>&1 | Write-Host
$pullExit = $LASTEXITCODE

if ($pullExit -ne 0) {
    Write-Host ""
    Write-Host "No se pudo descargar la imagen preconstruida." -ForegroundColor Yellow
    Write-Host "  - Espera a que termine GitHub Actions:" -ForegroundColor Cyan
    Write-Host "    https://github.com/Roberto-rgb-code/Prueba_Tecnica_Desarrollador_Fullstack_ia/actions"
    Write-Host "  - O compila local (30-45 min, ~20 GB disco):" -ForegroundColor Cyan
    Write-Host '    $env:TOKA_BUILD="1"; .\scripts\run-single-container.ps1' -ForegroundColor Gray
    if ($env:TOKA_BUILD -ne "1") { exit 1 }
    Write-Host ""
    Write-Host "TOKA_BUILD=1 — compilando localmente. NO cierres esta ventana..." -ForegroundColor Yellow
    & docker compose -f docker-compose.single.yml build toka
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

& docker compose -f docker-compose.single.yml up -d
$exitCode = $LASTEXITCODE
$ErrorActionPreference = $prevEap

if ($exitCode -ne 0) {
    Write-Host "ERROR: docker compose up fallo (codigo $exitCode)" -ForegroundColor Red
    exit $exitCode
}

Write-Host ""
Write-Host "=== Sistema iniciado ===" -ForegroundColor Green
Write-Host "  App:  http://localhost:3000"
Write-Host ""
Write-Host "Espera 2-3 minutos (SQL Server arranca dentro del contenedor)." -ForegroundColor Yellow
Write-Host "Ver logs:  docker compose -f docker-compose.single.yml logs -f" -ForegroundColor Gray
Write-Host "Detener:   docker compose -f docker-compose.single.yml down" -ForegroundColor Gray
