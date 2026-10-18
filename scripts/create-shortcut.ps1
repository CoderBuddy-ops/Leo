$wsh = New-Object -ComObject WScript.Shell
$desktop = [System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::Desktop)
$shortcutPath = Join-Path $desktop "Leo.lnk"
$shortcut = $wsh.CreateShortcut($shortcutPath)
$shortcut.TargetPath = "c:\Users\Yuvraj\Desktop\Project\nixi\scripts\launch-leo.vbs"
$shortcut.WorkingDirectory = "c:\Users\Yuvraj\Desktop\Project\nixi\app"
$shortcut.IconLocation = "c:\Users\Yuvraj\Desktop\Project\nixi\app\src-tauri\icons\icon.ico"
$shortcut.Description = "Leo - Personal AI Companion"
$shortcut.Save()
Write-Host "Created shortcut at:" $shortcutPath
