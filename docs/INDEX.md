# Documentation Index: Leo

**Product:** Leo — Personal AI Companion for Windows 10/11 x64  
**Last Updated:** 2026-10-19  
**Status:** Approved Canonical Index  

---

## 1. Product Requirements & Backlog
- [`PRD.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/PRD.md) — Canonical Product Requirements Document (Personas, JTBD, FR/NFRs, and 13 Ralph Loop Tasks).
- [`TASKS.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/TASKS.md) — Human-readable task dashboard linked to PRD.md and progress.txt.
- [`progress.txt`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/progress.txt) — Persistent, append-only Ralph iteration log.
- [`prompt.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/prompt.md) — Per-iteration contract for Ralph Loop execution.

---

## 2. Engineering & Operation
- [`AGENTS.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/AGENTS.md) — Canonical agent operating rules, safety invariants, and Ponytail minimal-solution ladder.
- [`SETUP.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/development/SETUP.md) — Environment prerequisites, MinGW GCC 16.2.0 toolchain, dev workflow, and test runner.
- [`RELEASE.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/release/RELEASE.md) — Packaging architecture, NSIS installer wizard, standalone binary, and clean-machine verification.
- [`CLEANUP_REPORT.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/maintenance/CLEANUP_REPORT.md) — Measured disk space cleanup and documentation consolidation audit.

---

## 3. Architecture & Security
- [`ARCHITECTURE.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/architecture/ARCHITECTURE.md) — Two-window webview model, native shell, system tray, and IPC bridge.
- [`SECURITY.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/security/SECURITY.md) — Zero raw audio egress, Gemini API key masking, screen capture exclusion (`WDA_EXCLUDEFROMCAPTURE`).
- [`COMPANION_AND_MOTION.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/design/COMPANION_AND_MOTION.md) — 2D canvas character dynamics, spring-damper equations, and reduced-motion policy.

---

## 4. Architecture Decision Records (ADRs)
- [`ADR 0001: Foundation Strategy`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/decisions/0001-foundation.md) — Cleanroom architecture and hybrid Tauri 2 foundation.
- [`ADR 0002: Speech Pipeline`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/decisions/0002-speech-pipeline.md) — Offline ASR strategy, local VAD, and clipboard restoration.
- [`ADR 0003: AI Provider Strategy`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/decisions/0003-ai-provider-strategy.md) — Unified provider interface, local loopback, and Gemini SSE streaming.
- [`ADR 0004: Safe Automation`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/decisions/0004-safe-automation.md) — Three-tier risk boundaries and shell injection prevention.
- [`ADR 0005: Screen Context`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/decisions/0005-screen-context.md) — Window capture exclusion and UI Automation safeguards.

---

## 5. Licensing & Legal Compliance
- [`THIRD_PARTY_NOTICES.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/THIRD_PARTY_NOTICES.md) — MIT licenses, upstream attributions, and cleanroom separation.

---

## 6. Historical Reference & Cleanroom Audits
Historical cleanroom audit documents are archived in [`docs/archive/`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/archive/):
- `docs/archive/COUCOU_AUDIT.md`
- `docs/archive/HEYNIXI_AUDIT.md`
- `docs/archive/REFERENCE_COMPARISON.md`
- `docs/archive/LICENSING_AND_ASSET_AUDIT.md`
- `docs/archive/SOURCE_REGISTER.md`
- `docs/archive/TOOLING_INVENTORY.md`
