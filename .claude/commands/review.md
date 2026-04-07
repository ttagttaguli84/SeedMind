---
description: Run a consistency review across all project documents
---

Delegate to the **reviewer** agent.

## Process
1. Reviewer agent reads all `docs/**/*.md` files (focus review on files changed in this session)
2. All 14 Reviewer Checklist items must be exhaustively checked — skipping items is not permitted; if "not applicable", the reason must be stated
3. Outputs issues to `logs/reports/review_YYYYMMDD.md`
4. If CRITICAL issues found: reviewer reports them → **main Claude** applies the fix (reviewer does NOT modify documents)
5. If WARNING issues found: add to `TODO.md`
6. Commit and push

## Checklist Summary (full details → `.claude/agents/reviewer.md`)
- Items 1–8: Basic consistency (canonical references, duplicate figures, enum sync, Part I/II agreement, cross-refs, tags)
- Item 9: PATTERN-005 JSON/C# field sync
- Item 10: PATTERN-006 MCP task array figure references
- Item 11: PATTERN-007 SO asset table content parameters
- Item 12: PATTERN-008 Facility document recipe direct entry prohibition
- Item 13: PATTERN-BAL-COST Net profit calculation including fuel cost
- Item 14: PATTERN-010 Architecture placeholder values/IDs notation

## Self-Improve Trigger
If 3+ reviews have been run and same issue recurs: auto-run `/self-improve`.
