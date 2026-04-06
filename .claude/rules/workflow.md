# Workflow Rules

## Document-Only Policy
- This project is in DESIGN PHASE — no code files (.cs, .json, .unity) are written
- All work produces markdown documents in `docs/`
- Code snippets in docs are illustrative, not executable

## Agent Collaboration
- **Designer** and **Architect** work in parallel on the same system
- **Reviewer** always runs after Designer+Architect complete
- **Self-Improve** runs when triggered (3+ same-type issues, or user request)

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
- Phase advances when all TODO items for that phase are complete
- Update README.md status when phase changes
- Write a devlog entry for each phase transition
