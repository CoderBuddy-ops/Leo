---
trigger: always_on
description: Security, privacy, secrets storage, and network disclosure rules for Leo
---

# Security & Privacy Rules

1. **Secrets Handling:**
   - Secrets and API keys must NEVER be stored in plaintext files, code bundles, logs, or error dumps.
   - Use Windows Credential Manager (`PasswordVault`) for all provider keys.

2. **Network Egress Boundaries:**
   - Loopback endpoints (`127.0.0.1`, `localhost`) are treated as local.
   - Any external endpoint request requires an explicit user-visible badge (*"Leaves your PC"*).
   - Audio is NEVER sent off-device.

3. **Window Capture Exclusion:**
   - Leo's companion windows must set `WDA_EXCLUDEFROMCAPTURE` to prevent accidental broadcast during screen shares.
