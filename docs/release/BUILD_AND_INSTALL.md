# Leo Build, Installation & Release Engineering Guide

**Status:** Canonical & Verified  
**Target:** Windows 10 & 11 (x64)  
**Native Engine:** Tauri 2 (Rust + Win32 DirectComposition + WebView2)  
**Frontend:** TypeScript + HTML5 Canvas (33.5 KB bundle)  

---

## 1. Verified Release Artifacts

| Artifact | Location | Size | Type |
|---|---|---|---|
| **Official Installer** | `C:\Users\Yuvraj\Desktop\Leo-Setup-x64.exe` | **4.05 MB** | Standard Windows NSIS Setup Wizard |
| **Release Folder** | `C:\Users\Yuvraj\Desktop\Leo-Release\` | ~27 MB | Self-contained staging folder |
| **Release Executable** | `C:\Users\Yuvraj\Desktop\Leo-Release\Leo.exe` | 22.8 MB | Native Windows GUI application |
| **Desktop Shortcut** | `C:\Users\Yuvraj\Desktop\Leo.lnk` | 1 KB | Targets `Leo.exe` directly |

---

## 2. Windows Installation Flow

1. **Running the Installer:**
   - Double-click `Leo-Setup-x64.exe`.
   - The installer automatically installs Leo to the standard per-user application directory:
     `%LOCALAPPDATA%\Programs\Leo\`
   - Creates a Start Menu entry: `Start > Leo`.
   - Creates a Desktop shortcut with the refined **Leo Icon V2**.
   - Registers Windows uninstaller metadata in Windows Settings / Control Panel.

2. **Uninstallation:**
   - Go to **Windows Settings > Apps > Installed apps**, search for **Leo**, and click **Uninstall**.
   - Or run `%LOCALAPPDATA%\Programs\Leo\uninstall.exe`.

---

## 3. Critical Fixes & Release Invariants

### Zero Localhost Dependency (Fixing ERR_CONNECTION_REFUSED)
- **The Problem:** Previously, a legacy script (`launch-leo.bat`) spawned Google Chrome with `--app="http://localhost:1420"` attempting to preview a development server. If Vite was not running, Chrome displayed `ERR_CONNECTION_REFUSED`.
- **The Fix:** The native Tauri 2 build (`Leo.exe`) compiles and bundles the production frontend statically (`frontendDist: "../dist"`). At runtime, WebView2 loads bundled local assets directly from the executable. **No local web server, node process, or localhost port is ever required.**

### Zero Console Window (Desktop GUI Subsystem)
- **The Problem:** Launching via batch scripts spawned an unwanted Command Prompt or PowerShell terminal.
- **The Fix:**
  - `app/src-tauri/src/main.rs` configures `#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]`, ensuring Windows initializes the process as a native GUI subsystem application with no console.
  - Desktop and Start Menu shortcuts point directly to `Leo.exe`.

### True Window Transparency & Always-On-Top
- Configured in `app/src-tauri/tauri.conf.json` with:
  - `"transparent": true`
  - `"decorations": false`
  - `"alwaysOnTop": true`
  - `"skipTaskbar": true`
- Root webview styling sets `html, body { background: transparent !important; }`, preventing any flash of white background.

---

## 4. How to Reproduce the Production Build

Run the following verified commands independently:

```powershell
# 1. Regenerate canonical Leo Icon V2 assets (if changed)
node scripts/generate-leo-icon.mjs

# 2. Run all unit test suites (25/25 passing)
node --test tests/leo_p0.test.mjs
node --test tests/leo_extended.test.mjs
node --test tests/leo_icon.test.mjs
node --test tests/leo_providers.test.mjs

# 3. Build frontend production bundle
cd app
npm run build

# 4. Package official NSIS Windows Installer
npx @tauri-apps/cli build --target x86_64-pc-windows-gnu --bundles nsis
```

The output installer will be generated at:
`app/src-tauri/target/x86_64-pc-windows-gnu/release/bundle/nsis/Leo_0.1.0_x64-setup.exe` (4.05 MB).

---

## 5. Troubleshooting & Logs

- **Log Directory:** Application crash logs and WebView2 state are saved in `%LOCALAPPDATA%\com.leo.assistant\`.
- **WebView2 Runtime:** Leo utilizes the evergreen Microsoft Edge WebView2 runtime installed by default on Windows 10 and Windows 11.
