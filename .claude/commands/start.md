---
description: Run at session start only. Context restore → highest-priority task → design expansion → review → commit and push, fully automated
---

You are running a **planning session** for SeedMind.

## Phase 0 — Context Restore (required at every new session)

1. Read `CLAUDE.md` — project rules
2. Read `TODO.md` — current backlog
3. Read `README.md` — current phase/status
4. Read the latest `docs/devlog/*.md` — what happened last session
5. Read `docs/design.md` and `docs/architecture.md` **only if the highest-priority task is a new system design (DES-*)**. For BAL/FIX/CON/ARC tasks, read only the documents relevant to that task.
6. Print a 3-line status summary to the user:
   ```
   [SeedMind] Phase N | TODO: N items | Last work: <last devlog title>
   Session goal: <highest-priority TODO item>
   Proceeding.
   ```

## Phase 1 — Task Selection

- Pick the highest-priority item from `TODO.md`
- If multiple items share the same priority, pick the one that unblocks the most other items

## Phase 2 — Design Expansion

Select the agent strategy based on task type:

| Task type | Strategy | Notes |
|-----------|----------|-------|
| `DES-*` (new system design) | designer + architect in parallel | Design and architecture simultaneously |
| `ARC-*` (architecture only) | architect only | Design document already exists |
| `BAL-*` (balance analysis) | designer only | Figure-heavy, no structural changes |
| `FIX-*` (document fix) | direct edit, no agent | Simple field addition/sync/reference fix |
| `CON-*` (content addition) | designer only | Filling content into existing structures |

**Parallel agent condition**: Spawn designer+architect simultaneously only for DES-* tasks where the system architecture document does not yet exist.

**DES-* parallel execution + PATTERN-010 reconciliation**: When architect runs in parallel with designer, design figures are not yet confirmed. Architect MUST use `[OPEN - to be filled after DES-XXX is confirmed]` tags for all unconfirmed values. After the designer finishes, run a **sync pass** — architect reads the completed design document and fills in all `[OPEN]` placeholders before the reviewer runs.

**When running architect agent solo**: Must read the relevant design documents (DES, CON) first before writing. Check the Canonical Data Mapping in `doc-standards.md` and do not record figures directly.

## Phase 3 — Review

Decide whether to run the reviewer based on task type:

| Task type | Run reviewer? | Reason |
|-----------|---------------|--------|
| `DES-*` new system | Required | Full validation of new documents needed |
| `ARC-*` new architecture | Required | JSON/C# mismatches, enum sync issues, etc. |
| `BAL-*` balance analysis | Required | Fuel cost errors and figure mismatches are frequent |
| `CON-*` content addition | Required | Cross-reference and canonical reference omissions are frequent |
| `FIX-*` simple fix (1–2 field sync) | Can skip | Only when the change is limited to a single field/reference |

**Conditions to skip FIX-* reviewer**: Skip only when the change is clearly limited to one section of one document and copies an already-confirmed canonical value as-is. Reviewer is required when introducing new figures or modifying multiple documents simultaneously.

**When running the reviewer**: All 14 Reviewer Checklist items must be exhaustively checked. If an item is skipped or marked "not applicable", the reason must be stated.

## Phase 4 — Wrap Up

1. Update `TODO.md` — remove completed items, add new ones discovered
2. If TODO.md drops below 10 rows: analyze docs for gaps, add new items
3. Update `README.md` current status if a phase milestone was reached
4. Write a devlog entry in `docs/devlog/NNN-<title>.md`
5. Commit and push

## Session Task Budget

Determine how many tasks to handle this session based on the priority of the highest-priority task selected in Phase 0:

| Highest-priority task Priority | Tasks to handle in session | Rationale |
|-------------------------------|---------------------------|-----------|
| 3 or above (high urgency) | 1 | High-priority tasks are complex — focus on one |
| 2 | 2 | Mid-priority tasks are moderately scoped |
| 1 (low urgency) | 3–4 | Low-priority tasks are small enough to batch |

After completing Phase 4, **if budget remains**:
1. Re-read `TODO.md` only (do not re-read other Phase 0 files)
2. Repeat Phase 1→4 for the next highest-priority item
3. End when budget is exhausted or only Priority-2+ items remain

## Rules
- Designer and architect must reference each other's output
- Reviewer has final say on consistency issues
- All output in Korean. Document content in Korean (technical terms in English where natural).
- Looping is allowed within budget. Must stop when budget is exhausted.
