# Leo — Personal AI Companion for Windows

[![Windows 10/11](https://img.shields.io/badge/Windows-10%2F11%20x64-0078D4?logo=windows)](https://microsoft.com/windows)
[![Tauri 2](https://img.shields.io/badge/Tauri-2-FFC131?logo=tauri&logoColor=black)](https://tauri.app)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.6-3178C6?logo=typescript)](https://www.typescriptlang.org/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**Leo** is a personal-first, on-device AI assistant designed for Windows 10 and 11. It resides in a sleek, compact floating capsule at the top edge of your primary screen, providing immediate voice assistance, universal push-to-talk dictation, safe native automation, context-aware screen help, and coding agent session monitoring.

---

## Key Capabilities

- **Top-Edge Floating Pill:** Unobtrusive, transparent, always-on-top capsule that never steals active keyboard focus. Expands on hover or click.
- **Original Companion Character ("Leo"):** A dynamic, physics-based 2D canvas character featuring continuous spring-damper gaze tracking, organic saccades, and responsive emotional states without heavy video or 3D assets.
- **100% On-Device Dictation:** Hold `Right Alt` anywhere to dictate. Leo cleans up spoken verbal filler words (*"um"*, *"uh"*), catches spoken corrections, formats bullet lists, and restores your previous clipboard within 150ms.
- **Local-First & Private:** Speech recognition and default model inference run on your computer. Audio is never uploaded to the cloud.
- **Pluggable AI Models:** Connects out of the box to local model servers (Ollama, LM Studio) running the Qwen 3.5 family, with optional opt-in cloud providers (Claude, OpenAI, Gemini).
- **Safe Native Actions:** Typed allowlisted tools for timers, reminders, volume adjustments, and safe File Explorer reveals without arbitrary shell execution.

---

## Quick Start & Verification

### Prerequisites
- Windows 10 or 11 (64-bit AMD64)
- Node.js `v20.0.0+` (verified on `v26.7.0`)
- Rust toolchain (`rustc` & `cargo`) for building native Windows `.msi` / `.exe` installer.

### Run Unit Tests
```powershell
# Run Leo P0 test suite
node --test tests/leo_p0.test.mjs

# Run reference windowing & FSM integration tests
cd reference_coucou/windows
npm test
```

---

## Documentation Index

Explore the complete engineering, architectural, design, and security specifications in the [`docs/`](docs/INDEX.md) folder:
- [Product Requirements Document (PRD)](docs/product/PRD.md)
- [System Architecture](docs/architecture/ARCHITECTURE.md)
- [Architecture Decision Records (ADRs)](docs/architecture/ADR/0001-foundation.md)
- [Companion & Motion Design](docs/design/COMPANION_AND_MOTION.md)
- [Security Threat Model](docs/security/THREAT_MODEL.md)
- [Network & Privacy Policy](docs/security/NETWORK_POLICY.md)
- [Third-Party Notices](THIRD_PARTY_NOTICES.md)
