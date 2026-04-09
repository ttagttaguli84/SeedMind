# Workflow Rules

## Document-Only Policy (Phase 1 only)
- While in Phase 1: no code files (.cs, .json, .unity) are written
- All work produces markdown documents in `docs/`
- Code snippets in docs are illustrative, not executable
- This policy is lifted automatically when Phase 2 begins

## Agent Collaboration

Agent selection is determined by task type (see `start.md` Phase 2/3 for full criteria):

| Task type | Phase 2 agent | Phase 3 reviewer |
|-----------|---------------|-----------------|
| DES-* new system | designer → architect (sequential) | Required |
| ARC-* architecture only | architect only | Required |
| BAL-* balance analysis | designer only | Required |
| CON-* content addition | designer only | Required |
| FIX-* simple fix | direct edit, no agent | Can skip (conditions apply) |

- **Architect** agent must check the Canonical Data Mapping in `doc-standards.md` before writing any document. Recording figures directly is prohibited; reference notation is required.
- **Architect** agent running solo must read the relevant design documents (DES, CON) first before writing. Placeholder values/IDs are prohibited — architecture is written only after design is confirmed (PATTERN-010).
- **Designer** agent working on BAL-* or CON-* tasks must show economic figure calculations step-by-step (including fuel cost and arithmetic errors) so the reviewer can verify them (PATTERN-BAL-COST).
- **Self-Improve** runs when triggered (3+ same-type issues, or user request)

## Reviewer Checklist

The reviewer must check items below when reviewing documents.
Scope varies by task type — see `start.md` Phase 3 for the per-task-type checklist range.
For DES-*/ARC-* tasks, all 14 items are required.

1. [ ] Do all figures (prices, growth days, probabilities, thresholds) have a canonical source reference?
2. [ ] Does the architecture document avoid independently copying figures from the design document?
3. [ ] Do code example defaults have a `// → see canonical` comment?
4. [ ] Are figures free of duplication across sections within the same document?
5. [ ] If an enum/type was extended, have all switch statements and code examples within the same document been updated?
6. [ ] Do Part I (design) and Part II (MCP implementation) agree on object hierarchy, initial values, and placement?
7. [ ] Does a Cross-references section exist and list all related documents?
8. [ ] Are `[OPEN]` and `[RISK]` tags used appropriately?
9. [ ] (PATTERN-005) Do the Part I JSON schema and Part II C# class within the same document have identical field names and field counts? — When a field is added or removed on one side, has the other side been updated immediately?
10. [ ] (PATTERN-006) When arrays or table-form figures are recorded directly in an MCP task document (including Part II), is a `(→ see canonical)` reference comment included? — When recorded directly, verify that the canonical document was modified simultaneously.
11. [ ] (PATTERN-007) When content-definition parameters such as tileSize/buildTimeDays/effectRadius/recipeCount are recorded as direct values in SO asset data tables (e.g. data-pipeline.md section 2.4), have they been replaced with a canonical content document (e.g. docs/content/facilities.md) reference? — Direct concrete values must be replaced with reference notation.
12. [ ] (PATTERN-008) When a recipe list (ingredients/outputs/prices) is recorded directly in a processing facility section of a facilities document (facilities.md, etc.), has it been replaced with a reference to the canonical document (processing-system.md)? — The only things a facility document is allowed to record are structural parameters such as slot count, fuel type, and processing speed multiplier.
13. [ ] (PATTERN-BAL-COST) In economy/balance documents, are fuel cost and material cost fully deducted when computing processing ROI and profit? — Calculating only "processed item price - raw material direct sale price" while omitting fuel cost is considered a miscalculation. Bakery/fermentation recipes must always include fuel cost when computing net profit.
14. [ ] (PATTERN-010) When an architecture document records unconfirmed values or IDs as placeholders, are they explicitly marked with the `[OPEN]` tag, and are design-unconfirmed values not recorded as if they were canonical values?

## Operational Rules (Human)

- **툴(seedmind.py GUI) 실행 중에는 `git push --force` 금지.**
  sync 기준점이 달라져 merge 충돌 발생. 툴을 Stop한 후 force-push할 것.

## Git Working Directory

**CRITICAL: Never use `cd <absolute-path> && git ...` for git operations.**

All git commands must run relative to the current working directory (`cwd`), which may be a worktree and not the main repository root. Using an absolute path overrides the worktree isolation and commits directly to `main`.

Correct:
```bash
git add docs/foo.md && git commit -m "..." && git push
```

Forbidden:
```bash
cd "C:/UE/SeedMind" && git add ...   # bypasses worktree — never do this
```

If a git command requires a repo path, use `-C .` (current dir) or omit the path entirely.

## Commit Cadence
- Commit after each logical unit of work (one system expansion, one review pass)
- Push after every commit
- Devlog entry at end of each session or major milestone

## TODO Management
- TODO.md is the single backlog
- Format: `| ID | Priority(1~5) | Description |`
- Priority 5 = most urgent
- PATTERN- prefix = systemic issue, handled by self-improve only

## Phase Progression

### Phase 1 → Phase 2 Completion Criteria (AI self-evaluated)
Check at the start of every session. When all conditions are met, transition immediately without waiting for user approval.

1. [ ] All systems have DES + ARC + MCP task documents.
   DES source varies by system — check the mapping below:

   | System | DES | ARC | MCP tasks |
   |--------|-----|-----|-----------|
   | farming | systems/farming-system.md | farming-architecture.md | farming-tasks.md |
   | crop-growth | systems/crop-growth.md | crop-growth-architecture.md | crop-growth-tasks.md |
   | time-season | systems/time-season.md | time-season-architecture.md | time-season-tasks.md |
   | economy | systems/economy-system.md | economy-architecture.md | economy-tasks.md |
   | inventory | systems/inventory-system.md | inventory-architecture.md | inventory-tasks.md |
   | quest | systems/quest-system.md | quest-architecture.md | quest-tasks.md |
   | achievement | systems/achievement-system.md | achievement-architecture.md | achievement-tasks.md |
   | ui | systems/ui-system.md | ui-architecture.md | ui-tasks.md |
   | tool-upgrade | systems/tool-upgrade.md | tool-upgrade-architecture.md | tool-upgrade-tasks.md |
   | tutorial | systems/tutorial-system.md | tutorial-architecture.md | tutorial-tasks.md |
   | save-load | systems/save-load-system.md | save-load-architecture.md | save-load-tasks.md |
   | collection | systems/collection-system.md | collection-architecture.md | collection-tasks.md |
   | decoration | systems/decoration-system.md | decoration-architecture.md | decoration-tasks.md |
   | energy | systems/energy-system.md | energy-architecture.md | energy-tasks.md |
   | farm-expansion | systems/farm-expansion.md | farm-expansion-architecture.md | farm-expansion-tasks.md |
   | fishing | systems/fishing-system.md | fishing-architecture.md | fishing-tasks.md |
   | gathering | systems/gathering-system.md | gathering-architecture.md | gathering-tasks.md |
   | sound | systems/sound-design.md | sound-architecture.md | sound-tasks.md |
   | visual | systems/visual-guide.md | visual-architecture.md | visual-tasks.md |
   | progression | balance/progression-curve.md (DES) | progression-architecture.md | progression-tasks.md |
   | facilities | content/facilities.md (DES) | facilities-architecture.md | facilities-tasks.md |
   | npc-shop | content/npcs.md (DES) | npc-shop-architecture.md | npc-shop-tasks.md |
   | blacksmith | content/blacksmith-npc.md (DES) | blacksmith-architecture.md | blacksmith-tasks.md |
   | livestock | content/livestock-system.md (DES) | livestock-architecture.md | livestock-tasks.md |
   | processing | content/processing-system.md (DES) | processing-architecture.md | processing-tasks.md |
2. [ ] Zero incomplete DES-*/ARC-* items in TODO
3. [ ] Zero unresolved PATTERN-* items in TODO (self-improve handled)
4. [ ] No implementation-blocking `[OPEN]` tags in core ARC documents

All four must be true simultaneously. If any condition is unmet, continue Phase 1 and replenish TODO as needed.

### Phase Transition Procedure (executed by AI autonomously)
When all four criteria above are confirmed:
1. Update `README.md` current status to Phase 2
2. Update `CLAUDE.md` current status + remove Document-only rule (핵심 규칙 2번)
3. Update `workflow.md` Document-Only Policy section
4. Write devlog entry for the phase transition
5. `git commit + push`
6. Begin Unity MCP task execution starting from `docs/mcp/scene-setup-tasks.md`

### Phase 2 → Phase 3 and beyond
Defined separately when Phase 2 scope is established.
