# Document Standards

## File Naming
- System docs: `docs/systems/<system-name>.md`
- MCP plans: `docs/mcp/<system-name>-tasks.md`
- Content specs: `docs/content/<category>.md`
- Balance sheets: `docs/balance/<topic>.md`
- Devlogs: `docs/devlog/NNN-<short-title>.md` (3-digit zero-padded)

## Document Structure
Every design document MUST include:
1. **Header**: title, date, author (Claude Code)
2. **Context**: what this document covers and why
3. **Specification**: the actual content
4. **Cross-references**: links to related documents
5. **Open questions**: tagged with `[OPEN]`
6. **Risks**: tagged with `[RISK]`

## Consistency Rules
- Crop names, building names, system names: use the EXACT same string everywhere
- Numbers (prices, growth days, thresholds): single source of truth in the most specific doc
- If a value appears in multiple docs, one must be marked as canonical with `(→ see docs/X.md)`

## Language
- Document content: Korean (technical terms in English where natural)
- Tags/labels: English (`[OPEN]`, `[RISK]`, `[TODO]`)
- File names: English, kebab-case
