param(
    [switch]$Release
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

if ($Release) {
    $exePath = Join-Path $repoRoot "src\Leo.App\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\Leo.App.exe"
} else {
    $exePath = Join-Path $repoRoot "src\Leo.App\bin\x64\Debug\net8.0-windows10.0.19041.0\Leo.App.exe"
}

if (-not (Test-Path $exePath)) {
    Write-Host "Executable not found. Building first..." -ForegroundColor Yellow
    & (Join-Path $PSScriptRoot "build.ps1")
}

Write-Host "==> Launching Leo: $exePath" -ForegroundColor Cyan
Start-Process -FilePath $exePath
