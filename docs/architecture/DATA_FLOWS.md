# Data Flow Architecture: Leo

**Document ID:** `FLOW-LEO-001`  
**Version:** `1.0.0` (Status: Approved)

---

## 1. Speech-to-Text & Universal Dictation Flow

The dictation pipeline operates entirely locally without sending audio to any network destination.

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant Hook as Low-Level Hook (Rust)
    participant Audio as Audio Capture (WASAPI)
    participant ASR as Offline ASR (sherpa-onnx)
    participant Filter as Cleanup Filter (TS)
    participant Win as Windows Focused App

    User->>Hook: Press & Hold Hotkey (Right Alt)
    Hook->>Audio: Start Microphone Stream
    Note over Audio: Audio stays in memory buffer
    User->>Hook: Release Hotkey (Right Alt)
    Hook->>Audio: Stop Capture Buffer
    Audio->>ASR: Send Raw PCM Audio
    ASR-->>Filter: Emit Raw Transcript
    Filter->>Filter: Remove Fillers ("um", "uh")
    Filter->>Filter: Resolve Spoken Self-Corrections
    Filter->>Filter: Format Lists & Capitalization
    Filter->>Win: Backup Clipboard
    Filter->>Win: Paste Clean Text (SendInput Ctrl+V)
    Filter->>Win: Restore Prior Clipboard (150ms)
```

---

## 2. Local AI Chat vs Optional Cloud Provider Flow

Leo strictly isolates local loopback requests from external network traffic:

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant UI as Leo Companion UI
    participant Router as Provider Router
    participant LocalServer as Local Model Server (Ollama / LM Studio)
    participant CloudAPI as External Cloud API (Anthropic / OpenAI / Gemini)

    User->>UI: Type Prompt or Ask Question
    UI->>Router: Dispatch Request

    alt Local Model Configured (Default)
        Note over Router,LocalServer: Network Destination: http://127.0.0.1:11434 (Loopback Only)
        Router->>LocalServer: POST /v1/chat/completions (Stream)
        LocalServer-->>UI: Yield Tokens (SSE)
        UI-->>User: Render Streaming Response
    else Cloud Provider Configured (User Explicit Opt-In)
        Note over UI: UI Displays "Leaves your PC" Badge
        Router->>CloudAPI: HTTPS POST /v1/messages (Encrypted)
        CloudAPI-->>UI: Yield Tokens (SSE)
        UI-->>User: Render Streaming Response
    end
```

---

## 3. Safe OS Tool Calling & Confirmation Flow

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant Model as AI Model
    participant Engine as Tool Registry
    participant Card as Confirmation Card
    participant Native as Win32 OS API

    Model->>Engine: Propose Tool Call (name, args)
    Engine->>Engine: Validate JSON Schema & Risk Tier

    alt Tier 0: Low Risk (Timer, Volume, Date)
        Engine->>Native: Execute OS Action
        Native-->>Engine: Action Result
        Engine-->>Model: Success Feedback
    else Tier 1/2: Moderate/High Risk (Launch Executable, Reveal File)
        Engine->>Card: Display Non-Blocking Confirmation Card
        Note over Card: Displays exact target path and arguments
        User->>Card: Click "Approve"
        Card->>Native: Execute Verified Command
        Native-->>Engine: Action Result
        Engine-->>Model: Action Completed
    else User Rejects or Times Out
        User->>Card: Click "Deny"
        Card-->>Model: Refusal Returned ("Action cancelled by user")
    end
```
