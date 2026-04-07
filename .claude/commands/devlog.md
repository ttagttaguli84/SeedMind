---
description: Write a development log entry summarizing recent work and decisions
---

## Process

1. Read `git log --oneline -10` for recent changes
2. Read the latest `docs/devlog/*.md` to get the current entry number
3. Write a new entry at `docs/devlog/NNN-*.md` covering:
   - What was done (decisions made, documents created/updated)
   - Why (reasoning behind each decision)
   - What's next (planned next steps)
   - AI self-assessment (confidence level, risks identified)
4. Commit and push

## Format
```markdown
# Devlog #NNN — [Title]
> YYYY-MM-DD | Phase N | Author: Claude Code (Opus)

## What Was Done Today
## Rationale
## Next Steps
## AI Self-Assessment
```
