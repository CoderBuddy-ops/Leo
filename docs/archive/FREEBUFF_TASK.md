# Freebuff Parallel Task Brief: Native Windows Dock Audit

**Assignee:** Freebuff (Parallel Read-Only Agent)  
**Lead & Integration Owner:** Antigravity  
**Project ID:** `0896c526-c4d4-4d03-a120-18d86922f334`  
**Date:** 2026-10-18  

---

## 1. Scope & Rules of Engagement

1. **Read-Only Audit:** Do NOT write to or modify files in the working directory while Antigravity is actively executing changes.
2. **File Scope Under Review:**
   - `app/src-tauri/tauri.conf.json` (window properties, transparency, dimensions)
   - `app/src-tauri/src/lib.rs` (Win32 window styling: `WS_EX_LAYERED`, `WS_EX_NOACTIVATE`, `SWP_FRAMECHANGED`, `WDA_EXCLUDEFROMCAPTURE`)
   - `app/index.html` (CSS root and body alpha transparency rules)
   - `app/src-tauri/.cargo/config.toml` (Rust target configuration: `x86_64-pc-windows-gnu`)
3. **Objective:**
   Audit and cross-verify the native compilation linker resolution (MinGW runtime sysroot vs MSVC Build Tools) and DWM transparency guarantees.

---

## 2. Specific Questions for Freebuff

1. Does the DWM composition under Windows 11 22H2+ support per-pixel alpha on borderless windows with `WS_EX_LAYERED` without requiring `WS_POPUP`?
2. Confirm the behavior of `SetWindowDisplayAffinity(hwnd, WDA_EXCLUDEFROMCAPTURE)`: when `WDA_EXCLUDEFROMCAPTURE` is combined with transparent layered windows, does DWM composite transparent pixels cleanly or blank the bounding box during screen-casting?
3. Review whether `tauri.conf.json` requires explicit `"background_color": [0, 0, 0, 0]` in Tauri 2 or if `"transparent": true` is sufficient.

---

## 3. Reporting Instructions

Document observations and findings concisely. Antigravity will review and integrate any recommendations.
