# Leo — Personal AI Companion for Windows

[![Windows 10/11](https://img.shields.io/badge/Windows-10%2F11%20x64-0078D4?logo=windows)](https://microsoft.com/windows)
[![Tauri 2](https://img.shields.io/badge/Tauri-2-FFC131?logo=tauri&logoColor=black)](https://tauri.app)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.6-3178C6?logo=typescript)](https://www.typescriptlang.org/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Tests: 31/31 Passing](https://img.shields.io/badge/Tests-31%2F31%20Passing-brightgreen.svg)](#run-automated-unit-tests)

**Leo** is a personal-first, on-device AI assistant designed for Windows 10 and 11. It resides in a sleek, compact floating capsule at the top edge of your primary screen, providing immediate voice assistance, universal push-to-talk dictation, safe native automation, and pluggable AI models (Google Gemini and local Ollama / LM Studio).

---

## Key Capabilities

- **Top-Edge Floating Capsule:** Unobtrusive, transparent, always-on-top capsule that never steals active keyboard focus. Features dynamic drawers for Chat (400×340) and Dictate (400×300).
- **Dedicated Settings Window:** Standalone full-featured settings page (`settings.html`) for configuring Gemini API keys (masked input), testing connection, selecting local AI endpoints, and tuning voice parameters.
- **Windows System Tray & Background Service:** Native notification-area icon with context menu (`Open Leo`, `Show/Hide Dock`, `Settings`, `Start with Windows`, `Quit`), click to focus, and hide-to-tray on close.
- **Original Procedural Character ("Leo"):** A dynamic, physics-based 2D canvas character featuring continuous spring-damper gaze tracking, organic saccades, and responsive emotional states with the approved Leo Icon V2 (blush marks, no mouth).
- **Zero-Localhost Execution:** Packaged release embeds static multi-page assets (`frontendDist: "../dist"`). Does not require Node, Vite, or open local ports in production.
- **Zero Console Window:** Native GUI subsystem (`#![windows_subsystem = "windows"]`) runs cleanly without background terminal windows.
- **100% On-Device Dictation (Zero Audio Egress):** Voice Activity Detection (VAD) and speech recognition run entirely on local hardware. Audio never leaves the local machine.
- **Pluggable AI Models with Egress Warnings:** Seamlessly connects to local model servers (Ollama, LM Studio) or Google Gemini via SSE streaming with mandatory `[Leaves your PC]` cloud badges.
- **Windows Privacy Protection:** Applies Win32 `WDA_EXCLUDEFROMCAPTURE` to keep Leo invisible during screen shares and recordings.

---

## Quick Start & Verification

### Official Windows Installer
The verified production installer is available in `C:\Users\Yuvraj\Desktop\Leo-Release\`:
```
Leo-Setup-0.1.0-x64.exe (4.06 MB)
```
Run the setup wizard to install Leo with Start Menu and Desktop shortcuts and clean uninstaller support.

### Run Automated Unit Tests (100% Passing)
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

### Build from Source
```powershell
# Build multi-page frontend bundle and NSIS Windows installer
powershell -ExecutionPolicy Bypass -File scripts/build-installer.ps1
```

---

## Canonical Documentation Index

Explore the consolidated documentation suite in [`docs/`](docs/INDEX.md):
- [Product Requirements Document (PRD)](PRD.md) — Product vision, personas, JTBD, FR/NFRs, and 13 Ralph Loop tasks.
- [Engineering & Agent Operating Guide](AGENTS.md) — Ponytail minimal-solution ladder, safety boundaries, verified commands.
- [System Architecture](docs/architecture/ARCHITECTURE.md) — Multi-page webview, native shell, tray lifecycle, and IPC bridge.
- [Development & Setup Guide](docs/development/SETUP.md) — Host prerequisites, MinGW GCC 16.2.0 toolchain, dev workflow.
- [Release & Packaging Guide](docs/release/RELEASE.md) — Installer packaging, NSIS 4.06 MB installer, standalone binary.
- [Security, Privacy & Network Policy](docs/security/SECURITY.md) — Zero audio egress, key masking, window capture exclusion.
- [Maintenance & Cleanup Report](docs/maintenance/CLEANUP_REPORT.md) — Measured disk cleanup and documentation audit.
- [Architecture Decision Records (ADRs)](docs/decisions/0001-foundation.md) — Consequential structural decisions (0001 to 0005).
- [Third-Party Notices](THIRD_PARTY_NOTICES.md) — Open-source licensing obligations and cleanroom separation.
