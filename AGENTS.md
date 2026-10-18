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
# 1. Regenerate Leo icon set from the facial animation idle frame
node scripts/generate-leo-icon.mjs

# 2. Run P0 Core Unit Tests (7/7 passing)
node --test tests/leo_p0.test.mjs

# 3. Run Extended Voice, VAD, IPC & Gesture Tests (10/10 passing)
node --test tests/leo_extended.test.mjs

# 4. Run Icon Asset Integrity Tests (8/8 passing)
node --test tests/leo_icon.test.mjs

# 3. Build Webview Frontend Bundle
cd app && npm run build

# 4. Preview Webview Application (Port 1420)
cd app && npx vite preview --port 1420
```

---

## 4. Current Milestone & Status

- **Status:** Webview interactive capsule is **100% operational** with original procedural face dynamics, breathing oscillation, squash-and-stretch interaction, speech cleanup, VAD, and offline ASR consent gate.
- **Unit Tests:** **25/25 tests passing (100%)** (7 P0 + 10 extended + 8 icon integrity).
- **Native Windows Shell:** Rust backend configured in `app/src-tauri/src/lib.rs` with `WS_EX_NOACTIVATE` and `WDA_EXCLUDEFROMCAPTURE`. Final native compilation (`cargo build`) is blocked on the Windows SDK C++ import libraries (`kernel32.lib`). See `TASKS.md` for exact remediation steps.
