# Third-Party Licensing and Asset Audit

**Audit Date:** 2026-10-17  
**Auditor:** Leo Principal Engineering Team  
**Scope:** Legal & licensing boundaries for HeyNixi reference and Coucou reference repository.

---

## 1. Upstream Reference: Coucou

- **Repository:** `https://github.com/Louis-CFM/coucou`
- **Inspected Commit:** `434d6a2b4af2e5ce117fa425170abe25620130b9` (Date: 2026-10-16 22:53:27 +0200)
- **Primary Source Code License:** **MIT License** (Copyright (c) 2026 Louis Raillé).
  - Permits copying, modifying, merging, publishing, distributing, sublicensing, and selling copies of the Software.
  - Requires preserving the copyright notice and permission notice in all copies or substantial portions of the Software.

### 1.1 Critical Reservation: `LICENSE-ASSETS.md`
Coucou's author explicitly carved out intellectual property that is **NOT** licensed under MIT:
> *"The MIT License covers the source code of Coucou. It does not cover the brand and the artwork listed here, which remain the property of Louis Raillé:*
> - *the names 'Coucou' and 'Mochi';*
> - *the Mochi character — its design, look, expressions and animations as a character;*
> - *the app icon and menu bar icon;*
> - *the sounds (`NotchBuddy/Resources/sounds/`);*
> - *the images, GIFs and videos in `docs/media/` and `design/`;*
> - *copies of these assets anywhere else in the repository."*

### 1.2 "What you can't do without written permission"
> *"Publish or distribute an app, a fork or a derivative work under the name 'Coucou' or 'Mochi', or with the Coucou icon, the Mochi character or the Coucou sounds... If you fork Coucou to ship your own app, that's welcome under the MIT License: just give it your own name, icon, character and sounds."*

### 1.3 Leo Compliance Mandate for Coucou
1. **Never use the names "Coucou" or "Mochi"** in user-facing branding, app bundle identifiers, window titles, or documentation.
2. **Never copy or distribute Mochi character assets** or vector SVG artwork. Leo must feature an original companion character ("Leo").
3. **Never bundle Coucou audio/sound files**. Leo will use an original synthesized or license-free audio feedback set.
4. **Preserve Louis Raillé's MIT copyright notice** in `THIRD_PARTY_NOTICES.md` and appropriate source headers for any reused architecture, FSM, and native windowing code.

---

## 2. Upstream Reference: HeyNixi

- **Target Origin:** `https://heynixi.com/`
- **Status:** Proprietary commercial software (beta).
- **Public Assets Analyzed:** `app.js` (205 KB), `styles.css` (45 KB), `index.html`.
- **License Terms:** No open-source license granted. Proprietary copyright belongs to HeyNixi authors.

### 2.1 Leo Compliance Mandate for HeyNixi
1. **Zero Asset Copying:** Do not copy Nixi logos, eye SVGs, brand styling, or proprietary audio assets.
2. **Zero Code Copying:** Do not copy minified JavaScript code directly into Leo. Re-implement algorithmic principles (spring-damper dynamics, ASR cleanup filters, SplitMix32 PRNG) using standard mathematical formulations and cleanroom TypeScript code.
3. **No Private Access:** Do not attempt to decompile unreleased private binaries or access restricted beta endpoints. Leo will be built independently from first principles.

---

## 3. Reusable Dependencies & Libraries Audit

| Dependency / Package | Upstream License | Permitted in Leo | Notes |
| :--- | :--- | :--- | :--- |
| `@tauri-apps/api` | MIT / Apache 2.0 | **Yes** | Standard Tauri 2 web API bindings |
| `@tauri-apps/cli` | MIT / Apache 2.0 | **Yes** | Tauri build tooling |
| `sherpa-onnx` | Apache 2.0 | **Yes** | Local ONNX runtime for offline speech-to-text (Parakeet/Whisper) |
| `Parakeet-TDT-0.6B-v3` | CC-BY-4.0 / Open Weights | **Yes** | NVIDIA speech model; weights downloaded with explicit consent |
| `Qwen 3.5` (0.8B / 2B) | Apache 2.0 | **Yes** | Open-weights small language model family for local chat |
| `SplitMix32` | Public Domain | **Yes** | Standard PRNG algorithm |
