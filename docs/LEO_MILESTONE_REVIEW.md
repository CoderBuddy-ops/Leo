# Leo milestone review — based on the submitted Antigravity logs

**Review date:** 2026-10-17  
**Basis:** The two pasted Antigravity logs supplied in this conversation. The underlying `C:\Users\Yuvraj\Desktop\Project\nixi` checkout was not available in this review environment, so source files and reported test runs were not independently re-executed here.

## Current state inferred from the logs

The report says the workspace contains:

- Research notes for HeyNixi, Coucou, tools and licensing.
- PRD, MVP scope, architecture, data flows, five ADRs, motion design, threat model and network policy.
- `src/companion/leo_face.ts`, `src/voice/speech_cleanup.ts` and `src/models/local_client.ts`.
- `tests/leo_p0.test.mjs`, reported as 7/7 passing.
- A cloned Coucou reference checkout, reported at `434d6a2b4af2e5ce117fa425170abe25620130b9`, with 399 tests reported as passing.

These are **reported outcomes**, not independently verified outcomes in this review. The logs indicate a useful prototype and document foundation, but do not establish a completed runnable Windows application.

## Priority findings

### P0 — Correct the PRNG identity

The HeyNixi function quoted in the report increments state by `1831565813` (`0x6D2B79F5`) and uses the multiply/xor mixing operations associated with Mulberry32. Calling it “SplitMix32” is inaccurate. Correct the reference audit, Leo implementation comments and tests. A valid test suite should check deterministic golden output for a fixed seed, not merely equality between two generators initialized identically.

### P0 — Verify Antigravity rule syntax

Antigravity's current rules documentation states that each Markdown file under `.agents/rules/` needs YAML frontmatter with a supported `trigger`; invalid or missing frontmatter can cause the rule to be ignored. Verify every rule file. Root `AGENTS.md` and `GEMINI.md` use a different convention and do not need the same frontmatter. Source: https://antigravity.google/docs/rules/

### P0 — Native app and vertical slice remain unverified

The report itself says Rust/Cargo are missing and `leo_face.ts` is not yet connected to the native transparent window. Accordingly, phases described as “executed” should be interpreted as reported research/docs/prototype progress, not proof that Phase 4 (buildable Windows baseline) or Phase 5 (end-to-end vertical slice) exit criteria are met.

### P1 — Check the document set against the full master prompt

The reported document matrix is not complete relative to Section 3 of the master prompt. The actual checkout needs a coverage check for missing product, design, AI, security, operational, development, testing and release documents. Specifically verify the existence and substantive quality of files such as `AGENTS.md`, `CONTRIBUTING.md`, `CHANGELOG.md`, `SECURITY.md`, `docs/product/USER_FLOWS.md`, `REQUIREMENTS_TRACEABILITY.md`, `docs/architecture/IPC_CONTRACTS.md`, `STATE_MACHINES.md`, `PROJECT_STRUCTURE.md`, `docs/design/DESIGN_SYSTEM.md`, `ACCESSIBILITY.md`, all `docs/ai/*` documents, and the full operations/testing/release documents. Do not create empty boilerplate merely to tick boxes.

### P1 — Make test reporting reproducible

The log shows test commands joined by semicolons. This can allow later commands to run even when earlier commands fail, and does not itself calculate an aggregate success state. Run each test command separately, record each exit status and output, and verify the actual glob includes the expected files. The totals “406 passed, 0 failed” remain unverified until raw test output or a fresh run supports them.

### P1 — Protect the reference clone from accidental inclusion

If `reference_coucou/` is a nested Git repository within Leo's root, ensure it cannot accidentally be staged as a gitlink or ship as part of Leo's product. Keep the upstream source as an ignored research checkout or choose an explicit submodule strategy. Preserve the MIT notices and the separate reserved-assets policy; do not copy Coucou/Mochi branding, character art, sounds or media.

### P1 — Separate research claims from executable functionality

A `speech_cleanup.ts` helper is text transformation, not speech recognition. A model client is not a functioning chat UI until integrated and tested against the expected API. A character class is not a native animated overlay until it is wired into a real application window. Update feature status to distinguish implemented-and-tested, implemented-but-unverified, scaffolded, planned and blocked.

## Recommended order

1. Verify Git/workspace state and the report's claimed files.
2. Correct PRNG naming and add golden-vector tests.
3. Validate Antigravity rule frontmatter and documentation coverage.
4. Rerun tests one command at a time with checked exit statuses.
5. Unblock Rust/Tauri prerequisites safely; verify MSVC and WebView2 requirements.
6. Build and launch an original Leo shell with transparent/topmost pill, tray entry and correct focus behavior.
7. Integrate the character engine and local chat end to end.
8. Choose and verify a real ASR engine before presenting dictation as working.
9. Complete separate security/privacy/license and accessibility reviews.
10. Update docs, task ledger, changelog and handoff status based on observed evidence.

## External verification links

- Antigravity Rules: https://antigravity.google/docs/rules/
- Antigravity Skills: https://antigravity.google/docs/skills
- Coucou repository and Windows implementation documentation: https://github.com/Louis-CFM/coucou

The current Coucou README describes its Windows implementation as Tauri 2 (Rust + TypeScript), with details in `windows/README.md`; it is useful reference documentation, not proof that Leo has adopted or integrated it. Source: https://github.com/louis-cfm/coucou
