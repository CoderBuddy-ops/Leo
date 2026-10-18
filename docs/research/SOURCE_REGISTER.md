# Research Source Register

**Audit Date:** 2026-10-17  
**Auditor:** Leo Principal Engineering Team

---

| Source ID | Source Type | URL / File Path | Date / Commit Hash | Key Claims Supported | Status & Confidence |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **SRC-001** | Live Website | `https://heynixi.com/` | 2026-10-17 | On-device voice assistant claims, Windows 10/11 support, marketing copy | **Observed** (High) |
| **SRC-002** | Shipped Asset | `app.js` (from `heynixi.com/app.js`) | 2026-10-17 | Qwen 3.5 model catalog (`Jf`), bandwidth heuristics (`F0`), canvas character engine (`Jt`), PRNG Mulberry32 | **Observed** (High) |
| **SRC-003** | Shipped Asset | `styles.css` (from `heynixi.com/styles.css`) | 2026-10-17 | Design tokens, color system, pill geometry, typography | **Observed** (High) |
| **SRC-004** | Git Repository | `https://github.com/Louis-CFM/coucou` | Commit `434d6a2b4af2e5ce117fa425170abe25620130b9` (2026-10-16) | Windows companion window, Tauri 2 Rust backend, Named pipe agent hooks, AltGr keyboard safety | **Observed** (High) |
| **SRC-005** | License File | `reference_coucou/LICENSE` | Commit `434d6a2` | Upstream code is MIT licensed to Louis Raillé | **Observed** (High) |
| **SRC-006** | Asset License | `reference_coucou/LICENSE-ASSETS.md` | Commit `434d6a2` | Mochi name, character, art, icons, and sounds are strictly reserved | **Observed** (High) |
| **SRC-007** | Unit Tests | `reference_coucou/windows/tests/` | Node v26 test run (2026-10-17) | 399 unit tests passing verifying FSM, shortcuts, recap, pill navigation | **Observed** (High) |
| **SRC-008** | REA Engine | `npx -y rea-agents@6.4.0` | 2026-10-17 (`jsrg_40786ec32875f21120a6a0dda38da5a4df5f6c8c4c3e3c12c5548fd189b37e58`) | Static JavaScript application graph extraction for `heynixi.com` | **Observed** (High) |
