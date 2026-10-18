# Leo — Claude Code Operating Guide

This repository follows the canonical engineering specifications and safety boundaries documented in **[`AGENTS.md`](./AGENTS.md)**.

## Quick Reference Commands

Always run test commands independently to preserve clear exit code reporting:

```bash
# Unit Tests (17/17 passing)
node --test tests/leo_p0.test.mjs
node --test tests/leo_extended.test.mjs

# Frontend Webview Production Build
cd app && npm run build

# Webview Preview Server (Port 1420)
cd app && npx vite preview --port 1420
```

## Critical Boundaries

1. **Local-First Audio:** Never send raw audio off-device.
2. **Cleanroom IP:** Never use the names "Coucou" or "Mochi", and never copy proprietary HeyNixi JavaScript or Coucou character assets.
3. **Windows Safety:** Never execute arbitrary shell strings (`cmd.exe /c`, `powershell`). Use typed, bounded schemas.
4. **Window Privacy:** The native companion window must set `WDA_EXCLUDEFROMCAPTURE` to protect screen-sharing privacy.

Refer to `TASKS.md` for current milestone progress and blockers.
