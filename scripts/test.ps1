$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "==> Running automated test suite..." -ForegroundColor Cyan
dotnet test (Join-Path $repoRoot "tests\Leo.Core.Tests\Leo.Core.Tests.csproj") --verbosity normal

if ($LASTEXITCODE -eq 0) {
    Write-Host "==> All automated tests passed successfully!" -ForegroundColor Green
} else {
    Write-Error "Tests failed with exit code $LASTEXITCODE"
}
