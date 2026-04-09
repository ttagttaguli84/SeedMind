---
description: Run at session start only. Context restore → highest-priority task → design expansion → review → commit and push, fully automated
---

You are running a **planning session** for SeedMind.

## Phase 0 — Context Restore (required at every new session)

> **Note**: CLAUDE.md, doc-standards.md, workflow.md are already loaded in system context. Do NOT read them again.

**Step 0 — Check current Phase:**
Read `CLAUDE.md` current status (already in context). If it says **Phase 2**, read `docs/mcp/progress.md` to find the current MCP execution position, then continue from there instead of running the document workflow below.

> Phase 2 flow: read `docs/mcp/progress.md` → identify next task file → execute MCP tasks → save scene → **immediately** mark task ✅ in progress.md → commit + push.
> **Important**: update progress.md right after each task file completes, not at session end. Session end is unknowable; task completion is definite.

---

Determine task type from `TODO.md` first, then load only what is needed:

**Step 1 — Always read (all task types):**
- `TODO.md` — identify highest-priority task and its type

**Step 2 — Conditional reads by task type:**

| Task type | Additional files to read |
|-----------|--------------------------|
| `DES-*` | `README.md`, latest `docs/devlog/*.md`, task-relevant system docs — design.md/architecture.md are read by agents directly, not injected |
| `ARC-*` | `README.md`, latest `docs/devlog/*.md`, relevant design doc, `docs/architecture.md` |
| `BAL-*` | latest `docs/devlog/*.md`, relevant balance/economy docs |
| `CON-*` | latest `docs/devlog/*.md`, relevant content/design docs |
| `FIX-*` | only the specific document(s) to be fixed |

**Step 3 — Save read content for agent injection (Phase 2). Only task-relevant files — do NOT include large base documents (design.md, architecture.md) unless the task directly modifies them.**

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
| `DES-*` (new system design) | designer → architect (sequential) | Designer completes first; architect reads confirmed design, no sync pass needed |
| `ARC-*` (architecture only) | architect only | Design document already exists |
| `BAL-*` (balance analysis) | designer only | Figure-heavy, no structural changes |
| `FIX-*` (document fix) | direct edit, no agent | Simple field addition/sync/reference fix |
| `CON-*` (content addition) | designer only | Filling content into existing structures |

**DES-* sequential execution**: Run designer first. After designer completes, architect reads the confirmed design document directly (no injection needed for large docs) and writes the architecture document. This eliminates the PATTERN-010 sync pass entirely.

**When running architect agent solo**: Must read the relevant design documents (DES, CON) first before writing. Check the Canonical Data Mapping in `doc-standards.md` and do not record figures directly.

### Agent Context Injection (cost optimization)

When spawning any agent, **prepend only task-relevant content** to the agent prompt:

```
## Pre-loaded Documents (do NOT re-read these files)
The following documents have already been read by the main session.
Use the content below directly. Do not call Read tool for these files.

### TODO.md
<paste TODO.md content>

### [task-relevant files only — e.g. the specific system doc being worked on]
<paste content>
```

**Injection rules:**
- Inject: TODO.md + the specific doc(s) being created or modified
- Do NOT inject: design.md, architecture.md, devlog files — these are large; let the agent Read them directly if needed (one-time cache_creation, same cost)
- For FIX-* tasks (no agent), skip entirely
- Reviewer agent: inject only the newly written document(s) — do NOT inject canonical reference docs; reviewer reads them directly if needed
- Exception: for BAL-* tasks, reviewer MUST read `docs/systems/economy-system.md` directly to verify fuel cost figures (checklist item 13)

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

**Reviewer checklist scope by task type** (do not exhaustively check items irrelevant to the task):

| Task type | Checklist items to verify |
|-----------|--------------------------|
| `FIX-*` | Items 1–4 only |
| `BAL-*` / `CON-*` | Items 1–8 + task-relevant items from 9–14 |
| `DES-*` / `ARC-*` | All 14 items |

## Phase 4 — Wrap Up

1. Update `TODO.md` — mark completed items, add new ones discovered
2. If TODO.md has zero active DES/ARC items: analyze docs for gaps, add new DES/ARC items (FIX/BAL/CON count is ignored for replenishment)
3. Update `README.md` current status if a phase milestone was reached
4. Write a devlog entry in `docs/devlog/NNN-<title>.md`
5. Commit and push

## Session Task Budget

Determine how many tasks to handle this session based on the priority of the highest-priority task selected in Phase 0:

| Highest-priority task Priority | Tasks to handle in session | Rationale |
|-------------------------------|---------------------------|-----------|
| 3 or above (high urgency) | 1 | High-priority tasks are complex — focus on one |
| 2 | 1 | Mid-priority tasks require full agent+review cycle |
| 1 (low urgency) | 2 | Low-priority tasks are small enough to pair |

After completing Phase 4, **if budget remains (Priority-1 only)**:
1. Re-read `TODO.md` only (do not re-read other Phase 0 files)
2. Repeat Phase 1→4 for the next highest-priority item
3. End when budget is exhausted or a Priority-2+ item is encountered

## Rules
- For DES-* tasks: architect must read and reference the designer's completed output before writing. Designer writes independently; architect references confirmed design.
- Reviewer has final say on consistency issues
- All output in Korean. Document content in Korean (technical terms in English where natural).
- Looping is allowed only within Priority-1 budget. Must stop when budget is exhausted.
- **FIX inline principle**: When a task (DES/ARC/BAL/CON) creates downstream fixes (reference additions, figure sync, enum updates), handle them within the same task — do NOT register separate FIX items in TODO. Only register a FIX in TODO when immediate resolution is impossible (e.g., depends on unconfirmed design, requires user decision).
