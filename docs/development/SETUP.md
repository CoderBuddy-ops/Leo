# Leo Development & Setup Guide

**Target Platform:** Windows 10/11 x64  
**Framework:** Tauri 2 (Rust Backend + TypeScript / Canvas Frontend)  
**Package Manager:** npm  

---

## 1. Prerequisites & Toolchain Setup

To develop, test, and build Leo on Windows, the following host toolchains are required:

### 1.1 Node.js
- **Version:** Node.js v20+ (v24.x tested and verified).
- **Verify:**
  ```powershell
  node --version
  npm --version
  ```

### 1.2 Rust Toolchain (`x86_64-pc-windows-gnu`)
- **Version:** Rust 1.85+ (1.95.0 tested).
- **GNU Target:** Leo compiles against the MinGW-w64 GNU target (`x86_64-pc-windows-gnu`) to ensure lightweight, zero-MSVC-redistributable desktop packaging.
- **Install Target:**
  ```powershell
  rustup target add x86_64-pc-windows-gnu
  ```

### 1.3 MinGW-w64 GCC Compiler & Binutils
- **Recommended Toolchain:** Brecht Sanders WinLibs MinGW-w64 (GCC 16.2.0, POSIX threads, MSVCRT).
- **Standard Location:** Automatically managed by Windows Package Manager (WinGet):
  ```
  C:\Users\<User>\AppData\Local\Microsoft\WinGet\Packages\BrechtSanders.WinLibs.POSIX.MSVCRT_Microsoft.Winget.Source_8wekyb3d8bbwe\mingw64\bin
  ```
- **Important Toolchain Note (dlltool collision):**
  If `rustc` reports `dlltool: CreateProcess failed`, it is because an unbundled `dlltool.exe` in `.cargo\bin` is taking precedence. Always run builds through [scripts/check-tauri.ps1](file:///c:/Users/Yuvraj/Desktop/Project/nixi/scripts/check-tauri.ps1) or [scripts/build-installer.ps1](file:///c:/Users/Yuvraj/Desktop/Project/nixi/scripts/build-installer.ps1), which prepend the complete MinGW64 toolchain directory to `PATH`.

### 1.4 NSIS (Nullsoft Scriptable Install System)
- **Version:** NSIS 3.0+ (3.13 tested).
- **Verify:**
  ```powershell
  makensis /VERSION
  ```

---

## 2. Project Setup

1. **Clone and enter the workspace:**
   ```powershell
   cd c:\Users\Yuvraj\Desktop\Project\nixi
   ```

2. **Install frontend dependencies:**
   ```powershell
   cd app
   npm install
   cd ..
   ```

---

## 3. Verified Verification & Testing Workflow

Run independent test suites to accurately verify each subsystem:

```powershell
# 1. P0 Core Dynamics & Cleanroom Logic (7/7 tests)
node --test tests/leo_p0.test.mjs

# 2. Extended Voice, VAD, IPC & Gesture State Tests (10/10 tests)
node --test tests/leo_extended.test.mjs

# 3. Canonical Icon V2 Integrity & Determinism (8/8 tests)
node --test tests/leo_icon.test.mjs

# 4. AI Provider Egress & Gemini SSE Tests (6/6 tests)
node --test tests/leo_providers.test.mjs
```

**Total Pass Rate:** 31 / 31 tests passing (100%).

---

## 4. Development Workflow

### Running Webview Frontend in Dev Mode
```powershell
cd app
npm run dev
```
Access the floating capsule UI at `http://localhost:1420` and the standalone settings page at `http://localhost:1420/settings.html`.

### Checking Native Rust Backend
To verify native Rust code without running a full release packaging cycle:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/check-tauri.ps1
```

---

## 5. Building the Production NSIS Installer

To build both the multi-page Vite bundle and the official Windows NSIS installer:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/build-installer.ps1
```

**Outputs:**
- Built application binary: `app/src-tauri/target/x86_64-pc-windows-gnu/release/leo.exe` (21.8 MB)
- Official NSIS Installer: `app/src-tauri/target/x86_64-pc-windows-gnu/release/bundle/nsis/Leo_0.1.0_x64-setup.exe` (4.06 MB)
- Staged Deliverables: `C:\Users\Yuvraj\Desktop\Leo-Release\`
