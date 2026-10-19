# ADR 0002: Local Speech Recognition (ASR) Pipeline Strategy

**Status:** Accepted  
**Date:** 2026-10-17  
**Deciders:** Leo Principal Engineering Team

---

## Context
Universal dictation into any application is a core feature of Leo. Dictation must operate offline, preserve privacy (no raw audio uploaded), exhibit low latency on Windows CPUs, and avoid breaking the user's active keyboard focus or clipboard history.

Candidate engines evaluated:
1. **Windows Media Speech API (`Windows.Media.SpeechRecognition`):** Deprecated in modern Windows SDKs, unreliable offline vocabulary support.
2. **`whisper.cpp` / Faster-Whisper:** High accuracy, but higher latency on CPU without AVX-512 / GPU acceleration.
3. **`sherpa-onnx` with NVIDIA Parakeet-TDT-0.6B-v3:** Sub-200ms real-time factor on standard x64 CPUs, fully offline, open weights, proven in Coucou's Windows implementation.
4. **Local Whisper Tiny/Base ONNX:** Lightweight (~75MB–150MB), excellent CPU performance.

---

## Decision
1. **Adopt a Pluggable ASR Provider Interface (`IAudioTranscriber`):**
   - Leo will not hardcode a single speech backend.
   - Primary default offline engine: **`sherpa-onnx`** with **Parakeet-TDT-0.6B-v3** (with optional lightweight Whisper Tiny/Base fallback).
2. **Honest Download Consent:**
   - Model weights (~700 MB) are **never downloaded silently**. The settings UI provides an explicit *"Download Offline Voice Model"* button displaying size, checksum, and storage location (`%LOCALAPPDATA%\Leo\Models`).
   - If no model is installed, Leo displays an honest *"Speech engine not configured"* status rather than silently falling back to cloud transcription or mocking output.
3. **Clipboard and Input Safety:**
   - Dictation text is inserted using Windows `SendInput` (synthesizing `Ctrl+V` after temporarily staging clean text in the clipboard).
   - Leo preserves and restores the user's prior clipboard content within 150ms of pasting.

---

## Consequences
- **Positive:** True offline privacy, deterministic performance, no cloud audio egress.
- **Negative:** First-time setup requires downloading ~700 MB of model weights if offline voice is enabled.
