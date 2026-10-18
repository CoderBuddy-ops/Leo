---
trigger: always_on
description: Architecture standards, module boundaries, typed IPC, and dependency direction for Leo
---

# Architecture Rules

1. **Separation of Concerns:**
   - Native OS primitives (Win32 windowing, keyboard hooks, named pipes, process spawning) reside in Rust.
   - UI layout, motion dynamics, character canvas, and local model streaming reside in TypeScript.
   - IPC contracts must be strictly typed and validated on both sides.

2. **No Unsafe Native Execution:**
   - Model outputs are untrusted. Never concatenate strings into shell commands.
   - All OS automation uses allowlisted typed schemas and bounded parameters.

3. **Architecture Decision Records (ADRs):**
   - Major structural decisions, new external dependencies, or engine swaps require an ADR in `docs/architecture/ADR/`.
