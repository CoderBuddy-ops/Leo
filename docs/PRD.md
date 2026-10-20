# Leo Product Requirements & Architectural Specification

> Canonical PRD for Leo — Native C# / .NET 8 / WinUI 3 Windows AI Desktop Companion.

## 1. Product Overview

Leo is an original local-first Windows desktop companion built natively in C# / .NET 8 / WinUI 3 using the Windows App SDK. It provides a delightful, fluid, distraction-free companion that hovers gracefully above everyday applications.

Key components:
1. **Compact Floating Capsule Dock**: An always-on-top, truly transparent, deeply rounded horizontal capsule. Renders an expressive procedural character (vertical oval eyes, soft symmetrical pink blush, no mouth, no blue center dot) with natural blinking, gaze tracking, breathing, and emotional reactivity.
2. **Integrated Collapsible Chat Drawer**: High-performance scrolling chat message history with real-time token streaming and cancellation.
3. **Dedicated Settings Window**: Standalone WinUI 3 window for configuring Google Gemini API credentials (encrypted in Windows `PasswordVault`), testing local AI loopback endpoints (Ollama / LM Studio), tuning speech preferences, and toggling reduced motion.
4. **Native System Tray Service**: Shell notification area presence (`Shell_NotifyIconW`) with a subclassed window procedure providing a responsive context menu (`Restore / Open Capsule`, `Hide Capsule`, `Settings...`, `Quit Leo`).
5. **Self-Contained Distribution**: 100% native standalone packaging with zero browser, webview, Vite, Node.js, or external runtime requirements.

---

## 2. User Personas & Jobs-to-be-Done (JTBD)

- **Persona 1: The Focused Software Engineer (Priya)**
  - *JTBD:* While writing code or debugging terminal commands, Priya wants to ask quick questions or dictate notes without switching windows, losing focus, or launching heavy web browsers.
- **Persona 2: The Privacy-Conscious Writer (Alex)**
  - *JTBD:* When drafting sensitive documents, Alex wants fast AI assistance that guarantees audio never leaves the machine, clearly flags external cloud requests, and securely encrypts API keys using operating-system-level hardware vaults.

---

## 3. Core Product Requirements

### 3.1 Functional Requirements
- **FR-1 (Floating Capsule Dock):** Borderless WinUI 3 window with DWM glass margin transparency (`DwmExtendFrameIntoClientArea` with `-1` margins). Compact horizontal dimension (440px width, 64px capsule height, `CornerRadius="32"`). Direct pointer dragging via Win32 `WM_NCLBUTTONDOWN`.
- **FR-2 (Original Procedural Character):** Physics-driven 2D character engine with damped harmonic oscillator springs (`k=280`, `d=22`). Deterministic `Mulberry32` PRNG for organic gaze saccades. Canonical Leo Icon V2 visual design (vertical oval eyes, subtle symmetrical pink blush marks, zero mouth, zero center dot).
- **FR-3 (Streaming Chat & Conversation Orchestration):** Interactive collapsible chat drawer (`ScrollViewer` + `StackPanel`). Asynchronous token streaming, real-time message bubble layout, cancellation support, and speech cleanup filter for spoken input.
- **FR-4 (Pluggable AI Backends):**
  - **Google Gemini**: SSE streaming client, API key encrypted in Windows `PasswordVault`, clear `[Leaves your PC]` cloud egress badges.
  - **Local AI (Ollama / LM Studio)**: OpenAI-compatible endpoints (`http://127.0.0.1:11434/v1`, `http://127.0.0.1:1234/v1`), strict loopback validation, dynamic model discovery via `GET /models`.
- **FR-5 (Dedicated Settings Window):** Standalone multi-tab WinUI 3 window for provider selection, credential management, model selection, voice tuning, and privacy controls.
- **FR-6 (Native System Tray):** Win32 `Shell_NotifyIconW` integration with window subclassing (`SetWindowSubclass`) to handle tray left-clicks (restore/toggle) and right-clicks (context menu).
- **FR-7 (Self-Contained Packaging):** Standalone x64 self-contained release with zero external runtime prerequisites.

### 3.2 Non-Functional Requirements
- **NFR-1 (Privacy & Zero Audio Egress):** Raw microphone audio never leaves the local machine. Signal analysis (RMS + ZCR) runs strictly in-process. Cloud calls require affirmative user action and display visible badges.
- **NFR-2 (Performance Budget):** 60 FPS active character rendering via high-precision `DispatcherQueueTimer` (~16.6ms intervals); minimal resource footprint when idle.
- **NFR-3 (Secrets Safety):** Secrets and API keys are stored exclusively in Windows `PasswordVault` (Resource: `Leo_Windows_AI_Assistant`). Never written to plaintext files or logs.
- **NFR-4 (Accessibility):** Supports reduced-motion preferences by halting continuous oscillation and idle breathing.

---

## 4. Verification & Acceptance Criteria

1. **Compilation**: `dotnet build Leo.sln` compiles with 0 warnings and 0 errors.
2. **Automated Testing**: `dotnet test tests/Leo.Core.Tests/Leo.Core.Tests.csproj` passes 18/18 unit tests (Mulberry32, springs, gestures, speech cleanup, VAD, loopback policy, streaming orchestrator, settings).
3. **Packaging**: `scripts/package-release.ps1` publishes standalone self-contained `win-x64` bundle and archives to `release/Leo-Windows-x64-v1.0.0.zip`.
4. **Window Transparency**: Capsule rendered with pure transparent exterior via DWM glass margins.
5. **System Tray**: Right-click displays native Win32 popup menu (`Restore`, `Hide`, `Settings`, `Quit`); Left-click toggles/restores window.
