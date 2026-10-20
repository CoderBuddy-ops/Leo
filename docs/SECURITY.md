# Security & Privacy Policy: Leo

**Document ID:** `SEC-LEO-002`  
**Version:** `2.0.0` (Status: Approved)  
**Target Platform:** Windows 10 & 11 (64-bit x64)  
**Technology:** Native C# / .NET 8 / WinUI 3 (Windows App SDK)  

---

## 1. Core Security & Privacy Invariants

### 1.1 Zero Audio Egress
- Audio buffers captured from the user's microphone are analyzed strictly in-process using local signal algorithms (`VoiceActivityDetector`: 16kHz mono RMS energy + Zero-Crossing Rate).
- Microphone audio is **never** uploaded to cloud servers or external endpoints under any circumstance.

### 1.2 Encrypted Credential Storage via Windows `PasswordVault`
- API keys (including Google Gemini keys) are **never** stored in plaintext files, source repositories, `.env` files, build bundles, or application logs.
- All credentials are persisted directly using the operating system's hardware-backed Windows Credential Manager:
  ```csharp
  // Windows.Security.Credentials.PasswordVault
  var vault = new PasswordVault();
  vault.Add(new PasswordCredential(ResourceName, UserName, apiKey));
  ```
- Keys are masked in the UI (`PasswordBox`) and redacted from all error messages and diagnostics.

### 1.3 Strict Network Boundary & Loopback Detection
- Local model endpoints (`http://127.0.0.1:11434/v1`, `http://localhost:1234/v1`, `http://[::1]:8080/v1`) are verified via `NetworkPolicy.IsLoopback(url)`.
- Whenever a request is sent to an external cloud endpoint (e.g., `generativelanguage.googleapis.com`), Leo displays a clear, user-visible badge:
  `[Leaves your PC]`
- Failed cloud requests are never masked as offline success responses.

### 1.4 Screen Share Privacy Protection
- Leo's floating capsule window is designed to set Win32 `SetWindowDisplayAffinity(hWnd, WDA_EXCLUDEFROMCAPTURE)` to prevent accidental capture or broadcasting during video conferencing, screen sharing, or background video recording.

### 1.5 Zero Unsafe Shell Execution
- Arbitrary command-line concatenation (`cmd.exe /c`, `powershell -c`) of model outputs is strictly forbidden.
- Any future OS automation actions must use strictly typed, allowlisted parameters with mandatory user confirmation for consequential disk or process operations.

---

## 2. Security Audit Checklist

| Check | Requirement | Verification Method | Status |
| :--- | :--- | :--- | :--- |
| **Secrets on Disk** | Zero plaintext API keys in `%APPDATA%` or repository | Inspected `settings.json` (contains only non-secret preferences) | PASSED |
| **PasswordVault** | Keys stored exclusively in OS credential vault | Verified `WindowsCredentialService.cs` implementation | PASSED |
| **Loopback Policy** | `127.0.0.1` and `localhost` recognized as local | Automated unit test `NetworkPolicy_CorrectlyDetectsLoopbackAddresses` | PASSED |
| **Secret Redaction** | Error messages strip API keys | Verified `GeminiProvider.RedactSecret()` logic | PASSED |
| **Zero Audio Egress** | No HTTP transmission of audio buffers | Verified `VoiceActivityDetector.cs` has zero network dependencies | PASSED |
