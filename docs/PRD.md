# Leo Product Requirements & Ralph Task Plan

> Canonical PRD for the Ralph Loop execution in Leo. This document defines the discrete, independently testable tasks. During iteration runs, tasks are implemented one by one with strict verification. Record execution progress in `progress.txt`.

## Product Overview

Leo is an original local-first Windows desktop companion inspired by the interaction patterns of HeyNixi and Coucou, but built with 100% original Leo assets, architecture, and design. It consists of:
1. **Compact Floating Capsule / Dock**: Always-on-top, truly transparent exterior, responsive micro-animations, expressive face, clear states (idle, listening, thinking, speaking, error).
2. **Dedicated Settings & Management Window**: Separate native window for AI provider setup (Google Gemini API key with masked input and secure storage), local AI detection (Ollama / LM Studio), voice/hotkey config, privacy controls, and diagnostics.
3. **Windows System Tray & Background Service**: Tray icon with contextual menu (Open Leo, Show/Hide Dock, Settings, Start with Windows, Quit) and clean single-instance lifecycle.
4. **Production Packaging**: Zero-localhost self-contained static bundling, no persistent terminal window, optimized installer payload (<5 MB NSIS setup), Start menu entry, desktop shortcut, uninstaller.

---

## User Personas & Jobs-to-be-Done (JTBD)

- **Persona 1: The Focused Software Engineer (Priya)**
  - *JTBD:* When coding in VS Code or running terminal sessions, Priya wants to dictate commit messages, ask questions about terminal errors, and approve agent tool calls without switching windows or breaking flow.
- **Persona 2: The Privacy-Conscious Writer (Alex)**
  - *JTBD:* When writing notes or drafting emails, Alex wants fast speech-to-text that runs 100% offline, automatically removes filler words, and keeps audio on the local machine.

---

## Core Product Requirements

### Functional Requirements
- **FR-1 (Floating Capsule Overlay):** Frameless, transparent companion window at the top center of the screen. Dynamic drawers for Chat (400×340) and Dictate (400×300).
- **FR-2 (Original Companion Character):** Expressive 2D canvas procedural character with physics-based gaze and moods, blush marks, and no mouth (canonical Leo Icon V2).
- **FR-3 (Universal Push-to-Talk Dictation):** Global hotkey (`Right Alt`), local 16kHz VAD, filler word removal, verbal self-correction resolution, and clipboard staging.
- **FR-4 (AI Provider Flexibility):** Strict Offline Mode, local loopback endpoints (Ollama / LM Studio), and Google Gemini with masked API key storage and explicit `[Leaves your PC]` cloud badges.
- **FR-5 (Dedicated Settings Window):** Standalone 560×640 window (`settings.html`) for provider configuration, connection tests, voice tuning, and privacy controls.
- **FR-6 (Windows System Tray):** Notification-area icon with menu (`Open Leo`, `Show/Hide Dock`, `Settings`, `Start with Windows`, `Quit`), click to focus, and hide-to-tray on close.
- **FR-7 (Self-Contained Distribution):** Zero-localhost static asset bundling, GUI subsystem (`#![windows_subsystem = "windows"]`), and branded NSIS installer (<5 MB).

### Non-Functional Requirements
- **NFR-1 (Privacy & Local-First):** Raw audio never leaves the host machine. Cloud calls require explicit configuration and display user-visible egress badges.
- **NFR-2 (Screen Share Safety):** Window applies Win32 `WDA_EXCLUDEFROMCAPTURE` to remain invisible during conference calls and recordings.
- **NFR-3 (Windows System Safety):** AltGr European keyboard layout compatibility (`€`, `ą`, `ß`). Zero execution of unconfirmed shell strings.
- **NFR-4 (Accessibility & Motion):** Full keyboard navigation (`Esc` closes, `Ctrl+Alt+Space` opens chat); respects `prefers-reduced-motion`.

---

## Ralph Loop Tasks


### Task 1: Baseline and Reproduce Release Failure
- **Objective**: Inspect Git status, framework, app entry points, scripts, package lockfiles, build logs, and actual launch paths. Reproduce `localhost refused to connect`, the blank square panel, terminal-on-launch behavior, and tray behavior.
- **Scope**: Diagnosis and reproduction documentation only. No destructive file deletions.
- **Verification**: Exact command traces, exit codes, and root-cause evidence documented in `progress.txt`.

### Task 2: Safe Repository Size Inventory & Cleanup
- **Objective**: Measure large directories/files; classify source, dependency cache, build output, model weights, backups, logs, and release bundles. Check references before removal. Preserve `.git`, source, lockfiles, user changes, secrets, and model weights.
- **Scope**: Run size inventory. Propose measured cleanup. Execute only safe project-local reproducible cleanup (such as cleaning target debug cache when appropriate).
- **Verification**: Before/after folder sizes recorded in MB/GB.

### Task 3: Make Production Launch Self-Contained (Zero-Localhost)
- **Objective**: Fix frontend asset packaging and native startup so installed Leo does not depend on localhost, Vite dev server, or a console window.
- **Scope**: Multi-page Vite configuration (`index.html` and `settings.html`), Tauri static asset bundling (`frontendDist: "../dist"`), `#![windows_subsystem = "windows"]`.
- **Verification**: `npm run build` generates both `dist/index.html` and `dist/settings.html`. Packaged app launches with dev server offline.

### Task 4: Repair Chat and Settings Views (Fix Blank Panels)
- **Objective**: Trace navigation/event handlers, native window creation, sizing, CSS, clipping, and IPC behind empty square popups. Implement functional panels and dedicated settings window.
- **Scope**: `app/src/main.ts`, `app/index.html`, `app/settings.html`, `src/ipc/leo_ipc.ts`. Sizing geometry (capsule expands to 400x340 for chat, 400x300 for dictate; settings opens dedicated window).
- **Verification**: Settings opens dedicated window; chat and voice drawers render full content without blank square truncation.

### Task 5: Implement Native Tray and Background Behavior
- **Objective**: Add a native notification-area icon and menu for Open Leo, Show/Hide Dock, Settings, Start with Windows, and Quit. Startup must be opt-in, and Quit must actually end the process.
- **Scope**: `app/src-tauri/src/lib.rs` system tray integration, window hide-to-tray on close, left-click tray activation, single instance handling.
- **Verification**: Tray menu items trigger expected window actions; close hides to tray; quit terminates cleanly.

### Task 6: Verify Transparent Floating Dock
- **Objective**: Ensure a true transparent native overlay with regular capsule geometry, correct always-on-top, dragging, focus rules, DPI/multiple-monitor handling, and off-screen recovery.
- **Scope**: Win32 window styles (`WS_EX_LAYERED`, `WDA_EXCLUDEFROMCAPTURE`), CSS transparent background.
- **Verification**: Transparent background outside capsule verified; no white/black box.

### Task 7: Establish Visual and Motion Quality
- **Objective**: Use canonical Leo Icon V2 (blush marks, no mouth); procedural 2D canvas face; smooth spring physics; idle/active/speaking/thinking states; reduced-motion support.
- **Scope**: `src/companion/leo_face.ts`, `src/companion/gestures.ts`, `app/src/main.ts`.
- **Verification**: 8/8 icon integrity tests pass; 60 FPS active / 0 FPS idle verified; reduced-motion preference respected.

### Task 8: Secure Provider Settings (Gemini Integration)
- **Objective**: Implement masked Gemini API key entry, secure local storage, Save/Cancel, Test Connection, model selection (`gemini-2.5-flash`), clear error handling, and `[Leaves your PC]` cloud egress notice.
- **Scope**: `src/models/gemini_client.ts`, `app/settings.html`, `app/src/main.ts`. Never leak keys into Git, logs, or plain text bundles.
- **Verification**: Test connection verifies key validity; key masked in UI; error messages redact secrets; 6/6 provider unit tests pass.

### Task 9: Add Offline and Local Provider Mode
- **Objective**: Detect/configure existing Ollama/LM Studio loopback endpoints (`http://127.0.0.1:11434`, `http://127.0.0.1:1234`). Make Offline mode prevent cloud requests. Do not auto-download model weights.
- **Scope**: `src/models/local_client.ts`, `app/settings.html`.
- **Verification**: Offline mode strictly gates cloud egress; loopback detection tests pass.

### Task 10: Voice, Notifications, Usage, and Permissions
- **Objective**: Local Voice Activity Detection (16kHz mono RMS + ZCR), offline speech recognition gate (user consent required before downloading weights), verbal filler cleanup, honest usage tracking.
- **Scope**: `src/voice/audio_vad.ts`, `src/voice/offline_asr.ts`, `src/voice/speech_cleanup.ts`.
- **Verification**: 10/10 extended voice & VAD tests pass. Zero audio egress invariant maintained.

### Task 11: Optimize Production Bundle
- **Objective**: Inspect dependency and bundle reports, remove unused assets/debug artifacts, ensure release build produces minimal footprint.
- **Scope**: Vite bundle tree-shaking, Cargo release profile optimization (`opt-level = 3`, `lto = true`, `codegen-units = 1`, `panic = "abort"`).
- **Verification**: Packaged binary and installer size measured.

### Task 12: Build Branded Windows Installer
- **Objective**: Build official Windows NSIS installer (`Leo-Setup-x64.exe`) using Tauri 2 bundle pipeline with custom icon, Start Menu shortcut, desktop shortcut, and uninstaller.
- **Scope**: `tauri.conf.json` NSIS bundle config, installer metadata.
- **Verification**: Installer successfully compiles and produces runnable setup executable.

### Task 13: Final QA and Release Folder
- **Objective**: Run full test suites (31/31 passing), verify production build, verify installer, publish verified artifacts to `C:\Users\Yuvraj\Desktop\Leo-Release\`.
- **Scope**: Automated test runner, build validation, artifact deployment.
- **Verification**: All unit tests pass, release directory populated, documentation updated in `TASKS.md` and `CHANGELOG.md`.
