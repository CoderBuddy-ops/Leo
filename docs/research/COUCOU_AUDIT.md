# Coucou Technical and Code Audit

**Repository:** [https://github.com/Louis-CFM/coucou](https://github.com/Louis-CFM/coucou)  
**Inspected Commit:** `434d6a2b4af2e5ce117fa425170abe25620130b9` (Merge PR #783, 2026-10-16)  
**Auditor:** Leo Principal Engineering Team

---

## 1. Overview and Architecture

Coucou is a companion app that originated on macOS (`NotchBuddy/` in Swift) to live in the MacBook camera notch, and was ported to Windows (`windows/`) using **Tauri 2 + Rust + TypeScript/Vite**.

```
reference_coucou/
├── windows/
│   ├── src/                    # TypeScript frontend
│   │   ├── core/               # Bridge, settings, logger, platform abstractions
│   │   ├── island/             # Top-edge pill, FSM, animations, layout
│   │   ├── flow/               # Dictation, notetaker, snippets, history
│   │   └── integrations/       # Claude Code, Spotify, GitHub, Codex
│   ├── src-tauri/              # Rust backend
│   │   ├── src/
│   │   │   ├── lib.rs          # Tauri command registration & plugin setup
│   │   │   ├── island.rs       # Win32 transparent window positioning & click-through
│   │   │   ├── shortcuts.rs    # Low-level Windows global hotkey hook (WH_KEYBOARD_LL)
│   │   │   ├── agents.rs       # Named pipes & IPC for Claude Code hooks
│   │   │   ├── local_chat.rs   # Local model server integration (Ollama / OpenAI compat)
│   │   │   ├── speech/         # Sherpa-onnx / Parakeet ASR engine bindings
│   │   │   └── tray.rs         # Windows system tray menu
│   └── tests/                  # Node.js native test suite (399 unit tests passing)
```

---

## 2. Windows Native Implementation Analysis

### 2.1 Always-on-Top Floating Window (`src-tauri/src/island.rs`)
- **[Observed]:** Uses Win32 API calls (`SetWindowPos`, `SetWindowLongPtrW`, `WS_EX_LAYERED`, `WS_EX_TOPMOST`, `WS_EX_TOOLWINDOW`) to create a frameless, transparent, always-on-top window anchored at the top-center of the primary monitor.
- **[Observed]:** High-DPI handling queries `GetDpiForWindow` / `GetDpiForMonitor` to scale physical coordinates correctly on 125%, 150%, and 200% scaling factors.
- **[Observed]:** Multi-monitor handling listens for display topology changes and re-anchors the window cleanly.

### 2.2 Global Keyboard Hooks & AltGr Safety (`src-tauri/src/shortcuts.rs`)
- **[Observed]:** Uses `SetWindowsHookExW` with `WH_KEYBOARD_LL` for low-level global hotkey detection.
- **[Observed]:** Critical Windows Internationalization Design: On Windows keyboards with `AltGr`, pressing `AltGr` emits both `VK_LCONTROL` and `VK_RMENU`. Coucou checks the active keyboard layout using `ToUnicodeEx` / `GetKeyboardLayout` to avoid capturing keystrokes that produce accented characters or currency symbols (e.g. `AltGr+E` produces `€` on European keyboards; `AltGr+A` produces `ą` on Polish keyboards).

### 2.3 Agent Session Event Hook & Named Pipes (`src-tauri/src/agents.rs`, `pipe.rs`)
- **[Observed]:** Implements a Windows Named Pipe server (`\\.\pipe\coucou-agent-pipe`) to receive JSON events from external coding agents (Claude Code, GitHub CLI, Codex).
- **[Observed]:** Fail-Safe Protocol: When a permission request arrives from an agent, a non-blocking card pops up. If Coucou times out or is closed, the request safely times out or fails open/prompts the terminal so the coding agent is never permanently hung.

### 2.4 Offline Speech Recognition (`src-tauri/src/speech/`)
- **[Observed]:** Implements offline speech-to-text using `sherpa-onnx` and NVIDIA `Parakeet-TDT-0.6B-v3` running directly on the CPU.
- **[Observed]:** Weights (~700 MB) are downloaded only after explicit user consent with SHA-256 verification.

---

## 3. Test Suite Verification

- **Command:** `node --import ./tests/setup.mjs --test "tests/**/*.test.mjs"`
- **Result:** **399 tests passing, 0 failing** (Executed 2026-10-17).
- **Coverage:** FSM state transitions, shortcut parsing, AltGr safety checks, diff viewers, markdown rendering, model server pickers, recap calculations, and pill navigation.
