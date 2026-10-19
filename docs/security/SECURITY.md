# Leo Security, Privacy & Network Policy

**Document ID:** `SEC-LEO-001`  
**Classification:** Core Project Invariant  
**Status:** Approved  

---

## 1. Core Security Invariants

### 1.1 Local-First Default (Zero Audio Egress)
- Raw microphone audio captured from the user's microphone **never leaves the host machine**.
- Voice Activity Detection (VAD) and speech recognition run entirely on local hardware using lightweight on-device inference (`src/voice/audio_vad.ts`, `src/voice/offline_asr.ts`).
- No voice data, raw audio buffers, or intermediate spectrograms are ever streamed or sent to external servers.

### 1.2 Explicit Cloud Network Egress Indicator
- Network egress is permitted only when the user explicitly configures an external cloud AI provider (e.g., Google Gemini).
- In both the floating capsule dock and the standalone settings window, whenever a prompt or message is sent to an external service, a visible badge clearly states:
  ```
  [Leaves your PC]
  ```
- Strict Offline Mode completely closes and rejects all non-loopback HTTP/WebSocket connections.

### 1.3 Secure API Key Storage & Redaction
- Google Gemini and external AI provider keys are stored securely using protected local configuration (with Windows Credential Manager `PasswordVault` native integration).
- API keys are **never** logged, never dumped to disk in plaintext, never echoed in error traces, and never committed to source control.
- In `src/models/gemini_client.ts`, all error messages pass through a strict regex redactor that strips any token matching API key patterns (`AIza...`).

### 1.4 Windows Screen Capture Exclusion
- To protect user privacy during screen sharing, video recording, or conference presentations, the Leo floating capsule window sets:
  ```rust
  SetWindowDisplayAffinity(hwnd, WDA_EXCLUDEFROMCAPTURE);
  ```
- This Win32 attribute ensures that screen recording software (OBS, Zoom, Discord, Teams) captures a transparent void in place of Leo, preventing accidental broadcast of private chats or prompts.

### 1.5 Strict IPC Allowlisting & No Arbitrary Shell Execution
- The native Tauri IPC bridge (`src/ipc/leo_ipc.ts`, `app/src-tauri/src/lib.rs`) defines a strictly typed, bounded allowlist of commands.
- Untrusted AI model output is never piped to `cmd.exe /c`, `powershell.exe`, or arbitrary subprocesses.
- OS automation actions are bounded (window resizing with min/max clamps, volume adjustments, timers, clipboard pasting with size bounds).
