# Product Requirements Document (PRD): Leo

**Document ID:** `PRD-LEO-001`  
**Version:** `1.0.0` (Status: Approved)  
**Target Release:** Windows 10/11 x64  
**Product Architect:** Leo Principal Engineering Team

---

## 1. Executive Summary & Vision

**Leo** is a personal-first, on-device AI companion for Windows 10 and 11. It lives in a compact floating capsule at the top edge of the primary monitor, providing instant voice assistance, universal push-to-talk dictation, safe desktop automation, context-aware screen reading, and seamless coding agent session oversight.

---

## 2. User Personas & Jobs-to-be-Done (JTBD)

- **Persona 1: The Focused Software Engineer (Priya)**
  - *JTBD:* When coding in VS Code or running terminal sessions, Priya wants to dictate commit messages, ask questions about terminal errors, and approve agent tool calls without switching windows or breaking flow.
- **Persona 2: The Privacy-Conscious Writer (Alex)**
  - *JTBD:* When writing notes or drafting emails, Alex wants fast speech-to-text that runs 100% offline, automatically removes filler words, and keeps audio on the local machine.

---

## 3. Functional Requirements (FR)

| Req ID | Title | Priority | Description | Acceptance Criteria |
| :--- | :--- | :--- | :--- | :--- |
| **FR-101** | Top-Edge Floating Pill | **P0** | Frameless, transparent companion window at the top center of the screen. | Renders at `[104, 54]`, expands on hover/click to `[340, 88]`, never steals keyboard focus. |
| **FR-102** | Original Companion Character | **P0** | Expressive 2D canvas procedural character with physics-based gaze and moods. | Second-order spring dynamics; 13 mood states; reacts to poke/hover; zero external artwork dependencies. |
| **FR-103** | Universal Push-to-Talk Dictation | **P0** | Hold hotkey (`Right Alt`), speak, release to insert clean text into the active app. | Verbatim transcription via local engine; clipboard saved & restored; active app receives formatted text. |
| **FR-104** | Speech Cleanup Engine | **P0** | Rule-based regex filter that cleans spoken transcripts. | Removes *"um"*, *"uh"*; catches self-corrections (*"[A, actually] B"* $\to$ *"B"*); formats bullet lists. |
| **FR-105** | Local Model Chat & Fallback | **P0** | Chat interface supporting local loopback servers (Ollama, LM Studio) and cloud APIs. | Streaming response, cancellation support, honest offline/disconnected status. |
| **FR-106** | System Tray & Settings | **P0** | System tray icon with menu (Open, Settings, Mute, Quit) and settings dialog. | Persistent configuration in LocalAppData; hotkey manager with AltGr collision detection. |
| **FR-107** | Safe Native OS Tools | **P0** | Low-risk automation tools (timer, reminder, volume, date/time). | Typed schemas; bounds checking; no arbitrary shell strings. |
| **FR-201** | Coding Agent Hook Relay | **P1** | Windows Named Pipe listener for Claude Code / Codex session notifications. | Non-blocking permission card; timeout fail-open; live diff view. |
| **FR-202** | On-Screen Context & UI Automation | **P1** | Extract text from foreground window with preview and redaction. | Inspects UIA accessibility tree with OCR fallback; user preview before submission. |
| **FR-203** | File Drop Context | **P1** | Drag and drop file onto pill for immediate question answering. | File size limit (10MB); MIME validation; prompt injection warning. |
| **FR-301** | Local Wake Phrase ("Hey Leo") | **P2** | Hands-free activation via local wake word engine. | Opt-in only; clear microphone indicator; false-positive rejection. |

---

## 4. Non-Functional Requirements (NFR)

| Req ID | Category | Requirement Description |
| :--- | :--- | :--- |
| **NFR-01** | **Privacy & Local-First** | Audio never leaves the local machine. Network egress only occurs when user explicitly selects a cloud model provider. |
| **NFR-02** | **Performance & Footprint** | Idle CPU $< 0.5\%$, idle RAM $< 90\text{ MB}$, pill animation 60 FPS smooth with frame-drop fallback on battery. |
| **NFR-03** | **Windows I18n Safety** | Global hotkeys must never swallow AltGr European characters (`€`, `ą`, `ß`, `é`). |
| **NFR-04** | **Security & Sandboxing** | No execution of arbitrary shell strings (`cmd.exe /c`, `powershell`). API keys stored in Windows Credential Manager. |
| **NFR-05** | **Accessibility** | Full keyboard navigation (`Esc` to close, `Ctrl+Alt+Space` to summon), high-contrast mode, reduced motion support. |
