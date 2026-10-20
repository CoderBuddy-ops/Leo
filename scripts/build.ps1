param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "==> Building Leo.sln ($Configuration)..." -ForegroundColor Cyan
dotnet build (Join-Path $repoRoot "Leo.sln") -c $Configuration

if ($LASTEXITCODE -eq 0) {
    Write-Host "==> Build succeeded!" -ForegroundColor Green
} else {
    Write-Error "Build failed with exit code $LASTEXITCODE"
}
