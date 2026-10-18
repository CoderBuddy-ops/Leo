# Leo Privacy & Security Architecture

**Last Updated:** 2026-10-18  
**Status:** Canonical & Enforced  

---

## 1. Core Invariants & Privacy Philosophy

Leo is designed from the ground up around a **Local-First, Zero-Surprise** privacy model:
1. **Zero Audio Egress:** Audio recorded by your microphone is processed **strictly on-device**. Raw voice audio is **never** uploaded to cloud servers, proprietary third parties, or remote speech APIs.
2. **Explicit Cloud AI Egress Badge:** When using an external cloud AI model (such as Google Gemini with your personal API key), Leo displays an unmistakable visual indicator:
   - `[Leaves your PC: External Google Gemini Request]`
   - Dock header indicator changes to amber: `Leaves PC`
3. **Strict Offline Mode:** An opt-in "Strict Offline Mode" toggle completely disables all network requests. The interface, local speech cleanup, and local models continue functioning without requiring an active internet connection.

---

## 2. Secrets & API Key Storage

1. **No Plaintext Storage in Git:**
   - Secrets, credentials, and API keys are **never** committed to version control, embedded in binary builds, or emitted to log files.
2. **Local Client Storage:**
   - The user's Google AI Studio / Gemini API key is entered directly by the user in Leo's Settings screen via a masked password field.
   - The key is saved locally in client storage under `leo_gemini_api_key_v1`.
3. **Automated Secret Redaction:**
   - Any runtime error, network failure, or API exception passes through `GeminiModelClient.redactSecret()`.
   - If the API key appears anywhere in error strings or URLs, it is automatically replaced with `[REDACTED_API_KEY]`.

---

## 3. Data Flow Architecture

```
[User Speech]
      │ (Local 16kHz Mono Stream)
      ▼
[Voice Activity Detector (VAD)] (Local RMS & ZCR in memory)
      │
      ▼
[Offline ASR Pipeline] (Local weights, explicit download consent)
      │
      ▼
[Speech Cleanup Filter] (Local regex & correction resolver)
      │
      ▼
[Leo Prompt Router]
      ├── If Local Model (Ollama / LM Studio) ──> Loopback (127.0.0.1:11434 / 1234)
      ├── If Strict Offline Mode ───────────────> Blocked (Zero egress guaranteed)
      └── If Google Gemini ─────────────────────> HTTPS POST to generativelanguage.googleapis.com
                                                  (With explicit visual egress badge)
```

---

## 4. Screen Sharing & Window Capture Protection

- Leo's floating companion window supports `WDA_EXCLUDEFROMCAPTURE` on Windows 10 (version 2004+) and Windows 11.
- This native OS feature prevents companion notes and prompts from being inadvertently broadcast during Microsoft Teams, Zoom, Discord, or Google Meet screen shares.

---

## 5. Usage & Transparency Dashboard

Leo provides a real-time Privacy & Usage monitor inside the Settings drawer:
- **Audio Privacy:** Displays `Zero Egress (100% on-device)`.
- **Active Model & Provider:** Shows currently connected provider (e.g. `Ollama / qwen-3.5-2b` or `Gemini / gemini-2.5-flash`).
- **Session Query Count:** Tracks total requests processed in this session.
- **Token Metrics:** Reports actual tokens returned by the model API (`usageMetadata`) or labels as `Local / N/A`.
- **Cost Transparency:** Clearly states `Cost unavailable` rather than fabricating arbitrary pricing estimates.
