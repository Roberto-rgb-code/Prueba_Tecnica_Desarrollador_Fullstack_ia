# Ejecutar Toka sin Docker Desktop
# Requisitos: .NET 8 SDK, Node.js 20+, SQL Server LocalDB

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $Root

Write-Host "=== Toka - Modo local (sin Docker) ===" -ForegroundColor Cyan

# Verificar LocalDB
Write-Host "Verificando LocalDB..." -ForegroundColor Yellow
sqllocaldb info MSSQLLocalDB 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "LocalDB no encontrado. Instala SQL Server Express LocalDB o Visual Studio." -ForegroundColor Red
    exit 1
}
sqllocaldb start MSSQLLocalDB 2>$null

Write-Host "Compilando solucion..." -ForegroundColor Yellow
dotnet build TokaUserManagement.sln -c Debug --nologo -v q
if ($LASTEXITCODE -ne 0) { exit 1 }

$env:ASPNETCORE_ENVIRONMENT = "Development"

function Start-ServiceProject {
    param([string]$Name, [string]$ProjectPath, [string]$Url)
    Write-Host "Iniciando $Name en $Url ..." -ForegroundColor Green
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$Root'; `$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run --project '$ProjectPath' --urls '$Url'" -WindowStyle Normal
    Start-Sleep -Seconds 3
}

Start-ServiceProject "AuthService"     "services\AuthService\AuthService.Api\AuthService.Api.csproj"     "http://localhost:5172"
Start-ServiceProject "UserService"     "services\UserService\UserService.Api\UserService.Api.csproj"     "http://localhost:5085"
Start-ServiceProject "RoleService"     "services\RoleService\RoleService.Api\RoleService.Api.csproj"     "http://localhost:5169"
Start-ServiceProject "AuditService"    "services\AuditService\AuditService.Api\AuditService.Api.csproj"    "http://localhost:5195"
Start-ServiceProject "AiAgentService"  "services\AiAgentService\AiAgentService.Api\AiAgentService.Api.csproj" "http://localhost:5180"
Start-Sleep -Seconds 2
Start-ServiceProject "Gateway"         "gateway\Gateway.Api\Gateway.Api.csproj"                          "http://localhost:5000"

Write-Host "Iniciando frontend..." -ForegroundColor Green
if (-not (Test-Path "frontend\node_modules")) {
    Set-Location frontend
    npm install
    Set-Location $Root
}
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$Root\frontend'; npm run dev" -WindowStyle Normal

Write-Host ""
Write-Host "=== Sistema iniciado ===" -ForegroundColor Cyan
Write-Host "  Frontend:  http://localhost:3000"
Write-Host "  Gateway:   http://localhost:5000"
Write-Host "  Swagger Auth: http://localhost:5172/swagger"
Write-Host ""
Write-Host "Registrate en el frontend y prueba el sistema." -ForegroundColor Yellow
Write-Host "Para detener: cierra las ventanas de PowerShell abiertas." -ForegroundColor Gray
