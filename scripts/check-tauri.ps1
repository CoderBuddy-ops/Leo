$mingwPath = "C:\Users\Yuvraj\AppData\Local\Microsoft\WinGet\Packages\BrechtSanders.WinLibs.POSIX.MSVCRT_Microsoft.Winget.Source_8wekyb3d8bbwe\mingw64\bin"
$env:PATH = "$mingwPath;$env:PATH"
Write-Host "Using MinGW dlltool:"
Get-Command dlltool | Select-Object -ExpandProperty Source
Set-Location "app\src-tauri"
cargo check --target x86_64-pc-windows-gnu
