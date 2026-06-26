$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host "Running unit tests with coverage..." -ForegroundColor Cyan
if (Test-Path "TestResults") { Remove-Item -Recurse -Force "TestResults" }

dotnet test TokaUserManagement.sln `
    --settings coverlet.runsettings `
    --collect:"XPlat Code Coverage" `
    --results-directory TestResults

$coverageFiles = Get-ChildItem -Path TestResults -Recurse -Filter "coverage.cobertura.xml"
if (-not $coverageFiles) {
    Write-Error "No coverage report generated."
}

$totalLines = 0
$coveredLines = 0
foreach ($file in $coverageFiles) {
    [xml]$xml = Get-Content $file.FullName
    $totalLines += [int]($xml.coverage.'lines-valid')
    $coveredLines += [int]($xml.coverage.'lines-covered')
}

$rate = if ($totalLines -gt 0) { [math]::Round(($coveredLines / $totalLines) * 100, 2) } else { 0 }
Write-Host ""
Write-Host "Coverage (Application + Agent services): $rate% ($coveredLines / $totalLines lines)" -ForegroundColor Green
Write-Host "Reports: $($coverageFiles[0].DirectoryName)" -ForegroundColor Gray

if ($rate -lt 70) {
    Write-Warning "Coverage is below 70% target."
    exit 1
}
