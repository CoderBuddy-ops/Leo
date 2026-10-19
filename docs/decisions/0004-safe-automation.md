# ADR 0004: Safe Native OS Tool Calling and Automation Sandbox

**Status:** Accepted  
**Date:** 2026-10-17  
**Deciders:** Leo Principal Engineering Team

---

## Context
AI desktop assistants often expose powerful operating system capabilities. However, allowing an LLM unconstrained execution of shell scripts or destructive file commands exposes the user to severe prompt injection vulnerabilities (e.g. from malicious web pages, emails, or untrusted code repositories).

---

## Decision
1. **Three-Tier Risk Architecture:**
   - **Tier 0 (Low Risk / Auto-execute):** Read-only time/date, battery status, timer/reminder creation, volume/media adjustments. These actions have typed JSON schemas, bounded inputs, and cannot cause data loss.
   - **Tier 1 (Moderate Risk / Explicit Notification):** Reveal a file in File Explorer (`explorer.exe /select,"<path>"`), open an approved URL in the default browser, focus an existing application window.
   - **Tier 2 (High Risk / Preview & Mandatory Confirmation):** Launching arbitrary executables, deleting/overwriting files, sending emails, or running shell scripts. Leo enforces a non-blocking confirmation card showing the exact command, target path, and risk explanation before execution.
2. **Path Normalization and Boundary Checks:**
   - Path traversal (`../`, absolute drive jumps) is strictly blocked.
   - Safe process invocation avoids shell string concatenation (`cmd /c` or `powershell -c`). Process arguments are passed as immutable arrays directly to Win32 `CreateProcessW` / Rust `std::process::Command`.
3. **Prompt Injection Defense:**
   - Text extracted from web pages, files, or OCR is treated as **untrusted data**.
   - System prompts isolate user instructions from context blocks using strict delimiter guards.

---

## Consequences
- **Positive:** Immune to silent arbitrary code execution via prompt injection.
- **Negative:** Certain automated workflows require a one-click user confirmation.
