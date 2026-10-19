$mingwPath = "C:\Users\Yuvraj\AppData\Local\Microsoft\WinGet\Packages\BrechtSanders.WinLibs.POSIX.MSVCRT_Microsoft.Winget.Source_8wekyb3d8bbwe\mingw64\bin"
$env:PATH = "$mingwPath;$env:PATH"
Set-Location "app"
Write-Host "1. Building Vite multi-page frontend bundle..."
npm run build
if ($LASTEXITCODE -ne 0) {
    Write-Error "Frontend build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "2. Building native Windows NSIS installer via Tauri CLI..."
npx @tauri-apps/cli build --target x86_64-pc-windows-gnu --bundles nsis
if ($LASTEXITCODE -ne 0) {
    Write-Error "Tauri build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "3. Build complete."
