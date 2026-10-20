# Documentation Index: Leo

**Product:** Leo — Personal AI Companion for Windows 10/11 x64  
**Architecture:** Pure C# / .NET 8 / WinUI 3 (Windows App SDK 1.6)  
**Last Updated:** 2026-10-20  
**Status:** Approved Canonical Index  

---

## 1. Product Requirements & Backlog
- [`README.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/README.md) — Main product overview, quick-start CLI commands, and feature summary.
- [`PRD.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/PRD.md) — Product Requirements Document (Personas, JTBD, FR/NFRs, and Verification Criteria).
- [`TASKS.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/TASKS.md) — Implementation task backlog and verification ledger.
- [`CHANGELOG.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/CHANGELOG.md) — Chronological release notes and migration log.

---

## 2. Architecture & Security
- [`ARCHITECTURE.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/architecture/ARCHITECTURE.md) — Native WinUI 3, DWM glass transparency, module boundaries, and Win32 interop.
- [`MIGRATION.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/MIGRATION.md) — Complete record of migration from Tauri 2 to C# / .NET 8 / WinUI 3.
- [`SECURITY.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/SECURITY.md) — Hardware-backed `PasswordVault` storage, loopback boundaries, and zero audio egress.

---

## 3. Engineering & CLI Scripts
- `scripts/build.ps1` — Builds `Leo.sln` using the official .NET SDK.
- `scripts/test.ps1` — Runs the automated xUnit unit test suite (`Leo.Core.Tests`).
- `scripts/launch.ps1` — Launches the native Leo application.
- `scripts/package-release.ps1` — Builds, publishes self-contained release, and archives to `release/Leo-Windows-x64-v1.0.0.zip`.

---

## 4. Licensing & Legal Compliance
- [`THIRD_PARTY_NOTICES.md`](file:///c:/Users/Yuvraj/Desktop/Project/nixi/docs/THIRD_PARTY_NOTICES.md) — Open source licenses and cleanroom IP attribution.
