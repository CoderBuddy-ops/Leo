# HeyNixi Technical and Architectural Audit

**Target URL:** [https://heynixi.com/](https://heynixi.com/)  
**Audit Date:** 2026-10-17  
**Analysis Engine:** REA (Reverse Engineer Anything) static JS analysis (`jsrg_40786ec32875f21120a6a0dda38da5a4df5f6c8c4c3e3c12c5548fd189b37e58`) & Node.js AST inspection.

---

## 1. Evidence Classification Standard

Every statement in this audit is classified according to the master specification:
- **[Observed]:** Directly seen in the verified public web application code (`app.js`, `styles.css`, HTTP responses).
- **[Documented]:** Stated in public marketing text or product claims on `heynixi.com`.
- **[Inferred]:** Plausible conclusion with technical supporting evidence.
- **[Unknown / Needs Test]:** Insufficient evidence; requires live desktop application execution.

---

## 2. Product Claims and Positioning

- **Target Platform:** Windows 10 & 11 (64-bit AMD64) **[Observed/Documented]**.
- **Hardware Requirements:** 64-bit Windows, microphone, ~2.5 GB free disk space, 8 GB RAM recommended; dedicated GPU optional **[Documented]**.
- **Core Value Proposition:** An on-device personal assistant that lives at the top edge of the primary display as an interactive "pill" / "capsule". Operates 100% locally for listening, dictation, commands, and text generation **[Documented]**.

---

## 3. Discovered Technical Architecture

### 3.1 Model Catalog and Hardware Recommendation Formula
In `app.js` (offset ~194200), the catalog `Jf` and bandwidth heuristics are hardcoded:

```javascript
// [Observed] in app.js
var Jf = [
  { id: "qwen3.5-0.8b", label: "Built-in model", size: "0.8B", bytes: 532517120, params: 0.8, verified: true, note: "Ships inside Nixi. Quick answers, drafts and polish." },
  { id: "qwen3.5-2b", label: "Qwen3.5 2B", size: "2B", bytes: 1280835840, params: 2, verified: true, note: "The recommended upgrade: better writing, sharper on-screen help." },
  { id: "qwen3.5-4b", label: "Qwen3.5 4B", size: "4B", bytes: 2740937888, params: 4, verified: false, note: "Stronger writing and reasoning." },
  { id: "qwen3.5-9b", label: "Qwen3.5 9B", size: "9B", bytes: 5680522464, params: 9, verified: false, note: "High quality for big graphics cards." },
  { id: "qwen3.5-27b", label: "Qwen3.5 27B", size: "27B", bytes: 16740812704, params: 27, verified: false, note: "The power tier, for 24 GB cards." }
];

// Heuristic VRAM memory bandwidth table (GB/s):
var F0 = v => v >= 24 ? 1000 : v >= 16 ? 700 : v >= 11 ? 600 : v >= 8 ? 400 : v >= 6 ? 300 : 200;

// [Observed] Exact TPS formula:
function D0(model, vram, isGpu) {
  let r = model.bytes / 1e9;
  let o = isGpu && vram > 0 ? F0(vram) : 25; // 25 GB/s estimated CPU bandwidth
  return Math.round(10 / (r / (0.6 * o) + 0.0035)) / 10;
}
```

- **[Observed]:** The default model is **Qwen 3.5 0.8B**, quantized to ~508 MB (~4.5 bits per parameter), bundled directly in the distribution for instant CPU inference.
- **[Inferred]:** Local inference engine on Windows is either `llama.cpp` / GGUF or ONNX Runtime with DirectML execution provider.

### 3.2 Procedural Character Animation Engine (`class Jt`)
Rather than relying on heavy 3D WebGL runtimes or animated GIFs, Nixi implements an inlined 2D canvas face engine (`Jt`):
- **Physics Model:** Second-order damped harmonic oscillators ($\ddot{x} + 2\zeta\omega_0 \dot{x} + \omega_0^2(x - x_{\text{target}}) = 0$) **[Observed]**.
- **Frequencies & Damping:**
  - `look_x`, `look_y`: $\omega_0 = 300, \zeta = 0.6$
  - `brow_tilt`: $\omega_0 = 150, \zeta = 0.6$
  - `eye_size`: $\omega_0 = 180, \zeta = 0.6$
  - `glow`: $\omega_0 = 120, \zeta = 0.85$
  - `stretch`: $\omega_0 = 320, \zeta = 0.45$
- **Facial States:** 13 distinct presets: `idle`, `hover`, `listening`, `thinking`, `concentrate`, `talking`, `happy`, `wink`, `nervous`, `sad`, `meh`, `surprised`, `sleepy` **[Observed]**.
- **PRNG:** Verified as **Mulberry32** with increment constant `1831565813` (`0x6D2B79F5`) and multiply/xor mixing operations generating uniform floats in $[0, 1)$ **[Observed]**.

### 3.3 Keyboard Shortcuts & Dictation Workflow
- **Command / Wake:** `Right Ctrl` (or vocal wake phrase `"Hey Nixi"`) **[Observed/Documented]**.
- **Dictation Hotkey:** `Right Alt` (hold-to-talk, release to paste) **[Observed/Documented]**.
- **Dictation Transformations [Observed in `pt` dataset]:**
  - Verbal filler removal (*"um"*, *"uh"*, *"like"*).
  - Spoken mid-sentence self-correction detection (e.g., *"[Thursday, actually] Friday"* $\to$ *"Friday"*).
  - List generation (verbal trigger *"{Bullet point}"* or *"bullet point"* formatted to indented `- ` markdown lists).
  - Real-time capitalization and punctuation.

---

## 4. Disputed Claims & Technical Boundaries

1. **"100% Offline / No Internet":**
   - *Status:* **Documented** for core features (ASR, LLM, commands). Opening external websites, web searches, and update checks still require internet connectivity.
2. **"Reads the real text, not a blurry screenshot":**
   - *Status:* **Inferred** to use Windows UI Automation (UIA) COM APIs or browser extension/CDP bridge for accessibility text extraction, with OCR fallback for non-accessible windows.
3. **Screen Share Invisibility:**
   - *Status:* **Needs Test**. Windows Desktop Duplication / Graphics Capture API allows windows to set `SetWindowDisplayAffinity(hwnd, WDA_EXCLUDEFROMCAPTURE)`, which prevents windows from appearing in OBS / Discord / Teams capture.
