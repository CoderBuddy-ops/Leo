---
trigger: always_on
description: Non-negotiable safety, privacy, legal, and operational invariants for Leo
---

# Project Invariants & Safety Rules

1. **Local-First & Personal Privacy:**
   - Audio from the user's microphone must NEVER be uploaded to external servers. Speech recognition runs on local hardware.
   - Any external API request (Claude, OpenAI, Gemini) must clearly display a visual indicator stating that data leaves the computer.

2. **Zero Intellectual Property Infringement:**
   - Strictly honor Coucou's `LICENSE-ASSETS.md`: NEVER use the names "Coucou" or "Mochi", and NEVER bundle Mochi character artwork, icons, or sounds.
   - Strictly honor HeyNixi copyright: NEVER copy proprietary minified JavaScript, branding, or logos.
   - All character animations, sounds, icons, and UI must be 100% original to Leo.

3. **Windows System Safety & Non-Destructive Behavior:**
   - Global keyboard shortcuts must be tested against Windows European AltGr layouts (`€`, `ą`, `ß`) to prevent swallowing user keystrokes.
   - Zero execution of arbitrary unconfirmed shell commands (`cmd.exe /c`, `powershell`).
   - Consequential file-system or process actions require user confirmation.

4. **Quality & Evidence Standard:**
   - No feature is declared "done" without real automated test coverage or documented verification steps.
   - Never fabricate benchmark results, test runs, or completed integrations.
