---
trigger: always_on
description: Quality engineering, test requirements, verification commands, and definition of done
---

# Quality & Testing Rules

1. **Independent Test Execution:**
   - Run test commands separately. Never combine with `;` to mask failures.
   - Capture exact exit codes and output lines.

2. **Definition of Done:**
   - Code changed in the correct module.
   - Automated unit/integration tests added and passing.
   - `TASKS.md` updated with real evidence.
