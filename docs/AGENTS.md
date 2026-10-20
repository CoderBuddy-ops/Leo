# Leo Engineering & Agent Operating Guide

Welcome to the **Leo** development workspace. This file defines project rules, architecture, testing protocols, and build commands for all AI coding assistants (Antigravity, Claude Code, and external tools) collaborating in this repository.

---

## 1. Project Invariants & Hard Safety Boundaries

1. **Local-First Default (Zero Audio Egress):**
   - Raw microphone audio must NEVER leave the host machine. Speech recognition runs on local hardware.
   - Any external API request (Claude, OpenAI, Gemini) must clearly display a visual indicator stating that data leaves the computer (`[Leaves your PC]`).

2. **Strict IP & Cleanroom Separation:**
   - Honor Coucou's `LICENSE-ASSETS.md`: NEVER use the names "Coucou" or "Mochi", and NEVER bundle Mochi character artwork, icons, or sounds.
   - Honor HeyNixi proprietary copyright: NEVER copy proprietary minified JavaScript, branding, or logos.
   - All character animations, sounds, icons, and UI must be 100% original to Leo.

3. **Windows System Safety:**
   - Global keyboard shortcuts must be tested against Windows European AltGr layouts (`€`, `ą`, `ß`) to prevent swallowing user keystrokes.
   - Zero execution of arbitrary unconfirmed shell commands (`cmd.exe /c`, `powershell`).
   - Consequential file-system or process actions require affirmative user confirmation.
   - Companion window must set `WDA_EXCLUDEFROMCAPTURE` to prevent accidental broadcast during screen shares.

4. **Quality & Evidence Standard:**
   - No feature is declared "done" without real automated test coverage or empirical verification.
   - Never chain test commands with `;` to mask failures; capture exact exit codes and output lines.
   - Never fabricate test runs, benchmark numbers, or completed native builds.

---

## 2. Repository Map

```
nixi/
├── .agents/rules/           # Antigravity rules (trigger: always_on)
├── app/                     # Tauri 2 Desktop Application
│   ├── index.html           # Capsule UI layout (copper-charcoal theme)
│   ├── public/favicon.png   # Black-hole icon favicon
│   ├── src/                 # Webview frontend logic
│   │   └── main.ts          # Canvas rendering, gesture loop, IPC wiring
│   └── src-tauri/           # Rust Tauri 2 native shell
│       ├── Cargo.toml       # Native Rust dependencies
│       ├── tauri.conf.json  # Tauri 2 windowing and icon bundle config
│       ├── icons/           # App icons (icon.ico, 32x32, 128x128, etc.)
│       └── src/
│           ├── main.rs      # Windows entry point
│           └── lib.rs       # Win32 window config & safe IPC commands
├── assets/brand/            # Canonical brand icons (leo-icon-1024.png)
├── docs/                    # Architectural specifications and ADRs
│   ├── architecture/        # Architecture & ADRs 0001-0005
│   ├── design/              # Companion and motion dynamics
│   ├── product/             # PRD & MVP scope
│   └── research/            # HeyNixi & Coucou cleanroom audits
├── src/                     # Core cross-platform TypeScript engines
│   ├── companion/
│   │   ├── leo_face.ts      # 2D Canvas procedural face, Mulberry32 PRNG, spring physics
│   │   └── gestures.ts      # Mood transitions and inactivity sleep controller
│   ├── ipc/
│   │   └── leo_ipc.ts       # Strictly typed, bounded native IPC bridge & dev fallback
│   ├── models/
│   │   └── local_client.ts  # OpenAI-compatible streaming loopback client
│   └── voice/
│       ├── audio_vad.ts     # 16kHz mono Voice Activity Detector (RMS, ZCR, hangover)
│       ├── offline_asr.ts   # Offline speech recognition pipeline & consent gate
│       └── speech_cleanup.ts# Verbal filler filter, correction resolver, bullet formatter
└── tests/                   # Independent automated unit tests
    ├── leo_p0.test.mjs       # P0 tests (PRNG golden vector, cleanup, loopback)
    └── leo_extended.test.mjs # Extended tests (VAD, ASR consent, IPC bounds, gestures)
```

---

## 3. Verified Build, Test, and Run Commands

Execute commands independently to accurately capture exit codes:

```bash
# 1. Regenerate Leo icon set from the facial animation idle frame (Refined Icon V2)
node scripts/generate-leo-icon.mjs

# 2. Run P0 Core Unit Tests (7/7 passing)
node --test tests/leo_p0.test.mjs

# 3. Run Extended Voice, VAD, IPC & Gesture Tests (10/10 passing)
node --test tests/leo_extended.test.mjs

# 4. Run Icon Asset Integrity Tests (8/8 passing)
node --test tests/leo_icon.test.mjs

# 5. Run AI Provider & Gemini Integration Tests (6/6 passing)
node --test tests/leo_providers.test.mjs

# 6. Build Webview Frontend Bundle
cd app && npm run build

# 7. Build Official Windows NSIS Installer (Leo-Setup-x64.exe)
cd app && npx @tauri-apps/cli build --target x86_64-pc-windows-gnu --bundles nsis
```

---

## 4. Ralph Loop & Ponytail Operating Principles

### Ralph Loop File Contracts
- **`PRD.md`**: Canonical, read-only product and task backlog divided into discrete, independently testable tasks (Task 1 to Task 13).
- **`progress.txt`**: Durable, append-only iteration log recording timestamp, task ID, files changed, exact commands/exit codes, outcomes, and next task.
- **`prompt.md`**: Execution contract for every iteration to inspect Git status, implement one task, run backpressure checks, and log progress.
- **`TASKS.md`**: Human-readable dashboard tracking task completion against the PRD.

### Ponytail Minimal-Solution Ladder (DietrichGebert/ponytail)
1. **Does it need to exist?** Skip features, options, and flexibility nobody asked for.
2. **Already in this codebase?** Reuse existing helpers, components, services, and patterns.
3. **Standard library or platform feature?** Use it unless the project has its own.
4. **An installed dependency?** Use it. Never add an external dependency for a few lines.
5. **Can it be one clear line?** Keep it simple and readable.
6. **Otherwise:** Write the minimum maintainable code that works.
*Safety Invariant*: Never cut input validation, error handling, security boundaries, or accessibility to save lines. Minimal code is complete code with the fluff removed.

---

## 5. Verified Build, Test, and Run Commands

Execute commands independently to accurately capture exit codes:

```powershell
# 1. Regenerate Leo icon set from the facial animation idle frame (Refined Icon V2)
node scripts/generate-leo-icon.mjs

# 2. Run P0 Core Unit Tests (7/7 passing)
node --test tests/leo_p0.test.mjs

# 3. Run Extended Voice, VAD, IPC & Gesture Tests (10/10 passing)
node --test tests/leo_extended.test.mjs

# 4. Run Icon Asset Integrity Tests (8/8 passing)
node --test tests/leo_icon.test.mjs

# 5. Run AI Provider & Gemini Integration Tests (6/6 passing)
node --test tests/leo_providers.test.mjs

# 6. Build Multi-Page Webview Frontend Bundle (index.html + settings.html)
cd app && npm run build

# 7. Check / Build Native Rust Backend (Prepend MinGW Toolchain to PATH)
powershell -ExecutionPolicy Bypass -File scripts/check-tauri.ps1

# 8. Build Official Windows NSIS Installer (Leo-Setup-x64.exe)
powershell -ExecutionPolicy Bypass -File scripts/build-installer.ps1
```

---

## 6. Current Milestone & Status

- **Status:** Native Windows floating capsule, dedicated settings window, system tray, and genuine installer are operational.
- **System Tray:** Notification-area icon with menu (`Open Leo`, `Show/Hide Dock`, `Settings`, `Start with Windows`, `Quit`), click to focus, hide-to-tray on close.
- **Dedicated Settings Window:** Standalone full-featured settings page (`settings.html`) with Gemini API key masked input, connection test, local AI discovery, voice config, and privacy controls.
- **Multi-Page Vite Bundle:** Builds both `dist/index.html` and `dist/settings.html` for self-contained, zero-localhost execution.
- **Zero Localhost Dependency:** Production app embeds static assets (`../dist`), never opening Chrome, dev servers, or localhost ports.
- **Zero Console Window:** Enforces `#![windows_subsystem = "windows"]` for seamless GUI execution without terminal windows.
- **Brand & Icon:** Refined Leo Icon V2 (canonical backup preserved, mouth removed, subtle symmetric soft pink blush marks added, all Windows/web sizes regenerated).
- **AI Models:** Dual local-first loopback (Ollama / LM Studio) + online Google Gemini SSE streaming client with masked key storage, secret redaction, and `[Leaves your PC]` egress notice.
- **Unit Tests:** **31/31 tests passing (100%)** (7 P0 + 10 extended + 8 icon integrity + 6 provider tests).

