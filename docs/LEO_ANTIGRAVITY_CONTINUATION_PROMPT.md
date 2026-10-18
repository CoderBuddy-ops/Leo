# Leo — Antigravity Continuation Prompt

> Use this in the existing `nixi` / Leo workspace. This prompt continues from the reported milestone; it is not a request to recreate the project from scratch.

## Mission

Resume implementation of **Leo**, the personal Windows 10/11 x64 AI companion, from the current repository state. Read `LEO_ANTIGRAVITY_MASTER_PROMPT.md`, all active `AGENTS.md`/`GEMINI.md` files and valid Antigravity rules, then inspect the actual repository. Continue through review, buildability, vertical-slice integration, and handoff. Do not stop after presenting another plan.

The prior session report claims that the following were created: a set of research/product/architecture/security docs; ADRs 0001–0005; `src/companion/leo_face.ts`, `src/voice/speech_cleanup.ts`, `src/models/local_client.ts`; `tests/leo_p0.test.mjs`; and a local `reference_coucou/` clone. It reports 7 Leo tests and 399 Coucou Windows tests passing. **Treat these as claims to verify against the current checkout, not as facts until you inspect the files and rerun checks.** Do not re-create files that already exist or overwrite unrelated user changes.

## 1. Establish actual repository state

1. Run `git status --short --branch`, `git diff --stat`, and `git diff --` before editing. Record the branch and current HEAD. Do not clean, reset, checkout, or delete user files.
2. Confirm the workspace root and the exact paths of the prompt, docs, source, tests, and upstream reference checkout. If the existing `reference_coucou/` is a nested Git repository, make sure it cannot accidentally be added as a gitlink or product source. Keep it ignored or store source references elsewhere unless the owner deliberately chooses a submodule.
3. Inspect package manifests, lockfiles, actual Tauri config, source imports, build scripts and tests before inventing commands. Inspect current files rather than relying on the earlier status text.
4. Discover skills and MCP tools available in this Antigravity environment. Search for **REA / Reverse Engineer Anything** and read the actual skill instructions if installed. Use relevant newly installed skills and MCPs only after inspecting their documented capabilities. Use genuine subagents if this surface supports them. Assign non-overlapping workstreams; if unsupported, perform the same reviews sequentially and report that limitation. Never make up skill names, agent runs, MCPs, or tool results.
5. Re-check the current upstream Coucou Windows code and its current commit if network access is available; keep the previously reported commit `434d6a2b4af2e5ce117fa425170abe25620130b9` as the previous inspected baseline, not automatically as `main`'s current HEAD. Preserve source/license notices and do not copy reserved Coucou/Mochi or HeyNixi branding, artwork, icons, sound or media.

## 2. Correct known research and rule-format issues

### 2.1 PRNG finding

The HeyNixi code reproduced in the previous report uses the increment constant `1831565813` (`0x6D2B79F5`) and the multiply/xor mixing pattern associated with **Mulberry32**, not canonical SplitMix32. Correct the research document(s), code comments, symbol names, and tests. If Leo intentionally uses a different PRNG, name it after the exact implementation and document that choice rather than claiming it is identical to HeyNixi. Add deterministic golden-vector tests for a fixed seed, range tests, and a test that would fail if the implementation's operations or constants change. Avoid asserting algorithm identity from a test that checks only that two sequences match.

### 2.2 Antigravity rule discovery

Audit every Markdown file in `.agents/rules/`. According to current Antigravity rule guidance, each file in that directory needs valid YAML frontmatter with a supported `trigger` value (`always_on`, `model_decision`, `glob`, or `manual`); model-decision rules also need an appropriate description. Invalid/missing frontmatter can cause a rule to be silently ignored. Validate this for all current rule files. `AGENTS.md`/`GEMINI.md` are a different always-active mechanism and do not use that frontmatter. Do not add contradictory root instruction files unnecessarily.

### 2.3 Evidence integrity

For each important reference claim, preserve the label `Observed`, `Documented`, `Inferred`, or `Unknown / Needs test`. Correct any overclaim that website bundle analysis establishes behavior of a private installed application. Keep the REA session ID as a provenance pointer, but record which conclusions came from its output and which were independently verified. Never say a model file is bundled, inference is entirely offline, a hotkey is registered globally, or a permission flow works unless evidence supports that specific claim.

## 3. Audit documentation coverage before adding files

Compare the actual repository against every required path in Section 3 of `LEO_ANTIGRAVITY_MASTER_PROMPT.md`. The last report's shortened document matrix is **not evidence of complete coverage**. Create a checklist that distinguishes `present and reviewed`, `present but stale/incomplete`, `missing`, and `not applicable with reason`.

At minimum inspect coverage for:

- Root: `README.md`, `AGENTS.md`, `CONTRIBUTING.md`, `CHANGELOG.md`, `LICENSE`, `THIRD_PARTY_NOTICES.md`, `SECURITY.md`, `TASKS.md`, `.gitignore`, `.editorconfig` and real formatter/lint/build configurations.
- Rules/skills: six modular project rules with valid format; narrow reusable Leo skills only if they add real value. Do not invent a local REA skill.
- Product: `docs/INDEX.md`, `docs/product/PRD.md`, `MVP_SCOPE.md`, `USER_FLOWS.md`, `COMPETITIVE_ANALYSIS.md`, `REQUIREMENTS_TRACEABILITY.md`.
- Research: tooling inventory, HeyNixi audit, Coucou audit, comparison, licensing/asset audit and dated source register.
- Architecture: architecture, data flows, IPC contracts, state machines, project structure and ADRs for the significant decisions.
- Design: design system, companion/motion, screen specifications and accessibility.
- AI: provider strategy, speech pipeline, tool calling/automation, screen context, memory design and model evaluation.
- Security: threat model, privacy, permissions, secrets/logging, safe automation, network policy and dependency policy.
- Operations/development/testing/release/memory: failure modes, troubleshooting, memory maintenance, dev setup, exact commands, test strategy, acceptance tests, build/release and roadmap/task breakdown.

Create or repair only what is needed. Avoid boilerplate: where a decision has not been made, say `Pending decision`, identify options and create a task/ADR instead of inventing a settled architecture. Update `docs/INDEX.md` to link to real files and mark their state.

## 4. Make the current code honest and testable

Review the three reported modules and test suite line by line:

- **Leo character engine:** validate finite-state/mood transitions, numerical stability under large/negative/invalid delta times, deterministic motion, reduced-motion behavior, idle performance and accessible state announcements. Keep the character original. Do not draw it into the OS overlay and claim this works until an actual host integration exists.
- **Speech cleanup:** add boundary cases for names, code, paths, URLs, acronyms, punctuation, non-English text, verbatim mode, false corrections, repeated words and already-formatted lists. Keep transformation optional and separate from raw transcript. Do not claim actual speech recognition exists just because text cleanup works.
- **Local model client:** parse HTTP errors and streaming chunks robustly, handle partial UTF-8/SSE chunks, server-not-running, malformed JSON, timeout, abort/cancel and non-2xx responses, and enforce a documented localhost/provider policy. Test that untrusted or remote endpoints cannot bypass explicit configuration. Do not log prompts, API keys, headers, or response content in diagnostics.
- **Unit test truthfulness:** run every intended test command separately, capture each exit status and raw summary, and fail the verification step if any command fails. Do not report a combined pass count from commands separated by `;` without checking each status. Confirm the test glob actually includes all intended files.
- **Runtime status:** distinguish the pure TypeScript/Node prototype from a usable Windows desktop app. No real microphone, ASR, Tauri IPC, tray, native transparent window, dictation insertion, model endpoint, file context or screen capture is complete until it is wired and exercised end to end.

## 5. Remove the native build blocker safely

The prior host report says `rustc`, `cargo` and Tauri CLI were not on `PATH`. Confirm the current state first, including standard install locations and whether `cargo` is installed but missing from `PATH`. Inspect actual `windows/` prerequisites, including MSVC Build Tools and WebView2 if the chosen Tauri configuration requires them.

Do not install system toolchains, make machine-wide policy changes or invoke elevated installation silently. If an install or reboot is needed, give the user a precise command/source and request approval where the environment requires it. **Do not stop all project work at this blocker:** complete build scripts, static checks, docs, TypeScript tests and integration scaffolding that can be validated without Rust; mark native build/launch explicitly `Blocked` until verified. Avoid pretending an installer exists.

After the toolchain is present and the user authorizes installation if necessary:

1. Pin/document versions and verify `rustc --version`, `cargo --version`, `node --version`, `npm --version`, Tauri version, MSVC and WebView2 as appropriate.
2. Run the exact repository install, formatter, lint/type-check, test, and `tauri dev`/build commands obtained from real manifests and docs.
3. Fix build errors iteratively. Do not add dependencies without a provenance/license check and an ADR/update when the dependency materially affects the architecture.

## 6. Prioritize one genuine Windows vertical slice

Prefer selective reuse of verified Coucou Windows patterns over a blind fork. Keep Leo's identity and source changes distinct; preserve required MIT notices and the explicit reserved-assets policy. Review actual current upstream code before choosing what to adapt.

Implement and validate in this order:

1. Buildable Tauri 2 + Rust + TypeScript Windows shell with Leo app identity and an original minimal UI.
2. Compact transparent/topmost window and tray entry; verify it does not steal focus, handles screen bounds/multi-monitor/DPI changes, and degrades cleanly. Wire `leo_face.ts` into this actual window instead of treating a standalone class test as UI integration.
3. Expand/collapse, settings open/close, typed UI state and visible idle/listening/thinking/error states. Check reduced motion and keyboard access.
4. Local text chat against an explicitly configured local model server, including not-configured/offline state, streamed response, abort and errors. No fake responses presented as real AI.
5. Dictation only after choosing and verifying a real Windows-compatible ASR engine. Evaluate candidate projects for Windows support, current maintenance, language/model requirements, license, download size and offline behavior. Keep an honest disabled/not-configured state while blocked. Model download must be explicit, cancellable, integrity-checked and documented.
6. Only after P0 is stable, add native text insertion/paste with exact clipboard restoration, configurable hotkeys, safe timers/media tools and then screen context/agent hooks. Each feature needs a small task, typed contract, permission behavior, negative/error tests and documentation.

## 7. Security, privacy and licenses

Before enabling voice, screenshot/window context, file access, persistent memory, coding-agent hooks or native automation:

- Read/reconcile the threat model and permissions table; validate each IPC payload and each adapter boundary.
- Keep telemetry off by default and local-only model flows on-device. Explicitly disclose any configured cloud provider and the exact categories of data it receives. Do not assume local inference just because an endpoint looks like localhost; define the allowed endpoint and redirect behavior.
- Use an OS secret store for keys. Never put secrets in TS bundles, docs, test fixtures, reports or logs.
- Never run arbitrary shell commands from model output, silently modify coding-agent hooks, silently enable auto-start or silently record microphone/screen content.
- Preserve upstream code licenses and add per-dependency notices. Do not reuse reserved Coucou/Mochi/HeyNixi assets or names. A LICENSE file for Leo's own code is a separate project-owner choice; if not specified, mark it `Pending owner decision` rather than choosing silently.

## 8. Subagent review and integration

Use available specialized subagents with non-overlapping review targets:

- Research/docs consistency and claims/evidence.
- Native Windows/Tauri build and overlay integration.
- Voice/model pipeline and test correctness.
- Security/privacy/license review.

Ask reviewers to return severity, exact file/symbol, evidence, impact, and concrete fix. Main agent owns reconciliation and edits. Run a final consistency review of requirements ↔ modules ↔ tasks ↔ tests. Use the actual tools and skills this environment exposes; report unavailable ones plainly.

## 9. Definition of done for this continuation

Do not label the MVP complete until the real Windows app builds and launches on Windows and the complete P0 flow has been exercised. If the machine cannot build it, report partial completion clearly.

Before handoff:

1. Run formatter, lint/type-check, all unit tests, the relevant integration tests and the native build if available.
2. Inspect `git diff --check`, `git diff --stat`, the full diff and status. Confirm no secrets, generated installer, model weights, user files, reserved brand assets or nested Git repository were accidentally included.
3. Update task statuses, docs index, requirements traceability, commands, changelog and blocker notes.
4. Record command-by-command results, actual app behavior verified, platform unavailable/blocked items and residual risks.
5. Report the 5–10 next tasks ordered by dependency and risk. Do not restart the entire research process unless evidence is stale or inconsistent.

## Required response format

Return these headings:

1. **Repository state verified** — branch, HEAD, changed-file baseline and actual existing outputs.
2. **Corrections made** — including the PRNG correction and rule-frontmatter checks.
3. **Documentation coverage** — complete/pending counts and links to missing/updated items.
4. **Implementation status** — distinguish implemented/tested, implemented/unverified, scaffolded, planned and blocked.
5. **Verification** — exact commands, separate exit statuses, pass/fail counts and failures.
6. **Security/license findings** — critical/high findings first.
7. **Blockers and decisions needed** — only genuine blockers.
8. **Next tasks** — prioritized stable task IDs.

Start by inspecting the checkout; continue work, not just another plan. Make reversible progress within the permissions and capabilities actually available.
