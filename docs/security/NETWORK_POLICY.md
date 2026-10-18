# Network Security & Egress Policy: Leo

**Document ID:** `NET-LEO-001`  
**Version:** `1.0.0` (Status: Approved)

---

## 1. Local-First Default Architecture

By default, **Leo performs zero outbound network calls over the public internet**.

- **Speech Recognition (ASR):** Runs 100% locally on CPU/GPU via `sherpa-onnx`.
- **Local Model Chat:** Connects exclusively to local loopback sockets (`127.0.0.1`, `localhost`) via HTTP/SSE.
- **Settings & Memory:** Persisted locally in `%LOCALAPPDATA%\Leo\`.
- **Telemetry & Crash Reporting:** **Disabled by default**. No telemetry SDKs or analytics beacons are embedded.

---

## 2. Permitted Network Egress Destinations

Outbound network connections are strictly restricted to the following user-configured scenarios:

| Scenario / Feature | Remote Host | Protocol | Data Sent | Data Received | User Approval Required |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Offline Model Download** | `huggingface.co` / CDN | HTTPS (443) | HTTP GET request | Model weights (`.onnx`) | **Explicit Click** in Settings |
| **Claude Cloud Chat** | `api.anthropic.com` | HTTPS (443) | Prompt text & conversation turn | Streaming completion | User must provide key & select provider |
| **OpenAI Cloud Chat** | `api.openai.com` | HTTPS (443) | Prompt text & conversation turn | Streaming completion | User must provide key & select provider |
| **Google Gemini Chat** | `generativelanguage.googleapis.com` | HTTPS (443) | Prompt text & conversation turn | Streaming completion | User must provide key & select provider |
| **App Update Check** | `api.github.com` | HTTPS (443) | Version query | Release tag & changelog | Automatic or Manual check in Settings |

---

## 3. Strict Prohibitions

1. **NO RAW AUDIO TRANSMISSION:** Leo **never** uploads microphone audio to any third-party or cloud server under any circumstances.
2. **NO SILENT EGRESS:** Any external API call triggers a visible UI badge (*"Leaves your PC"*) displaying the destination hostname.
3. **NO BACKGROUND BEACONS:** No advertising IDs, hardware telemetry, tracking pixels, or diagnostic pings are permitted.
