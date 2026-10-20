# System Architecture Document: Leo

**Document ID:** `ARCH-LEO-002`  
**Version:** `2.0.0` (Status: Approved)  
**Target Platform:** Windows 10 & 11 (64-bit x64, Minimum build 10.0.17763.0 / Target 10.0.19041.0)  
**Technology Stack:** Pure C# 12, .NET 8, WinUI 3 (Windows App SDK 1.6)  

---

## 1. High-Level System Architecture

Leo is a 100% native Windows desktop application built with modern C# and WinUI 3 via the Windows App SDK. Native Win32 windowing, Desktop Window Manager (DWM) composition, Credential Manager, and Shell NotifyIcon operate directly in-process without any intermediate browser webviews, Node.js processes, Vite development servers, Electron, or Tauri layers.

```mermaid
graph TD
    subgraph "Windows OS Layer"
        DWM["Desktop Window Manager (DwmExtendFrameIntoClientArea)<br>Layered Glass Margins (-1)"]
        SHELL["Shell_NotifyIconW & SetWindowSubclass<br>Native System Tray & Context Menu"]
        VAULT["Windows Credential Manager (PasswordVault)<br>Resource: Leo_Windows_AI_Assistant"]
        WIN_INPUT["Win32 Interop (WM_NCLBUTTONDOWN / HTCAPTION)<br>Direct Pointer Dragging"]
    end

    subgraph "Presentation Layer (Leo.App — WinUI 3)"
        CAPSULE_WIN["CapsuleWindow (WinUI 3 Window)<br>OverlappedPresenter (No border/titlebar)<br>AlwaysOnTop, BorderRadius=32 Dock"]
        FACE_CANVAS["Procedural Face Engine<br>Vertical Oval Eyes, Symmetrical Pink Blush<br>Zero Mouth, Zero Blue Dot"]
        CHAT_DRAWER["Collapsible Chat Drawer<br>ScrollViewer + StackPanel Message Bubbles"]
        SETTINGS_WIN["SettingsWindow (WinUI 3 Standalone)<br>Masked API Key, Loopback Tester, Privacy Badges"]
        TRAY_SVC["SystemTrayService<br>Subclassed WndProc with Restore, Hide, Settings, Quit"]
    end

    subgraph "Application Orchestration Layer (Leo.Application)"
        CONV_MGR["LeoConversationManager<br>Async Streaming Token Pipeline<br>Cancellation & Gesture State Sync"]
        SETTINGS_IFACE["ISettingsService & AppSettings<br>Preferences Contract"]
        AI_IFACE["IAiProvider Interface<br>StreamChatAsync & TestConnectionAsync"]
    end

    subgraph "Core Domain Layer (Leo.Core)"
        CHAR_ENGINE["LeoCharacterEngine (Damped Oscillator)<br>Mulberry32 Deterministic PRNG<br>Saccades, Blinking, Breathing Float"]
        GESTURE_CTRL["LeoGestureController<br>Idle, Listening, Thinking, Talking, Sleepy"]
        SPEECH_FILTER["SpeechCleanupFilter<br>Filler Removal, Self-Correction, Bullet Lists"]
        VAD["VoiceActivityDetector<br>16kHz Mono Dual-Criteria (RMS + ZCR) Hangover"]
        CONTRACTS["ChatMessage, AiProviderConfig, NetworkPolicy"]
    end

    subgraph "Infrastructure Layer (Leo.Infrastructure)"
        CRED_SVC["WindowsCredentialService<br>Encrypted PasswordVault Storage"]
        GEMINI_CLIENT["GeminiProvider (Google Gemini API)<br>REST SSE Streaming, Key Masking, Egress Badges"]
        LOCAL_CLIENT["LocalAiProvider (Ollama / LM Studio)<br>Loopback Detection, Dynamic /models Discovery"]
        SETTINGS_SVC["JsonSettingsService<br>File Persistence (%APPDATA%/Leo/settings.json)"]
    end

    DWM <--> CAPSULE_WIN
    SHELL <--> TRAY_SVC
    VAULT <--> CRED_SVC
    WIN_INPUT <--> CAPSULE_WIN

    CAPSULE_WIN --> FACE_CANVAS
    CAPSULE_WIN --> CHAT_DRAWER
    CAPSULE_WIN --> SETTINGS_WIN
    TRAY_SVC --> CAPSULE_WIN

    CAPSULE_WIN --> CONV_MGR
    SETTINGS_WIN --> SETTINGS_SVC

    CONV_MGR --> CHAR_ENGINE
    CONV_MGR --> GESTURE_CTRL
    CONV_MGR --> SPEECH_FILTER
    CONV_MGR --> AI_IFACE

    AI_IFACE <|.. GEMINI_CLIENT
    AI_IFACE <|.. LOCAL_CLIENT
    SETTINGS_IFACE <|.. SETTINGS_SVC
    GEMINI_CLIENT --> CRED_SVC
```

---

## 2. Solution Structure & Module Responsibilities

The solution `Leo.sln` is organized into clean architectural layers:

```
Leo.sln
├── src/
│   ├── Leo.Core/               # net8.0: Domain models, math, character physics, VAD, cleanup
│   ├── Leo.Application/        # net8.0: Use cases, conversation orchestration, provider interfaces
│   ├── Leo.Infrastructure/     # net8.0-windows10.0.19041.0: PasswordVault, Gemini SSE, Local AI, settings
│   └── Leo.App/                # net8.0-windows10.0.19041.0: WinUI 3 views, capsule dock, tray, XAML controls
├── tests/
│   └── Leo.Core.Tests/         # net8.0: 18 unit tests (100% passing) for character physics, VAD, gestures, AI
├── scripts/                    # build.ps1, test.ps1, package-release.ps1, launch.ps1
├── assets/                     # canonical brand assets (Leo Icon V2, icon.ico)
└── docs/                       # complete architectural, security, and developer documentation
```

### 2.1 `Leo.Core` (Pure .NET 8 Domain)
- **`LeoCharacterEngine`**: Physics simulation of the companion character using damped harmonic oscillator springs (`k=280`, `d=22`). Computes gaze saccades via deterministic `Mulberry32` PRNG, eyelid blinks (at random 2.5–5s intervals), squash-and-stretch impulses on clicks, and idle breathing.
- **`LeoMoodState` & `LeoMoodPresets`**: Preconfigured emotion states: `Idle`, `Hover`, `Listening`, `Thinking`, `Happy`, `Talking`, `Concentrate`, `Sleepy`, and `Offline`.
- **`LeoGestureController`**: Higher-level state machine handling auto-sleep timeouts and audio/thinking mode synchronization.
- **`SpeechCleanupFilter`**: Rule-based spoken transcript formatter that strips verbal filler words (*"um"*, *"uh"*, *"like"*), resolves spoken self-corrections (*"Thursday, actually Friday"*), and turns verbal bullet points into clean markdown.
- **`VoiceActivityDetector`**: Dual-criteria energy (RMS) and zero-crossing rate (ZCR) voice detection with silence hangover filtering for 16kHz audio buffers.
- **`AiContracts` & `NetworkPolicy`**: Chat contracts and loopback network policy verification.

### 2.2 `Leo.Application` (Orchestration & Abstractions)
- **`IAiProvider`**: Common asynchronous streaming interface for LLM backends (`StreamChatAsync`, `TestConnectionAsync`).
- **`ISettingsService`**: Abstraction for persistent application preferences.
- **`LeoConversationManager`**: Coordinates user prompts, speech cleanup, gesture state transitions, token streaming events, and generation cancellation.

### 2.3 `Leo.Infrastructure` (OS & External Service Integration)
- **`WindowsCredentialService`**: Safe credential persistence using Windows `Windows.Security.Credentials.PasswordVault`. API keys are encrypted at rest by the operating system and never written to plaintext JSON files.
- **`GeminiProvider`**: Official Google Gemini streaming client using HTTP/2 SSE streaming with model selection (`gemini-2.0-flash`, `gemini-1.5-flash`), error sanitization, and connection health checks.
- **`LocalAiProvider`**: Seamless connection to Ollama (`http://127.0.0.1:11434/v1`) or LM Studio (`http://127.0.0.1:1234/v1`) with loopback enforcement and dynamic `/models` discovery.
- **`JsonSettingsService`**: Manages user configuration persistence in `%APPDATA%\Leo\settings.json`.

### 2.4 `Leo.App` (WinUI 3 Desktop Experience)
- **`CapsuleWindow`**: Native borderless floating window configured via `AppWindow` with an `OverlappedPresenter` set to borderless, non-resizable, and always-on-top. DWM glass margins (`DwmExtendFrameIntoClientArea` with `-1` margins) deliver transparent bounds outside the capsule border. Direct `WM_NCLBUTTONDOWN` message dispatch enables lag-free native dragging.
- **Procedural Face**: Symmetrical vertical oval eyes with soft pink blush marks (`#AAFFA0AF`), zero mouth, and zero blue center dot.
- **Chat Drawer**: Collapsible drawer featuring a high-performance `ScrollViewer` + `StackPanel` layout for chat message bubbles and real-time streaming tokens.
- **`SettingsWindow`**: Standalone multi-tab settings window providing masked credential inputs, local endpoint test badges, speech volume sliders, and reduced-motion toggles.
- **`SystemTrayService`**: Win32 notification area tray icon using `Shell_NotifyIconW` and subclassed window procedure (`SetWindowSubclass`) to handle tray left-clicks (restore/toggle) and right-clicks (context menu).

---

## 3. Native Windows Desktop Integration Details

| Capability | Native API Implementation | Benefit |
| :--- | :--- | :--- |
| **Window Transparency** | `DwmExtendFrameIntoClientArea(hWnd, -1)` + WinUI `Grid.Background = null` | True transparent desktop overlay without black boxes or clipping |
| **Borderless Floating Dock** | `Microsoft.UI.Windowing.AppWindow` + `OverlappedPresenter` | Clean modern floating capsule above active applications |
| **Smooth Dragging** | Win32 `ReleaseCapture()` + `SendMessage(hWnd, WM_NCLBUTTONDOWN, HTCAPTION, 0)` | Instant, hardware-accelerated window dragging without cursor jitter |
| **System Tray Icon** | Win32 `Shell_NotifyIconW` + `SetWindowSubclass` | Native background presence; right-click context menu; hide-to-tray on close |
| **Credential Storage** | `Windows.Security.Credentials.PasswordVault` | Hardware-backed encrypted API key storage; zero plaintext secrets on disk |
| **Screen Share Privacy** | Win32 `WDA_EXCLUDEFROMCAPTURE` compatibility | Prevents capsule from being captured during video calls and recordings |
| **Multi-Monitor DPI** | Windows App SDK `DisplayArea.GetFromWindowId` | Auto-detects display work area and grounds capsule near bottom center |
