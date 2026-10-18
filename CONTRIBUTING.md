# Contributing to Leo

Thank you for contributing to **Leo**, the personal AI assistant for Windows 10/11 x64.

---

## 1. Development Principles

1. **Verify Before Claiming:**
   - Always run test commands independently (`node --test tests/leo_p0.test.mjs`).
   - Do not claim a native app works until it builds and launches on Windows.

2. **Respect Cleanroom Boundaries:**
   - Do not add proprietary assets from HeyNixi or reserved character assets from Coucou (`LICENSE-ASSETS.md`).
   - Re-use upstream MIT code cleanly with appropriate copyright notices in `THIRD_PARTY_NOTICES.md`.

3. **Architecture Decisions:**
   - Document any significant new dependency or architectural shift via an ADR in `docs/architecture/ADR/`.

---

## 2. Testing & Verification

```powershell
# Run Leo P0 unit tests
node --test tests/leo_p0.test.mjs

# Run reference windowing & FSM tests
cd reference_coucou/windows
npm test
```
