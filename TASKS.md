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

---

## Next Up (Prioritized Checklist — 2026-10-18)

1. **P0** — Resolve native C/C++ linker sysroot (awaiting user approval for Option 1 portable WinLibs vs Option 2 VS Build Tools).
2. **P0** — Execute `cargo build --release` in `app/src-tauri` to produce native Windows `Leo.exe`.
3. **P0** — Launch native `Leo.exe` and verify DWM transparency + always-on-top over active applications.
4. **P0** — Deliver runnable package to `C:\Users\Yuvraj\Desktop\Leo-Release\`.

