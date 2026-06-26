# Toka - Arranque a prueba de errores (Windows)
# Uso: .\start.ps1
$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $RepoRoot

$env:DOCKER_BUILDKIT = "0"
$env:COMPOSE_DOCKER_CLI_BUILD = "0"

Write-Host "=== Toka User Management ===" -ForegroundColor Cyan
Write-Host "Verificando Docker..." -ForegroundColor Gray

$prevEap = $ErrorActionPreference
$ErrorActionPreference = "Continue"

& docker info *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker Desktop no esta corriendo. Abrelo y espera a 'Running'." -ForegroundColor Red
    exit 1
}

Write-Host "Construyendo e iniciando (primera vez: 15-25 min, NO cierres la ventana)..." -ForegroundColor Yellow
& docker compose up -d --build
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR en el build. Revisa el espacio en disco (necesitas ~15 GB)." -ForegroundColor Red
    exit 1
}

# Segundo 'up' para arrancar cualquier contenedor que quedo en estado 'Created'
Write-Host "Asegurando que todos los servicios arranquen..." -ForegroundColor Gray
& docker compose up -d | Out-Null

$ErrorActionPreference = $prevEap

Write-Host ""
Write-Host "=== Estado de los contenedores ===" -ForegroundColor Green
& docker compose ps

Write-Host ""
Write-Host "Listo. Espera 2-3 min a que SQL Server cree las bases y abre:" -ForegroundColor Cyan
Write-Host "  http://localhost:3000" -ForegroundColor White
Write-Host ""
Write-Host "Si la pagina no carga al primer intento, espera un poco mas y recarga." -ForegroundColor Gray
Write-Host "Ver logs:  docker compose logs -f" -ForegroundColor Gray
Write-Host "Detener:   docker compose down" -ForegroundColor Gray
