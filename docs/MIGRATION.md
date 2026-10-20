# Migration Ledger: Tauri / TypeScript to C# / .NET 8 / WinUI 3

**Date:** October 20, 2026  
**Migration Author:** Principal Software Architect & Release Engineer  
**Status:** Completed & Verified  

---

## 1. Executive Summary

Leo has been successfully migrated from a hybrid Tauri 2 / TypeScript webview stack to a 100% native **C# / modern .NET 8 / WinUI 3 (Windows App SDK 1.6)** desktop architecture.

The new architecture eliminates the dependency on web browsers, webviews, Node.js development servers, and localhost ports, providing:
- Instant native startup without webview initialization lag.
- Pure DWM glass margin transparency outside the capsule dock without black/white artifacts.
- Hardware-backed encrypted API key storage using the Windows `PasswordVault`.
- Standalone self-contained release deployment (`win-x64`) with zero external runtime prerequisites.

---

## 2. Technology Stack Decision Matrix

| Dimension | Legacy Stack (Tauri 2) | Migrated Stack (C# / WinUI 3) | Architectural Rationale |
| :--- | :--- | :--- | :--- |
| **Language** | Rust + TypeScript | Pure C# 12 | Single-language desktop solution; seamless Win32 / COM interop. |
| **Framework** | Tauri 2 + Vite | WinUI 3 + Windows App SDK 1.6 | Official Microsoft Windows UI framework with fluent design and native XAML. |
| **Runtime** | Node.js (build) + Rust runtime | .NET 8 LTS (`net8.0-windows10.0.19041.0`) | High-performance managed runtime with AOT and self-contained deployment. |
| **Windowing & Transparency** | Webview2 transparent composition | Win32 `AppWindow` + `DwmExtendFrameIntoClientArea` | Native DWM glass margins with direct window compositing; zero webview artifacts. |
| **Dragging** | CSS `-webkit-app-region: drag` | Win32 `WM_NCLBUTTONDOWN` message dispatch | Zero-lag native dragging without cursor jitter or event suppression. |
| **System Tray** | Tauri tray plugin | Native `Shell_NotifyIconW` + `SetWindowSubclass` | Complete Win32 menu control (`Restore`, `Hide`, `Settings`, `Quit`). |
| **Secrets Management** | Plaintext config files / custom vault | Windows `Windows.Security.Credentials.PasswordVault` | Hardware-backed operating system credential encryption. |
| **Testing** | Node test runner (`node --test`) | xUnit + .NET Test SDK (`dotnet test`) | High-speed parallel unit testing integrated into standard MSBuild tooling. |
| **Packaging** | NSIS via Tauri CLI | Self-contained single-folder release & Zip archive | Reproducible standalone bundle containing runtime and dependencies. |

---

## 3. Preserved Assets and Intellectual Property

In strict accordance with project invariants:
1. **Canonical Leo Icon V2**:
   - Master source asset: `assets/brand/leo-face-icon-1024.png`
   - Multi-resolution icon set: `assets/icons/icon.ico`, `icon.png`, `128x128.png`, `32x32.png`
   - Face rendering: Vertical oval eyes with subtle symmetrical pink blush marks (`#AAFFA0AF`), zero mouth, and zero blue center dot.
2. **Cleanroom IP Protection**:
   - Zero Mochi or Coucou artwork, icons, or audio imported.
   - Zero proprietary minified code from HeyNixi.
   - 100% original C# procedural character engine matching mathematical golden vectors.
3. **Local Privacy Invariant**:
   - Zero audio egress. Microphone analysis (RMS + ZCR) runs locally on-device.
   - External cloud calls (Google Gemini) display user-visible egress badges (`[Leaves your PC]`).

---

## 4. Key Implementation Lessons & Resolved Challenges

### 4.1 CLI WinUI 3 Build Resolution (`MSB4062`)
- **Problem:** Building WinUI 3 unpackaged apps from the command line without full Visual Studio IDE installed can throw `MSB4062: The Microsoft.Build.Packaging.Pri.Tasks.ExpandPriContent task could not be loaded`.
- **Solution:** Configured `Leo.App.csproj` with:
  ```xml
  <EnableMsixTooling>true</EnableMsixTooling>
  <WindowsPackageType>None</WindowsPackageType>
  <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
  <Platforms>x64</Platforms>
  <RuntimeIdentifiers>win-x64</RuntimeIdentifiers>
  ```
  This allows `dotnet build Leo.sln` to compile in ~5 seconds with zero MSBuild errors.

### 4.2 Code-Only WinUI 3 Control Instantiation
- **Problem:** Using controls like `ListView` without XAML application generic resource dictionaries loaded can fail because container item styles (`ListViewItem`) are missing.
- **Solution:** Utilized a high-performance `ScrollViewer` + `StackPanel` architecture for the collapsible chat drawer. Message bubbles are dynamically instantiated `Border` elements with role-specific styling.

### 4.3 Native Tray Menu Interactivity via Window Subclassing
- **Problem:** WinUI 3 manages its own internal window procedure; custom `WM_TRAYICON` messages sent to `hWnd` by `Shell_NotifyIconW` are not handled by default.
- **Solution:** Registered a Win32 subclass via `comctl32!SetWindowSubclass`. Intercepts `WM_TRAYICON`, routing `WM_RBUTTONUP` to `TrackPopupMenuEx` and `WM_LBUTTONUP` to window restore/activation.

---

## 5. Verification Matrix

| Verification Target | Command / Check | Result |
| :--- | :--- | :--- |
| **Solution Compilation** | `dotnet build Leo.sln` | Build succeeded (0 warnings, 0 errors, 2.19s). |
| **Unit Test Suite** | `dotnet test tests/Leo.Core.Tests/Leo.Core.Tests.csproj` | Passed! 18 passed, 0 failed, 0 skipped (101ms). |
| **Standalone Release Build** | `dotnet publish src/Leo.App/Leo.App.csproj -c Release -r win-x64 --self-contained true` | Standalone executable and native libraries published. |
| **Release Archive Packaging** | `scripts/package-release.ps1` | `release/Leo-Windows-x64-v1.0.0.zip` (63.6 MB) created with SHA256 checksum. |
| **Runtime Execution** | `scripts/launch.ps1` | Window launches, capsule rendered with transparent glass borders, tray registered. |
