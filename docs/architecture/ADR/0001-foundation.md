# ADR 0001: Foundation and Upstream Code Reuse Strategy

**Status:** Accepted  
**Date:** 2026-10-17  
**Deciders:** Leo Principal Engineering Team

---

## Context
Leo requires a robust Windows-native companion window that sits at the top of the primary monitor, provides transparency, does not steal focus during dictation or notifications, supports global hotkeys without breaking European AltGr keyboard layouts, and handles DPI changes and multi-monitor setups reliably.

We evaluated two potential approaches for building Leo's core Windows runtime:
1. **Greenfield Re-write:** Build from scratch using Electron or raw WinUI 3 / Tauri 2.
2. **Selective Clean Reuse from Coucou Windows:** Leverage Coucou's MIT-licensed native Win32 window management, FSM, and low-level keyboard hook infrastructure from `reference_coucou/windows/`, while strictly isolating and replacing all branding, characters, audio assets, and UI skins.

---

## Decision
We decide to adopt a **selective clean reuse strategy** based on Coucou's Windows implementation:

1. **Retain Proven Native Infrastructure (MIT Licensed):**
   - Win32 transparent window setup (`WS_EX_TOPMOST`, `WS_EX_TOOLWINDOW`, DPI scaling) in Rust / Tauri 2.
   - Low-level keyboard hook (`WH_KEYBOARD_LL`) with verified AltGr European layout safety.
   - Robust Island Finite State Machine (FSM) validated by 399 unit tests.
   - Non-blocking Named Pipe agent integration protocol (`pipe.rs`, `agents.rs`).

2. **Strict Brand and Asset Isolation:**
   - In accordance with `LICENSE-ASSETS.md`, zero Coucou/Mochi branding, character drawings, sounds, or visual assets will be retained.
   - Replace Mochi with an original geometric companion character: **Leo**.
   - Create a cleanroom HTML5 canvas face and spring-damper physics engine inspired by Nixi's dynamic equations, but implemented independently.

3. **Incorporate HeyNixi's Best Architectural Inventions:**
   - Local small language model integration (Qwen 3.5 family) with hardware profiling heuristics.
   - Universal Push-to-Talk dictation with deterministic verbal filler word removal and spoken self-correction cleanup.
   - Typed allowlisted OS tools with explicit user confirmation for consequential actions.

---

## Consequences
- **Positive:**
  - Avoids reinventing complex Win32 transparent window management, multi-monitor coordinate math, and Windows international keyboard handling.
  - Gives Leo an immediate, rock-solid test suite (399 unit tests) for core island state transitions.
  - Fully respects Louis Raillé's MIT license and copyright notices in `THIRD_PARTY_NOTICES.md`.
- **Negative / Risks:**
  - Requires maintaining parity with upstream Tauri 2 dependencies.
  - Requires continuous diligence to guarantee that no reserved Coucou/Mochi asset strings leak into user-facing production binaries.
