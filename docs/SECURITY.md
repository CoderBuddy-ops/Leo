# Security Policy: Leo

---

## 1. Reporting Security Issues
As a personal-first assistant designed to run on the user's local PC, security and privacy are fundamental. If you discover a vulnerability or potential data leakage vector, please review [`docs/security/THREAT_MODEL.md`](docs/security/THREAT_MODEL.md) and open an issue or pull request with a reproducible test case.

---

## 2. Core Security Guarantees
1. **Zero Raw Audio Egress:** Audio buffers are captured locally, transcribed via local ONNX runtime, and discarded from memory.
2. **Encrypted Key Storage:** Cloud API keys are stored exclusively in the OS vault (`Windows.Security.Credentials.PasswordVault`), never in plain configuration files.
3. **No Unsafe Execution:** Arbitrary shell strings (`cmd.exe /c`, `powershell`) are strictly prohibited. Native OS interactions use typed allowlisted schemas with user confirmation for consequential actions.
