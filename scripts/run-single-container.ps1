# Levanta el stack completo (mismo comando que el README)
$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $RepoRoot

$env:DOCKER_BUILDKIT = "0"
$env:COMPOSE_DOCKER_CLI_BUILD = "0"

Write-Host "=== Toka User Management ===" -ForegroundColor Cyan
Write-Host "Primera vez: 15-25 min (compila microservicios). No cierres la terminal." -ForegroundColor Yellow

$prevEap = $ErrorActionPreference
$ErrorActionPreference = "Continue"
& docker compose up -d --build
$exitCode = $LASTEXITCODE
$ErrorActionPreference = $prevEap

if ($exitCode -ne 0) {
    Write-Host "ERROR: revisa que Docker Desktop este en ejecucion (docker info)" -ForegroundColor Red
    exit $exitCode
}

Write-Host ""
Write-Host "=== Contenedores iniciados ===" -ForegroundColor Green
Write-Host "  App:  http://localhost:3000"
Write-Host ""
Write-Host "Espera 2-3 min (SQL Server). Ver logs: docker compose logs -f" -ForegroundColor Yellow
