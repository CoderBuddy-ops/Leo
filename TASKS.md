# Master Task Backlog: Leo

**Status:** Active  
**Last Updated:** 2026-10-17

---

## Phase 1 & 2: Research, Licensing & Foundation (Completed)

- [x] `TSK-001`: Inspect workspace, discover REA and development toolchains.
- [x] `TSK-002`: Clone and audit Coucou Windows reference (`434d6a2b4af2e5ce117fa425170abe25620130b9`).
- [x] `TSK-003`: Run REA static analysis on HeyNixi (`app.js`, `styles.css`) and extract model/physics heuristics.
- [x] `TSK-004`: Complete legal asset audit (`LICENSE-ASSETS.md`) and establish cleanroom rules.
- [x] `TSK-005`: Author foundational ADRs (0001 to 0005) and core specifications (PRD, MVP Scope, Architecture, Threat Model, Network Policy).
- [x] `TSK-006`: Execute Coucou Windows test suite (`npm test`) and achieve 399/399 passing tests.

---

## Phase 3: Documentation Matrix & Specification (Completed)

- [x] `TSK-010`: Create `docs/INDEX.md` documentation registry.
- [x] `TSK-011`: Create `docs/research/TOOLING_INVENTORY.md`.
- [x] `TSK-012`: Create `docs/research/HEYNIXI_AUDIT.md`.
- [x] `TSK-013`: Create `docs/research/COUCOU_AUDIT.md`.
- [x] `TSK-014`: Create `docs/research/REFERENCE_COMPARISON.md`.
- [x] `TSK-015`: Create `docs/research/LICENSING_AND_ASSET_AUDIT.md`.
- [x] `TSK-016`: Create `docs/research/SOURCE_REGISTER.md`.
- [x] `TSK-017`: Create `docs/design/COMPANION_AND_MOTION.md`.
- [x] `TSK-018`: Create `docs/security/THREAT_MODEL.md`.
- [x] `TSK-019`: Create `docs/security/NETWORK_POLICY.md`.
- [x] `TSK-020`: Create `.agents/rules/00-project-invariants.md`.

---

## Phase 4 & 5: Implementation & First Vertical Slice (In Progress)

- [x] `TSK-030`: Implement Leo's original procedural character engine (`src/companion/leo_face.ts`) with spring-damper dynamics and 9 mood states.
- [x] `TSK-031`: Implement Leo's speech transcript cleanup filter (`src/voice/speech_cleanup.ts`) removing verbal fillers, detecting spoken corrections, and formatting bullet lists.
- [x] `TSK-032`: Implement local model client (`src/models/local_client.ts`) supporting OpenAI-compatible streaming endpoints (Ollama/LM Studio) with cancellation.
- [x] `TSK-033`: Add automated unit tests for Leo character engine, speech cleanup filter, and local model client.
- [x] `TSK-034`: Execute tests and verify 100% pass rate (7/7 P0 tests passing).
- [x] `TSK-036`: Implement local Voice Activity Detector (`src/voice/audio_vad.ts`) with RMS energy, ZCR, and silence hangover filtering.
- [x] `TSK-037`: Implement offline speech recognition pipeline (`src/voice/offline_asr.ts`) with explicit user consent, progress tracking, and transcript cleanup integration.
- [x] `TSK-038`: Implement strongly-typed IPC bridge (`src/ipc/leo_ipc.ts`) with strict parameter boundary validation and dev fallback.
- [x] `TSK-039`: Implement companion gesture and auto-sleep controller (`src/companion/gestures.ts`).
- [x] `TSK-040`: Build interactive floating capsule UI in `app/` (`app/index.html`, `app/src/main.ts`) with live digital face, audio meter, dictation bar, prompt drawer, and cloud egress warning.
- [x] `TSK-041`: Execute extended test suite (`tests/leo_extended.test.mjs`) with 10/10 passing tests (17/17 tests passing overall).
- [x] `TSK-043`: Extract and configure canonical black-hole brand icon assets (`assets/brand/leo-icon-1024.png`, `app/src-tauri/icons/icon.ico`, `app/public/favicon.png`).
- [x] `TSK-044`: Refine design language: deep-space/charcoal foundation, copper accretion highlights, warm cream typography, and accessible cyan focus indicators.
- [x] `TSK-045`: Enhance Leo character dynamics: subtle breathing float, physical squash-and-stretch interaction bounce, crescent happy eyes (`⌒ ⌒`), and `prefers-reduced-motion` support.
- [x] `TSK-046`: Author canonical agent-agnostic operating guides (`AGENTS.md`, `CLAUDE.md`) for cross-agent interoperability.
- [x] `TSK-047`: Create standalone Windows desktop launcher and shortcut (`C:\Users\Yuvraj\Desktop\Leo.lnk`) with black-hole application icon, silent VBS wrapper, and top-center capsule placement.
- [x] `TSK-048`: Design and generate the circular professional Leo icon from a real idle frame of the facial animation engine (`scripts/generate-leo-icon.mjs`); wire into native Tauri icon set, favicon, and brand assets with structural + determinism test coverage (25/25 tests passing).
- [x] `TSK-049`: Fix WebView2 root canvas white background symptom in `app/index.html` (explicit `html, body { background: transparent !important; }`), configure Win32 `SWP_FRAMECHANGED` and `WS_EX_LAYERED` in `app/src-tauri/src/lib.rs`, and author `docs/research/FREEBUFF_TASK.md` for parallel Freebuff coordination.
- [x] `TSK-042`: Resolve native Windows compilation toolchain for `leo.exe`:
  - Discovered full local WinLibs MinGW GCC 16.2.0 toolchain (`gcc.exe`, `as.exe`, `ld.exe`, `windres.exe`, `ar.exe`, `crtbegin.o`, `crtend.o`, `libkernel32.a`) in user WinGet packages (`C:\Users\Yuvraj\AppData\Local\Microsoft\WinGet\Packages\BrechtSanders.WinLibs.POSIX.MSVCRT_Microsoft.Winget.Source_8wekyb3d8bbwe\mingw64`).
  - Configured `app/src-tauri/.cargo/config.toml` with WinLibs GCC linker and ar binaries.
  - Verified compilation of test binary (`hello.exe`) and successful `cargo check` across all 100+ native Tauri dependencies in 2m 10s.
- [x] `TSK-050`: Install and verify UI/UX Pro Max design intelligence skill (`https://uupm.cc/`, `ui-ux-pro-max` v2.15.0) via official installer into `.agents/skills/` (7 design skills: `ui-ux-pro-max`, `brand`, `design`, `design-system`, `ui-styling`, `banner-design`, `slides`); verified local Python 3 queries for dark companion palettes, OLED styles, and motion accessibility.
- [x] `TSK-051`: Install essential IDE development extensions (`rust-lang.rust-analyzer`, `tauri-apps.tauri-vscode`, `bradlc.vscode-tailwindcss`, `dbaeumer.vscode-eslint`, `esbenp.prettier-vscode`).
- [x] `TSK-052`: Refine Leo Icon V2:
  - Preserved original master backup at `assets/brand/leo-face-icon-1024.v2-original.png`.
  - Removed mouth stroke and added symmetrical, subtle soft pink blush marks beside eyes in `scripts/generate-leo-icon.mjs`.
  - Regenerated all Windows `.ico` sizes (256/128/64/48/32/16), PNG icons (512, 256, 128, 32), and web favicon; 8/8 icon determinism tests passing.
- [x] `TSK-053`: Fix `ERR_CONNECTION_REFUSED`, terminal window, and localhost dependency:
  - Removed Chrome `--app=localhost:1420` legacy launcher script.
  - Re-pointed desktop shortcut `C:\Users\Yuvraj\Desktop\Leo.lnk` directly to native `Leo.exe`.
  - Verified bundled static frontend (`frontendDist: "../dist"`), zero localhost dependency, and GUI subsystem (`#pragma windows_subsystem = "windows"`).
- [x] `TSK-054`: Build official Windows NSIS Installer:
  - Packaged `Leo_0.1.0_x64-setup.exe` (4.05 MB) via Tauri CLI + NSIS 3.13.
  - Staged to `C:\Users\Yuvraj\Desktop\Leo-Setup-x64.exe` and `C:\Users\Yuvraj\Desktop\Leo-Release\Leo-Setup-x64.exe`.
- [x] `TSK-055`: Implement Google Gemini Online AI & Unified Provider Architecture:
  - Implemented SSE streaming client in `src/models/gemini_client.ts` with API key masking and secret redaction.
  - Implemented `src/models/ai_provider.ts` supporting Strict Offline Mode, Local Ollama, LM Studio, and Google Gemini with usage reporting and privacy badges.
  - Added unit test suite `tests/leo_providers.test.mjs` (6/6 passing).
- [x] `TSK-056`: Author canonical release and privacy documentation:
  - `docs/release/BUILD_AND_INSTALL.md`
  - `docs/security/PRIVACY.md`

---

## Verification & Test Metrics (100% Passing)

- **Total Unit Tests:** **31 / 31 passing (100%)**
  - `tests/leo_p0.test.mjs`: 7/7 passing
  - `tests/leo_extended.test.mjs`: 10/10 passing
  - `tests/leo_icon.test.mjs`: 8/8 passing
  - `tests/leo_providers.test.mjs`: 6/6 passing
- **Frontend Build:** `npm run build` succeeds in 320ms (33.5 KB bundle).
- **Native Release Installer:** `Leo-Setup-x64.exe` (4.05 MB).
- **Standalone Package:** `C:\Users\Yuvraj\Desktop\Leo-Release\Leo.exe` (22.8 MB) with `WebView2Loader.dll`.

