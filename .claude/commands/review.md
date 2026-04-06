---
description: Run a consistency review across all project documents
---

Delegate to the **reviewer** agent.

## Process
1. Reviewer agent reads all `docs/**/*.md` files
2. Checks consistency, completeness, cross-references, feasibility
3. Outputs issues to `logs/reports/review_YYYYMMDD.md`
4. If CRITICAL issues found: fix them immediately
5. If WARNING issues found: add to `TODO.md`
6. Commit and push

## Self-Improve Trigger
If 3+ reviews have been run and same issue recurs: auto-run `/self-improve`.
