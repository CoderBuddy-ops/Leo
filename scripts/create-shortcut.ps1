$wsh = New-Object -ComObject WScript.Shell
$desktop = [System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::Desktop)
$shortcutPath = Join-Path $desktop "Leo.lnk"
$shortcut = $wsh.CreateShortcut($shortcutPath)
$shortcut.TargetPath = "C:\Users\Yuvraj\Desktop\Leo-Release\Leo.exe"
$shortcut.WorkingDirectory = "C:\Users\Yuvraj\Desktop\Leo-Release"
$shortcut.IconLocation = "c:\Users\Yuvraj\Desktop\Project\nixi\app\src-tauri\icons\icon.ico"
$shortcut.Description = "Leo - Personal AI Companion"
$shortcut.Save()
Write-Host "Created shortcut at:" $shortcutPath
