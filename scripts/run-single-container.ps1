# Levanta el stack (mismo comando que el README)
$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $RepoRoot

Write-Host "=== Toka User Management ===" -ForegroundColor Cyan
Write-Host "Levantando contenedores (la 1.a vez puede tardar 10-15 min)..." -ForegroundColor Yellow

$prevEap = $ErrorActionPreference
$ErrorActionPreference = "Continue"
& docker compose -f docker-compose.single.yml up -d
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
Write-Host "Espera 2-3 min y abre el navegador. Ver logs:" -ForegroundColor Yellow
Write-Host "  docker compose -f docker-compose.single.yml logs -f" -ForegroundColor Gray
