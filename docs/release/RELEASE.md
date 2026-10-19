# Leo Windows Release & Packaging Guide

**Version:** 0.1.0  
**Target:** Windows 10/11 x64  
**Packaging Format:** NSIS (Nullsoft Scriptable Install System) + Portable Binary  
**Deliverables Directory:** `C:\Users\Yuvraj\Desktop\Leo-Release\`  

---

## 1. Verified Release Artifacts

| Deliverable File | Size | Format | Description |
| :--- | :--- | :--- | :--- |
| **`Leo-Setup-0.1.0-x64.exe`** | 4.06 MB | NSIS Setup Wizard | Official Windows installer. Installs per-user to `%LOCALAPPDATA%\Programs\Leo`. Creates Start Menu and optional Desktop shortcut. Registers Windows Uninstaller. |
| **`Leo-Setup-x64.exe`** | 4.06 MB | NSIS Setup Wizard | Convenient direct-named copy of the installer. |
| **`Leo.exe`** | 21.8 MB | Portable Binary | Standalone executable with static web assets embedded directly. Enforces `#![windows_subsystem = "windows"]` (zero console window). |
| **`WebView2Loader.dll`** | 157 KB | Dynamic Library | Microsoft Edge WebView2 loader required alongside `Leo.exe` for portable execution. |
| **`RELEASE-MANIFEST.txt`** | 2 KB | Text Document | Cryptographic integrity and deployment manifest. |

---

## 2. Packaging Architecture

### 2.1 Multi-Page Static Bundling (Zero-Localhost)
- Production assets are generated via `app/vite.config.ts`, bundling both `index.html` (capsule dock) and `settings.html` (standalone settings) into `app/dist/`.
- Tauri's `tauri.conf.json` embeds static assets via `frontendDist: "../dist"`.
- The packaged executable never spins up a local HTTP server, Chrome instance, or binds localhost ports, allowing full offline operation.

### 2.2 GUI Subsystem (Zero Console Window)
- The main Rust entry point (`app/src-tauri/src/main.rs`) declares:
  ```rust
  #![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]
  ```
- This instructs Windows to launch Leo as a native GUI application without allocating or attaching a console/terminal window.

### 2.3 System Tray & Windows Lifecycle
- When the user closes the floating capsule overlay, `WindowEvent::CloseRequested` is intercepted with `api.prevent_close()`, hiding the window while keeping the notification-area icon responsive.
- The tray context menu offers:
  - `Open Leo` (shows and focuses floating dock)
  - `Show/Hide Dock` (toggles capsule visibility)
  - `Settings` (opens dedicated standalone settings window)
  - `Start with Windows` (configurable background startup)
  - `Quit` (clean application shutdown with `app.exit(0)`)

---

## 3. Installation & Verification Instructions

### 3.1 Clean Machine Installation Test
1. Run `C:\Users\Yuvraj\Desktop\Leo-Release\Leo-Setup-0.1.0-x64.exe`.
2. Follow the setup wizard prompts.
3. Confirm installation directory: `%LOCALAPPDATA%\Programs\Leo`.
4. Verify Start Menu shortcut and Desktop shortcut are created with the canonical Leo Icon V2.
5. Launch Leo: confirm the compact copper-charcoal capsule appears at the top center of the screen without a terminal window.
6. Verify the black-hole tray icon appears in the Windows notification area.

### 3.2 Uninstallation Test
1. Open Windows **Settings** $\to$ **Installed apps** (or `appwiz.cpl`).
2. Locate **Leo**.
3. Select **Uninstall**.
4. Confirm all binaries and shortcuts are cleanly removed.
