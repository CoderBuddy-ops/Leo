# ADR 0003: Local and Cloud AI Model Provider Abstraction

**Status:** Accepted  
**Date:** 2026-10-17  
**Deciders:** Leo Principal Engineering Team

---

## Context
Leo provides type-to-chat, screen summarization, voice commands, and first-draft generation. Users need local-first execution for privacy, but may optionally choose to connect external model providers (Ollama, LM Studio, Anthropic Claude, OpenAI, or Google Gemini) for complex coding or reasoning tasks.

---

## Decision
1. **Unified Provider Abstraction (`IModelProvider`):**
   - Leo defines a typed TypeScript & Rust provider interface supporting discovery, health check, prompt streaming, cancellation, and token usage estimation.
   - Core supported adapters:
     - `local-openai-compat`: Directly connects to local endpoints running on loopback (Ollama `http://localhost:11434/v1`, LM Studio `http://localhost:1234/v1`, or vLLM).
     - `cloud-claude`: Anthropic Claude API (user-supplied API key).
     - `cloud-openai`: OpenAI API (user-supplied API key).
     - `cloud-gemini`: Google Gemini API (user-supplied API key).
2. **Strict Loopback vs External Disclosure:**
   - The UI explicitly differentiates loopback endpoints (`127.0.0.1`, `localhost`) from external internet endpoints.
   - For any external provider, a prominent badge (*"Leaves your PC"*) and provider name are displayed prior to prompt submission.
3. **Secret Storage:**
   - External API keys are stored exclusively in **Windows Credential Manager** (`Windows.Security.Credentials.PasswordVault` or `credman`), never in plaintext JSON, localStorage, logs, or error dumps.

---

## Consequences
- **Positive:** Unlocks both offline privacy and cutting-edge cloud models with total transparency.
- **Negative:** Requires handling network latency and provider-specific error formats gracefully.
