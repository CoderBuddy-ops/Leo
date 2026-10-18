# Changelog: Leo

All notable changes to **Leo** are documented in this file.

---

## [0.1.0-alpha.6] - 2026-10-18

### Added
- **Official Windows NSIS Installer (`Leo-Setup-x64.exe`):**
  - Generated lean, official 4.05 MB Windows installer via Tauri 2 and NSIS 3.13 (`Leo_0.1.0_x64-setup.exe`).
  - Standard per-user installation into `%LOCALAPPDATA%\Programs\Leo` with Start Menu shortcut, Desktop shortcut, and Windows uninstaller metadata.
  - Staged to `C:\Users\Yuvraj\Desktop\Leo-Setup-x64.exe` and `C:\Users\Yuvraj\Desktop\Leo-Release\Leo-Setup-x64.exe`.
- **Google Gemini Online AI Client (`src/models/gemini_client.ts`):**
  - Direct SSE streaming client supporting `gemini-2.5-flash`, `gemini-2.5-pro`, and `gemini-1.5-flash`.
  - Secure credential handling: user API key stored locally in client storage, never logged or committed to Git.
  - Automated secret redaction (`redactSecret()`) ensuring API keys are scrubbed from all error traces.
- **Unified AI Provider Architecture (`src/models/ai_provider.ts`):**
  - Supports Strict Offline Mode (blocks all cloud egress), Local Ollama, Local LM Studio, and Google Gemini.
  - Real-time privacy badges: `Local` (green dot) vs `[Leaves your PC]` (amber warning badge).
  - Integrated usage metrics dashboard displaying active model, session query counts, token usage, and privacy status.
- **Canonical Release & Privacy Documentation:**
  - `docs/release/BUILD_AND_INSTALL.md`: comprehensive build, install, uninstall, and size audit guide.
  - `docs/security/PRIVACY.md`: strict zero audio egress invariants, key protection, and data flow architecture.
- **Automated Test Expansion:**
  - Added `tests/leo_providers.test.mjs` (6/6 passing tests).
  - Total automated test suite: **31/31 passing tests (100%)** across all 4 test suites.

### Changed
- **Leo Icon V2 Refinement:**
  - Preserved canonical backup at `assets/brand/leo-face-icon-1024.v2-original.png`.
  - Removed mouth stroke from character face in `scripts/generate-leo-icon.mjs`.
  - Added subtle, symmetrical soft pink blush marks on each side of the face (`PALETTE.blush = [250, 128, 146]`).
  - Regenerated all Windows `.ico` sizes (256/128/64/48/32/16), PNG icons (512, 256, 128, 32), and web favicon.
- **Definitive Fix for `ERR_CONNECTION_REFUSED` & Terminal Window:**
  - Traced and eliminated legacy `scripts/launch-leo.bat` and `launch-leo.vbs` which launched Chrome `--app="http://localhost:1420"` and spawned console windows.
  - Re-pointed desktop shortcut `C:\Users\Yuvraj\Desktop\Leo.lnk` directly to native `Leo.exe`.
  - Verified production build embeds static assets (`frontendDist: "../dist"`), operating completely offline with zero localhost dependency.
- **Fluidity & G² Curvature Styling:**
  - Enhanced capsule styling with curvature-continuous squircle silhouette (`border-radius: 36px`), multi-layer glassmorphism blur, copper accretion highlights, and smooth state transitions.

## [0.1.0-alpha.5] - 2026-10-18

### Added
- **UI/UX Pro Max Design Intelligence (`https://uupm.cc/`):** Installed `ui-ux-pro-max-skill` v2.15.0 via official installer into `.agents/skills/` (exposing 7 design skills: `ui-ux-pro-max`, `brand`, `design`, `design-system`, `ui-styling`, `banner-design`, `slides`) and global user registry; verified local Python 3 search execution for dark OLED styles, amber companion palettes, and motion accessibility.
- **Development Extensions:** Installed essential IDE extensions via VS Code CLI: `rust-lang.rust-analyzer` (v0.3.3081), `tauri-apps.tauri-vscode` (v0.2.9), `bradlc.vscode-tailwindcss` (v0.16.0), `dbaeumer.vscode-eslint` (v3.0.34), and `esbenp.prettier-vscode` (v12.4.0).
- **Native Windows Desktop Release (`Leo.exe`):**
  - Resolved native compilation sysroot using verified WinLibs MinGW GCC 16.2.0 toolchain (`gcc.exe`, `ld.exe`, `as.exe`, `windres.exe`, `ar.exe`, `crtbegin.o`, `crtend.o`, `libkernel32.a`).
  - Resolved MinGW GNU 64k PE export ordinal overflow (`export ordinal too large: 96929`) by configuring `crate-type = ["staticlib", "rlib"]` for desktop target in `app/src-tauri/Cargo.toml`.
  - Compiled optimized production release binary `leo.exe` (22.7 MB).
  - Staged runnable production package to `C:\Users\Yuvraj\Desktop\Leo-Release\Leo.exe` alongside `WebView2Loader.dll`.
  - Updated desktop shortcut `C:\Users\Yuvraj\Desktop\Leo.lnk` with valid target, working directory, and black-hole character icon.
  - Verified live runtime execution: `Leo.exe` (PID 1444, Responding=True) and child `msedgewebview2.exe` (PID 14144, Responding=True).
- Automated test suite pass rate: **25/25 passing tests (100%)** (7 P0 + 10 extended + 8 icon integrity).

## [0.1.0-alpha.4] - 2026-10-18

### Changed
- Enforced strict root transparency in `app/index.html` with explicit `html, body { background: transparent !important; background-color: transparent !important; }` to eliminate WebView2 default opaque white canvas backdrop.
- Enhanced Win32 window configuration in `app/src-tauri/src/lib.rs` to explicitly preserve `WS_EX_LAYERED`, apply `SetWindowPos` with `SWP_FRAMECHANGED`, and safely gate `WDA_EXCLUDEFROMCAPTURE` to protect DWM per-pixel alpha composition.
- Formulated parallel agent task contract in `docs/research/FREEBUFF_TASK.md` for read-only audit delegation to Freebuff Desktop.
- Updated `docs/research/TOOLING_INVENTORY.md` with verified Rust 1.99.1, Freebuff Desktop orchestrator (port 8787), Claude Code, Auggie, and detailed linker sysroot diagnostic evidence.

## [0.1.0-alpha.3] - 2026-10-18

## [0.1.0-alpha.2] - 2026-10-18

### Added
- Local Voice Activity Detector (`src/voice/audio_vad.ts`) calculating RMS energy and Zero Crossing Rate with silence hangover filtering.
- Offline ASR pipeline manager (`src/voice/offline_asr.ts`) enforcing affirmative user consent, download progress tracking, and integration with `SpeechCleanupFilter`.
- Strongly typed, bounded IPC bridge (`src/ipc/leo_ipc.ts`) with strict parameter validation and in-memory development fallback.
- Companion gesture controller (`src/companion/gestures.ts`) managing organic mood transitions and inactivity sleep.
- Interactive floating companion capsule UI (`app/index.html`, `app/src/main.ts`) with live animated digital face, push-to-talk dictation, audio level meter, prompt streaming drawer, and dynamic network egress badge.
- Win32 native window configurations in Rust backend (`app/src-tauri/src/lib.rs`): `WS_EX_NOACTIVATE` (preventing focus stealing), `WDA_EXCLUDEFROMCAPTURE` (excluding companion from screen capture), and DPI-aware monitor positioning.
- Extended automated unit test suite (`tests/leo_extended.test.mjs`) with 10/10 passing tests (17/17 tests passing overall).

## [0.1.0-alpha.1] - 2026-10-17

### Added
- Completed Phase 1 research audits: HeyNixi (`HEYNIXI_AUDIT.md`), Coucou Windows (`COUCOU_AUDIT.md`), and legal licensing (`LICENSING_AND_ASSET_AUDIT.md`).
- Established foundational ADRs (0001 through 0005) covering windowing, speech pipeline, model abstraction, safe automation, and screen context.
- Implemented original Leo 2D canvas character engine (`src/companion/leo_face.ts`) with substepped spring-damper dynamics and 9 emotional states.
- Implemented deterministic Mulberry32 PRNG with golden vector test validation.
- Implemented speech cleanup filter (`src/voice/speech_cleanup.ts`) removing verbal fillers and formatting lists.
- Implemented local model client (`src/models/local_client.ts`) supporting loopback endpoints (Ollama / LM Studio).
- Created Antigravity rules under `.agents/rules/` with valid YAML frontmatter.
- Created Leo unit test suite (`tests/leo_p0.test.mjs`) passing 7/7 checks.
