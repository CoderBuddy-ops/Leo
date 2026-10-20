$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$releaseDir = Join-Path $repoRoot "release"
$publishDir = Join-Path $repoRoot "src\Leo.App\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish"

Write-Host "==> Publishing standalone self-contained release..." -ForegroundColor Cyan
dotnet publish (Join-Path $repoRoot "src\Leo.App\Leo.App.csproj") -c Release -r win-x64 --self-contained true

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed with exit code $LASTEXITCODE"
}

if (-not (Test-Path $releaseDir)) {
    New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null
}

$zipPath = Join-Path $releaseDir "Leo-Windows-x64-v1.0.0.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Write-Host "==> Archiving release bundle to $zipPath..." -ForegroundColor Cyan
Compress-Archive -Path "$publishDir\*" -DestinationPath $zipPath -Force

$hash = (Get-FileHash -Path $zipPath -Algorithm SHA256).Hash
$hashFile = Join-Path $releaseDir "SHA256SUMS.txt"
"$hash  Leo-Windows-x64-v1.0.0.zip" | Out-File -FilePath $hashFile -Encoding utf8

Write-Host "==> Standalone Release Created Successfully!" -ForegroundColor Green
Write-Host "    Archive: $zipPath" -ForegroundColor White
Write-Host "    SHA256:  $hash" -ForegroundColor White
