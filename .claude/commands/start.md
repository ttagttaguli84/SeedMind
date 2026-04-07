---
description: Run at session start only. Context restore → highest-priority task → design expansion → review → commit and push, fully automated
---

You are running a **planning session** for SeedMind.

## Phase 0 — Context Restore (required at every new session)

> **Note**: CLAUDE.md, doc-standards.md, workflow.md are already loaded in system context. Do NOT read them again.

Determine task type from `TODO.md` first, then load only what is needed:

**Step 1 — Always read (all task types):**
- `TODO.md` — identify highest-priority task and its type

**Step 2 — Conditional reads by task type:**

| Task type | Additional files to read |
|-----------|--------------------------|
| `DES-*` | `README.md`, latest `docs/devlog/*.md`, `docs/design.md`, `docs/architecture.md`, task-relevant system docs |
| `ARC-*` | `README.md`, latest `docs/devlog/*.md`, relevant design doc, `docs/architecture.md` |
| `BAL-*` | latest `docs/devlog/*.md`, relevant balance/economy docs |
| `CON-*` | latest `docs/devlog/*.md`, relevant content/design docs |
| `FIX-*` | only the specific document(s) to be fixed |

**Step 3 — Save all read content in memory for agent injection (Phase 2).**

**Step 4 — Print a 3-line status summary:**
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

### Agent Context Injection (cost optimization)

When spawning any agent, **prepend the following block to the agent prompt**:

```
## Pre-loaded Documents (do NOT re-read these files)
The following documents have already been read by the main session.
Use the content below directly. Do not call Read tool for these files.

### TODO.md
<paste TODO.md content>

### [other files read in Phase 0]
<paste each file's content>
```

- Only inject files that were actually read in Phase 0.
- For FIX-* tasks (no agent), this step is skipped entirely.
- Reviewer agent: inject the newly written document content + any canonical docs checked during Phase 2.

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
