# Documentation Index: Leo

**Product:** Leo — Personal AI Companion for Windows 10/11 x64  
**Last Updated:** 2026-10-17  
**Status:** Approved

---

## 1. Governance & Root Invariants
- [`00-project-invariants.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/.agents/rules/00-project-invariants.md) — Non-negotiable safety, privacy, and intellectual property invariants.
- [`TASKS.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/TASKS.md) — Master executable prioritized task ledger and burndown.

---

## 2. Research & Upstream Reference Audits
- [`TOOLING_INVENTORY.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/research/TOOLING_INVENTORY.md) — Host toolchains, REA engine status, skills, and MCP discovery.
- [`LICENSING_AND_ASSET_AUDIT.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/research/LICENSING_AND_ASSET_AUDIT.md) — Coucou MIT vs `LICENSE-ASSETS.md` reservations & HeyNixi legal cleanroom rules.
- [`HEYNIXI_AUDIT.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/research/HEYNIXI_AUDIT.md) — Reverse-engineering findings (Qwen 3.5 model catalog, SplitMix32 PRNG, spring physics).
- [`COUCOU_AUDIT.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/research/COUCOU_AUDIT.md) — Windows native architecture, AltGr keyboard safety, 399 unit tests verified.
- [`REFERENCE_COMPARISON.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/research/REFERENCE_COMPARISON.md) — Comparative matrix across HeyNixi, Coucou, and Leo.
- [`SOURCE_REGISTER.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/research/SOURCE_REGISTER.md) — Traceability register for all analyzed commits, URLs, and code artifacts.

---

## 3. Product & Strategy Specifications
- [`PRD.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/product/PRD.md) — Product Requirements Document: personas, functional requirements (`FR-xxx`), non-functional requirements (`NFR-xxx`).
- [`MVP_SCOPE.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/product/MVP_SCOPE.md) — Prioritized P0 deliverables, explicitly deferred capabilities, and acceptance criteria.

---

## 4. Architecture & Technical Decision Records (ADRs)
- [`ARCHITECTURE.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/architecture/ARCHITECTURE.md) — System context, C4 container views, and module boundaries.
- [`DATA_FLOWS.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/architecture/DATA_FLOWS.md) — Speech dictation, model provider routing, and safe OS tool execution flows.
- [`ADR 0001: Foundation Strategy`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/architecture/ADR/0001-foundation.md) — Selective clean reuse of Coucou's MIT Windows windowing & FSM.
- [`ADR 0002: Speech Pipeline`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/architecture/ADR/0002-speech-pipeline.md) — Offline ASR strategy (`sherpa-onnx`) and clipboard insertion safety.
- [`ADR 0003: AI Provider Strategy`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/architecture/ADR/0003-ai-provider-strategy.md) — Provider-agnostic interface and local loopback vs cloud routing.
- [`ADR 0004: Safe Automation`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/architecture/ADR/0004-safe-automation.md) — Three-tier risk architecture and prompt injection guards.
- [`ADR 0005: Screen Context`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/architecture/ADR/0005-screen-context.md) — Tiered UI Automation, window capture, and capture exclusion (`WDA_EXCLUDEFROMCAPTURE`).

---

## 5. Design & User Experience
- [`COMPANION_AND_MOTION.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/design/COMPANION_AND_MOTION.md) — Leo character geometry, spring-damper equations, mood state machine, and battery policies.

---

## 6. Security & Privacy
- [`THREAT_MODEL.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/security/THREAT_MODEL.md) — STRIDE threat matrix, trust boundaries, prompt injection mitigations.
- [`NETWORK_POLICY.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/security/NETWORK_POLICY.md) — Local-first policy, zero raw audio egress, explicit cloud badges.
