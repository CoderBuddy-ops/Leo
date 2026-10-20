# Leo — Personal AI Companion for Windows

[![Windows 10/11](https://img.shields.io/badge/Windows-10%2F11%20x64-0078D4?logo=windows)](https://microsoft.com/windows)
[![.NET 8](https://img.shields.io/badge/.NET-8.0%20LTS-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![WinUI 3](https://img.shields.io/badge/WinUI-3%20(Windows%20App%20SDK%201.6)-0078D4?logo=windows)](https://learn.microsoft.com/windows/apps/winui/winui3/)
[![C# 12](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)](https://learn.microsoft.com/dotnet/csharp/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Tests: 18/18 Passing](https://img.shields.io/badge/Tests-18%2F18%20Passing-brightgreen.svg)](#run-automated-unit-tests)

**Leo** is a personal-first, on-device AI assistant designed natively for Windows 10 and 11. It resides in a sleek, compact floating capsule dock that hovers gracefully above your active applications, providing immediate AI assistance, streaming conversations, audio reactivity, and pluggable models (Google Gemini and local Ollama / LM Studio) without ever stealing your focus or running a background web server.

---

## Key Capabilities

- **Compact Floating Capsule Dock:** An unobtrusive, truly transparent, always-on-top dock. Built using WinUI 3 and Win32 Desktop Window Manager (DWM) glass margins with zero webview overhead.
- **Original Procedural Character ("Leo"):** A dynamic physics-based 2D companion driven by a damped harmonic oscillator spring model (`k=280`, `d=22`). Features organic gaze saccades, nonuniform blinks, breathing float, and the canonical Leo Icon V2 (vertical oval eyes, soft symmetrical pink blush marks, no mouth, and no blue center dot).
- **Collapsible Chat Drawer:** Integrated expandable drawer with high-performance scrolling message history and real-time streaming response tokens.
- **Dedicated Settings Window:** Standalone multi-tab settings window for configuring Google Gemini API credentials, selecting local model endpoints, adjusting voice parameters, and enabling reduced-motion accessibility.
- **Hardware-Backed Credential Storage:** API keys are encrypted at rest using the native Windows `PasswordVault`. Secrets are never stored in plaintext JSON, code bundles, or logs.
- **Windows System Tray & Subclassed Lifecycle:** Native notification area icon with responsive context menu (`Restore / Open Capsule`, `Hide Capsule`, `Settings...`, `Quit Leo`).
- **Zero-Localhost / Zero-Webview Execution:** 100% compiled native code. Does not require Node.js, Vite development servers, Electron, or open localhost ports.
- **100% On-Device Privacy (Zero Audio Egress):** Local Voice Activity Detection (VAD) and audio analysis operate strictly on local hardware. Audio is never uploaded off-device.
- **Pluggable AI Models with Egress Warnings:** Seamlessly connects to local model servers (Ollama, LM Studio) or Google Gemini with user-visible egress badges (`[Leaves your PC]`).

---

## Architecture Overview

```
Leo.sln
├── src/
│   ├── Leo.Core/               # net8.0: Domain models, character physics, VAD, cleanup
│   ├── Leo.Application/        # net8.0: Use cases, conversation orchestrator, provider contracts
│   ├── Leo.Infrastructure/     # net8.0-windows10.0.19041.0: PasswordVault, Gemini SSE, Local AI
│   └── Leo.App/                # net8.0-windows10.0.19041.0: WinUI 3 views, capsule dock, tray service
├── tests/
│   └── Leo.Core.Tests/         # net8.0: xUnit test suite (18 unit tests, 100% passing)
├── scripts/                    # PowerShell automation (build, test, launch, package)
├── assets/                     # Approved brand assets (Leo Icon V2, icon.ico)
└── docs/                       # Complete architecture, PRD, security, and migration documentation
```

---

## Quick Start & Developer CLI

### 1. Build Solution
```powershell
.\scripts\build.ps1
# Or via .NET CLI directly:
dotnet build Leo.sln
```

### 2. Run Automated Tests
```powershell
.\scripts\test.ps1
# Or via .NET CLI:
dotnet test tests/Leo.Core.Tests/Leo.Core.Tests.csproj
```

### 3. Launch Application
```powershell
.\scripts\launch.ps1
```

### 4. Package Standalone Release Bundle
```powershell
.\scripts\package-release.ps1
```
The self-contained standalone distribution is packaged into `release/`:
```
release/
├── Leo-Windows-x64-v1.0.0.zip   (63.6 MB self-contained with runtime)
└── SHA256SUMS.txt
```

---

## Documentation Index

Explore the documentation suite in [`docs/`](docs/INDEX.md):
- [Product Requirements Document (PRD)](docs/PRD.md) — Product vision, requirements, and acceptance criteria.
- [System Architecture](docs/architecture/ARCHITECTURE.md) — Native WinUI 3, DWM glass margins, layer boundaries, and Win32 interop.
- [Migration Ledger](docs/MIGRATION.md) — Complete record of migration from Tauri 2 to C# / .NET 8 / WinUI 3.
- [Security & Privacy](docs/SECURITY.md) — Hardware-backed `PasswordVault` storage, loopback boundaries, and zero audio egress.
- [Task Tracking](docs/TASKS.md) — Complete implementation backlog and verification evidence.
- [Changelog](docs/CHANGELOG.md) — Chronological release and migration notes.
