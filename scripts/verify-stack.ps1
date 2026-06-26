# Verifica que el stack single-container responde correctamente
$ErrorActionPreference = "Stop"
$BaseUrl = "http://localhost:3000"

Write-Host "=== Verificacion Toka ===" -ForegroundColor Cyan

Write-Host "`n[1/4] Contenedores Docker..." -ForegroundColor Yellow
$ps = docker compose -f docker-compose.single.yml ps --format json 2>$null | ConvertFrom-Json
if (-not $ps) {
    Write-Host "  ERROR: No hay contenedores levantados. Ejecuta .\scripts\run-single-container.ps1" -ForegroundColor Red
    exit 1
}
$ps | ForEach-Object { Write-Host "  $($_.Service): $($_.State)" }

Write-Host "`n[2/4] Health check (hasta 3 min)..." -ForegroundColor Yellow
$ok = $false
for ($i = 0; $i -lt 36; $i++) {
    try {
        $r = Invoke-WebRequest -Uri "$BaseUrl/health" -UseBasicParsing -TimeoutSec 5
        if ($r.StatusCode -eq 200) { $ok = $true; break }
    } catch { Start-Sleep -Seconds 5 }
}
if (-not $ok) {
    Write-Host "  ERROR: /health no responde. Revisa logs: docker compose -f docker-compose.single.yml logs -f toka" -ForegroundColor Red
    exit 1
}
Write-Host "  OK - /health responde 200"

Write-Host "`n[3/4] API Gateway (roles protegido)..." -ForegroundColor Yellow
try {
    Invoke-WebRequest -Uri "$BaseUrl/api/roles" -UseBasicParsing -TimeoutSec 10 | Out-Null
    Write-Host "  OK - /api/roles responde"
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 401) {
        Write-Host "  OK - /api/roles responde 401 (requiere login, gateway operativo)"
    } else {
        Write-Host "  ADVERTENCIA: /api/roles no disponible: $_" -ForegroundColor Yellow
    }
}

Write-Host "`n[4/4] Agente IA (pregunta sobre roles)..." -ForegroundColor Yellow
$body = @{ question = "Que roles existen en el sistema?" } | ConvertTo-Json
try {
    $agent = Invoke-RestMethod -Uri "$BaseUrl/api/agent/query" -Method POST -Body $body -ContentType "application/json; charset=utf-8" -TimeoutSec 120
    Write-Host "  Proveedor: $($agent.llmProvider)"
    Write-Host "  Latencia:  $($agent.metrics.latencyMs) ms"
    if ($agent.answer -match "Admin|User|rol") {
        Write-Host "  OK - Respuesta coherente sobre roles" -ForegroundColor Green
    } else {
        Write-Host "  ADVERTENCIA: Respuesta inesperada (revisa Ollama): $($agent.answer.Substring(0, [Math]::Min(120, $agent.answer.Length)))..." -ForegroundColor Yellow
    }
} catch {
    Write-Host "  ERROR: Agente IA no respondio: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== Stack operativo ===" -ForegroundColor Green
Write-Host "Abre $BaseUrl y registrate para usar la UI completa."
