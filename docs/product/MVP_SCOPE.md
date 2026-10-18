# Minimum Viable Product (MVP) Scope: Leo

**Document ID:** `MVP-LEO-001`  
**Version:** `1.0.0`  
**Status:** Approved  
**Target Horizon:** P0 Release Foundation

---

## 1. Explicit MVP Inclusions (P0 Foundation)

The P0 MVP delivers a fully functional, testable Windows desktop assistant vertical slice:

1. **Native Windows Companion Shell:**
   - Always-on-top, transparent, frameless window anchored at top-center of primary display.
   - High-DPI and multi-monitor coordinate normalization.
   - Interactive pill: resting state `[104, 54]`, expands to `[340, 88]` on hover or click.
   - Never steals focus from active applications.

2. **Original Leo Companion Character:**
   - 100% original programmatic 2D canvas character ("Leo").
   - Spring-damper physics engine for gaze tracking, blinking, hop bounces, and eye glow.
   - Expressive state transitions: `idle`, `listening`, `thinking`, `happy`, `talking`, `offline`.
   - Zero proprietary external assets.

3. **Universal Push-to-Talk Dictation Pipeline:**
   - Global hotkey (`Right Alt`) for push-to-talk.
   - Pluggable speech engine interface (`IAudioTranscriber`) with honest "engine not installed" state.
   - Client-side speech cleanup filter (filler word removal, verbal self-corrections, bullet lists).
   - Safe text insertion via clipboard staging and immediate restoration.

4. **Local & Cloud Chat:**
   - Provider abstraction supporting local OpenAI-compatible model servers (Ollama, LM Studio).
   - Streaming text response, prompt cancellation, and markdown display.
   - Clear network boundary badges (*"Local model - stays on PC"* vs *"Cloud provider - leaves PC"*).

5. **Safe Native Tool Execution:**
   - Typed tools: Timers, reminders, volume adjustments, date/time lookup.
   - Bounds-checked arguments and structured error handling.

6. **Settings & Tray Management:**
   - Windows system tray icon with quick actions (Open, Settings, Mute, Exit).
   - Settings window with hotkey manager and local model endpoint configuration.

---

## 2. Explicitly Deferred Capabilities (P1 & P2)

| Feature | Deferred To | Rationale |
| :--- | :--- | :--- |
| **Coding Agent Pipe Hooks (Claude Code)** | **P1** | Focus P0 on core assistant stability and dictation. |
| **Screen Reading & UI Automation Context** | **P1** | Requires rigorous UIA COM testing across multiple Windows apps. |
| **File Drag-and-Drop Context** | **P1** | Requires file parser and prompt-injection sanitization. |
| **Hands-Free Wake Word ("Hey Leo")** | **P2** | Avoid accidental activations until VAD and false-positive filtering are proven. |
| **Autonomous Web Browsing & Search** | **P2** | Keep P0 strictly focused on local-first productivity. |

---

## 3. Measurable MVP Exit Criteria

1. **Build & Test Verification:**
   - `npm test` runs 100% clean with zero failures.
   - TypeScript compiles cleanly with `tsc --noEmit`.
2. **Runtime Stability:**
   - Idle CPU utilization $< 0.5\%$.
   - Windows AltGr European keyboard test passes (no swallowed currency/accent keys).
3. **Legal & Compliance:**
   - `THIRD_PARTY_NOTICES.md` fully accounts for all MIT attributions.
   - Zero Coucou, Mochi, or HeyNixi branding/media assets bundled in the distribution.
