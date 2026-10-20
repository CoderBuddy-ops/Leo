# Master Task Backlog: Leo

**Status:** Completed & Verified — C# / .NET 8 / WinUI 3 Architecture  
**Architecture:** [ARCHITECTURE.md](architecture/ARCHITECTURE.md)  
**Migration Ledger:** [MIGRATION.md](MIGRATION.md)  
**PRD Specification:** [PRD.md](PRD.md)  
**Last Updated:** 2026-10-20  

---

## Phase 7: C# / Modern .NET 8 / WinUI 3 Master Migration & Release (Completed)

- [x] `TSK-070`: **Repository Audit & Baseline**:
  - Inspected repository at `c:\Users\Yuvraj\Desktop\Project\nixi`, created migration branch `migration/dotnet-winui3`.
  - Tagged baseline state `baseline-before-dotnet-migration`.
- [x] `TSK-071`: **Asset & Trademark Preservation**:
  - Preserved canonical Leo Icon V2 (`assets/brand/leo-face-icon-1024.png`) and multi-resolution Windows icons in `assets/icons/icon.ico`.
  - Enforced zero Mochi/Coucou/HeyNixi IP infringement.
- [x] `TSK-072`: **C# / .NET 8 Solution Architecture**:
  - Created root `Leo.sln` with layered separation: `Leo.Core`, `Leo.Application`, `Leo.Infrastructure`, `Leo.App`, and `Leo.Core.Tests`.
  - Configured project targets (`net8.0` and `net8.0-windows10.0.19041.0`).
- [x] `TSK-073`: **Core Companion Domain & Physics Engine**:
  - Ported `Mulberry32` PRNG matching exact golden vectors.
  - Implemented `LeoCharacterEngine` with damped harmonic oscillator springs (`k=280`, `d=22`), organic gaze saccades, nonuniform blinks, breathing float, and squash-and-stretch.
  - Implemented `LeoMoodState` presets (Idle, Hover, Listening, Thinking, Happy, Talking, Concentrate, Sleepy, Offline).
  - Implemented `LeoGestureController` managing timeouts and voice state transitions.
- [x] `TSK-074`: **Voice, VAD & Speech Transcript Cleanup**:
  - Implemented `SpeechCleanupFilter` removing verbal fillers (*"um"*, *"uh"*), resolving spoken self-corrections, and formatting bullet lists.
  - Implemented `VoiceActivityDetector` with 16kHz mono RMS + ZCR dual-criteria detection and silence hangover.
- [x] `TSK-075`: **Hardware-Backed Credential Management**:
  - Implemented `WindowsCredentialService` using Windows `Windows.Security.Credentials.PasswordVault` (Resource: `Leo_Windows_AI_Assistant`).
- [x] `TSK-076`: **AI Provider Adapters**:
  - Implemented `GeminiProvider` with HTTP/2 SSE streaming, model selection (`gemini-2.0-flash`), key masking, secret redaction, and `[Leaves your PC]` cloud badges.
  - Implemented `LocalAiProvider` connecting to Ollama / LM Studio with loopback detection and dynamic `/models` discovery.
  - Implemented `JsonSettingsService` for `%APPDATA%\Leo\settings.json`.
- [x] `TSK-077`: **Native WinUI 3 Floating Capsule Dock**:
  - Created `CapsuleWindow` with `AppWindow` and borderless, non-resizable `OverlappedPresenter`.
  - Applied DWM glass margins (`DwmExtendFrameIntoClientArea` with `-1` margins) for transparent exterior bounds.
  - Implemented hardware-accelerated pointer dragging via Win32 `WM_NCLBUTTONDOWN`.
  - Rendered procedural face with vertical oval eyes, subtle symmetrical pink blush marks, zero mouth, and no blue center dot.
- [x] `TSK-078`: **Interactive Collapsible Chat Drawer**:
  - Built high-performance `ScrollViewer` + `StackPanel` layout for chat message history and token streaming.
  - Connected Enter-to-send, Send/Stop buttons, and dynamic capsule expansion/collapse.
- [x] `TSK-079`: **Native System Tray Service**:
  - Implemented `SystemTrayService` using Win32 `Shell_NotifyIconW`.
  - Subclassed window procedure (`SetWindowSubclass`) to handle tray left-clicks (restore/toggle) and right-clicks (context menu with Restore, Hide, Settings, Quit).
- [x] `TSK-080`: **Standalone Settings Window**:
  - Created `SettingsWindow` with masked API key input, connection testing, local endpoint discovery, speech sliders, and reduced-motion toggles.
- [x] `TSK-081`: **Automated Test Suite**:
  - Created 18 automated xUnit tests in `Leo.Core.Tests`.
  - Verified 18/18 passing in 101ms via `dotnet test`.
- [x] `TSK-082`: **CLI MSBuild Packaging Fix (`MSB4062`)**:
  - Resolved unpackaged WinUI 3 CLI build issue using `<WindowsPackageType>None</WindowsPackageType>` and `<EnableMsixTooling>true</EnableMsixTooling>`.
- [x] `TSK-083`: **Standalone Self-Contained Release**:
  - Published self-contained `win-x64` release and packaged into `release/Leo-Windows-x64-v1.0.0.zip` (63.6 MB) with SHA256 checksums.
- [x] `TSK-084`: **Developer CLI Automation**:
  - Authored `scripts/build.ps1`, `scripts/test.ps1`, `scripts/launch.ps1`, and `scripts/package-release.ps1`.
- [x] `TSK-085`: **Documentation Migration**:
  - Updated `README.md`, `PRD.md`, `ARCHITECTURE.md`, `MIGRATION.md`, `SECURITY.md`, `TASKS.md`, `CHANGELOG.md`, and `INDEX.md`.

---

## Historical Phases (Archive)

### Phase 1–3: Research, Licensing & Specs (Completed)
- [x] `TSK-001` to `TSK-020`: Research, tool inventory, licensing audit, cleanroom isolation.

### Phase 4–6: Initial Vertical Slices & Ralph Loop (Completed / Migrated)
- [x] `TSK-030` to `TSK-067`: Initial prototype validation, icon generation, and Ralph Loop setup.
