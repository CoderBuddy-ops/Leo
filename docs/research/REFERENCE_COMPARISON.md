# Reference Comparison Matrix

**Audit Date:** 2026-10-17  
**Comparison:** HeyNixi vs Coucou vs Leo (Target Product)

---

## 1. Comparative Feature and Architecture Matrix

| Capability / Behavior | HeyNixi Evidence & Confidence | Coucou Implementation Evidence & Path | Proposed Leo Behavior | Windows Implementation Strategy | Offline / Cloud Status | Security Implications | Phase |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Top-Edge Floating Pill** | [Observed] Canvas & CSS pill `[104, 54]` to `[340, 88]` | [Observed] `windows/src-tauri/src/island.rs`, `windows/src/island/` | Compact pill at top of screen with hover/click expand | Tauri 2 Win32 transparent window (`WS_EX_TOPMOST`, `WS_EX_TOOLWINDOW`) | Local-only | Transparent window must ignore mouse events when collapsed | **P0** |
| **Expressive Character** | [Observed] 2D canvas face (`Jt`), spring physics | [Observed] Mochi canvas character (Proprietary asset) | **Leo**: Original geometric character with spring-damper gaze & mood dynamics | Custom HTML5 Canvas engine, zero external character assets | Local-only | No camera access; purely programmatic animations | **P0** |
| **System Tray & Settings** | [Inferred] Standard Windows tray icon | [Observed] `windows/src-tauri/src/tray.rs` & `settings.rs` | Tray icon with quick actions, settings modal, hotkey manager | Tauri 2 TrayIcon API & Win32 system notifications | Local-only | Settings saved in LocalAppData, encrypted secrets | **P0** |
| **Universal Push-to-Talk Dictation** | [Observed] `Right Alt` hotkey, paste into active focus | [Observed] `Right Ctrl` hotkey, `sherpa-onnx` Parakeet model | Hold hotkey to talk, release to paste into active app | Low-level hook (`WH_KEYBOARD_LL`) + Pluggable ASR + `SendInput` paste | Local-only (offline) | Requires microphone permission; clipboard restore | **P0** |
| **Speech Cleanup & Formatting** | [Observed] Verbal filler removal, self-corrections, bullet points | [Observed] Claude API clean-up in `flow/` | Local rule-based regex and token transformation pipeline | Deterministic TypeScript text filters (extensible to local SLM) | Local-only | Verbatim mode option ensures code identifiers are never corrupted | **P0** |
| **Local LLM Chat & Inference** | [Observed] Qwen 3.5 (0.8B default to 27B) | [Observed] `local_chat.rs` (Ollama & OpenAI-compat endpoints) | Pluggable local model server client (Ollama/LM Studio/OpenAI API) | HTTP client over localhost loopback with streaming | Local-first, optional cloud | Explicit destination warnings; never send sensitive data silently | **P0** |
| **Coding Agent Hooks & Permissions** | [Observed] None (assistant focused) | [Observed] `windows/src-tauri/src/agents.rs` (Named pipe `coucou-agent-pipe`) | Non-blocking permission card for Claude Code / Codex / Git hooks | Windows Named Pipe server with timeout and fail-open guarantees | Local-only | Prevents external agent hangs; clear Allow/Deny UI | **P1** |
| **Screen Reading & Context** | [Observed] DOM reading & screen QA | [Observed] File drop context in `webview_drop.rs` | Window text extraction with preview before sending to model | Windows UI Automation (UIA) COM APIs + OCR fallback | Local-first | User preview + redaction prevents secret leakage | **P1** |
| **Safe Native Automation** | [Observed] Volume, tabs, dark mode, timers, file search | [Observed] Spotify, window focus | Typed allowlisted tools (timers, volume, safe file reveal) | Rust standard library & Windows Shell APIs (`ShellExecuteExW`) | Local-only | Whitelist only; strictly no arbitrary shell execution | **P0 / P1** |

---

## 2. Explicit "Do Not Copy" Boundaries

1. **Branding & Assets:**
   - **Do NOT copy** Coucou's Mochi artwork, sounds, SVG paths, or trademarks (`LICENSE-ASSETS.md`).
   - **Do NOT copy** HeyNixi's branding, character eyes, logo, or marketing media.
2. **Proprietary Code:**
   - **Do NOT copy** minified JavaScript from `heynixi.com`. All Leo algorithms are cleanroom implementations from mathematical specifications.
3. **Unsafe Practices:**
   - **Do NOT allow** unconfirmed arbitrary shell/PowerShell command execution.
   - **Do NOT allow** background recording without an unmistakable visual indicator.
