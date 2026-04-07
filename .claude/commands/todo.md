---
description: Read TODO.md and execute the highest-priority documentation task
---

Read `TODO.md`. It is a markdown table: `ID | Priority(1~5) | Description`.

## Task Selection
- Pick the highest-priority item
- PATTERN- prefix items: delegate to `/self-improve`, do not handle directly

## Execution
1. Read relevant existing documents for context
2. Select the agent based on task type (following `start.md` Phase 2/3 strategy):
   - `DES-*` new system: designer + architect in parallel
   - `ARC-*` architecture only: architect only (read design document first)
   - `BAL-*` balance: designer only
   - `CON-*` content: designer only
   - `FIX-*` simple fix: direct edit, no agent
3. Create or update documents
4. Decide whether to run `/review` (based on `start.md` Phase 3 criteria — can skip for simple FIX-* fixes)
5. Remove completed row from TODO.md
6. If TODO.md drops below 10 rows: analyze docs for gaps, add new items
7. Commit and push

## Rules
- One logical task per commit
- Always verify cross-references after changes
- Output: completed item, changed files, any new issues found
