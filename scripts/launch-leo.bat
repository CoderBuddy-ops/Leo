@echo off
cd /d "c:\Users\Yuvraj\Desktop\Project\nixi\app"
start "" /b npx vite preview --port 1420
ping -n 2 127.0.0.1 >nul
start "" "C:\Program Files\Google\Chrome\Application\chrome.exe" --app="http://localhost:1420" --window-size=350,100 --window-position=500,12
