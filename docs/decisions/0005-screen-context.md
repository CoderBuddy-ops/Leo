# ADR 0005: Screen Context Acquisition and Privacy Policy

**Status:** Accepted  
**Date:** 2026-10-17  
**Deciders:** Leo Principal Engineering Team

---

## Context
Users frequently want to ask questions about on-screen material ("Summarize this page", "What does this compiler error mean?"). Screen capture can inadvertently expose sensitive personal data (passwords, banking information, private chat messages, medical records).

---

## Decision
1. **Tiered Context Acquisition:**
   - **Tier A (Structured UI Automation):** For accessible applications, query Windows UI Automation (UIA) COM interfaces to extract clean text representations directly from the foreground window, avoiding lossy OCR and pixel processing.
   - **Tier B (Bounded Active-Window Screenshot):** If UI Automation yields insufficient text, capture only the **currently focused foreground window** (never the entire multi-monitor desktop), using Windows Graphics Capture or `PrintWindow` API.
2. **Mandatory Preview and Redaction:**
   - Screen context is **never silently transmitted** to a model.
   - Leo displays an inspectable preview card allowing the user to review what text/pixels were extracted, redact sensitive regions, or cancel the request.
3. **Capture Exclusion (`WDA_EXCLUDEFROMCAPTURE`):**
   - The Leo companion pill and teleprompter windows configure `SetWindowDisplayAffinity(hwnd, WDA_EXCLUDEFROMCAPTURE)`. This ensures Leo does not capture its own UI and remains hidden from screen-sharing software (OBS, Microsoft Teams, Discord).

---

## Consequences
- **Positive:** Uncompromising privacy posture; high-fidelity text extraction without OCR blur.
- **Negative:** Some legacy non-standard Windows apps will fall back to active-window screenshot capture.
