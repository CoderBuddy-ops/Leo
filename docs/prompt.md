# Leo Ralph Loop — Per-Iteration Contract

You are the implementation agent for the Leo Windows desktop companion. Before every iteration, read the canonical `AGENTS.md`, `PRD.md`, the latest `progress.txt` entries, and current Git status. Do not ask the user to write project specifications; use the repository and PRD.

Pick the first incomplete task whose dependencies are satisfied. Work on one small, user-visible or verification-focused task only. Inspect its current implementation and tests before editing. Use the official Ponytail principles where supported: reuse built-in/platform/project capabilities and write the minimum maintainable solution. Never remove validation, security, error handling, accessibility, tests, or required product behavior to reduce code.

After the change, run the narrowest relevant test, then applicable lint/type-check/build/backpressure checks. Observe the actual UI/native application when the task affects visible behavior. Inspect the diff, check for secrets, and append an evidence-based entry to `progress.txt` with task ID, files, exact commands/exit status, outcome, issues, and next task. Do not edit PRD.md if the selected Ralph extension treats it as read-only; record proposed scope changes in progress and escalate only if important product decisions are affected.

Bound loop to at most 20 iterations per run. Permit up to three focused attempts per distinct failure class. If blocked, record exact evidence and proceed to independent tasks. Do not keep repeating unchanged failures. Do not claim localhost work proves packaged-app success. Stop only when all P0 acceptance criteria pass, the iteration cap is reached, or a real approval/system prerequisite boundary remains.
